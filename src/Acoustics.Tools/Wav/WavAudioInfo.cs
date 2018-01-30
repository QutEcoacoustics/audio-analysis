namespace Acoustics.Tools.Wav
{
    using System;

    /// <summary>
    /// The wav audio info.
    /// </summary>
    public class WavAudioInfo
    {
        /// <summary>
        /// Gets or sets   Number of channels.
        /// </summary>
        public short Channels { get; set; }

        /// <summary>
        /// Gets or sets Sample rate of audio (number of samples per second).
        /// </summary>
        public int SampleRate { get; set; }

        /// <summary>
        /// Gets or sets   Bits per sample.
        /// </summary>
        public short BitsPerSample { get; set; }

        /// <summary>
        /// Gets or sets   Bytes Per Sample / Block Align.
        /// </summary>
        public short BytesPerSample { get; set; }

        /// <summary>
        ///  Gets or sets  Compression code (1 for uncompressed PCM audio).
        /// </summary>
        public short CompressionCode { get; set; }

        /// <summary>
        ///  Gets or sets  Bytes per second.
        /// </summary>
        public int BytesPerSecond { get; set; }

        /// <summary>
        ///  Gets or sets  Total duration of audio.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        ///  Gets Epsilon.
        /// </summary>
        public double Epsilon
        {
            get { return Math.Pow(0.5, this.BitsPerSample - 1); }
        }

        /// <summary>
        ///  Gets or sets Number of frames (samples).
        /// </summary>
        public long Frames { get; set; }
    }
}
