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

            int numSubCols = cols / 1024;
            int subCols = cols % 1024;

            cols = cols - subCols;


            //var intensityList = new List<double[]>();
            //var periodicityList = new List<double[]>();

            var intensityArray = new double[rows];
            var periodicityArray = new double[rows];


            for (int r = (zeroBinIndex + 3); r < (rows - 3); r++)
            {
                var intensity = new double[numSubCols];
                var periodicity = new double[numSubCols];

                for (int i = 0; i < numSubCols; i++)
                {

                    var listLow = new List<double>();
                    var listHigh = new List<double>();
                    for (int c = i * 1024; c < (i + 1) * 1024; c++)
                    {
                        listLow.Add(matrix[r, c]);
                        //listHigh.Add(matrix[r + 1, c]);

                    }

                    var arrayLow = listLow.ToArray();
                    //var arrayHigh = listHigh.ToArray();


                    //for (int j = 0; j < 1024; j++)
                    //{
                    //    for (int k = (r - 2); k < (r + 2); k++)
                    //    {
                    //        arrayLow[j] = matrix[k, j] + arrayLow[j];
                    //    }
                    //}

                    arrayLow = DataTools.DiffFromMean(arrayLow);

                    //for (int m = 0; m < 1024; m++)
                    //{
                    //    for (int n = ((r + 1) - 2); n < ((r + 1) + 2); n++)
                    //    {
                    //        arrayHigh[m] = matrix[n, m] + arrayHigh[m];
                    //    }
                    //}

                    //arrayHigh = DataTools.DiffFromMean(arrayHigh);


                    // do Xcorrelation

                    //double[] signal4 = { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };

                    //var spectrum = CrossCorrelation.CrossCorr(signal4, signal4);
                    //var spectrum = CrossCorrelation.CrossCorr(arrayLow, arrayHigh);

                    var spectrum = CrossCorrelation.CrossCorr(arrayLow, arrayLow);
                    for (int s = 0; s < 3; s++) spectrum[s] = 0.0;

                    int maxId = DataTools.GetMaxIndex(spectrum);
                    double intensityValue = spectrum[maxId];
                    intensity[i] = intensityValue;

                    double period = 0.0;
                    if (maxId != 0) period = 2 * 1024 / (double)maxId;
                    periodicity[i] = period;

                }
                //var result = (intensity.Sum) / (intensity.Length);

                intensityArray[r] = XieFunction.Sum(intensity) / (intensity.Length);
                periodicityArray[r] = XieFunction.Sum(periodicity) / (periodicity.Length);

            }


            //var intensity = new double[rows];
            //var periodicity = new double[rows];

            //for (int r = (zeroBinIndex + 3); r < (rows - 3); r++)
            //{
            //    var arrayLow = new double[cols];
            //    var arrayHigh = new double[cols];
            //    for (int j = 0; j < cols; j++)
            //    {
            //        for (int k = (r - 2); k < (r + 2); k++)
            //        {
            //            arrayLow[j] = matrix[k, j] + arrayLow[j];
            //        }
            //    }

            //    arrayLow = DataTools.DiffFromMean(arrayLow);

            //    for (int j = 0; j < cols; j++)
            //    {
            //        for (int k = ((r + 1) - 2); k < ((r + 1) + 2); k++)
            //        {
            //            arrayHigh[j] = matrix[k, j] + arrayHigh[j];
            //        }
            //    }

            //    arrayHigh = DataTools.DiffFromMean(arrayHigh);

            //    var spectrum = CrossCorrelation.CrossCorr(arrayLow, arrayHigh);

            //    for (int s = 0; s < 3; s++) spectrum[s] = 0.0;

            //    int maxId = DataTools.GetMaxIndex(spectrum);
            //    double intensityValue = spectrum[maxId];
            //    intensity[r] = intensityValue;

            //    double period = 0.0;
            //    if (maxId != 0) period = 2 * cols / (double)maxId;
            //    periodicity[r] = period;
            
            //}

            return periodicityArray;
        }
    }
}
