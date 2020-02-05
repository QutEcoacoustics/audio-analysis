// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AudioReadingRequest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the AudioReadingRequest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Tools
{
    using System;

    using Shared;

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

            var segment =
                $"Request starts at {this.Start.Humanise()} ({this.Start.TotalMilliseconds}ms) and finishes at {this.End.Humanise()} ({this.End.TotalMilliseconds}ms) ({duration.Humanise()} - {duration.TotalMilliseconds}ms).";

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
