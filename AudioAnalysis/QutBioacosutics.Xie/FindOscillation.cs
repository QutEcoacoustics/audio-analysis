using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLibrary;
using MathNet.Numerics;
using System.Drawing;
using System.Drawing.Imaging;
using AudioAnalysisTools.StandardSpectrograms;
using AudioAnalysisTools.DSP;

namespace QutBioacosutics.Xie
{
    class FindOscillation
    {
        public double[] DetectOscillation(double[,] matrix, int zeroBinIndex)
        {
            matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);

            // Get oscillation of the whole duration of one recording
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
      
            int numSubCols = cols / 128;
            int subCols = cols % 128;

            var score = new double[rows];
            var result = new double[rows,cols];

            // Find peaks in the frequency direction
            var peakMatrix = new double[rows, cols];
            var freqlocalPeaks = new FindLocalPeaks();
            peakMatrix = freqlocalPeaks.LocalPeaksOscillation(matrix, 6, 4, 3);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (peakMatrix[i, j] < 0)
                    {
                        peakMatrix[i, j] = 1;
                    }
                    
                }           
            }

            for (int i = 1; i < (rows-1); i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if ((peakMatrix[i, j] > 0) & (peakMatrix[(i+1), j] == 0) & (peakMatrix[(i-1), j] == 0))
                    {
                        peakMatrix[i, j] = 0;
                        peakMatrix[(i+1), j] = 0;
                        peakMatrix[(i-1), j] = 0;
                    }                
                }            
            }

            cols = cols - subCols;
            int windoezSize = 128;
            var numberPeriodicity = new double[numSubCols];

            for (int r = zeroBinIndex; r < rows; r++)
            {
        
                for (int i = 0; i < numSubCols; i++)
                {
                    var listLow = new List<int>();

                    var arrayLow = new int[windoezSize];
                    for (int c = i * windoezSize; c < (i + 1) * windoezSize; c++)
                    {
                        if (peakMatrix[r, c] > 0)
                        {
                            listLow.Add(c);
                        }

                    }

                    arrayLow = listLow.ToArray();
                    if (arrayLow.Length > 2)
                    {
                        for (int j = 0; j < (arrayLow.Length - 3); j++)
                        {          
                                double tempA = arrayLow[j + 1] - arrayLow[j];
                                double tempB = arrayLow[j + 2] - arrayLow[j + 1];
                                double tempC = arrayLow[j + 3] - arrayLow[j + 2];

                                if ((tempA < 30) & (tempB < 30) & (tempC < 30))
                                {
                                    double diffA = tempB - tempA;
                                    double diffB = tempC - tempB;
                                    if (diffA < 4 & diffB < 4)
                                    {
                                        for (int t = arrayLow[0]; t < arrayLow[arrayLow.Length - 1]; t++)
                                        {
                                            result[r, t] = 1;
                                        }
                                        score[r]++;
                                        break;
                                    }
                                }
                        }                        
                    }                          
                }
            }

            // Calculate the last part
            for (int r = zeroBinIndex; r < rows; r++)
            {
                var listLow = new List<int>();
                var arrayLow = new int[windoezSize];
                for (int c = (cols + 1); c < cols + subCols; c++)
                {
                    if (peakMatrix[r, c] > 0)
                    {
                        listLow.Add(c);
                    }
                }

                arrayLow = listLow.ToArray();
                if (arrayLow.Length > 2)
                {
                    for (int j = 0; j < (arrayLow.Length - 3); j++)
                    {
                        double tempA = arrayLow[j + 1] - arrayLow[j];
                        double tempB = arrayLow[j + 2] - arrayLow[j + 1];
                        double tempC = arrayLow[j + 3] - arrayLow[j + 2];

                        if ((tempA < 30) & (tempB < 30) & (tempC < 30))
                        {
                            double diffA = tempB - tempA;
                            double diffB = tempC - tempB;
                            if (Math.Abs(diffA) < 4 & Math.Abs(diffB) < 4)
                            {
                                for (int t = arrayLow[0]; t < arrayLow[arrayLow.Length - 1]; t++)
                                {
                                    result[r, t] = 1;
                                }
                                score[r]++;
                                break;
                            }

                        }
                    }
                }

            }

            // Normalize the score
            var norArray = new double[score.Length];
            for (int i = 0; i < score.Length; i++)
            {
                norArray[i] = score[i] / (numSubCols + 1);
            }
            return norArray;
        }

        public static double[,] CalculateOscillationRate(SpectrogramStandard sonogram, Configuration.CanetoadConfiguration canetoadConfig)
        {
            int minHz = canetoadConfig.MaximumFrequencyCanetoad;
            int maxHz = canetoadConfig.MaximumFrequencyCanetoad;

            double dctDuration = canetoadConfig.Dct_DurationCanetoad;
            double dctThreshold = canetoadConfig.Dct_ThresholdCanetoad;

            int minOscilFreq = canetoadConfig.MinimumOscillationNumberCanetoad;
            int maxOscilFreq = canetoadConfig.MaximumOscillationNumberCanetoad;

            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);

            int dctLength = (int)Math.Round(sonogram.FramesPerSecond * dctDuration);
            int minIndex = (int)(minOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi
            int maxIndex = (int)(maxOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi

            if (maxIndex > dctLength) return null;       //safety check

            int rows = sonogram.Data.GetLength(0);
            int cols = sonogram.Data.GetLength(1);
            Double[,] hits = new Double[rows, cols];
            Double[,] matrix = sonogram.Data;

            double[,] cosines = MFCCStuff.Cosines(dctLength, dctLength); //set up the cosine coefficients
            for (int c = minBin; c <= maxBin; c++)//traverse columns - skip DC column
            {
                for (int r = 0; r < rows - dctLength; r++)
                {
                    var array = new double[dctLength];
                    //accumulate J columns of values
                    for (int i = 0; i < dctLength; i++)
                        for (int j = 0; j < 5; j++) array[i] += matrix[r + i, c + j];

                    array = DataTools.SubtractMean(array);
                    //     DataTools.writeBarGraph(array);

                    double[] dct = MFCCStuff.DCT(array, cosines);
                    for (int i = 0; i < dctLength; i++) dct[i] = Math.Abs(dct[i]);//convert to absolute values
                    dct[0] = 0.0; dct[1] = 0.0; dct[2] = 0.0; dct[3] = 0.0; dct[4] = 0.0;//remove low freq oscillations from consideration
                    dct = DataTools.normalise2UnitLength(dct);
                    //dct = DataTools.normalise(dct); //another option to normalise
                    int indexOfMaxValue = DataTools.GetMaxIndex(dct);
                    double oscilFreq = indexOfMaxValue / dctDuration * 0.5; //Times 0.5 because index = Pi and not 2Pi

                    //DataTools.MinMax(dct, out min, out max);
                    //      DataTools.writeBarGraph(dct);

                    //mark DCT location with oscillation freq, only if oscillation freq is in correct range and amplitude
                    if ((indexOfMaxValue >= minIndex) && (indexOfMaxValue <= maxIndex) && (dct[indexOfMaxValue] > dctThreshold))
                    {
                        for (int i = 0; i < dctLength; i++) hits[r + i, c] = oscilFreq;
                    }
                    r += 5; //skip rows
                }
                c++; //do alternate columns
            }

            return hits;
        }
    }
}
