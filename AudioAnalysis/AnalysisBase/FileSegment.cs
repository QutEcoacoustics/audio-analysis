namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents a file and a segment within the file.
    /// 
    /// TODO!! THERE IS A REAL PROBLEM HERE BECAUSE THE ONE CLASS MUST REPRESENT INFO ABOUT CURRENT FILE SEGMENT 
    /// AND THE ORIGINAL FILE FROM WHICH THE SEGMENT WAS EXTRACTED. PERHAPS A SOLUTIOn IS TO HAVE 
    /// a FileSegment member in the FileSegment class. SEE BELOW
    /// </summary>
    public class FileSegment
    {
        /// <summary>
        /// Gets or sets the Original File.
        /// </summary>
        public FileInfo OriginalFile { get; set; }

        /// <summary>
        /// Gets or sets SegmentStartOffset.
        /// </summary>
        public TimeSpan? SegmentStartOffset { get; set; }

        /// <summary>
        /// Gets or sets SegmentEndOffset.
        /// </summary>
        public TimeSpan? SegmentEndOffset { get; set; }

        /// <summary>
        /// Gets or sets the entire audio file Duration.
        /// </summary>
        public TimeSpan OriginalFileDuration { get; set; }

        /// <summary>
        /// Gets or sets the original audio file Sample rate.
        /// May be required when oding analysis
        /// </summary>
        public int? OriginalFileSampleRate { get; set; }

        /// <summary>
        /// This member may be way of dealing with the extraction history of segments.
        /// However I have not used it.
        /// </summary>
        public FileSegment veryOriginalAudioFile { get; set; }

        /// <summary>
        /// Validate the <see cref="FileSegment"/> properties.
        /// </summary>
        /// <returns>
        /// True if properties are valid, otherwise false.
        /// </returns>
        [Pure]
        public bool Validate()
        {
            if (this.OriginalFile == null ||
                 !File.Exists(this.OriginalFile.FullName))
            {
                return false;
            }

            if (this.SegmentStartOffset.HasValue && this.SegmentStartOffset < TimeSpan.Zero)
            {
                return false;
            }

            if (this.SegmentStartOffset.HasValue && this.SegmentEndOffset.HasValue && this.SegmentStartOffset >= this.SegmentEndOffset)
            {
                return false;
            }

            return true;
        }
    }
}
