// <copyright file="BirdMorningChorus1.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools.ContentTypes
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using TowseyLibrary;

    public class BirdMorningChorus1
    {
        //TEMPLATE DESCRIPTION
        // Name of the template
        public const string Name = "BirdMorningChorus1";

        // The TEMPLATE PROVENANCE
        // The source file name from which the indices are extracted.
        private const string BaseName = "SM304256_0+1_20151114_041652";

        //THESE ARE SPECIFIC ROW BOUNDS FOR PREPARING THIS TEMPLATE
        // The freq bins will be averaged over the time period.
        private const int StartRowId = 47;
        private const int EndRowId = 53;

        // Full array (256 freq bins) of spectral indices is reduced by the following factor by averaging.
        private const int ReductionFactor = 16;

        // Only want the interval 2-8 kHz for bird morning chorus.
        // After reducing 256 freq bins to 16, each bin has width 689Hz.
        // Therefore to get band 2-8 kHz, need to remove the bottom two freq bins and the top four freq bins.
        // This leaves a template with 10 freq bins.
        // At the present time this editing is done manually.

        private static readonly Dictionary<string, double[]> BirdChorusTemplate = new Dictionary<string, double[]>
        {
            ["ACI"] = new[] { 0.274, 0.366, 0.591, 0.820, 0.997, 0.975, 0.796, 0.846, 0.605, 0.260 },
            ["ENT"] = new[] { 0.293, 0.415, 0.804, 0.972, 0.910, 0.876, 0.923, 0.971, 0.840, 0.491 },
            ["EVN"] = new[] { 0.445, 0.691, 0.291, 0.266, 0.407, 0.417, 0.306, 0.321, 0.199, 0.091 },
            ["BGN"] = new[] { 0.140, 0.099, 0.072, 0.059, 0.055, 0.051, 0.048, 0.048, 0.045, 0.042 },
            ["PMN"] = new[] { 0.671, 0.967, 0.924, 0.998, 1.000, 1.000, 0.998, 1.000, 0.952, 0.633 },
        };

        public static KeyValuePair<string, double> GetContent(Dictionary<string, double[]> oneMinuteOfIndices)
        {
            var reducedIndices = ContentDescription.ReduceIndicesByFactor(oneMinuteOfIndices, ReductionFactor);

            // remove first two freq bins and last four freq bins
            int bottomBin = 2;
            int topBin = 11;
            reducedIndices = ContentDescription.ApplyBandPass(reducedIndices, bottomBin, topBin);

            var oneMinuteVector = ContentDescription.ConvertDictionaryToVector(reducedIndices);
            var templateVector = ContentDescription.ConvertDictionaryToVector(BirdChorusTemplate);

            //Get Euclidian distance and normalize the distance
            var distance = DataTools.EuclideanDistance(templateVector, oneMinuteVector);
            distance /= Math.Sqrt(templateVector.Length);

            return new KeyValuePair<string, double>(Name, 1 - distance);
        }

        public static Dictionary<string, double[]> GetTemplate(DirectoryInfo dir)
        {
            var dictionaryOfIndices = ContentDescription.ReadIndexMatrices(dir, BaseName);
            var birdIndices = ContentDescription.AverageIndicesOverMinutes(dictionaryOfIndices, StartRowId, EndRowId);
            var reducedIndices = ContentDescription.ReduceIndicesByFactor(birdIndices, ReductionFactor);
            return reducedIndices;
        }

        public static void WriteTemplateToFile(DirectoryInfo ipDir, DirectoryInfo opDir)
        {
            var template = GetTemplate(ipDir);
            var opPath = Path.Combine(opDir.FullName, Name + "Template.csv");
            FileTools.WriteDictionaryToFile(template, opPath);
        }
    }
}
