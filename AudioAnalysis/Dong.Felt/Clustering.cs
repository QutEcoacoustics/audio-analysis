

namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AudioAnalysisTools;
    using System.Drawing;

    /// <summary>
    /// A class for clustering (grouping) events according to some rules. 
    /// </summary>
    class Clustering
    {
        
        /// <summary>
        /// The ClusterEdge method wants to cluster edges which are connected into a acousticEvent. 
        /// </summary>
        /// <param name="poiList"></param>
        /// <param name="rowsCount"></param>
        /// <param name="colsCount"></param>
        public static List<AcousticEvents> ClusterEdges(List<PointOfInterest> poiList, int rowsCount, int colsCount)
        {
            var matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);
            var result = new List<AcousticEvents>();
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
                        int count = 0;
                        var tuple = Traverse(matrix, row, col, count);
                        var maxRowIndex = tuple.Item1;
                        var maxColIndex = tuple.Item2 + 1;
                        result.Add(new AcousticEvents(minColIndex * 0.0116, (maxColIndex - minColIndex) * 0.0116, 11025 - maxRowIndex * 43.0, 11025 - minRowIndex * 43.0));
                    }
                }
            }

            return result;
        }

        public static List<AcousticEvents> ClusterEvents(List<AcousticEvents> acousticEvent, int threshold)
        {
            var result = new List<AcousticEvents>();
            var copyAcousticEvents = new List<AcousticEvents>(acousticEvent);
            foreach (var ae in acousticEvent)
            {
                foreach (var ae1 in copyAcousticEvents)
                {
                    if (ae == ae1)
                    {
                        continue;
                    }
                    else
                    {
                        //if (Distance.ManhattanDistance(ae.Centroid, ae1.Centroid) < threshold)
                        //{
                        //    result.Add(new AcousticEvents(AcousticEvents.MergeEvents(ae, ae1).TimeStart,
                        //                                  AcousticEvents.MergeEvents(ae, ae1).Duration,
                        //                                  AcousticEvents.MergeEvents(ae, ae1).MinFreq,
                        //                                  AcousticEvents.MergeEvents(ae, ae1).MaxFreq));
                        //    acousticEvent.Remove(ae);
                        //}
                        result.Add(new AcousticEvents(AcousticEvents.MergeEvents(ae, ae1).TimeStart,
                                                      AcousticEvents.MergeEvents(ae, ae1).Duration,
                                                      AcousticEvents.MergeEvents(ae, ae1).MinFreq,
                                                      AcousticEvents.MergeEvents(ae, ae1).MaxFreq));
                    }
                }
            }
            return result; 
        }

        /// <summary>
        /// Traverse method is used to iteratively search edges connected each other. 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="count">
        /// To calculate the number of times this method is called. 
        /// </param>
        /// <returns></returns>
        public static Tuple<int, int> Traverse(PointOfInterest[,] matrix,int row, int col, int count)
        {
            var rowsCount = matrix.GetLength(0);
            var colsCount = matrix.GetLength(1);         
            var rightPoi = matrix[row, col + 1];
            var bottomPoi = matrix[row + 1, col];
            var rightBottomPoi = matrix[row + 1, col + 1];
            //var traversedFlag = new bool[rowsCount, colsCount];
            // base condition (limit condition)
            if ((!StatisticalAnalysis.checkBoundary(row + 1, col + 1, rowsCount, colsCount)) || (rightPoi == null && bottomPoi == null && rightBottomPoi == null))
            {                          
                return Tuple.Create(row, col);               
            }
            // continuation
            else 
            {
                if (matrix[row, col + 1] != null && matrix[row + 1, col] == null && matrix[row + 1, col + 1] == null)
                {
                    matrix[row, col + 1] = null;
                    return Traverse(matrix, row, col + 1, count + 1);
                }
                else if (matrix[row + 1, col] != null && matrix[row, col + 1] == null && matrix[row + 1, col + 1] == null)
                {
                    matrix[row + 1, col] = null;
                    return Traverse(matrix, row + 1, col, count + 1);
                }
                else if (matrix[row + 1, col + 1] != null && matrix[row, col + 1] == null && matrix[row + 1, col] == null)
                {
                    matrix[row + 1, col + 1] = null;
                    return Traverse(matrix, row + 1, col + 1, count + 1);
                }
                else
                {
                    return Traverse(matrix, row + 1, col + 1, count + 1);
                }
            }
        }



    }
}
