// <copyright file="SourceMetadata.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisBase.Segment
{
    using System;

    /// <inheritdoc cref="ISourceMetadata"/>
    public class SourceMetadata : ISourceMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceMetadata"/> class.
        /// </summary>
        /// <param name="duration">The duration of the source audio object.</param>
        /// <param name="sampleRate">The sample rate of the source audio object.</param>
        /// <param name="identifier">A unique textual identifier.</param>
        /// <param name="recordedDate">The Date the audio recording was recorded.</param>
        public SourceMetadata(TimeSpan duration, int sampleRate, string identifier, DateTimeOffset? recordedDate)
        {
            this.Duration = duration;
            this.SampleRate = sampleRate;
            this.Identifier = identifier;
            this.RecordedDate = recordedDate;
        }

        /// <summary>
        /// Gets Duration - the length of the source audio object.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Gets SampleRate - the number of samples per second in the source audio object.
        /// Store sample rate of original audio object.
        /// May need original SR during the analysis, esp if have upsampled from the original SR.
        /// </summary>
        public int SampleRate { get; }

        /// <summary>
        /// Gets Identifier - a string that uniquely identifies the source audio object.
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Gets the date the source audio object started recording at.
        /// </summary>
        public DateTimeOffset? RecordedDate { get; }
    }
}