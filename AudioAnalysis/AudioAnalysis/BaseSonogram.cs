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
		public BaseSonogramConfig Configuration { get; private set; }

		public double MaxAmplitude { get; private set; }
		public int SampleRate { get; private set; }
		public int NyquistFrequency { get { return SampleRate / 2; } }
		public TimeSpan Duration { get; private set; }

		public double FrameDuration { get { return Configuration.WindowSize / (double)SampleRate; } } // Duration of full frame or window in seconds
		public double FrameOffset { get { return FrameDuration * (1 - Configuration.WindowOverlap); } } // Duration of non-overlapped part of window/frame in seconds
		public double FBinWidth { get { return (SampleRate / 2) / (double)Configuration.FreqBinCount; } }
		public double FramesPerSecond { get { return 1 / FrameOffset; } }
		public int FrameCount { get; private set; } // Originally temporarily set to (int)(Duration.TotalSeconds / FrameOffset) then reset later

        public double[] LogEnergy { get; private set; } // Energy per signal frame
		public double[] Decibels { get; private set; } // Normalised decibels per signal frame

		public double NoiseSubtracted { get; private set; } // Noise (dB) subtracted from each frame decibel value
		public double FrameMax_dB { get; private set; }
		public double FrameMin_dB { get; private set; }
		public double Frame_SNR { get { return FrameMax_dB - NoiseSubtracted; } }
        //sonogram.FrameMax_dB - sonogram.NoiseSubtracted;
		public double MinDecibelReference { get; private set; } // Min reference dB value after noise substraction
        public double MaxDecibelReference { get; private set; } // Used to normalise the energy values for MFCCs
        public double SegmentationThresholdK1 { get; private set; }
        public double SegmentationThresholdK2 { get; private set; } 

        public bool   ExtractSubband { get; set; } // extract sub-band when making spectrogram image
        private int   freqBand_Min;
        private int   freqBand_Max;
		public double FreqBandMax_dB { get; private set; }
		public double FreqBandNoise_dB { get; private set; }
		public double FreqBand_SNR { get; private set; }

		public int[] SigState { get; private set; } // Integer coded signal state ie  0=non-vocalisation, 1=vocalisation, etc.

		public double[,] Data { get; protected set; } //the spectrogram data matrix
		#endregion



        public BaseSonogram(BaseSonogramConfig config, WavReader wav, bool doExtractSubband)
		{
			Configuration = config;
            this.ExtractSubband = doExtractSubband;

			SampleRate = wav.SampleRate;
			Duration = wav.Time;

			MaxAmplitude = wav.CalculateMaximumAmplitude();

			this.freqBand_Min = config.MinFreqBand ?? 0;
			this.freqBand_Max = config.MaxFreqBand ?? NyquistFrequency;
            //bool ExtractSubband = this.freqBand_Min > 0 || this.freqBand_Max < NyquistFrequency;

			double[] signal = wav.Samples;

			// SIGNAL PRE-EMPHASIS helps with speech signals
			if (config.DoPreemphasis)
				signal = DSP.PreEmphasis(signal, 0.96);

			// FRAME WINDOWING
			int step = (int)(config.WindowSize * (1 - config.WindowOverlap));
			double[,] frames = DSP.Frames(signal, config.WindowSize, step);
			FrameCount = frames.GetLength(0);


			// ENERGY PER FRAME
            LogEnergy = DSP.SignalLogEnergy(frames);

			// FRAME NOISE SUBTRACTION: subtract background noise to produce decibels array in which zero dB = average noise
			double Q;
			double min_dB;
			double max_dB;
            double minEnergyRatio = DSP.MinEnergyReference - DSP.MaxEnergyReference;
            Decibels = DSP.NoiseSubtract(LogEnergy, out min_dB, out max_dB, minEnergyRatio, out Q);
			NoiseSubtracted = Q;
			FrameMin_dB = min_dB; //min decibels of all frames 
			FrameMax_dB = max_dB;
			MinDecibelReference = min_dB - Q;
            MaxDecibelReference = (DSP.MaxEnergyReference * 10) - Q;

			// ZERO CROSSINGS
			//this.zeroCross = DSP.ZeroCrossings(frames);

            //AUDIO SEGMENTATION
			double k1; double k2; int k1_k2delay; int syllableDelay; int minPulse;
			SigState = DetermineEndpointsOfVocalisations(out k1, out k2, out k1_k2delay, out syllableDelay, out minPulse);
            this.SegmentationThresholdK1 = k1;
            this.SegmentationThresholdK2 = k2;


			var fractionOfHighEnergyFrames = Speech.FractionHighEnergyFrames(Decibels, k2);
			if ((fractionOfHighEnergyFrames > 0.8) && (Configuration.DoNoiseReduction))
			{
				Log.WriteLine("\n\t################### Sonogram.Make(WavReader wav): WARNING ##########################################");
				Log.WriteLine("\t################### This is a high energy recording. The fraction of high energy frames = "
																+ fractionOfHighEnergyFrames.ToString("F2") + " > 80%");
				Log.WriteLine("\t################### Noise reduction algorithm may not work well in this instance!\n");
			}

			//generate the spectra of FFT AMPLITUDES
			//calculate a minimum amplitude to prevent taking log of small number. This would increase the range when normalising
			double epsilon = Math.Pow(0.5, wav.BitsPerSample - 1);
			var amplitudeM = MakeAmplitudeSpectra(frames, TowseyLib.FFT.GetWindowFunction(FftConfiguration.WindowFunction), epsilon);
			//Log.WriteIfVerbose("\tDim of amplitude spectrum =" + amplitudeM.GetLength(1));

			//EXTRACT REQUIRED FREQUENCY BAND
            if (ExtractSubband)
			{
				int c1 = (int)(this.freqBand_Min / FBinWidth);
				int c2 = (int)(this.freqBand_Max / FBinWidth);
				amplitudeM = DataTools.Submatrix(amplitudeM, 0, c1, amplitudeM.GetLength(0) - 1, c2);
				Log.WriteIfVerbose("\tDim of required sub-band  =" + amplitudeM.GetLength(1));
				//DETERMINE ENERGY IN FFT FREQ BAND
				Decibels = FreqBandEnergy(amplitudeM);
				//DETERMINE ENDPOINTS OF VOCALISATIONS
				SigState = Speech.VocalizationDetection(Decibels, k1, k2, k1_k2delay, syllableDelay, minPulse, null);
			}

			Make(amplitudeM);
		} //end Make(WavReader wav)

        /// <summary>
        /// WARNING: calculation of k1 and k2 is faulty.
        /// MinDecibelReference should not be used ie k1 = EndpointDetectionConfiguration.SegmentationThresholdK1;
        /// See the alternative below
        /// 
        /// ************* PARAMETERS FOR:- ENDPOINT DETECTION of VOCALISATIONS 
        /// See Lamel et al 1981.
        /// They use k1, k2, k3 and k4, minimum pulse length and k1_k2Latency.
        /// Here we set k1 = k3, k4 = k2,  k1_k2Latency = 0.186s (5 frames)
        ///                  and "minimum pulse length" = 0.075s (2 frames) 
        /// SEGMENTATION_THRESHOLD_K1 = decibels above the minimum level
        /// SEGMENTATION_THRESHOLD_K2 = decibels above the minimum level
        /// K1_K2_LATENCY = seconds delay between signal reaching k1 and k2 thresholds
        /// VOCAL_DELAY = seconds delay required to separate vocalisations 
        /// MIN_VOCAL_DURATION = minimum length of energy pulse - do not use this - accept all pulses.
        /// SEGMENTATION_THRESHOLD_K1=3.5
        /// SEGMENTATION_THRESHOLD_K2=6.0
        /// K1_K2_LATENCY=0.05
        /// VOCAL_DELAY=0.2
        /// </summary>
        /// <param name="k1"></param>
        /// <param name="k2"></param>
        /// <param name="k1_k2delay"></param>
        /// <param name="syllableDelay"></param>
        /// <param name="minPulse"></param>
        /// <returns></returns>
		int[] DetermineEndpointsOfVocalisations(out double k1, out double k2, out int k1_k2delay, out int syllableDelay, out int minPulse)
		{
            //k1 = MinDecibelReference + EndpointDetectionConfiguration.SegmentationThresholdK1;
            //k2 = MinDecibelReference + EndpointDetectionConfiguration.SegmentationThresholdK2;
            k1 = EndpointDetectionConfiguration.SegmentationThresholdK1;
            k2 = EndpointDetectionConfiguration.SegmentationThresholdK2;
            k1_k2delay = (int)(EndpointDetectionConfiguration.K1K2Latency / FrameOffset); //=5  frames delay between signal reaching k1 and k2 thresholds
            syllableDelay = (int)(EndpointDetectionConfiguration.VocalDelay / FrameOffset); //=10 frames delay required to separate vocalisations 
            minPulse = (int)(EndpointDetectionConfiguration.MinPulseDuration / FrameOffset); //=2 frames is min vocal length
			//Console.WriteLine("k1_k2delay=" + k1_k2delay + "  syllableDelay=" + syllableDelay + "  minPulse=" + minPulse);
			return Speech.VocalizationDetection(this.Decibels, k1, k2, k1_k2delay, syllableDelay, minPulse, null);
		}

		double[,] MakeAmplitudeSpectra(double[,] frames, TowseyLib.FFT.WindowFunc w, double epsilon)
		{
			int frameCount = frames.GetLength(0);
			int N = frames.GetLength(1);  //= the FFT windowSize 
			int binCount = (N / 2) + 1;  // = fft.WindowSize/2 +1 for the DC value;

			var fft = new TowseyLib.FFT(N, w); // init class which calculates the FFT

			//calculate a minimum amplitude to prevent taking log of small number. This would increase the range when normalising
			int smoothingWindow = 3; //to smooth the spectrum 

			double[,] sonogram = new double[frameCount, binCount];

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
					sonogram[i, j] = amplitude;
				}
			} //end of all frames
			return sonogram;
		}

		double[] FreqBandEnergy(double[,] fftAmplitudes)
		{
            double[] logEnergy = DSP.SignalLogEnergy(fftAmplitudes);

			//NOTE: FreqBand LogEnergy levels are higher than Frame levels but SNR remains same.
			//double min; double max;
			//DataTools.MinMax(logEnergy, out min, out max);
			//Console.WriteLine("FrameNoise_dB   =" + State.FrameNoise_dB    + "  FrameMax_dB   =" + State.FrameMax_dB    + "  SNR=" + State.Frame_SNR);
			//Console.WriteLine("FreqBandNoise_dB=" + State.FreqBandNoise_dB + "  FreqBandMax_dB=" + State.FreqBandMax_dB + "  SNR=" + State.FreqBand_SNR);
			//Console.WriteLine("FreqBandNoise_dB=" + (min*10) + "  FreqBandMax_dB=" + (max*10) + "  SNR=" + State.FreqBand_SNR);

			//noise reduce the energy array to produce decibels array
            double minEnergyRatio = DSP.MinEnergyReference - DSP.MaxEnergyReference;
            double Q; double min_dB; double max_dB;
            double[] decibels = DSP.NoiseSubtract(logEnergy, out min_dB, out max_dB, minEnergyRatio, out Q);
			NoiseSubtracted = Q;
			FreqBandNoise_dB = min_dB; //min decibels of all frames 
			FreqBandMax_dB = max_dB;
			FreqBand_SNR = max_dB - min_dB;
			MinDecibelReference = min_dB - NoiseSubtracted;
			return decibels;
		}



		protected abstract void Make(double[,] amplitudeM);


		public Image GetImage()
		{
			return GetImage(1, false, false);
		}

        public Image GetImage(bool doHighlightSubband, bool add1kHzLines)
		{
            Log.WriteIfVerbose("BaseSonogram.GetImage(bool doHighlightSubband, bool add1kHzLines)");
            Log.WriteIfVerbose("    doHighlightSubband=" + doHighlightSubband + "   add1kHzLines=" + add1kHzLines);
            return GetImage(1, doHighlightSubband, add1kHzLines);
		}

		protected virtual Image GetImage(int binHeight, bool doHighlightSubband, bool add1kHzLines)
		{
			var data = Data;
			int width = data.GetLength(0); // Number of spectra in sonogram
            int fftBins = data.GetLength(1);
            int imageHeight = fftBins * binHeight; // image ht = sonogram ht. Later include grid and score scales
            
            //set up min, max, range for normalising of dB values
			double min; double max;
			DataTools.MinMax(data, out min, out max);
			double range = max - min;

            //int? minHighlightFreq = this.freqBand_Min;
            //int? maxHighlightFreq = this.freqBand_Max;
            //int minHighlightBin = (minHighlightFreq == null) ? 0 : (int)Math.Round((double)minHighlightFreq / (double)NyquistFrequency * fftBins);
            //int maxHighlightBin = (maxHighlightFreq == null) ? 0 : (int)Math.Round((double)maxHighlightFreq / (double)NyquistFrequency * fftBins);
            //calculate top and bottom of sub-band 
            int minHighlightBin = (int)Math.Round((double)this.freqBand_Min / (double)NyquistFrequency * fftBins);
            int maxHighlightBin = (int)Math.Round((double)this.freqBand_Max / (double)NyquistFrequency * fftBins);
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
            double[] logEnergy = this.LogEnergy;
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
                    if (maxE < LogEnergy[x])
                    {
                        maxE = LogEnergy[x];
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
                Color gridCol = Color.Black;
                if ((w % 2) == 0) gridCol = Color.White;
                for (int p = 0; p < vScale.Length; p++) //over all Y-axis pixels
                {
                    if (vScale[p] == 0) continue;
                    int y = imageHeight - p;
                    bmp.SetPixel(w, y, gridCol);
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
            //if (this.doMelScale) vScale = CreateMelYaxis(kHz, sHeight);

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
        //There are four CONSTRUCTORS
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="wav"></param>
		public SpectralSonogram(string configFile, WavReader wav)
			: this (BaseSonogramConfig.Load(configFile), wav)
		{ }
		public SpectralSonogram(BaseSonogramConfig config, WavReader wav)
			: base(config, wav, false)
		{ }
        public SpectralSonogram(BaseSonogramConfig config, WavReader wav, bool doExtractSubband)
            : base(config, wav, doExtractSubband)
        { }





		protected override void Make(double[,] amplitudeM)
		{
			Data = MakeSpectrogram_fullBandWidth(amplitudeM);
		}

		/// <summary>
		/// Converts amplitude spectra to power spectra
		/// Does NOT apply filter bank i.e. returns full bandwidth spectrogram
		/// </summary>
		double[,] MakeSpectrogram_fullBandWidth(double[,] amplitudeM)
		{
            Log.WriteIfVerbose("BaseSonogram.MakeSpectrogram_fullBandWidth(double[,] matrix)");
            double[,] m = Speech.DecibelSpectra(amplitudeM);//convert amplitude spectrogram to dB spectrogram

			if (Configuration.DoNoiseReduction)
			{
				Log.WriteIfVerbose("\t... doing noise reduction.");
				m = ImageTools.NoiseReduction(m); //Mel scale conversion should be done before noise reduction
			}
			return m;
		}
	}




    public class CepstralSonogram : BaseSonogram
    {
        public CepstralSonogram(string configFile, WavReader wav)
            : this(CepstralSonogramConfig.Load(configFile), wav)
        { }

        public CepstralSonogram(CepstralSonogramConfig config, WavReader wav)
            : base(config, wav, false)
        { }

        public double MaxMel { get; private set; }      // Nyquist frequency on Mel scale

        protected override void Make(double[,] amplitudeM)
        {
            var config = Configuration as CepstralSonogramConfig;
            Data = MakeCepstrogram(amplitudeM, Decibels, config.MfccConfiguration.CcCount, config.MfccConfiguration.IncludeDelta, config.MfccConfiguration.IncludeDoubleDelta);
        }

        protected double[,] MakeCepstrogram(double[,] matrix, double[] decibels, int ccCount, bool includeDelta, bool includeDoubleDelta)
        {
            Log.WriteIfVerbose(" MakeCepstrogram(matrix, decibels, includeDelta=" + includeDelta + ", includeDoubleDelta=" + includeDoubleDelta + ")");

            double[,] m = ApplyFilterBank(matrix);
            m = Speech.DecibelSpectra(m);

            if (Configuration.DoNoiseReduction)
            {
                Log.WriteIfVerbose("\t... doing noise reduction.");
                m = ImageTools.NoiseReduction(m); //Mel scale conversion should be done before noise reduction
            }

            // not sure if we really need this... commented out for the moment because it'll use lots of memory
            //SpectralM = m; //stores the reduced bandwidth, filtered, noise reduced spectra as new spectrogram

            //calculate cepstral coefficients and normalise
            m = Speech.Cepstra(m, ccCount);
            m = DataTools.normalise(m);

            //calculate the full range of MFCC coefficients ie including energy and deltas, etc
            //normalise energy between 0.0 decibels and max decibels.
            double[] E = Speech.NormaliseDecibelArray(decibels, MaxDecibelReference);
            return Speech.AcousticVectors(m, E, includeDelta, includeDoubleDelta);
        }

        double[,] ApplyFilterBank(double[,] matrix)
        {
            Log.WriteIfVerbose(" ApplyFilterBank(double[,] matrix)");
            //error check that filterBankCount < FFTbins
            int FFTbins = Configuration.FreqBinCount;  //number of Hz bands = 2^N +1. Subtract DC bin
            var config = Configuration as CepstralSonogramConfig;
            if (config.MfccConfiguration.FilterbankCount > FFTbins)
                throw new Exception("####### FATAL ERROR:- Sonogram.ApplyFilterBank():- Cannot calculate cepstral coefficients. FilterbankCount > FFTbins. (" + config.MfccConfiguration.FilterbankCount + " > " + FFTbins + ")\n\n");

            MaxMel = Speech.Mel(NyquistFrequency);
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
			: base(AVSonogramConfig.Load(configFile), wav)
		{ }

		public AcousticVectorsSonogram(AVSonogramConfig config, WavReader wav)
			: base(config, wav)
		{ }

		protected override void Make(double[,] amplitudeM)
		{
			var config = Configuration as AVSonogramConfig;
			Data = MakeAcousticVectors(amplitudeM, Decibels, config.MfccConfiguration.CcCount, config.MfccConfiguration.IncludeDelta, config.MfccConfiguration.IncludeDoubleDelta, config.DeltaT);
		}

		double[,] MakeAcousticVectors(double[,] matrix, double[] decibels, int ccCount, bool includeDelta, bool includeDoubleDelta, int deltaT)
		{
			Log.WriteIfVerbose(" MakeAcousticVectors(matrix, decibels, includeDelta=" + includeDelta + ", includeDoubleDelta=" + includeDoubleDelta + ", deltaT=" + deltaT + ")");

			double[,] m = MakeCepstrogram(matrix, decibels, ccCount, includeDelta, includeDoubleDelta);

			//initialise feature vector for template - will contain three acoustic vectors - for T-dT, T and T+dT
			int frameCount = m.GetLength(0);
			int cepstralL = m.GetLength(1);  // length of cepstral vector 
			int featurevL = 3 * cepstralL;   // to accomodate cepstra for T-2, T and T+2

			double[] featureVector = new double[featurevL];
			double[,] acousticM = new double[frameCount, featurevL]; //init the matrix of acoustic vectors
			for (int i = deltaT; i < frameCount - deltaT; i++)
			{
				double[] rowTm2 = DataTools.GetRow(m, i - deltaT);
				double[] rowT = DataTools.GetRow(m, i);
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
			: base(BaseSonogramConfig.Load(configFile), wav, false)
		{ }

		public SobelEdgeSonogram(BaseSonogramConfig config, WavReader wav)
			: base(config, wav, false)
		{ }

		protected override void Make(double[,] amplitudeM)
		{
			Data = SobelEdgegram(amplitudeM);
		}

		double[,] SobelEdgegram(double[,] matrix)
		{
			double[,] m = Speech.DecibelSpectra(matrix);
			if (Configuration.DoNoiseReduction)
				m = ImageTools.NoiseReduction(m);
			return ImageTools.SobelEdgeDetection(m);
		}
    }// end SobelEdgeSonogram : BaseSonogram
}