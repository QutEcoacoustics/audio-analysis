// <copyright file="BatchCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Download
{
    using System.IO;
    using System.Threading.Tasks;
    using AnalysisPrograms.Production;
    using AnalysisPrograms.Production.Validation;
    using McMaster.Extensions.CommandLineUtils;

    [Command(BatchCommandName, Description = "Download a multiple files from a remote repository")]
    public class BatchCommand : SearchCommand
    {
        private const string BatchCommandName = "batch";

        [Option(
            Description = "If used will not place downloaded files into sub-folders")]
        public override bool Flat { get; set; }

        [Option(Description = "A directory to write output to")]
        [DirectoryExistsOrCreate(createIfNotExists: false)]
        [LegalFilePath]
        public override DirectoryInfo Output { get; set; }

        public override async Task<int> Execute(CommandLineApplication app)
        {
            this.ValidateBatchOptions();

            this.ShowOptions();

            await this.SignIn();

            var results = await this.DownloadFiles();

            this.PrintSummary(results);

            return ExceptionLookup.Ok;
        }
    }

}