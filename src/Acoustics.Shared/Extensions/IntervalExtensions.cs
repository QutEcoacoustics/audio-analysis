// <copyright file="IntervalExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

// ReSharper disable once CheckNamespace
namespace Acoustics.Shared
{
    using System;
    using System.Runtime.CompilerServices;

    public static class IntervalExtensions
    {
        /// <summary>
        /// Greedily grow the current interval to the new duration without exceeding the limits.
        /// Will grow the range around the center of the current range if possible.
        /// Will not fail, and will return as much range as possible, without exceeding limits.
        /// </summary>
        public static Interval<double> Grow(
            this Interval<double> range,
            Interval<double> limits,
            double growAmount,
            int? roundDigits = null)
        {
            var limitMagnitude = limits.Size();

            var newMagnitude = growAmount + range.Size();
            if (newMagnitude > limitMagnitude)
            {
                return limits;
            }

            var halfGrow = growAmount / 2.0;

            var newMin = range.Minimum - halfGrow;
            var newMax = range.Maximum + halfGrow;

            if (newMin < limits.Minimum)
            {
                newMax = limits.Minimum + newMagnitude;
                newMin = limits.Minimum;
            }
            else if (newMax > limits.Maximum)
            {
                newMin = limits.Maximum - newMagnitude;
                newMax = limits.Maximum;
            }

            // round the lower bound
            if (roundDigits.HasValue)
            {
                newMin = Math.Round(newMin, roundDigits.Value, MidpointRounding.AwayFromZero);
            }

            // round the upper bound
            if (roundDigits.HasValue)
            {
                newMax = Math.Round(newMax, roundDigits.Value, MidpointRounding.AwayFromZero);
            }

            return new Interval<double>(newMin, newMax, range.Topology);
        }

        public static double Center(this Interval<double> range)
        {
            return range.Minimum + (range.Size() / 2.0);
        }

        public static TimeSpan Center(this Interval<TimeSpan> range)
        {
            return range.Minimum.Add(range.Size().Divide(2.0));
        }

        public static double Size(this Interval<double> range)
        {
            return Math.Abs(range.Maximum - range.Minimum);
        }

        public static TimeSpan Size(this Interval<TimeSpan> range)
        {
            return range.Maximum.Subtract(range.Minimum);
        }

        public static Interval<double> Shift(this Interval<double> range, double shift)
        {
            return new Interval<double>(
                range.Minimum + shift,
                range.Maximum + shift,
                range.Topology);
        }

        public static Interval<TimeSpan> Shift(this Interval<TimeSpan> range, TimeSpan shift)
        {
            return new Interval<TimeSpan>(
                range.Minimum.Add(shift),
                range.Maximum.Add(shift),
                range.Topology);
        }

        public static Interval<double> Add(this Interval<double> rangeA, Interval<double> rangeB)
        {
            // https://en.wikipedia.org/wiki/Interval_arithmetic

            return new Interval<double>(
                rangeA.Minimum + rangeB.Minimum,
                rangeA.Maximum + rangeB.Maximum,
                rangeA.CombineTopology(rangeB));
        }

        public static Interval<double> Subtract(this Interval<double> rangeA, Interval<double> rangeB)
        {
            // https://en.wikipedia.org/wiki/Interval_arithmetic

            return new Interval<double>(
                rangeA.Minimum - rangeB.Maximum,
                rangeA.Maximum - rangeB.Minimum,
                rangeA.CombineTopology(rangeB));
        }

        public static Interval<double> Multiply(this Interval<double> rangeA, Interval<double> rangeB)
        {
            // https://en.wikipedia.org/wiki/Interval_arithmetic
            double
                a1b1 = rangeA.Minimum * rangeB.Minimum,
                a1b2 = rangeA.Minimum * rangeB.Maximum,
                a2b1 = rangeA.Maximum * rangeB.Minimum,
                a2b2 = rangeA.Maximum * rangeB.Maximum;

            return new Interval<double>(
                MathExtensions.Min(a1b1, a1b2, a2b1, a2b2),
                MathExtensions.Max(a1b1, a1b2, a2b1, a2b2),
                rangeA.CombineTopology(rangeB));
        }

        public static Interval<double> Divide(this Interval<double> rangeA, Interval<double> rangeB)
        {
            // https://en.wikipedia.org/wiki/Interval_arithmetic

            return rangeA.Multiply(rangeB.Invert());
        }

        public static Interval<double> Invert(this Interval<double> range)
        {
            // https://en.wikipedia.org/wiki/Interval_arithmetic
            if (range.Maximum == 0)
            {
                return new Interval<double>(double.NegativeInfinity, 1 / range.Minimum);
            }

            if (range.Minimum == 0)
            {
                return new Interval<double>(1 / range.Maximum, double.PositiveInfinity);
            }

            return new Interval<double>(1 / range.Maximum, 1 / range.Minimum, range.Topology);
        }

        /// <summary>
        /// Normalizes a value as a unit value given the bounds of an interval.
        /// </summary>
        /// <param name="range">The interval to use as the bounds.</param>
        /// <param name="value">The value to normalize.</param>
        /// <returns>A value scaled to [0,1]. The value may exceed the bounds [0,1]; i.e. the value is not clamped.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double NormalizeValue(this Interval<double> range, double value)
        {
            return (value - range.Minimum) / (range.Maximum - range.Minimum);
        }

        /// <summary>
        /// Normalizes a value as a unit value given the bounds of an interval.
        /// </summary>
        /// <param name="value">The value to normalize.</param>
        /// <param name="range">The interval to use as the bounds.</param>
        /// <returns>A value scaled to [0,1]. The value may exceed the bounds [0,1]; i.e. the value is not clamped.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Normalize(this double value, Interval<double> range)
        {
            return (value - range.Minimum) / (range.Maximum - range.Minimum);
        }

        /// <summary>
        /// Restricts a <see cref="double"/> to be within a specified range.
        /// </summary>
        /// <param name="value">The The value to clamp.</param>
        /// <param name="range">The interval to clamp the value to.</param>
        /// <returns>
        /// The <see cref="double"/> representing the clamped value.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(this double value, Interval<double> range)
        {
            if (value >= range.Maximum)
            {
                return range.Maximum;
            }

            if (value <= range.Minimum)
            {
                return range.Minimum;
            }

            return value;
        }

        /// <summary>
        /// Restricts a <see cref="double"/> to be within a specified range.
        /// </summary>
        /// <param name="range">The interval to clamp the value to.</param>
        /// <param name="value">The The value to clamp.</param>
        /// <returns>
        /// The <see cref="double"/> representing the clamped value.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ClampvValue(this Interval<double> range, double value)
        {
            if (value >= range.Maximum)
            {
                return range.Maximum;
            }

            if (value <= range.Minimum)
            {
                return range.Minimum;
            }

            return value;
        }

        public static Interval<T> AsInterval<T>(this (T Minimum, T Maximum) pair, Topology topology = default)
            where T : struct, IComparable<T>, IFormattable
        {
            return new Interval<T>(pair.Minimum, pair.Maximum, topology);
        }

        public static Interval<T> AsIntervalTo<T>(this T minimum, T maximum, Topology topology = default)
            where T : struct, IComparable<T>, IFormattable
        {
            return new Interval<T>(minimum, maximum, topology);
        }

        public static Interval<T> AsIntervalFromZero<T>(this T maximum, Topology topology = default)
            where T : struct, IComparable<T>, IFormattable
        {
            return new Interval<T>(default, maximum, topology);
        }
    }
}