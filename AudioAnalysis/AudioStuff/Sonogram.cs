using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using TowseyLib;

namespace AudioStuff
{
    public enum SonogramType { spectral, cepstral, acousticVectors, sobelEdge }


	public sealed class Sonogram
	{
        public const int binWidth = 1000; //1 kHz bands for calculating acoustic indices 

        //constants for analysing the logEnergy array for signal segmentation
        public const double minLogEnergy = -6.0;        // typical noise value for BAC2 recordings = -4.5
        public const double maxLogEnergy = -0.60206;    // = Math.Log10(0.25) which assumes max average frame amplitude = 0.5
        //public const double maxLogEnergy = -0.444;    // = Math.Log10(0.36) which assumes max average frame amplitude = 0.6
        //public const double maxLogEnergy = -0.310;    // = Math.Log10(0.49) which assumes max average frame amplitude = 0.7
        //note that the cicada recordings reach max average frame amplitude = 0.55

        //Following const used to normalise the logEnergy values to the background noise.
        //Has the effect of setting bcakground noise level to 0 dB. Value of 10dB is in Lamel et al, 1981 
        //Lamel et al call it "Adaptive Level Equalisatsion".
        public const double noiseThreshold = 10.0; //dB
        




        private SonoConfig state = new SonoConfig();  //class containing state of all application parameters
        public  SonoConfig State { get { return state; } set { state = value; } }

        public string BmpFName { get { return state.BmpFName; } }


        private double[] frameEnergy; //energy per signal frame
        public double[]  FrameEnergy { get { return frameEnergy; } /*set { frameEnergy = value; }*/ }

        private double[] decibels; //normalised decibels per signal frame
        public double[]  Decibels { get { return decibels; } /*set { decibels = value; }*/ }

        //private double[] fftDecibels; //energy per FFT freq band
        //public double[]  FFTDecibels { get { return fftDecibels; } /*set { fftDecibels = value; }*/ }

        private int[] zeroCross = null; //number of zero crossings per frame
        public  int[] ZeroCross { get { return zeroCross; } /*set { zeroCross = value; }*/ }

        private int[] sigState; //integer coded signal state ie  0=non-vocalisation, 1=vocalisation, etc.
        public  int[] SigState { get { return sigState; } /*set { sigState = value; }*/ }

        private double[,] amplitudM; //the original matrix of FFT amplitudes i.e. unprocessed sonogram
        public  double[,] AmplitudM { get { return amplitudM; } /*set { amplitudM = value; }*/ }

        private double[,] spectralM; //the sonogram of log energy spectra
        public  double[,] SpectralM { get { return spectralM; } /*set { spectralM = value; }*/ }

        private double[,] cepstralM; //the matrix of energy, mfccs, delta and doubleDelta coefficients ie 3x(1+12)=39
        public  double[,] CepstralM { get { return cepstralM; } /*set { cepstralM = value; }*/ }

        private double[,] acousticM; //the matrix of acoustic vectors ie 3x39 for frames T-2, T, T+2
        public  double[,] AcousticM { get { return acousticM; } /*set { acousticM = value; }*/ }



        //****************************************************************************************************
        //****************************************************************************************************
        //****************************************************************************************************
        //  CONSTRUCTORS
        


        /// <summary>
        /// CONSTRUCTOR 1
        /// Use this constructor when initialising  a sonogram from within a template
        /// </summary>
        /// <param name="props"></param>
        /// <param name="wav"></param>
        public Sonogram(SonoConfig state, WavReader wav)
        {
            this.state = state;
            Make(wav);
            if(state.Verbosity > 0) WriteInfo();
        }

        /// <summary>
        /// CONSTRUCTOR 2
        /// </summary>
        /// <param name="iniFName"></param>
        /// <param name="wavFName"></param>
        public Sonogram(string iniFName)
        {
            state.ReadConfig(iniFName);
        }

        /// <summary>
        /// CONSTRUCTOR 3
        /// </summary>
        /// <param name="iniFName"></param>
        /// <param name="wavFName"></param>
        public Sonogram(string iniFName, string wavPath)
        {
            state.ReadConfig(iniFName);
            Make(wavPath);
        }

        /// <summary>
        /// CONSTRUCTOR 4
        /// This constructor called by the Template class when it creates a new Template
        /// Creates matrix of acoustic vectors.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="wavPath"></param>
        public Sonogram(SonoConfig state, string wavPath)
        {
            this.state = state;
            Make(wavPath);
        }

        /// <summary>
        /// CONSTRUCTOR 5
        /// </summary>
        /// <param name="iniFName"></param>
        /// <param name="wavPath"></param>
        /// <param name="wavBytes"></param>
        /// <returns></returns>
        public Sonogram(string iniFName, string wavPath, byte[] wavBytes)
        {
            state.ReadConfig(iniFName);

            FileInfo fi = new FileInfo(wavPath);
            state.WavFileDir = fi.DirectoryName;
            state.WavFName = fi.Name.Substring(0, fi.Name.Length - 4);
            state.WavFileExt = fi.Extension;
            state.SetDateAndTime(state.WavFName);

            //initialise WAV class with bytes array
            WavReader wav = new WavReader(wavBytes, state.WavFName);
            Make(wav);
            if (state.Verbosity != 0) WriteInfo();
        }

        /// <summary>
        /// CONSTRUCTOR 6
        /// </summary>
        /// <param name="iniFName"></param>
        /// <param name="wavPath"></param>
        /// <param name="rawData"></param>
        /// <param name="sampleRate"></param>
        public Sonogram(string iniFName, string sigName, double[] rawData, int sampleRate)
        {
            state.ReadConfig(iniFName);
            state.WavFName = sigName;
            state.WavFileExt = WavReader.wavFExt;
            //state.WavFileExt = "sig";

            //initialise WAV class with double array
            WavReader wav = new WavReader(rawData, sampleRate, sigName);
            Make(wav);
            if (state.Verbosity != 0) WriteInfo();
        }

        public static SonogramType SetSonogramType(string typeName)
        {
            SonogramType type = SonogramType.spectral; //the default
            if ((typeName == null) || (typeName == "")) return SonogramType.spectral;
            if (typeName.StartsWith("spectral")) return SonogramType.spectral;
            if (typeName.StartsWith("cepstral")) return SonogramType.cepstral;
            if (typeName.StartsWith("acousticVectors")) return SonogramType.acousticVectors;
            if (typeName.StartsWith("sobelEdge")) return SonogramType.sobelEdge;
            return type;
        }



