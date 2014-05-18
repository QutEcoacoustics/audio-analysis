// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompareFeatures.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the CompareFeatures type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Compare Features.
    /// </summary>
    public class CompareFeatures
    {
        private readonly int featureCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareFeatures"/> class.
        /// </summary>
        /// <param name="featureCount">
        /// The feature count.
        /// </param>
        public CompareFeatures(int featureCount)
        {
            this.featureCount = featureCount;
        }

        /// <summary>
        /// Get the area of overlap between two rectangles.
        /// Point of origin is top left.
        /// </summary>
        /// <param name="refItemX">
        /// The ref Item X.
        /// </param>
        /// <param name="refItemY">
        /// The ref Item Y.
        /// </param>
        /// <param name="refItemWidth">
        /// The ref Item Width.
        /// </param>
        /// <param name="refItemHeight">
        /// The ref Item Height.
        /// </param>
        /// <param name="compareItemX">
        /// The compare Item X.
        /// </param>
        /// <param name="compareItemY">
        /// The compare Item Y.
        /// </param>
        /// <param name="compareItemWidth">
        /// The compare Item Width.
        /// </param>
        /// <param name="compareItemHeight">
        /// The compare Item Height.
        /// </param>
        /// <returns>
        /// Amount of overlap.
        /// </returns>
        public static double GetOverlapArea(
            double refItemX,
            double refItemY,
            double refItemWidth,
            double refItemHeight,
            double compareItemX,
            double compareItemY,
            double compareItemWidth,
            double compareItemHeight)
        {
            // ref item
            if (refItemX < 0)
            {
                throw new ArgumentException("Reference Item had an invalid value.", "refItemX");
            }

            if (refItemY < 0)
            {
                throw new ArgumentException("Reference Item had an invalid value.", "refItemY");
            }

            if (refItemWidth < 0)
            {
                throw new ArgumentException("Reference Item had an invalid value.", "refItemWidth");
            }

            if (refItemHeight < 0)
            {
                throw new ArgumentException("Reference Item had an invalid value.", "refItemHeight");
            }

            // compare item
            if (compareItemX < 0)
            {
                throw new ArgumentException("Compare Item had an invalid value.", "compareItemX");
            }

            if (compareItemY < 0)
            {
                throw new ArgumentException("Compare Item had an invalid value.", "compareItemY");
            }

            if (compareItemWidth < 0)
            {
                throw new ArgumentException("Compare Item had an invalid value.", "compareItemWidth");
            }

            if (compareItemHeight < 0)
            {
                throw new ArgumentException("Compare Item had an invalid value.", "compareItemHeight");
            }

            // get values
            double refItemTop = refItemY;
            double refItemLeft = refItemX;
            double refItemBottom = refItemY + refItemHeight;
            double refItemRight = refItemX + refItemWidth;

            double compareItemTop = compareItemY;
            double compareItemLeft = compareItemX;
            double compareItemBottom = compareItemY + compareItemHeight;
            double compareItemRight = compareItemX + compareItemWidth;

            // check for no overlap
            if (refItemLeft > compareItemRight ||
                refItemRight < compareItemLeft ||
                refItemTop > compareItemBottom ||
                refItemBottom < compareItemTop)
            {
                return 0;
            }

            // there is overlap so calculate it.
            // origin is top left, so we want largest top and largest left, smallest bottom and smallest right
            ////var smallestTop = Math.Min(refItemTop, compareItemTop);
            var largestTop = Math.Max(refItemTop, compareItemTop);
            var smallestRight = Math.Min(refItemRight, compareItemRight);
            ////var largestRight = Math.Max(refItemRight, compareItemRight);
            var smallestBottom = Math.Min(refItemBottom, compareItemBottom);
            ////var largestBottom = Math.Max(refItemBottom, compareItemBottom);
            ////var smallestLeft = Math.Min(refItemLeft, compareItemLeft);
            var largestLeft = Math.Max(refItemLeft, compareItemLeft);

            var overlapArea = Math.Abs(largestTop - smallestBottom) * Math.Abs(smallestRight - largestLeft);

            return overlapArea;
        }

        /// <summary>
        /// Get the percentage of overlap between two rectangles.
        /// Point of origin is top left.
        /// </summary>
        /// <param name="refItemX">
        /// The ref Item X.
        /// </param>
        /// <param name="refItemY">
        /// The ref Item Y.
        /// </param>
        /// <param name="refItemWidth">
        /// The ref Item Width.
        /// </param>
        /// <param name="refItemHeight">
        /// The ref Item Height.
        /// </param>
        /// <param name="compareItemX">
        /// The compare Item X.
        /// </param>
        /// <param name="compareItemY">
        /// The compare Item Y.
        /// </param>
        /// <param name="compareItemWidth">
        /// The compare Item Width.
        /// </param>
        /// <param name="compareItemHeight">
        /// The compare Item Height.
        /// </param>
        /// <returns>
        /// Percentage of overlap.
        /// </returns>
        public static double GetOverlapPercent(
            double refItemX,
            double refItemY,
            double refItemWidth,
            double refItemHeight,
            double compareItemX,
            double compareItemY,
            double compareItemWidth,
            double compareItemHeight)
        {
            var result = GetOverlapArea(
                refItemX,
                refItemY,
                refItemWidth,
                refItemHeight,
                compareItemX,
                compareItemY,
                compareItemWidth,
                compareItemHeight);

            var refArea = refItemWidth * refItemHeight;
            var compareArea = compareItemWidth * compareItemHeight;

            if (refArea < 1)
            {
                throw new ArgumentException("Reference item area was not greater than 0.");
            }

            var percentage = result / refArea;
            return percentage;
        }


        /// <summary>
        /// Compare Items.
        /// </summary>
        /// <param name="refItem">
        /// The ref item.
        /// </param>
        /// <param name="compareItem">
        /// The compare item.
        /// </param>
        /// <param name="features">
        /// The features.
        /// </param>
        /// <typeparam name="T">
        /// Item type.
        /// </typeparam>
        /// <returns>
        /// Returns value between 0 and 1. 1 = identical, 0=opposite.
        /// </returns>
        /// <exception cref="InvalidOperationException">Number of features given does not match expected.</exception>
        public double Compare<T>(T refItem, T compareItem, params Func<T, double>[] features)
        {
            if (features.Length != this.featureCount)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Number of features given ({0}) does not match expected ({1}).",
                        features.Length,
                        this.featureCount));
            }

            // features will stay in the same order (hopefully)
            var refFeatures = CalculateFeatures(refItem, features);
            var compareFeatures = CalculateFeatures(compareItem, features);

            // Euclidean distance calculation
            double sum = 0;

            for (var index = 0; index < refFeatures.Count(); index++)
            {
                sum += SquaredDiff(refFeatures.Skip(index).First(), compareFeatures.Skip(index).First());
            }

            var val = Math.Sqrt(sum);
            return val;
        }

        private static IEnumerable<double> CalculateFeatures<T>(T item, IEnumerable<Func<T, double>> features)
        {
            return features.Select(feature => feature(item));
        }

        private static double SquaredDiff(double a, double b)
        {
            return Math.Pow(b - a, 2);
        }
    }
}