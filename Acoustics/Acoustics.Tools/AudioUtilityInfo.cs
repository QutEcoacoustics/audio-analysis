namespace Acoustics.Tools
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Audio file Info from Utility.
    /// </summary>
    public class AudioUtilityInfo
    {
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
        /// 
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
    }
}
