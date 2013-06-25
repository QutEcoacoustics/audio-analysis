

namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AudioAnalysisTools;

    /// <summary>
    /// A class for clustering (grouping) events according to some rules. 
    /// </summary>
    class Clustering
    {
        
        /// <summary>
        /// The ClusterEdge method wants to cluster edges. 
        /// </summary>
        /// <param name="poiList"></param>
        /// <param name="rowsCount"></param>
        /// <param name="colsCount"></param>
        public static List<AcousticEvent> ClusterEdge(List<PointOfInterest> poiList, int rowsCount, int colsCount)
        {
            var matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);
            var result = new List<AcousticEvent>();
            //search for poi connected with each other in three positions, on the right, below and the right-below.
            for (int row = 0; row < rowsCount; row++)
            {
                for (int col = 0; col < colsCount; col++)
                {
                    if (matrix[row, col] == null)
                    {
                        continue;
                    }
                    else
                    {
                        var minRowIndex = row;                       
                        var minColIndex = col;
                        var tuple = Traverse(matrix, row, col);
                        var maxRowIndex = tuple.Item1;
                        var maxColIndex = tuple.Item2;
                        result.Add(new AcousticEvent(minColIndex * 0.0116, (maxColIndex - minColIndex) * 0.0116, 11025 - maxRowIndex * 43.0, 11025 - minRowIndex * 43.0)); 
                    }
                }
            }

            return result;
        }

        public static Tuple<int, int> Traverse(PointOfInterest[,] m,int r, int c)
        {
            var rowsCount = m.GetLength(0);
            var colsCount = m.GetLength(1);
            var maxRowIndex = r;
            var maxColIndex = c;
            if (StatisticalAnalysis.checkBoundary(r + 1, c + 1, rowsCount, colsCount) && (m[r, c + 1] != null || m[r + 1, c] != null || m[r + 1, c + 1] != null))
            {
                if (m[r + 1, c] != null)
                {
                    maxRowIndex++;
                    Traverse(m, maxRowIndex, maxColIndex);
                }
                else
                {
                    if (m[r, c + 1] != null)
                    {
                        maxColIndex++;
                        Traverse(m, maxRowIndex, maxColIndex);
                    }
                    else if (m[r + 1, c + 1] != null)
                    {
                        maxRowIndex++;
                        maxColIndex++;
                        Traverse(m, maxRowIndex, maxColIndex);
                    }
                }
            }
            var result = Tuple.Create(maxRowIndex, maxColIndex);
            return result;           
        }



    }
}
