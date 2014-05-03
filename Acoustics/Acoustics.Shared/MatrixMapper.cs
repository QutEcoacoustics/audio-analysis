using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly T[,] matrix;
        private readonly TwoDimensionalArray dimensionality;
        private readonly IEnumerable<T[]> enumerableMatrix;
        private readonly bool isMatrix;
        private T[] current;

        public MatrixMapper(T[,] matrix, TwoDimensionalArray dimensionality)
        {
            this.matrix = matrix;
            this.dimensionality = dimensionality;
            Rows = TwoDimensionalArray.RowMajor == dimensionality ? matrix.RowLength() : matrix.ColumnLength();
            Columns = TwoDimensionalArray.ColumnMajor == dimensionality ? matrix.ColumnLength() : matrix.RowLength();

            this.isMatrix = true;
        }

        public MatrixMapper(IEnumerable<T[]> matrix)
        {
            this.enumerableMatrix = matrix;
            Columns = this.enumerableMatrix.First().Length;
            Rows = null;

            this.isMatrix = false;
        }

        public int Columns { get; private set; }

        public int? Rows { get; private set; }

        public T this[int i, int j]
        {
            get
            {
                if (this.isMatrix)
                {
                    // i is ignored, internal state of object relied on
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
                for (int i = 0; i < Rows; i++)
                {
                    yield return i;
                }
            }
            else
            {
                int rowCounter = -1;
                foreach (var current in this.enumerableMatrix)
                {
                    rowCounter++;
                    this.current = current;
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
