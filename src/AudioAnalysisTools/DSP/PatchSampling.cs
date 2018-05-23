// <copyright file="PatchSampling.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using Accord.Math;
    using TowseyLibrary;

    public static class PatchSampling
    {
        /*
         * sample a set of patches ("sequential" or "random" or "overlapped random") from a spectrogram
         * in "sequential" mode, it generates non-overlapping patches from the whole input matrix, and
         * in this case the "numOfPatches" can be simply set to zero.
         * However, in "random" mode, the method requires an input for the "numOfPatches" parameter.
        */
        public static double[][] GetPatches(double[,] spectrogram, int patchWidth, int patchHeight, int numOfPatches, string samplingMethod)
        {
            List<double[]> patches = new List<double[]>();

            int rows = spectrogram.GetLength(0);
            int cols = spectrogram.GetLength(1);
            int seed = 100;
            Random rn = new Random(seed);

            if (samplingMethod == "sequential")
            {
                //generate non-overlapping patches
                //convert matrix to submatrix
                for (int r = 0; r < rows / patchHeight; r++)
                {
                    for (int c = 0; c < cols / patchWidth; c++)
                    {
                        double[,] submatrix = MatrixTools.Submatrix(spectrogram, r * patchHeight, c * patchWidth,
                            (r * patchHeight) + patchHeight - 1, (c * patchWidth) + patchWidth - 1);

                        //convert a matrix to a vector by concatenating columns and
                        //store it to the array of vectors
                        patches.Add(MatrixTools.Matrix2Array(submatrix));
                    }
                }
            }
            else
            {
                if (samplingMethod == "random")
                {
                    for (int i = 0; i < numOfPatches; i++)
                    {
                        int rInt = rn.Next(0, rows - patchHeight); //selecting a random number from the height of the matrix
                        int cInt = rn.Next(0, cols - patchWidth); //selecting a random number from the width of the matrix
                        double[,] submatrix = MatrixTools.Submatrix(spectrogram, rInt, cInt,
                            rInt + patchHeight - 1, cInt + patchWidth - 1);

                        //convert a matrix to a vector by concatenating columns and
                        //store it to the array of vectors
                        patches.Add(MatrixTools.Matrix2Array(submatrix));
                    }
                }
                else
                {
                    if (samplingMethod == "overlapped random")
                    {
                        int no = 0;
                        while (no < numOfPatches)
                        {
                            //First select a random patch
                            int rInt = rn.Next(0, rows - patchHeight); //selecting a random number from the height of the matrix
                            int cInt = rn.Next(0, cols - patchWidth); //selecting a random number from the width of the matrix
                            double[,] submatrix = MatrixTools.Submatrix(spectrogram, rInt, cInt, rInt + patchHeight - 1, cInt + patchWidth - 1);

                            //convert a matrix to a vector by concatenating columns and
                            //store it to the array of vectors
                            patches.Add(MatrixTools.Matrix2Array(submatrix));
                            no++;

                            //shifting the row by one
                            //note that we don't shift column as we select full band patches
                            rInt = rInt + 1;

                            //Second, slide the patch window (rInt+1) to select the next patch
                            double[,] submatrix2 = MatrixTools.Submatrix(spectrogram, rInt, cInt,
                                rInt + patchHeight - 1, cInt + patchWidth - 1);
                            patches.Add(MatrixTools.Matrix2Array(submatrix2));
                            no++;

                            /*
                            //shifting the row by three
                            rInt = rInt + 2;
                            //Second, slide the patch window (rInt+1) to select the next patch
                            double[,] submatrix3 = MatrixTools.Submatrix(spectrogram, rInt, cInt,
                                rInt + patchHeight - 1, cInt + patchWidth - 1);
                            patches.Add(MatrixTools.Matrix2Array(submatrix3));
                            no++;
                            */

                        }
                    }
                }
            }

            return patches.ToArray();
        }

        /*
         * converts a set of patches to a matrix of original size after applying pca.
         * the assumption here is that the input matrix is a sequential non-overlapping patches.
        */
        public static double[,] ConvertPatches(double[,] whitenedPatches, int patchWidth, int patchHeight, int colSize)
        {
            int ht = whitenedPatches.GetLength(0);
            double[][] patches = whitenedPatches.ToJagged();
            List<double[,]> allPatches = new List<double[,]>();

            for (int row = 0; row < ht; row++)
            {
                allPatches.Add(Array2Matrix(patches[row], patchWidth, patchHeight, "column"));
            }

            double[,] matrix = ConvertList2Matrix(allPatches, colSize, patchWidth, patchHeight);

            return matrix;
        }

        /*
         * converts a vector to a matrix either in the direction of "column" or "row".
         * For example, the "Matrix2Array" method in MatrixTools.cs builds the vector
         * by concatenating the columns
         */
        public static double[,] Array2Matrix(double[] vector, int patchWidth, int patchHeight, string concatenationDirection)
        {
            double[,] m = new double[patchHeight, patchWidth];

            if (concatenationDirection == "column")
            {
                for (int col = 0; col < vector.Length; col += patchHeight)
                {
                    for (int row = 0; row < patchHeight; row++)
                    {
                        m[row, col / patchHeight] = vector[col + row];
                    }
                }

            }
            else
            {
                if (concatenationDirection == "row")
                {
                    for (int row = 0; row < vector.Length; row += patchWidth)
                    {
                        for (int col = 0;  col < patchWidth; col++)
                        {
                            m[row / patchWidth, col] = vector[col + row];
                        }
                    }
                }
            }

            return m;
        }

        /*
         * converts a list<double[,]> to a matrix.
         * construct the original matrix from a set of sequential patches
         */
        public static double[,] ConvertList2Matrix(List<double[,]> list, int colSize, int patchWidth, int patchHeight)
        {
            double[][,] arrayOfPatches = list.ToArray();
            int rows = list.Count; //number of patches
            //int cols = list[0].Length; //assume all vectors in list are of same length
            int noItemInRow = colSize / patchWidth; 
            int noItemInCol = rows / noItemInRow;
            double[,] mx = new double[noItemInCol * patchHeight, noItemInRow * patchWidth]; 
            for (int i = 0; i < noItemInCol; i++) //the number of patches in each row of the matrix 
            {
                for (int j = 0; j < noItemInRow; j++) //the number of patches in each column of the matrix
                {
                    for (int r = 0; r < list[(i * noItemInRow) + j].GetLength(0); r++) //patch id = (i * noItemInRow) + j
                    {
                        for (int c = 0; c < list[(i * noItemInRow) + j].GetLength(1); c++)
                        {
                            mx[r + (i * patchHeight), c + (j * patchWidth)] = arrayOfPatches[(i * noItemInRow) + j][r, c];
                        }
                    }
                }
            }

            return mx;
        }

        /*
         * converts a spectrogram matrix to submatrices by dividing the column of input matrix to different freq bands with equal size
         * Output submatrices have same number of rows and same number of columns
         * noOfBand as an input parameter indicates how many output bands are needed
         * Note that if we want the first 1/4 as the lower band, the second and third 1/4 as the mid,
         * and the last 1/4 is the upper freq band, we need to use the commented part of the code.
         */
        public static List<double[,]> GetFreqBandMatrices(double[,] matrix, int noOfBands)
        {
            List<double[,]> allSubmatrices = new List<double[,]>();
            int cols = matrix.GetLength(1); //number of freq bins
            int rows = matrix.GetLength(0);
            int newCol = cols / noOfBands;

            int bandId = 0;
            while (bandId < noOfBands)
            {
                double[,] m = new double[rows, newCol];
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < newCol; j++)
                    {
                        m[i, j] = matrix[i, j + (newCol * bandId)];
                    }
                }

                allSubmatrices.Add(m);
                bandId++;
            }

            /*
            double[,] minFreqBandMatrix = new double[rows, newCol];
            double[,] maxFreqBandMatrix = new double[rows, newCol];

            //Note that I am not aware of any faster way to copy a part of 2D-array
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < newCol; j++)
                {
                    minFreqBandMatrix[i, j] = matrix[i, j];
                }
            }

            allSubmatrices.Add(minFreqBandMatrix);

            if (noOfBands == 3)
            {
                double[,] midFreqBandMatrix = new double[rows, newCol * 2];
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < newCol * 2; j++)
                    {
                        midFreqBandMatrix[i, j] = matrix[i, j + newCol];
                    }
                }

                allSubmatrices.Add(midFreqBandMatrix);
            }
            else
            {
                if (noOfBands == 4)
                {
                    double[,] mid1FreqBandMatrix = new double[rows, newCol];
                    double[,] mid2FreqBandMatrix = new double[rows, newCol];
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < newCol; j++)
                        {
                            mid1FreqBandMatrix[i, j] = matrix[i, j + newCol];
                        }
                    }

                    allSubmatrices.Add(mid1FreqBandMatrix);

                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < newCol; j++)
                        {
                            mid2FreqBandMatrix[i, j] = matrix[i, j + newCol * 2];
                        }
                    }

                    allSubmatrices.Add(mid2FreqBandMatrix);
                }
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < newCol; j++)
                {
                    maxFreqBandMatrix[i, j] = matrix[i, j + newCol * 3];
                }
            }

            allSubmatrices.Add(maxFreqBandMatrix);
            */
            return allSubmatrices;
        }

        /*
         * concatenate submatrices column-wise into one matrix, i.e., the number of rows for the output matrix
         * is equal to the number of rows of each of the frequency band matrices.
         */
        public static double[,] ConcatFreqBandMatrices(List<double[,]> submatrices)
        {
            //The assumption here is all frequency band matrices have the same number of columns.
            int colSize = submatrices.Count * submatrices[0].GetLength(1);
            int rowSize = submatrices[0].GetLength(0);
            double[,] matrix = new double[rowSize, colSize];
            int count = 0;
            while (count < submatrices.Count)
            {
                AddToArray(matrix, submatrices[count], "column", submatrices[count].GetLength(1) * count);
                count++;
            }

            //If we have frequency band matrices with different number of columns,
            //Then the below commented code need to be used.
            /*
            double[][,] submat = submatrices.ToArray();
            int colSize = 0;
            for (int i = 0; i < submat.Length; i++)
            {
                colSize = colSize + submat[i].GetLength(1);
            }

            //storing the number of rows of each submatrice in an array
            int[] noRows = new int[submat.Length];
            for (int i = 0; i < submat.Length; i++)
            {
                noRows[i] = submat[i].GetLength(0);
            }

            //find the max number of rows from noRows array
            int maxRows = noRows.Max();

            double[,] matrix = new double[maxRows, colSize];

            //might be better way to do this
            AddToArray(matrix, submat[0], "column");
            AddToArray(matrix, submat[1], "column", submat[0].GetLength(1));
            AddToArray(matrix, submat[2], "column", submat[0].GetLength(1) + submat[1].GetLength(1));
            */

            return matrix;
        }

        //adding a 2D-array to another 2D-array either by "column" or by "row"
        public static void AddToArray(double[,] result, double[,] array, string mergingDirection, int start = 0)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if (mergingDirection == "column")
                    {
                        result[i, j + start] = array[i, j];
                    }
                    else
                    {
                        if (mergingDirection == "row")
                        {
                            result[i + start, j] = array[i, j];
                        }
                    }
                }
            }
        }

        //convert a list of patch matrices to one matrix
        public static double[,] ListOf2DArrayToOne2DArray(List<double[,]> listOfPatchMatrices)
        {
            int noPat = listOfPatchMatrices[0].GetLength(0);
            double[,] allPatchesMatrix = new double[listOfPatchMatrices.Count * noPat, listOfPatchMatrices[0].GetLength(1)];
            for (int i = 0; i < listOfPatchMatrices.Count; i++)
            {
                var m = listOfPatchMatrices[i];
                /*
                if (m.GetLength(0) != noPat)
                {
                    throw new ArgumentException("All arrays must be the same length");
                }
                */
                AddToArray(allPatchesMatrix, m, "row", i * m.GetLength(0));
            }

            return allPatchesMatrix;
        }

        // adding a row of zero/one to 2D array
        // it should be a better way to so this!!!
        public static double[][] AddRow(double[,] matrix)
        {
            double[] array = new double[matrix.GetLength(1)];
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                array[j] = 1.0; //0.0; //
            }

            List<double[]> list = new List<double[]>();
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                list.Add(matrix.ToJagged()[i]);
            }

            list.Add(array);
            return list.ToArray();
        }

        public static double GetMaxValue(double[] data)
        {
            double max = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i] > max)
                {
                    max = data[i];
                }
            }

            return max;
        }
    }
}
