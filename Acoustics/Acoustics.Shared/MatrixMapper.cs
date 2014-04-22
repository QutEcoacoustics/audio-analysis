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
        private readonly T[,] _matrix;
        private readonly TwoDimensionalArray _dimensionality;
        private readonly IEnumerable<T[]> _enumerableMatrix;
        private readonly bool _isMatrix;
        private T[] _current;

        public MatrixMapper(T[,] matrix, TwoDimensionalArray dimensionality)
        {
            _matrix = matrix;
            _dimensionality = dimensionality;
            Rows = TwoDimensionalArray.RowMajor == dimensionality ? matrix.RowLength() : matrix.ColumnLength();
            Columns = TwoDimensionalArray.ColumnMajor == dimensionality ? matrix.ColumnLength() : matrix.RowLength();

            _isMatrix = true;
        }

        public MatrixMapper(IEnumerable<T[]> matrix)
        {
            _enumerableMatrix = matrix;
            Columns = _enumerableMatrix.First().Length;
            Rows = null;

            _isMatrix = false;
        }

        public int Columns { get; private set; }

        public int? Rows { get; private set; }

        public T this[int i, int j]
        {
            get
            {
                if (_isMatrix)
                {
                    return _current[j];
                }

                if (_dimensionality == TwoDimensionalArray.RowMajor)
                {
                    return _matrix[i, j];
                }

                if (_dimensionality == TwoDimensionalArray.ColumnMajor)
                {
                    return _matrix[j, i];
                }

                throw new Exception();
            }
        }


        public IEnumerator<int> GetEnumerator()
        {
            if (_isMatrix)
            {
                for (int i = 0; i < Rows; i++)
                {
                    yield return i;
                }
            }
            else
            {
                int rowCounter = -1;
                foreach (var current in _enumerableMatrix)
                {
                    rowCounter++;
                    _current = current;
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
