﻿namespace Acoustics.Tools
{
    using System;
    using System.Globalization;
    using System.Linq;

    using Acoustics.Shared;
    using Acoustics.Tools.Audio;

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
        public int? TargetSampleRate { get; set; }

        /// <summary>
        /// Gets or sets the target channel numbers (eg. 1,2,3,{1,2},{1,2,3,4} ... ).
        /// Channels are 1-indexed!
        /// </summary>
        public int[] Channels { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to mix down to mono.
        /// </summary>
        public bool? MixDownToMono { get; set; }

        /// <summary>
        /// Gets or sets the bandpass low.
        /// </summary>
        public double? BandpassLow { get; set; }

        /// <summary>
        /// Gets or sets the bandpass high.
        /// </summary>
        public double? BandpassHigh { get; set; }

        /// <summary>
        /// Gets or sets the band pass type.
        /// </summary>
        public BandPassType BandPassType { get; set; }

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
                    var msg =
                        $"Start ({this.OffsetStart.Value.TotalMilliseconds}) must be equal or less than End ({this.OffsetEnd.Value.TotalMilliseconds}).";

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

            if (!this.ValidateBandPass(throwExceptions))
            {
                return false;
            }

            if (this.TargetSampleRate.HasValue && this.TargetSampleRate < 1)
            {
                if (throwExceptions)
                {
                    throw new ArgumentOutOfRangeException("SampleRate", "Sample rate should be greater than 0.");
                }

                return false;
            }

            if (this.Channels.NotNull() && (this.Channels.Length == 0 || this.Channels.Any(c => c < 1)))
            {
                if (throwExceptions)
                {
                    throw new ChannelNotAvailableException("Channel number should be greater than 0.");
                }

                return false;
            }

            /*if (this.Channel.HasValue && this.MixDownToMono.HasValue && this.MixDownToMono.Value)
            {
                // can't mix down to mono and select a channel
                if (throwExceptions)
                {
                    throw new ArgumentOutOfRangeException("Channel", "Channel number should not be set if mix down to mono is true.");
                }

                return false;
            }*/

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
                var start = this.OffsetStart ?? TimeSpan.Zero;
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
                this.OffsetStart?.TotalMilliseconds ?? 0,
                this.OffsetEnd.HasValue ? this.OffsetEnd.Value.Humanise() : "end",
                this.OffsetEnd.HasValue ? this.OffsetEnd.Value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture) + "ms" : "unknown",
                duration);

            string channels = string.Empty;
            if (this.Channels.NotNull())
            {
                channels = " Using channel numbers " + string.Join(", ", this.Channels) + ".";
            }


            var sampleRate = this.TargetSampleRate.HasValue
                                 ? " Sample rate of " + this.TargetSampleRate.Value + " hertz."
                                 : string.Empty;

            var mixDown = this.MixDownToMono.HasValue && this.MixDownToMono.Value ? "Mix channels down to mono." : string.Empty;

            var channel = this.Channels.NotNull() ? "Extract channels " + string.Join(", ", this.Channels) + "." : string.Empty;

            return segment + channels + sampleRate + mixDown + channel;
        }

        private bool ValidateBandPass(bool throwExceptions)
        {
            if (this.BandpassLow.HasValue && this.BandpassLow.Value < 0)
            {
                if (throwExceptions)
                {
                    throw new ArgumentOutOfRangeException("BandPassLow", "BandPassLow must be equal or greater than zero.");
                }

                return false;
            }

            if (this.BandpassHigh.HasValue && this.BandpassHigh.Value <= 0)
            {
                if (throwExceptions)
                {
                    throw new ArgumentOutOfRangeException("BandPassHigh", "End must be greater than zero.");
                }

                return false;
            }


            if (this.BandpassLow.HasValue || this.BandpassHigh.HasValue)
            {
                if (this.BandPassType == BandPassType.None)
                {
                    if (throwExceptions)
                    {
                        throw new ArgumentException("Bandpass type should be set if BandpassHigh or BandpassLow is set", "BandPassType");
                    }

                    return false;
                }

                if (!this.TargetSampleRate.HasValue)
                {
                    if (throwExceptions)
                    {
                        throw new ArgumentException("Bandpass requires a sample rate to be set", "SampleRate");
                    }

                    return false;
                }
            }

            if (this.BandpassLow.HasValue && this.BandpassHigh.HasValue)
            {

                if (this.BandpassLow.Value > this.BandpassHigh.Value)
                {
                    var msg = string.Format(
                        "Start ({0}) must be equal or less than End ({1}).",
                        this.BandpassLow.Value,
                        this.BandpassHigh.Value);

                    if (throwExceptions)
                    {
                        throw new ArgumentOutOfRangeException("BandPassLow", msg);
                    }

                    return false;
                }

                if (this.BandpassLow.Value == this.BandpassHigh.Value)
                {
                    if (throwExceptions)
                    {
                        throw new ArgumentOutOfRangeException("BandPassHigh", "Start and end should not be equal.");
                    }

                    return false;
                }
            }

            return true;
        }
    }



    public enum BandPassType
    {
        None = 0,

        /// <summary>
        /// Sinc kaiser-windowed low / high / band pass filter.
        /// Very high attenuation. Steep shoulders.
        /// </summary>
        Sinc = 2,

        /// <summary>
        /// Two-pole butterworth band-pass, dropping off at 3dB per octave.
        /// </summary>
        Bandpass = 1
    }
}
