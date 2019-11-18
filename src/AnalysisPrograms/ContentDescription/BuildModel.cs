using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalysisPrograms.ContentDescription
{
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using AnalysisBase;
    using AnalysisPrograms.Production;
    using AnalysisPrograms.Production.Arguments;
    using AnalysisPrograms.Production.Validation;
    using AudioAnalysisTools.ContentDescriptionTools;
    using McMaster.Extensions.CommandLineUtils;

    public partial class BuildModel
    {
        public const string CommandName = "BuildContentDescriptionModel";

        [Command(
            CommandName,
            Description = "TODO")]
        public class Arguments: SubCommandBase
        {
            [Argument(
                0,
                Description = "TODO")]
            [Required]
            [FileExists]
            [LegalFilePath]
            public FileInfo TemplateManifest { get; set; }

            [Argument(
                1,
                Description = "TODO")]
            [Required]
            [FileExists]
            [LegalFilePath]
            public FileInfo TemplateDefinitions { get; set; }

            [Argument(
                2,
                Description = "A directory to write output to")]
            [Required]
            [DirectoryExistsOrCreate(createIfNotExists: true)]
            [LegalFilePath]
            public virtual DirectoryInfo Output { get; set; }

            public override Task<int> Execute(CommandLineApplication app)
            {
                BuildModel.Execute(this);

                return this.Ok();
            }
        }

        public static void Execute(Arguments arguments)
        {
            // TODO: inline CreateNewFileOfTemplateDefinitions to this method.
            TemplateManifest.CreateNewFileOfTemplateDefinitions(arguments.TemplateManifest,
                arguments.TemplateDefinitions);

            LoggedConsole.WriteSuccessLine("Completed");
        }
    }
}
