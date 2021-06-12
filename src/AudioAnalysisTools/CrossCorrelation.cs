// <copyright file="CrossCorrelation.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using Accord.Math;
    using AudioAnalysisTools.DSP;
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

        /// <summary>
        /// This method assume the matrix is derived from a spectrogram rotated so that the matrix rows are spectral columns of sonogram.
        ///
        /// </summary>
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

                for (int s = 0; s < zeroBinCount; s++)
                {
                    spectrum[s] = 0.0;  //in real data these bins are dominant and hide other frequency content
                }

                spectrum = DataTools.NormaliseArea(spectrum);
                int maxId = DataTools.GetMaxIndex(spectrum);
                double intensityValue = spectrum[maxId];
                intensity[r] = intensityValue;

                double period = 0.0;
                if (maxId != 0)
                {
                    period = 2 * colCount / (double)maxId;
                }

                periodicity[r] = period;

                prevRow = thisRow;
            }

            return Tuple.Create(intensity, periodicity);
        } //DetectBarsInTheRowsOfaMatrix()

        /// <summary>
        /// A METHOD TO DETECT HARMONICS IN THE ROWS of the passed portion of a sonogram.
        /// This method assume the matrix is derived from a spectrogram rotated so that the matrix rows are spectral columns of sonogram.
        /// Was first developed for crow calls.
        /// First looks for a decibel profile that matches the passed call duration and decibel loudness.
        /// Then samples the centre portion for the correct harmonic period.
        /// </summary>
        /// <param name="m">data matrix.</param>
        /// <param name="dBThreshold">Minimum sound level.</param>
        /// <param name="callSpan">Minimum length of call of interest.</param>
        /// <returns>a tuple.</returns>
        public static Tuple<double[], double[], double[]> DetectHarmonicsInSonogramMatrix(double[,] m, double dBThreshold, int callSpan)
        {
            int rowCount = m.GetLength(0);
            int colCount = m.GetLength(1);
            var intensity = new double[rowCount];     //an array of period intensity
            var periodicity = new double[rowCount];   //an array of the periodicity values
            double[] dBArray = MatrixTools.GetRowAverages(m);
            dBArray = DataTools.filterMovingAverage(dBArray, 3);

            // for all time frames
            for (int t = 0; t < rowCount; t++)
            {
                if (dBArray[t] < dBThreshold)
                {
                    continue;
                }

                var row = MatrixTools.GetRow(m, t);
                var spectrum = AutoAndCrossCorrelation.CrossCorr(row, row);
                int zeroBinCount = 3; //to remove low freq content which dominates the spectrum
                for (int s = 0; s < zeroBinCount; s++)
                {
                    spectrum[s] = 0.0;  //in real data these bins are dominant and hide other frequency content
                }

                spectrum = DataTools.NormaliseArea(spectrum);
                int maxId = DataTools.GetMaxIndex(spectrum);
                double intensityValue = spectrum[maxId];
                intensity[t] = intensityValue;

                double period = 0.0;
                if (maxId != 0)
                {
                    period = 2 * colCount / (double)maxId;
                }

                periodicity[t] = period;
            }

            return Tuple.Create(dBArray, intensity, periodicity);
        }

        /// <summary>
        /// A METHOD TO DETECT a set of stacked HARMONICS/FORMANTS in the sub-band of a spectrogram.
        /// Developed for GenericRecognizer of harmonics.
        /// NOTE 1: This method assumes the matrix is derived from a spectrogram rotated so that the matrix rows are spectral columns of the spectrogram.
        /// NOTE 2: As of March 2020, this method averages the values in five adjacent frames. This is to reduce noise.
        ///         But it requires that the frequency of any potential formants is not changing rapidly.
        ///         This may not be suitable for detecting human speech. However can reduce the frame step.
        /// NOTE 3: This method assumes that the minimum  number of formants in a stack = 3.
        ///         This means that the first 4 values in the returned array of DCT coefficients are set = 0 (see below).
        /// </summary>
        /// <param name="m">data matrix derived from the subband of a spectrogram.</param>
        /// <param name="xThreshold">Minimum acceptable value to be considered part of a harmonic.</param>
        /// <returns>three arrays: dBArray, intensity, maxIndexArray.</returns>
        public static Tuple<double[], double[], int[]> DetectHarmonicsInSpectrogramData(double[,] m, double xThreshold)
        {
            int rowCount = m.GetLength(0);
            int colCount = m.GetLength(1);
            var binCount = m.GetLength(1);

            //set up the cosine coefficients
            double[,] cosines = MFCCStuff.Cosines(binCount, binCount);

            // set up time-frame arrays to store decibels, formant intensity and max index.
            var dBArray = new double[rowCount];
            var intensity = new double[rowCount];
            var maxIndexArray = new int[rowCount];

            // for all time frames
            for (int t = 2; t < rowCount - 2; t++)
            {
                // Smooth the frame values by taking the average of five adjacent frames
                var frame1 = MatrixTools.GetRow(m, t - 2);
                var frame2 = MatrixTools.GetRow(m, t - 1);
                var frame3 = MatrixTools.GetRow(m, t);
                var frame4 = MatrixTools.GetRow(m, t + 1);
                var frame5 = MatrixTools.GetRow(m, t + 2);

                // set up a frame of average db values.
                var avFrame = new double[colCount];
                for (int i = 0; i < colCount; i++)
                {
                    //avFrame[i] = (frame2[i] + frame3[i] + frame4[i]) / 3;
                    avFrame[i] = (frame1[i] + frame2[i] + frame3[i] + frame4[i] + frame5[i]) / 5;
                }

                // ignore frame if its maximum decibel value is below the threshold.
                double maxValue = avFrame.Max();
                dBArray[t] = maxValue;
                if (maxValue < xThreshold)
                {
                    continue;
                }

                // do the autocross-correlation prior to doing the DCT.
                double[] xr = AutoAndCrossCorrelation.AutoCrossCorr(avFrame);

                // xr has twice length of frame and is symmetrical. Require only first half.
                double[] normXr = new double[colCount];
                for (int i = 0; i < colCount; i++)
                {
                    // Typically normalise the xcorr values for overlap count.
                    // i.e. normXr[i] = xr[i] / (colCount - i);
                    // But for harmonics, this introduces too much noise - need to give less weight to the less overlapped values.
                    // Therefore just normalise by dividing values by the first, so first value = 1.
                    normXr[i] = xr[i] / xr[0];
                    //normXr[i] = xr[i];
                }

                //normXr = DataTools.DiffFromMean(normXr);

                // fit the x-correlation array to a line to remove first order trend.
                // This will help in detecting the correct maximum DCT coefficient.
                var xValues = new double[normXr.Length];
                for (int j = 0; j < xValues.Length; j++)
                { xValues[j] = j; }

                // get the line of best fit and subtract to get deviation from the line.
                Tuple<double, double> values = MathNet.Numerics.Fit.Line(xValues, normXr);
                var intercept = values.Item1;
                var slope = values.Item2;
                for (int j = 0; j < xValues.Length; j++)
                {
                    var lineValue = (slope * j) + intercept;
                    normXr[j] -= lineValue;
                }

                // now do DCT across the detrended auto-cross-correlation
                // set the first four values in the returned DCT coefficients to 0.
                // We require a minimum of three formants, that is two gaps.
                int lowerDctBound = 4;
                var dctCoefficients = Oscillations2012.DoDct(normXr, cosines, lowerDctBound);
                int indexOfMaxValue = DataTools.GetMaxIndex(dctCoefficients);
                intensity[t] = dctCoefficients[indexOfMaxValue];
                maxIndexArray[t] = indexOfMaxValue;
            }

            return Tuple.Create(dBArray, intensity, maxIndexArray);
        }
    }
}