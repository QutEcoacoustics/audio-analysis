using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;


namespace AnalysisPrograms
{
    public class CrossCorrelation
    {

        //these keys are used to define a cross-correlation event in a sonogram.
        public const string key_COUNT = "count";
        public const string key_START_FRAME = "startFrame";
        public const string key_END_FRAME = "endFrame";
        public const string key_FRAME_COUNT = "frameCount";
        public const string key_START_SECOND = "startSecond";
        public const string key_END_SECOND = "endSecond";
        public const string key_MIN_FREQBIN = "minFreqBin";
        public const string key_MAX_FREQBIN = "maxFreqBin";
        public const string key_MIN_FREQ = "minFreq";
        public const string key_MAX_FREQ = "maxFreq";
        public const string key_SCORE = "score";
        public const string key_PERIODICITY = "periodicity";
        //public const string key_COUNT = "count";


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
            //System.Console.WriteLine("{0}", alglib.ap.format(f, 3));
            //for (int i = 0; i < op.Length; i++) Console.WriteLine("{0}   {1:f2}", i, op[i]);

            //rearrange corr output and normalise
            int xcorrLength = 2 * n;
            double[] xCorr = new double[xcorrLength];
            //for (int i = 0; i < n - 1; i++) newOp[i] = r[i + n];   //rearrange corr output
            //for (int i = n - 1; i < opLength-1; i++) newOp[i] = r[i - n + 1];
            for (int i = 0; i < n - 1; i++) xCorr[i] = r[i + n] / (i + 1);  //rearrange and normalise
            for (int i = n - 1; i < xcorrLength - 1; i++) xCorr[i] = r[i - n + 1] / (xcorrLength - i - 1);


            //add extra value at end so have length = power of 2 for FFT
            //xCorr[xCorr.Length - 1] = xCorr[xCorr.Length - 2];
            //Console.WriteLine("xCorr length = " + xCorr.Length);
            //for (int i = 0; i < xCorr.Length; i++) Console.WriteLine("{0}   {1:f2}", i, xCorr[i]);
            //DataTools.writeBarGraph(xCorr);

            xCorr = DataTools.DiffFromMean(xCorr);
            FFT.WindowFunc wf = FFT.Hamming;
            var fft = new FFT(xCorr.Length, wf);

            var spectrum = fft.Invoke(xCorr);
            return spectrum;
        }//CrossCorrelation()



