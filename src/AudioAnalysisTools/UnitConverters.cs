// <copyright file="UnitConverters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using Acoustics.Shared;
    using AudioAnalysisTools.Events.Interfaces;
    using AudioAnalysisTools.Scales;
    using SixLabors.ImageSharp;
    using System;

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

            this.TimeScale = new LinearTemporalScale(sampleRate, frameSize, frameSize);
            this.SpectralScale = new LinearScale((0.0, this.NyquistFrequency), (totalBinCount, 0.0)); // invert y-axis
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
            int totalBinCount = frameSize / 2;
            this.TimeScale = new LinearTemporalScale(sampleRate, frameSize, stepSize);
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
            int totalBinCount = frameSize / 2;
            this.TimeScale = new LinearTemporalScale(sampleRate, frameSize, this.StepSize);
        }

        public double SegmentStartOffset { get; }

        public int SampleRate { get; }

        public int FrameSize { get; }

        public int StepSize { get; }

        public double FrameOverlap { get; }

        public int NyquistFrequency { get; }

        /// <summary>
        /// Gets the temporal scale.
        /// </summary>
        /// <remarks>
        /// Measured in seconds per pixel.
        /// </remarks>
        public LinearScale TemporalScale { get; }

        /// <summary>
        /// Gets the temporal scale.
        /// </summary>
        /// <remarks>
        /// Measured in seconds per pixel.
        /// </remarks>
        public LinearTemporalScale TimeScale { get; }

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

        public double LinearScale_SecondsDurationFromFrameCount(int frameCount)
        {
            return this.TimeScale.GetSecondsDurationFromFrameCount(frameCount);
        }
    }
}
