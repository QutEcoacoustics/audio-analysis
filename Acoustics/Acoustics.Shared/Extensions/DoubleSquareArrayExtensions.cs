// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoubleSquareArrayExtensions.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared.Extensions
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Drawing;

    /// <summary>
    /// The double square array extensions.
    /// </summary>
    public static class DoubleSquareArrayExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The point intersect.
        /// </summary>
        /// <param name="array">
        /// The array.
        /// </param>
        /// <param name="point">
        /// The point.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool PointIntersect(this double[,] array, Point point)
        {
            Contract.Requires(array.Rank == 2);

            int dimX = array.GetLength(0);
            int dimY = array.GetLength(1);

            return point.X >= array.GetLowerBound(0) && point.X < dimX && point.Y >= array.GetLowerBound(1) && point.Y < dimY;
        }

        /// <summary>
        /// The point intersect.
        /// </summary>
        /// <param name="array">
        /// The array.
        /// </param>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <param name="y">
        /// The y.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool PointIntersect(this double[,] array, int x, int y)
        {
            Contract.Requires(array.Rank == 2);

            int dimX = array.GetLength(0);
            int dimY = array.GetLength(1);


            return x >= array.GetLowerBound(0) && x < dimX && y >= array.GetLowerBound(1) && y < dimY;
        }

        #endregion
    }
}