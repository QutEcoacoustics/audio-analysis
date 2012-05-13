namespace AnalysisRunner
{
    using System.Collections.Generic;
    using System.IO;

    using Acoustics.Shared;
    using Acoustics.Tools.Audio;

    using AnalysisBase;

    /// <summary>
    /// Local source file preparer.
    /// </summary>
    public class LocalSourcePreparer : ISourcePreparer
    {
        private readonly IAudioUtility audioUtility;

        private readonly ISegmenter segmenter;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalSourcePreparer"/> class.
        /// </summary>
        /// <param name="audioUtility">
        /// The audio Utility.
        /// </param>
        /// <param name="segmenter">
        /// The segmenter.
        /// </param>
        public LocalSourcePreparer(IAudioUtility audioUtility, ISegmenter segmenter)
        {
            this.audioUtility = audioUtility;
            this.segmenter = segmenter;
        }

        /// <summary>
        /// Get the source files based on analysis <paramref name="settings"/>.
        /// </summary>
        /// <param name="settings">
        ///   The <paramref name="settings"/>.
        /// </param>
        /// <param name="fileSegments">
        /// File segments to create.
        /// </param>
        /// <returns>
        /// Enumerable of source files.
        /// </returns>
        public IEnumerable<FileInfo> PrepareFiles(PreparerSettings settings, IEnumerable<FileSegment> fileSegments)
        {
            throw new System.NotImplementedException();
        }
    }
}