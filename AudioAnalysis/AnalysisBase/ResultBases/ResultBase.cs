// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResultBase.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
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
    public abstract class ResultBase
    {
        private TimeSpan segmentStartOffset;

        /// <summary>
        /// Gets or sets the filename of the audio file this result produced.
        /// </summary>
        public string FileName { get; set; }

        // found no use for this (or usage) disabled
        ////public int SegmentCount { get; set; }

        /// <summary>
        /// Gets or sets the SegmentStartOffset.
        /// I.e. the time since the start of the original audio recording.
        /// E.g. Given segment 78 of a 120min audio file, with a segment size of 60 seconds, this property would hold 78 minutes.
        /// </summary>
        public TimeSpan SegmentStartOffset
        {
            get
            {
                return this.segmentStartOffset;
            }
            set
            {
                this.StartOffsetMinute = (int)(value).TotalMinutes;
                this.segmentStartOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets the duration of audio segment that produced this result.
        /// This is recording because there is typically some error in cutting out segments of an audio file.
        /// This property was previously aliased "SEGMENT_TIMESPAN" and "SegTimeSpan".
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public TimeSpan SegmentDuration { get; set; }

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
            return this.SegmentStartOffset.CompareTo(other.SegmentStartOffset);
        }
    }
}