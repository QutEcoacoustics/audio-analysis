// <copyright file="RibbonPlot.Arguments.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Draw.RibbonPlots
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using AnalysisPrograms.Production.Arguments;
    using AnalysisPrograms.Production.Validation;
    using McMaster.Extensions.CommandLineUtils;

    public partial class RibbonPlot
    {
        public const string CommandName = "DrawRibbonPlots";

        public const string AdditionalNotes =
            "This command treats all found ribbon plots as belonging to the same site. " +
            "Thus files with duplicate dates are not permitted.";

        [Command(
            CommandName,
            Description = "Combines ribbon plots together into a stacked chart",
            ExtendedHelpText = AdditionalNotes)]
        public class Arguments : SubCommandBase
        {
            [Argument(
                0,
                Description = "One or more directories where that contain ribbon FCS files (index results).")]
            public DirectoryInfo[] InputDirectories { get; set; }

            [Option(
                CommandOptionType.SingleValue,
                Description = "Directory where the output ribbon plot is saved.")]
            [DirectoryExistsOrCreate(createIfNotExists: true)]
            [LegalFilePath]
            public DirectoryInfo OutputDirectory { get; set; }

            [Option(
                CommandOptionType.SingleValue,
                Description =
                    "TimeSpan offset hint required if file names do not contain time zone info. NO DEFAULT IS SET",
                ShortName = "z")]
            public TimeSpan? TimeSpanOffsetHint { get; set; }

            [Option(
                CommandOptionType.SingleValue,
                Description =
                    "Changes when `Midnight` occurs. Defaults to `00:00`. If you want ribbons that span across the night, then set `Midnight` to `12:00`")]
            public TimeSpan? Midnight { get; set; } = TimeSpan.Zero;

            public override Task<int> Execute(CommandLineApplication app)
            {
                return Draw.RibbonPlots.RibbonPlot.Execute(this);
            }
        }
    }
}