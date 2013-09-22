namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AudioAnalysisTools;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using Representations;

    class StatisticalAnalysis
    {

        // Transpose matrix by transform the top row to the bottom row, the column doesn't change. 
        public static PointOfInterest[,] TransposePOIsToMatrix(List<PointOfInterest> list, int rows, int cols)
        {
            PointOfInterest[,] m = new PointOfInterest[rows, cols];

            foreach (PointOfInterest poi in list)
            {
                m[rows - poi.Point.Y, poi.Point.X] = poi;
            }
            return m;
        }

        /// <summary>
        /// this method can be used for transforming a double 2 Dimension array to a double 1D array  
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[] TwoDMatrixTo1D(double[,] matrix)
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
                        subMatrix[row, col].RidgeMagnitude = matrix[row1 + row, col1 + col].RidgeMagnitude;
                        subMatrix[row, col].RidgeOrientation = matrix[row1 + row, col1 + col].RidgeOrientation;
                    }
                    else
                    {
                        subMatrix[row, col].OrientationCategory = 20;
                    }

                }
            }
            return subMatrix;
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
        public static RidgeDescriptionNeighbourhoodRepresentation[,] SubRegionMatrix(RidgeDescriptionNeighbourhoodRepresentation[,] matrix, int row1, int col1, int row2, int col2)
        {
            var maxRowIndex = matrix.GetLength(0);
            var maxColIndex = matrix.GetLength(1);
            int subRowCount = row2 - row1;
            int subColCount = col2 - col1;

            var subMatrix = new RidgeDescriptionNeighbourhoodRepresentation[subRowCount, subColCount];
            for (int row = 0; row < subRowCount; row++)
            {
                for (int col = 0; col < subColCount; col++)
                {
                    subMatrix[row, col] = new RidgeDescriptionNeighbourhoodRepresentation();

                    if (checkBoundary(row1 + row, col1 + col, maxRowIndex, maxColIndex))
                    {
                        if (matrix[row1 + row, col1 + col] != null)
                        {
                            subMatrix[row, col] = matrix[row1 + row, col1 + col];
                        }
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

        public static int NumberOfpoiInSlice(List<RidgeNeighbourhoodFeatureVector> fv)
        {
            int result = 0;
            foreach (var f in fv)
            {
                if (f != null)
                {
                    result++;
                }
            }
            return result;
        }

        public static int NumberOfpoiInSlice(RidgeNeighbourhoodFeatureVector fv)
        {
            int result = 0;
            var horizontalIndex = fv.HorizontalVector.GetLength(0);
            var DiagonalIndex = fv.PositiveDiagonalVector.GetLength(0);

            for (int i = 0; i < horizontalIndex; i++)
            {
                if (fv.HorizontalVector[i] != 0)
                {
                    result++;
                }
                if (fv.VerticalVector[i] != 0)
                {
                    result++;
                }
            }

            for (int j = 0; j < DiagonalIndex; j++)
            {
                if (fv.PositiveDiagonalVector[j] != 0)
                {
                    result++;
                }
                if (fv.NegativeDiagonalVector[j] != 0)
                {
                    result++;
                }
            }
            return result;
        }

        public static void WriteCSV<T>(IEnumerable<T> items, string path)
        {
            Type itemType = typeof(T);
            var props = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(p => p.Name);
            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine(string.Join(", ", props.Select(p => p.Name)));
                foreach (var item in items)
                {
                    writer.WriteLine(string.Join(", ", props.Select(p => p.GetValue(item, null))));
                }
            }
        }

        public static int MilliSecondsToFrameIndex(double milliSeconds)
        {
            // int maxFrequencyBand = 256;
            //double frequencyScale = 43.0;
            double framePerSecond = 86.0;  // ms
            int timeTransfromUnit = 1000; // from ms to s 
            return (int)(milliSeconds / timeTransfromUnit * framePerSecond);
        }

        public static int FrequencyToFruencyBandIndex(double frequency)
        {

            double frequencyScale = 43.0;
            return (int)(frequency / frequencyScale);
        }

        public static double SecondsToMillionSeconds(double seconds)
        {
            var unit = 1000.0;
            return seconds * unit;
        }

        public static RidgeDescriptionNeighbourhoodRepresentation[,] RidgeNhListToArray(List<RidgeDescriptionNeighbourhoodRepresentation> ridgeNhList, int NhCountInRow, int NhCountInColumn)
        {
            var result = new RidgeDescriptionNeighbourhoodRepresentation[NhCountInRow, NhCountInColumn];
            var listCount = ridgeNhList.Count;
            for (int i = 0; i < listCount; i++)
            {
                result[i / NhCountInColumn, i % NhCountInColumn] = ridgeNhList[i];
            }
            return result;
        }

        /// <summary>
        /// This method depends on  dominant poi count and dominant magnitude sum, max magnitude in the nh. 
        /// </summary>
        /// <param name="nh"></param>
        /// <param name="nhlength"></param>
        /// <returns></returns>
        public static int NormaliseNeighbourhoodScore(PointOfInterest[,] nh, int nhlength)
        {
            var nhSize = nhlength * nhlength;
            var point = new Point(0, 0);
            var ridgeNeighbourhoodFeatureVector = RectangularRepresentation.SliceRidgeRepresentation(nh, point.X, point.Y);
            var ridgeDominantOrientationRepresentation = RectangularRepresentation.SliceMainSlopeRepresentation(ridgeNeighbourhoodFeatureVector);
            var dominantOrientationType = ridgeDominantOrientationRepresentation.Item1;
            var dominantPOICount = ridgeDominantOrientationRepresentation.Item2;
            var dominantMagnitude = new double[dominantPOICount];
            var i = 0;
            var dominantMagnitudeSum = 0.0;
            for (int rowIndex = 0; rowIndex < nh.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < nh.GetLength(1); colIndex++)
                {
                    if (nh[rowIndex, colIndex].OrientationCategory == dominantOrientationType)
                    {
                        dominantMagnitude[i] = nh[rowIndex, colIndex].RidgeMagnitude;
                        dominantMagnitudeSum += nh[rowIndex, colIndex].RidgeMagnitude;
                        i++;
                    }
                }
            }
            var maxMagnitude = 0.0;
            double magnitudeRelativeFraction = 0.0;
            if (dominantPOICount != 0)
            {
                maxMagnitude = dominantMagnitude.Max();
                magnitudeRelativeFraction = dominantMagnitudeSum / (dominantPOICount * maxMagnitude);
            }            
            var dominantPoiFraction = dominantPOICount / (double)nhSize;
            var fraction = magnitudeRelativeFraction * dominantPoiFraction;
            var normaliseScore = (int)(nhSize * fraction);

            return normaliseScore;
        }

        public static RegionRerepresentation[,] RegionRepresentationListToArray(List<RegionRerepresentation> candidatesList, int rowsCount, int colsCount)
        {
            var result = new RegionRerepresentation[rowsCount, colsCount];
            var listCount = candidatesList.Count;
            for (int i = 0; i < listCount; i++)
            {
                result[i / colsCount, i % colsCount] = candidatesList[i];              
            }
            return result;
        }

        public static double ScoreVectorStatisticalAnalysis(List<List<RegionRerepresentation>> scoreVectorList)
        {
            var frequencyBandCount = scoreVectorList.Count;
            var frameCount = 0;
            if (scoreVectorList != null)
            {
                frameCount = scoreVectorList[0].Count;
            }
            
            for (int rowIndex = 0; rowIndex < frequencyBandCount; rowIndex++)
            {
                for (int colIndex = 0; colIndex < frameCount; colIndex++)
                {
                    //var scoreSum += scoreVectorList[rowIndex].ElementAt(colIndex).score;

                }
            }
                return 0.0;
        }

        public static RidgeDescriptionNeighbourhoodRepresentation[,] RegionRepresentationToNHArray(RegionRerepresentation region)
        {           
            var rowsCount = region.NhCountInRow;
            var colsCount = region.NhCountInCol;
            var result = new RidgeDescriptionNeighbourhoodRepresentation[rowsCount, colsCount];
            var neighbourhoodList = region.ridgeNeighbourhoods;
            var listLength = neighbourhoodList.Count;
            for (int i = 0; i < listLength; i++)
            {
                result[i / colsCount, i % colsCount] = neighbourhoodList[i];
            }
            return result;
        }

        public static double ConvertOrientationFrom0PiToNegativePi2(double radians)
        {
            var result = 0.0;
            if (radians > Math.PI / 2 && radians <= Math.PI)
            {
                result = radians - Math.PI;
            }
            else
            {
                result = radians;
            }
            return result;
        }

        public static double ConvertRadiusToDegree(double radians)
        {
            var result = radians / Math.PI * 180;
            return result;
        }

        public static double ConvertDegreeToRadians(double degree)
        {
            var result = degree / 180 * Math.PI;
            return result;
        }
    }
}
