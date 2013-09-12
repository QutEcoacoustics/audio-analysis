namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents a segment file. Also stores the orginal file. 
    /// Be aware that the original file may also be a segment file.
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
        /// May be required when doing analysis.
        /// </summary>
        public int? OriginalFileSampleRate { get; set; }

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

        public override string ToString()
        {
            return string.Format("{0} ({3}{4}) {1} - {2}",
                this.OriginalFile.Name,
                this.SegmentStartOffset.HasValue ? this.SegmentStartOffset.Value.ToString() : "start",
                this.SegmentEndOffset.HasValue ? this.SegmentEndOffset.Value.ToString() : "end",
                this.OriginalFileDuration,
                this.OriginalFileSampleRate.HasValue ? ", "+this.OriginalFileSampleRate.Value+"hz" : string.Empty
                );
        }
    }
}
