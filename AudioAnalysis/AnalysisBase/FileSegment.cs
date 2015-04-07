// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileSegment.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Represents a segment file. Also stores the original file.
//   Be aware that the original file may also be a segment file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    using Acoustics.Shared;

    using log4net;

    /// <summary>
    /// Represents a segment file. Also stores the original file. 
    /// Be aware that the original file may also be a segment file.
    /// </summary>
    public class FileSegment
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DateTimeOffset? fileStartDate;
        private readonly bool fileDateRequired;

        public FileSegment(FileInfo originalFile)
            : this(originalFile, false)
        {

        }

        public FileSegment(FileInfo originalFile, bool fileDateRequried)
        {
            this.fileDateRequired = fileDateRequried;
            this.OriginalFile = originalFile;
            this.fileStartDate = this.AudioFileStart();

            if (this.fileDateRequired && !this.fileStartDate.HasValue)
            {
                throw new InvalidOperationException("A file date is required but one has not been sucessfully parsed");
            }
        }

        /// <summary>
        /// Gets the Original File.
        /// </summary>
        public FileInfo OriginalFile { get; private set; }

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
        /// Gets the OriginalFileStartDate
        /// </summary>
        public DateTimeOffset? OriginalFileStartDate {
            get
            {
                return this.fileStartDate;
            } 
        }

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

            if (this.fileDateRequired && this.fileStartDate == null)
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
            return string.Format(
                "{0} ({3}{4}) {1} - {2}",
                this.OriginalFile.Name,
                this.SegmentStartOffset.HasValue ? this.SegmentStartOffset.Value.ToString() : "start",
                this.SegmentEndOffset.HasValue ? this.SegmentEndOffset.Value.ToString() : "end",
                this.OriginalFileDuration,
                this.OriginalFileSampleRate.HasValue ? ", " + this.OriginalFileSampleRate.Value + "hz" : string.Empty);
        }

        public DateTime? FileModifedDateTime()
        {
            if (this.OriginalFile != null && this.OriginalFile.Exists)
            {
                var createTime = this.OriginalFile.CreationTime;
                ////var accessTime = this.OriginalFile.LastAccessTime;
                var modifyTime = this.OriginalFile.LastWriteTime;

                // just assume the earliest date is the one to use
                var result = createTime < modifyTime ? createTime : modifyTime;
                return result;
            }

            return null;
        }

        private DateTimeOffset? AudioFileStart()
        {
            DateTimeOffset parsedDate;
            var fileDateFound = FileDateHelpers.FileNameContainsDateTime(this.OriginalFile.Name, out parsedDate);

            if (fileDateFound)
            {
                Log.Debug("Parsed file start date as " + parsedDate.ToString("O"));
                return parsedDate;
            }

            if (this.fileDateRequired)
            {
                return null;
            }

            var dateTime = this.FileModifedDateTime();
            if (this.OriginalFileDuration > TimeSpan.Zero && dateTime.HasValue &&
                dateTime.Value > DateTime.MinValue && dateTime.Value < DateTime.MaxValue)
            {
                return dateTime - this.OriginalFileDuration;
            }

            return null;
        }
    }
}
