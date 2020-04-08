// <copyright file="AmplitudeSpectrogram.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using Acoustics.Tools.Wav;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.WavTools;

    /// <summary>
    /// This class is designed to produce a full-bandwidth amplitude spectrogram.
    /// </summary>
    public class AmplitudeSpectrogram
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AmplitudeSpectrogram"/> class.
        /// </summary>
        public AmplitudeSpectrogram(SpectrogramSettings config, WavReader wav)
        {
            this.Configuration = config;
            this.Attributes = new SpectrogramAttributes();

            double minDuration = 1.0;
            if (wav.Time.TotalSeconds < minDuration)
            {
                LoggedConsole.WriteLine("Signal must at least {0} seconds long to produce a sonogram!", minDuration);
                return;
            }

            //set attributes for the current recording and spectrogram type
            this.Attributes.SampleRate = wav.SampleRate;
            this.Attributes.Duration = wav.Time;
            this.Attributes.NyquistFrequency = wav.SampleRate / 2;
            this.Attributes.Duration = wav.Time;
            this.Attributes.MaxAmplitude = wav.CalculateMaximumAmplitude();
            this.Attributes.FrameDuration = TimeSpan.FromSeconds(this.Configuration.WindowSize / (double)wav.SampleRate);

            var recording = new AudioRecording(wav);
            var fftdata = DSP_Frames.ExtractEnvelopeAndFfts(
                recording,
                config.WindowSize,
                config.WindowOverlap,
                this.Configuration.WindowFunction);

            // now recover required data
            //epsilon is a signal dependent minimum amplitude value to prevent possible subsequent log of zero value.
            this.Attributes.Epsilon = fftdata.Epsilon;
            this.Attributes.WindowPower = fftdata.WindowPower;
            this.Attributes.FrameCount = fftdata.FrameCount;
            this.Data = fftdata.AmplitudeSpectrogram;

            // IF REQUIRED CONVERT TO MEL SCALE
            if (this.Configuration.DoMelScale)
            {
                // this mel scale conversion uses the "Greg integral" !
                this.Data = MFCCStuff.MelFilterBank(this.Data, this.Configuration.MelBinCount, this.Attributes.NyquistFrequency, 0, this.Attributes.NyquistFrequency);
            }
        }

        public SpectrogramSettings Configuration { get; set; }

        public SpectrogramAttributes Attributes { get; set; }

        /// <summary>
        /// Gets or sets the spectrogram data matrix of doubles.
        /// </summary>
        public double[,] Data { get; set; }
    }
}