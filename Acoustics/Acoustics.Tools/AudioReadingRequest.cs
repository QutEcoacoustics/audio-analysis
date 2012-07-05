// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AudioReadingRequest.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the AudioReadingRequest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Tools
{
    using System;

    using Acoustics.Shared;

    /// <summary>
    /// Audio Reading Request.
    /// </summary>
    public class AudioReadingRequest
    {
        /// <summary>
        /// Gets or sets the start time relative to the start of the audio reading.
        /// </summary>
        public TimeSpan Start { get; set; }

        /// <summary>
        /// Gets or sets the end time relative to the start of the audio reading.
        /// </summary>
        public TimeSpan End { get; set; }

        /// <summary>
        /// Gets or sets the target media type.
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets the target Sample Rate.
        /// </summary>
        public int? SampleRate { get; set; }

        /// <summary>
        /// Gets or sets the target number of Channels.
        /// </summary>
        public int? Channels { get; set; }

        /// <summary>
        /// Validate this Audio Reading Request.
        /// </summary>
        /// <returns>
        /// True if valid, otherwise false.
        /// </returns>
        public bool Validate()
        {
            if (!string.IsNullOrWhiteSpace(this.MediaType) && !MediaTypes.IsMediaTypeRecognised(this.MediaType))
            {
                return false;
            }

            if (this.SampleRate.HasValue && this.SampleRate < 1)
            {
                return false;
            }

            if (this.Channels.HasValue && this.Channels < 1)
            {
                return false;
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
        /// Get a string representation of this audio reading request.
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

            var mediaType = string.IsNullOrWhiteSpace(this.MediaType)
                                ? string.Empty
                                : " Media type of " + MediaTypes.CanonicaliseMediaType(this.MediaType) + ".";

            var channels = this.Channels.HasValue ? " " + this.Channels.Value + " channel(s)." : string.Empty;
            var sampleRate = this.SampleRate.HasValue
                                 ? " Sample rate of " + this.SampleRate.Value + " hertz."
                                 : string.Empty;

            return segment + mediaType + channels + sampleRate;
        }
    }
}
