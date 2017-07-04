// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResultBase.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the ResultBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisBase.ResultBases
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// This is the base type for every type of result we produce.
    /// This includes: events, summary indices, and spectral indices.
    /// These classes will hold much redundant information - the flat format is useful for CSV output.
    /// </summary>
    public abstract class ResultBase : IComparable<ResultBase>, IComparable
    {
        private TimeSpan startOffset;

        /// <summary>
        /// Gets or sets the filename of the audio file this result produced.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the time StartOffset.
        /// This basically allows every sort of result to be sorted/time indexed from the start of the file/recording.
        /// It replaced SegmentStartOffset but is NOT THE SAME.
        /// I.e. the time since the start of the original audio recording.
        /// E.g. Given segment 78 of a 120min audio file, with a segment size of 60 seconds, this property would hold 78 minutes.
        /// And again: StartOffset is the time offset between the start of the recording and the start of the current result.
        /// </summary>
        public TimeSpan StartOffset
        {
            get
            {
                return this.startOffset;
            }

            set
            {
                this.StartOffsetMinute = (int)value.TotalMinutes;
                this.StartOffsetSecond = (float)value.TotalSeconds;
                this.startOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets the duration of audio segment that produced this result.
        /// This is recording because there is typically some error in cutting out segments of an audio file.
        /// This property was previously aliased "SEGMENT_TIMESPAN" and "SegTimeSpan".
        /// </summary>
        public TimeSpan SegmentDuration { get; set; }

        /// <summary>
        /// Gets the StartOffsetSecond.
        /// This is an representation of <c>SegmentStartOffset</c>.
        /// </summary>
        [Obsolete]
        internal float StartOffsetSecond { get; private set; }

        /// <summary>
        /// Gets the StartOffsetMinute.
        /// This is an integer representation of <c>SegmentStartOffset</c>.
        /// This property was previously aliased as "START_MIN", "start-min", and <c>AudioAnalysisTools.Keys.EVENT_START_MIN</c>.
        /// </summary>
        public int StartOffsetMinute { get; private set; }

        /// <summary>
        /// Defines an innate order of Analysis results based on the <c>SegmentStartOffset</c>.
        /// </summary>
        /// <param name="other">The other AnalysisResult to compare to.</param>
        /// <returns>A integer representing the relative order between the two instances.</returns>
        public virtual int CompareTo(ResultBase other)
        {
            if (other == null)
            {
                return 1;
            }

            return this.StartOffset.CompareTo(other.StartOffset);
        }

        /// <inheritdoc/>
        public virtual int CompareTo(object obj)
        {
            return this.CompareTo((ResultBase)obj);
        }
    }
}