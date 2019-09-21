// <copyright file="ContentTemplate.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using System.Collections.Generic;
    using System.IO;
    using Acoustics.Shared.ConfigFile;
    using AudioAnalysisTools.Indices;
    using TowseyLibrary;
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

        public int StartRowId { get; set; }

        public int EndRowId { get; set; }

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
        /// THis method is the same for all Content Types but uses constants appropriate the template type.
        /// </summary>
        public static ContentTemplate CreateTemplate(string filePath, ContentTemplate templateManifest)
        {
            // Read all indices from the complete recording
            var dictionaryOfIndices = DataProcessing.ReadIndexMatrices(filePath);
            var algorithmType = templateManifest.FeatureExtractionAlgorithm;
            Dictionary<string, double[]> newTemplate;

            switch (algorithmType)
            {
                case 1:
                    newTemplate = ContentAlgorithms.CreateFullBandTemplate1(templateManifest, dictionaryOfIndices);
                    templateManifest.Template = newTemplate;
                    break;
                case 2:
                    newTemplate = ContentAlgorithms.CreateBroadbandTemplate1(templateManifest, dictionaryOfIndices);
                    templateManifest.Template = newTemplate;
                    break;
                case 3:
                    newTemplate = ContentAlgorithms.CreateNarrowBandTemplate1(templateManifest, dictionaryOfIndices);
                    templateManifest.Template = newTemplate;
                    break;
                default:
                    //LoggedConsole.WriteWarnLine("Algorithm " + algorithmType + " does not exist.");
                    templateManifest.Template = null;
                    break;
            }

            return templateManifest;
        }

        /// <summary>
        /// THis method is the same for all Content Types.
        /// </summary>
        public static void WriteTemplateToFile(string filePath, ContentTemplate templateManifest, DirectoryInfo opDir)
        {
            var template = CreateTemplate(filePath, templateManifest);
            var opPath = Path.Combine(opDir.FullName, templateManifest.Name + "Template.csv");
            //FileTools.WriteDictionaryToFile(templateManifest, opPath);
        }

        // The following random data was used to try some statistical experiments.
        // get dummy data
        //var rn = new RandomNumber(DateTime.Now.Second + (int)DateTime.Now.Ticks + 333);
        //var distance = rn.GetDouble();
    }
}
