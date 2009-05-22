using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;
using TowseyLib;
using System.Drawing;
using System.Drawing.Imaging;

namespace AudioAnalysis
{

    public enum SonogramType { amplitude, spectral, cepstral, acousticVectors, sobelEdge }


	public abstract class BaseSonogram
    {

        #region Properties
        public SonogramConfig Configuration { get; private set; }

		public double MaxAmplitude { get; private set; }
		public int SampleRate { get; protected set; }
		public int NyquistFrequency { get { return SampleRate / 2; } }
        public TimeSpan Duration { get; protected set; }

		public double FrameDuration { get { return Configuration.WindowSize / (double)SampleRate; } } // Duration of full frame or window in seconds
		public double FrameOffset { get { return FrameDuration * (1 - Configuration.WindowOverlap); } } // Duration of non-overlapped part of window/frame in seconds
		public double FBinWidth { get { return (SampleRate / 2) / (double)Configuration.FreqBinCount; } }
		public double FramesPerSecond { get { return 1 / FrameOffset; } }
        public int FrameCount { get; protected set; } // Originally temporarily set to (int)(Duration.TotalSeconds / FrameOffset) then reset later

        //energy and dB per frame
        public SNR SnrFrames { get; private set; }
        public double[] DecibelsPerFrame { get { return SnrFrames.Decibels; } protected set {} }//decibels per signal frame

        //energy and dB per frame sub-band
        public bool   ExtractSubband { get; set; } // extract sub-band when making spectrogram image
        protected int subBand_MinHz; //min freq (Hz) of the required subband
        protected int subBand_MaxHz; //max freq (Hz) of the required subband
        public SNR    SnrSubband { get; private set; }
        public double[] DecibelsInSubband { get; protected set; }  // Normalised decibels in extracted freq band

        public double[] DecibelsNormalised { get; protected set; }
        public double Max_dBReference { get; protected set; } // Used to normalise the dB values for MFCCs


        public int[] SigState { get; protected set; }   // Integer coded signal state ie  0=non-vocalisation, 1=vocalisation, etc.

		public double[,] Data { get; protected set; } //the spectrogram data matrix
		#endregion

        /// <summary>
        /// use this constructor when want to extract time segment of existing sonogram
        /// </summary>
        /// <param name="config"></param>
        public BaseSonogram(SonogramConfig config)
        {
            Configuration = config;
            this.subBand_MinHz = config.MinFreqBand ?? 0;
            this.subBand_MaxHz = config.MaxFreqBand ?? NyquistFrequency;
        }

