// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AcousticEntropy.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the AcousticEntropy type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools
{
    using System;

    using TowseyLibrary;

    public static class AcousticEntropy
    {
        public static double[] CalculateTemporalEntropySpectrum(double[,] spectrogram)
        {
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);
            double[] tenSp = new double[freqBinCount];      // array of H[t] indices, one for each freq bin

            // for all frequency bins
            for (int j = 0; j < freqBinCount; j++)         
            {
                double[] column = MatrixTools.GetColumn(spectrogram, j);

                // ENTROPY of freq bin
                tenSp[j] = DataTools.Entropy_normalised(DataTools.SquareValues(column));
            }

            return tenSp;
        }


        public static Tuple<double, double> CalculateSpectralEntropies(double[,] amplitudeSpectrogram, int lowerBinBound, int reducedFreqBinCount)
        {
            // iv: ENTROPY OF AVERAGE SPECTRUM - at this point the spectrogram is a noise reduced amplitude spectrogram
            // Entropy is a measure of ENERGY dispersal, therefore must square the amplitude.
            var tuple = SpectrogramTools.CalculateSpectralAvAndVariance(amplitudeSpectrogram);
            double[] reducedSpectrum = DataTools.Subarray(tuple.Item1, lowerBinBound, reducedFreqBinCount); // remove low band
            double entropyOfAvSpectrum = DataTools.Entropy_normalised(reducedSpectrum);           // ENTROPY of spectral averages
            if (double.IsNaN(entropyOfAvSpectrum)) entropyOfAvSpectrum = 1.0;

            // v: ENTROPY OF VARIANCE SPECTRUM - at this point the spectrogram is a noise reduced amplitude spectrogram
            reducedSpectrum = DataTools.Subarray(tuple.Item2, lowerBinBound, reducedFreqBinCount); // remove low band
            double entropyOfVarianceSpectrum = DataTools.Entropy_normalised(reducedSpectrum);     // ENTROPY of spectral variances
            if (double.IsNaN(entropyOfVarianceSpectrum)) entropyOfVarianceSpectrum = 1.0;
            // DataTools.writeBarGraph(indices.varianceSpectrum);
            // Log.WriteLine("H(Spectral Variance) =" + HSpectralVar);

            return Tuple.Create(entropyOfAvSpectrum, entropyOfVarianceSpectrum);
        } // CalculateSpectralEntropies()



        /// <summary>
        /// CALCULATES THE ENTROPY OF DISTRIBUTION of maximum SPECTRAL PEAKS.
        /// </summary>
        /// <param name="amplitudeSpectrogram"></param>
        /// <param name="lowerBinBound"></param>
        /// <param name="reducedFreqBinCount"></param>
        /// <returns></returns>
        public static double CalculateEntropyOfSpectralPeaks(double[,] amplitudeSpectrogram, int lowerBinBound, int nyquistBin)
        {
            //     First extract High band SPECTROGRAM which is now noise reduced
            var midBandSpectrogram = MatrixTools.Submatrix(amplitudeSpectrogram, 0, lowerBinBound, amplitudeSpectrogram.GetLength(0) - 1, nyquistBin - 1);
            var tuple_AmplitudePeaks = SpectrogramTools.HistogramOfSpectralPeaks(midBandSpectrogram);
            double entropyOfPeakFreqDistr = DataTools.Entropy_normalised(tuple_AmplitudePeaks.Item1);
            if (Double.IsNaN(entropyOfPeakFreqDistr)) entropyOfPeakFreqDistr = 1.0;
            return entropyOfPeakFreqDistr;
        } // CalculateEntropyOfSpectralPeaks()


    } // class AcousticEntropy
}
