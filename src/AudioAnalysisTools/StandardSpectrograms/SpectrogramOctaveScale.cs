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
        public SpectrogramOctaveScale(SonogramConfig config, FrequencyScale scale, WavReader wav)
            : base(config, scale, wav)
        {
        }

        /// <summary>
        /// Converts amplitude matrix to octave frequency scale spectrogram.
        /// IMPORTANT: DOES NOISE REDUCTION after conversion.
        /// </summary>
        /// <param name="amplitudeM">Matrix of amplitude values.</param>
        public override void Make(double[,] amplitudeM)
        {
            double windowPower = this.Configuration.WindowPower;
            int sampleRate = this.SampleRate;
            double epsilon = this.Configuration.epsilon;
            double[,] m = OctaveFreqScale.ConvertAmplitudeSpectrogramToFreqScaledDecibels(amplitudeM, windowPower, sampleRate, epsilon, this.FreqScale);

            // Do noise reduction
            var tuple = SNR.NoiseReduce(m, this.Configuration.NoiseReductionType, this.Configuration.NoiseReductionParameter);
            this.Data = DataTools.normalise(tuple.Item1);

            //store the full bandwidth modal noise profile
            this.ModalNoiseProfile = tuple.Item2;
        }
    }
}