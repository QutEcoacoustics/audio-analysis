// <copyright file="LinearTemporalScale.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Scales
{
    using System;

    /// <summary>
    /// This class converts between frames and time duration in seconds.
    /// A complication arises when the frames of a spectrogram are overlapped.
    /// </summary>
    public class LinearTemporalScale
    {
        public LinearTemporalScale(int sampleRate, int frameSize, int stepSize)
        {
            this.FrameDurationSeconds = frameSize / (double)sampleRate;
            this.FrameStepSeconds = stepSize / (double)sampleRate;
        }

        public double FrameDurationSeconds { get; }

        public double FrameStepSeconds { get; }

        public double GetSecondsDurationFromFrameCount(int frameCount)
        {
            return frameCount * this.FrameStepSeconds;
        }

        //public double FromDelta(double yDelta)
        //{
        //    var normalDelta = yDelta / this.rd;
        //    var d = normalDelta * this.dd;
        //    return this.clamp ? d.Clamp(this.d1, this.d2) : d;
        //}
    }
}
