// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Interval.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Range of Min-Max.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared
{
    using System;
    using System.Diagnostics.CodeAnalysis;

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
        Open = 0b0_1,
        LeftClosedRightOpen = 0b0_0,
        LeftOpenRightClosed = 0b1_1,
        Closed = 0b1_0,
#pragma warning restore SA1025 // Code should not contain multiple whitespace in a row

        Exclusive = Open,
        MinimumInclusiveMaximumExclusive = LeftClosedRightOpen,
        MinimumExclusiveMaximumInclusive = LeftOpenRightClosed,
        Inclusive = Closed,

        Default = LeftClosedRightOpen,
    }

    /// <summary>
    /// Represents a interval between two points on the same dimension.
    /// Encoding boundness is left up to the type used.
    /// </summary>
    /// <typeparam name="T">
    /// The type used to represent the points in this interval.
    /// </typeparam>
    public readonly struct Interval<T> : IEquatable<Interval<T>>, IComparable<Interval<T>>
        where T : struct, IComparable<T>
    {
        public Interval(T minimum, T maximum)
        {
            if (minimum.CompareTo(maximum) == 1)
            {
                throw new ArgumentException(
                    $"{nameof(Interval<T>)}'s minimum ({minimum}) must be less than the maximum ({maximum})",
                    nameof(minimum));
            }

            this.Minimum = minimum;
            this.Maximum = maximum;
            this.Topology = Topology.Default;
        }

        public Interval(T minimum, T maximum, Topology topology)
        {
            if (minimum.CompareTo(maximum) == 1)
            {
                throw new ArgumentException(
                    $"{nameof(Interval<T>)}'s minimum ({minimum}) must be less than the maximum ({maximum})",
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

        public bool IsDegenerate => this.IsEmpty;

        public bool IsProper => !this.IsEmpty;

        public bool IsDefault => this.Equals(default);

        public bool IsMinimumInclusive => this.Topology == Topology.LeftClosedRightOpen || this.Topology == Topology.Closed;

        public bool IsMaximumInclusive => this.Topology == Topology.LeftOpenRightClosed || this.Topology == Topology.Closed;

        public bool IsOpen => this.Topology == Topology.Open;

        public bool IsClosed => this.Topology == Topology.Closed;

        public static implicit operator Interval<T>((T Minimum, T Maximum) item)
        {
            return new Interval<T>(item.Minimum, item.Maximum);
        }

        public static implicit operator Interval<T>((T Minimum, T Maximum, Topology Topology) item)
        {
            return new Interval<T>(item.Minimum, item.Maximum, item.Topology);
        }

        /// <summary>
        /// Equals operator.
        /// </summary>
        /// <param name="first">The first interval.</param>
        /// <param name="second">The second interval.</param>
        /// <returns>True if equal, otherwise false.</returns>
        public static bool operator ==(Interval<T> first, Interval<T> second)
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
        public static bool operator !=(Interval<T> first, Interval<T> second)
        {
            return !(first == second);
        }

        public bool Contains(T scalar, Topology type = Topology.Default)
        {
            return ScalarEqualOrGreaterThanAnchor(scalar, this.Minimum, this.IsMinimumInclusive) &&
                   ScalarEqualOrLessThanAnchor(scalar, this.Maximum, this.IsMaximumInclusive);
        }

        public bool Contains(Interval<T> interval)
        {
            return ScalarEqualOrGreaterThanAnchor(
                       interval.Minimum,
                       this.Minimum,
                       this.IsMinimumInclusive || (!this.IsMinimumInclusive && !interval.IsMinimumInclusive))
                   && ScalarEqualOrLessThanAnchor(
                       interval.Maximum,
                       this.Maximum,
                       this.IsMaximumInclusive || (!this.IsMaximumInclusive && !interval.IsMaximumInclusive));
        }

        public bool IntersectsWith(Interval<T> interval)
        {
            return ScalarEqualOrGreaterThanAnchor(
                       interval.Maximum,
                       this.Minimum,
                       interval.IsMaximumInclusive || this.IsMinimumInclusive) &&
                   ScalarEqualOrLessThanAnchor(
                       interval.Minimum,
                       this.Maximum,
                       interval.IsMinimumInclusive || this.IsMaximumInclusive);
        }

        public bool TryGetUnion(Interval<T> interval, out Interval<T> union)
        {
            if (this.IntersectsWith(interval))
            {
                T newMin = this.Minimum.CompareTo(interval.Minimum) < 0 ? this.Minimum : interval.Minimum;
                T newMax = this.Maximum.CompareTo(interval.Maximum) > 0 ? this.Maximum : interval.Maximum;

                union = new Interval<T>(newMin, newMax, this.CombineTopology(interval));
                return true;
            }

            union = default;
            return false;
        }

        public void Deconstruct(out T minimum, out T maximum)
        {
            minimum = this.Minimum;
            maximum = this.Maximum;
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
        public bool Equals(Interval<T> other)
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
            if (obj is null)
            {
                return false;
            }

            return obj is Interval<T> interval && this.Equals(interval);
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
        /// Gets string representation of the Interval.
        /// technially incorrectly representing this value.
        /// </summary>
        /// <returns>
        /// String representation.
        /// </returns>
        public override string ToString()
        {
            var left = this.IsMinimumInclusive ? "[" : "(";
            var right = this.IsMaximumInclusive ? "]" : ")";
            return $"{nameof(Interval<T>)}: {left}{this.Minimum}, {this.Maximum}{right}";
        }

        public int CompareTo(Interval<T> other)
        {
            var minimumComparison = this.Minimum.CompareTo(other.Minimum);

            if (minimumComparison != 0)
            {
                return minimumComparison;
            }

            return this.Maximum.CompareTo(other.Maximum);
        }

        public Topology CombineTopology(Interval<T> second)
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

        private bool IsBothMinimumInclusive(Interval<T> other) => this.IsMinimumInclusive && other.IsMinimumInclusive;

        private bool IsBothMaximumInclusive(Interval<T> other) => this.IsMaximumInclusive && other.IsMaximumInclusive;
    }
}