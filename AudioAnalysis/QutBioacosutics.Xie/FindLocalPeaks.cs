using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;



namespace QutBioacosutics.Xie
{
    public class FindLocalPeaks
    {

        public double[,] LocalPeaks(double[,] amplitude, double ampthreshold, int range, int distance)
        {
            
            var row = amplitude.GetLength(1);
            var column = amplitude.GetLength(0);

            // var rotatedMatrix = MatrixTools.MatrixRotate90Anticlockwise(amplitude);
            // temp_amp is a matrix used to save local peaks
            double[,] localpeaks = new double[column,row];
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
                        & (amplitude[nc, nr] - amplitude[nc, nr - 1 + range]) > (ampthreshold - 3)
                        & (amplitude[nc, nr] - amplitude[nc, nr - 1 + range]) > (ampthreshold - 3))
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

                  for(int i = 0; i < columnPeak.Count; i++)
                  {
                      localpeaks[nc, rowIndex[i]] = amplitude[nc, rowIndex[i]];
              
                  }

             }
            

            return localpeaks;

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


        public double[,] LocalLongPeaks(double[,] amplitude, double ampthreshold, int range, int distance)
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
                        & (amplitude[nc, nr] - amplitude[nc, nr - range]) > ampthreshold)
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


        public double[,] LocalPeaksOscillationNew(double[,] amplitude)
        {

            var row = amplitude.GetLength(1);
            var column = amplitude.GetLength(0);

            // var rotatedMatrix = MatrixTools.MatrixRotate90Anticlockwise(amplitude);
            // temp_amp is a matrix used to save local peaks
            double[,] localpeaks = new double[column, row];

            int numSubCols = column / 128;
            int subCols = column % 128;

            for (int i = 0; i < row; i++)
            {
                for (int c = 0; c < numSubCols; c++)
                {

                    var tempList = new List<double>();
                    for (int j = c * 128; j < (c + 1) * 128; j++)
                    {
                        tempList.Add(amplitude[i, j]);
                    }

                    var tempArray = tempList.ToArray();

                    tempArray = DataTools.filterMovingAverage(tempArray,5);

                    tempArray = DataTools.AutoCorrelation(tempArray,0,127);






                }            
            }



                return null;
        }


        /// <summary>
        /// Find maximum value in each frequency band according to the frog's dominant frequency.
        /// 9 frog species and canetoad will be analysed, they are Litoria rothii (1800Hz) , Litoria caerulea (500Hz), Litoria fellax (4750Hz), 
        /// Litoria gracillenta (2700Hz), Litoria latopalmata (3000Hz), Litoria nasuta (2800Hz), Canetoad, Mixophyes fasiolatus (1200Hz), 
        /// Litoria verreauxii(3100Hz), Rheobatrachus silus (1500Hz)
        /// 
        /// Thus the frequency band is divided into 300 - 800Hz, 1100Hz - 1400Hz, 1400Hz - 1600Hz, 1600 - 2000Hz, 2500Hz - 3200Hz, 4500Hz - 5000Hz.
        /// Among them, the frequency band, 2500 - 3200Hz, contains three frog species. If there are frog chorus, a mismatch is unavoidable.
        /// 
        /// Ampthreshold is a experimental value, here it is 4 dB
        /// </summary>
         
        public double[,] LocalPeaksDifferentBands(double[,] matrix, double ampthreshold)
        {
            // According to the frequency bands, calculate the bin range.


            // Find the maximum of the specify bin range.


            return null;
        }



    }
}
