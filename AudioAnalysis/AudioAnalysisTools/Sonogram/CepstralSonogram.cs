using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;
using Acoustics.Tools.Wav;
using System.IO;


namespace AudioAnalysisTools.Sonogram
{
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

        public CepstralSonogram(AmplitudeSonogram sg, int minHz, int maxHz): this(sg)
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
            this.Data = SpectrogramTools.ExtractFreqSubband(sg.Data, minHz, maxHz,
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



        //##################################################################################################################################


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
            double[,] m = SpectrogramTools.ExtractFreqSubband(data, minHz, maxHz, doMelScale, freqBinCount, freqBinWidth);
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
            double[] noise_subband = SpectrogramTools.ExtractModalNoiseSubband(modalNoise, minHz, maxHz, doMelScale,
                                                                           sonogram.NyquistFrequency, sonogram.FBinWidth);
            //CALCULATE CEPSTROGRAM
            Log.WriteLine("# Extracting Cepstrogram");
            CepstralSonogram cepstrogram = new CepstralSonogram(basegram, minHz, maxHz);  //cepstrogram has dim[N,13]
            var tuple = System.Tuple.Create(sonogram, cepstrogram, modalNoise, noise_subband);
            return tuple;
        }



    } // end class CepstralSonogram


    //##################################################################################################################################
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



}
