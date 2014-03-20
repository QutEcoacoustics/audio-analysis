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

            //var trackMatrix = new double[(rows - 1),cols];

            //for (int i = 0; i < (rows - 1); i++)
            //{
            //    for (int j = 0; j < cols; j++)
            //    {
            //        if(matrix[i,j] > 0)
            //        {
            //            trackMatrix[i,j] = 1;                     
            //        }                                  
            //    }            
            //}


            //for (int i = 0; i < (cols - 1); i++)
            //{

            //    double[] prevColumn = MatrixTools.GetColumn(trackMatrix, i);
            //    double[] thiscolumn = MatrixTools.GetColumn(trackMatrix, (i + 1));


            //    if ((XieFunction.ArrayCount(prevColumn) > colThreshold) & (XieFunction.ArrayCount(thiscolumn) > colThreshold))
            //    {
            //        var spectrum = CrossCorrelation.CrossCorr(prevColumn, thiscolumn);

            //        for (int n = 0; n < zeroBinIndex; n++) spectrum[n] = 0.0;
            //        spectrum = DataTools.NormaliseArea(spectrum);
            //        int maxIndex = DataTools.GetMaxIndex(spectrum);
            //        double intensityValue = spectrum[maxIndex];
            //        intensity[i] = intensityValue;

            //        double period = 0.0;
            //        if (maxIndex != 0)
            //        {
            //            period = 2 * rows / maxIndex;
            //        }

            //        periodicity[i] = period;

            //        i++;
            //    }
            //}

            //var result = new List<int>[cols];
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
                        if (diffIndex[j] < 20 & diffIndex[j+1] < 20 & diffIndex[j+2] < 20 & diffIndex[j+3] < 20)
                        {
                            int tempA = diffIndex[j + 1] - diffIndex[j];
                            int tempB = diffIndex[j + 2] - diffIndex[j + 1];
                            int tempC = diffIndex[j + 3] - diffIndex[j + 2];

                            if ((tempA < 3) & (tempB < 3) & (tempC < 3))
                            {
                                result[index[j], c] = 1;

                                result[index[j + 1], c] = 1;

                                result[index[j + 2], c] = 1;

                            }                       
                        }
                        //int avtemp = (tempA + tempB + tempC) / 3;
                        //int avRow = (index[j] + index[j + 1] + index[j + 2]);
                    }
                }
   
            }


            // if the structure is stable for a distance, then it will be regarded as a harmonic structure

            //int minBin = 0;
            //int maxBin = rows;
            //int hzWidth = 0;
            //int expectedHarmonicCount = 2;
            //double amplitudeThreshold = 0.0;

            //result = MatrixTools.MatrixRotate90Clockwise(result);

            //var results = HarmonicAnalysis.CountHarmonicTracks(result, minBin, maxBin,hzWidth,expectedHarmonicCount,amplitudeThreshold);

            //double[] scores = results.Item1;
            //var hits = results.Item2;

            //return Tuple.Create(scores, hits);

            return result;
        
        }

    }
}
