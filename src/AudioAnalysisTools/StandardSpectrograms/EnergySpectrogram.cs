// <copyright file="EnergySpectrogram.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using Acoustics.Tools.Wav;
    using TowseyLibrary;

    /// <summary>
    /// There are three CONSTRUCTORS
    /// </summary>
    public class EnergySpectrogram
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnergySpectrogram"/> class.
        /// Use this constructor when you have two paths for config file and audio file
        /// </summary>
        public EnergySpectrogram(string configFile, string audioFile)
            : this(SonogramConfig.Load(configFile), new WavReader(audioFile))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnergySpectrogram"/> class.
        /// Use this constructor when you have config and audio objects
        /// It creates an amplitude spectrogram
        /// </summary>
        public EnergySpectrogram(SonogramConfig config, WavReader wav)
            : this(new AmplSpectrogram(config, wav))
        {
        }

        public EnergySpectrogram(AmplSpectrogram amplitudeSpectrogram)
        {
            this.Configuration = amplitudeSpectrogram.Configuration;
            this.Duration = amplitudeSpectrogram.Duration;
            this.SampleRate = amplitudeSpectrogram.SampleRate;

            this.Duration = amplitudeSpectrogram.Duration;
            this.SampleRate = amplitudeSpectrogram.SampleRate;
            this.NyquistFrequency = amplitudeSpectrogram.SampleRate / 2;
            this.MaxAmplitude = amplitudeSpectrogram.MaxAmplitude;
            this.FrameCount = amplitudeSpectrogram.FrameCount;
            this.FrameDuration = amplitudeSpectrogram.FrameDuration;
            this.FrameStep = amplitudeSpectrogram.FrameStep;
            this.FBinWidth = amplitudeSpectrogram.FBinWidth;
            this.FramesPerSecond = amplitudeSpectrogram.FramesPerSecond;

            // CONVERT AMPLITUDES TO ENERGY
            //this.Data = PowerSpectralDensity.GetEnergyValues(amplitudeSpectrogram.Data);
            this.Data = MatrixTools.SquareValues(amplitudeSpectrogram.Data);
        }

        public SonogramConfig Configuration { get; set; }

        /// <summary>
        /// Gets or sets the spectrogram data matrix of doubles
        /// Note matrix orientation: ROWS = spectra;  COLUMNS = frequency bins
        /// </summary>
        public double[,] Data { get; set; }

        public int SampleRate { get; set; }

        public TimeSpan Duration { get; protected set; }

        // the following values are dependent on sampling rate.
        public int NyquistFrequency { get; set; }

        public double MaxAmplitude { get; set; }

        // Duration of full frame or window in seconds
        public double FrameDuration { get; protected set; }

        // Duration of non-overlapped part of window/frame in seconds
        public double FrameStep { get; protected set; }

        public double FBinWidth { get; protected set; }

        public double FramesPerSecond { get; protected set; }

        public int FrameCount { get; protected set; }
    }
}
