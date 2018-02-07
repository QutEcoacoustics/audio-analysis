// <copyright file="HelpArgs.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Arguments
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;

    [Command("help", Description = "Prints the full help for the program and all actions")]
    public class HelpArgs
        : SubCommandBase
    {
        [Argument(0, Description = "The command to get help on")]
        public string CommandName { get; set; }

        public override Task<int> Execute(CommandLineApplication app)
        {
            if (this.CommandName.IsNullOrEmpty())
            {
                MainEntry.PrintUsage(null, MainEntry.Usages.All);
            }

            var command = app.Commands.FirstOrDefault(x =>
                x.Name.Equals(this.CommandName, StringComparison.InvariantCultureIgnoreCase));

            if (command == null)
            {
                throw new CommandParsingException(
                    app,
                    $"Could not find a command with name that matches `{this.CommandName}`.");
            }

            MainEntry.PrintUsage(null, MainEntry.Usages.Single, command.Name);

            return this.Ok();
        }
    }
}