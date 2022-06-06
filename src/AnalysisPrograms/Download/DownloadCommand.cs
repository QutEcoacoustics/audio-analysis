// <copyright file="DownloadCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Download
{
    using System.Threading.Tasks;
    using AnalysisPrograms.Production.Arguments;
    using McMaster.Extensions.CommandLineUtils;

    [Command(DownloadCommandName, Description = "Downloads audio from a repository")]
    [Subcommand(typeof(RepositoriesCommand))]
    [Subcommand(typeof(FileCommand))]
    [Subcommand(typeof(SearchCommand))]
    [Subcommand(typeof(BatchCommand))]
    public class DownloadCommand : SubCommandBase
    {
        public const string DownloadCommandName = "download";

        public override Task<int> Execute(CommandLineApplication app)
        {
            MainEntry.PrintUsage("Choose a sub command.", MainEntry.Usages.Node, DownloadCommandName);

            return this.Ok();
        }
    }
}