        /// <summary>
        /// BASE CONSTRUCTOR
        /// </summary>
        /// <param name="config"></param>
        /// <param name="wav"></param>
        /// <param name="doExtractSubband"></param>
        public BaseSonogram(SonogramConfig config, WavReader wav)
		{
			Configuration = config;

			SampleRate      = wav.SampleRate;
			Duration        = wav.Time;
			MaxAmplitude    = wav.CalculateMaximumAmplitude();
            double[] signal = wav.Samples;

			this.subBand_MinHz = config.MinFreqBand ?? 0;
			this.subBand_MaxHz = config.MaxFreqBand ?? NyquistFrequency;
            this.ExtractSubband = this.subBand_MinHz > 0 || this.subBand_MaxHz < NyquistFrequency;
            if (config.DisplayFullBandwidthImage) this.ExtractSubband = false;//if sono only intended for image


			// SIGNAL PRE-EMPHASIS helps with speech signals
			if (config.DoPreemphasis) signal = DSP.PreEmphasis(signal, 0.96);

			// FRAME WINDOWING
            double[,] frames = DSP.Frames(signal, config.WindowSize, config.WindowOverlap);
			FrameCount = frames.GetLength(0);

			// ENERGY PER FRAME and NORMALISED dB PER FRAME AND SNR
            this.SnrFrames = new SNR(frames);
            this.Max_dBReference = SnrFrames.MaxReference_dBWrtNoise;  // Used to normalise the dB values for feature extraction
            this.DecibelsNormalised = SnrFrames.NormaliseDecibelArray_ZeroOne(this.Max_dBReference);

			// ZERO CROSSINGS
			//this.zeroCross = DSP.ZeroCrossings(frames);

            //AUDIO SEGMENTATION
            SigState = EndpointDetectionConfiguration.DetermineVocalisationEndpoints(DecibelsPerFrame, this.FrameOffset);

            var fractionOfHighEnergyFrames = SnrFrames.FractionHighEnergyFrames(EndpointDetectionConfiguration.K2Threshold);
			if (fractionOfHighEnergyFrames > 0.8)
			{
                Log.WriteIfVerbose("\nWARNING ##########################################");
                Log.WriteIfVerbose("\t################### BaseSonogram(BaseSonogramConfig config, WavReader wav, bool doExtractSubband)");
                Log.WriteIfVerbose("\t################### This is a high energy recording. The fraction of high energy frames = "
																+ fractionOfHighEnergyFrames.ToString("F2") + " > 80%");
                Log.WriteIfVerbose("\t################### Noise reduction algorithm may not work well in this instance!\n");
			}

			//generate the spectra of FFT AMPLITUDES
			//calculate a minimum amplitude to prevent taking log of small number. This would increase the range when normalising
			double epsilon = Math.Pow(0.5, wav.BitsPerSample - 1);
			var amplitudeM = MakeAmplitudeSpectra(frames, TowseyLib.FFT.GetWindowFunction(FftConfiguration.WindowFunction), epsilon);
			//Log.WriteIfVerbose("\tDim of amplitude spectrum =" + amplitudeM.GetLength(0)+", " + amplitudeM.GetLength(1));

			//EXTRACT REQUIRED FREQUENCY BAND
            if (ExtractSubband)
			{
                amplitudeM = ExtractFreqSubband(amplitudeM, this.subBand_MinHz, this.subBand_MaxHz);
				Log.WriteIfVerbose("\tDim of required sub-band  =" + amplitudeM.GetLength(1));
                CalculateSubbandSNR(amplitudeM);
            }

			Make(amplitudeM);
        } //end CONSTRUCTOR BaseSonogram(WavReader wav)


        protected abstract void Make(double[,] amplitudeM);



		double[,] MakeAmplitudeSpectra(double[,] frames, TowseyLib.FFT.WindowFunc w, double epsilon)
		{
			int frameCount = frames.GetLength(0);
			int N = frames.GetLength(1);  // = FFT windowSize 
			int binCount = (N / 2) + 1;   // = fft.WindowSize/2 +1 for the DC value;

			var fft = new TowseyLib.FFT(N, w); // init class which calculates the FFT

			//calculate a minimum amplitude to prevent taking log of small number. This would increase the range when normalising
			int smoothingWindow = 3; //to smooth the spectrum 

			double[,] sgM = new double[frameCount, binCount];

			for (int i = 0; i < frameCount; i++)//foreach time step
			{
				double[] data = DataTools.GetRow(frames, i);
				double[] f1 = fft.Invoke(data);
				f1 = DataTools.filterMovingAverage(f1, smoothingWindow); //to smooth the spectrum - reduce variance
				for (int j = 0; j < binCount; j++) //foreach freq bin
				{
					double amplitude = f1[j];
					if (amplitude < epsilon)
						amplitude = epsilon; // to prevent possible log of a very small number
					sgM[i, j] = amplitude;
				}
			} //end of all frames
			return sgM;
		}


        public double[,] ExtractFreqSubband(WavReader wav, int minHz, int maxHz)
        {
			double[] signal = wav.Samples;
            double[,] frames = DSP.Frames(signal, this.Configuration.WindowSize, this.Configuration.WindowOverlap);
			//calculate a minimum amplitude to prevent taking log of small number. This would increase the range when normalising
			double epsilon = Math.Pow(0.5, wav.BitsPerSample - 1);
			var amplitudeM = MakeAmplitudeSpectra(frames, TowseyLib.FFT.GetWindowFunction(FftConfiguration.WindowFunction), epsilon);
            this.ExtractSubband = true;
            this.subBand_MinHz = minHz;
            this.subBand_MaxHz = maxHz;
            return ExtractFreqSubband(amplitudeM, minHz, maxHz);
        }

        public void CalculateSubbandSNR(WavReader wav, int minHz, int maxHz)
        {
            var subband = ExtractFreqSubband(wav, minHz, maxHz);
            CalculateSubbandSNR(subband);
        }

