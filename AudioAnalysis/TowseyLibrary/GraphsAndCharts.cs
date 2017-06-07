// <copyright file="GraphsAndCharts.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;

    public static class GraphsAndCharts
    {
        public static Image DrawHistogram(string label, int[] histogram, int upperPercentileBin, Dictionary<string, double> statistics, int imageWidth, int height)
        {
            int sum = histogram.Sum();
            Pen pen1 = new Pen(Color.White);

            // Pen pen2 = new Pen(Color.Red);
            var pen3 = new Pen(Color.Wheat);
            var pen4 = new Pen(Color.Purple);
            var brush = new SolidBrush(Color.Red);
            var stringFont = new Font("Arial", 9);

            //Font stringFont = new Font("Tahoma", 9);
            //SizeF stringSize = new SizeF();

            //imageWidth = 300;
            int barWidth = imageWidth / histogram.Length;
            int upperBound = upperPercentileBin * barWidth;

            DataTools.getMaxIndex(histogram, out int modeBin);
            modeBin *= barWidth;

            int grid1 = imageWidth / 4;
            int grid2 = imageWidth / 2;
            int grid3 = (imageWidth * 3) / 4;

            Bitmap bmp = new Bitmap(imageWidth, height, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            g.DrawLine(pen3, grid1, height - 1, grid1, 0);
            g.DrawLine(pen3, grid2, height - 1, grid2, 0);
            g.DrawLine(pen3, grid3, height - 1, grid3, 0);
            g.DrawLine(pen1, 0, height - 1, imageWidth, height - 1);

            // draw mode bin and upper percentile bound
            g.DrawLine(pen4, modeBin, height - 1, modeBin, 0);
            g.DrawLine(pen4, upperBound, height - 1, upperBound, 0);

            g.DrawString(label, stringFont, Brushes.Wheat, new PointF(4, 3));

            if (statistics != null)
            {
                string[] statKeys = statistics.Keys.ToArray();
                for (int s = 0; s < statKeys.Length; s++)
                {
                    int y = s * 12; // 10 = height of line of text
                    string str;
                    if (statKeys[s] == "count")
                    {
                        str = $"{statKeys[s]}={statistics[statKeys[s]] :f0}";
                    }
                    else
                    {
                        str = $"{statKeys[s]}={statistics[statKeys[s]] :f3}";
                    }

                    g.DrawString(str, stringFont, Brushes.Wheat, new PointF(grid2, y));
                } // for loop
            } // f(statistics != null)

            for (int b = 0; b < histogram.Length; b++)
            {
                int x = b * barWidth;
                int y = (int)Math.Ceiling(histogram[b] * height * 2 / (double)sum);
                g.FillRectangle(brush, x, height - y - 1, barWidth, y);
            }

            return bmp;
        }

        /// <summary>
        /// This method places startTime in the centre of the waveform image and then cuts out buffer on either side.
        /// </summary>
        public static Image DrawWaveform(string label, double[] signal)
        {
            int height = 300;
            double max = -2.0;
            foreach (double value in signal)
            {
                double absValue = Math.Abs(value);
                if (absValue > max)
                {
                    max = absValue;
                }
            }

            double scalingFactor = 0.5 / max;
            Image image = DrawWaveform(label, signal, signal.Length, height, scalingFactor);
            return image;
        }

        /// <summary>
        /// This method places startTime in the centre of the waveform image and then cuts out buffer on either side.
        /// </summary>
        public static Image DrawWaveform(string label, double[] signal, double scalingFactor)
        {
            int height = 300;
            Image image = DrawWaveform(label, signal, signal.Length, height, scalingFactor);
            return image;
        }

        /// <summary>
        /// Asumes signal is between -1 and +1.
        /// </summary>
        public static Image DrawWaveform(string label, double[] signal, int imageWidth, int height, double scalingFactor)
        {
            Pen pen1 = new Pen(Color.White);
            Pen pen2 = new Pen(Color.Lime);
            Pen pen3 = new Pen(Color.Wheat);
            Font stringFont = new Font("Arial", 9);

            int barWidth = imageWidth / signal.Length;

            // DataTools.getMaxIndex(signal, out int maxBin);
            // double maxValue = signal[maxBin];
            // double sum = histogram.Sum();
            // double[] normalArray = DataTools.NormaliseArea()

            int yzero = height / 2;
            int grid1 = imageWidth / 4;
            int grid2 = imageWidth / 2;
            int grid3 = (imageWidth * 3) / 4;

            Bitmap bmp1 = new Bitmap(imageWidth, height, PixelFormat.Format24bppRgb);
            Graphics g1 = Graphics.FromImage(bmp1);
            g1.Clear(Color.Black);
            g1.DrawLine(pen3, 0, yzero, imageWidth, yzero);
            g1.DrawLine(pen3, grid1, height - 1, grid1, 0);
            g1.DrawLine(pen3, grid2, height - 1, grid2, 0);
            g1.DrawLine(pen3, grid3, height - 1, grid3, 0);
            g1.DrawLine(pen1, 0, height - 1, imageWidth, height - 1);

            // draw mode bin and upper percentile bound
            //g1.DrawLine(pen4, modeBin, height - 1, modeBin, 0);
            //g1.DrawLine(pen4, upperBound, height - 1, upperBound, 0);

            int previousY = yzero;
            for (int b = 0; b < signal.Length; b++)
            {
                int x = b * barWidth;
                int y = yzero - (int)Math.Ceiling(signal[b] * height * scalingFactor);
                g1.DrawLine(pen2, x, previousY, x + 1, y);
                previousY = y;
            }

            Bitmap bmp2 = new Bitmap(imageWidth, 20, PixelFormat.Format24bppRgb);
            Graphics g2 = Graphics.FromImage(bmp2);
            g2.DrawLine(pen1, 0, bmp2.Height - 1, imageWidth, bmp2.Height - 1);
            g2.DrawString(label, stringFont, Brushes.Wheat, new PointF(4, 3));

            Image[] images = { bmp2, bmp1 };
            Image bmp = ImageTools.CombineImagesVertically(images);
            return bmp;
        }

        public static void DrawGraph(double[] rawdata, string label, FileInfo file)
        {
            var normalisedIndex = DataTools.normalise(rawdata);
            var image2 = GraphsAndCharts.DrawGraph(label, normalisedIndex, 100);
            image2.Save(file.FullName);
        }

        /// <summary>
        /// Assumes passed data has been normalised in 0,1.
        /// </summary>
        public static Image DrawGraph(string label, double[] normalisedData, int imageHeight)
        {
            int imageWidth = normalisedData.Length;
            var pen1 = new Pen(Color.White);
            var pen2 = new Pen(Color.Red);

            // Pen pen3 = new Pen(Color.Wheat);
            // Pen pen4 = new Pen(Color.Purple);
            // SolidBrush brush = new SolidBrush(Color.Red);
            var stringFont = new Font("Arial", 9);

            //Font stringFont = new Font("Tahoma", 9);
            //SizeF stringSize = new SizeF();

            var bmp1 = new Bitmap(imageWidth, imageHeight, PixelFormat.Format24bppRgb);
            var g1 = Graphics.FromImage(bmp1);
            g1.Clear(Color.Black);

            //for (int i = 1; i < 10; i++)
            //{
            //    int grid = imageWidth * i / 10;
            //    g1.DrawLine(pen3, grid, height - 1, grid, 0);
            //}
            //g1.DrawLine(pen1, 0, height - 1, imageWidth, imageHeight - 1);
            // draw mode bin and upper percentile bound
            //g.DrawLine(pen4, modeBin, height - 1, modeBin, 0);
            //g.DrawLine(pen4, upperBound, height - 1, upperBound, 0);

            int previousY = (int)Math.Ceiling(normalisedData[0] * imageHeight);
            for (int x = 1; x < imageWidth; x++)
            {
                int y = (int)Math.Ceiling(normalisedData[x] * imageHeight);
                g1.DrawLine(pen2, x - 1, imageHeight - previousY, x, imageHeight - y);
                previousY = y;
            }

            Bitmap bmp2 = new Bitmap(imageWidth, 20, PixelFormat.Format24bppRgb);
            Graphics g2 = Graphics.FromImage(bmp2);
            g2.DrawLine(pen1, 0, 0, imageWidth, 0);
            g2.DrawLine(pen1, 0, bmp2.Height - 1, imageWidth, bmp2.Height - 1);
            g2.DrawString(label, stringFont, Brushes.Wheat, new PointF(4, 3));

            Image[] images = { bmp2, bmp1 };
            Image bmp = ImageTools.CombineImagesVertically(images);
            return bmp;
        }

        public static Image DrawGraph(string label, double[] histogram, int imageWidth, int height, int scalingFactor)
        {
            Pen pen1 = new Pen(Color.White);

            //Pen pen2 = new Pen(Color.Red);
            Pen pen3 = new Pen(Color.Wheat);

            //Pen pen4 = new Pen(Color.Purple);
            var brush = new SolidBrush(Color.Red);
            var stringFont = new Font("Arial", 9);

            //Font stringFont = new Font("Tahoma", 9);
            //SizeF stringSize = new SizeF();

            int barWidth = imageWidth / histogram.Length;

            // double sum = histogram.Sum();
            // DataTools.getMaxIndex(histogram, out int maxBin);

            //double maxValue = histogram[maxBin];
            var bmp1 = new Bitmap(imageWidth, height, PixelFormat.Format24bppRgb);
            var g1 = Graphics.FromImage(bmp1);
            g1.Clear(Color.Black);

            for (int i = 1; i < 10; i++)
            {
                int grid = imageWidth * i / 10;
                g1.DrawLine(pen3, grid, height - 1, grid, 0);
            }

            g1.DrawLine(pen1, 0, height - 1, imageWidth, height - 1);

            // draw mode bin and upper percentile bound
            //g.DrawLine(pen4, modeBin, height - 1, modeBin, 0);
            //g.DrawLine(pen4, upperBound, height - 1, upperBound, 0);

            for (int b = 0; b < histogram.Length; b++)
            {
                int x = b * barWidth;
                int y = (int)Math.Ceiling(histogram[b] * height * scalingFactor);
                g1.FillRectangle(brush, x, height - y - 1, barWidth, y);
            }

            Bitmap bmp2 = new Bitmap(imageWidth, 20, PixelFormat.Format24bppRgb);
            Graphics g2 = Graphics.FromImage(bmp2);
            g2.DrawLine(pen1, 0, bmp2.Height - 1, imageWidth, bmp2.Height - 1);
            g2.DrawString(label, stringFont, Brushes.Wheat, new PointF(4, 3));

            Image[] images = { bmp2, bmp1 };
            Image bmp = ImageTools.CombineImagesVertically(images);
            return bmp;
        }

        public static Image DrawWaveAndFft(double[] signal, int sr, TimeSpan startTime, double[] fftSpectrum, int maxHz, double[] scores)
        {
            int imageHeight = 300;
            double max = -2.0;

            foreach (double value in signal)
            {
                double absValue = Math.Abs(value);
                if (absValue > max)
                {
                    max = absValue;
                }
            }

            double scalingFactor = 0.5 / max;

            // now process neighbourhood of each max
            int nyquist = sr / 2;
            int windowWidth = signal.Length;
            int binCount = windowWidth / 2;
            double hzPerBin = nyquist / (double)binCount;

            if (fftSpectrum == null)
            {
                FFT.WindowFunc wf = FFT.Hamming;
                var fft = new FFT(windowWidth, wf);
                fftSpectrum = fft.Invoke(signal);
            }

            int requiredBinCount = (int)(maxHz / hzPerBin);
            var subBandSpectrum = DataTools.Subarray(fftSpectrum, 1, requiredBinCount); // ignore DC in bin zero.

            var endTime = startTime + TimeSpan.FromSeconds(windowWidth / (double)sr);

            string title1 = $"Bandpass filtered: tStart={startTime.ToString()},  tEnd={endTime.ToString()}";
            var image4A = DrawWaveform(title1, signal, signal.Length, imageHeight, scalingFactor);

            string title2 = $"FFT 1->{maxHz}Hz.,    hz/bin={hzPerBin:f1},    score={scores[0] :f3}={scores[1] :f3}+{scores[2] :f3}";
            var image4B = DrawGraph(title2, subBandSpectrum, signal.Length, imageHeight, 1);

            var imageList = new List<Image> { image4A, image4B };

            Pen pen1 = new Pen(Color.Wheat);
            var stringFont = new Font("Arial", 9);
            var bmp2 = new Bitmap(signal.Length, 25, PixelFormat.Format24bppRgb);
            var g2 = Graphics.FromImage(bmp2);
            g2.DrawLine(pen1, 0, 0, signal.Length, 0);
            g2.DrawLine(pen1, 0, bmp2.Height - 1, signal.Length, bmp2.Height - 1);
            int barWidth = signal.Length / subBandSpectrum.Length;
            for (int i = 1; i < subBandSpectrum.Length - 1; i++)
            {
                if ((subBandSpectrum[i] > subBandSpectrum[i - 1]) && (subBandSpectrum[i] > subBandSpectrum[i + 1]))
                {
                    string label = $"{i + 1},";
                    g2.DrawString(label, stringFont, Brushes.Wheat, new PointF((i * barWidth) - 3, 3));
                }
            }

            imageList.Add(bmp2);
            var image = ImageTools.CombineImagesVertically(imageList);
            return image;
        }
    }
}
