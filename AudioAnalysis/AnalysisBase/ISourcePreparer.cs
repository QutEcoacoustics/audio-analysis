namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Segment;

    /// <summary>
    /// Interface for preparing source files.
    /// </summary>
    public interface ISourcePreparer
    {
        /// <summary>
        /// Prepare an audio file. This will be a single segment of a larger audio file, modified based on the analysisSettings.
        /// </summary>
        /// <param name="outputDirectory">
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
        Task<FileSegment> PrepareFile(DirectoryInfo outputDirectory, string source, string outputMediaType, TimeSpan startOffset, TimeSpan endOffset, int targetSampleRateHz);

        /// <summary>
        /// Prepare an audio file. This will be a single segment of a larger audio file, modified based on the analysisSettings.
        /// </summary>
        /// <param name="outputDirectory">
        ///     The analysis Base Directory.
        /// </param>
        /// <param name="source">
        ///     The source audio file.
        /// </param>
        /// <param name="outputMediaType">
        ///     The output Media Type.
        /// </param>
        /// <param name="startOffset">
        ///     The start Offset from start of entire original file.
        /// </param>
        /// <param name="endOffset">
        ///     The end Offset from start of entire original file.
        /// </param>
        /// <param name="targetSampleRateHz">
        ///     The target Sample Rate Hz.
        /// </param>
        /// <param name="temporaryFilesDirectory">
        ///     The directory for temporary files.
        /// </param>
        /// <param name="channelSelection"></param>
        /// <param name="mixDownToMono"></param>
        /// <returns>
        /// The prepared file.
        /// </returns>
        Task<FileSegment> PrepareFile<TSource>(DirectoryInfo outputDirectory, TSource source, string outputMediaType, TimeSpan startOffset, TimeSpan endOffset, int targetSampleRateHz, DirectoryInfo temporaryFilesDirectory, int[] channelSelection, bool? mixDownToMono);

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
        /// <remarks>
        /// This API does not fit with the other two. We should consider factoring it out.
        /// </remarks>
        IEnumerable<ISegment<TSource>> CalculateSegments<TSource>(IEnumerable<ISegment<TSource>> fileSegments, AnalysisSettings settings);
    }
}
