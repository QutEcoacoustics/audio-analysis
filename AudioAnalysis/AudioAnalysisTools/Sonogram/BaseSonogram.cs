namespace AudioAnalysisTools
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Text;

    using Acoustics.Tools.Wav;

    using TowseyLib;

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

        //following values are dependent on sampling rate.
        public int NyquistFrequency { get { return SampleRate / 2; } }
        public double FrameDuration { get { return Configuration.WindowSize / (double)SampleRate; } }     // Duration of full frame or window in seconds
        public double FrameOffset { get { return FrameDuration * (1 - Configuration.WindowOverlap); } } // Duration of non-overlapped part of window/frame in seconds
        public double FBinWidth { get { return (SampleRate / 2) / (double)Configuration.FreqBinCount; } }// FreqBinCount=WindowSize/2
        public double FramesPerSecond { get { return 1 / FrameOffset; } }
        public int FrameCount { get; protected set; } //Temporarily set to (int)(Duration.TotalSeconds/FrameOffset) then reset later

        //energy and dB per frame
        public SNR SnrFullband { get; set; }
        public double[] DecibelsPerFrame { get { return SnrFullband.Decibels; } protected set { } }//decibels per signal frame

        //energy and dB per frame sub-band
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
            this.Configuration.fftConfig.SampleRate = wav.SampleRate; //also set the Nyquist
            this.Duration = wav.Time;
            this.MaxAmplitude = wav.CalculateMaximumAmplitude();
            double[] signal = wav.Samples;

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

                //AUDIO SEGMENTATION
                SigState = EndpointDetectionConfiguration.DetermineVocalisationEndpoints(DecibelsPerFrame, this.FrameOffset);

                var fractionOfHighEnergyFrames = SnrFullband.FractionHighEnergyFrames(EndpointDetectionConfiguration.K2Threshold);
                if (fractionOfHighEnergyFrames > SNR.FRACTIONAL_BOUND_FOR_MODE)
                {
                    Log.WriteIfVerbose("\nWARNING ##############");
                    Log.WriteIfVerbose("\t################### BaseSonogram(): This is a high energy recording. Percent of high energy frames = {0:f0} > {1:f0}%",
                                              fractionOfHighEnergyFrames * 100, SNR.FRACTIONAL_BOUND_FOR_MODE * 100);
                    Log.WriteIfVerbose("\t################### Noise reduction algorithm may not work well in this instance!\n");
                }
            }

            //generate the spectra of FFT AMPLITUDES
            //var amplitudeM = MakeAmplitudeSonogram(frames, TowseyLib.FFT.GetWindowFunction(this.Configuration.FftConfig.WindowFunction));
            TowseyLib.FFT.WindowFunc w = TowseyLib.FFT.GetWindowFunction(this.Configuration.fftConfig.WindowFunction);
            double power;
            var amplitudeM = BaseSonogram.MakeAmplitudeSonogram(signal, frameIDs, w, out power);
            this.Configuration.WindowPower = power;

            //EXTRACT REQUIRED FREQUENCY BAND
            if (DoExtractSubband)
            {
                amplitudeM = BaseSonogram.ExtractFreqSubband(amplitudeM, this.subBand_MinHz, this.subBand_MaxHz,
                             this.Configuration.DoMelScale, this.Configuration.FreqBinCount, this.FBinWidth);
                Log.WriteIfVerbose("\tDim of required sub-band =" + amplitudeM.GetLength(1));
                CalculateSubbandSNR(amplitudeM);
            }

            this.Data = amplitudeM;
        } //end CONSTRUCTOR BaseSonogram(SonogramConfig config, WavReader wav, bool dummy)


        /// <summary>
        /// this constructor is the one most used - it automatically makes the Amplitude spectrum and then, using a call to Make(),
        /// converts that matrix to a Spectrogram whose values are decibels. 
        /// </summary>
        /// <param name="config">All parameters required to make spectrogram</param>
        /// <param name="wav">the recording whose spectrogram is to be made</param>
        public BaseSonogram(SonogramConfig config, WavReader wav)
            : this(config, wav, false)
        {
            Make(this.Data);
        } //end CONSTRUCTOR BaseSonogram(SonogramConfig config, WavReader wav)


        //public BaseSonogram(string recordingFileName, int frameSize, double windowOverlap, int bitsPerSample,  double windowPower, int sr, 
        //                    TimeSpan duration, double[,] amplitudeSpectrogram)
        //{
        //    SonogramConfig sonoConfig = new SonogramConfig(); //default values config
        //    sonoConfig.SourceFName = recordingFileName;
        //    sonoConfig.WindowSize = frameSize;
        //    sonoConfig.WindowOverlap = windowOverlap;
        //    sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("NONE");
        //    sonoConfig.epsilon = Math.Pow(0.5, bitsPerSample - 1);
        //    sonoConfig.WindowPower = windowPower;
        //    sonoConfig.fftConfig.SampleRate = sr;
        //    sonoConfig.Duration = duration;
        //    BaseSonogram sonogram = new SpectralSonogram(sonoConfig, amplitudeSpectrogram);
        //    sonogram.SetTimeScale(duration);
        //}


        /// <summary>
        /// use this constructor when already have the amplitude spectorgram in matrix
        /// Init normalised signal energy array but do nothing with it. This has to be done from outside
        /// </summary>
        /// <param name="config"></param>
        public BaseSonogram(SonogramConfig config, double[,] amplitudeSpectrogram)
        {
            Configuration = config;
            this.FrameCount = amplitudeSpectrogram.GetLength(0);
            this.SampleRate = this.Configuration.fftConfig.SampleRate;

            //init normalised signal energy array but do nothing with it. This has to be done from outside
            this.DecibelsNormalised = new double[this.FrameCount];
            
            this.Data = amplitudeSpectrogram;
            Make(this.Data);
        }


        public abstract void Make(double[,] amplitudeM);



        private static double[,] MakeAmplitudeSonogram(double[] signal, int[,] frames, TowseyLib.FFT.WindowFunc w, out double power)
        {
            int frameCount = frames.GetLength(0);
            int N = frames[0, 1] + 1;     //window or frame width
            //int smoothingWindow = 0;      //to smooth the spectrum //#################ADJUST THIS TO REDUCE VARIANCE - BUT NOT USING HERE

            //var fft = new TowseyLib.FFT(N, w);     // init class which calculates the FFT
            var fft = new TowseyLib.FFT(N, w, true); // init class which calculates the MATLAB compatible .NET FFT
            power = fft.WindowPower; //store for later use when calculating dB
            double[,] amplitudeSonogram = new double[frameCount, fft.CoeffCount]; //init amplitude sonogram
            double[] window = new double[N];
            double[] f1;

            for (int i = 0; i < frameCount; i++) //foreach frame or time step
            {
                //set up the window
                for (int j = 0; j < N; j++) window[j] = signal[frames[i, 0] + j];

                f1 = fft.InvokeDotNetFFT(window); //returns fft amplitude spectrum
                //double[] f1 = fft.InvokeDotNetFFT(DataTools.GetRow(frames, i)); //returns fft amplitude spectrum
                //double[] f1 = fft.Invoke(DataTools.GetRow(frames, i)); //returns fft amplitude spectrum

                //if (smoothingWindow > 2) f1 = DataTools.filterMovingAverage(f1, smoothingWindow); //smooth spectrum to reduce variance
                for (int j = 0; j < fft.CoeffCount; j++) //foreach freq bin
                {
                    amplitudeSonogram[i, j] = f1[j]; //transfer amplitude
                }
            } //end of all frames
            return amplitudeSonogram;
        }


        public double[,] ExtractFreqSubband(WavReader wav, int minHz, int maxHz)
        {
            //double[,] frames = DSP.Frames(wav.Samples, this.Configuration.WindowSize, this.Configuration.WindowOverlap);
            int[,] framesIDs = DSP_Frames.FrameStartEnds(wav.Samples.Length, this.Configuration.WindowSize, this.Configuration.WindowOverlap);
            //calculate a minimum amplitude to prevent taking log of small number. This would increase the range when normalising
            //double epsilon = Math.Pow(0.5, wav.BitsPerSample - 1);

            //var amplitudeM = MakeAmplitudeSonogram(frames, TowseyLib.FFT.GetWindowFunction(this.Configuration.FftConfig.WindowFunction));
            double power;
            var amplitudeM = MakeAmplitudeSonogram(wav.Samples, framesIDs, TowseyLib.FFT.GetWindowFunction(this.Configuration.fftConfig.WindowFunction), out power);
            //this.ExtractSubband = true;
            this.subBand_MinHz = minHz;
            this.subBand_MaxHz = maxHz;
            return BaseSonogram.ExtractFreqSubband(amplitudeM, minHz, maxHz, this.Configuration.DoMelScale,
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
            SigState = EndpointDetectionConfiguration.DetermineVocalisationEndpoints(this.DecibelsInSubband, this.FrameOffset);
        }

        public void SetTimeScale(TimeSpan duration)
        {
            this.Duration = duration; 
        }

        //public void FrequencyScale(int nyquist, double binWidth)
        //{
        //    this.FBinWidth = binWidth;
        //}

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

        public Image GetImage()
        {
            return GetImage(this.NyquistFrequency, 1, false, false);
        }

        public Image GetImage(bool doHighlightSubband, bool add1kHzLines)
        {
            // Log.WriteIfVerbose("BaseSonogram.GetImage(bool doHighlightSubband, bool add1kHzLines)");
            // Log.WriteIfVerbose("    doHighlightSubband=" + doHighlightSubband + "   add1kHzLines=" + add1kHzLines);
            return GetImage(this.NyquistFrequency, 1, doHighlightSubband, add1kHzLines);
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

        public virtual Image GetImage(int maxFrequency, int binHeight, bool doHighlightSubband, bool add1kHzLines)
        {
            int width = this.Data.GetLength(0); // Number of spectra in sonogram
            int fftBins = this.Data.GetLength(1);
            int maxBin = (int)Math.Floor(fftBins * maxFrequency / (double)this.NyquistFrequency);

            int imageHeight = maxBin * binHeight; // image ht = sonogram ht. Later include grid and score scales

            //set up min, max, range for normalising of dB values
            double min; double max;
            DataTools.MinMax(this.Data, out min, out max);
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
            for (int y = 0; y < maxBin; y++) //over all freq bins
            {
                for (int r = 0; r < binHeight; r++) //repeat this bin if ceptral image
                {
                    for (int x = 0; x < width; x++) //for pixels in the line
                    {
                        // normalise and bound the value - use min bound, max and 255 image intensity range
                        double value = (this.Data[x, y] - min) / (double)range;
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
            return (Image)bmp;
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
                    time = i * this.FrameOffset;
                    sb.AppendLine("\t" + (i - 1) + "\t" + time.ToString("F4"));
                    sb.Append(name + "\t" + i + "\t" + time.ToString("F4"));
                }
                else //come to silence
                    if ((this.SigState[i] == 0) && (prevState > 0))
                    {
                        prevState = 0;
                        time = i * this.FrameOffset;
                        sb.AppendLine("\t" + (i - 1) + "\t" + time.ToString("F4"));
                        sb.Append("SILENCE\t" + i + "\t" + time.ToString("F4"));
                    }
            }

            time = length * this.FrameOffset;
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


        private static bool IsInBand(int y, int? minFreq, int? maxFreq)
        {
            if (minFreq == null && maxFreq == null)
            {
                return false;
            }

            return (minFreq == null || y > minFreq) && (maxFreq == null || y < maxFreq);
        }


        private void Draw1kHzLines(Bitmap bmp)
        {
            const int kHz = 1000;
            double kHzBinWidth = kHz / this.FBinWidth;
            int width = bmp.Width;
            int height = bmp.Height;

            int bandCount = (int)Math.Floor(height / kHzBinWidth);
            int[] gridLineLocations = new int[bandCount];
            for (int b = 0; b < bandCount; b++)
            {
                gridLineLocations[b] = (int)(height - ((b + 1) * kHzBinWidth));
            }

            //get melscale locations
            if (this.Configuration.DoMelScale) gridLineLocations = CreateMelYaxis(kHz, height);//WARNING!!!! NEED TO REWORK THIS BUT NOW SELDOM USED

            for (int b = 0; b < bandCount; b++) //over each band
            {
                int y = gridLineLocations[b];
                for (int x = 1; x < width; x++)
                {
                    bmp.SetPixel(x - 1, y, Color.White);
                    bmp.SetPixel(x, y,     Color.Black);
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



        public static double[,] ExtractFreqSubband(double[,] m, int minHz, int maxHz, bool doMelscale, int binCount, double binWidth)
        {
            int c1;
            int c2;
            AcousticEvent.Freq2BinIDs(doMelscale, minHz, maxHz, binCount, binWidth, out c1, out c2);
            return DataTools.Submatrix(m, 0, c1, m.GetLength(0) - 1, c2);
        }

        /// <summary>
        /// Extracts an acoustic event from a sonogram given the location of a user defined rectangular marquee.
        /// NOTE: Nyquist value is used ONLY if using mel scale.
        /// </summary>
        /// <param name="m">the sonogram data as matrix of reals</param>
        /// <param name="start">start time in seconds</param>
        /// <param name="end">end time in seconds</param>
        /// <param name="frameOffset">the time scale: i.e. the duration in seconds of each frame</param>
        /// <param name="minHz">lower freq bound of the event</param>
        /// <param name="maxHz">upper freq bound of the event</param>
        /// <param name="doMelscale">informs whether the sonogram data is linear or mel scale</param>
        /// <param name="Nyquist">full freq range 0-Nyquist</param>
        /// <param name="binWidth">the frequency scale i.e. herz per bin width - assumes linear scale</param>
        /// <returns></returns>
        public static double[,] ExtractEvent(double[,] m, double start, double end, double frameOffset,
                                             int minHz, int maxHz, bool doMelscale, int Nyquist, double binWidth)
        {
            int r1;
            int r2;
            AcousticEvent.Time2RowIDs(start, end - start, frameOffset, out r1, out r2);
            int c1;
            int c2;
            AcousticEvent.Freq2BinIDs(doMelscale, minHz, maxHz, Nyquist, binWidth, out c1, out c2);
            return DataTools.Submatrix(m, r1, c1, r2, c2);
        }


        public static double[] ExtractModalNoiseSubband(double[] modalNoise, int minHz, int maxHz, bool doMelScale, int Nyquist, double binWidth)
        {
            //extract subband modal noise profile
            int c1, c2;
            AcousticEvent.Freq2BinIDs(doMelScale, minHz, maxHz, Nyquist, binWidth, out c1, out c2);
            int subbandCount = c2 - c1 + 1;
            var subband = new double[subbandCount];
            for (int i = 0; i < subbandCount; i++) subband[i] = modalNoise[c1 + i];
            return subband;
        }

        public static System.Tuple<SpectralSonogram, CepstralSonogram, double[], double[]> GetAllSonograms(string path, SonogramConfig sonoConfig, int minHz, int maxHz)
        {
            Log.WriteLine("# Extract spectrogram and cepstrogram from from file: " + Path.GetFileName(path));
            AudioRecording recording = new AudioRecording(path);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            var tuple = GetAllSonograms(recording, sonoConfig, minHz, maxHz);
            return tuple;
        }

        /// <summary>
        /// Returns a Spectrogram and Cepstrogram from the passed recording. These are NOT noise reduced.
        /// however, tuple also returns the modal noise and subband modal noise.
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="sonoConfig"></param>
        /// <param name="minHz"></param>
        /// <param name="maxHz"></param>
        /// <returns></returns>
        public static System.Tuple<SpectralSonogram, CepstralSonogram, double[], double[]> GetAllSonograms(AudioRecording recording, SonogramConfig sonoConfig, int minHz, int maxHz)
        {
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            int sr = recording.SampleRate;
            bool doMelScale = sonoConfig.DoMelScale;
            int ccCount = sonoConfig.mfccConfig.CcCount;
            bool includeDelta = sonoConfig.mfccConfig.IncludeDelta;
            bool includeDoubleDelta = sonoConfig.mfccConfig.IncludeDoubleDelta;
            sonoConfig.SourceFName = recording.FileName;

            AmplitudeSonogram basegram = new AmplitudeSonogram(sonoConfig, recording.GetWavReader());
            SpectralSonogram sonogram = new SpectralSonogram(basegram);  //spectrogram has dim[N,257]
            recording.Dispose();

            Log.WriteLine("Signal: Duration={0}, Sample Rate={1}", sonogram.Duration, sr);
            Log.WriteLine("Frames: Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f0}%, Offset={3:f1}ms, Frames/s={4:f1}",
                                           sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                          (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, sonoConfig.WindowOverlap * 100);
            int binCount = (int)(maxHz / sonogram.FBinWidth) - (int)(minHz / sonogram.FBinWidth) + 1;
            Log.WriteLine("Freqs : {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount);
            Log.WriteLine("MFCCs : doMelScale=" + doMelScale + ";  ccCount=" + ccCount + ";  includeDelta=" + includeDelta + ";  includeDoubleDelta=" + includeDoubleDelta);

            //CALCULATE MODAL NOISE PROFILE - USER MAY REQUIRE IT FOR NOISE REDUCTION
            double[] modalNoise = sonogram.SnrFullband.ModalNoiseProfile;
            //extract subband modal noise profile
            double[] noise_subband = BaseSonogram.ExtractModalNoiseSubband(modalNoise, minHz, maxHz, doMelScale,
                                                                           sonogram.NyquistFrequency, sonogram.FBinWidth);
            //CALCULATE CEPSTROGRAM
            Log.WriteLine("# Extracting Cepstrogram");
            CepstralSonogram cepstrogram = new CepstralSonogram(basegram, minHz, maxHz);  //cepstrogram has dim[N,13]
            var tuple = System.Tuple.Create(sonogram, cepstrogram, modalNoise, noise_subband);
            return tuple;
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

        /// <summary>
        /// This method does nothing because do not want to change the amplitude sonogram in any way.
        /// Actually the constructor of this class calls the BaseSonogram constructor that does NOT include a call to make().
        /// Consequently this method should never be called. Just a place filler.
        /// </summary>
        /// <param name="amplitudeM"></param>
        public override void Make(double[,] amplitudeM)
        {
        }

    }



    //##################################################################################################################################

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
            : this(SonogramConfig.Load(configFile), wav)
        { }
        public SpectralSonogram(SonogramConfig config, WavReader wav)
            : base(config, wav)
        { }

                /// <summary>
        /// use this constructor when w
        /// </summary>
        /// <param name="config"></param>
        public SpectralSonogram(SonogramConfig config, double[,] amplitudeSpectrogram)
            : base(config, amplitudeSpectrogram)
        {
            Configuration = config;
            this.FrameCount = amplitudeSpectrogram.GetLength(0);
            this.SampleRate = config.fftConfig.SampleRate;
            this.Data = amplitudeSpectrogram;
            Make(this.Data);
        }

        public static SpectralSonogram GetSpectralSonogram(string recordingFileName, int frameSize, double windowOverlap, int bitsPerSample, double windowPower, int sr,
                                                           TimeSpan duration, NoiseReductionType nrt, double[,] amplitudeSpectrogram)
        {
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recordingFileName;
            sonoConfig.WindowSize = frameSize;
            sonoConfig.WindowOverlap = windowOverlap;
            sonoConfig.NoiseReductionType = nrt;
            sonoConfig.epsilon = Math.Pow(0.5, bitsPerSample - 1);
            sonoConfig.WindowPower = windowPower;
            sonoConfig.fftConfig.SampleRate = sr;
            sonoConfig.Duration = duration;
            var sonogram = new SpectralSonogram(sonoConfig, amplitudeSpectrogram);
            sonogram.SetTimeScale(duration);
            return sonogram;
        }



        public SpectralSonogram(AmplitudeSonogram sg)
            : base(sg.Configuration)
        {
            this.DecibelsPerFrame = sg.DecibelsPerFrame;
            this.DecibelsNormalised = sg.DecibelsNormalised;
            this.Duration = sg.Duration;
            this.Configuration.epsilon = sg.Configuration.epsilon;
            this.FrameCount = sg.FrameCount;
            this.Max_dBReference = sg.Max_dBReference;
            this.MaxAmplitude = sg.MaxAmplitude;
            this.SampleRate = sg.SampleRate;
            this.SigState = sg.SigState;
            this.SnrFullband = sg.SnrFullband;
            this.Data = sg.Data;
            this.Make(this.Data); //converts amplitude matrix to dB spectrogram
        }


        /// <summary>
        /// use this constructor to cut out a portion of a spectrum from start to end time.
        /// </summary>
        /// <param name="sg"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public SpectralSonogram(SpectralSonogram sg, double startTime, double endTime)
            : base(sg.Configuration)
        {
            int startFrame = (int)Math.Round(startTime * sg.FramesPerSecond);
            int endFrame = (int)Math.Round(endTime * sg.FramesPerSecond);
            int frameCount = endFrame - startFrame + 1;

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
            for (int i = 0; i < frameCount; i++) this.DecibelsPerFrame[i] = sg.DecibelsPerFrame[startFrame + i];

            ////energy and dB per frame sub-band
            //this.ExtractSubband = this.ExtractSubband;
            this.subBand_MinHz = sg.subBand_MinHz; //min freq (Hz) of the required subband
            this.subBand_MaxHz = sg.subBand_MaxHz; //max freq (Hz) of the required subband
            //sg.SnrSubband { get; private set; }
            this.DecibelsInSubband = new double[frameCount];  // Normalised decibels in extracted freq band
            for (int i = 0; i < frameCount; i++) this.DecibelsInSubband[i] = sg.DecibelsInSubband[startFrame + i];

            //public double[] DecibelsNormalised { get; private set; } 
            this.Max_dBReference = sg.Max_dBReference; // Used to normalise the dB values for MFCCs
            this.DecibelsNormalised = new double[frameCount];
            for (int i = 0; i < frameCount; i++) this.DecibelsNormalised[i] = sg.DecibelsNormalised[startFrame + i];

            this.SigState = new int[frameCount];    //Integer coded signal state ie  0=non-vocalisation, 1=vocalisation, etc.
            for (int i = 0; i < frameCount; i++) this.SigState[i] = sg.SigState[startFrame + i];

            //the spectrogram data matrix
            int featureCount = sg.Data.GetLength(1);
            this.Data = new double[frameCount, featureCount];
            for (int i = 0; i < frameCount; i++) //each row of matrix is a frame
                for (int j = 0; j < featureCount; j++) //each col of matrix is a feature
                    this.Data[i, j] = sg.Data[startFrame + i, j];
        }//end CONSTRUCTOR



        public override void Make(double[,] amplitudeM)
        {
            this.SampleRate = this.Configuration.fftConfig.SampleRate;
            double[,] m = amplitudeM;

            // (i) IF REQUIRED CONVERT TO FULL BAND WIDTH MEL SCALE
            if (Configuration.DoMelScale)// m = ApplyFilterBank(m); //following replaces next method
            {
                m = Speech.MelFilterBank(m, Configuration.FreqBinCount, this.NyquistFrequency, 0, this.NyquistFrequency); // using the Greg integral
            }

            // (ii) CONVERT AMPLITUDES TO DECIBELS
            m = Speech.DecibelSpectra(m, this.Configuration.WindowPower, this.SampleRate, this.Configuration.epsilon);

            int frameCount = amplitudeM.GetLength(0); //i.e. row count

            // (iii) NOISE REDUCTION
            var tuple = SNR.NoiseReduce(m, Configuration.NoiseReductionType, this.Configuration.NoiseReductionParameter);
            this.Data = tuple.Item1;   // store data matrix

            if (this.SnrFullband != null)
            {
                this.SnrFullband.ModalNoiseProfile = tuple.Item2; // store the full bandwidth modal noise profile
            }
        }


        /// <summary>
        /// Normalise the dynamic range of spectrogram between 0dB and value of DynamicRange.
        /// Also must adjust the SNR.DecibelsInSubband and this.DecibelsNormalised
        /// </summary>
        /// <param name="dynamicRange"></param>
        public void NormaliseDynamicRange(double dynamicRange)
        {
            int frameCount = this.Data.GetLength(0);
            int featureCount = this.Data.GetLength(1);
            double minIntensity; // min value in matrix
            double maxIntensity; // max value in matrix
            DataTools.MinMax(this.Data, out minIntensity, out maxIntensity);
            double[,] newMatrix = new double[frameCount, featureCount];

            for (int i = 0; i < frameCount; i++) //each row of matrix is a frame
                for (int j = 0; j < featureCount; j++) //each col of matrix is a feature
                {
                    newMatrix[i, j] = this.Data[i, j];
                }
            this.Data = newMatrix;
        }


        public System.Tuple<double[,], double[]> GetCepstrogram(int minHz, int maxHz, bool doMelScale, int ccCount)
        {
            return GetCepstrogram(this.Data, minHz, maxHz, this.Configuration.FreqBinCount, this.FBinWidth, doMelScale, ccCount);
        }

        /// <summary>
        /// The data passed to this method must be the Spectral sonogram.
        /// </summary>
        /// <param name="data">the Spectral sonogram</param>
        /// <param name="minHz"></param>
        /// <param name="maxHz"></param>
        /// <param name="freqBinCount"></param>
        /// <param name="freqBinWidth"></param>
        /// <param name="doMelScale"></param>
        /// <param name="ccCount"></param>
        /// <returns></returns>
        public static System.Tuple<double[,], double[]> GetCepstrogram(double[,] data, int minHz, int maxHz,
                                                        int freqBinCount, double freqBinWidth, bool doMelScale, int ccCount)
        {
            ImageTools.DrawMatrix(data, @"C:\SensorNetworks\Output\MFCC_LewinsRail\tempImage1.jpg", false);
            double[,] m = BaseSonogram.ExtractFreqSubband(data, minHz, maxHz, doMelScale, freqBinCount, freqBinWidth);
            ImageTools.DrawMatrix(m, @"C:\SensorNetworks\Output\MFCC_LewinsRail\tempImage2.jpg", false);

            //DO NOT DO NOISE REDUCTION BECAUSE ALREADY DONE
            //double[] modalNoise = SNR.CalculateModalNoise(m, 7); //calculate modal noise profile and smooth
            //m = SNR.NoiseReduce_Standard(m, modalNoise);
            //m = SNR.NoiseReduce_FixedRange(m, this.Configuration.DynamicRange);

            m = Speech.Cepstra(m, ccCount);
            m = DataTools.normalise(m);
            ImageTools.DrawMatrix(m, @"C:\SensorNetworks\Output\MFCC_LewinsRail\tempImage3.jpg", false);
            double[] modalNoise = null;
            return System.Tuple.Create(m, modalNoise);
        }
    } //end of class SpectralSonogram : BaseSonogram


    //##################################################################################################################################


    public class CepstralSonogram : BaseSonogram
    {
        public CepstralSonogram(string configFile, WavReader wav)
            : this(SonogramConfig.Load(configFile), wav)
        { }
        public CepstralSonogram(SonogramConfig config, WavReader wav)
            : base(config, wav)
        { }

        public CepstralSonogram(AmplitudeSonogram sg)
            : base(sg.Configuration)
        {
            this.Configuration = sg.Configuration;
            this.DecibelsPerFrame = sg.DecibelsPerFrame;
            this.DecibelsNormalised = sg.DecibelsNormalised;
            this.Duration = sg.Duration;
            this.FrameCount = sg.FrameCount;
            this.Max_dBReference = sg.Max_dBReference;
            this.MaxAmplitude = sg.MaxAmplitude;
            this.SampleRate = sg.SampleRate;
            this.SigState = sg.SigState;
            this.SnrFullband = sg.SnrFullband;
            this.Data = sg.Data;
            this.Make(this.Data); //converts amplitude matrix to cepstral sonogram
        }

        public CepstralSonogram(AmplitudeSonogram sg, int minHz, int maxHz)
            : this(sg)
        {
            this.DecibelsPerFrame = sg.DecibelsPerFrame;
            this.DecibelsNormalised = sg.DecibelsNormalised;
            this.Duration = sg.Duration;
            //this.epsilon = sg.epsilon;
            this.FrameCount = sg.FrameCount;
            this.Max_dBReference = sg.Max_dBReference;
            this.MaxAmplitude = sg.MaxAmplitude;
            this.SampleRate = sg.SampleRate;
            this.SigState = sg.SigState;
            this.SnrFullband = sg.SnrFullband;

            this.subBand_MinHz = minHz;
            this.subBand_MaxHz = maxHz;

            //double[] noise_subband = BaseSonogram.ExtractModalNoiseSubband(this.SnrFullband.ModalNoiseProfile, minHz, maxHz, sg.doMelScale,
            //                                                   sonogram.Configuration.FreqBinCount, sonogram.FBinWidth); 
            this.Data = BaseSonogram.ExtractFreqSubband(sg.Data, minHz, maxHz,
                             this.Configuration.DoMelScale, sg.Configuration.FreqBinCount, sg.FBinWidth);
            CalculateSubbandSNR(this.Data);
            this.Make(this.Data);          //converts amplitude matrix to cepstral sonogram
        }

        public override void Make(double[,] amplitudeM)
        {
            var tuple = CepstralSonogram.MakeCepstrogram(this.Configuration, amplitudeM, this.DecibelsNormalised, this.SampleRate);
            this.Data = tuple.Item1;
            this.SnrFullband.ModalNoiseProfile = tuple.Item2; //store the full bandwidth modal noise profile
        }

        /// <summary>
        /// NOTE!!!! The decibel array has been normalised in 0 - 1.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="decibels"></param>
        /// <param name="ccCount"></param>
        /// <param name="includeDelta"></param>
        /// <param name="includeDoubleDelta"></param>
        /// <returns></returns>
        protected static System.Tuple<double[,], double[]> MakeCepstrogram(SonogramConfig config, double[,] matrix, double[] decibels, int sampleRate)
        {
            double[,] m = matrix;
            int nyquist = sampleRate / 2;
            double epsilon = config.epsilon;
            bool includeDelta = config.mfccConfig.IncludeDelta;
            bool includeDoubleDelta = config.mfccConfig.IncludeDoubleDelta;

            //Log.WriteIfVerbose(" MakeCepstrogram(matrix, decibels, includeDelta=" + includeDelta + ", includeDoubleDelta=" + includeDoubleDelta + ")");

            //(i) APPLY FILTER BANK
            int bandCount = config.mfccConfig.FilterbankCount;
            bool doMelScale = config.mfccConfig.DoMelScale;
            int ccCount = config.mfccConfig.CcCount;
            int FFTbins = config.FreqBinCount;  //number of Hz bands = 2^N +1. Subtract DC bin
            int minHz = config.MinFreqBand ?? 0;
            int maxHz = config.MaxFreqBand ?? nyquist;

            Log.WriteIfVerbose("ApplyFilterBank(): Dim prior to filter bank  =" + matrix.GetLength(1));
            //error check that filterBankCount < FFTbins
            if (bandCount > FFTbins)
                throw new Exception("## FATAL ERROR in BaseSonogram.MakeCepstrogram():- Can't calculate cepstral coeff. FilterbankCount > FFTbins. (" + bandCount + " > " + FFTbins + ")\n\n");

            //this is the filter count for full bandwidth 0-Nyquist. This number is trimmed proportionately to fit the required bandwidth. 
            if (doMelScale) m = Speech.MelFilterBank(m, bandCount, nyquist, minHz, maxHz); // using the Greg integral
            else m = Speech.LinearFilterBank(m, bandCount, nyquist, minHz, maxHz);
            Log.WriteIfVerbose("\tDim after filter bank=" + m.GetLength(1) + " (Max filter bank=" + bandCount + ")");

            //(ii) CONVERT AMPLITUDES TO DECIBELS
            m = Speech.DecibelSpectra(m, config.WindowPower, sampleRate, epsilon); //from spectrogram

            //(iii) NOISE REDUCTION
            var tuple1 = SNR.NoiseReduce(m, config.NoiseReductionType, config.NoiseReductionParameter);
            m = tuple1.Item1;

            //(iv) calculate cepstral coefficients 
            m = Speech.Cepstra(m, ccCount);
            //(v) normalise
            m = DataTools.normalise(m);
            //(vi) Calculate the full range of MFCC coefficients ie including decibel and deltas, etc
            m = Speech.AcousticVectors(m, decibels, includeDelta, includeDoubleDelta);
            var tuple2 = System.Tuple.Create(m, tuple1.Item2);
            return tuple2; // return matrix and full bandwidth modal noise profile
        }

    } // end class CepstralSonogram


    //##################################################################################################################################



    public class TriAvSonogram : CepstralSonogram
    {
        public TriAvSonogram(string configFile, WavReader wav)
            : base(SonogramConfig.Load(configFile), wav)
        { }

        public TriAvSonogram(SonogramConfig config, WavReader wav)
            : base(config, wav)
        { }

        public override void Make(double[,] amplitudeM)
        {
            Data = MakeAcousticVectors(this.Configuration, amplitudeM, this.DecibelsNormalised, this.SampleRate);
        }

        static double[,] MakeAcousticVectors(SonogramConfig config, double[,] matrix, double[] decibels, int sampleRate)
        {
            int ccCount = config.mfccConfig.CcCount;
            bool includeDelta = config.mfccConfig.IncludeDelta;
            bool includeDoubleDelta = config.mfccConfig.IncludeDoubleDelta;
            int deltaT = config.DeltaT;

            Log.WriteIfVerbose(" MakeAcousticVectors(matrix, decibels, includeDelta=" + includeDelta + ", includeDoubleDelta=" + includeDoubleDelta + ", deltaT=" + deltaT + ")");
            var tuple = CepstralSonogram.MakeCepstrogram(config, matrix, decibels, sampleRate);
            double[,] m = tuple.Item1;
            //this.SnrFullband.ModalNoiseProfile = tuple.Item2; //store the full bandwidth modal noise profile


            //initialise feature vector for template - will contain three acoustic vectors - for T-dT, T and T+dT
            int frameCount = m.GetLength(0);
            int cepstralL = m.GetLength(1);  // length of cepstral vector 
            int featurevL = 3 * cepstralL;   // to accomodate cepstra for T-2, T and T+2

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



    //##################################################################################################################################



    public class SobelEdgeSonogram : BaseSonogram
    {
        public SobelEdgeSonogram(string configFile, WavReader wav)
            : base(SonogramConfig.Load(configFile), wav)
        { }

        public SobelEdgeSonogram(SonogramConfig config, WavReader wav)
            : base(config, wav)
        { }

        public override void Make(double[,] amplitudeM)
        {
            Data = SobelEdgegram(amplitudeM);
        }

        double[,] SobelEdgegram(double[,] matrix)
        {
            double[,] m = Speech.DecibelSpectra(matrix, this.Configuration.WindowPower, this.SampleRate, this.Configuration.epsilon); //from spectrogram
            //double[,] m = Speech.DecibelSpectra(matrix);

            //NOISE REDUCTION
            var output = SNR.NoiseReduce(m, Configuration.NoiseReductionType, this.Configuration.NoiseReductionParameter);
            this.SnrFullband.ModalNoiseProfile = output.Item2;
            return ImageTools.SobelEdgeDetection(output.Item1);
        }
    }// end SobelEdgeSonogram : BaseSonogram



}