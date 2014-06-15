using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Acoustics.Shared.Extensions;

namespace Acoustics.Shared
{
    public enum TwoDimensionalArray
    {
        ColumnMajor,
        RowMajor
    }

    internal class MatrixMapper<T> : IEnumerable<int>
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
            Rows = TwoDimensionalArray.RowMajor == dimensionality ? matrix.RowLength() : matrix.ColumnLength();
            Columns = TwoDimensionalArray.ColumnMajor == dimensionality ? matrix.ColumnLength() : matrix.RowLength();

            isMatrix = true;
        }

        public MatrixMapper(IEnumerable<T[]> matrix)
        {
            enumerableMatrix = matrix;
            Columns = enumerableMatrix.First().Length;
            Rows = null;

            isMatrix = false;
        }

        public int Columns { get; private set; }

        public int? Rows { get; private set; }

        public T this[int i, int j]
        {
            get
            {
                if (isMatrix)
                {
                    return current[j];
                }

                if (dimensionality == TwoDimensionalArray.RowMajor)
                {
                    return matrix[i, j];
                }

                if (dimensionality == TwoDimensionalArray.ColumnMajor)
                {
                    return matrix[j, i];
                }

                throw new Exception();
            }
        }


        public IEnumerator<int> GetEnumerator()
        {
            if (isMatrix)
            {
                for (int i = 0; i < Rows; i++)
                {
                    yield return i;
                }
            }
            else
            {
                int rowCounter = -1;
                foreach (var currentItem in enumerableMatrix)
                {
                    rowCounter++;
                    this.current = currentItem;
                    yield return rowCounter;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}