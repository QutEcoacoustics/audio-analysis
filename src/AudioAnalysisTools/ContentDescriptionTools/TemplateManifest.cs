// <copyright file="TemplateManifest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using System;
    using System.Collections.Generic;

    public enum TemplateStatus
    {
        None,
        Standard,
        Locked,
    }

    public class TemplateManifest
    {
        //TEMPLATE DESCRIPTION
        // Name of the template
        public string Name { get; set; }

        public int TemplateId { get; set; }

        /// <summary>
        /// Gets or sets a comment about the template.
        /// e.g. "Detects light rain".
        /// </summary>
        public string GeneralComment { get; set; }

        /// <summary>
        /// Gets or sets a comment about the status.
        /// e.g. "Locked, Standard, None".
        /// </summary>
        public string StatusComment { get; set; }

        /// <summary>
        /// Gets or sets the template manifest status.
        /// Status can be "locked", etc.
        /// </summary>
        public TemplateStatus Status { get; set; }

        public DateTime MostRecentEdit { get; set; }

        //TEMPLATE PROVENANCE

        /// <summary>
        /// Gets or sets the template source directory".
        /// </summary>
        public string TemplateSourceSubdirectory { get; set; }

        /// <summary>
        /// Gets or sets the template source file name".
        /// </summary>
        public string TemplateSourceFileName { get; set; }

        /// <summary>
        /// Gets or sets the template Recording Location".
        /// </summary>
        public string TemplateRecordingLocation { get; set; }

        /// <summary>
        /// Gets or sets the template Recording DateTime".
        /// </summary>
        public string TemplateRecordingDateTime { get; set; }

        //ALGORITHMIC PARAMETERS ASSOCIATED WITH TEMPLATE
        public byte FeatureExtractionAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets the temporal Selection - the minutes are inclusive".
        /// </summary>
        public string TemporalSelectionComment { get; set; }

        /// <summary>
        /// Gets or sets the first minute (or matrix row assuming one-minute per row) of the selected indices.
        /// The rows/minutes are inclusive.
        /// </summary>
        public int StartRowId { get; set; }

        public int EndRowId { get; set; }

        /// <summary>
        /// Gets or sets the factor by which a spectrum of index values is reduced.
        /// Full array (256 freq bins) of spectral indices is reduced by the following factor by averaging.
        /// This is to reduce correlation and computation.
        /// </summary>
        public int SpectralReductionFactor { get; set; }

        /// <summary>
        /// Gets or sets the FreqBand Comment.
        /// </summary>
        public string FreqBandComment { get; set; }

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

        public Dictionary<string, double[]> Template { get; set; }

        /// <summary>
        /// THis method is the same for all Content Types but uses constants appropriate the template type.
        /// </summary>
        public static TemplateManifest CreateTemplate(string filePath, TemplateManifest templateManifest)
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

        // The following random data was used to try some statistical experiments.
        // get dummy data
        //var rn = new RandomNumber(DateTime.Now.Second + (int)DateTime.Now.Ticks + 333);
        //var distance = rn.GetDouble();
    }
}