        public double[,] ExtractFreqSubband(double[,] m, int minHz, int maxHz)
        {
            int c1 = (int)(minHz / FBinWidth);
            int c2 = (int)(maxHz / FBinWidth);
            return DataTools.Submatrix(m, 0, c1, m.GetLength(0) - 1, c2);
        }

        public void CalculateSubbandSNR(double[,] subband)
        {
            this.SnrSubband = new SNR(subband); //subband is the amplitude values
            //RECALCULATE DecibelsNormalised and dB REFERENCE LEVEL - need for MFCCs
            this.DecibelsInSubband = SnrSubband.Decibels;
            this.Max_dBReference   = SnrSubband.MaxReference_dBWrtNoise;
            this.DecibelsNormalised = SnrSubband.NormaliseDecibelArray_ZeroOne(this.Max_dBReference);
            //RECALCULATE ENDPOINTS OF VOCALISATIONS
            SigState = EndpointDetectionConfiguration.DetermineVocalisationEndpoints(this.DecibelsInSubband, this.FrameOffset);
        }

        /// <summary>
        /// IMPORTANT: Mel scale conversion should be done before noise reduction
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public double[,] NoiseReduce_Standard(double[,] matrix)
        {
            double decibelThreshold = 6.5;   //SETS MIN DECIBEL BOUND

            double minIntensity; // min value in matrix
            double maxIntensity; // max value in matrix
            DataTools.MinMax(matrix, out minIntensity, out maxIntensity);
            double[,] mnr = matrix;
            //mnr = ImageTools.WienerFilter(mnr); //has slight blurring effect and so decide not to use
            mnr = SNR.RemoveModalNoise(mnr);
            mnr = SNR.RemoveBackgroundNoise(mnr, decibelThreshold); 
            return mnr;
        }

        /// <summary>
        /// IMPORTANT: Mel scale conversion should be done before noise reduction
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public double[,] NoiseReduce_FixedRange(double[,] matrix)
        {
            double decibelThreshold = 6.5;   //SETS MIN DECIBEL BOUND
            double dynamicRange = this.Configuration.DynamicRange;
            Log.WriteIfVerbose("\tNoise reduction: dynamic range = " + dynamicRange);

            double minIntensity; // min value in matrix
            double maxIntensity; // max value in matrix
            DataTools.MinMax(matrix, out minIntensity, out maxIntensity);
            double[,] mnr = matrix;
            mnr = SNR.RemoveModalNoise(mnr);
            mnr = SNR.RemoveBackgroundNoise(mnr, decibelThreshold);
            return mnr;
        }

		public Image GetImage()
		{
			return GetImage(1, false, false);
		}

        public Image GetImage(bool doHighlightSubband, bool add1kHzLines)
		{
           // Log.WriteIfVerbose("BaseSonogram.GetImage(bool doHighlightSubband, bool add1kHzLines)");
           // Log.WriteIfVerbose("    doHighlightSubband=" + doHighlightSubband + "   add1kHzLines=" + add1kHzLines);
            return GetImage(1, doHighlightSubband, add1kHzLines);
		}

