// <copyright file="SilverEyeMezTasmanIs.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools.ContentTypes
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using TowseyLibrary;

    public class SilverEyeMezTasmanIs
    {
        //TEMPLATE DESCRIPTION
        // Name of the template
        public const string Name = "SilverEye_TasmanIs";

        // The TEMPLATE PROVENANCE
        // The source file name from which the indices are extracted.
        private const string BaseName = "SM304256_0+1_20151114_071652";

        //THESE ARE SPECIFIC ROW BOUNDS FOR PREPARING THIS TEMPLATE
        // The freq bins will be averaged over the time period.
        private const int StartRowId = 6;
        private const int EndRowId = 11;

        // Full array (256 freq bins) of spectral indices is reduced by the following factor by averaging.
        private const int ReductionFactor = 16;

        // Bandpass filter to be applied
        private const int FreqBinCount = 256 / ReductionFactor;
        private const int BottomFreq = 3000; //Hertz
        private const int TopFreq = 4000; //Hertz

        // Only want the interval 3-4 kHz for Silver-eye band.
        // After reducing 256 freq bins to 16, each bin has width 689Hz.
        // Therefore to get band 3-4 kHz, need to remove the bottom and top bins.
        // This leaves a template with 2 or 3 freq bins which are then averaged, so that each index has one value.
        // At the present time this editing is done manually.

        private static readonly Dictionary<string, double[]> SilverEyeTemplate = new Dictionary<string, double[]>
        {
            ["ACI"] = new[] { 0.779 },
            ["ENT"] = new[] { 0.393 },
            ["EVN"] = new[] { 0.686 },
            ["BGN"] = new[] { 0.085 },
            ["PMN"] = new[] { 0.883 },
        };

        public static KeyValuePair<string, double> GetContent(Dictionary<string, double[]> oneMinuteOfIndices)
        {
            var reducedIndices = DataProcessing.ReduceIndicesByFactor(oneMinuteOfIndices, ReductionFactor);

            //var freqBinBounds = DataProcessing.GetFreqBinBounds(BottomFreq, TopFreq);
            //reducedIndices = DataProcessing.ApplyBandPass(reducedIndices, freqBinBounds[0], freqBinBounds[1]);
            //var oneMinuteVector = DataProcessing.ConvertDictionaryToVector(reducedIndices);
            //var templateVector = DataProcessing.ConvertDictionaryToVector(SilverEyeTemplate);

            //Get Euclidian distance and normalize the distance
            // Now pass the template up the full frequency spectrum to get a spectrum of scores.
            var spectralScores = DataProcessing.ScanSpectrumWithTemplate(SilverEyeTemplate, reducedIndices);

            // Now check how much of spectral weight is in the correct freq band ie between 3-4 kHz.
            var freqBinBounds = DataProcessing.GetFreqBinBounds(BottomFreq, TopFreq, FreqBinCount);
            double callSum = DataTools.Subarray(spectralScores, freqBinBounds[0], freqBinBounds[1]).Sum();
            double totalSum = DataTools.Subarray(spectralScores, 1, spectralScores.Length - 3).Sum();
            double score = callSum / totalSum;

            return new KeyValuePair<string, double>(Name, score);
        }

        //public static Dictionary<string, double[]> GetTemplate(DirectoryInfo dir)
        //{
        //    var dictionaryOfIndices = DataProcessing.ReadIndexMatrices(dir, BaseName);
        //    var birdIndices = DataProcessing.AverageIndicesOverMinutes(dictionaryOfIndices, StartRowId, EndRowId);
        //    var reducedIndices = DataProcessing.ReduceIndicesByFactor(birdIndices, ReductionFactor);
        //    var freqBinBounds = DataProcessing.GetFreqBinBounds(BottomFreq, TopFreq, FreqBinCount);
        //    reducedIndices = DataProcessing.ApplyBandPass(reducedIndices, freqBinBounds[0], freqBinBounds[1]);
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
