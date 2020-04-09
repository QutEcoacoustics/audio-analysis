// <copyright file="UnitConverters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using Acoustics.Shared;
    using AudioAnalysisTools.Events.Interfaces;
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
    }
}
