
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
                        var poi = new PointOfInterest(time, herz);
                        poi.Point = point;
                        // RidgeOrientation ranges from 0 to pi, they are 0, pi/4, pi/2, 3pi/4.
                        poi.RidgeOrientation = direction;
                        // convert the orientation into - pi/2 to pi/2 from 0 ~ pi
                        //poi.RidgeOrientation = StatisticalAnalysis.ConvertOrientationFrom0PiToNegativePi2(direction);

                        // OrientationCategory only has four values, they are 0, 1, 2, 3, 4. 
                        poi.OrientationCategory = (int)Math.Round((direction * 4) / Math.PI);

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

                        /// Fill the gap by adding two more neighbourhood points.
                        var neighbourPoint1 = new Point(0, 0);
                        var neighbourPoi1 = new PointOfInterest(neighbourPoint1);
                        var neighbourPoi2 = new PointOfInterest(neighbourPoint1);
                        FillinGap(poi, rows, cols, out neighbourPoi1, out neighbourPoi2);
                        poiList.Add(poi);
                        poiList.Add(neighbourPoi1);
                        poiList.Add(neighbourPoi2);
                    }
                }
            }
        }

        public void FillinGap(PointOfInterest poi, int rowsMax, int colsMax, out PointOfInterest neighbourPoi1, out PointOfInterest neighbourPoi2)
        {
            var col = poi.Point.X;  // c
            var row = poi.Point.Y; // r
            var colsMin = 0;
            var rowsMin = 0;
            var neighbourPoint1 = new Point(0, 0);
            var neighbourPoint2 = new Point(0, 0);
            neighbourPoi1 = new PointOfInterest(neighbourPoint1);
            neighbourPoi2 = new PointOfInterest(neighbourPoint2);
            if (poi.OrientationCategory == 0)
            {
                if (col + 1 < colsMax && col - 1 > colsMin)
                {
                    var Neighpoi1 = new PointOfInterest(neighbourPoint1);
                    var Neighpoi2 = new PointOfInterest(neighbourPoint2);
                    PoiCopy(poi, out neighbourPoi1, col - 1, row);
                    PoiCopy(poi, out neighbourPoi2, col + 1, row);
                }
            }
            if (poi.OrientationCategory == 1)
            {
                if (col + 1 < colsMax && col - 1 > colsMin && row + 1 < rowsMax && row - 1 > rowsMin)
                {

                    var Neighpoi1 = new PointOfInterest(neighbourPoint1);
                    var Neighpoi2 = new PointOfInterest(neighbourPoint2);
                    PoiCopy(poi, out neighbourPoi1, col - 1, row + 1);
                    PoiCopy(poi, out neighbourPoi2, col + 1, row - 1);
                }
            }
            if (poi.OrientationCategory == 2)
            {
                if (row + 1 < rowsMax && row - 1 > rowsMin)
                {
                    var Neighpoi1 = new PointOfInterest(neighbourPoint1);
                    var Neighpoi2 = new PointOfInterest(neighbourPoint2);
                    PoiCopy(poi, out neighbourPoi1, col, row - 1);
                    PoiCopy(poi, out neighbourPoi2, col, row + 1);
                }
            }
            if (poi.OrientationCategory == 3)
            {
                if (col + 1 < colsMax && col - 1 > colsMin && row + 1 < rowsMax && row - 1 > rowsMin)
                {
                    var Neighpoi1 = new PointOfInterest(neighbourPoint1);
                    var Neighpoi2 = new PointOfInterest(neighbourPoint2);
                    PoiCopy(poi, out neighbourPoi1, col - 1, row - 1);
                    PoiCopy(poi, out neighbourPoi2, col + 1, row + 1);
                }
            }
        }

        public void PoiCopy(PointOfInterest point, out PointOfInterest copyPoi, int xCordinate, int yCordinate)
        {
            var newPoint = new Point(0, 0);
            copyPoi = new PointOfInterest(newPoint);
            copyPoi.Point = new Point(xCordinate, yCordinate);
            copyPoi.RidgeOrientation = point.RidgeOrientation;
            copyPoi.OrientationCategory = point.OrientationCategory;
            copyPoi.RidgeMagnitude = point.RidgeMagnitude;
            copyPoi.Intensity = point.Intensity;
            copyPoi.TimeScale = point.TimeScale;
            copyPoi.HerzScale = point.HerzScale;
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