		protected virtual Image GetImage(int binHeight, bool doHighlightSubband, bool add1kHzLines)
		{
			var data = this.Data;
			int width = data.GetLength(0); // Number of spectra in sonogram
            int fftBins = data.GetLength(1);
            int imageHeight = fftBins * binHeight; // image ht = sonogram ht. Later include grid and score scales
            
            //set up min, max, range for normalising of dB values
			double min; double max;
			DataTools.MinMax(data, out min, out max);
			double range = max - min;

            //int? minHighlightFreq = this.subBand_MinHz;
            //int? maxHighlightFreq = this.subBand_MaxHz;
            //int minHighlightBin = (minHighlightFreq == null) ? 0 : (int)Math.Round((double)minHighlightFreq / (double)NyquistFrequency * fftBins);
            //int maxHighlightBin = (maxHighlightFreq == null) ? 0 : (int)Math.Round((double)maxHighlightFreq / (double)NyquistFrequency * fftBins);
            //calculate top and bottom of sub-band 
            int minHighlightBin = (int)Math.Round((double)this.subBand_MinHz / (double)NyquistFrequency * fftBins);
            int maxHighlightBin = (int)Math.Round((double)this.subBand_MaxHz / (double)NyquistFrequency * fftBins);
            if (this.Configuration.DoMelScale)
            {
                double maxMel = Speech.Mel(this.NyquistFrequency);
                int melRange = (int)(maxMel - 0 + 1);
                double pixelPerMel = imageHeight / (double)melRange;
                double minBandMel = Speech.Mel(this.subBand_MinHz);
                double maxBandMel = Speech.Mel(this.subBand_MaxHz);
                minHighlightBin = (int)Math.Round((double)minBandMel * pixelPerMel);
                maxHighlightBin = (int)Math.Round((double)maxBandMel * pixelPerMel);
            }
			Color[] grayScale = ImageTools.GrayScale();

			Bitmap bmp = new Bitmap(width, imageHeight, PixelFormat.Format24bppRgb);
			int yOffset = imageHeight;
			for (int y = 0; y < data.GetLength(1); y++) //over all freq bins
			{
				for (int r = 0; r < binHeight; r++) //repeat this bin if ceptral image
				{
					for (int x = 0; x < width; x++) //for pixels in the line
					{
						// normalise and bound the value - use min bound, max and 255 image intensity range
						double value = (data[x, y] - min) / (double)range;
						int c = 255 - (int)Math.Floor(255.0 * value); //original version
						if (c < 0)
							c = 0;
						else if (c >= 256)
							c = 255;

						int g = c + 40; // green tinge used in the template scan band 
						if (g >= 256) g = 255;
                        Color col = (doHighlightSubband && IsInBand(y, minHighlightBin, maxHighlightBin)) ? Color.FromArgb(c, g, c) : grayScale[c];
                        bmp.SetPixel(x, yOffset - 1, col);
					}//for all pixels in line
					yOffset--;
				} //end repeats over one track
			}//end over all freq bins

            if (add1kHzLines) Draw1kHzLines(bmp);
			return bmp;
        }


        /// <summary>
        /// factor must be an integer. 2 mean image reduced by factor of 2; 3 reduced by factor of 3 etc.
        /// </summary>
        /// <param name="factor"></param>
        /// <returns></returns>
        public Image GetImage_ReducedSonogram(int factor)
        {
            return GetImage_ReducedSonogram(factor, true);
        }

        public Image GetImage_ReducedSonogramWithWidth(int width, bool drawGridLines)
        {
            var data = Data; //sonogram intensity values
            int frameCount = data.GetLength(0); // Number of spectra in sonogram

            int factor = frameCount / width;

            if (factor <= 1)
            {
                return GetImage();
            }

            return GetImage_ReducedSonogram(factor, drawGridLines);
        }

        public Image GetImage_ReducedSonogram(int factor, bool drawGridLines)
        {
          //  double[] logEnergy = this.LogEnPerFrame;
            var data = Data; //sonogram intensity values
            int frameCount  = data.GetLength(0); // Number of spectra in sonogram
            int imageHeight = data.GetLength(1); // image ht = sonogram ht. Later include grid and score scales
            int imageWidth  = frameCount / factor;
            int subSample   = frameCount / imageWidth;

            //set up min, max, range for normalising of dB values
            double min; double max;
            DataTools.MinMax(data, out min, out max);
            double range = max - min;

            Color[] grayScale = ImageTools.GrayScale();

            //set up the 1000kHz scale
            int[] vScale = CreateLinearYaxis(1000, imageHeight); //calculate location of 1000Hz grid lines

            Bitmap bmp = new Bitmap(imageWidth, imageHeight, PixelFormat.Format24bppRgb);
            for (int w = 0; w < imageWidth; w++)
            {
                int start = w * subSample;
                int end = ((w + 1) * subSample) - 1;
                double maxE = -Double.MaxValue;
                int maxID = 0;
                for (int x = start; x < end; x++)
                {
                    if (maxE < DecibelsPerFrame[x]) //NOTE!@#$%^ This was changed from LogEnergy on 30th March 2009.
                    {
                        maxE = DecibelsPerFrame[x];
                        maxID = x;
                    }
                }
                //have found the frame with max energy. Now draw its spectrum
                for (int y = 0; y < data.GetLength(1); y++) //over all freq bins
                {
                    // normalise and bound the value - use min bound, max and 255 image intensity range
                    double value = (data[maxID, y] - min) / (double)range;
                    int c = 255 - (int)Math.Floor(255.0 * value); //original version
                    if (c < 0) c = 0;
                       else if (c >= 256) c = 255;
                     Color col = grayScale[c];
                     bmp.SetPixel(w, imageHeight-y-1, col);
                }//end over all freq bins

                //set up grid color

                if (drawGridLines)
                {
                    Color gridCol = Color.Black;
                    if ((w % 2) == 0) gridCol = Color.Black;
                    for (int p = 0; p < vScale.Length; p++) //over all Y-axis pixels
                    {
                        if (vScale[p] == 0) continue;
                        int y = imageHeight - p;
                        bmp.SetPixel(w, y, gridCol);
                    }
                }

            }
            return bmp;
        }


