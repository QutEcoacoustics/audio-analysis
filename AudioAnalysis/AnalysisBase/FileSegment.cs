// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileSegment.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
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
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    using Acoustics.Shared;

    using log4net;
    using SegmentAnalysis;

    /// <summary>
    /// Represents a segment of a target file. It can also store the parent file that a new segment has been derived from.
    /// A segment is just a stored start and end for a target file - it represents a future, or a request.
    /// Other functions can take the segment request, cut out the selected range, and return a new file segment.
    /// New file segments, or so segments that represent a whole file, will not have the segment properties set because they do not represent a request anymore.
    /// </summary>
    public class FileSegment : ICloneable, ISegment<FileInfo>
    {
        /// <summary>
        /// How FileSegment should try and parse the file's absolute date.
        /// </summary>
        public enum FileDateBehavior
        {
            /// <summary>
            /// Try and parse the file's absolute date
            /// </summary>
            Try,

            /// <summary>
            /// Parse the file's absolute date and fail if unsuccessful
            /// </summary>
            Required,

            /// <summary>
            /// Do no try and parse the file's date at all.
            /// </summary>
            None
        }

        private readonly FileDateBehavior dateBehavior;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private DateTimeOffset? fileStartDate;
        private bool triedToParseDate = false;

        public FileSegment(FileInfo targetFile, int? sampleRate = null, TimeSpan? duration = null, FileDateBehavior dateBehavior = FileDateBehavior.None)
        {
            this.dateBehavior = dateBehavior;
            this.TargetFile = targetFile;
            this.TargetFileSampleRate = sampleRate;
            this.TargetFileDuration = duration;
            this.Alignment = TimeAlignment.None;

            this.ParseDate();
        }

        /// <summary>
        /// Allow specifying an absolutely aligned (to the nearest minute) file segment.
        /// Implies `FileDateBehavior.Required`.
        /// </summary>
        public FileSegment(FileInfo targetFile, TimeAlignment alignment)
        {
            this.dateBehavior = alignment == TimeAlignment.None ? FileDateBehavior.Try : FileDateBehavior.Required;
            this.TargetFile = targetFile;
            this.Alignment = alignment;

            this.ParseDate();
        }

        private void ParseDate()
        {
            if (this.dateBehavior != FileDateBehavior.None)
            {
                this.triedToParseDate = true;
                this.fileStartDate = this.AudioFileStart();

                if (this.dateBehavior == FileDateBehavior.Required)
                {
                    if (!this.fileStartDate.HasValue)
                    {
                        throw new InvalidFileDateException("A file date is required but one has not been successfully parsed");
                    }
                }
            }
        }

        /// <summary>
        /// Gets the style of alignment padding to use - indicates that bad start dates *should* be shifted to the nearest minute.
        /// </summary>
        public TimeAlignment Alignment { get; private set; }

        /// <summary>
        /// Gets the target file for this file segment.
        /// </summary>
        public FileInfo TargetFile { get; }

        /// <summary>
        /// Gets or sets SegmentStartOffset - the value that represents what starting point of the target file should be used.
        /// </summary>
        public TimeSpan? SegmentStartOffset { get; set; }

        /// <summary>
        /// Gets or sets SegmentEndOffset - the value that represents what ending point of the target file should be used.
        /// </summary>
        public TimeSpan? SegmentEndOffset { get; set; }

        /// <summary>
        /// Gets ISegmentSet - whether or not either of the segment properties have been set.
        /// If IsSegmentSet is true, then it means this file segment represents a fraction of the target file.
        /// </summary>
        public bool IsSegmentSet => this.SegmentStartOffset.HasValue || this.SegmentEndOffset.HasValue;

        /// <summary>
        /// Gets or sets the entire audio file duration FOR THE TARGET FILE.
        /// </summary>
        public TimeSpan? TargetFileDuration { get; set; }

        /// <summary>
        /// Gets or sets the TARGET FILE'S audio file Sample rate.
        /// May be required when doing analysis.
        /// </summary>
        public int? TargetFileSampleRate { get; set; }

        /// <summary>
        /// Gets the TargetFileStartDate
        /// </summary>
        public DateTimeOffset? TargetFileStartDate {
            get
            {
                if (!this.fileStartDate.HasValue && !this.triedToParseDate)
                {
                    this.triedToParseDate = true;
                    this.fileStartDate = this.AudioFileStart();
                }

                return this.fileStartDate;
            }
        }

        /// <summary>
        /// Validate the <see cref="FileSegment"/> properties.
        /// </summary>
        /// <returns>
        /// True if properties are valid, otherwise false.
        /// </returns>
        public bool Validate()
        {
            if (this.TargetFile == null ||
                 !File.Exists(this.TargetFile.FullName))
            {
                return false;
            }

            if (this.dateBehavior == FileDateBehavior.Required && this.fileStartDate == null)
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

        /// <summary>
        /// Returns a friendly string representation of this object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "{0} ({3}{4}) {1} - {2}",
                this.TargetFile.Name,
                this.SegmentStartOffset?.ToString() ?? "start",
                this.SegmentEndOffset?.ToString() ?? "end",
                this.TargetFileDuration,
                this.TargetFileSampleRate.HasValue ? ", " + this.TargetFileSampleRate.Value + "hz" : string.Empty);
        }

        /// <inheritdoc/>
        public object Clone()
        {
            var newSegment = new FileSegment(
                targetFile: this.TargetFile,
                sampleRate: this.TargetFileSampleRate,
                duration: this.TargetFileDuration,
                dateBehavior: FileDateBehavior.None);

            if (this.dateBehavior != FileDateBehavior.None)
            {
                newSegment.fileStartDate = this.TargetFileStartDate;
            }

            return newSegment;
        }

        /// <summary>
        /// Gets the modified date time of the target file.
        /// </summary>
        /// <returns>The date if the file is set and if it exists. Otherwise returns null.</returns>
        public DateTime? TargetFileModifiedDateTime()
        {
            if (this.TargetFile != null && this.TargetFile.Exists)
            {
                var createTime = this.TargetFile.CreationTime;
                ////var accessTime = this.targetFile.LastAccessTime;
                var modifyTime = this.TargetFile.LastWriteTime;

                // just assume the earliest date is the one to use
                var result = createTime < modifyTime ? createTime : modifyTime;
                return result;
            }

            return null;
        }

        private DateTimeOffset? AudioFileStart()
        {
            DateTimeOffset parsedDate;
            bool fileDateFound = FileDateHelpers.FileNameContainsDateTime(this.TargetFile.Name, out parsedDate);

            if (fileDateFound)
            {
                Log.Info("Parsed file start date as " + parsedDate.ToString("O"));
                return parsedDate;
            }

            if (this.dateBehavior == FileDateBehavior.Required)
            {
                return null;
            }

            /*
             *  Disabling this block - it is too unpredictable.
             *  No guarantee where a date is coming from.
             *  Additionally, in a performance centric scenario, an
             *  extra call to audio libs is unnecessary
             */
            /*
            var dateTime = this.FileModifiedDateTime();
            if (this.OriginalFileDuration > TimeSpan.Zero && dateTime.HasValue &&
                dateTime.Value > DateTime.MinValue && dateTime.Value < DateTime.MaxValue)
            {
                return dateTime - this.OriginalFileDuration;
            }
            */

            return null;
        }

        FileInfo ISegment<FileInfo>.Source => this.TargetFile;

        double ISegment<FileInfo>.StartOffsetSeconds => this.SegmentStartOffset.Value.TotalSeconds;

        double ISegment<FileInfo>.EndOffsetSeconds => this.SegmentEndOffset.Value.TotalSeconds;
    }
}
