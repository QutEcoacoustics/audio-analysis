﻿// --------------------------------------------------------------------------------------------------------------------
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
    using System.Collections.Generic;

    /// <summary>
    /// Represents a range between two points on the same dimenson.
    /// This type does not encode any notion of endpoint clusivity - we do not know if a range is left-open, right-open,
    /// open, or closed.
    /// </summary>
    /// <typeparam name="T">
    /// The type used to represent the points in this range.
    /// </typeparam>
    public struct Range<T> : IEquatable<Range<T>>, IComparable<Range<T>>
        where T : struct, IComparable<T>
    {
        public Range(T minimum, T maximum)
        {
            this.Minimum = minimum;
            this.Maximum = maximum;
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
            return this.Minimum.Equals(other.Minimum) && this.Maximum.Equals(other.Maximum);
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

            return obj is Range<T> && this.Equals((Range<T>)obj);
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
                return (this.Minimum.GetHashCode() * 397) ^ this.Maximum.GetHashCode();
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
            return $"Range: [{this.Minimum}, {this.Maximum}]";
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
    }
}