        /// <summary>
        /// Makes the sonogram given path wav file.
        /// Assumes that sonogram class already initialised
        /// </summary>
        /// <param name="wavFName"></param>
        public void Make(string wavPath)
        {
            FileInfo fi = new FileInfo(wavPath);
            state.WavFileDir = fi.DirectoryName;
            state.WavFName = fi.Name.Substring(0, fi.Name.Length - 4);
            state.WavFileExt = fi.Extension;
            state.SetDateAndTime(state.WavFName);

            //read the .WAV file
            WavReader wav = new WavReader(wavPath);
            Make(wav);
            if (state.Verbosity != 0) WriteInfo();
        }

        private void Make(WavReader wav)
        {
            //store essential parameters for this sonogram
            if (wav.Amplitude_AbsMax == 0.0) throw new ArgumentException("Wav file has zero signal. Cannot make sonogram.");
            this.state.WavMax        = wav.Amplitude_AbsMax;
            this.state.WavFName      = wav.WavFileName;
            this.state.SampleRate    = wav.SampleRate;
            this.state.SampleCount   = wav.SampleCount;
            this.state.TimeDuration  = state.SampleCount / (double)state.SampleRate;
            
            this.state.MinFreq       = 0;                     //the default minimum freq (Hz)
            this.state.NyquistFreq   = state.SampleRate / 2;  //Nyquist
            if (this.state.FreqBand_Min <= 0)
                this.state.FreqBand_Min  = this.state.MinFreq;    //default min of the freq band to be analysed  
            if (this.state.FreqBand_Max <= 0)
                this.state.FreqBand_Max  = this.state.NyquistFreq;    //default max of the freq band to be analysed
            if ((this.state.FreqBand_Min > 0)||(this.state.FreqBand_Max < this.state.NyquistFreq)) this.state.doFreqBandAnalysis = true;


            this.state.FrameDuration = state.WindowSize / (double)state.SampleRate; // window duration in seconds
            this.state.FrameOffset   = this.state.FrameDuration * (1 - this.state.WindowOverlap);// duration in seconds
            this.state.FreqBinCount  = this.state.WindowSize / 2; // other half is phase info
            this.state.MelBinCount   = this.state.FreqBinCount; // same has number of Hz bins
            this.state.FBinWidth     = this.state.NyquistFreq / (double)this.state.FreqBinCount;
            this.state.FrameCount = (int)(this.state.TimeDuration / this.state.FrameOffset);
            this.state.FramesPerSecond = 1 / this.state.FrameOffset;

            double[] signal = wav.Samples;
            //SIGNAL PRE-EMPHASIS helps with speech signals
            bool doPreemphasis = false;
            if (doPreemphasis)
            {
                double coeff = 0.96;
                signal = DSP.PreEmphasis(signal, coeff);
            }

            //FRAME WINDOWING
            int step = (int)(this.state.WindowSize * (1 - this.state.WindowOverlap));
            double[,] frames = DSP.Frames(signal, this.state.WindowSize, step);
            this.state.FrameCount = frames.GetLength(0);

            //ENERGY PER FRAME
            this.frameEnergy = DSP.SignalLogEnergy(frames, Sonogram.minLogEnergy, Sonogram.maxLogEnergy);
            //Console.WriteLine("FrameNoiseDecibels=" + this.State.FrameNoiseLogEnergy + "  FrameMaxDecibels=" + this.State.FrameMaxLogEnergy);
            
            //NOISE SUBTRACTION: subtract background noise to produce decibels array in which zero dB = average noise
            double minEnergyRatio = Sonogram.minLogEnergy - Sonogram.maxLogEnergy;
            double Q;
            double min_dB;
            double max_dB;
            this.decibels = DSP.NoiseSubtract(this.frameEnergy, out min_dB, out max_dB, minEnergyRatio, Sonogram.noiseThreshold, out Q);
            this.State.NoiseSubtracted = Q;
            this.State.FrameNoise_dB = min_dB; //min decibels of all frames 
            this.State.FrameMax_dB = max_dB;
            this.State.Frame_SNR = max_dB - min_dB;
            this.State.MinDecibelReference = min_dB - Q;
            this.State.MaxDecibelReference = (Sonogram.maxLogEnergy * 10) - Q;

            // ZERO CROSSINGS
            //this.zeroCross = DSP.ZeroCrossings(frames);

            //DETERMINE ENDPOINTS OF VOCALISATIONS
            double k1 = this.State.MinDecibelReference + this.State.SegmentationThreshold_k1;
            double k2 = this.State.MinDecibelReference + this.State.SegmentationThreshold_k2;
            int k1_k2delay = (int)(this.State.k1_k2Latency / this.State.FrameOffset); //=5  frames delay between signal reaching k1 and k2 thresholds
            int syllableDelay = (int)(this.State.vocalDelay / this.State.FrameOffset); //=10 frames delay required to separate vocalisations 
            int minPulse = (int)(this.State.minPulseDuration / this.State.FrameOffset); //=2 frames is min vocal length
            //Console.WriteLine("k1_k2delay=" + k1_k2delay + "  syllableDelay=" + syllableDelay + "  minPulse=" + minPulse);
            this.sigState = Speech.VocalizationDetection(this.decibels, k1, k2, k1_k2delay, syllableDelay, minPulse, null);

            //generate the spectra of FFT AMPLITUDES
            //calculate a minimum amplitude to prevent taking log of small number. This would increase the range when normalising
            double epsilon = Math.Pow(0.5, wav.BitsPerSample - 1);
            this.amplitudM = GenerateAmplitudeSpectra(frames, this.state.WindowFnc, epsilon);

            if (this.state.doFreqBandAnalysis)
            {
                int c1 = (int)(this.state.freqBand_Min / this.state.FBinWidth);
                int c2 = (int)(this.state.freqBand_Max / this.state.FBinWidth);
                this.amplitudM = DataTools.Submatrix(this.AmplitudM, 0, c1, this.amplitudM.GetLength(0)-1, c2);
                //DETERMINE ENERGY IN FFT FREQ BAND
                this.decibels = FreqBandEnergy(this.amplitudM);
                //DETERMINE ENDPOINTS OF VOCALISATIONS
                this.sigState = Speech.VocalizationDetection(this.decibels, k1, k2, k1_k2delay, syllableDelay, minPulse, null);
            }

            //POST-PROCESS to final SPECTROGRAM
            if (this.State.SonogramType == SonogramType.spectral) this.spectralM = MakeSpectrogram(this.amplitudM);
            else
            if (this.State.SonogramType == SonogramType.cepstral) this.cepstralM = MakeCepstrogram(this.amplitudM, this.decibels, this.state.IncludeDelta, this.state.IncludeDoubleDelta);
            else
                if (this.State.SonogramType == SonogramType.acousticVectors) this.acousticM = MakeAcousticVectors(this.amplitudM, this.decibels, this.state.IncludeDelta, this.state.IncludeDoubleDelta, this.state.DeltaT);
            else
            if (this.State.SonogramType == SonogramType.sobelEdge) this.spectralM = SobelEdgegram(this.amplitudM);
        }

