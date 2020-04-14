// <copyright file="UnitConverters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using Acoustics.Shared;
    using AudioAnalysisTools.Events.Interfaces;
    using AudioAnalysisTools.Scales;
    using SixLabors.ImageSharp;

    public class UnitConverters
    {
        public UnitConverters(
            double segmentStartOffset, double segmentDuration, double nyquistFrequency, int imageWidth, int imageHeight)
        {
            this.SegmentStartOffset = segmentStartOffset;
            this.TemporalScale = new LinearScale(
                (segmentStartOffset, segmentStartOffset + segmentDuration),
                (0, imageWidth));
            this.SpectralScale = new LinearScale(
                (0.0, nyquistFrequency),
                (imageHeight, 0.0)); // invert y-axis
        }

        public double SegmentStartOffset { get; }

        /// <summary>
        /// Gets the temporal scale.
        /// </summary>
        /// <remarks>
        /// Measured in seconds per pixel.
        /// </remarks>
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
        /// <param name="@event">The event to get the border for.</param>
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
    }
}
