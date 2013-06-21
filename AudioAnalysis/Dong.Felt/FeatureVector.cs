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
    /// A class for generating featureVector for decribing the acoustic events(bird calls).
    /// </summary>   
    public class FeatureVector
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets the HorizontalByteVector, part of a composite  edge featurevector, representing the horizontal direction of edge(one kind of feature). 
        /// </summary>
        public List<int> HorizontalVector1 { get; set; }
        public int[] HorizontalVector { get; set; }

        /// <summary>
        /// Gets or sets the HorizontalBitVector, part of a composite  edge featurevector, representing the horizontal direction of edge(one kind of feature). 
        /// </summary>
        public int[] HorizontalBitVector { get; set; }

        /// <summary>
        /// Gets or sets the HorizontalBitVector, part of a composite  edge featurevector, representing the horizontal direction of edge(one kind of feature). 
        /// </summary>
        public double[] HorizontalFractionVector { get; set; }

        /// <summary>
        /// Gets or sets the VerticalBitVector, part of composite edge featurevector, representing the vertital direction of edge(one kind of feature).
        /// </summary>
        public List<int> VerticalVector1 { get; set; }
        public int[] VerticalVector { get; set; }

        /// <summary>
        /// Gets or sets the VerticalBitVector, part of a composite  edge featurevector, representing the vertital direction of edge(one kind of feature). 
        /// </summary>
        public int[] VerticalBitVector { get; set; }

        /// <summary>
        /// Gets or sets the VerticalBitVector, part of a composite  edge featurevector, representing the vertital direction of edge(one kind of feature). 
        /// </summary>
        public double[] VerticalFractionVector { get; set; }

        /// <summary>
        /// Gets or sets the PositiveDiagonalByteVector, part of composite edge featurevector, representing the NorthEast direction of edge(one kind of feature).
        /// </summary>
        public int[] PositiveDiagonalVector { get; set; }

        /// <summary>
        /// Gets or sets the PositiveDiagonalByteVector, part of composite edge featurevector, representing the NorthEast direction of edge(one kind of feature).
        /// </summary>
        public int[] PositiveDiagonalBitVector { get; set; }

        /// <summary>
        /// Gets or sets the PositiveDiagonalBitVector, part of a composite  edge featurevector, representing the NorthEast direction of edge(one kind of feature). 
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
        /// Gets or sets the NegativeDiagonalBitVector, part of a composite  edge featurevector, representing the NorthWest direction of edge(one kind of feature). 
        /// </summary>
        public double[] NegativeDiagonalFractionVector { get; set; }

        /// <summary>
        /// Gets or sets the point of a particular pointsOfInterest
        /// </summary>
        public Point Point { get; set; }

        /// <summary>
        /// Gets or sets the percentageByteVector, another type of edge feature vector, representing the percentage of each direction of edge account for. 
        /// Especially, it [0], horizontal, [1], vertical, [2], positiveDiagonal, [3], negativeDiagonal.
        /// </summary>
        public double[] PercentageByteVector { get; set; }

        /// <summary>
        /// Gets or sets the similarityScore
        /// </summary>
        public double SmilarityScore { get; set; }

        /// <summary>
        /// Gets or sets the NeighbourhoodSize, it can determine the search area for generating feature vectors.
        /// </summary>
        public int NeighbourhoodSize { get; set; }

        public double Intensity { get; set; }

        #region constructor
        /// <summary>
        /// A constructor takes in percentageByteVector
        /// </summary>
        /// <param name="percentageVector"></param>
        public FeatureVector(double[] percentageByteVector)
        {
            PercentageByteVector = percentageByteVector;
        }

        /// <summary>
        /// A constructor takes in point
        /// </summary>
        /// <param name="pointofInterest"></param>
        public FeatureVector(Point point)
        {
            Point = point;
        }
        #endregion constructor

        #endregion public property

        #region Public Method

        /// <summary>
        /// The method of DirectionByteFeatureVectors can be used to generate integer directionFeatureVectors, which means it includes sub-feature vector 
        /// for each direction. And the size of each sub-featureVector is determined by sizeOfNeighbourhood.
        /// </summary>
        /// <param name="poiList"> pointsOfInterest to be used to calculate the DirectionByteFeatureVector.</param>
        /// <param name="rowsCount"> the column count of original spectrogram. </param> 
        /// <param name="colsCount"> the row count of original spectrogram. </param>
        /// <param name="sizeOfNeighbourhood"> 
        /// the size of Neighbourhood will determine the size of search area.</param>
        /// <returns>
        /// It will return a list of featureVector objects whose DirectionByteFeatureVectors have been assigned, this can be used for similarity matching. 
        /// </returns>
        public static List<FeatureVector> IntegarDirectionFeatureVectors(List<PointOfInterest> poiList, int rowsCount, int colsCount, int sizeOfNeighbourhood)
        {
            var result = new List<FeatureVector>();

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
                        // search from the anchor point among pointOfInterest, in the centroid of neighbourhood, and then search its Neighbourhood to get featureVector
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
                        
                        result.Add(new FeatureVector(new Point(row, col)) { HorizontalVector = horizontalDirection, VerticalVector = verticalDirection,
                                                                            PositiveDiagonalVector = positiveDiagonalDirection,
                                                                            NegativeDiagonalVector = negativeDiagonalDirection});
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
        public static double[] NormalizedFeatureVector(int[] array)
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
        /// <param name="featureVectorList"></param>
        /// <returns> returns a list featureVector which contains the direction bit Feature vector
        /// </returns>
        public static List<FeatureVector> DirectionBitFeatureVectors(List<FeatureVector> featureVectorList)
        {
            var horizontalBitVectorCount = featureVectorList[0].HorizontalBitVector.Count();
            var verticalBitVectorCount = featureVectorList[0].VerticalBitVector.Count();
          
            var thresholdForBitVector = 0.2;          
            foreach (var fv in featureVectorList)
            {
                var normalizedHorizontalBitVector = NormalizedFeatureVector(fv.HorizontalVector);
                var normalizedVerticalBitVector = NormalizedFeatureVector(fv.VerticalVector);
                var normalizedPositiveDiagonalVector = NormalizedFeatureVector(fv.PositiveDiagonalVector);
                var normalizedNegativeDiagonalVector = NormalizedFeatureVector(fv.NegativeDiagonalVector);
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
            
            return featureVectorList; 
        }

        /// <summary>
        /// The method of DirectionByteFeatureVectors can be used to generate directionFractionFeatureVectors, which means it includes sub-feature vector 
        /// for each direction. And the size of each sub-featureVector is determined by sizeOfNeighbourhood. Especially, each value in the feature vector is a fraction.
        /// </summary>
        /// <param name="poiList"></param>
        /// <param name="rowsCount"></param>
        /// <param name="colsCount"></param>
        /// <param name="sizeOfNeighbourhood"></param>
        /// <returns></returns>
        public static List<FeatureVector> FractionDirectionFeatureVectors(List<PointOfInterest> poiList, int rowsCount, int colsCount, int sizeOfNeighbourhood)
        {
            var result = new List<FeatureVector>();

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
                        // search from the anchor point among pointOfInterest, in the centroid of neighbourhood, and then search its Neighbourhood to get featureVector
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
                   
                        result.Add(new FeatureVector(new Point(row, col))
                        {
                            HorizontalFractionVector = NormalizedFeatureVector(horizontalDirection),
                            VerticalFractionVector = NormalizedFeatureVector(verticalDirection),
                            PositiveDiagonalFractionVector = NormalizedFeatureVector(positiveDiagonalDirection),
                            NegativeDiagonalFractionVector = NormalizedFeatureVector(negativeDiagonalDirection)
                        });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// The method of DirectionByteFeatureVectors can be used to generate directionByteFeatureVectors, which means it includes sub-feature vector 
        /// for each direction. And the size of each sub-featureVector is determined by sizeOfNeighbourhood.
        /// </summary>
        /// <param name="poiList"> pointsOfInterest to be used to calculate the DirectionByteFeatureVector.</param>
        /// <param name="rowsCount"> the column count of original spectrogram. </param> 
        /// <param name="colsCount"> the row count of original spectrogram. </param>
        /// <param name="sizeOfNeighbourhood"> 
        /// the size of Neighbourhood will determine the size of search area.</param>
        /// <returns>
        /// It will return a list of featureVector objects whose DirectionByteFeatureVectors have been assigned, this can be used for similarity matching. 
        /// </returns>
        public static List<FeatureVector> DirectionByteFeatureVectors(List<PointOfInterest> poiList, int rowsCount, int colsCount, int sizeOfNeighbourhood)
        {
            var result = new List<FeatureVector>();

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

                        result.Add(new FeatureVector(new Point(row, col)) { HorizontalVector = horizontalDirection, VerticalVector = verticalDirection, Intensity = Matrix[row, col].Intensity });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// This method is to generate percentageByteFeatureVectors in where each of byte represents hte percentage of each direction accounts for.
        /// it will be done in a fixed neighbourhood. 
        /// </summary>
        /// <param name="poiList"> pointsOfInterest to be used to calculate the DirectionByteFeatureVector.</param>
        /// <param name="rowsCount"> the column count of original spectrogram.</param>
        /// <param name="colsCount"> the row count of original spectrogram.</param>
        /// <param name="sizeOfNeighbourhood"> the size of Neighbourhood will determine the size of search area.</param>
        /// <returns> 
        /// It will return a list of featureVector objects whose PercentageByteFeatureVectors have been assigned, this can be used for similarity matching. 
        /// </returns>
        public static List<FeatureVector> PercentageByteFeatureVectors(List<PointOfInterest> poiList, int rowsCount, int colsCount, int sizeOfNeighbourhood)
        {
            var result = new List<FeatureVector>();
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

                        percentageVector[0] = percentageOfHorizontal;
                        percentageVector[1] = percentageOfVertical;
                        percentageVector[2] = percentageOfpositiveDiagonal;
                        percentageVector[3] = percentageOfnegativeDiagonal;
                        result.Add(new FeatureVector(new Point(row, col)) { PercentageByteVector = percentageVector });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// This feacture vector generation can be got by user providing an event. 
        /// </summary>
        /// <param name="poiList"></param>
        /// <param name="maxFrequency"></param>
        /// 
        /// <param name="minFrequency"></param>
        /// <param name="duration"></param>
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
        public static List<List<FeatureVector>> FeatureVectorForQuery(List<PointOfInterest> poiList, double maxFrequency, double minFrequency,
                                                               double duration, int herzPerSlice, double durationPerSlice, double herzScale, double timeScale,
                                                               double sampleRate, int rowsCount, int colsCount)
        {
            var rowsCountPerSlice = (int)Math.Ceiling(herzPerSlice / herzScale);  // 13 pixels  560Hz
            var colsCountPerSlice = (int)Math.Ceiling(durationPerSlice / timeScale); // 13 pixels 0.15 second
            var MaxRowIndex = (int)Math.Ceiling((sampleRate - minFrequency) / herzScale);
            var MinRowIndex = (int)Math.Floor((sampleRate - maxFrequency) / herzScale);
            var radiusOfSlice = rowsCountPerSlice / 2;
            var numberOfRowSlices = (int)Math.Ceiling((maxFrequency - minFrequency) / herzPerSlice);
            var numberOfColSlices = (int)Math.Ceiling(duration / durationPerSlice);
            //var result1 = new List<FeatureVector>[numberOfRowSlices, numberOfColSlices];
            var result = new List<List<FeatureVector>>();
            var Matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);
            // search along the time axis
            for (int row = MinRowIndex; row < MaxRowIndex; row += rowsCountPerSlice)
            {
                // leave it here to consider about its efficiency
                for (int col = 0; col < colsCount; col++)
                {
                    for (int sliceRowIndex = 0; sliceRowIndex < numberOfRowSlices; sliceRowIndex++)
                    {
                        for (int sliceColIndex = 0; sliceColIndex < numberOfColSlices; sliceColIndex++)
                        {                           
                            if (StatisticalAnalysis.checkBoundary(row + (sliceRowIndex + 1) * rowsCountPerSlice, col + (sliceColIndex + 1) * colsCountPerSlice, rowsCount, colsCount))
                            {
                                var subMatrix = StatisticalAnalysis.Submatrix(Matrix, row + sliceRowIndex * rowsCountPerSlice, col + sliceColIndex * colsCountPerSlice, row + (sliceRowIndex + 1) * rowsCountPerSlice, col + (sliceColIndex + 1) * colsCountPerSlice);
                                var partialFeatureVector = FeatureVector.SliceDirectionFeatureVectors(subMatrix, row, col);
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

        public static FeatureVector SliceDirectionFeatureVectors(PointOfInterest[,] matrix, int PointX, int PointY)
        {
            var result = new FeatureVector(new Point(PointX, PointY)); 
            var sizeOfNeighbourhood = matrix.GetLength(0);
            // To search in a neighbourhood, the original pointsOfInterst should be converted into a pointOfInterst of Matrix
            var radiusOfNeighbourhood = sizeOfNeighbourhood / 2;
            // limite the frequency rang
            for (int row = 0; row < sizeOfNeighbourhood; row++)
            {
                for (int col = 0; col < sizeOfNeighbourhood; col++)
                {    
                    // search from the anchor point among pointOfInterest, in the centroid of neighbourhood, and then search its Neighbourhood to get featureVector
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
                             if (StatisticalAnalysis.checkBoundary(row + rowNeighbourhoodIndex, col + colNeighbourhoodIndex, sizeOfNeighbourhood, sizeOfNeighbourhood))
                             {
                                 if ((matrix[row + rowNeighbourhoodIndex, col + colNeighbourhoodIndex] != null) && matrix[row + rowNeighbourhoodIndex, col + colNeighbourhoodIndex].OrientationCategory == (int)Direction.East)
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
                             if (StatisticalAnalysis.checkBoundary(row + colNeighbourhoodIndex, col + rowNeighbourhoodIndex, sizeOfNeighbourhood, sizeOfNeighbourhood))
                              {
                                  if ((matrix[row + colNeighbourhoodIndex, col + rowNeighbourhoodIndex] != null) && matrix[row + colNeighbourhoodIndex, col + rowNeighbourhoodIndex].OrientationCategory == (int)Direction.North)
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
                              if (StatisticalAnalysis.checkBoundary(row + offsetIndex + NeighbourhoodIndex, col + offsetIndex - NeighbourhoodIndex, sizeOfNeighbourhood, sizeOfNeighbourhood))
                              {
                                  if ((matrix[row + offsetIndex + NeighbourhoodIndex, col + offsetIndex - NeighbourhoodIndex] != null) && matrix[row + offsetIndex + NeighbourhoodIndex, col + offsetIndex - NeighbourhoodIndex].OrientationCategory == (int)Direction.NorthEast)
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
                               if (StatisticalAnalysis.checkBoundary(row - offsetIndex + NeighbourhoodIndex, col + offsetIndex + NeighbourhoodIndex, sizeOfNeighbourhood, sizeOfNeighbourhood))
                                {
                                    if ((matrix[row - offsetIndex + NeighbourhoodIndex, col + offsetIndex + NeighbourhoodIndex] != null) && matrix[row - offsetIndex + NeighbourhoodIndex, col + offsetIndex + NeighbourhoodIndex].OrientationCategory == (int)Direction.NorthWest)
                                    {
                                        negativeDiagonalDirection[offsetIndex + radiusOfNeighbourhood]++;
                                    }
                                }
                            }
                        }
                        result.HorizontalVector = horizontalDirection;
                        result.VerticalVector = verticalDirection;
                        result.PositiveDiagonalVector = positiveDiagonalDirection;
                        result.NegativeDiagonalVector = negativeDiagonalDirection;
                }
            }
            return result;
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
            //var Matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);
            //var result = new int[CeilCount];
            //for (int i = 0; i < CeilCount; i++)
            //{
            //    for (int col = i * numberOfframePerTimeunit; col < (i + 1) * numberOfframePerTimeunit; col++)
            //    {
            //         for (int row = 0; row < rowsCount; row++)
            //         {
            //             if (StatisticalAnalysis.checkBoundary(row, col, rowsCount, colsCount))
            //             {
            //                 if (Matrix[row, col] != null)
            //                 {
            //                     result[i]++;
            //                 }
            //             }
            //         }
            //    }
            //}
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

        #endregion

    }
}
