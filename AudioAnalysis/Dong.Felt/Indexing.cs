﻿
namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Representations;
    using System.IO;
    using Dong.Felt.Configuration;
    using Dong.Felt.Features;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools;
    using TowseyLibrary;
    using System.Globalization;
    public class Indexing
    {

        // Todo: extract query region representaiton by providing the boundary information of the query. 
        public static List<RegionRerepresentation> ExtractQueryRegionRepresentation(Query query, int neighbourhoodLength,
           string audioFileName, SpectrogramStandard spectrogram)
        {
            var results = new List<RegionRerepresentation>();
            
            var frequencyRange = query.frequencyRange;
            var duration = query.duration;
            var maxFreq = query.maxFrequency;
            var minFreq = query.minFrequency;
            var startTime = query.startTime;
            var endTime = query.endTime;
            var frequencyIndex = query.nhStartRowIndex;
            var frameIndex = query.nhStartColIndex;

            var nhfrequencyUnit = neighbourhoodLength * spectrogram.FBinWidth;
            var nhframeUnit = neighbourhoodLength * spectrogram.FrameDuration;

            var nhCountInRow = (int)(frequencyRange / nhfrequencyUnit);
            var nhCountInColumn = (int)(duration / nhframeUnit); 

            if (nhCountInRow * nhfrequencyUnit > frequencyRange)
            {
                nhCountInRow++;
            }
            if (nhCountInColumn  * nhframeUnit > duration)
            {
                nhCountInColumn++; 
            }
            return results;
        }
        
        /// <summary>
        /// To extract query region representation from an audio file which contains the query. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="ridgeNeighbourhood"></param>
        /// <param name="audioFileName"></param>
        /// <returns>
        /// returns a list of region representation, each region represtation contains a ridge nh representation and some derived property. 
        /// </returns>
        public static List<RegionRerepresentation> ExtractQueryRegionRepresentationFromAudioNhRepresentations(Query query, int neighbourhoodLength,
            List<RidgeDescriptionNeighbourhoodRepresentation> nhRepresentationList, string audioFileName, SpectrogramStandard spectrogram)
        {
            var nhCountInRow = query.maxNhRowIndex;
            var nhCountInColumn = query.maxNhColIndex;
            var ridgeNeighbourhood = StatisticalAnalysis.NhListToArray(nhRepresentationList, nhCountInRow, nhCountInColumn);
            var results = new List<RegionRerepresentation>();
            var nhRowsCount = query.nhCountInRow;
            var nhColsCount = query.nhCountInColumn;
            var nhStartRowIndex = query.nhStartRowIndex;
            var nhStartColIndex = query.nhStartColIndex;
            var tempResult = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            var maxiRowIndex = nhStartRowIndex + nhRowsCount;
            var maxiColIndex = nhStartColIndex + nhColsCount;

            for (int rowIndex = nhStartRowIndex; rowIndex < maxiRowIndex; rowIndex++)
            {
                for (int colIndex = nhStartColIndex; colIndex < maxiColIndex; colIndex++)
                {
                    tempResult.Add(ridgeNeighbourhood[rowIndex, colIndex]);
                }
            }
            // The top left nh frequency and frame index will be the index of a region representation. 
            for (int i = 0; i < tempResult.Count; i++)
            {
                var frequencyIndex = tempResult[0].FrequencyIndex;
                var frameIndex = tempResult[0].FrameIndex;
                var rowIndexInRegion = i / nhColsCount;
                var colIndexInRegion = i % nhColsCount;
                var regionItem = new RegionRerepresentation(tempResult[i], frequencyIndex, frameIndex, nhRowsCount, nhColsCount,
                    rowIndexInRegion, colIndexInRegion, audioFileName);
                results.Add(regionItem);
            }
            return results;
        }

        public static RegionRerepresentation ExtractQRepreFromAudioStRepr(Query query,
            List<PointOfInterest> stList, string audioFileName, SpectrogramStandard spectrogram)
        {
            double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            var rowsCount = matrix.GetLength(0) - 1;
            var colsCount = matrix.GetLength(1);
            var stMatrix = StatisticalAnalysis.TransposePOIsToMatrix(stList, rowsCount, colsCount);
            var m = new double[14, 14];
            foreach (var s in stList)
            {
                if ((s.Point.X == 87) && (s.Point.Y == 4032))
                {
                    m = s.fftMatrix;
                }
            }
            var frequencyScale = spectrogram.FBinWidth;
            var timeScale = spectrogram.FrameDuration / 2;
            // be careful about the index here.
            var rowStart = spectrogram.Configuration.FreqBinCount - (int)Math.Ceiling(query.maxFrequency / frequencyScale);
            var rowEnd = spectrogram.Configuration.FreqBinCount - (int)Math.Ceiling(query.minFrequency / frequencyScale); 
            var colStart = (int)(query.startTime / 1000 / timeScale);
            var colEnd = (int)(query.endTime / 1000 / timeScale);     
      
            var regionMatrix = StatisticalAnalysis.SubmatrixFromPointOfInterest(stMatrix, rowStart, colStart, rowEnd, colEnd);
            var result = new RegionRerepresentation();
            result.StartRowIndex = rowStart;
            result.EndRowIndex = rowEnd;
            result.StartColIndex = colStart;
            result.EndColIndex = colEnd;
            result.fftFeatures = regionMatrix;
            result.TimeIndex = query.startTime;
            result.FrequencyIndex = query.maxFrequency;
            result.FrequencyRange = query.maxFrequency - query.minFrequency;
            result.Duration = TimeSpan.FromMilliseconds(query.endTime - query.startTime);
            result.SourceAudioFile = audioFileName;
            return result;
        }

        public static List<RegionRerepresentation> ExtractCandidateRegionRepresentationFromAudioNhRepresentations(Query query, int neighbourhoodLength,
            List<RidgeDescriptionNeighbourhoodRepresentation> nhRepresentationList, string audioFileName, SpectrogramStandard spectrogram)
        {
            var results = new List<RegionRerepresentation>();
            var nhFrequencyRange = neighbourhoodLength * spectrogram.FBinWidth;
            var maxNhCountInRow = (int)(spectrogram.NyquistFrequency / nhFrequencyRange);
            if (spectrogram.NyquistFrequency % nhFrequencyRange == 0)
            {
                maxNhCountInRow--;
            }
            var minNhCountInColumn = (int)(spectrogram.FrameCount / neighbourhoodLength);
            if (spectrogram.FrameCount % neighbourhoodLength == 0)
            {
                minNhCountInColumn--;
            }
            var ridgeNeighbourhood = StatisticalAnalysis.NhListToArray(nhRepresentationList, maxNhCountInRow, minNhCountInColumn);
            var rowsCount = ridgeNeighbourhood.GetLength(0);
            var colsCount = ridgeNeighbourhood.GetLength(1);

            var nhRowsCount = query.nhCountInRow;
            var nhColsCount = query.nhCountInColumn;
            var nhStartRowIndex = query.nhStartRowIndex;

            for (int colIndex = 0; colIndex < colsCount; colIndex++)
            {
                if (StatisticalAnalysis.checkBoundary(nhStartRowIndex + nhRowsCount - 1, colIndex + nhColsCount - 1, rowsCount, colsCount))
                {
                    var subRegionMatrix = StatisticalAnalysis.SubRegionMatrix(ridgeNeighbourhood, nhStartRowIndex, colIndex, nhStartRowIndex + nhRowsCount, colIndex + nhColsCount);
                    var nhList = new List<RidgeDescriptionNeighbourhoodRepresentation>();
                    for (int i = 0; i < nhRowsCount; i++)
                    {
                        for (int j = 0; j < nhColsCount; j++)
                        {
                            nhList.Add(subRegionMatrix[i, j]);
                        }
                    }
                    for (int i = 0; i < nhList.Count; i++)
                    {
                        var frequencyIndex = nhList[0].FrequencyIndex;
                        var frameIndex = nhList[0].FrameIndex;
                        var rowIndexInRegion = (int)(i / nhColsCount);
                        var colIndexInRegion = i % nhColsCount;
                        var regionItem = new RegionRerepresentation(nhList[i], frequencyIndex, frameIndex, nhRowsCount,
                            nhColsCount, rowIndexInRegion, colIndexInRegion, audioFileName);
                        results.Add(regionItem);
                    }
                }
            }

            return results;
        }

        public static List<RegionRerepresentation> ExtractCandiRegionRepreFromAudioStList(SpectrogramStandard spectrogram,
            string audioFileName, List<PointOfInterest> stList, RegionRerepresentation queryRepresentation)
        {
            var result = new List<RegionRerepresentation>();
            double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            var rowsCount = matrix.GetLength(0) - 1;
            var colsCount = matrix.GetLength(1);
             
            var freqScale = spectrogram.FBinWidth;
            var timeScale = spectrogram.FrameDuration / 2.0;
            var startRowIndex = queryRepresentation.StartRowIndex;
            var endRowIndex = queryRepresentation.EndRowIndex;         
            var colRange = queryRepresentation.fftFeatures.GetLength(1) - 1;
            // The one sets all the features in the region to 0,  this one is useful to calculate the distance based on purely Euclidean
            //var stMatrix = StatisticalAnalysis.TransposePOIsToMatrix2(stList, rowsCount, colsCount);
            // The one sets the none structure tensor point to null features. 
            var stMatrix = StatisticalAnalysis.TransposePOIsToMatrix(stList, rowsCount, colsCount);
            
            var searchStep = 5;
            for (int colIndex = 0; colIndex < colsCount; colIndex += searchStep)
            {
                if (StatisticalAnalysis.checkBoundary(startRowIndex, colIndex, endRowIndex, colsCount))
                {
                    var subRegionMatrix = StatisticalAnalysis.SubmatrixFromPointOfInterest(stMatrix, startRowIndex, colIndex, 
                                                                    endRowIndex, colIndex + colRange);
                    // check whether the region is null
                    if (!StatisticalAnalysis.checkNullRegion(subRegionMatrix))
                    {
                        var regionItem = new RegionRerepresentation();                      
                        regionItem.fftFeatures = subRegionMatrix;
                        regionItem.StartRowIndex = startRowIndex;
                        regionItem.EndRowIndex = endRowIndex;
                        regionItem.StartColIndex = colIndex;
                        regionItem.EndColIndex = colIndex + colRange;
                        regionItem.TimeIndex = colIndex * timeScale * 1000;
                        regionItem.FrequencyIndex = queryRepresentation.FrequencyIndex;
                        regionItem.FrequencyRange = queryRepresentation.FrequencyRange;
                        regionItem.Duration = queryRepresentation.Duration;
                        regionItem.SourceAudioFile = audioFileName;
                        result.Add(regionItem);
                    }
                }
            }
            return result;
        }

        public static List<RegionRerepresentation> ExtractCandidatesRegionRepresentationFromRegionRepresntations(List<RegionRerepresentation> query, List<RegionRerepresentation> regionList)
        {
            var result = new List<RegionRerepresentation>();
            for (int i = 0; i < regionList.Count; i++)
            {
                var freDifferenceThreshold = 1;
                if (Math.Abs(regionList[i].MaxFrequencyIndex - query[0].FrequencyIndex) < freDifferenceThreshold)
                {
                    result.Add(regionList[i]);
                }
            }
            return result;
        }

        public static List<Candidates> WeightedEuclideanDistance(List<RegionRerepresentation> query,
            List<RegionRerepresentation> candidates, double weight1, double weight2)
        {
            var result = new List<Candidates>();
            var regionCountInAcandidate = query[0].NhCountInCol * query[0].NhCountInRow;
            var candidatesCount = candidates.Count;
            var queryFeatures = query[0].Features;
            for (int i = 0; i < candidatesCount; i++)
            {
                var duration = candidates[i].Duration.TotalMilliseconds;
                var distance = SimilarityMatching.WeightedDistanceScoreRegionFeature(query[0].Features, candidates[i].Features);
                var item = new Candidates(distance, candidates[i].FrameIndex,
                        duration, candidates[i].FrequencyIndex, candidates[i].FrequencyIndex - candidates[i].FrequencyRange,
                        candidates[i].SourceAudioFile);
                result.Add(item);
            }
            return result;
        }

        // Need to be changed. 
        public static List<Candidates> EuclideanDistanceOnFFTMatrix(RegionRerepresentation query, List<RegionRerepresentation> candidates,
            double matchedThreshold, double weight)
        {
            var result = new List<Candidates>();
            foreach (var c in candidates)
            {
                var distance = SimilarityMatching.EuclideanDistanceScore(query, c, matchedThreshold, weight);
                var formattedDistance = Convert.ToDouble(distance.ToString("F03", CultureInfo.InvariantCulture));
                var item = new Candidates(formattedDistance, c.TimeIndex,
                        c.Duration.TotalMilliseconds, c.FrequencyIndex, c.FrequencyIndex - c.FrequencyRange,
                        c.SourceAudioFile);
                result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// To calculate the distance between regionRepresentation of a query and a candidate. 
        /// This distance calculation method will be based on 2 values feature vector. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidates"></param>
        /// <param name="weight1"></param>
        /// <param name="weight2"></param>
        /// <returns></returns>
        public static List<Candidates> WeightedEuclideanDistCalculation(List<RegionRerepresentation> query, List<RegionRerepresentation> candidates, double weight1, double weight2)
        {
            var result = new List<Candidates>();
            var tempRegionList = new List<RegionRerepresentation>();
            var regionCountInAcandidate = query[0].NhCountInCol * query[0].NhCountInRow;
            var candidatesCount = candidates.Count;
            for (int i = 0; i < candidatesCount; i += regionCountInAcandidate)
            {
                // The frequencyDifference is a problem. 
                tempRegionList = StatisticalAnalysis.SubRegionFromRegionList(candidates, i, regionCountInAcandidate);
                var duration = tempRegionList[0].Duration.TotalMilliseconds;
                var distance = SimilarityMatching.WeightedDistanceScoreRegionRepresentation2(query, tempRegionList, weight1, weight2);
                var item = new Candidates(distance, tempRegionList[0].FrameIndex,
                        duration, tempRegionList[0].FrequencyIndex, tempRegionList[0].FrequencyIndex - tempRegionList[0].FrequencyRange,
                        tempRegionList[0].SourceAudioFile);
                result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// This distance calculation method will be based on 4 values feature vector. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidates"></param>
        /// <param name="weight1"></param>
        /// <param name="weight2"></param>
        /// <param name="weight3"></param>
        /// <param name="weight4"></param>
        /// <returns></returns>
        public static List<Candidates> WeightedEuclideanDistCalculation2(List<RegionRerepresentation> query, List<RegionRerepresentation> candidates,
            double weight1, double weight2, double weight3, double weight4)
        {
            var result = new List<Candidates>();
            var tempRegionList = new List<RegionRerepresentation>();
            var regionCountInAcandidate = query[0].NhCountInCol * query[0].NhCountInRow;
            var candidatesCount = candidates.Count;
            for (int i = 0; i < candidatesCount; i += regionCountInAcandidate)
            {
                // The frequencyDifference is a problem. 
                tempRegionList = StatisticalAnalysis.SubRegionFromRegionList(candidates, i, regionCountInAcandidate);
                var duration = tempRegionList[0].Duration.TotalMilliseconds;
                var distance = SimilarityMatching.WeightedDistanceScoreRegionRepresentation3(query, tempRegionList, weight1, weight2,
                    weight3, weight4);
                var item = new Candidates(distance, tempRegionList[0].FrameIndex,
                        duration, tempRegionList[0].FrequencyIndex, tempRegionList[0].FrequencyIndex - tempRegionList[0].FrequencyRange,
                        tempRegionList[0].SourceAudioFile);
                result.Add(item);
            }
            return result;
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
        public static List<Candidates> WeightedEuclideanDistCalculation3(List<RegionRerepresentation> query, List<RegionRerepresentation> candidates,
            double weight1, double weight2, double weight3, double weight4, double weight5, double weight6)
        {
            var result = new List<Candidates>();
            var tempRegionList = new List<RegionRerepresentation>();
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

        // this function is used for calculating the distance based on HOG features. 
        public static List<Candidates> HoGEuclideanDist(List<RegionRerepresentation> query, List<RegionRerepresentation> candidates)
        {
            var result = new List<Candidates>();
            var tempRegionList = new List<RegionRerepresentation>();
            var regionCountInAcandidate = query[0].NhCountInCol * query[0].NhCountInRow;
            var candidatesCount = candidates.Count;
            for (int i = 0; i < candidatesCount; i += regionCountInAcandidate)
            {
                // The frequencyDifference is a problem. 
                tempRegionList = StatisticalAnalysis.SubRegionFromRegionList(candidates, i, regionCountInAcandidate);
                var duration = tempRegionList[0].Duration.TotalMilliseconds;
                var distance = SimilarityMatching.DistanceHoGRepresentation(query, tempRegionList);
                var item = new Candidates(distance, tempRegionList[0].FrameIndex,
                        duration, tempRegionList[0].FrequencyIndex, tempRegionList[0].FrequencyIndex - tempRegionList[0].FrequencyRange,
                        tempRegionList[0].SourceAudioFile);
                result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// Euclidean Distance calculation for feature5. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidates"></param>
        /// <returns></returns>
        public static List<Candidates> Feature5EuclideanDist(List<RegionRerepresentation> query, List<RegionRerepresentation> candidates)
        {
            var result = new List<Candidates>();
            var tempRegionList = new List<RegionRerepresentation>();
            var regionCountInAcandidate = query[0].NhCountInCol * query[0].NhCountInRow;
            var candidatesCount = candidates.Count;
            for (int i = 0; i < candidatesCount; i += regionCountInAcandidate)
            {
                // The frequencyDifference is a problem. 
                tempRegionList = StatisticalAnalysis.SubRegionFromRegionList(candidates, i, regionCountInAcandidate);
                var notNullNhCount = 0;
                var nhCountInRegion = tempRegionList.Count;
                for (int index = 0; index < nhCountInRegion; index++)
                {
                    if (tempRegionList[index].POICount != 0)
                    {
                        notNullNhCount++;
                    }                                 
                }
                if (notNullNhCount >= 0.1 * nhCountInRegion)
                {
                    var duration = tempRegionList[0].Duration.TotalMilliseconds;
                    var distance = SimilarityMatching.DistanceFeature5Representation(query, tempRegionList, 2);
                    var item = new Candidates(distance, tempRegionList[0].FrameIndex,
                            duration, tempRegionList[0].FrequencyIndex, tempRegionList[0].FrequencyIndex - tempRegionList[0].FrequencyRange,
                            tempRegionList[0].SourceAudioFile);
                    result.Add(item);
                }
            }
            return result;
        }

        /// <summary>
        /// This similarity tuple records the distance, timePosition, frequencyband. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidates"></param>
        /// <returns></returns>
        //public static List<Tuple<double, double, double>> SimilairtyScoreFromAudioRegionVectorRepresentation(RegionRerepresentation query, List<List<RegionRerepresentation>> candidates)
        //{
        //    // to get the distance and frequency band index
        //    var result = new List<Tuple<double, double, double>>();
        //    var vectorCount = candidates.Count;
        //    // each sublist has the same count, so here we want to get its length from the first value. 
        //    var regionCountINVector = candidates[0].Count;
        //    var regionIndicator = 0;
        //    var j = 0;
        //    foreach (var c in candidates)
        //    {
        //        var miniDistance = 60000000.0;
        //        var distanceListForOneVector = new List<double>();
        //        for (int i = 0; i < regionCountINVector; i++)
        //        {
        //            var distance = SimilarityMatching.DistanceScoreRegionRepresentation(query, c[i]);
        //            distanceListForOneVector.Add(distance);
        //            var minDistance = distanceListForOneVector.Min();
        //            if (minDistance < miniDistance)
        //            {
        //                regionIndicator = i;
        //                miniDistance = minDistance;
        //            }
        //        }
        //        var neighbourhoodDuration = 5 * 11.6;
        //        result.Add(Tuple.Create(miniDistance, j * neighbourhoodDuration, c[regionIndicator].FrequencyIndex));
        //        j++;
        //    }
        //    return result;
        //}

        public static List<double> DistanceScoreFromAudioRegionVectorRepresentation(RegionRerepresentation query, List<List<RegionRerepresentation>> candidates)
        {
            // to get the distance and frequency band index
            var result = new List<double>();
            var vectorCount = candidates.Count;
            // each sublist has the same count, so here we want to get its length from the first value. 
            var regionCountINVector = candidates[0].Count;
            var regionIndicator = 0;
            var j = 0;
            foreach (var c in candidates)
            {
                var miniDistance = 60000000.0;
                var distanceListForOneVector = new List<double>();
                for (int i = 0; i < regionCountINVector; i++)
                {
                    //var distance = SimilarityMatching.DistanceScoreRegionRepresentation(query, c[i]);
                    //distanceListForOneVector.Add(distance);
                    var minDistance = distanceListForOneVector.Min();
                    if (minDistance < miniDistance)
                    {
                        regionIndicator = i;
                        miniDistance = minDistance;
                    }
                }
                result.Add(miniDistance);
                j++;
            }
            return result;
        }

        public static List<Tuple<double, double, double>> DistanceListToSimilarityScoreList(List<Tuple<double, double, double>> distanceList)
        {
            var result = new List<Tuple<double, double, double>>();
            var listLength = distanceList.Count;
            var distance = new List<double>();
            foreach (var d in distanceList)
            {
                distance.Add(d.Item1);
            }
            var similarityScoreList = StatisticalAnalysis.ConvertDistanceToPercentageSimilarityScore(distance);
            for (int i = 0; i < listLength; i++)
            {
                result.Add(Tuple.Create(similarityScoreList[i], distanceList[i].Item2, distanceList[i].Item3));
            }
            return result;
        }

        /// <summary>
        /// Function to scan a list of representation in an audio file  within the same frequency band with the query.
        /// This name should be changed, because it is not doing indexing. It atually extracts the a list of region representation. 
        /// And the region size is the same as the query. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="queryRepresentation"></param>
        /// <param name="ridgeNeighbourhood"></param>
        /// <returns></returns>
        public static List<RegionRerepresentation> RegionRepresentationFromAudioNhRepresentations(List<RegionRerepresentation> queryRepresentation,
            List<RidgeDescriptionNeighbourhoodRepresentation> nhRepresentationList, string audioFileName,
            int neighbourhoodLength, SpectrogramConfiguration spectrogramConfig, SpectrogramStandard spectrogram)
        {
            var result = new List<RegionRerepresentation>();
            var nhFrequencyRange = neighbourhoodLength * spectrogram.FBinWidth;
            var maxNhCountInRow = (int)(spectrogram.NyquistFrequency / nhFrequencyRange);
            if (spectrogram.NyquistFrequency % nhFrequencyRange == 0)
            {
                maxNhCountInRow--;
            }
            var minNhCountInColumn = (int)(spectrogram.FrameCount / neighbourhoodLength);
            if (spectrogram.FrameCount % neighbourhoodLength == 0)
            {
                minNhCountInColumn--;
            }
            var ridgeNeighbourhood = StatisticalAnalysis.NhListToArray(nhRepresentationList, maxNhCountInRow, minNhCountInColumn);
            //var frequencyScale = spectrogramConfig.FrequencyScale;
            //var timeScale = spectrogramConfig.TimeScale; // millisecond

            var rowsCount = ridgeNeighbourhood.GetLength(0);
            var colsCount = ridgeNeighbourhood.GetLength(1);

            int nhCountInRowForQuery = queryRepresentation[0].NhCountInRow;
            int nhCountInColForQuery = queryRepresentation[0].NhCountInCol;

            for (var rowIndex = 0; rowIndex < rowsCount; rowIndex++)
            {
                for (var colIndex = 0; colIndex < colsCount; colIndex++)
                {
                    if (StatisticalAnalysis.checkBoundary(rowIndex + nhCountInRowForQuery - 1, colIndex + nhCountInColForQuery - 1, rowsCount, colsCount))
                    {
                        var subRegionMatrix = StatisticalAnalysis.SubRegionMatrix(ridgeNeighbourhood, rowIndex, colIndex, rowIndex + nhCountInRowForQuery, colIndex + nhCountInColForQuery);
                        var nhList = new List<RidgeDescriptionNeighbourhoodRepresentation>();
                        for (int i = 0; i < nhCountInRowForQuery; i++)
                        {
                            for (int j = 0; j < nhCountInColForQuery; j++)
                            {
                                nhList.Add(subRegionMatrix[i, j]);
                            }
                        }
                        for (int i = 0; i < nhList.Count; i++)
                        {
                            var frequencyIndex = nhList[0].FrequencyIndex;
                            var frameIndex = nhList[0].FrameIndex;
                            var rowIndexInRegion = (int)(i / nhCountInColForQuery);
                            var colIndexInRegion = i % nhCountInColForQuery;
                            var regionItem = new RegionRerepresentation(nhList[i], frequencyIndex, frameIndex, nhCountInRowForQuery,
                                nhCountInColForQuery, rowIndexInRegion, colIndexInRegion, audioFileName);
                            result.Add(regionItem);
                        }
                    }
                }
            }
            return result;
        }

        public static List<RegionRerepresentation> FixedFrequencyRegionRepresentationList2(List<RegionRerepresentation> candidatesList, int rowsCount1, int colsCount1)
        {
            var result = new List<RegionRerepresentation>();
            var listCount = candidatesList.Count;
            var nhCountInRow = candidatesList[0].NhCountInRow;
            var nhCountInCol = candidatesList[0].NhCountInCol;
            var rowsCount = rowsCount1 - nhCountInRow + 1;
            var colsCount = colsCount1 - nhCountInCol + 1;
            var candidatesArray = StatisticalAnalysis.RegionRepresentationListToArray(candidatesList, rowsCount, colsCount);
            var count = candidatesArray.GetLength(0) * candidatesArray.GetLength(1);
            var nhFrequencyIndexInRow = (int)candidatesList[0].FrequencyIndex;
            var nhRowIndex = (int)(nhFrequencyIndexInRow / 5 * 43);
            for (int colIndex = 0; colIndex < colsCount; colIndex++)
            {
                result.Add(candidatesArray[nhRowIndex, colIndex]);
            }
            return result;
        }

        /// <summary>
        /// This method takes the candidatesList and output a list of list of region representation.  Especially, each sub-list(also called a vector) of region representation
        /// stores the region reprentation for each frequency bin(row).
        /// </summary>
        /// <param name="candidatesList"></param>
        /// <returns></returns>
        public static List<List<RegionRerepresentation>> RegionRepresentationListToVectors(List<RegionRerepresentation> candidatesList, int rowsCount1, int colsCount1)
        {
            var result = new List<List<RegionRerepresentation>>();
            var listCount = candidatesList.Count;

            var nhCountInRow = candidatesList[0].NhCountInRow;
            var nhCountInCol = candidatesList[0].NhCountInCol;
            var rowsCount = rowsCount1 - nhCountInRow + 1;
            var colsCount = colsCount1 - nhCountInCol + 1;
            var candidatesArray = StatisticalAnalysis.RegionRepresentationListToArray(candidatesList, rowsCount, colsCount);
            var count = candidatesArray.GetLength(0) * candidatesArray.GetLength(1);
            for (int colIndex = 0; colIndex < colsCount; colIndex++)
            {
                var tempList = new List<RegionRerepresentation>();
                for (int rowIndex = 0; rowIndex < rowsCount; rowIndex++)
                {
                    tempList.Add(candidatesArray[rowIndex, colIndex]);
                }
                result.Add(tempList);
            }
            return result;
        }

    }
}
