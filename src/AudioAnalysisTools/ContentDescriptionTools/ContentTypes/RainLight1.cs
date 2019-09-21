// <copyright file="RainLight1.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools.ContentTypes
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using TowseyLibrary;

    public class RainLight1
    {
        //TEMPLATE DESCRIPTION
        // Name of the template
        public const string Name = "LightRain1";

        // The TEMPLATE PROVENANCE
        // The source file name from which the indices are extracted.
        private const string BaseName = "SM304256_0+1_20151114_071652";

        //THESE ARE SPECIFIC ROW BOUNDS FOR PREPARING THIS TEMPLATE
        // The freq bins will be averaged over the time period.
        private const int StartRowId = 32;
        private const int EndRowId = 36;

        // Full array (256 freq bins) of spectral indices is reduced by the following factor by averaging.
        private const int ReductionFactor = 16;

        private static readonly Dictionary<string, double[]> LightRainTemplate = new Dictionary<string, double[]>
        {
            ["ACI"] = new[] { 0.076, 0.046, 0.167, 0.360, 0.426, 0.443, 0.545, 0.595, 0.564, 0.612, 0.659, 0.570, 0.542, 0.520, 0.485, 0.485 },
            ["ENT"] = new[] { 0.065, 0.061, 0.176, 0.289, 0.249, 0.255, 0.296, 0.292, 0.262, 0.386, 0.462, 0.262, 0.222, 0.243, 0.217, 0.205 },
            ["EVN"] = new[] { 0.136, 0.009, 0.022, 0.051, 0.072, 0.092, 0.109, 0.150, 0.175, 0.176, 0.193, 0.155, 0.171, 0.135, 0.109, 0.133 },
            ["BGN"] = new[] { 0.366, 0.249, 0.181, 0.148, 0.122, 0.111, 0.106, 0.105, 0.104, 0.111, 0.111, 0.111, 0.105, 0.100, 0.090, 0.048 },
            ["PMN"] = new[] { 0.182, 0.076, 0.243, 0.459, 0.470, 0.501, 0.592, 0.651, 0.625, 0.699, 0.792, 0.599, 0.572, 0.550, 0.490, 0.488 },
        };

        public static KeyValuePair<string, double> GetContent(Dictionary<string, double[]> oneMinuteOfIndices)
        {
            var reducedIndices = DataProcessing.ReduceIndicesByFactor(oneMinuteOfIndices, ReductionFactor);
            var oneMinuteVector = DataProcessing.ConvertDictionaryToVector(reducedIndices);
            var templateVector = DataProcessing.ConvertDictionaryToVector(LightRainTemplate);

            //Get Euclidian distance and normalise the distance
            var distance = DataTools.EuclideanDistance(templateVector, oneMinuteVector);
            distance /= Math.Sqrt(templateVector.Length);

            // get dummy data
            //var rn = new RandomNumber(DateTime.Now.Second + (int)DateTime.Now.Ticks + 333);
            //var distance = rn.GetDouble();

            return new KeyValuePair<string, double>(Name, 1 - distance);
        }

        ///// <summary>
        ///// string baseName = "SM304256_0+1_20151114_071652".
        ///// </summary>
        //public static Dictionary<string, double[]> GetTemplate(DirectoryInfo dir)
        //{
        //    var dictionaryOfIndices = DataProcessing.ReadIndexMatrices(dir, BaseName);
        //    var windIndices = DataProcessing.AverageIndicesOverMinutes(dictionaryOfIndices, StartRowId, EndRowId);
        //    var reducedIndices = DataProcessing.ReduceIndicesByFactor(windIndices, ReductionFactor);
        //    return reducedIndices;
        //}

        //public static void WriteTemplateToFile(DirectoryInfo ipDir, DirectoryInfo opDir)
        //{
        //    var template = GetTemplate(ipDir);
        //    var opPath = Path.Combine(opDir.FullName, Name + "Template.csv");
        //    FileTools.WriteDictionaryToFile(template, opPath);
        //}
    }
}
