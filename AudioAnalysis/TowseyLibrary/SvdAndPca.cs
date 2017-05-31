namespace TowseyLibrary
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MathNet.Numerics;
    //using MathNet.Numerics.ComplexExtensions;
    using MathNet.Numerics.LinearAlgebra;
    using MathNet.Numerics.LinearAlgebra.Double;
    using MathNet.Numerics.LinearAlgebra.Generic;
    using MathNet.Numerics.LinearAlgebra.Generic.Factorization;

    /// <summary>
    /// contains methods and test example to do Singular Value decomposition and Principal Components Analysis
    ///
    /// IMPORTANT NOTE: The underlying storage of the Matrix class is a one dimensional array in column-major order (column by column).
    ///                 NOT Row by row!!
    ///
    ///
    /// </summary>
    public static class SvdAndPca
    {

        /// <summary>
        /// The singular value decomposition of an M by N rectangular matrix A has the form
        ///        A(mxn) = U(mxm) * S(mxn) * V'(nxn)
        /// where
        ///     U is an orthogonal matrix, whose columns are the left singular vectors;
        ///     S is a diagonal matrix, whose min(m,n) diagonal entries are the singular values;
        ///     V is an orthogonal matrix, whose columns are the right singular vectors;
        ///     Note 1: the transpose of V is used in the decomposition, and that the diagonal matrix S is typically stored as a vector.
        ///     Note 2: the values on the diagonal of S are the square-root of the eigenvalues.
        ///
        /// THESE TWO METHODS HAVE BEEN TESTED ON TOY EXAMPLES AND WORKED i.e. returned conrrect values
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[] SingularValueDecompositionVector(double[,] matrix)
        {
            bool computeVectors = false;
            var svd = new MathNet.Numerics.LinearAlgebra.Double.Factorization.DenseSvd(DenseMatrix.OfArray(matrix), computeVectors);
            Vector<double> singularValues = svd.S();
            return singularValues.ToArray();
        }
        public static System.Tuple<Vector<double>, Matrix<double>> SingularValueDecompositionOutput(double[,] matrix)
        {
            // we want to compute the U and V matrices of singular vectors.
            bool computeVectors = true;
            var svd = new MathNet.Numerics.LinearAlgebra.Double.Factorization.DenseSvd(DenseMatrix.OfArray(matrix), computeVectors);

            // svd.W returns the singular values on diagonal in matrix
            //Matrix<double> singularValues = svd.W();

            // svd.S returns the singular values in a vector
            Vector<double> singularValues = svd.S();

            // svd.U returns the singular vectors in matrix
            Matrix<double> UMatrix = svd.U();
            return Tuple.Create(singularValues, UMatrix);
        }


        /// <summary>
        /// returns the eigen values and eigen vectors of a matrix
        /// IMPORTANT: THIS METHOD NEEDS DEBUGGING.
        /// IT RETURNS THE NEGATIVE VALUES OF HTE EIGEN VECTORS ON A TOY EXMAPLE
        ///                 double[,] matrix = {
        ///                                { 3.0, -1.0 },
        ///                                { -1.0, 3.0 }
        ///                            };
        /// eigen values are correct ie, 2.0, 4.0; but in the wrong order????
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static System.Tuple<double[], double[,]> EigenVectors(double[,] matrix)
        {
            Evd<double> eigen = DenseMatrix.OfArray(matrix).Evd();
            Vector<System.Numerics.Complex> eigenvaluesComplex = eigen.EigenValues();

            //WriteArrayOfComplexNumbers(eigenvalues);

            double[] eigenvaluesReal = new double[eigenvaluesComplex.Count];
            for (int i = 0; i < eigenvaluesComplex.Count; i++)
            {
                System.Numerics.Complex c = eigenvaluesComplex[i];
                double magnitude = c.Magnitude;
                Console.WriteLine("eigen value[{0}]     {1}     Magnitude={2}", i, c.ToString(), magnitude);
            }

            Matrix<double> eigenvectorsComplex = eigen.EigenVectors();
            double[,] eigenvectorsReal = new double[eigenvaluesComplex.Count, matrix.GetLength(0)];
            for (int col = 0; col < eigenvectorsComplex.RowCount; col++)
            {
                Vector<double> ev = eigenvectorsComplex.Column(col);
                for (int i = 0; i < ev.Count; i++)
                {
                    eigenvectorsReal[col, i] = ev[i];
                    Console.WriteLine("eigen vector {0}, value {1} = {2}", col, i, ev[i]);
                }
            }

            return Tuple.Create(eigenvaluesReal, eigenvectorsReal);
        }

        public static void WriteArrayOfComplexNumbers(Vector<System.Numerics.Complex> v)
        {
            Console.WriteLine("A column vector of complex numbers");
            for (int i = 0; i < v.Count; i++)
            {
                System.Numerics.Complex c = v[i];
                double magnitude = c.Magnitude;
                Console.WriteLine("eigen value[{0}]     {1}     Magnitude={2}", i, c.ToString(), magnitude);
            }
        }


        //=============================================================================



        public static void ExampleOfSVD_1()
        {
            double[,] matrix1 = {
                                        { 3.0, -1.0 },
                                        { -1.0, 3.0 },
                                    };
            EigenVectors(matrix1);


            //double[,] matrix2 = {
            //                        {1, 1},
            //                        {0, 0},
            //                        {Wavelets.SQRT2, -Wavelets.SQRT2}
            //                     };
            double[,] matrix2 = {
                                        {2, 4},
                                        {1, 3},
                                        {0, 0},
                                        {0, 0},
                                     };
            var tuple = SingularValueDecompositionOutput(matrix2);
            Vector<double> sdValues = tuple.Item1;
            Matrix<double> UMatrix = tuple.Item2;

            foreach (double d in sdValues) Console.WriteLine("sdValue = {0}", d);
            double ratio = (sdValues[0] - sdValues[1]) / sdValues[0];
            Console.WriteLine("(e1-e2)/e1 = {0}", ratio);

            string path1 = @"C:\SensorNetworks\Output\Test\testMatrix.png";
            ImageTools.DrawReversedMatrix(matrix2, path1);
            string path2 = @"C:\SensorNetworks\Output\Test\SvdMatrix.png";
            ImageTools.DrawReversedMDNMatrix(UMatrix, path2);

        }

        public static void ExampleOfSVD_2()
        {

            //// this example given in
            //double[,] matrix1 = {
            //                            { 1.0, 1.0, 1.0, 0.0, 0.0 },
            //                            { 3.0, 3.0, 3.0, 0.0, 0.0 },
            //                            { 4.0, 4.0, 4.0, 0.0, 0.0 },
            //                            { 5.0, 5.0, 5.0, 0.0, 0.0 },
            //                            { 0.0, 2.0, 0.0, 4.0, 4.0 },
            //                            { 0.0, 0.0, 0.0, 5.0, 5.0 },
            //                            { 0.0, 1.0, 0.0, 2.0, 2.0 },
            //                    };

            // this example given on page 21 of Singular Value Decomposition Tutorial" by Kirk Baker  March 29, 2005 (Revised January 14, 2013)
            // see that page for an interpretation of the U matrix.
            double[,] matrix1 = {
                                        { 2.0, 0.0, 8.0, 6.0, 0.0 },
                                        { 1.0, 6.0, 0.0, 1.0, 7.0 },
                                        { 5.0, 0.0, 7.0, 4.0, 0.0 },
                                        { 7.0, 0.0, 8.0, 5.0, 0.0 },
                                        { 0.0,10.0, 0.0, 0.0, 7.0 },
                                };

            /*       WORDS   ||       DOCUMENTS
             *        doctor { 2.0, 0.0, 8.0, 6.0, 0.0 },
                      car    { 1.0, 6.0, 0.0, 1.0, 7.0 },
                      nurse  { 5.0, 0.0, 7.0, 4.0, 0.0 },
                      hospit { 7.0, 0.0, 8.0, 5.0, 0.0 },
                      wheel  { 0.0,10.0, 0.0, 0.0, 7.0 },
             *
             *
             * THE U - MATRIX should be
             * −0.54  0.07  0.82 −0.11  0.12
               −0.10 −0.59 −0.11 −0.79 −0.06
               −0.53  0.06 −0.21  0.12 −0.81
               −0.65  0.07 −0.51  0.06  0.56
               −0.06 −0.80  0.09  0.59  0.04
             *
             * However note that the signs of the values in the matrix are frequently reversed.
             * Interpretation:
             * The signs in the first column vector are all negative, indicating the general cooccurence of words and documents.
             * Two groups are visible in the second column of U: car and wheel have negative coefficients, while doctor, nurse, and hospital are all positive -
             * this indicates a grouping in which wheel only cooccurs with car.
             * The third dimension indicates a grouping in which car, nurse, and hospital occur only with each other.
             * The fourth dimension points out a pattern in which nurse and hospital occur in the absence of wheel.
             * The fifth dimension indicates a grouping in which doctor and hospital occur in the absence of wheel.
             * */




            // we want to compute the U and V matrices of singular vectors.
            bool computeVectors = true;
            var svd = new MathNet.Numerics.LinearAlgebra.Double.Factorization.DenseSvd(DenseMatrix.OfArray(matrix1), computeVectors);

            // svd.S returns the singular values in a vector
            Vector<double> singularValues = svd.S();
            foreach (double d in singularValues)
                Console.WriteLine("singular value = {0}", d);

            // svd.U returns the LEFT singular vectors in matrix
            Matrix<double> UMatrix = svd.U();
            Console.WriteLine("\n\n");
            MatrixTools.WriteMatrix(UMatrix.ToArray());
            string path1 = @"C:\SensorNetworks\Output\Test\testMatrixSVD_U.png";
            ImageTools.DrawReversedMDNMatrix(UMatrix, path1);

            // svd.VT returns the RIGHT singular values
            Matrix<double> VMatrix = svd.VT();
            Console.WriteLine("\n\n");
            MatrixTools.WriteMatrix(VMatrix.ToArray());
            string path2 = @"C:\SensorNetworks\Output\Test\testMatrixSVD_VT.png";
            ImageTools.DrawReversedMDNMatrix(VMatrix, path2);

        }

        /// <summary>
        /// These examples are used to do Wavelet Packet Decomposition and then do SVD on the returned WPD trees.
        /// </summary>
        public static void ExampleOfSVD_3()
        {
            //double[] signal = {1,0,0,0,0,0,0,0};
            //double[] signal = { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 };
            //double[] signal = { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 };
            //this signal contains four cycles
            //double[] signal = { 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0 };
            //this signal contains eight cycles
            //double[] signal = { 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0,
            //                    1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0 };

            //this signal contains 16 cycles
            //double[] signal = { 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
            //                    1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0 };

            //this signal contains four step cycles
            //double[] signal = { 1, 1, 0.5, 0, -0.5, -1.0, -1.0, -0.5, 0, 0.5, 1.0, 1.0, 0.5, 0.0, -0.5, -1, -1, -0.5, 0, 0.5, 1.0, 1.0, 0.5, 0.0, -0.5, -1.0, -1.0,  -0.5, 0, 0.5, 1.0, 1.0,
            //                    1, 1, 0.5, 0, -0.5, -1.0, -1.0, -0.5, 0, 0.5, 1.0, 1.0, 0.5, 0.0, -0.5, -1, -1, -0.5, 0, 0.5, 1.0, 1.0, 0.5, 0.0, -0.5, -1.0, -1.0,  -0.5, 0, 0.5, 1.0, 1.0 };

            //this 128 sample signal contains 32 cycles
            double[] signal = { 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
                                    1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
                                    1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
                                    1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, };
            // add noise to signal
            RandomNumber rn = new RandomNumber();
            double[] rv = RandomNumber.GetRandomVector(128, rn);

            // note that even when noise is twice amplitude of signal the first column of UMatrix is excellent reproduction of
            // first column when signal has no added noise.
            // relative noise amplitude
            double noiseAmplitude = 2.0;
            DataTools.Normalise(rv, 0.0, noiseAmplitude);
            // signal plus noise
            signal = DataTools.AddVectors(signal, rv);
            // normalising seems to make little difference to the result
            signal = DataTools.normalise(signal);

            int levelNumber = 5;
            //double[] V = Wavelets.GetWPDSequenceAggregated(signal, levelNumber);

            //for (int i = 0; i < V.Length; i++)
            //{
            //    if (Math.Abs(V[i]) > 0.01) Console.WriteLine("energy[{0}] = {1}", i, V[i]);
            //    else Console.WriteLine("energy[{0}] = {1}", i, " ");
            //}
        }


        public static void TestEigenValues()
        {
            /// used to caluclate eigen values and singular valuse
                //double[,] M = {
                //                    { 1.0,  1.0 },
                //                    { 1.0,  1.0 }
                //                };
                //double[,] M = {
                //                    { 2.0,  7.0 },
                //                    {-1.0, -6.0 }
                //                };

                // NOTE: When the matrix is square symmetric, the singular values equal the eigenvalues.
                // e1=4.0     e2=2.0 and the singular values are the same
                double[,] M = {
                                    { 3.0, -1.0 },
                                    {-1.0,  20.0 },
                                };

                // e1=e2=0.333333
                //double[,] M = {
                //                    { 1.0, -1.0 },
                //                    {4/(double)9,  -1.0/(double)3 }
                //                };
                System.Tuple<double[], double[,]> result = EigenVectors(M);

                Log.WriteLine("\n\n Singlar values");
                double[] singValues = SingularValueDecompositionVector(M);
                foreach (double value in singValues)
                {
                    Console.WriteLine(value);
                }


                double[] eigenValues = StructureTensor.CalculateEigenValues(M);
                Console.WriteLine("\n\n EigenValues = {0} and {1}", eigenValues[0], eigenValues[1]);
        }

    }
}
