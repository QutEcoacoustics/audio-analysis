
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
                                            {-2.0,  0.0,  2.0},
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
        public static Tuple<double[,], double[,]> Gradient(double[,] m, double[,] edgeMaskX, double[,] edgeMaskY)
        {
            int MaximumXindex = m.GetLength(0);
            int MaximumYindex = m.GetLength(1);

            var gradientX = new double[MaximumXindex, MaximumYindex];
            var gradientY = new double[MaximumXindex, MaximumYindex];
            var result = new Tuple<double[,], double[,]>(gradientX, gradientY);
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
                    result.Item1[row, col] = sumX;
                    result.Item2[row, col] = sumY;
                }
            }

            return result;
        }

        public static double[,] GradientMagnitude(double[,] gradientX, double[,] gradientY)
        {
            int MaximumXindex = gradientX.GetLength(0);
            int MaximumYindex = gradientX.GetLength(1);

            double[,] result = new double[MaximumXindex, MaximumYindex];
           
            for (int row = 0; row < MaximumXindex; row++)
            {
                for (int col = 0; col < MaximumYindex; col++)
                {
                    // Manhattan distance not the Euclidean distance
                    result[row, col] = Math.Abs(gradientX[row, col]) + Math.Abs(gradientY[row, col]);
                }
            }

            return result;
        }

        // Finding the edge direction 
        public static double[,] GradientDirection(double[,] gradientX, double[,] gradientY)
        {
            int MaximumXindex = gradientX.GetLength(0);
            int MaximumYindex = gradientX.GetLength(1);

            double[,] newAngle = new double[MaximumXindex, MaximumYindex];

            for (int row = 0; row < MaximumXindex; row++)
            {
                for (int col = 0; col < MaximumYindex; col++)
                {
                    if (gradientX[row, col] == 0)
                    {
                        if (gradientY[row, col] == 0)
                        {
                            newAngle[row, col] = 0; 
                        }
                        else
                        {
                            newAngle[row, col] = 90; 
                        }
                    }
                    else
                    {
                        newAngle[row, col] = (Math.Atan(gradientY[row, col] / gradientX[row, col])) * 180 / Math.PI;
                        if ((-22.5 < newAngle[row, col] && newAngle[row, col] <= 22.5) || (157.5 < newAngle[row, col] && newAngle[row, col] <= -157.5))
                        {
                            newAngle[row, col] = 0;                          
                        }

                        if ((22.5 < newAngle[row, col] && newAngle[row, col] <= 67.5) || (-157.5 < newAngle[row, col] && newAngle[row, col] <= -112.5))
                        {
                            newAngle[row, col] = 45;                          
                        }

                        if ((67.5 < newAngle[row, col] && newAngle[row, col] <= 112.5) || (-112.5 < newAngle[row, col] && newAngle[row, col] <= -67.5))
                        {
                            newAngle[row, col] = 90;                           
                        }

                        if ((-67.5 < newAngle[row, col] && newAngle[row, col] <= -22.5) || (112.5 < newAngle[row, col] && newAngle[row, col] <= 157.5))
                        {
                            newAngle[row, col] = -45;                          
                        }
                    }                 
                }
            }

            return newAngle;
        }

        // Non-maximum suppression: Only local maxima should be marked as edges, it should be very thin like a one pixel.
        // still something wrong with horizontal and vertical line, because it still has two pixels wide. 
        public static double[,] NonMaximumSuppression(double[,] gradientMagnitude, double[,] direction, int neighborhoodSize)
        {
            int MaximumXindex = gradientMagnitude.GetLength(0);
            int MaximumYindex = gradientMagnitude.GetLength(1);
            int kernelRadius = neighborhoodSize / 2; 

            // check the direction 
            for (int i = kernelRadius; i < MaximumXindex - kernelRadius; i++)
            {
                for (int j = kernelRadius; j < MaximumYindex - kernelRadius; j++)
                {
                    // Horizontal edge
                    if (direction[i, j] == 0)
                    {
                        if ((gradientMagnitude[i, j] < gradientMagnitude[i, j + 1]) || (gradientMagnitude[i, j] < gradientMagnitude[i, j - 1]))
                        {
                            // check whether it's connected with local maxima
                            if (gradientMagnitude[i, j] == gradientMagnitude[i, j + 1] || gradientMagnitude[i, j] == gradientMagnitude[i, j - 1])
                            {

                            }
                            else
                            {
                                gradientMagnitude[i, j] = 0;
                            }
                        }                    
                    }

                    //45 Degree Edge
                    if (direction[i, j] == 45)
                    {
                        if ((gradientMagnitude[i, j] < gradientMagnitude[i - 1, j - 1]) || (gradientMagnitude[i, j] < gradientMagnitude[i + 1, j + 1]))
                        {
                            gradientMagnitude[i, j] = 0;
                        }
                    }

                    // Vertical Edge
                    if (direction[i, j] == 90)
                    {
                        if ((gradientMagnitude[i, j] < gradientMagnitude[i + 1, j]) || (gradientMagnitude[i, j] < gradientMagnitude[i - 1, j]))
                        {
                            gradientMagnitude[i, j] = 0;
                        }
                    }

                    //-45 Degree Edge
                    if (direction[i, j] == -45)
                    {
                        if ((gradientMagnitude[i, j] < gradientMagnitude[i + 1, j - 1]) || (gradientMagnitude[i, j] < gradientMagnitude[i - 1, j + 1]))
                        {
                            gradientMagnitude[i, j] = 0;
                        }               
                    }
                }
            }

            return gradientMagnitude;
        }
  
      //4.Double thresholding: Potential edges are determined by thresholding.  Canny detection algorithm uses double thresholding. Edge pixels stronger than 
      // the high threshold are marked as strong; edge pixels weaker than the low threshold are marked as weak are suppressed and edge poxels between the two
      // thresholds are marked as weak.   The vaule is 2.0, 4.0, 6.0, 8.0
        public static double[,] DoubleThreshold(double[,] nonMaxima)
        {         
            int MaximumXindex = nonMaxima.GetLength(0);
            int MaximumYindex = nonMaxima.GetLength(1);
    
            double minimum; double maximum;
            DataTools.MinMax(nonMaxima, out minimum, out maximum);
            var normMatrix = DataTools.normalise(nonMaxima);
            // Calculate the histogram of pixel intensity
            const int maxIndex = 100;
            var histogram = new int[maxIndex + 1];
            for (int row = 1; row < MaximumXindex; row++)
            {
                for (int col = 21; col < MaximumYindex; col++)
                {
                    for (int i = 33; i <= maxIndex; i++)
                    {
                        if (normMatrix[row, col] >= (i / (double)maxIndex) && normMatrix[row, col] < ((i + 1) / (double)maxIndex))
                        {
                            histogram[i]++;
                            break;
                        }
                    }
                }
            }
            var sum = 0;
            var numberOfPixel = MaximumXindex * MaximumYindex;
            double highThreshold = 0.0;
            double lowThreshold = 0.0;

            for (int j = maxIndex; j >= 0 ; j--)
            {   
                sum = sum + histogram[j];
                var percentageOfStrongEdge = 0.07;
                if (sum >= percentageOfStrongEdge * numberOfPixel)
                {
                    highThreshold = j * 1 / (double)maxIndex;
                    break; 
                }
            }
            lowThreshold = 0.5 * highThreshold;

            for (int row = 0; row < MaximumXindex; row++)
            {
                for (int col = 0; col < MaximumYindex; col++)
                {
                    if (normMatrix[row, col] > highThreshold)
                    {
                        normMatrix[row, col] = 1.0;
                    }
                    else
                    {
                        if (normMatrix[row, col] < lowThreshold)
                        {
                            normMatrix[row, col] = 0.0;
                        }
                        else
                        {
                            normMatrix[row, col] = 0.5;
                        }
                    }
                }
            }
            //double highThreshold = 1.66 * median;
            //double lowThreshold = 0.66 * median;
            //var median = (minimum + maximum) / 2; 
            //double highThreshold = 1.66 * median;
            //double lowThreshold = 0.66 * median;

            //for (int row = 0; row < MaximumXindex; row++)
            //{
            //    for (int col = 0; col < MaximumYindex; col++)
            //    {
            //        if (nonMaxima[row, col] >= highThreshold)
            //        {
            //            nonMaxima[row, col] = 1.0;
            //        }
            //        else
            //        {
            //            if (nonMaxima[row, col] < lowThreshold)
            //            {
            //                nonMaxima[row, col] = 0.0;
            //            }
            //            else
            //            {
            //                nonMaxima[row, col] = 0.5;
            //            }
            //        }
            //    }
            //}
            return normMatrix;
        }

     //5.Edge tracking by hysteresis: Final edges are determined by suppressing all edges that are not connected to a very certain (strong) edge.
        public static double[,] HysterisisThresholding(double[,] nonMaximaEdges, int kernelSize)
        {
            //travel in a 3 * 3 neighbourhood
            int MaximumXindex = nonMaximaEdges.GetLength(0);
            int MaximumYindex = nonMaximaEdges.GetLength(1); 
            int kernelRadius = kernelSize / 2;

            // only check the weak edge, here its intensity should be 0.5

            var edgeMap = new double[MaximumXindex, MaximumYindex];
            for (int i = kernelRadius; i < (MaximumXindex - 1) - kernelRadius; i++)
            {
                for (int j = kernelRadius; j < (MaximumYindex - 1) - kernelRadius; j++)
                {
                    if (nonMaximaEdges[i, j] == 0.5)
                    {
                        // Do the traverse
                        // 1
                        if (nonMaximaEdges[i + 1, j] == 1.0)
                        {
                            nonMaximaEdges[i,j] = 1.0;
                        }
                        // 2
                        if (nonMaximaEdges[i + 1, j - 1] == 1.0)
                        {
                            nonMaximaEdges[i,j] = 1.0;
                        }
                        // 3
                        if (nonMaximaEdges[i - 1, j] == 1.0)
                        {
                            nonMaximaEdges[i - 1, j] = 1.0;
                        }
                        // 4
                        if (nonMaximaEdges[i - 1, j - 1] == 1.0)
                        {
                            nonMaximaEdges[i,j] = 1.0;
                            
                        }
                        // 5
                        if (nonMaximaEdges[i - 1, j] == 1.0)
                        {
                            nonMaximaEdges[i,j] = 1.0;
                           
                        }
                        // 6
                        if (nonMaximaEdges[i - 1, j + 1] == 1.0)
                        {
                            nonMaximaEdges[i,j] = 1.0;
                           
                        }
                        // 7
                        if (nonMaximaEdges[i, j + 1] == 1.0)
                        {
                            nonMaximaEdges[i,j] = 1.0;
                            
                        }
                        // 8
                        if (nonMaximaEdges[i + 1, j + 1] == 1.0)
                        {
                            nonMaximaEdges[i,j] = 1.0;
                            
                        }
                    }
                }
            }

            return nonMaximaEdges;
        }

    }
}
