// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoubleExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the DoubleExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace System
{
    using System.Runtime.CompilerServices;

    public static class DoubleExtensions
    {
        public static TimeSpan Seconds(this double seconds) => TimeSpan.FromSeconds(seconds);

        public static TimeSpan Seconds(this int seconds) => TimeSpan.FromSeconds(seconds);

        public static TimeSpan Seconds(this long seconds) => TimeSpan.FromSeconds(seconds);

        /// <summary>
        /// Round a number to <paramref name="digits"/> significant places.
        /// </summary>
        /// <remarks>
        /// Sourced from: http://stackoverflow.com/questions/374316/round-a-double-to-x-significant-figures.
        /// </remarks>
        /// <param name="d">The value to round.</param>
        /// <param name="digits">The number of significant digits to keep.</param>
        /// <returns>The rounded value.</returns>
        public static double RoundToSignificantDigits(this double d, int digits)
        {
            // preserve value as much as possible - this is reasonable since we will rescale to original magnitude
            decimal dec = (decimal)d;
            if (dec == 0)
            {
                return 0;
            }

            decimal scale = (decimal)Math.Pow(10, Math.Floor(Math.Log10((double)Math.Abs(dec))) + 1);
            return (double)(scale * Math.Round(dec / scale, digits, MidpointRounding.AwayFromZero));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Round(this double d, MidpointRounding rounding = MidpointRounding.AwayFromZero) => Math.Round(d, rounding);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Floor(this double d) => Math.Floor(d);

        /// <summary>
        /// Scales a unit double value, that is in the interval [0.0, 1.0] to a byte [0, 255]
        /// value between 0 and 255, clamping out of bound values and rounding away from zero.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The clamped and rounded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ScaleUnitToByte(this double value)
        {
            value *= byte.MaxValue;

            if (value > byte.MaxValue)
            {
                return byte.MaxValue;
            }

            if (value < byte.MinValue)
            {
                return byte.MinValue;
            }

            return (byte)value.Round();
        }
    }
}