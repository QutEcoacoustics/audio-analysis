
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
        public static List<RidgeDescriptionNeighbourhoodRepresentation> ExtractQueryFromFile(Query query, RidgeDescriptionNeighbourhoodRepresentation[,] ridgeNeighbourhood, int neighbourhoodLength)
        {
            var result = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            var startTime = StatisticalAnalysis.SecondsToMillionSeconds(query.startTime);
            var nhRowsCount = query.nhCountInRow;
            var nhColsCount = query.nhCountInColumn;
            var nhStartRowIndex = query.nhStartRowIndex;
            var nhStartColIndex = query.nhStartColIndex;
            for (int rowIndex = nhStartRowIndex; rowIndex < nhStartRowIndex + nhRowsCount; rowIndex++)
            {
                for (int colIndex = nhStartColIndex; colIndex < nhStartColIndex + nhColsCount; colIndex++)
                {
                    ridgeNeighbourhood[rowIndex, colIndex].nhCountInRow = nhRowsCount;
                    ridgeNeighbourhood[rowIndex, colIndex].nhCountInColumn = nhColsCount;
                    ridgeNeighbourhood[rowIndex, colIndex].RowIndex = rowIndex * 559;
                    ridgeNeighbourhood[rowIndex, colIndex].ColIndex = colIndex * 150.8;
                    result.Add(ridgeNeighbourhood[rowIndex, colIndex]);
                }
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
        public static List<RegionRerepresentation> CandidatesRepresentationFromFile(Query query, List<RidgeDescriptionNeighbourhoodRepresentation> queryRepresentation, RidgeDescriptionNeighbourhoodRepresentation[,] ridgeNeighbourhood, FileInfo audioFile, FileInfo textFile)
        {
            var result = new List<RegionRerepresentation>();
            var rowsCount = ridgeNeighbourhood.GetLength(0);
            var colsCount = ridgeNeighbourhood.GetLength(1);
            var nhCountInRow = query.nhCountInRow;
            var nhCountInColumn = query.nhCountInColumn;
            var rowIndex = 0;
            var colIndex = 0;
            var queryRepresentationMatrix = StatisticalAnalysis.RidgeNhListToArray(queryRepresentation, nhCountInRow, nhCountInColumn);
            for (rowIndex = 0; rowIndex < rowsCount; rowIndex++)
            {
                for (colIndex = 0; colIndex < colsCount; colIndex++)
                {
                    if (StatisticalAnalysis.checkBoundary(rowIndex, colIndex, rowIndex + nhCountInRow, colIndex + nhCountInColumn))
                    {
                        var subRegionMatrix = StatisticalAnalysis.SubRegionMatrix(ridgeNeighbourhood, rowIndex, colIndex, rowIndex + nhCountInRow, colIndex + nhCountInColumn);
                        var region = new RegionRerepresentation(rowIndex, colIndex, audioFile, textFile);
                        for (int i = 0; i < nhCountInRow; i++)
                        {
                            for (int j = 0; j < nhCountInColumn; j++)
                            {
                                region.score.Add(subRegionMatrix[i, j].score);
                            }
                        }
                        result.Add(region);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// This method takes the candidatesList and output a list of list of region representation.  Especially, the sub-list of region representation
        /// stores the region reprentation for each frequency bin(row).
        /// </summary>
        /// <param name="candidatesList"></param>
        /// <returns></returns>
        public static List<List<RegionRerepresentation>> IndexingInRegionRepresentationList(List<RegionRerepresentation> candidatesList)
        {
            var result = new List<List<RegionRerepresentation>>();
            var listCount = candidatesList.Count;
            var nhHeightInFrequency = 559;
            var nhWidthInMillisecond = 150.8;
            var rowsCount = (int)(candidatesList[listCount - 1].AudioFrequencyIndex / nhHeightInFrequency) + 1;
            var colsCount = (int)(candidatesList[listCount - 1].AudioTimeIndex / nhWidthInMillisecond) + 1;
            var candidatesArray = StatisticalAnalysis.RegionRepresentationListToArray(candidatesList, rowsCount, colsCount);
            
            for (int rowIndex = 0; rowIndex < rowsCount; rowIndex++)
            {
                for (int colIndex = 0; colIndex < colsCount; colIndex++)
                {
                    //To do: create a vector of distance score for each frequency band. 
                    result[rowIndex].Add(candidatesArray[rowIndex, colIndex]);
                }
            }
            return result;
        }
    }
}
