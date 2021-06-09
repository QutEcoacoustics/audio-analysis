// <copyright file="SpectrogramCepstral.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using Acoustics.Tools.Wav;
    using AudioAnalysisTools.DSP;
    using TowseyLibrary;

    public class SpectrogramCepstral : BaseSonogram
    {
        public SpectrogramCepstral(string configFile, WavReader wav)
            : this(SonogramConfig.Load(configFile), wav)
        {
        }

        public SpectrogramCepstral(SonogramConfig config, WavReader wav)
            : base(config, wav)
        {
        }

        public SpectrogramCepstral(AmplitudeSonogram sg)
            : base(sg.Configuration)
        {
            this.Configuration = sg.Configuration;
            this.DecibelsPerFrame = sg.DecibelsPerFrame;
            this.Duration = sg.Duration;
            this.FrameCount = sg.FrameCount;
            this.DecibelReference = sg.DecibelReference;
            this.MaxAmplitude = sg.MaxAmplitude;
            this.SampleRate = sg.SampleRate;
            this.SigState = sg.SigState;
            this.SnrData = sg.SnrData;
            this.Data = sg.Data;

            //converts amplitude matrix to cepstral sonogram
            this.Make(this.Data);
        }

        /// <summary>
        /// Converts amplitude matrix to cepstral sonogram.
        /// </summary>
        /// <param name="amplitudeM">Matrix of amplitude values.</param>
        public override void Make(double[,] amplitudeM)
        {
            this.Data = SpectrogramCepstral.MakeCepstrogram(this.Configuration, amplitudeM, this.DecibelsPerFrame, this.SampleRate);
        }

        //##################################################################################################################################

        /// <summary>
        /// Returns a cepstrogram matrix of mfcc values from a spectrogram matrix of amplitude values.
        /// This was revised May/June 2021 in light of more recent literature on mfcc's.
        /// </summary>
        protected static double[,] MakeCepstrogram(SonogramConfig config, double[,] matrix, double[] frameLogEnergy, int sampleRate)
        {
            int nyquist = sampleRate / 2;
            double epsilon = config.epsilon;
            bool includeDelta = config.mfccConfig.IncludeDelta;
            bool includeDoubleDelta = config.mfccConfig.IncludeDoubleDelta;

            //(i) MAKE THE FILTER BANK
            int bandCount = config.mfccConfig.FilterbankCount;
            int ccCount = config.mfccConfig.CcCount;
            int fftBinCount = config.FreqBinCount;  //number of Hz bands = 2^N +1. Subtract DC bin

            Log.WriteIfVerbose("ApplyFilterBank(): Dim prior to filter bank  =" + matrix.GetLength(1));

            //error check that filterBankCount < Number of FFT bins
            if (bandCount > fftBinCount)
            {
                throw new Exception(
                    "## FATAL ERROR in BaseSonogram.MakeCepstrogram():- Cannot prepare filterbank. Filter count > number of FFT bins. (" +
                    bandCount + " > " + fftBinCount + ")\n\n");
            }

            // (ii) CONVERT AMPLITUDES TO ENERGY
            double[,] m = MatrixTools.SquareValues(matrix);

            // (iii) Do MEL-FILTERBANK.
            // The filter count for full bandwidth 0-Nyquist.
            m = MFCCStuff.MelFilterBank(m, bandCount, nyquist, 0, nyquist);
            Log.WriteIfVerbose("\tDim after filter bank=" + m.GetLength(1) + " (Max filter bank=" + bandCount + ")");

            // (iv) TAKE LOG OF THE ENERGY VALUES AFTER FILTERBANK
            m = MFCCStuff.GetLogEnergySpectrogram(m, config.WindowPower, sampleRate, epsilon); //from spectrogram

            // (v) NORMALISE AND SQUARE THE MEL VALUES BEFORE DOING DCT
            // This reduces the smaller values wrt the higher energy values.
            // Some mfcc references state that it helps to increase the accuracy of ASR.
            m = MatrixTools.NormaliseMatrixValues(m);
            m = MatrixTools.SquareValues(m);

            // (vi) calculate cepstral coefficients and normalise
            m = MFCCStuff.Cepstra(m, ccCount);
            m = MatrixTools.NormaliseMatrixValues(m);

            // (vii) Calculate the full range of MFCC coefficients ie including decibel and deltas, etc
            // normalise the array of frame log-energy values before adding into the mfcc feature vectors.
            var frameLogEnergyNormed = DataTools.normalise(frameLogEnergy);
            m = MFCCStuff.AcousticVectors(m, frameLogEnergyNormed, includeDelta, includeDoubleDelta);

            return m;
        }
    }
}