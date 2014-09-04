﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLibrary;

using System.Drawing;


namespace AudioAnalysisTools.DSP
{
    public static class NoiseRemoval_Briggs
    {

        /// <summary>
        /// Obtains a background noise profile from the passed percentile of lowest energy frames and then divide cell energy by
        /// the profile.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="percentileThreshold"></param>
        /// <returns></returns>
        public static double[,] BriggsNoiseFilter(double[,] matrix, int percentileThreshold) 
        {
            double[] profile = NoiseRemoval_Briggs.GetNoiseProfile_LowestPercentile(matrix, percentileThreshold);
            profile = DataTools.filterMovingAverage(profile, 3);

            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] outM = new double[rowCount, colCount]; //to contain noise reduced matrix

            for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
            {
                for (int y = 0; y < rowCount; y++) //for all rows
                {
                    outM[y, col] = matrix[y, col] / profile[col];
                } //end for all rows
            } //end for all cols
            return outM;
        }

        /// <summary>
        /// Obtains a background noise profile from the passed percentile of lowest energy frames.
        /// Then take square root of the cell energy divided by the noise profile.
        /// Taking the square root has the effect of reducing image contrast.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="percentileThreshold"></param>
        /// <returns></returns>
        public static double[,] BriggsNoiseFilterUsingSqrRoot(double[,] matrix, int percentileThreshold)
        {
            double[] profile = NoiseRemoval_Briggs.GetNoiseProfile_LowestPercentile(matrix, percentileThreshold);
            profile = DataTools.filterMovingAverage(profile, 3);

            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] outM = new double[rowCount, colCount]; //to contain noise reduced matrix

