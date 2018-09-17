// <copyright file="SpectralPeakTracking2018.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
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

            // find the peak bin index in each spectrum/frame of the input spectrogram
            int[] peakBinsIndex = GetPeakBinsIndex(dbSpectrogram, settings.MinFreqBin, settings.MaxFreqBin);

            // find the local peak per spectrum
            int[][] localPeaks = FindLocalSpectralPeaks(dbSpectrogram, peakBinsIndex, settings.WidthMidFreqBand, settings.TopBuffer, settings.BottomBuffer, settings.DbThreshold);

            // Do Spectral Peak Tracking
            // SpectralTrack.GetSpectralPeakTracks()
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

        public static int[][] FindLocalSpectralPeaks(double[,] matrix, int[] peakBinsIndex, int widthMidBand,
            int topBufferSize, int bottomBufferSize, double threshold)
        {
            int frameCount = matrix.GetLength(0);

            // save the target peak bins index [frameCount, freqBinCount]
            List<int[]> targetPeakBinsIndex = new List<int[]>();

            // for all frames of the input spectrogram
            for (int r = 0; r < frameCount; r++)
            {
                // retrieve each frame
                double[] spectrum = DataTools.GetRow(matrix, r);

                //find the boundaries of middle frequency band: the min bin index and the max bin index
                int minMid = peakBinsIndex[r] - (widthMidBand / 2);
                int maxMid = peakBinsIndex[r] + (widthMidBand / 2);

                // find the average energy
                double midBandAvgEnergy = CalculateAverageEnergy(spectrum, minMid, maxMid);

                //find the boundaries of top frequency band: the min bin index and the max bin index
                int minTop = maxMid + 1;
                int maxTop = minTop + topBufferSize;

                // find the average energy
                double topBandAvgEnergy = CalculateAverageEnergy(spectrum, minTop, maxTop);

                //find the boundaries of top frequency band: the min bin index and the max bin index
                int maxBottom = minMid - 1;
                int minBottom = maxBottom - bottomBufferSize;

                // find the average energy
                double bottomBandAvgEnergy = CalculateAverageEnergy(spectrum, minBottom, maxBottom);

                // peak energy in each spectrum
                double peakEnergy = midBandAvgEnergy - ((topBandAvgEnergy + bottomBandAvgEnergy) / 2);

                int[] ind = new int[2];

                // record the peak if teh peak energy is higher than a threshold
                if (peakEnergy > threshold)
                {
                    ind[0] = r;
                    ind[1] = peakBinsIndex[r];
                    targetPeakBinsIndex.Add(ind);
                }
            }

            return targetPeakBinsIndex.ToArray();
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

            double avgEnergy = sum / (maxInd - minInd + 1);

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
