// <copyright file="NoiseRemoval_Briggs.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using SixLabors.ImageSharp;
    using AudioAnalysisTools.StandardSpectrograms;
    using SixLabors.ImageSharp.PixelFormats;
    using TowseyLibrary;

    public static class NoiseRemoval_Briggs
    {
        /// <summary>
        /// Assumes the passed matrix is a spectrogram. i.e. rows=frames, cols=freq bins.
        /// WARNING: This method should NOT be used for short recordings (i.e LT approx 10-15 seconds long)
        /// Obtains a background noise profile from the passed percentile of lowest energy frames,
        /// Then divide cell energy by the profile value.
        /// This method was adapted from a paper by Briggs.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="percentileThreshold"></param>
        /// <returns></returns>
        public static double[,] NoiseReduction_byDivision(double[,] matrix, int percentileThreshold)
        {
            double[] profile = NoiseProfile.GetNoiseProfile_fromLowestPercentileFrames(matrix, percentileThreshold);
            profile = DataTools.filterMovingAverage(profile, 3);

            // to prevent division by zero.
            double epsilon = 0.0001;

            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] outM = new double[rowCount, colCount]; //to contain noise reduced matrix

            for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
            {
                double denominator = profile[col];
                if (denominator < epsilon)
                {
                    denominator = epsilon;
                }

                for (int y = 0; y < rowCount; y++) //for all rows
                {
                    outM[y, col] = matrix[y, col] / denominator;
                } //end for all rows
            } //end for all cols

            return outM;
        }

        /// <summary>
        /// Assumes the passed matrix is a spectrogram. i.e. rows=frames, cols=freq bins.
        /// WARNING: This method should NOT be used for short recordings (i.e LT approx 10-15 seconds long)
        /// because it obtains a background noise profile from the passed percentile of lowest energy frames.
        ///
        /// Same method as above except take square root of the cell energy divided by the noise.
        /// Taking the square root has the effect of reducing image contrast.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="percentileThreshold"></param>
        /// <returns></returns>
        public static double[,] NoiseReduction_byDivisionAndSqrRoot(double[,] matrix, int percentileThreshold)
        {
            double[] profile = NoiseProfile.GetNoiseProfile_fromLowestPercentileFrames(matrix, percentileThreshold);
            profile = DataTools.filterMovingAverage(profile, 3);

            // to prevent division by zero.
            double epsilon = 0.0001;

            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] outM = new double[rowCount, colCount]; //to contain noise reduced matrix

            for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
            {
                double denominator = profile[col];
                if (denominator < epsilon)
                {
                    denominator = epsilon;
                }

                for (int y = 0; y < rowCount; y++) //for all rows
                {
                    outM[y, col] = Math.Sqrt(matrix[y, col] / denominator);
                } //end for all rows
            } //end for all cols

            return outM;
        }

        /// <summary>
        /// Assumes the passed matrix is a spectrogram. i.e. rows=frames, cols=freq bins.
        /// WARNING: This method should NOT be used for short recordings (i.e LT approx 10-15 seconds long)
        /// Obtains a background noise profile from the passed percentile of lowest energy frames,
        /// Then subtracts the noise profile value from every cell.
        /// This method was adapted from a paper by Briggs.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="percentileThreshold"></param>
        /// <returns></returns>
        public static double[,] NoiseReduction_byLowestPercentileSubtraction(double[,] matrix, int percentileThreshold)
        {
            double[] profile = NoiseProfile.GetNoiseProfile_fromLowestPercentileFrames(matrix, percentileThreshold);
            profile = DataTools.filterMovingAverage(profile, 3);

            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] outM = new double[rowCount, colCount]; //to contain noise reduced matrix

            for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
            {
                for (int y = 0; y < rowCount; y++) //for all rows
                {
                    outM[y, col] = matrix[y, col] - profile[col];
                } //end for all rows
            } //end for all cols

            return outM;
        }

        /// <summary>
        /// Assumes the passed matrix is a spectrogram. i.e. rows=frames, cols=freq bins.
        /// Does column-wise LCN (Local Contrast Normalisation.
        /// The denominator = (contrastLevel + Math.Sqrt(localVariance[y])
        /// A low contrastLevel = 0.1 give more grey image.
        /// A high contrastLevel = 1.0 give mostly white high contrast image.
        /// It tried various other normalisation equations as can be seen below.
        /// Taking square-root of top line results in too much background.
        /// The algorithm is not overly sensitive to the neighbourhood size.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="neighbourhood">suitable vaues are odd numbers 9 - 59</param>
        /// <param name="contrastLevel">Suitable values are 0.1 to 1.0.</param>
        /// <returns></returns>
        public static double[,] NoiseReduction_byLCNDivision(double[,] matrix, int neighbourhood, double contrastLevel)
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

                // NormaliseMatrixValues with local column variance
                for (int y = 0; y < rowCount; y++) //for all rows
                {
                    //outM[y, col] = matrix[y, col] / (contrastLevel + localVariance[y]);
                    outM[y, col] = matrix[y, col] / (contrastLevel + Math.Sqrt(localVariance[y]));

                    //outM[y, col] = Math.Sqrt(matrix[y, col]) / (contrastLevel + Math.Sqrt(localVariance[y]));
                    //outM[y, col] = Math.Sqrt(matrix[y, col] / (contrastLevel + Math.Sqrt(localVariance[y])));
                    //outM[y, col] = matrix[y, col] / (1 + (1.0 * Math.Sqrt(localVariance[y])));
                    //outM[y, col] = Math.Sqrt(matrix[y, col] / (1 + (0.10 * localVariance[y])));
                } //end for all rows
            } //end for all cols

            return outM;
        }

        /// <summary>
        /// Assumes the passed matrix is a spectrogram. i.e. rows=frames, cols=freq bins.
        /// First obtains background noise profile calculated from lowest 20% of cells for each freq bin independently.
        /// Loop over freq bins (columns) - subtract noise and divide by LCN (Local Contrast Normalisation.
        ///
        /// The LCN denominator = (contrastLevelConstant + Sqrt(localVariance[y])
        /// Note that sqrt of variance = std dev.
        /// A low contrastLevel = 0.1 give more grey image.
        /// A high contrastLevel = 1.0 give mostly white high contrast image.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="lowPercent"></param>
        /// <param name="neighbourhood"></param>
        /// <param name="contrastLevel"></param>
        /// <returns></returns>
        public static double[,] NoiseReduction_ShortRecordings_SubtractAndLCN(double[,] matrix, int lowPercent, int neighbourhood, double contrastLevel)
        {
            double[] noiseProfile = NoiseProfile.GetNoiseProfile_BinWiseFromLowestPercentileCells(matrix, lowPercent);
            noiseProfile = DataTools.filterMovingAverage(noiseProfile, 5);
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);

            //to contain noise reduced matrix
            double[,] outM = new double[rowCount, colCount];

            //for all cols i.e. freq bins
            for (int col = 0; col < colCount; col++)
            {
                double[] column = MatrixTools.GetColumn(matrix, col);
                double[] localVariance = NormalDist.CalculateLocalVariance(column, neighbourhood);

                // NormaliseMatrixValues with local column variance
                for (int y = 0; y < rowCount; y++) //for all rows
                {
                    //outM[y, col] = matrix[y, col] / (contrastLevel + localVariance[y]);
                    outM[y, col] = (matrix[y, col] - noiseProfile[col]) / (contrastLevel + Math.Sqrt(localVariance[y]));
                } //end for all rows
            } //end for all cols

            return outM;
        }

