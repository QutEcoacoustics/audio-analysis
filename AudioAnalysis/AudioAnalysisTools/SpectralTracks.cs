using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLibrary;

namespace AudioAnalysisTools
{



    /// <summary>
    /// this struct describes spectral peak tracks ie whistles and chirps.
    /// </summary>
    public struct SPTrackInfo
    {
        public double[,] peaks;
        public List<SpectralTrack> listOfSPTracks;
        public int trackCount;
        public TimeSpan avTrackDuration;
        public double[] spSpectrum;

        public SPTrackInfo(double[,] _peaks, List<SpectralTrack> _tracks, int _trackCount, TimeSpan _avTrackDuration, double[] _spSpectrum)
        {
            peaks = _peaks;
            listOfSPTracks = _tracks;
            trackCount = _trackCount;
            avTrackDuration = _avTrackDuration;
            spSpectrum = _spSpectrum;
        }

    } // SPTrackInfo()


    public static class SpectralPeakTracks
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


            double[,] peaks = LocalPeaks(spectrogram, threshold);

            double[] spectrum = new double[colCount];
            double[] freqBin;

            int totalTrackCount = 0;
            int cummulativeFrameCount = 0;
            for (int col = 0; col < colCount; col++)
            {
                freqBin = MatrixTools.GetColumn(peaks, col);
                var tracksInOneBin = new TracksInOneFrequencyBin(col, freqBin, framesPerSecond);
                
                // add data to spectrum
                spectrum[col] = tracksInOneBin.CompositeTrackScore(); 

                totalTrackCount += tracksInOneBin.TrackCount;     // accumulate counts over all frequency bins
                cummulativeFrameCount += tracksInOneBin.TotalFrameLength; // accumulate track frames over all frequency bins 
            }

            double avFramesPerTrack = 0.0;
            if (totalTrackCount > 0) avFramesPerTrack = cummulativeFrameCount / (double)totalTrackCount;
            TimeSpan avTrackDuration = TimeSpan.FromSeconds(avFramesPerTrack / framesPerSecond);

            List<SpectralTrack> tracks = null; //filler for the moment
            var info = new SPTrackInfo(peaks, tracks, totalTrackCount, avTrackDuration, spectrum);
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
            int columnBuffer = 1;
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = columnBuffer; col < (colCount - columnBuffer); col++)
                {
                    if (dBSpectrogram[row, col] <= dBThreshold) continue; // skip small values

                    if (   (dBSpectrogram[row, col] > dBSpectrogram[row, col + 1])
                        && (dBSpectrogram[row, col] > dBSpectrogram[row, col - 1])
                        //&& (dBSpectrogram[row, col] > dBSpectrogram[row, col + 2])
                        //&& (dBSpectrogram[row, col] > dBSpectrogram[row, col - 2])
                        // && ((dBSpectrogram[row, col] - dBSpectrogram[row, col + 3])
                        // && ((dBSpectrogram[row, col] - dBSpectrogram[row, col - 3])
                        )
                       // if (((dBSpectrogram[row, col] - dBSpectrogram[row, col + 1]) > 0.0)
                       // && ((dBSpectrogram[row, col] - dBSpectrogram[row, col - 1]) > 0.0)
                       //// && ((dBSpectrogram[row, col] - dBSpectrogram[row, col + 2]) > dBThreshold)
                       //// && ((dBSpectrogram[row, col] - dBSpectrogram[row, col - 2]) > dBThreshold)
                       //// && ((dBSpectrogram[row, col] - dBSpectrogram[row, col + 3]) > dBThreshold)
                       //// && ((dBSpectrogram[row, col] - dBSpectrogram[row, col - 3]) > dBThreshold)
                       // )
                    {
                        localpeaks[row, col] = dBSpectrogram[row, col];
                    }
                }
            }
            return localpeaks;
        } // LocalPeaks()




    }
}
