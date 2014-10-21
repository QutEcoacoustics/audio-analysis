/// This class works on analysing ridges 

namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AudioAnalysisTools;
    using TowseyLibrary;
    using System.Drawing;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.WavTools;
    using Dong.Felt.Representations;

    public class POISelection
    {
        public enum RidgeOrientationType { NONE, HORIZONTAL, POSITIVE_QUATERPI, VERTICAL, NEGATIVE_QUATERPI }

        public List<PointOfInterest> poiList { get; set; }

        public List<PointOfInterest> HorPoiList { get; set; }

        public List<PointOfInterest> VerPoiList { get; set; }

        public List<PointOfInterest> PosQuPiPoiList { get; set; }

        public List<PointOfInterest> NegQuPiPoiList { get; set; }

        public int RowsCount { get; set; }

        public int ColsCount { get; set; }

        #region Public Methods


        public POISelection(List<PointOfInterest> list)
        {
            poiList = list;
        }

        public static List<PointOfInterest> RidgeDetection(SpectrogramStandard spectrogram, RidgeDetectionConfiguration ridgeConfiguration)
        {
            // list size based on avg result size
            var instance = new POISelection(new List<PointOfInterest>(9000));

            instance.RidgeDetectionInternal(spectrogram, ridgeConfiguration);

            return instance.poiList;
        }

        public static List<PointOfInterest> PostRidgeDetection4Dir(SpectrogramStandard spectrogram, RidgeDetectionConfiguration ridgeConfig)
        {
            var instance = new POISelection(new List<PointOfInterest>());
            double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            var rows = matrix.GetLength(0)-1;
            var cols = matrix.GetLength(1);
            var ridgeMagnitudeMatrix = new double[rows, cols];
            var byteMatrix = FourDirectionsRidgeDetection(matrix, out ridgeMagnitudeMatrix, ridgeConfig);
            instance.ConvertRidgeIndicatorToPOIList(byteMatrix, ridgeMagnitudeMatrix, spectrogram);
            return instance.poiList;
        }

        public static List<PointOfInterest> PostRidgeDetectionAmpSpec(BaseSonogram sonogram, RidgeDetectionConfiguration ridgeConfig)
        {
            var instance = new POISelection(new List<PointOfInterest>());
            instance.FourDirRidgeDetectionAmpSpec(sonogram, ridgeConfig);
            return instance.poiList;
        }

        /// <summary>
        /// This version of ridge detection use 8 masks to calculate 8 directional ridges.
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="ridgeConfig"></param>
        /// <returns></returns>
        public static List<PointOfInterest> PostRidgeDetection8Dir(SpectrogramStandard spectrogram, RidgeDetectionConfiguration ridgeConfig)
        {
            var instance = new POISelection(new List<PointOfInterest>());
            double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            var rows = matrix.GetLength(0);
            var cols = matrix.GetLength(1);
            var ridgeMagnitudeMatrix = new double[rows, cols];
            var byteMatrix = EightDirectionsRidgeDetection(matrix, out ridgeMagnitudeMatrix, ridgeConfig);
            instance.ConvertRidgeIndicatorToPOIList(byteMatrix, ridgeMagnitudeMatrix, spectrogram);
            return instance.poiList;
        }

        public static List<PointOfInterest> PoiSelection(SpectrogramStandard spectrogram, RidgeDetectionConfiguration ridgeConfig, string featurePropSet)
        {
             var result = new List<PointOfInterest>();
             if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet5 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet9 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet10)                
             {
                 result = PostRidgeDetection4Dir(spectrogram, ridgeConfig);
             }
             if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet6 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet8)
             {
                 result = Post8DirGradient(spectrogram, ridgeConfig);
             }
             return result;
        }

        public static List<PointOfInterest> Post8DirGradient(SpectrogramStandard spectrogram, RidgeDetectionConfiguration ridgeConfig)
        {
            var instance = new POISelection(new List<PointOfInterest>());
            double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            var rows = matrix.GetLength(0);
            var cols = matrix.GetLength(1);
            var ridgeMagnitudeMatrix = new double[rows, cols];
            var byteMatrix = Gradient8DirCalculation(matrix, out ridgeMagnitudeMatrix, ridgeConfig);
            instance.ConvertRidgeIndiToPOIList2(byteMatrix, ridgeMagnitudeMatrix, spectrogram);
            return instance.poiList;
        }

        internal void RidgeDetectionInternal(SpectrogramStandard spectrogram, RidgeDetectionConfiguration ridgeConfiguration)
        {
            double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            int ridgeLength = ridgeConfiguration.RidgeMatrixLength;
            double magnitudeThreshold = ridgeConfiguration.RidgeDetectionmMagnitudeThreshold;
            double secondsScale = spectrogram.Configuration.GetFrameOffset(spectrogram.SampleRate); // 0.0116
            var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
            double herzScale = spectrogram.FBinWidth; //43 hz
            double freqBinCount = spectrogram.Configuration.FreqBinCount; //256
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
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
                    ImageAnalysisTools.Sobel5X5RidgeDetection4Direction(subM, out isRidge, out magnitude, out direction);
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
                        // OrientationCategory only has four values, they are 0, 1, 2, 3. 
                        poi.OrientationCategory = (int)Math.Round((direction * 8) / Math.PI);
                        poi.RidgeMagnitude = magnitude;
                        poi.Intensity = matrix[r, c];
                        poi.TimeScale = timeScale;
                        poi.HerzScale = herzScale;
                        poiList.Add(poi);
                    }
                }
            }  /// filter out some redundant ridges
        }

        public void FourDirRidgeDetectionAmpSpec(BaseSonogram spectrogram, RidgeDetectionConfiguration ridgeConfiguration)
        {
            double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            int ridgeLength = ridgeConfiguration.RidgeMatrixLength;
            double magnitudeThreshold = ridgeConfiguration.RidgeDetectionmMagnitudeThreshold;
            double secondsScale = spectrogram.Configuration.GetFrameOffset(spectrogram.SampleRate); // 0.0116
            var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
            double herzScale = spectrogram.FBinWidth; //43 hz
            double freqBinCount = spectrogram.Configuration.FreqBinCount; //256
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int halfLength = ridgeLength / 2;
            for (int r = halfLength; r < rows - halfLength; r++)
            {
                for (int c = halfLength; c < cols - halfLength; c++)
                {
                    var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix
                    var boundary = StatisticalAnalysis.checkBoundary(r - 3, c - 3, rows - 3, cols - 3);
                    var boundary2 = StatisticalAnalysis.checkBoundary(r + 3, c + 3, rows - 3, cols - 3);
                    var subM2 = new double[7, 7];
                    if (boundary == true && boundary2 == true)
                    {
                        subM2 = MatrixTools.Submatrix(matrix, r - 3, c - 3, r + 3, c + 3);
                    }
                    double magnitude;
                    double direction;
                    bool isRidge = false;
                    // magnitude is dB, direction is double value which is times of pi/4, from the start of 0. Because here we just used four different masks.
                    ImageAnalysisTools.Sobel5X5RidgeDetection4Direction(subM, out isRidge, out magnitude, out direction);
                    if (subM2 != null)
                    {
                        ImageAnalysisTools.RidgeDetectConfirmation(subM2, out isRidge);
                    }
                    if (magnitude > magnitudeThreshold && isRidge == true)
                    {
                        Point point = new Point(c, r);
                        TimeSpan time = TimeSpan.FromSeconds(c * secondsScale);
                        double herz = (freqBinCount - r) * herzScale;
                        // time will be assigned to timelocation of the poi, herz will go to frequencyposition of the poi. 
                        var poi = new PointOfInterest(time, herz);
                        poi.TimeLocation = time;
                        poi.Herz = herz;
                        poi.Point = point;
                        // RidgeOrientation are 0, pi/4, pi/2, 3pi/4.
                        poi.RidgeOrientation = direction;
                        // OrientationCategory only has four values, they are 0, 1, 2, 3. 
                        poi.OrientationCategory = (int)Math.Round((direction * 8) / Math.PI);
                        poi.RidgeMagnitude = magnitude;
                        poi.Intensity = matrix[r, c];
                        poi.TimeScale = timeScale;
                        poi.HerzScale = herzScale;
                        poiList.Add(poi);                        
                    }
                }
            }  /// filter out some redundant ridges   
        }

        public void ConvertRidgeIndicatorToPOIList(byte[,] ridgeIndiMatrix, double[,] RidgeMagnitudematrix, SpectrogramStandard spectrogram)
        {
            double secondsScale = spectrogram.Configuration.GetFrameOffset(spectrogram.SampleRate); // 0.0116
            var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
            double herzScale = spectrogram.FBinWidth; //43 hz
            double freqBinCount = spectrogram.Configuration.FreqBinCount; //256
            int rows = ridgeIndiMatrix.GetLength(0);
            int cols = ridgeIndiMatrix.GetLength(1);
            var spectrogramMatrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data); 
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (ridgeIndiMatrix[r, c] > 0)
                    {
                        Point point = new Point(c, r);
                        TimeSpan time = TimeSpan.FromSeconds(c * secondsScale);
                        double herz = (freqBinCount - r - 1) * herzScale;
                        // time will be assigned to timelocation of the poi, herz will go to frequencyposition of the poi. 
                        var poi = new PointOfInterest(time, herz);
                        poi.Point = point;
                        // OrientationCategory only has 4/8 values, they are 0, 1, 2, 3,4,5,6,7. 
                        poi.OrientationCategory = ridgeIndiMatrix[r, c] - 1;
                        poi.RidgeMagnitude = RidgeMagnitudematrix[r, c];
                        poi.Intensity = spectrogramMatrix[r, c];
                        poi.TimeScale = timeScale;
                        poi.HerzScale = herzScale;
                        poiList.Add(poi);
                    }
                }
            }
            var prunedPoiList = ImageAnalysisTools.PruneAdjacentTracksBasedOn4Direction(poiList, rows, cols);
            //var prunedPoiList1 = ImageAnalysisTools.IntraPruneAdjacentTracks(prunedPoiList, rows, cols);
            //var filteredPoiList = ImageAnalysisTools.RemoveIsolatedPoi(poiList, rows, cols, 7, 3);
            //var filteredPoiList = ImageAnalysisTools.FilterRidges(prunedPoiList1, rows, cols, ridgeConfiguration.FilterRidgeMatrixLength, ridgeConfiguration.MinimumNumberInRidgeInMatrix);
            poiList = prunedPoiList;
        }

        // This version is suitable for HOG features. 
        public void ConvertRidgeIndiToPOIList2(byte[,] ridgeIndiMatrix, double[,] RidgeMagnitudematrix, SpectrogramStandard spectrogram)
        {
            double secondsScale = spectrogram.Configuration.GetFrameOffset(spectrogram.SampleRate); // 0.0116
            var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
            double herzScale = spectrogram.FBinWidth; //43 hz
            double freqBinCount = spectrogram.Configuration.FreqBinCount; //256
            int rows = ridgeIndiMatrix.GetLength(0);
            int cols = ridgeIndiMatrix.GetLength(1);
            var spectrogramMatrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (ridgeIndiMatrix[r, c] > 0)
                    {
                        Point point = new Point(c, r);
                        TimeSpan time = TimeSpan.FromSeconds(c * secondsScale);
                        double herz = (freqBinCount - r - 1) * herzScale;
                        // time will be assigned to timelocation of the poi, herz will go to frequencyposition of the poi. 
                        var poi = new PointOfInterest(time, herz);
                        poi.Point = point;
                        // OrientationCategory only has 4/8 values, they are 0, 1, 2, 3,4,5,6,7. 
                        poi.OrientationCategory = ridgeIndiMatrix[r, c] - 1;
                        poi.RidgeMagnitude = RidgeMagnitudematrix[r, c];
                        poi.Intensity = spectrogramMatrix[r, c];
                        poi.TimeScale = timeScale;
                        poi.HerzScale = herzScale;
                        poiList.Add(poi);
                    }
                }
            }           
            var filteredPoiList = ImageAnalysisTools.RemoveIsolatedPoi(poiList, rows, cols, 7, 3);
            poiList = filteredPoiList;
        }

        /// <summary>
        /// This version of ridge detection involves original ridge detection and removing ridges in shadow. 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="newMatrix"></param>
        /// <param name="ridgeConfiguration"></param>
        /// <returns></returns>
        public static byte[,] FourDirectionsRidgeDetection(double[,] matrix, out double[,] newMatrix, 
        RidgeDetectionConfiguration ridgeConfiguration)
        {         
            int ridgeLength = ridgeConfiguration.RidgeMatrixLength;
            double magnitudeThreshold = ridgeConfiguration.RidgeDetectionmMagnitudeThreshold;            
            int rows = matrix.GetLength(0)-1;
            int cols = matrix.GetLength(1);
            int halfLength = ridgeLength / 2;
            var hits = new byte[rows, cols];
            newMatrix = new double[rows, cols];
            for (int r = halfLength + 1; r < rows - halfLength - 1; r++)
            {
                for (int c = halfLength + 1; c < cols - halfLength - 1; c++)
                {
                    if (hits[r, c] > 0) continue;
                    var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix
                    double magnitude = 0.0;
                    double direction = 0.0;
                    bool isRidge = false;
                    // magnitude is dB, direction is double value which is times of pi/4, from the start of 0.
                    if (ridgeLength == 3)
                    {
                        ImageAnalysisTools.Sobel3X3RidgeDetection4Direction(subM, out isRidge, out magnitude, out direction);
                    }
                    if (ridgeLength == 5)
                    {
                        ImageAnalysisTools.Sobel5X5RidgeDetection4Direction(subM, out isRidge, out magnitude, out direction);
                    }
                    if (ridgeLength == 7)
                    {
                        ImageAnalysisTools.Sobel7X7RidgeDetection4Direction(subM, out isRidge, out magnitude, out direction);
                    }
                    if (magnitude > magnitudeThreshold && isRidge == true)
                    {
                        var subM2 = MatrixTools.Submatrix(matrix, r - halfLength - 1, c - halfLength - 1, r + halfLength + 1, c + halfLength + 1);                      
                        double av, sd;
                        NormalDist.AverageAndSD(subM2, out av, out sd);
                        double localThreshold = sd * 1.3;
                        if ((subM2[halfLength+1, halfLength+1] - av) < localThreshold) continue;                       
                        var orientation = (int)Math.Round((direction * 8) / Math.PI);
                        hits[r, c] = (byte)(orientation+1);
                        newMatrix[r, c] = magnitude;
                        if (orientation == 2)
                        {
                            hits[r - 1, c + 1] = (byte)(orientation + 1);
                            newMatrix[r - 1, c + 1] = magnitude;
                            hits[r + 1, c - 1] = (byte)(orientation + 1);
                            newMatrix[r + 1, c - 1] = magnitude;                           
                        }
                        else if (orientation == 6)
                        {
                            hits[r + 1, c + 1] = (byte)(orientation + 1);
                            newMatrix[r + 1, c + 1] = magnitude;
                            hits[r - 1, c - 1] = (byte)(orientation + 1);
                            newMatrix[r - 1, c - 1] = magnitude;                            
                        }
                        else if (orientation == 4)
                        {
                            hits[r - 1, c] = (byte)(orientation + 1);
                            newMatrix[r - 1, c] = magnitude;
                            hits[r + 1, c] = (byte)(orientation + 1);
                            newMatrix[r + 1, c] = magnitude;
                            hits[r - 2, c] = (byte)(orientation + 1);
                            newMatrix[r - 2, c] = magnitude;
                            hits[r + 2, c] = (byte)(orientation + 1);
                            newMatrix[r + 2, c] = magnitude;
                        }
                        else if (orientation == 0)
                        {
                            hits[r, c - 1] = (byte)(orientation + 1);
                            newMatrix[r, c - 1] = magnitude;
                            hits[r, c + 1] = (byte)(orientation + 1);
                            newMatrix[r, c + 1] = magnitude;
                            hits[r, c - 2] = (byte)(orientation + 1);
                            newMatrix[r, c - 2] = magnitude;
                            hits[r, c + 2] = (byte)(orientation + 1);
                            newMatrix[r, c + 2] = magnitude;
                        }                        
                    }
                }                   
            }  /// filter out some redundant ridges          
            return hits;
        }

        /// <summary>
        /// This version of ridge detection involves original ridge detection and removing ridges in shadow. 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="newMatrix"></param>
        /// <param name="ridgeConfiguration"></param>
        /// <returns></returns>
        public static byte[,] FourDirections3RidgeDetection(double[,] matrix, out double[,] newMatrix,
        RidgeDetectionConfiguration ridgeConfiguration)
        {
            int ridgeLength = ridgeConfiguration.RidgeMatrixLength;
            double magnitudeThreshold = ridgeConfiguration.RidgeDetectionmMagnitudeThreshold;
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int halfLength = ridgeLength / 2;
            var hits = new byte[rows, cols];
            newMatrix = new double[rows, cols];
            for (int r = halfLength + 1; r < rows - halfLength - 1; r++)
            {
                for (int c = halfLength + 1; c < cols - halfLength - 1; c++)
                {
                    if (hits[r, c] > 0) continue;
                    var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix
                    double magnitude;
                    double direction;
                    bool isRidge = false;
                    // magnitude is dB, direction is double value which is times of pi/4, from the start of 0. 
                    ImageAnalysisTools.Sobel5X5RidgeDetection4Direction(subM, out isRidge, out magnitude, out direction);
                    if (magnitude > magnitudeThreshold && isRidge == true)
                    {
                        var subM2 = MatrixTools.Submatrix(matrix, r - halfLength - 1, c - halfLength - 1, r + halfLength + 1, c + halfLength + 1);
                        double av, sd;
                        NormalDist.AverageAndSD(subM2, out av, out sd);
                        double localThreshold = sd * 1.3;
                        if ((subM2[halfLength + 1, halfLength + 1] - av) < localThreshold) continue;
                        var orientation = (int)Math.Round((direction * 8) / Math.PI);
                        hits[r, c] = (byte)(orientation + 1);
                        newMatrix[r, c] = magnitude;
                        if (orientation == 2)
                        {
                            hits[r - 1, c + 1] = (byte)(orientation + 1);
                            newMatrix[r - 1, c + 1] = magnitude;
                            hits[r + 1, c - 1] = (byte)(orientation + 1);
                            newMatrix[r + 1, c - 1] = magnitude;
                            //hits[r - 2, c + 2] = (byte)(direction + 1);
                            //hits[r + 2, c - 2] = (byte)(direction + 1);
                        }
                        else if (orientation == 6)
                        {
                            hits[r + 1, c + 1] = (byte)(orientation + 1);
                            newMatrix[r + 1, c + 1] = magnitude;
                            hits[r - 1, c - 1] = (byte)(orientation + 1);
                            newMatrix[r - 1, c - 1] = magnitude;
                            //hits[r + 2, c + 2] = (byte)(direction + 1);
                            //hits[r - 2, c - 2] = (byte)(direction + 1);
                        }
                        else if (orientation == 4)
                        {
                            hits[r - 1, c] = (byte)(orientation + 1);
                            newMatrix[r - 1, c] = magnitude;
                            hits[r + 1, c] = (byte)(orientation + 1);
                            newMatrix[r + 1, c] = magnitude;
                            hits[r - 2, c] = (byte)(orientation + 1);
                            newMatrix[r - 2, c] = magnitude;
                            hits[r + 2, c] = (byte)(orientation + 1);
                            newMatrix[r + 2, c] = magnitude;
                        }
                        else if (orientation == 0)
                        {
                            hits[r, c - 1] = (byte)(orientation + 1);
                            newMatrix[r, c - 1] = magnitude;
                            hits[r, c + 1] = (byte)(orientation + 1);
                            newMatrix[r, c + 1] = magnitude;

                            hits[r, c - 2] = (byte)(orientation + 1);
                            newMatrix[r, c - 2] = magnitude;
                            hits[r, c + 2] = (byte)(orientation + 1);
                            newMatrix[r, c + 2] = magnitude;
                        }
                    }
                }
            }  /// filter out some redundant ridges          
            return hits;
        }

        /// <summary>
        /// This version adds intensityThreshold
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="newMatrix"></param>
        /// <param name="ridgeConfiguration"></param>
        /// <returns></returns>
        public static byte[,] FourDirectionsRidgeDetection2(double[,] matrix, out double[,] newMatrix,
       RidgeDetectionConfiguration ridgeConfiguration)
        {
            int ridgeLength = ridgeConfiguration.RidgeMatrixLength;
            double magnitudeThreshold = ridgeConfiguration.RidgeDetectionmMagnitudeThreshold;
            var p = 0.002;
            var intensityList = intensityThresholdForSpectrogram(matrix);
            var intensityThreshold = calculateIntensityThreshold(intensityList, p);
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int halfLength = ridgeLength / 2;
            var hits = new byte[rows, cols];
            newMatrix = new double[rows, cols];
            for (int r = halfLength + 1; r < rows - halfLength - 1; r++)
            {
                for (int c = halfLength + 1; c < cols - halfLength - 1; c++)
                {
                    if (hits[r, c] > 0) continue;
                    var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix
                    double magnitude;
                    double direction;
                    bool isRidge = false;
                    // magnitude is dB, direction is double value which is times of pi/4, from the start of 0. 
                    ImageAnalysisTools.Sobel5X5RidgeDetection4Direction(subM, out isRidge, out magnitude, out direction);
                    if (magnitude > magnitudeThreshold && isRidge == true && (subM[halfLength, halfLength] > intensityThreshold))
                    {
                        var subM2 = MatrixTools.Submatrix(matrix, r - halfLength - 1, c - halfLength - 1, r + halfLength + 1, c + halfLength + 1);
                        double av, sd;
                        NormalDist.AverageAndSD(subM2, out av, out sd);
                        double localThreshold = sd * 1.3;
                        if ((subM2[halfLength + 1, halfLength + 1] - av) < localThreshold) continue;
                        var orientation = (int)Math.Round((direction * 8) / Math.PI);
                        hits[r, c] = (byte)(orientation + 1);
                        newMatrix[r, c] = magnitude;
                        if (orientation == 2)
                        {
                            hits[r - 1, c + 1] = (byte)(orientation + 1);
                            newMatrix[r - 1, c + 1] = magnitude;
                            hits[r + 1, c - 1] = (byte)(orientation + 1);
                            newMatrix[r + 1, c - 1] = magnitude;
                            //hits[r - 2, c + 2] = (byte)(direction + 1);
                            //hits[r + 2, c - 2] = (byte)(direction + 1);
                        }
                        else if (orientation == 6)
                        {
                            hits[r + 1, c + 1] = (byte)(orientation + 1);
                            newMatrix[r + 1, c + 1] = magnitude;
                            hits[r - 1, c - 1] = (byte)(orientation + 1);
                            newMatrix[r - 1, c - 1] = magnitude;
                            //hits[r + 2, c + 2] = (byte)(direction + 1);
                            //hits[r - 2, c - 2] = (byte)(direction + 1);
                        }
                        else if (orientation == 4)
                        {
                            hits[r - 1, c] = (byte)(orientation + 1);
                            newMatrix[r - 1, c] = magnitude;
                            hits[r + 1, c] = (byte)(orientation + 1);
                            newMatrix[r + 1, c] = magnitude;
                            hits[r - 2, c] = (byte)(orientation + 1);
                            newMatrix[r - 2, c] = magnitude;
                            hits[r + 2, c] = (byte)(orientation + 1);
                            newMatrix[r + 2, c] = magnitude;
                        }
                        else if (orientation == 0)
                        {
                            hits[r, c - 1] = (byte)(orientation + 1);
                            newMatrix[r, c - 1] = magnitude;
                            hits[r, c + 1] = (byte)(orientation + 1);
                            newMatrix[r, c + 1] = magnitude;

                            hits[r, c - 2] = (byte)(orientation + 1);
                            newMatrix[r, c - 2] = magnitude;
                            hits[r, c + 2] = (byte)(orientation + 1);
                            newMatrix[r, c + 2] = magnitude;
                        }

                    }
                }
            }  /// filter out some redundant ridges          
            return hits;
        }

        public static byte[,] EightDirectionsRidgeDetection(double[,] matrix, out double[,] newMatrix, RidgeDetectionConfiguration ridgeConfiguration)
        {
            int ridgeLength = ridgeConfiguration.RidgeMatrixLength;
            double magnitudeThreshold = ridgeConfiguration.RidgeDetectionmMagnitudeThreshold;
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int halfLength = ridgeLength / 2;
            var hits = new byte[rows, cols];
            newMatrix = new double[rows, cols];

            for (int r = halfLength + 1; r < rows - halfLength - 1; r++)
            {
                for (int c = halfLength + 1; c < cols - halfLength - 1; c++)
                {
                    if (hits[r, c] > 0) continue;
                    var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix
                    double magnitude;
                    double direction;
                    bool isRidge = false;
                    // magnitude is dB, direction is double value which is times of pi/4, from the start of 0. 
                    ImageAnalysisTools.Sobel5X5RidgeDetection8Direction(subM, out isRidge, out magnitude, out direction);
                    if (magnitude > magnitudeThreshold && isRidge == true)
                    {
                        var subM2 = MatrixTools.Submatrix(matrix, r - halfLength - 1, c - halfLength - 1, r + halfLength + 1, c + halfLength + 1);
                        double av, sd;
                        NormalDist.AverageAndSD(subM2, out av, out sd);
                        double localThreshold = sd * 1.3;
                        if ((subM[halfLength, halfLength] - av) < localThreshold) continue;
                        var orientation = (int)Math.Round((direction * 8) / Math.PI);
                        hits[r, c] = (byte)(orientation + 1);
                        newMatrix[r, c] = magnitude;
                        if (orientation == 2 )
                        {
                            hits[r - 1, c + 1] = (byte)(orientation + 1);
                            newMatrix[r - 1, c + 1] = magnitude;
                            hits[r + 1, c - 1] = (byte)(orientation + 1);
                            newMatrix[r + 1, c - 1] = magnitude;
                            //hits[r - 2, c + 2] = (byte)(direction + 1);
                            //hits[r + 2, c - 2] = (byte)(direction + 1);
                        }
                        else if (orientation == 6)
                        {
                            hits[r + 1, c + 1] = (byte)(orientation + 1);
                            newMatrix[r + 1, c + 1] = magnitude;
                            hits[r - 1, c - 1] = (byte)(orientation + 1);
                            newMatrix[r - 1, c - 1] = magnitude;
                            //hits[r + 2, c + 2] = (byte)(direction + 1);
                            //hits[r - 2, c - 2] = (byte)(direction + 1);
                        }
                        else if (orientation == 3 || orientation == 4 || orientation == 5)
                        {
                            hits[r - 1, c] = (byte)(orientation + 1);
                            newMatrix[r - 1, c] = magnitude;
                            hits[r + 1, c] = (byte)(orientation + 1);
                            newMatrix[r + 1, c] = magnitude;
                            hits[r - 2, c] = (byte)(orientation + 1);
                            newMatrix[r - 2, c] = magnitude;
                            hits[r + 2, c] = (byte)(orientation + 1);
                            newMatrix[r + 2, c] = magnitude;
                        }
                        else if (orientation == 0 || orientation == 1 || orientation == 7)
                        {
                            hits[r, c - 1] = (byte)(orientation + 1);
                            newMatrix[r, c - 1] = magnitude;
                            hits[r, c + 1] = (byte)(orientation + 1);
                            newMatrix[r, c + 1] = magnitude;

                            hits[r, c - 2] = (byte)(orientation + 1);
                            newMatrix[r, c - 2] = magnitude;
                            hits[r, c + 2] = (byte)(orientation + 1);
                            newMatrix[r, c + 2] = magnitude;
                        }

                    }
                }
            }  /// filter out some redundant ridges          
            return hits;
        }
        
        public static byte[,] Gradient8DirCalculation(double[,] matrix, out double[,] newMatrix, RidgeDetectionConfiguration ridgeConfiguration)
        {
            int ridgeLength = ridgeConfiguration.RidgeMatrixLength;
            double magnitudeThreshold = ridgeConfiguration.RidgeDetectionmMagnitudeThreshold;
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int halfLength = ridgeLength / 2;
            var hits = new byte[rows, cols];
            newMatrix = new double[rows, cols];
            for (int r = 0; r < rows - halfLength; r++)
            {
                for (int c = 0; c < cols - halfLength; c++)
                {
                    if (hits[r, c] > 0) continue;
                    var subM = MatrixTools.Submatrix(matrix, r, c, r + halfLength, c + halfLength); // extract NxN submatrix
                    double magnitude;
                    double direction;
                    bool isRidge = false;
                    // magnitude is dB, direction is double value which is times of pi/4, from the start of 0. 
                    ImageAnalysisTools.GradientCalculation(subM, out isRidge, out magnitude, out direction);
                    if (magnitude > magnitudeThreshold && isRidge == true)
                    {
                        newMatrix[r, c] = magnitude; 
                        if (direction >= 0.0 && direction < Math.PI/8)
                        {
                            hits[r, c] = (byte)(1);                                                    
                        }
                        if (direction >= Math.PI / 8 && direction < Math.PI / 4)
                        {
                            hits[r, c] = (byte)(2);                                                
                        }
                        if (direction >= Math.PI / 4 && direction < Math.PI*3 / 8)
                        {
                            hits[r, c] = (byte)(3);                                                 
                        }
                        if (direction >= Math.PI * 3 / 8 && direction < Math.PI / 2)
                        {
                            hits[r, c] = (byte)(4);                         
                        }
                        if (direction >= Math.PI / 2 && direction < Math.PI * 5 / 8)
                        {
                            hits[r, c] = (byte)(5);
                        }
                        if (direction >= Math.PI * 5 / 8 && direction < Math.PI*3 / 4)
                        {
                            hits[r, c] = (byte)(6);
                        }
                        if (direction >= Math.PI * 3 / 4 && direction < Math.PI * 7 / 8)
                        {
                            hits[r, c] = (byte)(7);
                        }
                        if (direction >= Math.PI * 7 / 8 && direction < Math.PI)
                        {
                            hits[r, c] = (byte)(8);
                        }
                    }
                }
            }     
            return hits;
        }

        public static List<double> intensityThresholdForSpectrogram(double[,] matrix)
        {            
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);          
            var intensityList = new List<double>();
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    if (matrix[r, c] > 0)
                    {
                        intensityList.Add(matrix[r, c]);
                    }
                }
            }

            return intensityList;
        }

        public static double calculateIntensityThreshold(List<double> intensityList, double intensityThreshold)
        {
            var result = 0.0;
            var maxIntensity = intensityList.Max();
            var binCount = 1000;
            var l = StructureTensorAnalysis.GetMaximumLength(intensityList, maxIntensity, intensityThreshold, binCount);
            result = l * maxIntensity / binCount;
            return result;
        }
        
        public void ImprovedRidgeDetection(SpectrogramStandard spectrogram, RidgeDetectionConfiguration ridgeConfiguration)
        {
            // This step tries to convert spectrogram data into image matrix. The spectrogram data has the dimension of totalFrameCount * totalFreCount and the matrix is totalFreCount * totalFrameCount. 
            // Notice that the matrix is a normal matrix. RowIndex and ColumnIndex all follow the matrix definition. The data is first stored in rows
            //(corresponding to the frame in spectrogram from small to large) and then in columns (corresponding to the frequencyBin from high to low).
            double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            int ridgeLength = ridgeConfiguration.RidgeMatrixLength;
            double magnitudeThreshold = ridgeConfiguration.RidgeDetectionmMagnitudeThreshold;
            double secondsScale = spectrogram.Configuration.GetFrameOffset(spectrogram.SampleRate); // 0.0116
            var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
            double herzScale = spectrogram.FBinWidth; //43 hz
            double freqBinCount = spectrogram.Configuration.FreqBinCount; //256
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
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
                    ImageAnalysisTools.Sobel5X5RidgeDetection4Direction(subM, out isRidge, out magnitude, out direction);
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
                        // OrientationCategory only has four values, they are 0, 1, 2, 3. 
                        poi.OrientationCategory = (int)Math.Round((direction * 8) / Math.PI);
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
            /// filter out some redundant ridges               
            var prunedPoiList = ImageAnalysisTools.PruneAdjacentTracks(poiList, rows, cols);
            var prunedPoiList1 = ImageAnalysisTools.IntraPruneAdjacentTracks(prunedPoiList, rows, cols);
            var filteredPoiList = ImageAnalysisTools.RemoveIsolatedPoi(prunedPoiList1, rows, cols, ridgeConfiguration.FilterRidgeMatrixLength, ridgeConfiguration.MinimumNumberInRidgeInMatrix);
            //var connectedPoiList = PoiAnalysis.ConnectPOI(filteredPoiList);
            var refinedPoiList = POISelection.RefineRidgeDirection(filteredPoiList, rows, cols);
            poiList = refinedPoiList;
        }

        /// <summary>
        /// To select ridges on the spectrogram data, matrix. 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="ridgeLength"> By default, it's 5. Because our ridge detection is 5*5 neighbourhood.
        /// </param>
        /// <param name="magnitudeThreshold"> The range usually goest to 5 ~ 7. If the value is low, it will give you more ridges.
        /// otherwise, less ridges will return. 
        /// </param>
        /// <param name="secondsScale"> This depends on the FFT parameters you've done. For my case, it's 0.0116 s. It means every
        /// pixel represents such second.
        /// </param>
        /// <param name="timeScale"> As above, it's 11.6 ms for each frame. 
        /// </param>
        /// <param name="herzScale"> As above, it's 43 Hz for each frequency bin. 
        /// </param>
        /// <param name="freqBinCount"> As above, it's 256 frequency bins.
        /// </param>
        public void SelectRidgesFromMatrix(double[,] matrix, int ridgeLength, double magnitudeThreshold, double secondsScale, TimeSpan timeScale, double herzScale, double freqBinCount)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
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
                    ImageAnalysisTools.Sobel5X5RidgeDetection4Direction(subM, out isRidge, out magnitude, out direction);
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
                        // OrientationCategory only has four values, they are 0, 1, 2, 3. 
                        poi.OrientationCategory = (int)Math.Round((direction * 8) / Math.PI);
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

        /// <summary>
        /// Divide the poilist into 4 groups according to different orientationCategories. Basically, we have 4 groups: v, h, pd, nd.
        /// </summary>
        /// <param name="poiList"></param>
        /// <returns></returns>
        public static List<List<PointOfInterest>> POIListDivision(List<PointOfInterest> poiList)
        {
            var poiVerticalGroup = new List<PointOfInterest>();
            var poiHorizontalGroup = new List<PointOfInterest>();
            var poiPDGroup = new List<PointOfInterest>();
            var poiNDGroup = new List<PointOfInterest>();
            var result = new List<List<PointOfInterest>>();

            foreach (var p in poiList)
            {
                if (p.OrientationCategory == (int)Direction.North)
                {
                    poiVerticalGroup.Add(p);
                }
                if (p.OrientationCategory == (int)Direction.East)
                {
                    poiHorizontalGroup.Add(p);
                }
                if (p.OrientationCategory == (int)Direction.NorthEast)
                {
                    poiPDGroup.Add(p);
                }
                if (p.OrientationCategory == (int)Direction.NorthWest)
                {
                    poiNDGroup.Add(p);
                }
            }
            result.Add(poiVerticalGroup);
            result.Add(poiHorizontalGroup);
            result.Add(poiPDGroup);
            result.Add(poiNDGroup);

            return result;
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

        // using the gradient to calculate the real values for each poi's magnitude and direction.  
        public static List<PointOfInterest> CalulateRidgeRealValues(List<PointOfInterest> poiList, int rowsMax, int colsMax)
        {
            var poiMatrix = StatisticalAnalysis.TransposePOIsToMatrix(poiList, rowsMax, colsMax);
            for (int r = 0; r < rowsMax - 1; r++)
            {
                for (int c = 0; c < colsMax - 1; c++)
                {
                    if (poiMatrix[r, c] != null && poiMatrix[r, c].RidgeMagnitude != 0.0)
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
                        var realOrientation = poiMatrix[r, c].RidgeOrientation;
                        if (realOrientation > 0 && realOrientation < Math.PI / 8)
                        {
                            poiMatrix[r, c].RidgeOrientation = 0;
                        }
                        else
                        {
                            if (realOrientation > Math.PI / 8 && realOrientation < Math.PI / 4)
                            {
                                poiMatrix[r, c].RidgeOrientation = 1;
                            }
                        }
                    }
                }
            }
            var result = StatisticalAnalysis.TransposeMatrixToPOIlist(poiMatrix);
            return result;
        }

        ///Until now, ridge direction has up to 4, which are 0, pi/2, pi/4, -pi/4. 
        ///But they might be not enough to differenciate the lines with slope change, so here we refine the original 4 direction to 12. 
        public static List<PointOfInterest> RefineRidgeDirection(List<PointOfInterest> poiList, int rowsMax, int colsMax)
        {
            var poiMatrix = StatisticalAnalysis.TransposePOIsToMatrix(poiList, rowsMax, colsMax);
            int lenghth = 5;
            int radius = lenghth / 2;
            for (int row = radius; row < rowsMax - radius; row++)
            {
                for (int col = radius; col < colsMax - radius; col++)
                {
                    if (poiMatrix[row, col].RidgeMagnitude != 0)
                    {
                        var matrix = StatisticalAnalysis.SubmatrixFromPointOfInterest(poiMatrix, row - radius, col - radius, row + radius, col + radius);
                        double[,] m = 
                    {{matrix[0,0].RidgeMagnitude, matrix[0,1].RidgeMagnitude, matrix[0,2].RidgeMagnitude, matrix[0,3].RidgeMagnitude, matrix[0,4].RidgeMagnitude},
                    {matrix[1,0].RidgeMagnitude, matrix[1,1].RidgeMagnitude, matrix[1,2].RidgeMagnitude, matrix[1,3].RidgeMagnitude, matrix[1,4].RidgeMagnitude},
                    {matrix[2,0].RidgeMagnitude, matrix[2,1].RidgeMagnitude, matrix[2,2].RidgeMagnitude, matrix[2,3].RidgeMagnitude, matrix[2,4].RidgeMagnitude},
                    {matrix[3,0].RidgeMagnitude, matrix[3,1].RidgeMagnitude, matrix[3,2].RidgeMagnitude, matrix[3,3].RidgeMagnitude, matrix[3,4].RidgeMagnitude},
                    {matrix[4,0].RidgeMagnitude, matrix[4,1].RidgeMagnitude, matrix[4,2].RidgeMagnitude, matrix[4,3].RidgeMagnitude, matrix[4,4].RidgeMagnitude},
                    };
                        var magnitude = 0.0;
                        var direction = 0.0;
                        var poiCountInMatrix = 0;
                        for (int i = 0; i < lenghth; i++)
                        {
                            for (int j = 0; j < lenghth; j++)
                            {
                                if (m[i, j] > 0)
                                {
                                    poiCountInMatrix++;
                                }
                            }
                        }
                        if (poiCountInMatrix >= 5)
                        {
                            RecalculateRidgeDirection(m, out magnitude, out direction);
                            poiMatrix[row, col].RidgeMagnitude = magnitude;
                            poiMatrix[row, col].RidgeOrientation = direction;
                        }
                    }
                }
            }
            var result = StatisticalAnalysis.TransposeMatrixToPOIlist(poiMatrix);
            return result;
        }

        // Refine Directions
        public static void RecalculateRidgeDirection(double[,] m, out double magnitude, out double direction)
        {
            double[,] dir0Mask = { {  0,   0,   0,   0,   0},
                                   {  0,   0,   0,   0,   0},
                                   {0.1, 0.1, 0.1, 0.1, 0.1},
                                   {  0,   0,   0,   0,   0},
                                   {  0,   0,   0,   0,   0},
                                 };
            double[,] dir1Mask = { {  0,   0,   0,   0,   0},
                                   {  0,   0,   0,   0, 0.1},
                                   {  0, 0.1, 0.1, 0.1,   0},
                                   {0.1,   0,   0,   0,   0},
                                   {  0,   0,   0,   0,   0},
                                 };
            double[,] dir2Mask = { {  0,   0,   0,   0,   0},
                                   {  0,   0,   0, 0.1, 0.1},
                                   {  0,   0, 0.1,   0,   0},
                                   {0.1, 0.1,   0,   0,   0}, 
                                   {  0,   0,   0,   0,   0},
                                 };
            // The fourth mask for pi/4. But something got wrong.
            double[,] dir3Mask = { {  0,   0,   0,   0, 0.1},
                                   {  0,   0,   0, 0.1,   0},
                                   {  0,   0, 0.1,   0,   0},
                                   {  0, 0.1,   0,   0,   0}, 
                                   {0.1,   0,   0,   0,   0},
                                 };
            double[,] dir4Mask = { {  0,   0,   0, 0.1,   0},
                                   {  0,   0,   0, 0.1,   0},
                                   {  0,   0, 0.1,   0,   0},
                                   {  0, 0.1,   0,   0,   0},
                                   {  0, 0.1,   0,   0,   0},
                                 };
            double[,] dir5Mask = { {  0,   0,   0, 0.1,   0},
                                   {  0,   0, 0.1,   0,   0},
                                   {  0,   0, 0.1,   0,   0},
                                   {  0,   0, 0.1,   0,   0}, 
                                   {  0, 0.1,   0,   0,   0},
                                 };
            double[,] dir6Mask = { {  0,   0, 0.1,   0,   0},
                                   {  0,   0, 0.1,   0,   0},
                                   {  0,   0, 0.1,   0,   0},
                                   {  0,   0, 0.1,   0,   0},
                                   {  0,   0, 0.1,   0,   0},
                                 };
            double[,] dir7Mask = { {  0, 0.1,   0,   0,   0},
                                   {  0,   0, 0.1,   0,   0},
                                   {  0,   0, 0.1,   0,   0},
                                   {  0,   0, 0.1,   0,   0}, 
                                   {  0,   0,   0, 0.1,   0},
                                 };
            double[,] dir8Mask = { {  0, 0.1,   0,   0,   0},
                                   {  0, 0.1,   0,   0,   0},
                                   {  0,   0, 0.1,   0,   0},
                                   {  0,   0,   0, 0.1,   0},
                                   {  0,   0,   0, 0.1,   0},
                                 };
            // The tenth mask for 3*pi/4. But something got wrong.
            double[,] dir9Mask = { {0.1,   0,   0,   0,   0},
                                   {  0, 0.1,   0,   0,   0},
                                   {  0,   0, 0.1,   0,   0},
                                   {  0,   0,   0, 0.1,   0},
                                   {  0,   0,   0,   0, 0.1},
                                  };
            double[,] dir10Mask = {{  0,   0,   0,   0,   0},
                                   {0.1, 0.1,   0,   0,   0},
                                   {  0,   0, 0.1,   0,   0},
                                   {  0,   0,   0, 0.1, 0.1},
                                   {  0,   0,   0,   0,   0},
                                  };
            double[,] dir11Mask = {{  0,   0,   0,   0,   0},
                                   {0.1,   0,   0,   0,   0},
                                   {  0, 0.1, 0.1, 0.1,   0},
                                   {  0,   0,   0,   0, 0.1},
                                   {  0,   0,   0,   0,   0},
                                  };
            double[] magnitudes = new double[12];
            magnitudes[0] = MatrixTools.DotProduct(dir0Mask, m);
            magnitudes[1] = MatrixTools.DotProduct(dir1Mask, m);
            magnitudes[2] = MatrixTools.DotProduct(dir2Mask, m);
            magnitudes[3] = MatrixTools.DotProduct(dir3Mask, m);
            magnitudes[4] = MatrixTools.DotProduct(dir4Mask, m);
            magnitudes[5] = MatrixTools.DotProduct(dir5Mask, m);
            magnitudes[6] = MatrixTools.DotProduct(dir6Mask, m);
            magnitudes[7] = MatrixTools.DotProduct(dir7Mask, m);
            magnitudes[8] = MatrixTools.DotProduct(dir8Mask, m);
            magnitudes[9] = MatrixTools.DotProduct(dir9Mask, m);
            magnitudes[10] = MatrixTools.DotProduct(dir10Mask, m);
            magnitudes[11] = MatrixTools.DotProduct(dir11Mask, m);

            int indexMin, indexMax;
            double sumMin, sumMax;
            DataTools.MinMax(magnitudes, out indexMin, out indexMax, out sumMin, out sumMax);
            magnitude = sumMax;
            direction = indexMax * Math.PI / (double)12;
        }

        public void SelectPointOfInterestFromAudioFile(string wavFilePath, int ridgeLength, double magnitudeThreshold)
        {
            //var spectrogram = SpectrogramGeneration(wavFilePath);
            var recording = new AudioRecording(wavFilePath);
            var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.5 };
            var spectrogram = new SpectrogramStandard(config, recording.WavReader);
            double secondsScale = spectrogram.Configuration.GetFrameOffset(recording.SampleRate);
            var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
            double herzScale = spectrogram.FBinWidth;
            double freqBinCount = spectrogram.Configuration.FreqBinCount;
            var matrix = SpectrogramIntensityToArray(spectrogram);
            var rowsCount = matrix.GetLength(0);
            var colsCount = matrix.GetLength(1);

            var pointsOfInterest = new POISelection(new List<PointOfInterest>());
            pointsOfInterest.SelectRidgesFromMatrix(matrix, ridgeLength, magnitudeThreshold, secondsScale, timeScale, herzScale, freqBinCount);
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
       
        public double[,] SpectrogramIntensityToArray(SpectrogramStandard spectrogram)
        {
            var matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            return matrix;
        }

        #endregion

    }
}
