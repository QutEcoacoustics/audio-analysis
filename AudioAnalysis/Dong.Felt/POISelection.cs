
namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AudioAnalysisTools;
    using TowseyLib;
    using System.Drawing;

    class POISelection
    {
        public enum RidgeOrientationType { NONE, HORIZONTAL, POSITIVE_QUATERPI, VERTICAL, NEGATIVE_QUATERPI }

        public List<PointOfInterest> poiList { get; set; }

        public int RowsCount { get; set; }

        public int ColsCount { get; set; }

        #region Public Methods

        public POISelection()
        {

        }

        public POISelection(List<PointOfInterest> list)
        {
            poiList = list;
        }

        public void SelectRidgesFromMatrix(double[,] matrix, int rows, int cols, int ridgeLength, double magnitudeThreshold, double secondsScale, TimeSpan timeScale, double herzScale, double freqBinCount)
        {
            int halfLength = ridgeLength / 2;
            for (int r = halfLength; r < rows - halfLength; r++)
            {
                for (int c = halfLength; c < cols - halfLength; c++)
                {
                    var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix
                    double magnitude;
                    double direction;
                    bool isRidge = false;
                    // magnitude is dB, direction is double value which is times of pi/4, from the start of 0. Because here we just used four different masks.
                    ImageAnalysisTools.Sobel5X5RidgeDetection8Direction(subM, out isRidge, out magnitude, out direction);
                    if (magnitude > magnitudeThreshold)
                    {
                        Point point = new Point(c, r);
                        TimeSpan time = TimeSpan.FromSeconds(c * secondsScale);
                        double herz = (freqBinCount - r - 1) * herzScale;
                        // time will be assigned to timelocation of the poi, herz will go to frequencyposition of the poi. 
                        var poi = new PointOfInterest(time, herz);
                        poi.Point = point;

                        // RidgeOrientation are 0, pi/4, pi/2, 3pi/4.
                        poi.RidgeOrientation = direction;
                        // convert the orientation into - pi/2 to pi/2 from 0 ~ pi
                        //poi.RidgeOrientation = StatisticalAnalysis.ConvertOrientationFrom0PiToNegativePi2(direction);

                        // OrientationCategory only has four values, they are 0, 1, 2, 3. 
                        poi.OrientationCategory = (int)Math.Round((direction * 8) / Math.PI);

                        /// Here when I added another 4 directional masks, they will become useful. 
                        //if (poi.OrientationCategory == 1 || poi.OrientationCategory == 3)
                        //{
                        //    poi.OrientationCategory = 2;
                        //}
                        //if (poi.OrientationCategory == 5 || poi.OrientationCategory == 7)
                        //{
                        //    poi.OrientationCategory = 6;
                        //}                   
                        poi.RidgeMagnitude = magnitude;
                        poi.Intensity = matrix[r, c];
                        poi.TimeScale = timeScale;
                        poi.HerzScale = herzScale;

                        var neighbourPoint1 = new Point(0, 0);
                        var neighbourPoi1 = new PointOfInterest(neighbourPoint1);
                        var neighbourPoi2 = new PointOfInterest(neighbourPoint1);
                        /// Fill the gap by adding two more neighbourhood points.
                        FillinGaps(poi, poiList, rows, cols, matrix, out neighbourPoi1, out neighbourPoi2, secondsScale, freqBinCount);
                        poiList.Add(poi);
                        poiList.Add(neighbourPoi1);
                        poiList.Add(neighbourPoi2);
                    }
                }
            }
        }

        /// <summary>
        /// Fill the gap between seperated points. 
        /// </summary>
        /// <param name="poi"></param>
        /// <param name="rowsMax"></param>
        /// <param name="colsMax"></param>
        /// <param name="matrix"></param>
        /// <param name="neighbourPoi1"></param>
        /// <param name="neighbourPoi2"></param>
        /// <param name="secondsScale"></param>
        /// <param name="freqBinCount"></param>
        public void FillinGaps(PointOfInterest poi, List<PointOfInterest> pointOfInterestList, int rowsMax, int colsMax, double[,] matrix, out PointOfInterest neighbourPoi1, out PointOfInterest neighbourPoi2, double secondsScale, double freqBinCount)
        {    
            var neighbourPoint1 = new Point(0, 0);          
            neighbourPoi1 = new PointOfInterest(neighbourPoint1);
            neighbourPoi2 = new PointOfInterest(neighbourPoint1);
            var poiListLength = pointOfInterestList.Count;
            if (poiListLength != 0)
            {
                if (poiList[poiListLength - 1].TimeLocation == poi.TimeLocation && poiList[poiListLength - 1].Herz == poi.Herz)
                {
                    // Finish the copy work. 
                    if (poi.RidgeMagnitude > poiList[poiListLength - 1].RidgeMagnitude)
                    {
                        CallPoiCopy(poi, out neighbourPoi1, out neighbourPoi2, colsMax, rowsMax, matrix, secondsScale, freqBinCount);
                    }
                    else
                    {
                        CallPoiCopy(poiList[poiListLength - 1], out neighbourPoi1, out neighbourPoi2, colsMax, rowsMax, matrix, secondsScale, freqBinCount);
                    }
                }
                else
                {
                    CallPoiCopy(poi, out neighbourPoi1, out neighbourPoi2, colsMax, rowsMax, matrix, secondsScale, freqBinCount);
                }
            }
            else
            {
                CallPoiCopy(poi, out neighbourPoi1, out neighbourPoi2, colsMax, rowsMax, matrix, secondsScale, freqBinCount);
            }
        }

        public void CallPoiCopy(PointOfInterest poi, out PointOfInterest neighbourPoi1, out PointOfInterest neighbourPoi2, int colsMax, int rowsMax, double[,] matrix, double secondsScale, double freqBinCount)
        {
            var col = poi.Point.X;  // c
            var row = poi.Point.Y; // r
            var colsMin = 0;
            var rowsMin = 0;
            var neighbourPoint1 = new Point(0, 0);
            neighbourPoi1 = new PointOfInterest(neighbourPoint1);
            neighbourPoi2 = new PointOfInterest(neighbourPoint1);
            if (poi.OrientationCategory == (int)Direction.East)
            {
                if (col + 1 < colsMax && col - 1 > colsMin)
                {                  
                    neighbourPoi1 = PoiCopy(poi, col - 1, row, matrix, secondsScale, freqBinCount);
                    neighbourPoi2 = PoiCopy(poi, col + 1, row, matrix, secondsScale, freqBinCount);

                }
            }
            if (poi.OrientationCategory == (int)Direction.NorthEast)
            {
                if (col + 1 < colsMax && col - 1 > colsMin && row + 1 < rowsMax && row - 1 > rowsMin)
                {                  
                    neighbourPoi1 = PoiCopy(poi, col - 1, row + 1, matrix, secondsScale, freqBinCount);
                    neighbourPoi2 = PoiCopy(poi, col + 1, row - 1, matrix, secondsScale, freqBinCount);

                }
            }
            if (poi.OrientationCategory == (int)Direction.North)
            {
                if (row + 1 < rowsMax && row - 1 > rowsMin)
                {                
                    neighbourPoi1 = PoiCopy(poi, col, row - 1, matrix, secondsScale, freqBinCount);
                    neighbourPoi2 = PoiCopy(poi, col, row + 1, matrix, secondsScale, freqBinCount);
                }
            }
            if (poi.OrientationCategory == (int)Direction.NorthWest)
            {
                if (col + 1 < colsMax && col - 1 > colsMin && row + 1 < rowsMax && row - 1 > rowsMin)
                {
                    neighbourPoi1 = PoiCopy(poi, col - 1, row - 1, matrix, secondsScale, freqBinCount);
                    neighbourPoi2 = PoiCopy(poi, col + 1, row + 1, matrix, secondsScale, freqBinCount);

                }
            }
        }

        // Copy a pointOfInterst to another pointOfInterest. 
        public PointOfInterest PoiCopy(PointOfInterest point, int xCordinate, int yCordinate, double[,] matrix, double secondsScale, double freqBinCount)
        {
            var newPoint = new Point(xCordinate, yCordinate);
            TimeSpan time = TimeSpan.FromSeconds(xCordinate * secondsScale);
            double herz = (freqBinCount - yCordinate - 1) * point.HerzScale;
            var copyPoi = new PointOfInterest(time, herz);
            copyPoi.Point = newPoint;
            copyPoi.RidgeOrientation = point.RidgeOrientation;
            copyPoi.OrientationCategory = point.OrientationCategory;
            copyPoi.RidgeMagnitude = point.RidgeMagnitude;
            copyPoi.Intensity = matrix[yCordinate, xCordinate];
            copyPoi.TimeScale = point.TimeScale;
            copyPoi.HerzScale = point.HerzScale;
            return copyPoi;
        }

        // using the structure tensor to calculate the real values for each poi's magnitude and direction.  
        public static List<PointOfInterest> CalulateRidgeRealValues(List<PointOfInterest> poiList, int rowsMax, int colsMax)
        {
            var poiMatrix = StatisticalAnalysis.TransposePOIsToMatrix(poiList, rowsMax, colsMax);
            for (int r = 0; r < rowsMax -1; r++)
            {
                for (int c = 0; c < colsMax - 1 ; c++)
                {
                    if (poiMatrix[r, c] != null)
                    {
                        var deltaMagnitudeY = poiMatrix[r, c].RidgeMagnitude;
                        var deltaMagnitudeX = poiMatrix[r, c].RidgeMagnitude;                                            
                        var neighbouringHPoint = poiMatrix[r + 1, c];                     
                        var neighbouringVPoint = poiMatrix[r, c + 1];                      
                        if (poiMatrix[r + 1, c] != null)
                        {
                            deltaMagnitudeX = neighbouringHPoint.RidgeMagnitude - poiMatrix[r, c].RidgeMagnitude;
                        } 
                        if (poiMatrix[r, c + 1] != null)
                        {
                            deltaMagnitudeY = neighbouringVPoint.RidgeMagnitude - poiMatrix[r, c].RidgeMagnitude;
                        }
                        
                        poiMatrix[r, c].RidgeMagnitude = Math.Sqrt(Math.Pow(deltaMagnitudeX, 2) + Math.Pow(deltaMagnitudeY, 2));
                        // because the gradient direction is perpendicular with its real direction, here we add another pi/2 to get its real value.
                        poiMatrix[r, c].RidgeOrientation = Math.Atan(deltaMagnitudeY / deltaMagnitudeX) + Math.PI / 2;                                
                    }
                }
            }
            var result = StatisticalAnalysis.TransposeMatrixToPOIlist(poiMatrix);
                /// It takes time to execute this two foreach when there are a bunch of points of interest in the list. 
                //foreach (var p in poiList)
                //{
                //    var deltaMagnitudeY = p.RidgeMagnitude;
                //    var deltaMagnitudeX = p.RidgeMagnitude;
                //    var poiXcordinates = p.Point.X; 
                //    var poiYcordinates = p.Point.Y;
                //    var neighbouringHPoint = new Point(poiXcordinates + 1, poiYcordinates);
                //    var neighbouringVPoint = new Point(poiXcordinates, poiYcordinates + 1);
                //    foreach (var p1 in poiList)
                //    {
                //        if ((p1.Point.X == neighbouringHPoint.X) && (p1.Point.Y == neighbouringHPoint.Y))
                //        {
                //            deltaMagnitudeX = p1.RidgeMagnitude - p.RidgeMagnitude;
                //        }
                //        if ((p1.Point.X == neighbouringVPoint.X) && (p1.Point.Y == neighbouringVPoint.Y))
                //        {
                //            deltaMagnitudeY = p1.RidgeMagnitude - p.RidgeMagnitude;
                //        }
                //        if (deltaMagnitudeY != 0 && deltaMagnitudeX != 0)
                //        {
                //            p.RidgeMagnitude = Math.Sqrt(Math.Pow(deltaMagnitudeX, 2) + Math.Pow(deltaMagnitudeY, 2));
                //            // because the gradient direction is perpendicular with its real direction, here we add another pi/2 to get its real value.
                //            p.RidgeOrientation = Math.Atan(deltaMagnitudeY / deltaMagnitudeX) + Math.PI / 2;
                //        }
                //    }              
                //}
                return result;
        }

        public void SelectPointOfInterestFromAudioFile(string wavFilePath, int ridgeLength, double magnitudeThreshold)
        {
            //var spectrogram = SpectrogramGeneration(wavFilePath);
            var recording = new AudioRecording(wavFilePath);
            var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.5 };
            var spectrogram = new SpectralSonogram(config, recording.GetWavReader());
            double secondsScale = spectrogram.Configuration.GetFrameOffset(recording.SampleRate);
            var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
            double herzScale = spectrogram.FBinWidth;
            double freqBinCount = spectrogram.Configuration.FreqBinCount;
            var matrix = SpectrogramIntensityToArray(spectrogram);
            var rowsCount = matrix.GetLength(0);
            var colsCount = matrix.GetLength(1);

            var pointsOfInterest = new POISelection();
            pointsOfInterest.SelectRidgesFromMatrix(matrix, rowsCount, colsCount, ridgeLength, magnitudeThreshold, secondsScale, timeScale, herzScale, freqBinCount);
            poiList = pointsOfInterest.poiList;
            RowsCount = rowsCount;
            ColsCount = colsCount;
        }

        public static List<PointOfInterest> FilterPointsOfInterest(List<PointOfInterest> poiList, int rowsCount, int colsCount)
        {
            var pruneAdjacentPoi = ImageAnalysisTools.PruneAdjacentTracks(poiList, rowsCount, colsCount);
            var filterNeighbourhoodSize = 7;
            var numberOfEdge = 3;
            var filterPoiList = ImageAnalysisTools.RemoveIsolatedPoi(pruneAdjacentPoi, rowsCount, colsCount, filterNeighbourhoodSize, numberOfEdge);
            return filterPoiList;
        }

        public SpectralSonogram SpectrogramGeneration(string wavFilePath)
        {
            var recording = new AudioRecording(wavFilePath);
            var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.5 };
            var spectrogram = new SpectralSonogram(config, recording.GetWavReader());

            return spectrogram;
        }

        public double[,] SpectrogramIntensityToArray(SpectralSonogram spectrogram)
        {
            var matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            return matrix;
        }

        #endregion

    }
}
