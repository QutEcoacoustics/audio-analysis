using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioAnalysisTools.DSP
{
    using System.CodeDom;
    using Accord.Math;
    using TowseyLibrary;

    public static class PatchSampling
    {
        /*
         * sample a set of patches ("sequential" or "random") from a spectrogram
         * in "sequential" mode, it generates non-overlapping patches from the whole input matrix, and
         * in this case the "numOfPatches" can be simply set to zero.
         * However, in "random" mode, the method requires an input for the "numOfPatches" parameter.
        */
        public static double[][] GetPatches(double[,] spectrogram, int patchWidth, int patchHeight, int numOfPatches, string samplingMethod)
        {
            List<double[]> patches = new List<double[]>();

            int rows = spectrogram.GetLength(0); //3247
            int cols = spectrogram.GetLength(1); //256
            Random rn = new Random();

            if (samplingMethod == "sequential")
            {
                //int numberOfPatches = (rows / patchHeight) * (cols / patchWidth);

                //checking whether the number of patches are smaller than size of patch matrix
                //we don't want to generate a matrix with less columns than rows for PCA!
                //if (numberOfPatches >= patchHeight * patchWidth)
                //{
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
                //}
                //else
               // {
                    //generate overlapping patches
                    //if (numberOfPatches < patchHeight * patchWidth)
                   // {

                    //}
                //}
            }
            else
            {
                if (samplingMethod == "random")
                {
                    for (int i = 0; i < numOfPatches; i++) //(rows / patchHeight) * patchHeight --> select 3232 random patches
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
                allPatches.Add(Array2Matrix(patches[row], patchWidth, patchHeight));
            }

            double[,] matrix = ConvertList2Matrix(allPatches, colSize, patchWidth, patchHeight);

            return matrix;
        }

        /*
         * converts a vector to a matrix.
         * The vector is built using the "Matrix2Array" method in MatrixTools.cs
         * The vector is built by concatenating columns
         */
        public static double[,] Array2Matrix(double[] vector, int patchWidth, int patchHeight)
        {
            double[,] m = new double[patchHeight, patchWidth];

            for (int col = 0; col < vector.Length; col += patchWidth)
            {
                for (int row = 0; row < patchWidth; row++)
                {
                    m[row, col / patchHeight] = vector[col + row];
                }
            }

            return m;
        }

        /*
         * converts a list<double[,]> to a matrix.
         * construct the original matrix from a set of patches
         */
        public static double[,] ConvertList2Matrix(List<double[,]> list, int colSize, int patchWidth, int patchHeight) //(List<double[,]> list, int noItemInRow, int colSize, int patchHeight)
        {
            double[][,] arrayOfPatches = list.ToArray();
            int rows = list.Count; //number of patches 3232 //4158
            //int cols = list[0].Length; //assume all vectors in list are of same length //256 //196
            int noItemInRow = colSize / patchWidth; //256/14=18
            int noItemInCol = rows / noItemInRow; //4158/18=231
            double[,] mx = new double[noItemInCol * patchHeight, noItemInRow * patchWidth]; //new double[(rows / noItemInRow) * patchHeight, colSize];
            for (int i = 0; i < noItemInCol; i++) //the number of patches in each row of the matrix 3232/16=202
            {
                for (int j = 0; j < noItemInRow; j++) //the number of patches in each column of the matrix 16
                {
                    for (int r = 0; r < list[(i * noItemInRow) + j].GetLength(0); r++) //patch id = (i * noItemInRow) + j
                    {
                        for (int c = 0; c < list[(i * noItemInRow) + j].GetLength(1); c++)
                        {
                            mx[r + (i * patchHeight), c + (j * patchWidth)] = arrayOfPatches[(i * noItemInRow) + j][r, c]; //mx[r + (i * noItemInRow), c + (j * noItemInRow)] = arrayOfPatches[(i * noItemInRow) + j][r, c];  //[r, c + ((i * noItemInRow) * list[i * noItemInRow].GetLength(0))]
                        }
                    }
                }
            }

            return mx;
        }

        /*
         * converts a sepctrogram matrix to 3 matrices by dividing the column (freq) into 3 parts
         * currently the first 1/4 is the lower, the second and third 1/4 forms the mid, and the last 1/4 is the upper freq band.
         */
        public static List<double[,]> GetFreqBandMatrices(double[,] matrix)
        {
            List<double[,]> allSubmatrices = new List<double[,]>();
            int cols = matrix.GetLength(1);
            int rows = matrix.GetLength(0);
            int newCol = cols / 4;

            double[,] minFreqBandMatrix = new double[rows, newCol];
            double[,] midFreqBandMatrix = new double[rows, newCol*2];
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
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < newCol*2; j++)
                {
                    midFreqBandMatrix[i, j] = matrix[i, j + newCol];
                }
            }

            allSubmatrices.Add(midFreqBandMatrix);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < newCol; j++)
                {
                    maxFreqBandMatrix[i, j] = matrix[i, j + newCol * 3];
                }
            }

            allSubmatrices.Add(maxFreqBandMatrix);
            return allSubmatrices;
        }

        /*
         * concatenate submatrices with the same # of rows but different # of columns into one matrix.
         */
        public static double[,] ConcatFreqBandMatrices(List<double[,]> submatrices)
        {
            double[][,] submat = submatrices.ToArray();
            int colSize = 0;
            for (int i = 0; i < submat.Length; i++)
            {
                colSize = colSize + submat[i].GetLength(1);
            }

            double[,] matrix = new double[submat[1].GetLength(0), colSize];

            //might be better way to do this
            AddToArray(matrix, submat[0]);
            AddToArray(matrix, submat[1], submat[0].GetLength(1));
            AddToArray(matrix, submat[2], submat[0].GetLength(1) + submat[1].GetLength(1));
            return matrix;
        }

        public static void AddToArray(double[,] result, double[,] array, int start = 0)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    result[i, j + start] = array[i, j];
                }
            }
        }
    }
}
