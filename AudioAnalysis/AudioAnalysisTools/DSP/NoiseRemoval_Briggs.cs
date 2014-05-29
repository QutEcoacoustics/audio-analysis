using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLibrary;

using System.Drawing;


namespace AudioAnalysisTools.DSP
{
    public static class NoiseRemoval_Briggs
    {

        public static double[,] BriggsNoiseFilterOnce(double[,] matrix, double parameter) 
        {
            double[] profile = NoiseRemoval_Briggs.GetNoiseProfile_LowestPercentile(matrix, parameter);
            double[,] m = NoiseRemoval_Briggs.BriggsNoiseRemoval(matrix, profile);
            return m;
        }

        public static double[,] BriggsNoiseFilterTwice(double[,] matrix, double parameter)
        {
            double[] profile = NoiseRemoval_Briggs.GetNoiseProfile_LowestPercentile(matrix, parameter);
            //double[,] m = NoiseRemoval_Briggs.BriggsNoiseRemoval(matrix, profile);
            double[,] m = NoiseRemoval_Briggs.BriggsNoiseRemovalUsingSquareroot(matrix, profile);
            profile = NoiseRemoval_Briggs.GetNoiseProfile_LowestPercentile(m, parameter);
            //m = NoiseRemoval_Briggs.BriggsNoiseRemoval(m, profile);
            m = NoiseRemoval_Briggs.BriggsNoiseRemovalUsingSquareroot(m, profile);            
            return m;
        }


        public static double[] GetNoiseProfile_LowestPercentile(double[,] matrix, double lowPercentile)
        {
            double[] energyLevels = MatrixTools.GetRowAverages(matrix);
            var sorted = DataTools.SortArrayInAscendingOrder(energyLevels);
            int[] order = sorted.Item1;
            //double[] values = sorted.Item2;
            //for (int i = 0; i < 20; i++) Console.WriteLine(values[i]);

            int colCount = matrix.GetLength(1);
            int cutoff = (int)(lowPercentile * matrix.GetLength(0));
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
                //noiseProfile[c] += 0.0000000001; //to avoid zero values - which is very unlikely given we are in dB.
            }
            //for (int i = 0; i < colCount; i++) Console.WriteLine(noiseProfile[i]);

            return noiseProfile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] BriggsNoiseRemoval(double[,] matrix, double[] noiseProfile)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] outM = new double[rowCount, colCount]; //to contain noise reduced matrix

            for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
            {
                for (int y = 0; y < rowCount; y++) //for all rows
                {
                    outM[y, col] = matrix[y, col] / noiseProfile[col];
                } //end for all rows
            } //end for all cols
            return outM;
        } // end of TruncateModalNoise()

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] BriggsNoiseRemovalUsingSquareroot(double[,] matrix, double[] noiseProfile)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[] profile = DataTools.SquareRootOfValues(noiseProfile);
            double[,] outM = new double[rowCount, colCount]; //to contain noise reduced matrix

            for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
            {
                for (int y = 0; y < rowCount; y++) //for all rows
                {
                    outM[y, col] = Math.Sqrt(matrix[y, col]) / profile[col];
                } //end for all rows
            } //end for all cols
            return outM;
        } // end of TruncateModalNoise()



        public static Image BriggsNoiseFilterTwiceAndGetSonograms(double[,] matrix, double parameter,
                                                                  TimeSpan recordingDuration, TimeSpan X_AxisInterval, TimeSpan stepDuration, int Y_AxisInterval)
        {
            double[] profile = NoiseRemoval_Briggs.GetNoiseProfile_LowestPercentile(matrix, parameter);
            //double[,] m = NoiseRemoval_Briggs.BriggsNoiseRemoval(matrix, profile);
            double[,] m = NoiseRemoval_Briggs.BriggsNoiseRemovalUsingSquareroot(matrix, profile);
            //TimeSpan recordingDuration, 
            //TimeSpan X_AxisInterval, 
            //TimeSpan xAxisPixelDuration, ie stepDuration
            //int Y_AxisInterval

            string title = "TITLE ONE";
            Image image1 = DrawSonogram(m, recordingDuration, X_AxisInterval, stepDuration, Y_AxisInterval, title);


            //profile = NoiseRemoval_Briggs.GetNoiseProfile_LowestPercentile(m, parameter);
            //m = NoiseRemoval_Briggs.BriggsNoiseRemoval(m, profile);
            //m = NoiseRemoval_Briggs.BriggsNoiseRemovalUsingSquareroot(m, profile);
            title = "TITLE TWO";
            Image image2 = DrawSonogram(m, recordingDuration, X_AxisInterval, stepDuration, Y_AxisInterval, title);

            double[,] m1 = MatrixTools.SubtractAndTruncate2Zero(m, 0.9);
            title = "TITLE THREE";
            Image image3 = DrawSonogram(m1, recordingDuration, X_AxisInterval, stepDuration, Y_AxisInterval, title);
            m = ImageTools.WienerFilter(m, 17); //Briggs uses 17
            m = SNR.RemoveNeighbourhoodBackgroundNoise(m, 0.4);

            m = MatrixTools.ThresholdMatrix2RealBinary(m, 1.4); //use this for single filter
            //m = MatrixTools.ThresholdMatrix2RealBinary(m, 1.2); //use this for double filter
            title = "TITLE FOUR";
            Image image4 = DrawSonogram(m, recordingDuration, X_AxisInterval, stepDuration, Y_AxisInterval, title);

            Image[] array = new Image[4];
            array[0] = image1;
            array[1] = image2;
            array[2] = image3;
            array[3] = image4;
            Image combinedImage = ImageTools.CombineImagesVertically(array);

            return combinedImage;
        }


        static Image DrawSonogram(double[,] data, TimeSpan recordingDuration, TimeSpan X_interval, TimeSpan xAxisPixelDuration, int Y_interval, string title)
        {
            //double framesPerSecond = 1000 / xAxisPixelDuration.TotalMilliseconds;
            Image image = BaseSonogram.GetSonogramImage(data);

            Image titleBar = BaseSonogram.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            TimeSpan minuteOffset = TimeSpan.Zero;
            image = BaseSonogram.FrameSpectrogram(image, titleBar, minuteOffset, X_interval, xAxisPixelDuration, Y_interval);

            return image;
        }



    } // class NoiseRemoval_Briggs
}