        public double[,] GenerateAmplitudeSpectra(double[,] frames, FFT.WindowFunc w, double epsilon)
        {
            int frameCount = frames.GetLength(0);
            int N = frames.GetLength(1);  //= the FFT windowSize 
            int binCount = (N / 2) + 1;  // = fft.WindowSize/2 +1 for the DC value;

            FFT fft = new FFT(N, w); // init class which calculates the FFT

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
                    if (amplitude < epsilon) amplitude = epsilon; //to prevent possible log of a very small number
                    sonogram[i, j] = amplitude;
                }
            } //end of all frames
            return sonogram;
        }


        public double[] FreqBandEnergy(double[,] fftAmplitudes) 
        {
            //Console.WriteLine("minDefinedLogEnergy=" + Sonogram.minLogEnergy.ToString("F2") + "  maxLogEnergy=" + Sonogram.maxLogEnergy);
            double[] logEnergy = DSP.SignalLogEnergy(fftAmplitudes, Sonogram.minLogEnergy, Sonogram.maxLogEnergy);

            //NOTE: FreqBand LogEnergy levels are higher than Frame levels but SNR remains same.
            //double min; double max;
            //DataTools.MinMax(logEnergy, out min, out max);
            //Console.WriteLine("FrameNoise_dB   =" + this.State.FrameNoise_dB    + "  FrameMax_dB   =" + this.State.FrameMax_dB    + "  SNR=" + this.State.Frame_SNR);
            //Console.WriteLine("FreqBandNoise_dB=" + this.State.FreqBandNoise_dB + "  FreqBandMax_dB=" + this.State.FreqBandMax_dB + "  SNR=" + this.State.FreqBand_SNR);
            //Console.WriteLine("FreqBandNoise_dB=" + (min*10) + "  FreqBandMax_dB=" + (max*10) + "  SNR=" + this.State.FreqBand_SNR);

            //noise reduce the energy array to produce decibels array
            double minFraction = Sonogram.minLogEnergy - Sonogram.maxLogEnergy;
            double Q;
            double min_dB;
            double max_dB;
            double[] decibels = DSP.NoiseSubtract(logEnergy, out min_dB, out max_dB, minFraction, Sonogram.noiseThreshold, out Q);
            this.State.NoiseSubtracted = Q;
            this.State.FreqBandNoise_dB = min_dB; //min decibels of all frames 
            this.State.FreqBandMax_dB = max_dB;
            this.State.FreqBand_SNR = max_dB - min_dB;
            this.State.MinDecibelReference = min_dB - this.State.NoiseSubtracted;
            this.State.MaxDecibelReference = this.State.MinDecibelReference + this.State.FreqBand_SNR;
            //this.State.MaxDecibelReference = (Sonogram.maxLogEnergy * 10) - this.State.NoiseSubtracted;
            //Console.WriteLine("Q=" + this.State.NoiseSubtracted + "  MinDBReference=" + this.State.MinDecibelReference + "  MaxDecibelReference=" + this.State.MaxDecibelReference);
            return decibels;
        }


        /// <summary>
        /// trims the values of the passed spectrogram using the Min and Max percentile values in the ini file.
        /// First calculate the value cut-offs for the given percentiles.
        /// Second, calculate the min, avg and max values of the spectrogram.
        /// </summary>
        /// <param name="SPEC"></param>
        /// <param name="min"></param>
        /// <param name="avg"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public double[,] Trim(double[,] SPEC, out double min, out double avg, out double max)
        {
            int frameCount = SPEC.GetLength(0);
            int binCount   = SPEC.GetLength(1);

            //normalise and compress/bound the values
            double minCut;
            double maxCut;
            DataTools.PercentileCutoffs(SPEC, this.state.MinPercentile, this.state.MaxPercentile, out minCut, out maxCut);
            this.state.MinCut = minCut;
            this.state.MaxCut = maxCut;
            this.amplitudM = DataTools.boundMatrix(this.amplitudM, minCut, maxCut);

            min = Double.MaxValue;
            max = Double.MinValue;
            double sum = 0.0;

            for (int i = 0; i < frameCount; i++)//foreach time step
            {
                for (int j = 0; j < binCount; j++) //foreach freq bin
                {
                    double value = SPEC[i, j];
                    if (value < min) min = value;
                    else
                        if (value > max) max = value;
                    sum += value;
                }
            } //end of all frames
            avg = sum / (frameCount * binCount);
            return SPEC;
        }




        public double[,] MakeSpectrogram(double[,] matrix)
        {
            if (this.state.Verbosity > 0) Console.WriteLine(" MakeSpectrogram(double[,] matrix)");
            //error check that filterBankCount < FFTbins
            //int FFTbins = this.State.FreqBinCount;  //number of Hz bands = 2^N +1. Subtract DC bin
            //if (this.State.FilterbankCount > FFTbins)
            //{
            //    //throw new Exception("ERROR - Sonogram.LinearCepstrogram():- Cannot calculate cepstral coefficients. FilterbankCount > FFTbins. " + this.State.FilterbankCount + ">" + FFTbins);
            //    Console.WriteLine("Change size of filter bank from " + this.State.FilterbankCount + " to " + FFTbins);
            //    this.State.FilterbankCount = FFTbins;
            //}

            double Nyquist = this.state.NyquistFreq;
            this.State.MaxMel = Speech.Mel(Nyquist);
            double[,] m = matrix;
            if (this.state.DoMelScale)
            {
                //Console.WriteLine(" Mel Nyquist= " + this.State.MaxMel.ToString("F1"));
                //Console.WriteLine(" Mel Band Count = " + this.state.MelBinCount + " FilterbankCount= " + this.State.FilterbankCount);
                int bandCount = this.State.FilterbankCount; //the default
                if (this.State.SonogramType == SonogramType.spectral) bandCount = this.State.MelBinCount;

                m = Speech.MelConversion(m, bandCount, Nyquist, this.state.freqBand_Min, this.state.freqBand_Max);  //using the Greg integral
                //m = Speech.MelConversion(m, this.State.FilterbankCount, Nyquist);  //using the Greg integral
                //m = Speech.MelFilterbank(m, this.State.FilterbankCount, Nyquist);  //using the Matlab algorithm
            }
            m = Speech.DecibelSpectra(m);
            if (this.State.DoNoiseReduction) m = ImageTools.NoiseReduction(m);
            return m;
        }

        public double[,] MakeCepstrogram(double[,] matrix, double[] decibels, bool includeDelta, bool includeDoubleDelta)
        {
            if (this.state.Verbosity > 0) Console.WriteLine(" MakeCepstrogram(matrix, decibels, includeDelta=" + includeDelta + ", includeDoubleDelta=" + includeDoubleDelta+")");
            
            double[,] m = MakeSpectrogram(matrix);
            this.spectralM = m;
            m = Speech.Cepstra(m, this.State.ccCount);
            m = DataTools.normalise(m); //normalise the MFCC spectrogram

            //normalise energy between 0.0 decibels and max decibels.
            double[] E = Speech.NormaliseEnergyArray(decibels, this.state.MinDecibelReference, this.state.MaxDecibelReference);
            m = Speech.AcousticVectors(m, E, includeDelta, includeDoubleDelta);
            return m;
        }


        public double[,] MakeAcousticVectors(double[,] matrix, double[] decibels, bool includeDelta, bool includeDoubleDelta, int deltaT)
        {
            if (this.state.Verbosity > 0) Console.WriteLine(" MakeAcousticVectors(matrix, decibels, includeDelta=" + includeDelta + ", includeDoubleDelta=" + includeDoubleDelta + ", deltaT=" + deltaT + ")");
            
            double[,] m = MakeCepstrogram(matrix, decibels, includeDelta, includeDoubleDelta);
            this.cepstralM = m;

            //initialise feature vector for template - will contain three acoustic vectors - for T-dT, T and T+dT
            int frameCount = m.GetLength(0);
            int cepstralL  = m.GetLength(1);  // length of cepstral vector 
            int featurevL  = 3 * cepstralL;   // to accomodate cepstra for T-2, T and T+2

            double[] featureVector = new double[featurevL];
            double[,] acousticM = new double[frameCount, featurevL]; //init the matrix of acoustic vectors
            for (int i = deltaT; i < frameCount-deltaT; i++)
            {
                double[] rowTm2 = DataTools.GetRow(m, i - deltaT);
                double[] rowT   = DataTools.GetRow(m, i);
                double[] rowTp2 = DataTools.GetRow(m, i + deltaT);

                for (int j = 0; j < cepstralL; j++) acousticM[i, j] = rowTm2[j];
                for (int j = 0; j < cepstralL; j++) acousticM[i, cepstralL + j] = rowT[j];
                for (int j = 0; j < cepstralL; j++) acousticM[i, cepstralL + cepstralL + j] = rowTp2[j];
            }


            //return m;
            return acousticM;
        }


        public double[,] SobelEdgegram(double[,] matrix)
        {
            double[,] m = Speech.DecibelSpectra(matrix);
            if (this.State.DoNoiseReduction) m = ImageTools.NoiseReduction(m);
            m = ImageTools.SobelEdgeDetection(m);
            return m;
        }



        public double[,] Gradient()
        {
            double gradThreshold = 2.0;
            int fWindow = 11;
            int tWindow = 9;
            double[,] blurM = ImageTools.Blur(this.amplitudM, fWindow, tWindow);
            int height = blurM.GetLength(0);
            int width  = blurM.GetLength(1);
            double[,] outData = new double[height, width];

            double min = Double.MaxValue;
            double max = -Double.MaxValue;

            for (int x = 0; x < width; x++) outData[0, x] = 0.5; //patch in first  time step with zero gradient
            for (int x = 0; x < width; x++) outData[1, x] = 0.5; //patch in second time step with zero gradient
           // for (int x = 0; x < width; x++) this.gradM[2, x] = 0.5; //patch in second time step with zero gradient

            for (int y = 2; y < height - 1; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double grad1 = blurM[y, x] - blurM[y - 1, x];//calculate one step gradient
                    double grad2 = blurM[y, x] - blurM[y - 2, x];//calculate two step gradient

                    //get min and max gradient
                    if (grad1 < min) min = grad1;
                    else
                    if (grad1 > max) max = grad1;

                    // quantize the gradients
                    if (grad1 < -gradThreshold) outData[y, x] = 0.0;
                    else
                        if (grad1 > gradThreshold) outData[y, x] = 1.0;
                        else
                            if (grad2 < -gradThreshold) outData[y, x] = 0.0;
                            else
                                if (grad2 > gradThreshold) outData[y, x] = 1.0;
                                else outData[y, x] = 0.5;
                }
            }

            //for (int x = 0; x < width; x++) this.gradM[height - 1, x] = 0.5; //patch in last time step with medium gradient
            return outData;
        }


        public double[] CalculatePowerHisto()
        {
            int bandCount = this.State.NyquistFreq / Sonogram.binWidth;
            this.State.kHzBandCount = bandCount;
            int tracksPerBand = this.State.FreqBinCount / bandCount;
            int height = this.amplitudM.GetLength(0); //time dimension
            int width = this.amplitudM.GetLength(1);
            double[] power = new double[bandCount];


            for (int f = 0; f < bandCount; f++) // over all 11 bands
            {
                int minTrack = f * tracksPerBand;
                int maxTrack = ((f + 1) * tracksPerBand) - 1;
                for (int y = 0; y < height; y++) //full duration of recording
                {
                    for (int x = minTrack; x < maxTrack; x++) //full width of freq band
                    {
                        power[f] += this.amplitudM[y, x]; //sum the power
                    }
                }

            }

            double[] histo = new double[bandCount];
            for (int f = 0; f < bandCount; f++)
            {
                histo[f] = power[f] / (double)tracksPerBand / state.FrameCount;
            }
            return histo;
        }


        public double[] CalculateEventHisto(double[,] gradM)
        {
            int bandCount = this.State.NyquistFreq / Sonogram.binWidth;
            this.State.kHzBandCount = bandCount;
            int tracksPerBand = this.State.FreqBinCount / bandCount;
            int height = this.amplitudM.GetLength(0); //time dimension
            int width = this.amplitudM.GetLength(1);
            int[] counts = new int[bandCount];

            for (int f = 0; f < bandCount; f++) // over all 11 bands
            {
                int minTrack = f * tracksPerBand;
                int maxTrack = ((f + 1) * tracksPerBand) - 1;
                for (int y = 1; y < height; y++) //full duration of recording
                {
                    for (int x = minTrack; x < maxTrack; x++) //full width of freq band
                    {
                        if (gradM[y, x] != gradM[y-1, x]) counts[f]++; //count any gradient change
                    }
                }
            }
            double[] histo = new double[bandCount];
            for (int f = 0; f < bandCount; f++)
            {
                histo[f] = counts[f] / (double)tracksPerBand / state.TimeDuration;
            }
            return histo;
        }
        public double[] CalculateEvent2Histo(double[,] gradM)
        {
            int bandCount = this.State.NyquistFreq / Sonogram.binWidth;
            this.State.kHzBandCount = bandCount;
            int tracksPerBand = this.State.FreqBinCount / bandCount;
            int height = this.amplitudM.GetLength(0); //time dimension
            int width  = this.amplitudM.GetLength(1);
            double[] positiveGrad = new double[bandCount];
            double[] negitiveGrad = new double[bandCount];


            for (int f = 0; f < bandCount; f++) // over all 11 bands
            {
                int minTrack = f * tracksPerBand;
                int maxTrack = ((f + 1) * tracksPerBand) - 1;
                for (int y = 0; y < height; y++) //full duration of recording
                {
                    for (int x = minTrack; x < maxTrack; x++) //full width of freq band
                    {
                        double d = gradM[y,x];
                        if (d == 0) negitiveGrad[f]++;
                        else if (d == 1) positiveGrad[f]++;
                    }
                }
            }
            double[] histo = new double[bandCount];
            for (int f = 0; f < bandCount; f++)
            {
                if (positiveGrad[f] > negitiveGrad[f]) histo[f] = positiveGrad[f] / (double)tracksPerBand / state.TimeDuration;
                else                                   histo[f] = negitiveGrad[f] / (double)tracksPerBand / state.TimeDuration;
            }
            return histo;
        }

        public double[] CalculateActivityHisto(double[,] gradM)
        {
            int bandCount = this.State.NyquistFreq / Sonogram.binWidth;
            this.State.kHzBandCount = bandCount;
            int tracksPerBand = this.State.FreqBinCount / bandCount;
            int height = this.amplitudM.GetLength(0); //time dimension
            int width = this.amplitudM.GetLength(1);
            double[] activity = new double[bandCount];


            for (int f = 0; f < bandCount; f++) // over all 11 bands
            {
                int minTrack = f * tracksPerBand;
                int maxTrack = ((f + 1) * tracksPerBand) - 1;
                for (int y = 0; y < height; y++) //full duration of recording
                {
                    for (int x = minTrack; x < maxTrack; x++) //full width of freq band
                    {
                        activity[f] += (gradM[y, x] * gradM[y, x]); //add square of gradient
                    }
                }

            }

            double[] histo = new double[bandCount];
            for (int f = 0; f < bandCount; f++)
            {
                histo[f] = activity[f] / (double)tracksPerBand / state.TimeDuration;
            }
            return histo;
        }



        public void WriteInfo()
        {
            Console.WriteLine("\nSONOGRAM INFO");
            Console.WriteLine(" Wav Sample Rate = " + this.State.SampleRate + "\tNyquist Freq = " + this.state.NyquistFreq+" Hz");
            Console.WriteLine(" SampleCount     = " + this.state.SampleCount + "\tDuration=" + this.State.TimeDuration.ToString("F3") + "s");
            Console.WriteLine(" Frame Size      = " + this.state.WindowSize + "\t\tFrame Overlap = " + (int)(this.state.WindowOverlap*100)+"%");
            Console.WriteLine(" Frame duration  = " + (this.state.FrameDuration*1000).ToString("F1") + " ms. \tFrame Offset = " + (this.state.FrameOffset*1000).ToString("F1") + " ms");
            Console.WriteLine(" Frame count     = " + this.state.FrameCount + "\t\tFrames/sec = " + this.state.FramesPerSecond.ToString("F1"));
            Console.WriteLine(" Min freq        = " + this.state.FreqBand_Min + " Hz. \tMax freq = " + this.state.FreqBand_Max + " Hz");
            Console.WriteLine(" Freq Bin Width  = " + (this.state.NyquistFreq / (double)this.state.FreqBinCount).ToString("F1") + " Hz");
            Console.Write(    " Min Frame Noise = " + this.State.FrameNoise_dB.ToString("F2")+" dB");
            Console.WriteLine("\tS/N Ratio = " + this.State.Frame_SNR.ToString("F2") + " dB (maxFrameLogEn-minFrameLogEn)");
            if (this.state.doFreqBandAnalysis)
            {
                Console.Write(" Min FBand Noise = " + this.State.FreqBandNoise_dB.ToString("F2") + " dB");
                Console.WriteLine("\tS/N Ratio = " + this.State.FreqBand_SNR.ToString("F2") + " dB (maxFreqBand-minFreqBand)");
            }
            Console.WriteLine(" Modal Noise(dB) = " + this.State.NoiseSubtracted.ToString("F2")+ "  (This dB level subtracted for normalisation)");
            Console.WriteLine(" Min reference dB= " + this.state.MinDecibelReference.ToString("F2") + "\tMax reference dB=" + this.state.MaxDecibelReference.ToString("F2"));
            //Console.WriteLine(" Min power=" + this.state.PowerMin.ToString("F3") + " Avg power=" + this.state.PowerAvg.ToString("F3") + " Max power=" + this.state.PowerMax.ToString("F3"));
            //Console.WriteLine(" Min percentile=" + this.state.MinPercentile.ToString("F2") + "  Max percentile=" + this.state.MaxPercentile.ToString("F2"));
            //Console.WriteLine(" Min cutoff=" + this.state.MinCut.ToString("F3") + "  Max cutoff=" + this.state.MaxCut.ToString("F3"));

            //write out sonogram params
            Console.WriteLine("\nSONOGRAM TYPE = " + this.State.SonogramType);
            if (this.state.DoMelScale)
            {
                Console.WriteLine(" Mel Nyquist= " + this.State.MaxMel.ToString("F1"));
                Console.WriteLine(" Mel Band Count = " + this.state.MelBinCount);// + " FilterbankCount= " + this.State.FilterbankCount);
            }
            if (this.state.SonogramType == SonogramType.cepstral)
            {
                Console.WriteLine(" Filterbank count = " + this.State.FilterbankCount + "\t\tCepstral coeff count = " + this.state.ccCount);
            }
            
        }

        public void WriteStatistics()
        {
            Console.WriteLine("\nSONOGRAM STATISTICS");
            Console.WriteLine(" Max power=" + this.State.PowerMax.ToString("F3") + " dB");
            Console.WriteLine(" Avg power=" + this.State.PowerAvg.ToString("F3") + " dB");
            //results.WritePowerHisto();
            //results.WritePowerEntropy();
            //results.WriteEventHisto();
            //results.WriteEventEntropy();
        }


        public void SetOutputDir(string dir)
        {
            this.state.SonogramDir = dir;
        }

        public SonogramType GetSonogramType()
        {
            return this.state.SonogramType;
        }

        public void SetVerbose(int v)
        {
            this.state.Verbosity = v;
        }


