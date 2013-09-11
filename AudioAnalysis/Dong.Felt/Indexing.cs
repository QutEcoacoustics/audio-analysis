
namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Representations;
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
                    ridgeNeighbourhood[rowIndex, colIndex].RowIndex = rowIndex;
                    ridgeNeighbourhood[rowIndex, colIndex].ColIndex = colIndex;
                    result.Add(ridgeNeighbourhood[rowIndex, colIndex]);
                }
            }

            return result; 
        }

        // function to scan a list of representation in an audio file  within the same frequency band with the query.
        public static List<Tuple<double, int, int>> CandidatesIndexFromFile(Query query, List<RidgeDescriptionNeighbourhoodRepresentation> queryRepresentation, RidgeDescriptionNeighbourhoodRepresentation[,] ridgeNeighbourhood)
        {
            var result = new List<Tuple<double, int, int>>();
            var rowsCount = ridgeNeighbourhood.GetLength(0);
            var colsCount = ridgeNeighbourhood.GetLength(1);
            var nhCountInRow = query.nhCountInRow;
            var nhCountInColumn = query.nhCountInColumn;
            var queryRepresentationMatrix = StatisticalAnalysis.RidgeNhListToArray(queryRepresentation, nhCountInRow, nhCountInColumn);
            for (int rowIndex = 0; rowIndex < rowsCount; rowIndex++)
            {
                for (int colIndex = 0; colIndex < colsCount; colIndex++)
                {
                    var subRegionMatrix = StatisticalAnalysis.SubRegionMatrix(ridgeNeighbourhood, rowIndex, colIndex, rowIndex + nhCountInRow, colIndex + nhCountInColumn);
                    var distance = SimilarityMatching.SimilarityScoreRidgeDiscription(ridgeNeighbourhood, queryRepresentationMatrix);
                    result.Add(Tuple.Create(distance, rowIndex, colIndex));
                }
            }

            return result;
        }

    }
}
