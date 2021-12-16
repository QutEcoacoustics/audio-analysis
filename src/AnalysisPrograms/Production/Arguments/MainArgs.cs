// <copyright file="MainArgs.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Arguments
{
    using Acoustics.Shared;
    using AnalysisPrograms.AnalyseLongRecordings;
    using AnalysisPrograms.Download;
    using AnalysisPrograms.Draw.RibbonPlots;
    using AnalysisPrograms.Draw.Zooming;
    using AnalysisPrograms.EventStatistics;
    using AnalysisPrograms.Recognizers.Base;
    using AnalysisPrograms.SpectrogramGenerator;
    using McMaster.Extensions.CommandLineUtils;
    using System.Threading.Tasks;

    [Command(
        Meta.Name,
        AllowArgumentSeparator = true,
        Description = Meta.Description,
        UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.Throw)]
    [HelpOption(Inherited = true, ShowInHelpText = true)]
    [VersionOption(BuildMetadata.VersionString, Template = "--version")]
    [Subcommand(
        typeof(HelpArgs),
        typeof(ListArgs),
        typeof(DownloadCommand),
        typeof(AnalysesAvailable),
        typeof(CheckEnvironment.Arguments),
        typeof(AnalyseLongRecording.Arguments),
        typeof(Audio2Sonogram.Arguments),
        typeof(Create4Sonograms.Arguments),
        typeof(DrawSummaryIndexTracks.Arguments),
        typeof(EventStatisticsEntry.Arguments),
        typeof(RecognizerEntry.Arguments),
        typeof(AudioCutter.Arguments),
        typeof(AudioFileCheck.Arguments),
        typeof(Aed.Arguments),
        typeof(ConcatenateIndexFiles.Arguments),
        typeof(DrawLongDurationSpectrograms.Arguments),
        typeof(DrawZoomingSpectrograms.Arguments),
        typeof(RibbonPlot.Arguments),
        typeof(DrawEasyImage.Arguments),
        typeof(ContentDescription.BuildModel.Arguments),
        typeof(Audio2InputForConvCnn.Arguments),
        typeof(DifferenceSpectrogram.Arguments),
        typeof(EPR.Arguments),
        typeof(GroundParrotRecogniser.Arguments),
        typeof(LSKiwi3.Arguments),
        typeof(LSKiwiROC.Arguments),
        typeof(SnrAnalysis.Arguments),
        typeof(OscillationsGeneric.Arguments),
        typeof(Segment.Arguments),
        typeof(SurfAnalysis.Arguments),
        typeof(SpeciesAccumulationCurve.Arguments),
        typeof(SPT.Arguments),
        typeof(DummyAnalysis.Arguments),
        typeof(FileRenamer.Arguments),
        typeof(Sandpit.Arguments),
        typeof(MahnooshSandpit.Arguments))]
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
            Description = "Set the log verbosity level. Valid values: None = 0, Error = 1, Warn = 2, Info = 3, Debug = 4, Trace = 5, Verbose = 6, All = 7",
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
            Description = "Set the logging to very verbose. Equivalent to LogLevel = Trace = 5")]
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