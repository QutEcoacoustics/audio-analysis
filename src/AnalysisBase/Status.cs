// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Status.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Interface a compatible analysis must implement.
//   This is a strong typed version of <c>IAnalyser</c> intentionally removed from the old inheritance tree.
//   DO NOT MODIFY THIS FILE UNLESS INSTRUCTED TO!
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisBase
{
    public enum Status
    {
        /// <summary>
        /// Unknown or not set.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Code exists and it may run without failure.
        /// </summary>
        InDevelopment,

        /// <summary>
        /// The analysis works as determined by at least one unit test.
        /// </summary>
        Alpha,

        /// <summary>
        /// The analysis is covered by a suite of external tests as well as internal unit tests.
        /// </summary>
        Beta,

        /// <summary>
        /// Works, internal tests, external tests, wide spread utility.
        /// </summary>
        Maintained,

        /// <summary>
        /// Not maintined, or updated. May work but most likely stale and out of date.
        /// </summary>
        Unmaintained,

        /// <summary>
        /// Does not work, or you are advised not to use due to fundamental problems.
        /// </summary>
        Retired,
    }
}