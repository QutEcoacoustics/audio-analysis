
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
    using AudioAnalysisTools;

    class ImageAnalysisTools
    {
        #region public property

        // 7 * 7 gaussian blur
        public static double[,] gaussianBlur7 = {{0.00000067,	0.00002292,	0.00019117,	0.00038771,	0.00019117,	0.00002292,	0.00000067},
                                                {0.00002292,	0.00078633,	0.00655965,	0.01330373,	0.00655965,	0.00078633,	0.00002292},
                                                {0.00019117,	0.00655965,	0.05472157,	0.11098164,	0.05472157,	0.00655965,	0.00019117},
                                                {0.00038771,	0.01330373,	0.11098164,	0.22508352,	0.11098164,	0.01330373,	0.00038771},
                                                {0.00019117,	0.00655965,	0.05472157,	0.11098164,	0.05472157,	0.00655965,	0.00019117},
                                                {0.00002292,	0.00078633,	0.00655965,	0.01330373,	0.00655965,	0.00078633,	0.00002292},
                                                {0.00000067,	0.00002292,	0.00019117,	0.00038771,	0.00019117,	0.00002292,	0.00000067}};

        //5 * 5 gaussian blur
        public static double[,] gaussianBlur5 = {{0.0000,       0.0000,     0.0002,     0.0000,    0.0000},
                                                 {0.0000,       0.0113,     0.0837,     0.0113,    0.0000},
                                                 {0.0002,       0.0837,     0.6187,     0.0837,    0.0002},
                                                 {0.0000,       0.0113,     0.0837,     0.0113,    0.0000},
                                                 {0.0000,       0.0000,     0.0002,     0.0000,    0.0000}};

        // 3* 3 Sobel edge mask
        public static double[,] SobelX =  { {-1.0,  0.0,  1.0},
                                            {-2.0,  0.0,  2.0},
                                            {-1.0,  0.0,  1.0} };

        public static double[,] SobelY =  { { 1.0,  2.0,  1.0},
                                            { 0.0,  0.0,  0.0},
                                            {-1.0, -2.0, -1.0} };

        // 7 * 7 rectangular Sobel ridge mask
        public static double[,] SobelRidge7X = { {0.0,  0.0,  -1.0,  2.0,  -1.0,  0.0,  0.0},
                                                 {0.0,  0.0,  -1.0,  2.0,  -1.0,  0.0,  0.0},
                                                 {0.0,  0.0,  -1.0,  2.0,  -1.0,  0.0,  0.0},
                                                 {0.0,  0.0,  -1.0,  2.0,  -1.0,  0.0,  0.0},
                                                 {0.0,  0.0,  -1.0,  2.0,  -1.0,  0.0,  0.0},
                                                 {0.0,  0.0,  -1.0,  2.0,  -1.0,  0.0,  0.0},
                                                 {0.0,  0.0,  -1.0,  2.0,  -1.0,  0.0,  0.0},
                                               };

        public static double[,] SobelRidge7Y = { { 0.0,  0.0,    0.0,   0.0,   0.0,   0.0,   0.0 },
                                                 { 0.0,  0.0,    0.0,   0.0,   0.0,   0.0,   0.0 },
                                                 {-1.0, -1.0,   -1.0,  -1.0,  -1.0,  -1.0,  -1.0 },
                                                 { 2.0,  2.0,    2.0,   2.0,   2.0,   2.0,   2.0 },
                                                 {-1.0, -1.0,   -1.0,  -1.0,  -1.0,  -1.0,  -1.0 },
                                                 { 0.0,  0.0,    0.0,   0.0,   0.0,   0.0,   0.0 },
                                                 { 0.0,  0.0,    0.0,   0.0,   0.0,   0.0,   0.0 },
                                               };

        // 3 * 3 sobel ridge mask
        public static double[,] SobelRidge3X = { {-1.0,  2.0,  -1.0},
                                                 {-2.0,  4.0,  -2.0},
                                                 {-1.0,  2.0,  -1.0} };

        public static double[,] SobelRidge3Y = { {-1.0,  -2.0,  -1.0},
                                                 { 2.0,   4.0,   2.0},
                                                 {-1.0,  -2.0,  -1.0} };

        // 5 * 5 sobel ridge mask
        //public static double[,] SobelRidge5X = { { 0.0,  -1.0,  2.0,  -1.0,  0.0},
        //                                         {-1.0,  -2.0,  4.0,  -2.0, -1.0},
        //                                         {-2.0,  -4.0,  12.0, -4.0, -2.0},
        //                                         {-1.0,  -2.0,  4.0,  -2.0, -1.0},
        //                                         { 0.0,  -1.0,  2.0,  -1.0, 0.0} };

        public static double[,] SobelRidge5X = { {-1.0,  -1.0,  4.0,  -1.0, -1.0},
                                                 {-1.0,  -1.0,  4.0,  -1.0, -1.0},
                                                 {-1.0,  -1.0,  4.0,  -1.0, -1.0},
                                                 {-1.0,  -1.0,  4.0,  -1.0, -1.0},
                                                 {-1.0,  -1.0,  4.0,  -1.0, -1.0} };


        //public static double[,] SobelRidge5Y = { {0.0,   -1.0,  -2.0, -1.0,  0.0},
        //                                         {-1.0,  -2.0,  -4.0, -2.0, -1.0},
        //                                         {2.0,    4.0,  12.0,  4.0,  2.0},
        //                                         {-1.0,  -2.0,  -4.0, -2.0, -1.0}, 
        //                                         {0.0,   -1.0,  -2.0, -1.0,  0.0} };

        public static double[,] SobelRidge5Y = { {-1.0,  -1.0,  -1.0,  -1.0, -1.0},
                                                 {-1.0,  -1.0,  -1.0,  -1.0, -1.0},
                                                 { 4.0,   4.0,   4.0,   4.0,  4.0},
                                                 {-1.0,  -1.0,  -1.0,  -1.0, -1.0},
                                                 {-1.0,  -1.0,  -1.0,  -1.0, -1.0} };

        // 5 * 5 Corner mask, still need to think about it
        public static double[,] SobelCorner = { {0.0,  -1.0,  2.0,  -1.0,   0.0},
                                                {0.0,  -1.0,  2.0,  -1.0,  -1.0},
                                                {0.0,  -1.0,  2.0,   2.0,   2.0},
                                                {0.0,  -1.0, -1.0,  -1.0,  -1.0},
                                                {0.0,   0.0,  0.0,   0.0,   0.0} };

        public static double[,] SobelCornerY = { { 0.0,  -1.0,  2.0,  -1.0,  0.0},
                                                {-1.0,  -2.0,  4.0,  -2.0, -1.0},
                                                {-2.0,  -4.0,  12.0, -4.0, -2.0},
                                                {-1.0,  -2.0,  4.0,  -2.0, -1.0},
                                                { 0.0,  -1.0,  2.0,  -1.0,  0.0} };

        public const double Pi = Math.PI;

        #endregion
        /// Canny detector for edge detection in a noisy image 
        /// it involves five steps here, first, it needs to do the Gaussian convolution, 
        /// then a simple derivative operator(like Roberts Cross or Sobel operator) is applied to the smoothed image to highlight regions of the image. 

        #region Public Methods
        // Generate the gaussian kernel automatically
        public static double[,] GenerateGaussianKernel(int sizeOfKernel, double sigma)
        {

            var kernel = new double[sizeOfKernel, sizeOfKernel];
            double coeffients1 = 2 * Pi * sigma * sigma;
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

        // Gaussian Filtering, smoothing the image using gaussian kernel convolution
        public static double[,] GaussianFilter(double[,] m, double[,] gaussianKernel)
        {
            int MaximumXindex = m.GetLength(0);
            int MaximumYindex = m.GetLength(1);
            var result = new double[MaximumXindex, MaximumYindex];

            int gaussianKernelRadius = gaussianKernel.GetLength(0) / 2;

            for (int row = gaussianKernelRadius; row < (MaximumXindex - gaussianKernelRadius); row++)
            {
                for (int col = gaussianKernelRadius; col < (MaximumYindex - gaussianKernelRadius); col++)
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

        // Finding gradients(it is actually a vector), we could use different edgemasks to get it
        // here, Sobel X and Y Masks are used to generate X & Y Gradients of Image
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
                    if (m[row, col] > 0.3)
                    {
                        for (int i = -edgeMaskRadius; i <= edgeMaskRadius; i++)
                        {
                            for (int j = -edgeMaskRadius; j <= edgeMaskRadius; j++)
                            {
                                sumX = sumX + m[row + j, col + i] * edgeMaskX[i + edgeMaskRadius, j + edgeMaskRadius];
                                sumY = sumY + m[row + j, col + i] * edgeMaskY[i + edgeMaskRadius, j + edgeMaskRadius];
                            }
                        }
                    }
                    // on average to normalise the value for 5 * 5 ridge detection
                    result.Item1[row, col] = sumX / 20;
                    result.Item2[row, col] = sumY / 20;
                }
            }

            return result;
        }

        // For calculating the gradient, we can also use some edgeMask with equal weights(all are equal to 1.0)
        // it could be 3*3, 5*5, 7*7......
        public static Tuple<double[,], double[,]> GradientWithEqualWeightsMask(double[,] m, int sizeOfMask)
        {
            int MaximumXindex = m.GetLength(0);
            int MaximumYindex = m.GetLength(1);

            var gradientX = new double[MaximumXindex, MaximumYindex];
            var gradientY = new double[MaximumXindex, MaximumYindex];
            var result = new Tuple<double[,], double[,]>(gradientX, gradientY);
            int edgeMaskRadius = sizeOfMask / 2;

            for (int row = edgeMaskRadius; row < (MaximumXindex - edgeMaskRadius); row++)
            {
                for (int col = edgeMaskRadius; col < (MaximumYindex - edgeMaskRadius); col++)
                {
                    var sumX = 0.0;
                    var sumY = 0.0;
                    for (int i = 1; i <= edgeMaskRadius; i++)
                    {
                        for (int j = -edgeMaskRadius; j <= edgeMaskRadius; j++)
                        {
                            sumX = sumX + (m[row + i, col + j] - m[row - i, col + j]) / 2.0;
                            sumY = sumY + (m[row + j, col - i] - m[row + j, col + i]) / 2.0;
                        }
                    }
                    result.Item1[row, col] = sumX;
                    result.Item2[row, col] = sumY;
                }
            }

            return result;
        }

        // From the gradient, we can get the gradientMagnitude which means the edge strengh. Simply put, it means how much the intensity of pixels changes in an image. 
        // If it is large, it changes quickly; otherwise, it changes slowly. 
        // Here there are two ways to calculate: Manhattan distance = |GX| + |GY| , OR we can also use euclidean distance measure to get it
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
                    // Euclidean distance
                    //result[row, col] = Math.Sqrt(Math.Pow(gradientX[row, col], 2) + Math.Pow(gradientY[row, col], 2));
                }
            }

            return result;
        }

        // Find the edge direction, from it we can know the intensity changes come along with which direction
        // But, its direction always perpendicular with the direction of normal edge
        // this is done based on the gradient of sobel edge mask, so it only calculate the direction in a 3* 3 neighbourhood 
        public static double[,] GradientDirection(double[,] gradientX, double[,] gradientY, double[,] gradientMagnitude)
        {
            int MaximumXindex = gradientX.GetLength(0);
            int MaximumYindex = gradientX.GetLength(1);

            double[,] newAngle = new double[MaximumXindex, MaximumYindex];

            for (int row = 0; row < MaximumXindex; row++)
            {
                for (int col = 0; col < MaximumYindex; col++)
                {
                    // here it's kind of tricky thing 
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
                        //newAngle[row, col] = (Math.Atan(gradientY[row, col] / gradientX[row, col])) * 180 / Pi;
                        newAngle[row, col] = Math.Atan(gradientY[row, col] / gradientX[row, col]);

                        const double MinNegativeZero = -7 * Pi / 8;
                        const double MaxNegativeZero = -Pi / 8;
                        const double MinPositiveZero = Pi / 8;
                        const double MaxPositiveZero = 7 * Pi / 8;

                        // the degree of direction is around 45/-135 degree neighbourhood
                        const double MinNegative45 = -7 * Pi / 8;
                        const double MinPositive45 = Pi / 8;
                        const double MaxNegative45 = -5 * Pi / 8;
                        const double MaxPositive45 = 3 * Pi / 8;

                        // the degree of direction is around 90/-90 degree neighbourhood
                        const double MaxNegative90 = -3 * Pi / 8;
                        const double MinPositive90 = 3 * Pi / 8;
                        const double MinNegative90 = -5 * Pi / 8;
                        const double MaxPositive90 = 5 * Pi / 8;

                        // the degree of direction is around -45/135 degree neighbourhood
                        const double MinNegative135 = -3 * Pi / 8;
                        const double MinPositive135 = 5 * Pi / 8;
                        const double MaxNegative135 = -Pi / 8;
                        const double MaxPositive135 = 7 * Pi / 8;

                        if ((MaxNegativeZero < newAngle[row, col] && newAngle[row, col] <= MinPositiveZero) || (MaxPositiveZero < newAngle[row, col]) || (newAngle[row, col] <= MinNegativeZero))
                        {
                            newAngle[row, col] = 0;
                        }

                        if ((MinPositive45 < newAngle[row, col] && newAngle[row, col] <= MaxPositive45) || (MinNegative45 < newAngle[row, col] && newAngle[row, col] <= MaxNegative45))
                        {
                            newAngle[row, col] = 45;
                        }

                        if ((MinPositive90 < newAngle[row, col] && newAngle[row, col] <= MaxPositive90) || (MinNegative90 < newAngle[row, col] && newAngle[row, col] <= MaxNegative90))
                        {
                            newAngle[row, col] = 90;
                        }

                        if ((MinPositive135 < newAngle[row, col] && newAngle[row, col] <= MaxPositive135) || (MinNegative135 < newAngle[row, col] && newAngle[row, col] <= MaxNegative135))
                        {
                            newAngle[row, col] = -45;
                        }
                    }
                }
            }
            return newAngle;
        }

        // Non-maximum suppression: Only local maxima (magnitude value) in a neighborhood should be marked as edges, it should be very thin like a one pixel.
        // here the size of neighbourhood is just 3 * 3
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
                    // Horizontal direction
                    if (direction[i, j] == 0)
                    {
                        if ((gradientMagnitude[i, j] < gradientMagnitude[i, j + 1]) || (gradientMagnitude[i, j] < gradientMagnitude[i, j - 1]))
                        {
                            gradientMagnitude[i, j] = 0.0;
                        }
                    }

                    //45 Degree direction
                    if (direction[i, j] == 45)
                    {
                        if ((gradientMagnitude[i, j] < gradientMagnitude[i - 1, j - 1]) || (gradientMagnitude[i, j] < gradientMagnitude[i + 1, j + 1]))
                        {
                            gradientMagnitude[i, j] = 0.0;
                        }
                    }

                    // Vertical direction
                    if (direction[i, j] == 90)
                    {
                        if ((gradientMagnitude[i, j] < gradientMagnitude[i + 1, j]) || (gradientMagnitude[i, j] < gradientMagnitude[i - 1, j]))
                        {
                            gradientMagnitude[i, j] = 0.0;
                        }
                    }

                    //-45 Degree direction
                    if (direction[i, j] == -45)
                    {
                        if ((gradientMagnitude[i, j] < gradientMagnitude[i + 1, j - 1]) || (gradientMagnitude[i, j] < gradientMagnitude[i - 1, j + 1]))
                        {
                            gradientMagnitude[i, j] = 0.0;
                        }
                    }
                }
            }

            return gradientMagnitude;
        }

        //4.Double thresholding: Potential edges are determined by thresholding.  Canny detection algorithm uses double thresholding. Edge pixels stronger than 
        // the high threshold are marked as strong; edge pixels weaker than the low threshold are marked as weak are suppressed and edge poxels between the two
        // thresholds are marked as weak.   The vaule is 2.0, 4.0, 6.0, 8.0
        public static double[,] DoubleThreshold(double[,] nonMaxima, double highThreshold, double lowThreshold)
        {
            int MaximumXindex = nonMaxima.GetLength(0);
            int MaximumYindex = nonMaxima.GetLength(1);

            for (int row = 0; row < MaximumXindex; row++)
            {
                for (int col = 0; col < MaximumYindex; col++)
                {

                    if (nonMaxima[row, col] > highThreshold)
                    {
                        nonMaxima[row, col] = 1.0;
                    }
                    else
                    {
                        if (nonMaxima[row, col] < lowThreshold)
                        {
                            nonMaxima[row, col] = 0.0;
                        }
                        else
                        {
                            nonMaxima[row, col] = 0.5;
                        }
                    }
                }
            }
            return nonMaxima;
        }

        // this linking work still can do something
        //5.Edge tracking by hysteresis: Final edges are determined by suppressing all edges that are not connected to a very strong edge.
        public static double[,] HysterisisThresholding(double[,] nonMaximaEdges, double[,] direction, int kernelSize)
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
                    if (nonMaximaEdges[i, j] > 0)
                    {
                        var sum = 0;
                        // Do the traverse
                        // 1
                        if (nonMaximaEdges[i + 1, j] > 0.0)
                        {
                            if (direction[i + 1, j] == direction[i, j])
                            {
                                sum++;
                                nonMaximaEdges[i, j] = 0.0;
                                continue;
                            }
                        }
                        // 2
                        if (nonMaximaEdges[i + 1, j - 1] > 0.0)
                        {
                            if (direction[i + 1, j - 1] == direction[i, j])
                            {
                                sum++;
                            }
                        }
                        // 3
                        if (nonMaximaEdges[i, j - 1] > 0.0)
                        {
                            if (direction[i, j - 1] == direction[i, j])
                            {
                                sum++;
                            }
                        }
                        // 4
                        if (nonMaximaEdges[i - 1, j - 1] > 0.0)
                        {
                            if (direction[i - 1, j - 1] == direction[i, j])
                            {
                                sum++;
                            }
                        }
                        // 5
                        if (nonMaximaEdges[i - 1, j] > 0.0)
                        {
                            if (direction[i - 1, j] == direction[i, j])
                            {
                                sum++;
                            }
                        }
                        // 6
                        if (nonMaximaEdges[i - 1, j + 1] > 0.0)
                        {
                            if (direction[i - 1, j + 1] == direction[i, j])
                            {
                                sum++;
                            }
                        }
                        // 7
                        if (nonMaximaEdges[i, j + 1] > 0.0)
                        {
                            if (direction[i, j + 1] == direction[i, j])
                            {
                                sum++;
                            }
                        }
                        // 8
                        if (nonMaximaEdges[i + 1, j + 1] > 0.0)
                        {
                            if (direction[i + 1, j + 1] == direction[i, j])
                            {
                                sum++;
                            }
                        }
                        if (sum >= 3)
                        {
                            nonMaximaEdges[i, j] = 0.0;
                        }
                    }
                }
            }

            return nonMaximaEdges;
        }

        // Making the edge thin, search in a neighbourhood
        public static double[,] Thinning(double[,] magnitude, double[,] direction, int sizeOfNeighbour)
        {
            int MaximumXindex = magnitude.GetLength(0);
            int MaximumYindex = magnitude.GetLength(1);

            var halfLength = sizeOfNeighbour / 2;
            var result = new double[MaximumXindex, MaximumYindex];
            for (int row = halfLength; row < MaximumXindex - halfLength; row++)
            {
                for (int col = halfLength; col < MaximumYindex - halfLength; col++)
                {
                    if (magnitude[row, col] > 0)
                    {
                        for (int i = -halfLength; i <= halfLength; i++)
                        {
                            for (int j = -halfLength; j <= halfLength; j++)
                            {
                                if (magnitude[row + i, col + j] > 0 && i != 0 && j != 0)
                                {
                                    if (direction[row + i, col + j] == direction[row, col])
                                    {
                                        magnitude[row, col] = 0.0;
                                        break;
                                    }
                                }
                            }
                        }
                        result[row, col] = magnitude[row, col];
                    }
                    else
                    {
                        result[row, col] = magnitude[row, col];
                    }
                }
            }

            return result;
        }

        // Remove edge with small intensity in a neighbourhood
        public static double[,] filterPointsOfInterest(double[,] magnitude, double[,] matrix, int sizeOfNeighbourhood)
        {
            int MaximumXindex = magnitude.GetLength(0);
            int MaximumYindex = magnitude.GetLength(1);

            var radiusOfNeighbourhood = sizeOfNeighbourhood / 2;
            var result = new double[MaximumXindex, MaximumYindex];

            for (int row = radiusOfNeighbourhood; row < MaximumXindex - radiusOfNeighbourhood; row++)
            {
                for (int col = radiusOfNeighbourhood; col < MaximumYindex - radiusOfNeighbourhood; col++)
                {
                    // Improved1 it works but still need to filter out more points
                    if (magnitude[row, col] > 0)
                    {
                        double threshold = 0.4;
                        // for ridge detection, check whether its intensity is greater than a value
                        if (matrix[row, col] > threshold)
                        {
                            result[row, col] = magnitude[row, col];
                        }
                    }
                }
            }
            //result = magnitude; 
            return result;
        }

        // Remove the poi which are too close in a 3 * 3 neighbourhood 
        public static double[,] removeClosePoi(double[,] magnitude, int sizeOfNeighbourhood)
        {
            int MaximumXindex = magnitude.GetLength(0);
            int MaximumYindex = magnitude.GetLength(1);

            var radiusOfNeighbourhood = sizeOfNeighbourhood / 2;
            var visitedFlag = new bool[MaximumXindex, MaximumYindex];
            var result = new double[MaximumXindex, MaximumYindex];

            for (int row = radiusOfNeighbourhood; row < MaximumXindex - radiusOfNeighbourhood; row++)
            {
                for (int col = radiusOfNeighbourhood; col < MaximumYindex - radiusOfNeighbourhood; col++)
                {
                    if (magnitude[row, col] > 0)
                    {
                        visitedFlag[row, col] = true;
                        for (int i = -radiusOfNeighbourhood; i <= radiusOfNeighbourhood; i++)
                        {
                            for (int j = -radiusOfNeighbourhood; j <= radiusOfNeighbourhood; j++)
                            {
                                if (magnitude[row + i, col + j] > 0 && visitedFlag[row + i, col + j] == false)
                                {
                                    magnitude[row, col] = 0.0;
                                    visitedFlag[row + i, col + j] = true;
                                }
                            }
                        }
                    }
                }
            }

            result = magnitude;
            return result;
        }
        /// <summary>
        /// This version of Sobel's edge detection taken from  Graig A. Lindley, Practical Image Processing
        /// which includes C code.
        /// HOWEVER MODIFED TO PROCESS 5x5 matrix
        /// MATRIX must be square with odd number dimensions
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static void Sobel5X5RidgeDetection4Direction(double[,] m, out bool isRidge, out double magnitude, out double direction)
        {
            // We have four possible ridges with slopes 0, Pi/4, pi/2, 3Pi/4
            // Slope categories are 0 to 3.
            // We calculate the ridge magnitude for each possible ridge direction using masks.

            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            if ((rows != cols) || (rows != 5)) // must be square 5X5 matrix 
            {
                isRidge = false;
                magnitude = 0.0;
                direction = 0.0;
                return;
            }

            double[,] ridgeDir0Mask = { {-0.1,-0.1,-0.1,-0.1,-0.1},
                                        {-0.1,-0.1,-0.1,-0.1,-0.1},
                                        { 0.4, 0.4, 0.4, 0.4, 0.4},
                                        {-0.1,-0.1,-0.1,-0.1,-0.1},
                                        {-0.1,-0.1,-0.1,-0.1,-0.1}
                                      };
           
            double[,] ridgeDir1Mask = { {-0.1,-0.1,-0.1,-0.1, 0.4},
                                        {-0.1,-0.1,-0.1, 0.4,-0.1},
                                        {-0.1,-0.1, 0.4,-0.1,-0.1},
                                        {-0.1, 0.4,-0.1,-0.1,-0.1},
                                        { 0.4,-0.1,-0.1,-0.1,-0.1}
                                      };
            
            double[,] ridgeDir2Mask = { {-0.1,-0.1, 0.4,-0.1,-0.1},
                                        {-0.1,-0.1, 0.4,-0.1,-0.1},
                                        {-0.1,-0.1, 0.4,-0.1,-0.1},
                                        {-0.1,-0.1, 0.4,-0.1,-0.1},
                                        {-0.1,-0.1, 0.4,-0.1,-0.1}
                                      };
            
            double[,] ridgeDir3Mask = { { 0.4,-0.1,-0.1,-0.1,-0.1},
                                        {-0.1, 0.4,-0.1,-0.1,-0.1},
                                        {-0.1,-0.1, 0.4,-0.1,-0.1},
                                        {-0.1,-0.1,-0.1, 0.4,-0.1},
                                        {-0.1,-0.1,-0.1,-0.1, 0.4}
                                      };
           
            double[] ridgeMagnitudes = new double[4];
            ridgeMagnitudes[0] = MatrixTools.DotProduct(ridgeDir0Mask, m);
            ridgeMagnitudes[1] = MatrixTools.DotProduct(ridgeDir1Mask, m);
            ridgeMagnitudes[2] = MatrixTools.DotProduct(ridgeDir2Mask, m);
            ridgeMagnitudes[3] = MatrixTools.DotProduct(ridgeDir3Mask, m);
            
            int indexMin, indexMax;
            double diffMin, diffMax;
            DataTools.MinMax(ridgeMagnitudes, out indexMin, out indexMax, out diffMin, out diffMax);

            double threshold = 0; // dB
            isRidge = (ridgeMagnitudes[indexMax] > threshold);
            magnitude = diffMax / 2;
            /// four directions
            direction = indexMax * Math.PI / (double)4;          
        }

        /// <summary>
        /// This function is used for calculating ridge detection in 8 directions. 
        /// </summary>
        /// <param name="m"></param>
        /// <param name="isRidge"></param>
        /// <param name="magnitude"> it is ridge magnitude output
        /// </param>
        /// <param name="direction"> it is ridge direction output
        /// </param>
        public static void Sobel5X5RidgeDetection8Direction(double[,] m, out bool isRidge, out double magnitude, out double direction)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            if ((rows != cols) || (rows != 5)) // must be square 5X5 matrix 
            {
                isRidge = false;
                magnitude = 0.0;
                direction = 0.0;
                return;
            }

            double[,] ridgeDir0Mask = { {-0.1,-0.1,-0.1,-0.1,-0.1},
                                        {-0.1,-0.1,-0.1,-0.1,-0.1},
                                        { 0.4, 0.4, 0.4, 0.4, 0.4},
                                        {-0.1,-0.1,-0.1,-0.1,-0.1},
                                        {-0.1,-0.1,-0.1,-0.1,-0.1}
                                      };
            double[,] ridgeDir1Mask = { {-0.1,-0.1,-0.1,-0.1,-0.1},
                                        {-0.1,-0.1,-0.1, 0.4, 0.4},
                                        {-0.1,-0.1, 0.4,-0.1,-0.1},
                                        { 0.4, 0.4,-0.1,-0.1,-0.1},
                                        {-0.1,-0.1,-0.1,-0.1,-0.1}
                                      };
            double[,] ridgeDir2Mask = { {-0.1,-0.1,-0.1,-0.1, 0.4},
                                        {-0.1,-0.1,-0.1, 0.4,-0.1},
                                        {-0.1,-0.1, 0.4,-0.1,-0.1},
                                        {-0.1, 0.4,-0.1,-0.1,-0.1},
                                        { 0.4,-0.1,-0.1,-0.1,-0.1}
                                      };
            double[,] ridgeDir3Mask = { {-0.1,-0.1,-0.1, 0.4,-0.1},
                                        {-0.1,-0.1,-0.1, 0.4,-0.1},
                                        {-0.1,-0.1, 0.4,-0.1,-0.1},
                                        {-0.1, 0.4,-0.1,-0.1,-0.1},
                                        {-0.1, 0.4,-0.1,-0.1,-0.1}
                                      };
            double[,] ridgeDir4Mask = { {-0.1,-0.1, 0.4,-0.1,-0.1},
                                        {-0.1,-0.1, 0.4,-0.1,-0.1},
                                        {-0.1,-0.1, 0.4,-0.1,-0.1},
                                        {-0.1,-0.1, 0.4,-0.1,-0.1},
                                        {-0.1,-0.1, 0.4,-0.1,-0.1}
                                      };
            double[,] ridgeDir5Mask = { {-0.1, 0.4,-0.1,-0.1,-0.1},
                                        {-0.1, 0.4,-0.1,-0.1,-0.1},
                                        {-0.1,-0.1, 0.4,-0.1,-0.1},
                                        {-0.1,-0.1,-0.1, 0.4,-0.1},
                                        {-0.1,-0.1,-0.1, 0.4,-0.1}
                                      };
            double[,] ridgeDir6Mask = { { 0.4,-0.1,-0.1,-0.1,-0.1},
                                        {-0.1, 0.4,-0.1,-0.1,-0.1},
                                        {-0.1,-0.1, 0.4,-0.1,-0.1},
                                        {-0.1,-0.1,-0.1, 0.4,-0.1},
                                        {-0.1,-0.1,-0.1,-0.1, 0.4}
                                      };
            double[,] ridgeDir7Mask = { {-0.1,-0.1,-0.1,-0.1,-0.1},
                                        { 0.4, 0.4,-0.1,-0.1,-0.1},
                                        {-0.1,-0.1, 0.4,-0.1,-0.1},
                                        {-0.1,-0.1,-0.1, 0.4, 0.4},
                                        {-0.1,-0.1,-0.1,-0.1,-0.1}
                                      };

            double[] ridgeMagnitudes = new double[8];
            ridgeMagnitudes[0] = MatrixTools.DotProduct(ridgeDir0Mask, m);
            ridgeMagnitudes[1] = MatrixTools.DotProduct(ridgeDir1Mask, m);
            ridgeMagnitudes[2] = MatrixTools.DotProduct(ridgeDir2Mask, m);
            ridgeMagnitudes[3] = MatrixTools.DotProduct(ridgeDir3Mask, m);
            ridgeMagnitudes[4] = MatrixTools.DotProduct(ridgeDir4Mask, m);
            ridgeMagnitudes[5] = MatrixTools.DotProduct(ridgeDir5Mask, m);
            ridgeMagnitudes[6] = MatrixTools.DotProduct(ridgeDir6Mask, m);
            ridgeMagnitudes[7] = MatrixTools.DotProduct(ridgeDir7Mask, m);

            int indexMin, indexMax;
            double diffMin, diffMax;
            DataTools.MinMax(ridgeMagnitudes, out indexMin, out indexMax, out diffMin, out diffMax);

            double threshold = 0; // dB
            isRidge = (ridgeMagnitudes[indexMax] > threshold);
            magnitude = diffMax / 2;
            direction = indexMax * Math.PI / (double)8;
        }

        public static double[,] SobelEdgeDetectorImproved(double[,] m, double relThreshold)
        {
            //define indices into grid using Lindley notation
            const int a = 0; const int b = 1; const int c = 2; const int d = 3; const int e = 4;
            const int f = 5; const int g = 6; const int h = 7; const int i = 8;
            int mRows = m.GetLength(0);
            int mCols = m.GetLength(1);
            double[,] normM = DataTools.normalise(m);
            double[,] newMatrix = new double[mRows, mCols];//init new matrix to return
            double[] grid = new double[9]; //to represent 3x3 grid
            double min = Double.MaxValue; double max = -Double.MaxValue;

            for (int y = 1; y < mRows - 1; y++)
                for (int x = 1; x < mCols - 1; x++)
                {
                    grid[a] = normM[y - 1, x - 1];
                    grid[b] = normM[y, x - 1];
                    grid[c] = normM[y + 1, x - 1];
                    grid[d] = normM[y - 1, x];
                    grid[e] = normM[y, x];
                    grid[f] = normM[y + 1, x];
                    grid[g] = normM[y - 1, x + 1];
                    grid[h] = normM[y, x + 1];
                    grid[i] = normM[y + 1, x + 1];
                    double[] differences = new double[4];
                    double DivideAEI_avBelow = (grid[d] + grid[g] + grid[h]) / (double)3;
                    double DivideAEI_avAbove = (grid[b] + grid[c] + grid[f]) / (double)3;
                    differences[0] = Math.Abs(DivideAEI_avAbove - DivideAEI_avBelow);

                    double DivideBEH_avBelow = (grid[a] + grid[d] + grid[g]) / (double)3;
                    double DivideBEH_avAbove = (grid[c] + grid[f] + grid[i]) / (double)3;
                    differences[1] = Math.Abs(DivideBEH_avAbove - DivideBEH_avBelow);

                    double DivideCEG_avBelow = (grid[f] + grid[h] + grid[i]) / (double)3;
                    double DivideCEG_avAbove = (grid[a] + grid[b] + grid[d]) / (double)3;
                    differences[2] = Math.Abs(DivideCEG_avAbove - DivideCEG_avBelow);

                    double DivideDEF_avBelow = (grid[g] + grid[h] + grid[i]) / (double)3;
                    double DivideDEF_avAbove = (grid[a] + grid[b] + grid[c]) / (double)3;
                    differences[3] = Math.Abs(DivideDEF_avAbove - DivideDEF_avBelow);
                    double gridMin; double gridMax;
                    DataTools.MinMax(differences, out gridMin, out gridMax);

                    newMatrix[y, x] = gridMax;
                    if (min > gridMin) min = gridMin;
                    if (max < gridMax) max = gridMax;
                }

            //double relThreshold = 0.2;
            double threshold = min + ((max - min) * relThreshold);

            for (int y = 1; y < mRows - 1; y++)
            {
                for (int x = 1; x < mCols - 1; x++)
                {
                    if (newMatrix[y, x] <= threshold) newMatrix[y, x] = 0.0;
                }

            }
            return newMatrix;
        }

        public static Tuple<double[,], double[,]> CannyEdgeDetector(double[,] matrix)
        {
            // For using Michael's sobel edge algorithm

            var MaximumXIndex = matrix.GetLength(0);
            var MaximumYIndex = matrix.GetLength(1);
            var result1 = new double[MaximumXIndex, MaximumYIndex];
            var result2 = new double[MaximumXIndex, MaximumYIndex];
            var result = new Tuple<double[,], double[,]>(result1, result2);
            var sizeOfMatrix = 5;
            var radiusOfMatrix = sizeOfMatrix / 2;
            var magnitudeThreshold = 0.15;

            for (int row = radiusOfMatrix; row < MaximumXIndex - radiusOfMatrix; row++)
            {
                for (int col = radiusOfMatrix; col < MaximumYIndex - radiusOfMatrix; col++)
                {
                    var subMatrix = MatrixTools.Submatrix(matrix, row - radiusOfMatrix, col - radiusOfMatrix, row + radiusOfMatrix, col + radiusOfMatrix);
                    double magnitude, direction;
                    bool isRidge = false;

                    TowseyLib.ImageTools.Sobel5X5RidgeDetection(subMatrix, out isRidge, out magnitude, out direction);
                    if (isRidge && (magnitude > magnitudeThreshold))
                    {
                        result1[row, col] = 1.0;
                        result2[row, col] = direction;
                    }
                }
            }

            //var sizeOfRidge = 5;
            //var halfLength = sizeOfRidge/2; 
            // better be odd number 3, 5
            //var kernelSizeOfGaussianBlur = 5;
            //double SigmaOfGaussianBlur = 1.0;
            //var gaussianFilter = ImageAnalysisTools.GaussianFilter(matrix, ImageAnalysisTools.GenerateGaussianKernel(kernelSizeOfGaussianBlur, SigmaOfGaussianBlur));
            //var gradient = ImageAnalysisTools.GradientWithEqualWeightsMask(matrix, 5);
            //var gradient = ImageAnalysisTools.Gradient(matrix, SobelX, SobelY);

            //var gradient = ImageAnalysisTools.Gradient(matrix, SobelRidge5X, SobelRidge5Y);
            ////var gradientMagnitude = ImageAnalysisTools.GradientMagnitude(gradient.Item1, gradient.Item2);
            //////var gradientMagnitude = ImageAnalysisTools.SobelEdgeDetectorImproved(matrix, 0.2);
            ////var gradientDirection = ImageAnalysisTools.GradientDirection(gradient.Item1, gradient.Item2, gradientMagnitude);

            //var nonMaxima = ImageAnalysisTools.NonMaximumSuppression(gradientMagnitude, gradientDirection, 3);
            ////var sobelEdge = ImageTools.SobelEdgeDetection(matrix);
            //var doubleThreshold = ImageAnalysisTools.DoubleThreshold(nonMaxima, 0.15, 0.15);
            //var thin = ImageAnalysisTools.Thinning(doubleThreshold, gradientDirection, 3);
            //var filterpoi = ImageAnalysisTools.filterPointsOfInterest(thin, matrix, 7);
            //var removeClose = ImageAnalysisTools.removeClosePoi(filterpoi, 3);
            ////var hysterisis = ImageAnalysisTools.HysterisisThresholding(doubleThreshold, gradientDirection, 3);
            //magnitude = removeClose; 
            //direction = gradientDirection;

            result = Tuple.Create(result1, result2);
            return result;
        }

        // cut off the overlapped lines in the same direction
        public static List<PointOfInterest> PruneAdjacentTracks(List<PointOfInterest> poiList, int rows, int cols)
        {
            var M = PointOfInterest.TransferPOIsToMatrix(poiList, rows, cols);
            for (int r = 1; r < rows - 1; r++)
            {
                for (int c = 1; c < cols - 1; c++)
                {
                    if (M[r, c] == null) continue;
                    if (M[r, c].OrientationCategory == 0)  // horizontal line
                    {
                        if ((M[r - 1, c] != null) && (M[r - 1, c].OrientationCategory == 0))
                        {
                            if (M[r - 1, c].RidgeMagnitude < M[r, c].RidgeMagnitude) M[r - 1, c] = null;
                        }
                        if ((M[r + 1, c] != null) && (M[r + 1, c].OrientationCategory == 0))
                        {
                            if (M[r + 1, c].RidgeMagnitude < M[r, c].RidgeMagnitude) M[r + 1, c] = null;
                        }
                    }
                    else if (M[r, c].OrientationCategory == 4) // vertical line
                    {
                        if ((M[r, c - 1] != null) && (M[r, c - 1].OrientationCategory == 4))
                        {
                            if (M[r, c - 1].RidgeMagnitude < M[r, c].RidgeMagnitude) M[r, c - 1] = null;
                        }
                        if ((M[r, c + 1] != null) && (M[r, c + 1].OrientationCategory == 4))
                        {
                            if (M[r, c + 1].RidgeMagnitude < M[r, c].RidgeMagnitude) M[r, c + 1] = null;
                        }
                    } // if (OrientationCategory)
                    else if (M[r, c].OrientationCategory == 2)  // positive diagonal line
                    {
                        // Check whether it is connected to other poi with orientation 2. 
                        if ((M[r - 1, c + 1] != null) && (M[r - 1, c + 1].OrientationCategory == 2))
                        {
                            /// above and below
                            if ((M[r - 1, c] != null) && (M[r - 1, c].OrientationCategory == 2))
                            {
                                if (M[r - 1, c].RidgeMagnitude < M[r, c].RidgeMagnitude)
                                {
                                    M[r - 1, c] = null;
                                }
                            }
                            if ((M[r + 1, c] != null) && (M[r + 1, c].OrientationCategory == 2))
                            {
                                if (M[r + 1, c].RidgeMagnitude < M[r, c].RidgeMagnitude)
                                {
                                    M[r + 1, c] = null;
                                }
                            }
                            /// left and right
                            if ((M[r, c - 1] != null) && (M[r, c - 1].OrientationCategory == 2)) // 
                            {
                                if (M[r, c - 1].RidgeMagnitude < M[r, c].RidgeMagnitude)
                                {
                                    M[r, c - 1] = null;
                                }
                            }
                            if ((M[r, c + 1] != null) && (M[r, c + 1].OrientationCategory == 2)) //
                            {
                                if (M[r, c + 1].RidgeMagnitude < M[r, c].RidgeMagnitude)
                                {
                                    M[r, c + 1] = null;
                                }
                            }
                        }
                    }
                    else if (M[r, c].OrientationCategory == 6)  // negative diagonal line
                    {
                        if ((M[r + 1, c + 1] != null) && (M[r + 1, c + 1].OrientationCategory == 6))
                        {
                            /// above and below
                            if ((M[r - 1, c] != null) && (M[r - 1, c].OrientationCategory == 6))
                            {
                                if (M[r - 1, c].RidgeMagnitude < M[r, c].RidgeMagnitude)
                                {
                                    M[r - 1, c] = null;
                                }
                            }
                            if ((M[r + 1, c] != null) && (M[r + 1, c].OrientationCategory == 6))
                            {
                                if (M[r + 1, c].RidgeMagnitude < M[r, c].RidgeMagnitude)
                                {
                                    M[r + 1, c] = null;
                                }
                            }
                            /// left and right
                            if ((M[r, c - 1] != null) && (M[r, c - 1].OrientationCategory == 6)) // 
                            {
                                if (M[r, c - 1].RidgeMagnitude < M[r, c].RidgeMagnitude)
                                {
                                    M[r, c - 1] = null;
                                }
                            }
                            if ((M[r, c + 1] != null) && (M[r, c + 1].OrientationCategory == 6)) //
                            {
                                if (M[r, c + 1].RidgeMagnitude < M[r, c].RidgeMagnitude)
                                {
                                    M[r, c + 1] = null;
                                }
                            }
                        }
                    } // end if (M[r, c].OrientationCategory == 0) 
                } // c
            } // for r loop
            return PointOfInterest.TransferPOIMatrix2List(M);
        } // PruneAdjacentTracks()

        //The difference between this function and PruneAdjacentTracks is tha this function trys to cut off the overlapped lines in different direction. 
        public static List<PointOfInterest> IntraPruneAdjacentTracks(List<PointOfInterest> poiList, int rows, int cols)
        {
            var M = PointOfInterest.TransferPOIsToMatrix(poiList, rows, cols);
            for (int r = 1; r < rows - 1; r++)
            {
                for (int c = 1; c < cols - 1; c++)
                {
                    if (M[r, c] == null) continue;
                    if (M[r, c].OrientationCategory == 0)  // horizontal line
                    {
                        if ((M[r - 1, c] != null) && (M[r - 1, c].OrientationCategory == 2
                            || M[r - 1, c].OrientationCategory == 6))
                        {
                            if (M[r - 1, c].RidgeMagnitude < M[r, c].RidgeMagnitude) M[r - 1, c] = null;
                        }
                        if ((M[r + 1, c] != null) && (M[r + 1, c].OrientationCategory == 2
                            || M[r + 1, c].OrientationCategory == 6))
                        {
                            if (M[r + 1, c].RidgeMagnitude < M[r, c].RidgeMagnitude) M[r + 1, c] = null;
                        }
                    }
                    else if (M[r, c].OrientationCategory == 4) // vertical line
                    {
                        if ((M[r, c - 1] != null) && (M[r, c - 1].OrientationCategory == 2
                            || M[r, c - 1].OrientationCategory == 6))
                        {
                            if (M[r, c - 1].RidgeMagnitude < M[r, c].RidgeMagnitude) M[r, c - 1] = null;
                        }
                        if ((M[r, c + 1] != null) && (M[r, c + 1].OrientationCategory == 2
                            || M[r, c + 1].OrientationCategory == 6))
                        {
                            if (M[r, c + 1].RidgeMagnitude < M[r, c].RidgeMagnitude) M[r, c + 1] = null;
                        }
                    } // end if (M[r, c].OrientationCategory == 0) 
                } // c
            } // for r loop
            return PointOfInterest.TransferPOIMatrix2List(M);
        } // PruneAdjacentTracks()
        
        /// <summary>
        /// This function aims to remove isolated points of interest for filtering out the poi. 
        /// The principle is that there are less than the threshold of the count of poi detected in a neighbourhood (13 * 13). 
        /// </summary>
        /// <param name="poiList"></param>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <param name="sizeOfNeighbourhood"></param>
        /// <param name="thresholdForLeastPoint"></param>
        /// <returns></returns>
        public static List<PointOfInterest> RemoveIsolatedPoi(List<PointOfInterest> poiList, int rows, int cols, int sizeOfNeighbourhood, int thresholdForLeastPoint)
        {
            var M = PointOfInterest.TransferPOIsToMatrix(poiList, rows, cols);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    // Check whether it can fit in one neighbourhood, if not, just ignore it. Here didn't think about the part 
                    // where the boundary of index is out of range, so the last row (corresponding to the bottom line in the spectrogram)
                    // didn't do the filtering. 
                    if (r + sizeOfNeighbourhood - 1 < rows && c + sizeOfNeighbourhood - 1 < cols)
                    {
                        var numberOfpoi = 0;
                        // search in a neighbourhood
                        for (int i = 0; i < sizeOfNeighbourhood; i++)
                        {
                            for (int j = 0; j < sizeOfNeighbourhood; j++)
                            {
                                if (M[r + i, c + j] != null)
                                {
                                    numberOfpoi++;
                                }
                            }
                        }
                        if (numberOfpoi < thresholdForLeastPoint)
                        {
                            for (int i = 0; i < sizeOfNeighbourhood; i++)
                            {
                                for (int j = 0; j < sizeOfNeighbourhood; j++)
                                {
                                    if (M[r + i, c + j] != null)
                                    {
                                        M[r + i, c + j] = null;
                                    }
                                }
                            }
                        }
                        c += sizeOfNeighbourhood - 1;
                    }                    
                } 
                r += sizeOfNeighbourhood - 1;              
            }
            return PointOfInterest.TransferPOIMatrix2List(M);
        }

        /// Change poi spectrogram into black and white image and just show the poi on the spectrogram. 
        public static double[,] ShowPOIOnSpectrogram(SpectralSonogram spectrogram, List<PointOfInterest> poiList, int rows, int cols)
        {          
            foreach (var poi in poiList)
            {
                var xCoordinate = poi.Point.Y;
                var yCoordinate = poi.Point.X;
                spectrogram.Data[yCoordinate, cols - xCoordinate] = 20.0;
            }
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (spectrogram.Data[r, c] == 20.0)
                    {
                        spectrogram.Data[r, c] = 1.0;
                    }
                    else
                    {
                        spectrogram.Data[r, c] = 0.0;
                    }
                }
            }
            return spectrogram.Data;
        }

        #endregion
    }
}
