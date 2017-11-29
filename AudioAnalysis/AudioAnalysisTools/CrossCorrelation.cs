﻿// <copyright file="CrossCorrelation.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using DSP;
    using TowseyLibrary;

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


         public static Tuple<double[,], double[,], double[,], double[]> DetectBarsUsingXcorrelation(double[,] m, int rowStep, int rowWidth, int colStep, int colWidth,
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
                     double StandardDeviationCount = 0.1; // number of noise SDs to calculate noise threshold - determines severity of noise reduction
                     SNR.BackgroundNoise bgn = SNR.SubtractBackgroundNoiseFromSignal(amplitudeArray, StandardDeviationCount);
                     amplitudeArray = bgn.NoiseReducedSignal;
                 }
                 //double noiseThreshold = 0.005;
                 //for (int i = 1; i < amplitudeArray.Length - 1; i++)
                 //{
                 //    if ((amplitudeArray[i - 1] < noiseThreshold) && (amplitudeArray[i + 1] < noiseThreshold)) amplitudeArray[i] = 0.0;
                 //}
                 //DataTools.writeBarGraph(amplitudeArray);
                 if (b == 2) array2return = amplitudeArray; //returned for debugging purposes only

                 //ii: DETECT HARMONICS
                 var results = AutoAndCrossCorrelation.DetectPeriodicityInLongArray(amplitudeArray, rowStep, rowWidth, zeroBinCount);
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
                        {
                            for (int c = minCol; c < maxCol; c++)
                         {
                             //hitsMatrix[r, c] = relativePeriod;
                             hitsMatrix[r, c] = periodicity[rs];
                         }
                        }
                    } // if()
                 } // for loop over numberOfRowSteps
             } // for loop over numberOfColSteps

             return Tuple.Create(intensityMatrix, periodicityMatrix, hitsMatrix, array2return);
         }




        /// <summary>
        /// This method assume the matrix is derived from a spectrogram rotated so that the matrix rows are spectral columns of sonogram.
        ///
        /// </summary>
        /// <param name="m"></param>
        /// <param name="amplitudeThreshold"></param>
        /// <returns></returns>
        public static Tuple<double[], double[]> DetectBarsInTheRowsOfaMatrix(double[,] m, double threshold, int zeroBinCount)
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

                var spectrum = AutoAndCrossCorrelation.CrossCorr(prevRow, thisRow);

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



         /// A METHOD TO DETECT HARMONICS IN THE ROWS of the passed portion of a sonogram.
         /// This method assume the matrix is derived from a spectrogram rotated so that the matrix rows are spectral columns of sonogram.
         /// Was first developed for crow calls.
         /// First looks for a decibel profile that matches the passed call duration and decibel loudness
         /// Then samples the centre portion for the correct harmonic period.
         /// </summary>
         /// <param name="m"></param>
         /// <param name="amplitudeThreshold"></param>
         /// <returns></returns>
         public static Tuple<double[], double[], double[]> DetectHarmonicsInSonogramMatrix(double[,] m, double dBThreshold, int callSpan)
         {
             int zeroBinCount = 3; //to remove low freq content which dominates the spectrum
             int halfspan = callSpan / 2;

             double[] dBArray = MatrixTools.GetRowAverages(m);
             dBArray = DataTools.filterMovingAverage(dBArray, 3);

             bool doNoiseRemoval = true;
             if (doNoiseRemoval)
             {
                 double StandardDeviationCount = 0.1; // number of noise SDs to calculate noise threshold - determines severity of noise reduction
                 SNR.BackgroundNoise bgn = SNR.SubtractBackgroundNoiseFromSignal(dBArray, StandardDeviationCount);
                 dBArray = bgn.NoiseReducedSignal;
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
                 var spectrum = AutoAndCrossCorrelation.CrossCorr(prevRow, thisRow);

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




    } //class
}
