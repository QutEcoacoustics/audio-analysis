namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

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
                this.OriginalFileSampleRate.HasValue ? ", " + this.OriginalFileSampleRate.Value + "hz" : string.Empty
                );
        }

        public DateTime? FileModifedDateTime()
        {
            if (this.OriginalFile != null && this.OriginalFile.Exists)
            {
                var createTime = this.OriginalFile.CreationTime;
                //var accessTime = this.OriginalFile.LastAccessTime;
                var modifyTime = this.OriginalFile.LastWriteTime;

                // just assume the earliest date is the one to use
                var result = createTime < modifyTime ? createTime : modifyTime;
                return result;
            }

            return null;
        }

        // Prefix_YYYYMMDD_hhmmss.wav
        private readonly string fileNameWithPrefixPattern = @".*_(\d{8}_\d{6})\..+";

        public DateTime? FileNameDateTime()
        {
            if (this.OriginalFile != null && this.OriginalFile.Exists)
            {
                var fileName = this.OriginalFile.Name;

                if (Regex.IsMatch(fileName, fileNameWithPrefixPattern, RegexOptions.IgnoreCase))
                {
                    var match = Regex.Match(fileName, fileNameWithPrefixPattern, RegexOptions.IgnoreCase);
                    DateTime dt;
                    if (DateTime.TryParseExact(
                        match.Groups[1].Value,
                        "yyyyMMdd_HHmmss",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal,
                        out dt))
                    {

                        return dt;
                    }
                }

                return DateTime.MinValue;
            }

            return null;
        }

        public DateTime? AudioFileStart()
        {
            var dateTime = FileNameDateTime();
            if (dateTime.HasValue && dateTime.Value > DateTime.MinValue && dateTime.Value < DateTime.MaxValue)
            {
                return dateTime;
            }

            dateTime = FileModifedDateTime();
            if (this.OriginalFileDuration > TimeSpan.Zero && dateTime.HasValue &&
                dateTime.Value > DateTime.MinValue && dateTime.Value < DateTime.MaxValue)
            {
                return dateTime - this.OriginalFileDuration;
            }

            return null;
        }
    }
}
