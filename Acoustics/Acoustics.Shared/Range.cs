// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Range.cs" company="MQUTeR">
//   -
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
    /// Range of Min-Max.
    /// </summary>
    /// <typeparam name="T">
    /// Type of range.
    /// </typeparam>
    public class Range<T> : IEquatable<Range<T>> where T : struct
    {
        /// <summary>
        /// Gets or sets Minimum.
        /// </summary>
        public T Minimum { get; set; }

        /// <summary>
        /// Gets or sets Maximum.
        /// </summary>
        public T Maximum { get; set; }

        /// <summary>
        /// Equals operator.
        /// </summary>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <returns>
        /// True if equsl, otherwise false.
        /// </returns>
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
            if ((object)other == null)
            {
                return false;
            }

            return EqualityComparer<T>.Default.Equals(this.Maximum, other.Maximum) &&
                   EqualityComparer<T>.Default.Equals(this.Minimum, other.Minimum);
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
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast return false.
            var p = obj as Range<T>;

            return (object)p != null && Equals(p);
        }

        /// <summary>
        /// Get Hash Code.
        /// </summary>
        /// <returns>
        /// Hash Code.
        /// </returns>
        public override int GetHashCode()
        {
            return this.Minimum.GetHashCode() ^ this.Maximum.GetHashCode();
        }

        /// <summary>
        /// Get string representation.
        /// </summary>
        /// <returns>
        /// String representation.
        /// </returns>
        public override string ToString()
        {
            return this.Minimum + " - " + this.Maximum;
        }

        /// <summary>
        /// Get count number of randoms ranges of rangeAmount between rangeOverMin and rangeOverMax.
        /// </summary>
        /// <param name="rangeOverMin">
        /// The range over min.
        /// </param>
        /// <param name="rangeOverMax">
        /// The range over max.
        /// </param>
        /// <param name="rangeAmount">
        /// The range amount.
        /// </param>
        /// <param name="count">
        /// The number of ranges.
        /// </param>
        /// <returns>
        /// Enumerable of ranges.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The rangeOverMin must be less than rangeOverMax.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The rangeAmount must be larger than the difference between rangeOverMin and rangeOverMax.
        /// </exception>
        public static IEnumerable<Range<int>> GetRandomRanges(int rangeOverMin, int rangeOverMax, int rangeAmount, int count)
        {
            if (rangeOverMin >= rangeOverMax)
            {
                throw new ArgumentException("The rangeOverMin must be less than rangeOverMax.", "rangeOverMin");
            }

            if (rangeAmount > rangeOverMax - rangeOverMin)
            {
                throw new ArgumentException("The rangeAmount must be less than or equal to the difference between rangeOverMin and rangeOverMax.", "rangeAmount");
            }

            var random = new Random();

            for (var index = 0; index < count; index++)
            {
                var randomPoint = random.Next(rangeOverMin, rangeOverMax);
                var lowerRange = randomPoint - rangeAmount;

                if (lowerRange < 0)
                {
                    lowerRange = 0;
                }

                var upperRange = lowerRange + rangeAmount;

                if (upperRange > rangeOverMax)
                {
                    upperRange = rangeOverMax;
                    lowerRange = rangeOverMax - rangeAmount;
                }

                yield return new Range<int> { Minimum = lowerRange, Maximum = upperRange };
            }
        }
    }
}
