namespace AudioAnalysisTools
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Acoustics.Tools.Wav;

    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.DSP;

    using TowseyLibrary;
    using System.Collections.Generic;

    /// <summary>
    /// Sonogram type.
    /// </summary>
    public enum SonogramType
    {
        /// <summary>
        /// Ampltude Sonogram.
        /// </summary>
        Amplitude, 

        /// <summary>
        /// Spectral Sonogram.
        /// </summary>
        Spectral, 

        /// <summary>
        /// Cepstral Sonogram.
        /// </summary>
        Cepstral, 

        /// <summary>
        /// Acoustic Vectors Sonogram.
        /// </summary>
        AcousticVectors, 

        /// <summary>
        /// Sobel Edge Sonogram.
        /// </summary>
        SobelEdge
    }

    /// <summary>
    /// Base Sonogram.
    /// </summary>
    public abstract partial class BaseSonogram : IDisposable
    {

        #region Properties
        public SonogramConfig Configuration { get; set; }

        public double MaxAmplitude { get; set; }
        public int SampleRate { get; set; }
        public TimeSpan Duration { get; protected set; }

        // the following values are dependent on sampling rate.
        public int NyquistFrequency { get { return SampleRate / 2; } }

        // Duration of full frame or window in seconds
        public double FrameDuration { get { return Configuration.WindowSize / (double)SampleRate; } }
        
        // Duration of non-overlapped part of window/frame in seconds
        public double FrameStep { get { return this.Configuration.GetFrameOffset(SampleRate); } }
        //public double FrameStep { get { return FrameDuration * (1 - Configuration.WindowOverlap); } }
        
        public double FBinWidth { get { return (SampleRate / 2) / (double)Configuration.FreqBinCount; } }// FreqBinCount=WindowSize/2
        public double FramesPerSecond { get { return 1 / FrameStep; } }
        public int FrameCount { get; protected set; } //Temporarily set to (int)(Duration.TotalSeconds/FrameOffset) then reset later

        // energy and dB per frame
        public SNR SnrFullband { get; set; }
        public double[] DecibelsPerFrame { get { return SnrFullband.Decibels; } protected set { } }//decibels per signal frame

        // energy and dB per frame sub-band
        protected int subBand_MinHz; //min freq (Hz) of the required subband
        protected int subBand_MaxHz; //max freq (Hz) of the required subband
        public SNR SnrSubband { get; private set; }
        public double[] DecibelsInSubband { get; protected set; }  // Normalised decibels in extracted freq band

        public double[] DecibelsNormalised { get; set; }
        public double Max_dBReference { get; protected set; } // Used to normalise the dB values for MFCCs

        public int[] SigState { get; protected set; }   // Integer coded signal state ie  0=non-vocalisation, 1=vocalisation, etc.

        public double[,] Data { get; set; } //the spectrogram data matrix, AFTER conversion to dB and noise removal 
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
        /// This constructor contains all steps required to prepare the amplitude spectrogram.
        /// The third boolean parameter is simply a placefiller to ensure a different Constructor signature
        /// from the principle Constructor which follows.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="wav"></param>
        /// <param name="doExtractSubband"></param>
        public BaseSonogram(SonogramConfig config, WavReader wav, bool dummy)
            : this(config)
        {
            bool DoExtractSubband = this.subBand_MinHz > 0 || this.subBand_MaxHz < NyquistFrequency;
            if (config.DoFullBandwidth) DoExtractSubband = false;   //if sono only intended for image

            //set config params to the current recording           
            this.SampleRate = wav.SampleRate;
            this.Configuration.Duration = wav.Time;
            this.Configuration.SampleRate = wav.SampleRate; //also set the Nyquist
            this.Duration = wav.Time;
            double minDuration = 1.0;
            if (this.Duration.TotalSeconds < minDuration)
            {
                LoggedConsole.WriteLine("Signal must at least {0} seconds long to produce a sonogram!", minDuration);
                return;
            }
            this.MaxAmplitude = wav.CalculateMaximumAmplitude();
            double[] signal = wav.GetChannel(config.Channel);

            //calculate a signal dependent minimum amplitude value to prevent possible subsequent log of zero value.
            this.Configuration.epsilon = Math.Pow(0.5, wav.BitsPerSample - 1);

            // SIGNAL PRE-EMPHASIS helps with speech signals
            if (config.DoPreemphasis) signal = DSP_Filters.PreEmphasis(signal, 0.96);

            // FRAME WINDOWING
            int[,] frameIDs = DSP_Frames.FrameStartEnds(signal.Length, config.WindowSize, config.WindowOverlap);
            FrameCount = frameIDs.GetLength(0);

            // ENERGY PER FRAME and NORMALISED dB PER FRAME AND SNR
            if (config.DoSnr)
            {
                this.SnrFullband = new SNR(signal, frameIDs);
                this.Max_dBReference = SnrFullband.MaxReference_dBWrtNoise;  // Used to normalise the dB values for feature extraction
                this.DecibelsNormalised = SnrFullband.NormaliseDecibelArray_ZeroOne(this.Max_dBReference);

                var fractionOfHighEnergyFrames = SnrFullband.FractionHighEnergyFrames(EndpointDetectionConfiguration.K2Threshold);
                if (fractionOfHighEnergyFrames > SNR.FRACTIONAL_BOUND_FOR_MODE)
                {
                    Log.WriteIfVerbose("\nWARNING ##############");
                    Log.WriteIfVerbose("\t################### BaseSonogram(): This is a high energy recording. Percent of high energy frames = {0:f0} > {1:f0}%",
                                              fractionOfHighEnergyFrames * 100, SNR.FRACTIONAL_BOUND_FOR_MODE * 100);
                    Log.WriteIfVerbose("\t################### Noise reduction algorithm may not work well in this instance!\n");
                }

                //AUDIO SEGMENTATION/END POINT DETECTION - based on Lamel
                // next line is a hack since setting segmentation/endpoinht detection parameters apepars to be broken as of September 2014.
                EndpointDetectionConfiguration.SetDefaultSegmentationConfig();
                SigState = EndpointDetectionConfiguration.DetermineVocalisationEndpoints(DecibelsPerFrame, this.FrameStep);

            }

            //generate the spectra of FFT AMPLITUDES
            //var amplitudeM = MakeAmplitudeSonogram(frames, TowseyLib.FFT.GetWindowFunction(this.Configuration.FftConfig.WindowFunction));
            FFT.WindowFunc w = FFT.GetWindowFunction(this.Configuration.WindowFunction);
            double power;
            var amplitudeM = BaseSonogram.MakeAmplitudeSonogram(signal, frameIDs, w, out power);
            this.Configuration.WindowPower = power;

            //EXTRACT REQUIRED FREQUENCY BAND
            if (DoExtractSubband)
            {
                amplitudeM = SpectrogramTools.ExtractFreqSubband(amplitudeM, this.subBand_MinHz, this.subBand_MaxHz,
                             this.Configuration.DoMelScale, this.Configuration.FreqBinCount, this.FBinWidth);
                Log.WriteIfVerbose("\tDim of required sub-band =" + amplitudeM.GetLength(1));
                CalculateSubbandSNR(amplitudeM);
            }

            this.Data = amplitudeM;
        } //end CONSTRUCTOR BaseSonogram(SonogramConfig config, WavReader wav, bool dummy)


        /// <summary>
        /// this constructor is the one most used - it automatically makes the Amplitude spectrum and 
        /// then, using a call to Make(), it converts the Amplitude matrix to a Spectrogram whose values are decibels. 
        /// </summary>
        /// <param name="config">All parameters required to make spectrogram</param>
        /// <param name="wav">the recording whose spectrogram is to be made</param>
        public BaseSonogram(SonogramConfig config, WavReader wav)
            : this(config, wav, false)
        {
            Make(this.Data);
        } //end CONSTRUCTOR BaseSonogram(SonogramConfig config, WavReader wav)


        /// <summary>
        /// use this constructor when already have the amplitude spectorgram in matrix
        /// Init normalised signal energy array but do nothing with it. This has to be done from outside
        /// </summary>
        /// <param name="config"></param>
        public BaseSonogram(SonogramConfig config, double[,] amplitudeSpectrogram)
        {
            Configuration = config;
            this.FrameCount = amplitudeSpectrogram.GetLength(0);
            this.SampleRate = this.Configuration.SampleRate;

            //init normalised signal energy array but do nothing with it. This has to be done from outside
            this.DecibelsNormalised = new double[this.FrameCount];
            
            this.Data = amplitudeSpectrogram;
            Make(this.Data);
        }


        public abstract void Make(double[,] amplitudeM);


        public double[,] ExtractFreqSubband(WavReader wav, int minHz, int maxHz)
        {
            //double[,] frames = DSP.Frames(wav.Samples, this.Configuration.WindowSize, this.Configuration.WindowOverlap);
            int[,] framesIDs = DSP_Frames.FrameStartEnds(wav.Samples.Length, this.Configuration.WindowSize, this.Configuration.WindowOverlap);
            //calculate a minimum amplitude to prevent taking log of small number. This would increase the range when normalising
            //double epsilon = Math.Pow(0.5, wav.BitsPerSample - 1);

            //var amplitudeM = MakeAmplitudeSonogram(frames, TowseyLib.FFT.GetWindowFunction(this.Configuration.FftConfig.WindowFunction));
            double power;
            var amplitudeM = MakeAmplitudeSonogram(wav.Samples, framesIDs, FFT.GetWindowFunction(this.Configuration.WindowFunction), out power);
            //this.ExtractSubband = true;
            this.subBand_MinHz = minHz;
            this.subBand_MaxHz = maxHz;
            return SpectrogramTools.ExtractFreqSubband(amplitudeM, minHz, maxHz, this.Configuration.DoMelScale,
                                                   this.Configuration.FreqBinCount, this.FBinWidth);
        }


        public void CalculateSubbandSNR(WavReader wav, int minHz, int maxHz)
        {
            var subband = ExtractFreqSubband(wav, minHz, maxHz);
            CalculateSubbandSNR(subband);
        }

        public void CalculateSubbandSNR(double[,] subband)
        {
            this.SnrSubband = new SNR(subband); //subband is the amplitude values
            //RECALCULATE DecibelsNormalised and dB REFERENCE LEVEL - need for MFCCs
            this.DecibelsInSubband = SnrSubband.Decibels;
            this.Max_dBReference = SnrSubband.MaxReference_dBWrtNoise;
            this.DecibelsNormalised = SnrSubband.NormaliseDecibelArray_ZeroOne(this.Max_dBReference);
            //RECALCULATE ENDPOINTS OF VOCALISATIONS
            SigState = EndpointDetectionConfiguration.DetermineVocalisationEndpoints(this.DecibelsInSubband, this.FrameStep);
        }

        public void SetTimeScale(TimeSpan duration)
        {
            this.Duration = duration; 
        }

        public void SetBinarySpectrum(byte[,] binary)
        {
            int rows = binary.GetLength(0);
            int cols = binary.GetLength(1);
            var m = new double[rows, cols];
            for (int r = 0; r < rows; r++) //init matrix to min
            {
                for (int c = 0; c < cols; c++) m[r, c] = (double)binary[r, c];
            }

            this.Data = m;
        }

        public Image GetImageFullyAnnotated(string title)
        {
            Image image = this.GetImage();
            image = this.GetImageFullyAnnotated(image, title);
            return image;
        }

        public Image GetColourAmplitudeSpectrogramFullyAnnotated(string title, double[,] dbSpectrogramData, double[,] nrSpectrogramData, byte[,] hits)
        {
            Image image = SpectrogramTools.CreateFalseColourAmplitudeSpectrogram(dbSpectrogramData, nrSpectrogramData, hits);
            image = this.GetImageFullyAnnotated(image, title);
            return image;
        }

        public Image GetColourDecibelSpectrogramFullyAnnotated(string title, double[,] dbSpectrogramData, double[,] nrSpectrogramData, byte[,] hits)
        {
            Image image = SpectrogramTools.CreateFalseColourDecibelSpectrogram(dbSpectrogramData, nrSpectrogramData, hits);
            image = this.GetImageFullyAnnotated(image, title);
            return image;
        }


        public Image GetImageFullyAnnotated(string title, int[,] gridLineLocations)
        {
            Image image = this.GetImage();
            FrequencyScale.DrawFrequencyLinesOnImage((Bitmap) image, gridLineLocations);

            //var minuteOffset = TimeSpan.Zero;
            //var xAxisTicInterval = TimeSpan.FromSeconds(1.0);
            //var xInterval = TimeSpan.FromSeconds(10);
            //TimeSpan xAxisPixelDuration = TimeSpan.FromTicks((long)(this.Duration.Ticks / (double)this.FrameCount));
            //int HertzInterval = 1000;
            //if (this.Configuration.WindowSize < 512)
            //    HertzInterval = 2000;
            //double secondsDuration = xAxisPixelDuration.TotalSeconds * image.Width;
            //TimeSpan fullDuration = TimeSpan.FromSeconds(secondsDuration);
            //SpectrogramTools.DrawGridLinesOnImage((Bitmap)image, minuteOffset, fullDuration, xAxisTicInterval, this.NyquistFrequency, HertzInterval);

            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            Bitmap timeBmp = Image_Track.DrawTimeTrack(this.Duration, image.Width);

            var list = new List<Image>();
            list.Add(titleBar);
            list.Add(timeBmp);
            list.Add(image);
            list.Add(timeBmp);

            Image compositeImage = ImageTools.CombineImagesVertically(list);
            return compositeImage;
        }


        public Image GetImageFullyAnnotated(Image image, string title)
        {

            var minuteOffset = TimeSpan.Zero;
            var xAxisTicInterval = TimeSpan.FromSeconds(1.0);
            var xInterval = TimeSpan.FromSeconds(10);
            TimeSpan xAxisPixelDuration = TimeSpan.FromTicks((long)(this.Duration.Ticks / (double)this.FrameCount));
            int herzInterval = 1000;
            if (this.Configuration.WindowSize < 512) herzInterval = 2000;
            double secondsDuration = xAxisPixelDuration.TotalSeconds * image.Width;
            TimeSpan fullDuration = TimeSpan.FromSeconds(secondsDuration);

            // init frequency scale
            int frameSize = image.Height;
            var freqScale = new DSP.FrequencyScale(this.NyquistFrequency, frameSize, herzInterval);
            SpectrogramTools.DrawGridLinesOnImage((Bitmap)image, minuteOffset, fullDuration, xAxisTicInterval, freqScale);

            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            Bitmap timeBmp = Image_Track.DrawTimeTrack(this.Duration, image.Width);

            var list = new List<Image>();
            list.Add(titleBar);
            list.Add(timeBmp);
            list.Add(image);
            list.Add(timeBmp);

            Image compositeImage = ImageTools.CombineImagesVertically(list);
            return compositeImage;
        }

        public Image GetImage()
        {
            return GetImage(1, false, false);
        }

        public Image GetImage(bool doHighlightSubband, bool add1kHzLines)
        {
            return GetImage(1, doHighlightSubband, add1kHzLines);
        }

        public virtual Image GetImage(int binHeight, bool doHighlightSubband, bool add1kHzLines)
        {
            int maxFrequency = this.NyquistFrequency;
            Image image = BaseSonogram.GetSonogramImage(this.Data, this.NyquistFrequency, maxFrequency, this.Configuration.DoMelScale, binHeight,
                                             doHighlightSubband, this.subBand_MinHz, this.subBand_MaxHz);
            bool doMelScale = false;
            //double freqBinWidth = 0.0;
            double freqBinWidth = this.FBinWidth;
            if (add1kHzLines) 
                FrequencyScale.Draw1KHzLines((Bitmap)image, doMelScale, maxFrequency, freqBinWidth);
            return image;
        }

        public StringBuilder GetSegmentationText()
        {
            StringBuilder sb = new StringBuilder("_LABEL_\tST_FR\tST_SEC\tEND_FR\tEND_SEC\n");
            int prevState = 0;
            string name = this.Configuration.CallName;

            int length = this.SigState.Length;

            //START
            if (this.SigState[0] == 0) sb.Append("SILENCE\t0\t0.0000");
            else
                if (this.SigState[0] > 0)
                {
                    prevState = 1;
                    sb.Append(name + "\t0\t0.0000");
                }

            double time = 0.0;
            for (int i = 0; i < length; i++)
            {
                if ((this.SigState[i] > 0) && (prevState == 0))
                {
                    prevState = 1;
                    time = i * this.FrameStep;
                    sb.AppendLine("\t" + (i - 1) + "\t" + time.ToString("F4"));
                    sb.Append(name + "\t" + i + "\t" + time.ToString("F4"));
                }
                else //come to silence
                    if ((this.SigState[i] == 0) && (prevState > 0))
                    {
                        prevState = 0;
                        time = i * this.FrameStep;
                        sb.AppendLine("\t" + (i - 1) + "\t" + time.ToString("F4"));
                        sb.Append("SILENCE\t" + i + "\t" + time.ToString("F4"));
                    }
            }

            time = length * this.FrameStep;
            sb.Append("\t" + (length - 1) + "\t" + time.ToString("F4"));

            return sb;
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
            int frameCount = data.GetLength(0); // Number of spectra in sonogram
            int imageHeight = data.GetLength(1); // image ht = sonogram ht. Later include grid and score scales
            int imageWidth = frameCount / factor;
            int subSample = frameCount / imageWidth;

            //set up min, max, range for normalising of dB values
            double min; double max;
            DataTools.MinMax(data, out min, out max);
            double range = max - min;

            Color[] grayScale = ImageTools.GrayScale();

            //set up the 1000kHz scale
            int herzInterval = 1000;
            int[] vScale = FrequencyScale.CreateLinearYaxis(herzInterval, this.NyquistFrequency, imageHeight); //calculate location of 1000Hz grid lines
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
                    bmp.SetPixel(w, imageHeight - y - 1, col);
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


        public void Dispose()
        {
            this.Configuration = null;
            this.SnrFullband = null;
            this.SnrSubband = null;
            this.DecibelsPerFrame = null;
            this.DecibelsInSubband = null;
            this.DecibelsNormalised = null;
            this.Data = null;
            this.SigState = null;
        }



        //##################################################################################################################################
        //########### STATIC METHODS #######################################################################################################
        //##################################################################################################################################

        private static bool IsInBand(int y, int? minFreq, int? maxFreq)
        {
            if (minFreq == null && maxFreq == null)
            {
                return false;
            }

            return (minFreq == null || y > minFreq) && (maxFreq == null || y < maxFreq);
        }

        /// <summary>
        /// converts the dB data in sonogram.Data to grey scale image of spectrogram.
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        public static System.Tuple<double[,], double, double> Data2ImageData(double[,] M)
        {
            int width = M.GetLength(0);   // Number of spectra in sonogram
            int fftBins = M.GetLength(1);
            double min; double max;
            DataTools.MinMax(M, out min, out max);
            double range = max - min; //for normalisation


            double[,] Mt = new double[fftBins, width];
            for (int f = 0; f < fftBins; f++)
                for (int t = 0; t < width; t++)
                {
                    // normalise and bound the value - use 0-255 image intensity range
                    double value = (M[t, f] - min) / (double)range;
                    int c = 255 - (int)Math.Floor(255.0 * value); //original version
                    if (c < 0) c = 0;
                    else if (c >= 256) c = 255;
                    Mt[fftBins - 1 - f, t] = c;
                }
            return System.Tuple.Create(Mt, min, max);
        }


        private static double[,] MakeAmplitudeSonogram(double[] signal, int[,] frames, FFT.WindowFunc w, out double power)
        {
            int frameCount = frames.GetLength(0);
            int N = frames[0, 1] + 1;     //window or frame width
            //int smoothingWindow = 0;      //to smooth the spectrum //#################ADJUST THIS TO REDUCE VARIANCE - BUT NOT USING HERE

            //var fft = new TowseyLib.FFT(N, w);     // init class which calculates the FFT
            var fft = new FFT(N, w, true); // init class which calculates the MATLAB compatible .NET FFT
            power = fft.WindowPower; //store for later use when calculating dB
            double[,] amplitudeSonogram = new double[frameCount, fft.CoeffCount]; //init amplitude sonogram
            double[] window = new double[N];
            double[] f1;

            for (int i = 0; i < frameCount; i++) //foreach frame or time step
            {
                //set up the window
                for (int j = 0; j < N; j++) 
                    window[j] = signal[frames[i, 0] + j];

                f1 = fft.InvokeDotNetFFT(window); //returns fft amplitude spectrum
                //f1 = fft.Invoke(window); //returns fft amplitude spectrum
                //f1 = fft.InvokeDotNetFFT(DataTools.GetRow(frames, i)); //returns fft amplitude spectrum
                //f1 = fft.Invoke(DataTools.GetRow(frames, i)); //returns fft amplitude spectrum

                //In the early days, we used to smooth the spectrum to reduce variance. But do not do this anymore here. Can do later as required.
                //if (smoothingWindow > 2) 
                //  f1 = DataTools.filterMovingAverage(f1, smoothingWindow);

                // transfer amplitude spectrum to a matrix
                for (int j = 0; j < fft.CoeffCount; j++) //foreach freq bin
                {
                    amplitudeSonogram[i, j] = f1[j];
                }
            } //end of all frames
            return amplitudeSonogram;
        }

        /// <summary>
        /// Returns an image of the data matrix.
        /// Normalises the values from min->max to 0->1.
        /// Thus the grey-scale image pixels will range from 0 to 255. 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Image GetSonogramImage(double[,] data)
        {
            int width = data.GetLength(0); // Number of spectra in sonogram
            int binCount = data.GetLength(1);
            int binHeight = 1;

            int imageHeight = binCount * binHeight; // image ht = sonogram ht

            //set up min, max for normalising of sonogram values
            int minRank = 50;
            int maxRank = 1000;
            //double min = BaseSonogram.GetMinForSonogramImage(data, N);
            //double max = BaseSonogram.GetMaxForSonogramImage(data, N);

            double[] minmax = BaseSonogram.GetMinMaxForSonogramImage(data, minRank, maxRank);
            double min = minmax[0];
            double max = minmax[1];            

            double range = max - min;

            //int minHighlightBin = (int)Math.Round((double)this.subBand_MinHz / (double)NyquistFrequency * fftBins);
            //int maxHighlightBin = (int)Math.Round((double)this.subBand_MaxHz / (double)NyquistFrequency * fftBins);
            //if (DoMelScale)
            //{
            //    double maxMel = MFCCStuff.Mel(this.NyquistFrequency);
            //    int melRange = (int)(maxMel - 0 + 1);
            //    double pixelPerMel = imageHeight / (double)melRange;
            //    double minBandMel = MFCCStuff.Mel(this.subBand_MinHz);
            //    double maxBandMel = MFCCStuff.Mel(this.subBand_MaxHz);
            //    minHighlightBin = (int)Math.Round((double)minBandMel * pixelPerMel);
            //    maxHighlightBin = (int)Math.Round((double)maxBandMel * pixelPerMel);
            //}
            Color[] grayScale = ImageTools.GrayScale();

            Bitmap bmp = new Bitmap(width, imageHeight, PixelFormat.Format24bppRgb);
            int yOffset = imageHeight;
            for (int y = 0; y < binCount; y++) //over all freq bins
            {
                for (int r = 0; r < binHeight; r++) //repeat this bin if pixel rows per bin>1
                {
                    for (int x = 0; x < width; x++) //for pixels in the line
                    {
                        // normalise and bound the value - use min bound, max and 255 image intensity range
                        double value = (data[x, y] - min) / (double)range;
                        int c = 255 - (int)Math.Floor(255.0 * value); //original version
                        if (c < 0) c = 0;
                        else 
                        if (c >= 256) c = 255;

                        //int g = c + 40; // green tinge used in the template scan band 
                        //if (g >= 256) g = 255;
                        bmp.SetPixel(x, yOffset - 1, grayScale[c]);
                    }//for all pixels in line
                    yOffset--;
                } //end repeats over one track
            }//end over all freq bins

            return (Image)bmp;
        }



        public static Image GetSonogramImage(double[,] data, int nyquistFreq, int maxFrequency, bool doMelScale, int binHeight, 
                                             bool doHighlightSubband, int subBand_MinHz, int subBand_MaxHz)
        {
            int width = data.GetLength(0); // Number of spectra in sonogram
            int fftBins = data.GetLength(1);
            int maxBin = (int)Math.Floor(fftBins * maxFrequency / (double)nyquistFreq);

            int imageHeight = maxBin * binHeight; // image ht = sonogram ht. Later include grid and score scales


            //set up min, max, range for normalising of dB values
            double min; double max;
            DataTools.MinMax(data, out min, out max);

            double range = max - min;

            // readjust min and max to create the effect of contrast stretching. It enhances the spectrogram a bit
            double fractionalStretching = 0.01;
            min = min + (range * fractionalStretching);
            max = max - (range * fractionalStretching);
            range = max - min;
            //LoggedConsole.WriteLine("range={0}   max={1}   min={2}", range, max, min);
            //string path2 = @"C:\SensorNetworks\Output\Sonograms\image2.png";
            //Histogram.DrawDistributionsAndSaveImage(data, path2);

            //int? minHighlightFreq = this.subBand_MinHz;
            //int? maxHighlightFreq = this.subBand_MaxHz;
            //int minHighlightBin = (minHighlightFreq == null) ? 0 : (int)Math.Round((double)minHighlightFreq / (double)NyquistFrequency * fftBins);
            //int maxHighlightBin = (maxHighlightFreq == null) ? 0 : (int)Math.Round((double)maxHighlightFreq / (double)NyquistFrequency * fftBins);

            //calculate top and bottom of sub-band 
            int minHighlightBin = (int)Math.Round((double)subBand_MinHz / (double)nyquistFreq * fftBins);
            int maxHighlightBin = (int)Math.Round((double)subBand_MaxHz / (double)nyquistFreq * fftBins);
            if (doMelScale)
            {
                double maxMel = MFCCStuff.Mel(nyquistFreq);
                int melRange = (int)(maxMel - 0 + 1);
                double pixelPerMel = imageHeight / (double)melRange;
                double minBandMel = MFCCStuff.Mel(subBand_MinHz);
                double maxBandMel = MFCCStuff.Mel(subBand_MaxHz);
                minHighlightBin = (int)Math.Round((double)minBandMel * pixelPerMel);
                maxHighlightBin = (int)Math.Round((double)maxBandMel * pixelPerMel);
            }
            Color[] grayScale = ImageTools.GrayScale();

            Bitmap bmp = new Bitmap(width, imageHeight, PixelFormat.Format24bppRgb);
            int yOffset = imageHeight;
            for (int y = 0; y < maxBin; y++) //over all freq bins
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

            return (Image)bmp;
        }


        /// <summary>
        /// Returns an image of the data matrix.
        /// Normalises the values from min->max according to passed rank values.
        /// Therefore pixels in the normalised grey-scale image will range from 0 to 255. 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Image GetSonogramImage(double[,] data, int minPercentile, int maxPercentile)
        {
            int width = data.GetLength(0); // Number of spectra in sonogram
            int binCount = data.GetLength(1);
            int binHeight = 1;
            int imageHeight = binCount * binHeight; // image ht = sonogram ht

            double[] lowAverage = BaseSonogram.GetAvSpectrum_LowestPercentile(data, minPercentile);
            double[] hihAverage = BaseSonogram.GetAvSpectrum_HighestPercentile(data, maxPercentile);
            double min = lowAverage.Min();
            double max = hihAverage.Max();


            double range = max - min;
            Color[] grayScale = ImageTools.GrayScale();

            Bitmap bmp = new Bitmap(width, imageHeight, PixelFormat.Format24bppRgb);
            int yOffset = imageHeight;
            for (int y = 0; y < binCount; y++) //over all freq bins
            {
                for (int r = 0; r < binHeight; r++) //repeat this bin if pixel rows per bin>1
                {
                    for (int x = 0; x < width; x++) //for pixels in the line
                    {
                        // normalise and bound the value - use min bound, max and 255 image intensity range
                        double value = (data[x, y] - min) / (double)range;
                        int c = 255 - (int)Math.Floor(255.0 * value); //original version
                        if (c < 0) c = 0;
                        else
                        if (c >= 256) c = 255;
                        bmp.SetPixel(x, yOffset - 1, grayScale[c]);
                    }//for all pixels in line
                    yOffset--;
                } //end repeats over one track
            }//end over all freq bins

            return (Image)bmp;
        }


        static public double GetMinForSonogramImage(double[,] data, int N)
        {
            double[] rowAvgs = MatrixTools.GetColumnsAverages(data);
            int[] rankOrder = DataTools.GetRankedIndicesInAscendingOrder(rowAvgs);
            double[] minFrame = MatrixTools.GetRow(data, rankOrder[N]);
            return minFrame.Min();
        }
        static public double GetMaxForSonogramImage(double[,] data, int N)
        {
            double[] rowAvgs = MatrixTools.GetColumnsAverages(data);
            int[] rankOrder = DataTools.GetRankedIndicesInDecendingOrder(rowAvgs);
            double[] maxFrame = MatrixTools.GetRow(data, rankOrder[N]);
            return maxFrame.Max();
        }
        /// <summary>
        /// calculates the minimum and max bound values for display of sonogram.
        /// Do not necessarily want to display sonogram normalised between min and max values.
        /// therefore normalise according the Nth smallest and nth largest values in data matrix.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="minRank"></param>
        /// <param name="maxRank"></param>
        /// <returns></returns>
        static public double[] GetMinMaxForSonogramImage(double[,] data, int minRank, int maxRank)
        {
            double[] array = MatrixTools.Matrix2Array(data);
            double minValue = DataTools.GetNthSmallestValue(array, minRank);
            double maxValue = DataTools.GetNthLargestValue(array, maxRank);
            double[] returnArray = { minValue, maxValue };
            return returnArray;
        }

        public static double[] GetAvSpectrum_LowestPercentile(double[,] matrix, int lowPercentile)
        {
            double[] energyLevels = MatrixTools.GetRowAverages(matrix);
            var sorted = DataTools.SortArrayInAscendingOrder(energyLevels);
            int[] order = sorted.Item1;
            //double[] values = sorted.Item2;
            //for (int i = 0; i < 20; i++) Console.WriteLine(values[i]);

            int colCount = matrix.GetLength(1);
            int cutoff = (int)(lowPercentile * matrix.GetLength(0) / (double)100);
            double[] avSpectrum = new double[colCount];

            // sum the lowest percentile frames 
            for (int i = 0; i < cutoff; i++)
            {
                double[] row = DataTools.GetRow(matrix, order[i]);
                for (int c = 0; c < colCount; c++)
                {
                    avSpectrum[c] += row[c];
                }
                //Console.WriteLine(values[i]);
            }
            // get average of the lowest percentile frames 
            for (int c = 0; c < colCount; c++)
            {
                avSpectrum[c] /= cutoff;
                //noiseProfile[c] += 0.0000000001; //to avoid zero values - which is very unlikely given we are in dB.
            }
            //for (int i = 0; i < colCount; i++) Console.WriteLine(noiseProfile[i]);

            return avSpectrum;
        }
        public static double[] GetAvSpectrum_HighestPercentile(double[,] matrix, int highPercentile)
        {
            double[] energyLevels = MatrixTools.GetRowAverages(matrix);
            var sorted = DataTools.SortArray(energyLevels); // sorts array in descending order
            int[] order = sorted.Item1;
            //double[] values = sorted.Item2;
            //for (int i = 0; i < 20; i++) Console.WriteLine(values[i]);

            int colCount = matrix.GetLength(1);
            int cutoff = (int)(highPercentile * matrix.GetLength(0) / (double)100);
            double[] avSpectrum = new double[colCount];

            // sum the lowest percentile frames 
            for (int i = 0; i < cutoff; i++)
            {
                double[] row = DataTools.GetRow(matrix, order[i]);
                for (int c = 0; c < colCount; c++)
                {
                    avSpectrum[c] += row[c];
                }
                //Console.WriteLine(values[i]);
            }
            // get average of the lowest percentile frames 
            for (int c = 0; c < colCount; c++)
            {
                avSpectrum[c] /= cutoff;
            }
            //for (int i = 0; i < colCount; i++) Console.WriteLine(noiseProfile[i]);

            return avSpectrum;
        }


        //public static Image FrameSonogram(Image image, Image titleBar, TimeSpan minOffset, TimeSpan xAxisTicInterval, TimeSpan xAxisPixelDuration,
        //                                  TimeSpan labelInterval, int nyquist, int herzInterval)
        //{
        //    int Y_interval = (int)(image.Height / (double)(nyquist / (double)herzInterval));
        //    image = FrameSonogram(image, titleBar, minOffset, xAxisTicInterval, xAxisPixelDuration, labelInterval, nyquist, herzInterval);
        //    return image;
        //}

        public static Image FrameSonogram(Image image, Image titleBar, TimeSpan minuteOffset, TimeSpan xAxisTicInterval, TimeSpan xAxisPixelDuration,
                                          TimeSpan labelInterval, int nyquist, int herzInterval)
        {
            double secondsDuration = xAxisPixelDuration.TotalSeconds * image.Width;
            TimeSpan fullDuration = TimeSpan.FromSeconds(secondsDuration);

            // init frequency scale
            int frameSize = image.Height;
            var freqScale = new DSP.FrequencyScale(nyquist, frameSize, herzInterval);
            SpectrogramTools.DrawGridLinesOnImage((Bitmap)image, minuteOffset, fullDuration, xAxisTicInterval, freqScale);

            int imageWidth = image.Width;
            int trackHeight = 20;
            int imageHt = image.Height + trackHeight + trackHeight + trackHeight;

            Bitmap timeBmp = BaseSonogram.DrawTimeTrack(minuteOffset, xAxisPixelDuration, xAxisTicInterval, labelInterval, imageWidth, trackHeight, "Seconds");

            Bitmap compositeBmp = new Bitmap(imageWidth, imageHt); //get canvas for entire image
            Graphics gr = Graphics.FromImage(compositeBmp);
            gr.Clear(Color.Black);
            int offset = 0;
            gr.DrawImage(titleBar, 0, offset); //draw in the top time scale
            offset += timeBmp.Height;
            gr.DrawImage(timeBmp, 0, offset); //draw
            offset += titleBar.Height;
            gr.DrawImage(image, 0, offset); //draw
            offset += image.Height;
            gr.DrawImage(timeBmp, 0, offset); //draw
            return compositeBmp;
        }

        public static Image DrawTitleBarOfGrayScaleSpectrogram(string title, int width)
        {
            Bitmap bmp = new Bitmap(width, SpectrogramConstants.HEIGHT_OF_TITLE_BAR);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            Pen pen = new Pen(Color.White);
            Font stringFont = new Font("Arial", 9);
            //Font stringFont = new Font("Tahoma", 9);
            SizeF stringSize = new SizeF();

            //string text = title;
            int X = 4;
            g.DrawString(title, stringFont, Brushes.Wheat, new PointF(X, 3));

            stringSize = g.MeasureString(title, stringFont);
            X += (stringSize.ToSize().Width + 70);
            string text = String.Format("(c) QUT.EDU.AU");
            stringSize = g.MeasureString(text, stringFont);
            int X2 = width - stringSize.ToSize().Width - 2;
            if (X2 > X) g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X2, 3));

            g.DrawLine(new Pen(Color.Gray), 0, 0, width, 0);//draw upper boundary
            //g.DrawLine(pen, duration + 1, 0, trackWidth, 0);
            return bmp;
        }

        // mark of time scale according to scale.
        public static Bitmap DrawTimeTrack(TimeSpan offsetMinute, TimeSpan xAxisPixelDuration, TimeSpan xAxisTicInterval, TimeSpan labelInterval, int trackWidth, int trackHeight, string title)
        {
            Bitmap bmp = new Bitmap(trackWidth, trackHeight);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);

            double elapsedTime = offsetMinute.TotalSeconds;
            double pixelDuration = xAxisPixelDuration.TotalSeconds;
            int labelSecondsInterval = (int)labelInterval.TotalSeconds;
            Pen whitePen = new Pen(Color.White);
            //Pen grayPen = new Pen(Color.Gray);
            Font stringFont = new Font("Arial", 8);

            // for columns, draw in second lines
            double xInterval = (int)(xAxisTicInterval.TotalMilliseconds / xAxisPixelDuration.TotalMilliseconds);
            for (int x = 1; x < trackWidth; x++) //for pixels in the line
            {
                elapsedTime += pixelDuration;
                if (x % xInterval <= pixelDuration)
                {
                    g.DrawLine(whitePen, x, 0, x, trackHeight);
                    int totalSeconds = (int)Math.Round(elapsedTime);
                    if (totalSeconds % labelSecondsInterval == 0)
                    {
                        int minutes = totalSeconds / 60;
                        int seconds = totalSeconds % 60;
                        string time = string.Format("{0}m{1}s", minutes, seconds);
                        g.DrawString(time, stringFont, Brushes.White, new PointF(x + 1, 2)); //draw time
                    }
                }
            }



            g.DrawLine(whitePen, 0, 0, trackWidth, 0);//draw upper boundary
            g.DrawLine(whitePen, 0, trackHeight - 1, trackWidth, trackHeight - 1);//draw lower boundary
            g.DrawLine(whitePen, trackWidth, 0, trackWidth, trackHeight - 1);//draw right end boundary

            g.DrawString(title, stringFont, Brushes.White, new PointF(4, 3));
            //bmp.Save(@"C:\SensorNetworks\Output\SNR\timebmp.png");
            return bmp;
        }



    } //end abstract class BaseSonogram



    //##################################################################################################################################


    /// <summary>
    /// This class is designed to produce a sonogram of full-bandwidth spectral amplitudes 
    /// and to go no further.
    /// The constructor calls the three argument BaseSonogram constructor.
    /// </summary>
    public class AmplitudeSonogram : BaseSonogram
    {
        public AmplitudeSonogram(SonogramConfig config, WavReader wav)
            : base(config, wav, false)
        { }

        public AmplitudeSonogram(SonogramConfig config, double[,] amplitudeData)
            : base(config, amplitudeData)
        {
            var frames = new double[4,4];
            this.SnrFullband = new SNR(frames);
            this.SnrFullband.Decibels = new double[amplitudeData.GetLength(0)];
        }

        /// <summary>
        /// This method does nothing because do not want to change the amplitude sonogram in any way.
        /// Actually the constructor of this class calls the BaseSonogram constructor that does NOT include a call to make().
        /// Consequently this method should never be called. Just a place filler.
        /// </summary>
        /// <param name="amplitudeM"></param>
        public override void Make(double[,] amplitudeM)
        {
        }

    } // class AmplitudeSonogram : BaseSonogram

}