// <copyright file="SpectralPeakTracking2018.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using AudioAnalysisTools.StandardSpectrograms;
    using SixLabors.ImageSharp;
    using TowseyLibrary;

    /// <summary>
    /// This class contain the pure algorithm that finds spectral peak tracks from a db spectrogram and settings.
    /// </summary>
    public static class SpectralPeakTracking2018
    {
        public class Output
        {
            public int[][] TargetPeakBinsIndex { get; set; }

            public int[][] BandIndex { get; set; }

            public List<object[]> peakTrackInfoList { get; set; }

            //public List<SpectralTrack> SpecTracks { get; set; }
        }

        public static Output SpectralPeakTracking(double[,] spectrogram, SpectralPeakTrackingSettings settings, double hertzPerFreqBin, double timePerFrame)
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
            Tuple<int[][], int[][]> localPeaksAndBands = FindLocalSpectralPeaks(spectrogram, peakBinsIndex, syllableBinWidth, topSideBinWidth, bottomSideBinWidth, settings.DbThreshold);

            // make an array of the index of local peaks. Zero means there is no local peak identified.
            int[] SpectralPeakArray = MakeSpectralPeakArray(spectrogram, localPeaksAndBands.Item1);

            // find a track of peaks in a pre-defined set of boundaries
            var peakTrackInfo = SpectralPeakTracking(spectrogram, SpectralPeakArray, timePerFrame, hertzPerFreqBin);

            /*
            // Do Spectral Peak Tracking
            // debug this
            double frameDuration = (1024 * (1 - 0.2)) / 22050;
            double framesPerSecond = 1 / frameDuration;
            int herzOffset = 0;
            TimeSpan minDuration = TimeSpan.FromMilliseconds(40);
            TimeSpan maxIntraSyllableGap = TimeSpan.FromMilliseconds(350);
            int maxFreq = 6000; //???

            var spectralTracks = SpectralTrack.GetSpectralTracks(SpectralPeakArray, framesPerSecond, hertzPerFreqBin, herzOffset, minDuration, maxIntraSyllableGap, maxFreq);
            */
            var output = new Output()
            {
                TargetPeakBinsIndex = localPeaksAndBands.Item1,
                BandIndex = localPeaksAndBands.Item2,

                //SpecTracks = spectralTracks,
                peakTrackInfoList = peakTrackInfo,
            };

            return output;
        }

        /// <summary>
        /// outputs an array of peak bins indices per frame.
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

        /// <summary>
        /// find spectral peaks per frame by subtracting the average energy of top and bottom band from the syllable band energy.
        /// then if it is higher than a dB threshold, the index of the peak bin will be returned.
        /// </summary>
        public static Tuple<int[][], int[][]> FindLocalSpectralPeaks(double[,] matrix, int[] peakBinsIndex, int widthMidBand,
            int topBufferSize, int bottomBufferSize, double threshold)
        {
            int frameCount = matrix.GetLength(0);

            // save the target peak bins index [frameCount, freqBinCount]
            List<int[]> targetPeakBinsIndex = new List<int[]>();

            // save the bands' boundaries in each frame
            List<int[]> bandIndex = new List<int[]>();

            // for all frames of the input spectrogram
            for (int r = 0; r < frameCount; r++)
            {
                // retrieve each frame
                double[] spectrum = DataTools.GetRow(matrix, r);

                // smoothing to remove noise
                //spectrum = DataTools.filterMovingAverage(spectrum, 3);

                //find the boundaries of middle frequency band: the min bin index and the max bin index
                int minMid = peakBinsIndex[r] - (widthMidBand / 2);
                int maxMid = peakBinsIndex[r] + (widthMidBand / 2);

                // find the average energy
                double midBandAvgEnergy = CalculateAverageEnergy(spectrum, minMid, maxMid);

                //find the boundaries of top frequency band: the min bin index and the max bin index
                int minTop = maxMid + 1; //maxMid + 2; //
                int maxTop = minTop + topBufferSize;

                // find the average energy
                double topBandAvgEnergy = CalculateAverageEnergy(spectrum, minTop, maxTop);

                //find the boundaries of top frequency band: the min bin index and the max bin index
                int maxBottom = minMid - 1; //minMid - 2; //
                int minBottom = maxBottom - bottomBufferSize;

                // find the average energy
                double bottomBandAvgEnergy = CalculateAverageEnergy(spectrum, minBottom, maxBottom);

                // peak energy in each spectrum
                //double peakEnergy = midBandAvgEnergy - ((topBandAvgEnergy + bottomBandAvgEnergy) / 2);
                double peakEnergyInDb = 10 * Math.Log10(midBandAvgEnergy / ((topBandAvgEnergy + bottomBandAvgEnergy) / 2));

                int[] ind = new int[2];
                int[] bandInd = new int[5];

                // convert avg energy to decibel values
                //var peakEnergyInDb = 10 * Math.Log10(peakEnergy);

                // record the peak if the peak energy is higher than a threshold
                if (peakEnergyInDb > threshold)
                {
                    ind[0] = r;
                    ind[1] = peakBinsIndex[r];
                    targetPeakBinsIndex.Add(ind);
                }

                // saving the index of top, mid, and bottom band boundaries
                bandInd[0] = r;
                bandInd[1] = minBottom;
                bandInd[2] = minMid;
                bandInd[3] = maxMid;
                bandInd[4] = maxTop;
                bandIndex.Add(bandInd);
            }

            return Tuple.Create(targetPeakBinsIndex.ToArray(), bandIndex.ToArray());
        }

        /// <summary>
        /// if there is any local peak in a frame, this method will middle the peak bin and will count the following peaks in a
        /// pre-defined boundary (startX, endX, startY, endY). If the number of peaks in that boundary is higher than a threshold,
        /// that will be considered as a call.
        /// </summary>
        public static List<object[]> SpectralPeakTracking(double[,] spectrogram, int[] SpectralPeakArray, double timePerFrame, double hertzPerFreqBin)
        {
            int startX;
            int endX;
            int startY;
            int endY;

            List<object[]> peakTrackInfoList = new List<object[]>();

            for (int i = 0; i < SpectralPeakArray.Length; i++)
            {
                int peakCount = 0;
                var score = 0;
                if (SpectralPeakArray[i] != 0)
                {
                    object[] peakTrackInfo = new object[6];
                    startX = i;
                    startY = SpectralPeakArray[i] - 2;
                    endX = startX + 17;
                    endY = startY + 4;

                    //double[,] targetMatrix = GetArbitraryMatrix(spectrogram, startY, endY, startX, endX);

                    if (endX < spectrogram.GetLength(0) && endY < spectrogram.GetLength(1))
                    {
                        for (int j = startX; j < endX; j++)
                        {
                            if (SpectralPeakArray[j] >= startY && SpectralPeakArray[j] <= endY)
                            {
                                peakCount++;
                            }
                        }

                        score = peakCount; //peakCount / 18;

                        if (score >= 5) //4 / 18
                        {
                            peakTrackInfo[0] = i; // frame number
                            peakTrackInfo[1] = i * timePerFrame;
                            peakTrackInfo[2] = SpectralPeakArray[i]; // bin number
                            peakTrackInfo[3] = SpectralPeakArray[i] * hertzPerFreqBin;
                            peakTrackInfo[4] = score;
                            peakTrackInfo[5] = "Yes";
                            peakTrackInfoList.Add(peakTrackInfo);
                        }

                        /*
                        peakTrackInfo[0] = i; // frame number
                        peakTrackInfo[1] = i * timePerFrame;
                        peakTrackInfo[2] = SpectralPeakArray[i]; // bin number
                        peakTrackInfo[3] = SpectralPeakArray[i] * hertzPerFreqBin;
                        peakTrackInfo[4] = score;

                        if (score >= 4) //4 / 18
                        {
                            peakTrackInfo[5] = "Yes";
                        }
                        else
                        {
                            peakTrackInfo[5] = "No";
                        }
                        peakTrackInfoList.Add(peakTrackInfo);
                        */
                    }
                }
            }

            return peakTrackInfoList;
        }

        /// <summary>
        /// outputs the average energy within a specified band.
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

        /// <summary>
        /// outputs a matrix with arbitrary boundaries to Y (freq) and X (time) axes.
        /// </summary>
        public static double[,] GetArbitraryMatrix(double[,] matrix, int startY, int endY, int startX, int endX)
        {
            double[,] outputMatrix = new double[endX - startX + 1, endY - startY + 1];

            int minColumnIndex = startY - 1;
            int maxColumnIndex = endY - 1;

            // copying a part of the original matrix with pre-defined boundaries to X and Y axes to a new matrix
            for (int row = startX; row <= endX; row++)
            {
                for (int col = startY; col <= endY; col++)
                {
                    outputMatrix[row - startX, col - startY] = matrix[row, col];
                }
            }

            return outputMatrix;
        }

        /// <summary>
        /// outputs a matrix with the same size of the input matrix.
        /// all values are zero, except the points of interest (i.e., local spectral peaks).
        /// these bins can be filled with amplitude values or 1.
        /// </summary>
        public static double[,] MakeHitMatrix(double[,] matrix, int[][] pointsOfInterest, int[][] bandIndex)
        {
            // initialize a matrix with the same size of the input matrix with zero values
            double[,] hits = new double[matrix.GetLength(0), matrix.GetLength(1)];

            for (int i = 0; i < pointsOfInterest.GetLength(0); i++)
            {
                int rowIndex = pointsOfInterest[i][0];
                int colIndex = pointsOfInterest[i][1];
                hits[rowIndex, colIndex] = 1.0; // matrix[rowIndex, colIndex]
            }

            /*
            // bands
            for (int i = 0; i < bandIndex.GetLength(0); i++)
            {
                int rowIndex = bandIndex[i][0];
                int colIndex1 = bandIndex[i][1];
                int colIndex2 = bandIndex[i][2];
                int colIndex3 = bandIndex[i][3];
                int colIndex4 = bandIndex[i][4];
                hits[rowIndex, colIndex1] = 1.0;
                hits[rowIndex, colIndex2] = 1.0;
                hits[rowIndex, colIndex3] = 1.0;
                hits[rowIndex, colIndex4] = 1.0;
            }
            */
            return hits;
        }

        public static int[] MakeSpectralPeakArray(double[,] matrix, int[][] targetPeakBinsIndex)
        {
            int[] peakArray = new int[matrix.GetLength(0)];
            for (int i = 0; i < targetPeakBinsIndex.GetLength(0); i++)
            {
                int ind = targetPeakBinsIndex[i][0];
                peakArray[ind] = targetPeakBinsIndex[i][1];
            }

            return peakArray;
        }

        /// <summary>
        /// draw the spectrogram with red marks indicating the local spectral peaks.
        /// </summary>
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

        /// <summary>
        /// draw the spectrogram with spectral tracks.
        /// </summary>
        public static Image DrawTracks(BaseSonogram sonogram, double[,] hits, List<SpectralTrack_TO_BE_REMOVED> tracks)
        {
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage());
            image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(ImageTrack.GetSegmentationTrack(sonogram));

            if (tracks != null)
            {
                image.AddTracks(tracks, sonogram.FramesPerSecond, sonogram.FBinWidth);
            }

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

        // width of the middle frequency search band in Hertz
        public const int DefaultSyllableBandWidth = 500;

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