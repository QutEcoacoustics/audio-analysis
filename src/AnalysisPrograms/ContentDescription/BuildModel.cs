// <copyright file="BuildModel.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.ContentDescription
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Drawing;
    using System.IO;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using AnalysisPrograms.Production.Arguments;
    using AnalysisPrograms.Production.Validation;
    using AudioAnalysisTools.ContentDescriptionTools;
    using McMaster.Extensions.CommandLineUtils;
    using TowseyLibrary;

    /// <summary>
    /// THis class builds/makes a set of content description templates.
    /// Templates are initially defined manually in a YAML file. Each template in a YAML file is called a "manifest".
    /// The array of manifests in a yml file is used to calculate an array of "functional templates" in a json file.
    /// The json file is generated automatically from the information provided in the manifests.yml file.
    /// A  template manifest contains the "provenance" of the template (i.e. details of the recordings, source locations etc used to make the functional template.
    /// It also contains the information required to calculate the template definition.
    /// The core of a functional template is its definition, which is stored as a dictionary of spectral indices.
    /// The functional template also contains information required to scan new recordings with the template definition.
    ///
    /// IMPORTANT NOTE: At current time (Nov, 2019) Functional Templates are made by reading csv files containing pre-calculated spectral indices.
    ///                 In addition, the Functional Templates can subsequently be tested (this is optional) by reading csv files of spectral indices.
    ///                 The first two FileInfo arguments in the arguments list are compulsory and templates cannot be made without them.
    ///                 Arguments 3 and 4 are optional. They must be provided for testing the templates. Testing also requires files of pre-calculated spectral indices.
    /// TODO: Refactor the code so that functional templates can be made and tested reading directly from .wav recordings files.
    /// </summary>
    public class BuildModel
    {
        public const string CommandName = "BuildContentDescriptionModel";

        [Command(
            CommandName,
            Description = "Reads a file of template manifests and creates an output file of functional templates that are used for audio content description.")]
        public class Arguments : SubCommandBase
        {
            [Argument(
                0,
                Description = "Path to an input yml file containing an array of template manifests.")]
            [Required]
            [FileExists]
            [LegalFilePath]
            public FileInfo TemplateManifest { get; set; }

            [Argument(
                1,
                Description = "Path to an output json file containing an array of functional templates.")]
            [Required]
            [FileExists]
            [LegalFilePath]
            public FileInfo TemplateDefinitions { get; set; }

            [Argument(
                2,
                Description = "Optional argument: Path to a txt file containing list of pre-calculated test data files.")]
            //[Required]
            [FileExists]
            [LegalFilePath]
            public FileInfo ListOfTestIndexFiles { get; set; }

            [Argument(
                3,
                Description = "Optional argument: Image of LDFC spectrogram consistent with the data-files listed in previous argument.")]
            //[Required]
            //[DirectoryExistsOrCreate(createIfNotExists: true)]
            [FileExists]
            [LegalFilePath]
            public virtual FileInfo ImageOfLdfcSpectrogram { get; set; }

            public override Task<int> Execute(CommandLineApplication app)
            {
                BuildModel.Execute(this);

                return this.Ok();
            }
        }

        public static void Execute(Arguments arguments)
        {
            FileInfo manifestFile = arguments.TemplateManifest;
            FileInfo functionalTemplatesFile = arguments.TemplateDefinitions;

            // Read in all template manifests
            var manifests = Yaml.Deserialize<TemplateManifest[]>(manifestFile);

            // Read current template definitions and convert to dictionary
            var arrayOfFunctionalTemplates = Json.Deserialize<FunctionalTemplate[]>(functionalTemplatesFile);
            var dictionaryOfCurrentTemplates = DataProcessing.ConvertArrayOfFunctionalTemplatesToDictionary(arrayOfFunctionalTemplates);

            // init a new template list for output.
            var newTemplateList = new List<FunctionalTemplate>();

            // cycle through all the manifests
            foreach (var manifest in manifests)
            {
                var name = manifest.Name;
                if (!dictionaryOfCurrentTemplates.ContainsKey(name))
                {
                    // the current manifest is not an existing template - therefore make it.
                    var newTemplate = new FunctionalTemplate(manifest)
                    {
                        Template = TemplateManifest.CreateTemplateDefinition(manifest),
                        MostRecentEdit = DateTime.Now,
                    };
                    newTemplateList.Add(newTemplate);
                    continue;
                }

                if (manifest.EditStatus == EditStatus.Edit)
                {
                    // This option edits an existing functional template in the json file. The template definition is (re)calculated.
                    // Effectively the same as creating a new template.
                    var newTemplate = new FunctionalTemplate(manifest)
                    {
                        Template = TemplateManifest.CreateTemplateDefinition(manifest),
                        MostRecentEdit = DateTime.Now,
                    };
                    newTemplateList.Add(newTemplate);
                    continue;
                }

                if (manifest.EditStatus == EditStatus.Copy)
                {
                    // This option keeps an existing functional template unchanged.
                    var existingTemplate = dictionaryOfCurrentTemplates[name];
                    newTemplateList.Add(existingTemplate);
                    continue;
                }

                if (manifest.EditStatus == EditStatus.Ignore)
                {
                    // Do not output this template to the list of functional templates.
                    continue;
                }
            }

            var functionalTemplatesFileName = functionalTemplatesFile.Name;
            var functionalTemplatesFilePath = Path.Combine(manifestFile.DirectoryName ?? throw new InvalidOperationException(), functionalTemplatesFileName);

            // Save the previous templates file
            string backupPath = Path.Combine(manifestFile.DirectoryName, functionalTemplatesFileName + ".Backup.json");
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }

            //Now copy the file first
            File.Copy(functionalTemplatesFilePath, backupPath, true);

            //Now Rename the File
            //File.Move(NewFilePath, Path.Combine(NewFileLocation, "File.txt"));

            // No need to move the backup because serializing over-writes the current templates file.
            var opTemplatesFile = new FileInfo(functionalTemplatesFilePath);
            Json.Serialise(opTemplatesFile, newTemplateList.ToArray());

            if (arguments.ListOfTestIndexFiles != null)
            {
                TestTemplates(arguments.ListOfTestIndexFiles, opTemplatesFile, arguments.ImageOfLdfcSpectrogram);
            }
        }

        public static void TestTemplates(FileInfo listOfIndexFiles, FileInfo templatesFile, FileInfo imageOfLdfcSpectrogram)
        {
            var contentDictionary = ContentSignatures.ContentDescriptionOfMultipleRecordingFiles(listOfIndexFiles, templatesFile);

            // Write the results to a csv file
            var outputDirectory = templatesFile.DirectoryName;
            var filePath = Path.Combine(outputDirectory ?? throw new InvalidOperationException("Output directory does not exist."), "AcousticSignatures.csv");
            FileTools.WriteDictionaryAsCsvFile(contentDictionary, filePath);

            // get content description plots and use to examine score distributions.
            var contentPlots = ContentSignatures.GetPlots(contentDictionary);
            var images = GraphsAndCharts.DrawPlotDistributions(contentPlots);
            var plotsImage = ImageTools.CombineImagesVertically(images);
            var path1 = Path.Combine(outputDirectory, "ScoreDistributions.png");
            plotsImage.Save(path1);

            // Attach plots to LDFC spectrogram and write to file
            var imageList = new List<Image>();
            if (imageOfLdfcSpectrogram != null)
            {
                var ldfcSpectrogram = Image.FromFile(imageOfLdfcSpectrogram.FullName);
                imageList.Add(ldfcSpectrogram);
            }

            if (contentPlots != null)
            {
                int plotHeight = 30;
                foreach (var plot in contentPlots)
                {
                    var imageOfPlot = plot.DrawAnnotatedPlot(plotHeight);
                    imageList.Add(imageOfPlot);
                }
            }

            if (imageList.Count != 0)
            {
                var opImage = ImageTools.CombineImagesVertically(imageList);
                var path2 = Path.Combine(outputDirectory, templatesFile.BaseName() + ".TestOfTemplates.png");
                opImage.Save(path2);
            }

            //Console.WriteLine("# Finished test of content description templates");
        }
    }
}
