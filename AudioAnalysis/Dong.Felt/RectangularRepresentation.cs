
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
            var MaxRowIndex = (int)((nyquistFrequency - minFrequency) / herzScale);
            var MinRowIndex = (int)((nyquistFrequency - maxFrequency) / herzScale);

            var extendedFrequencyRange = 0;
            if ((MaxRowIndex - MinRowIndex) % sizeofNeighbourhood != 0)
            {
                extendedFrequencyRange = sizeofNeighbourhood - (MaxRowIndex - MinRowIndex) % sizeofNeighbourhood;
            }
            var numberOfFrames = duration / timeScale;
            var halfExtendedFrequencyRange = extendedFrequencyRange / 2;
            var numberOfRowSlices = (int)Math.Ceiling((double)(MaxRowIndex - MinRowIndex) / sizeofNeighbourhood);
            var numberOfColSlices = 0;
            if (numberOfFrames % sizeofNeighbourhood != 0)
            {
                numberOfColSlices = (int)numberOfFrames / sizeofNeighbourhood + 1;
            }
            else
            { numberOfColSlices = (int)numberOfFrames / sizeofNeighbourhood; }
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
                            var partialFeatureVector = RectangularRepresentation.SliceEdgeRepresentation(subMatrix, centroidRowIndexInSlice, centroidColIndexInSlice);
                            //    var subMatrix1 = StatisticalAnalysis.Submatrix(Matrix, startRowIndexInSlice, startRowIndexInSlice + numberOfRowSlices * sizeofNeighbourhood, startColIndexInSlice, startColIndexInSlice + numberOfRowSlices * sizeofNeighbourhood);
                            //    var centroid = GetCentroid(subMatrix1);

                            result[listCount - 1].Add(new FeatureVector(new Point(centroidRowIndexInSlice, centroidColIndexInSlice))
                            {
                                HorizontalVector = partialFeatureVector.HorizontalVector,
                                VerticalVector = partialFeatureVector.VerticalVector,
                                PositiveDiagonalVector = partialFeatureVector.PositiveDiagonalVector,
                                NegativeDiagonalVector = partialFeatureVector.NegativeDiagonalVector,
                                TimePosition = col,
                                //Centroid = new Point(centroid.X, centroid.Y)
                            });
                        }
                    }
                }
            }

            return result;
        }


        public static List<List<FeatureVector>> MainSlopeRepresentationForIndexing(List<PointOfInterest> poiList, List<FeatureVector> query, int sizeofNeighbourhood, int rowsCount, int colsCount, int frameSearchStep, int frequencySearchStep, int frequencyOffset)
        {
            var result = new List<List<FeatureVector>>();
            if (query != null)
            {
                var maxRowIndex = query[0].MaxRowIndex;
                var minRowIndex = query[0].MinRowIndex;
                var numberOfRowSlices = (maxRowIndex - minRowIndex) / sizeofNeighbourhood;
                var minColIndex = query[0].MinColIndex;
                var maxColIndex = query[0].MaxColIndex;
                var numberOfColSlices = (maxColIndex - minColIndex) / sizeofNeighbourhood;

                var halfRowNeighbourhood = sizeofNeighbourhood / 2;
                var halfColNeighbourhood = sizeofNeighbourhood / 2;

                var Matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);
                var listCount = 0;
                //search along frequency band by frequencyOffset. 
                for (int row = minRowIndex - frequencyOffset; row < minRowIndex + frequencyOffset; row += frequencySearchStep)
                {
                    // search along time position by searchStep
                    for (int col = 0; col < colsCount; col += frameSearchStep)
                    {
                        // option one for the box that is not enough for a entire box, just ignore this part
                        // here, you need to check whether i
                        var boxMaxColIndex = col + numberOfColSlices * sizeofNeighbourhood;
                        var boxMaxRowIndex = row + numberOfRowSlices * sizeofNeighbourhood;
                        if (!(boxMaxColIndex < colsCount) || !(boxMaxRowIndex < rowsCount))
                        {
                            break;
                        }
                        result.Add(new List<FeatureVector>());
                        listCount++;
                        for (int sliceRowIndex = 0; sliceRowIndex < numberOfRowSlices; sliceRowIndex++)
                        {
                            for (int sliceColIndex = 0; sliceColIndex < numberOfColSlices; sliceColIndex++)
                            {
                                var startRowIndexInSlice = row + (sliceRowIndex * sizeofNeighbourhood);
                                var endRowIndexInSlice = row + ((sliceRowIndex + 1) * sizeofNeighbourhood);
                                var startColIndexInSlice = col + (sliceColIndex * sizeofNeighbourhood);
                                var endColIndexInSlice = col + ((sliceColIndex + 1) * sizeofNeighbourhood);
                                if (StatisticalAnalysis.checkBoundary(endRowIndexInSlice, endColIndexInSlice, rowsCount, colsCount))
                                {
                                    var subMatrix = StatisticalAnalysis.Submatrix(Matrix, startRowIndexInSlice, startColIndexInSlice, endRowIndexInSlice, endColIndexInSlice);
                                    var centroidRowIndexInSlice = startRowIndexInSlice + halfRowNeighbourhood;
                                    var centroidColIndexInSlice = startColIndexInSlice + halfColNeighbourhood;
                                    var partialFeatureVector = RectangularRepresentation.SliceEdgeRepresentation(subMatrix, centroidRowIndexInSlice, centroidColIndexInSlice);
                                    var slopeValue = RectangularRepresentation.SliceSlopeRepresentation(partialFeatureVector);

                                    result[listCount - 1].Add(new FeatureVector(new Point(centroidRowIndexInSlice, centroidColIndexInSlice))
                                    {
                                        Slope = new Tuple<int, int>(slopeValue.Item1, slopeValue.Item2),
                                        SlopeScore = slopeValue.Item1 * slopeValue.Item2,
                                        MinFrequency = 11025 - (row + numberOfRowSlices * sizeofNeighbourhood) * 43.0,
                                        MaxFrequency = 11025 - row * 43.0,
                                        TimePosition = col,
                                    });
                                }
                            }
                        }
                    }
                }                  
            }
            return result;
        }

        public static Point GetCentroid(PointOfInterest[,] subMatrix)
        {
            var rowMaxIndex = subMatrix.GetLength(0);
            var colMaxIndex = subMatrix.GetLength(1);
            var frequencyList = new List<int>();
            var frameList = new List<int>();

            for (int i = 0; i < rowMaxIndex; i++)
            {
                for (int j = 0; j < colMaxIndex; j++)
                {
                    if (subMatrix[i, j] != null && subMatrix[i, j].OrientationCategory < 10)
                    {
                        frequencyList.Add(subMatrix[i, j].Point.X);
                        frameList.Add(subMatrix[i, j].Point.Y);
                    }
                }
            }
            frequencyList.Sort();
            var maxFrequency = frequencyList[0];
            var minFrequency = frequencyList[frequencyList.Count - 1];

            frameList.Sort();
            var minFrame = frameList[0];
            var maxFrame = frameList[frameList.Count - 1];
            var centroid = new Point(Math.Abs(maxFrequency - minFrequency), Math.Abs(maxFrame - minFrame));
            return centroid;
        }

        public static Tuple<int, int> SliceSlopeRepresentation(FeatureVector slice)
        {
            var horizontalCount = OrientationValueCount(slice.HorizontalVector);
            var verticalCount = OrientationValueCount(slice.VerticalVector);
            var positiveDiagonalCount = OrientationValueCount(slice.PositiveDiagonalVector);
            var negativeDiagonalCount = OrientationValueCount(slice.NegativeDiagonalVector);
            var array = new int[4];
            array[0] = horizontalCount;
            array[1] = positiveDiagonalCount;
            array[2] = verticalCount;
            array[3] = negativeDiagonalCount;
            // maxValue ( slope Index, slope Count)
            var result = new Tuple<int, int>(0, 0);
            var tempMaxCount = 0; 
            var zero = 0;
            var slopeIndexOffset = 1;
            for (int i = 0; i < array.Length - 1; i++)
            {

                if (tempMaxCount < array[i])
                {
                    tempMaxCount = array[i];
                    result = Tuple.Create(i + slopeIndexOffset, array[i]);
                }
                else
                {
                    if (tempMaxCount == 0)
                    {
                        result = Tuple.Create(zero, array[i]);
                    }
                }
            }

            return result;
        }

        public static int OrientationValueCount(int[] orientationArray)
        {
            var arrayCount = orientationArray.Length;
            var result = 0;
            for (int i = 0; i < arrayCount; i++)
            {
                result += orientationArray[i];
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
        public static FeatureVector SliceEdgeRepresentation(PointOfInterest[,] matrix, int PointX, int PointY)
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
                    var startPointRowIndex = anchorPoint.X + offsetIndex + offset;
                    var startPointColIndex = anchorPoint.Y - offsetIndex;
                    var maxRowIndex = sizeOfNeighbourhood;
                    var maxColIndex = sizeOfNeighbourhood;
                    if (StatisticalAnalysis.checkBoundary(startPointRowIndex, startPointColIndex, maxRowIndex, maxColIndex))
                    {
                        // 
                        if ((matrix[startPointRowIndex, startPointColIndex] != null) && (matrix[startPointRowIndex, startPointColIndex].OrientationCategory == (int)Direction.NorthEast))
                        {
                            var index = sizeOfNeighbourhood - offset - 1;
                            positiveDiagonalDirection[index]++;
                        }
                    }
                }
            }
            for (int offset = 1; offset < sizeOfNeighbourhood; offset++)
            {
                for (int offsetIndex = -radiusOfNeighbourhood; offsetIndex <= radiusOfNeighbourhood; offsetIndex++)
                {
                    var startPointRowIndex = anchorPoint.X + offsetIndex - offset;
                    var startPointColIndex = anchorPoint.Y - offsetIndex;
                    var maxRowIndex = sizeOfNeighbourhood;
                    var maxColIndex = sizeOfNeighbourhood;
                    if (StatisticalAnalysis.checkBoundary(startPointRowIndex, startPointColIndex, sizeOfNeighbourhood, sizeOfNeighbourhood))
                    {
                        if ((matrix[startPointRowIndex, startPointColIndex] != null) && (matrix[startPointRowIndex, startPointColIndex].OrientationCategory == (int)Direction.NorthEast))
                        {
                            // here I minus one because I want to keep the index of array is in the range of array length.
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
        public static List<FeatureVector> SlopeRepresentationForQuery(List<PointOfInterest> poiList, double maxFrequency, double minFrequency, double startTime,
                                                               double duration, int sizeofNeighbourhood, double herzScale, double timeScale,
                                                               double nyquistFrequency, int rowsCount, int colsCount)
        {
            var MaxRowIndex = (int)((nyquistFrequency - minFrequency) / herzScale);
            var MinRowIndex = (int)((nyquistFrequency - maxFrequency) / herzScale);
            var MinColIndex = (int)(startTime / timeScale);
            var MaxColIndex = (int)((startTime + duration) / timeScale);
            var extendedFrequencyRange = 0;
            var extendedTimeRange = 0;
            if ((MaxRowIndex - MinRowIndex) % sizeofNeighbourhood != 0)
            {
                extendedFrequencyRange = sizeofNeighbourhood - (MaxRowIndex - MinRowIndex) % sizeofNeighbourhood;
            }
            if ((MaxColIndex - MinColIndex) % sizeofNeighbourhood != 0)
            {
                extendedTimeRange = sizeofNeighbourhood - (int)(MaxColIndex - MinColIndex) % sizeofNeighbourhood;
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
                        var partialFeatureVector = RectangularRepresentation.SliceEdgeRepresentation(subMatrix, row + halfRowNeighbourhood, col + halfColNeighbourhood);
                        var startRowIndex = MinRowIndex - halfExtendedFrequencyRange;
                        var startColIndex = MinColIndex - halfExtendedTimeRange;
                        result.Add(new FeatureVector(new Point(row + halfRowNeighbourhood, col + halfColNeighbourhood))
                        {
                            HorizontalVector = partialFeatureVector.HorizontalVector,
                            VerticalVector = partialFeatureVector.VerticalVector,
                            PositiveDiagonalVector = partialFeatureVector.PositiveDiagonalVector,
                            NegativeDiagonalVector = partialFeatureVector.NegativeDiagonalVector,
                            TimePosition = MinColIndex,
                            
                        });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// This representation for query is done by calculating the major slope property. which means, we can derive the main oriention in each slice.
        /// </summary>
        /// <param name="poiList"></param>
        /// <param name="maxFrequency"></param>
        /// <param name="minFrequency"></param>
        /// <param name="startTime"></param>
        /// <param name="duration"></param>
        /// <param name="sizeofNeighbourhood"></param>
        /// <param name="herzScale"></param>
        /// <param name="timeScale"></param>
        /// <param name="nyquistFrequency"></param>
        /// <param name="rowsCount"></param>
        /// <param name="colsCount"></param>
        /// <returns></returns>
        public static List<FeatureVector> MainSlopeRepresentationForQuery(List<PointOfInterest> poiList, double maxFrequency, double minFrequency, double startTime,
                                                               double duration, int sizeofNeighbourhood, double herzScale, double timeScale,
                                                               double nyquistFrequency, int rowsCount, int colsCount)
        {
            var MaxRowIndex = (int)((nyquistFrequency - minFrequency) / herzScale);
            var MinRowIndex = (int)((nyquistFrequency - maxFrequency) / herzScale);
            var MinColIndex = (int)(startTime / timeScale);
            var MaxColIndex = (int)((startTime + duration) / timeScale);
            var extendedFrequencyRange = 0;
            var extendedTimeRange = 0;
            if ((MaxRowIndex - MinRowIndex) % sizeofNeighbourhood != 0)
            {
                extendedFrequencyRange = sizeofNeighbourhood - (MaxRowIndex - MinRowIndex) % sizeofNeighbourhood;
            }
            if ((MaxColIndex - MinColIndex) % sizeofNeighbourhood != 0)
            {
                extendedTimeRange = sizeofNeighbourhood - (int)(MaxColIndex - MinColIndex) % sizeofNeighbourhood;
            }
            var halfExtendedFrequencyRange = extendedFrequencyRange / 2;
            var halfExtendedTimeRange = extendedTimeRange / 2;
            var halfRowNeighbourhood = sizeofNeighbourhood / 2;
            var halfColNeighbourhood = sizeofNeighbourhood / 2;

            var result = new List<FeatureVector>();
            var Matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);
            // search along the fixed frequency range.
            for (int row = MinRowIndex - halfExtendedFrequencyRange; row < MaxRowIndex + extendedFrequencyRange - halfExtendedFrequencyRange; row += sizeofNeighbourhood)          
            {
                for (int col = MinColIndex - halfExtendedTimeRange; col < MaxColIndex + extendedTimeRange - halfExtendedTimeRange; col += sizeofNeighbourhood)
                {
                    if (StatisticalAnalysis.checkBoundary(row + sizeofNeighbourhood, col + sizeofNeighbourhood, rowsCount, colsCount))
                    {
                        var subMatrix = StatisticalAnalysis.Submatrix(Matrix, row, col, row + sizeofNeighbourhood, col + sizeofNeighbourhood);
                        var partialFeatureVector = RectangularRepresentation.SliceEdgeRepresentation(subMatrix, row + halfRowNeighbourhood, col + halfColNeighbourhood);
                        var slopeValue = RectangularRepresentation.SliceSlopeRepresentation(partialFeatureVector);
                        var startRowIndex = MinRowIndex - halfExtendedFrequencyRange;
                        var startColIndex = MinColIndex - halfExtendedTimeRange;
                        result.Add(new FeatureVector(new Point(row + halfRowNeighbourhood, col + halfColNeighbourhood))
                        {
                            Slope = new Tuple<int, int>(slopeValue.Item1, slopeValue.Item2),
                            SlopeScore = slopeValue.Item1 * slopeValue.Item2,
                            TimePosition = MinColIndex,
                            MinRowIndex = MinRowIndex - halfExtendedFrequencyRange,
                            MaxRowIndex = MaxRowIndex + extendedFrequencyRange - halfExtendedFrequencyRange,
                            MinColIndex = MinColIndex - halfExtendedTimeRange,
                            MaxColIndex = MaxColIndex + extendedTimeRange - halfExtendedTimeRange,
                        });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// This improved rectangular representation is done by smoothing the slope values, e.g. 0 5 0  -> 1 3 1
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static List<FeatureVector> ImprovedQueryFeatureVector(List<FeatureVector> query)
        {
            var result = new List<FeatureVector>();

            if (query != null)
            {
                var numberOfHorizontalBit = query[0].HorizontalVector.Count();
                var numberOfDiagonalBit = query[0].PositiveDiagonalVector.Count();
                var listCount = query.Count();
                for (int listIndex = 0; listIndex < listCount; listIndex++)
                {
                    for (int i = 0; i < numberOfHorizontalBit; i++)
                    {
                        if (query[listIndex].HorizontalVector[i] > 1)
                        {
                            var temp = query[listIndex].HorizontalVector[i];
                            if ((i - 1) >= 0 && (i + 1) < numberOfHorizontalBit && query[listIndex].HorizontalVector[i - 1] == 0 && query[listIndex].HorizontalVector[i + 1] == 0)
                            {
                                if (temp % 2 == 0)
                                {
                                    query[listIndex].HorizontalVector[i] = temp / 2;
                                    query[listIndex].HorizontalVector[i - 1] = temp - query[listIndex].HorizontalVector[i];
                                    query[listIndex].HorizontalVector[i + 1] = query[listIndex].HorizontalVector[i - 1];
                                }
                                else
                                {
                                    query[listIndex].HorizontalVector[i] = temp / 2 + 1;
                                    query[listIndex].HorizontalVector[i - 1] = temp - query[listIndex].HorizontalVector[i];
                                    query[listIndex].HorizontalVector[i + 1] = query[listIndex].HorizontalVector[i - 1];
                                }
                            }
                        }
                        if (query[listIndex].VerticalVector[i] > 1)
                        {
                            var temp = query[listIndex].VerticalVector[i];
                            if ((i - 1) >= 0 && (i + 1) < numberOfHorizontalBit && query[listIndex].HorizontalVector[i - 1] == 0 && query[listIndex].HorizontalVector[i + 1] == 0)
                            {
                                if (temp % 2 == 0)
                                {
                                    query[listIndex].VerticalVector[i] = temp / 2;
                                    query[listIndex].VerticalVector[i - 1] = temp - query[listIndex].VerticalVector[i];
                                    query[listIndex].VerticalVector[i + 1] = query[listIndex].VerticalVector[i - 1];
                                }
                                else
                                {
                                    query[listIndex].VerticalVector[i] = temp / 2 + 1;
                                    query[listIndex].VerticalVector[i - 1] = temp - query[listIndex].VerticalVector[i];
                                    query[listIndex].VerticalVector[i + 1] = query[listIndex].VerticalVector[i - 1];
                                } // end else
                            }// end if
                        } // end if
                    } // end for

                    for (int i = 0; i < numberOfDiagonalBit; i++)
                    {
                        if (query[listIndex].PositiveDiagonalVector[i] > 1)
                        {
                            var temp = query[listIndex].PositiveDiagonalVector[i];
                            if ((i - 1) >= 0 && (i + 1) < numberOfDiagonalBit && query[listIndex].PositiveDiagonalVector[i - 1] == 0 && query[listIndex].PositiveDiagonalVector[i + 1] == 0)
                            {
                                if (temp % 2 == 0)
                                {
                                    query[listIndex].PositiveDiagonalVector[i] = temp / 2;
                                    query[listIndex].PositiveDiagonalVector[i - 1] = temp - query[listIndex].PositiveDiagonalVector[i];
                                    query[listIndex].PositiveDiagonalVector[i + 1] = query[listIndex].PositiveDiagonalVector[i - 1];
                                }
                                else
                                {
                                    query[listIndex].PositiveDiagonalVector[i] = temp / 2 + 1;
                                    query[listIndex].PositiveDiagonalVector[i - 1] = temp - query[listIndex].PositiveDiagonalVector[i];
                                    query[listIndex].PositiveDiagonalVector[i + 1] = query[listIndex].PositiveDiagonalVector[i - 1];
                                }
                            }
                        }
                        if (query[listIndex].NegativeDiagonalVector[i] > 1)
                        {
                            var temp = query[listIndex].NegativeDiagonalVector[i];
                            if ((i - 1) >= 0 && (i + 1) < numberOfDiagonalBit && query[listIndex].NegativeDiagonalVector[i - 1] == 0 && query[listIndex].NegativeDiagonalVector[i + 1] == 0)
                            {
                                if (temp % 2 == 0)
                                {
                                    query[listIndex].NegativeDiagonalVector[i] = temp / 2;
                                    query[listIndex].NegativeDiagonalVector[i - 1] = temp - query[listIndex].VerticalVector[i];
                                    query[listIndex].NegativeDiagonalVector[i + 1] = query[listIndex].VerticalVector[i - 1];
                                }
                                else
                                {
                                    query[listIndex].NegativeDiagonalVector[i] = temp / 2 + 1;
                                    query[listIndex].NegativeDiagonalVector[i - 1] = temp - query[listIndex].NegativeDiagonalVector[i];
                                    query[listIndex].NegativeDiagonalVector[i + 1] = query[listIndex].NegativeDiagonalVector[i - 1];
                                } // end else
                            }// end if
                        } // end if
                    } // end for

                } // end for


            }// end if
            result = query;
            return result;
        }
        #endregion

    }
}
