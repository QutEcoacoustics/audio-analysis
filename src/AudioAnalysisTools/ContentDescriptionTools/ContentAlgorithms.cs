// <copyright file="ContentAlgorithms.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using TowseyLibrary;

    public static class ContentAlgorithms
    {
        public static double GetContent1(Dictionary<string, double[]> oneMinuteOfIndices, ContentTemplate template, Dictionary<string, double[]> templateIndices)
        {
            var reductionFactor = template.SpectralReductionFactor;
            var reducedIndices = DataProcessing.ReduceIndicesByFactor(oneMinuteOfIndices, reductionFactor);
            var oneMinuteVector = DataProcessing.ConvertDictionaryToVector(reducedIndices);
            var templateVector = DataProcessing.ConvertDictionaryToVector(templateIndices);

            var distance = DataTools.EuclideanDistance(templateVector, oneMinuteVector);

            //normalise the distance
            distance /= Math.Sqrt(templateVector.Length);
            return 1 - distance;
        }
    }
}
