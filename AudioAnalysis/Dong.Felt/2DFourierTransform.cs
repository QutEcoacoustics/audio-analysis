﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.Transformations;
using TowseyLibrary;
//using System.Numerics;
using MathNet.Numerics;


namespace Dong.Felt
{
    class _2DFourierTransform
    {
        /// Calculate the discrete Fourier Transform for an image 
        public static double[,] DiscreteFourierTransform(double[,] imageData)
        {
            var matrixRowCount = imageData.GetLength(0);
            var matrixColCount = imageData.GetLength(1);
            // Step 1: convert matrix to complex array
            var sampleData = Matrix2PaddedVector(imageData);
            // Step 2: do 2d fft and convert complex array to array of real magnitude values
            var dims = new int[] { matrixRowCount, matrixColCount };
            var magnitude = Magnitude2DFourierTransform(sampleData, dims);           
            // Step 3: do the shift for array of magnitude values.
           //var outputData = fftShift(magnitude);
            return magnitude;
        }

        /// <summary>
        /// Concatenates the columns of the passed matrix and inserts zeros in every second position.
        /// The output vector is now assumed to be a vector of Complex numbers, 
        /// with the real values in the even positions and the imaginary numbers in the odd positions.
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        public static double[] Matrix2PaddedVector(double[,] M)
        {
            int rowCount = M.GetLength(0);
            int colCount = M.GetLength(1);
            // set up sampleData with additional space for padding zeroes.
            var sampleData = new double[rowCount * colCount * 2];
            for (var r = 0; r < rowCount; r++)
            {
                for (var c = 0; c < colCount; c++)
                {                             
                    sampleData[((r * rowCount) + c) * 2] = M[r, c];
                    sampleData[((r * rowCount) + c) * 2 + 1] = 0.0;
                }
            }
            return sampleData;
        }

        /// <summary>
        /// do 2d fourier transform on complex sampleData, so first construct complex sampleData, then calculate 
        /// the magnitude of sampleData.
        /// </summary>
        /// <param name="sampleData"></param>
        /// <param name="dims"></param>
        /// <returns></returns>
        public static double[,] Magnitude2DFourierTransform(double[] sampleData, int[] dims)
        {
            var cft = new ComplexFourierTransformation();           
            cft.TransformForward(sampleData, dims);
            Complex[] sampleComplexPairs = new Complex[sampleData.Length / 2];
            for (int i = 0; i < sampleData.Length; i++)
            {
                var item = new Complex();
                // even number save real values for complex
                // odd number save imaginary values for complex
                if (i % 2 == 0)
                {
                    item.Real = sampleData[i];
                    item.Imag = sampleData[i + 1];
                    sampleComplexPairs[i / 2]= item;
                }
            }
            var outputData = new double[dims[0], dims[1]];
            for (var r = 0; r < dims[0]; r++)
            {
                for (var c = 0; c < dims[1]; c++)
                {
                    outputData[r, c] = Math.Sqrt(Math.Pow(sampleComplexPairs[r * dims[0] + c].Real, 2.0)
                        + Math.Pow(sampleComplexPairs[r * dims[0] + c].Imag, 2.0));
                }
            }
            return outputData;
        }

        /// <summary>
        /// This method tries to shift the magnitude image generated by 2DFourierTransform into an image where its center 
        /// is in the middle. 
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] fftShift(double[,] matrix)
        {
            var rowCount = matrix.GetLength(0);
            var colCount = matrix.GetLength(1);

            var result = new double[rowCount, colCount];
            var subMatrixLength = rowCount / 2;

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    if (r < subMatrixLength)
                    {
                        if (c < subMatrixLength)
                        {
                            // the first quard.
                            result[r + subMatrixLength, c + subMatrixLength] = matrix[r, c];
                        }
                        else
                        {
                            // the second quard. 
                            result[r + subMatrixLength, c - subMatrixLength] = matrix[r, c];
                        }
                    }
                    else{
                        // the third quard.
                        if (c < subMatrixLength)
                        {
                            result[r - subMatrixLength, c + subMatrixLength] = matrix[r, c];
                        }
                        else
                        {
                            // the fourth quard
                            result[r - subMatrixLength, c - subMatrixLength] = matrix[r, c];
                        }
                    }
                }
            }
                return result;
        }
    }
}
