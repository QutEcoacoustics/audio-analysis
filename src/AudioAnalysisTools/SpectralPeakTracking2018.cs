// <copyright file="SpectralPeakTracking2018.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared.ConfigFile;
    using StandardSpectrograms;
    using TowseyLibrary;

    /// <summary>
    /// This class contain the pure algorithm that finds spectral peak tracks from a db spectrogram and settings
    /// </summary>
    public static class SpectralPeakTracking2018
    {
        public static void SpectralPeakTracking(double[,] dbSpectrogram, SpectralPeakTrackingSettings settings)
        {
            if (dbSpectrogram == null)
            {
                throw new ArgumentNullException(nameof(dbSpectrogram));
            }

            int frameCount = dbSpectrogram.GetLength(0);
            //int freqBinCount = dbSpectrogram.GetLength(1);

            var peakBinsIndex = GetPeakBinsIndex(dbSpectrogram, settings.MinFreqBin, settings.MaxFreqBin);

            // for all frames in dB array
            for (int r = 0; r < frameCount; r++)
            {
                double[] spectrum = DataTools.GetRow(dbSpectrogram, r);

                //find the boundaries of middle frequency band
                int minMid = peakBinsIndex[r] - (settings.WidthMidFreqBand / 2);
                int maxMid = peakBinsIndex[r] + (settings.WidthMidFreqBand / 2);

                double midBandAvgEnergy = CalculateAverageEnergy(spectrum, minMid, maxMid);


                //double topBandAvgEnergy = CalculateAverageEnergy(spectrum, );


            }


        }


        /// <summary>
        /// outputs an array of peak bins indices per frame
        /// </summary>
        public static int[] GetPeakBinsIndex(double[,] matrix, int minFreqBin, int maxFreqBin)
        {
            // get a submatrix with min and max frequency bins defined in settings.
            double[,] targetMatrix = GetArbitraryFreqBandMatrix(matrix, minFreqBin, maxFreqBin);

            // find the peak bins in each spectral of teh target matrix
            int[] peakBins = SpectrogramTools.HistogramOfSpectralPeaks(targetMatrix).Item2;

            // map the index of peak bins in the target matrix to original input matrix
            for (int i = 0; i < peakBins.Length; i++)
            {
                peakBins[i] = peakBins[i] + minFreqBin - 1;
            }

            return peakBins;
        }

        /// <summary>
        /// outputs the average energy within a specified band
        /// </summary>
        public static double CalculateAverageEnergy(double[] spectrum, int minInd, int maxInd)
        {
            double sum = 0.0;

            for (int i = minInd; i <= maxInd; i++)
            {
                sum = sum + spectrum[i];
            }

            double avgEnergy = sum / ( maxInd - minInd + 1 );

            return avgEnergy;
        }

        /// <summary>
        /// outputs a matrix with arbitrary minimum and maximum frequency bins.
        /// this method exists in PatchSampling class.
        /// </summary>
        public static double[,] GetArbitraryFreqBandMatrix(double[,] matrix, int minFreqBin, int maxFreqBin)
        {
            double[,] outputMatrix = new double[matrix.GetLength(0), maxFreqBin - minFreqBin + 1];

            int minColumnIndex = minFreqBin - 1;
            int maxColumnIndex = maxFreqBin - 1;

            // copying a part of the original matrix with pre-defined boundaries to Y axis (freq bins) to a new matrix
            for (int col = minColumnIndex; col <= maxColumnIndex; col++)
            {
                for (int row = 0; row < matrix.GetLength(0); row++)
                {
                    outputMatrix[row, col - minColumnIndex] = matrix[row, col];
                }
            }

            return outputMatrix;
        }


    }

    public class SpectralPeakTrackingSettings
    {
        // min and max Hertz of band in which searching for peak energy
        public const int DefaultMinFreqBin = 1500;
        public const int DefaultMaxFreqBin = 3500;

        // width of the middle frequency search band in Hertz.
        public const int DefaultWidthMidFreqBand = 2000;

        // a bottom and top buffer band in Hertz
        public const int DefaultBottomBuffer = 1000;
        public const int DefaultTopBuffer = 4000;

        // a decibel threshold for detecting a peak
        public const double DefaultDbThreshold = 0.0;

        public int MinFreqBin { get; set; } = DefaultMinFreqBin;

        public int MaxFreqBin { get; set; } = DefaultMaxFreqBin;

        public int WidthMidFreqBand { get; set; } = DefaultWidthMidFreqBand;

        public int BottomBuffer { get; set; } = DefaultBottomBuffer;

        public int TopBuffer { get; set; } = DefaultTopBuffer;

        public double DbThreshold { get; set; } = DefaultDbThreshold;
    }
}
