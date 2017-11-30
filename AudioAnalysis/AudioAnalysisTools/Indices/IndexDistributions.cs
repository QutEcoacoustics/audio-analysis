﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexDistributions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the IndexDistributions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.Indices
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using TowseyLibrary;

    using Zio;

    public static class IndexDistributions
    {
        public const string SummaryIndexStatisticsFilenameFragment = "SummaryIndexStatistics";
        public const string SummaryIndexDistributionsFilenameFragment = "SummaryIndexDistributions";
        public const string SpectralIndexStatisticsFilenameFragment = "SpectralIndexStatistics";
        public const string SpectralIndexDistributionsFilenameFragment = "SpectralIndexDistributions";

        // This constant sets the upper percentile bound for RGB normalisation (0-255) of spectral indices.
        // The relevant distribution is derived from the index distribution statistics file.
        public const int UpperPercentileDefault = 98;
        public const string UpperPercentileLabel = "98%"; // corresponding label

        public class SpectralStats
        {
            public double Minimum { get; set; }

            public double Maximum { get; set; }

            public double Mode { get; set; }

            public double StandardDeviation { get; set; }

            public int UpperPercentile { get; set; }

            public int UpperPercentileBin { get; set; }

            public int[] Distribution { get; set; }

            public int Count { get; set; }

            public double GetValueOfNthPercentile(int percentile)
            {
                int length = this.Distribution.Length;
                double threshold = percentile / 100D;
                double[] probs = DataTools.NormaliseArea(this.Distribution);
                double[] cumProb = DataTools.ConvertProbabilityDistribution2CummulativeProbabilites(probs);
                int percentileBin = 0;
                for (int i = 0; i < length - 1; i++)
                {
                    if (cumProb[i] >= threshold)
                    {
                        percentileBin = i;
                        break;
                    }
                }

                this.UpperPercentileBin = percentileBin;
                double binWidth = (this.Maximum - this.Minimum) / length;
                double value = this.Minimum + (binWidth * percentileBin);
                return value;
            }
        }

        public static Dictionary<string, SpectralStats> ReadSummaryIndexDistributionStatistics(DirectoryInfo opDir, string fileStem)
        {
            FileInfo statsFile = new FileInfo(GetSummaryStatsPath(opDir, fileStem));
            if (!statsFile.Exists)
            {
                return null;
            }

            return Deserialize(statsFile);
        }

        public static Dictionary<string, SpectralStats> ReadSpectralIndexDistributionStatistics(DirectoryInfo opDir, string fileStem)
        {
            FileInfo statsFile = new FileInfo(GetSpectralStatsPath(opDir, fileStem));
            if (!statsFile.Exists)
            {
                return null;
            }

            return Deserialize(statsFile);
        }

        public static Dictionary<string, SpectralStats> WriteSpectralIndexDistributionStatistics(Dictionary<string, double[,]> spectrogramMatrices, DirectoryInfo outputDirectory, string fileStem)
        {
            // to accumulate the images
            int width = 300;  // pixels
            int height = 100; // pixels
            var imageList = new List<Image>();
            Dictionary<string, SpectralStats> indexDistributionStatistics = new Dictionary<string, SpectralStats>();

            string[] spectrogramKeys = spectrogramMatrices.Keys.ToArray();

            foreach (string key in spectrogramKeys)
            {
                if (spectrogramMatrices.ContainsKey(key))
                {
                    var matrix = spectrogramMatrices[key];
                    SpectralStats stats = GetModeAndOneTailedStandardDeviation(matrix, width, UpperPercentileDefault);
                    indexDistributionStatistics.Add(key, stats); // add index statistics
                    double value = stats.GetValueOfNthPercentile(UpperPercentileDefault);

                    imageList.Add(
                        GraphsAndCharts.DrawHistogram(
                            key,
                            stats.Distribution,
                            stats.UpperPercentileBin,
                            new Dictionary<string, double>()
                            {
                                { "min",  stats.Minimum },
                                { "max",  stats.Maximum },
                                { "mode", stats.Mode },
                                { "sd",   stats.StandardDeviation },
                                { UpperPercentileLabel,  value },
                                { "count",  stats.Count },
                            },
                            width,
                            height));
                }
            }

            FileInfo statsFile = new FileInfo(GetSpectralStatsPath(outputDirectory, fileStem));
            Json.Serialise(statsFile, indexDistributionStatistics);

            Image image3 = ImageTools.CombineImagesVertically(imageList.ToArray());
            string imagePath = GetSpectralImagePath(outputDirectory, fileStem);
            image3.Save(imagePath);

            return indexDistributionStatistics;
        }

        public static Image DrawImageOfDistribution(double[,] matrix, int width, int height, string label)
        {
            SpectralStats stats = GetModeAndOneTailedStandardDeviation(matrix, width, UpperPercentileDefault);
            double value = stats.GetValueOfNthPercentile(UpperPercentileDefault);

            var image =
                GraphsAndCharts.DrawHistogram(
                    label,
                    stats.Distribution,
                    stats.UpperPercentileBin,
                    new Dictionary<string, double>()
                    {
                                { "min",  stats.Minimum },
                                { "max",  stats.Maximum },
                                { "mode", stats.Mode },
                                { "sd",   stats.StandardDeviation },
                                { UpperPercentileLabel,  value },
                                { "count",  stats.Count },
                    },
                    width,
                    height);
            return image;
        }

        public static Dictionary<string, SpectralStats> WriteSummaryIndexDistributionStatistics(Dictionary<string, double[]> summaryIndices, DirectoryInfo outputDirectory, string fileStem)
        {
            // to accumulate the images
            int width = 100;  // pixels
            int height = 100; // pixels
            var imageList = new List<Image>();
            Dictionary<string, SpectralStats> indexDistributionStatistics = new Dictionary<string, SpectralStats>();

            string[] indexKeys = summaryIndices.Keys.ToArray();

            foreach (string key in indexKeys)
            {
                if (summaryIndices.ContainsKey(key))
                {
                    double[] array = summaryIndices[key];
                    SpectralStats stats = GetModeAndOneTailedStandardDeviation(array, width, UpperPercentileDefault);
                    indexDistributionStatistics.Add(key, stats); // add index statistics
                    double value = stats.GetValueOfNthPercentile(UpperPercentileDefault);

                    imageList.Add(
                        GraphsAndCharts.DrawHistogram(
                            key,
                            stats.Distribution,
                            stats.UpperPercentileBin,
                            new Dictionary<string, double>()
                            {
                                { "min",  stats.Minimum },
                                { "max",  stats.Maximum },
                                { "mode", stats.Mode },
                                { "sd",   stats.StandardDeviation },
                                { UpperPercentileLabel,  value },
                                { "count",  stats.Count },
                            },
                            width,
                            height));
                }
            }

            FileInfo statsFile = new FileInfo(GetSummaryStatsPath(outputDirectory, fileStem));
            Json.Serialise(statsFile, indexDistributionStatistics);

            Image image3 = ImageTools.CombineImagesVertically(imageList.ToArray());
            string imagePath = GetSummaryImagePath(outputDirectory, fileStem);
            image3.Save(imagePath);

            return indexDistributionStatistics;
        }

        public static SpectralStats GetModeAndOneTailedStandardDeviation(double[,] matrix)
        {
            int binCount = 100;
            int upperPercentile = 0;
            double[] values = DataTools.Matrix2Array(matrix);
            const bool displayHistogram = false;
            double min, max, mode, sd;
            DataTools.GetModeAndOneTailedStandardDeviation(values, displayHistogram, out min, out max, out mode, out sd);
            int[] histogram = Histogram.Histo(matrix, binCount);

            return new SpectralStats()
            {
                Minimum = min,
                Maximum = max,
                Mode = mode,
                StandardDeviation = sd,
                UpperPercentile = upperPercentile,
                Distribution = histogram,
            };
        }

        public static SpectralStats GetModeAndOneTailedStandardDeviation(double[,] matrix, int binCount, int upperPercentile)
        {
            double[] values = DataTools.Matrix2Array(matrix);
            return GetModeAndOneTailedStandardDeviation(values, binCount, upperPercentile);
        }

        public static SpectralStats GetModeAndOneTailedStandardDeviation(double[] values, int binCount, int upperPercentile)
        {
            const bool displayHistogram = false;
            double min, max, mode, sd;
            DataTools.GetModeAndOneTailedStandardDeviation(values, displayHistogram, out min, out max, out mode, out sd);
            int[] histogram = Histogram.Histo(values, binCount);

            return new SpectralStats()
            {
                Minimum = min,
                Maximum = max,
                Mode = mode,
                StandardDeviation = sd,
                UpperPercentile = upperPercentile,
                Distribution = histogram,
                Count = values.Length,
            };
        }

        public static string GetSummaryStatsPath(DirectoryInfo outputDirectory, string fileStem)
        {
            return FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, SummaryIndexStatisticsFilenameFragment, "json");
        }

        public static string GetSummaryImagePath(DirectoryInfo outputDirectory, string fileStem)
        {
            return FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, SummaryIndexDistributionsFilenameFragment, "png");
        }

        public static string GetSpectralStatsPath(DirectoryInfo outputDirectory, string fileStem)
        {
            return FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, SpectralIndexStatisticsFilenameFragment, "json");
        }

        public static string GetSpectralImagePath(DirectoryInfo outputDirectory, string fileStem)
        {
            return FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, SpectralIndexDistributionsFilenameFragment, "png");
        }

        public static Dictionary<string, SpectralStats> Deserialize(FileInfo file)
        {
            return Deserialize(file.ToFileEntry());
        }

        public static Dictionary<string, SpectralStats> Deserialize(FileEntry file)
        {
            return Json.Deserialise<Dictionary<string, SpectralStats>>(file);
        }
    }
}
