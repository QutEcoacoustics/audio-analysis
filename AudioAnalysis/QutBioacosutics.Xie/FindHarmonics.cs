using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;
using AudioAnalysisTools;


namespace QutBioacosutics.Xie
{
    class FindHarmonics
    {
        public double[,] getHarmonic(double[,] matrix, int colThreshold, int zeroBinIndex)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            var intensity = new double[cols];
            var periodicity = new double[cols];

       
            var result = new double[rows, cols];
            var tempMatrix = new double[rows, cols];
            
            for (int c = 0; c < cols; c++)
            {
                var tempArray = new int[rows];
                double[] Column = MatrixTools.GetColumn(matrix, c);

                var index = XieFunction.ArrayIndex(Column);
            
                var diffIndex = new List<int>();
                if(index.Length > colThreshold)
                {                    
                    for (int i = 0; i < (index.Length - 1); i++)
                    {
                        var temp = index[i + 1] - index[i];
                        diffIndex.Add(temp);                    
                    }


                    for (int j = 0; j < (diffIndex.Count - 3); j++)
                    {
                        if (Math.Abs(diffIndex[j]) < 20 & Math.Abs(diffIndex[j + 1]) < 20 & Math.Abs(diffIndex[j + 2]) < 20 & Math.Abs(diffIndex[j + 3]) < 20)
                        {
                            int tempA = diffIndex[j + 1] - diffIndex[j];
                            int tempB = diffIndex[j + 2] - diffIndex[j + 1];
                            int tempC = diffIndex[j + 3] - diffIndex[j + 2];

                            if ((Math.Abs(tempA) < 3) & (Math.Abs(tempB) < 3) & (Math.Abs(tempC) < 3))
                            {
                                result[index[j], c] = 1;

                                result[index[j + 1], c] = 1;

                                result[index[j + 2], c] = 1;

                                result[index[j + 3], c] = 1;

                            }                       
                        }

                    }
                }
   
            }


            return result;
        
        }

    }
}