// #########################################################################################################################################################

        public static double[,] BriggsNoiseFilterAndGetMask(double[,] matrix, int percentileThreshold, double binaryThreshold)
        {
            double[,] m = NoiseReduction_byDivision(matrix, percentileThreshold);

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
                                TimeSpan recordingDuration, TimeSpan X_AxisInterval, TimeSpan stepDuration, int nyquist, int herzInterval)
        {
            //double[,] m = NoiseRemoval_Briggs.BriggsNoiseFilter(matrix, percentileThreshold);
            double[,] m = NoiseReduction_byDivisionAndSqrRoot(matrix, percentileThreshold);

           var images = new List<Image<Rgb24>>();

            string title = "TITLE ONE";
            var image1 = DrawSonogram(m, recordingDuration, X_AxisInterval, stepDuration, nyquist, herzInterval, title);
            images.Add(image1);

            m = ImageTools.WienerFilter(m, 7); //Briggs uses 17
            m = MatrixTools.SubtractAndTruncate2Zero(m, 1.0);
            title = "TITLE TWO";
            var image2 = DrawSonogram(m, recordingDuration, X_AxisInterval, stepDuration, nyquist, herzInterval, title);
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
            var image4 = DrawSonogram(m, recordingDuration, X_AxisInterval, stepDuration, nyquist, herzInterval, title);
            images.Add(image4);

            Image combinedImage = ImageTools.CombineImagesVertically(images);

            return combinedImage;
        }

        public static Image<Rgb24> DrawSonogram(double[,] data, TimeSpan recordingDuration, TimeSpan X_interval, TimeSpan xAxisPixelDuration,
                                         int nyquist, int herzInterval, string title)
        {
            // the next two variables determine how the greyscale sonogram image is normalised.
            // The low  normalisation bound is min value of the average spectrogram derived from the lowest  percent of frames
            // The high normalisation bound is max value of the average spectrogram derived from the highest percent of frames
            int minPercentile = 5;
            int maxPercentile = 10;

            var image = BaseSonogram.GetSonogramImage(data, minPercentile, maxPercentile);

            Image titleBar = BaseSonogram.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            TimeSpan minuteOffset = TimeSpan.Zero;
            TimeSpan labelInterval = TimeSpan.FromSeconds(5);
            image = BaseSonogram.FrameSonogram(image, titleBar, minuteOffset, X_interval, xAxisPixelDuration, labelInterval, nyquist, herzInterval);

            return image;
        }
    } // class NoiseRemoval_Briggs
}
