using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysisTools
{
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
                var deltaI = 0.0;          // set up an array to take all values in a freq bin i.e. column of matrix
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

    }
}