		bool IsInBand(int y, int? minFreq, int? maxFreq)
		{
			if (minFreq == null && maxFreq == null)
				return false;
			return (minFreq == null || y > minFreq) && (maxFreq == null || y < maxFreq);
		}


        private void Draw1kHzLines(Bitmap bmp)
        {
            const int kHz = 1000;
            int width = bmp.Width;
            int height = bmp.Height;

            //calculate height of the sonogram
            int sHeight = height;
            int[] vScale = CreateLinearYaxis(kHz, sHeight); //calculate location of 1000Hz grid lines
            if (this.Configuration.DoMelScale) vScale = CreateMelYaxis(kHz, sHeight);

            for (int p = 0; p < vScale.Length; p++) //over all Y-axis pixels
            {
                if (vScale[p] == 0) continue;
                int y = sHeight - p;
                for (int x = 1; x < width; x++)
                {
                    bmp.SetPixel(x-1, y, Color.White);
                    bmp.SetPixel(x,   y, Color.Black);
                    x++;
                }
            }
        }//end AddGridLines()


        public int[] CreateLinearYaxis(int herzInterval, int imageHt)
        {
            //int freqRange = this.maxFreq - this.minFreq + 1;
            int minFreq = 0;
            int maxFreq = this.NyquistFrequency;
            int freqRange = maxFreq - minFreq + 1;
            double pixelPerHz = imageHt / (double)freqRange;
            int[] vScale = new int[imageHt];
            //Console.WriteLine("freqRange=" + freqRange + " herzInterval=" + herzInterval + " imageHt=" + imageHt + " pixelPerHz=" + pixelPerHz);

            for (int f = minFreq + 1; f < maxFreq; f++)
            {
                if (f % 1000 == 0)  //convert freq value to pixel id
                {
                    int hzOffset = f - minFreq;
                    int pixelID = (int)(hzOffset * pixelPerHz) + 1;
                    if (pixelID >= imageHt) pixelID = imageHt - 1;
                    //Console.WriteLine("f=" + f + " hzOffset=" + hzOffset + " pixelID=" + pixelID);
                    vScale[pixelID] = 1;
                }
            }
            return vScale;
        }


        /// <summary>
        /// use this method to generate grid lines for mel scale image
        /// </summary>
        public int[] CreateMelYaxis(int herzInterval, int imageHt)
        {
            int minFreq = 0;
            int maxFreq = this.NyquistFrequency;
            //int freqRange = maxFreq - minFreq + 1;
            double minMel = Speech.Mel(minFreq);
            int melRange = (int)(Speech.Mel(maxFreq) - minMel + 1);
            //double pixelPerHz = imageHt / (double)freqRange;
            double pixelPerMel = imageHt / (double)melRange;
            int[] vScale = new int[imageHt];
            //Console.WriteLine("minMel=" + minMel.ToString("F1") + " melRange=" + melRange + " herzInterval=" + herzInterval + " imageHt=" + imageHt + " pixelPerMel=" + pixelPerMel);

            for (int f = minFreq + 1; f < maxFreq; f++)
            {
                if (f % 1000 == 0)  //convert freq value to pixel id
                {
                    //int hzOffset  = f - this.minFreq;
                    int melOffset = (int)(Speech.Mel(f) - minMel);
                    int pixelID = (int)(melOffset * pixelPerMel) + 1;
                    if (pixelID >= imageHt) pixelID = imageHt - 1;
                    //Console.WriteLine("f=" + f + " melOffset=" + melOffset + " pixelID=" + pixelID);
                    vScale[pixelID] = 1;
                }
            }
            return vScale;
        }



    } //end abstract class BaseSonogram