            for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
            {
                for (int y = 0; y < rowCount; y++) //for all rows
                {
                    outM[y, col] = Math.Sqrt(matrix[y, col] / profile[col]);
                } //end for all rows
            } //end for all cols
            return outM;
        }

        /// <summary>
        /// Obtains a background noise profile from the passed percentile of lowest energy frames.
        /// Also calculates the local column variance.
        /// Then divide the cell energy by the row noise + local row variance.
        /// However the backgroundnoise profile has little effect.
        /// The dominant effect is due to local column variance.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="percentileThreshold"></param>
        /// <returns></returns>
        public static double[,] FilterGlobalLocal(double[,] matrix, int percentileThreshold)
        {
            double[] profile = NoiseRemoval_Briggs.GetNoiseProfile_LowestPercentile(matrix, percentileThreshold);
            profile = DataTools.filterMovingAverage(profile, 3);
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] outM = new double[rowCount, colCount]; //to contain noise reduced matrix

            for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
            {
                double[] column = MatrixTools.GetColumn(matrix, col);
                double[] localVariance = NormalDist.CalculateLocalVariance(column, 15);
                for (int y = 0; y < rowCount; y++) //for all rows
                {
                    outM[y, col] = matrix[y, col] / (1.0 + profile[col] + localVariance[y]);
                } //end for all rows
            } //end for all cols
            return outM;
        }

        public static double[,] FilterWithLocalColumnVariance(double[,] matrix, int neighbourhood)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            //to contain noise reduced matrix
            double[,] outM = new double[rowCount, colCount]; 
            //for all cols i.e. freq bins
            for (int col = 0; col < colCount; col++) 
            {
                double[] column = MatrixTools.GetColumn(matrix, col);
                double[] localVariance = NormalDist.CalculateLocalVariance(column, neighbourhood);
                // normalise with local column variance
                for (int y = 0; y < rowCount; y++) //for all rows
                {
                    outM[y, col] = matrix[y, col] / (0.5 + Math.Sqrt(localVariance[y]));
                } //end for all rows
            } //end for all cols
            return outM;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="lowPercentile">The percent of lowest energy frames to be included in estimation of noise profile.</param>
        /// <returns></returns>
        public static double[] GetNoiseProfile_LowestPercentile(double[,] matrix, int lowPercentile)
        {
            double[] energyLevels = MatrixTools.GetRowAverages(matrix);
            var sorted = DataTools.SortArrayInAscendingOrder(energyLevels);
            int[] order = sorted.Item1;
            //double[] values = sorted.Item2;
            //for (int i = 0; i < 20; i++) Console.WriteLine(values[i]);

            int colCount = matrix.GetLength(1);
            int cutoff = (int)(lowPercentile * matrix.GetLength(0) / 100);
            if (cutoff == 0) throw new Exception("Illegal zero value for cutoff in method NoiseRemoval_Briggs.GetNoiseProfile_LowestPercentile()");
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
                if (noiseProfile[c] < 0.00001)
                    noiseProfile[c] = 0.00001; //to avoid zero values - which is very unlikely if in dB.
            }
            //for (int i = 0; i < colCount; i++) Console.WriteLine(noiseProfile[i]);

            return noiseProfile;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="matrix"></param>
        ///// <returns></returns>
        //public static double[,] BriggsNoiseDivision(double[,] matrix, double[] noiseProfile)
        //{
        //    int rowCount = matrix.GetLength(0);
        //    int colCount = matrix.GetLength(1);
        //    double[,] outM = new double[rowCount, colCount]; //to contain noise reduced matrix

        //    for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
        //    {
        //        for (int y = 0; y < rowCount; y++) //for all rows
        //        {
        //            outM[y, col] = matrix[y, col] / noiseProfile[col];
        //        } //end for all rows
        //    } //end for all cols
        //    return outM;
        //} // end of TruncateModalNoise()

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="matrix"></param>
        ///// <returns></returns>
        //public static double[,] BriggsNoiseDivisionUsingSquareroot(double[,] matrix, double[] noiseProfile)
        //{
        //    int rowCount = matrix.GetLength(0);
        //    int colCount = matrix.GetLength(1);
        //    double[] profile = DataTools.SquareRootOfValues(noiseProfile);
        //    double[,] outM = new double[rowCount, colCount]; //to contain noise reduced matrix

        //    for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
        //    {
        //        for (int y = 0; y < rowCount; y++) //for all rows
        //        {
        //            outM[y, col] = Math.Sqrt(matrix[y, col]) / profile[col];
        //        } //end for all rows
        //    } //end for all cols
        //    return outM;
        //} // end of TruncateModalNoise()


        public static double[,] BriggsNoiseFilterAndGetMask(double[,] matrix, int percentileThreshold, double binaryThreshold)
        {
            double[,] m = NoiseRemoval_Briggs.BriggsNoiseFilter(matrix, percentileThreshold); 

            // smooth and truncate
            m = ImageTools.WienerFilter(m, 7); //Briggs uses 17
            m = MatrixTools.SubtractAndTruncate2Zero(m, 1.0);

            // make binary
            m = MatrixTools.ThresholdMatrix2RealBinary(m, binaryThreshold);

            //agaion smooth and truncate
            m = ImageTools.GaussianBlur_5cell(m);
            //m = ImageTools.GaussianBlur_5cell(m); //do a seoncd time
            //m = ImageTools.Blur(m, 10); // use a simple neighbourhood blurring function.
            double binaryThreshold2 = binaryThreshold * 0.8;
            m = MatrixTools.ThresholdMatrix2RealBinary(m, binaryThreshold2); 

            return m;
        }

        public static Image BriggsNoiseFilterAndGetSonograms(double[,] matrix, int percentileThreshold, double binaryThreshold,
                                                                  TimeSpan recordingDuration, TimeSpan X_AxisInterval, TimeSpan stepDuration, int Y_AxisInterval)
        {
            //double[,] m = NoiseRemoval_Briggs.BriggsNoiseFilter(matrix, percentileThreshold);
            double[,] m = NoiseRemoval_Briggs.BriggsNoiseFilterUsingSqrRoot(matrix, percentileThreshold);

            List<Image> images = new List<Image>();

            string title = "TITLE ONE";
            Image image1 = DrawSonogram(m, recordingDuration, X_AxisInterval, stepDuration, Y_AxisInterval, title);
            images.Add(image1);

            m = ImageTools.WienerFilter(m, 7); //Briggs uses 17
            m = MatrixTools.SubtractAndTruncate2Zero(m, 1.0);
            title = "TITLE TWO";
            Image image2 = DrawSonogram(m, recordingDuration, X_AxisInterval, stepDuration, Y_AxisInterval, title);
            images.Add(image2);

            //int[] histo = Histogram.Histo(m, 100);
            //DataTools.writeArray(histo);
            //DataTools.WriteMinMaxOfArray(MatrixTools.Matrix2Array(m));

            m = MatrixTools.ThresholdMatrix2RealBinary(m, binaryThreshold);   //works for low SNR recordings
            //title = "TITLE THREE";
            //Image image3 = DrawSonogram(m, recordingDuration, X_AxisInterval, stepDuration, Y_AxisInterval, title);
            //images.Add(image3);

            m = ImageTools.GaussianBlur_5cell(m);
            //m = ImageTools.GaussianBlur_5cell(m);
            //m = ImageTools.Blur(m, 5); // use a simple neighbourhood blurring function.

            double binaryThreshold2 = binaryThreshold * 0.8;
            m = MatrixTools.ThresholdMatrix2RealBinary(m, binaryThreshold2); 
            title = "TITLE FOUR";
            Image image4 = DrawSonogram(m, recordingDuration, X_AxisInterval, stepDuration, Y_AxisInterval, title);
            images.Add(image4);

            Image combinedImage = ImageTools.CombineImagesVertically(images);

            return combinedImage;
        }

        public static Image DrawSonogram(double[,] data, TimeSpan recordingDuration, TimeSpan X_interval, TimeSpan xAxisPixelDuration, int Y_interval, string title)
        {
            // the next two variables determine how the greyscale sonogram image is normalised.
            // The low  normalisation bound is min value of the average spectrogram derived from the lowest  percent of frames
            // The high normalisation bound is max value of the average spectrogram derived from the highest percent of frames
            int minPercentile = 5;  
            int maxPercentile = 10;

            Image image = BaseSonogram.GetSonogramImage(data, minPercentile, maxPercentile);

            Image titleBar = BaseSonogram.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            TimeSpan minuteOffset = TimeSpan.Zero;
            TimeSpan labelInterval = TimeSpan.FromSeconds(5);
            image = BaseSonogram.FrameSonogram(image, titleBar, minuteOffset, X_interval, xAxisPixelDuration, labelInterval, Y_interval);

            return image;
        }



    } // class NoiseRemoval_Briggs
}
