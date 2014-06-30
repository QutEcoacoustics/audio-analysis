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

    internal abstract class MatrixMapper<TMatrix> : IEnumerable<int>
    {
        public abstract int Columns { get; protected set; }

        public abstract TMatrix[] Current { get; set; }

        public abstract TMatrix this[int i, int j] { get; }

        public abstract IEnumerator<int> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    internal class ObjectArrayMapper<TBase, TMatrix> : MatrixMapper<TMatrix>
    {
        private readonly IEnumerable<TBase> objectMatrix;
        private readonly Func<TBase, TMatrix[]> selector;


        public ObjectArrayMapper(IEnumerable<TBase> matrix, Func<TBase, TMatrix[]> selector)
        {
            // here
            this.objectMatrix = matrix;
            this.selector = selector;
            this.Columns = selector(this.objectMatrix.First()).Length;
            ////this.isMatrix = false;
        }

        public override IEnumerator<int> GetEnumerator()
        {

                int rowCounter = -1;
                foreach (var currentItem in this.objectMatrix)
                {
                    rowCounter++;
                    this.Current = this.selector(currentItem);
                    yield return rowCounter;
                }
        }

        public override int Columns { get; protected set; }

        public override TMatrix[] Current { get; set; }

        public override TMatrix this[int i, int j]
        {
            get
            {
                return this.Current[j];
                
                throw new Exception();
            }
        }
    }

    internal class EnumerableMapper<TMatrix> : MatrixMapper<TMatrix>
    {
        private readonly IEnumerable<TMatrix[]> enumerableMatrix;
        public EnumerableMapper(IEnumerable<TMatrix[]> matrix)
        {
            this.enumerableMatrix = matrix;
            this.Columns = this.enumerableMatrix.First().Length;
        }

        public override IEnumerator<int> GetEnumerator()
        {
            int rowCounter = -1;
            foreach (var currentItem in this.enumerableMatrix)
            {
                rowCounter++;
                this.Current = currentItem;
                yield return rowCounter;
            }

        }

        public override int Columns { get; protected set; }

        public override TMatrix[] Current { get; set; }

        public override TMatrix this[int i, int j]
        {
            get
            {
                return this.Current[j];

                throw new Exception();
            }
        }
    }

    internal class TwoDimArrayMapper<TMatrix> : MatrixMapper<TMatrix>
    {
        private readonly TwoDimensionalArray dimensionality;
        private readonly TMatrix[,] matrix;
        public int? Rows { get; private set; }

        public TwoDimArrayMapper(TMatrix[,] matrix, TwoDimensionalArray dimensionality)
        {
            this.matrix = matrix;
            this.dimensionality = dimensionality;
            this.Rows = TwoDimensionalArray.RowMajor == dimensionality ? matrix.RowLength() : matrix.ColumnLength();
            this.Columns = TwoDimensionalArray.ColumnMajor == dimensionality
                               ? matrix.ColumnLength()
                               : matrix.RowLength();
        }

        public override IEnumerator<int> GetEnumerator()
        {
            for (int i = 0; i < this.Rows; i++)
            {
                yield return i;
            }
        }

        public override int Columns { get; protected set; }

        public override TMatrix[] Current { get; set; }

        public override TMatrix this[int i, int j]
        {
            get
            {
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
    }
}