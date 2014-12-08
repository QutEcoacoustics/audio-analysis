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
    using System.Globalization;
    using AForge.Math;
    using AudioAnalysisTools.StandardSpectrograms;
    using TowseyLibrary;

    class StatisticalAnalysis
    {
        public static void MaxIndex(double[] data, out int maxIndex)
        {
            var count = data.Count();
            var max = 0.0;
            maxIndex = 0;
            for (var i = 0; i < count; i++)
            {
                if (data[i] > max)
                {
                    max = data[i];
                    maxIndex = i;
                }
            }
        }

        public static double[,] MatrixRotate90Clockwise(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var ret = new double[cols, rows];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    ret[c, r] = m[rows - r - 1, c];
            return ret;
        }

        public static double[,] ZeroPaddingMatrix(double[,] m, int colsNumber)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var zero = new double[rows, cols+colsNumber];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    zero[r, c] = m[r, c];

            for (int r = 0; r < rows; r++)
            {
                for (int c = cols - 1; c < cols + colsNumber; c++)
                {
                    zero[r, c] = 0.0;
                }
            }
            return zero;
        }

        /// <summary>
        /// Returns the submatrix of passed matrix.
        /// Row, column indices start at 0
        /// </summary>
        /// <param name="M"></param>
        /// <param name="r1"></param>
        /// <param name="c1"></param>
        /// <param name="r2"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static double[,] SubEvenLengthmatrix(double[,] M, int r1, int c1, int matrixLength)
        {
            var matrixRowCount = M.GetLength(0);
            var matrixColCount = M.GetLength(1);
            double[,] sm = new double[matrixLength, matrixLength];
            var leftRowOffset = matrixLength / 2 - 1;
            var rightRowOffset = matrixLength / 2;

            var topColOffset = matrixLength / 2 - 1;
            var bottomColOffset = matrixLength / 2;

            var startRowIndex = r1 - leftRowOffset;
            var startLeftColIndex = c1 - topColOffset;

            for (int i = r1 - leftRowOffset; i < r1 + rightRowOffset; i++)
            {
                for (int j = c1 - topColOffset; j < c1 + bottomColOffset; j++)
                {
                    // Four center point 
                    // Top left center point
                    if (checkBoundary(i, j, matrixRowCount, matrixColCount, 0, 0))
                    {
                        sm[i - startRowIndex, j - startLeftColIndex] = M[i, j];
                    }
                }
            }
            return sm;
        }

        /// <summary>
        /// Count ridge poi in an event, e.g. query
        /// </summary>
        /// <param name="acousticEvent"></param>
        /// <returns></returns>
        public static int CountPOIInEvent(List<RegionRerepresentation> acousticEvent)
        {
            var result = 0;
            foreach (var e in acousticEvent)
            {
                if (e != null)
                {
                    result += e.POICount;
                }
            }
            return result;
        }

        /// <summary>
        /// Count nh which has ridges in it. 
        /// </summary>
        /// <param name="acousticEvent"></param>
        /// <returns></returns>
        public static int CountNhInEvent(List<RegionRerepresentation> acousticEvent)
        {
            var result = 0;
            foreach (var e in acousticEvent)
            {
                if (e != null)
                {

                    if (e.POICount != 0)
                    {
                        result++;
                    }
                }
            }
            return result;
        }



        public static List<List<RegionRerepresentation>> SplitRegionRepresentationListToBlock(List<RegionRerepresentation> regionRepresentationList)
        {
            var result = new List<List<RegionRerepresentation>>();
            var regionCountInBlock = 0;
            var blockCount = 0;
            if (regionRepresentationList != null)
            {
                regionCountInBlock = regionRepresentationList[0].NhCountInCol * regionRepresentationList[0].NhCountInRow;
                blockCount = regionRepresentationList.Count / regionCountInBlock;
            }
            for (int i = 0; i < regionRepresentationList.Count; i += regionCountInBlock)
            {
                var tempResult = StatisticalAnalysis.SubRegionFromRegionList(regionRepresentationList, i, regionCountInBlock);
                var temp = new List<RegionRerepresentation>();
                foreach (var t in tempResult)
                {
                    temp.Add(t);
                }
                result.Add(temp);
            }
            return result;
        }

        public static double MeasureHLineOfBestfit(PointOfInterest[,] poiMatrix, double lineOfSlope, double intersect)
        {
            var r = 0.0;
            var Sreg = 0.0;
            var Stot = 0.0;
            var poiMatrixLength = poiMatrix.GetLength(0);
            var matrixRadius = poiMatrixLength / 2;
            var improvedRowIndex = 0.0;
            var poiCount = 0;
            for (int rowIndex = 0; rowIndex < poiMatrixLength; rowIndex++)
            {
                for (int colIndex = 0; colIndex < poiMatrixLength; colIndex++)
                {
                    if (poiMatrix[rowIndex, colIndex].RidgeMagnitude != 100.0 &&
                        poiMatrix[rowIndex, colIndex].OrientationCategory == (int)Direction.East)
                    {
                        int tempColIndex = colIndex - matrixRadius;
                        int tempRowIndex = matrixRadius - rowIndex;
                        double verticalDistance = lineOfSlope * tempColIndex + intersect - tempRowIndex;
                        improvedRowIndex += tempRowIndex;
                        Sreg += Math.Pow(verticalDistance, 2.0);
                        poiCount++;
                    }
                }
            }
            var nullLineYIntersect = improvedRowIndex / poiCount;
            for (int rowIndex = 0; rowIndex < poiMatrixLength; rowIndex++)
            {
                for (int colIndex = 0; colIndex < poiMatrixLength; colIndex++)
                {
                    if (poiMatrix[rowIndex, colIndex].RidgeMagnitude != 100.0 &&
                        poiMatrix[rowIndex, colIndex].OrientationCategory == (int)Direction.East)
                    {
                        int tempRowIndex = matrixRadius - rowIndex;
                        double verticalDistance1 = tempRowIndex - nullLineYIntersect;
                        Stot += Math.Pow(verticalDistance1, 2.0);
                    }
                }
            }
            if (Stot != 0)
            {
                r = 1 - Sreg / Stot;
            }
            else
            {
                r = 1;
            }
            return r;
        }

        public static double MeasureVLineOfBestfit(PointOfInterest[,] poiMatrix, double lineOfSlope, double intersect)
        {
            var r = 0.0;
            var Sreg = 0.0;
            var Stot = 0.0;
            var poiMatrixLength = poiMatrix.GetLength(0);
            var matrixRadius = poiMatrixLength / 2;
            var improvedRowIndex = 0.0;
            var poiCount = 0;
            for (int rowIndex = 0; rowIndex < poiMatrixLength; rowIndex++)
            {
                for (int colIndex = 0; colIndex < poiMatrixLength; colIndex++)
                {
                    if (poiMatrix[rowIndex, colIndex].RidgeMagnitude != 100.0 &&
                        poiMatrix[rowIndex, colIndex].OrientationCategory == (int)Direction.North)
                    {
                        int tempColIndex = colIndex - matrixRadius;
                        int tempRowIndex = matrixRadius - rowIndex;
                        double verticalDistance = lineOfSlope * tempColIndex + intersect - tempRowIndex;
                        improvedRowIndex += tempRowIndex;
                        Sreg += Math.Pow(verticalDistance, 2.0);
                        poiCount++;
                    }
                }
            }
            var nullLineYIntersect = improvedRowIndex / poiCount;
            for (int rowIndex = 0; rowIndex < poiMatrixLength; rowIndex++)
            {
                for (int colIndex = 0; colIndex < poiMatrixLength; colIndex++)
                {
                    if (poiMatrix[rowIndex, colIndex].RidgeMagnitude != 100.0 &&
                         poiMatrix[rowIndex, colIndex].OrientationCategory == (int)Direction.North)
                    {
                        int tempRowIndex = matrixRadius - rowIndex;
                        double verticalDistance1 = tempRowIndex - nullLineYIntersect;
                        Stot += Math.Pow(verticalDistance1, 2.0);
                    }
                }
            }
            if (lineOfSlope == Math.PI / 2)
            {
                r = 1;
            }
            else
            {
                if (Stot != 0)
                {
                    r = 1 - Sreg / Stot;
                }
                else
                {
                    r = 1;
                }
            }
            return r;
        }

        public static double MeasureLineOfBestfit(PointOfInterest[,] poiMatrix, double lineOfSlope, double intersect)
        {
            var r = 0.0;
            var Sreg = 0.0;
            var Stot = 0.0;
            var poiMatrixLength = poiMatrix.GetLength(0);
            var matrixRadius = poiMatrixLength / 2;
            var improvedRowIndex = 0.0;
            var poiCount = 0;
            for (int rowIndex = 0; rowIndex < poiMatrixLength; rowIndex++)
            {
                for (int colIndex = 0; colIndex < poiMatrixLength; colIndex++)
                {
                    if (poiMatrix[rowIndex, colIndex].RidgeMagnitude != 100.0)
                    {
                        int tempColIndex = colIndex - matrixRadius;
                        int tempRowIndex = matrixRadius - rowIndex;
                        double verticalDistance = lineOfSlope * tempColIndex + intersect - tempRowIndex;
                        improvedRowIndex += tempRowIndex;
                        Sreg += Math.Pow(verticalDistance, 2.0);
                        poiCount++;
                    }
                }
            }
            var nullLineYIntersect = improvedRowIndex / poiCount;
            for (int rowIndex = 0; rowIndex < poiMatrixLength; rowIndex++)
            {
                for (int colIndex = 0; colIndex < poiMatrixLength; colIndex++)
                {
                    if (poiMatrix[rowIndex, colIndex].RidgeMagnitude != 100.0)
                    {
                        int tempRowIndex = matrixRadius - rowIndex;
                        double verticalDistance1 = tempRowIndex - nullLineYIntersect;
                        Stot += Math.Pow(verticalDistance1, 2.0);
                    }
                }
            }

            if (Stot != 0)
            {
                r = 1 - Sreg / Stot;
            }
            else
            {
                r = 1;
            }
            return r;
        }

        public static RegionRerepresentation[,] RegionRepreListToMatrix(List<RegionRerepresentation> region)
        {
            var rowsCount = region[0].NhCountInRow;
            var colsCount = region[0].NhCountInCol;
            var result = new RegionRerepresentation[rowsCount, colsCount];

            for (int i = 0; i < rowsCount; i++)
            {
                for (int j = 0; j < colsCount; j++)
                {
                    result[i, j] = region[j + i * colsCount];
                }
            }
            return result;
        }

        public static List<Candidates> NormalizeCandidateDistance(List<Candidates> candidates)
        {
            var distanceList = new List<double>();
            foreach (var c in candidates)
            {
                distanceList.Add(c.Score);
            }
            var miniDistance = distanceList.Min();
            var maxDistance = distanceList.Max();
            foreach (var c in candidates)
            {
                c.Score = (c.Score - miniDistance) / (maxDistance - miniDistance);
            }
            return candidates;
        }

        /// <summary>
        /// this is feature set 1, it only involves 2 values, magnitude and orientation.
        /// </summary>
        /// <param name="nhList"></param>
        /// <returns></returns>
        public static List<RidgeDescriptionNeighbourhoodRepresentation> NormalizeProperties(List<RidgeDescriptionNeighbourhoodRepresentation> nhList)
        {
            var result = new List<RidgeDescriptionNeighbourhoodRepresentation>();

            var magnitudeList = new List<double>();
            var orientationList = new List<double>();
            foreach (var nh in nhList)
            {
                // Typically, magnitude should be greater than 0 and less than 20.
                // otherwise, it is assigned to a default value, 100
                // should copy or new a RidgeDescriptionNeighbourhoodRepresentation object, otherwise the next steps will change the original, so I couldn't get 
                // the original value after this function
                if (nh.magnitude != 100)
                {
                    magnitudeList.Add(nh.magnitude);
                    orientationList.Add(nh.orientation);
                }
            }
            var averageMagnitude = magnitudeList.Average();
            var averageOrientation = orientationList.Average();
            var squareDiffMagnitude = 0.0;
            var squareDiffOrientation = 0.0;
            foreach (var nh in nhList)
            {
                squareDiffMagnitude += Math.Pow(nh.magnitude - averageMagnitude, 2);
                squareDiffOrientation += Math.Pow(nh.orientation - averageOrientation, 2);
            }
            var standDevMagnitude = Math.Sqrt(squareDiffMagnitude / nhList.Count);
            var standDevOrientation = Math.Sqrt(squareDiffOrientation / nhList.Count);
            foreach (var nh in nhList)
            {
                if (nh.magnitude != 100)
                {

                    nh.magnitude = (nh.magnitude - averageMagnitude) / standDevMagnitude;
                    //nh.magnitude = (nh.magnitude - minimagnitude) / (maxmagnitude - minimagnitude);
                    //nh.orientation = (nh.orientation - miniOrientation) / (maxOrientation - miniOrientation);   
                    nh.orientation = (nh.orientation - averageOrientation) / standDevOrientation;
                }
                result.Add(nh);
            }
            return result;
        }

        /// <summary>
        /// This method will involve 4 values in a feature vector for a nh. They are magnitude, orientation, dominantOrientation, 
        /// dominantPOICount.
        /// </summary>
        /// <param name="nhList"></param>
        /// <returns></returns>
        public static List<RidgeDescriptionNeighbourhoodRepresentation> NormalizeProperties2(List<RidgeDescriptionNeighbourhoodRepresentation> nhList)
        {
            var result = new List<RidgeDescriptionNeighbourhoodRepresentation>();

            var magnitudeList = new List<double>();
            var orientationList = new List<double>();
            var dominantOrientationList = new List<double>();
            var dominantPoiCountList = new List<double>();
            foreach (var nh in nhList)
            {
                if (nh.magnitude != 100)
                {
                    magnitudeList.Add(nh.magnitude);
                    orientationList.Add(nh.orientation);
                    dominantOrientationList.Add(nh.dominantOrientationType);
                    dominantPoiCountList.Add(nh.dominantPOICount);
                }
            }
            var averageMagnitude = magnitudeList.Average();
            var averageOrientation = orientationList.Average();
            var averageDominantOrientation = dominantOrientationList.Average();
            var averageDominantPoiCount = dominantPoiCountList.Average();
            var squareDiffMagnitude = 0.0;
            var squareDiffOrientation = 0.0;
            var squareDiffDominantOrien = 0.0;
            var squareDiffDominantPoiCount = 0.0;
            foreach (var nh in nhList)
            {
                squareDiffMagnitude += Math.Pow(nh.magnitude - averageMagnitude, 2);
                squareDiffOrientation += Math.Pow(nh.orientation - averageOrientation, 2);
                squareDiffDominantOrien += Math.Pow(nh.dominantOrientationType - averageDominantOrientation, 2);
                squareDiffDominantPoiCount += Math.Pow(nh.dominantPOICount - averageDominantPoiCount, 2);
            }
            var standDevMagnitude = Math.Sqrt(squareDiffMagnitude / nhList.Count);
            var standDevOrientation = Math.Sqrt(squareDiffOrientation / nhList.Count);
            var standDevDominant = Math.Sqrt(squareDiffDominantOrien / nhList.Count);
            var standDevDominantPoiCount = Math.Sqrt(squareDiffDominantPoiCount / nhList.Count);

            foreach (var nh in nhList)
            {
                if (nh.magnitude != 100)
                {
                    nh.magnitude = (nh.magnitude - averageMagnitude) / standDevMagnitude;
                    nh.orientation = (nh.orientation - averageOrientation) / standDevOrientation;
                    nh.dominantOrientationType = (nh.dominantOrientationType - averageDominantOrientation) / standDevDominant;
                    nh.dominantPOICount = (nh.dominantPOICount - averageDominantPoiCount) / standDevDominantPoiCount;
                }
                result.Add(nh);
            }
            return result;
        }

        /// <summary>
        /// This method will invlove six values as a feature vector for a neighbourhood. They are hMagnitude, hOrientation, vMagnitude,
        /// vOrientation, hRmeasure, vRmeasure. 
        /// </summary>
        /// <param name="nhList"></param>
        /// <returns></returns>
        public static List<RidgeDescriptionNeighbourhoodRepresentation> NormalizeProperties3(List<RidgeDescriptionNeighbourhoodRepresentation> nhList)
        {
            var result = new List<RidgeDescriptionNeighbourhoodRepresentation>();

            var hMagnitudeList = new List<double>();
            var hOrientationList = new List<double>();
            var vMagnitudeList = new List<double>();
            var vOrientationList = new List<double>();
            var hRmeasureList = new List<double>();
            var vRmeasureList = new List<double>();
            foreach (var nh in nhList)
            {
                if (nh.HOrientationPOIMagnitude != 100)
                {
                    hMagnitudeList.Add(nh.HOrientationPOIMagnitude);
                    hOrientationList.Add(nh.LinearHOrientation);
                }
                if (nh.HLineOfBestfitMeasure != 100)
                {
                    hRmeasureList.Add(nh.HLineOfBestfitMeasure);
                }
                if (nh.VOrientationPOIMagnitude != 100)
                {
                    vMagnitudeList.Add(nh.VOrientationPOIMagnitude);
                    vOrientationList.Add(nh.LinearVOrientation);

                }
                if (nh.VLineOfBestfitMeasure != 100)
                {
                    vRmeasureList.Add(nh.VLineOfBestfitMeasure);
                }
            }

            var averageHMagnitude = hMagnitudeList.Average();
            var averageHOrientation = hOrientationList.Average();
            var averageVMagnitude = vMagnitudeList.Average();
            var averageVOrientation = vOrientationList.Average();
            var averageHRmeasure = hRmeasureList.Average();
            var averageVRmeasure = vRmeasureList.Average();

            var squareDiffHMagnitude = 0.0;
            var squareDiffHOrientation = 0.0;
            var squareDiffVMagnitude = 0.0;
            var squareDiffVOrientation = 0.0;
            var squareDiffHRmeasure = 0.0;
            var squareDiffVRmeasure = 0.0;
            foreach (var nh in nhList)
            {
                if (nh.HOrientationPOIMagnitude != 100)
                {
                    squareDiffHMagnitude += Math.Pow(nh.HOrientationPOIMagnitude - averageHMagnitude, 2);
                    squareDiffHOrientation += Math.Pow(nh.LinearHOrientation - averageHOrientation, 2);
                }
                if (nh.HLineOfBestfitMeasure != 100)
                {
                    squareDiffHRmeasure += Math.Pow(nh.HLineOfBestfitMeasure - averageHRmeasure, 2);
                }
                if (nh.VOrientationPOIMagnitude != 100)
                {
                    squareDiffVMagnitude += Math.Pow(nh.VOrientationPOIMagnitude - averageVMagnitude, 2);
                    squareDiffVOrientation += Math.Pow(nh.LinearVOrientation - averageVOrientation, 2);
                }
                if (nh.VLineOfBestfitMeasure != 100)
                {
                    squareDiffVRmeasure += Math.Pow(nh.VLineOfBestfitMeasure - averageVRmeasure, 2);
                }
            }
            var standDevHMagnitude = Math.Sqrt(squareDiffHMagnitude / nhList.Count);
            var standDevHOrientation = Math.Sqrt(squareDiffHOrientation / nhList.Count);
            var standDevVMagnitude = Math.Sqrt(squareDiffVMagnitude / nhList.Count);
            var standDevVOrientation = Math.Sqrt(squareDiffVOrientation / nhList.Count);
            var standDevHRmeasure = Math.Sqrt(squareDiffHRmeasure / nhList.Count);
            var standDevVRmeasure = Math.Sqrt(squareDiffVRmeasure / nhList.Count);

            foreach (var nh in nhList)
            {

                if (nh.POICount == 0)
                {
                    result.Add(nh);
                }
                else
                {
                    if (nh.HOrientationPOIMagnitude != 100)
                    {
                        nh.HOrientationPOIMagnitude = (nh.HOrientationPOIMagnitude - averageHMagnitude) / standDevHMagnitude;
                        nh.LinearHOrientation = (nh.LinearHOrientation - averageHOrientation) / standDevHOrientation;
                        nh.HLineOfBestfitMeasure = (nh.HLineOfBestfitMeasure - averageHRmeasure) / standDevHRmeasure;
                    }
                    if (nh.VOrientationPOIMagnitude != 100)
                    {
                        nh.VOrientationPOIMagnitude = (nh.VOrientationPOIMagnitude - averageVMagnitude) / standDevVMagnitude;
                        nh.LinearVOrientation = (nh.LinearVOrientation - averageVOrientation) / standDevVOrientation;
                        nh.VLineOfBestfitMeasure = (nh.VLineOfBestfitMeasure - averageVRmeasure) / standDevVRmeasure;
                    }
                    result.Add(nh);
                }
            }
            return result;
        }

        public static List<RidgeDescriptionNeighbourhoodRepresentation> Nh4HistogramCoding(List<RidgeDescriptionNeighbourhoodRepresentation> nhList)
        {
            var result = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            foreach (var nh in nhList)
            {               
                if (nh.magnitude != 0.0)
                {
                    var HOrientationPOIHist = nh.HOrientationPOIHistogram;

                    if (HOrientationPOIHist >= 0.0 && HOrientationPOIHist < 0.25)
                    {
                        nh.HOrientationPOIHistogram = 0.0;
                    }
                    else if (HOrientationPOIHist >= 0.25 && HOrientationPOIHist < 0.5)
                    {
                        nh.HOrientationPOIHistogram = 1.0;
                    }
                    else if (HOrientationPOIHist >= 0.5 && HOrientationPOIHist < 0.75)
                    {
                        nh.HOrientationPOIHistogram = 2.0;
                    }
                    else if (HOrientationPOIHist >= 0.75 && HOrientationPOIHist <= 1.0)
                    {
                        nh.HOrientationPOIHistogram = 3.0;
                    }
                    var VOrientationPOIHist = nh.VOrientationPOIHistogram;
                    if (VOrientationPOIHist >= 0.0 && VOrientationPOIHist < 0.25)
                    {
                        nh.VOrientationPOIHistogram = 0.0;
                    }
                    else if (VOrientationPOIHist >= 0.25 && VOrientationPOIHist < 0.5)
                    {
                        nh.VOrientationPOIHistogram = 1.0;
                    }
                    else if (VOrientationPOIHist >= 0.5 && VOrientationPOIHist < 0.75)
                    {
                        nh.VOrientationPOIHistogram = 2.0;
                    }
                    else if (VOrientationPOIHist >= 0.75 && VOrientationPOIHist <= 1.0)
                    {
                        nh.VOrientationPOIHistogram = 3.0;
                    }

                    var PDOrientationPOIHist = nh.PDOrientationPOIHistogram;
                    if (PDOrientationPOIHist >= 0.0 && PDOrientationPOIHist < 0.25)
                    {
                        nh.PDOrientationPOIHistogram = 0.0;
                    }
                    else if (PDOrientationPOIHist >= 0.25 && PDOrientationPOIHist < 0.5)
                    {
                        nh.PDOrientationPOIHistogram = 1.0;
                    }
                    else if (PDOrientationPOIHist >= 0.5 && PDOrientationPOIHist < 0.75)
                    {
                        nh.PDOrientationPOIHistogram = 2.0;
                    }
                    else if (PDOrientationPOIHist >= 0.75 && PDOrientationPOIHist <= 1.0)
                    {
                        nh.PDOrientationPOIHistogram = 3.0;
                    }

                    var NDOrientationPOIHist = nh.NDOrientationPOIHistogram;
                    if (NDOrientationPOIHist >= 0.0 && NDOrientationPOIHist < 0.25)
                    {
                        nh.NDOrientationPOIHistogram = 0.0;
                    }
                    else if (NDOrientationPOIHist >= 0.25 && NDOrientationPOIHist < 0.5)
                    {
                        nh.NDOrientationPOIHistogram = 1.0;
                    }
                    else if (NDOrientationPOIHist >= 0.5 && NDOrientationPOIHist < 0.75)
                    {
                        nh.NDOrientationPOIHistogram = 2.0;
                    }
                    else if (NDOrientationPOIHist >= 0.75 && NDOrientationPOIHist <= 1.0)
                    {
                        nh.NDOrientationPOIHistogram = 3.0;
                    }

                    var RowEntropy = nh.RowEnergyEntropy;
                    if (RowEntropy >= 0.0 && RowEntropy < 0.25)
                    {
                        nh.RowEnergyEntropy = 0.0;
                    }
                    else if (RowEntropy >= 0.25 && RowEntropy < 0.5)
                    {
                        nh.RowEnergyEntropy = 1.0;
                    }
                    else if (RowEntropy >= 0.5 && RowEntropy < 0.75)
                    {
                        nh.RowEnergyEntropy = 2.0;
                    }
                    else if (RowEntropy >= 0.75 && RowEntropy <= 1.0)
                    {
                        nh.RowEnergyEntropy = 3.0;
                    }

                    var ColEntropy = nh.ColumnEnergyEntropy;
                    if (ColEntropy >= 0.0 && ColEntropy < 0.25)
                    {
                        nh.ColumnEnergyEntropy = 0.0;
                    }
                    else if (ColEntropy >= 0.25 && ColEntropy < 0.5)
                    {
                        nh.ColumnEnergyEntropy = 1.0;
                    }
                    else if (ColEntropy >= 0.5 && ColEntropy < 0.75)
                    {
                        nh.ColumnEnergyEntropy = 2.0;
                    }
                    else if (ColEntropy >= 0.75 && ColEntropy <= 1.0)
                    {
                        nh.ColumnEnergyEntropy = 3.0;
                    }
                        
                }
                result.Add(nh);
            }
            
            return result;
        }

        public static List<RidgeDescriptionNeighbourhoodRepresentation> NormalizeNhPropertiesForHistogram(List<RidgeDescriptionNeighbourhoodRepresentation> nhList)
        {
            var result = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            var magnitudeList = new List<double>();
            foreach (var nh in nhList)
            {
                // Typically, magnitude should be greater than 0 and less than 20.
                // otherwise, it is assigned to a default value, 100
                if (nh.POIMagnitudeSum != 0.0)
                {
                    magnitudeList.Add(nh.POIMagnitudeSum);
                }
            }
            var maxmagnitude = magnitudeList.Max();
            foreach (var nh in nhList)
            {
                if (nh.POIMagnitudeSum != 0)
                {
                    nh.Orientation0POIMagnitude = (nh.Orientation0POIMagnitude) / maxmagnitude ;
                    nh.Orientation1POIMagnitude = (nh.Orientation1POIMagnitude) / maxmagnitude ;
                    nh.Orientation2POIMagnitude = (nh.Orientation2POIMagnitude) / maxmagnitude ;
                    nh.Orientation3POIMagnitude = (nh.Orientation3POIMagnitude) / maxmagnitude ; 
                    nh.Orientation4POIMagnitude = (nh.Orientation4POIMagnitude) / maxmagnitude ;
                    nh.Orientation5POIMagnitude = (nh.Orientation5POIMagnitude) / maxmagnitude ;
                    nh.Orientation6POIMagnitude = (nh.Orientation6POIMagnitude) / maxmagnitude ;
                    nh.Orientation7POIMagnitude = (nh.Orientation7POIMagnitude) / maxmagnitude ; 
                }
                result.Add(nh);
            }
            return result;
        }

        public static List<RidgeDescriptionNeighbourhoodRepresentation> NormalizeNhProperties(List<RidgeDescriptionNeighbourhoodRepresentation> nhList)
        {
            var result = new List<RidgeDescriptionNeighbourhoodRepresentation>();

            var magnitudeList = new List<double>();
            foreach (var nh in nhList)
            {
                // Typically, magnitude should be greater than 0 and less than 20.
                // otherwise, it is assigned to a default value, 100
                if (nh.magnitude != 100)
                {
                    magnitudeList.Add(nh.magnitude);
                }
            }
            var minimagnitude = magnitudeList.Min();
            var maxmagnitude = magnitudeList.Max();
            foreach (var nh in nhList)
            {
                if (nh.magnitude != 100)
                {
                    nh.magnitude = (nh.magnitude - minimagnitude) / (maxmagnitude - minimagnitude);
                }
                result.Add(nh);
            }
            return result;
        }

        public static List<RegionRerepresentation> SubRegionFromRegionList(List<RegionRerepresentation> regionList, int startIndex, int count)
        {
            var result = new List<RegionRerepresentation>();
            var endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
            {
                result.Add(regionList[i]);
            }
            return result;
        }
        
        /// <summary>
        /// Returns the submatrix of passed matrix.
        /// The returned submatrix includes the rows and column passed as bounds.
        /// Assume that RowTop < RowBottom, ColumnLeft < ColumnRight. 
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
            var rowsCount = poiMatrix.GetLength(0);
            var colsCount = poiMatrix.GetLength(1);

            for (int i = 0; i < subRowCount; i++)
            {
                for (int j = 0; j < subColCount; j++)
                {
                    if (checkBoundary(r1 + i, c1 + j, rowsCount, colsCount))
                    {
                        sm[i, j] = poiMatrix[r1 + i, c1 + j];
                    }
                }
            }
            return sm;
        }
        public static bool CheckFullMatrix(PointOfInterest[,] poiMatrix)
        {
            var count = 0;
            var rowsCount = poiMatrix.GetLength(0);
            var colsCount = poiMatrix.GetLength(1);
            for (var i = 0; i < rowsCount; i++)
            {
                for (var j = 0; j < colsCount; j++)
                {
                    if (poiMatrix[i, j] != null)
                    {
                        count++;
                    }
                }
            }
            if (count == rowsCount * colsCount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// This function tries to transfer a poiList into a matrix. The dimension of matrix is same with (cols * rows).
        /// </summary>
        /// <param name="list"></param>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <returns></returns>
        public static PointOfInterest[,] TransposePOIsToMatrix(List<PointOfInterest> list, SpectrogramStandard spectrogram,
            int rows, int cols)
        {
            PointOfInterest[,] m = new PointOfInterest[rows, cols];
            //var fftMatrix = list[0].fftMatrix; 
            //var matrixRowCount = fftMatrix.GetLength(0);
            //var matrixColCount = fftMatrix.GetLength(1);
            //var defaultFFTMatrix = new double[matrixRowCount, matrixColCount];
            var spectrogramMatrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            for (int colIndex = 0; colIndex < cols; colIndex++)
            {
                for (int rowIndex = 0; rowIndex < rows; rowIndex++)
                {
                    var point = new Point(colIndex, rowIndex);
                    var tempPoi = new PointOfInterest(point);
                    tempPoi.RidgeMagnitude = 0.0;
                    // tempPoi.fftMatrix = defaultFFTMatrix;
                    tempPoi.OrientationCategory = 10;
                    tempPoi.Intensity = spectrogramMatrix[rowIndex, colIndex]; 
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
        /// This function tries to transfer a poiList into a matrix. The dimension of matrix is same with (cols * rows).
        /// </summary>
        /// <param name="list"></param>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <returns></returns>
        public static PointOfInterest[,] TransposePOIsToMatrix(List<PointOfInterest> list,
            int rows, int cols)
        {
            PointOfInterest[,] m = new PointOfInterest[rows, cols];
            //var fftMatrix = list[0].fftMatrix; 
            //var matrixRowCount = fftMatrix.GetLength(0);
            //var matrixColCount = fftMatrix.GetLength(1);
            //var defaultFFTMatrix = new double[matrixRowCount, matrixColCount];            
            for (int colIndex = 0; colIndex < cols; colIndex++)
            {
                for (int rowIndex = 0; rowIndex < rows; rowIndex++)
                {
                    var point = new Point(colIndex, rowIndex);
                    var tempPoi = new PointOfInterest(point);
                    tempPoi.RidgeMagnitude = 0.0;
                    // tempPoi.fftMatrix = defaultFFTMatrix;
                    tempPoi.OrientationCategory = 10;                  
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
        /// This version is for structure tensor matrix 
        /// This function tries to transfer a poiList into a matrix. The dimension of matrix is same with (cols * rows).
        /// </summary>
        /// <param name="list"></param>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <returns></returns>
        public static PointOfInterest[,] TransposePOIsToMatrix1(List<PointOfInterest> list, int rows, int cols)
        {
            PointOfInterest[,] m = new PointOfInterest[rows, cols];
            //var fftMatrix = list[0].fftMatrix; 
            //var matrixRowCount = fftMatrix.GetLength(0);
            //var matrixColCount = fftMatrix.GetLength(1);
            //var defaultFFTMatrix = new double[matrixRowCount, matrixColCount]; 
            for (int colIndex = 0; colIndex < cols; colIndex++)
            {
                for (int rowIndex = 0; rowIndex < rows; rowIndex++)
                {
                    var point = new Point(colIndex, rowIndex);
                    var tempPoi = new PointOfInterest(point);
                    tempPoi.RidgeMagnitude = 0.0;
                    // tempPoi.fftMatrix = defaultFFTMatrix;
                    tempPoi.OrientationCategory = 10;
                    m[rowIndex, colIndex] = tempPoi;
                }
            }
            foreach (PointOfInterest poi in list)
            {
                // There is a trick. The coordinate of poi is derived by graphic device. The coordinate of poi starts from top left and its X coordinate is equal to the column 
                // of the matrix (X = colIndex). Another thing is Y starts from the top while the matrix should start from bottom 
                // to get the real frequency and time location in the spectram. However, to draw ridges on the spectrogram, we 
                // have to use the graphical coorinates. And especially, rows = 257, the index of the matrix is supposed to 256.
                // Changed 2014-8-22 for structure tensor calculation
                m[poi.Point.X, poi.Point.Y] = poi;
            }
            return m;
        }

        public static PointOfInterest[,] TransposePOIsToMatrix2(List<PointOfInterest> list, int rows, int cols)
        {
            PointOfInterest[,] m = new PointOfInterest[rows, cols];
            var fftMatrix = list[0].fftMatrix;
            var matrixRowCount = fftMatrix.GetLength(0);
            var matrixColCount = fftMatrix.GetLength(1);
            var defaultFFTMatrix = new double[matrixRowCount, matrixColCount];
            for (int colIndex = 0; colIndex < cols; colIndex++)
            {
                for (int rowIndex = 0; rowIndex < rows; rowIndex++)
                {
                    var point = new Point(colIndex, rowIndex);
                    var tempPoi = new PointOfInterest(point);
                    tempPoi.RidgeMagnitude = 0.0;
                    tempPoi.fftMatrix = defaultFFTMatrix;
                    m[rowIndex, colIndex] = tempPoi;
                }
            }
            foreach (PointOfInterest poi in list)
            {
                m[poi.Point.X, poi.Point.Y] = poi;
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
        /// It is a reverse process to TransposePOIsToMatrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static List<double> TransposeMatrixToDoublelist(double[,] matrix)
        {
            var result = new List<double>();
            var rowsMax = matrix.GetLength(0);
            var colsMax = matrix.GetLength(1);
            for (int r = 0; r < rowsMax; r++)
            {
                for (int c = 0; c < colsMax; c++)
                {
                    if (matrix[r, c] != null)
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
                        subMatrix[row, col].Intensity = matrix[row1 + row, col1 + col].Intensity;
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
        /// Substract double matrix from the origional matrix by providing the top-left and bottom right index of the sub-matrix. 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="row1"></param>
        /// <param name="col1"></param>
        /// <param name="row2"></param>
        /// <param name="col2"></param>
        /// <returns></returns>
        public static double[,] Submatrix(double[,] matrix, int row1, int col1, int row2, int col2)
        {
            int subRowCount = row2 - row1;
            int subColCount = col2 - col1;

            var subMatrix = new double[subRowCount, subColCount];
            for (int row = 0; row < subRowCount; row++)
            {
                for (int col = 0; col < subColCount; col++)
                {
                    subMatrix[row, col] = matrix[row1 + row, col1 + col];
                }
            }
            return subMatrix;
        }

        public static double averageMatrix(double[,] matrix)
        {
            var maxXIndex = matrix.GetLength(0);
            var maxYIndex = matrix.GetLength(1);
            var sum = 0.0;

            for (int i = 0; i < maxXIndex; i++)
            {
                for (int j = 0; j < maxYIndex; j++)
                {
                    sum += matrix[i, j];
                }
            }

            return sum / maxXIndex * maxYIndex;
        }



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

        public static bool checkBoundary(double indexX, double indexY, double maxiXIndex, double maxYIndex, double miniXIndex = 0, double miniYIndex = 0)
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

        public static bool checkNullRegion(PointOfInterest[,] matrix)
        {
            var poiCount = 0;
            for (var r = 0; r < matrix.GetLength(0); r++)
            {
                for (var c = 0; c < matrix.GetLength(1); c++)
                {
                    if (matrix[r, c] != null)
                    {
                        if (matrix[r, c].fftMatrix != null)
                        {
                            poiCount++;
                        }
                    }
                }
            }
            if (poiCount == 0)
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

        public static bool NullPoiMatrix(PointOfInterest[,] poiMatrix)
        {
            var rowsCount = poiMatrix.GetLength(0);
            var colsCount = poiMatrix.GetLength(1);
            var sumCount = rowsCount * colsCount;
            var count = 0;
            for (var r = 0; r < rowsCount; r++)
            {
                for (var c = 0; c < colsCount; c++)
                {
                    if (poiMatrix[r, c].RidgeMagnitude == 0.0)
                    {
                        count++;
                    }
                }
            }
            if (count == sumCount)
            {
                return true;
            }
            else                
            {
                return false;
            }
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
        /// double list to double array.
        /// </summary>
        /// <param name="ridgeNhList"></param>
        /// <param name="NhCountInRow"></param>
        /// <param name="NhCountInColumn"></param>
        /// <returns></returns>
        public static double[,] DoubleListToArray(List<double> list, int maxRowIndex, int maxColIndex)
        {
            var listCount = list.Count;
            var result = new double[maxRowIndex, maxColIndex];

            for (int i = 0; i < listCount; i++)
            {
                result[i / maxColIndex, i % maxColIndex] = list[i];
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

        public static List<Candidates> ConvertCombinedDistanceToSimilarityScore(List<Candidates> candidates, List<RegionRerepresentation> candidatesList,
            double weight1, double weight2)
        {
            var result = new List<Candidates>();
            var distanceList = new List<double>();
            foreach (var c in candidates)
            {
                distanceList.Add(c.Score);
            }
            var max = distanceList.Max();
            for (int i = 0; i < candidates.Count; i++)
            {
                var similarityScore = 1 - candidates[i].Score / max;
                var score = weight1 * similarityScore + weight2 * candidatesList[i].Features.featureBlockMatch;
                score = Convert.ToDouble(similarityScore.ToString("F03", CultureInfo.InvariantCulture));
                var item = new Candidates(score, candidates[i].StartTime, candidates[i].EndTime - candidates[i].StartTime,
                    candidates[i].MaxFrequency, candidates[i].MinFrequency, candidates[i].SourceFilePath);
                result.Add(item);
            }
            return result;
        }

        public static List<Candidates> ConvertDistanceToSimilarityScore(List<Candidates> candidates)
        {
            var result = new List<Candidates>();
            var distanceList = new List<double>();
            foreach (var c in candidates)
            {
                distanceList.Add(c.Score);
            }
            var max = 0.0;
            if (distanceList.Count != 0)
            {
                max = distanceList.Max();
            }
            foreach (var c in candidates)
            {
                var similarityScore = 0.0;
                if (max != 0.0)
                {
                    similarityScore = 1 - c.Score / max;
                }
                var score = Convert.ToDouble(similarityScore.ToString("F03", CultureInfo.InvariantCulture));

                var item = new Candidates(score, c.StartTime, c.EndTime - c.StartTime, c.MaxFrequency, c.MinFrequency, c.SourceFilePath);
                result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// Autiomatic gray coding is an cyclic way to represent a closed sequence. And the radians is a cyclic case.  
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static List<RidgeDescriptionNeighbourhoodRepresentation> ConvertRadiansToRoundedValues(List<RidgeDescriptionNeighbourhoodRepresentation> radians, int bitCount)
        {
            var result = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            var stateCount = Math.Pow(2, bitCount);
            // by default or ideally, max = pi/2, min = -pi/2.
            //var valueRange = radians.Max() - radians.Min();
            var valueRange = Math.PI;
            //var mini = -Math.PI / 2;
            var increasement = valueRange / stateCount;
            // the radians will be round to stateCount values.
            for (int i = 0; i < radians.Count; i++)
            {
                var incresementCount = (int)(radians[i].orientation / increasement);
                if (radians[i].orientation >= incresementCount * increasement && radians[i].orientation < (incresementCount + 1) * increasement)
                {
                    radians[i].orientation = incresementCount;
                    result.Add(radians[i]);
                }
            }
            return result;
        }

        // todo : to finish the cursive part
        void GrayCode(int numBits)
        {

            var initialBits = new char[2];
            if (numBits == 1)
            {
                initialBits[0] = '0';
                initialBits[1] = '1';
            }
            else
            {
                if (numBits > 1)
                {
                    GrayCode(numBits - 1);
                }
                char[] mirroredBits = Reverse(initialBits);
                initialBits.Concat(mirroredBits);
                // for the first n bits, append 0
                char[] prefix1 = new char[1] { '0' };
                char[] prefix2 = new char[1] { '1' };
                for (int i = 0; i < numBits; i++)
                {
                    char[] tempBit = new char[] { initialBits[i] };
                    //initialBits[i] = prefix1.Concat(tempBit);
                }
                // for the last n bits, append 1
                for (int i = numBits; i < numBits * 2; i++)
                {
                    //initialBits[i] = prefix1.Concat(tempBit); ;
                }
            }
        }

        public char[] Reverse(char[] bits)
        {
            var result = new char[bits.Count()];
            for (int i = 0; i < bits.Count(); i++)
            {
                result[i] = bits[bits.Count() - 1 - i];
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
