// <copyright file="SpectralPeakTracking2018.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using StandardSpectrograms;
    using TowseyLibrary;

    /// <summary>
    /// This class contain the pure algorithm that finds spectral peak tracks from a db spectrogram and settings
    /// </summary>
    public static class SpectralPeakTracking2018
    {
        public static int[][] SpectralPeakTracking(double[,] spectrogram, SpectralPeakTrackingSettings settings, double hertzPerFreqBin)
        {
            if (spectrogram == null)
            {
                throw new ArgumentNullException(nameof(spectrogram));
            }

            int MinSearchFreqBin = Convert.ToInt32(settings.MinSearchFreq / hertzPerFreqBin);
            int MaxSearchFreqBin = Convert.ToInt32(settings.MaxSearchFreq / hertzPerFreqBin);

            // find the peak bin index in each spectrum/frame of the input spectrogram
            int[] peakBinsIndex = GetPeakBinsIndex(spectrogram, MinSearchFreqBin, MaxSearchFreqBin);

            var syllableBinWidth = Convert.ToInt32(settings.SyllableBandWidth / hertzPerFreqBin);
            var topSideBinWidth = Convert.ToInt32(settings.TopSideBand / hertzPerFreqBin);
            var bottomSideBinWidth = Convert.ToInt32(settings.BottomSideBand / hertzPerFreqBin);

            // find the local peak per spectrum
            int[][] localPeaks = FindLocalSpectralPeaks(spectrogram, peakBinsIndex, syllableBinWidth, topSideBinWidth, bottomSideBinWidth, settings.DbThreshold);

            return localPeaks;

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

            // find the peak bins in each spectral of the target matrix
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

                // convert avg enerrgy to decibel values
                var peakEnergyInDb = 10 * Math.Log10(peakEnergy);

                // record the peak if teh peak energy is higher than a threshold
                if (peakEnergyInDb > threshold)
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

        public static double[,] MakeHitMatrix(double[,] matrix, int[][] pointsOfInterest)
        {
            double[,] hits = new double[matrix.GetLength(0), matrix.GetLength(1)];

            for (int i = 0; i < pointsOfInterest.GetLength(0); i++)
            {
                int rowIndex = pointsOfInterest[i][0];
                int colIndex = pointsOfInterest[i][1];
                hits[rowIndex, colIndex] = 1.0;
            }

            return hits;
        }

        public static Image DrawSonogram(BaseSonogram sonogram, double[,] hits)
        {
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage());
            image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(ImageTrack.GetSegmentationTrack(sonogram));

            if (hits != null)
            {
                image.OverlayRedMatrix(hits, 1.0);
            }

            return image.GetImage();
        }
    }

    public class SpectralPeakTrackingSettings
    {
        // min and max Hertz of band in which searching for peak energy
        public const int DefaultMinSearchFreq = 1500;
        public const int DefaultMaxSearchFreq = 3500;

        // width of the middle frequency search band in Hertz.
        public const int DefaultSyllableBandWidth = 1000;

        // a bottom and top buffer band in Hertz
        public const int DefaultBottomSideBand = 500;
        public const int DefaultTopSideBand = 500;

        // a decibel threshold for detecting a peak
        public const double DefaultDbThreshold = 12.0;

        public int MinSearchFreq { get; set; } = DefaultMinSearchFreq;

        public int MaxSearchFreq { get; set; } = DefaultMaxSearchFreq;

        public int SyllableBandWidth { get; set; } = DefaultSyllableBandWidth;

        public int BottomSideBand { get; set; } = DefaultBottomSideBand;

        public int TopSideBand { get; set; } = DefaultTopSideBand;

        public double DbThreshold { get; set; } = DefaultDbThreshold;
    }
}
