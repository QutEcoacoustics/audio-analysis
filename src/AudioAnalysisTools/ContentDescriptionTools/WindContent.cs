using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using System.IO;
    using TowseyLibrary;

    public static class WindContent
    {
        private const int ReductionFactor = 16;

        private static Dictionary<string, double[]> StrongWindTemplate = new Dictionary<string, double[]>
        {
            ["ACI"] = new[] { 0.086, 0.043, 0.041, 0.023, 0.032, 0.027, 0.029, 0.031, 0.032, 0.032, 0.034, 0.069, 0.033, 0.024, 0.018, 0.018 },
            ["ENT"] = new[] { 0.124, 0.112, 0.146, 0.163, 0.157, 0.157, 0.143, 0.122, 0.113, 0.095, 0.087, 0.121, 0.075, 0.060, 0.054, 0.067 },
            ["EVN"] = new[] { 0.376, 0.440, 0.590, 0.621, 0.648, 0.621, 0.565, 0.363, 0.273, 0.191, 0.164, 0.221, 0.104, 0.040, 0.017, 0.032 },
            ["BGN"] = new[] { 0.472, 0.360, 0.273, 0.199, 0.156, 0.121, 0.096, 0.085, 0.075, 0.069, 0.064, 0.061, 0.060, 0.058, 0.054, 0.026 },
            ["PMN"] = new[] { 0.468, 0.507, 0.687, 0.743, 0.757, 0.751, 0.665, 0.478, 0.391, 0.317, 0.276, 0.367, 0.187, 0.109, 0.071, 0.096 },
        };

        public static KeyValuePair<string, double> GetStrongWindContent(Dictionary<string, double[]> oneMinuteOfIndices)
        {
            const string name = "StrongWind1";

            var reducedIndices = ContentDescription.ReduceIndicesByFactor(oneMinuteOfIndices, ReductionFactor);
            var oneMinuteVector = ContentDescription.ConvertDictionaryToVector(reducedIndices);
            var templateVector = ContentDescription.ConvertDictionaryToVector(StrongWindTemplate);

            var distance = DataTools.EuclidianDistance(templateVector, oneMinuteVector);

            //normalise the distance
            distance /= Math.Sqrt(templateVector.Length);

            // get dummy data
            //var rn = new RandomNumber(DateTime.Now.Second + (int)DateTime.Now.Ticks + 333);
            //var distance = rn.GetDouble();

            return new KeyValuePair<string, double>(name, 1 - distance);
        }

        public static Dictionary<string, double[]> GetStrongWindTemplate(Dictionary<string, double[,]> dictionaryOfIndices)
        {
            var windIndices = ContentDescription.AverageIndicesOverMinutes(dictionaryOfIndices, 23, 27);
            var reducedIndices = ContentDescription.ReduceIndicesByFactor(windIndices, ReductionFactor);
            return reducedIndices;
        }

        public static void WriteStrongWindTemplateToFile(Dictionary<string, double[,]> dictionaryOfIndices, string path)
        {
            var template = WindContent.GetStrongWindTemplate(dictionaryOfIndices);
            FileTools.WriteDictionaryToFile(template, path);
        }

        // #######################################################################################################################

        public static KeyValuePair<string, double> GetLightWindContent(Dictionary<string, double[]> oneMinuteOfIndices)
        {
            const string name = "LightWind1";
            var rn = new RandomNumber(DateTime.Now.Second + DateTime.Now.Millisecond + 40);
            var score = rn.GetDouble();
            return new KeyValuePair<string, double>(name, score);
        }
    }
}
