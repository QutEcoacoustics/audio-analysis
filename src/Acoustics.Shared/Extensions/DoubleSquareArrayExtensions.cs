// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoubleSquareArrayExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace System
{
    using System;
    using Drawing;
    using Acoustics.Shared.Contracts;

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
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] = value;
                }
            }

            return array;
        }

        public static U[,] Map<T, U>(this T[,] array, Func<T, U> f)
        {
            var jUpper = array.GetLength(1);
            var iUpper = array.GetLength(0);

            var result = new U[iUpper, jUpper];
            for (int i = 0; i < iUpper; i++)
            {
                for (int j = 0; j < jUpper; j++)
                {
                    result[i, j] = f(array[i,j]);
                }
            }

            return result;
        }

        public static U Fold<T, U>(this T[,] array, Func<U, T, U> f, U seed)
        {
            var jUpper = array.GetLength(1);
            var iUpper = array.GetLength(0);

            var result = new U[iUpper, jUpper];
            for (int i = 0; i < iUpper; i++)
            {
                for (int j = 0; j < jUpper; j++)
                {
                    seed = f(seed, array[i, j]);
                }
            }

            return seed;
        }

        #endregion

        /// <summary>
        /// returns the min and max values in a matrix of doubles.
        /// </summary>
        /// <param name="data">
        /// The audio data.
        /// </param>
        /// <param name="min">
        /// The min value.
        /// </param>
        /// <param name="max">
        /// The max value.
        /// </param>
        public static void MinMax(this double[,] data, out double min, out double max)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            min = data[0, 0];
            max = data[0, 0];

            for (int i = 1; i < rows; i++)
            {
                for (int j = 1; j < cols; j++)
                {
                    if (data[i, j] < min)
                    {
                        min = data[i, j];
                    }
                    else if (data[i, j] > max)
                    {
                        max = data[i, j];
                    }
                }
            }
        }
    }
}