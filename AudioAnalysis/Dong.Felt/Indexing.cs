
namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Representations;
    using System.IO;
    public class Indexing
    {

        /// function to extract the query from a audio file which contains the query.
        public static RegionRerepresentation ExtractQueryRegionRepresentationFromAudioNhRepresentations(Query query, RidgeDescriptionNeighbourhoodRepresentation[,] ridgeNeighbourhood, string audioFileName)
        {            
            var startTime = StatisticalAnalysis.SecondsToMillionSeconds(query.startTime);
            var audioFile = new FileInfo(audioFileName);
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
           var result = new RegionRerepresentation(tempResult,
               nhRowsCount, nhColsCount, audioFile);           

           return result;
        }

        /// <summary>
        /// This similarity tuple records the distance, timePosition, frequencyband. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidates"></param>
        /// <returns></returns>
        public static List<Tuple<double, double, double>> SimilairtyScoreFromAudioRegionVectorRepresentation(RegionRerepresentation query, List<List<RegionRerepresentation>> candidates)
        {
            // to get the distance and frequency band index
            var result = new List<Tuple<double, double, double>>();
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
                    var distance = SimilarityMatching.DistanceScoreRegionRepresentation(query, c[i]);
                    distanceListForOneVector.Add(distance); 
                    var minDistance = distanceListForOneVector.Min();
                    if (minDistance < miniDistance)
                    {
                        regionIndicator = i;
                        miniDistance = minDistance;
                    }                   
                }
                result.Add(Tuple.Create(miniDistance, j * 150.8, c[regionIndicator].FrequencyIndex));
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
        public static List<RegionRerepresentation> CandidatesRepresentationFromAudioNhRepresentations(RegionRerepresentation queryRepresentation, RidgeDescriptionNeighbourhoodRepresentation[,] ridgeNeighbourhood, string audioFileName)
        {
            var result = new List<RegionRerepresentation>();
            var audioFile = new FileInfo(audioFileName);
            var rowsCount = ridgeNeighbourhood.GetLength(0);
            var colsCount = ridgeNeighbourhood.GetLength(1);
            var nhCountInRow = queryRepresentation.NhCountInRow;
            var nhCountInColumn = queryRepresentation.NhCountInCol;
            for (var rowIndex = 0; rowIndex < rowsCount; rowIndex++)
            {
                for (var colIndex = 0; colIndex < colsCount; colIndex++)
                {
                    if (StatisticalAnalysis.checkBoundary(rowIndex + nhCountInRow, colIndex + nhCountInColumn, rowsCount, colsCount))
                    {
                        var subRegionMatrix = StatisticalAnalysis.SubRegionMatrix(ridgeNeighbourhood, rowIndex, colIndex, rowIndex + nhCountInRow, colIndex + nhCountInColumn);
                        var nhList = new List<RidgeDescriptionNeighbourhoodRepresentation>();
                        for (int i = 0; i < nhCountInRow; i++)
                        {
                            for (int j = 0; j < nhCountInColumn; j++)
                            {
                                nhList.Add(subRegionMatrix[i, j]);
                            }
                        }
                        var region = new RegionRerepresentation(nhList, nhCountInRow, nhCountInColumn, audioFile);
                        result.Add(region);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// This method takes the candidatesList and output a list of list of region representation.  Especially, each sub-list(also called a vector) of region representation
        /// stores the region reprentation for each frequency bin(row).
        /// </summary>
        /// <param name="candidatesList"></param>
        /// <returns></returns>
        public static List<List<RegionRerepresentation>> RegionRepresentationListToVectors(List<RegionRerepresentation> candidatesList)
        {
            var result = new List<List<RegionRerepresentation>>();
            var listCount = candidatesList.Count;
            var nhHeightInHerz = 559;
            //var nhWidthInMillisecond = 150.8;
            var lastCandidate = candidatesList[listCount - 1];
            var rowsCount = (int)(lastCandidate.FrequencyIndex / nhHeightInHerz) + 1;
            //var colsCount = (int)(lastCandidate.TimeIndex / nhWidthInMillisecond) + 1;
            var colsCount = listCount / rowsCount;
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
