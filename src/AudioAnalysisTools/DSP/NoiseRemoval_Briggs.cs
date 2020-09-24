// <copyright file="NoiseRemoval_Briggs.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using AudioAnalysisTools.StandardSpectrograms;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using TowseyLibrary;

    public static class NoiseRemoval_Briggs
    {
        /// <summary>
        /// Assumes the passed matrix is a spectrogram. i.e. rows=frames, cols=freq bins.
        /// WARNING: This method should NOT be used for short recordings (i.e LT approx 10-15 seconds long)
        /// because it obtains a background noise profile from the passed percentile of lowest energy frames.
        ///
        /// Same method as above except take square root of the cell energy divided by the noise.
        /// Taking the square root has the effect of reducing image contrast.
        /// </summary>
        public static double[,] NoiseReductionByDivisionAndSqrRoot(double[,] matrix, int percentileThreshold)
        {
            double[] profile = NoiseProfile.GetNoiseProfile_fromLowestPercentileFrames(matrix, percentileThreshold);
            profile = DataTools.filterMovingAverage(profile, 3);

            // to prevent division by zero.
            double epsilon = 0.0001;

            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] outM = new double[rowCount, colCount]; //to contain noise reduced matrix

            // for all cols i.e. freq bins
            for (int col = 0; col < colCount; col++)
            {
                double denominator = profile[col];
                if (denominator < epsilon)
                {
                    denominator = epsilon;
                }

                // for all rows
                for (int y = 0; y < rowCount; y++)
                {
                    outM[y, col] = Math.Sqrt(matrix[y, col] / denominator);
                }
            }

            return outM;
        }

        /// <summary>
        /// Assumes the passed matrix is a spectrogram. i.e. rows=frames, cols=freq bins,
        /// and that all values in data matrix are positive.
        /// WARNING: This method should NOT be used for short recordings (i.e LT approx 10-15 seconds long)
        /// Obtains a background noise profile from the passed percentile of lowest energy frames,
        /// Then subtracts the noise profile value from every cell.
        /// This method was adapted from a paper by Briggs.
        /// </summary>
        /// <param name="matrix">the passed amplitude or energy spectrogram.</param>
        /// <param name="percentileThreshold">Must be an integer percent.</param>
        /// <returns>Spectrogram data matrix with noise subtracted.</returns>
        public static double[,] NoiseReductionByLowestPercentileSubtraction(double[,] matrix, int percentileThreshold)
        {
            double[] profile = NoiseProfile.GetNoiseProfile_fromLowestPercentileFrames(matrix, percentileThreshold);
            profile = DataTools.filterMovingAverage(profile, 3);

            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);

            //to contain noise reduced matrix
            double[,] outM = new double[rowCount, colCount];

            //for all cols i.e. freq bins
            for (int col = 0; col < colCount; col++)
            {
                //for all rows
                for (int y = 0; y < rowCount; y++)
                {
                    outM[y, col] = matrix[y, col] - profile[col];
                }
            }

            return outM;
        }

        // # NEXT FOUR METHODS ARE FOR LOCAL CONTRAST NORMALISATION ###############################################################################################

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
        public static double[,] NoiseReductionByLcn(double[,] matrix, int lowPercent, int neighbourhood, double contrastLevel)
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
                double[] localVariance = CalculateLocalVariance1(column, neighbourhood);

                // NormaliseMatrixValues with local column variance
                for (int y = 0; y < rowCount; y++)
                {
                    outM[y, col] = (matrix[y, col] - noiseProfile[col]) / (contrastLevel + Math.Sqrt(localVariance[y]));
                }
            }

            return outM;
        }

        /// <summary>
        /// THis method similar to the above BUT:
        /// 1: it does not do initial subtraction of lowest percentile noise.
        /// 2: it calculates local variance from local matrix andnot local frequency bin.
        /// Assumes the passed matrix is a spectrogram. i.e. rows=frames, cols=freq bins.
        /// Currently, the denominator = (contrastLevel + Math.Sqrt(localVariance[y])
        /// A low contrastLevel = 0.1 give more grey image.
        /// A high contrastLevel = 1.0 give mostly white high contrast image.
        /// I tried various other normalisation equations as can be seen below.
        /// NOTE: Taking square-root of top line results in too much background.
        /// The algorithm is not overly sensitive to the neighbourhood size.
        /// </summary>
        /// <param name="neighbourhood">suitable vaues are odd numbers 9 - 59.</param>
        /// <param name="contrastLevel">Suitable values are 0.1 to 1.0.</param>
        public static double[,] NoiseReductionByLcn(double[,] matrix, int neighbourhood, double contrastLevel)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);

            //to contain noise reduced matrix
            double[,] outM = new double[rowCount, colCount];

            //for all cols i.e. freq bins
            for (int col = 0; col < colCount; col++)
            {
                double[] localVariance = CalculateLocalVariance2(matrix, col, neighbourhood);

                // NormaliseMatrixValues with local matrix standard deviation
                for (int y = 0; y < rowCount; y++)
                {
                    //outM[y, col] = matrix[y, col] / (contrastLevel + localVariance[y]);
                    //outM[y, col] = Math.Sqrt(matrix[y, col]) / (contrastLevel + Math.Sqrt(localVariance[y]));
                    outM[y, col] = matrix[y, col] / (contrastLevel + Math.Sqrt(localVariance[y]));

                }
            } //end for all cols

            return outM;
        }

        /// <summary>
        /// This was written for the local contrast normalisation (LCN) of amplitude spectrograms.
        /// However the contrast is calculated wrt the local part of frequency bin or column.
        /// Plugging up ends of the returned array as done here is a hack but it does not really matter.
        /// </summary>
        public static double[] CalculateLocalVariance1(double[] data, int window)
        {
            int length = data.Length;
            int halfwindow = window / 2;
            double[] variances = new double[length];

            for (int i = 0; i <= length - window; i++)
            {
                double[] subV = DataTools.Subarray(data, i, window);
                NormalDist.AverageAndVariance(subV, out var av, out var variance);
                variances[i + halfwindow] = variance;
            }

            // plug up the ends of array
            for (int i = 0; i < halfwindow; i++)
            {
                variances[i] = variances[halfwindow];
                variances[length - i - 1] = variances[length - halfwindow - 1];
            }

            return variances;
        }

        /// <summary>
        /// THis method is equivalent to the above method - CalculateLocalVariance1(),
        /// except that the local variance is derived from a local matrix rather than the local frequency bin.
        /// </summary>
        public static double[] CalculateLocalVariance2(double[,] matrix, int colId, int rowWindow)
        {
            // the column window must be an odd number.
            int colWindow = 5;
            int halfColWindow = colWindow / 2;
            int halfRowWindow = rowWindow / 2;

            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);

            double[] variances = new double[rowCount];

            // set up the column Ids.
            int c1 = colId - halfColWindow;
            int c2 = colId + halfColWindow;
            if (c1 < 0)
            {
                c1 = 0;
                c2 = colWindow - 1;
            }
            else
            if (c2 >= colCount)
            {
                c1 = colCount - colWindow - 1;
                c2 = colCount - 1;
            }

            for (int i = 0; i < rowCount; i++)
            {
                int r1 = i - halfRowWindow;
                int r2 = i + halfRowWindow - 1;
                if (r1 < 0)
                {
                    r1 = 0;
                    r2 = rowWindow - 1;
                }
                else
                if (r2 >= rowCount)
                {
                    r1 = rowCount - rowWindow;
                    r2 = rowCount - 1;
                }

                double[,] subMatrix = MatrixTools.Submatrix(matrix, r1, c1, r2, c2);
                var vector = MatrixTools.Matrix2Array(subMatrix);
                NormalDist.AverageAndVariance(vector, out var av, out var variance);
                variances[i] = variance;
            }

            return variances;
        }

        // #########################################################################################################################################################

        public static double[,] BriggsNoiseFilterAndGetMask(double[,] matrix, int percentileThreshold, double binaryThreshold)
        {
            double[,] m = NoiseReductionByDivision(matrix, percentileThreshold);

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

        /// <summary>
        /// Assumes the passed matrix is a spectrogram. i.e. rows=frames, cols=freq bins.
        /// WARNING: This method should NOT be used for short recordings (i.e LT approx 10-15 seconds long)
        /// Obtains a background noise profile from the passed percentile of lowest energy frames,
        /// Then divide cell energy by the profile value.
        /// This method was adapted from a paper by Briggs.
        /// </summary>
        public static double[,] NoiseReductionByDivision(double[,] matrix, int percentileThreshold)
        {
            double[] profile = NoiseProfile.GetNoiseProfile_fromLowestPercentileFrames(matrix, percentileThreshold);
            profile = DataTools.filterMovingAverage(profile, 3);

            // to prevent division by zero.
            double epsilon = 0.0001;

            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] outM = new double[rowCount, colCount]; //to contain noise reduced matrix

            // for all cols i.e. freq bins
            for (int col = 0; col < colCount; col++)
            {
                double denominator = profile[col];
                if (denominator < epsilon)
                {
                    denominator = epsilon;
                }

                // for all rows
                for (int y = 0; y < rowCount; y++)
                {
                    outM[y, col] = matrix[y, col] / denominator;
                }
            } //end for all cols

            return outM;
        }

        public static Image BriggsNoiseFilterAndGetSonograms(
            double[,] matrix,
            int percentileThreshold,
            double binaryThreshold,
            TimeSpan recordingDuration,
            TimeSpan xAxisInterval,
            TimeSpan stepDuration,
            int nyquist,
            int herzInterval)
        {
            double[,] m = NoiseReductionByDivisionAndSqrRoot(matrix, percentileThreshold);

            var images = new List<Image<Rgb24>>();

            string title = "TITLE ONE";
            var image1 = DrawSonogram(m, recordingDuration, xAxisInterval, stepDuration, nyquist, herzInterval, title);
            images.Add(image1);

            m = ImageTools.WienerFilter(m, 7); //Briggs uses 17
            m = MatrixTools.SubtractAndTruncate2Zero(m, 1.0);
            title = "TITLE TWO";
            var image2 = DrawSonogram(m, recordingDuration, xAxisInterval, stepDuration, nyquist, herzInterval, title);
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
            var image4 = DrawSonogram(m, recordingDuration, xAxisInterval, stepDuration, nyquist, herzInterval, title);
            images.Add(image4);

            Image combinedImage = ImageTools.CombineImagesVertically(images);

            return combinedImage;
        }

        public static Image<Rgb24> DrawSonogram(
            double[,] data,
            TimeSpan recordingDuration,
            TimeSpan xInterval,
            TimeSpan xAxisPixelDuration,
            int nyquist,
            int herzInterval,
            string title)
        {
            // the next two variables determine how the greyscale sonogram image is normalised.
            // The low  normalisation bound is min value of the average spectrogram derived from the lowest  percent of frames
            // The high normalisation bound is max value of the average spectrogram derived from the highest percent of frames
            int minPercentile = 5;
            int maxPercentile = 10;

            var image = BaseSonogram.GetSonogramImage(data, minPercentile, maxPercentile);

            var titleBar = BaseSonogram.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            TimeSpan minuteOffset = TimeSpan.Zero;
            TimeSpan labelInterval = TimeSpan.FromSeconds(5);
            image = BaseSonogram.FrameSonogram(image, titleBar, minuteOffset, xInterval, xAxisPixelDuration, labelInterval, nyquist, herzInterval);

            return image;
        }
    }
}