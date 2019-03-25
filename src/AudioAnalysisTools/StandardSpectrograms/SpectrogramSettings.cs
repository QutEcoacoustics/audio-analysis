// <copyright file="SpectrogramSettings.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using DSP;
    using TowseyLibrary;

    public class SpectrogramSettings
    {
        /// <summary>
        /// Gets or sets SourceFileName
        /// Although this is not a setting, we need to store it right at the beginning.
        /// </summary>
        public string SourceFileName { get; set; }

        public int WindowSize { get; set; } = 512;

        public double WindowOverlap { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets exact frame step in samples - an alternative to overlap
        /// Note that the default setting should be same as WindowSize i.e. no overlap.
        /// </summary>
        public int WindowStep { get; set; } = 512;

        public string WindowFunction { get; set; } = WindowFunctions.HAMMING.ToString();

        public int SmoothingWindow { get; set; } = 3;

        public bool DoMelScale { get; set; } = false;

        /// <summary>
        /// Gets or sets MelBinCount
        /// This is used only if DoMelScale = true.
        /// </summary>
        public int MelBinCount { get; set; } = 256;

        public NoiseReductionType NoiseReductionType { get; set; } = NoiseReductionType.None;

        public double NoiseReductionParameter { get; set; } = 0.0;
    }

    public class SpectrogramAttributes
    {
        public int SampleRate { get; set; }

        public double MaxAmplitude { get; set; }

        public int NyquistFrequency { get; set; }

        public TimeSpan Duration { get; set; }

        public int FrameCount { get; set; }

        /// <summary>
        /// Gets or sets duration of full frame or window in seconds
        /// </summary>
        public TimeSpan FrameDuration { get; set; }

        public double FramesPerSecond { get; set; } //= 1 / this.FrameStep;

        public double FBinWidth { get; set; }

        //this.FBinWidth = this.NyquistFrequency / (double) this.FreqBinCount;
        public double Epsilon { get; set; }

        public double WindowPower { get; set; }

        /// <summary>
        /// returns the duration of that part of frame not overlapped with following frame.
        /// Duration is given in seconds.
        /// Assumes window size and overlap fraction already known.
        /// </summary>
        public static TimeSpan GetFrameOffset(int windowSize, double windowOverlap, int sampleRate)
        {
            int step = DSP_Frames.FrameStep(windowSize, windowOverlap);
            return TimeSpan.FromSeconds(step / (double)sampleRate);
        }
    }
}
