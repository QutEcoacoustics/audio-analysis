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
        public double[] RhzSpectrum { get; private set; } // spectrum of horizontal ridges 
        public double[] RvtSpectrum { get; private set; } // spectrum of vertical ridges 
        public double[] RpsSpectrum { get; private set; } // spectrum of positive slope ridges 
        public double[] RngSpectrum { get; private set; } // spectrum of negative slope ridges 


        /// <summary>
        /// CONSTRUCTOR
        /// NOTE: Orientation of passed spectrogram is: row = spectral frames, columns = frequency bins
        /// </summary>
        /// <param name="dBSpectrogram"></param>
        /// <param name="framesPerSecond"></param>
        /// <param name="binWidth"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public SpectralPeakTracks(double[,] dBSpectrogram, double framesPerSecond, double dBThreshold)
        {
            var rowCount = dBSpectrogram.GetLength(0);
            var colCount = dBSpectrogram.GetLength(1);
            GetPeakTracksSpectrum(dBSpectrogram, dBThreshold);

            GetRidgeSpectra(dBSpectrogram, dBThreshold);

        }

        public void GetRidgeSpectra(double[,] dbSpectrogramData, double dBThreshold)
        {
            var rowCount = dbSpectrogramData.GetLength(0);
            var colCount = dbSpectrogramData.GetLength(1);


            double ridgeThreshold = 1.0;
            double[,] matrix = dbSpectrogramData;
            //double[,] matrix = ImageTools.WienerFilter(dbSpectrogramData, 3);
            // returns a byte matrix of ridge directions
            // 0 = no ridge detected or below magnitude threshold.
            // 1 = ridge direction = horizontal or slope = 0;
            // 2 = ridge is positive slope or pi/4
            // 3 = ridge is vertical or pi/2
            // 4 = ridge is negative slope or 3pi/4. 
            byte[,] hits = RidgeDetection.Sobel5X5RidgeDetectionExperiment(matrix, ridgeThreshold);

            double[] spectrum = new double[colCount];
            byte[] freqBin;

            // accumulate info for the horizontal ridges
            for (int col = 0; col < colCount; col++) // i.e. for each frequency bin
            {
                freqBin = MatrixTools.GetColumn(hits, col);
                int count = freqBin.Count(x => x==1);
                if (count < 3) continue; // i.e. not a track.
                spectrum[col] = count / (double)rowCount;
            }
            this.RhzSpectrum = spectrum;

            // accumulate info for the vertical ridges
            spectrum = new double[colCount];
            for (int col = 0; col < colCount; col++) // i.e. for each frequency bin
            {
                freqBin = MatrixTools.GetColumn(hits, col);
                int count = freqBin.Count(x => x==3);
                if (count < 3) continue; // i.e. not a track.
                spectrum[col] = count / (double)rowCount;
            }
            this.RvtSpectrum = spectrum;

            // accumulate info for the up slope ridges
            spectrum = new double[colCount];
            for (int col = 0; col < colCount; col++) // i.e. for each frequency bin
            {
                freqBin = MatrixTools.GetColumn(hits, col);
                int count = freqBin.Count(x => x==2);
                if (count < 2) continue; // i.e. not a track.
                spectrum[col] = count / (double)rowCount;
            }
            this.RpsSpectrum = spectrum;
            // accumulate info for the down slope ridges
            spectrum = new double[colCount];
            for (int col = 0; col < colCount; col++) // i.e. for each frequency bin
            {
                freqBin = MatrixTools.GetColumn(hits, col);
                int count = freqBin.Count(x => x==4);
                if (count < 2) continue; // i.e. not a track.
                spectrum[col] = count / (double)rowCount;
            }
            this.RngSpectrum = spectrum;

        }



        public void GetPeakTracksSpectrum(double[,] dBSpectrogram, double dBThreshold)
        {
            var rowCount = dBSpectrogram.GetLength(0);
            var colCount = dBSpectrogram.GetLength(1);

            this.Peaks = LocalSpectralPeaks(dBSpectrogram, dBThreshold);

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
            this.SptSpectrum = spectrum;
            this.TrackDensity = cummulativeFrameCount / (double)rowCount;

            //double avFramesPerTrack = 0.0;
            //if (totalTrackCount > 0) 
            //    avFramesPerTrack = cummulativeFrameCount / (double)totalTrackCount;
            //this.TotalTrackCount = totalTrackCount;
            //this.AvTrackDuration = TimeSpan.FromSeconds(avFramesPerTrack / framesPerSecond);
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
