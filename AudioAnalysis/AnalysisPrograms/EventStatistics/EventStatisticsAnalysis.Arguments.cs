// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalyseLongRecordings.Arguments.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the AnalyseLongRecording type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.EventStatistics
{
    using System;
    using System.IO;
    using AnalysisBase;
    using PowerArgs;
    using Production;

    public partial class EventStatisticsAnalysis
    {
        [CustomDetailedDescription]
        public class Arguments : SourceConfigOutputDirArguments, IArgClassValidator
        {
            public Arguments()
            {
            }

            [ArgDescription("The source event (annotations) file to operate on")]
            [Production.ArgExistingFile()]
            [ArgPosition(1)]
            [ArgRequired]
            public new FileInfo Source { get; set; }

            [ArgDescription("A TEMP directory where cut files will be stored. Use this option for efficiency (e.g. write to a RAM Disk).")]
            [Production.ArgExistingDirectory]
            public DirectoryInfo TempDir { get; set; }

            [ArgDescription("An array of channels to select. Default is all channels.")]
            public int[] Channels { get; set; } = null;

            [ArgDescription("Mix all selected input channels down into one mono channel. Default is to mixdown.")]
            [DefaultValue(true)]
            public bool MixDownToMono { get; set; } = true;

            [ArgDescription("The Acoustic Workbench website to download data from. Defaults to https://www.ecosounds.org")]
            public string WorkbenchApi { get; set; }

            [ArgDescription("The authentication token to use for the Acoustic Workbench website. If not specified you will prompted for log in credentials.")]
            public string AuthenticationToken { get; set; }

            /* Arguments for local event analysis:

            [ArgDescription("TimeSpan offset hint required if file names do not contain time zone info. NO DEFAULT IS SET")]
            public TimeSpan? TimeSpanOffsetHint { get; set; }
            */

            public static string AdditionalNotes()
            {
                return @"
At this stage only remote analysis is supported - that means querying an Acoustic Workbench website.
Support may be added in the future for other data sources (e.g. local). If you want this, file an issue!

For remote resources, the input file needs to have either one of these sets of fields:
- AudioEventId/audio_event_id
- AudioRecordingId/audio_recording_id, EventStartSeconds/event_start_seconds, EventEndSeconds/event_end_seconds
";

            }

            public void Validate()
            {
            }
        }
    }
}