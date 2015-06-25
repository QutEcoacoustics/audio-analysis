namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using Representations;
    using Dong.Felt.Features;
    using System.Globalization;
using System.IO;


    enum MatchIndex//public enum MatchIndex
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

    public class SimilarityMatching
    {

        #region Public Properties

        /// <summary>
        /// Gets or sets the SimilarityScore, it can be derived from the calculationg of similarity score. 
        /// </summary>
        public double SimilarityScore { get; set; }

        #endregion

        #region Public Methods

        public static List<Candidates> CandidateCalculation(MFCC query, List<MFCC> candidates)
        {
            var results = new List<Candidates>();
            foreach (var c in candidates)
            {
                var distance = DistanceFor2MFCCs(query, c);
                var audioFile = Path.ChangeExtension(c.audioFile,".wav");
                var item = new Candidates(distance, c.StartTime, c.EndTime - c.StartTime, 0.0, 0.0, audioFile);
                results.Add(item);
            }
            return results;
        }

        public static double DistanceFor2MFCCs(MFCC a, MFCC b)
        {
            var normalisedMFCCsA = StatisticalAnalysis.NormalizeMFCCData(a.MFCCoefficients);
            var normalisedMFCCsB = StatisticalAnalysis.NormalizeMFCCData(b.MFCCoefficients);
            var distance = Distance.AvgDistanceForLists(normalisedMFCCsA, normalisedMFCCsB);           
            return distance;
        }

        public static double DistanceForOrientationHistogram(RidgeNeighbourhoodFeatureVector instance, RidgeNeighbourhoodFeatureVector template)
        {
            var distance = 0;
            var numberOfBitCount = instance.VerticalBitVector.Count();
            for (int i = 0; i < numberOfBitCount; i++)
            {
                //sumV = sumV + Math.Abs(instance. - template);
            }
            return distance;
        }

        /// <summary>
        /// Calculate the average distance between two featureVectors. 
        /// </summary>
        /// <param name="instance"> </param>
        /// <param name="template"> </param>
        /// <returns>
        /// return the avgDistance.
        /// </returns>
        public static double AvgDistance(RidgeNeighbourhoodFeatureVector instance, RidgeNeighbourhoodFeatureVector template)
        {
            var avgdistance = 0.0;
            var numberOfScaleCount = instance.VerticalBitVector.Count();
            var sumDistance = distanceForBitFeatureVector(instance, template);
            avgdistance = sumDistance / numberOfScaleCount;
            return avgdistance;
        }

        /// <summary>
        /// Calculate the sum distance of each bit in a neighbourhood between two featureVectors. one is from the template, one is from candidate event.
        /// The distance rule follows the Manhattan distance funtion. 
        /// Especially, the vector is composed mainly of "verticalBit" and "horizontalBit". 
        /// </summary>
        /// <param name="instance">the featureVector of the candidate needs to be compared.</param>
        /// <param name="template">a particular species templage' featureVector needs to be compared.</param>
        /// <returns>return the sum of distance of all feature vector bits.</returns>
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

        public static double ScoreOver2EventRegions(RegionRepresentation q, RegionRepresentation c, int n, double weight1, double weight2)
        {
            var score = 0.0;
            var relevantQueryVRepresentation = Indexing.GetRelevantIndexInEvents(q, q.vEventList);
            var relevantQueryHRepresentation = Indexing.GetRelevantIndexInEvents(q, q.hEventList);
            var relevantQueryPRepresentation = Indexing.GetRelevantIndexInEvents(q, q.pEventList);
            var relevantQueryNRepresentation = Indexing.GetRelevantIndexInEvents(q, q.nEventList);

            // calculate score for vEvents, hEvents, pEvents, nEvents
            var vScore = Indexing.ScoreOver2Events2(relevantQueryVRepresentation, c, c.vEventList, n);
            var hScore = Indexing.ScoreOver2Events2(relevantQueryHRepresentation, c, c.hEventList, n);
            var pScore = Indexing.ScoreOver2Events2(relevantQueryPRepresentation, c, c.pEventList, n);
            var nScore = Indexing.ScoreOver2Events2(relevantQueryNRepresentation, c, c.nEventList, n);
            // Get the average score
            if (q.NotNullEventListCount == 4 && ((weight1 + weight2) > 0.5))
            {
                if (q.MajorEvent.InsideRidgeOrientation == 0)
                {
                    score = vScore * weight1 + hScore * weight2 + (pScore + nScore) * weight2 / 2.0;
                }
                if (q.MajorEvent.InsideRidgeOrientation == 1)
                {
                    score = hScore * weight1 + vScore * weight2 + (pScore + nScore) * weight2 / 2.0;
                }
                if (q.MajorEvent.InsideRidgeOrientation == 2)
                {
                    score = pScore * weight1 + vScore * weight2 + (hScore + nScore) * weight2 / 2.0;
                }
                if (q.MajorEvent.InsideRidgeOrientation == 3)
                {
                    score = nScore * weight1 + vScore * weight2 + (hScore + pScore) * weight2 / 2.0;
                }
            }
            else
            {
                score = (vScore + hScore + pScore + nScore) / q.NotNullEventListCount;
            }
            return score;
        }

        // N is count of histogram, max is the total number of frequencybins
        public static double SimilarityWeight(int n, double max, double real)
        {
            var result = 0.0;
            var histogramUnit = n / max;
            if (real == 0.0)
            {
                result = 1.0;
            }
            else
            {
                result = Math.Floor((1 - real) / histogramUnit) * histogramUnit;
            }
            
            return result;
        }

        public static double ScoreOver2EventList(List<EventBasedRepresentation> events1,
            List<EventBasedRepresentation> events2, int n)
        {
            var overlapScore = 0.0;            
            if (events1.Count > 0 && events2.Count > 0)
            {
                foreach (var q in events1)
                {
                    // find the N cloest event to compare
                    var nClosestEventList = Indexing.FindNCloestEvents(events2, q, n);
                    var index = Indexing.FindMaximumScoreEvent(nClosestEventList, q);
                    // Another check on frame offset
                    var frameCheck = Indexing.OverFrameOffset(q.Left, nClosestEventList[index].Left, 0, 10);
                    var subScore = 0.0;
                    if (frameCheck)
                    {
                        var leftAnchor = nClosestEventList[index].Left;
                        var qLeft = leftAnchor;
                        var overlap = StatisticalAnalysis.EventOverlapInPixel(
                            qLeft,
                            q.Bottom,
                            qLeft + q.Width,
                            q.Bottom + q.Height,
                            nClosestEventList[index].Left,
                            nClosestEventList[index].Bottom,
                            nClosestEventList[index].Left + nClosestEventList[index].Width,
                            nClosestEventList[index].Bottom + nClosestEventList[index].Height);
                        subScore = ((double)overlap / q.Area + (double)overlap / nClosestEventList[index].Area) / 2.0;                       
                    }
                    overlapScore += subScore;
                }
                overlapScore /= events1.Count;
            }
            var result = overlapScore;
            return result;
        }
        /// <summary>
        /// According to the relationship of distance and similarityScore, the farer the distance between two feature vectors,
        /// the less similarityScore can be obtained. 
        /// </summary>
        /// <param name="avgDistance"></param>
        /// <param name="neighbourhoodSize"></param>
        /// <returns></returns>
        public static double SimilarityScoreForAvgDistance(double avgDistance, int neighbourhoodSize)
        {
            var similarityScore = 1 - avgDistance / neighbourhoodSize;

            return similarityScore;
        }

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

        public static double WeightedDistanceScoreRegionFeature(Feature query, Feature candidate)
        {
            var result = 0.0;
            if (query != null && candidate != null)
            {
                for (int index = 0; index < query.orientationHistogram.Count; index++)
                {
                    var orientationHisDifference = Math.Abs(query.orientationHistogram[index] -
                        candidate.orientationHistogram[index]);
                    result += orientationHisDifference;
                }
            }
            return result;
        }

        public static double EuclideanDistanceScore(RegionRepresentation query, RegionRepresentation candidate,
            double matchedDistanceThreshold, double weight)
        {
            var result = 0.0;
            var notNullPOIInQuery = 0;
            //var sumDisdistance = 0.0;
            var matchedNotNullPOICount = 0;
            var matchedNullPOICount = 0;
            var queryPOIMatrix = query.fftFeatures;
            var rowsCount = queryPOIMatrix.GetLength(0);
            var colsCount = queryPOIMatrix.GetLength(1);
            if (query != null && candidate != null)
            {
                if (candidate.fftFeatures != null)
                {
                    var candidatePOIMatrix = candidate.fftFeatures;
                    for (int i = 0; i < rowsCount; i++)
                    {
                        for (int j = 0; j < colsCount; j++)
                        {
                            if (queryPOIMatrix[i, j] != null)
                            {
                                if (queryPOIMatrix[i, j].fftMatrix != null)
                                {
                                    notNullPOIInQuery++;
                                    /// One is based on Euclidean distance
                                    //if (candidatePOIMatrix[i, j] != null && candidatePOIMatrix[i, j].fftMatrix != null)
                                    //{
                                    //    var queryFFTMatrix = queryPOIMatrix[i, j].fftMatrix;
                                    //    var candidateFFTMatrix = candidatePOIMatrix[i, j].fftMatrix;
                                    //    var fftDifference = 0.0;
                                    //    for (int r = 0; r < queryFFTMatrix.GetLength(0); r++)
                                    //    {
                                    //        for (int c = 0; c < queryFFTMatrix.GetLength(1); c++)
                                    //        {
                                    //            fftDifference += Math.Sqrt(Math.Pow((queryFFTMatrix[r, c] - candidateFFTMatrix[r, c]), 2.0));
                                    //        }
                                    //    }
                                    //    sumDisdistance += fftDifference;
                                    //}
                                    /// One is based on position matching                          
                                    if (candidatePOIMatrix[i, j] != null && candidatePOIMatrix[i, j].fftMatrix != null)
                                    {
                                        var queryFFTMatrix = queryPOIMatrix[i, j].fftMatrix;
                                        var candidateFFTMatrix = candidatePOIMatrix[i, j].fftMatrix;
                                        var fftDifference = 0.0;
                                        for (int r = 0; r < queryFFTMatrix.GetLength(0); r++)
                                        {
                                            for (int c = 0; c < queryFFTMatrix.GetLength(1); c++)
                                            {
                                                fftDifference += Math.Sqrt(Math.Pow((queryFFTMatrix[r, c] - candidateFFTMatrix[r, c]), 2.0));
                                            }
                                        }
                                        if (fftDifference < matchedDistanceThreshold)
                                        {
                                            matchedNotNullPOICount++;
                                        }
                                    }
                                }
                                else if (candidatePOIMatrix[i, j] != null && candidatePOIMatrix[i, j].fftMatrix == null)
                                {
                                    matchedNullPOICount++;
                                }
                            }
                        }
                    }
                }
            }
            /// The one is based on purely Euclidean distance
            //if (notNullPOIInQuery != 0)
            //{
            //    result = sumDisdistance / notNullPOIInQuery;
            //}         
            /// The one is based on position matching
            if (notNullPOIInQuery != 0)
            {
                result = (double)(matchedNotNullPOICount + weight * matchedNullPOICount) / (rowsCount * colsCount);
            }
            return result;
        }

        /// <summary>
        /// This weighted Euclidean distance function is little bit different from the one below this method. The distance result is obtained 
        /// based on the sum of sub-region in the process of calculation. There are four properties as feature vector. 
        /// -changed the comparison, only compare the nh which the query has something in there. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidate"></param>
        /// <param name="weight1"></param>
        /// <param name="weight2"></param>
        /// <returns></returns>
        public static double WeightedDistanceScoreRegionRepresentation3(List<RegionRepresentation> query, List<RegionRepresentation> candidate,
            double weight1, double weight2, double weight3, double weight4)
        {
            var result = 0.0;
            if (query != null && candidate != null)
            {
                var nhCount = query[0].NhCountInCol * query[0].NhCountInRow;
                var nhSumNotNull = 0.0;
                var nhSumNull = 0.0;
                for (int index = 0; index < nhCount; index++)
                {
                    if (query[index].magnitude != 100)
                    {
                        var queryMagnitude = query[index].magnitude;
                        var queryOrientation = query[index].orientation;
                        var queryDominantOrientationType = query[index].dominantOrientationType;
                        var queryDominantPoiCount = query[index].dominantPOICount;
                        var candidateMagnitude = candidate[index].magnitude;
                        var candidateOrientation = candidate[index].orientation;
                        var candidateDominantOrientationType = candidate[index].dominantOrientationType;
                        var candidateDominantPoiCount = candidate[index].dominantPOICount;
                        var orientationDifference = Math.Abs(queryOrientation - candidateOrientation);
                        var magnitudeDifference = Math.Abs(queryMagnitude - candidateMagnitude);
                        var orientationTypeDiff = Math.Abs(queryDominantOrientationType - candidateDominantOrientationType);
                        var dominantPoiCountDiff = Math.Abs(queryDominantPoiCount - candidateDominantPoiCount);
                        nhSumNotNull += Math.Sqrt(weight1 * Math.Pow(magnitudeDifference, 2) + weight2 * Math.Pow(orientationDifference, 2)
                            + weight3 * Math.Pow(orientationTypeDiff, 2) + weight4 * Math.Pow(dominantPoiCountDiff, 2));
                    }
                    else
                    {
                        var queryMagnitude = query[index].magnitude;
                        var queryOrientation = query[index].orientation;
                        var queryDominantOrientationType = query[index].dominantOrientationType;
                        var queryDominantPoiCount = query[index].dominantPOICount;
                        var candidateMagnitude = candidate[index].magnitude;
                        var candidateOrientation = candidate[index].orientation;
                        var candidateDominantOrientationType = candidate[index].dominantOrientationType;
                        var candidateDominantPoiCount = candidate[index].dominantPOICount;
                        var orientationDifference = Math.Abs(queryOrientation - candidateOrientation);
                        var magnitudeDifference = Math.Abs(queryMagnitude - candidateMagnitude);
                        var orientationTypeDiff = Math.Abs(queryDominantOrientationType - candidateDominantOrientationType);
                        var dominantPoiCountDiff = Math.Abs(queryDominantPoiCount - candidateDominantPoiCount);
                        nhSumNull += Math.Sqrt(weight1 * Math.Pow(magnitudeDifference, 2) + weight2 * Math.Pow(orientationDifference, 2)
                            + weight3 * Math.Pow(orientationTypeDiff, 2) + weight4 * Math.Pow(dominantPoiCountDiff, 2));
                    }
                }
                result = nhSumNotNull + nhSumNull;
            }
            return result;
        }

        /// <summary>
        /// This weighted Euclidean distance function is little bit different from the one below this method. The distance result is obtained 
        /// based on the sum of sub-region in the process of calculation.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidate"></param>
        /// <param name="weight1"></param>
        /// <param name="weight2"></param>
        /// <returns></returns>
        public static double WeightedDistanceScoreRegionRepresentation2(List<RegionRepresentation> query, List<RegionRepresentation> candidate, double weight1, double weight2)
        {
            var result = 0.0;
            if (query != null && candidate != null)
            {
                var nhCount = query[0].NhCountInCol * query[0].NhCountInRow;
                //var nhSum1 = 0.0;
                var nhSum2 = 0.0;
                for (int index = 0; index < nhCount; index++)
                {

                    var queryMagnitude = query[index].magnitude;
                    var queryOrientation = query[index].orientation;
                    var candidateMagnitude = candidate[index].magnitude;
                    var candidateOrientation = candidate[index].orientation;

                    var orientationDifference = Math.Abs(queryOrientation - candidateOrientation);
                    var magnitudeDifference = Math.Abs(queryMagnitude - candidateMagnitude);
                    nhSum2 += Math.Sqrt(weight1 * Math.Pow(magnitudeDifference, 2) + weight2 * Math.Pow(orientationDifference, 2));

                }
                result = nhSum2;
            }
            return result;
        }


        /// <summary>
        /// This version is used for calculating distance based on feature set 6.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidate"></param>
        /// <param name="poiCountThreshold"></param>
        /// <returns></returns>
        public static double DistanceFeature8HoGBased(List<RegionRepresentation> query, 
            List<RegionRepresentation> candidate, int poiCountThreshold,double weight1, double weight2)
        {
            var result = 0.0;
            if (query != null && candidate != null)
            {
                var nhCount = query[0].NhCountInCol * query[0].NhCountInRow;
                var Orientation0POIHistDiff = 0.0;
                var Orientation1POIHistDiff = 0.0;
                var Orientation2POIHistDiff = 0.0;
                var Orientation3POIHistDiff = 0.0;
                var Orientation4POIHistDiff = 0.0;
                var Orientation5POIHistDiff = 0.0;
                var Orientation6POIHistDiff = 0.0;
                var Orientation7POIHistDiff = 0.0;
                //var pOICountPercentageDiff = Math.Abs(queryPOICountPercentage - candidatePOICountPercentage);               
                var max0POIHistDiff = 1.0;
                var max1POIHistDiff = 1.0;
                var max2POIHistDiff = 1.0;
                var max3POIHistDiff = 1.0;
                var max4POIHistDiff = 1.0;
                var max5POIHistDiff = 1.0;
                var max6POIHistDiff = 1.0;
                var max7POIHistDiff = 1.0;
                var matchedNhCount = 0;
                var maxDistance = Math.Sqrt(max0POIHistDiff + max1POIHistDiff + max2POIHistDiff + max3POIHistDiff
                            + max4POIHistDiff + max5POIHistDiff +
                            max6POIHistDiff + max7POIHistDiff);
                for (int index = 0; index < nhCount; index++)
                {
                    // to check whether they match
                    if (query[index].POICount <= poiCountThreshold && candidate[index].POICount <= poiCountThreshold)
                    {
                        result += weight1;
                        matchedNhCount++;
                    }
                    else if (query[index].POICount > poiCountThreshold && candidate[index].POICount > poiCountThreshold)
                    {
                        matchedNhCount++;
                        var queryOrientation0POIHistogram = query[index].Orientation0POIMagnitude;
                        var queryOrientation1POIHistogram = query[index].Orientation1POIMagnitude;
                        var queryOrientation2POIHistogram = query[index].Orientation2POIMagnitude;
                        var queryOrientation3POIHistogram = query[index].Orientation3POIMagnitude;
                        var queryOrientation4POIHistogram = query[index].Orientation4POIMagnitude;
                        var queryOrientation5POIHistogram = query[index].Orientation5POIMagnitude;
                        var queryOrientation6POIHistogram = query[index].Orientation6POIMagnitude;
                        var queryOrientation7POIHistogram = query[index].Orientation7POIMagnitude;
                        var queryPOICountPercentage = query[index].POICountPercentage;                      

                        var candidateOrientation0POIHistogram = candidate[index].Orientation0POIMagnitude;
                        var candidateOrientation1POIHistogram = candidate[index].Orientation1POIMagnitude;
                        var candidateOrientation2POIHistogram = candidate[index].Orientation2POIMagnitude;
                        var candidateOrientation3POIHistogram = candidate[index].Orientation3POIMagnitude;
                        var candidateOrientation4POIHistogram = candidate[index].Orientation4POIMagnitude;
                        var candidateOrientation5POIHistogram = candidate[index].Orientation5POIMagnitude;
                        var candidateOrientation6POIHistogram = candidate[index].Orientation6POIMagnitude;
                        var candidateOrientation7POIHistogram = candidate[index].Orientation7POIMagnitude;
                        //var candidatePOICountPercentage = candidate[index].POICountPercentage;                       
                        Orientation0POIHistDiff = Math.Abs(queryOrientation0POIHistogram - candidateOrientation0POIHistogram);
                        Orientation1POIHistDiff = Math.Abs(queryOrientation1POIHistogram - candidateOrientation1POIHistogram);
                        Orientation2POIHistDiff = Math.Abs(queryOrientation2POIHistogram - candidateOrientation2POIHistogram);
                        Orientation3POIHistDiff = Math.Abs(queryOrientation3POIHistogram - candidateOrientation3POIHistogram);
                        Orientation4POIHistDiff = Math.Abs(queryOrientation4POIHistogram - candidateOrientation4POIHistogram);
                        Orientation5POIHistDiff = Math.Abs(queryOrientation5POIHistogram - candidateOrientation5POIHistogram);
                        Orientation6POIHistDiff = Math.Abs(queryOrientation6POIHistogram - candidateOrientation6POIHistogram);
                        Orientation7POIHistDiff = Math.Abs(queryOrientation7POIHistogram - candidateOrientation7POIHistogram);
                        //var pOICountPercentageDiff = Math.Abs(queryPOICountPercentage - candidatePOICountPercentage);                     

                        var euclideanDistance = Math.Sqrt(Math.Pow(Orientation0POIHistDiff, 2) + Math.Pow(Orientation1POIHistDiff, 2)
                                          + Math.Pow(Orientation2POIHistDiff, 2) + Math.Pow(Orientation3POIHistDiff, 2)
                                          + Math.Pow(Orientation4POIHistDiff, 2) + Math.Pow(Orientation5POIHistDiff, 2)
                                          + Math.Pow(Orientation6POIHistDiff, 2) + Math.Pow(Orientation7POIHistDiff, 2)
                                         );
                        result += weight2 * (1 - euclideanDistance / maxDistance);
                    }
                }
                var matchedPercentage = matchedNhCount / (double)nhCount;
                var averageSimilarityScore = result / matchedNhCount;
                result = matchedPercentage * averageSimilarityScore;
                result = Convert.ToDouble(result.ToString("F03", CultureInfo.InvariantCulture));
            }
            return result;
        }
       
        /// <summary>
        /// This version is used for calculating distance based on feature set 6.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidate"></param>
        /// <param name="poiCountThreshold"></param>
        /// <returns></returns>
        public static double DistanceFeature9Representation(List<RegionRepresentation> query,
            List<RegionRepresentation> candidate, int poiCountThreshold, double weight1, double weight2)
        {
            var result = 0.0;
            if (query != null && candidate != null)
            {
                var nhCount = query[0].NhCountInCol * query[0].NhCountInRow;
                
                var columnEnergyEntropyDiff = 0.0;
                var rowEnergyEntropyDiff = 0.0;               
                var maxColEnergyEntroDiff = 1.0;
                var maxRowEnergyEntroDiff = 1.0;
                var matchedNhCount = 0;
                for (int index = 0; index < nhCount; index++)
                {
                    // to check whether they match
                    if (query[index].POICount <= poiCountThreshold && candidate[index].POICount <= poiCountThreshold)
                    {
                        result += weight1;
                        matchedNhCount++;
                    }
                    else if (query[index].POICount > poiCountThreshold && candidate[index].POICount > poiCountThreshold)
                    {
                        matchedNhCount++;                        
                        var queryColumnEnergyEntropy = query[index].ColumnEnergyEntropy;
                        var queryRowEnergyEntropy = query[index].RowEnergyEntropy;
                        var candidateColumnEnergyEntropy = candidate[index].ColumnEnergyEntropy;
                        var candidateRowEnergyEntropy = candidate[index].RowEnergyEntropy;                       
                        columnEnergyEntropyDiff = Math.Abs(queryColumnEnergyEntropy - candidateColumnEnergyEntropy);
                        rowEnergyEntropyDiff = Math.Abs(queryRowEnergyEntropy - candidateRowEnergyEntropy);

                        var euclideanDistance =Math.Sqrt(Math.Pow(columnEnergyEntropyDiff, 2) + Math.Pow(rowEnergyEntropyDiff, 2));                      
                        var maxDistance = Math.Sqrt(maxColEnergyEntroDiff + maxRowEnergyEntroDiff);                       
                        result += weight2 * (1 - euclideanDistance / maxDistance);
                    }
                }
                var matchedPercentage = matchedNhCount / (double)nhCount;
                var averageSimilarityScore = result / matchedNhCount;
                result = matchedPercentage * averageSimilarityScore;
                result = Convert.ToDouble(result.ToString("F03", CultureInfo.InvariantCulture));
            }
            return result;
        }

        /// <summary>
        /// This version is used for calculating distance based on feature set 6.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidate"></param>
        /// <param name="poiCountThreshold"></param>
        /// <returns></returns>
        public static double DistanceFeature10Calculation(List<RegionRepresentation> query,
            List<RegionRepresentation> candidate, int poiCountThreshold)
        {
            var result = 0.0;
            if (query != null && candidate != null)
            {
                var nhCount = query[0].NhCountInCol * query[0].NhCountInRow;
                var matchedNhCount = 0;
                for (int index = 0; index < nhCount; index++)
                {
                    // to check whether they match
                    var queryPoints = query[index].PointList;
                    var candidatePoints = candidate[index].PointList;
                    if (query[index].POICount <= poiCountThreshold && candidate[index].POICount <= poiCountThreshold)
                    {                     
                        matchedNhCount++;
                    }
                    else if (query[index].POICount > poiCountThreshold && candidate[index].POICount > poiCountThreshold)
                    {
                        matchedNhCount++;
                        if (queryPoints != null && candidatePoints != null)
                        {
                            result += Distance.HausdorffDistanceForPoints(query[index].PointList, candidate[index].PointList);
                        }
                        else
                        {
                            result += 100;
                        }
                    }
                }
                var matchedPercentage = matchedNhCount / (double)nhCount;
                var averageSimilarityScore = result / matchedNhCount;
                result = matchedPercentage * averageSimilarityScore;
                result = Convert.ToDouble(result.ToString("F03", CultureInfo.InvariantCulture));
            }
            return result;
        }

        /// <summary>
        /// This version is used for calculating distance based on feature set 12 HoG 8 (count based) + feature 9.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidate"></param>
        /// <param name="poiCountThreshold"></param>
        /// <returns></returns>
        public static double DistanceFeature12Based(List<RegionRepresentation> query,
            List<RegionRepresentation> candidate, int poiCountThreshold, double weight1, double weight2)
        {
            var result = 0.0;
            if (query != null && candidate != null)
            {
                var nhCount = query[0].NhCountInCol * query[0].NhCountInRow;
                var Orientation0POIHistDiff = 0.0;
                var Orientation1POIHistDiff = 0.0;
                var Orientation2POIHistDiff = 0.0;
                var Orientation3POIHistDiff = 0.0;
                var Orientation4POIHistDiff = 0.0;
                var Orientation5POIHistDiff = 0.0;
                var Orientation6POIHistDiff = 0.0;
                var Orientation7POIHistDiff = 0.0;
                var columnEnergyEntropyDiff = 0.0;
                var rowEnergyEntropyDiff = 0.0;
                var max0POIHistDiff = 1.0;
                var max1POIHistDiff = 1.0;
                var max2POIHistDiff = 1.0;
                var max3POIHistDiff = 1.0;
                var max4POIHistDiff = 1.0;
                var max5POIHistDiff = 1.0;
                var max6POIHistDiff = 1.0;
                var max7POIHistDiff = 1.0;
                var maxColEnergyEntroDiff = 1.0;
                var maxRowEnergyEntroDiff = 1.0;
                var matchedNhCount = 0;
                var maxDistance = Math.Sqrt(max0POIHistDiff + max1POIHistDiff + max2POIHistDiff + max3POIHistDiff
                            + max4POIHistDiff + max5POIHistDiff +
                            max6POIHistDiff + max7POIHistDiff +
                            maxColEnergyEntroDiff + maxRowEnergyEntroDiff);
                for (int index = 0; index < nhCount; index++)
                {
                    // to check whether they match
                    if (query[index].POICount <= poiCountThreshold && candidate[index].POICount <= poiCountThreshold)
                    {
                        result += weight1;
                        matchedNhCount++;
                    }
                    else if (query[index].POICount > poiCountThreshold && candidate[index].POICount > poiCountThreshold)
                    {
                        matchedNhCount++;
                        var queryOrientation0POIHistogram = query[index].Orientation0POIMagnitude;
                        var queryOrientation1POIHistogram = query[index].Orientation1POIMagnitude;
                        var queryOrientation2POIHistogram = query[index].Orientation2POIMagnitude;
                        var queryOrientation3POIHistogram = query[index].Orientation3POIMagnitude;
                        var queryOrientation4POIHistogram = query[index].Orientation4POIMagnitude;
                        var queryOrientation5POIHistogram = query[index].Orientation5POIMagnitude;
                        var queryOrientation6POIHistogram = query[index].Orientation6POIMagnitude;
                        var queryOrientation7POIHistogram = query[index].Orientation7POIMagnitude;
                        var queryColumnEnergyEntropy = query[index].ColumnEnergyEntropy;
                        var queryRowEnergyEntropy = query[index].RowEnergyEntropy;

                        var candidateOrientation0POIHistogram = candidate[index].Orientation0POIMagnitude;
                        var candidateOrientation1POIHistogram = candidate[index].Orientation1POIMagnitude;
                        var candidateOrientation2POIHistogram = candidate[index].Orientation2POIMagnitude;
                        var candidateOrientation3POIHistogram = candidate[index].Orientation3POIMagnitude;
                        var candidateOrientation4POIHistogram = candidate[index].Orientation4POIMagnitude;
                        var candidateOrientation5POIHistogram = candidate[index].Orientation5POIMagnitude;
                        var candidateOrientation6POIHistogram = candidate[index].Orientation6POIMagnitude;
                        var candidateOrientation7POIHistogram = candidate[index].Orientation7POIMagnitude;
                        var candidateColumnEnergyEntropy = candidate[index].ColumnEnergyEntropy;
                        var candidateRowEnergyEntropy = candidate[index].RowEnergyEntropy;

                        Orientation0POIHistDiff = Math.Abs(queryOrientation0POIHistogram - candidateOrientation0POIHistogram);
                        Orientation1POIHistDiff = Math.Abs(queryOrientation1POIHistogram - candidateOrientation1POIHistogram);
                        Orientation2POIHistDiff = Math.Abs(queryOrientation2POIHistogram - candidateOrientation2POIHistogram);
                        Orientation3POIHistDiff = Math.Abs(queryOrientation3POIHistogram - candidateOrientation3POIHistogram);
                        Orientation4POIHistDiff = Math.Abs(queryOrientation4POIHistogram - candidateOrientation4POIHistogram);
                        Orientation5POIHistDiff = Math.Abs(queryOrientation5POIHistogram - candidateOrientation5POIHistogram);
                        Orientation6POIHistDiff = Math.Abs(queryOrientation6POIHistogram - candidateOrientation6POIHistogram);
                        Orientation7POIHistDiff = Math.Abs(queryOrientation7POIHistogram - candidateOrientation7POIHistogram);
                        columnEnergyEntropyDiff = Math.Abs(queryColumnEnergyEntropy - candidateColumnEnergyEntropy);
                        rowEnergyEntropyDiff = Math.Abs(queryRowEnergyEntropy - candidateRowEnergyEntropy);
                        var euclideanDistance = Math.Sqrt(Math.Pow(Orientation0POIHistDiff, 2) + Math.Pow(Orientation1POIHistDiff, 2)
                                          + Math.Pow(Orientation2POIHistDiff, 2) + Math.Pow(Orientation3POIHistDiff, 2)
                                          + Math.Pow(Orientation4POIHistDiff, 2) + Math.Pow(Orientation5POIHistDiff, 2)
                                          + Math.Pow(Orientation6POIHistDiff, 2) + Math.Pow(Orientation7POIHistDiff, 2)
                                          + Math.Pow(columnEnergyEntropyDiff, 2) + Math.Pow(rowEnergyEntropyDiff, 2)
                                         );
                        result += weight2 * (1 - euclideanDistance / maxDistance);
                    }
                }
                var matchedPercentage = matchedNhCount / (double)nhCount;
                var averageSimilarityScore = result / matchedNhCount;
                result = matchedPercentage * averageSimilarityScore;
                result = Convert.ToDouble(result.ToString("F03", CultureInfo.InvariantCulture));
            }
            return result;
        }

        /// <summary>
        /// This version is used for calculating distance based on feature set 12 HoG 8 (count based) + feature 9.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidate"></param>
        /// <param name="poiCountThreshold"></param>
        /// <returns></returns>
        public static double DistanceFeature21Based(List<RegionRepresentation> query,
            List<RegionRepresentation> candidate)    
        {
            var result = 0.0;
            if (query != null && candidate != null)
            {             
                var queryFV = FeatureExtraction.ExtractColRowRidge(query);
                var candidateFV = FeatureExtraction.ExtractColRowRidge(candidate);
                var fvLength = queryFV[0];
                for (int index = 0; index < fvLength; index++)
                {                  
                    var fBitDiff = Math.Abs(queryFV[index] - candidateFV[index]);                   
                    var euclideanDistance = Math.Sqrt(Math.Pow(Convert.ToDouble(fBitDiff), 2.0));
                        result += euclideanDistance;
                }              
                result = Convert.ToDouble(result.ToString("F03", CultureInfo.InvariantCulture));
            }
            return result;
        }

        public static double DistanceFeature14Based(List<RegionRepresentation> query,
            List<RegionRepresentation> candidate, int poiCountThreshold, double weight1, double weight2)
        {
            var result = 0.0;
            if (query != null && candidate != null)
            {
                var nhCount = query[0].NhCountInCol * query[0].NhCountInRow;
                var Orientation0POIHistDiff = 0.0;
                var Orientation1POIHistDiff = 0.0;
                var Orientation2POIHistDiff = 0.0;
                var Orientation3POIHistDiff = 0.0;
                
                var max0POIHistDiff = 1.0;
                var max1POIHistDiff = 1.0;
                var max2POIHistDiff = 1.0;
                var max3POIHistDiff = 1.0;
               
                var matchedNhCount = 0;
                var maxDistance = Math.Sqrt(max0POIHistDiff + max1POIHistDiff + max2POIHistDiff + max3POIHistDiff);
                for (int index = 0; index < nhCount; index++)
                {
                    // to check whether they match
                    if (query[index].POICount <= poiCountThreshold && candidate[index].POICount <= poiCountThreshold)
                    {
                        result += weight1;
                        matchedNhCount++;
                    }
                    else if (query[index].POICount > poiCountThreshold && candidate[index].POICount > poiCountThreshold)
                    {
                        matchedNhCount++;
                        var queryOrientation0POIHistogram = query[index].Orientation0POIMagnitude;
                        var queryOrientation1POIHistogram = query[index].Orientation1POIMagnitude;
                        var queryOrientation2POIHistogram = query[index].Orientation2POIMagnitude;
                        var queryOrientation3POIHistogram = query[index].Orientation3POIMagnitude;
                        
                        var candidateOrientation0POIHistogram = candidate[index].Orientation0POIMagnitude;
                        var candidateOrientation1POIHistogram = candidate[index].Orientation1POIMagnitude;
                        var candidateOrientation2POIHistogram = candidate[index].Orientation2POIMagnitude;
                        var candidateOrientation3POIHistogram = candidate[index].Orientation3POIMagnitude;
                       

                        Orientation0POIHistDiff = Math.Abs(queryOrientation0POIHistogram - candidateOrientation0POIHistogram);
                        Orientation1POIHistDiff = Math.Abs(queryOrientation1POIHistogram - candidateOrientation1POIHistogram);
                        Orientation2POIHistDiff = Math.Abs(queryOrientation2POIHistogram - candidateOrientation2POIHistogram);
                        Orientation3POIHistDiff = Math.Abs(queryOrientation3POIHistogram - candidateOrientation3POIHistogram);
                        
                        var euclideanDistance = Math.Sqrt(Math.Pow(Orientation0POIHistDiff, 2) + Math.Pow(Orientation1POIHistDiff, 2)
                                          + Math.Pow(Orientation2POIHistDiff, 2) + Math.Pow(Orientation3POIHistDiff, 2)                                          
                                         );
                        result += weight2 * (1 - euclideanDistance / maxDistance);
                    }
                }
                var matchedPercentage = matchedNhCount / (double)nhCount;
                var averageSimilarityScore = result / matchedNhCount;
                result = matchedPercentage * averageSimilarityScore;
                result = Convert.ToDouble(result.ToString("F03", CultureInfo.InvariantCulture));
            }
            return result;
        }

        public static double DistanceFeature16Based(List<RegionRepresentation> query,
            List<RegionRepresentation> candidate, int poiCountThreshold, double weight1, double weight2)       
        {
            var result = 0.0;
            if (query != null && candidate != null)
            {
                var nhCount = query[0].NhCountInCol * query[0].NhCountInRow;
                var Orientation0POIHistDiff = 0.0;
                var Orientation1POIHistDiff = 0.0;
                var Orientation2POIHistDiff = 0.0;
                var Orientation3POIHistDiff = 0.0;               
                var columnEnergyEntropyDiff = 0.0;
                var rowEnergyEntropyDiff = 0.0;
                var max0POIHistDiff = 1.0;
                var max1POIHistDiff = 1.0;
                var max2POIHistDiff = 1.0;
                var max3POIHistDiff = 1.0;                
                var maxColEnergyEntroDiff = 1.0;
                var maxRowEnergyEntroDiff = 1.0;
                var matchedNhCount = 0;
                var maxDistance = Math.Sqrt(max0POIHistDiff + max1POIHistDiff + max2POIHistDiff + max3POIHistDiff
                            + maxColEnergyEntroDiff + maxRowEnergyEntroDiff);
                for (int index = 0; index < nhCount; index++)
                {
                    // to check whether they match
                    if (query[index].POICount <= poiCountThreshold && candidate[index].POICount <= poiCountThreshold)
                    {
                        result += weight1;
                        matchedNhCount++;
                    }
                    else if (query[index].POICount > poiCountThreshold && candidate[index].POICount > poiCountThreshold)
                    {
                        matchedNhCount++;
                        var queryOrientation0POIHistogram = query[index].Orientation0POIMagnitude;
                        var queryOrientation1POIHistogram = query[index].Orientation1POIMagnitude;
                        var queryOrientation2POIHistogram = query[index].Orientation2POIMagnitude;
                        var queryOrientation3POIHistogram = query[index].Orientation3POIMagnitude;                       
                        var queryColumnEnergyEntropy = query[index].ColumnEnergyEntropy;
                        var queryRowEnergyEntropy = query[index].RowEnergyEntropy;

                        var candidateOrientation0POIHistogram = candidate[index].Orientation0POIMagnitude;
                        var candidateOrientation1POIHistogram = candidate[index].Orientation1POIMagnitude;
                        var candidateOrientation2POIHistogram = candidate[index].Orientation2POIMagnitude;
                        var candidateOrientation3POIHistogram = candidate[index].Orientation3POIMagnitude;                       
                        var candidateColumnEnergyEntropy = candidate[index].ColumnEnergyEntropy;
                        var candidateRowEnergyEntropy = candidate[index].RowEnergyEntropy;

                        Orientation0POIHistDiff = Math.Abs(queryOrientation0POIHistogram - candidateOrientation0POIHistogram);
                        Orientation1POIHistDiff = Math.Abs(queryOrientation1POIHistogram - candidateOrientation1POIHistogram);
                        Orientation2POIHistDiff = Math.Abs(queryOrientation2POIHistogram - candidateOrientation2POIHistogram);
                        Orientation3POIHistDiff = Math.Abs(queryOrientation3POIHistogram - candidateOrientation3POIHistogram);
                        
                        columnEnergyEntropyDiff = Math.Abs(queryColumnEnergyEntropy - candidateColumnEnergyEntropy);
                        rowEnergyEntropyDiff = Math.Abs(queryRowEnergyEntropy - candidateRowEnergyEntropy);
                        var euclideanDistance = Math.Sqrt(Math.Pow(Orientation0POIHistDiff, 2) + Math.Pow(Orientation1POIHistDiff, 2)
                                          + Math.Pow(Orientation2POIHistDiff, 2) + Math.Pow(Orientation3POIHistDiff, 2)                                          
                                          + Math.Pow(columnEnergyEntropyDiff, 2) + Math.Pow(rowEnergyEntropyDiff, 2)
                                         );
                        result += weight2 * (1 - euclideanDistance / maxDistance);
                    }
                }
                var matchedPercentage = matchedNhCount / (double)nhCount;
                var averageSimilarityScore = result / matchedNhCount;
                result = matchedPercentage * averageSimilarityScore;
                result = Convert.ToDouble(result.ToString("F03", CultureInfo.InvariantCulture));
            }
            return result;
        }

        

        public static double DistanceHoGRepresentation(List<RegionRepresentation> query, List<RegionRepresentation> candidate)
        {
            var result = 0.0;
            if (query != null && candidate != null)
            {
                var nhCount = query[0].NhCountInCol * query[0].NhCountInRow;
                for (int index = 0; index < nhCount; index++)
                {
                    var sum = 0.0;
                    if (query[index].HistogramOfOrientatedGradient != null && candidate[index].HistogramOfOrientatedGradient != null)
                    {
                        List<double> queryHOG = query[index].HistogramOfOrientatedGradient;
                        List<double> candidateHOG = candidate[index].HistogramOfOrientatedGradient;
                        for (int i = 0; i < queryHOG.Count; i++)
                        {
                            if (query[index].POICount != 0)
                            {
                                sum += Math.Abs(queryHOG[i] - candidateHOG[i]);
                            }
                        }
                    }
                    result += sum;
                }
            }
            return result;
        }

        /// <summary>
        /// This distance measure is based on feature prop 5 where each nh is represented by 6 values: hH, hV, hP, hD, eF, eT.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidate"></param>
        /// <param name="poiCountThreshold"></param>
        /// <param name="weight1"></param>
        /// <param name="weight2"></param>
        /// <returns></returns>
        public static double DistanceFeatureProp5Based(List<RegionRepresentation> query,
            List<RegionRepresentation> candidate, int poiCountThreshold, double weight1, double weight2)
        {
            var result = 0.0;
            if (query != null && candidate != null)
            {
                var nhCount = query[0].NhCountInCol * query[0].NhCountInRow; 
                var maxHPOIHistDiff = 1.0;
                var maxPPOIHistDiff = 1.0;
                var maxVPOIHistDiff = 1.0;
                var maxNPOIHistDiff = 1.0;
                var maxColEnergyEntroDiff = 1.0;
                var maxRowEnergyEntroDiff = 1.0;
                var matchedNhCount = 0;
                for (int index = 0; index < nhCount; index++)
                {
                    // if the case is against the two following conditions, then result = 0.0, so it has no effect on final result.  
                    // to check whether they are null neighbourhood
                    if (query[index].POICount <= poiCountThreshold && candidate[index].POICount <= poiCountThreshold)
                    {
                        result += weight1;
                        matchedNhCount++;
                    }
                    // to check whether both of them contain ridges
                    else if (query[index].POICount > poiCountThreshold && candidate[index].POICount > poiCountThreshold)
                    {
                        matchedNhCount++;
                        var hOrientationPOIHistDiff = Math.Abs(query[index].HOrientationPOIHistogram
                            - candidate[index].HOrientationPOIHistogram);
                        var pDOrientationPOIHistDiff = Math.Abs(query[index].PDOrientationPOIHistogram
                            - candidate[index].PDOrientationPOIHistogram);
                        var vOrientationPOIHistDiff = Math.Abs(query[index].VOrientationPOIHistogram
                            - candidate[index].VOrientationPOIHistogram);
                        var nDOrientationPOIHistDiff = Math.Abs(query[index].NDOrientationPOIHistogram
                            - candidate[index].NDOrientationPOIHistogram);
                        var columnEnergyEntropyDiff = Math.Abs(query[index].ColumnEnergyEntropy
                            - candidate[index].ColumnEnergyEntropy);
                        var rowEnergyEntropyDiff = Math.Abs(query[index].RowEnergyEntropy
                            - candidate[index].RowEnergyEntropy);

                        var euclideanDistance = Math.Sqrt(Math.Pow(hOrientationPOIHistDiff, 2) + Math.Pow(pDOrientationPOIHistDiff, 2)
                                          + Math.Pow(vOrientationPOIHistDiff, 2) + Math.Pow(nDOrientationPOIHistDiff, 2)
                                          + Math.Pow(columnEnergyEntropyDiff, 2) + Math.Pow(rowEnergyEntropyDiff, 2));
                        //var mahattenDistance = hOrientationPOIHistDiff + pDOrientationPOIHistDiff + vOrientationPOIHistDiff
                        //    + nDOrientationPOIHistDiff + columnEnergyEntropyDiff + rowEnergyEntropyDiff;
                        var maxDistance = Math.Sqrt(maxHPOIHistDiff
                            + maxPPOIHistDiff
                            + maxVPOIHistDiff
                            + maxNPOIHistDiff
                            + maxColEnergyEntroDiff
                            + maxRowEnergyEntroDiff);
                        result += weight2 * (1 - euclideanDistance / maxDistance);
                    }                    
                }
                var matchedPercentage = matchedNhCount / (double)nhCount;
                var averageSimilarityScore = result / matchedNhCount;
                result = matchedPercentage * averageSimilarityScore;
                result = Convert.ToDouble(result.ToString("F03", CultureInfo.InvariantCulture));
            }
            return result;
        }

        /// <summary>
        /// This weighted Euclidean distance function is little bit different from the one below this method. The distance result is obtained 
        /// based on the sum of sub-region in the process of calculation. There are four properties as feature vector. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidate"></param>
        /// <param name="weight1"></param>
        /// <param name="weight2"></param>
        /// <returns></returns>
        public static double WeightedDistanceScoreRegionRepresentation4(List<RegionRepresentation> query, List<RegionRepresentation> candidate,
            double weight1, double weight2, double weight3, double weight4, double weight5, double weight6)
        {
            var result = 0.0;
            if (query != null && candidate != null)
            {
                var nhCount = query[0].NhCountInCol * query[0].NhCountInRow;
                var nhSum = 0.0;
                for (int index = 0; index < nhCount; index++)
                {

                    var queryHMagnitude = query[index].HOrientationPOIMagnitude;
                    var queryHOrientation = query[index].LinearHOrientation;
                    var queryVMagnitude = query[index].VOrientationPOIMagnitude;
                    var queryVOrientation = query[index].LinearVOrientation;
                    var queryHRmeasure = query[index].HLineOfBestfitMeasure;
                    var queryVRmeasure = query[index].VLineOfBestfitMeasure;

                    var candidateHMagnitude = candidate[index].HOrientationPOIMagnitude;
                    var candidateHOrientation = candidate[index].LinearHOrientation;
                    var candidateVMagnitude = candidate[index].VOrientationPOIMagnitude;
                    var candidateVOrientation = candidate[index].LinearVOrientation;
                    var candidateHRmeasure = candidate[index].HLineOfBestfitMeasure;
                    var candidateVRmeasure = candidate[index].VLineOfBestfitMeasure;

                    var hMagnitudeDiff = Math.Abs(queryHMagnitude - candidateHMagnitude);
                    var hOrientationDiff = Math.Abs(queryHOrientation - candidateHOrientation);
                    var vMagnitudeDiff = Math.Abs(queryVMagnitude - candidateVMagnitude);
                    var vOrientationDiff = Math.Abs(queryVOrientation - candidateVOrientation);
                    var hRmeasureDiff = Math.Abs(queryHRmeasure - candidateHRmeasure);
                    var vRmeasureDiff = Math.Abs(queryVRmeasure - candidateVRmeasure);

                    nhSum += Math.Sqrt(weight1 * Math.Pow(hMagnitudeDiff, 2) + weight2 * Math.Pow(hOrientationDiff, 2)
                        + weight3 * Math.Pow(vMagnitudeDiff, 2) + weight4 * Math.Pow(vOrientationDiff, 2)
                        + weight5 * Math.Pow(hRmeasureDiff, 2) + weight6 * Math.Pow(vRmeasureDiff, 2));
                }
                result = nhSum;
            }
            return result;
        }
        
        /// <summary>
        /// Weighted Euclidean distance measurement is based on a bunch of neighbourhoods calculation. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidate"></param>
        /// <returns>
        /// The final out is sum of subdistance for each sub-region(neighbourhood) in candidate or query. 
        /// </returns>
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

        /// <summary>
        /// To check whether a feature vector is null. 
        /// </summary>
        /// <param name="featureVector"></param>
        /// <returns></returns>
        public static bool checkNullFeatureVector(RidgeNeighbourhoodFeatureVector featureVector)
        {

            var numberOfHorizontalFeatureVectorBit = featureVector.HorizontalVector.Count();
            var numberOfDiagonalFeatureVectorBit = featureVector.PositiveDiagonalVector.Count();
            var featureVectorBitCount = 0;
            if (featureVector != null)
            {
                for (int i = 0; i < numberOfHorizontalFeatureVectorBit; i++)
                {
                    if (featureVector.HorizontalVector[i] != 0)
                    {
                        featureVectorBitCount++;
                    }
                    if (featureVector.HorizontalVector[i] != 0)
                    {
                        featureVectorBitCount++;
                    }
                }
                for (int j = 0; j < numberOfDiagonalFeatureVectorBit; j++)
                {
                    if (featureVector.PositiveDiagonalVector[j] != 0)
                    {
                        featureVectorBitCount++;
                    }
                    if (featureVector.NegativeDiagonalVector[j] != 0)
                    {
                        featureVectorBitCount++;
                    }
                }
            }
            if (featureVectorBitCount == 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// To check whether a list of featurevectors is null.  
        /// </summary>
        /// <param name="featureVectorList"></param>
        /// <returns></returns>
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

        /// <summary>
        /// This distance calculation method will be based on 6 values feature vector. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidates"></param>
        /// <param name="weight1"></param>
        /// <param name="weight2"></param>
        /// <param name="weight3"></param>
        /// <param name="weight4"></param>
        /// <returns></returns>
        public static List<Candidates> WeightedEuclideanDistCalculation3(List<RegionRepresentation> query, List<RegionRepresentation> candidates,
            double weight1, double weight2, double weight3, double weight4, double weight5, double weight6)
        {
            var result = new List<Candidates>();
            var tempRegionList = new List<RegionRepresentation>();
            var regionCountInAcandidate = query[0].NhCountInCol * query[0].NhCountInRow;
            var candidatesCount = candidates.Count;
            for (int i = 0; i < candidatesCount; i += regionCountInAcandidate)
            {
                // The frequencyDifference is a problem. 
                tempRegionList = StatisticalAnalysis.SubRegionFromRegionList(candidates, i, regionCountInAcandidate);
                var duration = tempRegionList[0].Duration.TotalMilliseconds;
                var distance = SimilarityMatching.WeightedDistanceScoreRegionRepresentation4(query, tempRegionList, weight1, weight2,
                    weight3, weight4, weight5, weight6);
                var item = new Candidates(distance, tempRegionList[0].FrameIndex,
                        duration, tempRegionList[0].FrequencyIndex, tempRegionList[0].FrequencyIndex - tempRegionList[0].FrequencyRange,
                        tempRegionList[0].SourceAudioFile);
                result.Add(item);
            }
            return result;
        }
        #endregion
    }
}
