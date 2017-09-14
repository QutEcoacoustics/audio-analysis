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
        private double resultStartSeconds;

        /// <summary>
        /// Gets or sets the filename of the audio file this result produced.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the result start time in seconds.
        /// This is relative to the start of the recording.
        /// This basically allows every sort of result to be sorted/time indexed from the start of the file/recording.
        /// </summary>
        /// <remarks>
        /// I.e. the time since the start of the original audio recording.
        /// E.g. Given segment 78 of a 120min audio file, with a segment size of 60 seconds, this property would hold 78 minutes.
        /// And again: StartOffset is the time offset between the start of the recording and the start of the current result.
        /// </remarks>
        public double ResultStartSeconds
        {
            get
            {
                return this.resultStartSeconds;
            }

            set
            {
                this.ResultMinute = (int)Math.Floor(value % 60.0);
                this.resultStartSeconds = value;
            }
        }

        /// <summary>
        /// Gets or sets the duration of audio segment that produced this result.
        /// This is tracked because there is typically some error in cutting out segments of an audio file and it is
        /// useful to know how much audio was actually used to generate the result.
        /// </summary>
        public double SegmentDurationSeconds { get; set; }

        /// <summary>
        /// Gets the ResultMinute.
        /// This is an integer representation of <see cref="ResultStartSeconds"/>.
        /// </summary>
        public int ResultMinute { get; private set; }

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

            return this.ResultStartSeconds.CompareTo(other.ResultStartSeconds);
        }

        /// <inheritdoc/>
        public virtual int CompareTo(object obj)
        {
            return this.CompareTo((ResultBase)obj);
        }
    }
}