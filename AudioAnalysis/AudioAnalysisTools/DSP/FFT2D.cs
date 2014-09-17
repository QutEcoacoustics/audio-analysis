﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics; // this is needed for the class ComplexExtensions which does the calculation of the magnitude of a complex number.
using MathNet.Numerics.Transformations; // this is needed for the 2D FFT
//using MathNet.Numerics.ComplexExtensions;

using TowseyLibrary;
using System.Drawing;



namespace AudioAnalysisTools.DSP
{

    /// <summary>
    /// Performs two dimensional FFT on a matrix of values.
    /// IMPORTANT: The matrix passed to this class for performing of 2D FFT need not necessarily have width equal to height
    /// but both width and height MUST be a power of two.
    /// </summary>
    public class FFT2D
    {
        /// <summary>
        /// Performs a 2D-Fourier transform on data in the passed Matrix/image.
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        public static double[,] FFT2Dimensional(double[,] M)
        {
            // Step 1: convert matrix to complex array
            double[] sampleData = Matrix2PaddedVector(M);
            int rowCount = M.GetLength(0);
            int colCount = M.GetLength(1);

            // Step 2: do 2d fft 
            int[] dims = { rowCount, colCount };
            var cft = new ComplexFourierTransformation();
            cft.TransformForward(sampleData, dims);

            // Step 3: Convert complex output array to array of real magnitude values
            var magnitudeMatrix = FFT2DOutput2MatrixOfMagnitude(sampleData, dims);  

            // Step 3: do the shift for array of magnitude values.
            // var outputData = magnitudeMatrix; // no shifting
            var outputData = fftShift(magnitudeMatrix);

            return outputData;
        }


        /// <summary>
        /// Concatenates the columns of the passed matrix and inserts zeros in every second position.
        /// The matrix is assumed to be an image and therefore read it using image coordinates.
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
        /// First construct complex sampleData, then calculate the magnitude of sampleData.
        /// </summary>
        /// <param name="sampleData"></param>
        /// <param name="dims"></param>
        /// <returns></returns>
        public static double[,] FFT2DOutput2MatrixOfMagnitude(double[] sampleData, int[] dims)
        {

            // After 2D-FFT transformation, the sampleData array now has alternating real and imaginary values.
            // Create an array of class Complex.
            Complex[] sampleComplexPairs = new Complex[sampleData.Length / 2];
            for (int i = 0; i < sampleData.Length; i++)
            {
                var item = new Complex();
                // even number save real values for complex
                if (i % 2 == 0)
                {
                    item.Real = sampleData[i];
                    item.Imag = sampleData[i + 1];
                    sampleComplexPairs[i / 2]= item;
                }
            }


            //int[] dims = { rowCount, colCount };
            int rowCount = dims[0];
            int colCount = dims[1];

            var outputData = new double[rowCount, colCount];
            for (var r = 0; r < rowCount; r++)
            {
                for (var c = 0; c < colCount; c++)
                {
                    outputData[r, c] = Math.Sqrt(Math.Pow(sampleComplexPairs[(r * rowCount) + c].Real, 2.0)
                                               + Math.Pow(sampleComplexPairs[(r * rowCount) + c].Imag, 2.0));
                    //var magnitude = ComplexExtensions.SquareRoot(sampleComplexPairs[r * matrixRowCount + c]);
                }
            }
            return outputData;
        }
 



        /// <summary>
        /// This method "shifts" (that is, "rearranges") the quadrants of the magnitude matrix generated by the 2DFourierTransform
        /// such that the Top Left  quadrant is swapped with the Bottom-Right quadrant 
        ///       and the Top-Right quadrant is swapped with the Bottom-Left. 
        /// This has the effect of shifting the low frequency coefficients into the centre of the matrix and the high frequency
        /// coefficients are shifted to the edge of the image. 
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] fftShift(double[,] matrix)
        {
            var rowCount = matrix.GetLength(0);
            var colCount = matrix.GetLength(1);
 
            var shiftedMatrix = new double[rowCount, colCount];
            var quadrantLength = rowCount / 2;
 
            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    if (r < quadrantLength)
                    {
                        if (c < quadrantLength)
                        {
                            // the first quadrant.
                            shiftedMatrix[r + quadrantLength, c + quadrantLength] = matrix[r, c];
                        }
                        else
                        {
                            // the second quadrant.
                            shiftedMatrix[r + quadrantLength, c - quadrantLength] = matrix[r, c];
                        }
                    }
                    else{
                        // the third quadrant.
                        if (c < quadrantLength)
                        {
                            shiftedMatrix[r - quadrantLength, c + quadrantLength] = matrix[r, c];
                        }
                        else
                        {
                            // the fourth quadrant
                            shiftedMatrix[r - quadrantLength, c - quadrantLength] = matrix[r, c];
                        }
                    }
                }
            }
            return shiftedMatrix;
        }



        /// <summary>
        /// reads an image into a matrix.
        /// Takes weighted average of the RGB colours in each pixel.
        /// </summary>
        /// <param name="imageFilePath"></param>
        /// <returns></returns>
        public static double[,] GetImageDataAsGrayIntensity(string imageFilePath, bool reversed)
        {
            Bitmap image = (Bitmap)Image.FromFile(imageFilePath, true);         
            var rowCount = image.Height;
            var colCount = image.Width;
            var result = new double[rowCount, colCount];
            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    result[r, c] = (0.299 * image.GetPixel(c, r).R) 
                                 + (0.587 * image.GetPixel(c, r).G) 
                                 + (0.114 * image.GetPixel(c, r).B);
                    if (reversed) // reverse the image
                    {
                        result[r, c] = 255 - result[r, c];
                    }  
                }
            }
            return result;
        }



        /// <summary>
        /// METHOD to TEST the FFT2D.
        /// </summary>
        public static void TestFFT2D()
        {
            string imageFilePath = @"C:\SensorNetworks\Output\FFT2D\test5.png";
            bool reversed = false;
            double[,] matrix = FFT2D.GetImageDataAsGrayIntensity(imageFilePath, reversed);
            //matrix = MatrixTools.normalise(matrix);
            double[,] output = FFT2D.FFT2Dimensional(matrix);
            Console.WriteLine("Sum={0}", (MatrixTools.Matrix2Array(output)).Sum());
            //draws matrix after normalisation with white=low and black=high
            ImageTools.DrawReversedMatrix(output, @"C:\SensorNetworks\Output\FFT2D\test5_2DFFT.png");
        }



    }


}
