using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLibrary;
using AudioAnalysisTools;
using Accord.Math;


namespace QutBioacosutics.Xie
{
    class FindHarmonics
    {
        public static double[,] GetHarmonic(double[,] matrix, int component, int sensity, int diffThreshold)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            var result = new double[rows, cols];
            
            for (int c = 0; c < cols; c++)
            {
                var tempArray = new int[rows];
                double[] Column = MatrixTools.GetColumn(matrix, c);
                var index = XieFunction.ArrayIndex(Column);
                var diffIndex = new List<int>();
                if(index.Length > component)
                {                    
                    for (int i = 0; i < (index.Length - 1); i++)
                    {
                        var temp = index[i + 1] - index[i];
                        diffIndex.Add(temp);                    
                    }

                    for (int j = 0; j < (diffIndex.Count - 2); j++)
                    {
                        if (Math.Abs(diffIndex[j]) < diffThreshold & Math.Abs(diffIndex[j + 1]) < diffThreshold & Math.Abs(diffIndex[j + 2]) < diffThreshold)
                        {
                            int tempA = diffIndex[j + 1] - diffIndex[j];
                            int tempB = diffIndex[j + 2] - diffIndex[j + 1];
                           
                            if ((Math.Abs(tempA) < sensity) & (Math.Abs(tempB) < sensity) )
                            {
                                for (int n = index[j]; n < index[j + 1]; n++)
                                {
                                    result[n, c] = 1;
                                                                
                                }
                            }                       
                        }
                    }
                }
            }

            return result;        
        }

    }
}
