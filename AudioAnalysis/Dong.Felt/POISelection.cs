
namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AudioAnalysisTools;
    using TowseyLib;
    using System.Drawing;

    public class POISelection
    {
        public enum RidgeOrientationType { NONE, HORIZONTAL, POSITIVE_QUATERPI, VERTICAL, NEGATIVE_QUATERPI }

        public List<PointOfInterest> poiList { get; set; }

        public int RowsCount { get; set; }

        public int ColsCount { get; set; }

        #region Public Methods

        public static List<PointOfInterest> RidgeDetection(SpectralSonogram spectrogram, RidgeDetectionConfiguration ridgeConfiguration)
        {
            // list size based on avg result size
            var instance = new POISelection(new List<PointOfInterest>(9000));

            instance.RidgeDetectionInternal(spectrogram, ridgeConfiguration);

            return instance.poiList;
        }

        public POISelection(List<PointOfInterest> list)
        {
            poiList = list;
        }

        internal void RidgeDetectionInternal(SpectralSonogram spectrogram, RidgeDetectionConfiguration ridgeConfiguration)
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

        public void ImprovedRidgeDetection(SpectralSonogram spectrogram, RidgeDetectionConfiguration ridgeConfiguration)
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
            poiList = filteredPoiList;
        }

        public List<PointOfInterest> RidgeDetectionNoFilter(SpectralSonogram spectrogram, RidgeDetectionConfiguration ridgeConfiguration, double[,] falseMatrix)
        {           
            //double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(falseMatrix);
            int ridgeLength = ridgeConfiguration.RidgeMatrixLength;
            double magnitudeThreshold = ridgeConfiguration.RidgeDetectionmMagnitudeThreshold;
            double secondsScale = spectrogram.Configuration.GetFrameOffset(spectrogram.SampleRate); // 0.0116
            var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
            double herzScale = spectrogram.FBinWidth; //43 hz
            double freqBinCount = spectrogram.Configuration.FreqBinCount; //256
            int rows = falseMatrix.GetLength(0);
            int cols = falseMatrix.GetLength(1);
            int halfLength = ridgeLength / 2;
            for (int r = halfLength; r < rows - halfLength; r++)
            {
                for (int c = halfLength; c < cols - halfLength; c++)
                {
                    var subM = MatrixTools.Submatrix(falseMatrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix
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
                        poi.Intensity = falseMatrix[r, c];
                        poi.TimeScale = timeScale;
                        poi.HerzScale = herzScale;         
                        poiList.Add(poi);                
                    }
                }
            }
            return poiList;
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
            for (int r = 0; r < rowsMax - 1; r++)
            {
                for (int c = 0; c < colsMax - 1; c++)
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
            return result;
        }

        ///Until now, ridge direction has up to 4, which are 0, pi/2, pi/4, -pi/4. 
        ///But they might be not enough to differenciate the lines with slope change, so here we refine the original 4 direction to 12. 
        public static List<PointOfInterest> RefineRidgeDirection(List<PointOfInterest> poiList, int rowsMax, int colsMax)
        {
            var poiMatrix = StatisticalAnalysis.TransposePOIsToMatrix(poiList, rowsMax, colsMax);
            int lenghth = 5;
            int Radius = lenghth / 2;
            for (int row = Radius; row < rowsMax - Radius; row++)
            {
                for (int col = Radius; col < colsMax - Radius; col++)
                {
                    if (poiMatrix[row, col].RidgeMagnitude != 0)
                    {
                        var matrix = StatisticalAnalysis.SubmatrixFromPointOfInterest(poiMatrix, row - Radius, col - Radius, row + Radius, col + Radius);
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
            var spectrogram = new SpectralSonogram(config, recording.GetWavReader());
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
