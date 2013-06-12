

namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AudioAnalysisTools;
    using System.Drawing;
    public class FeatureVector
    {
        #region Public Properties
        
        public int[] HorizontalBitVector { get; set; }

        public int[] VerticalBitVector { get; set; }

        public int[] PositiveBitVector { get; set; }

        public int[] NegativeBitVector { get; set; }
        /// <summary>
        /// Gets or sets the point
        /// </summary>
        public Point point { get; set; }
        /// <summary>
        /// Gets or sets the percentageBitVector
        /// </summary>
        public double[] PercentageBitVector { get; set; }

        public double Vertical { get; set; }

        public double Horizontal { get; set; }

        public double PositiveDiagonal { get; set; }

        public double NegativeDiagonal { get; set; }
        /// <summary>
        /// Gets or sets the similarityScore
        /// </summary>
        public double SmilarityScore { get; set; }

        // constructor 
        public FeatureVector(double[] percentageVector)
        {
            PercentageBitVector = percentageVector;
        }

        // constructor for four direction of percentage bit vector
        public FeatureVector(Point pointofInterest)
        {
            point = pointofInterest; 
        }

        #endregion

        #region Public Method

        // one general representation 
        public static List<FeatureVector> GenerateBitOfFeatureVectors(List<PointOfInterest> poiList, int rows, int cols, int sizeOfNeighbourhood)
        {           
            var radiusOfNeighbourhood = sizeOfNeighbourhood / 2;
            var result = new List<FeatureVector>();

            var M = PointOfInterest.TransferPOIsToMatrix(poiList, rows, cols);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (M[r, c] == null)
                    {
                        continue;
                    }
                    else
                    {
                        // search in a neighbourhood
                        var verticalDirection = new int[sizeOfNeighbourhood];
                        var horizontalDirection = new int[sizeOfNeighbourhood];
                        //var positiveDiagonalDirection = new int[2 * sizeOfNeighbourhood - 1];
                        //var negativeDiagonalDirection = new int[2 * sizeOfNeighbourhood - 1];

                        for (int rowNeighbourhoodIndex = 0; rowNeighbourhoodIndex < sizeOfNeighbourhood; rowNeighbourhoodIndex++)
                        {
                            for (int colNeighbourhoodIndex = 0; colNeighbourhoodIndex < sizeOfNeighbourhood; colNeighbourhoodIndex++)
                            {
                                if (r + rowNeighbourhoodIndex < rows && c + colNeighbourhoodIndex < cols)
                                {
                                    if ((M[r + rowNeighbourhoodIndex, c + colNeighbourhoodIndex] != null) && M[r + rowNeighbourhoodIndex, c + colNeighbourhoodIndex].OrientationCategory == 0)
                                    {
                                        horizontalDirection[rowNeighbourhoodIndex]++;
                                    }
                                }
                            }
                        }

                        // For a vertical direction, we need to check each column
                        for (int rowNeighbourhoodIndex = 0; rowNeighbourhoodIndex < sizeOfNeighbourhood; rowNeighbourhoodIndex++)
                        {
                            for (int colNeighbourhoodIndex = 0; colNeighbourhoodIndex < sizeOfNeighbourhood; colNeighbourhoodIndex++)
                            {
                                if (r + colNeighbourhoodIndex < rows && c + rowNeighbourhoodIndex + colNeighbourhoodIndex * sizeOfNeighbourhood < cols)
                                {
                                    if ((M[r + colNeighbourhoodIndex, c + rowNeighbourhoodIndex + colNeighbourhoodIndex * sizeOfNeighbourhood] != null) && M[r + colNeighbourhoodIndex, c + rowNeighbourhoodIndex + colNeighbourhoodIndex * sizeOfNeighbourhood].OrientationCategory == 4)
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
                        result.Add(new FeatureVector(new Point(r, c)) { HorizontalBitVector = horizontalDirection, VerticalBitVector = verticalDirection}); 
                    }
                    
                }
            }
            return result;
        }

        public static List<FeatureVector> GeneratePercentageOfFeatureVectors(List<PointOfInterest> poiList, int rows, int cols, int sizeOfNeighbourhood)
        {                                     
            var result = new List<FeatureVector>();
            var Matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rows, cols);
            //var radiusOfNeighbourhood = sizeOfNeighbourhood / 2;  
            for (int r = 56; r < rows; r++)
            {
                for (int c = 39; c < cols; c++)
                {
                    if (Matrix[r, c] == null)
                    {
                        continue;
                    }
                    else
                    {
                        // search in a neighbourhood
                        int numberOfverticalDirection = 0;
                        int numberOfhorizontalDirection = 0;
                        int numberOfpositiveDiagonalDirection = 0;
                        int numberOfnegativeDiagonalDirection = 0;
                        var sum = 0.0; var percentageOfVertical = 0.0; var percentageOfHorizontal = 0.0;
                        var percentageOfpositiveDiagonal = 0.0; var percentageOfnegativeDiagonal = 0.0;
                        var numberOfDirections = 4;
                        var percentageVector = new double[numberOfDirections];
                        //for (int i = -radiusOfNeighbourhood; i <= radiusOfNeighbourhood; i++)
                        for (int i = 0; i < sizeOfNeighbourhood; i++)
                        {
                            //for (int j = -radiusOfNeighbourhood; j <= radiusOfNeighbourhood; j++)
                            for (int j = 0; j < sizeOfNeighbourhood; j++)
                            {
                                if (r + i >= 0 && c + j >= 0 && r + i < rows && c + j < cols)
                                {
                                    if (Matrix[r + i, c + j] != null)
                                    {
                                        if (Matrix[r + i, c + j].OrientationCategory == 4)
                                        {
                                            numberOfverticalDirection++;
                                        }
                                        if (Matrix[r + i, c + j].OrientationCategory == 0)
                                        {
                                            numberOfhorizontalDirection++;
                                        }
                                        if (Matrix[r + i, c + j].OrientationCategory == 2)
                                        {
                                            numberOfpositiveDiagonalDirection++;
                                        }
                                        if (Matrix[r + i, c + j].OrientationCategory == 6)
                                        {
                                            numberOfnegativeDiagonalDirection++;
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

                        percentageVector[0] = percentageOfVertical;
                        percentageVector[1] = percentageOfHorizontal;
                        percentageVector[2] = percentageOfpositiveDiagonal;
                        percentageVector[3] = percentageOfnegativeDiagonal;
                        result.Add(new FeatureVector(new Point(r, c)) { PercentageBitVector = percentageVector });
                    }
                }                 
            }
            return result;
        }

        public static int[,] DiagonalMask(int sizeOfNeighbourhood)
        {
            var result = new int[sizeOfNeighbourhood, sizeOfNeighbourhood];

            // above part
            for (int i = 0; i < sizeOfNeighbourhood / 2; i++)
            {               
                for (int j = 0; j < sizeOfNeighbourhood / 2 - i; j++)
                {
                    result[i, j] = 0;                    
                }
                for (int k = -i; k <= i; k++)
                {
                    result[i, sizeOfNeighbourhood / 2 + k] = 1;
                }
            }

            // for middle part
            for (int j = 0; j < sizeOfNeighbourhood; j++)
            {
                result[sizeOfNeighbourhood / 2, j] = 1; 
            }

            // for below part
            for (int i = sizeOfNeighbourhood / 2 + 1; i < sizeOfNeighbourhood; i++)
            {
                for (int j = 0; j < sizeOfNeighbourhood - i; j++)
                {
                    result[i, j] = 0;                 
                }
                for (int k = -(sizeOfNeighbourhood - i - 1); k <= sizeOfNeighbourhood - i - 1; k++)
                {
                    result[i, sizeOfNeighbourhood / 2 + k] = 1;
                }
            }
            return result;
        }


        //public static bool checkBoundary(int miniXIndex, int maxiXIndex, int rowCount,int colsCount)
        //{
        //    if ()
        //}
        #endregion
    }
}
