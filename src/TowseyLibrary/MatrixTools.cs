// <copyright file="MatrixTools.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    //using MathNet.Numerics.LinearAlgebra;
    //using MathNet.Numerics.LinearAlgebra.Double;
    //using MathNet.Numerics.LinearAlgebra.Generic;

    public static class MatrixTools
    {
        /// <summary>
        /// TODO: This method concatenates time-sequence data but does not check that the files are in temporal sequence.
        ///       Nor does it check for temporal gaps.
        /// This method assumes that the column count for each matrix in list is identical.
        /// </summary>
        public static double[,] ConcatenateMatrixRows(List<double[,]> list)
        {
            int colCount = list[0].GetLength(1);
            int rowCount = 0;

            foreach (double[,] item in list)
            {
                rowCount += item.GetLength(0);
            }

            var opMatrix = new double[rowCount, colCount];
            int thisRowId = 0;

            // loop through the matrices
            for (int m = 0; m < list.Count; m++)
            {
                int rows = list[m].GetLength(0);

                // the rows of each matrix
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < colCount; c++)
                    {
                        opMatrix[thisRowId, c] = list[m][r, c];
                    }

                    thisRowId++;
                }
            }

            return opMatrix;
        }

        /// <summary>
        /// Concatenates two matrices that have the same column count.
        /// That is, each row of the output matrix is the join of the equivalent two rows of the input matrices.
        /// </summary>
        public static double[,] ConcatenateMatrixRows(double[,] m1, double[,] m2)
        {
            int m1ColCount = m1.GetLength(1);
            int m2ColCount = m2.GetLength(1);
            int m1RowCount = m1.GetLength(0);
            int m2RowCount = m2.GetLength(0);

            if (m1ColCount != m2ColCount)
            {
                throw new ArgumentException($"Cannot join these matrices. They do not have the same column count. {m1ColCount} != {m2ColCount}.");
            }

            var opMatrix = new double[m1RowCount + m2RowCount, m1ColCount];
            for (int r = 0; r < m1RowCount; r++)
            {
                for (int c = 0; c < m1ColCount; c++)
                {
                    opMatrix[r, c] = m1[r, c];
                }
            }

            for (int r = 0; r < m2RowCount; r++)
            {
                int row = m1RowCount + r;
                for (int c = 0; c < m1ColCount; c++)
                {
                    opMatrix[row, c] = m2[r, c];
                }
            }

            return opMatrix;
        }

        /// <summary>
        /// Concatenates two matrices that have the same row count.
        /// That is, each row of the output matrix is the join of the equivalent two rows of the input matrices.
        /// WARNING: If the two matrices do not have the same number of rows, an exception is thrown.
        /// </summary>
        public static double[,] ConcatenateTwoMatrices(double[,] matrix1, double[,] matrix2)
        {
            int rowCount1 = matrix1.GetLength(0);
            int colCount1 = matrix1.GetLength(1);
            int rowCount2 = matrix2.GetLength(0);
            int colCount2 = matrix2.GetLength(1);

            if (rowCount1 != rowCount2)
            {
                throw new ArgumentException($"Cannot join these matrices. They do not have the same row count. {rowCount1} != {rowCount2}.");
            }

            double[,] opMatrix = new double[rowCount1, colCount1 + colCount2];

            for (int row = 0; row < rowCount1; row++)
            {
                for (int col = 0; col < colCount1; col++)
                {
                    opMatrix[row, col] = matrix1[row, col];
                }

                for (int col = 0; col < colCount2; col++)
                {
                    opMatrix[row, colCount1 + col] = matrix2[row, col];
                }
            }

            return opMatrix;
        }

        public static bool MatrixDimensionsAreEqual(double[,] m1, double[,] m2)
        {
            if (m1.GetLength(0) == m2.GetLength(0) && m1.GetLength(1) == m2.GetLength(1))
            {
                return true;
            }

            //  throw new Exception("ERROR! Matrix dims must be same for matrix subtraction.");
            return false;
        }

        public static bool CentreIsLocalMaximum(double[,] m, double threshold)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);

            int centreRow = rows / 2;
            int centreCol = cols / 2;
            double centreValue = m[centreRow, centreCol];
            double sum = 0;
            int count = rows * cols;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (m[r, c] > centreValue)
                    {
                        return false;
                    }

                    sum += m[r, c];
                }
            }

            sum -= centreValue;
            double av = sum / (count - 1);
            if (centreValue - av < threshold)
            {
                return false;
            }

            //  throw new Exception("ERROR! Matrix dims must be same for matrix subtraction.");
            return true;
        }

        /// <summary>
        /// Adds a frame around a matrix by adding row and columns of zeros.
        /// </summary>
        /// <param name="matrix">matrix.</param>
        /// <param name="frameWidth">The number of rows/columns of zeros to be added.</param>
        public static double[,] FrameMatrixWithZeros(double[,] matrix, int frameWidth)
        {
            int inRowCount = matrix.GetLength(0);
            int inColCount = matrix.GetLength(1);

            int outRowCount = inRowCount + (2 * frameWidth);
            int outColCount = inColCount + (2 * frameWidth);

            var outputMatrix = new double[outRowCount, outColCount];

            for (int r = 0; r < inRowCount; r++)
            {
                for (int c = 0; c < inColCount; c++)
                {
                    outputMatrix[r + frameWidth, c + frameWidth] = matrix[r, c];
                }
            }

            return outputMatrix;
        }

        /// <summary>
        /// Returns the submatrix of passed matrix.
        /// The returned submatrix includes the rows and column passed as bounds.
        /// Assume that RowTop GT RowBottom, ColumnLeft LT ColumnRight.
        /// Row, column indices start at 0.
        /// </summary>
        /// <param name="m">the parent matrix.</param>
        /// <param name="r1">start row.</param>
        /// <param name="c1">start column.</param>
        /// <param name="r2">end row inclusive.</param>
        /// <param name="c2">end column inclusive.</param>
        /// <returns>matrix to be returned.</returns>
        public static T[,] Submatrix<T>(T[,] m, int r1, int c1, int r2, int c2)
        {
            int subRowCount = r2 - r1 + 1;
            int subColCount = c2 - c1 + 1;

            T[,] sm = new T[subRowCount, subColCount];

            for (int i = 0; i < subRowCount; i++)
            {
                for (int j = 0; j < subColCount; j++)
                {
                    sm[i, j] = m[r1 + i, c1 + j];
                }
            }

            return sm;
        }

        /// <summary>
        /// Returns an array of row averages in the submatrix of passed matrix.
        /// This method combines two methods, Submatrix() &amp; GetRowAverages(), for efficiency
        /// Assume that RowTop LT RowBottom, ColumnLeft LT ColumnRight.
        /// Row, column indices start at 0.
        /// </summary>
        public static double[] GetRowAveragesOfSubmatrix(double[,] m, int r1, int c1, int r2, int c2)
        {
            int subRowCount = r2 - r1 + 1;
            int subColCount = c2 - c1 + 1;
            double[] array = new double[subRowCount];

            for (int i = 0; i < subRowCount; i++)
            {
                double sum = 0;
                for (int j = 0; j < subColCount; j++)
                {
                    sum += m[r1 + i, c1 + j];
                    array[i] = sum / subColCount;
                }
            }

            return array;
        }

        public static double[,] ConvertList2Matrix(List<double[]> list)
        {
            int rows = list.Count;
            int cols = list[0].Length; //assume all vectors in list are of same length
            double[,] op = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    op[i, j] = list[i][j];
                }
            }

            return op;
        }

        /// <summary>
        /// This method assumes that the passed matrix of double already takes values between 0.0 and 1.0.
        /// </summary>
        public static byte[,] ConvertMatrixOfDouble2Byte(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            var outM = new byte[rows, cols];
            var maxValue = byte.MaxValue;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    outM[r, c] = (byte)(matrix[r, c] * maxValue);
                }
            }

            return outM;
        }

        public static double[,] ConvertMatrixOfByte2Double(byte[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            var outM = new double[rows, cols];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    outM[r, c] = matrix[r, c];
                }
            }

            return outM;
        }

        /// <summary>
        /// Converts a matrix to a vector by concatenating its columns.
        /// </summary>
        public static double[] Matrix2Array(double[,] matrix)
        {
            int ht = matrix.GetLength(0);
            int width = matrix.GetLength(1);
            double[] v = new double[ht * width];

            int id = 0;
            for (int col = 0; col < width; col++)
            {
                for (int row = 0; row < ht; row++)
                {
                    v[id++] = matrix[row, col];
                }
            }

            return v;
        }

        /// <summary>
        /// converts a vector to a matrix in the direction of column.
        /// For example, the "Matrix2Array" method in MatrixTools.cs builds the vector by concatenating the columns.
        /// </summary>
        public static double[,] ArrayToMatrixByColumn(double[] vector, int columnSize, int rowSize)
        {
            double[,] matrix = new double[rowSize, columnSize];
            for (int c = 0; c < vector.Length; c += rowSize)
            {
                for (int r = 0; r < rowSize; r++)
                {
                    matrix[r, c / rowSize] = vector[c + r];
                }
            }

            return matrix;
        }

        /// <summary>
        /// converts a vector to a matrix in the direction of row.
        /// </summary>
        public static double[,] ArrayToMatrixByRow(double[] vector, int columnSize, int rowSize)
        {
            double[,] matrix = new double[rowSize, columnSize];

            for (int r = 0; r < vector.Length; r += columnSize)
            {
                for (int c = 0; c < columnSize; c++)
                {
                    matrix[r / columnSize, c] = vector[c + r];
                }
            }

            return matrix;
        }

        /// <summary>
        /// Converts a matrix of doubles to binary using passed threshold.
        /// </summary>
        public static int[,] ThresholdMatrix2Binary(double[,] matrix, double threshold)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            var op = new int[rowCount, colCount];

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    if (matrix[r, c] > threshold)
                    {
                        op[r, c] = 1;
                    }
                }
            }

            return op;
        }

        /// <summary>
        /// Converts a matrix of doubles to binary using passed threshold.
        /// </summary>
        public static double[,] ThresholdMatrix2RealBinary(double[,] matrix, double threshold)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            var op = new double[rowCount, colCount];

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    if (matrix[r, c] > threshold)
                    {
                        op[r, c] = 1.0;
                    }
                }
            }

            return op;
        }

        public static double[,] SubtractAndTruncate2Zero(double[,] matrix, double threshold)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] outM = new double[rows, cols];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    outM[r, c] = matrix[r, c] - threshold;
                    if (outM[r, c] < 0.0)
                    {
                        outM[r, c] = 0.0;
                    }
                }
            }

            return outM;
        }

        /// <summary>
        /// Noise reduce matrix by subtracting the median value and truncating negative values to zero.
        /// </summary>
        public static double[,] SubtractMedian(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[] array = DataTools.Matrix2Array(matrix);
            double median = DataTools.GetMedian(array);

            double[,] outM = new double[rows, cols];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    outM[r, c] = matrix[r, c] - median;
                    if (outM[r, c] < 0.0)
                    {
                        outM[r, c] = 0.0;
                    }
                }
            }

            return outM;
        }

        public static double[,] SubtractConstant(double[,] matrix, double constant)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] outM = new double[rows, cols];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    outM[r, c] = matrix[r, c] - constant;
                    if (outM[r, c] < 0.0)
                    {
                        outM[r, c] = 0.0;
                    }
                }
            }

            return outM;
        }

        /// <summary>
        /// truncate values below threshold to zero.
        /// </summary>
        public static double[,] Truncate2Zero(double[,] matrix, double threshold)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] outM = new double[rows, cols];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (matrix[r, c] > threshold)
                    {
                        outM[r, c] = matrix[r, c];
                    }
                }
            }

            return outM;
        }

        public static double[,] Matrix2LogValues(double[,] matrix)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] op = new double[rowCount, colCount];

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    op[r, c] = Math.Log10(matrix[r, c]);
                }
            }

            return op;
        }

        /// <summary>
        /// Convert the power values in a matrix of spectrogram values to Decibels using: dB = 10*log10(power).
        /// Assume that all matrix values are positive i.e. due to prior noise removal.
        /// NOTE: This method also returns the min and max decibel values in the passed matrix.
        /// NOTE: A decibel value should be a ratio.
        ///       Here the ratio is implied ie it is relative to the value of maximum power in the original normalised signal.
        /// </summary>
        /// <param name="m">matrix of positive power values.</param>
        /// <param name="min">min value to be return by out.</param>
        /// <param name="max">max value to be return by out.</param>
        public static double[,] SpectrogramPower2DeciBels(double[,] m, double powerEpsilon, out double min, out double max)
        {
            //convert epsilon power to decibels
            double minDecibels = 10 * Math.Log10(powerEpsilon);

            min = double.MaxValue;
            max = -double.MaxValue;

            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double[,] returnM = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (m[i, j] <= powerEpsilon)
                    {
                        returnM[i, j] = minDecibels;
                    }
                    else
                    {
                        returnM[i, j] = 10 * Math.Log10(m[i, j]);
                    }

                    if (returnM[i, j] < min)
                    {
                        min = returnM[i, j];
                    }
                    else
                    if (returnM[i, j] > max)
                    {
                        max = returnM[i, j];
                    }
                }
            }

            return returnM;
        }

        /// <summary>
        /// Convert the Decibels values in a matrix of spectrogram values to power values.
        /// Assume that all matrix values are positive due to prior noise removal.
        /// </summary>
        /// <param name="m">matrix of positive Decibel values.</param>
        public static double[,] SpectrogramDecibels2Power(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double[,] retM = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    //convert decibels to power
                    double power = Math.Exp(m[i, j] / 10 * Math.Log(10));
                    retM[i, j] = power;
                }
            }

            return retM;
        }

        public static double[,] Matrix2ZScores(double[,] matrix, double av, double sd)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] op = new double[rowCount, colCount];

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    op[r, c] = (matrix[r, c] - av) / sd;
                }
            }

            return op;
        }

        /// <summary>
        /// Squares the values in a matrix.
        /// Primarily used when converting FFT coefficients in amplitude spectrogram to power values.
        /// </summary>
        public static double[,] SquareValues(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] newM = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    newM[i, j] = matrix[i, j] * matrix[i, j];
                }
            }

            return newM;
        }

        /// <summary>
        /// Multiplies the values in a matrix by a factor.
        /// Used to convert log-energy values to decibels.
        /// </summary>
        public static double[,] MultiplyMatrixByFactor(double[,] matrix, int factor)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] newM = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    newM[i, j] = matrix[i, j] * factor;
                }
            }

            return newM;
        }

        public static double[,] LogTransform(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] newM = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (matrix[i, j] <= 0.0)
                    {
                        newM[i, j] = 0.0;
                    }
                    else
                    {
                        newM[i, j] = Math.Log(1 + matrix[i, j]);
                    }
                }
            }

            return newM;
        }

        public static double[,] SquareRootOfValues(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] newM = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    newM[i, j] = Math.Sqrt(matrix[i, j]);
                }
            }

            return newM;
        }

        /// <summary>
        /// The ColourFilter parameter determines how much the low index values are emphasized or de-emphasized.
        /// The purpose is to make low intensity features stand out (emphasis) or become even less obvious (de-emphasis).
        /// This parameter applies a function that lies between y=x^-2 and y=x^2, i.e. between the square-root and the square.
        /// For an acoustic index value of X, newX = [(1/c - 1) * X^2 + X] * c, where c = the supplied filterCoeff.
        /// When filterCoeff = 1.0, small values are maximally emphasized, i.e. y=sqrt(x).
        /// When filterCoeff = 0.0, the matrix remains unchanged, that is, y=x.
        /// When filterCoeff =-1.0, small values are maximally de-emphasized, i.e. y=x^2.
        /// Generally usage suggests that a value of -0.25 is suitable. i.e. a slight de-emphasis.
        /// ..
        /// Visual example https://www.wolframalpha.com/input/?i=plot+y+%3D+%5B(1%2Fc+-+1)+*+x%5E2+%2B+x%5D+*+c+,+x%3D0..1,+c%3D0.0..2.0.
        /// </summary>
        public static double[,] FilterBackgroundValues(double[,] m, double filterCoeff)
        {
            double tolerance = 0.000001;
            if (Math.Abs(filterCoeff) < tolerance)
            {
                // no change
                return m;
            }

            if (filterCoeff < -0.9)
            {
                // maximal de-emphasis of background values
                return SquareValues(m);
            }

            if (filterCoeff > 0.9)
            {
                // maximal emphasis of background values
                return SquareRootOfValues(m);
            }

            // shift the coefficient
            double param = 1 / (filterCoeff + 1);
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double[,] newM = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    newM[i, j] = (((param - 1) * (m[i, j] * m[i, j])) + m[i, j]) / param;
                }
            }

            return newM;
        }

        /// <summary>
        /// bounds a matrix of numbers between a minimum and a maximum.
        /// Numbers that fall outside the bound are truncated to the bound.
        /// </summary>
        /// <param name="matrix">the matrix to be bound.</param>
        /// <param name="min">The minimum bound.</param>
        /// <param name="max">The maximum bound.</param>
        public static double[,] BoundMatrix(double[,] matrix, double min, double max)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] newM = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (matrix[i, j] <= min)
                    {
                        newM[i, j] = min;
                    }
                    else if (matrix[i, j] >= max)
                    {
                        newM[i, j] = max;
                    }
                    else
                    {
                        newM[i, j] = matrix[i, j];
                    }
                }
            }

            return newM;
        }

        /// <summary>
        /// Sets any element in matrix with value> 0.0 to zero if all surrounding elements also = zero.
        /// </summary>
        public static void SetSingletonsToZero(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            int count = 0;
            int total = 0;
            const double tolerance = 0.000001;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (m[r, c] <= 0.0)
                    {
                        continue;
                    }

                    total++;
                    if (Math.Abs(m[r, c + 1]) < tolerance && Math.Abs(m[r - 1, c + 1]) < tolerance
                        && Math.Abs(m[r - 1, c]) < tolerance && Math.Abs(m[r - 1, c - 1]) < tolerance
                        && Math.Abs(m[r, c - 1]) < tolerance && Math.Abs(m[r + 1, c - 1]) < tolerance
                        && Math.Abs(m[r + 1, c]) < tolerance && Math.Abs(m[r + 1, c + 1]) < tolerance)
                    {
                        m[r, c] = 0.0;
                        count++;
                    }
                }
            }

            Console.WriteLine("Zeroed {0} of {1} non-zero cells.", count, total);
        }

        /// <summary>
        /// Sets any element in matrix with value> 0.0 to zero if all surrounding elements also = zero.
        /// </summary>
        public static void SetDoubletsToZero(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            int total = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (m[r, c] <= 0.0)
                    {
                        continue;
                    }

                    // check if more than two poi's in nearest neighbours.
                    int count = 0;
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            if (m[r + i, c + j] > 0.0)
                            {
                                count++;
                            }
                        }
                    }

                    if (count > 2)
                    {
                        continue;
                    }

                    // now check the 8 directions. Assume adjacent because have already called PruneSingletons();
                    if (m[r, c + 1] > 0.0 && m[r, c + 2] > 0.0)
                    {
                        continue; // three in a row
                    }

                    if (m[r - 1, c + 1] > 0.0 && m[r - 2, c + 2] > 0.0)
                    {
                        continue; // three on a diagonal
                    }

                    if (m[r - 1, c] > 0.0 && m[r - 2, c] > 0.0)
                    {
                        continue; // three in a col
                    }

                    if (m[r - 1, c - 1] > 0.0 && m[r - 2, c - 2] > 0.0)
                    {
                        continue; // three on a diagonal
                    }

                    if (m[r, c - 1] > 0.0 && m[r, c - 2] > 0.0)
                    {
                        continue; // three in a row
                    }

                    if (m[r + 1, c - 1] > 0.0 && m[r + 2, c - 2] > 0.0)
                    {
                        continue; // three on a diagonal
                    }

                    if (m[r + 1, c] > 0.0 && m[r + 2, c] > 0.0)
                    {
                        continue; // three in a col
                    }

                    if (m[r + 1, c + 1] > 0.0 && m[r + 2, c + 2] > 0.0)
                    {
                        continue; // three on a diagonal
                    }

                    //if ((m[r - 1, c] > 0.0) && (m[r + 1, c] == 0.0)) continue; // three in a column
                    //if ((m[r - 1, c - 1] > 0.0) && (m[r + 1, c + 1] == 0.0)) continue; // three on a diagonal
                    //if ((m[r - 1, c + 1] > 0.0) && (m[r + 1, c - 1] == 0.0)) continue; // three on a diagonal

                    // if get to here then must be a doublet.
                    total++;

                    // zero all cells
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            m[r + i, c + j] = 0.0;
                        }
                    }
                }
            } // for loop

            Console.WriteLine("Removed {0} doublets.", total);
        } // SetSingletonsToZero

        /// <summary>
        /// returns the min and max percentile values of the values in passed matrix.
        /// </summary>
        /// <param name="matrix">the matrix.</param>
        /// <param name="minPercentile">minPercentile.</param>
        /// <param name="maxPercentile">maxPercentile.</param>
        /// <param name="minCut">power value equivalent to minPercentile.</param>
        /// <param name="maxCut">power value equivalent to maxPercentile.</param>
        public static void PercentileCutoffs(double[,] matrix, int minPercentile, int maxPercentile, out double minCut, out double maxCut)
        {
            if (maxPercentile < minPercentile)
            {
                throw new ArgumentException("maxPercentile must be greater than or equal to minPercentile");
            }

            if (minPercentile < 0)
            {
                throw new ArgumentException("minPercentile must be at least 0%");
            }

            if (maxPercentile > 100)
            {
                throw new ArgumentException("maxPercentile must be at most 100%");
            }

            // Must first calculate the min and max values in the matrix.
            DataTools.MinMax(matrix, out var min, out var max);
            if (max <= min)
            {
                throw new ArgumentException("max=" + max + " must be > min=" + min);
            }

            minCut = min;
            maxCut = max;

            //const double tolerance = 0.0000001;
            //if (Math.Abs(minPercentile) < tolerance && Math.Abs(maxPercentile - 1.0) < tolerance)
            //{
            //    return;
            //}

            //const int n = 1024;      //number of bins for histogram
            const int n = 100;      //number of bins for histogram
            int[] bins = new int[n]; //histogram of power in sonogram

            int rows = matrix.GetLength(0); //width
            int cols = matrix.GetLength(1); //height
            double range = max - min;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    //NormaliseMatrixValues power for given min and max
                    int binId = (int)Math.Floor(n * (matrix[i, j] - min) / range); //NormaliseMatrixValues
                    if (binId < 0)
                    {
                        binId = 0; //range check
                    }

                    if (binId >= n)
                    {
                        binId = n - 1;
                    }

                    bins[binId]++;
                }
            }

            //int minThres = (int)Math.Floor(minPercentile * rows * cols);
            //int minThres = (int)Math.Floor(minPercentile * rows * cols);

            double binWidth = range / 100.00;
            int minThres = (int)Math.Floor(binWidth * minPercentile);
            minCut = min;
            for (int k = 0; k < n; k++)
            {
                minThres -= bins[k];
                if (minThres < 0.0)
                {
                    minCut = min + (k * range / n);
                    break;
                }
            }

            int maxThres = (int)Math.Ceiling((1.0 - maxPercentile) * rows * cols);
            maxCut = max;
            for (int k = n; k > 0; k--)
            {
                maxThres -= bins[k - 1];
                if (maxThres < 0.0)
                {
                    maxCut = min + (k * range / n);
                    break;
                }
            }
        }

        //=============================================================================

        public static void WriteMatrix(double[,] matrix, string format)
        {
            int rowCount = matrix.GetLength(0); //height
            int colCount = matrix.GetLength(1); //width
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    LoggedConsole.Write(" " + matrix[i, j].ToString(format));
                    if (j < colCount - 1)
                    {
                        LoggedConsole.Write(",");
                    }
                }

                LoggedConsole.WriteLine();
            }
        }

        public static void WriteMatrix(double[,] matrix)
        {
            WriteMatrix(matrix, "F2");
        }

        public static void WriteMatrix(int[,] matrix)
        {
            int rowCount = matrix.GetLength(0); //height
            int colCount = matrix.GetLength(1); //width
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    LoggedConsole.Write(" " + matrix[i, j]);
                    if (j < colCount - 1)
                    {
                        LoggedConsole.Write(",");
                    }
                }

                LoggedConsole.WriteLine();
            }
        }

        public static void WriteMatrix2File(double[,] matrix, string fPath)
        {
            int rowCount = matrix.GetLength(0); // height
            int colCount = matrix.GetLength(1); // width
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    LoggedConsole.Write(" " + matrix[i, j].ToString("F2"));
                    if (j < colCount - 1)
                    {
                        LoggedConsole.Write(",");
                    }
                }

                LoggedConsole.WriteLine();
            }
        }

        /// <summary>
        /// ADD matrix m2 to matrix m1.
        /// </summary>
        public static double[,] AddMatrices(double[,] m1, double[,] m2)
        {
            if ((m1 == null) || (m2 == null))
            {
                return null;
            }

            int m1Rows = m1.GetLength(0);
            int m1Cols = m1.GetLength(1);
            int m2Rows = m2.GetLength(0);
            int m2Cols = m2.GetLength(1);
            if (m1Rows != m2Rows)
            {
                throw new Exception("ERROR! Matrix dims must be same for matrix addition.");
            }

            if (m1Cols != m2Cols)
            {
                throw new Exception("ERROR! Matrix dims must be same for matrix addition.");
            }

            double[,] newMatrix = (double[,])m1.Clone();
            for (int i = 0; i < m1Rows; i++)
            {
                for (int j = 0; j < m1Cols; j++)
                {
                    newMatrix[i, j] = m1[i, j] + m2[i, j];
                }
            }

            return newMatrix;
        }

        /// <summary>
        /// Create a matrix whose values are the max of two passed matrices, m1 and m2.
        /// </summary>
        public static double[,] MaxOfTwoMatrices(double[,] m1, double[,] m2)
        {
            if ((m1 == null) || (m2 == null))
            {
                return null;
            }

            int m1Rows = m1.GetLength(0);
            int m1Cols = m1.GetLength(1);
            int m2Rows = m2.GetLength(0);
            int m2Cols = m2.GetLength(1);
            if (m1Rows != m2Rows || m1Cols != m2Cols)
            {
                throw new Exception("ERROR! Matrix dimensions must be same.");
            }

            double[,] newMatrix = new double[m1Rows, m1Cols];
            for (int i = 0; i < m1Rows; i++)
            {
                for (int j = 0; j < m1Cols; j++)
                {
                    newMatrix[i, j] = Math.Max(m1[i, j], m2[i, j]);
                }
            }

            return newMatrix;
        }

        /// <summary>
        /// Adds two matrices using weighted sum.
        /// Typically expected that that w1 + w2 = 0 and both matrices are normalised.
        /// </summary>
        public static double[,] AddMatricesWeightedSum(double[,] m1, double w1, double[,] m2, double w2)
        {
            if ((m1 == null) || (m2 == null))
            {
                return null;
            }

            int m1Rows = m1.GetLength(0);
            int m1Cols = m1.GetLength(1);
            int m2Rows = m2.GetLength(0);
            int m2Cols = m2.GetLength(1);
            if (m1Rows != m2Rows || m1Cols != m2Cols)
            {
                throw new Exception("ERROR! Matrix dimensions must be same.");
            }

            double[,] newMatrix = new double[m1Rows, m1Cols];
            for (int i = 0; i < m1Rows; i++)
            {
                for (int j = 0; j < m1Cols; j++)
                {
                    newMatrix[i, j] = (w1 * m1[i, j]) + (w2 * m2[i, j]);
                }
            }

            return newMatrix;
        }

        /// <summary>
        /// DIVIDE matrix m1 by factor.
        /// </summary>
        public static double[,] DivideMatrix(double[,] m1, double factor)
        {
            if (m1 == null)
            {
                return null;
            }

            int m1Rows = m1.GetLength(0);
            int m1Cols = m1.GetLength(1);

            double[,] newMatrix = (double[,])m1.Clone();
            for (int i = 0; i < m1Rows; i++)
            {
                for (int j = 0; j < m1Cols; j++)
                {
                    newMatrix[i, j] = m1[i, j] / factor;
                }
            }

            return newMatrix;
        }

        /// <summary>
        /// Subtract matrix m2 from matrix m1.
        /// </summary>
        public static double[,] SubtractMatrices(double[,] m1, double[,] m2)
        {
            int m1Rows = m1.GetLength(0);
            int m1Cols = m1.GetLength(1);
            int m2Rows = m2.GetLength(0);
            int m2Cols = m2.GetLength(1);
            if (m1Rows != m2Rows)
            {
                throw new Exception("ERROR! Matrix dims must be same for matrix subtraction.");
            }

            if (m1Cols != m2Cols)
            {
                throw new Exception("ERROR! Matrix dims must be same for matrix subtraction.");
            }

            double[,] newMatrix = (double[,])m1.Clone();
            for (int i = 0; i < m1Rows; i++)
            {
                for (int j = 0; j < m1Cols; j++)
                {
                    newMatrix[i, j] = m1[i, j] - m2[i, j];
                }
            }

            return newMatrix;
        }

        public static double[,] RemoveLastNRows(double[,] m1, int number)
        {
            int m1Rows = m1.GetLength(0);
            int m1Cols = m1.GetLength(1);
            int newRowCount = m1Rows - number;

            double[,] newMatrix = new double[newRowCount, m1Cols];
            for (int r = 0; r < newRowCount; r++)
            {
                for (int c = 0; c < m1Cols; c++)
                {
                    newMatrix[r, c] = m1[r, c];
                }
            }

            return newMatrix;
        }

        /// <summary>
        /// Add rows of nan to pad out a matrix.
        /// </summary>
        public static double[,] AddBlankRows(double[,] m1, int number)
        {
            int m1Rows = m1.GetLength(0);
            int m1Cols = m1.GetLength(1);
            int newRowCount = m1Rows + number;

            double[,] newMatrix = new double[newRowCount, m1Cols];
            for (int r = 0; r < m1Rows; r++)
            {
                for (int c = 0; c < m1Cols; c++)
                {
                    newMatrix[r, c] = m1[r, c];
                }
            }

            for (int r = 0; r < number; r++)
            {
                for (int c = 0; c < m1Cols; c++)
                {
                    newMatrix[m1Rows + r, c] = double.NaN;
                }
            }

            return newMatrix;
        }

        public static double[,] SubtractValuesFromOne(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);

            double[,] newMatrix = new double[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    newMatrix[r, c] = 1 - m[r, c];
                }
            }

            return newMatrix;
        }

        public static byte[] GetColumn(byte[,] m, int columnIndex)
        {
            int rows = m.GetLength(0);
            byte[] column = new byte[rows];
            for (int i = 0; i < rows; i++)
            {
                column[i] = m[i, columnIndex];
            }

            return column;
        }

        public static double[] GetColumn(double[,] m, int columnIndex)
        {
            int rows = m.GetLength(0);
            double[] column = new double[rows];
            for (int i = 0; i < rows; i++)
            {
                column[i] = m[i, columnIndex];
            }

            return column;
        }

        public static void SetColumn(double[,] m, int colId, double[] array)
        {
            int rows = m.GetLength(0);
            for (int r = 0; r < rows; r++)
            {
                m[r, colId] = array[r];
            }
        }

        public static int SumColumn(int[,] m, int colId)
        {
            int rows = m.GetLength(0);
            int sum = 0;
            for (int i = 0; i < rows; i++)
            {
                sum += m[i, colId];
            }

            return sum;
        }

        public static int SumColumn(byte[,] m, int colId)
        {
            int rows = m.GetLength(0);
            int sum = 0;
            for (int i = 0; i < rows; i++)
            {
                sum += m[i, colId];
            }

            return sum;
        }

        public static double[] GetRow(double[,] m, int rowId)
        {
            int cols = m.GetLength(1);
            double[] row = new double[cols];
            for (int i = 0; i < cols; i++)
            {
                row[i] = m[rowId, i];
            }

            return row;
        }

        public static void SetRow(double[,] m, int rowId, double[] array)
        {
            int cols = m.GetLength(1);
            for (int c = 0; c < cols; c++)
            {
                m[rowId, c] = array[c];
            }
        }

        public static double GetRowSum(double[,] m, int rowId)
        {
            double[] row = GetRow(m, rowId);
            return row.Sum();
        }

        public static int SumRow(int[,] m, int rowId)
        {
            int cols = m.GetLength(1);
            int sum = 0;
            for (int i = 0; i < cols; i++)
            {
                sum += m[rowId, i];
            }

            return sum;
        }

        public static double[] SumRows(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double[] rowSums = new double[rows];
            for (int r = 0; r < rows; r++)
            {
                double sum = 0.0;
                for (int c = 0; c < cols; c++)
                {
                    sum += m[r, c];
                }

                rowSums[r] = sum;
            }

            //sum = 0.0;
            //for (int j = 0; j < cols; j++) sum += colSums[j];
            //LoggedConsole.WriteLine("sum="+sum.ToString("F5"));
            return rowSums;
        }

        public static double[] SumColumns(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double[] colSums = new double[cols];
            for (int j = 0; j < cols; j++)
            {
                double sum = 0.0;
                for (int i = 0; i < rows; i++)
                {
                    sum += m[i, j];
                }

                colSums[j] = sum;
            }

            //sum = 0.0;
            //for (int j = 0; j < cols; j++) sum += colSums[j];
            //LoggedConsole.WriteLine("sum="+sum.ToString("F5"));
            return colSums;
        }

        public static double[] GetColumnMedians(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double[] colMedians = new double[cols];
            for (int j = 0; j < cols; j++)
            {
                double[] column = GetColumn(m, j);
                Tuple<int[], double[]> tuple = DataTools.SortArray(column);
                colMedians[j] = tuple.Item2[rows / 2];
            }

            return colMedians;
        }

        public static double[] GetColumnSums(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double[] colSums = new double[cols];
            for (int j = 0; j < cols; j++)
            {
                double sum = 0.0;
                for (int i = 0; i < rows; i++)
                {
                    sum += m[i, j];
                }

                colSums[j] = sum;
            }

            return colSums;
        }

        public static double[] GetRowSums(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double[] rowSums = new double[rows];
            for (int r = 0; r < rows; r++)
            {
                double sum = 0.0;
                for (int c = 0; c < cols; c++)
                {
                    sum += m[r, c];
                }

                rowSums[r] = sum;
            }

            return rowSums;
        }

        public static double[] GetColumnAverages(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double[] colSums = new double[cols];
            for (int j = 0; j < cols; j++)
            {
                double sum = 0.0;
                for (int i = 0; i < rows; i++)
                {
                    sum += m[i, j];
                }

                colSums[j] = sum / rows;
            }

            return colSums;
        }

        public static double[] GetRowAverages(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double[] rowSums = new double[rows];
            for (int r = 0; r < rows; r++)
            {
                double sum = 0.0;
                for (int c = 0; c < cols; c++)
                {
                    sum += m[r, c];
                }

                rowSums[r] = sum / cols;
            }

            return rowSums;
        }

        public static double SumPositiveDiagonal(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            if (rows != cols)
            {
                return double.NaN;
            }

            double sum = 0.0;
            for (int r = 0; r < rows; r++)
            {
                sum += m[r, cols - 1 - r];
            }

            return sum;
        }

        public static void SumTriangleAbovePositiveDiagonal(double[,] m, out double sum, out int count)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            if (rows != cols)
            {
                sum = double.NaN;
                count = 0;
                return;
            }

            sum = 0.0;
            count = 0;
            for (int r = 0; r < rows - 1; r++)
            {
                for (int c = 0; c < cols - 1 - r; c++)
                {
                    sum += m[r, c];
                    count++;
                }
            }
        }

        public static void SumTriangleBelowPositiveDiagonal(double[,] m, out double sum, out int count)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            if (rows != cols)
            {
                sum = double.NaN;
                count = 0;
                return;
            }

            sum = 0.0;
            count = 0;
            for (int r = 1; r < rows; r++)
            {
                for (int c = cols - r; c < cols; c++)
                {
                    sum += m[r, c];
                    count++;
                }
            }
        }

        public static void AverageValuesInTriangleAboveAndBelowPositiveDiagonal(double[,] m, out double upperAv, out double lowerAv)
        {
            SumTriangleAbovePositiveDiagonal(m, out var sum, out var count);
            upperAv = sum / count;
            SumTriangleBelowPositiveDiagonal(m, out sum, out count);
            lowerAv = sum / count;
        }

        public static double SumNegativeDiagonal(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            if (rows != cols)
            {
                return double.NaN;
            }

            double sum = 0.0;
            for (int r = 0; r < rows; r++)
            {
                sum += m[r, r];
            }

            return sum;
        }

        public static void SumTriangleAboveNegativeDiagonal(double[,] m, out double sum, out int count)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            if (rows != cols)
            {
                sum = double.NaN;
                count = 0;
                return;
            }

            sum = 0.0;
            count = 0;
            for (int r = 0; r < rows - 1; r++)
            {
                for (int c = r + 1; c < cols; c++)
                {
                    sum += m[r, c];
                    count++;
                }
            }
        }

        public static void SumTriangleBelowNegativeDiagonal(double[,] m, out double sum, out int count)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            if (rows != cols)
            {
                sum = double.NaN;
                count = 0;
                return;
            }

            sum = 0.0;
            count = 0;
            for (int r = 1; r < rows; r++)
            {
                for (int c = 0; c < r; c++)
                {
                    sum += m[r, c];
                    count++;
                }
            }
        }

        public static void AverageValuesInTriangleAboveAndBelowNegativeDiagonal(double[,] m, out double upperAv, out double lowerAv)
        {
            SumTriangleAboveNegativeDiagonal(m, out var sum, out var count);
            upperAv = sum / count;
            SumTriangleBelowNegativeDiagonal(m, out sum, out count);
            lowerAv = sum / count;
        }

        /// <summary>
        /// Normalises matrix values so that the min and max values become [0,1], respectively.
        /// </summary>
        public static double[,] NormaliseMatrixValues(double[,] m)
        {
            double min = double.MaxValue;
            double max = -double.MaxValue;

            int rows = m.GetLength(0);
            int cols = m.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (m[i, j] > max)
                    {
                        max = m[i, j];
                    }
                    else
                        if (m[i, j] < min)
                    {
                        min = m[i, j];
                    }
                }
            }

            double range = max - min;

            double[,] ret = new double[rows, cols];
            if (Math.Abs(range) < 0.000001)
            {
                return ret;
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    ret[i, j] = (m[i, j] - min) / range;
                }
            }

            return ret;
        }

        ///// <summary>
        ///// Rotates a matrix 90 degrees clockwise.
        ///// Used for Syntactic pattern recognition
        ///// Converts Image matrix to Spectrogram data orientation
        ///// </summary>
        ///// <param name="m">the matrix to rotate</param>
        //public static char[,] MatrixRotate90Clockwise(char[,] m)
        //{
        //    int rows = m.GetLength(0);
        //    int cols = m.GetLength(1);
        //    var ret = new char[cols, rows];
        //    for (int r = 0; r < rows; r++)
        //    {
        //        for (int c = 0; c < cols; c++)
        //        {
        //            ret[c, r] = m[rows - r, c];
        //        }
        //    }

        //    return ret;
        //}

        ///// <summary>
        ///// Turns a matrix upside-down and back-to-front!
        ///// This is  a rotation NOT a transpose.
        ///// </summary>
        ///// <param name="m">the matrix to rotate</param>
        ///// <returns></returns>
        //public static double[,] MatrixRotate180(double[,] m)
        //{
        //    int rows = m.GetLength(0);
        //    int cols = m.GetLength(1);
        //    var ret = new double[rows, cols];
        //    for (int r = 0; r < rows; r++)
        //    {
        //        for (int c = 0; c < cols; c++)
        //        {
        //            ret[r, c] = m[rows - r - 1, cols - c - 1];
        //        }
        //    }

        //    return ret;
        //}

        /// <summary>
        /// Rotates a matrix 90 degrees clockwise.
        /// </summary>
        /// <param name="m">the matrix to rotate.</param>
        public static double[,] MatrixRotate90Clockwise(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var ret = new double[cols, rows];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    ret[c, r] = m[rows - r - 1, c];
                }
            }

            return ret;
        }

        public static byte[,] MatrixRotate90Clockwise(byte[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var ret = new byte[cols, rows];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    ret[c, r] = m[rows - r - 1, c];
                }
            }

            return ret;
        }

        /// <summary>
        /// Rotates a matrix 90 degrees clockwise.
        /// </summary>
        /// <param name="m">the matrix to rotate.</param>
        public static int[,] MatrixRotate90Clockwise(int[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var ret = new int[cols, rows];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    ret[c, r] = m[rows - r - 1, c];
                }
            }

            return ret;
        }

        /// <summary>
        /// Rotates a matrix 90 degrees anticlockwise.
        /// Used for Syntactic pattern recognition
        /// Converts Image matrix to Spectrogram data orientation.
        /// </summary>
        /// <param name="m">the matrix to rotate.</param>
        public static double[,] MatrixRotate90Anticlockwise(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var ret = new double[cols, rows];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    ret[c, r] = m[r, cols - 1 - c];
                }
            }

            return ret;
        }

        // TODO delete the below methods is after some time.
        ///// <summary>
        ///// performs a matrix transform
        ///// </summary>
        ///// <param name="m">the matrix to transform</param>
        //public static double[,] MatrixTranspose(double[,] m)
        //{
        //    int rows = m.GetLength(0);
        //    int cols = m.GetLength(1);
        //    double[,] Mt = new double[cols, rows];
        //    for (int r = 0; r < rows; r++)
        //    {
        //        for (int c = 0; c < cols; c++)
        //        {
        //            Mt[c, r] = m[r, c];
        //        }
        //    }
        //    return Mt;
        //}

        ///// <summary>
        ///// transforms a matrix of char.
        ///// Used for Syntatctic pttern recognition
        ///// </summary>
        ///// <param name="m">the matrix to transform</param>
        //public static char[,] MatrixTranspose(char[,] m)
        //{
        //    int rows = m.GetLength(0);
        //    int cols = m.GetLength(1);
        //    var ret = new char[cols, rows];
        //    for (int r = 0; r < rows; r++)
        //    {
        //        for (int c = 0; c < cols; c++)
        //        {
        //            ret[c, r] = m[r, c];
        //        }
        //    }
        //    return ret;
        //}

        ///// <summary>
        ///// performs a matrix transform
        ///// </summary>
        ///// <param name="m">the matrix to transform</param>
        //public static byte[,] MatrixTranspose(byte[,] m)
        //{
        //    int rows = m.GetLength(0);
        //    int cols = m.GetLength(1);
        //    byte[,] Mt = new byte[cols, rows];
        //    for (int r = 0; r < rows; r++)
        //    {
        //        for (int c = 0; c < cols; c++)
        //        {
        //            Mt[c, r] = m[r, c];
        //        }
        //    }
        //    return Mt;
        //}

        //public static double[,] ScaleMatrix(double[,] matrix, int newRowCount, int newColCount)
        //{
        //    int mrows = matrix.GetLength(0);
        //    int mcols = matrix.GetLength(1);
        //    var newMatrix = new double[newRowCount, newColCount];
        //    double rowScale = newRowCount / (double)matrix.GetLength(0);
        //    for (int r = 0; r < mrows; r++)
        //    {
        //        for (int c = 0; c < mcols; c++)
        //        {
        //            if (Math.Abs(matrix[r, c]) < 0.000001)
        //        {
        //            continue;
        //        }
        //            int newRow = (int)Math.Round(r * rowScale);
        //            // int newCol = (int)Math.Round(c * colScale);
        //            int newCol = (int)Math.Round(c * 4.1);
        //            //newCol = 1;
        //            if (newRow >= newRowCount)
        //            {
        //                newRow = newRowCount - 1;
        //            }
        //            if (newCol >= newColCount)
        //            {
        //                newCol = newColCount - 1;
        //            }
        //            //if (newMatrix[newRow, newCol] < matrix[r, c])
        //            newMatrix[newRow, newCol] = matrix[r, c];
        //        }
        //    }
        //    return newMatrix;
        //}

        ///// <summary>
        ///// a difference filter over the rows from row 0.
        ///// To be used to remove background noise from sonogram
        ///// </summary>
        //public static double[,] FilterMatrixRows(double[,] m)
        //{
        //    int rowCount = m.GetLength(0);
        //    int colCount = m.GetLength(1);
        //    double[,] returnMatrix = new double[rowCount, colCount];
        //    for (int r = 0; r < rowCount; r++)
        //    {
        //        for (int c = 1; c < colCount; c++)
        //        {
        //            returnMatrix[r, c] = m[r, c] - m[r, c - 1];
        //        }
        //    }
        //    return returnMatrix;
        //}

        //===========================================================================================================================================================

        /// <summary>
        /// returns EUCLIDIAN DISTANCE BETWEEN two matrices.
        /// </summary>
        public static double EuclidianDistance(double[,] m1, double[,] m2)
        {
            //check m1 and m2 have same dimensions
            int rows1 = m1.GetLength(0);
            int cols1 = m1.GetLength(1);
            int rows2 = m2.GetLength(0);
            int cols2 = m2.GetLength(1);
            if (rows1 != rows2)
            {
                throw new Exception("Matrices have unequal row numbers.");
            }

            if (cols1 != cols2)
            {
                throw new Exception("Matrices have unequal column numbers.");
            }

            //calculate euclidian distance
            double sum = 0.0;
            for (int i = 0; i < rows1; i++)
            {
                for (int j = 0; j < cols1; j++)
                {
                    double v = m1[i, j] - m2[i, j];
                    sum += v * v;
                }
            }

            return Math.Sqrt(sum);
        }

        /// <summary>
        /// Multiplies two matrices by summing m1[r,c]*m2[r,c].
        /// </summary>
        public static double DotProduct(double[,] m1, double[,] m2)
        {
            //check m1 and m2 have same dimensions
            int rows1 = m1.GetLength(0);
            int cols1 = m1.GetLength(1);
            int rows2 = m2.GetLength(0);
            int cols2 = m2.GetLength(1);
            if (rows1 != rows2)
            {
                throw new Exception("Matrices have unequal row numbers.");
            }

            if (cols1 != cols2)
            {
                throw new Exception("Matrices have unequal column numbers.");
            }

            double sum = 0.0;
            for (int i = 0; i < rows1; i++)
            {
                for (int j = 0; j < cols1; j++)
                {
                    sum += m1[i, j] * m2[i, j];
                }
            }

            return sum;
        }

        /// <summary>
        /// Rescales the values of a matrix so that its in and max values are those passed.
        /// </summary>
        public static double[,] RescaleMatrixBetweenMinAndMax(double[,] m, double normMin, double normMax)
        {
            double min = double.MaxValue;
            double max = -double.MaxValue;

            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double[,] ret = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (m[i, j] > max)
                    {
                        max = m[i, j];
                    }

                    if (m[i, j] < min)
                    {
                        min = m[i, j];
                    }
                }
            }

            double range = max - min;
            double normRange = normMax - normMin;

            //LoggedConsole.WriteLine("range ="+ range+"  normRange="+normRange);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    double norm01 = (m[i, j] - min) / range;
                    ret[i, j] = normMin + (norm01 * normRange);
                }
            }

            return ret;
        }

        /// <summary>
        /// Normalises a matrix so that ---
        /// all values LT passed MIN are truncated to 0
        /// and
        /// all values GT passed MAX are truncated to 1.
        /// </summary>
        public static double[,] NormaliseInZeroOne(double[,] m, double truncateMin, double truncateMax)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double range = truncateMax - truncateMin;
            double[,] m2Return = new double[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    m2Return[r, c] = (m[r, c] - truncateMin) / range;
                    if (m2Return[r, c] > 1.0)
                    {
                        m2Return[r, c] = 1.0;
                    }
                    else if (m2Return[r, c] < 0.0)
                    {
                        m2Return[r, c] = 0.0;
                    }
                }
            }

            return m2Return;
        }

        /// <summary>
        /// Normalises a matrix so that all values lie between 0 and 1.
        /// Min value in matrix set to 0.0.
        /// Max value in matrix is set to 1.0.
        /// Rerturns the min and the max.
        /// </summary>
        public static double[,] NormaliseInZeroOne(double[,] m, out double min, out double max)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            DataTools.MinMax(m, out min, out max);
            double range = max - min;
            double[,] m2Return = new double[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    m2Return[r, c] = (m[r, c] - min) / range;
                    if (m2Return[r, c] > 1.0)
                    {
                        m2Return[r, c] = 1.0;
                    }
                    else if (m2Return[r, c] < 0.0)
                    {
                        m2Return[r, c] = 0.0;
                    }
                }
            }

            return m2Return;
        }

        /// <summary>
        /// Normalises a matrix so that the values in each column lie between 0 and 1.
        /// This method is used in producing mfcc's where all the coefficients are weighted so has to have similar range.
        /// </summary>
        public static double[,] NormaliseMatrixColumns(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);

            double[,] m2Return = new double[rows, cols];

            // extract each column in turn, normalise and return.
            for (int c = 0; c < cols; c++)
            {
                var column = MatrixTools.GetColumn(m, c);
                var colNormalised = DataTools.normalise(column);

                // return the column.
                MatrixTools.SetColumn(m2Return, c, colNormalised);
            }

            return m2Return;
        }

        /// <summary>
        /// normalises the values in a matrix such that the minimum value
        /// is the average of the edge values.
        /// Truncate thos original values that are below the edge average.
        /// This method is used to NormaliseMatrixValues image templates where there should be no power at the edge.
        /// </summary>
        public static double[,] Normalise_zeroEdge(double[,] m, double normMin, double normMax)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double edge = 0.0;
            for (int i = 0; i < rows; i++)
            {
                edge += m[i, 0] + m[i, cols - 1]; //sum the sides
            }

            for (int i = 1; i < cols - 1; i++)
            {
                edge += m[0, i] + m[rows - 1, i]; //sum top and bottom
            }

            edge /= (2 * rows) + (2 * cols) - 4;

            //truncate everything to edge average
            //for (int i = 0; i < rows; i++)
            //    for (int j = 0; j < cols; j++) if (m[i, j] < edge) m[i, j] = edge;
            //set all edges to edge average
            for (int i = 0; i < rows; i++)
            {
                m[i, 0] = edge;
            }

            for (int i = 0; i < rows; i++)
            {
                m[i, cols - 1] = edge;
            }

            for (int i = 1; i < cols - 1; i++)
            {
                m[0, i] = edge;
            }

            for (int i = 1; i < cols - 1; i++)
            {
                m[rows - 1, i] = edge;
            }

            //find min and max
            double min = double.MaxValue;
            double max = -double.MaxValue;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (m[i, j] > max)
                    {
                        max = m[i, j];
                    }

                    if (m[i, j] < min)
                    {
                        min = m[i, j];
                    }
                }
            }

            double range = max - min;
            double normRange = normMax - normMin;

            // LoggedConsole.WriteLine("range ="+ range+"  normRange="+normRange);
            double[,] ret = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    double norm01 = (m[i, j] - min) / range;
                    ret[i, j] = normMin + (norm01 * normRange);
                }
            }

            return ret;
        }

        //***************************************************************************************************************************************

        /// <summary>
        /// returns the min and max values in a matrix of doubles.
        /// </summary>
        public static void MinMax(double[,] data, out double min, out double max)
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
        /// returns the min and max values in a matrix.
        /// </summary>
        public static void MinMax(int[,] data, out int min, out int max)
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

        //=============================================================================

        public static double[,] SmoothColumns(double[,] matrix, int window)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            var smoothMatrix = new double[rows, cols];
            for (int c = 0; c < cols; c++)
            {
                var array = DataTools.GetColumn(matrix, c);
                array = DataTools.filterMovingAverage(array, window);
                DataTools.SetColumn(smoothMatrix, c, array);
            }

            return smoothMatrix;
        }

        public static double[,] SmoothRows(double[,] matrix, int window)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            var smoothMatrix = new double[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                var row = DataTools.GetRow(matrix, r);
                row = DataTools.filterMovingAverage(row, window);
                SetRow(smoothMatrix, r, row);
            }

            return smoothMatrix;
        }

        //=============================================================================

        /// <summary>
        /// REMOVE ORPHAN PEAKS.
        /// </summary>
        public static byte[,] RemoveOrphanOnesInBinaryMatrix(byte[,] binary)
        {
            int rows = binary.GetLength(0);
            int cols = binary.GetLength(1);
            byte[,] newM = new byte[rows, cols];
            const double tolerance = 0.00001;

            //row at a time, each row = one frame.
            for (int r = 1; r < rows - 1; r++)
            {
                for (int c = 1; c < cols - 1; c++)
                {
                    if (Math.Abs(binary[r, c]) < 0.000001)
                    {
                        continue;
                    }

                    newM[r, c] = 1;
                    if (binary[r - 1, c] == 0 && Math.Abs(binary[r + 1, c]) < tolerance && binary[r + 1, c + 1] == 0
                        && Math.Abs(binary[r, c + 1]) < tolerance && Math.Abs(binary[r - 1, c + 1]) < tolerance && binary[r + 1, c - 1] == 0
                        && Math.Abs(binary[r, c - 1]) < tolerance && Math.Abs(binary[r - 1, c - 1]) < tolerance)
                    {
                        newM[r, c] = 0;
                    }
                }
            }

            return newM;
        }

        public static byte[,] ThresholdBinarySpectrum(byte[,] binary, double[,] m, double threshold)
        {
            int rows = binary.GetLength(0);
            int cols = binary.GetLength(1);
            byte[,] mOut = new byte[rows, cols];

            //row at a time, each row = one frame.
            for (int r = 1; r < rows - 1; r++)
            {
                for (int c = 1; c < cols - 1; c++)
                {
                    //LoggedConsole.WriteLine("m[r, c]=" + m[r, c]);
                    if (binary[r, c] == 0)
                    {
                        continue;
                    }

                    if (m[r, c] < threshold)
                    {
                        mOut[r, c] = 0;
                    }
                    else
                    {
                        mOut[r, c] = 1;
                    }
                }
            }

            return mOut;
        }

        //public static byte[,] PickOutLines(byte[,] binary)
        //{
        //    int N = 7;
        //    int L = N - 1;
        //    int side = N / 2;
        //    int threshold = N - 1; //6 out 7 matches required

        //    //initialise the syntactic elements - four straight line segments
        //    int[,] LH00 = new int[2, L]; //{ {0,0,0,0,0,0 }, {-3,-2,-1,1,2,3 } };
        //    for (int i = 0; i < L; i++)
        //    {
        //        LH00[0, i] = 0;
        //    }

        //    for (int i = 0; i < side; i++)
        //    {
        //        LH00[1, i] = i - side;
        //    }
        //    for (int i = 0; i < side; i++)
        //    {
        //        LH00[1, side + i] = i + 1;
        //    }
        //    int[,] LV90 = new int[2, L]; // = { { -3, -2, -1, 1, 2, 3 }, { 0, 0, 0, 0, 0, 0 } };
        //    for (int i = 0; i < L; i++)
        //    {
        //        LV90[1, i] = 0;
        //    }
        //    for (int i = 0; i < side; i++)
        //    {
        //        LV90[0, i] = i - side;
        //    }
        //    for (int i = 0; i < side; i++)
        //    {
        //        LV90[0, side + i] = i + 1;
        //    }
        //    // int[,] Lp45 = { { 3, 2, 1, -1, -2, -3 }, { -3, -2, -1, 1, 2, 3 } };
        //    // int[,] Lm45 = { { -3, -2, -1, 1, 2, 3 }, { -3, -2, -1, 1, 2, 3 } };
        //    int rows = binary.GetLength(0);
        //    int cols = binary.GetLength(1);
        //    byte[,] op = new byte[rows, cols];
        //    //row at a time, each row = one frame.
        //    for (int r = side; r < rows - side; r++)
        //    {
        //        for (int c = side; c < cols - side; c++)
        //        {
        //            int HL00sum = binary[r, c];
        //            int VL90sum = binary[r, c];
        //            int Lm45sum = binary[r, c];
        //            int Lp45sum = binary[r, c];
        //            for (int i = 0; i < L; i++)
        //            {
        //                if (binary[r + LH00[0, i], c + LH00[1, i]] == 1)
        //                {
        //                    HL00sum++;
        //                }
        //                if (binary[r + LV90[0, i], c + LV90[1, i]] == 1)
        //                {
        //                    VL90sum++;
        //                }
        //                // if (binary[r + Lm45[0, i], c + Lm45[1, i]] == 1) Lm45sum++;
        //                // if (binary[r + Lp45[0, i], c + Lp45[1, i]] == 1) Lp45sum++;
        //            }
        //            int[] scores = new int[4];
        //            scores[0] = HL00sum;
        //            scores[1] = Lp45sum;
        //            scores[2] = VL90sum;
        //            scores[3] = Lm45sum;
        //            DataTools.getMaxIndex(scores, out int maxIndex);
        //            if ((maxIndex == 0) && (HL00sum >= threshold))
        //            {
        //                for (int i = 0; i < L; i++)
        //                {
        //                    op[r + LH00[0, i], c + LH00[1, i]] = 1;
        //                }
        //            }
        //            //if ((maxIndex == 1) && (Lp45sum >= threshold))
        //            //{
        //            //    for (int i = 0; i < L; i++) op[r + Lp45[0, i], c + Lp45[1, i]] = 1;
        //            //}
        //            if ((maxIndex == 2) && (VL90sum >= threshold))
        //            {
        //                for (int i = 0; i < L; i++)
        //                {
        //                    op[r + LV90[0, i], c + LV90[1, i]] = 1;
        //                }
        //            }
        //            //if ((maxIndex == 3) && (Lm45sum >= threshold))
        //            //{
        //            //    for (int i = 0; i < L; i++) op[r + Lm45[0, i], c + Lm45[1, i]] = 1;
        //            //}
        //        }
        //    }
        //    return op;
        //}

        /// <summary>
        /// Writes the r, c location of the maximum valuesin the matrix.
        /// </summary>
        public static void WriteLocationOfMaximumValues(double[,] m)
        {
            int rowCount = m.GetLength(0);
            int colCount = m.GetLength(1);
            double max = -double.MaxValue;

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    if (m[r, c] > max)
                    {
                        max = m[r, c];
                        Console.WriteLine("Max value to now at r={0}, c={1},  value={2}", r, c, m[r, c]);
                    }
                }
            }
        }

        /*
         * Returns the max value in each row of passed matrix
         */
        public static double[] GetMaximumRowValues(double[,] m)
        {
            int rowCount = m.GetLength(0);
            int colCount = m.GetLength(1);
            double[] returnV = new double[rowCount];

            for (int r = 0; r < rowCount; r++)
            {
                var max = -double.MaxValue;
                for (int c = 0; c < colCount; c++)
                {
                    if (m[r, c] > max)
                    {
                        max = m[r, c];
                    }
                }

                returnV[r] = max;
            }

            return returnV;
        }

        /*
         * Returns the max value in each row of passed matrix
         */
        public static double[] GetMaximumColumnValues(double[,] m)
        {
            int rowCount = m.GetLength(0);
            int colCount = m.GetLength(1);
            double[] returnV = new double[colCount];

            for (int c = 0; c < colCount; c++)
            {
                var max = -double.MaxValue;
                for (int r = 0; r < rowCount; r++)
                {
                    if (m[r, c] > max)
                    {
                        max = m[r, c];
                    }
                }

                returnV[c] = max;
            }

            return returnV;
        }

        public static void PrintMatrix(double[,] matrix)
        {
            var rowLength = matrix.GetLength(1);
            for (var i = 0; i < matrix.GetLength(0); i++)
            {
                var row = new StringBuilder(rowLength * 6);

                for (int j = 0; j < rowLength; j++)
                {
                    row.Append(matrix[i, j]);
                    row.Append(" | ");
                }

                Debug.WriteLine(rowLength);
            }
        }
    }
}
