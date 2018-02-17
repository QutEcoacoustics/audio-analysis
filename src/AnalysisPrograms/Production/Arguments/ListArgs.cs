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