// <copyright file="RangeTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acoustics.Shared
{
    public static class RangeExtensions
    {
        /// <summary>
        /// Greedily grow the current range to the new duration without exceeding the limits.
        /// Will grow the range around the center of the current range if possible.
        /// Will not fail, and will return as much range as possible, without exceeding limits or duration.
        /// </summary>
        /// <param name="range"></param>
        /// <param name="limits"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static Range<double> Grow(this Range<double> range, Range<double> limits, double duration)
        {
            var limitMagnitude = limits.Magnitude();

            if (duration > limitMagnitude)
            {
                duration = limitMagnitude;
            }

            var rangeCenter = range.Center();

            var newDurationHalf = duration / 2.0;
            var newMin = rangeCenter - newDurationHalf;
            var newMax = rangeCenter + newDurationHalf;

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

        public static Range<T> AsRangeFromZero<T>(this T maximum)
            where T : struct, IComparable<T>
        {
            return new Range<T>(default(T), maximum);
        }
    }
}
