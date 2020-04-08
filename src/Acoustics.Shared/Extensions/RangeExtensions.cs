// <copyright file="RangeExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

// ReSharper disable once CheckNamespace
namespace Acoustics.Shared
{
    using System;

    public static class RangeExtensions
    {
        /// <summary>
        /// Greedily grow the current range to the new duration without exceeding the limits.
        /// Will grow the range around the center of the current range if possible.
        /// Will not fail, and will return as much range as possible, without exceeding limits.
        /// </summary>
        public static Range<double> Grow(
            this Range<double> range,
            Range<double> limits,
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

            return new Range<double>(newMin, newMax, range.Topology);
        }

        public static double Center(this Range<double> range)
        {
            return range.Minimum + (range.Size() / 2.0);
        }

        public static TimeSpan Center(this Range<TimeSpan> range)
        {
            return range.Minimum.Add(range.Size().Divide(2.0));
        }

        public static double Size(this Range<double> range)
        {
            return Math.Abs(range.Maximum - range.Minimum);
        }

        public static TimeSpan Size(this Range<TimeSpan> range)
        {
            return range.Maximum.Subtract(range.Minimum);
        }

        public static Range<double> Shift(this Range<double> range, double shift)
        {
            return new Range<double>(
                range.Minimum + shift,
                range.Maximum + shift,
                range.Topology);
        }

        public static Range<TimeSpan> Shift(this Range<TimeSpan> range, TimeSpan shift)
        {
            return new Range<TimeSpan>(
                range.Minimum.Add(shift),
                range.Maximum.Add(shift),
                range.Topology);
        }

        public static Range<double> Add(this Range<double> rangeA, Range<double> rangeB)
        {
            // https://en.wikipedia.org/wiki/Interval_arithmetic

            return new Range<double>(
                rangeA.Minimum + rangeB.Minimum,
                rangeA.Maximum + rangeB.Maximum,
                rangeA.CombineTopology(rangeB));
        }

        public static Range<double> Subtract(this Range<double> rangeA, Range<double> rangeB)
        {
            // https://en.wikipedia.org/wiki/Interval_arithmetic

            return new Range<double>(
                rangeA.Minimum - rangeB.Maximum,
                rangeA.Maximum - rangeB.Minimum,
                rangeA.CombineTopology(rangeB));
        }

        public static Range<double> Multiply(this Range<double> rangeA, Range<double> rangeB)
        {
            // https://en.wikipedia.org/wiki/Interval_arithmetic
            double
                a1b1 = rangeA.Minimum * rangeB.Minimum,
                a1b2 = rangeA.Minimum * rangeB.Maximum,
                a2b1 = rangeA.Maximum * rangeB.Minimum,
                a2b2 = rangeA.Maximum * rangeB.Maximum;

            return new Range<double>(
                MathExtensions.Min(a1b1, a1b2, a2b1, a2b2),
                MathExtensions.Max(a1b1, a1b2, a2b1, a2b2),
                rangeA.CombineTopology(rangeB));
        }

        public static Range<double> Divide(this Range<double> rangeA, Range<double> rangeB)
        {
            // https://en.wikipedia.org/wiki/Interval_arithmetic

            return rangeA.Multiply(rangeB.Invert());
        }

        public static Range<double> Invert(this Range<double> range)
        {
            // https://en.wikipedia.org/wiki/Interval_arithmetic
            if (range.Maximum == 0)
            {
                return new Range<double>(double.NegativeInfinity, 1 / range.Minimum);
            }

            if (range.Minimum == 0)
            {
                return new Range<double>(1 / range.Maximum, double.PositiveInfinity);
            }

            return new Range<double>(1 / range.Maximum, 1 / range.Minimum, range.Topology);
        }

        public static Range<T> AsRange<T>(this (T Minimum, T Maximum) pair, Topology topology = Topology.Default)
            where T : struct, IComparable<T>
        {
            return new Range<T>(pair.Minimum, pair.Maximum, topology);
        }

        public static Range<T> To<T>(this T minimum, T maximum, Topology topology = Topology.Default)
            where T : struct, IComparable<T>
        {
            return new Range<T>(minimum, maximum, topology);
        }

        public static Range<T> AsRangeFromZero<T>(this T maximum, Topology topology = Topology.Default)
            where T : struct, IComparable<T>
        {
            return new Range<T>(default, maximum, topology);
        }
    }
}