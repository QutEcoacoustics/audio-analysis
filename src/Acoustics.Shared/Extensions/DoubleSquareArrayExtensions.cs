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
    using Drawing;
    using System;
    using Acoustics.Shared.Contracts;

    /// <summary>
    /// The double square array extensions.
    /// </summary>
    public static class DoubleSquareArrayExtensions
    {
        public static double Average(this double[,] matrix)
        {
            var total = 0.0;
            var count = RowLength(matrix) * ColumnLength(matrix);

            for (int i = 0; i < RowLength(matrix); i++)
            {
                for (int j = 0; j < ColumnLength(matrix); j++)
                {
                    total += matrix[i, j];
                }
            }

            return total / count;
        }

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

        public static int LastRowIndex<T>(this T[,] matrix)
        {
            return RowLength(matrix) - 1;
        }

        public static int LastColumnIndex<T>(this T[,] matrix)
        {
            return ColumnLength(matrix) - 1;
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
            var width = array.GetLength(0);
            var height = array.GetLength(1);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
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
                    result[i, j] = f(array[i, j]);
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

        /// <summary>
        /// returns an empty matrix with the same number of rows and columns of the input matrix.
        /// </summary>
        public static T[,] EmptyCopy<T>(this T[,] matrix)
        {
            return new T[matrix.GetLength(0), matrix.GetLength(1)];
        }

        /// <summary>
        /// retrieving a full column of a matrix
        /// columnIndex is the column we want to access
        /// </summary>
        public static T[] GetColumn<T>(this T[,] matrix, int columnIndex)
        {
            T[] column = new T[matrix.GetLength(0)];
            for (int row = 0; row < matrix.GetLength(0); row++)
            {
                column[row] = matrix[row, columnIndex];
            }

            return column;
        }

        /// <summary>
        /// retrieving a full row of a matrix
        /// rowIndex is the row we want to access
        /// </summary>
        public static T[] GetRow<T>(this T[,] matrix, int rowIndex)
        {
            T[] row = new T[matrix.GetLength(1)];
            for (int column = 0; column < matrix.GetLength(1); column++)
            {
                row[column] = matrix[rowIndex, column];
            }

            return row;
        }

        /// <summary>
        /// The merging direction when adding a 2D-array to another 2D-array.
        /// </summary>
        public enum MergingDirection
        {
            Row = 0,

            Column = 1,
        }

        /// <summary>
        /// adding a 2D-array to another 2D-array either by "column" or by "row"
        /// </summary>

        public static void AddToArray<T>(this T[,] result, T[,] array, MergingDirection mergingDirection, int start = 0)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if (mergingDirection == MergingDirection.Row)
                    {
                        result[i + start, j] = array[i, j];
                    }
                    else
                    {
                        if (mergingDirection == MergingDirection.Column)
                        {
                            result[i, j + start] = array[i, j];
                        }
                    }
                }
            }
        }
    }
}