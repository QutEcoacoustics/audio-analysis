// <copyright file="ISourceMetadata.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisBase.Segment
{
    using System;

    /// <summary>
    /// Information about a source audio object.
    /// </summary>
    internal interface ISourceMetadata
    {
        /// <summary>
        /// Gets Duration - the length of the source audio object.
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// Gets SampleRate - the number of samples per second in the source audio object.
        /// Store sample rate of original audio object.
        /// May need original SR during the analysis, esp if have upsampled from the original SR.
        /// </summary>
        int SampleRate { get; }

        /// <summary>
        /// Gets Identifier - a string that uniquely identifies the source audio object.
        /// </summary>
        string Identifier { get; }

        /// <summary>
        /// Gets the date the source audio object started recording at.
        /// </summary>
        DateTimeOffset? RecordedDate { get; }
    }
}