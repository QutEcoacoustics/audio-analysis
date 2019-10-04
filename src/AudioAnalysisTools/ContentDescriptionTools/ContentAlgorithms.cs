// <copyright file="ContentAlgorithms.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TowseyLibrary;

    public static class ContentAlgorithms
    {
        /// <summary>
        /// This algorithm is used for full band width events such as a rain and wind.
        /// It calculates a content score based on a template match to what is in the full spectrum.
        /// </summary>
        /// <param name="manifest">A description of the template which is to be created.</param>
        /// <param name="templateIndices">The actual dictionary of template arrays.</param>
        /// <returns>A new template.</returns>
        public static Dictionary<string, double[]> CreateFullBandTemplate1(TemplateManifest manifest, Dictionary<string, double[,]> templateIndices)
        {
            // Get the template provenance. Assume array contains only one element.
            var provenanceArray = manifest.Provenance;
            var provenance = provenanceArray[0];
            var startRowId = provenance.StartOffset;
            var endRowId = provenance.EndOffset;

            var dictionaryOfVector = DataProcessing.AverageIndicesOverMinutes(templateIndices, startRowId, endRowId);
            var reducedIndices = DataProcessing.ReduceIndicesByFactor(dictionaryOfVector, manifest.SpectralReductionFactor);

            return reducedIndices;
        }

        /// <summary>
        /// This algorithm is used for full band width events such as a rain and wind.
        /// It calculates a content score based on a template match to what is in the full spectrum.
        /// </summary>
        /// <param name="oneMinuteOfIndices">Derived from the source recording.</param>
        /// <param name="template">A previously prepared template.</param>
        /// <param name="templateIndices">The actual dictionary of template arrays.</param>
        /// <returns>A similarity score.</returns>
        public static double GetFullBandContent1(Dictionary<string, double[]> oneMinuteOfIndices, TemplateManifest template, Dictionary<string, double[]> templateIndices)
        {
            var reducedIndices = DataProcessing.ReduceIndicesByFactor(oneMinuteOfIndices, template.SpectralReductionFactor);
            var oneMinuteVector = DataProcessing.ConvertDictionaryToVector(reducedIndices);
            var templateVector = DataProcessing.ConvertDictionaryToVector(templateIndices);

            var distance = DataTools.EuclideanDistance(templateVector, oneMinuteVector);

            // Normalize the distance
            distance /= Math.Sqrt(templateVector.Length);
            return 1 - distance;
        }

        // ###################################################################################

        /// <summary>
        /// This algorithm is used for broad band events such as a bird chorus.
        /// It selects acoustic content over a band of several kHz and calculates a content score based on a template match to what is in the band.
        /// </summary>
        /// <param name="manifest">A previously prepared template.</param>
        /// <param name="templateIndices">The actual dictionary of template arrays.</param>
        /// <returns>A similarity score.</returns>
        public static Dictionary<string, double[]> CreateBroadbandTemplate1(TemplateManifest manifest, Dictionary<string, double[,]> templateIndices)
        {
            // Get the template provenance. Assume array contains only one element.
            var provenanceArray = manifest.Provenance;
            var provenance = provenanceArray[0];
            var startRowId = provenance.StartOffset;
            var endRowId = provenance.EndOffset;

            var reductionFactor = manifest.SpectralReductionFactor;
            var dictionaryOfVector = DataProcessing.AverageIndicesOverMinutes(templateIndices, startRowId, endRowId);

            // remove first two freq bins and last four freq bins, i.e. bottomBin = 2 and topBin = 11;
            int freqBinCount = ContentDescription.FreqBinCount / reductionFactor;
            int bottomFreq = manifest.BandMinHz; //Hertz
            int topFreq = manifest.BandMaxHz; //Hertz
            var freqBinBounds = DataProcessing.GetFreqBinBounds(bottomFreq, topFreq, freqBinCount);
            var reducedIndices = DataProcessing.ReduceIndicesByFactor(dictionaryOfVector, reductionFactor);
            reducedIndices = DataProcessing.ApplyBandPass(reducedIndices, freqBinBounds[0], freqBinBounds[1]);
            return reducedIndices;
        }

        /// <summary>
        /// This algorithm is used for broad band events such as a bird chorus.
        /// It selects acoustic content over a band of several kHz and calculates a content score based on a template match to what is in the band.
        /// </summary>
        /// <param name="oneMinuteOfIndices">Derived from the source recording.</param>
        /// <param name="template">A previously prepared template.</param>
        /// <param name="templateIndices">The actual dictionary of template arrays.</param>
        /// <returns>A similarity score.</returns>
        public static double GetBroadbandContent1(Dictionary<string, double[]> oneMinuteOfIndices, TemplateManifest template, Dictionary<string, double[]> templateIndices)
        {
            var reductionFactor = template.SpectralReductionFactor;
            int freqBinCount = ContentDescription.FreqBinCount / reductionFactor;
            int bottomFreq = template.BandMinHz; //Hertz
            int topFreq = template.BandMaxHz; //Hertz
            var freqBinBounds = DataProcessing.GetFreqBinBounds(bottomFreq, topFreq, freqBinCount);
            var reducedIndices = DataProcessing.ReduceIndicesByFactor(oneMinuteOfIndices, reductionFactor);

            // remove top freq bins and bottom freq bins;
            reducedIndices = DataProcessing.ApplyBandPass(reducedIndices, freqBinBounds[0], freqBinBounds[1]);
            var oneMinuteVector = DataProcessing.ConvertDictionaryToVector(reducedIndices);
            var templateVector = DataProcessing.ConvertDictionaryToVector(templateIndices);

            //Get Euclidean distance and normalize the distance
            var distance = DataTools.EuclideanDistance(templateVector, oneMinuteVector);

            // Normalize the distance
            distance /= Math.Sqrt(templateVector.Length);
            return 1 - distance;
        }

        // ###################################################################################

        /// <summary>
        /// This algorithm is used for narrow band events such as an insect bird chorus or content due to narrow band calls of a single bird species.
        /// It searches the full spectrum for a match to the template and then
        ///  calculates how much of the match weight is in the correct narrow freq band.
        /// </summary>
        /// <param name="manifest">A previously prepared template.</param>
        /// <param name="templateIndices">The actual dictionary of template arrays.</param>
        /// <returns>A similarity score.</returns>
        public static Dictionary<string, double[]> CreateNarrowBandTemplate1(TemplateManifest manifest, Dictionary<string, double[,]> templateIndices)
        {
            // Get the template provenance. Assume array contains only one element.
            var provenanceArray = manifest.Provenance;
            var provenance = provenanceArray[0];
            var startRowId = provenance.StartOffset;
            var endRowId = provenance.EndOffset;

            var reductionFactor = manifest.SpectralReductionFactor;
            var dictionaryOfVector = DataProcessing.AverageIndicesOverMinutes(templateIndices, startRowId, endRowId);

            // remove first two freq bins and last four freq bins, i.e. bottomBin = 2 and topBin = 11;
            int freqBinCount = ContentDescription.FreqBinCount / reductionFactor;
            int bottomFreq = manifest.BandMinHz; //Hertz
            int topFreq = manifest.BandMaxHz; //Hertz
            var freqBinBounds = DataProcessing.GetFreqBinBounds(bottomFreq, topFreq, freqBinCount);
            var reducedIndices = DataProcessing.ReduceIndicesByFactor(dictionaryOfVector, reductionFactor);
            reducedIndices = DataProcessing.ApplyBandPass(reducedIndices, freqBinBounds[0], freqBinBounds[1]);
            return reducedIndices;
        }

        /// <summary>
        /// This algorithm is used for narrow band events such as an insect bird chorus or content due to narrow band calls of a single bird species.
        /// It searches the full spectrum for a match to the template and then
        ///  calculates how much of the match weight is in the correct narrow freq band.
        /// </summary>
        /// <param name="oneMinuteOfIndices">Derived from the source recording.</param>
        /// <param name="template">A previously prepared template.</param>
        /// <param name="templateIndices">The actual dictionary of template arrays.</param>
        /// <returns>A similarity score.</returns>
        public static double GetNarrowBandContent1(Dictionary<string, double[]> oneMinuteOfIndices, TemplateManifest template, Dictionary<string, double[]> templateIndices)
        {
            var reductionFactor = template.SpectralReductionFactor;
            var reducedIndices = DataProcessing.ReduceIndicesByFactor(oneMinuteOfIndices, reductionFactor);

            // Now pass the template up the full frequency spectrum to get a spectrum of scores.
            var spectralScores = DataProcessing.ScanSpectrumWithTemplate(templateIndices, reducedIndices);

            // Now check how much of spectral weight is in the correct freq band ie between 3-4 kHz.
            int freqBinCount = ContentDescription.FreqBinCount / reductionFactor;
            int bottomFreq = template.BandMinHz; //Hertz
            int topFreq = template.BandMaxHz; //Hertz
            var freqBinBounds = DataProcessing.GetFreqBinBounds(bottomFreq, topFreq, freqBinCount);
            double callSum = DataTools.Subarray(spectralScores, freqBinBounds[0], freqBinBounds[1]).Sum();
            double totalSum = DataTools.Subarray(spectralScores, 1, spectralScores.Length - 3).Sum();
            double score = callSum / totalSum;
            return score;
        }

        // ###################################################################################
    }
}
