
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
        public List<PointOfInterest> poiList {get; set;}

        public int RowsCount {get; set;}

        public int ColsCount {get; set; }

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
                    ImageAnalysisTools.Sobel5X5RidgeDetection(subM, out isRidge, out magnitude, out direction);
                    if (magnitude > magnitudeThreshold)
                    {
                        Point point = new Point(c, r);
                        
                        TimeSpan time = TimeSpan.FromSeconds(c * secondsScale);
                        double herz = (freqBinCount - r - 1) * herzScale;
                        var poi = new PointOfInterest(time, herz);
                        poi.Point = point;
                        //poi.RidgeOrientation = direction;
                        // convert the orientation into - pi/2 to pi / 2 from 0 ~ pi
                        poi.RidgeOrientation = StatisticalAnalysis.ConvertOrientationFrom0PiToNegativePi2(direction);
                        poi.OrientationCategory = (int)Math.Round((direction * 8) / Math.PI);
                        //if (poi.OrientationCategory == 1)
                        //{
                        //    Point point1 = new Point(c, r);
                        //    var poi1 = new PointOfInterest(time, herz);
                        //    poi1.Point = point1;
                        //    poi1.RidgeOrientation = poi.RidgeOrientation;
                        //    poi1.OrientationCategory = poi.OrientationCategory;
                        //    poi1.RidgeMagnitude = poi.RidgeMagnitude;
                        //    //poi1.Intensity = matrix[r, c];
                        //    poi1.TimeScale = timeScale;
                        //    poi1.HerzScale = herzScale;
                        //    poiList.Add(poi1);
                        //}                     
                        poi.RidgeMagnitude = magnitude;
                        poi.Intensity = matrix[r, c];
                        poi.TimeScale = timeScale;
                        poi.HerzScale = herzScale;
                        poiList.Add(poi);
                    }
                }
            }
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
