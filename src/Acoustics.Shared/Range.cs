// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Range.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Range of Min-Max.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared
{
    using System;

    public enum Topology : byte
    {
        /*
         * Our flags are defined like this to ensure the default value is [a,b).
         * The meaning of bit 1 is: Is the left exclusive? (1 yes, 0 no)
         * The meaning of bit 2 is: Is the right inclusive? (1 yes, 0 no)
         *
         * Note bit 1 is on the right.
         */
#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
        Open                = 0b0_1,
        LeftClosedRightOpen = 0b0_0,
        LeftOpenRightClosed = 0b1_1,
        Closed              = 0b1_0,
#pragma warning restore SA1025 // Code should not contain multiple whitespace in a row

        Exclusive = Open,
        MinimumInclusiveMaximumExclusive = LeftClosedRightOpen,
        MinimumExclusiveMaximumInclusive = LeftOpenRightClosed,
        Inclusive = Closed,

        Default = LeftClosedRightOpen,
    }

    /// <summary>
    /// Represents a range between two points on the same dimenson.
    /// This type does not encode any notion of endpoint clusivity - we do not know if a range is left-open, right-open,
    /// open, or closed.
    /// </summary>
    /// <typeparam name="T">
    /// The type used to represent the points in this range.
    /// </typeparam>
    public readonly struct Range<T> : IEquatable<Range<T>>, IComparable<Range<T>>
        where T : struct, IComparable<T>
    {
        public Range(T minimum, T maximum)
        {
            if (minimum.CompareTo(maximum) == 1)
            {
                throw new ArgumentException(
                    $"Range's minimum ({minimum}) must be less than the maximum ({maximum})",
                    nameof(minimum));
            }

            this.Minimum = minimum;
            this.Maximum = maximum;
            this.Topology = Topology.Default;
        }

        public Range(T minimum, T maximum, Topology topology)
        {
            if (minimum.CompareTo(maximum) == 1)
            {
                throw new ArgumentException(
                    $"Range's minimum ({minimum}) must be less than the maximum ({maximum})",
                    nameof(minimum));
            }

            this.Minimum = minimum;
            this.Maximum = maximum;
            this.Topology = topology;
        }

        /// <summary>
        /// Gets the Minimum.
        /// </summary>
        public T Minimum { get; }

        /// <summary>
        /// Gets the Maximum.
        /// </summary>
        public T Maximum { get; }

        /// <summary>
        /// Gets the type of topology this interval has.
        /// </summary>
        public Topology Topology { get; }

        public bool IsEmpty => this.Minimum.Equals(this.Maximum);

        public bool IsDefault => this.Equals(default(Range<T>));

        public bool IsMinimumInclusive => this.Topology == Topology.LeftClosedRightOpen || this.Topology == Topology.Closed;

        public bool IsMaximumInclusive => this.Topology == Topology.LeftOpenRightClosed || this.Topology == Topology.Closed;

        /// <summary>
        /// Equals operator.
        /// </summary>
        /// <param name="first">The first range.</param>
        /// <param name="second">The second range.</param>
        /// <returns>True if equal, otherwise false.</returns>
        public static bool operator ==(Range<T> first, Range<T> second)
        {
            return first.Equals(second);
        }

        /// <summary>
        /// Not equal operator.
        /// </summary>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <returns>
        /// True if not equals, otherwise false.
        /// </returns>
        public static bool operator !=(Range<T> first, Range<T> second)
        {
            return !(first == second);
        }

        public bool Contains(T scalar, Topology type = Topology.Default)
        {
            return ScalarEqualOrGreaterThanAnchor(scalar, this.Minimum, this.IsMinimumInclusive) &&
                   ScalarEqualOrLessThanAnchor(scalar, this.Maximum, this.IsMaximumInclusive);
        }

        public bool Contains(Range<T> range)
        {
            return ScalarEqualOrGreaterThanAnchor(
                       range.Minimum,
                       this.Minimum,
                       this.IsMinimumInclusive || (!this.IsMinimumInclusive && !range.IsMinimumInclusive))
                   && ScalarEqualOrLessThanAnchor(
                       range.Maximum,
                       this.Maximum,
                       this.IsMaximumInclusive || (!this.IsMaximumInclusive && !range.IsMaximumInclusive));
        }

        public bool IntersectsWith(Range<T> range)
        {
            return ScalarEqualOrGreaterThanAnchor(
                       range.Maximum,
                       this.Minimum,
                       range.IsMaximumInclusive || this.IsMinimumInclusive) &&
                   ScalarEqualOrLessThanAnchor(
                       range.Minimum,
                       this.Maximum,
                       range.IsMinimumInclusive || this.IsMaximumInclusive);
        }

        public bool TryGetUnion(Range<T> range, out Range<T> union)
        {
            if (this.IntersectsWith(range))
            {
                T newMin = this.Minimum.CompareTo(range.Minimum) < 0 ? this.Minimum : range.Minimum;
                T newMax = this.Maximum.CompareTo(range.Maximum) > 0 ? this.Maximum : range.Maximum;

                union = new Range<T>(newMin, newMax, this.CombineTopology(range));
                return true;
            }

            union = default(Range<T>);
            return false;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        public bool Equals(Range<T> other)
        {
            return this.Minimum.Equals(other.Minimum) && this.Maximum.Equals(other.Maximum) && this.Topology == other.Topology;
        }

        /// <summary>
        /// Equals another instance.
        /// </summary>
        /// <param name="obj">
        /// The obj to compare.
        /// </param>
        /// <returns>
        /// True if equal, otherwise false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Range<T> range && this.Equals(range);
        }

        /// <summary>
        /// Get Hash Code.
        /// </summary>
        /// <returns>
        /// Hash Code.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Minimum.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Maximum.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)this.Topology;
                return hashCode;
            }
        }

        /// <summary>
        /// Gets string representation of the Range.
        /// Note: our range has no notion of inclusive or exclusive endpoints, thus the square bracket notation is
        /// technially incorrectly representing this value.
        /// </summary>
        /// <returns>
        /// String representation.
        /// </returns>
        public override string ToString()
        {
            var left = this.IsMinimumInclusive ? "[" : "(";
            var right = this.IsMaximumInclusive ? "]" : ")";
            return $"Range: {left}{this.Minimum}, {this.Maximum}{right}";
        }

        public int CompareTo(Range<T> other)
        {
            var minimumComparison = this.Minimum.CompareTo(other.Minimum);

            if (minimumComparison != 0)
            {
                return minimumComparison;
            }

            return this.Maximum.CompareTo(other.Maximum);
        }

        public Topology CombineTopology(Range<T> second)
        {
            if (this.Topology == second.Topology)
            {
                return this.Topology;
            }

            bool min = this.IsBothMinimumInclusive(second);
            bool max = this.IsBothMaximumInclusive(second);

            if (min && max)
            {
                return Topology.Inclusive;
            }
            else if (min)
            {
                return Topology.MinimumInclusiveMaximumExclusive;
            }
            else if (max)
            {
                return Topology.MinimumExclusiveMaximumInclusive;
            }

            return Topology.Exclusive;
        }

        private static bool ScalarEqualOrGreaterThanAnchor(T scalar, T anchor, bool isMinimumInclusive)
        {
            int comparison = anchor.CompareTo(scalar);
            bool result = false;
            switch (comparison)
            {
                case -1:
                    {
                        result = true;
                        break;
                    }

                case 0:
                    {
                        result = isMinimumInclusive;
                        break;
                    }

                case 1:
                    {
                        break;
                    }
            }

            return result;
        }

        private static bool ScalarEqualOrLessThanAnchor(T scalar, T anchor, bool isMaximumInclusive)
        {
            int comparison = anchor.CompareTo(scalar);
            bool result = false;
            switch (comparison)
            {
                case -1:
                {
                    break;
                }

                case 0:
                {
                    result = isMaximumInclusive;
                    break;
                }

                case 1:
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        private bool IsBothMinimumInclusive(Range<T> other) => this.IsMinimumInclusive && other.IsMinimumInclusive;

        private bool IsBothMaximumInclusive(Range<T> other) => this.IsMaximumInclusive && other.IsMaximumInclusive;

    }
}
