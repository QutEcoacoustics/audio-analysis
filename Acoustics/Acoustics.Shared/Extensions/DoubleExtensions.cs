// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoubleExtensions.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the DoubleExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace System
{
    using System;

    public static class DoubleExtensions
    {
        public static TimeSpan Seconds(this double seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        /// <summary>
        /// Round a number to `digits` significant places.
        /// Sourced from: http://stackoverflow.com/questions/374316/round-a-double-to-x-significant-figures
        /// </summary>
        /// <param name="d"></param>
        /// <param name="digits"></param>
        /// <returns></returns>
        public static double RoundToSignficantDigits(this double d, int digits)
        {
            // preserve value as much as possible - this is reasonable since we will rescale to orginal magnitude
            decimal dec = (decimal)d;
            if (dec == 0)
            {
                return 0;
            }

            decimal scale = (decimal)Math.Pow(10, Math.Floor(Math.Log10((double)Math.Abs(dec))) + 1);
            return (double)(scale * Math.Round(dec / scale, digits));
        }
    }
}
