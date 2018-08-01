// <copyright file="AmplitudeSonogram.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using Acoustics.Tools.Wav;
    using DSP;
    using WavTools;

    /// <summary>
    /// This class is designed to produce a full-bandwidth spectrogram of spectral amplitudes
    /// </summary>
    public class AmplSpectrogram
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AmplSpectrogram"/> class.
        /// Use this constructor when you have two paths for config file and audio file
        /// </summary>
        public AmplSpectrogram(string configFile, string audioFile)
            : this(SonogramConfig.Load(configFile), new WavReader(audioFile))
        {
        }

        public AmplSpectrogram(SonogramConfig config, WavReader wav)
        {
            this.Configuration = config;
            this.Duration = wav.Time;
            double minDuration = 1.0;
            if (this.Duration.TotalSeconds < minDuration)
            {
                LoggedConsole.WriteLine("Signal must at least {0} seconds long to produce a sonogram!", minDuration);
                return;
            }

            //set config params to the current recording
            this.SampleRate = wav.SampleRate;
            this.Configuration.Duration = wav.Time;
            this.Configuration.SampleRate = wav.SampleRate; //also set the Nyquist
            this.MaxAmplitude = wav.CalculateMaximumAmplitude();

            var recording = new AudioRecording(wav);
            var fftdata = DSP_Frames.ExtractEnvelopeAndFfts(
                recording,
                config.WindowSize,
                config.WindowOverlap,
                this.Configuration.WindowFunction);

            // now recover required data
            //epsilon is a signal dependent minimum amplitude value to prevent possible subsequent log of zero value.
            this.Configuration.epsilon = fftdata.Epsilon;
            this.Configuration.WindowPower = fftdata.WindowPower;
            this.FrameCount = fftdata.FrameCount;
            this.Data = fftdata.AmplitudeSpectrogram;

            // IF REQUIRED CONVERT TO MEL SCALE
            if (this.Configuration.DoMelScale)
            {
                this.Data = MFCCStuff.MelFilterBank(this.Data, this.Configuration.MelBinCount, this.NyquistFrequency, 0, this.NyquistFrequency); // using the Greg integral
            }
        }

        public SonogramConfig Configuration { get; set; }

        public double MaxAmplitude { get; set; }

        public int SampleRate { get; set; }

        public TimeSpan Duration { get; protected set; }

        // the following values are dependent on sampling rate.
        public int NyquistFrequency => this.SampleRate / 2;

        // Duration of full frame or window in seconds
        public double FrameDuration => this.Configuration.WindowSize / (double)this.SampleRate;

        // Duration of non-overlapped part of window/frame in seconds
        public double FrameStep => this.Configuration.GetFrameOffset(this.SampleRate);

        public double FBinWidth => this.NyquistFrequency / (double)this.Configuration.FreqBinCount;

        public double FramesPerSecond => 1 / this.FrameStep;

        public int FrameCount { get; protected set; }

        /// <summary>
        /// Gets or sets the spectrogram data matrix of doubles
        /// </summary>
        public double[,] Data { get; set; }
    }
}
