// <copyright file="Plot.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared.ImageSharp;
    using SixLabors.Fonts;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Drawing.Processing;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    /// <summary>
    /// Represents a single array of data with X and Y scales and other info useful for plotting a graph.
    /// Was first used to represent a track of scores at the bottom of a sonogram image.
    /// </summary>
    public class Plot
    {
        public Plot(string _title, double[] _data, double _threshold)
        {
            this.title = _title;
            this.data = _data;
            this.threshold = _threshold;
        }

        /// <summary>
        /// Prepares a plot of an array of score values.
        /// To obtain a more useful display, the maximum display value is set to 3 times the threshold value.
        /// </summary>
        /// <param name="array">an array of double.</param>
        /// <param name="title">to accompany the plot.</param>
        /// <param name="threshold">A threshold value to be drawn on the plot.</param>
        /// <returns>the plot.</returns>
        public static Plot PreparePlot(double[] array, string title, double threshold)
        {
            double intensityNormalizationMax = 3 * threshold;
            var eventThreshold = threshold / intensityNormalizationMax;
            var normalisedIntensityArray = DataTools.NormaliseInZeroOne(array, 0, intensityNormalizationMax);
            var plot = new Plot(title, normalisedIntensityArray, eventThreshold);
            return plot;
        }

        public static double[] PruneScoreArray(double[] scores, double scoreThreshold, int minDuration, int maxDuration)
        {
            double[] returnData = new double[scores.Length];

            int count = scores.Length;
            bool isHit = false;
            int startId = 0;

            // pass over all frames
            for (int i = 0; i < count; i++)
            {
                if (isHit == false && scores[i] >= scoreThreshold)
                {
                    // start of an event
                    isHit = true;
                    startId = i;
                }
                else // check for the end of an event
                if (isHit && scores[i] < scoreThreshold)
                {
                    // this is end of an event, so initialise it
                    isHit = false;
                    int endId = i;

                    int duration = endId - startId;
                    if (duration < minDuration || duration > maxDuration)
                    {
                        continue; // skip events with duration shorter than threshold
                    }

                    // pass over all frames
                    for (int j = startId; j < endId; j++)
                    {
                        returnData[j] = scores[j];
                    }
                }
            } // end of pass over all frames

            return returnData;
        }

        public static void FindStartsAndEndsOfScoreEvents(
            double[] scores,
            double scoreThreshold,
            int minDuration,
            int maxDuration,
            out double[] prunedScores,
            out List<Point> startEnds)
        {
            prunedScores = new double[scores.Length];
            startEnds = new List<Point>();

            int count = scores.Length;
            bool isHit = false;
            int startId = 0;

            // pass over all frames
            for (int i = 0; i < count; i++)
            {
                // Start of an event
                if (isHit == false && scores[i] >= scoreThreshold)
                {
                    isHit = true;
                    startId = i;
                }
                else // check for the end of an event
                if (isHit && scores[i] < scoreThreshold)
                {
                    // this is end of an event, so initialise it
                    isHit = false;
                    int endId = i - 1;

                    int duration = endId - startId + 1;
                    if (duration < minDuration || duration > maxDuration)
                    {
                        // skip events with duration shorter than threshold
                        continue;
                    }

                    // pass over all frames
                    for (int j = startId; j <= endId; j++)
                    {
                        prunedScores[j] = scores[j];
                    }

                    startEnds.Add(new Point(startId, endId));
                }
            }
        }

        public string title { get; set; }

        public double[] data { get; set; }

        public double threshold { get; set; }

        public void NormaliseData()
        {
            this.data = DataTools.normalise(this.data);
        }

        public void ScaleDataArray(int newLength)
        {
            this.data = DataTools.ScaleArray(this.data, newLength);
        }

        public void NormalizeData(double min, double max)
        {
            this.data = DataTools.NormaliseInZeroOne(this.data, min, max);
        }

        /// <summary>
        /// Assumes that the data has been normalised by a call to plot.NormalizeData(double min, double max) or equivalent.
        /// </summary>
        /// <param name="height">height of the plot.</param>
        public Image<Rgb24> DrawPlot(int height)
        {
            var image = new Image<Rgb24>(this.data.Length, height);
            image.Mutate(g =>
            {
                g.Clear(Color.LightGray);
            });

            if (this.data == null)
            {
                return image;
            }

            int dataLength = this.data.Length;
            double min = 0.0;
            double max = 1.0;
            double range = max - min;

            // Next two lines are for sub-sampling if the score array is compressed to fit smaller image width.
            double subSample = dataLength / (double)image.Width;
            if (subSample < 1.0)
            {
                subSample = 1;
            }

            for (int w = 0; w < image.Width; w++)
            {
                int start = (int)Math.Round(w * subSample);
                int end = (int)Math.Round((w + 1) * subSample);
                if (end >= dataLength)
                {
                    continue;
                }

                // Find max value in sub-sample - if there is a sub-sample
                double subsampleMax = -double.MaxValue;
                for (int x = start; x < end; x++)
                {
                    if (subsampleMax < this.data[x])
                    {
                        subsampleMax = this.data[x];
                    }
                }

                double fraction = (subsampleMax - min) / range;
                int id = height - 1 - (int)(height * fraction);
                if (id < 0)
                {
                    id = 0;
                }
                else if (id > height)
                {
                    id = height; // impose bounds
                }

                for (int z = id; z < height; z++)
                {
                    image[w, z] = Color.Black; // draw the score bar
                }

                image[w, height - 1] = Color.Black; // draw base line
            }

            // Add in horizontal threshold significance line
            double f = (this.threshold - min) / range;
            int lineId = height - 1 - (int)(height * f);
            if (lineId < 0)
            {
                return image;
            }

            if (lineId > height)
            {
                return image;
            }

            for (int x = 0; x < image.Width; x++)
            {
                image[x, lineId] = Color.Lime;
            }

            return image;
        }

        public Image<Rgb24> DrawAnnotatedPlot(int height)
        {
            var image = this.DrawPlot(height);
            int length = image.Width;
            var pen = new Pen(Color.White, 1);

            var font = Drawing.Tahoma9;
            image.Mutate(g =>
            {
                g.DrawLine(pen, 0, 0, length - 1, 0);
                g.DrawTextSafe(this.title, font, Color.Red, new PointF(8, 0));

                if (this.data.Length > 500)
                {
                    var size = TextMeasurer.Measure(this.title, new RendererOptions(font));
                    g.DrawTextSafe(this.title, font, Color.Red, new PointF(length - size.Width - 2, 0));
                }

                if (this.data.Length > 1200)
                {
                    // ReSharper disable once PossibleLossOfFraction
                    g.DrawTextSafe(this.title, font, Color.Red, new PointF(length / 2, 0));
                }
            });
            return image;
        }
    }
}