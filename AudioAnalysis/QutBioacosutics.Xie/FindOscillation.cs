using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;

namespace QutBioacosutics.Xie
{
    class FindOscillation
    {
        public double[] getOscillation(double[,] matrix, int zeroBinIndex)
        {

            matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);

            // get oscillation of the whole duration of one recording

            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            var intensity = new double[rows];
            var periodicity = new double[rows]; 

            for (int r = (zeroBinIndex + 2); r < (rows - 2); r++)
            {
                var arrayLow = new double[cols];
                var arrayHigh = new double[cols];
                for (int j = 0; j < cols; j++)
                {
                    for (int i = (r - 2); i < (r + 2); i++)
                    {
                        arrayLow[j] = matrix[i, j] + arrayLow[j];                     
                    }
                }

                arrayLow = DataTools.DiffFromMean(arrayLow);

                for (int j = 0; j < cols; j++)
                {
                    for (int i = ((r + 1) - 2); i < ((r + 1) + 2); i++)
                    {
                        arrayHigh[j] = matrix[i, j] + arrayHigh[j];
                    }
                }

                arrayHigh = DataTools.DiffFromMean(arrayHigh);


                // do Xcorrelation

                var spectrum = CrossCorrelation.CrossCorr(arrayLow, arrayHigh);

                for (int s = 0; s < 3; s++) spectrum[s] = 0.0;

                int maxId = DataTools.GetMaxIndex(spectrum);
                double intensityValue = spectrum[maxId];
                intensity[r] = intensityValue;

                double period = 0.0;
                if (maxId != 0) period = 2 * cols / (double)maxId;
                periodicity[r] = period;
            }


            return periodicity;
        }
    }
}
