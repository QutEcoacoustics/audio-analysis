// <copyright file="TemplateManifest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Accord.IO;
    using Acoustics.Shared;

    /// <summary>
    /// Templates are initially defined manually in a YAML file. Each template in a YAML file is called a "manifest".
    /// The array of manifests in a yml file is used to calculate an array of "functional templates" in a json file.
    /// The json file is generated automatically from the information provided in the manifests.yml file.
    /// A  template manifest contains the "provenance" of the template (i.e. details of the recordings, source locations etc used to make the functional template.
    /// It also contains the information required to calculate the template definition.
    /// The core of a functional template is its definition, which is stored as a dictionary of spectral indices.
    /// The functional template also contains information required to scan new recordings with the template definition.
    ///
    /// Each template manifest in a yml file contains an EditStatus field which describes what to with the manifest.
    /// There are there options as described below.
    /// </summary>
    public enum EditStatus
    {
        Edit,   // This option edits an existing functional template in the json file. The template definition is (re)calculated.
        Copy,   // This option keeps an existing functional template unchanged.
        Ignore, // This option keeps an existing functional template unchanged except changes its UseStatus boolean field to FALSE.
    }

    /// <summary>
    /// This is base class for both template manifests and functional templates.
    /// Most of the fields and properties are common to both manifests and functional templates.
    /// Manifests contain the template provenance. This does not appear in the functional template because provenance includes path data.
    /// TODO Set up inheritance from base class so that there is separate class for manifests and functional templates.
    ///
    /// This class also contains methods to create new or edit existing functional templates based on info in the manifests.
    /// </summary>
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
            var newTemplateList = new List<FunctionalTemplate>();

            // cycle through all the manifests
            for (var i = 0; i < manifests.Length; i++)
            {
                var manifest = manifests[i];
                var name = manifest.Name;
                if (!dictionaryOfCurrentTemplates.ContainsKey(name))
                {
                    // the current manifest is not an existing template - therefore make it.
                    var newTemplate = CreateNewTemplateFromManifest(manifest);
                    newTemplate.Template = CreateTemplateDefinition(manifest);
                    newTemplate.MostRecentEdit = DateTime.Now;
                    newTemplateList.Add(newTemplate);
                    continue;
                }

                if (manifest.EditStatus == EditStatus.Edit)
                {
                    // This option edits an existing functional template in the json file. The template definition is (re)calculated.
                    var newTemplate = CreateNewTemplateFromManifest(manifest);
                    newTemplate.Template = CreateTemplateDefinition(manifest);
                    newTemplate.MostRecentEdit = DateTime.Now;
                    newTemplateList.Add(newTemplate);
                    continue;
                }

                if (manifest.EditStatus == EditStatus.Copy)
                {
                    // TODO: intentionally broken. FunctionalTemplates should be immutable. If they need to change create a new one (could be a copy, but it would have a version or edit date etc...).
                    throw new NotImplementedException();
                    // This option keeps an existing functional template unchanged.
                    //var existingTemplate = dictionaryOfCurrentTemplates[name];
                    //existingTemplate.UseStatus = true;
                    //existingTemplate.Provenance = null;
                    //newTemplateList.Add(existingTemplate);
                    //continue;
                }

                if (manifest.EditStatus == EditStatus.Ignore)
                {
                    // TODO: intentionally broken. FunctionalTemplates should be immutable. If they need to change create a new one (could be a copy, but it would have a version or edit date etc...).
                    // TODO: Per the comment above, if they're regenerated, there's no need to ignore some
                    throw new NotImplementedException();
                    // This option keeps an existing functional template unchanged except changes its UseStatus boolean field to FALSE.
                    //var existingTemplate = dictionaryOfCurrentTemplates[name];
                    //existingTemplate.Provenance = null;
                    //existingTemplate.UseStatus = false;
                    //newTemplateList.Add(existingTemplate);
                }
            }

            var templatesFileName = templateDefinitionsFile.Name;
            var templatesFilePath = Path.Combine(manifestFile.DirectoryName ?? throw new InvalidOperationException(), templatesFileName);

            // Save the previous templates file
            string backupTemplatesFilePath = Path.Combine(manifestFile.DirectoryName, templatesFileName + ".Backup.json");
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
        public static Dictionary<string, double[]> CreateTemplateDefinition(TemplateManifest templateManifest)
        {
            // Get the template provenance. Assume array contains only one element.
            var provenanceArray = templateManifest.Provenance;
            var provenance = provenanceArray[0];
            var sourceDirectory = provenance.Directory;
            var baseName = provenance.Basename;

            // Read all indices from the complete recording. The path variable is a partial path requiring to be appended.
            var path = Path.Combine(sourceDirectory, baseName + ContentSignatures.AnalysisString);
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

        public static FunctionalTemplate CreateNewTemplateFromManifest(TemplateManifest templateManifest)
        {
            var newTemplate = new FunctionalTemplate()
            {
                // TODO: is clone actually needed here?
                //       I chose clone because it mirrors the functionality that *was* here - i.e. a refactor
                Manifest = templateManifest.DeepClone(),
                UseStatus = true,
            };

            if (templateManifest.EditStatus == EditStatus.Ignore)
            {
                newTemplate.UseStatus = false;
            }

            return newTemplate;
        }

        //TEMPLATE DESCRIPTION
        // Name of the template
        public string Name { get; set; }

        //TEMPLATE DESCRIPTION
        // Name of the template
        public string Description { get; set; }

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



        public SourceAudioProvenance[] Provenance { get; set; }

        // The following random data was used to try some statistical experiments.
        // get dummy data
        //var rn = new RandomNumber(DateTime.Now.Second + (int)DateTime.Now.Ticks + 333);
        //var distance = rn.GetDouble();
    }

    /// <summary>
    /// This class holds info about provenance of a recording used to construct a template.
    /// </summary>
    public class SourceAudioProvenance
    {
        // TODO: use this property as the source audio for which indices will be calculated from
        /// <summary>
        /// Gets or sets the template Recording Location.
        /// </summary>
        public string Path { get; set; }

        // TODO: remove when calculating indices directly from audio segments
        /// <summary>
        /// Gets or sets the directory containing the source index files".
        /// </summary>
        public string Directory { get; set; }

        // TODO: remove when calculating indices directly from audio segments
        /// <summary>
        /// Gets or sets the basename for the source index files".
        /// Gets or sets the first minute (or matrix row assuming one-minute per row) of the selected indices.
        /// The rows/minutes are inclusive.
        /// </summary>
        public string Basename { get; set; }

        /// <summary>
        /// Gets or sets the template Recording Location".
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the first minute (or matrix row assuming one-minute per row) of the selected indices.
        /// The rows/minutes are inclusive.
        /// </summary>
        public int StartOffset { get; set; }

        public int EndOffset { get; set; }
    }

    public class FunctionalTemplate
    {
        public TemplateManifest Manifest { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the template or not.
        /// UseStatus can be true or false.
        /// </summary>
        public bool UseStatus { get; set; }

        /// <summary>
        /// Gets or sets the date the functional template was created.
        /// </summary>
        public DateTimeOffset MostRecentEdit { get; set; }

        public Dictionary<string, double[]> Template { get; set; }
    }
}
