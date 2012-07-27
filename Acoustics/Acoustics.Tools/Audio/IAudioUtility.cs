﻿namespace Acoustics.Tools.Audio
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
        /// <param name="sourceMimeType">
        /// The <paramref name="source"/> Mime Type.
        /// </param>
        /// <param name="output">
        /// The <paramref name="output"/> audio file. Ensure the file does not exist.
        /// </param>
        /// <param name="outputMimeType">
        /// The <paramref name="output"/> Mime Type.
        /// </param>
        /// <param name="request">
        /// The segment <paramref name="request"/>.
        /// </param>
        void Segment(FileInfo source, string sourceMimeType, FileInfo output, string outputMimeType, AudioUtilityRequest request);

        /// <summary>
        /// Calculate duration of <paramref name="source"/> audio file.
        /// </summary>
        /// <param name="source">
        /// The <paramref name="source"/> audio file.
        /// </param>
        /// <param name="sourceMimeType">
        /// The <paramref name="source"/> Mime Type.
        /// </param>
        /// <returns>
        /// Duration of <paramref name="source"/> audio file.
        /// </returns>
        TimeSpan Duration(FileInfo source, string sourceMimeType);

        /// <summary>
        /// Get meta data for the given file.
        /// </summary>
        /// <param name="source">File to get meta data from. This should be an audio file.</param>
        /// <returns>A dictionary containing meta data for the given file.</returns>
        Dictionary<string, string> Info(FileInfo source);
    }
}
