// <copyright file="RepositoriesCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Download
{
    using System;
    using System.Threading.Tasks;
    using AnalysisPrograms.AcousticWorkbench.Orchestration;
    using AnalysisPrograms.Production.Arguments;
    using McMaster.Extensions.CommandLineUtils;
    using Spectre.Console;

    [Command(RepositoriesCommandName, Description = "Lists available repositories which we can download from")]
    public class RepositoriesCommand : SubCommandBase
    {
        public const string RepositoriesCommandName = "repositories";

        public override Task<int> Execute(CommandLineApplication app)
        {
            LoggedConsole.WriteLine("Available repositories:");

            var table = new Table();
            table.AddColumn(nameof(Repository.Name));
            table.AddColumn(nameof(Repository.HomeUri));

            foreach (var repository in Repositories.Known)
            {
                table.AddRow(repository.Name, repository.HomeUri);
            }

            AnsiConsole.Write(table);

            return this.Ok();
        }
    }

}