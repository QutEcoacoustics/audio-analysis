// <copyright file="InvalidSegmentException.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisBase.Segment
{
    using System;

    /// <summary>
    /// An exception thrown when an <see cref="ISegment{TSource}"/> has invalid data set.
    /// </summary>
    public class InvalidSegmentException : Exception
    {
        public InvalidSegmentException()
        {
            // needed for reflection instance activation
        }

        public InvalidSegmentException(string message)
            : base(message)
        {
        }

        public InvalidSegmentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}