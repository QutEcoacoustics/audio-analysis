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
        public double[] getOscillation(double[,] matrix, int zeroBinIndex)
        {

            matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);

            // get oscillation of the whole duration of one recording

            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            //int numSubRows = rows / 5;
            //int subRows = rows % 5;

            //var tempMatrix = new double[rows, cols];

            //for (int j = 0; j < cols; j++)
            //{
            //    for (int r = 0; r < numSubRows; r++)
            //    {
            //        double temp = 0;
            //        for (int i = r * 5; i < (r + 1) * 5; i++)
            //        {
            //            temp = matrix[i, j] + temp;
            //        }

            //        for (int i = r * 5; i < (r + 1) * 5; i++)
            //        {
            //            tempMatrix[i, j] = temp;                    
            //        }
            //    }
            //}

            //matrix = tempMatrix;
       
            int numSubCols = cols / 128;
            int subCols = cols % 128;

            //var intensityArray = new double[rows];
            var score = new double[rows];
            var result = new double[rows,cols];

            // find peaks in the frequency direction

            //matrix = XieFunction.MedianFilter(matrix, 6);
        
            var peakMatrix = new double[rows, cols];
            var freqlocalPeaks = new FindLocalPeaks();
            peakMatrix = freqlocalPeaks.LocalPeaksOscillation(matrix, 6, 4, 3);
            //peakMatrix = freqlocalPeaks.LocalPeaksOscillationNew(matrix);


            //peakMatrix = XieFunction.MedianFilter(peakMatrix, 3);

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

            //peakMatrix = MatrixTools.MatrixRotate90Anticlockwise(peakMatrix);
            //var image = ImageTools.DrawMatrix(peakMatrix);
            //image.Save(@"C:\Jie\output\3.png");

            cols = cols - subCols;
            int windoezSize = 128;
            var numberPeriodicity = new double[numSubCols];

            for (int r = zeroBinIndex; r < rows; r++)
            {
                //var periodicity = new double[numSubCols];
        
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
                                        //for (int t = i * windoezSize; t < (i + 1) * windoezSize; t++)
                                        //{

                                        //}
                                        score[r]++;
                                        break;
                                    }

                                }
                        }                        
                    }                          
                }
            }

            // calculate the last part
            for (int r = zeroBinIndex; r < rows; r++)
            {
                //var periodicity = new double[numSubCols];

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

            // normalize the score
            var norArray = new double[score.Length];
            for (int i = 0; i < score.Length; i++)
            {
                norArray[i] = score[i] / (numSubCols + 1);
            }

            return norArray;
        }

        // A method for extracting oscillation structure
        public double[,] BardeliFindOscillation(double[,] matrix)
        {
            // 5 bins are used to calculate the energy

            matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);

            matrix = XieFunction.MedianFilter(matrix, 5);

            // get oscillation of the whole duration of one recording
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            var rowsIndex = rows / 5;
            var rowsResidual = rows % 5;

            var colsIndex = cols / 128;
            var colsResidual = cols % 128;

            //var diffArray = new double[cols - 1];
            //var diffMatrix = new double[rows, (cols - 1)];

            for (int i = 0; i < rowsIndex; i++)
            {
                for (int j = 0; j < colsIndex; j++)
                {
                    for (int c = j * 128; c < (j + 1) * 128; c++)
                    {
                        var tempArray = new double[128];
                        for (int r = i * 5; r < (i + 1) * 5; r++)
                        {
                            tempArray[c] = matrix[r, c] + tempArray[c];
                                                   
                        }
                        // find loacal peaks

                        //double[] signal2 = { 1, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1 };
                        //tempArray = signal2;
                        //var crossArray = XieFunction.CrossCorrelation(tempArray, tempArray);
                        //var xiecorrelation =  MathNet.Numerics.Statistics.Correlation[tempArray];
                        //var result = (tempArray.Length, tempArray.Length);









                    }
                
                }         
            }

            return null;
        }

        public double[,] DctFindOscillation(double[,] matrix, int zeroBinIndex)
        {
            //matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);

            // get oscillation of the whole duration of one recording

            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            var colsIndex = cols / 128;
            var colsResidual = cols % 128;

            var score = new double[rows];
            var result = new double[rows, cols];

            // find peaks in the frequency direction

            matrix = XieFunction.MedianFilter(matrix, 5);


            for (int i = 0; i < rows; i++)
            {
                for (int n = 0; n < (colsIndex - 1); n++)
                {
                    var tempList = new List<double>();
                    var tempArray = new double[128];
                    var tempDct = new double[128];
                    var arrayF = new double[128]; 
                    for (int k = n * 128; k < (n + 1) * 128; k++)
                    {
                        tempList.Add(matrix[i, k]);

                    }

                    var array = tempList.ToArray();
                    //DataTools.writeBarGraph(array);

                    
                    var A = DataTools.AutoCorrelation(array, 0, (array.Length - 1));

                    int L = A.Length;
                    double[] smooth = DataTools.filterMovingAverage(A, 3);


                    var peakLocation = new List<int>();
                    for (int s = 2; s < (smooth.Length - 2); s++)
                    {
                        if (smooth[s] - smooth[s - 1] > 0 & smooth[s] - smooth[s + 1] > 0
                            & smooth[s] - smooth[s - 2] > 0 & smooth[s] - smooth[s - 2] > 0)
                        {
                            peakLocation.Add(s);
                            s = s + 2;
                        }
                    }

                    //find the interval
                    var peakArray = peakLocation.ToArray();
                    var intervalList = new List<int>();
                    for (int s = 1; s < peakArray.Length; s++)
                    {
                        intervalList.Add(peakArray[s] - peakArray[s - 1]);
                    }

                    var intervalArray = intervalList.ToArray();

                    var doubleArray = new double[intervalArray.Length];
                    for (int s = 0; s < intervalArray.Length; s++)
                    {
                        doubleArray[s] = (double)intervalArray[s];
                    }



                    if (doubleArray.Length > 3)
                    {
                        for (int s = 0; s < (doubleArray.Length - 3); s++)
                        {
                            var arrayList = new List<double>();
                            for (int t = s; t < s + 2; t++)
                            {
                                arrayList.Add(doubleArray[t]);                            
                            }
                            double average;
                            double sd;
                            NormalDist.AverageAndSD(arrayList.ToArray(), out average, out sd);
                            if (sd * 3 < average)
                            {
                                for (int c1 = n * 128; c1 < (n + 1) * 128; c1++)
                                {
                                    result[i, c1] = 1;

                                }
                            
                            }
                            
                        }

                    }

                }

            }
            return result;
        }

        public double[,] CrossCorrelationFindOscillation(double[,] matrix, int zeroBinIndex)
        {

            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            var colsIndex = cols / 128;
            var colsResidual = cols % 128;

            int numSubCols = cols / 128;
            int subCols = cols % 128;

            var score = new double[rows];
            var result = new double[rows, cols];


            matrix = XieFunction.MedianFilter(matrix, 6);

            var peakMatrix = new double[rows, cols];

            var freqlocalPeaks = new FindLocalPeaks();

            peakMatrix = freqlocalPeaks.LocalPeaksOscillation(matrix, 6, 4, 3);
        
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (peakMatrix[i, j] < 0)
                    {
                        peakMatrix[i, j] = 0;
                    }
                    if (peakMatrix[i, j] >= 1)
                    {
                        peakMatrix[i, j] = 1;
                    }

                }
            }

            for (int i = 1; i < (rows - 1); i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if ((peakMatrix[i, j] > 0) & (peakMatrix[(i + 1), j] == 0) & (peakMatrix[(i - 1), j] == 0))
                    {
                        peakMatrix[i, j] = 0;
                        peakMatrix[(i + 1), j] = 0;
                        peakMatrix[(i - 1), j] = 0;
                    }
                }
            }

            // get oscillation of the whole duration of one recording

            for (int i = 0; i < rows; i++)
            {
                for (int n = 0; n < (colsIndex - 1); n++)
                {
                    var tempList = new List<double>();
                    var tempArray = new double[128];
                    var tempDct = new double[128];
                    var arrayF = new double[128];
                    for (int k = n * 128; k < (n + 1) * 128; k++)
                    {
                        tempList.Add(peakMatrix[i, k]);

                    }

                    tempArray = tempList.ToArray();

                    
                    //double[] signal2 = { 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1};
                    //double[] signal2 = { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };

                    var crossArray = tempArray;

                    //var crossArray = signal2;
                    //var crossArray = XieFunction.CrossCorrelation(signal2, signal2);

                    // find position
                    var position = new List<int>();
                    var interval = new List<int>();
                    for (int n1 = 0; n1 < crossArray.Length; n1++)
                    {
                        if (crossArray[n1] > 0)
                            position.Add(n1);                    
                    }

                    // find interval
                    if (interval.Count >= 3)
                    {
                        for (int p = 1; p < position.Count; p++)
                        {
                            var temp = 2 * (position[p] - position[p - 1]);
                            interval.Add(temp);
                        }

                        var intervalArray = interval.ToArray();
                        var newArray = new double[intervalArray.Length];
                        for (int a = 0; a < intervalArray.Length; a++)
                        {
                            newArray[a] = (double)intervalArray[a];
                        }

                        var sd = XieFunction.StandarDeviation(newArray);
                        if (sd < 3)
                        {
                            for (int k = (n * 128); k < (n * 128); k++)
                            {
                                result[i, k] = 1;
                            }

                        }
                    
                    
                    }


                }
            }

            return result;
        }


        // Frog species:Mixophyes fasciolatus, Litoria caerulea, Litoria fallax, Litoria gracilenta, Litoria nasuta, 
        // Litoria verreauxii, Litoria rothii, Litoria latopalmata, Cane_Toad.
        // Calculate the oscillation rate for 9 frog species.
        // Parameters for different frog species: 1. Frequency Band, 2. Dct duration, 3.Minimum OscFreq, 4. Maximum OscFreq, 5. Min amplitude, 6. Min duration, 7. Max duration.
        // Step.1: divide the frequency band into several bands for 9 frog species properly
        // Step.2: for each frequency band, If there is only one frog species,just find the maximum to form tracks. 
        // otherwise find the local maximum to form tracks
        // Step.3: According to tracks, calculate oscillation rate in different frequency bands.

        // CANE_TOAD

        public double[,] OscillationRate(SpectrogramStandard sonogram, int minHz, int maxHz,
                                                   double dctDuration, int minOscilFreq, int maxOscilFreq, double minAmplitude)
        {
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
            //following two lines write matrix of cos values for checking.
            //string txtPath = @"C:\SensorNetworks\Output\cosines.txt";
            //FileTools.WriteMatrix2File_Formatted(cosines, txtPath, "F3");

            //following two lines write bmp image of cos values for checking.
            //string bmpPath = @"C:\SensorNetworks\Output\cosines.png";
            //ImageTools.DrawMatrix(cosines, bmpPath, true);

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
                    if ((indexOfMaxValue >= minIndex) && (indexOfMaxValue <= maxIndex) && (dct[indexOfMaxValue] > minAmplitude))
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
