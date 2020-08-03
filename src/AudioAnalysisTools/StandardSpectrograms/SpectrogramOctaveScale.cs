// <copyright file="SpectrogramOctaveScale.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using Acoustics.Tools.Wav;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.WavTools;
    using TowseyLibrary;

    public class SpectrogramOctaveScale : BaseSonogram
    {
        public SpectrogramOctaveScale(string configFile, WavReader wav)
            : this(SonogramConfig.Load(configFile), wav)
        {
        }

        public SpectrogramOctaveScale(SonogramConfig config, WavReader wav)
            : base(config, wav)
        {
        }

        public SpectrogramOctaveScale(AmplitudeSonogram sg)
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
            this.Make(sg.Data);
        }

        /// <summary>
        /// Converts amplitude matrix to octave frequency scale spectrogram.
        /// </summary>
        /// <param name="amplitudeM">Matrix of amplitude values.</param>
        public override void Make(double[,] amplitudeM)
        {
            // Make the octave scale spectrogram.
            // Linear portion extends from 0 to H hertz where H can = 1000, 500, 250, 125.
            int linearLimit = 1000;
            var m = MakeOctaveScaleSpectrogram(this.Configuration, amplitudeM, this.SampleRate, linearLimit);

            // Do noise reduction
            var tuple = SNR.NoiseReduce(m, this.Configuration.NoiseReductionType, this.Configuration.NoiseReductionParameter);
            this.Data = DataTools.normalise(tuple.Item1);

            //store the full bandwidth modal noise profile
            this.ModalNoiseProfile = tuple.Item2;
        }

        //##################################################################################################################################

        /// <summary>
        /// Converts amplitude spectrogram to octave scale using one of the possible octave scale types.
        /// </summary>
        public static double[,] MakeOctaveScaleSpectrogram(SonogramConfig config, double[,] matrix, int sampleRate, int linearLimit)
        {
            //var freqScale = new FrequencyScale(FreqScaleType.LinearOctaveStandard);
            //var freqScale = new FrequencyScale(FreqScaleType.OctaveDataReduction);
            //var freqScale = new FrequencyScale(FreqScaleType.Linear125Octaves6Tones30Nyquist11025);
            var freqScale = new FrequencyScale(FreqScaleType.Linear62Octaves7Tones31Nyquist11025);

            double[,] m = OctaveFreqScale.ConvertAmplitudeSpectrogramToDecibelOctaveScale(matrix, freqScale);
            return m;
        }
    }
}