using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLibrary;
using AudioAnalysisTools.StandardSpectrograms;


namespace QutBioacosutics.Xie
{
    public class FindLocalPeaks
    {

        public static double[,] LocalPeaks(SpectrogramStandard sonogram, double ampthreshold, int range, int distance, int minHz, int maxHz)
        {
            var matrix = sonogram.Data;

            var row = matrix.GetLength(1);
            var column = matrix.GetLength(0);

            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);
            double[,] localpeaks = new double[column,row];
            for (int nc = 0; nc < column; nc++)
            {
                int index = 0;
                List<double> columnPeak = new List<double>();
                List<int> rowIndex = new List<int>();
                for (int nr = minBin; nr < maxBin; nr++)
                {
                    if ((matrix[nc, nr] - matrix[nc, nr + range]) > ampthreshold
                        & (matrix[nc, nr] - matrix[nc, nr - range]) > ampthreshold
                        & (matrix[nc, nr] - matrix[nc, nr - 1 + range]) > (ampthreshold - 3)
                        & (matrix[nc, nr] - matrix[nc, nr - 1 + range]) > (ampthreshold - 3))
                    {
                        columnPeak.Add(matrix[nc, nr]);
                        rowIndex.Add(nr);
                        index = index + 1;
                    }                   
                }
                // Remove close peaks
                if (index > 1)
                {
                    for (int j = 0; j < (index - 1); j++)
                    {
                        if ((rowIndex[j + 1] - rowIndex[j]) < distance)
                        {
                            if (columnPeak[j + 1] > columnPeak[j])
                            {
                                columnPeak.Remove(columnPeak[j]);
                                rowIndex.Remove(rowIndex[j]);
                                j = j - 1;
                            }
                            else
                            {
                                columnPeak.Remove(columnPeak[j + 1]);
                                rowIndex.Remove(rowIndex[j + 1]);
                                j = j - 1;
                            }
                        }

                        if (j >= (rowIndex.Count - 2)) 
                        {
                            break;
                        }
                     } 
                  }

                  for(int i = 0; i < columnPeak.Count; i++)
                  {
                      localpeaks[nc, rowIndex[i]] = matrix[nc, rowIndex[i]];
                  }
             }
            return localpeaks;
        }

        public static double[,] Max(SpectrogramStandard sonogram, double ampthreshold, int minHz, int maxHz)
        {
            var matrix = MatrixTools.MatrixRotate90Anticlockwise(sonogram.Data);
            var row = matrix.GetLength(0);
            var column = matrix.GetLength(1);

            int maxBin = row - (int)(minHz / sonogram.FBinWidth) - 1;
            int minBin = row - (int)(maxHz / sonogram.FBinWidth) - 1;
           
            double[,] result = new double[row, column];
            for (int nc = 0; nc < column; nc++)
            {
                List<double> columnValue = new List<double>();
                int binTemp = 0;
                for (int nr = minBin; nr < maxBin; nr++)
                {
                    columnValue.Add(matrix[nr, nc]);
                }
                
                double max = columnValue.Max();
                if (max > ampthreshold)
                {
                    binTemp = columnValue.IndexOf(max) + minBin;
                    result[binTemp, nc] = max;
                }
            }
            return result;
        }

        public double[,] LocalPeaksOscillation(double[,] amplitude, double ampthreshold, int range, int distance)
        {

            var row = amplitude.GetLength(1);
            var column = amplitude.GetLength(0);

            // var rotatedMatrix = MatrixTools.MatrixRotate90Anticlockwise(amplitude);
            // temp_amp is a matrix used to save local peaks
            double[,] localpeaks = new double[column, row];
            // temp is an array for saving peaks of each column
            // double[] temp = new double[row];

            for (int nc = 0; nc < column; nc++)
            {
                int index = 0;

                List<double> columnPeak = new List<double>();
                List<int> rowIndex = new List<int>();

                for (int nr = range; nr < (row - range); nr++)
                {
                    if ((amplitude[nc, nr] - amplitude[nc, nr + range]) > ampthreshold
                        & (amplitude[nc, nr] - amplitude[nc, nr - range]) > ampthreshold
                        & (amplitude[nc, nr] - amplitude[nc, nr + (range - 1)]) > (ampthreshold-1)
                        & (amplitude[nc, nr] - amplitude[nc, nr - (range - 1)]) > (ampthreshold-1)
                        & (amplitude[nc, nr] - amplitude[nc, nr + (range - 2)]) > (ampthreshold-2)
                        & (amplitude[nc, nr] - amplitude[nc, nr - (range - 2)]) > (ampthreshold-2)
                        )
                    {
                        columnPeak.Add(amplitude[nc, nr]);
                        rowIndex.Add(nr);
                        index = index + 1;

                    }
                }

                // remove close peaks

                if (index > 1)
                {
                    for (int j = 0; j < (index - 1); j++)
                    {
                        if ((rowIndex[j + 1] - rowIndex[j]) < distance)
                        {
                            if (columnPeak[j + 1] > columnPeak[j])
                            {
                                columnPeak.Remove(columnPeak[j]);
                                rowIndex.Remove(rowIndex[j]);
                                j = j - 1;
                            }
                            else
                            {
                                columnPeak.Remove(columnPeak[j + 1]);
                                rowIndex.Remove(rowIndex[j + 1]);
                                j = j - 1;
                            }
                        }

                        if (j >= (rowIndex.Count - 2))
                        {
                            break;
                        }

                    }
                }

                for (int i = 0; i < columnPeak.Count; i++)
                {
                    localpeaks[nc, rowIndex[i]] = amplitude[nc, rowIndex[i]];

                }

            }

            return localpeaks;
        }

    }
}
