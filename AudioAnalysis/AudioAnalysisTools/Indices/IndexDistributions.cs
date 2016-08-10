// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexDistributions.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the IndexDistributions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.Indices
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using TowseyLibrary;
    using System.IO;
    using Acoustics.Shared;

    public static class IndexDistributions
    {
        public const string SummaryIndexStatisticsFilenameFragment     = "SummaryIndexStatistics";
        public const string SummaryIndexDistributionsFilenameFragment  = "SummaryIndexDistributions";
        public const string SpectralIndexStatisticsFilenameFragment    = "SpectralIndexStatistics";
        public const string SpectralIndexDistributionsFilenameFragment = "SpectralIndexDistributions";

        // This constant sets the upper percentile bound for RGB normalisation (0-255) of spectral indices.
        // The relevant distribution is derived from the index distribution statistics file.
        public const int  UPPER_PERCENTILE_DEFAULT = 98;
        public const string UPPER_PERCENTILE_LABEL = "98%"; // corresponding label


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

            //public double GetValueOfThresholdPercentile()
            //{
            //    return this.GetValueOfNthPercentile(this.UpperPercentile);
            //}
            public double GetValueOfNthPercentile(int percentile)
            {
                int length = Distribution.Length;
                double threshold = percentile / (double)100;
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
                double binWidth = (this.Maximum - this.Minimum) / (double)length;
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

            double[,] matrix;
            string[] spectrogramKeys = spectrogramMatrices.Keys.ToArray();

            foreach (string key in spectrogramKeys)
            {
                if (spectrogramMatrices.ContainsKey(key))
                {
                    matrix = spectrogramMatrices[key];
                    SpectralStats stats = GetModeAndOneTailedStandardDeviation(matrix, width, IndexDistributions.UPPER_PERCENTILE_DEFAULT);
                    indexDistributionStatistics.Add(key, stats); // add index statistics
                    double value = stats.GetValueOfNthPercentile(IndexDistributions.UPPER_PERCENTILE_DEFAULT);

                    imageList.Add(
                        ImageTools.DrawHistogram(
                            key,
                            stats.Distribution,
                            stats.UpperPercentileBin,
                            new Dictionary<string, double>()
                            {
                                { "min",  stats.Minimum },
                                { "max",  stats.Maximum },
                                { "mode", stats.Mode },
                                { "sd",   stats.StandardDeviation},
                                { IndexDistributions.UPPER_PERCENTILE_LABEL,  value},
                                { "count",  stats.Count},
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
                    SpectralStats stats = GetModeAndOneTailedStandardDeviation(array, width, IndexDistributions.UPPER_PERCENTILE_DEFAULT);
                    indexDistributionStatistics.Add(key, stats); // add index statistics
                    double value = stats.GetValueOfNthPercentile(IndexDistributions.UPPER_PERCENTILE_DEFAULT);

                    imageList.Add(
                        ImageTools.DrawHistogram(
                            key,
                            stats.Distribution,
                            stats.UpperPercentileBin,
                            new Dictionary<string, double>()
                            {
                                { "min",  stats.Minimum },
                                { "max",  stats.Maximum },
                                { "mode", stats.Mode },
                                { "sd",   stats.StandardDeviation},
                                { IndexDistributions.UPPER_PERCENTILE_LABEL,  value},
                                { "count",  stats.Count},
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


        public static SpectralStats GetModeAndOneTailedStandardDeviation(double[,] M)
        {
            int binCount = 100; 
            int upperPercentile = 0;
            double[] values = DataTools.Matrix2Array(M);
            const bool DisplayHistogram = false;
            double min, max, mode, SD;
            DataTools.GetModeAndOneTailedStandardDeviation(values, DisplayHistogram, out min, out max, out mode, out SD);
            int[] histogram = Histogram.Histo(M, binCount);

            return new SpectralStats()
            {
                Minimum = min,
                Maximum = max,
                Mode = mode,
                StandardDeviation = SD,
                UpperPercentile = upperPercentile,
                Distribution = histogram
            };
        }

        public static SpectralStats GetModeAndOneTailedStandardDeviation(double[,] M, int binCount, int upperPercentile)
        {
            double[] values = DataTools.Matrix2Array(M);
            return GetModeAndOneTailedStandardDeviation(values, binCount, upperPercentile);
        }

        public static SpectralStats GetModeAndOneTailedStandardDeviation(double[] values, int binCount, int upperPercentile)
        {
            const bool DisplayHistogram = false;
            double min, max, mode, SD;
            DataTools.GetModeAndOneTailedStandardDeviation(values, DisplayHistogram, out min, out max, out mode, out SD);
            int[] histogram = Histogram.Histo(values, binCount);

            return new SpectralStats()
            {
                Minimum = min,
                Maximum = max,
                Mode = mode,
                StandardDeviation = SD,
                UpperPercentile = upperPercentile,
                Distribution = histogram,
                Count = values.Length
            };
        }

        public static string GetSummaryStatsPath(DirectoryInfo outputDirectory, string fileStem)
        {
            return FilenameHelpers.AnalysisResultName(outputDirectory, fileStem, SummaryIndexStatisticsFilenameFragment, "json");
        }

        public static string GetSummaryImagePath(DirectoryInfo outputDirectory, string fileStem)
        {
            return FilenameHelpers.AnalysisResultName(outputDirectory, fileStem, SummaryIndexDistributionsFilenameFragment, "png");
        }

        public static string GetSpectralStatsPath(DirectoryInfo outputDirectory, string fileStem)
        {
            return FilenameHelpers.AnalysisResultName(outputDirectory, fileStem, SpectralIndexStatisticsFilenameFragment, "json");
        }

        public static string GetSpectralImagePath(DirectoryInfo outputDirectory, string fileStem)
        {
            return FilenameHelpers.AnalysisResultName(outputDirectory, fileStem, SpectralIndexDistributionsFilenameFragment, "png");
        }

        public static Dictionary<string, SpectralStats> Deserialize(FileInfo file)
        {
            return Json.Deserialise<Dictionary<string, SpectralStats>>(file);
        }


    }
}
