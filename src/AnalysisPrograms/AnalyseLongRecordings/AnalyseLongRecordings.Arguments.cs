// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalyseLongRecordings.Arguments.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the AnalyseLongRecording type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.AnalyseLongRecordings
{
    using System.IO;
    using System.Threading.Tasks;
    using AnalysisBase;
    using McMaster.Extensions.CommandLineUtils;
    using Production;
    using Production.Arguments;
    using Production.Validation;

    public partial class AnalyseLongRecording
    {
        public const string CommandName = "audio2csv";

        private const string AdditionalNotes = @"
AlignToMinute Options:
  - None:        does no alignment (default) 
  - TrimBoth:    does alignment and removes fractional sections 
  - TrimNeither: does alignment and keeps fractional sections 
  - TrimStart:   does alignment and keeps last fractional segment 
  - TrimEnd:     does alignment and keeps first fractional segment ";

        [Command(
            CommandName,
            Description = "Analyses long recordings, cutting them into blocks. Performs the analysis specified in Config file.",
            ExtendedHelpText = AdditionalNotes)]
        public class Arguments : SourceConfigOutputDirArguments
        {
            public Arguments()
            {
#if DEBUG
                this.WhenExitCopyLog = true;
                this.WhenExitCopyConfig = true;
#endif
            }

            [Option(Description = "Sets the name of the analysis to run. If not set, analysis identifer is parsed from the config file name.")]
            public string AnalysisIdentifier { get; set; }

            [Option(
                CommandOptionType.SingleValue,
                Description = "A TEMP directory where cut files will be stored. Use this option for efficiency (e.g. write to a RAM Disk).",
                ShortName = "t")]
            [DirectoryExistsOrCreate(createIfNotExists: true)]
            [LegalFilePath]
            public DirectoryInfo TempDir { get; set; }

            [Option(
                CommandOptionType.SingleValue,
                Description = "The start offset to start analyzing from (in seconds)")]
            [InRange(min: 0)]
            public double? StartOffset { get; set; }

            [Option(
                CommandOptionType.SingleValue,
                Description = "The end offset to stop analyzing (in seconds)")]
            [InRange(min: 0)]
            public double? EndOffset { get; set; }

            [Option(
                Description = "Allow advancing the start of the analysis to the nearest minute. A valid datetime must be available in the file name. Seed additional notes for options.",
                ShortName = "")]
            public TimeAlignment AlignToMinute { get; set; } = TimeAlignment.None;

            [Option(Description = "An array of channels to select. Default is all channels.")]
            public int[] Channels { get; set; } = null;

            [Option(
                CommandOptionType.SingleValue,
                Description = "Mix all selected input channels down into one mono channel. Default is to mixdown.")]
            public bool MixDownToMono { get; set; } = true;

            [Option(
                Description = "Whether or not run this analysis in parallel - multiple segments can be analyzed at the same time",
                ShortName = "p")]
            public bool Parallel { get; set; } = false;

            [Option(
                Description = "If true, attempts to copy the executable's log file to output directory. If it can't determine an output directory, it copies to the working directory.",
                ShortName = "")]
            public bool WhenExitCopyLog { get; set; }

            [Option(
                Description = "If true, attempts to copy the executable's config file to output directory. If it can't determine an output directory, it copies to the working directory. If it can't find a config file, nothing is copied",
                ShortName = "")]
            public bool WhenExitCopyConfig { get; set; }

            public override Task<int> Execute(CommandLineApplication app)
            {
                AnalyseLongRecording.Execute(this);

                return this.Ok();
            }
        }
    }
}