// <copyright file="WindStrong1.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools.ContentTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using TowseyLibrary;

    public abstract class BaseContentType
    {
        public const int ReductionFactor = 16;

        public virtual string Name() => "Something";

        public static Dictionary<string, double[]> GetTemplate(Dictionary<string, double[,]> dictionaryOfIndices)
        {
            var windIndices = ContentDescription.AverageIndicesOverMinutes(dictionaryOfIndices, 23, 27);
            var reducedIndices = ContentDescription.ReduceIndicesByFactor(windIndices, ReductionFactor);
            return reducedIndices;
        }

        public static void WriteTemplateToFile(Dictionary<string, double[,]> dictionaryOfIndices, string path)
        {
            var template = GetTemplate(dictionaryOfIndices);
            FileTools.WriteDictionaryToFile(template, path);
        }

        // get dummy data
        //var rn = new RandomNumber(DateTime.Now.Second + (int)DateTime.Now.Ticks + 333);
        //var distance = rn.GetDouble();
    }
}
