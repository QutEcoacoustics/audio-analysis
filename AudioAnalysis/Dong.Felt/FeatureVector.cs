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
        public int[] HorizontalByteVector { get; set; }

        /// <summary>
        /// Gets or sets the VerticalByteVector, part of composite edge featurevector, representing the vertital direction of edge(one kind of feature).
        /// </summary>
        public int[] VerticalByteVector { get; set; }

        /// <summary>
        /// Gets or sets the PositiveDiagonalByteVector, part of composite edge featurevector, representing the NorthEast direction of edge(one kind of feature).
        /// </summary>
        public int[] PositiveDiagonalByteVector { get; set; }

        /// <summary>
        /// Gets or sets the NegativeDiagonalByteVector, part of composite edge feature vector, representing the NorthWest direction of edge(one kind of feature).
        /// </summary>
        public int[] NegativeDiagonalByteVector { get; set; }

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

        //one general representation 
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
        public static List<FeatureVector> DirectionByteFeatureVectors1(List<PointOfInterest> poiList, int rowsCount, int colsCount, int sizeOfNeighbourhood)
        {
            var result = new List<FeatureVector>();
            var radiusOfNeighbourhood = sizeOfNeighbourhood / 2;
            // To search in a neighbourhood, the original pointsOfInterst should be converted into a pointOfInterst of Matrix
            var Matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);
            for (int row = 13; row < rowsCount; row++)
            {
                for (int col = 25; col < colsCount; col++)
                {
                    if (StatisticalAnalysis.checkBoundary(row, col, rowsCount, colsCount))
                    {
                    // search from the first pointOfInterest which has edge value, and then search its right and down with the size of NeighbourhoodSize
                    var verticalDirection = new int[sizeOfNeighbourhood];
                    var horizontalDirection = new int[sizeOfNeighbourhood];
                    //var positiveDiagonalDirection = new int[2 * sizeOfNeighbourhood - 1];
                    //var negativeDiagonalDirection = new int[2 * sizeOfNeighbourhood - 1]; 

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
                        for (int colNeighbourhoodIndex = 0; colNeighbourhoodIndex <= radiusOfNeighbourhood; colNeighbourhoodIndex++)
                        {
                            if (StatisticalAnalysis.checkBoundary(row + colNeighbourhoodIndex, col + rowNeighbourhoodIndex, rowsCount, colsCount))
                            {
                                if ((Matrix[row + colNeighbourhoodIndex, col + rowNeighbourhoodIndex] != null) && Matrix[row + colNeighbourhoodIndex, col +                                                   rowNeighbourhoodIndex].OrientationCategory == (int)Direction.North)
                                {
                                    verticalDirection[rowNeighbourhoodIndex + radiusOfNeighbourhood]++;
                                }
                            }
                        }
                    }

                    //// For a positiveDiagonal direction, we need to check each diagonal, it's usually has sizeOfneighbourhood + sizeOfNeighbourhood -1
                    //    for (int colNeighbourhoodIndex = 0; colNeighbourhoodIndex < sizeOfNeighbourhood; colNeighbourhoodIndex++)
                    //    {
                    //        for (int k = 0; k <= colNeighbourhoodIndex; k++)
                    //        {
                    //            // scan along the first row, so the i = 0 
                    //            if ((r + 0 - k >= 0) && (c + colNeighbourhoodIndex - k >= 0) && (c + colNeighbourhoodIndex - k < cols))
                    //            {
                    //                if ((M[r + 0 - k, c + colNeighbourhoodIndex - k] != null) && M[r + 0 - k, c + colNeighbourhoodIndex - k].OrientationCategory == 2)
                    //                {
                    //                    negativeDiagonalDirection[sizeOfNeighbourhood - colNeighbourhoodIndex - 1]++;
                    //                }
                    //            }

                    //        }
                    //   }
                    //   for (int rowNeighbourhoodIndex = 1; rowNeighbourhoodIndex < sizeOfNeighbourhood; rowNeighbourhoodIndex++)
                    //   {
                    //        for (int k = 0; k <= sizeOfNeighbourhood - rowNeighbourhoodIndex - 1; k++)
                    //        {
                    //            if ((r + rowNeighbourhoodIndex - k >= 0) && (r + rowNeighbourhoodIndex - k < rows) && (c + sizeOfNeighbourhood - 1 - k >= 0) && (c + sizeOfNeighbourhood - 1 - k < cols))
                    //            // scan along the last col, so the column here is sizeOfNeighbourhood - 1
                    //            if ((M[r + rowNeighbourhoodIndex - k, c + sizeOfNeighbourhood - 1 - k] != null) && M[r + rowNeighbourhoodIndex - k, c + sizeOfNeighbourhood - 1 - k].OrientationCategory == 2)
                    //            {
                    //                 positiveDiagonalDirection[sizeOfNeighbourhood + rowNeighbourhoodIndex - 1]++; 
                    //            }
                    //        }
                    //    }

                    //    // For a negativeDiagonal direction, we need to check each diagonal, it's usually has sizeOfneighbourhood + sizeOfNeighbourhood -1
                    //    for (int colNeighbourhoodIndex = sizeOfNeighbourhood - 1; colNeighbourhoodIndex >= 0; colNeighbourhoodIndex--)
                    //    {
                    //        for (int k = 0; k <= sizeOfNeighbourhood - colNeighbourhoodIndex - 1; k++)
                    //        {
                    //            if ((r + 0 + k < rows) && (c + colNeighbourhoodIndex + k < cols))
                    //            // scan along the first row, but in a versus direction
                    //            if ((M[r + 0 + k, c + colNeighbourhoodIndex + k] != null) && M[r + 0 + k, c + colNeighbourhoodIndex + k].OrientationCategory == 6)
                    //            {
                    //                 negativeDiagonalDirection[sizeOfNeighbourhood - colNeighbourhoodIndex - 1]++;
                    //            }
                    //        }
                    //   }
                    //   for (int rowNeighbourhoodIndex = 1; rowNeighbourhoodIndex < sizeOfNeighbourhood; rowNeighbourhoodIndex++)
                    //   {
                    //       for (int k = 0; k <= sizeOfNeighbourhood - rowNeighbourhoodIndex - 1; k++)
                    //       {
                    //            if ((r + rowNeighbourhoodIndex + k < rows) && (c + 0 + k < cols))
                    //            // scan along the first column
                    //            if ((M[r + rowNeighbourhoodIndex + k, c + 0 + k] != null) && M[r + rowNeighbourhoodIndex + k, c + 0 + k].OrientationCategory == 6)
                    //            {
                    //                 negativeDiagonalDirection[sizeOfNeighbourhood + rowNeighbourhoodIndex - 1]++; 
                    //            }
                    //       }
                    //   }       
                    result.Add(new FeatureVector(new Point(row, col)) { HorizontalByteVector = horizontalDirection, VerticalByteVector = verticalDirection });
                    }
                }
            }
            return result;
        }

        //one general representation 
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

                        //// For a positiveDiagonal direction, we need to check each diagonal, it's usually has sizeOfneighbourhood + sizeOfNeighbourhood -1
                        //    for (int colNeighbourhoodIndex = 0; colNeighbourhoodIndex < sizeOfNeighbourhood; colNeighbourhoodIndex++)
                        //    {
                        //        for (int k = 0; k <= colNeighbourhoodIndex; k++)
                        //        {
                        //            // scan along the first row, so the i = 0 
                        //            if ((r + 0 - k >= 0) && (c + colNeighbourhoodIndex - k >= 0) && (c + colNeighbourhoodIndex - k < cols))
                        //            {
                        //                if ((M[r + 0 - k, c + colNeighbourhoodIndex - k] != null) && M[r + 0 - k, c + colNeighbourhoodIndex - k].OrientationCategory == 2)
                        //                {
                        //                    negativeDiagonalDirection[sizeOfNeighbourhood - colNeighbourhoodIndex - 1]++;
                        //                }
                        //            }

                        //        }
                        //   }
                        //   for (int rowNeighbourhoodIndex = 1; rowNeighbourhoodIndex < sizeOfNeighbourhood; rowNeighbourhoodIndex++)
                        //   {
                        //        for (int k = 0; k <= sizeOfNeighbourhood - rowNeighbourhoodIndex - 1; k++)
                        //        {
                        //            if ((r + rowNeighbourhoodIndex - k >= 0) && (r + rowNeighbourhoodIndex - k < rows) && (c + sizeOfNeighbourhood - 1 - k >= 0) && (c + sizeOfNeighbourhood - 1 - k < cols))
                        //            // scan along the last col, so the column here is sizeOfNeighbourhood - 1
                        //            if ((M[r + rowNeighbourhoodIndex - k, c + sizeOfNeighbourhood - 1 - k] != null) && M[r + rowNeighbourhoodIndex - k, c + sizeOfNeighbourhood - 1 - k].OrientationCategory == 2)
                        //            {
                        //                 positiveDiagonalDirection[sizeOfNeighbourhood + rowNeighbourhoodIndex - 1]++; 
                        //            }
                        //        }
                        //    }

                        //    // For a negativeDiagonal direction, we need to check each diagonal, it's usually has sizeOfneighbourhood + sizeOfNeighbourhood -1
                        //    for (int colNeighbourhoodIndex = sizeOfNeighbourhood - 1; colNeighbourhoodIndex >= 0; colNeighbourhoodIndex--)
                        //    {
                        //        for (int k = 0; k <= sizeOfNeighbourhood - colNeighbourhoodIndex - 1; k++)
                        //        {
                        //            if ((r + 0 + k < rows) && (c + colNeighbourhoodIndex + k < cols))
                        //            // scan along the first row, but in a versus direction
                        //            if ((M[r + 0 + k, c + colNeighbourhoodIndex + k] != null) && M[r + 0 + k, c + colNeighbourhoodIndex + k].OrientationCategory == 6)
                        //            {
                        //                 negativeDiagonalDirection[sizeOfNeighbourhood - colNeighbourhoodIndex - 1]++;
                        //            }
                        //        }
                        //   }
                        //   for (int rowNeighbourhoodIndex = 1; rowNeighbourhoodIndex < sizeOfNeighbourhood; rowNeighbourhoodIndex++)
                        //   {
                        //       for (int k = 0; k <= sizeOfNeighbourhood - rowNeighbourhoodIndex - 1; k++)
                        //       {
                        //            if ((r + rowNeighbourhoodIndex + k < rows) && (c + 0 + k < cols))
                        //            // scan along the first column
                        //            if ((M[r + rowNeighbourhoodIndex + k, c + 0 + k] != null) && M[r + rowNeighbourhoodIndex + k, c + 0 + k].OrientationCategory == 6)
                        //            {
                        //                 negativeDiagonalDirection[sizeOfNeighbourhood + rowNeighbourhoodIndex - 1]++; 
                        //            }
                        //       }
                        //   }       
                        result.Add(new FeatureVector(new Point(row, col)) { HorizontalByteVector = horizontalDirection, VerticalByteVector = verticalDirection, Intensity = Matrix[row, col].Intensity });
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
