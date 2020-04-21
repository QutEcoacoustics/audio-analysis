// <copyright file="AutoAndCrossCorrelation.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.IO;

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
                {
                    Console.WriteLine(j);
                }
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
            for (int s = 1; s < zeroCount; s++)
            {
                spectrum[s] = 0.0;  // in real data these bins are dominant and hide other frequency content
            }

            spectrum = DataTools.NormaliseArea(spectrum);
            int maxId = DataTools.GetMaxIndex(spectrum);
            double intensityValue = spectrum[maxId];

            if (maxId == 0)
            {
                LoggedConsole.WriteLine("max id = 0");
            }
            else
            {
                double period = 2 * n / (double)maxId;
                LoggedConsole.WriteLine("max id = {0};   period = {1:f2};    intensity = {2:f3}", maxId, period, intensityValue);
            }
        }

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

        public static double[] AutoCrossCorr(double[] v)
        {
            int n = v.Length;
            alglib.corrr1d(v, n, v, n, out double[] xr);
            return xr;
        }

        /// <summary>
        /// returns the fft spectrum of a cross-correlation function.
        /// </summary>
        public static double[] CrossCorr(double[] v1, double[] v2)
        {
            int n = v1.Length; // assume both vectors of same length
            alglib.corrr1d(v1, n, v2, n, out var r);

            // alglib.complex[] f;
            // alglib.fftr1d(newOp, out f);
            // System.LoggedConsole.WriteLine("{0}", alglib.ap.format(f, 3));
            // for (int i = 0; i < op.Length; i++) LoggedConsole.WriteLine("{0}   {1:f2}", i, op[i]);

            // rearrange corr output and NormaliseMatrixValues
            int xcorrLength = 2 * n;
            double[] xCorr = new double[xcorrLength];

            // for (int i = 0; i < n - 1; i++) newOp[i] = r[i + n];   //rearrange corr output
            // for (int i = n - 1; i < opLength-1; i++) newOp[i] = r[i - n + 1];
            for (int i = 0; i < n - 1; i++)
            {
                xCorr[i] = r[i + n] / (i + 1);  // rearrange and NormaliseMatrixValues
            }

            for (int i = n - 1; i < xcorrLength - 1; i++)
            {
                xCorr[i] = r[i - n + 1] / (xcorrLength - i - 1);
            }

            // add extra value at end so have length = power of 2 for FFT
            // xCorr[xCorr.Length - 1] = xCorr[xCorr.Length - 2];
            // LoggedConsole.WriteLine("xCorr length = " + xCorr.Length);
            // for (int i = 0; i < xCorr.Length; i++) LoggedConsole.WriteLine("{0}   {1:f2}", i, xCorr[i]);
            // DataTools.writeBarGraph(xCorr);

            xCorr = DataTools.DiffFromMean(xCorr);
            FFT.WindowFunc wf = FFT.Hamming;
            var fft = new FFT(xCorr.Length, wf);

            var spectrum = fft.Invoke(xCorr);
            return spectrum;
        }

        // =============================================================================

        /// <summary>
        /// Pearsons correlation coefficient.
        /// Equals the covariance normalised by the sd's.
        /// </summary>
        public static double CorrelationCoefficient(double[] seriesX, double[] seriesY)
        {
            NormalDist.AverageAndSD(seriesX, out var meanX, out var sdX);
            NormalDist.AverageAndSD(seriesX, out var meanY, out var sdY);

            double covar = 0.0;
            for (int i = 0; i < seriesX.Length; i++)
            {
                covar += (seriesX[i] - meanX) * (seriesY[i] - meanY);
            }

            covar /= sdX * sdY;
            covar /= seriesX.Length - 1;
            return covar;
        }

        // =============================================================================

        public static Tuple<double, double> DetectPeriodicityInArray(double[] array, int zeroBinCount)
        {
            var spectrum = CrossCorr(array, array);

            spectrum = DataTools.NormaliseArea(spectrum);

            // decrease weight of low frequency bins
            double gradient = 10 / (double)zeroBinCount;
            for (int s = 0; s < zeroBinCount; s++)
            {
                double divisor = 10 - (gradient * s);
                spectrum[s] /= divisor;  // in real data these bins are dominant and hide other frequency content
            }

            int maxId = DataTools.GetMaxIndex(spectrum);
            double intensityValue = spectrum[maxId];

            double period = 0.0;
            if (maxId != 0)
            {
                period = 2 * array.Length / (double)maxId;
            }

            return Tuple.Create(intensityValue, period);
        }

        public static Tuple<double[], double[]> DetectPeriodicityInLongArray(double[] array, int step, int segmentLength, int zeroBinCount)
        {
            int n = array.Length;
            int stepCount = n / step;
            var intensity = new double[stepCount];     // an array of period intensity
            var periodicity = new double[stepCount];     // an array of the periodicity values

            //  step through the array
            for (int i = 0; i < stepCount; i++)
            {
                int start = i * step;
                double[] subarray = DataTools.Subarray(array, start, segmentLength);
                var spectrum = CrossCorr(subarray, subarray);

                spectrum = DataTools.NormaliseArea(spectrum);
                double gradient = 10 / (double)zeroBinCount;
                for (int s = 0; s < zeroBinCount; s++)
                {
                    double divisor = 10 - (gradient * s);
                    spectrum[s] /= divisor;  // in real data these bins are dominant and hide other frequency content
                }

                int maxId = DataTools.GetMaxIndex(spectrum);
                double intensityValue = spectrum[maxId];
                intensity[i] = intensityValue;

                double period = 0.0;
                if (maxId != 0)
                {
                    period = 2 * segmentLength / (double)maxId;
                }

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
                if (lowerSubarray.Length != sampleLength || upperSubarray.Length != sampleLength)
                {
                    break;
                }

                var spectrum = CrossCorr(lowerSubarray, upperSubarray);
                int zeroCount = 3;
                for (int s = 0; s < zeroCount; s++)
                {
                    spectrum[s] = 0.0;  // in real data these bins are dominant and hide other frequency content
                }

                spectrum = DataTools.NormaliseArea(spectrum);
                int maxId = DataTools.GetMaxIndex(spectrum);
                double period = 2 * sampleLength / (double)maxId; // convert maxID to period in frames
                if (period < minPeriod || period > maxPeriod)
                {
                    continue;
                }

                //  lay down score for sample length
                for (int j = 0; j < sampleLength; j++)
                {
                    if (intensity[start + j] < spectrum[maxId])
                    {
                        intensity[start + j] = spectrum[maxId];
                        periodicity[start + j] = period;
                    }
                }
            }

            return Tuple.Create(intensity, periodicity);
        } // DetectXcorrelationInTwoArrays()

        // ##########################################################################################################
        // THE BELOW FIVE METHODS WORK ATOGEHTER.
        // DO NOT KNOW HWERE I GOT THEM FROM!

        public static double GetAverage(double[] data)
        {
            int len = data.Length;

            if (len == 0)
            {
                throw new Exception("No data");
            }

            double sum = 0;

            for (int i = 0; i < data.Length; i++)
            {
                sum += data[i];
            }

            return sum / len;
        }

        public static double GetVariance(double[] data)
        {
            int len = data.Length;

            // Get average
            double avg = GetAverage(data);

            double sum = 0;

            for (int i = 0; i < data.Length; i++)
            {
                sum += Math.Pow(data[i] - avg, 2);
            }

            return sum / len;
        }

        public static double GetStdev(double[] data)
        {
            return Math.Sqrt(GetVariance(data));
        }

        public static double GetCorrelation(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new Exception("Length of sources is different");
            }

            double avgX = GetAverage(x);
            double stdevX = GetStdev(x);
            double avgY = GetAverage(y);
            double stdevY = GetStdev(y);
            double covXY = 0;
            double pearson = 0;
            int len = x.Length;
            for (int i = 0; i < len; i++)
            {
                covXY += (x[i] - avgX) * (y[i] - avgY);
            }

            covXY /= len;
            pearson = covXY / (stdevX * stdevY);
            return pearson;
        }

        public static double[] GetAutoCorrelationOfSeries(double[] x)
        {
            int half = x.Length / 2;
            double[] autoCorrelation = new double[half];
            double[] a = new double[half];
            double[] b = new double[half];
            for (int i = 0; i < half; i++)
            {
                a[i] = x[i];
                b[i] = x[i + i];
                autoCorrelation[i] = GetCorrelation(a, b);
                if (i % 1000 == 0)
                {
                    Console.WriteLine(i);
                }
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
        public static double[] MyCrossCorrelation(double[] x1, double[] x2)
        {
            int length = x1.Length;
            int outputLength = length * 2;
            int centralIndex = length - 1;
            var AC = new double[outputLength];
            for (int lag = 0; lag < length; lag++)
            {
                double rigtShiftSum = 0.0;
                int count = 0;
                for (int i = lag; i < length; i++)
                {
                    rigtShiftSum += x1[i] * x2[i - lag];
                    count++;
                }

                // get average
                AC[centralIndex + lag] = rigtShiftSum / count;

                // Console.WriteLine(count);
            }

            //  -1 here because the output array is even length.
            for (int lag = 1; lag < length - 1; lag++)
            {
                double leftShiftSum = 0.0;
                int count = 0;
                for (int i = 0; i < length - lag; i++)
                {
                    leftShiftSum += x1[i] * x2[i + lag];
                    count++;
                }

                // get average
                AC[centralIndex - lag] = leftShiftSum / count;

                // Console.WriteLine(count);
            }

            return AC;
        }

        /// <summary>
        /// A Java version of autocorrelation.
        /// </summary>
        public static double[] AutoCorrelationOldJavaVersion(double[] X)
        {
            int size = X.Length;
            double[] R = new double[size];
            double sum;

            for (int i = 0; i < size; i++)
            {
                sum = 0;
                for (int j = 0; j < size - i; j++)
                {
                    sum += X[j] * X[j + i];
                }

                R[i] = sum / (size - i);
            }

            return R;
        }
    }
}