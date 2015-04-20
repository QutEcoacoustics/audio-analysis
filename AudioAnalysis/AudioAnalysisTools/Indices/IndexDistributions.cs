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

        public class SpectralStats
        {
            public double Minimum { get; set; }

            public double Maximum { get; set; }

            public double Mode { get; set; }

            public double StandardDeviation { get; set; }

            public int UpperPercentile { get; set; }

            public int UpperPercentileBin { get; set; }

            public int[] Distribution { get; set; }

            public double GetValueOfThresholdPercentile()
            {
                return this.GetValueOfNthPercentile(this.UpperPercentile);
            }
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

        public static Dictionary<string, SpectralStats> ReadIndexDistributionStatistics(DirectoryInfo opDir, string fileStem)
        {           
            FileInfo statsFile = new FileInfo(GetStatsPath(opDir, fileStem));
            if (! statsFile.Exists) 
                return null;
            var indexDistributionStatistics = Json.Deserialise<Dictionary<string, SpectralStats>>(statsFile);
            return indexDistributionStatistics;
        }

        public static Dictionary<string, SpectralStats> WriteIndexDistributionStatistics(Dictionary<string, double[,]> spectrogramMatrices, DirectoryInfo opDir, string fileStem)
        {
            // this sets the upper normalisation bound for image colour of spectral indices - derived from index distribution.
            int upperPercentile = 99;
            string label = "99%";

            // to accumulate the images
            int width = 100;  // pixels 
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
                    SpectralStats stats = GetModeAndOneTailedStandardDeviation(matrix, width, upperPercentile);
                    indexDistributionStatistics.Add(key, stats); // add index statistics
                    double value = stats.GetValueOfThresholdPercentile();

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
                                { label,  value},
                            },
                            width,
                            height));
                }
            }

            FileInfo statsFile = new FileInfo(GetStatsPath(opDir, fileStem));
            Json.Serialise(statsFile, indexDistributionStatistics);

            Image image3 = ImageTools.CombineImagesVertically(imageList.ToArray());
            string imagePath = GetImagePath(opDir, fileStem);
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

        public static string GetStatsPath(DirectoryInfo opDir, string fileStem)
        {
            string imagePath = Path.Combine(opDir.FullName, fileStem + ".IndexStatistics.json");
            return imagePath;
        }

        public static string GetImagePath(DirectoryInfo opDir, string fileStem)
        {
            string imagePath = Path.Combine(opDir.FullName, fileStem + ".IndexDistributions.png");
            return imagePath;
        }



    }
}
