// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeAlignment.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the TimeAlignment type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisBase
{
    /// <summary>
    /// Indicates the automatic alignment style used to produce segments that align
    /// to the nearest minute in absolute time.
    /// </summary>
    public enum TimeAlignment
    {
        /// <summary>
        /// No alignment is done
        /// </summary>
        None,

        /// <summary>
        /// Alignment to the nearest minute is done.
        /// Both start and end fractional components are discarded.
        /// </summary>
        TrimBoth,

        /// <summary>
        /// Alignment to the nearest minute is done.
        /// The end fractional component is discarded but the start fractional component is kept.
        /// </summary>
        TrimStart,

        /// <summary>
        /// Alignment to the nearest minute is done.
        /// The start fractional component is discarded but the end fractional component is kept.
        /// </summary>
        TrimEnd,

        /// <summary>
        /// Alignment to the nearest minute is done.
        /// Neither start or end fractional components are discarded.
        /// </summary>
        TrimNeither
    }
}
