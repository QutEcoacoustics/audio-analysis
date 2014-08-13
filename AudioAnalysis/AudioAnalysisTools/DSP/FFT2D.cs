using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;  // this is needed for the use of complex numbers. 
using MathNet.Numerics; // this is needed for the class ComplexExtensions which does the calculation of the magnitude of a complex number.
using MathNet.Numerics.Transformations; // this is needed for the 2D FFT
//using MathNet.Numerics.ComplexExtensions;

using TowseyLibrary;



namespace AudioAnalysisTools.DSP
{

    /// <summary>
    /// Performs two dimensional FFT on a matrix of values.
    /// IMPORTANT: The matrix passed to this class for performing of 2D FFT need not necessarily have width equal to height
    /// but both width and height MUST be a power of two.
    /// </summary>
    class FFT2D
    {

        public double[,] FFT2Dimensional(double[,] M)
        {
            double[] sampleData = this.Matrix2PaddedVector(M);
            int rowCount = M.GetLength(0);
            int colCount = M.GetLength(1);
            int[] dims = { rowCount, colCount };

            var cft = new ComplexFourierTransformation();
            cft.TransformForward(sampleData, dims);
            System.Numerics.Complex[] sampleComplexPairs = new System.Numerics.Complex[sampleData.Length / 2];

            // After transformation, sampleData now has real and imaginary values in alternating blocks of rowCount / 2.
            // For example: if the passed image/matrix is 16 * 16, sampleData now has 8 reals followed by 8 imaginary numbers
            // repeated 32 times. The Re and Im values are combined into MAGNITUDES
            for (int i = 0; i < sampleData.Length; i++)
            {
                var step = rowCount / 2;
                var ite = i / step;
                var mod = i % step;
                var sampleDataColCount = sampleData.Length / rowCount;

                System.Numerics.Complex item = new System.Numerics.Complex();
                if (ite % 2 == 0)
                {
                    //item.Real = sampleData[(ite * step) + mod];
                    //item.Im = sampleData[((ite + 1) * step) + mod];
                    //sampleComplexPairs[(ite * step / 2) + mod] = item;
                    double real      = sampleData[ (ite      * step) + mod];
                    double imaginary = sampleData[((ite + 1) * step) + mod];
                    item = new System.Numerics.Complex(real, imaginary);
                    double magnitude = ComplexExtensions.MagnitudeSquared(item);

                }
            }

            var outputData = new double[rowCount, colCount];
            for (var r = 0; r < rowCount; r++)
            {
                for (var c = 0; c < colCount; c++)
                {
                    //outputData[c, r] = ComplexExtensions.MagnitudeSquared(item[c]);
                    //outputData[c, r] = Math.Sqrt(Math.Pow(sampleComplexPairs[r * rowCount + c].Real, 2.0)
                    //    + Math.Pow(sampleComplexPairs[r * rowCount + c].Imag, 2.0));
                }
            }
            return outputData;
        }


        /// <summary>
        /// Concatenates the columns of the passed matrix and inserts zeros in every second position.
        /// The output vector is now assumed to be a vector of Complex numbers, 
        /// with the real values in the even positions and the imaginary numbers in the odd positions.
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        private double[] Matrix2PaddedVector(double[,] M)
        {
            int rowCount = M.GetLength(0);
            int colCount = M.GetLength(1);
            var vector = new double[rowCount * colCount];

            // set up vector with additional space for padding zeroes.
            var sampleData = new double[rowCount * colCount * 2];
            for (var r = 0; r < rowCount * 2; r++)
            {
                var colData = MatrixTools.GetColumn(M, r / 2);
                if (r % 2 == 0)
                {
                    for (var c = 0; c < colData.Length; c++)
                    {
                        sampleData[(r * rowCount) + c] = colData[c];
                    }
                }
                else
                {
                    for (var c = 0; c < colData.Length; c++)
                    {
                        sampleData[r * rowCount + c] = 0.0;
                    }
                }
            }
            return vector;
        }

    }
}
