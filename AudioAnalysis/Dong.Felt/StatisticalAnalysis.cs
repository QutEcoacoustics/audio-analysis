namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    class StatisticalAnalysis
    {
        // 2D matrix to 1D matrix
        /// <summary>
        /// this method can be used for transforming a double 2 Dimension array to a double 1D array  
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[] MatrixTransform(double[,] matrix)
        {
            var row = matrix.GetLength(0);
            var col = matrix.GetLength(1);

            int lengthOfMatrix = row * col; 
            var result = new double[lengthOfMatrix];

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    result[i * row + j] = matrix[i, j]; 
                }
            }

            return result; 
        }

        // check wether it's an integer
        public static bool checkIfInteger(int value)
        {
            var result = false;
            if (value - Math.Floor((double)value) < 0.00000000001)
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// check whether the index of  Matrix is out of boundary.
        /// </summary>
        /// <param name="indexX"> x index needs to be checked.</param> 
        /// <param name="indexY"> y index needs to be checked.</param>
        /// <param name="maxiXIndex"> it is the upper limit for x index.</param>
        /// <param name="maxYIndex"> it is the upper limit for y index.</param>
        /// <param name="miniXIndex"> it is the bottom limit for x index, by default it's 0.</param>
        /// <param name="miniYIndex"> it is the bottom limit for y index, by default it's 0.</param>
        /// <returns>
        /// if it is not out of index range, it will return true, otherwise it will return false. 
        /// </returns> 
        public static bool checkBoundary(int indexX, int indexY, int maxiXIndex, int maxYIndex, int miniXIndex = 0, int miniYIndex = 0)
        {
            if (indexX >= miniXIndex && indexX < maxiXIndex && indexY >= miniYIndex && indexY < maxYIndex)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //public static List<double> DistanceHistogram(List<FeatureVector> distance, int neighbourhoodSize)
        //{
        //    var numberOfHistogramBar = neighbourhoodSize;
        //    var histogram = new int[neighbourhoodSize];

        //    foreach (var d in distance)
        //    {
        //        for (int histogramIndex = 0; histogramIndex < numberOfHistogramBar; histogramIndex++)
        //        {
        //            if ((d >= histogramIndex / neighbourhoodSize) && (d < (histogramIndex + 1) / neighbourhoodSize))
        //            {
        //                histogram[histogramIndex]++;
        //            }
        //        }
        //    }
        //    return histogram; 
        //}



    }
}
