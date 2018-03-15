// <copyright file="MainArgs.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Arguments
{
    using System;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using AnalyseLongRecordings;
    using Draw.Zooming;
    using EventStatistics;
    using McMaster.Extensions.CommandLineUtils;
    using Recognizers.Base;

    [Command(
        Meta.Name,
        AllowArgumentSeparator = true,
        Description = Meta.Description,
        ThrowOnUnexpectedArgument = true)]
    [HelpOption(Inherited = true, ShowInHelpText = true)]
    [Subcommand("help", typeof(HelpArgs))]
    [Subcommand("list", typeof(ListArgs))]
    [Subcommand("AnalysesAvailable", typeof(AnalysesAvailable))]
    [Subcommand(CheckEnvironment.CommandName, typeof(CheckEnvironment.Arguments))]
    [Subcommand(AnalyseLongRecording.CommandName, typeof(AnalyseLongRecording.Arguments))]
    [Subcommand(Audio2Sonogram.CommandName, typeof(Audio2Sonogram.Arguments))]
    [Subcommand(Create4Sonograms.CommandName, typeof(Create4Sonograms.Arguments))]
    [Subcommand(DrawSummaryIndexTracks.CommandName, typeof(DrawSummaryIndexTracks.Arguments))]
    [Subcommand(EventStatisticsEntry.CommandName, typeof(EventStatisticsEntry.Arguments))]
    [Subcommand(RecognizerEntry.CommandName, typeof(RecognizerEntry.Arguments))]
    [Subcommand(AudioCutter.CommandName, typeof(AudioCutter.Arguments))]
    [Subcommand(AudioFileCheck.CommandName, typeof(AudioFileCheck.Arguments))]
    [Subcommand(Aed.CommandName, typeof(Aed.Arguments))]
    [Subcommand(ConcatenateIndexFiles.CommandName, typeof(ConcatenateIndexFiles.Arguments))]
    [Subcommand(DrawLongDurationSpectrograms.CommandName, typeof(DrawLongDurationSpectrograms.Arguments))]
    [Subcommand(DrawZoomingSpectrograms.CommandName, typeof(DrawZoomingSpectrograms.Arguments))]
    [Subcommand(DrawEasyImage.CommandName, typeof(DrawEasyImage.Arguments))]
    [Subcommand(Audio2InputForConvCnn.CommandName, typeof(Audio2InputForConvCnn.Arguments))]
    [Subcommand(DifferenceSpectrogram.CommandName, typeof(DifferenceSpectrogram.Arguments))]
    [Subcommand(EPR.CommandName, typeof(EPR.Arguments))]
    [Subcommand(GroundParrotRecogniser.CommandName, typeof(GroundParrotRecogniser.Arguments))]
    [Subcommand(LSKiwi3.CommandName, typeof(LSKiwi3.Arguments))]
    [Subcommand(LSKiwiROC.CommandName, typeof(LSKiwiROC.Arguments))]
    [Subcommand(SnrAnalysis.CommandName, typeof(SnrAnalysis.Arguments))]
    [Subcommand(OscillationsGeneric.CommandName, typeof(OscillationsGeneric.Arguments))]
    [Subcommand(Segment.CommandName, typeof(Segment.Arguments))]
    [Subcommand(SurfAnalysis.CommandName, typeof(SurfAnalysis.Arguments))]
    [Subcommand(SpeciesAccumulationCurve.CommandName, typeof(SpeciesAccumulationCurve.Arguments))]
    [Subcommand(SPT.CommandName, typeof(SPT.Arguments))]
    [Subcommand(DummyAnalysis.CommandName, typeof(DummyAnalysis.Arguments))]
    [Subcommand(FileRenamer.CommandName, typeof(FileRenamer.Arguments))]
    [Subcommand(Sandpit.CommandName, typeof(Sandpit.Arguments))]
    public class MainArgs
    {
        private async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            MainEntry.BeforeExecute(this, app);

            MainEntry.PrintUsage("A command must be provided, here are some suggestions:", MainEntry.Usages.All);
            return ExceptionLookup.ActionRequired;
        }

        private LogVerbosity logLevel = LogVerbosity.Info;

        public MainArgs()
        {
            this.DebugOption = DebugOptions.Prompt;
        }

        [Option(
            Description =
                "Do not show the debug prompt AND automatically attach a debugger. Has no effect in RELEASE builds",
            Inherited = true,
            ShortName = "D")]
        public bool Debug { get; set; }

        [Option(
            Description =
                "Do not show the debug prompt and do not attach a debugger. Has no effect in RELEASE builds",
            Inherited = true,
            ShortName = "n")]
        public bool NoDebug { get; set; }

        public DebugOptions DebugOption
        {
            get
            {
                if (this.NoDebug)
                {
                    return DebugOptions.No;
                }
                else if (this.Debug)
                {
                    return DebugOptions.Yes;
                }
                else
                {
                    return DebugOptions.Prompt;
                }
            }

            set
            {
                switch (value)
                {
                    case DebugOptions.Yes:
                        this.NoDebug = false;
                        this.Debug = true;
                        break;
                    case DebugOptions.No:
                        this.Debug = false;
                        this.NoDebug = true;
                        break;
                    default:
                        this.Debug = this.NoDebug = false;
                        break;
                }
            }
        }

        [Option(
            Description = "Set the log vebosity level. Valid values: None = 0, Error = 1, Warn = 2, Info = 3, Debug = 4, Trace = 5, Verbose = 6, All = 7",
            Inherited = true,
            ShortName = null)]
        public LogVerbosity LogLevel
        {
            get
            {
                if (this.Verbose)
                {
                    return LogVerbosity.Debug;
                }

                if (this.VVerbose)
                {
                    return LogVerbosity.Trace;
                }

                if (this.VVVerbose)
                {
                    return LogVerbosity.All;
                }

                return this.logLevel;
            }

            set
            {
                this.Verbose = value == LogVerbosity.Debug;
                this.VVerbose = value == LogVerbosity.Trace;
                this.VVVerbose = value == LogVerbosity.All;

                this.logLevel = value;
            }
        }

        [Option(
            "-v",
            Inherited = true,
            Description = "Set the logging to be verbose. Equivalent to LogLevel = Debug = 4")]
        public bool Verbose { get; set; }

        [Option(
            "-vv",
            Inherited = true,
            Description = "Set the logging to very verbose. Equivalent to LogLevel = Trace = 4")]
        public bool VVerbose { get; set; }

        [Option(
            "-vvv",
            Inherited = true,
            Description = "Set the logging to very very verbose. Equivalent to LogLevel = ALL = 7")]
        public bool VVVerbose { get; set; }

        [Option(
            "--quiet",
            Inherited = true,
            Description = "Reduce console logging to WARN and ERROR. Full logs still logged in file.")]
        public bool QuietConsole { get; set; }
    }
}
