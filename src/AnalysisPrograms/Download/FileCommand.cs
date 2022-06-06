// <copyright file="FileCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Download
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AnalysisPrograms.Production;
    using AnalysisPrograms.Production.Validation;
    using global::AcousticWorkbench;
    using global::AcousticWorkbench.Models;
    using McMaster.Extensions.CommandLineUtils;
    using Spectre.Console;

    [Command(FileCommandName, Description = "Download a single file from a remote repository", ExtendedHelpText = ExtendedHelp)]
    public class FileCommand : DownloadBaseCommand
    {
        private const string FileCommandName = "file";

        private const string ExtendedHelp = @$"
Each file argument must be of the form of an integer ID for the audio recording you want to download.

{RepositoriesHint}";

        [Option(
            Description = "If used will not place downloaded files into sub-folders")]
        public override bool Flat { get; set; }

        [Option(Description = "A directory to write output to")]
        [DirectoryExistsOrCreate(createIfNotExists: false)]
        [LegalFilePath]
        public override DirectoryInfo Output { get; set; }

        [Argument(
            0,
            Description = "One or more audio files to download")]
        [Required]
        public ulong[] Ids { get; set; }

        public override async Task<int> Execute(CommandLineApplication app)
        {
            this.ValidateCommon();

            this.ShowOptions();

            await this.SignIn();

            this.Console.WriteLine("Starting downloads...");

            var results = await this.DownloadFiles();

            this.PrintSummary(results);

            return ExceptionLookup.Ok;
        }

        protected override void AddOptionToShowOptions(Grid grid)
        {
            grid.AddRow(nameof(this.Ids), this.Ids.Join(", "));
            base.AddOptionToShowOptions(grid);
        }

        protected override Task<IReadOnlyCollection<AudioRecording>> PageQuery(AudioRecordingService service, int page)
        {
            return service.FilterRecordingsForDownload(ids: this.Ids, page: page);
        }
    }

}