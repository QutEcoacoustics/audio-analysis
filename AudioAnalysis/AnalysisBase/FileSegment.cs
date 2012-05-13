﻿namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents a file and a segment within the file.
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
        public TimeSpan SegmentStartOffset { get; set; }

        /// <summary>
        /// Gets or sets SegmentEndOffset.
        /// </summary>
        public TimeSpan SegmentEndOffset { get; set; }

        /// <summary>
        /// Validate the <see cref="FileSegment"/> properties.
        /// </summary>
        /// <returns>
        /// True if properties are valid, otherwise false.
        /// </returns>
        public bool Validate()
        {
            return this.OriginalFile != null
                 && File.Exists(this.OriginalFile.FullName)
                 && this.SegmentStartOffset >= TimeSpan.Zero
                 && this.SegmentEndOffset > TimeSpan.Zero
                 && this.SegmentStartOffset < this.SegmentEndOffset;
        }
    }
}
