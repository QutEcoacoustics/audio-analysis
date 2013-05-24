using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dong.Felt
{
    class StatisticalAnalysis
    {
        // 2D matrix to 1D matrix
        public static double[] MatrixTransformation(double[,] matrix)
        {
            var row = matrix.GetLength(0);
            var col = matrix.GetLength(1);

            int lengthOfMatrix = row * col; 
            var result = new double[lengthOfMatrix];

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    result[i * row + j] = matrix[i, j]; 
                }
            }

            return result; 
        }

    }
}