         public static void TestCrossCorrelation()
         {
            double[] signal2  = {1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 };
            double[] signal4  = {1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
            double[] signal6 = { 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0 };
            double[] signal7 = { 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1 };
            double[] signal8 = { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 };
            double[] signal10 = {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0 };
            double[] signal16 = {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int n = signal2.Length;
            double[] pattern2  = { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 };
            double[] pattern4  = { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
            double[] pattern6  = { 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0 };
            double[] pattern7  = { 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1 };
            double[] pattern8  = { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 };
            double[] pattern10 = { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0 };
            double[] pattern16 = { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            Console.WriteLine("Signal length = {0}", n);
            int smoothWindow = 3;
            double[] signal  = DataTools.filterMovingAverage(signal16,  smoothWindow);
            double[] pattern = DataTools.filterMovingAverage(pattern16, smoothWindow);

            var spectrum = CrossCorrelation.CrossCorr(signal, pattern);
            int zeroCount = 3;
            for (int s = 1; s < zeroCount; s++) spectrum[s] = 0.0;  //in real data these bins are dominant and hide other frequency content
            spectrum = DataTools.NormaliseArea(spectrum);
            int maxId = DataTools.GetMaxIndex(spectrum);
            double intensityValue = spectrum[maxId];

            if (maxId == 0) Console.WriteLine("max id = 0");
            else
            {
                double period = 2 * n / (double)maxId;
                Console.WriteLine("max id = {0};   period = {1:f2};    intensity = {2:f3}", maxId, period, intensityValue);
            }
         }//TestCrossCorrelation()


         public static System.Tuple<double[,], double[,], double[,], double[]> DetectBarsUsingXcorrelation(double[,] m, int rowStep, int rowWidth, int colStep, int colWidth,
                                                                                                 double intensityThreshold, int zeroBinCount)
         {
             bool doNoiseremoval = true;
             //intensityThreshold = 0.3;

             int rowCount = m.GetLength(0);
             int colCount = m.GetLength(1);
             int numberOfColSteps = colCount / colStep;
             int numberOfRowSteps = rowCount / rowStep;


             var intensityMatrix   = new double[numberOfRowSteps, numberOfColSteps];
             var periodicityMatrix = new double[numberOfRowSteps, numberOfColSteps];
             var hitsMatrix        = new double[rowCount, colCount];
             double[] array2return = null;

             for (int b = 0; b < numberOfColSteps; b++)
             {
                 int minCol = b * colStep;
                 int maxCol = minCol + colWidth - 1;

                 double[,] subMatrix = MatrixTools.Submatrix(m, 0, minCol, (rowCount - 1), maxCol);
                 double[] amplitudeArray = MatrixTools.GetRowAverages(subMatrix);

                 if (doNoiseremoval)
                 {
                     double Q, oneSD;
                     amplitudeArray = SNR.NoiseSubtractMode(amplitudeArray, out Q, out oneSD);
                 }
                 //double noiseThreshold = 0.005;
                 //for (int i = 1; i < amplitudeArray.Length - 1; i++)
                 //{
                 //    if ((amplitudeArray[i - 1] < noiseThreshold) && (amplitudeArray[i + 1] < noiseThreshold)) amplitudeArray[i] = 0.0;
                 //}
                 //DataTools.writeBarGraph(amplitudeArray);
                 if (b == 2) array2return = amplitudeArray; //returned for debugging purposes only

                 //ii: DETECT HARMONICS
                 var results = CrossCorrelation.DetectPeriodicityInLongArray(amplitudeArray, rowStep, rowWidth, zeroBinCount);
                 double[] intensity = results.Item1;     //an array of periodicity scores
                 double[] periodicity = results.Item2;

                 //transfer periodicity info to a matrices.
                 for (int rs = 0; rs < numberOfRowSteps; rs++)
                 {
                     intensityMatrix[rs, b]   = intensity[rs];
                     periodicityMatrix[rs, b] = periodicity[rs];

                     //mark up the hits matrix
                     //double relativePeriod = periodicity[rs] / rowWidth / 2;
                     if (intensity[rs] > intensityThreshold)
                     {
                         int minRow = rs * rowStep;
                         int maxRow = minRow + rowStep - 1;
                         for (int r = minRow; r < maxRow; r++)
                         for (int c = minCol; c < maxCol; c++)
                         {
                             //hitsMatrix[r, c] = relativePeriod;
                             hitsMatrix[r, c] = periodicity[rs];
                         }
                     } // if()
                 } // for loop over numberOfRowSteps
             } // for loop over numberOfColSteps

             return Tuple.Create(intensityMatrix, periodicityMatrix, hitsMatrix, array2return);
         }



         //public static System.Tuple<double[,]> DetectStripesUsingXcorrelation(double[,] m)
         //{
         //    int rowCount = m.GetLength(0);
         //    int colCount = m.GetLength(1);
         //    var hits = new double[rowCount, colCount];
         //    return Tuple.Create(hits);
         //}


         public static Tuple<double[], double[]> DetectPeriodicityInLongArray(double[] array, int step, int segmentLength, int zeroBinCount)
        {
            int n = array.Length;
            int stepCount   = n / step;
            var intensity   = new double[stepCount];     //an array of period intensity
            var periodicity = new double[stepCount];     //an array of the periodicity values
            for (int i = 0; i < stepCount; i++)          //step through the array
            {
                int start = i * step;
                double[] subarray = DataTools.Subarray(array, start, segmentLength);
                var spectrum = CrossCorrelation.CrossCorr(subarray, subarray);

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




         /// <summary>
         /// this method requires debuggin.
         /// I started it but did not finish as another method seemed to work better.
         /// </summary>
         /// <param name="array"></param>
         /// <param name="intensityThreshold"></param>
         /// <returns></returns>
         public static List<Dictionary<string, double>> DetectBarsEventsBySegmentationAndXcorrelation(double[] array, double intensityThreshold)
         {
             int zeroBinCount = 5; //to remove low freq content which dominates the spectrum

             double[] smoothedArray = DataTools.filterMovingAverage(array, 5); //to close up gaps
             double noiseThreshold = 0.001;
             int minSegmentLength = 16;
             var events = new List<Dictionary<string, double>>();
             //DataTools.writeBarGraph(amplitudeArray);

             var segmentStartEnds = SNR.SegmentIntensityArray(smoothedArray, noiseThreshold, minSegmentLength);

             //loop over the segments only
             foreach (int[] segment in segmentStartEnds)
             {
                 int xcorrLength = 16;
                 int segmentLength = segment[1] - segment[0] + 1;
                 if (segmentLength > 64) xcorrLength = 64;
                 else if (segmentLength > 32) xcorrLength = 32;
                 else if (segmentLength > 16) xcorrLength = 16;

                 double[] extract = DataTools.Subarray(array, segment[0], xcorrLength);
                 if (extract == null) continue;

                 var results = CrossCorrelation.DetectPeriodicityInArray(extract, zeroBinCount);
                 double intensity = results.Item1;     //an array of periodicity scores
                 double periodicity = results.Item2;

                 if (intensity > intensityThreshold)
                 {
                     var singleEvent = new Dictionary<string, double>();
                     singleEvent[key_START_FRAME] = segment[0]; //start location
                     singleEvent[key_END_FRAME] = segment[1]; //end   location
                     singleEvent[key_FRAME_COUNT] = segment[1] - segment[0] + 1; //number of frames in the event
                     singleEvent[key_SCORE] = intensity;
                     singleEvent[key_PERIODICITY] = periodicity;
                     events.Add(singleEvent);
                 } // if()
             }//foreach segment in the array
             return events;
         }


        public static Tuple<double, double> DetectPeriodicityInArray(double[] array, int zeroBinCount)
        {
                var spectrum = CrossCorrelation.CrossCorr(array, array);

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


        /// <summary>
        /// This method assume the matrix is derived from a spectrogram rotated so that the matrix rows are spectral columns of sonogram.
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <param name="amplitudeThreshold"></param>
        /// <returns></returns>
        public static System.Tuple<double[], double[]> DetectBarsInTheRowsOfaMatrix(double[,] m, double threshold, int zeroBinCount)
        {
            int rowCount = m.GetLength(0);
            int colCount = m.GetLength(1);
            var intensity = new double[rowCount];     //an array of period intensity
            var periodicity = new double[rowCount];     //an array of the periodicity values

            double[] prevRow = MatrixTools.GetRow(m, 0);
            prevRow = DataTools.DiffFromMean(prevRow);

            for (int r = 1; r < rowCount; r++)
            {
                double[] thisRow = MatrixTools.GetRow(m, r);
                thisRow = DataTools.DiffFromMean(thisRow);

                var spectrum = CrossCorrelation.CrossCorr(prevRow, thisRow);

                for (int s = 0; s < zeroBinCount; s++) spectrum[s] = 0.0;  //in real data these bins are dominant and hide other frequency content
                spectrum = DataTools.NormaliseArea(spectrum);
                int maxId = DataTools.GetMaxIndex(spectrum);
                double intensityValue = spectrum[maxId];
                intensity[r] = intensityValue;

                double period = 0.0;
                if (maxId != 0) period = 2 * colCount / (double)maxId;
                periodicity[r] = period;

                prevRow = thisRow;
            }// rows
            return Tuple.Create(intensity, periodicity);
        }  //DetectBarsInTheRowsOfaMatrix()



         /// This method assume the matrix is derived from a spectrogram rotated so that the matrix rows are spectral columns of sonogram.
         /// Was first developed for crow calls.
         /// First looks for a decibel profile that matches the passed call duration and decibel loudness
         /// Then samples the centre portion for the correct harmonic period.
         /// </summary>
         /// <param name="m"></param>
         /// <param name="amplitudeThreshold"></param>
         /// <returns></returns>
         public static System.Tuple<double[], double[], double[]> DetectHarmonicsInSonogramMatrix(double[,] m, double dBThreshold, int callSpan)
         {
             int zeroBinCount = 3; //to remove low freq content which dominates the spectrum
             int halfspan = callSpan / 2;

             double[] dBArray = MatrixTools.GetRowAverages(m);
             dBArray = DataTools.filterMovingAverage(dBArray, 3);

             bool doNoiseRemoval = true;
             if (doNoiseRemoval)
             {
                 double Q, oneSD;
                 dBArray = SNR.NoiseSubtractMode(dBArray, out Q, out oneSD);
             }

             bool[] peaks = DataTools.GetPeaks(dBArray);

             int rowCount = m.GetLength(0);
             int colCount = m.GetLength(1);
             var intensity = new double[rowCount];     //an array of period intensity
             var periodicity = new double[rowCount];     //an array of the periodicity values


             for (int r = halfspan; r < rowCount - halfspan; r++)
             {
                 //APPLY A FILTER: must satisfy the following conditions for a call.
                 if (!peaks[r]) continue;
                 if (dBArray[r] < dBThreshold) continue;
                 double lowerDiff = dBArray[r] - dBArray[r - halfspan];
                 double upperDiff = dBArray[r] - dBArray[r + halfspan];
                 if ((lowerDiff < dBThreshold) || (upperDiff < dBThreshold)) continue;

                 double[] prevRow = DataTools.DiffFromMean(MatrixTools.GetRow(m, r - 1));
                 double[] thisRow = DataTools.DiffFromMean(MatrixTools.GetRow(m, r));
                 var spectrum = CrossCorrelation.CrossCorr(prevRow, thisRow);

                 for (int s = 0; s < zeroBinCount; s++) spectrum[s] = 0.0;  //in real data these bins are dominant and hide other frequency content
                 spectrum = DataTools.NormaliseArea(spectrum);
                 int maxId = DataTools.GetMaxIndex(spectrum);
                 double intensityValue = spectrum[maxId];
                 intensity[r] = intensityValue;

                 double period = 0.0;
                 if (maxId != 0) period = 2 * colCount / (double)maxId;
                 periodicity[r] = period;

                 prevRow = thisRow;
             }// rows
             return Tuple.Create(dBArray, intensity, periodicity);
         } //DetectHarmonicsInSonogramMatrix()



         public static System.Tuple<double[], double[]> DetectXcorrelationInTwoArrays(double[] array1, double[] array2, int step, int sampleLength, double minPeriod, double maxPeriod)
         {
            int length = array1.Length;
            int stepCount = length / step;
            double[] intensity   = new double[length];
            double[] periodicity = new double[length]; 

            for (int i = 0; i < stepCount; i++)
            {
                int start = step * i;
                double[] lowerSubarray = DataTools.Subarray(array1, start, sampleLength);
                double[] upperSubarray = DataTools.Subarray(array2, start, sampleLength);
                if ((lowerSubarray == null) || (upperSubarray == null)) break;
                var spectrum = CrossCorrelation.CrossCorr(lowerSubarray, upperSubarray);
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
                        intensity[start + j]   = spectrum[maxId];
                        periodicity[start + j] = period;
                    }
                }
            }
            return Tuple.Create(intensity, periodicity);
        }  //DetectXcorrelationInTwoArrays()

    } //class
} //AnalysisPrograms