	public class SpectralSonogram : BaseSonogram
	{
        //There are three CONSTRUCTORS
        //Use the third constructor when you want to init a new Spectrogram by extracting portion of an existing sonogram.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="wav"></param>
        public SpectralSonogram(string configFile, WavReader wav)
			: this (SonogramConfig.Load(configFile), wav)
		{ }
		public SpectralSonogram(SonogramConfig config, WavReader wav)
			: base(config, wav)
		{ }
        public SpectralSonogram(SpectralSonogram sg, double startTime, double endTime)
            : base(sg.Configuration)
        {
            int startFrame = (int)Math.Round(startTime * sg.FramesPerSecond);
            int endFrame   = (int)Math.Round(endTime   * sg.FramesPerSecond);
            int frameCount = endFrame-startFrame + 1;

            //sg.MaxAmplitude { get; private set; }
            this.SampleRate = sg.SampleRate;
            this.Duration = TimeSpan.FromSeconds(endTime - startTime);
            //sg.FrameDuration ={ get { return Configuration.WindowSize / (double)SampleRate; } } // Duration of full frame or window in seconds
            //sg.FrameOffset { get { return FrameDuration * (1 - Configuration.WindowOverlap); } } // Duration of non-overlapped part of window/frame in seconds
            //sg.FBinWidth { get { return (SampleRate / 2) / (double)Configuration.FreqBinCount; } }
            //sg.FramesPerSecond { get { return 1 / FrameOffset; } }
            this.FrameCount = frameCount;

            ////energy and dB per frame
            //public SNR SnrFrames { get; private set; }
            this.DecibelsPerFrame = new double[frameCount];  // Normalised decibels per signal frame
            for(int i = 0; i < frameCount; i++) this.DecibelsPerFrame[i] = sg.DecibelsPerFrame[startFrame+i]; 

            ////energy and dB per frame sub-band
            this.ExtractSubband = this.ExtractSubband;
            this.subBand_MinHz  = sg.subBand_MinHz; //min freq (Hz) of the required subband
            this.subBand_MaxHz  = sg.subBand_MaxHz; //max freq (Hz) of the required subband
            //sg.SnrSubband { get; private set; }
            this.DecibelsInSubband = new double[frameCount];  // Normalised decibels in extracted freq band
            for(int i = 0; i < frameCount; i++) this.DecibelsInSubband[i] = sg.DecibelsInSubband[startFrame+i]; 

            //public double[] DecibelsNormalised { get; private set; } 
            this.Max_dBReference = sg.Max_dBReference; // Used to normalise the dB values for MFCCs
            this.DecibelsNormalised = new double[frameCount];
            for(int i = 0; i < frameCount; i++) this.DecibelsNormalised[i] = sg.DecibelsNormalised[startFrame+i];

            this.SigState = new int[frameCount];    //Integer coded signal state ie  0=non-vocalisation, 1=vocalisation, etc.
            for(int i = 0; i < frameCount; i++) this.SigState[i] = sg.SigState[startFrame+i]; 

            //the spectrogram data matrix
            int featureCount = sg.Data.GetLength(1);
            this.Data = new double[frameCount, featureCount];
            for(int i = 0; i < frameCount; i++) //each row of matrix is a frame
                for (int j = 0; j < featureCount; j++) //each col of matrix is a feature
                    this.Data[i, j] = sg.Data[startFrame + i, j];
        }//end CONSTRUCTOR



		protected override void Make(double[,] amplitudeM)
		{
            double[,] m = amplitudeM;
            if (Configuration.DoMelScale) m = ApplyFilterBank(m);
            //CONVERT AMPLITUDES TO DECIBELS
            m = Speech.DecibelSpectra(m);//convert amplitude spectrogram to dB spectrogram
            //NOISE REDUCTION
            if (Configuration.NoiseReductionType == ConfigKeys.NoiseReductionType.STANDARD)            m = NoiseReduce_Standard(m);
            else
            if (Configuration.NoiseReductionType == ConfigKeys.NoiseReductionType.FIXED_DYNAMIC_RANGE) m = NoiseReduce_FixedRange(m);
            
            this.Data = m; //store data matrix
		}

