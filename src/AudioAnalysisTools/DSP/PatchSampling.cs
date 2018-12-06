// <copyright file="PatchSampling.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accord.Math;
    using TowseyLibrary;
    using WavTools;

    public static class PatchSampling
    {
        /// <summary>
        /// sample a set of patches ("sequential" or "random" or "overlapped random") from a spectrogram
        /// in "sequential" mode, it generates non-overlapping patches from the whole input matrix, and
        /// in this case the "numOfPatches" can be simply set to zero.
        /// However, in "random" mode, the method requires an input for the "numOfPatches" parameter.
        /// </summary>

        /// <summary>
        /// The sampling method.
        /// </summary>
        public enum SamplingMethod
        {
            /// <summary>
            /// Sequential patches.
            /// </summary>
            Sequential = 0,

            /// <summary>
            /// Random Patches.
            /// </summary>
            Random = 1,

            /// <summary>
            /// Overlapping Random Patches.
            /// </summary>
            OverlappedRandom = 2,
        }

        public static double[][] GetPatches(double[,] spectrogram, int patchWidth, int patchHeight, int numberOfPatches, SamplingMethod samplingMethod)
        {
            List<double[]> patches = new List<double[]>();
            if (samplingMethod == SamplingMethod.Sequential)
            {
                patches = GetSequentialPatches(spectrogram, patchWidth, patchHeight);
            }
            else
            {
                if (samplingMethod == SamplingMethod.Random)
                {
                    patches = GetRandomPatches(spectrogram, patchWidth, patchHeight, numberOfPatches);
                }
                else
                {
                    if (samplingMethod == SamplingMethod.OverlappedRandom)
                    {
                       patches = GetOverlappedRandomPatches(spectrogram, patchWidth, patchHeight, numberOfPatches);
                    }
                }
            }

            return patches.ToArray();
        }

        /// <summary>
        /// converts a set of patches to a matrix of original size after applying pca.
        /// the assumption here is that the input matrix is a sequential non-overlapping patches.
        /// </summary>
        public static double[,] ConvertPatches(double[,] whitenedPatches, int patchWidth, int patchHeight, int columnSize)
        {
            int ht = whitenedPatches.GetLength(0);
            double[][] patches = whitenedPatches.ToJagged();
            List<double[,]> allPatches = new List<double[,]>();

            for (int row = 0; row < ht; row++)
            {
                allPatches.Add(MatrixTools.ArrayToMatrixByColumn(patches[row], patchWidth, patchHeight));
            }

            double[,] matrix = ConcatenateGridOfPatches(allPatches, columnSize, patchWidth, patchHeight);

            return matrix;
        }

        /// <summary>
        /// construct the original matrix from a list of sequential patches
        /// all vectors in list are of the same length
        /// </summary>
        public static double[,] ConcatenateGridOfPatches(List<double[,]> list, int columnSize, int patchWidth, int patchHeight)
        {
            double[][,] arrayOfPatches = list.ToArray();

            // number of patches
            int rows = list.Count;
            int numberOfItemsInRow = columnSize / patchWidth;
            int numberOfItemsInColumn = rows / numberOfItemsInRow;
            double[,] matrix = new double[numberOfItemsInColumn * patchHeight, numberOfItemsInRow * patchWidth];

            // the number of patches in each row of the matrix
            for (int i = 0; i < numberOfItemsInColumn; i++)
            {
                // the number of patches in each column of the matrix
                for (int j = 0; j < numberOfItemsInRow; j++)
                {

                    // the id of patch is equal to (i * numberOfItemsInRow) + j
                    for (int r = 0; r < list[(i * numberOfItemsInRow) + j].GetLength(0); r++)
                    {
                        for (int c = 0; c < list[(i * numberOfItemsInRow) + j].GetLength(1); c++)
                        {
                            matrix[r + (i * patchHeight), c + (j * patchWidth)] = arrayOfPatches[(i * numberOfItemsInRow) + j][r, c];
                        }
                    }
                }
            }

            return matrix;
        }

        /// <summary>
        /// converts a spectrogram matrix to submatrices by dividing the column of input matrix to
        /// different freq bands with equal size. Output submatrices have same number of rows and same number 
        /// of columns. numberOfBands as an input parameter indicates how many output bands are needed.
        /// </summary>
        public static List<double[,]> GetFreqBandMatrices(double[,] matrix, int numberOfBands)
        {
            List<double[,]> allSubmatrices = new List<double[,]>();

            // number of freq bins
            int columns = matrix.GetLength(1);
            int rows = matrix.GetLength(0);
            int newColumn = columns / numberOfBands;

            int bandId = 0;
            while (bandId < numberOfBands)
            {
                double[,] submatrix = new double[rows, newColumn];
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < newColumn; j++)
                    {
                        submatrix[i, j] = matrix[i, j + (newColumn * bandId)];
                    }
                }

                allSubmatrices.Add(submatrix);
                bandId++;
            }

            // Note that if we want the first 1/4 as the lower band, the second and third 1/4 as the mid,
            // and the last 1/4 is the upper freq band, we need to use the commented part of the code.
            /*
            double[,] minFreqBandMatrix = new double[rows, newColumn];
            double[,] maxFreqBandMatrix = new double[rows, newColumn];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < newColumn; j++)
                {
                    minFreqBandMatrix[i, j] = matrix[i, j];
                }
            }

            allSubmatrices.Add(minFreqBandMatrix);

            double[,] midFreqBandMatrix = new double[rows, newColumn * 2];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < newColumn * 2; j++)
                {
                    midFreqBandMatrix[i, j] = matrix[i, j + newColumn];
                }
            }

            allSubmatrices.Add(midFreqBandMatrix);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < newColumn; j++)
                {
                    maxFreqBandMatrix[i, j] = matrix[i, j + newColumn * 3];
                }
            }

            allSubmatrices.Add(maxFreqBandMatrix);
            */
            return allSubmatrices;
        }

        /// <summary>
        /// outputs a matrix with arbitrary minimum and maximum frequency bins.
        /// </summary>
        public static double[,] GetArbitraryFreqBandMatrix(double[,] matrix, int minFreqBin, int maxFreqBin)
        {
            double[,] outputMatrix = new double[matrix.GetLength(0), maxFreqBin - minFreqBin + 1];

            int minColumnIndex = minFreqBin - 1;
            int maxColumnIndex = maxFreqBin - 1;

            // copying a part of the original matrix with pre-defined boundaries to Y axis (freq bins) to a new matrix
            for (int col = minColumnIndex; col <= maxColumnIndex; col++)
            {
                for (int row = 0; row < matrix.GetLength(0); row++)
                {
                    outputMatrix[row, col - minColumnIndex] = matrix[row, col];
                }
            }

            return outputMatrix;
        }

        /// <summary>
        /// concatenate submatrices column-wise into one matrix, i.e., the number of rows for the output matrix
        /// is equal to the number of rows of each of the frequency band matrices.
        /// </summary>
        public static double[,] ConcatFreqBandMatrices(List<double[,]> submatrices)
        {
            // The assumption here is all frequency band matrices have the same number of columns.
            int columnSize = submatrices.Count * submatrices[0].GetLength(1);
            int rowSize = submatrices[0].GetLength(0);
            double[,] matrix = new double[rowSize, columnSize];
            int count = 0;
            while (count < submatrices.Count)
            {
                matrix.AddToArray(submatrices[count], DoubleSquareArrayExtensions.MergingDirection.Column, submatrices[count].GetLength(1) * count);
                count++;
            }

            // If we have frequency band matrices with different number of columns,
            // Then the below commented code need to be used.
            /*
            double[][,] submatrix = submatrices.ToArray();
            int colSize = 0;
            for (int i = 0; i < submatrix.Length; i++)
            {
                colSize = colSize + submatrix[i].GetLength(1);
            }

            // storing the number of rows of each submatrice in an array
            int[] noRows = new int[submatrix.Length];
            for (int i = 0; i < submatrix.Length; i++)
            {
                noRows[i] = submatrix[i].GetLength(0);
            }

            // find the max number of rows from noRows array
            int maxRows = noRows.Max();

            double[,] matrix = new double[maxRows, colSize];

            // might be better way to do this
            AddToArray(matrix, submatrix[0], "column");
            AddToArray(matrix, submatrix[1], "column", submatrix[0].GetLength(1));
            AddToArray(matrix, submatrix[2], "column", submatrix[0].GetLength(1) + submatrix[1].GetLength(1));
            */

            return matrix;
        }

        /// <summary>
        /// convert a list of patch matrices to one matrix
        /// </summary>
        public static double[,] ListOf2DArrayToOne2DArray(List<double[,]> listOfPatchMatrices)
        {
            int numberOfPatches = listOfPatchMatrices[0].GetLength(0);
            double[,] allPatchesMatrix = new double[listOfPatchMatrices.Count * numberOfPatches, listOfPatchMatrices[0].GetLength(1)];
            for (int i = 0; i < listOfPatchMatrices.Count; i++)
            {
                var m = listOfPatchMatrices[i];
                if (m.GetLength(0) != numberOfPatches)
                {
                    throw new ArgumentException("All arrays must be the same length");
                }

                allPatchesMatrix.AddToArray(m, DoubleSquareArrayExtensions.MergingDirection.Row, i * m.GetLength(0));
            }

            return allPatchesMatrix;
        }

        /// <summary>
        /// Adding a row of zero/one to 2D array
        /// </summary>
        public static double[,] AddRow(double[,] matrix)
        {
            double[,] newMatrix = new double[matrix.GetLength(0) + 1, matrix.GetLength(1)];
            double[] newArray = new double[matrix.GetLength(1)];

            int minX = matrix.GetLength(0);
            int minY = matrix.GetLength(1);

            // copying the original matrix to a new matrix (row by row)

            for (int i = 0; i < minX; ++i)
            {
                Array.Copy(matrix, i * matrix.GetLength(1), newMatrix, i * matrix.GetLength(1), minY);
            }

            // creating an array of "1.0" or "0.0"
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                newArray[j] = 1.0;
            }

            // convert the new array to a matrix
            double[,] matrix2 = MatrixTools.ArrayToMatrixByColumn(newArray, newArray.Length, 1);
            int minX2 = matrix2.GetLength(0);
            int minY2 = matrix2.GetLength(1);

            // copying the array of one or zero to the last row of the new matrix
            for (int i = 0; i < minX2; ++i)
            {
                Array.Copy(matrix2, i * matrix2.GetLength(1), newMatrix, minX * minY, minY2);
            }

            return newMatrix;
        }

        /// <summary>
        /// Generate non-overlapping sequential patches from a <paramref name="matrix"/>
        /// </summary>
        private static List<double[]> GetSequentialPatches(double[,] matrix, int patchWidth, int patchHeight)
        {
            List<double[]> patches = new List<double[]>();

            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);
            for (int r = 0; r < rows / patchHeight; r++)
            {
                for (int c = 0; c < columns / patchWidth; c++)
                {
                    double[,] submatrix = MatrixTools.Submatrix(matrix, r * patchHeight, c * patchWidth,
                        (r * patchHeight) + patchHeight - 1, (c * patchWidth) + patchWidth - 1);

                    // convert a matrix to a vector by concatenating columns and
                    // store it to the array of vectors
                    patches.Add(MatrixTools.Matrix2Array(submatrix));
                }
            }

            return patches;
        }

        /// <summary>
        /// Generate non-overlapping random patches from a matrix
        /// </summary>
        private static List<double[]> GetRandomPatches(double[,] matrix, int patchWidth, int patchHeight, int numberOfPatches)
        {
            // Note: to make the method more flexible in terms of selecting a random patch with any height and width,
            // first a random number generator is defined for both patchHeight and patchWidth.
            // However, the possibility of selecting duplicates especially when selecting too many random numbers from
            // a range (e.g., 1000 out of 1440) is high with a a random generator.
            // Since, we are mostly interested in full-band patches, i.e., patchWidth = (maxFreqBin - minFreqBin + 1) / numFreqBand,
            // it is important to select non-duplicate patchHeights. Hence, instead of a random generator for patchHeight,
            // a better solution is to make a sequence of numbers to be selected, shuffle them, and
            // finally, a first n (number of required patches) numbers could be selected.

            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);

            int seed = 100;
            Random randomNumber = new Random(seed);

            // not sure whether it is better to use new Guid() instead of randomNumber.Next()
            var randomRowNumbers = Enumerable.Range(0, rows - patchHeight).OrderBy(x => randomNumber.Next()).Take(numberOfPatches).ToList();
            List<double[]> patches = new List<double[]>();

            for (int i = 0; i < randomRowNumbers.Count; i++) //for (int i = 0; i < numberOfPatches; i++)
            {
                // selecting a random number from the height of the matrix
                //int rowRandomNumber = randomNumber.Next(0, rows - patchHeight);

                // selecting a random number from the width of the matrix
                int columnRandomNumber = randomNumber.Next(0, columns - patchWidth);

                double[,] submatrix = MatrixTools.Submatrix(matrix, randomRowNumbers[i], columnRandomNumber,
                    randomRowNumbers[i] + patchHeight - 1, columnRandomNumber + patchWidth - 1);

                //double[,] submatrix = MatrixTools.Submatrix(matrix, rowRandomNumber, columnRandomNumber, rowRandomNumber + patchHeight - 1, columnRandomNumber + patchWidth - 1);

                // convert a matrix to a vector by concatenating columns and
                // store it to the array of vectors
                patches.Add(MatrixTools.Matrix2Array(submatrix));
            }

            return patches;
        }

        /// <summary>
        /// Generate overlapped random patches from a matrix
        /// </summary>
        private static List<double[]> GetOverlappedRandomPatches(double[,] matrix, int patchWidth, int patchHeight, int numberOfPatches)
        {
            int seed = 100;
            Random randomNumber = new Random(seed);
            List<double[]> patches = new List<double[]>();

            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);
            int no = 0;
            while (no < numberOfPatches)
            {
                // First select a random patch
                // selecting a random number from the height of the matrix
                int rowRandomNumber = randomNumber.Next(0, rows - patchHeight);

                // selecting a random number from the width of the matrix
                int columnRandomNumber = randomNumber.Next(0, columns - patchWidth);
                double[,] submatrix = MatrixTools.Submatrix(matrix, rowRandomNumber, columnRandomNumber,
                    rowRandomNumber + patchHeight - 1, columnRandomNumber + patchWidth - 1);

                // convert a matrix to a vector by concatenating columns and
                // store it to the array of vectors
                patches.Add(MatrixTools.Matrix2Array(submatrix));
                no++;

                // shifting the row by one
                // note that if we select full band patches, then we don't need to shift the column.
                rowRandomNumber = rowRandomNumber + 1;

                // Second, slide the patch window (rowRandomNumber + 1) to select the next patch
                double[,] submatrix2 = MatrixTools.Submatrix(matrix, rowRandomNumber, columnRandomNumber,
                    rowRandomNumber + patchHeight - 1, columnRandomNumber + patchWidth - 1);
                patches.Add(MatrixTools.Matrix2Array(submatrix2));
                no++;

                // The below commented code can be used when shifting the row by three
                /*
                rInt = rInt + 2;
                // Second, slide the patch window (rowRandomNumber+1) to select the next patch
                double[,] submatrix3 = MatrixTools.Submatrix(spectrogram, rowRandomNumber, columnRandomNumber,
                    rowRandomNumber + patchHeight - 1, columnRandomNumber + patchWidth - 1);
                patches.Add(MatrixTools.MatrixToArray(submatrix3));
                no++;
                */
            }

            return patches;
        }

        /// <summary>
        /// cut audio to subsegments of desired length.
        /// return list of subsegments
        /// </summary>
        public static List<AudioRecording> GetSubsegmentsSamples(AudioRecording recording, double subsegmentDurationInSeconds, double frameStep)
        {
            List<AudioRecording> subsegments = new List<AudioRecording>();

            int sampleRate = recording.WavReader.SampleRate;
            var segmentDuration = recording.WavReader.Time.TotalSeconds;
            int segmentSampleCount = (int)(segmentDuration * sampleRate);
            int subsegmentSampleCount = (int)(subsegmentDurationInSeconds * sampleRate);
            double subsegmentFrameCount = subsegmentSampleCount / (double)frameStep;
            subsegmentFrameCount = (int)subsegmentFrameCount; //(int)Math.Ceiling(subsegmentFrameCount)
            subsegmentSampleCount = ((int)(subsegmentFrameCount * frameStep) < subsegmentSampleCount) ? subsegmentSampleCount : (int)(subsegmentFrameCount * frameStep);

            for (int i = 0; i < (int)(segmentSampleCount / subsegmentSampleCount); i++)
            {
                AudioRecording subsegmentRecording = recording;
                double[] subsamples = DataTools.Subarray(recording.WavReader.Samples, i * subsegmentSampleCount, subsegmentSampleCount);
                var wr = new Acoustics.Tools.Wav.WavReader(subsamples, 1, 16, sampleRate);
                subsegmentRecording = new AudioRecording(wr);
                subsegments.Add(subsegmentRecording);
            }

            return subsegments;
        }
    }
}