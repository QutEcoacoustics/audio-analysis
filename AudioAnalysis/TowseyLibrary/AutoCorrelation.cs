using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TowseyLibrary
{
    public static class AutoCorrelation
    {

        public static void Main()
        {

            string[] s = File.ReadAllLines("t.txt");
            double[] x = new double[s.Length];
            for (int j = 0; j < s.Length; j++)
            {
                x[j] = double.Parse(s[j]);
                if (j % 1000 == 0)
                    Console.WriteLine(j);
            }

            Console.WriteLine("Computing Autocorrelation...");
            var q = AutoCorrelation.GetAutoCorrelationOfSeries(x);
            File.Delete("result.txt");
            for (int i = 0; i < q.Length; i++)
            {
                Console.WriteLine(q[i]);
                File.AppendAllText("result.txt", q[i].ToString() + "\r\n");
            }
            Console.WriteLine("DONE");
        }

        public static double GetAverage(double[] data)
        {
            int len = data.Length;

            if (len == 0)
                throw new Exception("No data");

            double sum = 0;

            for (int i = 0; i < data.Length; i++)
                sum += data[i];

            return sum / len;
        }

        public static double GetVariance(double[] data)
        {
            int len = data.Length;

            // Get average
            double avg = GetAverage(data);

            double sum = 0;

            for (int i = 0; i < data.Length; i++)
                sum += System.Math.Pow((data[i] - avg), 2);

            return sum / len;
        }
        public static double GetStdev(double[] data)
        {
            return Math.Sqrt(GetVariance(data));
        }

        public static double GetCorrelation(double[] x, double[] y)
        {
            if (x.Length != y.Length)
                throw new Exception("Length of sources is different");
            double avgX = GetAverage(x);
            double stdevX = GetStdev(x);
            double avgY = GetAverage(y);
            double stdevY = GetStdev(y);
            double covXY = 0;
            double pearson = 0;
            int len = x.Length;
            for (int i = 0; i < len; i++)
                covXY += (x[i] - avgX) * (y[i] - avgY);
            covXY /= len;
            pearson = covXY / (stdevX * stdevY);
            return pearson;
        }

        public static double[] GetAutoCorrelationOfSeries(double[] x)
        {
            int half = (int)x.Length / 2;
            double[] autoCorrelation = new double[half];
            double[] a = new double[half];
            double[] b = new double[half];
            for (int i = 0; i < half; i++)
            {
                a[i] = x[i];
                b[i] = x[i + i];
                autoCorrelation[i] = GetCorrelation(a, b);
                if (i % 1000 == 0)
                    Console.WriteLine(i);
            }
            return autoCorrelation;
        }



        // ##########################################################################################################################


        /// <summary>
        /// my own effort at autocorrelation.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="minLag"></param>
        /// <param name="maxLag"></param>
        /// <returns></returns>
        public static double[] MyAutoCorrelation(double[] X, int minLag, int maxLag)
        {
            if (maxLag > X.Length) maxLag = X.Length;
            int lagCount = maxLag - minLag + 1;
            var A = new double[lagCount];
            for (int lag = minLag; lag <= maxLag; lag++)
            {
                double sum = 0.0;
                for (int i = 0; i < X.Length - lag; i++)
                {
                    sum += (X[i] * X[i + lag]);
                }
                A[lag - minLag] = sum / (X.Length - lag);
            }
            return A;
        }

        /// <summary>
        /// my own effort at autocorrelation.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="minLag"></param>
        /// <param name="maxLag"></param>
        /// <returns></returns>
        public static double[] MyAutoCorrelation(double[] X)
        {
            int length = X.Length;
            int outputLength = length * 2;
            int centralIndex = length - 1;
            var AC = new double[outputLength];
            for (int lag = 0; lag < length; lag++)
            {
                double rigtShiftSum = 0.0;
                int count = 0;
                for (int i = lag; i < length; i++)
                {
                    rigtShiftSum += (X[i] * X[i - lag]);
                    count++;
                }
                // get average
                AC[centralIndex + lag] = rigtShiftSum / (double)count;
                //Console.WriteLine(count);
            }
            for (int lag = 1; lag < length - 1; lag++) // -1 here because the output array is even length.
            {
                double leftShiftSum = 0.0;
                int count = 0;
                for (int i = 0; i < length - lag; i++) 
                {
                    leftShiftSum += (X[i] * X[i + lag]);
                    count++;
                }
                // get average
                AC[centralIndex - lag] = leftShiftSum / (double)count;
                //Console.WriteLine(count);
            }
            return AC;
        }

        /// <summary>
        /// A Java version of Kalman's below C++ code for the  autocorrelation function.
        /// </summary>
        /// <param name="size"></param>
        public static double[] autoCorrelationOldJavaVersion(double[] X)
        {

            int size = X.Length; 
            double[] R = new double[size];
            double sum;

            for (int i=0; i < size; i++) 
            {
                sum=0;
                for (int j=0;j<size-i;j++) 
                {
                    sum += X[j] * X[j+i];
                }
                R[i] = sum / (double)(size - i);
            }
            return R;
        }


    }
}
