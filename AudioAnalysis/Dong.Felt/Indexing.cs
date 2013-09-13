
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

        public static List<RegionRerepresentation> IndexingInRegionRepresentationList(List<RegionRerepresentation> candidatesList)
        {
            var result = new List<RegionRerepresentation>();
            var listCount = candidatesList.Count;
            var rowsCount = (int)(candidatesList[listCount - 1].AudioFrequencyIndex / 559) + 1;
            var colsCount = (int)(candidatesList[listCount - 1].AudioTimeIndex / 150.8) + 1;
            var candidatesArray = StatisticalAnalysis.RegionRepresentationListToArray(candidatesList, rowsCount, colsCount);          
            for (int rowIndex = 0; rowIndex < rowsCount; rowIndex++)
            {
                for (int colIndex = 0; colIndex < colsCount; colIndex++)
                {
                    //To do 
                }
            }
            return result;
        }
    }
}
