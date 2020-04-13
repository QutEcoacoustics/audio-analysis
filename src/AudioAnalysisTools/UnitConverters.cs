// <copyright file="UnitConverters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using Acoustics.Shared;
    using AudioAnalysisTools.Events.Interfaces;
    using AudioAnalysisTools.Scales;
    using SixLabors.ImageSharp;

    public class UnitConverters
    {
        public UnitConverters(double segmentStartOffset, double segmentDuration, double nyquistFrequency, int imageWidth, int imageHeight)
        {
            this.SegmentStartOffset = segmentStartOffset;
            this.TemporalScale = new LinearScale(
                (segmentStartOffset, segmentStartOffset + segmentDuration),
                (0, imageWidth));
            this.SpectralScale = new LinearScale(
                (0.0, nyquistFrequency),
                (imageHeight, 0.0)); // invert y-axis
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitConverters"/> class.
        /// Use this constructor or the next one when you have sample rate of recording from which spectrogram is derived.
        /// This constructor assumes that the step size equals the frame size.
        /// Enables calculations independent of the image size.
        /// </summary>
        /// <param name="segmentStartOffset">Start time in seconds of the current recording segment.</param>
        /// <param name="sampleRate">Sample rate of the recording segment.</param>
        /// <param name="frameSize">The window or frame size.</param>
        public UnitConverters(double segmentStartOffset, int sampleRate, int frameSize)
        {
            this.SegmentStartOffset = segmentStartOffset;
            this.SampleRate = sampleRate;
            this.FrameSize = frameSize;
            this.StepSize = frameSize;
            this.FrameOverlap = 0.0;
            this.NyquistFrequency = sampleRate / 2;
            int totalBinCount = frameSize / 2;
            this.HertzPerFreqBin = this.NyquistFrequency / totalBinCount;
            this.SecondsPerFrame = frameSize / (double)sampleRate;
            this.SecondsPerFrameStep = frameSize / (double)sampleRate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitConverters"/> class.
        /// Use this constructor or the next one when you have sample rate of recording from which spectrogram is derived.
        /// Enables calculations independent of the image size.
        /// </summary>
        /// <param name="segmentStartOffset">Start time in seconds of the current recording segment.</param>
        /// <param name="sampleRate">Sample rate of the recording segment.</param>
        /// <param name="frameSize">The window or frame size.</param>
        /// <param name="stepSize">THe step size which is LTE frame size.</param>
        public UnitConverters(double segmentStartOffset, int sampleRate, int frameSize, int stepSize)
            : this(segmentStartOffset, sampleRate, frameSize)
        {
            this.StepSize = stepSize;
            this.FrameOverlap = 1 - (stepSize / (double)frameSize);
            this.SecondsPerFrame = frameSize / (double)sampleRate;
            this.SecondsPerFrameStep = stepSize / (double)sampleRate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitConverters"/> class.
        /// Use this constructor or the above when you have sample rate of recording from which spectrogram is derived.
        /// Enables calculations independent of the image size.
        /// Supplied with the frame overlap rather than the step size.
        /// </summary>
        /// <param name="segmentStartOffset">Start time in seconds of the current recording segment.</param>
        /// <param name="sampleRate">Sample rate of the recording segment.</param>
        /// <param name="frameSize">The window or frame size.</param>
        /// <param name="frameOverlap">Fractional overlap of frames.</param>
        public UnitConverters(double segmentStartOffset, int sampleRate, int frameSize, double frameOverlap)
            : this(segmentStartOffset, sampleRate, frameSize)
        {
            this.FrameOverlap = frameOverlap;
            this.StepSize = (int)Math.Round(frameSize * (1 - frameOverlap));
            this.SecondsPerFrame = frameSize / (double)sampleRate;
            this.SecondsPerFrameStep = this.StepSize / (double)sampleRate;
        }

        public double SegmentStartOffset { get; }

        public int SampleRate { get; }

        public int FrameSize { get; }

        public int StepSize { get; }

        public double FrameOverlap { get; }

        public int NyquistFrequency { get; }

        public double SecondsPerFrame { get; }

        public double SecondsPerFrameStep { get; }

        public double HertzPerFreqBin { get; }

        /// <summary>
        /// Gets the temporal scale.
        /// </summary>
        /// <remarks>
        /// Measured in seconds per pixel.
        /// </remarks>
        public LinearScale TemporalScale { get; }

        /// <summary>
        /// Gets the temporal scale in second units.
        /// </summary>
        /// <remarks>
        /// Measured in seconds per spectrogram frame.
        /// </remarks>
        public LinearSecondsScale SecondsScale { get; }

        /// <summary>
        /// Gets the spectral scale.
        /// </summary>
        /// <remarks>
        /// Measured in hertz per pixel.
        /// </remarks>
        public LinearScale SpectralScale { get; }

        public RectangleF GetPixelRectangle(ISpectralEvent @event)
        {
            // todo: optimise
            return new RectangleF(
                    this.GetPoint(@event),
                    this.GetSize(@event));
        }

        public RectangleF GetPixelRectangle(ISpectralPoint point)
        {
            // todo: optimise
            return new RectangleF(
                (float)this.TemporalScale.To(point.Seconds.Minimum),
                (float)this.SpectralScale.To(point.Hertz.Minimum),
                (float)this.TemporalScale.ToDelta(point.Seconds.Size()),
                (float)this.SpectralScale.ToDelta(point.Hertz.Size()));
        }

        public PointF GetPoint(ISpectralEvent @event)
        {
            return new PointF(
                (float)this.TemporalScale.To(@event.EventStartSeconds),
                (float)this.SpectralScale.To(@event.LowFrequencyHertz));
        }

        public PointF GetPoint(ISpectralPoint point)
        {
            return new PointF(
                (float)this.TemporalScale.To(point.Seconds.Minimum),
                (float)this.SpectralScale.To(point.Hertz.Minimum));
        }

        public SizeF GetSize(ISpectralEvent @event)
        {
            var width = this.TemporalScale.ToDelta(@event.Duration);
            var height = this.SpectralScale.ToDelta(@event.BandWidth);
            return new SizeF((float)width, (float)height);
        }

        public float SecondsToPixels(double seconds) => (float)this.TemporalScale.To(seconds);

        public double PixelsToSeconds(float pixels) => this.TemporalScale.From(pixels);

        public float HertzToPixels(double seconds) => (float)this.SpectralScale.To(seconds);

        public double PixelsToHertz(float pixels) => this.SpectralScale.From(pixels);

        public double SegmentRelativeToRecordingRelative(double seconds) => seconds - this.SegmentStartOffset;

        public double RecordingRelativeToSegmentRelative(double seconds) => seconds + this.SegmentStartOffset;

        //public double SecondsDurationFromFrameCount(int frameCount)
        //{
        //    return this.SecondsScale.GetSecondsDurationFromFrameCount(frameCount);
        //}

        //public int FrameCountFromSecondsDuration(double secondsDuration)
        //{
        //    return this.SecondsScale.GetFrameCountFromSecondsDuration(secondsDuration);
        //}

        /// <summary>
        /// Returns the duration in seconds of the passed number of frames.
        /// NOTE: In the case where frames are overlapped, the last frame in any sequence is longer than the frame step.
        /// This correction becomes sgnificant when the frameCount is small.
        /// </summary>
        /// <param name="frameCount">The number of frames.</param>
        /// <returns>Duration inseconds.</returns>
        public double GetSecondsDurationFromFrameCount(int frameCount)
        {
            return ((frameCount - 1) * this.SecondsPerFrameStep) + this.SecondsPerFrame;
        }

        /// <summary>
        /// Returns the number of frames for the passed duration in seconds.
        /// TODO: Yet to be determined whether the exact frame count should be round, floor or celing.
        /// </summary>
        /// <param name="seconds">The elapsed time.</param>
        /// <returns>The number of frames.</returns>
        public int GetFrameCountFromSecondsDuration(double seconds)
        {
            int stepsMinusOne = (int)Math.Round((seconds - this.SecondsPerFrame) / this.SecondsPerFrameStep);
            return 1 + stepsMinusOne;
        }
    }
}
