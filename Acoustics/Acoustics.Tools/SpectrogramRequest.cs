// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpectrogramRequest.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the SpectrogramRequest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Tools
{
    using System;

    using Acoustics.Shared;

    /// <summary>
    /// Spectrogram Request.
    /// </summary>
    public class SpectrogramRequest
    {
        /// <summary>
        /// Gets or sets start of cache request segment in milliseconds relative to start of audio reading.
        /// </summary>
        public TimeSpan Start { get; set; }

        /// <summary>
        /// Gets or sets end of cache request segment in milliseconds relative to start of audio reading.
        /// </summary>
        public TimeSpan End { get; set; }

        /// <summary>
        /// Gets or sets Width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets Height.
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets Pixels Per Millisecond.
        /// </summary>
        public double? PixelsPerMillisecond { get; set; }

        /// <summary>
        /// Gets a value indicating whether the Calculated Width is Available.
        /// </summary>
        public bool IsCalculatedWidthAvailable
        {
            get
            {
                return this.Width.HasValue || this.PixelsPerMillisecond.HasValue;
            }
        }

        /// <summary>
        /// Gets CalculatedWidth.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public int CalculatedWidth
        {
            get
            {
                if (this.Width.HasValue)
                {
                    return this.Width.Value;
                }

                if (this.PixelsPerMillisecond.HasValue)
                {
                    return Convert.ToInt32(this.PixelsPerMillisecond.Value * (this.End - this.Start).TotalMilliseconds);
                }

                throw new InvalidOperationException("Cannot calculate width: neither width nor start and end were given.");
            }
        }

        /// <summary>
        /// Validate this Spectrogram Request.
        /// </summary>
        /// <returns>
        /// True if valid, otherwise false.
        /// </returns>
        public bool Validate()
        {
            if (this.Width.HasValue && this.Width < 1)
            {
                return false;
            }

            if (this.Height.HasValue && this.Height < 1)
            {
                return false;
            }

            if (this.PixelsPerMillisecond.HasValue && this.PixelsPerMillisecond <= 0)
            {
                return false;
            }

            if (this.PixelsPerMillisecond.HasValue && this.Width.HasValue)
            {
                var segmentDuration = (this.End - this.Start).TotalMilliseconds;
                double calculatedPpms = (double)this.Width.Value / segmentDuration;
                double calculatedWidth = (double)this.PixelsPerMillisecond.Value * segmentDuration;

                if (calculatedPpms != this.PixelsPerMillisecond.Value || calculatedWidth != this.Width.Value)
                {
                    return false;
                }
            }

            return this.Start != TimeSpan.MaxValue
                && this.Start != TimeSpan.MinValue
                && this.Start >= TimeSpan.Zero
                && this.End != TimeSpan.MaxValue
                && this.End != TimeSpan.MinValue
                && this.End > TimeSpan.Zero
                && this.Start < this.End;
        }

        /// <summary>
        /// Get a string representation of this spectrogram request.
        /// </summary>
        /// <returns>
        /// String representation.
        /// </returns>
        public override string ToString()
        {
            var duration = (this.End - this.Start).Duration();

            var segment = string.Format(
                "Request starts at {0} ({1}ms) and finishes at {2} ({3}ms) ({4} - {5}ms).",
                this.Start.Humanise(),
                this.Start.TotalMilliseconds,
                this.End.Humanise(),
                this.End.TotalMilliseconds,
                duration.Humanise(),
                duration.TotalMilliseconds);

            var width = this.Width.HasValue ? " Width of " + this.Width.Value + "px." : string.Empty;

            var height = this.Height.HasValue
                                 ? " Height of " + this.Height.Value + "px."
                                 : string.Empty;

            var ppms = this.PixelsPerMillisecond.HasValue ? " Pixels per millisecond of " + this.PixelsPerMillisecond.Value + "." : string.Empty;

            return segment + width + height + ppms;
        }
    }
}
