// <copyright file="LinearSecondsScale.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Scales
{
    using System;

    /// <summary>
    /// This class converts between frames and time duration in seconds.
    /// A complication arises when the frames of a spectrogram are overlapped.
    /// </summary>
    public class LinearSecondsScale
    {
        public LinearSecondsScale(int sampleRate, int frameSize, int stepSize)
        {
            this.SecondsPerFrame = frameSize / (double)sampleRate;
            this.SecondsPerFrameStep = stepSize / (double)sampleRate;
        }

        public double SecondsPerFrame { get; }

        public double SecondsPerFrameStep { get; }
    }
}
