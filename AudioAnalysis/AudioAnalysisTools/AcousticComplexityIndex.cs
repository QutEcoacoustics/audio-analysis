// <copyright file="AcousticComplexityIndex.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class AcousticComplexityIndex
    {
        /// <summary>
        /// Returns an array of ACOUSTIC COMPLEXITY INDICES
        /// This implements the index of N. Pieretti, A. Farina, D. Morri
        /// in "A new methodology to infer the singing activity of an avian community: The Acoustic Complexity Index (ACI)"
        /// in Ecological Indicators 11 (2011) pp868–873
        /// </summary>
        /// <param name="spectrogram">this is an amplitude spectrum.</param>
        /// <returns></returns>
        public static double[] CalculateACI(double[,] spectrogram)
        {
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);
            double[] aciArray = new double[freqBinCount];      // array of acoustic complexity indices, one for each freq bin
            for (int j = 0; j < freqBinCount; j++)             // for all frequency bins
            {
                var deltaI = 0.0;          //  to accumlate sum of differences
                var sumI = 0.0;
                for (int r = 0; r < frameCount - 1; r++)
                {
                    sumI += spectrogram[r, j];
                    deltaI += Math.Abs(spectrogram[r, j] - spectrogram[r + 1, j]);
                }
                if (sumI > 0.0) aciArray[j] = deltaI / sumI;      //store normalised ACI value
            }
            //DataTools.writeBarGraph(aciArray);

            return aciArray;
        } // AcousticComplexityIndex()


        /// <summary>
        /// Returns an array of DIFFERENCE values used in top line of calculation of ACOUSTIC COMPLEXITY INDICES
        /// See the above method.
        /// NOTE: There is one less difference than the number of elements in the freq bin.
        ///       When ACI is subsequently calculated, the SUM of freq bin values will be over all values
        ///       but the sum of DIFFERENCES will be over one less value.
        ///       WHen ACI is calculated over a long interval i.e. one minute this is not a problem. When calculated over 0.2s, need to compensate.
        ///       To get an almost correct approx to the ACI value calculated over a long interval, we add the average DIF to the total DIFF.
        ///       So the number of difference values equals the number of freq bin values, when it cons to calculate ACI = DIFF / SUM
        ///       This problem arises because of the very short sement duration e.g. 0.2 s segment = 8-9 frames.
        /// </summary>
        /// <param name="spectrogram">this is an amplitude spectrum.</param>
        /// <returns></returns>
        public static double[] SumOfAmplitudeDifferences(double[,] spectrogram)
        {
            int frameCount = spectrogram.GetLength(0);
            int deltaCount = frameCount - 1;
            int freqBinCount = spectrogram.GetLength(1);
            double[] differenceArray = new double[freqBinCount];  // array of difference values, one for each freq bin
            for (int j = 0; j < freqBinCount; j++)                // for all frequency bins
            {
                double deltaI = 0.0; // to accumlate sum of differences
                for (int r = 0; r < frameCount - 1; r++)
                {
                    deltaI += Math.Abs(spectrogram[r, j] - spectrogram[r + 1, j]);
                }
                differenceArray[j] = deltaI + (deltaI / (double)deltaCount); //store sum of differences
            }
            return differenceArray;
        } // SumDifferences()

    }
}
