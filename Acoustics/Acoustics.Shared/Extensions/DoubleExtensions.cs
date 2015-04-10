// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoubleExtensions.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the DoubleExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared.Extensions
{
    using System;

    public static class DoubleExtensions
    {
        public static TimeSpan Seconds(this double seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }
    }
}
