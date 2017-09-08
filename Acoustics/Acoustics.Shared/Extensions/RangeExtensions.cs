// <copyright file="RangeTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Acoustics.Shared
{
    public static class RangeExtensions
    {
        /// <summary>
        /// Greedily grow the current range to the new duration without exceeding the limits.
        /// Will grow the range around the center of the current range if possible.
        /// Will not fail, and will return as much range as possible, without exceeding limits.
        /// </summary>
        /// <param name="range"></param>
        /// <param name="limits"></param>
        /// <param name="growAmount"></param>
        /// <param name="roundDigits"></param>
        /// <returns></returns>
        public static Range<double> Grow(
            this Range<double> range,
            Range<double> limits,
            double growAmount,
            int? roundDigits = null)
        {
            var limitMagnitude = limits.Magnitude();

            if (growAmount + range.Magnitude() > limitMagnitude)
            {
                return limits;
            }

            var halfGrow = (growAmount / 2.0);

            var newMin = range.Minimum - halfGrow;

            // round the lower bound
            if (roundDigits.HasValue)
            {
                newMin = Math.Round(newMin, roundDigits.Value, MidpointRounding.AwayFromZero);
            }

            var newMax = range.Maximum + halfGrow;

            // round the upper bound
            if (roundDigits.HasValue)
            {
                newMax = Math.Round(newMax, roundDigits.Value, MidpointRounding.AwayFromZero);
            }

            if (newMin < limits.Minimum)
            {
                newMax += limits.Minimum - newMin;
                newMin = limits.Minimum;
            }
            else if (newMax > limits.Maximum)
            {
                newMin -= newMax - limits.Maximum;
                newMax = limits.Maximum;
            }

            return new Range<double>(newMin, newMax);
        }

        public static double Center(this Range<double> range)
        {
            return range.Minimum + (range.Magnitude() / 2.0);
        }

        public static double Magnitude(this Range<double> range)
        {
            return range.Maximum - range.Minimum;
        }

        public static Range<T> AsRange<T>(this (T Minimum, T Maximum) pair)
            where T : struct, IComparable<T>
        {
            return new Range<T>(pair.Minimum, pair.Maximum);
        }

        public static Range<T> To<T>(this T Minimum, T Maximum)
            where T : struct, IComparable<T>
        {
            return new Range<T>(Minimum, Maximum);
        }

        public static Range<T> AsRangeFromZero<T>(this T maximum)
            where T : struct, IComparable<T>
        {
            return new Range<T>(default(T), maximum);
        }
    }
}
