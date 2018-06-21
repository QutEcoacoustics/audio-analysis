// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventStatisticsEntry.Arguments.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the AnalyseLongRecording type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.EventStatistics
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Threading.Tasks;
    using AnalysisBase;
    using McMaster.Extensions.CommandLineUtils;
    using Production;
    using Production.Arguments;
    using Production.Validation;

    public partial class EventStatisticsEntry
    {
        public const string CommandName = "EventStatistics";

        private const string AdditionalNotes = @"
At this stage only remote analysis is supported - that means querying an Acoustic Workbench website.
Support may be added in the future for other data sources (e.g. local). If you want this, file an issue!

For remote resources, the input file needs to have either one of these sets of fields:
- AudioEventId/audio_event_id
- AudioRecordingId/audio_recording_id, EventStartSeconds/event_start_seconds, EventEndSeconds/event_end_seconds, LowFrequencyHertz/low_frequency_hertz, HighFrequencyHertz/high_frequency_herttz
";

        [Command(
            CommandName,
            Description = "[BETA] Accepts a list of acoustic events to analyze. Returns a data file of statistics",
            ExtendedHelpText = AdditionalNotes)]
        public class Arguments
            : SourceConfigOutputDirArguments
        {
            [Argument(
                0,
                Description = "The source event (annotations) file to operate on")]
            [ExistingFile]
            [Required]
            [LegalFilePath]
            public override FileInfo Source { get; set; }

            [Option(
                CommandOptionType.SingleValue,
                Description = "A TEMP directory where cut files will be stored. Use this option for efficiency (e.g. write to a RAM Disk).")]
            [DirectoryExistsOrCreate(createIfNotExists: true)]
            [LegalFilePath]
            public DirectoryInfo TempDir { get; set; }

            [Option(Description = "An array of channels to select. Default is all channels.")]
            public int[] Channels { get; set; } = null;

            [Option(
                CommandOptionType.SingleValue,
                Description = "Mix all selected input channels down into one mono channel. Default is to mixdown.")]
            public bool MixDownToMono { get; set; } = true;

            [Option(Description = "The Acoustic Workbench website to download data from. Defaults to https://www.ecosounds.org")]
            public string WorkbenchApi { get; set; }

            [Option(Description = "The authentication token to use for the Acoustic Workbench website. If not specified you will prompted for log in credentials.")]
            public string AuthenticationToken { get; set; }

            [Option(Description = "Whether or not run this analysis in parallel - multiple segments can be analyzed at the same time")]
            public bool Parallel { get; set; }

            /* Arguments for local event analysis:

            [Option(
                Description = "TimeSpan offset hint required if file names do not contain time zone info. NO DEFAULT IS SET",
                ShortName = "z")]
            public TimeSpan? TimeSpanOffsetHint { get; set; }
            */

            public override Task<int> Execute(CommandLineApplication app)
            {
                return EventStatisticsEntry.ExecuteAsync(this);
            }
        }
    }
}