// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatrixMapper.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
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

    using Extensions;

    public enum TwoDimensionalArray
    {
        /// <summary>
        /// This transform should be equivalent to RotateMatrix90DegreesClockwise
        /// <example>
        /// 1 | 2 --> 2 | 4 | 6
        /// 3 | 4     1 | 3 | 5
        /// 5 | 6
        /// </example>
        /// </summary>
        Rotate90AntiClockWise = 3,

        /// <summary>
        /// This is effectively a transpose
        /// <code>
        /// 1 | 2 --> 1 | 3 | 5
        /// 3 | 4     2 | 4 | 6
        /// 5 | 6
        /// </code>
        /// </summary>
        Transpose = 2,

        /// <summary>
        /// This transform should be equivalent to RotateMatrix90DegreesClockwise
        /// <example>
        /// 1 | 2 --> 5 | 3 | 1
        /// 3 | 4     6 | 4 | 2
        /// 5 | 6
        /// </example>
        /// </summary>
        Rotate90ClockWise = 1,

        /// <summary>
        /// Store/Read values in the same orientation as they are in memory
        /// <code>
        /// 1 | 2 --> 1 | 2
        /// 3 | 4     3 | 4
        /// 5 | 6     5 | 6
        /// </code>
        /// </summary>
        None = 0,
    }

    internal abstract class MatrixMapper<TMatrix> : IEnumerable<int>
    {
        public abstract int Columns { get; protected set; }

        public abstract TMatrix[] Current { get; set; }

        public abstract TMatrix this[int r, int c] { get; }

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

        public override TMatrix this[int r, int c]
        {
            get
            {
                return this.Current[c];

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

        public sealed override int Columns { get; protected set; }

        public override TMatrix[] Current { get; set; }

        public override TMatrix this[int r, int c]
        {
            get
            {
                return this.Current[c];

                throw new Exception();
            }
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
    }

    internal class TwoDimArrayMapper<TMatrix> : MatrixMapper<TMatrix>
    {
        private readonly TwoDimensionalArray dimensionality;
        private readonly TMatrix[,] matrix;

        public TwoDimArrayMapper(TMatrix[,] matrix, TwoDimensionalArray dimensionality)
        {
            this.matrix = matrix;
            this.dimensionality = dimensionality;

            this.Rows = dimensionality == TwoDimensionalArray.None ? matrix.RowLength() : matrix.ColumnLength();
            this.Columns = dimensionality == TwoDimensionalArray.None
                               ? matrix.ColumnLength()
                               : matrix.RowLength();
        }

        public int? Rows { get; private set; }

        public override int Columns { get; protected set; }

        public override TMatrix[] Current { get; set; }

        public override TMatrix this[int r, int c]
        {
            get
            {
                if (this.dimensionality == TwoDimensionalArray.None)
                {
                    return this.matrix[r, c];
                }

                if (this.dimensionality == TwoDimensionalArray.Transpose)
                {
                    return this.matrix[c, r];
                }

                if (this.dimensionality == TwoDimensionalArray.Rotate90ClockWise)
                {
                    return this.matrix[this.Columns - 1 - c,  r];
                }

                if (this.dimensionality == TwoDimensionalArray.Rotate90AntiClockWise)
                {
                    return this.matrix[c, this.Rows.Value - 1 - r];
                }

                throw new Exception();
            }
        }

        public override IEnumerator<int> GetEnumerator()
        {
            for (int i = 0; i < this.Rows; i++)
            {
                yield return i;
            }
        }
    }
}