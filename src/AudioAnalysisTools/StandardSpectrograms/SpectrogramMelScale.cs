// <copyright file="SpectrogramMelScale.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using Acoustics.Tools.Wav;
    using AudioAnalysisTools.DSP;
    using TowseyLibrary;

    public class SpectrogramMelScale : BaseSonogram
    {
        public SpectrogramMelScale(string configFile, WavReader wav)
            : this(SonogramConfig.Load(configFile), wav)
        {
        }

        public SpectrogramMelScale(SonogramConfig config, WavReader wav)
            : base(config, wav)
        {
        }

        public SpectrogramMelScale(AmplitudeSonogram sg)
            : base(sg.Configuration)
        {
            this.Configuration = sg.Configuration;
            this.DecibelsPerFrame = sg.DecibelsPerFrame;
            this.DecibelsNormalised = sg.DecibelsNormalised;
            this.Duration = sg.Duration;
            this.FrameCount = sg.FrameCount;
            this.DecibelReference = sg.DecibelReference;
            this.MaxAmplitude = sg.MaxAmplitude;
            this.SampleRate = sg.SampleRate;
            this.SigState = sg.SigState;
            this.SnrData = sg.SnrData;
            this.Data = sg.Data;

            //converts amplitude matrix to Mel-frequency scale spectrogram
            this.Make(this.Data);
        }

        public SpectrogramMelScale(AmplitudeSonogram sg, int minHz, int maxHz)
            : this(sg)
        {
            this.DecibelsPerFrame = sg.DecibelsPerFrame;
            this.DecibelsNormalised = sg.DecibelsNormalised;
            this.Duration = sg.Duration;
            this.FrameCount = sg.FrameCount;
            this.DecibelReference = sg.DecibelReference;
            this.MaxAmplitude = sg.MaxAmplitude;
            this.SampleRate = sg.SampleRate;
            this.SigState = sg.SigState;
            this.SnrData = sg.SnrData;

            //converts amplitude matrix to mel-frequency scale spectrogram
            this.Data = SpectrogramTools.ExtractFreqSubband(sg.Data, minHz, maxHz, this.Configuration.DoMelScale, sg.Configuration.FreqBinCount, sg.FBinWidth);
            this.Make(this.Data);
        }

        /// <summary>
        /// Converts amplitude matrix to mel-frequency scale spectrogram.
        /// </summary>
        /// <param name="amplitudeM">Matrix of amplitude values.</param>
        public override void Make(double[,] amplitudeM)
        {
            var tuple = MakeMelScaleSpectrogram(this.Configuration, amplitudeM, this.SampleRate);
            this.Data = tuple.Item1;
            this.ModalNoiseProfile = tuple.Item2; //store the full bandwidth modal noise profile
        }

        //##################################################################################################################################

        /// <summary>
        /// NOTE!!!! The decibel array has been normalised in 0 - 1.
        /// </summary>
        protected static Tuple<double[,], double[]> MakeMelScaleSpectrogram(SonogramConfig config, double[,] matrix, int sampleRate)
        {
            double[,] m = matrix;
            int nyquist = sampleRate / 2;
            double epsilon = config.epsilon;

            //(i) APPLY FILTER BANK
            int bandCount = config.mfccConfig.FilterbankCount;
            int fftBinCount = config.FreqBinCount;  //number of Hz bands = 2^N +1. Subtract DC bin

            Log.WriteIfVerbose("ApplyFilterBank(): Dim prior to filter bank  =" + matrix.GetLength(1));

            //error check that filterBankCount < Number of FFT bins
            if (bandCount > fftBinCount)
            {
                throw new Exception(
                    "## FATAL ERROR in MakeMelScaleSpectrogram(): Filterbank Count > number of FFT bins. (" +
                    bandCount + " > " + fftBinCount + ")\n\n");
            }

            //this is the filter count for full bandwidth 0-Nyquist. This number is trimmed proportionately to fit the required bandwidth.
            m = MFCCStuff.MelFilterBank(m, bandCount, nyquist, 0, nyquist);

            Log.WriteIfVerbose("\tDim after filter bank=" + m.GetLength(1) + " (Max filter bank=" + bandCount + ")");

            //(ii) CONVERT AMPLITUDES TO DECIBELS
            m = MFCCStuff.DecibelSpectra(m, config.WindowPower, sampleRate, epsilon); //from spectrogram

            //(iii) NOISE REDUCTION
            var tuple1 = SNR.NoiseReduce(m, config.NoiseReductionType, config.NoiseReductionParameter);
            m = tuple1.Item1;

            //(iv) Normalize Matrix Values
            m = DataTools.normalise(m);

            var tuple2 = Tuple.Create(m, tuple1.Item2);

            // return matrix and full bandwidth modal noise profile
            return tuple2;
        }
    } // end class SpectrogramMelScale
}