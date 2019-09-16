// <copyright file="BaseContentType.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools.ContentTypes
{
    using System.Collections.Generic;
    using System.IO;
    using TowseyLibrary;

    public abstract class BaseContentType
    {
        //TEMPLATE DESCRIPTION
        // Name of the template
        public const string Name = "UnknownContentType";

        // The TEMPLATE PROVENANCE
        // The source file name from which the indices are extracted.
        private const string BaseName = "BaseNameOfFile";
        private const string Location = "PlaceName";

        // Template date and Time could also be included where these cannot be inferred from the file name.

        //THESE ARE SPECIFIC ROW BOUNDS FOR PREPARING THIS TEMPLATE
        // Typically each freq bin will be averaged over the time period.
        private const int StartRowId = 0;
        private const int EndRowId = 59;

        // Full array (256 freq bins) of spectral indices is reduced by the following factor by averaging. This is to reduce correlation and computation.
        private const int ReductionFactor = 16;

        // Bandpass filter to be applied where the target content exists only within a narrow band, e.g. 3-4 kHz for Silver-eye band.
        private const int FreqBinCount = 256 / ReductionFactor;
        private const int BottomFreq = 0; //Hertz
        private const int TopFreq = 11000; //Hertz

        // The all important template that is used to find an acoustic content type.
        // These are calculate, written to a csv file and then appropriate parts are copied into a dictionary declaration such as this.
        // The arrays will all be of same length but will vary from length = 1 to 16 or potentially 256.
        private static readonly Dictionary<string, double[]> Template = new Dictionary<string, double[]>
        {
            ["ACI"] = new[] { 0.086, 0.043, 0.041, 0.023, 0.032, 0.027, 0.029, 0.031, 0.032, 0.032, 0.034, 0.069, 0.033, 0.024, 0.018, 0.018 },
            ["ENT"] = new[] { 0.124, 0.112, 0.146, 0.163, 0.157, 0.157, 0.143, 0.122, 0.113, 0.095, 0.087, 0.121, 0.075, 0.060, 0.054, 0.067 },
            ["EVN"] = new[] { 0.376, 0.440, 0.590, 0.621, 0.648, 0.621, 0.565, 0.363, 0.273, 0.191, 0.164, 0.221, 0.104, 0.040, 0.017, 0.032 },
            ["BGN"] = new[] { 0.472, 0.360, 0.273, 0.199, 0.156, 0.121, 0.096, 0.085, 0.075, 0.069, 0.064, 0.061, 0.060, 0.058, 0.054, 0.026 },
            ["PMN"] = new[] { 0.468, 0.507, 0.687, 0.743, 0.757, 0.751, 0.665, 0.478, 0.391, 0.317, 0.276, 0.367, 0.187, 0.109, 0.071, 0.096 },
        };

        /// <summary>
        /// THis method changes for each content type.
        /// </summary>
        /// <param name="oneMinuteOfIndices">the indices for this minute.</param>
        /// <returns>A score value for the content in this one minute of recording.</returns>
        public static KeyValuePair<string, double> GetContent(Dictionary<string, double[]> oneMinuteOfIndices)
        {
            double score = 0.0;
            return new KeyValuePair<string, double>(Name, score);
        }

        /// <summary>
        /// THis method is the same for all Content Types but uses constants appropriate the template type.
        /// </summary>
        public static Dictionary<string, double[]> GetTemplate(DirectoryInfo dir)
        {
            var dictionaryOfIndices = DataProcessing.ReadIndexMatrices(dir, BaseName);
            var birdIndices = DataProcessing.AverageIndicesOverMinutes(dictionaryOfIndices, StartRowId, EndRowId);
            var reducedIndices = DataProcessing.ReduceIndicesByFactor(birdIndices, ReductionFactor);
            var freqBinBounds = DataProcessing.GetFreqBinBounds(BottomFreq, TopFreq, FreqBinCount);
            reducedIndices = DataProcessing.ApplyBandPass(reducedIndices, freqBinBounds[0], freqBinBounds[1]);
            return reducedIndices;
        }

        /// <summary>
        /// THis method is the same for all Content Types.
        /// </summary>
        public static void WriteTemplateToFile(DirectoryInfo ipDir, DirectoryInfo opDir)
        {
            var template = GetTemplate(ipDir);
            var opPath = Path.Combine(opDir.FullName, Name + "Template.csv");
            FileTools.WriteDictionaryToFile(template, opPath);
        }

        // The following random data was used to try some statistical experiments.
        // get dummy data
        //var rn = new RandomNumber(DateTime.Now.Second + (int)DateTime.Now.Ticks + 333);
        //var distance = rn.GetDouble();
    }
}
