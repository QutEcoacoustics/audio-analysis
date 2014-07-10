// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoubleSquareArrayExtensions.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace System
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

        public static int RowLength<T>(this T[,] matrix)
        {
            return matrix.GetLength(0);
        }

        public static int ColumnLength<T>(this T[,] matrix)
        {
            return matrix.GetLength(1);
        }


        /// <summary>
        /// Fills a given array with a supplied value.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the given Array.
        /// </typeparam>
        /// <param name="array">
        /// The array to manipulate.
        /// </param>
        /// <param name="value">
        /// The Value to insert.
        /// </param>
        /// <returns>
        /// Returns a reference to the manipulated array.
        /// </returns>
        public static T[,] Fill<T>(this T[,] array, T value)
        {
            for (int i = 0; i < array.GetUpperBound(0); i++)
            {
                for (int j = 0; j < array.GetUpperBound(1); j++)
                {
                    array[i, j] = value;
                }
            }

            return array;
        }

        #endregion
    }
}