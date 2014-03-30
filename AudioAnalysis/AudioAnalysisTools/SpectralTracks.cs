using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;

namespace AudioAnalysisTools
{



    /// <summary>
    /// this struct describes spectral peak tracks ie whistles and chirps.
    /// </summary>
    public struct SPTrackInfo
    {
        public List<SpectralTrack> listOfSPTracks;
        public TimeSpan totalTrackDuration;
        public int percentDuration; // percent of recording length
        public double[] spSpectrum;

        public SPTrackInfo(List<SpectralTrack> _tracks, TimeSpan _totalTrackDuration, int _percentDuration, double[] _spSpectrum)
        {
            listOfSPTracks = _tracks;
            totalTrackDuration = _totalTrackDuration;
            percentDuration = _percentDuration;
            spSpectrum = _spSpectrum;
        }

    } // SPTrackInfo()


    public static class SpectralTracks
    {

        /// <summary>
        /// Called only from AcousticIndicesCalculate.Analysis()
        /// NOTE: Orientation of passed spectrogram is: row = frames, columns = frequency bins
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="framesPerSecond"></param>
        /// <param name="binWidth"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static SPTrackInfo GetSpectralPeakIndices(double[,] spectrogram, double framesPerSecond, double threshold)
        {
            //var minDuration = TimeSpan.FromMilliseconds(150);
            //var permittedGap = TimeSpan.FromMilliseconds(100);
            //int maxFreq = 10000;

            var rowCount = spectrogram.GetLength(0);
            var colCount = spectrogram.GetLength(1);

            //int framesPerHalfSecond = (int)(framesPerSecond / 2);
            //int numberOfHalfSeconds = (int)(rowCount / framesPerHalfSecond);
            int numberOfSeconds = (int)(rowCount / framesPerSecond);
            int numberOfSegments = numberOfSeconds;
            int framesPerSegment = (int)framesPerSecond;

            double[,] peaks = LocalPeaks(spectrogram, threshold);

            double[] spectrum = new double[colCount];
            double[] fBin;
            double peakThreshold = threshold * 2;
            //double bonusThreshold = threshold * 4;
            for (int col = 0; col < colCount; col++)
            {
                fBin = MatrixTools.GetColumn(peaks, col);

                int count = 0;
                int start = 0;
                for (int hs = 0; hs < numberOfSegments; hs++)
                {
                    double[] segment = DataTools.Subarray(fBin, start, framesPerSegment);
                    double sum = segment.Sum();
                    if (sum > threshold) count++;
                    if (sum > peakThreshold) count++; //effectively give extra weight to segments with greater amplitude
                    start += framesPerSegment;
                }
                spectrum[col] = count / (double)numberOfSegments;
                //spectrum[col] = fBin.Sum() / numberOfSegments;
                if (spectrum[col] > 1.0) spectrum[col] = 1.0; // normalise
            }

            List<SpectralTrack> tracks = null;
            TimeSpan totalTrackDuration = TimeSpan.Zero; 
            int percentDuration = 0;

            var info = new SPTrackInfo(tracks, totalTrackDuration, percentDuration, spectrum);
            return info;
        }

        /// <summary>
        /// DEPRACATED
        /// Called only from AcousticIndicesCalculate.Analysis()
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="framesPerSecond"></param>
        /// <param name="binWidth"></param>
        /// <param name="dBThreshold"></param>
        /// <returns></returns>
        //public static SPTrackInfo GetSpectralPeackTrackIndices(double[,] spectrogram, double framesPerSecond, double binWidth, int herzOffset, double threshold)
        //{
        //    var minDuration = TimeSpan.FromMilliseconds(150);
        //    var permittedGap = TimeSpan.FromMilliseconds(100);
        //    int maxFreq = 10000;

        //    var spTracks = SpectralTrack.GetSpectralPeakTracks(spectrogram, framesPerSecond, binWidth, herzOffset, threshold, minDuration, permittedGap, maxFreq);
        //    var duration = TimeSpan.Zero;
        //    int trackLength = 0;
        //    foreach (SpectralTrack track in spTracks)
        //    {
        //        duration += track.Duration();
        //        trackLength += track.Length;
        //    }
        //    int percentDuration = (int)Math.Round(100 * trackLength / (double)spectrogram.GetLength(0));
        //    return new SPTrackInfo(spTracks, duration, percentDuration);
        //}

        public static double[,] LocalPeaks(double[,] dBSpectrogram, double dBThreshold)
        {

            var rowCount = dBSpectrogram.GetLength(0);
            var colCount = dBSpectrogram.GetLength(1);

            double[,] localpeaks = new double[rowCount, colCount];

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 3; col < (colCount - 3); col++)
                {
                    if (  (dBSpectrogram[row, col] - dBSpectrogram[row, col + 1]) > 0.5
                        & (dBSpectrogram[row, col] - dBSpectrogram[row, col - 1]) > 0.5
                        & (dBSpectrogram[row, col] - dBSpectrogram[row, col + 2]) > dBThreshold
                        & (dBSpectrogram[row, col] - dBSpectrogram[row, col - 2]) > dBThreshold
                        & (dBSpectrogram[row, col] - dBSpectrogram[row, col + 3]) > dBThreshold
                        & (dBSpectrogram[row, col] - dBSpectrogram[row, col - 3]) > dBThreshold
                        )
                    {
                        localpeaks[row, col] = dBSpectrogram[row, col];
                    }
                }
            }
            return localpeaks;
        } // LocalPeaks()




    }
}
