// <copyright file="HelpArgs.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Arguments
{
    using System;
    using System.Threading.Tasks;

    using Acoustics.Shared;

    using McMaster.Extensions.CommandLineUtils;

    [Command(HelpCommandName, Description = "Prints the full help for the program and all actions")]
    public class HelpArgs
        : SubCommandBase
    {
        public const string HelpCommandName = "help";

        [Argument(0, Description = "The command to get help on")]
        public string CommandName { get; set; }

        public override Task<int> Execute(CommandLineApplication app)
        {
            if (this.CommandName.IsNullOrEmpty())
            {
                MainEntry.PrintUsage(null, MainEntry.Usages.Single, HelpCommandName);
            }
            else if (this.CommandName == Meta.Name)
            {
                MainEntry.PrintUsage(null, MainEntry.Usages.All);
            }
            else
            {
                MainEntry.PrintUsage(null, MainEntry.Usages.Single, this.CommandName);
            }

            return this.Ok();
        }
    }
}