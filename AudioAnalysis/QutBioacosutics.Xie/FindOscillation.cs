using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;
using MathNet.Numerics;

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

            // get oscillation of the whole duration of one recording
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            var rowsIndex = rows / 5;
            var rowsResidual = rows % 5;

            var diffArray = new double[cols - 1];

            var diffMatrix = new double[rows, (cols - 1)];

            for (int i = 0; i < rowsIndex; i++)
            {
                
                for (int c = 0; c < (cols - 1); c++) 
                {
                    double Energy = 0;
                    for (int j = i * 5; j < (i + 1) * 5; j++)
                    {
                        Energy = Energy + Math.Pow(matrix[j, c] - matrix[j, (c + 1)], 2) * matrix[j,c];
                    }

                    diffArray[c] = Energy;
                }

                for (int k = i * 5; k < (i + 1) * 5; k++)
                { 
                    for(int c = 0; c <(cols - 1); c++)
                    {
                        diffMatrix[k, c] = diffArray[c];
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

            int numSubCols = cols / 128;
            int subCols = cols % 128;

            //var intensityList = new List<double[]>();
            //var periodicityList = new List<double[]>();

            //var intensityArray = new double[rows];
            var score = new double[rows];
            var result = new double[rows, cols];

            // find peaks in the frequency direction

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

            //var image = ImageTools.DrawMatrix(peakMatrix);
            //image.Save(@"C:\Jie\output\3.png");

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

                    //double[] signal2 = { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 };
                    //double[] signal2 = { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };

                    var crossArray = XieFunction.CrossCorrelation(tempArray, tempArray);
                    
                    //tempDct = XieFunction.DCT(tempArray);
                    //calculate stand deviation of tempDct

                    var sd = XieFunction.StandarDeviation(tempDct);
                    var seaLevel = sd; 

                    for (int t = 0; t < tempArray.Length; t++)
                    {
                        if (tempArray[t] > seaLevel)
                        {
                            arrayF[t] = tempArray[t];
                        }
                        else
                        {
                            arrayF[t] = 0;
                        }
                    }

                    //find the start point and end point
                    var segmentList = new List<int[]>();
                    
                    //int h = 1;
                    //while(h < arrayF.Length)
                    //{
                        
                    //    if (arrayF[h] != 0 & arrayF[h - 1] == 0)
                    //    {
                    //        var segment = new int[2];
                    //        segment[0] = h;

                    //        var s = h + 1;
                    //        for (int j = s; j < (arrayF.Length - 3); j++)
                    //        {
                    //            if (arrayF[j] == 0 & ((arrayF[j + 1] != 0 & arrayF[j + 2] != 0) || (arrayF[j + 1] != 0 & arrayF[j + 3] != 0)))   //& arrayF[j + 3] != 0 & arrayF[j + 4] != 0
                    //            {
                    //                segment[1] = j;
                    //                segmentList.Add(segment);
                    //                break;
                    //            }
                    //            h = j + 2;
                    //        }
                    //        h++;
                    //        if (h == (arrayF.Length - 1))
                    //            break;
                    //    }
                    //    else
                    //    {
                    //        h++;
                    //    }
                    //}


                    var segment = new int[2];
                    for (int s3 = 0; s3 < arrayF.Length; s3++)
                    {
                        if (arrayF[s3] != 0 & arrayF[s3 + 1] == 0)
                        {
                            segment[0] = s3;
                            break;
                        }

                    }

                    for (int s4 = arrayF.Length; s4 > 2; s4--)
                    {
                        if (arrayF[s4 - 1] == 0 & arrayF[s4 - 2] != 0)
                        {
                            segment[1] = s4;
                            break;
                        }

                    }

                    segmentList.Add(segment);
                    
                    for (int s1 = 0; s1 < segmentList.Count; s1++)
                    {
                        int start = segmentList[s1][0];
                        int end = segmentList[s1][1];
                        var array = new double[arrayF.Length];
                        var position = new List<int>();
                        var interval = new List<int>();
                        for (int index = 0; index < arrayF.Length; index++)
                        {
                            if (index >= start & index <= end)
                            {
                                array[index] = arrayF[index];
                            }
                            else
                            {
                                array[index] = 0;
                            }
                                                    
                        }


                        var Iarray = XieFunction.IDCT(array);

                        for (int num = 0; num < (Iarray.Length - 1); num++)
                        {
                            if ((Iarray[num] > 0 & Iarray[num + 1] < 0) || (Iarray[num] < 0 & Iarray[num + 1] > 0))
                            {
                                position.Add(num);                           
                            }                        
                        }


                        for (int p = 1; p < position.Count; p++)
                        {
                            var temp = 2 * (position[p] - position[p - 1]);
                            interval.Add(temp);
                        }

                       var intervalArray = interval.ToArray();

                       if (intervalArray.Length > 4)
                       {
                           var intervalList = new List<double>();
                           for (int a = 0; a < intervalArray.Length; a++)
                           {
                               intervalList.Add((double)intervalArray[a]);

                           }


                           var average = intervalList.ToArray().Average();
                           var standard = XieFunction.StandarDeviation(intervalList.ToArray());


                           if (3 * standard < average)
                           {
                               for (int k = (n * 128 + start); k < (n * 128 + end); k++)
                               {
                                   result[i, k] = 1;

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






    }
}
