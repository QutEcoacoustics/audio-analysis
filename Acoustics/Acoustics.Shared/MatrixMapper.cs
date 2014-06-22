// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatrixMapper.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the TwoDimensionalArray type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Acoustics.Shared.Extensions;

    public enum TwoDimensionalArray
    {
        ColumnMajor,
        RowMajor
    }

    internal class MatrixMapper<T, TBase> : IEnumerable<int>
    {
        private readonly TwoDimensionalArray dimensionality;
        private readonly IEnumerable<T[]> enumerableMatrix;
        private readonly bool isMatrix;
        private readonly T[,] matrix;
        private T[] current;

        public MatrixMapper(T[,] matrix, TwoDimensionalArray dimensionality)
        {
            this.matrix = matrix;
            this.dimensionality = dimensionality;
            this.Rows = TwoDimensionalArray.RowMajor == dimensionality ? matrix.RowLength() : matrix.ColumnLength();
            this.Columns = TwoDimensionalArray.ColumnMajor == dimensionality ? matrix.ColumnLength() : matrix.RowLength();

            this.isMatrix = true;
        }

        public MatrixMapper(IEnumerable<T[]> matrix)
        {
            this.enumerableMatrix = matrix;
            this.Columns = this.enumerableMatrix.First().Length;
            this.Rows = null;

            this.isMatrix = false;
        }

        public MatrixMapper(IEnumerable<TBase> matrix, Func<TBase, T> selector)
        {
            // here
        }

        public int Columns { get; private set; }

        public int? Rows { get; private set; }

        public T this[int i, int j]
        {
            get
            {
                if (this.isMatrix)
                {
                    return this.current[j];
                }

                if (this.dimensionality == TwoDimensionalArray.RowMajor)
                {
                    return this.matrix[i, j];
                }

                if (this.dimensionality == TwoDimensionalArray.ColumnMajor)
                {
                    return this.matrix[j, i];
                }

                throw new Exception();
            }
        }


        public IEnumerator<int> GetEnumerator()
        {
            if (this.isMatrix)
            {
                for (int i = 0; i < this.Rows; i++)
                {
                    yield return i;
                }
            }
            else
            {
                int rowCounter = -1;
                foreach (var currentItem in this.enumerableMatrix)
                {
                    rowCounter++;
                    this.current = currentItem;
                    yield return rowCounter;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}