using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using TowseyLib;
using System.Drawing.Imaging;
using System.IO;
using AForge.Math;
using Accord.Math;



namespace QutBioacosutics.Xie
{
    public static class XieFunction
    {
        /// <summary>
        /// Adding Multiple Elements to a List on One Line
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="elements"></param>
        public static void AddMany<T>(this List<T> list, params T[] elements)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                list.Add(elements[i]);
            }
        }


        public static int ArrayCount(double[] Array)
        {
            int number = 0;
            for (int i = 0; i < Array.Length; i++)
            {
                if (Array[i] > 0) number++;          
            }
            return number;
        }



        //ArrayIndex function in R
        public static int[] ArrayIndex(double[] Array)
        {
            var index = new List<int>();
            for (int i = 0; i < Array.Length; i++)
            {
                if (Array[i] > 0)
                {
                    index.Add(i);               
                }                           
            }
      
            var result = index.ToArray();
            return result;
        }


        public static double Sum(params double[] customerssalary)
        {
            double result = 0;

            for (int i = 0; i < customerssalary.Length; i++)
            {
                result += customerssalary[i];
            }

            return result;
        }
        // Draw tracks on the spectrogram
        //public static void DrawSpectrogram(double[,] matrix)
        //{

        //}

        public static double[,] MedianFilter(double[,] matrix, int length)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            int numRows = rows / length;
            //int numCols = cols / length;

            var tempMatrix = new double[length, length];
            //var result = new double[rows, cols];
            for (int c = 0; c < cols; c++) 
            {
                for (int r = 0; r < numRows; r++)
                {
                    double temp = 0;
                    for (int i = length * r; i < length * (r + 1); i++)
                    {
                        //for (int j = length * r; j < length * (c + 1); j++)
                        //{
                            temp = matrix[i, c] + temp;
                        //}
                    }

                    //double average = temp / (length * length);

                    double average = temp / length;

                    for (int i = length * r; i < length * (r + 1); i++)
                    {
                        //for (int j = length * r; j < length * (c + 1); j++)
                        //{

                        matrix[i, c] = average;
                        //}
                    }

                }            
            }

            return matrix;
        }


        // save array to csv file
        public static void SaveArrayAsCSV<T>(T[] arrayToSave, string fileName)
        {
            using (StreamWriter file = new StreamWriter(fileName))
            {
                foreach (T item in arrayToSave)
                {
                    file.Write(item + ",");
                }
            }
        }

        // DCT
        public static double[] DCT(double[] data)
        {
            double[] result = new double[data.Length];
            double c = Math.PI / (2.0 * data.Length);
            double scale = Math.Sqrt(2.0 / data.Length);

            for (int k = 0; k < data.Length; k++)
            {
                double sum = 0;
                for (int n = 0; n < data.Length; n++)
                    sum += data[n] * Math.Cos((2.0 * n + 1.0) * k * c);
                result[k] = scale * sum;
            }

            data[0] = result[0] / Constants.Sqrt2;
            for (int i = 1; i < data.Length; i++)
                data[i] = result[i];

            return data;
        }

        //IDCT
        public static double[] IDCT(double[] data)
        {
            double[] result = new double[data.Length];
            double c = Math.PI / (2.0 * data.Length);
            double scale = Math.Sqrt(2.0 / data.Length);

            for (int k = 0; k < data.Length; k++)
            {
                double sum = data[0] / Constants.Sqrt2;
                for (int n = 1; n < data.Length; n++)
                    sum += data[n] * Math.Cos((2 * k + 1) * n * c);

                result[k] = scale * sum;
            }

            for (int i = 0; i < data.Length; i++)
                data[i] = result[i];

            return data;
        }

        // SD
        public static double StandarDeviation(double[] data)
        {
            double average = data.Average();
            double sumOfSquaresOfDifferences = data.Select(val => (val - average) * (val - average)).Sum();
            double sd = Math.Sqrt(sumOfSquaresOfDifferences / data.Length);

            return sd;      
        }



        public static double[] CrossCorrelation(double[] v1, double[] v2)
        {
            int n = v1.Length;
            double[] r;
            alglib.corrr1d(v1, n, v2, n, out r);

            int xcorrLength = 2 * n;
            double[] xCorr = new double[xcorrLength];
            for (int i = 0; i < n - 1; i++) xCorr[i] = r[i + n] / (i + 1);  
            for (int i = n - 1; i < xcorrLength - 1; i++) xCorr[i] = r[i - n + 1] / (xcorrLength - i - 1);

            return xCorr;
        }



    }
}
