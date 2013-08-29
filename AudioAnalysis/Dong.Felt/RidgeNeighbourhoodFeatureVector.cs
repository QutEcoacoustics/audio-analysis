namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AudioAnalysisTools;
    using System.Drawing;
    using TowseyLib;

    /// <summary>
    /// A class for generating RidgeNeighbourhoodFeatureVector for decribing the acoustic events(bird calls).
    /// </summary>   
    public class RidgeNeighbourhoodFeatureVector
    {
        #region Public Properties

        public int MinRowIndex { get; set; }

        public int MaxRowIndex { get; set; }

        public int MinColIndex { get; set; }

        public int MaxColIndex { get; set; }

        public double MinFrequency { get; set; }

        public double MaxFrequency { get; set; }

        public double duration { get; set; }

        public int frequencyRange { get; set; }

        public int neighbourhoodLength { get; set; }

        public int neighbourhoodWidth { get; set; }

        // the first value means the orentation type, and the second values means the count of poi with this orentation type. 
        public Tuple<int, int> Slope { get; set; }

        // Gets or sets the orientation type 
        public int OrientationType { get; set; }

        public int poiatParticularOrientationCount { get; set; }

        public int SlopeScore { get; set; }

        /// <summary>
        /// Gets or sets the HorizontalByteVector, part of a composite  edge RidgeNeighbourhoodFeatureVector, representing the horizontal direction of edge(one kind of feature). 
        /// </summary>
        public int[] HorizontalVector { get; set; }

        /// <summary>
        /// Gets or sets the HorizontalBitVector, part of a composite  edge RidgeNeighbourhoodFeatureVector, representing the horizontal direction of edge(one kind of feature). 
        /// </summary>
        public int[] HorizontalBitVector { get; set; }

        /// <summary>
        /// Gets or sets the HorizontalBitVector, part of a composite  edge RidgeNeighbourhoodFeatureVector, representing the horizontal direction of edge(one kind of feature). 
        /// </summary>
        public double[] HorizontalFractionVector { get; set; }

        /// <summary>
        /// Gets or sets the VerticalBitVector, part of composite RidgeNeighbourhoodFeatureVector, representing the vertital direction of edge(one kind of feature).
        /// </summary>
        public int[] VerticalVector { get; set; }

        /// <summary>
        /// Gets or sets the VerticalBitVector, part of a composite  RidgeNeighbourhoodFeatureVector, representing the vertital direction of edge(one kind of feature). 
        /// </summary>
        public int[] VerticalBitVector { get; set; }

        /// <summary>
        /// Gets or sets the VerticalBitVector, part of a composite  RidgeNeighbourhoodFeatureVector, representing the vertital direction of edge(one kind of feature). 
        /// </summary>
        public double[] VerticalFractionVector { get; set; }

        /// <summary>
        /// Gets or sets the PositiveDiagonalByteVector, part of composite RidgeNeighbourhoodFeatureVector, representing the NorthEast direction of edge(one kind of feature).
        /// </summary>
        public int[] PositiveDiagonalVector { get; set; }

        /// <summary>
        /// Gets or sets the PositiveDiagonalByteVector, part of composite RidgeNeighbourhoodFeatureVector, representing the NorthEast direction of edge(one kind of feature).
        /// </summary>
        public int[] PositiveDiagonalBitVector { get; set; }

        /// <summary>
        /// Gets or sets the PositiveDiagonalBitVector, part of a composite  RidgeNeighbourhoodFeatureVector, representing the NorthEast direction of edge(one kind of feature). 
        /// </summary>
        public double[] PositiveDiagonalFractionVector { get; set; }

        /// <summary>
        /// Gets or sets the NegativeDiagonalByteVector, part of composite edge feature vector, representing the NorthWest direction of edge(one kind of feature).
        /// </summary>
        public int[] NegativeDiagonalVector { get; set; }

        /// <summary>
        /// Gets or sets the NegativeDiagonalByteVector, part of composite edge feature vector, representing the NorthWest direction of edge(one kind of feature).
        /// </summary>
        public int[] NegativeDiagonalBitVector { get; set; }

        /// <summary>
        /// Gets or sets the NegativeDiagonalBitVector, part of a composite  RidgeNeighbourhoodFeatureVector, representing the NorthWest direction of edge(one kind of feature). 
        /// </summary>
        public double[] NegativeDiagonalFractionVector { get; set; }

        /// <summary>
        /// Gets or sets the anchor point of search neighbourhood. 
        /// </summary>
        public Point Point { get; set; }

        /// <summary>
        /// Gets or sets the percentageByteVector, another type of edge feature vector, representing the percentage of each direction of edge account for. 
        /// Especially, it [0], horizontal, [1], vertical, [2], positiveDiagonal, [3], negativeDiagonal.
        /// </summary>
        public double HorizontalPercentage { get; set; }

        public double VerticalPercentage { get; set; }

        public double PositiveDiagonalPercentage { get; set; }

        public double NegativeDiagonalPercentage { get; set; }

        /// <summary>
        /// Gets or sets the similarityScore
        /// </summary>
        //public double SmilarityScore { get; set; }

        //public double Intensity { get; set; }

        public Point Centroid { get; set; }

        public int TimePositionPix { get; set; }

        /// <summary>
        /// to keep the time position in the long audio file for calculating the representation for this position. 
        /// </summary>
        public double TimePosition_TopLeft { get; set; }

        /// <summary>
        /// to keep the frequencyband for calculating the representation for this position. 
        /// </summary>
        public double FrequencyBand_TopLeft { get; set; }

        #region constructor
       
        /// <summary>
        /// A constructor takes in point
        /// </summary>
        /// <param name="pointofInterest"></param>
        public RidgeNeighbourhoodFeatureVector(Point point)
        {
            Point = point;
        }
        
        #endregion constructor

        #endregion public properties


        #region Public Method

        /// <summary>
        /// This method uses another diagonal orientation edge representation.  And e.g. the neighbourhoodWindow size is 13 * 13, then the feature vector can 
        /// be up to 13(vertical edge) + 13(horizontal edge) + 25 (positiveDiagonal edge) + 25 (negativeDiagonal edge).
        /// </summary>
        /// <param name="poiList"></param>
        /// <param name="rowsCount"></param>
        /// <param name="colsCount"></param>
        /// <param name="sizeOfNeighbourhood"></param>
        /// <returns></returns>
        public static List<RidgeNeighbourhoodFeatureVector> IntegerEdgeOrientationRidgeNeighbourhoodFeatureVectors2(List<PointOfInterest> poiList, int rowsCount, int colsCount, int sizeOfNeighbourhood)
        {
            var result = new List<RidgeNeighbourhoodFeatureVector>();

            // To search in a neighbourhood, the original pointsOfInterst should be converted into a pointOfInterst of Matrix
            var Matrix = StructureTensorTest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);
            var radiusOfNeighbourhood = sizeOfNeighbourhood / 2;
            // limite the frequency rang
            for (int row = 12; row < rowsCount; row++)
            {
                for (int col = 12; col < colsCount; col++)
                {
                    if (Matrix[row, col] == null)
                    {
                        continue;
                    }
                    else
                    {
                        // search from the anchor point among pointOfInterest, in the centroid of neighbourhood, and then search its Neighbourhood to get RidgeNeighbourhoodFeatureVector
                        var verticalDirection = new int[sizeOfNeighbourhood];
                        var horizontalDirection = new int[sizeOfNeighbourhood];
                        var positiveDiagonalDirection = new int[2 * sizeOfNeighbourhood - 1];
                        var negativeDiagonalDirection = new int[2 * sizeOfNeighbourhood - 1];

                        // For the calculation of horizontal direction byte, we need to check each row 
                        for (int rowNeighbourhoodIndex = -radiusOfNeighbourhood; rowNeighbourhoodIndex <= radiusOfNeighbourhood; rowNeighbourhoodIndex++)
                        {
                            for (int colNeighbourhoodIndex = -radiusOfNeighbourhood; colNeighbourhoodIndex <= radiusOfNeighbourhood; colNeighbourhoodIndex++)
                            {
                                // check boundary of index 
                                if (StatisticalAnalysis.checkBoundary(row + rowNeighbourhoodIndex, col + colNeighbourhoodIndex, rowsCount, colsCount))
                                {
                                    if ((Matrix[row + rowNeighbourhoodIndex, col + colNeighbourhoodIndex] != null) && Matrix[row + rowNeighbourhoodIndex, col + colNeighbourhoodIndex].OrientationCategory == (int)Direction.East)
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
                                if (StatisticalAnalysis.checkBoundary(row + colNeighbourhoodIndex, col + rowNeighbourhoodIndex, rowsCount, colsCount))
                                {
                                    if ((Matrix[row + colNeighbourhoodIndex, col + rowNeighbourhoodIndex] != null) && Matrix[row + colNeighbourhoodIndex, col + rowNeighbourhoodIndex].OrientationCategory == (int)Direction.North)
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
                                if (StatisticalAnalysis.checkBoundary(row + offsetIndex + offset, col + offsetIndex, rowsCount - row + radiusOfNeighbourhood, colsCount - col + radiusOfNeighbourhood, rowsCount - row - radiusOfNeighbourhood - 1, colsCount - col - radiusOfNeighbourhood - 1))
                                {
                                    if ((Matrix[row + offsetIndex + offset, col + offsetIndex] != null) && (Matrix[row + offsetIndex + offset, col + offsetIndex].OrientationCategory == (int)Direction.NorthWest))
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
                                if (StatisticalAnalysis.checkBoundary(row + offsetIndex - offset, col + offsetIndex, rowsCount - row + radiusOfNeighbourhood, colsCount - col + radiusOfNeighbourhood, rowsCount - row - radiusOfNeighbourhood - 1, colsCount - col - radiusOfNeighbourhood - 1))
                                {
                                    if ((Matrix[row + offsetIndex - offset, col + offsetIndex] != null) && (Matrix[row + offsetIndex - offset, col + offsetIndex].OrientationCategory == (int)Direction.NorthWest))
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
                                if (StatisticalAnalysis.checkBoundary(row + offsetIndex, col - offsetIndex - offset, rowsCount - row + radiusOfNeighbourhood, colsCount - col + radiusOfNeighbourhood, rowsCount - row - radiusOfNeighbourhood - 1, colsCount - col - radiusOfNeighbourhood - 1))
                                {
                                    if ((Matrix[row + offsetIndex, col - offsetIndex - offset] != null) && (Matrix[row + offsetIndex, col - offsetIndex - offset].OrientationCategory == (int)Direction.NorthEast))
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
                                if (StatisticalAnalysis.checkBoundary(row + offsetIndex, col - offsetIndex + offset, rowsCount - row + radiusOfNeighbourhood, colsCount - col + radiusOfNeighbourhood, rowsCount - row - radiusOfNeighbourhood - 1, colsCount - col - radiusOfNeighbourhood - 1))
                                {
                                    if ((Matrix[row + offsetIndex, col - offsetIndex + offset] != null) && (Matrix[row + offsetIndex, col - offsetIndex + offset].OrientationCategory == (int)Direction.NorthEast))
                                    {
                                        positiveDiagonalDirection[sizeOfNeighbourhood + offset - 1]++;
                                    }
                                }
                            }
                        }
                        
                        result.Add(new RidgeNeighbourhoodFeatureVector(new Point(row, col))
                        {
                            HorizontalVector = horizontalDirection,
                            VerticalVector = verticalDirection,
                            PositiveDiagonalVector = positiveDiagonalDirection,
                            NegativeDiagonalVector = negativeDiagonalDirection
                        });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// The method of DirectionByteRidgeNeighbourhoodFeatureVectors can be used to generate integer directionRidgeNeighbourhoodFeatureVectors, it include 13 * 13 values for a 
        /// 13 * 13 neighbourhoodsize.
        /// </summary>
        /// <param name="poiList"> pointsOfInterest to be used to calculate the DirectionByteRidgeNeighbourhoodFeatureVector.</param>
        /// <param name="rowsCount"> the column count of original spectrogram. </param> 
        /// <param name="colsCount"> the row count of original spectrogram. </param>
        /// <param name="sizeOfNeighbourhood"> 
        /// the size of Neighbourhood will determine the size of search area.</param>
        /// <returns>
        /// It will return a list of RidgeNeighbourhoodFeatureVector objects whose DirectionByteRidgeNeighbourhoodFeatureVectors have been assigned, this can be used for similarity matching. 
        /// </returns>
        public static List<RidgeNeighbourhoodFeatureVector> IntegerEdgeOrientationRidgeNeighbourhoodFeatureVectors(List<PointOfInterest> poiList, int rowsCount, int colsCount, int sizeOfNeighbourhood)
        {
            var result = new List<RidgeNeighbourhoodFeatureVector>();

            // To search in a neighbourhood, the original pointsOfInterst should be converted into a pointOfInterst of Matrix
            var Matrix = StructureTensorTest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);
            var radiusOfNeighbourhood = sizeOfNeighbourhood / 2;
            // limite the frequency rang
            for (int row = 12; row < rowsCount; row++)
            {
                for (int col = 12; col < colsCount; col++)
                {
                    if (Matrix[row, col] == null)
                    {
                        continue;
                    }
                    else
                    {
                        // search from the anchor point among pointOfInterest, in the centroid of neighbourhood, and then search its Neighbourhood to get RidgeNeighbourhoodFeatureVector
                        var verticalDirection = new int[sizeOfNeighbourhood];
                        var horizontalDirection = new int[sizeOfNeighbourhood];
                        var positiveDiagonalDirection = new int[sizeOfNeighbourhood];
                        var negativeDiagonalDirection = new int[sizeOfNeighbourhood];

                        // For the calculation of horizontal direction byte, we need to check each row 
                        for (int rowNeighbourhoodIndex = -radiusOfNeighbourhood; rowNeighbourhoodIndex <= radiusOfNeighbourhood; rowNeighbourhoodIndex++)
                        {
                            for (int colNeighbourhoodIndex = -radiusOfNeighbourhood; colNeighbourhoodIndex <= radiusOfNeighbourhood; colNeighbourhoodIndex++)
                            {
                                // check boundary of index 
                                if (StatisticalAnalysis.checkBoundary(row + rowNeighbourhoodIndex, col + colNeighbourhoodIndex, rowsCount, colsCount))
                                {
                                    if ((Matrix[row + rowNeighbourhoodIndex, col + colNeighbourhoodIndex] != null) && Matrix[row + rowNeighbourhoodIndex, col + colNeighbourhoodIndex].OrientationCategory == (int)Direction.East)
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
                                if (StatisticalAnalysis.checkBoundary(row + colNeighbourhoodIndex, col + rowNeighbourhoodIndex, rowsCount, colsCount))
                                {
                                    if ((Matrix[row + colNeighbourhoodIndex, col + rowNeighbourhoodIndex] != null) && Matrix[row + colNeighbourhoodIndex, col + rowNeighbourhoodIndex].OrientationCategory == (int)Direction.North)
                                    {
                                        verticalDirection[rowNeighbourhoodIndex + radiusOfNeighbourhood]++;
                                    }
                                }
                            }
                        }
                        // For the calculation of positive diagonal direction byte, we need to check each diagnal column
                        for (int offsetIndex = -radiusOfNeighbourhood; offsetIndex <= radiusOfNeighbourhood; offsetIndex++)
                        {
                            for (int NeighbourhoodIndex = -radiusOfNeighbourhood; NeighbourhoodIndex <= radiusOfNeighbourhood; NeighbourhoodIndex++)
                            {
                                if (StatisticalAnalysis.checkBoundary(row + offsetIndex + NeighbourhoodIndex, col + offsetIndex - NeighbourhoodIndex, rowsCount, colsCount))
                                {
                                    if ((Matrix[row + offsetIndex + NeighbourhoodIndex, col + offsetIndex - NeighbourhoodIndex] != null) && Matrix[row + offsetIndex + NeighbourhoodIndex, col + offsetIndex - NeighbourhoodIndex].OrientationCategory == (int)Direction.NorthEast)
                                    {
                                        positiveDiagonalDirection[offsetIndex + radiusOfNeighbourhood]++;
                                    }
                                }
                            }
                        }

                        // For the calculation of negative diagonal direction byte, we need to check each diagnal column
                        for (int offsetIndex = -radiusOfNeighbourhood; offsetIndex <= radiusOfNeighbourhood; offsetIndex++)
                        {
                            for (int NeighbourhoodIndex = -radiusOfNeighbourhood; NeighbourhoodIndex <= radiusOfNeighbourhood; NeighbourhoodIndex++)
                            {
                                if (StatisticalAnalysis.checkBoundary(row - offsetIndex + NeighbourhoodIndex, col + offsetIndex + NeighbourhoodIndex, rowsCount, colsCount))
                                {
                                    if ((Matrix[row - offsetIndex + NeighbourhoodIndex, col + offsetIndex + NeighbourhoodIndex] != null) && Matrix[row - offsetIndex + NeighbourhoodIndex, col + offsetIndex + NeighbourhoodIndex].OrientationCategory == (int)Direction.NorthWest)
                                    {
                                        negativeDiagonalDirection[offsetIndex + radiusOfNeighbourhood]++;
                                    }
                                }
                            }
                        }

                        result.Add(new RidgeNeighbourhoodFeatureVector(new Point(row, col))
                        {
                            HorizontalVector = horizontalDirection,
                            VerticalVector = verticalDirection,
                            PositiveDiagonalVector = positiveDiagonalDirection,
                            NegativeDiagonalVector = negativeDiagonalDirection
                        });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Normalize the values in the feature Vector to the range (0,1) by taking a integer array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns>
        /// return a double array which is made up with decimals between 0 and 1.  
        /// </returns>
        public static double[] NormalizedRidgeNeighbourhoodFeatureVector(int[] array)
        {
            var histogramCount = array.Count();
            var result = new double[histogramCount];
            for (int i = 0; i < histogramCount; i++)
            {
                result[i] = array[i] / (double)histogramCount;
            }

            return result;
        }

        /// <summary>
        /// In order to make the comparison of feature vectors much more easier, we can set up a threshold to generate a bit fecture vector which is composed of     
        /// and 1. 
        /// </summary>
        /// <param name="RidgeNeighbourhoodFeatureVectorList"></param>
        /// <returns> returns a list RidgeNeighbourhoodFeatureVector which contains the direction bit Feature vector
        /// </returns>
        public static List<RidgeNeighbourhoodFeatureVector> DirectionBitRidgeNeighbourhoodFeatureVectors(List<RidgeNeighbourhoodFeatureVector> RidgeNeighbourhoodFeatureVectorList)
        {
            var horizontalBitVectorCount = RidgeNeighbourhoodFeatureVectorList[0].HorizontalBitVector.Count();
            var verticalBitVectorCount = RidgeNeighbourhoodFeatureVectorList[0].VerticalBitVector.Count();

            var thresholdForBitVector = 0.2;
            foreach (var fv in RidgeNeighbourhoodFeatureVectorList)
            {
                var normalizedHorizontalBitVector = NormalizedRidgeNeighbourhoodFeatureVector(fv.HorizontalVector);
                var normalizedVerticalBitVector = NormalizedRidgeNeighbourhoodFeatureVector(fv.VerticalVector);
                var normalizedPositiveDiagonalVector = NormalizedRidgeNeighbourhoodFeatureVector(fv.PositiveDiagonalVector);
                var normalizedNegativeDiagonalVector = NormalizedRidgeNeighbourhoodFeatureVector(fv.NegativeDiagonalVector);
                for (int index = 0; index < horizontalBitVectorCount; index++)
                {
                    if (normalizedHorizontalBitVector[index] > thresholdForBitVector)
                    {
                        fv.HorizontalBitVector[index] = 1;
                    }
                    else
                    {
                        fv.HorizontalBitVector[index] = 0;
                    }

                    if (normalizedVerticalBitVector[index] > thresholdForBitVector)
                    {
                        fv.VerticalBitVector[index] = 1;
                    }
                    else
                    {
                        fv.VerticalBitVector[index] = 0;
                    }

                    if (normalizedPositiveDiagonalVector[index] > thresholdForBitVector)
                    {
                        fv.PositiveDiagonalBitVector[index] = 1;
                    }
                    else
                    {
                        fv.PositiveDiagonalBitVector[index] = 0;
                    }

                    if (normalizedPositiveDiagonalVector[index] > thresholdForBitVector)
                    {
                        fv.NegativeDiagonalBitVector[index] = 1;
                    }
                    else
                    {
                        fv.NegativeDiagonalBitVector[index] = 0;
                    }
                }
            }

            return RidgeNeighbourhoodFeatureVectorList;
        }

        /// <summary>
        /// The method of DirectionByteRidgeNeighbourhoodFeatureVectors can be used to generate directionFractionRidgeNeighbourhoodFeatureVectors, which means it includes sub-feature vector 
        /// for each direction. And the size of each sub-RidgeNeighbourhoodFeatureVector is determined by sizeOfNeighbourhood. Especially, each value in the feature vector is a fraction.
        /// </summary>
        /// <param name="poiList"></param>
        /// <param name="rowsCount"></param>
        /// <param name="colsCount"></param>
        /// <param name="sizeOfNeighbourhood"></param>
        /// <returns></returns>
        public static List<RidgeNeighbourhoodFeatureVector> FractionDirectionRidgeNeighbourhoodFeatureVectors(List<PointOfInterest> poiList, int rowsCount, int colsCount, int sizeOfNeighbourhood)
        {
            var result = new List<RidgeNeighbourhoodFeatureVector>();

            // To search in a neighbourhood, the original pointsOfInterst should be converted into a pointOfInterst of Matrix
            var Matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);
            var radiusOfNeighbourhood = sizeOfNeighbourhood / 2;
            // limite the frequency rang
            for (int row = 116; row < 140; row++)
            {
                for (int col = 0; col < colsCount; col++)
                {
                    if (Matrix[row, col] == null)
                    {
                        continue;
                    }
                    else
                    {
                        // search from the anchor point among pointOfInterest, in the centroid of neighbourhood, and then search its Neighbourhood to get RidgeNeighbourhoodFeatureVector
                        var verticalDirection = new int[sizeOfNeighbourhood];
                        var horizontalDirection = new int[sizeOfNeighbourhood];
                        var positiveDiagonalDirection = new int[sizeOfNeighbourhood];
                        var negativeDiagonalDirection = new int[sizeOfNeighbourhood];

                        // For the calculation of horizontal direction byte, we need to check each row 
                        for (int rowNeighbourhoodIndex = -radiusOfNeighbourhood; rowNeighbourhoodIndex <= radiusOfNeighbourhood; rowNeighbourhoodIndex++)
                        {
                            for (int colNeighbourhoodIndex = -radiusOfNeighbourhood; colNeighbourhoodIndex <= radiusOfNeighbourhood; colNeighbourhoodIndex++)
                            {
                                // check boundary of index 
                                if (StatisticalAnalysis.checkBoundary(row + rowNeighbourhoodIndex, col + colNeighbourhoodIndex, rowsCount, colsCount))
                                {
                                    if ((Matrix[row + rowNeighbourhoodIndex, col + colNeighbourhoodIndex] != null) && Matrix[row + rowNeighbourhoodIndex, col + colNeighbourhoodIndex].OrientationCategory == (int)Direction.East)
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
                                if (StatisticalAnalysis.checkBoundary(row + colNeighbourhoodIndex, col + rowNeighbourhoodIndex, rowsCount, colsCount))
                                {
                                    if ((Matrix[row + colNeighbourhoodIndex, col + rowNeighbourhoodIndex] != null) && Matrix[row + colNeighbourhoodIndex, col + rowNeighbourhoodIndex].OrientationCategory == (int)Direction.North)
                                    {
                                        verticalDirection[rowNeighbourhoodIndex + radiusOfNeighbourhood]++;
                                    }
                                }
                            }
                        }
                        // For the calculation of positive diagonal direction byte, we need to check each diagnal column
                        for (int offsetIndex = -radiusOfNeighbourhood; offsetIndex <= radiusOfNeighbourhood; offsetIndex++)
                        {
                            for (int NeighbourhoodIndex = -radiusOfNeighbourhood; NeighbourhoodIndex <= radiusOfNeighbourhood; NeighbourhoodIndex++)
                            {
                                if (StatisticalAnalysis.checkBoundary(row + offsetIndex + NeighbourhoodIndex, col + offsetIndex - NeighbourhoodIndex, rowsCount, colsCount))
                                {
                                    if ((Matrix[row + offsetIndex + NeighbourhoodIndex, col + offsetIndex - NeighbourhoodIndex] != null) && Matrix[row + offsetIndex + NeighbourhoodIndex, col + offsetIndex - NeighbourhoodIndex].OrientationCategory == (int)Direction.NorthEast)
                                    {
                                        positiveDiagonalDirection[offsetIndex + radiusOfNeighbourhood]++;
                                    }
                                }
                            }
                        }

                        // For the calculation of negative diagonal direction byte, we need to check each diagnal column
                        for (int offsetIndex = -radiusOfNeighbourhood; offsetIndex <= radiusOfNeighbourhood; offsetIndex++)
                        {
                            for (int NeighbourhoodIndex = -radiusOfNeighbourhood; NeighbourhoodIndex <= radiusOfNeighbourhood; NeighbourhoodIndex++)
                            {
                                if (StatisticalAnalysis.checkBoundary(row - offsetIndex + NeighbourhoodIndex, col + offsetIndex + NeighbourhoodIndex, rowsCount, colsCount))
                                {
                                    if ((Matrix[row - offsetIndex + NeighbourhoodIndex, col + offsetIndex + NeighbourhoodIndex] != null) && Matrix[row - offsetIndex + NeighbourhoodIndex, col + offsetIndex + NeighbourhoodIndex].OrientationCategory == (int)Direction.NorthWest)
                                    {
                                        negativeDiagonalDirection[offsetIndex + radiusOfNeighbourhood]++;
                                    }
                                }
                            }
                        }

                        result.Add(new RidgeNeighbourhoodFeatureVector(new Point(row, col))
                        {
                            HorizontalFractionVector = NormalizedRidgeNeighbourhoodFeatureVector(horizontalDirection),
                            VerticalFractionVector = NormalizedRidgeNeighbourhoodFeatureVector(verticalDirection),
                            PositiveDiagonalFractionVector = NormalizedRidgeNeighbourhoodFeatureVector(positiveDiagonalDirection),
                            NegativeDiagonalFractionVector = NormalizedRidgeNeighbourhoodFeatureVector(negativeDiagonalDirection)
                        });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// The method of DirectionByteRidgeNeighbourhoodFeatureVectors can be used to generate directionByteRidgeNeighbourhoodFeatureVectors, which means it includes sub-feature vector 
        /// for each direction. And the size of each sub-RidgeNeighbourhoodFeatureVector is determined by sizeOfNeighbourhood.
        /// </summary>
        /// <param name="poiList"> pointsOfInterest to be used to calculate the DirectionByteRidgeNeighbourhoodFeatureVector.</param>
        /// <param name="rowsCount"> the column count of original spectrogram. </param> 
        /// <param name="colsCount"> the row count of original spectrogram. </param>
        /// <param name="sizeOfNeighbourhood"> 
        /// the size of Neighbourhood will determine the size of search area.</param>
        /// <returns>
        /// It will return a list of RidgeNeighbourhoodFeatureVector objects whose DirectionByteRidgeNeighbourhoodFeatureVectors have been assigned, this can be used for similarity matching. 
        /// </returns>
        public static List<RidgeNeighbourhoodFeatureVector> DirectionByteRidgeNeighbourhoodFeatureVectors(List<PointOfInterest> poiList, int rowsCount, int colsCount, int sizeOfNeighbourhood)
        {
            var result = new List<RidgeNeighbourhoodFeatureVector>();

            // To search in a neighbourhood, the original pointsOfInterst should be converted into a pointOfInterst of Matrix
            var Matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);
            for (int row = 0; row < rowsCount; row++)
            {
                for (int col = 0; col < colsCount; col++)
                {
                    if (Matrix[row, col] == null)
                    {
                        continue;
                    }
                    else
                    {
                        // search from the first pointOfInterest which has edge value, and then search its right and down with the size of NeighbourhoodSize
                        var verticalDirection = new int[sizeOfNeighbourhood];
                        var horizontalDirection = new int[sizeOfNeighbourhood];
                        //var positiveDiagonalDirection = new int[2 * sizeOfNeighbourhood - 1];
                        //var negativeDiagonalDirection = new int[2 * sizeOfNeighbourhood - 1]; 

                        // For the calculation of horizontal direction byte, we need to check each row 
                        for (int rowNeighbourhoodIndex = 0; rowNeighbourhoodIndex < sizeOfNeighbourhood; rowNeighbourhoodIndex++)
                        {
                            for (int colNeighbourhoodIndex = 0; colNeighbourhoodIndex < sizeOfNeighbourhood; colNeighbourhoodIndex++)
                            {
                                // check boundary of index 
                                if (StatisticalAnalysis.checkBoundary(row + rowNeighbourhoodIndex, col + colNeighbourhoodIndex, rowsCount, colsCount))
                                {
                                    if ((Matrix[row + rowNeighbourhoodIndex, col + colNeighbourhoodIndex] != null) && Matrix[row + rowNeighbourhoodIndex, col + colNeighbourhoodIndex].OrientationCategory == (int)Direction.East)
                                    {
                                        horizontalDirection[rowNeighbourhoodIndex]++;
                                    }
                                }
                            }
                        }

                        // For the calculation of vertical direction byte, we need to check each column
                        for (int rowNeighbourhoodIndex = 0; rowNeighbourhoodIndex < sizeOfNeighbourhood; rowNeighbourhoodIndex++)
                        {
                            for (int colNeighbourhoodIndex = 0; colNeighbourhoodIndex < sizeOfNeighbourhood; colNeighbourhoodIndex++)
                            {
                                if (StatisticalAnalysis.checkBoundary(row + colNeighbourhoodIndex, col + rowNeighbourhoodIndex, rowsCount, colsCount))
                                {
                                    if ((Matrix[row + colNeighbourhoodIndex, col + rowNeighbourhoodIndex] != null) && Matrix[row + colNeighbourhoodIndex, col + rowNeighbourhoodIndex].OrientationCategory == (int)Direction.North)
                                    {
                                        verticalDirection[rowNeighbourhoodIndex]++;
                                    }
                                }
                            }
                        }

                        //result.Add(new RidgeNeighbourhoodFeatureVector(new Point(row, col)) { HorizontalVector = horizontalDirection, VerticalVector = verticalDirection, Intensity = Matrix[row, col].Intensity });
                        result.Add(new RidgeNeighbourhoodFeatureVector(new Point(row, col)) { HorizontalVector = horizontalDirection, VerticalVector = verticalDirection});
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// This method is to generate percentageByteRidgeNeighbourhoodFeatureVectors in where each of byte represents hte percentage of each direction accounts for.
        /// it will be done in a fixed neighbourhood. 
        /// </summary>
        /// <param name="poiList"> pointsOfInterest to be used to calculate the DirectionByteRidgeNeighbourhoodFeatureVector.</param>
        /// <param name="rowsCount"> the column count of original spectrogram.</param>
        /// <param name="colsCount"> the row count of original spectrogram.</param>
        /// <param name="sizeOfNeighbourhood"> the size of Neighbourhood will determine the size of search area.</param>
        /// <returns> 
        /// It will return a list of RidgeNeighbourhoodFeatureVector objects whose PercentageByteRidgeNeighbourhoodFeatureVectors have been assigned, this can be used for similarity matching. 
        /// </returns>
        public static List<RidgeNeighbourhoodFeatureVector> PercentageRidgeNeighbourhoodFeatureVectors(List<PointOfInterest> poiList, int rowsCount, int colsCount, int sizeOfNeighbourhood)
        {
            var result = new List<RidgeNeighbourhoodFeatureVector>();
            var Matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);

            for (int row = 0; row < rowsCount; row++)
            {
                for (int col = 0; col < colsCount; col++)
                {
                    if (Matrix[row, col] == null)
                    {
                        continue;
                    }
                    else
                    {
                        // search from the first pointOfInterest which has edge value, and then search its right and down with the size of NeighbourhoodSize
                        int numberOfverticalDirection = 0;
                        int numberOfhorizontalDirection = 0;
                        int numberOfpositiveDiagonalDirection = 0;
                        int numberOfnegativeDiagonalDirection = 0;
                        var sum = 0.0; var percentageOfVertical = 0.0; var percentageOfHorizontal = 0.0;
                        var percentageOfpositiveDiagonal = 0.0; var percentageOfnegativeDiagonal = 0.0;
                        var numberOfDirections = 4;
                        var percentageVector = new double[numberOfDirections];

                        for (int rowIndex = 0; rowIndex < sizeOfNeighbourhood; rowIndex++)
                        {
                            for (int colIndex = 0; colIndex < sizeOfNeighbourhood; colIndex++)
                            {
                                if (StatisticalAnalysis.checkBoundary(row + rowIndex, col + colIndex, rowsCount, colsCount))
                                {
                                    if (Matrix[row + rowIndex, col + colIndex] != null)
                                    {
                                        if (Matrix[row + rowIndex, col + colIndex].OrientationCategory == (int)Direction.East)
                                        {
                                            numberOfhorizontalDirection++;
                                            continue;
                                        }
                                        if (Matrix[row + rowIndex, col + colIndex].OrientationCategory == (int)Direction.North)
                                        {
                                            numberOfverticalDirection++;
                                            continue;
                                        }
                                        if (Matrix[row + rowIndex, col + colIndex].OrientationCategory == (int)Direction.NorthEast)
                                        {
                                            numberOfpositiveDiagonalDirection++;
                                            continue;
                                        }
                                        if (Matrix[row + rowIndex, col + colIndex].OrientationCategory == (int)Direction.NorthWest)
                                        {
                                            numberOfnegativeDiagonalDirection++;
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                        sum = numberOfverticalDirection + numberOfhorizontalDirection + numberOfpositiveDiagonalDirection + numberOfnegativeDiagonalDirection;
                        percentageOfVertical = numberOfverticalDirection / sum;
                        percentageOfHorizontal = numberOfhorizontalDirection / sum;
                        percentageOfpositiveDiagonal = numberOfpositiveDiagonalDirection / sum;
                        percentageOfnegativeDiagonal = numberOfnegativeDiagonalDirection / sum;

                        result.Add(new RidgeNeighbourhoodFeatureVector(new Point(row, col)) { HorizontalPercentage = percentageOfHorizontal,
                                                                            VerticalPercentage = percentageOfVertical,
                                                                            PositiveDiagonalPercentage = percentageOfpositiveDiagonal,
                                                                            NegativeDiagonalPercentage = percentageOfnegativeDiagonal});
                    }
                }
            }
            return result;
        }

        public static Point GetCentroid(double frequencyBinWidth, double framePerSecond, double maxFrequency, double minFrequency, double timeStart, double timeEnd)
        {
            var centroid = new Point(0, 0);
            var x = (int)((maxFrequency - minFrequency) / frequencyBinWidth * 0.5);
            var y = (int)((timeEnd - timeStart) * framePerSecond * 0.5);
            return centroid = new Point(x, y);
        }

        #endregion

    }
}
