
namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Representations;
    using System.IO;
    using Dong.Felt.Configuration;
    using AudioAnalysisTools.Sonogram;
    public class Indexing
    {
        /// <summary>
        /// To extract query region representation from an audio file which contains the query. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="ridgeNeighbourhood"></param>
        /// <param name="audioFileName"></param>
        /// <returns>
        /// returns a list of region representation, each region represtation contains a ridge nh representation and some derived property. 
        /// </returns>
        public static List<RegionRerepresentation> ExtractQueryRegionRepresentationFromAudioNhRepresentations(Query query, int neighbourhoodLength, List<RidgeDescriptionNeighbourhoodRepresentation> nhRepresentationList,string audioFileName, SpectralSonogram spectrogram)
        {
            var nhFrequencyRange = neighbourhoodLength * spectrogram.FBinWidth;
            var nhCountInRow = (int)(spectrogram.NyquistFrequency / nhFrequencyRange);
            if (spectrogram.NyquistFrequency % nhFrequencyRange == 0)
            {
                nhCountInRow--;
            }
            var nhCountInColumn = (int)(spectrogram.FrameCount / neighbourhoodLength);
            if (spectrogram.FrameCount % neighbourhoodLength == 0)
            {
                nhCountInColumn--;
            }
            var ridgeNeighbourhood = StatisticalAnalysis.NhListToArray(nhRepresentationList, nhCountInRow, nhCountInColumn);
            var results = new List<RegionRerepresentation>();
            var nhRowsCount = query.nhCountInRow;
            var nhColsCount = query.nhCountInColumn;
            var nhStartRowIndex = query.nhStartRowIndex;
            var nhStartColIndex = query.nhStartColIndex;
            var tempResult = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            for (int rowIndex = nhStartRowIndex; rowIndex < nhStartRowIndex + nhRowsCount; rowIndex++)
            {
                for (int colIndex = nhStartColIndex; colIndex < nhStartColIndex + nhColsCount; colIndex++)
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
                var regionItem = new RegionRerepresentation(tempResult[i], frequencyIndex, frameIndex, nhRowsCount, nhColsCount, rowIndexInRegion, colIndexInRegion, audioFileName);
                results.Add(regionItem);
            }                     
           return results;
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

        /// <summary>
        /// To calculate the distance between regionRepresentation of a query and a candidate. 
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
        /// And the region size is same as the query. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="queryRepresentation"></param>
        /// <param name="ridgeNeighbourhood"></param>
        /// <returns></returns>
        public static List<RegionRerepresentation> RegionRepresentationFromAudioNhRepresentations(List<RegionRerepresentation> queryRepresentation, List<RidgeDescriptionNeighbourhoodRepresentation> nhRepresentationList, string audioFileName,
            int neighbourhoodLength, SpectrogramConfiguration spectrogramConfig, SpectralSonogram spectrogram)
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
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond

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
                            var regionItem = new RegionRerepresentation(nhList[i], frequencyIndex, frameIndex, nhCountInRowForQuery, nhCountInColForQuery, rowIndexInRegion, colIndexInRegion, audioFileName);
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
