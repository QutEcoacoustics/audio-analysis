using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLibrary;

namespace QutBioacosutics.Xie
{
    class RemoveSparseHits
    {
        /// <summary>
        /// For each coloum, if the number of hist is smaller than 3, remove it.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] PruneHits(double[,] Hits)
        {
            var matrix = MatrixTools.MatrixRotate90Anticlockwise(Hits);

            int row = matrix.GetLength(0);
            int col = matrix.GetLength(1);
            const int Count = 3;

            for (int c = 0; c < col; c++)
            {
                int cnt = 0;
                for (int r = 0; r < row; r++)
                {
                    if (matrix[r, c] != 0)
                        cnt++;
                }

                if (cnt < Count) 
                {
                    for (int r = 0; r < row; r++)
                    {
                        matrix[r, c] = 0;
                    }
                }
            }

            return matrix;
        }

    }
}
