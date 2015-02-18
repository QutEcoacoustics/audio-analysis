using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLibrary;

namespace AudioAnalysisTools
{



    /// <summary>
    /// Finds and stores info about spectral peak tracks ie whistles and chirps in the passed spectrogram.
    /// </summary>
    public class SpectralPeakTracks
    {
        public double[,] Peaks { get; private set; }
        //public int TotalTrackCount { get; private set; }
        //public TimeSpan AvTrackDuration { get; private set; }

        /// <summary>
        /// Average number of tracks per frame
        /// </summary>
        public double TrackDensity { get; private set; }
        /// <summary>
        /// the fractional peak cover; i.e. fraction of frames in freq bin that are a spectral peak.
        /// </summary>
        public double[] SptSpectrum { get; private set; } 

        /// <summary>
        /// CONSTRUCTOR
        /// NOTE: Orientation of passed spectrogram is: row = frames, columns = frequency bins
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="framesPerSecond"></param>
        /// <param name="binWidth"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public SpectralPeakTracks(double[,] spectrogram, double framesPerSecond, double threshold)
        {
            var rowCount = spectrogram.GetLength(0);
            var colCount = spectrogram.GetLength(1);

            this.Peaks = LocalSpectralPeaks(spectrogram, threshold);

            double[] spectrum = new double[colCount];
            double[] freqBin;

            int cummulativeFrameCount = 0;
            for (int col = 0; col < colCount; col++)
            {
                freqBin = MatrixTools.GetColumn(this.Peaks, col);
                //var tracksInOneBin = new TracksInOneFrequencyBin(col, freqBin, framesPerSecond);
                //spectrum[col] = tracksInOneBin.CompositeTrackScore();  // add data to spectrum
                int cover = freqBin.Count(x => x > 0.0);
                if (cover < 3) continue; // i.e. not a track.
                spectrum[col] = cover / (double)rowCount;
                cummulativeFrameCount += cover;                         // accumulate track frames over all frequency bins 
                //this.TotalTrackCount += tracksInOneBin.TrackCount;    // accumulate counts over all frequency bins
            }

            this.TrackDensity = cummulativeFrameCount / (double)rowCount;
            //double avFramesPerTrack = 0.0;
            //if (totalTrackCount > 0) 
            //    avFramesPerTrack = cummulativeFrameCount / (double)totalTrackCount;
            //this.TotalTrackCount = totalTrackCount;
            //this.AvTrackDuration = TimeSpan.FromSeconds(avFramesPerTrack / framesPerSecond);
            this.SptSpectrum = spectrum;
        }



        /// <summary>
        /// Finds local spectral peaks in a spectrogram, one frame at a time.
        /// IMPORTANT: Assume that the spectrogram matrix is oriented 90 degrees to visual orientation.
        /// i.e the rows = spectra; columns = freq bins.
        /// </summary>
        /// <param name="dBSpectrogram"></param>
        /// <param name="dBThreshold"></param>
        /// <returns></returns>
        public static double[,] LocalSpectralPeaks(double[,] dBSpectrogram, double dBThreshold)
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
