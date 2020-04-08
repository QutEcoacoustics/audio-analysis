// <copyright file="ListArgs.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Arguments
{
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;

    [Command(
        "list",
        Description = "Lists known commands")]
    public class ListArgs
        : SubCommandBase
    {
        public override Task<int> Execute(CommandLineApplication app)
        {
            MainEntry.PrintUsage(null, MainEntry.Usages.ListAvailable);

            return this.Ok();
        }
    }
}