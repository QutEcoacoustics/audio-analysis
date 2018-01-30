namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public static class AutoAndCrossCorrelation
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
            var q = GetAutoCorrelationOfSeries(x);
            File.Delete("result.txt");
            for (int i = 0; i < q.Length; i++)
            {
                Console.WriteLine(q[i]);
                File.AppendAllText("result.txt", q[i].ToString() + "\r\n");
            }
            Console.WriteLine("DONE");
        }



        public static void TestCrossCorrelation()
        {
            double[] signal2 = { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 };
            double[] signal4 = { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
            double[] signal6 = { 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0 };
            double[] signal7 = { 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1 };
            double[] signal8 = { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 };
            double[] signal10 = { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0 };
            double[] signal16 = { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int n = signal2.Length;
            double[] pattern2 = { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 };
            double[] pattern4 = { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
            double[] pattern6 = { 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0 };
            double[] pattern7 = { 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1 };
            double[] pattern8 = { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 };
            double[] pattern10 = { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0 };
            double[] pattern16 = { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            LoggedConsole.WriteLine("Signal length = {0}", n);
            int smoothWindow = 3;
            double[] signal = DataTools.filterMovingAverage(signal16, smoothWindow);
            double[] pattern = DataTools.filterMovingAverage(pattern16, smoothWindow);

            var spectrum = CrossCorr(signal, pattern);
            int zeroCount = 3;
            for (int s = 1; s < zeroCount; s++) spectrum[s] = 0.0;  //in real data these bins are dominant and hide other frequency content
            spectrum = DataTools.NormaliseArea(spectrum);
            int maxId = DataTools.GetMaxIndex(spectrum);
            double intensityValue = spectrum[maxId];

            if (maxId == 0) LoggedConsole.WriteLine("max id = 0");
            else
            {
                double period = 2 * n / (double)maxId;
                LoggedConsole.WriteLine("max id = {0};   period = {1:f2};    intensity = {2:f3}", maxId, period, intensityValue);
            }
        }//TestCrossCorrelation()




        /*************************************************************************
         * Need to install alglib dll to get this funcitonality
1-dimensional real cross-correlation.

For given Pattern/Signal returns corr(Pattern,Signal) (non-circular).

Correlation is calculated using reduction to  convolution.  Algorithm with
max(N,N)*log(max(N,N)) complexity is used (see  ConvC1D()  for  more  info
about performance).

IMPORTANT:
for  historical reasons subroutine accepts its parameters in  reversed
order: CorrR1D(Signal, Pattern) = Pattern x Signal (using  traditional
definition of cross-correlation, denoting cross-correlation as "x").

INPUT PARAMETERS
Signal  -   array[0..N-1] - real function to be transformed,
        signal containing pattern
N       -   problem size
Pattern -   array[0..M-1] - real function to be transformed,
        pattern to search withing signal
M       -   problem size

OUTPUT PARAMETERS
R       -   cross-correlation, array[0..N+M-2]:
        * positive lags are stored in R[0..N-1],
          R[i] = sum(pattern[j]*signal[i+j]
        * negative lags are stored in R[N..N+M-2],
          R[N+M-1-i] = sum(pattern[j]*signal[-i+j]

NOTE:
It is assumed that pattern domain is [0..M-1].  If Pattern is non-zero
on [-K..M-1],  you can still use this subroutine, just shift result by K.

-- ALGLIB --
Copyright 21.07.2009 by Bochkanov Sergey
 *
 *
 *
public static void corrr1d(
double[] signal,
int n,
double[] pattern,
int m,
out double[] r)

*************************************************************************/

        /// <summary>
        /// returns the fft spectrum of a cross-correlation function
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static double[] CrossCorr(double[] v1, double[] v2)
        {
            int n = v1.Length; //assume both vectors of same length
            double[] r;
            alglib.corrr1d(v1, n, v2, n, out r);
            //alglib.complex[] f;
            //alglib.fftr1d(newOp, out f);
            //System.LoggedConsole.WriteLine("{0}", alglib.ap.format(f, 3));
            //for (int i = 0; i < op.Length; i++) LoggedConsole.WriteLine("{0}   {1:f2}", i, op[i]);

            //rearrange corr output and NormaliseMatrixValues
            int xcorrLength = 2 * n;
            double[] xCorr = new double[xcorrLength];
            //for (int i = 0; i < n - 1; i++) newOp[i] = r[i + n];   //rearrange corr output
            //for (int i = n - 1; i < opLength-1; i++) newOp[i] = r[i - n + 1];
            for (int i = 0; i < n - 1; i++) xCorr[i] = r[i + n] / (i + 1);  //rearrange and NormaliseMatrixValues
            for (int i = n - 1; i < xcorrLength - 1; i++) xCorr[i] = r[i - n + 1] / (xcorrLength - i - 1);


            //add extra value at end so have length = power of 2 for FFT
            //xCorr[xCorr.Length - 1] = xCorr[xCorr.Length - 2];
            //LoggedConsole.WriteLine("xCorr length = " + xCorr.Length);
            //for (int i = 0; i < xCorr.Length; i++) LoggedConsole.WriteLine("{0}   {1:f2}", i, xCorr[i]);
            //DataTools.writeBarGraph(xCorr);

            xCorr = DataTools.DiffFromMean(xCorr);
            FFT.WindowFunc wf = FFT.Hamming;
            var fft = new FFT(xCorr.Length, wf);

            var spectrum = fft.Invoke(xCorr);
            return spectrum;
        }//CrossCorrelation()


        //=============================================================================

        /// <summary>
        /// Pearsons correlation coefficient.
        /// Equals the covariance normalised by the sd's.
        /// </summary>
        /// <param name="seriesX"></param>
        /// <param name="seriesY"></param>
        /// <returns></returns>
        public static double CorrelationCoefficient(double[] seriesX, double[] seriesY)
        {
            double meanX, sdX, meanY, sdY;
            NormalDist.AverageAndSD(seriesX, out meanX, out sdX);
            NormalDist.AverageAndSD(seriesX, out meanY, out sdY);

            double covar = 0.0;
            for (int i = 0; i < seriesX.Length; i++)
            {
                covar += ((seriesX[i] - meanX) * (seriesY[i] - meanY));
            }
            covar /= (sdX * sdY);
            covar /= (seriesX.Length - 1);
            return covar;
        }
        //=============================================================================



        public static Tuple<double, double> DetectPeriodicityInArray(double[] array, int zeroBinCount)
        {
            var spectrum = CrossCorr(array, array);

            spectrum = DataTools.NormaliseArea(spectrum);

            //decrease weight of low frequency bins
            double gradient = 10 / (double)zeroBinCount;
            for (int s = 0; s < zeroBinCount; s++)
            {
                double divisor = (double)(10 - (gradient * s));
                spectrum[s] /= divisor;  //in real data these bins are dominant and hide other frequency content
            }
            int maxId = DataTools.GetMaxIndex(spectrum);
            double intensityValue = spectrum[maxId];

            double period = 0.0;
            if (maxId != 0) period = 2 * array.Length / (double)maxId;
            return Tuple.Create(intensityValue, period);
        }



        public static Tuple<double[], double[]> DetectPeriodicityInLongArray(double[] array, int step, int segmentLength, int zeroBinCount)
        {
            int n = array.Length;
            int stepCount = n / step;
            var intensity = new double[stepCount];     //an array of period intensity
            var periodicity = new double[stepCount];     //an array of the periodicity values
            for (int i = 0; i < stepCount; i++)          //step through the array
            {
                int start = i * step;
                double[] subarray = DataTools.Subarray(array, start, segmentLength);
                var spectrum = CrossCorr(subarray, subarray);

                spectrum = DataTools.NormaliseArea(spectrum);
                double gradient = 10 / (double)zeroBinCount;
                for (int s = 0; s < zeroBinCount; s++)
                {
                    double divisor = (double)(10 - (gradient * s));
                    spectrum[s] /= divisor;  //in real data these bins are dominant and hide other frequency content
                }
                int maxId = DataTools.GetMaxIndex(spectrum);
                double intensityValue = spectrum[maxId];
                intensity[i] = intensityValue;

                double period = 0.0;
                if (maxId != 0) period = 2 * segmentLength / (double)maxId;
                periodicity[i] = period;
            }
            return Tuple.Create(intensity, periodicity);
        }



        public static Tuple<double[], double[]> DetectXcorrelationInTwoArrays(double[] array1, double[] array2, int step, int sampleLength, double minPeriod, double maxPeriod)
        {
            int length = array1.Length;
            int stepCount = length / step;
            double[] intensity = new double[length];
            double[] periodicity = new double[length];

            for (int i = 0; i < stepCount; i++)
            {
                int start = step * i;
                double[] lowerSubarray = DataTools.Subarray(array1, start, sampleLength);
                double[] upperSubarray = DataTools.Subarray(array2, start, sampleLength);
                if ((lowerSubarray.Length != sampleLength) || (upperSubarray.Length != sampleLength)) break;
                var spectrum = CrossCorr(lowerSubarray, upperSubarray);
                int zeroCount = 3;
                for (int s = 0; s < zeroCount; s++) spectrum[s] = 0.0;  //in real data these bins are dominant and hide other frequency content
                spectrum = DataTools.NormaliseArea(spectrum);
                int maxId = DataTools.GetMaxIndex(spectrum);
                double period = 2 * sampleLength / (double)maxId; //convert maxID to period in frames
                if ((period < minPeriod) || (period > maxPeriod)) continue;
                for (int j = 0; j < sampleLength; j++) //lay down score for sample length
                {
                    if (intensity[start + j] < spectrum[maxId])
                    {
                        intensity[start + j] = spectrum[maxId];
                        periodicity[start + j] = period;
                    }
                }
            }
            return Tuple.Create(intensity, periodicity);
        }  //DetectXcorrelationInTwoArrays()




        // ##########################################################################################################
        // THE BELOW FIVE METHODS WORK ATOGEHTER.
        // DO NOT KNOW HWERE I GOT THEM FROM!


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
                sum += Math.Pow((data[i] - avg), 2);

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
        /// my own effort at Crosscorrelation.
        /// Input array is assumed to be of even length.
        /// It returns an array twice length of input array.
        /// The first and last entries of the returned array will not be written to and contain zeros.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="minLag"></param>
        /// <param name="maxLag"></param>
        /// <returns></returns>
        public static double[] MyCrossCorrelation(double[] X1, double[] X2)
        {
            int length = X1.Length;
            int outputLength = length * 2;
            int centralIndex = length - 1;
            var AC = new double[outputLength];
            for (int lag = 0; lag < length; lag++)
            {
                double rigtShiftSum = 0.0;
                int count = 0;
                for (int i = lag; i < length; i++)
                {
                    rigtShiftSum += (X1[i] * X2[i - lag]);
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
                    leftShiftSum += (X1[i] * X2[i + lag]);
                    count++;
                }
                // get average
                AC[centralIndex - lag] = leftShiftSum / (double)count;
                //Console.WriteLine(count);
            }
            return AC;
        }

        /// <summary>
        /// A Java version of autocorrelation
        /// </summary>
        /// <param name="size"></param>
        public static double[] AutoCorrelationOldJavaVersion(double[] X)
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
