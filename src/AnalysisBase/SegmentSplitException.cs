// <copyright file="SegmentSplitException.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisBase
{
    using System;

    /// <inheritdoc />
    /// <summary>
    /// Represents an error thrown when some aspect of a segment split is not allowed.
    /// </summary>
    public class SegmentSplitException : Exception
    {
        /// <inheritdoc cref="Exception(string?)" />
        public SegmentSplitException(string message)
            : base(message)
        {
        }
    }
}