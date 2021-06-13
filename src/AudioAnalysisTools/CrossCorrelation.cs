// <copyright file="CrossCorrelation.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using TowseyLibrary;

    /// <summary>
    /// This class contains two methods that could eventually be deleted.
    /// The methods are only called by call recognizers that have not been maintained in a long time.
    /// </summary>
    public class CrossCorrelation
    {
        // THESE KEYS COMMENTED 2021 June 13 as they appear to be unused.
        //these keys are used to define a cross-correlation event in a sonogram.
        //public const string key_COUNT = "count";
        //public const string key_START_FRAME = "startFrame";
        //public const string key_END_FRAME = "endFrame";
        //public const string key_FRAME_COUNT = "frameCount";
        //public const string key_START_SECOND = "startSecond";
        //public const string key_END_SECOND = "endSecond";
        //public const string key_MIN_FREQBIN = "minFreqBin";
        //public const string key_MAX_FREQBIN = "maxFreqBin";
        //public const string key_MIN_FREQ = "minFreq";
        //public const string key_MAX_FREQ = "maxFreq";
        //public const string key_SCORE = "score";
        //public const string key_PERIODICITY = "periodicity";

        /// <summary>
        /// TODO THis method could eventually be deleted. It has been replaced by the other methods below.
        ///      Amongst other things, the term "periodicity" is used incorrectly in this method.
        ///      It actually refers to the "harmonic interval".
        /// This method assumes the matrix is derived from a spectrogram rotated so that the matrix rows are spectral timeframes of a spectrogram.
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
        /// TODO TODO this method could be deleted. It is called only by a method to detect crow calls.
        /// THis is long since superceded.
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
    }
}