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

        /// <summary>
        /// Returns the submatrix of passed matrix.
        /// The returned submatrix includes the rows and column passed as bounds.
        /// Assume that r1 < r2, c1 < c2. 
        /// Row, column indices start at 0
        /// </summary>
        /// <param name="M"></param>
        /// <param name="r1"></param>
        /// <param name="c1"></param>
        /// <param name="r2"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static PointOfInterest[,] SubmatrixFromPointOfInterest(PointOfInterest[,] poiMatrix, int r1, int c1, int r2, int c2)
        {
            int subRowCount = r2 - r1 + 1;
            int subColCount = c2 - c1 + 1;

            PointOfInterest[,] sm = new PointOfInterest[subRowCount, subColCount];

            for (int i = 0; i < subRowCount; i++)
            {
                for (int j = 0; j < subColCount; j++)
                {
                    sm[i, j] = poiMatrix[r1 + i, c1 + j];
                }
            }
            return sm;
        }
       
        /// <summary>
        /// This function tries to transfer a poiList into a matrix. The dimension of matrix is same with (cols * rows).
        /// </summary>
        /// <param name="list"></param>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <returns></returns>
        public static PointOfInterest[,] TransposePOIsToMatrix(List<PointOfInterest> list, int rows, int cols)
        {
            PointOfInterest[,] m = new PointOfInterest[rows, cols];
            for (int colIndex = 0; colIndex < cols; colIndex++)
            {
                for (int rowIndex = 0; rowIndex < rows; rowIndex++)
                {
                    var point = new Point(colIndex, rowIndex);
                    var tempPoi = new PointOfInterest(point);
                    tempPoi.RidgeMagnitude = 0.0;
                    m[rowIndex, colIndex] = tempPoi;
                }
            }
                foreach (PointOfInterest poi in list)
                {
                    // There is a trick. The coordinate of poi is derived by graphic device. The coordinate of poi starts from top left and its X coordinate is equal to the column 
                    // of the matrix (X = colIndex). Another thing is Y starts from the top while the matrix should start from bottom 
                    // to get the real frequency and time location in the spectram. However, to draw ridges on the spectrogram, we 
                    // have to use the graphical coorinates. And especially, rows = 257, the index of the matrix is supposed to 256.
                    m[poi.Point.Y, poi.Point.X] = poi;
                }
            return m;
        }

        /// <summary>
        /// It is a reverse process to TransposePOIsToMatrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static List<PointOfInterest> TransposeMatrixToPOIlist(PointOfInterest[,] matrix)
        {
            var result = new List<PointOfInterest>();
            var rowsMax = matrix.GetLength(0);
            var colsMax = matrix.GetLength(1);
            for (int r = 0; r < rowsMax; r++)
            {
                for (int c = 0; c < colsMax; c++)
                {
                    if (matrix[r, c].Point.X != 0 && matrix[r, c].Point.Y != 0)
                    {
                        result.Add(matrix[r, c]);
                    }
                }
            }
            return result;
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
                    var pointX = matrix[row + row1, col1 + col].Point.X;
                    var pointY = matrix[row + row1, col1 + col].Point.Y;
                    subMatrix[row, col] = new PointOfInterest(new Point(pointX, pointY));
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

        /// <summary>
        /// To check wether it's an integer.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fv"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fv"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="path"></param>
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

        /// <summary>
        /// Transfer millisends to frame index.
        /// </summary>
        /// <param name="milliSeconds"></param>
        /// <returns></returns>
        public static int MilliSecondsToFrameIndex(double milliSeconds)
        {
            // int maxFrequencyBand = 256;
            //double frequencyScale = 43.0;
            double framePerSecond = 86.0;  // ms
            int timeTransfromUnit = 1000; // from ms to s 
            return (int)(milliSeconds / timeTransfromUnit * framePerSecond);
        }

        /// <summary>
        /// Transfer frequency value to frequency band index. 
        /// </summary>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public static int FrequencyToFruencyBandIndex(double frequency)
        {

            double frequencyScale = 43.0;
            return (int)(frequency / frequencyScale);
        }

        /// <summary>
        /// Transfer seconds to million seconds. 
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static double SecondsToMillionSeconds(double seconds)
        {
            var unit = 1000.0;
            return seconds * unit;
        }

        /// <summary>
        /// ridge neighbourhood representation list to array.
        /// </summary>
        /// <param name="ridgeNhList"></param>
        /// <param name="NhCountInRow"></param>
        /// <param name="NhCountInColumn"></param>
        /// <returns></returns>
        public static RidgeDescriptionNeighbourhoodRepresentation[,] NhListToArray(List<RidgeDescriptionNeighbourhoodRepresentation> ridgeNhList, int NhCountInRow, int NhCountInColumn)
        {
            var listCount = ridgeNhList.Count;
            var result = new RidgeDescriptionNeighbourhoodRepresentation[NhCountInRow, NhCountInColumn];
            
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

        /// <summary>
        /// Region presentaion to array
        /// </summary>
        /// <param name="candidatesList"></param>
        /// <param name="rowsCount"></param>
        /// <param name="colsCount"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scoreVectorList"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        //public static RidgeDescriptionNeighbourhoodRepresentation[,] RegionRepresentationToNHArray(RegionRerepresentation region)
        //{           
        //    var rowsCount = region.NhCountInRow;
        //    var colsCount = region.NhCountInCol;
        //    var result = new RidgeDescriptionNeighbourhoodRepresentation[rowsCount, colsCount];
        //    var neighbourhoodList = region.ridgeNeighbourhoods;
        //    var listLength = neighbourhoodList.Count;
        //    for (int i = 0; i < listLength; i++)
        //    {
        //        result[i / colsCount, i % colsCount] = neighbourhoodList[i];
        //    }
        //    return result;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static double ConvertRadiansToDegree(double radians)
        {
            var result = radians / Math.PI * 180;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static double ConvertDegreeToRadians(double degree)
        {
            var result = degree / 180 * Math.PI;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distanceValue"></param>
        /// <returns></returns>
        public static List<double> ConvertDistanceToPercentageSimilarityScore(List<double> distanceValue)
        {
            var max = distanceValue.Max();
            var result = new List<double>();
            foreach (var d in distanceValue)
            {
                var similarityScore = 1 - d / max;
                result.Add(similarityScore);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="similarityScoreList"></param>
        /// <returns></returns>
        public static List<List<Tuple<double, double, double>>> SimilarityScoreListToVector(List<Tuple<double, double, double>> similarityScoreList)
        {
            var result = new List<List<Tuple<double, double, double>>>();
            similarityScoreList.Sort();           
            var scoreCount = similarityScoreList.Count;
            var tempResult = new List<Tuple<double, double, double>>();
            if (similarityScoreList != null)
            {
            tempResult.Add(similarityScoreList[0]);
            }
            for (int index = 1; index < scoreCount; index++)
            {                
                if ((similarityScoreList[index].Item1 == similarityScoreList[index - 1].Item1))
                {
                    tempResult.Add(similarityScoreList[index]);
                    if (index == scoreCount - 1)
                    {
                        result.Add(tempResult);
                    }
                }
                else
                {
                    if (index == scoreCount - 1)
                    {
                        result.Add(tempResult);
                        var tempResult1 = new List<Tuple<double, double, double>>();
                        tempResult1.Add(similarityScoreList[index]);
                        result.Add(tempResult1);
                    }
                    else
                    {
                    result.Add(tempResult);
                    var tempResult1 = new List<Tuple<double, double, double>>();
                    tempResult = tempResult1;
                    tempResult.Add(similarityScoreList[index]);
                    }
                }
            }
            return result;
        }
    }
}
