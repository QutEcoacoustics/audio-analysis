namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using Representations;

    public enum MatchIndex
    {
        /// <summary>
        /// This is North. Also known as vertical.
        /// </summary>
        [Description("This is Exact. It is exact match for null direction .")]
        Exact = 0,

        /// <summary>
        /// This is North East. Also known as forward slash or diagonal.
        /// </summary>
        [Description("This is North East. It is varied match for non-null direction.")]
        Variation = 1,
    }
    class SimilarityMatching
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets the SimilarityScore, it can be derived from the calculationg of similarity score. 
        /// </summary>
        public double SimilarityScore { get; set; }

        #endregion
        /// <summary>
        /// Calculate the distance between two featureVectors. one is from template, one is from another event
        /// </summary>
        /// <param name="instance"> one of featureVectors needs to be compared.</param>
        /// <param name="template"> a particular species featureVector needs to be compared.</param>
        /// <returns>
        /// return the avgDistance.
        /// </returns>
        public static double AvgDistance(RidgeNeighbourhoodFeatureVector instance, RidgeNeighbourhoodFeatureVector template)
        {
            var avgdistance = 0.0;
            var numberOfScaleCount = instance.VerticalBitVector.Count();
            var sumV = 0.0;
            var sumH = 0.0;
            for (int i = 0; i < numberOfScaleCount; i++)
            {
                // kind of Manhattan distance calculation    
                sumV = sumV + Math.Abs(instance.VerticalBitVector[i] - template.VerticalBitVector[i]);
                sumH = sumH = Math.Abs(instance.HorizontalBitVector[i] - template.HorizontalBitVector[i]);
            }
            var sum = (sumH + sumV) / 2;
            avgdistance = sum / numberOfScaleCount;

            return avgdistance;
        }

        public static int distanceForBitFeatureVector(RidgeNeighbourhoodFeatureVector instance, RidgeNeighbourhoodFeatureVector template)
        {
            var distance = 0;
            var numberOfBitCount = instance.VerticalBitVector.Count();
            var sumV = 0;
            var sumH = 0;
            for (int i = 0; i < numberOfBitCount; i++)
            {
                // kind of Manhattan distance calculation    
                sumV = sumV + Math.Abs(instance.VerticalBitVector[i] - template.VerticalBitVector[i]);
                sumH = sumH + Math.Abs(instance.HorizontalBitVector[i] - template.HorizontalBitVector[i]);
            }
            var sum = (sumH + sumV) / 2;
            distance = sum;

            return distance;
        }

        public static double SimilarityScoreForAvgDistance(double avgDistance, int neighbourhoodSize)
        {
            var similarityScore = 1 - avgDistance / neighbourhoodSize;

            return similarityScore;
        }

        ///// <summary>
        ///// One way to calculate Similarity Score for percentage byte vector representation.
        ///// </summary>
        ///// <param name="instance"> the instance's feature vector to be compared. </param>
        ///// <param name="template"> the template's feature vector to be compared. </param>
        ///// <returns> 
        ///// It will return a similarity score. 
        ///// </returns>
        //public static double SimilarityScoreOfPercentageByteVector(FeatureVector instance, FeatureVector template)
        //{
        //    // Initialize
        //    double similarityScore = 0.0;
        //    var threshold = new double[] { 0.1, 0.1, 0.1, 0.3 };
        //    if (Math.Abs(instance.PercentageByteVector[0] - template.PercentageByteVector[0]) < threshold[0]
        //     && Math.Abs(instance.PercentageByteVector[1] - template.PercentageByteVector[1]) < threshold[1]
        //     && Math.Abs(instance.PercentageByteVector[2] - template.PercentageByteVector[2]) < threshold[2]
        //     && Math.Abs(instance.PercentageByteVector[3] - template.PercentageByteVector[3]) < threshold[3])
        //    {
        //        similarityScore = 0.9;
        //    }

        //    return similarityScore;
        //}

        // To calculate the distance between query and potentialEvent. The return value is equal to the sum of every orientation subdistance. 
        public static int SimilarSliceNumberOfFeatureVector(List<RidgeNeighbourhoodFeatureVector> potentialEvent, List<RidgeNeighbourhoodFeatureVector> query)
        {
            var result = 0;
            var distanceThreshold = 15;
            if (query != null && potentialEvent != null)
            {
                var numberOfFeaturevector = query[0].HorizontalVector.Count();
                var numberOfdiagonalFeaturevector = query[0].PositiveDiagonalVector.Count();
                var numberOfSlices = query.Count();
                // Option 2 according to potential event length 
                // var numberOfSlices = potentiaEvent.Count();
                var horizontalDistance = 0.0;
                var verticalDistance = 0.0;
                var positiveDiagonalDistance = 0.0;
                var negativeDiagonalDistance = 0.0;

                for (int sliceIndex = 0; sliceIndex < numberOfSlices; sliceIndex++)
                {
                    for (int i = 0; i < numberOfFeaturevector; i++)
                    {
                        // check wether the query is null, then check if the potential is null, too.Yes, then it's similar. Otherwise, it is different.
                        if (checkNullFeatureVector(query[sliceIndex]))
                        {
                            if (checkNullFeatureVector(potentialEvent[sliceIndex]))
                            {
                                result++;
                            }
                        }
                        else
                        {
                            if (!checkNullFeatureVector(potentialEvent[sliceIndex]))
                            {
                                horizontalDistance += Distance.EuclideanDistanceForCordinates((double)potentialEvent[sliceIndex].HorizontalVector[i], 0.0, (double)query[sliceIndex].HorizontalVector[i], 0.0);
                                verticalDistance += Distance.EuclideanDistanceForCordinates((double)potentialEvent[sliceIndex].VerticalVector[i], 0.0, (double)query[sliceIndex].VerticalVector[i], 0.0);
                            }
                        }               
                    }
                    for (int j = 0; j < numberOfdiagonalFeaturevector; j++)
                    {
                        if (checkNullFeatureVector(query[sliceIndex]))
                        {
                            if (checkNullFeatureVector(potentialEvent[sliceIndex]))
                            {
                                result++;
                            }
                        }
                        else
                        {
                            if (!checkNullFeatureVector(potentialEvent[sliceIndex]))
                            {
                                positiveDiagonalDistance += Distance.EuclideanDistanceForCordinates((double)potentialEvent[sliceIndex].PositiveDiagonalVector[j], 0.0, (double)query[sliceIndex].PositiveDiagonalVector[j], 0.0);
                                negativeDiagonalDistance += Distance.EuclideanDistanceForCordinates((double)potentialEvent[sliceIndex].NegativeDiagonalVector[j], 0.0, (double)query[sliceIndex].NegativeDiagonalVector[j], 0.0);
                            }
                        }
                    }
                }
                if (horizontalDistance < distanceThreshold && verticalDistance < distanceThreshold && positiveDiagonalDistance < distanceThreshold && negativeDiagonalDistance < distanceThreshold)
                {
                     result++;
                }
            }
            return result;
        }

        public static double SimilarityScoreOfFeatureVector(List<RidgeNeighbourhoodFeatureVector> query, int similarSliceCount)
        {
            var totalNumberOfSlice = query.Count();
            var score = similarSliceCount / totalNumberOfSlice;
            return score;
        }

        /// <summary>
        /// One way to calculate Similarity Score for direction byte vector representation.
        /// </summary>
        /// <param name="instance"> the instance's feature vector to be compared. </param>
        /// <param name="template"> the template's feature vector to be compared. </param>
        /// <returns>
        /// /// It will return a similarity score. 
        /// </returns>
        public static double SimilarityScoreOfDirectionByteVector(RidgeNeighbourhoodFeatureVector instance, RidgeNeighbourhoodFeatureVector template)
        {
            var bitCount = instance.HorizontalBitVector.Count();

            double similarityScore = 0.0;
            var numberOfSameHorizontalByte = 0;
            var numberOfSameVerticalByte = 0;
            var horizontalThreshold = new double[] { 1, 4 }; // threshold[0], exact match for null direction,  threshold[1], 
            var verticalThreshold = new double[] { 1, 4 };
            for (int byteIndex = 0; byteIndex < bitCount; byteIndex++)
            {
                if (template.HorizontalBitVector[byteIndex] == 0) // they must match with each other in an exact way
                {
                    if (Math.Abs(instance.HorizontalBitVector[byteIndex] - template.HorizontalBitVector[byteIndex]) < horizontalThreshold[(int)MatchIndex.Exact])
                    {
                        numberOfSameHorizontalByte++;
                    }
                }
                else  // it can have some varieations in such a case
                {
                    if (Math.Abs(instance.HorizontalBitVector[byteIndex] - template.HorizontalBitVector[byteIndex]) < horizontalThreshold[(int)MatchIndex.Variation])
                    {
                        numberOfSameHorizontalByte++;
                    }
                }

                if (template.VerticalBitVector[byteIndex] == 0) // they must match with each other in an exact way
                {
                    if (Math.Abs(instance.VerticalBitVector[byteIndex] - template.VerticalBitVector[byteIndex]) < verticalThreshold[(int)MatchIndex.Exact])
                    {
                        numberOfSameVerticalByte++;
                    }
                }
                else  // it can have some varieations in such a case
                {
                    if (Math.Abs(instance.VerticalBitVector[byteIndex] - template.VerticalBitVector[byteIndex]) < verticalThreshold[(int)MatchIndex.Variation])
                    {
                        numberOfSameVerticalByte++;
                    }
                }
            }
            if (numberOfSameHorizontalByte > bitCount - 1 && numberOfSameVerticalByte > bitCount - 1)
            {
                similarityScore = 1;
            }
            else
            {
                if (numberOfSameHorizontalByte > bitCount - 2 && numberOfSameVerticalByte > bitCount - 2)
                //|| (numberOfSameHorizontalByte > bitCount - 1 && numberOfSameVerticalByte > bitCount - 2)
                //|| (numberOfSameHorizontalByte > bitCount - 2 && numberOfSameVerticalByte > bitCount - 1))
                {
                    similarityScore = 0.9;
                }
                else
                {
                    if (numberOfSameHorizontalByte > bitCount - 3 && numberOfSameVerticalByte > bitCount - 3)
                    {
                        similarityScore = 0.8;
                    }
                }
            }
            return similarityScore;
        }

        public static double SimilarityScoreOfFuzzyDirectionVector(RidgeNeighbourhoodFeatureVector instance)
        {
            double similarityScore = 0.0;
            var horizontalByteCount = instance.HorizontalBitVector.Count();
            var fuzzyVerticalLine = false;
            var fuzzyHorizontalLine = false;
            for (int byteIndex = 0; byteIndex < horizontalByteCount; byteIndex++)
            {
                if (byteIndex == 0)
                {
                    var numberOfOffset = 5;
                    for (int index1 = 0; index1 <= numberOfOffset; index1++)
                    {
                        if (instance.VerticalBitVector[byteIndex + index1] != 0
                            || (instance.VerticalBitVector[byteIndex + index1] != 0 && instance.VerticalBitVector[byteIndex + index1 + 1] != 0)
                            || (instance.VerticalBitVector[byteIndex + index1] != 0 && instance.VerticalBitVector[byteIndex + index1 + 1] != 0 && instance.VerticalBitVector[byteIndex + 2] != 0))
                        {
                            if ((instance.VerticalBitVector[byteIndex]
                                + instance.VerticalBitVector[byteIndex + index1 + 1]
                                + instance.VerticalBitVector[byteIndex + index1 + 2]) > 3)
                            {
                                fuzzyVerticalLine = true;
                            }
                        }
                    }

                }

                var lastHorizontalLineIndex = 5;
                if (byteIndex == horizontalByteCount - lastHorizontalLineIndex - 1)
                {
                    var numberOfOffset = 5;
                    for (int index1 = 0; index1 <= numberOfOffset; index1++)
                    {
                        if ((byteIndex + index1 + 2) < horizontalByteCount)
                        {
                            if (instance.HorizontalBitVector[byteIndex + index1] != 0
                                || (instance.HorizontalBitVector[byteIndex + index1] != 0 && instance.HorizontalBitVector[byteIndex + index1 + 1] != 0)
                                || (instance.HorizontalBitVector[byteIndex + index1] != 0 && instance.HorizontalBitVector[byteIndex + index1 + 1] != 0 && instance.HorizontalBitVector[byteIndex + 2] != 0))
                            {
                                if ((instance.HorizontalBitVector[byteIndex]
                                    + instance.HorizontalBitVector[byteIndex + index1 + 1]
                                    + instance.HorizontalBitVector[byteIndex + index1 + 2]) > 1)
                                {
                                    fuzzyHorizontalLine = true;
                                }
                            }
                        }
                    }
                }
            }
            if (fuzzyVerticalLine && fuzzyHorizontalLine)
            {
                similarityScore = 1;
            }
            return similarityScore;
        }

        public static double SimilarityScoreOfSlopeScore(List<RidgeNeighbourhoodFeatureVector> potentialEvent, List<RidgeNeighbourhoodFeatureVector> query)
        {
            var result = 0.0;
            if (query != null && potentialEvent != null)
            {
                var numberOfSlices = query.Count();
                for (int sliceIndex = 0; sliceIndex < numberOfSlices; sliceIndex++)
                {
                    var startPointX = (double)query[sliceIndex].SlopeScore;
                    var startPointY = 0.0;
                    var endPointX = (double)potentialEvent[sliceIndex].SlopeScore;
                    var endPointY = 0.0;
                    if (query[sliceIndex].Slope.Item1 == potentialEvent[sliceIndex].Slope.Item1)
                    {
                        result += Distance.EuclideanDistanceForCordinates(startPointX, startPointY, endPointX, endPointY);
                    }
                    else
                    {
                        result += Distance.EuclideanDistanceForCordinates(startPointX, startPointY, endPointX, endPointY) * 2;
                    }
                }
            }
            return result; 
        }

        public static double SimilarityScoreRidgeDiscription(RidgeDescriptionNeighbourhoodRepresentation[,] potentialEvent, RidgeDescriptionNeighbourhoodRepresentation[,] query)
        {
            var result = 0.0;
            var rowsCount = potentialEvent.GetLength(0);
            var colsCount = potentialEvent.GetLength(1);
            if (query != null && potentialEvent != null)
            {
                for (int rowIndex = 0; rowIndex < rowsCount; rowIndex++)
                {
                    for (int colIndex = 0; colIndex < colsCount; colIndex++)
                    {
                        result += Distance.EuclideanDistanceForCordinates(potentialEvent[rowIndex, colIndex].score, 0, query[rowIndex, colIndex].score, 0);
                    }                  
                }
            }
            return result;
        }

        public static double DistanceScoreRegionRepresentation(RegionRerepresentation query, RegionRerepresentation candidate)
        {
            var result = 0.0;
            var nhCount = query.ridgeNeighbourhoods.Count;
            for (int index = 0; index < nhCount; index++)
            {
                var ridgeNeighbourhoods = new List<RidgeDescriptionNeighbourhoodRepresentation>(query.ridgeNeighbourhoods);
                var queryScore = ridgeNeighbourhoods[index].score;
                var queryOrientationType =  ridgeNeighbourhoods[index].orientationType;
                var candidateScore = candidate.ridgeNeighbourhoods[index].score;
                var candidateOrientationType = candidate.ridgeNeighbourhoods[index].orientationType;
                result += Math.Sqrt(Math.Abs(queryOrientationType - candidateOrientationType) * Math.Pow((queryScore - candidateScore),2.0));
            }
            return result; 
        }

        public static double SimilarityScoreOfDifferentWeights(List<RidgeNeighbourhoodFeatureVector> potentialEvent, List<RidgeNeighbourhoodFeatureVector> query)
        {
            var result = 0.0;
            if (query != null && potentialEvent != null)
            {
                //var thresholdOfNumberOfPoi = 1;
                var numberOfFeaturevector = query[0].HorizontalVector.Count();
                var numberOfdiagonalFeaturevector = query[0].PositiveDiagonalVector.Count();
                var numberOfSlices = query.Count();
                // Option 2 according to potential event length 
                // var numberOfSlices = potentiaEvent.Count();
                var horizontalDistance = 0.0;
                var verticalDistance = 0.0;
                var positiveDiagonalDistance = 0.0;
                var negativeDiagonalDistance = 0.0;

                var startPointY = 0.0;
                var endPointY = 0.0;
                for (int sliceIndex = 0; sliceIndex < numberOfSlices; sliceIndex++)
                {
                    if (checkNullFeatureVectorList(potentialEvent))
                    {
                        result = 1000000;
                    }
                    else
                    {
                        //if (!checkNullFeatureVector(query[sliceIndex]) && !checkNullFeatureVector(potentialEvent[sliceIndex]))
                        //{
                            for (int i = 0; i < numberOfFeaturevector; i++)
                            {
                                var horizontalStartPointX = potentialEvent[sliceIndex].HorizontalVector[i];
                                var horizontalEndPointX = query[sliceIndex].HorizontalVector[i];
                                var verticalStartPointX = potentialEvent[sliceIndex].VerticalVector[i];
                                var verticalEndPointX = query[sliceIndex].VerticalVector[i];
                                horizontalDistance += Distance.EuclideanDistanceForCordinates(horizontalStartPointX, startPointY, horizontalEndPointX, endPointY);
                                verticalDistance += Distance.EuclideanDistanceForCordinates(verticalStartPointX, startPointY, verticalEndPointX, endPointY);
                            }
                            for (int j = 0; j < numberOfdiagonalFeaturevector; j++)
                            {
                                var positiveDiagonalStartPointX = potentialEvent[sliceIndex].PositiveDiagonalVector[j];
                                var positiveDiagonalEndPointX = query[sliceIndex].PositiveDiagonalVector[j];
                                var negativeDiagonalStartPointX = potentialEvent[sliceIndex].NegativeDiagonalVector[j];
                                var negativeDiagonalEndPointX = query[sliceIndex].NegativeDiagonalVector[j];

                                positiveDiagonalDistance += Distance.EuclideanDistanceForCordinates(positiveDiagonalStartPointX, 0.0, positiveDiagonalEndPointX, 0.0);
                                negativeDiagonalDistance += Distance.EuclideanDistanceForCordinates(negativeDiagonalStartPointX, 0.0, negativeDiagonalEndPointX, 0.0);
                            }// end for
                        //}// end if (double check)
                        //else
                        //{
                        //    if(checkNullFeatureVector(potentialEvent[sliceIndex]))
                        //    {
                        //        result += 10000000;
                        //    }
                        //}
                        result = horizontalDistance + verticalDistance + positiveDiagonalDistance + negativeDiagonalDistance;
                    }// end else
                }// end for
            }// end if
            else
            {
                result = 100000000;
            }

            return result;
        }

        public static bool checkNullFeatureVector(RidgeNeighbourhoodFeatureVector featureVector)
        {
            var result = 0;
            var numberOfHorizontalFeatureVectorBit = featureVector.HorizontalVector.Count();
            var numberOfDiagonalFeatureVectorBit = featureVector.PositiveDiagonalVector.Count();
            if (featureVector != null)
            {
                for (int i = 0; i < numberOfHorizontalFeatureVectorBit; i++)
                {
                    if (featureVector.HorizontalVector[i] != 0)
                    {
                        result++;
                    }
                    if (featureVector.HorizontalVector[i] != 0)
                    {
                        result++;
                    }
                }
                for (int j = 0; j < numberOfDiagonalFeatureVectorBit; j++)
                {
                    if (featureVector.PositiveDiagonalVector[j] != 0)
                    {
                        result++;
                    }
                    if (featureVector.NegativeDiagonalVector[j] != 0)
                    {
                        result++;
                    }
                }
            }
            if (result == 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static bool checkNullFeatureVectorList(List<RidgeNeighbourhoodFeatureVector> featureVectorList)
        {
            var result = 0;
            var numberOfSlices = featureVectorList.Count();
            if (featureVectorList != null)
            {

                for (int sliceIndex = 0; sliceIndex < numberOfSlices; sliceIndex++)
                {
                    if (StatisticalAnalysis.NumberOfpoiInSlice(featureVectorList[sliceIndex]) == 0)
                    {
                        result++;
                    }
                }
            }
            if (result == numberOfSlices)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

    }
}
