// <copyright file="TemporalMatrix.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class TemporalMatrix
    {
        public string TemporalDirection { get; set; }

        public TimeSpan DataScale { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public double[,] Matrix { get; set; }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="temporalDirection"></param>
        /// <param name="m"></param>
        /// <param name="dataScale"></param>
        public TemporalMatrix(string temporalDirection, double[,] m, TimeSpan dataScale)
        {
            if (temporalDirection.Equals("rows") || temporalDirection.Equals("columns"))
            {
                this.TemporalDirection = temporalDirection;
            }
            else
            {
                this.TemporalDirection = null;
                LoggedConsole.WriteErrorLine("temporalDirection can have only one of two values: <rows> or <columns>. ");
                throw new Exception();
            }

            this.DataScale = dataScale;
            this.Matrix = m;
        }

        public TimeSpan DataDuration()
        {
            if (this.TemporalDirection.Equals("rows")) // orientation is 90 degrees clockwise from standard visual orientation
            {
                return TimeSpan.FromTicks(this.Matrix.GetLength(0) * this.DataScale.Ticks);
            }
            else if (this.TemporalDirection.Equals("columns")) // orientation is standard visual orientation
            {
                return TimeSpan.FromTicks(this.Matrix.GetLength(1) * this.DataScale.Ticks);
            }

            return TimeSpan.Zero;
        }

        /// <summary>
        /// changes temporal dimension from rows to columns or vice-versa
        /// </summary>
        public void SwapTemporalDimension()
        {
            if (this.TemporalDirection.Equals("rows")) // orientation is 90 degrees clockwise from standard visual orientation
            {
                this.TemporalDirection = "columns";
                this.Matrix = MatrixTools.MatrixRotate90Anticlockwise(this.Matrix);
            }
            else if (this.TemporalDirection.Equals("columns")) // orientation is standard visual orientation
            {
                this.TemporalDirection = "rows";
                this.Matrix = MatrixTools.MatrixRotate90Clockwise(this.Matrix);
            }
        }

        public double[,] GetDataBlock(TimeSpan startTime, TimeSpan blockDuration)
        {
            int frameCount = (int)(blockDuration.Ticks / this.DataScale.Ticks);
            return this.GetDataBlock(startTime, frameCount);
        }

        public double[,] GetDataBlock(TimeSpan startTime, int frameCount)
        {
            this.StartTime = startTime;
            if (this.StartTime < TimeSpan.Zero)
            {
                this.StartTime = TimeSpan.Zero;
            }

            int startIndex = (int)(this.StartTime.Ticks / this.DataScale.Ticks);

            int endIndex = startIndex + frameCount - 1;

            //TimeSpan endTime = startTime + ImageDuration;
            //if (endTime > dataDuration) endTime = dataDuration;
            //int endIndex = (int)(endTime.Ticks / dataScale.Ticks);

            int rowCount = this.Matrix.GetLength(0);
            int columnCount = this.Matrix.GetLength(1);
            double[,] returnMatrix = null;

            if (this.TemporalDirection.Equals("rows")) // orientation is 90 degrees clockwise from standard visual orientation
            {
                if (endIndex >= rowCount)
                {
                    endIndex = rowCount - 1;
                }

                returnMatrix = MatrixTools.Submatrix(this.Matrix, startIndex, 0, endIndex, columnCount - 1);
            }
            else if (this.TemporalDirection.Equals("columns")) // orientation is standard visual orientation
            {
                if (endIndex >= columnCount)
                {
                    endIndex = columnCount - 1;
                }

                returnMatrix = MatrixTools.Submatrix(this.Matrix, 0, startIndex, rowCount - 1, endIndex);
            }

            return returnMatrix;
        }

        public double[,] ExpandSubmatrixInTemporalDirection(TimeSpan startTime, TimeSpan blockDuration, TimeSpan newScale)
        {
            double[,] subMatrix = this.GetDataBlock(startTime, blockDuration);

            int scalingFactor = (int)Math.Round(this.DataScale.TotalMilliseconds / newScale.TotalMilliseconds);
            if (scalingFactor <= 1)
            {
                return subMatrix;
            }

            //int step = scalingFactor - 1;
            int rowCount = subMatrix.GetLength(0);
            int colCount = subMatrix.GetLength(1);
            double[,] newMatrix = null;

            if (this.TemporalDirection.Equals("rows")) // orientation is 90 degrees clockwise from standard visual orientation
            {
                int expandedLength = rowCount * scalingFactor;
                newMatrix = new double[expandedLength, colCount];

                for (int c = 0; c < colCount; c++)
                {
                    int rowIndex = 0;
                    for (int r = 0; r < rowCount; r++)
                    {
                        rowIndex = r * scalingFactor;
                        for (int i = 0; i < scalingFactor; i++)
                        {
                            newMatrix[rowIndex + i, c] = subMatrix[r, c];
                        }
                    }
                }
            }
            else if (this.TemporalDirection.Equals("columns")) // orientation is standard visual orientation
            {
                int expandedLength = colCount * scalingFactor;
                newMatrix = new double[rowCount, expandedLength];

                for (int r = 0; r < rowCount; r++)
                {
                    int colIndex = 0;
                    for (int c = 0; c < colCount; c++)
                    {
                        colIndex = c * scalingFactor;
                        for (int i = 0; i < scalingFactor; i++)
                        {
                            newMatrix[r, colIndex + i] = subMatrix[r, c];
                        }
                    }
                }
            }

            return newMatrix;
        }

        public double[,] CompressMatrixInTemporalDirectionByTakingAverage(TimeSpan newScale)
        {
            int compressionFactor = (int)Math.Round(newScale.TotalMilliseconds / this.DataScale.TotalMilliseconds);
            if (compressionFactor <= 1)
            {
                return this.Matrix;
            }

            int step = compressionFactor - 1;

            //int step = scalingFactor - 1;
            int rowCount = this.Matrix.GetLength(0);
            int colCount = this.Matrix.GetLength(1);
            double[,] newMatrix = null;
            int compressedLength = 0;

            if (this.TemporalDirection.Equals("rows")) // orientation is 90 degrees clockwise from standard visual orientation
            {
                compressedLength = rowCount / compressionFactor;
                newMatrix = new double[compressedLength, colCount];
                double[] tempArray = new double[compressionFactor];

                for (int c = 0; c < colCount; c++)
                {
                    int rowIndex = 0;
                    for (int r = 0; r < rowCount - compressionFactor; r += step)
                    {
                        rowIndex = r / compressionFactor;
                        for (int i = 0; i < compressionFactor; i++)
                        {
                            tempArray[i] = this.Matrix[r + i, c];
                        }

                        newMatrix[rowIndex, c] = tempArray.Average();
                    }
                }
            }
            else if (this.TemporalDirection.Equals("columns")) // orientation is standard visual orientation
            {
                compressedLength = colCount / compressionFactor;
                newMatrix = new double[rowCount, compressedLength];
                double[] tempArray = new double[compressionFactor];

                for (int r = 0; r < rowCount; r++)
                {
                    int colIndex = 0;
                    for (int c = 0; c < colCount - compressionFactor; c += step)
                    {
                        colIndex = c / compressionFactor;
                        for (int i = 0; i < compressionFactor; i++)
                        {
                            tempArray[i] = this.Matrix[r, c + i];
                        }

                        newMatrix[r, colIndex] = tempArray.Average();
                    }
                }
            }

            return newMatrix;
        }

        public double[,] CompressMatrixInTemporalDirectionByTakingMax(TimeSpan newScale)
        {
            int compressionFactor = (int)Math.Round(newScale.TotalMilliseconds / this.DataScale.TotalMilliseconds);
            if (compressionFactor <= 1)
            {
                return this.Matrix;
            }

            int step = compressionFactor - 1;

            //int step = scalingFactor - 1;
            int rowCount = this.Matrix.GetLength(0);
            int colCount = this.Matrix.GetLength(1);
            double[,] newMatrix = null;
            int compressedLength = 0;

            if (this.TemporalDirection.Equals("rows")) // orientation is 90 degrees clockwise from standard visual orientation
            {
                compressedLength = rowCount / compressionFactor;
                newMatrix = new double[compressedLength, colCount];
                double[] tempArray = new double[compressionFactor];

                for (int c = 0; c < colCount; c++)
                {
                    int rowIndex = 0;
                    for (int r = 0; r < rowCount - compressionFactor; r += step)
                    {
                        rowIndex = r / compressionFactor;
                        for (int i = 0; i < compressionFactor; i++)
                        {
                            tempArray[i] = this.Matrix[r + i, c];
                        }

                        newMatrix[rowIndex, c] = tempArray.Max();
                    }
                }
            }
            else if (this.TemporalDirection.Equals("columns")) // orientation is standard visual orientation
            {
                compressedLength = colCount / compressionFactor;
                newMatrix = new double[rowCount, compressedLength];
                double[] tempArray = new double[compressionFactor];

                for (int r = 0; r < rowCount; r++)
                {
                    int colIndex = 0;
                    for (int c = 0; c < colCount - compressionFactor; c += step)
                    {
                        colIndex = c / compressionFactor;
                        for (int i = 0; i < compressionFactor; i++)
                        {
                            tempArray[i] = this.Matrix[r, c + i];
                        }

                        newMatrix[r, colIndex] = tempArray.Max();
                    }
                }
            }

            return newMatrix;
        }

        /// <summary>
        /// This method assumes that the matrix spectrograms are oriented so that the rows = spectra
        /// and the columns = freq bins, i.e. rotated 90 degrees from normal orientation.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="compressionFactor"></param>
        /// <returns></returns>
        public static double[,] CompressFrameSpectrograms(double[,] matrix, int compressionFactor)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            int compressedLength = rowCount / compressionFactor;
            var newMatrix = new double[compressedLength, colCount];
            double[] tempArray = new double[compressionFactor];
            int step = compressionFactor - 1;
            double average = 0.0;
            double difference = 0.0;
            int maxCompressionFactor = 6;
            double relativeCompression = compressionFactor / (double)maxCompressionFactor;
            if (relativeCompression > 1.0)
            {
                relativeCompression = 1.0;
            }

            for (int c = 0; c < colCount; c++)
            {
                int rowIndex = 0;
                for (int r = 0; r < rowCount - compressionFactor; r += step)
                {
                    rowIndex = r / compressionFactor;
                    for (int i = 0; i < compressionFactor; i++)
                    {
                        tempArray[i] = matrix[r + i, c];
                    }

                    average = tempArray.Average();
                    difference = tempArray.Max() - average;
                    newMatrix[rowIndex, c] = average + (difference * relativeCompression);
                }
            }

            return newMatrix;
        }
    }
}
