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
        /// ########################################################################TODO TODO need to init as AmplitudeSpectrogram and then use constructor at Line 25.
        /// </summary>
        /// <param name="amplitudeM">Matrix of amplitude values.</param>
        public override void Make(double[,] amplitudeM)
        {
            //var freqScale = new FrequencyScale(FreqScaleType.LinearOctaveStandard);
            //var freqScale = new FrequencyScale(FreqScaleType.OctaveDataReduction);
            var freqScale = new FrequencyScale(FreqScaleType.Linear125OctaveTones32Nyquist11025);
            //var freqScale = new FrequencyScale(FreqScaleType.Linear62OctaveTones31Nyquist11025);

            double[,] m = OctaveFreqScale.ConvertAmplitudeSpectrogramToDecibelOctaveScale(amplitudeM, freqScale);

            // Do noise reduction
            var tuple = SNR.NoiseReduce(m, this.Configuration.NoiseReductionType, this.Configuration.NoiseReductionParameter);
            this.Data = DataTools.normalise(tuple.Item1);

            //store the full bandwidth modal noise profile
            this.ModalNoiseProfile = tuple.Item2;
        }
    }
}