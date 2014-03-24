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

            int numSubCols = cols / 128;
            int subCols = cols % 128;
            
            //var intensityList = new List<double[]>();
            //var periodicityList = new List<double[]>();

            //var intensityArray = new double[rows];
            var score = new double[rows];
            var result = new double[rows,cols];

            // find peaks in the frequency direction

            matrix = XieFunction.MedianFilter(matrix, 6);

            //var image = ImageTools.DrawMatrix(matrix);
            //image.Save(@"C:\Jie\output\3.png");

            var peakMatrix = new double[rows, cols];

            var freqlocalPeaks = new FindLocalPeaks();

            peakMatrix = freqlocalPeaks.LocalPeaksOscillation(matrix,6,4,3);

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
                    var listLow = new List<double>();

                    var arrayLow = new double[windoezSize];
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
                                        
                                        for(int t = i * windoezSize; t < (i + 1) * windoezSize; t++)
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

            // calculate the last part
            for (int r = zeroBinIndex; r < rows; r++)
            {
                //var periodicity = new double[numSubCols];

                var listLow = new List<double>();

                var arrayLow = new double[windoezSize];
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

                                for (int t = cols; t < cols + subCols; t++)
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

            
            return score;
        }
    }
}
