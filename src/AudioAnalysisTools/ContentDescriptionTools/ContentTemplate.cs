// <copyright file="ContentTemplate.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using System.Collections.Generic;
    using System.IO;
    using Acoustics.Shared.ConfigFile;
    using AudioAnalysisTools.Indices;
    using YamlDotNet.Serialization;

    public enum TemplateStatus
    {
        None,
        Standard,
        Locked,
    }

    public class ContentTemplate
    {
        //TEMPLATE DESCRIPTION
        // Name of the template
        public string Name { get; set; }

        public int TemplateId { get; set; }

        public TemplateStatus Status { get; set; }

        public byte FeatureExtractionAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets the factor by which a spectrum of index values is reduced.
        /// Full array (256 freq bins) of spectral indices is reduced by the following factor by averaging.
        /// This is to reduce correlation and computation.
        /// </summary>
        public int SpectralReductionFactor { get; set; }

        /// <summary>
        /// Gets or sets the bottom freq of bandpass filter.
        /// Bandpass filter to be applied where the target content exists only within a narrow band, e.g. 3-4 kHz for Silver-eye band.
        /// Bottom of the required frequency band.
        /// </summary>
        public int BandMinHz { get; set; }

        /// <summary>
        /// Gets or sets the top freq of bandpass filter.
        /// Bandpass filter to be applied where the target content exists only within a narrow band, e.g. 3-4 kHz for Silver-eye band.
        /// Top of the required frequency band.
        /// </summary>
        public int BandMaxHz { get; set; }

        //public int FrameSize { get; set; }

        public Dictionary<string, double[]> Template { get; set; }

        /// <summary>
        /// Returns a cached set of configuration properties.
        /// WARNING CACHED!.
        /// </summary>
        //public static IndexPropertiesCollection GetIndexProperties(FileInfo configFile)
        //{
        //    return ConfigFile.Deserialize<IndexPropertiesCollection>(configFile);
        //}

        /*
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
            var finalTemplate = GetTemplate(ipDir);
            var opPath = Path.Combine(opDir.FullName, Name + "Template.csv");
            FileTools.WriteDictionaryToFile(finalTemplate, opPath);
        }
        */
    }
}
