// <copyright file="WindLight1.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools.ContentTypes
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using TowseyLibrary;

    public class WindLight1
    {
        public const string Name = "LightWind1";
        private const int ReductionFactor = 16;

        private static Dictionary<string, double[]> LightWindTemplate = new Dictionary<string, double[]>
        {
            ["ACI"] = new[] { 0.072, 0.035, 0.015, 0.008, 0.008, 0.009, 0.016, 0.018, 0.017, 0.015, 0.020, 0.022, 0.029, 0.026, 0.027, 0.098 },
            ["ENT"] = new[] { 0.063, 0.069, 0.071, 0.059, 0.052, 0.051, 0.050, 0.050, 0.050, 0.050, 0.050, 0.052, 0.051, 0.050, 0.051, 0.058 },
            ["EVN"] = new[] { 0.160, 0.099, 0.077, 0.023, 0.009, 0.009, 0.005, 0.004, 0.003, 0.004, 0.003, 0.006, 0.002, 0.004, 0.003, 0.015 },
            ["BGN"] = new[] { 0.387, 0.257, 0.151, 0.094, 0.069, 0.055, 0.049, 0.045, 0.042, 0.041, 0.041, 0.042, 0.043, 0.044, 0.043, 0.017 },
            ["PMN"] = new[] { 0.228, 0.199, 0.179, 0.087, 0.065, 0.048, 0.042, 0.034, 0.035, 0.035, 0.036, 0.038, 0.038, 0.037, 0.044, 0.081 },
        };

        public static KeyValuePair<string, double> GetContent(Dictionary<string, double[]> oneMinuteOfIndices)
        {
            var reducedIndices = ContentDescription.ReduceIndicesByFactor(oneMinuteOfIndices, ReductionFactor);
            var oneMinuteVector = ContentDescription.ConvertDictionaryToVector(reducedIndices);
            var templateVector = ContentDescription.ConvertDictionaryToVector(LightWindTemplate);

            var distance = DataTools.EuclidianDistance(templateVector, oneMinuteVector);

            //normalise the distance
            distance /= Math.Sqrt(templateVector.Length);
            return new KeyValuePair<string, double>(Name, 1 - distance);
        }

        public static Dictionary<string, double[]> GetTemplate(DirectoryInfo dir)
        {
        //THESE ARE SPECIFIC BOUNDS FOR PREPARING THIS TEMPLATE
            int startRowId = 50;
            int endRowId = 53;
            string baseName = "SM304256_0+1_20151114_021652";
            var dictionaryOfIndices = ContentDescription.ReadIndexMatrices(dir, baseName);
            var windIndices = ContentDescription.AverageIndicesOverMinutes(dictionaryOfIndices, startRowId, endRowId);
            var reducedIndices = ContentDescription.ReduceIndicesByFactor(windIndices, ReductionFactor);
            return reducedIndices;
        }

        public static void WriteTemplateToFile(DirectoryInfo ipDir, DirectoryInfo opDir)
        {
            var template = GetTemplate(ipDir);
            var opPath = Path.Combine(opDir.FullName, "LightWindTemplate1.csv");
            FileTools.WriteDictionaryToFile(template, opPath);
        }
    }
}
