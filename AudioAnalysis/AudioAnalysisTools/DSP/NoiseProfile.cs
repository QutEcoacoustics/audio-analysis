using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TowseyLibrary;

namespace AudioAnalysisTools.DSP
{


    /// <summary>
    /// contains info re noise profile of an entire spectrogram
    /// </summary>
    public class NoiseProfile
    {

        public double[] NoiseMode { get; set; }

        public double[] NoiseSd { get; set; }

        public double[] NoiseThresholds { get; set; }

        public double[] MinDb { get; set; }

        public double[] MaxDb { get; set; }




        // #############################################################################################################################
        // ################################# FOUR DIFFERENT METHODS TO CALCULATE THE BACKGROUND NOISE PROFILE
        //
        // (1) MODAL METHOD    (2) LOWEST PERCENTILE FRAMES METHOD    
        // (3) BIN-WISE LOWEST PERCENTILE CELLS METHOD           (4) FIRST N FRAMES          ######################

        /// <summary>
        /// (1) MODAL METHOD
        /// Assumes the passed matrix is a spectrogram. i.e. rows=frames, cols=freq bins.
        /// Returns the noise profile over freq bins. i.e. one noise value per freq bin.
        /// </summary>
        /// <param name="matrix">the spectrogram with origin top-left</param>
        /// <param name="SD_COUNT"></param>
        /// <returns></returns>
        public static NoiseProfile CalculateModalNoiseProfile(double[,] matrix, double SD_COUNT)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[] noiseMode = new double[colCount];
            double[] noiseSD = new double[colCount];
            double[] noiseThreshold = new double[colCount];
            double[] minsOfBins = new double[colCount];
            double[] maxsOfBins = new double[colCount];
            for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
            {
                double[] freqBin = MatrixTools.GetColumn(matrix, col);
                SNR.BackgroundNoise binNoise = SNR.CalculateModalBackgroundNoiseFromSignal(freqBin, SD_COUNT);
                noiseMode[col] = binNoise.NoiseMode;
                noiseSD[col] = binNoise.NoiseSd;
                noiseThreshold[col] = binNoise.NoiseThreshold;
                minsOfBins[col] = binNoise.MinDb;
                maxsOfBins[col] = binNoise.MaxDb;
            }

            NoiseProfile profile = new NoiseProfile()
            {
                NoiseMode = noiseMode,
                NoiseSd = noiseSD,
                NoiseThresholds = noiseThreshold,
                MinDb = minsOfBins,
                MaxDb = maxsOfBins
            };
            return profile;
        }

        /// <summary>
        /// (1) MODAL METHOD
        /// Calculates the modal background noise for each freqeuncy bin.
        /// Assumes the passed spectrogram is oriented as: rows=frames, cols=freq bins.
        /// Return the smoothed modal profile.
        /// By default set the number of SDs = 0.
        /// </summary>
        /// <param name="amplitudeSpectrogram"></param>
        /// <returns></returns>
        public static double[] CalculateBackgroundNoise(double[,] spectrogram)
        {
            double sdCount = 0.0;
            NoiseProfile profile = NoiseProfile.CalculateModalNoiseProfile(spectrogram, sdCount);
            double[] noiseValues = DataTools.filterMovingAverage(profile.NoiseThresholds, 7);
            return noiseValues;
        }



        /// <summary>
        /// (2) LOWEST PERCENTILE FRAMES METHOD
        /// Assumes the passed matrix is a spectrogram. i.e. rows=frames, cols=freq bins.
        /// Returns the noise profile over freq bins. i.e. one noise value per freq bin.
        /// First calculate the frame averages, sort in ascending order and accumulate the first N% of frames.
        /// WARNING: This method should NOT be used for short recordings i.e LT approx 10-15 seconds long.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="lowPercentile">The percent of lowest energy frames to be included in calculation of the noise profile.</param>
        /// <returns></returns>
        public static double[] GetNoiseProfile_fromLowestPercentileFrames(double[,] matrix, int lowPercentile)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            int cutoff = (int)(lowPercentile * rowCount / 100);
            if (cutoff == 0) throw new Exception("Illegal zero value for cutoff in method NoiseRemoval_Briggs.GetNoiseProfile_LowestPercentile()");

            double[] frameEnergyLevels = MatrixTools.GetRowAverages(matrix);
            var sorted = DataTools.SortArrayInAscendingOrder(frameEnergyLevels);
            int[] order = sorted.Item1;
            //double[] values = sorted.Item2;
            //for (int i = 0; i < 20; i++) Console.WriteLine(values[i]);

            double[] noiseProfile = new double[colCount];

            // sum the lowest percentile frames 
            for (int i = 0; i < cutoff; i++)
            {
                double[] row = DataTools.GetRow(matrix, order[i]);
                for (int c = 0; c < colCount; c++)
                {
                    noiseProfile[c] += row[c];
                }
                //Console.WriteLine(values[i]);
            }
            // get average of the lowest percentile frames 
            for (int c = 0; c < colCount; c++)
            {
                noiseProfile[c] /= cutoff;
            }
            return noiseProfile;
        }



        /// <summary>
        /// (3) BIN-WISE LOWEST PERCENTILE CELLS METHOD
        /// Assumes the passed matrix is a spectrogram. i.e. rows=frames, cols=freq bins.
        /// Returns the noise profile over freq bins. i.e. one noise value per freq bin.
        /// IMPORTANT: This is the preferred method to estiamte a noise profile for short recordings i.e LT approx 10-15 seconds long.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="lowPercentile">The percent of lowest energy frames to be included in calculation of the noise profile.</param>
        /// <returns></returns>
        public static double[] GetNoiseProfile_BinWiseFromLowestPercentileCells(double[,] matrix, int lowPercentile)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            int cutoff = (int)(lowPercentile * rowCount / 100);
            if (cutoff == 0) throw new Exception("Illegal zero value for cutoff in method NoiseRemoval_Briggs.GetNoiseProfile_LowestPercentile()");

            double[] noiseProfile = new double[colCount];

            // loop over all frequency bins
            for (int bin = 0; bin < colCount; bin++)
            {
                double[] freqBin = MatrixTools.GetColumn(matrix, bin);
                double[] orderedArray = (double[])freqBin.Clone();
                Array.Sort(orderedArray);
                double sum = 0.0;
                for (int i = 0; i < cutoff; i++)
                {
                    sum += orderedArray[i];
                }
                noiseProfile[bin] = sum / (double)cutoff;
            }
            return noiseProfile;
        }


        /// <summary>
        /// (4) FIRST N FRAMES
        /// IMPORTANT: this method assumes that the first N frames (N=frameCount) DO NOT contain signal.
        /// </summary>
        /// <param name="matrix">the spectrogram rotated with origin is top-left.</param>
        /// <param name="firstNFramesCount">the first N rows of the spectrogram</param>
        /// <returns></returns>
        public static double[] CalculateModalNoiseUsingStartFrames(double[,] matrix, int firstNFramesCount)
        {
            if (firstNFramesCount < 1) return null;
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[] modalNoise = new double[colCount];

            for (int row = 0; row < firstNFramesCount; row++) //for first N rows
            {
                for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
                {
                    modalNoise[col] += matrix[row, col];
                }
            } //end for all cols
            for (int col = 0; col < colCount; col++) modalNoise[col] /= firstNFramesCount;

            return modalNoise;
        }




    }
}
