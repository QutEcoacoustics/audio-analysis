// <copyright file="UnitConverters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Interfaces;
    using AudioAnalysisTools.Scales;
    using SixLabors.ImageSharp;
    using TowseyLibrary;

    public class UnitConverters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnitConverters"/> class.
        /// IMPORTANT NOTE: segmentDuration should be the duration spanned by the spectrogram image, not the actual duration recording.
        ///                 Given one frame per pixel column, the spectrogram duration = frameCount * seconds/frame.
        /// </summary>
        /// <param name="segmentStartOffset">Segment start relative to start of the recording.</param>
        /// <param name="segmentDuration">Set the time-scale. The spectrogram time-span. Typically 60 seconds.</param>
        /// <param name="nyquistFrequency">Sets the frequency scale.</param>
        /// <param name="imageWidth">Pixel width = number of time frames.</param>
        /// <param name="imageHeight">Pixel height = the number of frequency bins.</param>
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
        /// Gets the temporal scale. Measured in seconds per pixel.
        /// <c>To</c> converts seconds to pixels.
        /// <c>From</c> converts pixels to seconds.
        /// </summary>
        public LinearScale TemporalScale { get; }

        /// <summary>
        /// Gets the spectral scale.
        /// </summary>
        /// <remarks>
        /// Measured in hertz per pixel.
        /// </remarks>
        public LinearScale SpectralScale { get; }

        /// <summary>
        /// Gets a rectangle suitable for drawing.
        /// </summary>
        /// <remarks>
        /// Top and left are floored to pixel boundaries.
        /// Width and height are rounded up.
        /// **No border pixels are substracted from width or height!**.
        /// </remarks>
        /// <param name="event">The event to get the border for.</param>
        /// <returns>The rectangle representing the border.</returns>
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
                (float)this.SpectralScale.To(point.Hertz.Maximum),
                (float)this.TemporalScale.ToMagnitude(point.Seconds.Size()),
                (float)this.SpectralScale.ToMagnitude(point.Hertz.Size()));
        }

        /// <summary>
        /// Gets the top and left of an event, in a fashion suitable for drawing.
        /// </summary>
        /// <remarks>
        /// Top and left are floored to pixel boundaries.
        /// </remarks>
        /// <param name="event">The event to get the point for.</param>
        /// <returns>The point.</returns>
        public PointF GetPoint(ISpectralEvent @event)
        {
            var raw = new PointF(
                (float)this.TemporalScale.To(@event.EventStartSeconds),
                (float)this.SpectralScale.To(@event.HighFrequencyHertz));

            return Point.Truncate(raw);
        }

        public PointF GetPoint(ISpectralPoint point)
        {
            // TODO: this should probably be rounded
            // and rounding operation should be _round_ rather than truncate or ceiling
            // because we want the point to be in the "center" of the point even if an
            // image's dimensions change.

            return new PointF(
                (float)this.TemporalScale.To(point.Seconds.Minimum),
                (float)this.SpectralScale.To(point.Hertz.Maximum));
        }

        public PointF GetPointCentroid(ISpectralPoint point)
        {
            var centerX = point.Seconds.Center();
            var centerY = point.Hertz.Center();

            return new PointF(
                (float)this.TemporalScale.To(centerX),
                (float)this.SpectralScale.To(centerY));
        }

        /// <summary>
        /// Gets the width and height of an event.
        /// </summary>
        /// <remarks>
        /// Width and height are rounded up.
        /// </remarks>
        /// <param name="event">The event to get the size for.</param>
        /// <returns>The size.</returns>
        public SizeF GetSize(ISpectralEvent @event)
        {
            var width = this.TemporalScale.ToMagnitude(@event.EventDurationSeconds);
            var height = this.SpectralScale.ToMagnitude(@event.BandWidthHertz);
            var raw = new SizeF((float)width, (float)height);
            var rounded = Size.Round(raw);

            return rounded;
        }

        public float SecondsToPixels(double seconds) => (float)this.TemporalScale.To(seconds);

        public double PixelsToSeconds(float pixels) => this.TemporalScale.From(pixels);

        public float HertzToPixels(double seconds) => (float)this.SpectralScale.To(seconds);

        public double PixelsToHertz(float pixels) => this.SpectralScale.From(pixels);

        public double SegmentRelativeToRecordingRelative(double seconds) => seconds - this.SegmentStartOffset;

        public double RecordingRelativeToSegmentRelative(double seconds) => seconds + this.SegmentStartOffset;

        public int FrameFromStartTime(double startTime)
        {
            return (int)Math.Round((startTime - this.SegmentStartOffset) / this.SecondsPerFrameStep, MidpointRounding.AwayFromZero);
        }

        public int FrameFromEndTime(double endTime)
        {
            return (int)Math.Round((endTime - this.SegmentStartOffset - this.SecondsPerFrame) / this.SecondsPerFrameStep, MidpointRounding.AwayFromZero);
        }

        public double GetStartTimeInSecondsOfFrame(int frameId)
        {
            return this.SegmentStartOffset + (frameId * this.SecondsPerFrameStep);
        }

        public double GetEndTimeInSecondsOfFrame(int frameId)
        {
            return this.GetStartTimeInSecondsOfFrame(frameId) + this.SecondsPerFrame;
        }

        /// <summary>
        /// Returns the duration in seconds of the passed number of frames.
        /// NOTE: In the case where frames are overlapped, the last frame in any sequence is longer than the frame step.
        /// This correction becomes sgnificant when the frameCount is small.
        /// </summary>
        /// <param name="frameCount">The number of frames.</param>
        /// <returns>Duration inseconds.</returns>
        public double GetSecondsDurationFromFrameCount(int frameCount)
        {
            double overstep = this.SecondsPerFrame - this.SecondsPerFrameStep;
            return (frameCount * this.SecondsPerFrameStep) + overstep;
        }

        /// <summary>
        /// Returns the number of frames for the passed duration in seconds.
        /// Do the calculations in signal samples.
        /// TODO: Question should we do round or floor?.
        /// </summary>
        /// <param name="seconds">The elapsed time.</param>
        /// <returns>The number of frames.</returns>
        public int GetFrameCountFromSecondsDuration(double seconds)
        {
            int overstep = this.FrameSize - this.StepSize;
            double totalSamples = seconds * this.SampleRate;
            double balance = totalSamples - overstep;
            double frames = balance / this.StepSize;
            return (int)Math.Floor(frames);
        }

        public int GetFreqBinFromHertz(double hertz)
        {
            return (int)Math.Round(hertz / this.HertzPerFreqBin);
        }

        public double GetHertzFromFreqBin(int bin)
        {
            return bin * this.HertzPerFreqBin;
        }

        public double GetHertzHighFromFreqBin(int bin)
        {
            return (bin * this.HertzPerFreqBin) + this.HertzPerFreqBin;
        }

        public SpectralPoint ConvertPointToSpectralPoint(Point point, double value)
        {
            return new SpectralPoint(
                (this.GetStartTimeInSecondsOfFrame(point.X), this.GetEndTimeInSecondsOfFrame(point.X)),
                (this.GetHertzFromFreqBin(point.Y), this.GetHertzHighFromFreqBin(point.Y)),
                value);
        }

        public void SetBounds<T>(T @event, Oblong source)
            where T : SpectralEvent
        {
            @event.EventStartSeconds = this.GetStartTimeInSecondsOfFrame(source.RowTop);
            @event.EventEndSeconds = this.GetEndTimeInSecondsOfFrame(source.RowBottom);
            @event.LowFrequencyHertz = this.GetHertzFromFreqBin(source.ColumnLeft);
            @event.HighFrequencyHertz = this.GetHertzFromFreqBin(source.ColumnRight);
        }
    }
}
