
namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using TowseyLib; 

    class ImageAnalysisTools
    {
        // A 7 * 7 gaussian blur
        public static double[,] gaussianBlur7 = {{0.00000067,	0.00002292,	0.00019117,	0.00038771,	0.00019117,	0.00002292,	0.00000067},
                                                {0.00002292,	0.00078633,	0.00655965,	0.01330373,	0.00655965,	0.00078633,	0.00002292},
                                                {0.00019117,	0.00655965,	0.05472157,	0.11098164,	0.05472157,	0.00655965,	0.00019117},
                                                {0.00038771,	0.01330373,	0.11098164,	0.22508352,	0.11098164,	0.01330373,	0.00038771},
                                                {0.00019117,	0.00655965,	0.05472157,	0.11098164,	0.05472157,	0.00655965,	0.00019117},
                                                {0.00002292,	0.00078633,	0.00655965,	0.01330373,	0.00655965,	0.00078633,	0.00002292},
                                                {0.00000067,	0.00002292,	0.00019117,	0.00038771,	0.00019117,	0.00002292,	0.00000067}};

        // A 5 * 5 gaussian blur
        public static double[,] gaussianBlur5 = {{0.0000,       0.0000,     0.0002,     0.0000,    0.0000},
                                                 {0.0000,       0.0113,     0.0837,     0.0113,    0.0000},
                                                 {0.0002,       0.0837,     0.6187,     0.0837,    0.0002},
                                                 {0.0000,       0.0113,     0.0837,     0.0113,    0.0000},
                                                 {0.0000,       0.0000,     0.0002,     0.0000,    0.0000}};

        public static double[,] SobelX =  { {-1.0,  0.0,  1.0},
                                            {-2.0,  0.0,  -2.0},
                                            {-1.0,  0.0,  1.0} };

        public static double[,] SobelY =  { {1.0,  2.0,  1.0},
                                            {0.0,  0.0,  0.0},
                                            {-1.0, -2.0, -1.0} };
        
        /// Canny detector for edge detection in a noisy image 
        /// it involves five steps here, first, it needs to do the Gaussian convolution, then a simple derivative operator(like Roberts Cross or Sobel operator) is 
        /// applied to the smoothed image to highlight regions of the image. Finally, it will track .....
        /// 
        public static void CannyDetector(double[,] m)
        {
            // Do Gaussian smooth
            var smoothedMatrix = TowseyLib.ImageTools.Convolve(m, Kernal.gaussianBlur5);

            // Sobel operator
            int MaximumXIndex = m.GetLength(0);
            int MaximumYIndex = m.GetLength(1);

            var offset = (int)(SobelX.GetLength(0) / 2);
            //var numberOfVetex = MaximumXIndex * MaximumYIndex;

            var partialDifferenceX = new double[MaximumXIndex, MaximumYIndex];
            var partialDifferenceY = new double[MaximumXIndex, MaximumYIndex];

            for (int row = 0; row < MaximumXIndex; row++)
            {
                for (int col = 0; col < MaximumYIndex; col++)
                {
                    var TempDifferenceX = 0.0;
                    var TempDifferenceY = 0.0;
                    for (int indexX = -offset; indexX < offset; indexX++)
                    {
                        for (int indexY = -offset; indexY < offset; indexY++)
                        {
                            // Todo : fix the out of range
                            if ((row + indexY) >= 0 && (row + indexY) < MaximumXIndex && (col + indexX) >= 0 && (col + indexX) < MaximumYIndex)
                            {
                                //TempDifferenceX = m[row + indexY, col + indexX] * CannyDifferenceX[indexX + offset, indexY + offset];
                                //TempDifferenceY = m[row + indexY, col + indexX] * CannyDifferenceY[indexX + offset, indexY + offset];
                            }
                        }
                    }
                    partialDifferenceX[row, col] = TempDifferenceX;
                    partialDifferenceY[row, col] = TempDifferenceY;
                }
            }
            //PointF
        }

        // 1. Smoothing: Blurring of the image to remove noise.
        private void GenerateGaussianKernel(int sizeOfKernel, double sigma, out int weight)
        {
            double pi = (float)Math.PI;
            var kernel = new double[sizeOfKernel, sizeOfKernel];
            var GaussianKernel = new int[sizeOfKernel, sizeOfKernel];
            float[,] OP = new float[sizeOfKernel, sizeOfKernel];
            double coeffients1 = 1 / (2 * pi * sigma * sigma);
            double coeffients2 = 2 * sigma * sigma;

            int offset = sizeOfKernel / 2; 
            double min = 1000;

            for (int i = -offset; i <= offset; i++)
            {
                for (int j = -offset; j <= offset; j++)
                {
                    kernel[offset + i, offset + j] = ((1 / coeffients1) * (float)Math.Exp(-(Math.Pow(i, 2) + Math.Pow(j, 2)) / coeffients2));
                    if (kernel[offset + i, offset + j] < min)
                        min = kernel[offset + i, offset + j];
                }
            }

            //the process of normalizing the weights
            int reciprocal = (int)(1 / min);
            int sum = 0;
            if ((min > 0) && (min < 1))
            {
                for (int i = -offset; i <= offset; i++)
                {
                    for (int j = -offset; j <= offset; j++)
                    {
                        kernel[offset + i, offset + j] = (double)Math.Round(kernel[offset + i, offset + j] * reciprocal, 0);
                        GaussianKernel[offset + i, offset + j] = (int)kernel[offset + i, offset + j];
                        sum = sum + GaussianKernel[offset + i, offset + j];
                    }
                }
            }
            else
            {
                sum = 0;
                for (int i = -reciprocal; i <= reciprocal; i++)
                {
                    for (int j = -reciprocal; j <= reciprocal; j++)
                    {
                        kernel[reciprocal + i, reciprocal + j] = (float)Math.Round(kernel[reciprocal + i, reciprocal + j], 0);
                        GaussianKernel[reciprocal + i, reciprocal + j] = (int)kernel[reciprocal + i, reciprocal + j];
                        sum = sum + GaussianKernel[reciprocal + i, reciprocal + j];
                    }
                }
            }
            //Normalizing kernel Weight
            weight = sum;
            return;
        }

        // Following subroutine removes noise by Gaussian Filtering
        private double[,] GaussianFilter(double[,] m, int kernalSize, int kernelWeight)
        {
            int MaximumXindex = m.GetLength(0);
            int MaximumYindex = m.GetLength(1);
            var result = new double [MaximumXindex, MaximumYindex];

            int offset = kernalSize / 2;
            double sum = 0.0;
            result = m; // Removes Unwanted Data Omission due to kernel bias while convolution

            for (int row = offset; row <= ((MaximumXindex - 1) - offset); row++)
            {
                for (int col = offset; col <= ((MaximumYindex - 1) - offset); col++)
                {
                    sum = 0;
                    for (int i = -offset; i <= offset; i++)
                    {
                        for (int j = -offset; j <= offset; j++)
                        {
                            // Todo: fix this statement 
                            //sum = sum + ((float)m[i + k, j + l] * GaussianKernel[offset + k, offset + l]);
                        }
                    }
                    result[row, col] = (int)(Math.Round(sum / (float)kernelWeight));
                }
            }

            return result;
        }
        // 2. Finding gradients: The edges should be marked where the gradients of the image haslarge magnitudes.
        // Sobel X and Y Masks are used to generate X & Y Gradients of Image; next function implements differentiation using sobel Filter Mask
        //public static float[,] Differentiate(int[,] m, int[,] Filter)
        //{
        //    int i, j, k, l, Fh, Fw;
        //    Fw = Filter.GetLength(0);
        //    Fh = Filter.GetLength(1);
        //    float sum = 0;
        //    float[,] Output = new float[Width, Height];

        //    for (i = Fw / 2; i <= (Width - Fw / 2) - 1; i++)
        //    {
        //        for (j = Fh / 2; j <= (Height - Fh / 2) - 1; j++)
        //        {
        //            sum = 0;
        //            for (k = -Fw / 2; k <= Fw / 2; k++)
        //            {
        //                for (l = -Fh / 2; l <= Fh / 2; l++)
        //                {
        //                    sum = sum + Data[i + k, j + l] * Filter[Fw / 2 + k, Fh / 2 + l];
        //                }
        //            }
        //            Output[i, j] = sum;
        //        }
        //    }

        //    return Output;
        //}

        // 3. Non-maximum suppression: Only local maxima should be marked as edges.
        // We find gradient direction and using these direction we perform non maxima suppression (Read “Digital Image Processing- by R Gonzales-Pearson Education)
        // Perform Non maximum suppression:
           // NonMax = Gradient;
            //for (i = 0; i <= (Width - 1); i++)
            //{
            //    for (j = 0; j <= (Height - 1); j++)
            //    {
            //        NonMax[i, j] = Gradient[i, j];
            //    }
            //}
         
            //int Limit = KernelSize / 2;
            //int r, c;
            //float Tangent;
    

            //for (i = Limit; i <= (Width - Limit) - 1; i++)
            //{
            //    for (j = Limit; j <= (Height - Limit) - 1; j++)
            //    {

            //        if (DerivativeX[i, j] == 0)
            //            Tangent = 90F;
            //        else
            //            Tangent = (float)(Math.Atan(DerivativeY[i, j] / DerivativeX[i, j]) * 180 / Math.PI); //rad to degree



            //        //Horizontal Edge
            //        if (((-22.5 < Tangent) && (Tangent <= 22.5)) || ((157.5 < Tangent) && (Tangent <= -157.5)))
            //        {
            //            if ((Gradient[i, j] < Gradient[i, j + 1]) || (Gradient[i, j] < Gradient[i, j - 1]))
            //                NonMax[i, j] = 0;
            //        }


            //        //Vertical Edge
            //        if (((-112.5 < Tangent) && (Tangent <= -67.5)) || ((67.5 < Tangent) && (Tangent <= 112.5)))
            //        {
            //            if ((Gradient[i, j] < Gradient[i + 1, j]) || (Gradient[i, j] < Gradient[i - 1, j]))
            //                NonMax[i, j] = 0;
            //        }

            //        //+45 Degree Edge
            //        if (((-67.5 < Tangent) && (Tangent <= -22.5)) || ((112.5 < Tangent) && (Tangent <= 157.5)))
            //        {
            //            if ((Gradient[i, j] < Gradient[i + 1, j - 1]) || (Gradient[i, j] < Gradient[i - 1, j + 1]))
            //                NonMax[i, j] = 0;
            //        }

            //        //-45 Degree Edge
            //        if (((-157.5 < Tangent) && (Tangent <= -112.5)) || ((67.5 < Tangent) && (Tangent <= 22.5)))
            //        {
            //            if ((Gradient[i, j] < Gradient[i + 1, j + 1]) || (Gradient[i, j] < Gradient[i - 1, j - 1]))
            //                NonMax[i, j] = 0;
            //        }

            //    }
            //}

      //4.Double thresholding: Potential edges are determined by thresholding.

      //5.Edge tracking by hysteresis: Final edges are determined by suppressing all edges that are not connected to a very certain (strong) edge.
//      private void HysterisisThresholding(int[,] Edges)
//      {
//          int i, j;
//          int Limit= KernelSize/2;

//          for (i = Limit; i <= (Width - 1) - Limit; i++)
//{
//              for (j = Limit; j <= (Height - 1) - Limit; j++)
//                {
//                    if (Edges[i, j] == 1)
//                    {
//                        EdgeMap[i, j] = 1;

//                    }

//                }
//}
//            for (i = Limit; i <= (Width - 1) - Limit; i++)
//            {
//                for (j = Limit; j <= (Height  - 1) - Limit; j++)
//                {
//                    if (Edges[i, j] == 1)
//                    {
//                        EdgeMap[i, j] = 1;
//                        Travers(i, j);
//                        VisitedMap[i, j] = 1;
//                    }
//                }
//            }
//            return;
//        }

//        //Recursive Procedure 
//        public void Travers(int X, int Y)
//        {
//            if (VisitedMap[X, Y] == 1)
//            {
//                return;
//            }

//            //1
//            if (EdgePoints[X + 1, Y] == 2)
//            {
//                EdgeMap[X + 1, Y] = 1;
//                VisitedMap[X + 1, Y] = 1;
//                Travers(X + 1, Y);
//                return;
//            }
//            //2
//            if (EdgePoints[X + 1, Y - 1] == 2)
//            {
//                EdgeMap[X + 1, Y - 1] = 1;
//                VisitedMap[X + 1, Y - 1] = 1;
//                Travers(X + 1, Y - 1);
//                return;
//            }

//           //3

//            if (EdgePoints[X, Y - 1] == 2)
//            {
//                EdgeMap[X , Y - 1] = 1;
//                VisitedMap[X , Y - 1] = 1;
//                Travers(X , Y - 1);
//                return;
//            }

//            //4
//            if (EdgePoints[X - 1, Y - 1] == 2)
//            {
//                EdgeMap[X - 1, Y - 1] = 1;
//                VisitedMap[X - 1, Y - 1] = 1;
//                Travers(X - 1, Y - 1);
//                return;
//            }
//            //5
//            if (EdgePoints[X - 1, Y] == 2)
//            {
//                EdgeMap[X - 1, Y ] = 1;
//                VisitedMap[X - 1, Y ] = 1;
//                Travers(X - 1, Y );
//                return;
//            }
//            //6
//            if (EdgePoints[X - 1, Y + 1] == 2)
//            {
//                EdgeMap[X - 1, Y + 1] = 1;
//                VisitedMap[X - 1, Y + 1] = 1;
//                Travers(X - 1, Y + 1);
//                return;
//            }
//            //7
//            if (EdgePoints[X, Y + 1] == 2)
//            {
//                EdgeMap[X , Y + 1] = 1;
//                VisitedMap[X, Y + 1] = 1;
//                Travers(X , Y + 1);
//                return;
//            }
//            //8

//            if (EdgePoints[X + 1, Y + 1] == 2)
//            {
//                EdgeMap[X + 1, Y + 1] = 1;
//                VisitedMap[X + 1, Y + 1] = 1;
//                Travers(X + 1, Y + 1);
//                return;
//            }
//            //VisitedMap[X, Y] = 1;
//            return;
//        }               
//        //Canny Class Ends
//    }

    }
}
