namespace Acoustics.Tools.Audio
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Interface for manipulating audio.
    /// </summary>
    public interface IAudioUtility
    {
        /// <summary>
        /// Segment a <paramref name="source"/> audio file.
        /// <paramref name="output"/> file will be created.
        /// </summary>
        /// <param name="source">
        /// The <paramref name="source"/> audio file.
        /// </param>
        /// <param name="sourceMediaType">
        /// The <paramref name="source"/> Mime Type.
        /// </param>
        /// <param name="output">
        /// The <paramref name="output"/> audio file. Ensure the file does not exist.
        /// </param>
        /// <param name="outputMediaType">
        /// The <paramref name="output"/> Mime Type.
        /// </param>
        /// <param name="request">
        /// The segment <paramref name="request"/>.
        /// </param>
        void Modify(FileInfo source, string sourceMediaType, FileInfo output, string outputMediaType, AudioUtilityRequest request);

        /// <summary>
        /// Get meta data for the given file.
        /// </summary>
        /// <param name="source">File to get meta data from. This should be an audio file.</param>
        /// <returns>A dictionary containing meta data for the given file.</returns>
        AudioUtilityInfo Info(FileInfo source);
    }
}