        /// <summary>
        /// Normalise the dynamic range of spectrogram between 0dB and value of DynamicRange.
        /// Also must adjust the SNR.DecibelsInSubband and this.DecibelsNormalised
        /// </summary>
        /// <param name="dynamicRange"></param>
        public void NormaliseDynamicRange(double dynamicRange)
        {
            int frameCount = this.Data.GetLength(0);
            int featureCount=this.Data.GetLength(1);
            double minIntensity; // min value in matrix
            double maxIntensity; // max value in matrix
            DataTools.MinMax(this.Data, out minIntensity, out maxIntensity);
            double[,] newMatrix = new double[frameCount,featureCount];

            for (int i = 0; i < frameCount; i++) //each row of matrix is a frame
                for (int j = 0; j < featureCount; j++) //each col of matrix is a feature
                {
                    newMatrix[i, j] = this.Data[i, j];
                }
            this.Data = newMatrix;
        }


        double[,] ApplyFilterBank(double[,] matrix)
        {
            //Log.WriteIfVerbose(" ApplyFilterBank(double[,] matrix)");
            int FFTbins = Configuration.FreqBinCount;  //number of Hz bands = 2^N +1. Subtract DC bin
            double[,] m = Speech.MelFilterBank(matrix, FFTbins, NyquistFrequency, 0, NyquistFrequency); // using the Greg integral
            return m;
        } //end ApplyFilterBank(double[,] matrix)
    
   } //end of class SpectralSonogram : BaseSonogram




    public class CepstralSonogram : BaseSonogram
    {
        public CepstralSonogram(string configFile, WavReader wav)
            : this(CepstralSonogramConfig.Load(configFile), wav)
        { }
        public CepstralSonogram(CepstralSonogramConfig config, WavReader wav)
            : base(config, wav)
        { }

        public double MaxMel { get; private set; }      // Nyquist frequency on Mel scale

        protected override void Make(double[,] amplitudeM)
        {
            var config = Configuration as CepstralSonogramConfig;
            Data = MakeCepstrogram(amplitudeM, this.DecibelsNormalised, config.MfccConfiguration.CcCount, config.MfccConfiguration.IncludeDelta, config.MfccConfiguration.IncludeDoubleDelta);
        }

        protected double[,] MakeCepstrogram(double[,] matrix, double[] decibels, int ccCount, bool includeDelta, bool includeDoubleDelta)
        {
            //NOTE!!!! The decibel array has been normalised in 0 - 1.
            Log.WriteIfVerbose(" MakeCepstrogram(matrix, decibels, includeDelta=" + includeDelta + ", includeDoubleDelta=" + includeDoubleDelta + ")");

            double[,] m = ApplyFilterBank(matrix);//also does mel scale conversion
            m = Speech.DecibelSpectra(m);

            //NOISE REDUCTION
            if (Configuration.NoiseReductionType == ConfigKeys.NoiseReductionType.STANDARD) m = NoiseReduce_Standard(m);
            else
            if (Configuration.NoiseReductionType == ConfigKeys.NoiseReductionType.FIXED_DYNAMIC_RANGE) m = NoiseReduce_FixedRange(m);

            //calculate cepstral coefficients and normalise
            m = Speech.Cepstra(m, ccCount);
            m = DataTools.normalise(m);
            //calculate the full range of MFCC coefficients ie including decibel and deltas, etc
            return Speech.AcousticVectors(m, decibels, includeDelta, includeDoubleDelta);
        }

        double[,] ApplyFilterBank(double[,] matrix)
        {
            Log.WriteIfVerbose(" ApplyFilterBank(double[,] matrix)");
            //error check that filterBankCount < FFTbins
            int FFTbins = Configuration.FreqBinCount;  //number of Hz bands = 2^N +1. Subtract DC bin
            var config = Configuration as CepstralSonogramConfig;
            if (config.MfccConfiguration.FilterbankCount > FFTbins)
                throw new Exception("####### FATAL ERROR:- Sonogram.ApplyFilterBank():- Cannot calculate cepstral coefficients. FilterbankCount > FFTbins. (" + config.MfccConfiguration.FilterbankCount + " > " + FFTbins + ")\n\n");

            //this is the filter count for full bandwidth 0-Nyquist. This number is trimmed proportionately to fit the required bandwidth. 
            int bandCount = config.MfccConfiguration.FilterbankCount;
            double[,] m = matrix;
            Log.WriteIfVerbose("\tDim prior to filter bank  =" + m.GetLength(1));

            if (config.MfccConfiguration.DoMelScale)
                m = Speech.MelFilterBank(m, bandCount, NyquistFrequency, Configuration.MinFreqBand ?? 0, Configuration.MaxFreqBand ?? NyquistFrequency); // using the Greg integral
            else
                m = Speech.LinearFilterBank(m, bandCount, NyquistFrequency, Configuration.MinFreqBand ?? 0, Configuration.MaxFreqBand ?? NyquistFrequency);
            Log.WriteIfVerbose("\tDim after use of filter bank=" + m.GetLength(1) + " (Max filter bank=" + bandCount + ")");

            return m;
        } //end ApplyFilterBank(double[,] matrix)
    }


