namespace AnalysisBase
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Interface for preparing source files.
    /// </summary>
    public interface ISourcePreparer
    {
        /// <summary>
        /// Get the source files based on analysis <paramref name="settings"/>.
        /// </summary>
        /// <param name="settings">
        ///   The <paramref name="settings"/>.
        /// </param>
        /// <param name="fileSegments">
        /// 
        /// </param>
        /// <returns>
        /// Enumerable of source files.
        /// </returns>
        IEnumerable<FileInfo> PrepareFiles(PreparerSettings settings, IEnumerable<FileSegment> fileSegments);
    }
}
