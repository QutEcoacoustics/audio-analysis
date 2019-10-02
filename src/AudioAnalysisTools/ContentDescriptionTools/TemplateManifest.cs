// <copyright file="TemplateManifest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Acoustics.Shared;
    using Newtonsoft.Json;

    public enum EditStatus
    {
        CalculateTemplate,
        Edit,
        Copy,
        Ignore,
    }

    public enum UseStatus
    {
        Use,
        Ignore,
    }

    public class TemplateManifest
    {
        public static void CreateNewFileOfTemplateDefinitions(FileInfo manifestFile, FileInfo templateDefinitionsFile)
        {
            // Read in all template manifests
            var manifests = Yaml.Deserialize<TemplateManifest[]>(manifestFile);

            // Read current template definitions and convert to dictionary
            var arrayOfTemplates = Json.Deserialize<TemplateManifest[]>(templateDefinitionsFile);
            var dictionaryOfCurrentTemplates = DataProcessing.ConvertTemplateArrayToDictionary(arrayOfTemplates);

            // init a new template list for output.
            var newTemplateList = new List<TemplateManifest>();

            // cycle through all the manifests
            foreach (var templateManifest in manifests)
            {
                var name = templateManifest.Name;
                if (!dictionaryOfCurrentTemplates.ContainsKey(name))
                {
                    // the current manifest is not an existing template - therefore make it.
                    var newTemplate = CreateNewTemplateFromManifest(templateManifest);
                    var templateDict = CreateTemplateDeftn(templateManifest);
                    newTemplate.Template = templateDict;
                    newTemplate.MostRecentEdit = DateTime.Now;
                    newTemplateList.Add(newTemplate);
                    continue;
                }

                if (templateManifest.EditStatus == EditStatus.Copy)
                {
                    // add existing template unchanged.
                    var existingTemplate = dictionaryOfCurrentTemplates[name];
                    newTemplateList.Add(existingTemplate);
                    continue;
                }

                if (templateManifest.EditStatus == EditStatus.Ignore)
                {
                    // add existing template unchanged except change UseStatus to Ignore.
                    var existingTemplate = dictionaryOfCurrentTemplates[name];
                    existingTemplate.UseStatus = UseStatus.Ignore;
                    newTemplateList.Add(existingTemplate);
                    continue;
                }

                if (templateManifest.EditStatus == EditStatus.CalculateTemplate)
                {
                    // add existing template but recalculate the template definition
                    var existingTemplate = dictionaryOfCurrentTemplates[name];
                    existingTemplate.UseStatus = UseStatus.Use;
                    var templateDict = CreateTemplateDeftn(templateManifest);
                    existingTemplate.Template = templateDict;
                    newTemplateList.Add(existingTemplate);
                }
            }

            var templatesFilePath = Path.Combine(manifestFile.DirectoryName ?? throw new InvalidOperationException(), "TemplateDefinitions.json");

            // Save the previous templates file
            string backupTemplatesFilePath = Path.Combine(manifestFile.DirectoryName, "TemplateDefinitions.Backup.json");
            if (File.Exists(backupTemplatesFilePath))
            {
                File.Delete(backupTemplatesFilePath);
            }

            //Now copy the file first
            File.Copy(templatesFilePath, backupTemplatesFilePath, true);

            //Now Rename the File
            //File.Move(NewFilePath, Path.Combine(NewFileLocation, "File.txt"));

            // No need to move the backup because serializing over-writes the current templates file.
            var templatesFile = new FileInfo(templatesFilePath);

            //Yaml.Serialize(templatesFile, newTemplateList.ToArray());
            Json.Serialise(templatesFile, newTemplateList.ToArray());
        }

        /// <summary>
        /// THis method calculates new template based on passed manifest.
        /// </summary>
        public static Dictionary<string, double[]> CreateTemplateDeftn(TemplateManifest templateManifest)
        {
            // Get the template provenance. Assume array contains only one element.
            var provenanceArray = templateManifest.Provenance;
            var provenance = provenanceArray[0];
            var sourceDirectory = provenance.Directory;
            var baseName = provenance.Basename;

            // Read all indices from the complete recording. The path variable is a partial path requiring to be appended.
            var path = Path.Combine(sourceDirectory, baseName + ContentDescription.AnalysisString);
            var dictionaryOfIndices = DataProcessing.ReadIndexMatrices(path);
            var algorithmType = templateManifest.FeatureExtractionAlgorithm;
            Dictionary<string, double[]> newTemplateDeftn;

            switch (algorithmType)
            {
                case 1:
                    newTemplateDeftn = ContentAlgorithms.CreateFullBandTemplate1(templateManifest, dictionaryOfIndices);
                    break;
                case 2:
                    newTemplateDeftn = ContentAlgorithms.CreateBroadbandTemplate1(templateManifest, dictionaryOfIndices);
                    break;
                case 3:
                    newTemplateDeftn = ContentAlgorithms.CreateNarrowBandTemplate1(templateManifest, dictionaryOfIndices);
                    break;
                default:
                    //LoggedConsole.WriteWarnLine("Algorithm " + algorithmType + " does not exist.");
                    newTemplateDeftn = null;
                    break;
            }

            return newTemplateDeftn;
        }

        public static TemplateManifest CreateNewTemplateFromManifest(TemplateManifest templateManifest)
        {
            var newTemplate = new TemplateManifest
            {
                Name = templateManifest.Name,
                TemplateId = templateManifest.TemplateId,
                EditStatus = templateManifest.EditStatus,
                UseStatus = templateManifest.UseStatus,
                FeatureExtractionAlgorithm = templateManifest.FeatureExtractionAlgorithm,
                SpectralReductionFactor = templateManifest.SpectralReductionFactor,
                BandMinHz = templateManifest.BandMinHz,
                BandMaxHz = templateManifest.BandMaxHz,
                Provenance = null,
            };
            return newTemplate;
        }

        //TEMPLATE DESCRIPTION
        // Name of the template
        public string Name { get; set; }

        //TEMPLATE DESCRIPTION
        // Name of the template
        public string Description { get; set; }

        public int TemplateId { get; set; }

        /// <summary>
        /// Gets or sets a comment about the template.
        /// e.g. "Detects light rain".
        /// </summary>
        public string GeneralComment { get; set; }

        /// <summary>
        /// Gets or sets the template edit status.
        /// EditStatus can be "locked", etc.
        /// </summary>
        public EditStatus EditStatus { get; set; }

        /// <summary>
        /// Gets or sets the template manifest status.
        /// UseStatus can be "use" or "ignore".
        /// </summary>
        public UseStatus UseStatus { get; set; }

        public DateTime MostRecentEdit { get; set; }

        //ALGORITHMIC PARAMETERS ASSOCIATED WITH TEMPLATE
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

        public Dictionary<string, double[]> Template { get; set; }

        public TemplateProvenance[] Provenance { get; set; }

        // The following random data was used to try some statistical experiments.
        // get dummy data
        //var rn = new RandomNumber(DateTime.Now.Second + (int)DateTime.Now.Ticks + 333);
        //var distance = rn.GetDouble();
    }

    /// <summary>
    /// This class holds info about provenance of a recording used to construct a template.
    /// </summary>
    public class TemplateProvenance
    {
        //TEMPLATE PROVENANCE

        /// <summary>
        /// Gets or sets the directory containing the source index files".
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Gets or sets the basename for the source index files".
        /// </summary>
        public string Basename { get; set; }

        /// <summary>
        /// Gets or sets the template Recording Location".
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the template Recording DateTime".
        /// </summary>
        public string DateTime { get; set; }

        /// <summary>
        /// Gets or sets the first minute (or matrix row assuming one-minute per row) of the selected indices.
        /// The rows/minutes are inclusive.
        /// </summary>
        public int StartOffset { get; set; }

        public int EndOffset { get; set; }
    }
}
