namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AudioAnalysisTools;
    using System.Drawing;

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

        /// <summary>
        /// Substract matrix from the origional matrix by providing the top-left and bottom right index of the sub-matrix. 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="row1"></param>
        /// <param name="col1"></param>
        /// <param name="row2"></param>
        /// <param name="col2"></param>
        /// <returns></returns>
        public static PointOfInterest[,] Submatrix(PointOfInterest[,] matrix, int row1, int col1, int row2, int col2)
        {
            int subRowCount = row2 - row1;
            int subColCount = col2 - col1;

            var subMatrix = new PointOfInterest[subRowCount, subColCount];
            for (int row = 0; row < subRowCount; row++)
            {
                for (int col = 0; col < subColCount; col++)
                {
                    subMatrix[row, col] = new PointOfInterest(new Point(row1 + row, col1 + col));
                    if (matrix[row1 + row, col1 + col] != null)
                    {
                        subMatrix[row, col].OrientationCategory = matrix[row1 + row, col1 + col].OrientationCategory;
                    }
                    else
                    {
                        subMatrix[row, col].OrientationCategory = 20;
                    }

                }
            }
            return subMatrix;
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

        /// <summary>
        /// To calculate how many edges per timeUnit(such as per second)
        /// </summary>
        /// <param name="poiList"></param>
        /// <param name="rowsCount"></param>
        /// <param name="colsCount"></param>
        /// <param name="timeUnit"></param>
        /// <param name="secondScale"></param>
        /// <returns>
        /// returns an average value for a recording
        /// </returns>
        public static int EdgeStatistics(List<PointOfInterest> poiList, int rowsCount, int colsCount, double timeUnit, double secondScale)
        {
            var SecondToMillionSecondUnit = 1000;
            var numberOfframePerTimeunit = (int)(timeUnit * (SecondToMillionSecondUnit / (secondScale * SecondToMillionSecondUnit)));
            var UnitCount = (int)(colsCount / numberOfframePerTimeunit);
            var countOfpoi = poiList.Count();
            var avgEdgePerTimeunit = (int)(countOfpoi / UnitCount);
            //var Matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);
            //var result = new int[CeilCount];
            //for (int i = 0; i < CeilCount; i++)
            //{
            //    for (int col = i * numberOfframePerTimeunit; col < (i + 1) * numberOfframePerTimeunit; col++)
            //    {
            //         for (int row = 0; row < rowsCount; row++)
            //         {
            //             if (StatisticalAnalysis.checkBoundary(row, col, rowsCount, colsCount))
            //             {
            //                 if (Matrix[row, col] != null)
            //                 {
            //                     result[i]++;
            //                 }
            //             }
            //         }
            //    }
            //}
            return avgEdgePerTimeunit;
        }

        /// <summary>
        /// This mask is unuseful at this moment. Maybe use it later
        /// </summary>
        /// <param name="sizeOfNeighbourhood"></param>
        /// <returns></returns>
        public static int[,] DiagonalMask(int sizeOfNeighbourhood)
        {
            var result = new int[sizeOfNeighbourhood, sizeOfNeighbourhood];

            // above part
            for (int row = 0; row < sizeOfNeighbourhood / 2; row++)
            {
                for (int col = 0; col < sizeOfNeighbourhood / 2 - row; col++)
                {
                    result[row, col] = 0;
                }
                for (int colOffset = -row; colOffset <= row; colOffset++)
                {
                    result[row, sizeOfNeighbourhood / 2 + colOffset] = 1;
                }
            }

            // for middle part
            for (int col = 0; col < sizeOfNeighbourhood; col++)
            {
                result[sizeOfNeighbourhood / 2, col] = 1;
            }

            // for below part
            for (int row = sizeOfNeighbourhood / 2 + 1; row < sizeOfNeighbourhood; row++)
            {
                for (int col = 0; col < sizeOfNeighbourhood - row; col++)
                {
                    result[row, col] = 0;
                }
                for (int colOffset = -(sizeOfNeighbourhood - row - 1); colOffset <= sizeOfNeighbourhood - row - 1; colOffset++)
                {
                    result[row, sizeOfNeighbourhood / 2 + colOffset] = 1;
                }
            }
            return result;
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
