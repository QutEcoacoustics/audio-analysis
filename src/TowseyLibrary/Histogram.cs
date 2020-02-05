// <copyright file="Histogram.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Histogram
    {
        public static int[] Histo(double[] data, int binCount)
        {
            DataTools.MinMax(data, out var min, out double max);
            double range = max - min;
            int[] bins = new int[binCount];

            if (range == 0.0)
            {
                bins[0] = data.Length;
                return bins;
            }

            double binWidth = range / binCount;

            // init freq bin array
            for (int i = 0; i < data.Length; i++)
            {
                int id = (int)((data[i] - min) / binWidth);
                if (id >= binCount)
                {
                    id = binCount - 1;
                }
                else if (id < 0)
                {
                    id = 0;
                }

                bins[id]++;
            }

            return bins;
        }

        public static int[] Histo(double[] data, int binCount, out double binWidth, out double min, out double max)
        {
            DataTools.MinMax(data, out min, out max);
            double range = max - min;

            // init freq bin array
            int[] bins = new int[binCount];

            if (range == 0.0)
            {
                binWidth = 0.0;
                bins[0] = data.Length;
                return bins;
            }

            int nanCount = 0;
            binWidth = range / binCount;
            for (int i = 0; i < data.Length; i++)
            {
                double value = data[i];
                int id = 0;
                if (double.IsNaN(value))
                {
                    nanCount++;
                }
                else
                {
                    id = (int)((value - min) / binWidth);
                }

                if (id >= binCount)
                {
                    id = binCount - 1;
                }

                bins[id]++;
            }

            if (nanCount > 0)
            {
                string msg = $"#### WARNING from Histogram.Histo():  {nanCount}/{data.Length} values were NaN";
                LoggedConsole.WriteErrorLine(msg);
            }

            return bins;
        }

        public static int[] Histo(double[,] data, int binCount, out double binWidth, out double min, out double max)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            int[] histo = new int[binCount];
            min = double.MaxValue;
            max = -double.MaxValue;
            DataTools.MinMax(data, out min, out max);
            binWidth = (max - min) / binCount;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    int bin = (int)((data[i, j] - min) / binWidth);
                    if (bin >= binCount)
                    {
                        bin = binCount - 1;
                    }

                    if (bin < 0)
                    {
                        bin = 0;
                    }

                    histo[bin]++;
                }
            }

            return histo;
        }

        public static int[] Histo(byte[,] data, out byte min, out byte max)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            int[] histo = new int[256];
            DataTools.MinMax(data, out min, out max);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    histo[data[i, j]]++;
                }
            }

            return histo;
        }

        /// <summary>
        /// HISTOGRAM from a matrix of double.
        /// </summary>
        public static int[] Histo(double[,] data, int binCount)
        {
            DataTools.MinMax(data, out var min, out var max);
            double binWidth = (max - min) / binCount;
            return Histo(data, binCount, min, max, binWidth);
        }

        public static int[] Histo(double[,] data, int binCount, double min, double max, double binWidth)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            int[] histo = new int[binCount];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    int bin = (int)((data[i, j] - min) / binWidth);
                    if (bin >= binCount)
                    {
                        bin = binCount - 1;
                    }

                    if (bin < 0)
                    {
                        bin = 0;
                    }

                    histo[bin]++;
                }
            }

            return histo;
        }

        /// <summary>
        /// returns a fixed width histogram.
        /// Width is determined by user supplied min and max.
        /// </summary>
        /// <param name="data">the histogram data.</param>
        /// <param name="binWidth"> should be an integer width.</param>
        /// <param name="min">min value.</param>
        /// <param name="max">max value.</param>
        public static int[] Histo_FixedWidth(int[] data, int binWidth, int min, int max)
        {
            int range = max - min + 1;
            int binCount = range / binWidth;

            // init freq bin array
            var bins = new int[binCount];
            for (int i = 0; i < data.Length; i++)
            {
                int id = (data[i] - min) / binWidth;
                if (id >= binCount)
                {
                    id = binCount - 1;
                }
                else
                    if (id < 0)
                {
                    id = 0;
                }

                bins[id]++;
            }

            return bins;
        }

        /// <summary>
        /// returns a fixed width histogram.
        /// Width is determined by user supplied min and max.
        /// </summary>
        /// <param name="data">the data.</param>
        /// <param name="binWidth"> should be an integer width.</param>
        /// <param name="min">the min value.</param>
        /// <param name="max">the max value.</param>
        public static int[] Histo_FixedWidth(double[] data, double binWidth, double min, double max)
        {
            double range = max - min + 1;
            int binCount = (int)(range / binWidth);

            // init freq bin array
            int[] bins = new int[binCount];
            for (int i = 0; i < data.Length; i++)
            {
                int id = (int)((data[i] - min) / binWidth);
                if (id >= binCount)
                {
                    id = binCount - 1;
                }
                else
                    if (id < 0)
                {
                    id = 0;
                }

                bins[id]++;
            }

            return bins;
        }

        /// <summary>
        /// HISTOGRAM from an array of int.
        /// It assumes all values are positive.
        /// </summary>
        public static int[] Histo(int[] data)
        {
            int length = data.Length;
            DataTools.MinMax(data, out int min, out int max);

            int[] histo = new int[max + 1];

            for (int i = 0; i < length; i++)
            {
                histo[data[i]]++;
            }

            return histo;
        }

        /// <summary>
        /// HISTOGRAM from an array of int.
        /// </summary>
        public static int[] Histo(int[] data, int binCount, out double binWidth, out int min, out int max)
        {
            int length = data.Length;
            int[] histo = new int[binCount];
            min = int.MaxValue;
            max = -int.MaxValue;
            DataTools.MinMax(data, out min, out max);
            binWidth = (max - min + 1) / (double)binCount;

            for (int i = 0; i < length; i++)
            {
                int bin = (int)((data[i] - min) / binWidth);
                if (bin >= binCount)
                {
                    bin = binCount - 1;
                }

                histo[bin]++;
            }

            return histo;
        }

        /// <summary>
        ///  make histogram of integers where each bin has unit width.
        /// </summary>
        public static int[] Histo(int[] data, out int min, out int max)
        {
            int length = data.Length;
            min = int.MaxValue;
            max = -int.MaxValue;
            DataTools.MinMax(data, out min, out max);
            int binCount = max - min + 1;
            int[] histo = new int[binCount];

            for (int i = 0; i < length; i++)
            {
                int bin = data[i] - min; // min values go in bin zero
                histo[bin]++;
            }

            return histo;
        }

        public static void GetHistogramOfWaveAmplitudes(double[] waveform, int window, out int[] histogramOfAmplitudes, out double minAmplitude, out double maxAmplitude, out double binWidth)
        {
            int binCount = 100;
            int windowCount = waveform.Length / window;
            double[] amplitudeArray = new double[windowCount];

            for (int i = 0; i < windowCount; i++)
            {
                double[] subsample = DataTools.Subarray(waveform, i * window, window);
                DataTools.MinMax(subsample, out var min, out var max);
                amplitudeArray[i] = max - min;
            }

            histogramOfAmplitudes = Histo(amplitudeArray, binCount, out binWidth, out minAmplitude, out maxAmplitude);
        }

        /// <summary>
        /// Returns the bin ID that coincides with the passed percentile.
        /// </summary>
        public static int GetPercentileBin(int[] histogram, int percentile)
        {
            if (percentile > 99)
            {
                throw new Exception("percentile must be < 100");
            }

            double percentAsfraction = percentile / 100D;
            int sum = histogram.Sum();
            int percentileSum = 0;
            for (int i = 0; i < histogram.Length; i++)
            {
                percentileSum += histogram[i];
                if (percentileSum / (double)sum > percentAsfraction)
                {
                    return i;
                }
            }

            return histogram.Length - 1;
        }

        public static int[] Histo_addition(double[,] data, int[] histo, double min, double max, double binWidth)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            int binCount = histo.Length;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    int bin = (int)((data[i, j] - min) / binWidth);
                    if (bin >= binCount)
                    {
                        bin = binCount - 1;
                    }

                    if (bin < 0)
                    {
                        bin = 0;
                    }

                    histo[bin]++;
                }
            }

            return histo;
        }

        public static void WriteConciseHistogram(int[] data)
        {
            DataTools.MinMax(data, out int min, out int max);
            int[] histo = new int[max + 1];
            for (int i = 0; i < data.Length; i++)
            {
                histo[data[i]]++;
            }

            for (int i = min; i <= max; i++)
            {
                LoggedConsole.WriteLine(" " + i + "|" + histo[i]);
            }

            LoggedConsole.WriteLine();
        }

        public static void WriteConcise2DHistogram(int[,] array, int max)
        {
            int[,] matrix = new int[max, max];
            for (int i = 0; i < array.Length; i++)
            {
                matrix[array[i, 0], array[i, 1]]++;
            }

            for (int r = 0; r < max; r++)
            {
                for (int c = 0; c < max; c++)
                {
                    if (matrix[r, c] > 0)
                    {
                        LoggedConsole.WriteLine(r + "|" + c + "|" + matrix[r, c] + "  ");
                    }
                }

                LoggedConsole.WriteLine();
            }
        }

        public static void DrawDistributionsAndSaveImage(double[,] matrix, string imagePath)
        {
            // calculate statistics for values in matrix
            double[] values = DataTools.Matrix2Array(matrix);
            DataTools.GetModeAndOneTailedStandardDeviation(values, out int[] histogram, out double min, out double max, out int modalBin, out double mode, out double sd);

            int width = 100;  // pixels
            int height = 100; // pixels
            int upperPercentileBin = 0;

            string title = "wpd";
            var image = GraphsAndCharts.DrawHistogram(
                title,
                histogram,
                upperPercentileBin,
                new Dictionary<string, double>()
                {
                    { "min", min },
                    { "max", max },
                    { "modal", modalBin },
                    { "mode", mode },
                    { "sd", sd },
                },
                width,
                height);

            image.Save(imagePath);
        }
    }
}
