namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Interface for preparing source files.
    /// </summary>
    public interface ISourcePreparer
    {
        /// <summary>
        /// Prepare an audio file. This will be a single segment of a larger audio file, modified based on the analysisSettings.
        /// </summary>
        /// <param name="analysisBaseDirectory">
        /// The analysis Base Directory.
        /// </param>
        /// <param name="source">
        /// The source audio file.
        /// </param>
        /// <param name="outputMediaType">
        /// The output Media Type.
        /// </param>
        /// <param name="startOffset">
        /// The start Offset from start of entire original file.
        /// </param>
        /// <param name="endOffset">
        /// The end Offset from start of entire original file.
        /// </param>
        /// <param name="targetSampleRateHz">
        /// The target Sample Rate Hz.
        /// </param>
        /// <returns>
        /// The prepared file.
        /// </returns>
        FileInfo PrepareFile(DirectoryInfo analysisBaseDirectory, FileInfo source, string outputMediaType, TimeSpan startOffset, TimeSpan endOffset, int targetSampleRateHz);

        /// <summary>
        /// Calculate the file segments for analysis.
        /// </summary>
        /// <param name="fileSegments">
        /// The file segments.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <returns>
        /// Enumerable of file segments.
        /// </returns>
        IEnumerable<FileSegment> CalculateSegments(IEnumerable<FileSegment> fileSegments, AnalysisSettings settings);
    }
}
