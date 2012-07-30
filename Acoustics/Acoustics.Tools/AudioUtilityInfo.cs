namespace Acoustics.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Linq;

    /// <summary>
    /// Audio file Info from Utility.
    /// </summary>
    public class AudioUtilityInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioUtilityInfo"/> class.
        /// </summary>
        public AudioUtilityInfo()
        {
            this.RawData = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets the Sample Rate in hertz.
        /// stream:sample_rate.
        /// </summary>
        public int? SampleRate { get; set; }

        /// <summary>
        /// Gets or sets the bits per second (bits/second).
        /// format:bit_rate.
        /// </summary>
        public int? BitsPerSecond { get; set; }

        /// <summary>
        /// Gets or sets the bits per sample.
        /// </summary>
        public int? BitsPerSample { get; set; }

        /// <summary>
        /// Gets or sets the number of channels.
        /// stream:channels.
        /// </summary>
        public int? ChannelCount { get; set; }

        /// <summary>
        /// Gets or sets the duration.
        /// format:duration.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// Gets or sets the raw data.
        /// </summary>
        public Dictionary<string, string> RawData { get; set; }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The System.String.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            if (this.SampleRate.HasValue)
            {
                sb.Append("SampleRate: " + this.SampleRate.Value);
            }

            if (this.BitsPerSecond.HasValue)
            {
                sb.Append("BitsPerSecond: " + this.BitsPerSecond.Value);
            }

            if (this.ChannelCount.HasValue)
            {
                sb.Append("ChannelCount: " + this.ChannelCount.Value);
            }

            if (this.Duration.HasValue)
            {
                sb.Append("Duration: " + this.Duration.Value);
            }

            if (this.RawData != null && this.RawData.Count > 0)
            {
                sb.Append("RawData: " + string.Join(", ", this.RawData.Select(i => "[" + i.Key + ": " + i.Value + "]")));
            }

            return sb.ToString();
        }
    }
}