//***********************************************************************************************************************************
        //         IMAGE SAVING METHODS


        public void SaveImage(double[,] matrix, double[] zscores)
        {
            this.State.BmpFName = this.state.SonogramDir + this.state.WavFName + this.state.BmpFileExt;
            if(this.State.Verbosity!=0)Console.WriteLine("\n Image in file  = " + this.State.BmpFName);

            if (matrix == null)
            {
                throw new Exception("WARNING!!!!  matrix==null CANNOT SAVE THE SONOGRAM AS IMAGE!");
            }
            TrackType trackType = TrackType.score;
            if (zscores == null) trackType = TrackType.energy;
            //if (zscores == null) trackType = TrackType.zeroCrossings;

            SonoImage image = new SonoImage(this, trackType);
            Bitmap bmp = image.CreateBitmap(matrix, zscores);

            string fName = this.state.SonogramDir + this.state.WavFName + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }


        public void SaveImage(double[,] matrix, ArrayList shapes, Color col)
        {
            this.State.SonogramType = SonogramType.spectral; //image is linear scale not mel scale
            TrackType trackType     = TrackType.none;

            SonoImage image = new SonoImage(this, trackType);
            Bitmap bmp = image.CreateBitmap(matrix, null);
            if (shapes != null) bmp = image.AddShapeBoundaries(bmp, shapes, col);

            string fName = this.state.SonogramDir + this.state.WavFName + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }


        public void SaveImageOfSolids(double[,] matrix, ArrayList shapes, Color col)
        {
            this.State.SonogramType = SonogramType.spectral; //image is linear scale not mel scale
            TrackType trackType = TrackType.none;

            SonoImage image = new SonoImage(this, trackType);
            Bitmap bmp = image.CreateBitmap(matrix, null);
            if (shapes != null) bmp = image.AddShapeSolids(bmp, shapes, col);

            string fName = this.state.SonogramDir + this.state.WavFName + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }

        public void SaveImageOfCentroids(double[,] matrix, ArrayList shapes, Color col)
        {
            this.State.SonogramType = SonogramType.spectral; //image is linear scale not mel scale
            TrackType trackType = TrackType.none;

            SonoImage image = new SonoImage(this, trackType);
            Bitmap bmp = image.CreateBitmap(matrix, null);
            if (shapes != null) bmp = image.AddCentroidBoundaries(bmp, shapes, col);

            string fName = this.state.SonogramDir + this.state.WavFName + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }


        public void SaveImage(string opDir, double[] zscores, SonogramType sonogramType)
        {
            TrackType trackType = TrackType.none;
            SonoImage image = new SonoImage(state, sonogramType, trackType);
            Bitmap bmp = image.CreateBitmap(this.amplitudM, zscores);

            string fName = opDir + "//" + this.state.WavFName + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }



        /// <summary>
        /// WARNING!! This method must be consistent with the ANALYSIS HEADER line declared in Results.AnalysisHeader()
        /// </summary>
        /// <param name="id"></param>
        /// <param name="syllableDistribution"></param>
        /// <param name="categoryDistribution"></param>
        /// <param name="categoryCount"></param>
        /// <returns></returns>
        public string OneLineResult(int id, int[] syllableDistribution, int[] categoryDistribution, int categoryCount)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(id + Results.spacer); //CALLID
            //sb.Append(DateTime.Now.ToString("u") + spacer); //DATE
            sb.Append(this.State.WavFName.ToString() + Results.spacer); //sonogram FNAME
            sb.Append(this.State.Date.ToString() + Results.spacer); //sonogram date
            sb.Append(this.State.DeployName + Results.spacer); //Deployment name
            sb.Append(this.State.TimeDuration.ToString("F2") + Results.spacer); //length of recording
            sb.Append(this.State.Hour + Results.spacer); //hour when recording made
            sb.Append(this.State.Minute + Results.spacer); //hour when recording made
            sb.Append(this.State.TimeSlot + Results.spacer); //half hour when recording made

            sb.Append(this.State.WavMax.ToString("F4") + Results.spacer);
            sb.Append(this.State.FrameNoise_dB.ToString("F4") + Results.spacer);
            sb.Append(this.State.Frame_SNR.ToString("F4") + Results.spacer);
            sb.Append(this.State.PowerMax.ToString("F3") + Results.spacer);
            sb.Append(this.State.PowerAvg.ToString("F3") + Results.spacer);

            //syllable distribution
            if ((categoryCount == 0) || (syllableDistribution==null))
                for (int f = 0; f < Results.analysisBandCount; f++) sb.Append("0  " + Results.spacer);
            else
                for (int f = 0; f < Results.analysisBandCount; f++) sb.Append(syllableDistribution[f].ToString() + Results.spacer);
            sb.Append(DataTools.Sum(syllableDistribution) + Results.spacer);

            //category distribution
            if ((categoryCount == 0) || (syllableDistribution == null))
                for (int f = 0; f < Results.analysisBandCount; f++) sb.Append("0  " + Results.spacer);
            else
                for (int f = 0; f < Results.analysisBandCount; f++) sb.Append(categoryDistribution[f].ToString() + Results.spacer);
            sb.Append(categoryCount + Results.spacer);

            //monotony index
            double sum = 0.0;
            double monotony = 0.0;
            if ((categoryCount == 0) || (syllableDistribution == null))
            {
                for (int f = 0; f < Results.analysisBandCount; f++) sb.Append("0.0000" + Results.spacer);
                sb.Append("0.0000" + Results.spacer);
            }
            else
            {
                for (int f = 0; f < Results.analysisBandCount; f++)
                {
                    if (categoryDistribution[f] == 0) monotony = 0.0;
                    else                              monotony = syllableDistribution[f] / (double)categoryDistribution[f];
                    sb.Append(monotony.ToString("F4") + Results.spacer);
                    sum += monotony;
                }
                double av = sum / (double)Results.analysisBandCount;
                sb.Append(av.ToString("F4") + Results.spacer);
            }
            sb.Append(this.State.WavFName.ToString() + Results.spacer);
            
            return sb.ToString();
        }




    } //end class Sonogram



    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************



    
    public class SonoConfig
    {
        //GENRAL
        public int Verbosity { get; set; }

        //files and directories
        public string TemplateDir { get; set; }
        public string WavFName { get; set; }
        public string WavFileDir { get; set; }
        public string WavFileExt { get; set; }
        public string SonogramDir { get; set; }
        public string OutputDir { get; set; }
        public string BmpFName { get; set; }
        public string BmpFileExt { get; set; }


        //wav file info
        public string DeployName { get; set; }
        public string Date { get; set; }
        public int    Hour { get; set; }
        public int    Minute { get; set; }
        public int    TimeSlot { get; set; }
        public double WavMax { get; set; }

        //SIGNAL PARAMETERS
        public int SampleRate { get; set; }
        public int SampleCount { get; set; }
        public double TimeDuration { get; set; }

        // FRAMING or WINDOWING
        public int WindowSize { get; set; }
        public double WindowOverlap { get; set; }  //percent overlap of frames
        public double FrameDuration { get; set; }     //duration of full frame or window in seconds
        public double FrameOffset { get; set; }       //duration of non-overlapped part of window/frame in seconds
        public int    FrameCount { get; set; }        //number of frames
        public double FramesPerSecond { get; set; }

        //SIGNAL FRAME ENERGY AND SEGMENTATION PARAMETERS
        public double FrameMax_dB { get; set; }
        public double FrameNoise_dB { get; set; }
        public double Frame_SNR { get; set; }
        public double NoiseSubtracted { get; set; }         //noise (dB) subtracted from each frame decibel value
        public double MinDecibelReference { get; set; }     //min reference dB value after noise substraction
        public double MaxDecibelReference { get; set; }     //max reference dB value after noise substraction
        public double SegmentationThreshold_k1 { get; set; }//dB threshold for recognition of vocalisations
        public double SegmentationThreshold_k2 { get; set; }//dB threshold for recognition of vocalisations
        public double k1_k2Latency { get; set; }            //seconds delay between signal reaching k1 and k2 thresholds
        public double vocalDelay { get; set; }              //seconds delay required to separate vocalisations 
        public double minPulseDuration { get; set; }        //minimum length of energy pulse - do not use this - 

        //SPECTRAL ENERGY AND SEGMENTATION PARAMETERS
        public double FreqBandMax_dB { get; set; }
        public double FreqBandNoise_dB { get; set; }
        public double FreqBand_SNR { get; set; }
        public double FreqBand_NoiseSubtracted { get; set; }         //noise (dB) subtracted from each frame decibel value
        public double FreqBand_MinDecibelReference { get; set; }     //min reference dB value after noise substraction
        public double FreqBand_MaxDecibelReference { get; set; }     //max reference dB value after noise substraction

        //SONOGRAM parameters
        public int MinFreq { get; set; }                   //default min freq = 0 Hz  
        public int NyquistFreq { get; set; }               //default max freq = Nyquist = half audio sampling freq
        public int FreqBinCount { get; set; }         //number of FFT values 
        public double FBinWidth { get; set; }
        public int kHzBandCount { get; set; }         //number of one kHz bands
        public int freqBand_Min = -1000;              //min of the freq band to be analysed  
        public int FreqBand_Min { get { return freqBand_Min; } set { freqBand_Min = value;} }   
        public int freqBand_Max = -1000;              //max of the freq band to be analysed
        public int FreqBand_Max { get { return freqBand_Max; } set { freqBand_Max = value; } }
        public int FreqBand_Mid { get; set; }
        public bool   doFreqBandAnalysis = false;
        public double PowerMin { get; set; }                //min power in sonogram
        public double PowerAvg { get; set; }                //average power in sonogram
        public double PowerMax { get; set; }                //max power in sonogram

        //FFT parameters
        public string WindowFncName { get; set; }
        public FFT.WindowFunc WindowFnc { get; set; }
        public int NPointSmoothFFT { get; set; }      //number of points to smooth FFT spectra

        // MEL SCALE PARAMETERS
        public int FilterbankCount { get; set; }
        public int MelBinCount { get; set; }    //number of mel spectral values 
        public double MinMelPower { get; set; } //min power in mel sonogram
        public double MaxMelPower { get; set; } //max power in mel sonogram
        public double MaxMel { get; set; }      //Nyquist frequency on Mel scale

        // MFCC parameters
        public SonogramType SonogramType { get; set; }
        private bool doMelScale;
        public bool DoMelScale { get { return doMelScale; } set { doMelScale = value; } }
        public bool DoNoiseReduction { get; set; }
        public int    ccCount { get; set; }     //number of cepstral coefficients
        public double MinCepPower { get; set; } //min value in cepstral sonogram
        public double MaxCepPower { get; set; } //max value in cepstral sonogram
        private int deltaT;
        public int DeltaT { get { return deltaT; } set { deltaT = value; } }
        private bool includeDelta;
        public bool IncludeDelta { get { return includeDelta; } set { includeDelta = value; } }
        private bool includeDoubleDelta;
        public bool IncludeDoubleDelta { get { return includeDoubleDelta; } set { includeDoubleDelta = value; } }



        //FEATURE VECTOR PARAMETERS 
        public FV_Source FeatureVectorSource { get; set; }
        public string[] FeatureVector_SelectedFrames { get; set; } //store frames as strings for flexibility
        public int MarqueeStart { get; set; }
        public int MarqueeEnd { get; set; }
        public FV_Extraction FeatureVectorExtraction { get; set; }
        public int FeatureVectorExtractionInterval { get; set; }
        public bool FeatureVector_DoAveraging { get; set; }
        public string FeatureVector_DefaultNoiseFile { get; set; }

        public int FeatureVectorCount { get; set; }
        public int FeatureVectorLength { get; set; }
        public string[] FeatureVectorPaths { get; set; }
        public string[] FVSourceFiles { get; set; }
        public string DefaultNoiseFVFile { get; set; }

        //THE LANGUAGE MODEL
        public bool HighSensitivitySearch; //USE_HIGH_SENSITIVITY_SEARCH will increase false positives i.e. reduce specificity
        public int WordCount { get; set; }
        public string[] Words { get; set; }

        //SONG PARAMETERS
        public double TypicalSongDuration { get; set; }


        // SCORING PROTOCOL
        public ScoringProtocol ScoringProtocol { get; set; }

        //CALL PERIODICITY INFO
        public int CallPeriodicity_ms { get; set; }
        public int CallPeriodicity_frames { get; set; }
        public int CallPeriodicity_NH_ms { get; set; }
        public int CallPeriodicity_NH_frames { get; set; }



        //BITMAP IMAGE PARAMETERS 
        public bool AddGrid { get; set; }
        public TrackType TrackType { get; set; }

        public double MinPercentile { get; set; }
        public double MaxPercentile { get; set; }
        public double MinCut { get; set; } //power of min percentile
        public double MaxCut { get; set; } //power of max percentile


        //TEMPLATE PARAMETERS
        public int CallID { get; set; }
        public string CallName { get; set; }
        public string CallComment { get; set; }
        public string SourceFStem { get; set; }
        public string SourceFName { get; set; }
        public string SourceFPath { get; set; }

        //freq bins of the scanned part of sonogram
        public int MaxTemplateFreq { get; set; }
        public int MidTemplateFreq { get; set; }
        public int MinTemplateFreq { get; set; }

        public int BlurWindow { get; set; }
        public int BlurWindow_time { get; set; }
        public int BlurWindow_freq { get; set; }
        //public bool NormSonogram { get; set; }
        public int ZscoreSmoothingWindow { get; set; }
        public double ZScoreThreshold { get; set; }
        public double NoiseAv { get; set; }
        public double NoiseSd { get; set; }


        /// <summary>
        /// converts wave file names into component info 
        /// wave file name have following format: "BAC1_20071008-081607"
        /// </summary>
        /// <param name="FName"></param>
        public void SetDateAndTime(string fName)
        {
            if ((fName == null) || (fName.Length == 0))
            {
                SetDefaultDateAndTime("noName");
                return;
            }

            string[] parts = fName.Split('_');
            if (parts.Length == 1) 
            {
                SetDefaultDateAndTime(fName); 
                return;
            }
            this.DeployName = parts[0];
            parts = parts[1].Split('-');
            if (parts.Length == 1)
            {
                SetDefaultDateAndTime(fName);
                return;
            }
            this.Date = parts[0];
            this.Hour = Int32.Parse(parts[1].Substring(0,2));
            this.Minute = Int32.Parse(parts[1].Substring(2, 2));
            //############ WARNING!!! THE FOLLOWING LINE MUST BE CONSISTENT WITH TIMESLOT CONSTANT
            this.TimeSlot = ((this.Hour*60)+Minute)/30; //convert to half hour time slots
        }
        public void SetDefaultDateAndTime(string name)
        {
            this.DeployName = name;
            this.Date = "000000";
            this.Hour = 0;
            this.Minute = 0;
            this.TimeSlot = 0;
        }
        
        public void ReadConfig(string iniFName)
        {
            Configuration cfg = new Configuration(iniFName);
            ReadConfig(cfg);
        }

        public void ReadConfig(Configuration cfg)
        {
            //general parameters
            this.Verbosity = cfg.GetInt("VERBOSITY");

            //directory and file structure
            this.TemplateDir = cfg.GetString("TEMPLATE_DIR");
            this.WavFileDir = cfg.GetString("WAV_DIR");
            this.SonogramDir = cfg.GetString("SONOGRAM_DIR");
            this.OutputDir = cfg.GetString("OP_DIR");

            this.WavFileExt = cfg.GetString("WAV_FILEEXT");
            this.BmpFileExt = cfg.GetString("BMP_FILEEXT");

            //FRAMING PARAMETERS
            this.WindowSize = cfg.GetInt("WINDOW_SIZE");
            this.WindowOverlap = cfg.GetDouble("WINDOW_OVERLAP");

            //ENERGY AND SEGMENTATION PARAMETERS
            this.SegmentationThreshold_k1 = cfg.GetDouble("SEGMENTATION_THRESHOLD_K1"); //dB threshold for recognition of vocalisations
            this.SegmentationThreshold_k2 = cfg.GetDouble("SEGMENTATION_THRESHOLD_K2"); //dB threshold for recognition of vocalisations
            this.k1_k2Latency = cfg.GetDouble("K1_K2_LATENCY");           //seconds delay between signal reaching k1 and k2 thresholds
            this.vocalDelay = cfg.GetDouble("VOCAL_DELAY");              //seconds delay required to separate vocalisations 
            this.minPulseDuration = cfg.GetDouble("MIN_VOCAL_DURATION");        //minimum length of energy pulse - do not use this - 

            //FFT params
            this.WindowFncName = cfg.GetString("WINDOW_FUNCTION");
            this.WindowFnc = FFT.GetWindowFunction(this.WindowFncName);
            this.NPointSmoothFFT = cfg.GetInt("N_POINT_SMOOTH_FFT");

            // MFCC parameters
            this.SonogramType = Sonogram.SetSonogramType(cfg.GetString("SONOGRAM_TYPE"));
            this.doMelScale = cfg.GetBoolean("DO_MELSCALE");
            this.freqBand_Min = cfg.GetInt("MIN_FREQ");    //min of the freq band to be analysed  
            this.freqBand_Max = cfg.GetInt("MAX_FREQ");    //max of the freq band to be analysed
            this.DoNoiseReduction = cfg.GetBoolean("NOISE_REDUCE");
            this.FilterbankCount = cfg.GetInt("FILTERBANK_COUNT");
            this.ccCount = cfg.GetInt("CC_COUNT"); //number of cepstral coefficients
            this.IncludeDelta = cfg.GetBoolean("INCLUDE_DELTA");
            this.IncludeDoubleDelta = cfg.GetBoolean("INCLUDE_DOUBLE_DELTA");
            this.DeltaT = cfg.GetInt("DELTA_T"); //frames between acoustic vectors

            //sonogram image parameters
            this.TrackType = SonoImage.GetTrackType(cfg.GetString("TRACK_TYPE"));
            this.AddGrid = cfg.GetBoolean("ADDGRID");

            this.MinPercentile = cfg.GetDouble("MIN_PERCENTILE");
            this.MaxPercentile = cfg.GetDouble("MAX_PERCENTILE");
             this.BlurWindow = cfg.GetInt("BLUR_NEIGHBOURHOOD");
            this.BlurWindow_time = cfg.GetInt("BLUR_TIME_NEIGHBOURHOOD");
            this.BlurWindow_freq = cfg.GetInt("BLUR_FREQ_NEIGHBOURHOOD");
            //this.NormSonogram = cfg.GetBoolean("NORMALISE_SONOGRAM");

        }


    } //end class SonoConfig

}