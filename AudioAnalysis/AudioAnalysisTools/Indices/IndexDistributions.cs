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

    public static class IndexDistributions
    {

        public class SpectralStats
        {
            public double Minimum { get; set; }

            public double Maximum { get; set; }

            public double Mode { get; set; }

            public double StandardDeviation { get; set; }
        }

        public static Dictionary<string, SpectralStats> CalculateStatisticsForAllIndices(Dictionary<string, double[,]> spectrogramMatrices)
        {
            double[,] matrix;
            Dictionary<string, SpectralStats> indexStats = new Dictionary<string, SpectralStats>();

            string[] spectrogramKeys = spectrogramMatrices.Keys.ToArray();
            foreach (string key in spectrogramKeys)
            {
                if (spectrogramMatrices.ContainsKey(key))
                {
                    matrix = spectrogramMatrices[key];
                    SpectralStats stats = GetModeAndOneTailedStandardDeviation(matrix);
                    indexStats.Add(key, stats); // add index statistics
                }
            }
            return indexStats;
        }



        public static SpectralStats GetModeAndOneTailedStandardDeviation(double[,] M)
        {
            double[] values = DataTools.Matrix2Array(M);
            const bool DisplayHistogram = false;
            double min, max, mode, SD;
            DataTools.GetModeAndOneTailedStandardDeviation(values, DisplayHistogram, out min, out max, out mode, out SD);

            return new SpectralStats()
            {
                Minimum = min,
                Maximum = max,
                Mode = mode,
                StandardDeviation = SD
            };
        }


        /* public List<string> WriteStatisticsForAllIndices()
        {
           List<string> lines = new List<string>();
            foreach (string key in this.spectrogramKeys)
            {
                if (this.spectrogramMatrices.ContainsKey(key))
                {
                    string outString = "STATS for " + key + ":   ";
                    Dictionary<string, double> stats = this.GetIndexStatistics(key);
                    foreach (string stat in stats.Keys)
                    {
                        outString = string.Format("{0}  {1}={2:f3} ", outString, stat, stats[stat]);
                    }
                    lines.Add(outString);
                }
            }
            return lines;

        }*/

        public static void DrawIndexDistributionsAndSave(Dictionary<string, double[,]> spectrogramMatrices, string imagePath)
        {
            int width = 100;  // pixels 
            int height = 100; // pixels
            var list = new List<Image>();
            double[,] matrix;
            string[] spectrogramKeys = spectrogramMatrices.Keys.ToArray();
            foreach (string key in spectrogramMatrices.Keys)
            {
                // used to save mode and sd of the indices 
                matrix = spectrogramMatrices[key];
                SpectralStats stats = GetModeAndOneTailedStandardDeviation(matrix);
                int[] histogram = Histogram.Histo(spectrogramMatrices[key], width);
                list.Add(
                    ImageTools.DrawHistogram(
                        key,
                        histogram,
                        new Dictionary<string, double>()
                            {
                                { "min", stats.Minimum },
                                { "max", stats.Maximum },
                                { "mode", stats.Mode },
                                { "sd", stats.StandardDeviation },
                            },
                        width,
                        height));
            }

            Image image3 = ImageTools.CombineImagesVertically(list.ToArray());
            image3.Save(imagePath);
        }




    }
}
