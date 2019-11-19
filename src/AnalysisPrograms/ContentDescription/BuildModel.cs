// <copyright file="BuildModel.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.ContentDescription
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using AnalysisPrograms.Production.Arguments;
    using AudioAnalysisTools.ContentDescriptionTools;
    using McMaster.Extensions.CommandLineUtils;

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

            /*
            [Argument(
                2,
                Description = "A directory to write output to")]
            [Required]
            [DirectoryExistsOrCreate(createIfNotExists: true)]
            [LegalFilePath]
            public virtual DirectoryInfo Output { get; set; }
            */

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
        }
    }
}
