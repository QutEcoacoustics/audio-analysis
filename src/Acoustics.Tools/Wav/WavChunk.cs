// <copyright file="WavChunk.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Tools.Wav
{
    /// <summary>
    /// Wav chunk.
    /// </summary>
    public class WavChunk
    {
        /// <summary>
        /// Gets or sets Position.
        /// </summary>
        public long Position { get; set; }

        /// <summary>
        /// Gets or sets Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Length.
        /// </summary>
        public long Length { get; set; }
    }
}