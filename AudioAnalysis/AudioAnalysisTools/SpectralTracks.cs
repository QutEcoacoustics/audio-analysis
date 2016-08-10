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
        public SpectralPeakTracks(double[,] dBSpectrogram, TimeSpan frameStepTimeSpan)
        {
            double framesStepsPerSecond = 1 / frameStepTimeSpan.TotalSeconds;
            double peakThreshold = 6.0; //dB
            GetPeakTracksSpectrum(dBSpectrogram, peakThreshold);

            // this method was written just before leaving for Toulon to work with Herve Glotin.
            // It was change while in Toulon to the following line which does not require a threshold.
            // double ridgeThreshold = 4.0; // 4 dB
            // GetRidgeSpectraVersion1(dBSpectrogram, ridgeThreshold);
            this.GetRidgeSpectraVersion2(dBSpectrogram);
        }

        public void GetRidgeSpectraVersion1(double[,] dbSpectrogramData, double ridgeThreshold)
        {
            int rowCount = dbSpectrogramData.GetLength(0);
            int colCount = dbSpectrogramData.GetLength(1);
            int spanCount = rowCount - 4; // 4 because 5x5 grid means buffer of 2 on either side


            double[,] matrix = dbSpectrogramData;
            //double[,] matrix = ImageTools.WienerFilter(dbSpectrogramData, 3);
            // returns a byte matrix of ridge directions
            // 0 = no ridge detected or below magnitude threshold.
            // 1 = ridge direction = horizontal or slope = 0;
            // 2 = ridge is positive slope or pi/4
            // 3 = ridge is vertical or pi/2
            // 4 = ridge is negative slope or 3pi/4. 
            //byte[,] hits = RidgeDetection.Sobel5X5RidgeDetectionExperiment(matrix, ridgeThreshold);
            byte[,] hits = RidgeDetection.Sobel5X5RidgeDetectionVersion1(matrix, ridgeThreshold);

            //image for debugging
            //ImageTools.DrawMatrix(hits, @"C:\SensorNetworks\Output\BIRD50\temp\hitsSpectrogram.png");

            double[] spectrum = new double[colCount];
            byte[] freqBin;

            //Now aggregate hits to get ridge info
            //note that the Spectrograms were passed in flat-rotated orientation.
            //Therefore need to assign ridge number to re-oriented values. 
            // Accumulate info for the horizontal ridges
            for (int col = 0; col < colCount; col++) // i.e. for each frequency bin
            {
                freqBin = MatrixTools.GetColumn(hits, col);
                int count = freqBin.Count(x => x==3);
                if (count < 2) continue; // i.e. not a track.
                spectrum[col] = count / (double)spanCount;
            }
            this.RhzSpectrum = spectrum;

            // accumulate info for the vertical ridges
            spectrum = new double[colCount];
            for (int col = 0; col < colCount; col++) // i.e. for each frequency bin
            {
                freqBin = MatrixTools.GetColumn(hits, col);
                int count = freqBin.Count(x => x==1);
                if (count < 2) continue; // i.e. not a track.
                spectrum[col] = count / (double)spanCount;
            }
            this.RvtSpectrum = spectrum;

            // accumulate info for the up slope ridges
            spectrum = new double[colCount];
            for (int col = 0; col < colCount; col++) // i.e. for each frequency bin
            {
                freqBin = MatrixTools.GetColumn(hits, col);
                int count = freqBin.Count(x => x==4);
                //if (count < 2) continue; // i.e. not a track.
                spectrum[col] = count / (double)spanCount;
            }
            this.RpsSpectrum = spectrum;
            // accumulate info for the down slope ridges
            spectrum = new double[colCount];
            for (int col = 0; col < colCount; col++) // i.e. for each frequency bin
            {
                freqBin = MatrixTools.GetColumn(hits, col);
                int count = freqBin.Count(x => x==2);
                //if (count < 2) continue; // i.e. not a track.
                spectrum[col] = count / (double)spanCount;
            }
            this.RngSpectrum = spectrum;

        }


        public void GetRidgeSpectraVersion2(double[,] dbSpectrogramData)
        {
            int rowCount = dbSpectrogramData.GetLength(0);
            int colCount = dbSpectrogramData.GetLength(1);
            // calculate span = number of cells over which will take average of a feature.
            // -4 because 5x5 grid means buffer of 2 on either side
            int spanCount = rowCount - 4; 


            double[,] matrix = dbSpectrogramData;
            //ImageTools.DrawMatrix(matrix, @"C:\SensorNetworks\Output\BIRD50\temp\SpectrogramBeforeWeinerFilter.png");

            // DO NOT USE WIENER FILTERING because smooths the ridges and lose definition
            //matrix = ImageTools.WienerFilter(dbSpectrogramData, 3);
            //ImageTools.DrawMatrix(matrix, @"C:\SensorNetworks\Output\BIRD50\temp\hitsSpectrogramAfterWeinerFilter.png");

            // returns a byte matrix of ridge directions
            // 0 = ridge direction = horizontal or slope = 0;
            // 1 = ridge is positive slope or pi/4
            // 2 = ridge is vertical or pi/2
            // 3 = ridge is negative slope or 3pi/4. 
            List<double[,]> hits = RidgeDetection.Sobel5X5RidgeDetection_Version2(matrix);

            //image for debugging
            //ImageTools.DrawMatrix(hits[0], 0, 10.0, @"C:\SensorNetworks\Output\BIRD50\temp\hitsSpectrogram0.png");
            //ImageTools.DrawMatrix(hits[1], 0, 10.0, @"C:\SensorNetworks\Output\BIRD50\temp\hitsSpectrogram1.png");
            //ImageTools.DrawMatrix(hits[2], 0, 10.0, @"C:\SensorNetworks\Output\BIRD50\temp\hitsSpectrogram2.png");
            //ImageTools.DrawMatrix(hits[3], 0, 10.0, @"C:\SensorNetworks\Output\BIRD50\temp\hitsSpectrogram3.png");


            double[] spectrum = new double[colCount];
            double sum = 0;

            //Now aggregate hits to get ridge info
            //note that the Spectrograms were passed in flat-rotated orientation.
            //Therefore need to assign ridge number to re-oriented values. 

            // Accumulate info for the horizontal ridges
            var M = hits[2];
            for (int col = 0; col < colCount; col++) // i.e. for each frequency bin
            {
                sum = 0;
                // i.e. for each row or frame
                for (int row = 2; row < rowCount - 2; row++) { sum += M[row, col]; }
                spectrum[col] = sum / (double)spanCount;
            }
            this.RhzSpectrum = spectrum;

            // accumulate info for the vertical ridges
            M = hits[0];
            spectrum = new double[colCount];
            for (int col = 0; col < colCount; col++) // i.e. for each frequency bin
            {
                sum = 0;
                // i.e. for each row or frame
                for (int row = 2; row < rowCount - 2; row++) { sum += M[row, col]; }
                spectrum[col] = sum / (double)spanCount;
            }
            this.RvtSpectrum = spectrum;

            // accumulate info for the positive/up-slope ridges
            M = hits[3];
            spectrum = new double[colCount];
            for (int col = 0; col < colCount; col++) // i.e. for each frequency bin
            {
                sum = 0;
                // i.e. for each row or frame
                for (int row = 2; row < rowCount - 2; row++) 
                {
                    sum += M[row, col];
                }
                spectrum[col] = sum / (double)spanCount;
            }
            this.RpsSpectrum = spectrum;

            // accumulate info for the negative/down slope ridges
            M = hits[1];
            spectrum = new double[colCount];
            for (int col = 0; col < colCount; col++) // i.e. for each frequency bin
            {
                sum = 0;
                // i.e. for each row or frame
                for (int row = 2; row < rowCount - 2; row++) { sum += M[row, col]; }
                spectrum[col] = sum / (double)spanCount;
            }
            this.RngSpectrum = spectrum;

        }




        public void GetPeakTracksSpectrum(double[,] dBSpectrogram, double dBThreshold)
        {
            var rowCount = dBSpectrogram.GetLength(0);
            var colCount = dBSpectrogram.GetLength(1);
            int spanCount = rowCount - 4;

            this.Peaks = LocalSpectralPeaks(dBSpectrogram, dBThreshold);

            double[] spectrum = new double[colCount];
            int cummulativeFrameCount = 0;

            for (int col = 0; col < colCount; col++)
            {
                double sum = 0;
                int cover = 0;
                // i.e. for each row or frame
                for (int row = 2; row < rowCount - 2; row++)
                {
                    sum += this.Peaks[row, col];
                    if(this.Peaks[row, col] > 0.0) cover ++;
                }
                spectrum[col] = sum / (double)spanCount;
                cummulativeFrameCount += cover;


                //freqBin = MatrixTools.GetColumn(this.Peaks, col);
                ////var tracksInOneBin = new TracksInOneFrequencyBin(col, freqBin, framesPerSecond);
                ////spectrum[col] = tracksInOneBin.CompositeTrackScore();  // add data to spectrum
                //int cover = freqBin.Count(x => x > 0.0);
                //if (cover < 3) continue; // i.e. not a track.
                //spectrum[col] = cover / (double)rowCount;
                //cummulativeFrameCount += cover;                         // accumulate track frames over all frequency bins 
                ////this.TotalTrackCount += tracksInOneBin.TrackCount;    // accumulate counts over all frequency bins
            }
            this.SptSpectrum = spectrum;
            this.TrackDensity = cummulativeFrameCount / (double)spanCount;

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
            int columnBuffer = 2;
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = columnBuffer; col < (colCount - columnBuffer); col++)
                {
                    if (dBSpectrogram[row, col] <= dBThreshold) continue; // skip small values

                    if (   (dBSpectrogram[row, col] > dBSpectrogram[row, col + 1])
                        && (dBSpectrogram[row, col] > dBSpectrogram[row, col - 1])
                        && (dBSpectrogram[row, col] > dBSpectrogram[row, col + 2])
                        && (dBSpectrogram[row, col] > dBSpectrogram[row, col - 2])
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
                        //localpeaks[row, col] = dBSpectrogram[row, col] - ((dBSpectrogram[row, col+2] + dBSpectrogram[row, col-2]) * 0.5);
                    }
                }
            }
            return localpeaks;
        } // LocalPeaks()




    }
}
