
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
                                                               double nyquistFrequency, int rowsCount, int colsCount, int searchStep, int frequencyOffset)
        {
            var MaxRowIndex = (int)Math.Ceiling((nyquistFrequency - minFrequency) / herzScale);
            var MinRowIndex = (int)Math.Floor((nyquistFrequency - maxFrequency) / herzScale);

            var extendedFrequencyRange = 0;
            if ((MaxRowIndex - MinRowIndex) % sizeofNeighbourhood != 0)
            {
                extendedFrequencyRange = sizeofNeighbourhood - (MaxRowIndex - MinRowIndex) % sizeofNeighbourhood;
            }
            var numberOfFrames = duration / timeScale;
            var halfExtendedFrequencyRange = extendedFrequencyRange / 2;
            var numberOfRowSlices = (int)Math.Ceiling((double)(MaxRowIndex - MinRowIndex) / sizeofNeighbourhood);
            var numberOfColSlices = (int)Math.Ceiling((double)numberOfFrames / sizeofNeighbourhood);
            var halfRowNeighbourhood = sizeofNeighbourhood / 2;
            var halfColNeighbourhood = sizeofNeighbourhood / 2;
            var result = new List<List<FeatureVector>>();
            var Matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);
            var startRowIndex = MinRowIndex - halfExtendedFrequencyRange;
            var listCount = 0;

            //search along time position searchstep by searchstep. 
            for (int col = 0; col < colsCount; col += searchStep)
            {
                // option one for the box that is not enough for a entire box, just ignore this part
                // here, you need to check whether i
                var boxMaxColIndex = col + numberOfColSlices * sizeofNeighbourhood;
                if (!(boxMaxColIndex < colsCount))
                {
                    break;
                }
                result.Add(new List<FeatureVector>());
                listCount++;
                for (int sliceRowIndex = 0; sliceRowIndex < numberOfRowSlices; sliceRowIndex++)
                {
                    for (int sliceColIndex = 0; sliceColIndex < numberOfColSlices; sliceColIndex++)
                    {
                        var startRowIndexInSlice = startRowIndex + (sliceRowIndex * sizeofNeighbourhood);
                        var endRowIndexInSlice = startRowIndex + ((sliceRowIndex + 1) * sizeofNeighbourhood);
                        var startColIndexInSlice = col + (sliceColIndex * sizeofNeighbourhood);
                        var endColIndexInSlice = col + ((sliceColIndex + 1) * sizeofNeighbourhood);
                        if (StatisticalAnalysis.checkBoundary(endRowIndexInSlice, endColIndexInSlice, rowsCount, colsCount))
                        {
                            var subMatrix = StatisticalAnalysis.Submatrix(Matrix, startRowIndexInSlice, startColIndexInSlice, endRowIndexInSlice, endColIndexInSlice);
                            var centroidRowIndexInSlice = startRowIndexInSlice + halfRowNeighbourhood;
                            var centroidColIndexInSlice = startColIndexInSlice + halfColNeighbourhood;
                            var partialFeatureVector = RectangularRepresentation.SliceIntegerEdgeRepresentation(subMatrix, centroidRowIndexInSlice, centroidColIndexInSlice);
                            //            result[sliceRowIndex * sizeofNeighbourhood + sliceColIndex + col / searchStep].Add(new FeatureVector(new Point(row + halfRowNeighbourhood, col + halfColNeighbourhood))
                            result[listCount - 1].Add(new FeatureVector(new Point(centroidRowIndexInSlice, centroidColIndexInSlice))
                            {
                                HorizontalVector = partialFeatureVector.HorizontalVector,
                                VerticalVector = partialFeatureVector.VerticalVector,
                                PositiveDiagonalVector = partialFeatureVector.PositiveDiagonalVector,
                                NegativeDiagonalVector = partialFeatureVector.NegativeDiagonalVector,
                                TimePosition = col,

                            });
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
                        if ((matrix[anchorPoint.X + rowNeighbourhoodIndex, anchorPoint.Y + colNeighbourhoodIndex] != null) && matrix[anchorPoint.X + rowNeighbourhoodIndex, anchorPoint.Y + colNeighbourhoodIndex].OrientationCategory == (int)Direction.East)
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
                        if ((matrix[anchorPoint.X + colNeighbourhoodIndex, anchorPoint.Y + rowNeighbourhoodIndex] != null) && matrix[anchorPoint.X + colNeighbourhoodIndex, anchorPoint.Y + rowNeighbourhoodIndex].OrientationCategory == (int)Direction.North)
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
                        if ((matrix[anchorPoint.X + offsetIndex + offset, anchorPoint.Y + offsetIndex] != null) && (matrix[anchorPoint.X + offsetIndex + offset, anchorPoint.Y + offsetIndex].OrientationCategory == (int)Direction.NorthWest))
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
                        if ((matrix[anchorPoint.X + offsetIndex - offset, anchorPoint.Y + offsetIndex] != null) && (matrix[anchorPoint.X + offsetIndex - offset, anchorPoint.Y + offsetIndex].OrientationCategory == (int)Direction.NorthWest))
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
        public static List<FeatureVector> RepresentationForQuery(List<PointOfInterest> poiList, double maxFrequency, double minFrequency, double startTime,
                                                               double duration, int sizeofNeighbourhood, double herzScale, double timeScale,
                                                               double nyquistFrequency, int rowsCount, int colsCount)
        {
            var MaxRowIndex = (int)Math.Ceiling((nyquistFrequency - minFrequency) / herzScale);
            var MinRowIndex = (int)Math.Floor((nyquistFrequency - maxFrequency) / herzScale);
            var MinColIndex = (int)Math.Floor(startTime / timeScale);
            var MaxColIndex = (int)Math.Ceiling((startTime + duration) / timeScale);
            var extendedFrequencyRange = 0;
            var extendedTimeRange = 0;
            if ((MaxRowIndex - MinRowIndex) % sizeofNeighbourhood != 0)
            {
                extendedFrequencyRange = sizeofNeighbourhood - (MaxRowIndex - MinRowIndex) % sizeofNeighbourhood;
            }
            if ((MaxColIndex - MinColIndex) % sizeofNeighbourhood != 0)
            {
                extendedTimeRange = sizeofNeighbourhood - (MaxColIndex - MinColIndex) % sizeofNeighbourhood;
            }
            var halfExtendedFrequencyRange = extendedFrequencyRange / 2;
            var halfExtendedTimeRange = extendedTimeRange / 2;
            var halfRowNeighbourhood = sizeofNeighbourhood / 2;
            var halfColNeighbourhood = sizeofNeighbourhood / 2;
            var result = new List<FeatureVector>();
            var Matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);
            // search along the fixed frequency range.
            for (int row = MinRowIndex - halfExtendedFrequencyRange; row < MaxRowIndex + extendedFrequencyRange - halfExtendedFrequencyRange; row += sizeofNeighbourhood)
            //for (int row = 149; row < MaxRowIndex + extendedFrequencyRange - halfExtendedFrequencyRange; row += sizeofNeighbourhood)
            {
                // search along time position one by one. 
                //for (int col = 308; col < MaxColIndex + extendedTimeRange - halfExtendedTimeRange; col += sizeofNeighbourhood)
                for (int col = MinColIndex - halfExtendedTimeRange; col < MaxColIndex + extendedTimeRange - halfExtendedTimeRange; col += sizeofNeighbourhood)
                {
                    if (StatisticalAnalysis.checkBoundary(row + sizeofNeighbourhood, col + sizeofNeighbourhood, rowsCount, colsCount))
                    {
                        var subMatrix = StatisticalAnalysis.Submatrix(Matrix, row, col, row + sizeofNeighbourhood, col + sizeofNeighbourhood);
                        var partialFeatureVector = RectangularRepresentation.SliceIntegerEdgeRepresentation(subMatrix, row + halfRowNeighbourhood, col + halfColNeighbourhood);
                        result.Add(new FeatureVector(new Point(row + halfRowNeighbourhood, col + halfColNeighbourhood))
                        {
                            HorizontalVector = partialFeatureVector.HorizontalVector,
                            VerticalVector = partialFeatureVector.VerticalVector,
                            PositiveDiagonalVector = partialFeatureVector.PositiveDiagonalVector,
                            NegativeDiagonalVector = partialFeatureVector.NegativeDiagonalVector,
                            TimePosition = MinColIndex
                        });
                    }
                }
            }
            return result;
        }

        #endregion

    }
}
