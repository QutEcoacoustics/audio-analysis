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
    using Acoustics.Shared.Contracts;
    using Acoustics.Tools.Audio;
    using log4net;
    using Segment;

    /// <summary>
    /// Represents a segment of a target file. It can also store the parent file that a new segment has been derived
    /// from. A segment is just a stored start and end for a target file - it represents a future, or a request.
    /// Other functions can take the segment request, cut out the selected range, and return a new file segment.
    /// New file segments, or segments that represent a whole file, will not have the segment properties set
    /// because they do not represent a request anymore.
    /// </summary>
    public class FileSegment : ICloneable, ISegment<FileInfo>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly FileDateBehavior dateBehavior;

        private bool triedToParseDate = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSegment"/> class.
        /// Use this constructor if you know all the information about a segment beforehand.
        /// </summary>
        public FileSegment(FileInfo source, int sampleRate, TimeSpan duration, FileDateBehavior dateBehavior = FileDateBehavior.None, DateTimeOffset? suppliedDate = null)
        {
            this.dateBehavior = dateBehavior;
            this.Source = source;

            var basename = Path.GetFileNameWithoutExtension(this.Source.Name);
            var fileDate = this.ParseDate(suppliedDate);

            this.SourceMetadata = new SourceMetadata(duration, sampleRate, basename, fileDate);

            this.Alignment = TimeAlignment.None;

            Contract.Ensures(this.Validate(), "FileSegment did not validate");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSegment"/> class.
        /// Allow specifying an absolutely aligned (to the nearest minute) file segment.
        /// Implies `FileDateBehavior.Required`.
        /// NOTE: Start offset will be set to start of file, and end offset set to the end of the file.
        /// </summary>
        public FileSegment(FileInfo source, TimeAlignment alignment, IAudioUtility utility = null)
        {
            Contract.Requires(source != null);

            this.dateBehavior = alignment == TimeAlignment.None ? FileDateBehavior.Try : FileDateBehavior.Required;
            this.Source = source;
            this.Alignment = alignment;

            var basename = Path.GetFileNameWithoutExtension(this.Source.Name);
            var fileDate = this.ParseDate(null);

            var info = (utility ?? new MasterAudioUtility()).Info(source);
            this.SourceMetadata = new SourceMetadata(info.Duration.Value, info.SampleRate.Value, basename, fileDate);

            Contract.Ensures(this.Validate(), "FileSegment did not validate");
        }

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
            None,
        }

        /// <summary>
        /// Gets the style of alignment padding to use - indicates that bad start dates *should* be shifted to the
        /// nearest minute.
        /// </summary>
        public TimeAlignment Alignment { get; }

        /// <summary>
        /// Gets or sets SegmentStartOffset - the value that represents what starting point of the target file should
        /// be used.
        /// </summary>
        /// <remarks>
        /// These properties are the source for <see cref="ISegment{TSource}.StartOffsetSeconds"/> and are maintained
        /// as nullables for backwards compatibility. However as far as <see cref="ISegment{TSource}"/> is concerned
        /// null is not a valid value.
        /// </remarks>
        public TimeSpan? SegmentStartOffset { get; set; }

        /// <summary>
        /// Gets or sets SegmentEndOffset - the value that represents what ending point of the target file should be used.
        /// </summary>
        /// <remarks>
        /// These properties are the source for <see cref="ISegment{TSource}.EndOffsetSeconds"/> and are maintained
        /// as nullables for backwards compatibility. However as far as <see cref="ISegment{TSource}"/> is concerned
        /// null is not a valid value.
        /// </remarks>
        public TimeSpan? SegmentEndOffset { get; set; }

        /// <summary>
        /// Gets ISegmentSet - whether or not either of the segment properties have been set.
        /// </summary>
        public bool IsSegmentSet => this.SegmentStartOffset.HasValue || this.SegmentEndOffset.HasValue;

        /// <summary>
        /// Gets the entire audio file duration FOR THE TARGET FILE.
        /// </summary>
        public TimeSpan? TargetFileDuration => this.SourceMetadata?.Duration;

        /// <summary>
        /// Gets the TARGET FILE'S audio file Sample rate.
        /// May be required when doing analysis.
        /// </summary>
        public int? TargetFileSampleRate => this.SourceMetadata?.SampleRate;

        /// <summary>
        /// Gets the TargetFileStartDate
        /// </summary>
        public DateTimeOffset? TargetFileStartDate => this.SourceMetadata?.RecordedDate;

        /// <inheritdoc/>
        public object Clone()
        {
            if (this.SourceMetadata == null)
            {
                throw new NullReferenceException($"{nameof(this.SourceMetadata)} must not be null to clone segment");
            }

            var newSegment = new FileSegment(
                source: this.Source,
                sampleRate: this.SourceMetadata.SampleRate,
                duration: this.SourceMetadata.Duration,
                dateBehavior: this.dateBehavior,
                suppliedDate: this.TargetFileStartDate);

            return newSegment;
        }

        /// <summary>
        /// Gets the target file for this file segment.
        /// </summary>
        public FileInfo Source { get; }

        public SourceMetadata SourceMetadata { get; }

        public double StartOffsetSeconds => this.SegmentStartOffset?.TotalSeconds ?? 0.0;

        public double EndOffsetSeconds => this.SegmentEndOffset?.TotalSeconds ?? this.SourceMetadata.Duration.TotalSeconds;

        public ISegment<FileInfo> SplitSegment(double newStart, double newEnd)
        {
            var copy = (FileSegment)this.Clone();
            copy.SegmentStartOffset = newStart.Seconds();
            copy.SegmentEndOffset = newEnd.Seconds();

            return copy;
        }

        /// <summary>
        /// Validate the <see cref="FileSegment"/> properties.
        /// </summary>
        /// <returns>
        /// True if properties are valid, otherwise false.
        /// </returns>
        public bool Validate()
        {
            if (this.Source == null ||
                 !File.Exists(this.Source.FullName))
            {
                return false;
            }

            if (this.dateBehavior == FileDateBehavior.Required && this.SourceMetadata?.RecordedDate == null)
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
        /// Determines if two FileSegments represent the same part of a source file.
        /// It compares <see cref="Source"/>, <see cref="StartOffsetSeconds"/>, and <see cref="EndOffsetSeconds"/>.
        /// </summary>
        /// <param name="other">The other file segment to compare with.</param>
        /// <returns>True if the segments are considered equal.</returns>
        public bool Equals(ISegment<FileInfo> other)
        {
            if (other == null)
            {
                return false;
            }

            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Source.FullName == other.Source.FullName &&
                   this.StartOffsetSeconds == other.StartOffsetSeconds &&
                   this.EndOffsetSeconds == other.EndOffsetSeconds;
        }

        /// <summary>
        /// Returns a friendly string representation of this object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "{0} ({3}{4}) {1} - {2}",
                this.Source.Name,
                this.SegmentStartOffset?.ToString() ?? "<null start>",
                this.SegmentEndOffset?.ToString() ?? "<null end>",
                this.TargetFileDuration,
                this.TargetFileSampleRate.HasValue ? ", " + this.TargetFileSampleRate.Value + "hz" : string.Empty);
        }

        private DateTimeOffset? ParseDate(DateTimeOffset? suppliedDate = null)
        {
            if (this.dateBehavior == FileDateBehavior.None)
            {
                return null;
            }

            this.triedToParseDate = true;
            var date = suppliedDate ?? this.AudioFileStart();

            if (this.dateBehavior == FileDateBehavior.Required)
            {
                if (!date.HasValue)
                {
                    throw new InvalidFileDateException(
                        "A file date is required but one has not been successfully parsed");
                }
            }

            return date;
        }

        private DateTimeOffset? AudioFileStart()
        {
            DateTimeOffset parsedDate;
            bool fileDateFound = FileDateHelpers.FileNameContainsDateTime(this.Source.Name, out parsedDate);

            if (fileDateFound)
            {
                Log.Info("Parsed file start date as " + parsedDate.ToString("O"));
                return parsedDate;
            }

            if (this.dateBehavior == FileDateBehavior.Required)
            {
                return null;
            }

            // Historical note: This method previously supported inferring the date of the recording from the file's
            // last modified timestamp. This method ultimately proved unreliable and inefficient.
            // Support was removed for this edge case mid 2017.

            return null;
        }
    }
}