	public class AcousticVectorsSonogram : CepstralSonogram
	{
		public AcousticVectorsSonogram(string configFile, WavReader wav)
            : base(CepstralSonogramConfig.Load(configFile), wav)
		{ }

        public AcousticVectorsSonogram(CepstralSonogramConfig config, WavReader wav)
			: base(config, wav)
		{ }

		protected override void Make(double[,] amplitudeM)
		{
            var config = Configuration as CepstralSonogramConfig;
            Data = MakeAcousticVectors(amplitudeM, this.DecibelsNormalised, config.MfccConfiguration.CcCount, config.MfccConfiguration.IncludeDelta, config.MfccConfiguration.IncludeDoubleDelta, config.DeltaT);
        }

		double[,] MakeAcousticVectors(double[,] matrix, double[] decibels, int ccCount, bool includeDelta, bool includeDoubleDelta, int deltaT)
		{
			Log.WriteIfVerbose(" MakeAcousticVectors(matrix, decibels, includeDelta=" + includeDelta + ", includeDoubleDelta=" + includeDoubleDelta + ", deltaT=" + deltaT + ")");

			double[,] m = MakeCepstrogram(matrix, decibels, ccCount, includeDelta, includeDoubleDelta);

			//initialise feature vector for template - will contain three acoustic vectors - for T-dT, T and T+dT
			int frameCount = m.GetLength(0);
			int cepstralL = m.GetLength(1);  // length of cepstral vector 
			int featurevL = 3 * cepstralL;   // to accomodate cepstra for T-2, T and T+2

			double[,] acousticM = new double[frameCount, featurevL]; //init the matrix of acoustic vectors
			for (int i = deltaT; i < frameCount - deltaT; i++)
			{
				double[] rowTm2 = DataTools.GetRow(m, i - deltaT);
				double[] rowT   = DataTools.GetRow(m, i);
				double[] rowTp2 = DataTools.GetRow(m, i + deltaT);

				for (int j = 0; j < cepstralL; j++) acousticM[i, j] = rowTm2[j];
				for (int j = 0; j < cepstralL; j++) acousticM[i, cepstralL + j] = rowT[j];
				for (int j = 0; j < cepstralL; j++) acousticM[i, cepstralL + cepstralL + j] = rowTp2[j];
			}

			return acousticM;
		}
	} //end class AcousticVectorsSonogram : CepstralSonogram






	public class SobelEdgeSonogram : BaseSonogram
	{
		public SobelEdgeSonogram(string configFile, WavReader wav)
			: base(SonogramConfig.Load(configFile), wav)
		{ }

		public SobelEdgeSonogram(SonogramConfig config, WavReader wav)
			: base(config, wav)
		{ }

		protected override void Make(double[,] amplitudeM)
		{
			Data = SobelEdgegram(amplitudeM);
		}

		double[,] SobelEdgegram(double[,] matrix)
		{
			double[,] m = Speech.DecibelSpectra(matrix);
            //NOISE REDUCTION
            if (Configuration.NoiseReductionType == ConfigKeys.NoiseReductionType.STANDARD) m = NoiseReduce_Standard(m);
            else
            if (Configuration.NoiseReductionType == ConfigKeys.NoiseReductionType.FIXED_DYNAMIC_RANGE) m = NoiseReduce_FixedRange(m);
            return ImageTools.SobelEdgeDetection(m);
		}
    }// end SobelEdgeSonogram : BaseSonogram
}