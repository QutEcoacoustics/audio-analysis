
namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Text;
    using TowseyLib;
    using System.Data;   
    using System.Drawing;

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

        // Generate the gaussian kernel
        public static double[,] GenerateGaussianKernel(int sizeOfKernel, double sigma)
        {
            double pi = (double)Math.PI;
            var kernel = new double[sizeOfKernel, sizeOfKernel];
            double coeffients1 = 2 * pi * sigma * sigma;
            double coeffients2 = 2 * sigma * sigma;

            int kernelRadius = sizeOfKernel / 2; 
 
            double sum = 0.0;
            for (int i = -kernelRadius; i <= kernelRadius; i++)
            {
                for (int j = -kernelRadius; j <= kernelRadius; j++)
                {
                    kernel[kernelRadius + i, kernelRadius + j] = (1 / coeffients1) * (double)Math.Exp(-(Math.Pow(i, 2) + Math.Pow(j, 2)) / coeffients2);
                    sum += kernel[kernelRadius + i, kernelRadius + j];
                }
            }
            //the process of normalizing the kernel elements            
            for (int i = 0; i < sizeOfKernel; i++)
            {
                for (int j = 0; j < sizeOfKernel; j++)
                {
                    kernel[i, j] /= sum;
                }
            }

            return kernel;
        }

        // Gaussian Filtering, smoothing the image
        public static double[,] GaussianFilter(double[,] m, double[,] gaussianKernel)
        {
            int MaximumXindex = m.GetLength(0);
            int MaximumYindex = m.GetLength(1);
            var result = new double [MaximumXindex, MaximumYindex];

            int gaussianKernelRadius = gaussianKernel.GetLength(0) / 2;

            for (int row = gaussianKernelRadius; row < (MaximumXindex - gaussianKernelRadius); row++)
            {
                for (int col = gaussianKernelRadius; col <(MaximumYindex  - gaussianKernelRadius); col++)
                {
                    double sum = 0;
                    for (int i = -gaussianKernelRadius; i <= gaussianKernelRadius; i++)
                    {
                        for (int j = -gaussianKernelRadius; j <= gaussianKernelRadius; j++)
                        {
                            sum = sum + m[row + i, col + j] * gaussianKernel[i + gaussianKernelRadius, j + gaussianKernelRadius];
                        }
                    }
                    result[row, col] = sum;
                }
            }

            return result;
        }

        // Finding gradients(also known as the edge strenghs): The edges should be marked where the gradients of the image has large magnitudes.
        // Sobel X and Y Masks are used to generate X & Y Gradients of Image; 
        // here the magnitude = |GX| + |GY| , OR it can be equal to euclidean distance measure 
        public static double[,] FindGradient(double[,] m, double[,] edgeMaskX, double[,] edgeMaskY)
        {
            int MaximumXindex = m.GetLength(0);
            int MaximumYindex = m.GetLength(1);

            double[,] result = new double[MaximumXindex, MaximumYindex];
            int edgeMaskRadius = edgeMaskX.GetLength(0) / 2;

            for (int row = edgeMaskRadius; row < (MaximumXindex - edgeMaskRadius); row++)
            {
                for (int col = edgeMaskRadius; col < (MaximumYindex - edgeMaskRadius); col++)
                {
                    var sumX = 0.0;
                    var sumY = 0.0;
                    for (int i = -edgeMaskRadius; i <= edgeMaskRadius; i++)
                    {
                        for (int j = -edgeMaskRadius; j <= edgeMaskRadius; j++)
                        {
                            sumX = sumX + m[row + i, col + j] * edgeMaskX[i + edgeMaskRadius, j + edgeMaskRadius];
                            sumY = sumY + m[row + i, col + j] * edgeMaskY[i + edgeMaskRadius, j + edgeMaskRadius];
                        }
                    }
                    result[row, col] = Math.Abs(sumX) + Math.Abs(sumY);
                }
            }

            return result;
        }

        // Finding the edge direction 
        public static double[,] FindGradientDirection(double[,] gradientX, double[,] gradientY)
        {
            int MaximumXindex = gradientX.GetLength(0);
            int MaximumYindex = gradientX.GetLength(1);

            double[,] result = new double[MaximumXindex, MaximumYindex];

            for (int row = 0; row < MaximumXindex; row++)
            {
                for (int col = 0; col < MaximumYindex; col++)
                {
                    if (gradientX[row, col] == 0)
                    {
                        if (gradientY[row, col] == 0)
                        {
                            result[row, col] = 0; 
                        }
                        else
                        {
                            result[row, col] = 90; 
                        }
                    }
                    else
                    {
                        result[row, col] = (Math.Atan(gradientY[row, col] / gradientX[row, col])) * 180 / Math.PI;
                    }
                }
            }

            return result;
        }

        // Non-maximum suppression: Only local maxima should be marked as edges.

        public static double[,] NonMaximumSuppression(double[,] gradient, double[,] direction, int kernelSize)
        {
            int MaximumXindex = gradient.GetLength(0);
            int MaximumYindex = gradient.GetLength(1);
            int kernelRadius = kernelSize / 2; 

            var result = new double[MaximumXindex, MaximumYindex];
            
            for (int row = 0; row < MaximumXindex; row++)
            {
                for (int col = 0; col < MaximumYindex; col++)
                {
                    result[row, col] = gradient[row, col];
                }
            }

            // check the direction 
            for (int i = kernelRadius; i < MaximumXindex - kernelRadius; i++)
            {
                for (int j = kernelRadius; j < MaximumYindex - kernelRadius; j++)
                {
                    // Horizontal edge
                    if ((-22.5 < direction[i,j] && direction[i,j] <= 22.5) || (157.5 < direction[i,j] && direction[i,j] <= 157.5))
                    {
                        if ((gradient[i, j] < gradient[i, j + 1]) || (gradient[i, j] < gradient[i, j - 1])) 
                        {
                            result[i, j] = 0;
                        }                   
                    }
                    
                    // Vertical Edge
                    if ((-112.5 < direction[i,j] && direction[i,j] <= -67.5) || (67.5 < direction[i,j] && direction[i,j] <= 112.5))
                    {
                        if ((gradient[i, j] < gradient[i + 1, j]) || (gradient[i, j] < gradient[i - 1, j])) 
                        {
                            result[i, j] = 0;
                        }                   
                    }

                    // +45 Degree Edge
                    if ((-67.5 < direction[i,j] && direction[i,j] <= -22.5) || (112.5 < direction[i,j] && direction[i,j] <= 157.5))
                    {
                        if ((gradient[i, j] < gradient[i + 1, j - 1]) || (gradient[i, j] < gradient[i - 1, j + 1])) 
                        {
                            result[i, j] = 0;
                        }                   
                    }

                    //-45 Degree Edge
                    if ((-157.5 < direction[i,j] && direction[i,j] <= -112.5) || (67.5 < direction[i,j] && direction[i,j] <= 22.5))
                    {
                        if ((gradient[i, j] < gradient[i + 1, j + 1]) || (gradient[i, j] < gradient[i - 1, j - 1])) 
                        {
                            result[i, j] = 0;
                        }                   
                    }

                }
            }

            return result;
        }
  
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
