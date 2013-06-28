
namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Drawing;
    using AudioAnalysisTools;

    class RectangularRepresentation
    {
        #region Public Properties
        public static Color DEGAULT_BORDER_COLOR = Color.Crimson;
        public static Color DEFAULT_SCORE_COLOR = Color.Black;

        /// <summary>
        /// Gets or sets the MinFrequency from the rectangular (box users drew), which is the frequency value of top line of the box. 
        /// </summary>
        public double MinFrequency { get; set; }

        /// <summary>
        /// Gets or sets the MinFrequency from the rectangular (box users drew), which is the frequency value of bottom line of the box. 
        /// </summary>
        public double MaxFrequency { get; set; }

        /// <summary>
        /// Gets or sets the duration from the rectangular (box users drew), which is equal to the width of box.  
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// Gets or sets the startTime of the rectangular (box users drew), which is equal to the time value of left line of box.  
        /// </summary>
        public double StartTime { get; set; }

        /// <summary>
        /// Gets or sets the centroid of the box.  
        /// </summary>
        public Point Centroid { get; set; }

        /// <summary> 
        /// required for conversions to & from MEL scale AND for drawing event on spectrum.
        /// </summary>
        public int FreqBinCount { get; set; }

        /// <summary>
        /// required for conversions to & from MEL scale AND for drawing event on spectrum.
        /// </summary>
        public double FrequencyBinWidth { get; set; }

        /// <summary>
        /// required for conversions to & from MEL scale AND for drawing event on spectrum.
        /// </summary>
        public double FramesPerSecond { get; set; }
        #endregion

        #region Pulic Methods
        /// <summary>
        /// Constructor method.
        /// </summary>
        /// <param name="minFrequency"></param>
        /// <param name="maxFrequency"></param>
        /// <param name="startTime"></param>
        /// <param name="duration"></param>
        public RectangularRepresentation(double minFrequency, double maxFrequency, double startTime, double duration)
        {
            MaxFrequency = maxFrequency;
            MinFrequency = minFrequency;
            StartTime = startTime;
            Duration = duration;
        }

        /// <summary>
        /// Calculate the centroid of the rectangular box. 
        /// </summary>
        /// <param name="acousticEvent"></param>
        public void CentroidOfAcousticEvents(AcousticEvents acousticEvent)
        {
            var frequencyBinWidth = 43.0;
            var framePerSecond = 86.0;
            var x = (int)((acousticEvent.MaxFreq - acousticEvent.MinFreq) / frequencyBinWidth * 0.5);
            var y = (int)((acousticEvent.TimeStart - acousticEvent.TimeEnd) * framePerSecond * 0.5);
            this.Centroid = new Point(x, y);
        }

        /// <summary>
        ///  Indexing each potential event at each frame. Have done the test. 
        /// </summary>
        /// <param name="poiList"></param>
        /// <param name="maxFrequency"> this can be derived from a query.</param>
        /// <param name="minFrequency">this can be derived from a query.</param>
        /// <param name="duration">this can be derived from a query.</param>
        /// <param name="herzPerSlice"></param>
        /// <param name="durationPerSlice">
        /// Its unit should be second unit.
        /// </param>
        /// <param name="herzScale"></param>
        /// Represents the frequency range of one pixel covers.
        /// <param name="timeScale">
        /// Represents the duration of one pixel account for.
        /// </param>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        public static List<List<FeatureVector>> RepresentationForIndexing(List<PointOfInterest> poiList, double maxFrequency, double minFrequency,
                                                               double duration, int sizeofNeighbourhood, double herzScale, double timeScale,
                                                               double nyquistFrequency, int rowsCount, int colsCount)
        {

            var MaxRowIndex = (int)Math.Ceiling((nyquistFrequency - minFrequency) / herzScale);
            var MinRowIndex = (int)Math.Floor((nyquistFrequency - maxFrequency) / herzScale);
            var numberOfRowSlices = (int)Math.Ceiling((maxFrequency - minFrequency) / sizeofNeighbourhood);
            var numberOfColSlices = (int)Math.Ceiling(duration / sizeofNeighbourhood);
            var halfRowNeighbourhood = sizeofNeighbourhood / 2;
            var halfColNeighbourhood = sizeofNeighbourhood / 2;
            var result = new List<List<FeatureVector>>();
            var Matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);
            // search along the fixed frequency range.
            for (int row = MinRowIndex; row < MaxRowIndex; row += sizeofNeighbourhood)
            {
                // search along time position one by one. 
                for (int col = 0; col < colsCount; col++)
                {
                    for (int sliceRowIndex = 0; sliceRowIndex < numberOfRowSlices; sliceRowIndex++)
                    {
                        for (int sliceColIndex = 0; sliceColIndex < numberOfColSlices; sliceColIndex++)
                        {
                            if (StatisticalAnalysis.checkBoundary(row + (sliceRowIndex + 1) * sizeofNeighbourhood, col + (sliceColIndex + 1) * sizeofNeighbourhood, rowsCount, colsCount))
                            {
                                var subMatrix = StatisticalAnalysis.Submatrix(Matrix, row + sliceRowIndex * sizeofNeighbourhood, col + sliceColIndex * sizeofNeighbourhood, row + (sliceRowIndex + 1) * sizeofNeighbourhood, col + (sliceColIndex + 1) * sizeofNeighbourhood);
                                var partialFeatureVector = RectangularRepresentation.SliceIntegerEdgeRepresentation(subMatrix, row + halfRowNeighbourhood, col + halfColNeighbourhood);
                                result.Add(new List<FeatureVector>());
                                result[sliceRowIndex * sizeofNeighbourhood + sliceColIndex + col].Add(new FeatureVector(new Point(row + halfRowNeighbourhood, col + halfColNeighbourhood))
                                {
                                    HorizontalVector = partialFeatureVector.HorizontalVector,
                                    VerticalVector = partialFeatureVector.VerticalVector,
                                    PositiveDiagonalVector = partialFeatureVector.PositiveDiagonalVector,
                                    NegativeDiagonalVector = partialFeatureVector.NegativeDiagonalVector
                                });
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// This method is used for calculate each slice representation, each slice is derived from the origional rectangular. 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="PointX">This value is the X coordinate of centroid of rectangular.</param>
        /// <param name="PointY">This value is the Y coordinate of centroid of rectangular.</param>
        /// <returns></returns>
        public static FeatureVector SliceIntegerEdgeRepresentation(PointOfInterest[,] matrix, int PointX, int PointY)
        {
            var result = new FeatureVector(new Point(PointX, PointY));
            var sizeOfNeighbourhood = matrix.GetLength(0);
            // To search in a neighbourhood, the original pointsOfInterst should be converted into a pointOfInterst of Matrix
            var radiusOfNeighbourhood = sizeOfNeighbourhood / 2;

            var verticalDirection = new int[sizeOfNeighbourhood];
            var horizontalDirection = new int[sizeOfNeighbourhood];
            var positiveDiagonalDirection = new int[2 * sizeOfNeighbourhood - 1];
            var negativeDiagonalDirection = new int[2 * sizeOfNeighbourhood - 1];
            var anchorPoint = new Point(radiusOfNeighbourhood, radiusOfNeighbourhood);
            // For the calculation of horizontal direction byte, we need to check each row 
            for (int rowNeighbourhoodIndex = -radiusOfNeighbourhood; rowNeighbourhoodIndex <= radiusOfNeighbourhood; rowNeighbourhoodIndex++)
            {
                for (int colNeighbourhoodIndex = -radiusOfNeighbourhood; colNeighbourhoodIndex <= radiusOfNeighbourhood; colNeighbourhoodIndex++)
                {
                    // check boundary of index 
                    if (StatisticalAnalysis.checkBoundary(anchorPoint.X + rowNeighbourhoodIndex, anchorPoint.Y + colNeighbourhoodIndex, sizeOfNeighbourhood, sizeOfNeighbourhood))
                    {
                        if ((matrix[anchorPoint.X + rowNeighbourhoodIndex, anchorPoint.Y + colNeighbourhoodIndex] != null) && matrix[anchorPoint.X + rowNeighbourhoodIndex, anchorPoint.Y + colNeighbourhoodIndex].RidgeOrientation == (int)Direction.East)
                        {
                            horizontalDirection[rowNeighbourhoodIndex + radiusOfNeighbourhood]++;
                        }
                    }
                }
            }

            // For the calculation of vertical direction byte, we need to check each column
            for (int rowNeighbourhoodIndex = -radiusOfNeighbourhood; rowNeighbourhoodIndex <= radiusOfNeighbourhood; rowNeighbourhoodIndex++)
            {
                for (int colNeighbourhoodIndex = -radiusOfNeighbourhood; colNeighbourhoodIndex <= radiusOfNeighbourhood; colNeighbourhoodIndex++)
                {
                    if (StatisticalAnalysis.checkBoundary(anchorPoint.X + colNeighbourhoodIndex, anchorPoint.Y + rowNeighbourhoodIndex, sizeOfNeighbourhood, sizeOfNeighbourhood))
                    {
                        if ((matrix[anchorPoint.X + colNeighbourhoodIndex, anchorPoint.Y + rowNeighbourhoodIndex] != null) && matrix[anchorPoint.X + colNeighbourhoodIndex, anchorPoint.Y + rowNeighbourhoodIndex].RidgeOrientation == (int)Direction.North)
                        {
                            verticalDirection[rowNeighbourhoodIndex + radiusOfNeighbourhood]++;
                        }
                    }
                }
            }

            // For the calculation of negativeDiagonal direction, we need to check each diagonal line.
            for (int offset = 0; offset < sizeOfNeighbourhood; offset++)
            {
                for (int offsetIndex = -radiusOfNeighbourhood; offsetIndex <= radiusOfNeighbourhood; offsetIndex++)
                {
                    if (StatisticalAnalysis.checkBoundary(anchorPoint.X + offsetIndex + offset, anchorPoint.Y + offsetIndex, sizeOfNeighbourhood, sizeOfNeighbourhood))
                    {
                        if ((matrix[anchorPoint.X + offsetIndex + offset, anchorPoint.Y + offsetIndex] != null) && (matrix[anchorPoint.X + offsetIndex + offset, anchorPoint.Y + offsetIndex].RidgeOrientation == (int)Direction.NorthWest))
                        {
                            negativeDiagonalDirection[sizeOfNeighbourhood - offset - 1]++;
                        }
                    }
                }
            }
            for (int offset = 1; offset < sizeOfNeighbourhood; offset++)
            {
                for (int offsetIndex = -radiusOfNeighbourhood; offsetIndex <= radiusOfNeighbourhood; offsetIndex++)
                {
                    if (StatisticalAnalysis.checkBoundary(anchorPoint.X + offsetIndex - offset, anchorPoint.Y + offsetIndex, sizeOfNeighbourhood, sizeOfNeighbourhood))
                    {
                        if ((matrix[anchorPoint.X + offsetIndex - offset, anchorPoint.Y + offsetIndex] != null) && (matrix[anchorPoint.X + offsetIndex - offset, anchorPoint.Y + offsetIndex].RidgeOrientation == (int)Direction.NorthWest))
                        {
                            negativeDiagonalDirection[sizeOfNeighbourhood + offset - 1]++;
                        }
                    }
                }
            }

            // For the calculation of positiveDiagonal direction, we need to check each diagonal line.
            for (int offset = 0; offset < sizeOfNeighbourhood; offset++)
            {
                for (int offsetIndex = -radiusOfNeighbourhood; offsetIndex <= radiusOfNeighbourhood; offsetIndex++)
                {
                    if (StatisticalAnalysis.checkBoundary(anchorPoint.X + offsetIndex, anchorPoint.Y - offsetIndex - offset, sizeOfNeighbourhood, sizeOfNeighbourhood))
                    {
                        if ((matrix[anchorPoint.X + offsetIndex, anchorPoint.Y - offsetIndex - offset] != null) && (matrix[anchorPoint.X + offsetIndex, anchorPoint.Y - offsetIndex - offset].RidgeOrientation == (int)Direction.NorthEast))
                        {
                            positiveDiagonalDirection[sizeOfNeighbourhood - offset - 1]++;
                        }
                    }
                }
            }
            for (int offset = 1; offset < sizeOfNeighbourhood; offset++)
            {
                for (int offsetIndex = -radiusOfNeighbourhood; offsetIndex <= radiusOfNeighbourhood; offsetIndex++)
                {
                    if (StatisticalAnalysis.checkBoundary(anchorPoint.X + offsetIndex, anchorPoint.Y - offsetIndex + offset, sizeOfNeighbourhood, sizeOfNeighbourhood))
                    {
                        if ((matrix[anchorPoint.X + offsetIndex, anchorPoint.Y - offsetIndex + offset] != null) && (matrix[anchorPoint.X + offsetIndex, anchorPoint.Y - offsetIndex + offset].RidgeOrientation == (int)Direction.NorthEast))
                        {
                            positiveDiagonalDirection[sizeOfNeighbourhood + offset - 1]++;
                        }
                    }
                }
            }

            result.HorizontalVector = horizontalDirection;
            result.VerticalVector = verticalDirection;
            result.PositiveDiagonalVector = positiveDiagonalDirection;
            result.NegativeDiagonalVector = negativeDiagonalDirection;
            return result;
        }

        /// <summary>
        /// Still need to work on this one. 
        /// </summary>
        /// <param name="poiList"></param>
        /// <param name="maxFrequency"></param>
        /// <param name="minFrequency"></param>
        /// <param name="duration"></param>
        /// <param name="herzPerSlice"></param>
        /// <param name="durationPerSlice"></param>
        /// <param name="herzScale"></param>
        /// <param name="timeScale"></param>
        /// <param name="nyquistFrequency"></param>
        /// <param name="rowsCount"></param>
        /// <param name="colsCount"></param>
        /// <returns></returns>
        public static List<List<FeatureVector>> RepresentationForQuery(List<PointOfInterest> poiList, double maxFrequency, double minFrequency,
                                                               double duration, int herzPerSlice, double durationPerSlice, double herzScale, double timeScale,
                                                               double nyquistFrequency, int rowsCount, int colsCount)
        {
            var rowsCountPerSlice = (int)Math.Ceiling(herzPerSlice / herzScale);  // 13 pixels  560Hz
            var colsCountPerSlice = (int)Math.Ceiling(durationPerSlice / timeScale); // 13 pixels 0.15 second
            var MaxRowIndex = (int)Math.Ceiling((nyquistFrequency - minFrequency) / herzScale);
            var MinRowIndex = (int)Math.Floor((nyquistFrequency - maxFrequency) / herzScale);
            var numberOfRowSlices = (int)Math.Ceiling((maxFrequency - minFrequency) / herzPerSlice);
            var numberOfColSlices = (int)Math.Ceiling(duration / durationPerSlice);
            var halfNumberOfRowSlices = numberOfRowSlices / 2;
            var halfNumberOfColSlices = numberOfColSlices / 2;
            var result = new List<List<FeatureVector>>();
            var Matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);
            // search along the fixed frequency range.
            for (int row = MinRowIndex; row < MaxRowIndex; row += rowsCountPerSlice)
            {
                // search along time position one by one. 
                for (int col = 0; col < colsCount; col++)
                {
                    for (int sliceRowIndex = 0; sliceRowIndex < numberOfRowSlices; sliceRowIndex++)
                    {
                        for (int sliceColIndex = 0; sliceColIndex < numberOfColSlices; sliceColIndex++)
                        {
                            if (StatisticalAnalysis.checkBoundary(row + (sliceRowIndex + 1) * rowsCountPerSlice, col + (sliceColIndex + 1) * colsCountPerSlice, rowsCount, colsCount))
                            {
                                var subMatrix = StatisticalAnalysis.Submatrix(Matrix, row + sliceRowIndex * rowsCountPerSlice, col + sliceColIndex * colsCountPerSlice, row + (sliceRowIndex + 1) * rowsCountPerSlice, col + (sliceColIndex + 1) * colsCountPerSlice);
                                var partialFeatureVector = RectangularRepresentation.SliceIntegerEdgeRepresentation(subMatrix, row + halfNumberOfRowSlices, col + halfNumberOfColSlices);
                                result.Add(new List<FeatureVector>());
                                result[sliceRowIndex * numberOfColSlices + sliceColIndex].Add(new FeatureVector(new Point(row, col))
                                {
                                    HorizontalVector = partialFeatureVector.HorizontalVector,
                                    VerticalVector = partialFeatureVector.VerticalVector,
                                    PositiveDiagonalVector = partialFeatureVector.PositiveDiagonalVector,
                                    NegativeDiagonalVector = partialFeatureVector.NegativeDiagonalVector
                                });
                            }
                        }
                    }
                }
            }
            return result;
        }

        public static void RectangularToMatrix(RectangularRepresentation rectangular, double nyquistFrequency)
        {
            var maxRowIndex = (int)Math.Ceiling((nyquistFrequency - rectangular.MinFrequency) / rectangular.FrequencyBinWidth);
            var minRowIndex = (int)Math.Floor((nyquistFrequency - rectangular.MaxFrequency) / rectangular.FrequencyBinWidth);
            var startTimeIndex = (int)(rectangular.StartTime * rectangular.FramesPerSecond);
            var endTimeIndex = (int)((rectangular.Duration - rectangular.StartTime) * rectangular.FramesPerSecond);
            var rowsCount = maxRowIndex - minRowIndex;
            var colsCount = endTimeIndex - startTimeIndex;
            var matrix = new PointOfInterest[rowsCount, colsCount];
            for (int row = 0; row < rowsCount; row++)
            {
                for (int col = 0; col < colsCount; col++)
                {
                    matrix[row, col].Intensity = 0.0;
                }
            }
        }
        #endregion

    }
}
