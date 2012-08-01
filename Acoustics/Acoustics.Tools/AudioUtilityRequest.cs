namespace Acoustics.Tools
{
    using System;
    using System.Globalization;

    using Acoustics.Shared;

    /// <summary>
    /// Audio Utility request.
    /// </summary>
    public class AudioUtilityRequest
    {
        /// <summary>
        /// Gets or sets the offset start.
        /// </summary>
        public TimeSpan? OffsetStart { get; set; }

        /// <summary>
        /// Gets or sets the offset end.
        /// </summary>
        public TimeSpan? OffsetEnd { get; set; }

        /// <summary>
        /// Gets or sets the target Sample Rate in hertz.
        /// </summary>
        public int? SampleRate { get; set; }

        /// <summary>
        /// Gets or sets the target channel number (eg. 1,2,3 ... ).
        /// </summary>
        public int? Channel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to mix down to mono.
        /// </summary>
        public bool? MixDownToMono { get; set; }

        /// <summary>
        /// Validate this Audio Reading Request.
        /// </summary>
        /// <returns>
        /// True if valid, otherwise false.
        /// </returns>
        public bool Validate()
        {
            return this.DoValidation(false);
        }

        /// <summary>
        /// The validate checked. Throws exception if validation fails.
        /// </summary>
        /// <returns>
        /// True if valid, otherwise false.
        /// </returns>
        public bool ValidateChecked()
        {
            return this.DoValidation(true);
        }

        private bool DoValidation(bool throwExceptions)
        {
            if (this.OffsetStart.HasValue && this.OffsetStart.Value < TimeSpan.Zero)
            {
                if (throwExceptions)
                {
                    throw new ArgumentOutOfRangeException("OffsetStart", "Start must be equal or greater than zero.");
                }

                return false;
            }

            if (this.OffsetStart.HasValue && (this.OffsetStart.Value == TimeSpan.MaxValue || this.OffsetStart.Value == TimeSpan.MinValue))
            {
                if (throwExceptions)
                {
                    throw new ArgumentOutOfRangeException("OffsetStart", "Start must be a valid value.");
                }

                return false;
            }

            if (this.OffsetEnd.HasValue && this.OffsetEnd.Value <= TimeSpan.Zero)
            {
                if (throwExceptions)
                {
                    throw new ArgumentOutOfRangeException("OffsetEnd", "End must be greater than zero.");
                }

                return false;
            }

            if (this.OffsetEnd.HasValue && (this.OffsetEnd.Value == TimeSpan.MaxValue || this.OffsetEnd.Value == TimeSpan.MinValue))
            {
                if (throwExceptions)
                {
                    throw new ArgumentOutOfRangeException("OffsetEnd", "Start must be a valid value.");
                }

                return false;
            }

            if (this.OffsetStart.HasValue && this.OffsetEnd.HasValue)
            {
                if (this.OffsetStart.Value > this.OffsetEnd.Value)
                {
                    var msg = string.Format(
                        "Start ({0}) must be equal or less than End ({1}).",
                        this.OffsetStart.Value.TotalMilliseconds,
                        this.OffsetEnd.Value.TotalMilliseconds);

                    if (throwExceptions)
                    {
                        throw new ArgumentOutOfRangeException("OffsetStart", msg);
                    }

                    return false;
                }

                if (this.OffsetStart.Value == this.OffsetEnd.Value)
                {
                    if (throwExceptions)
                    {
                        throw new ArgumentOutOfRangeException("OffsetStart", "Start and end should not be equal.");
                    }

                    return false;
                }
            }

            if (this.SampleRate.HasValue && this.SampleRate < 1)
            {
                if (throwExceptions)
                {
                    throw new ArgumentOutOfRangeException("SampleRate", "Sample rate should be greater than 0.");
                }

                return false;
            }

            if (this.Channel.HasValue && this.Channel < 1)
            {
                if (throwExceptions)
                {
                    throw new ArgumentOutOfRangeException("Channel", "Channel number should be greater than 0.");
                }

                return false;
            }

            if (this.Channel.HasValue && this.MixDownToMono.HasValue && this.MixDownToMono.Value)
            {
                // can't mix down to mono and select a channel
                if (throwExceptions)
                {
                    throw new ArgumentOutOfRangeException("Channel", "Channel number should not be set if mix down to mono is true.");
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Get a string representation of this audio reading request.
        /// </summary>
        /// <returns>
        /// String representation.
        /// </returns>
        public override string ToString()
        {
            string duration;
            if (this.OffsetEnd.HasValue)
            {
                var start = this.OffsetStart.HasValue ? this.OffsetStart.Value : TimeSpan.Zero;
                var end = this.OffsetEnd.Value;

                var durationts = (end - start).Duration();
                duration = durationts.Humanise() + " " + durationts.TotalMilliseconds + "ms";
            }
            else
            {
                duration = "unknown";
            }

            var segment = string.Format(
                "Request starts at {0} ({1}ms) and finishes at {2} ({3}) ({4}).",
                this.OffsetStart.HasValue ? this.OffsetStart.Value.Humanise() : "beginning",
                this.OffsetStart.HasValue ? this.OffsetStart.Value.TotalMilliseconds : 0,
                this.OffsetEnd.HasValue ? this.OffsetEnd.Value.Humanise() : "end",
                this.OffsetEnd.HasValue ? this.OffsetEnd.Value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture) + "ms" : "unknown",
                duration);

            var channels = this.Channel.HasValue ? " Using channel number " + this.Channel.Value + "." : string.Empty;

            var sampleRate = this.SampleRate.HasValue
                                 ? " Sample rate of " + this.SampleRate.Value + " hertz."
                                 : string.Empty;

            var mixDown = this.MixDownToMono.HasValue && this.MixDownToMono.Value ? "Mix channels down to mono." : string.Empty;

            var channel = this.Channel.HasValue ? "Extract channel " + this.Channel.Value + "." : string.Empty;

            return segment + channels + sampleRate + mixDown + channel;
        }
    }
}
