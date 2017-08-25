namespace AnalysisPrograms.Production
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using Acoustics.Shared;
    using Acoustics.Shared.Extensions;
    using AnalysisBase;
    using PowerArgs;
    using TowseyLibrary;

    public enum DebugOptions
    {
        No = 0,
        Yes = 1,
        Prompt = 3
    }

    public partial class MainEntryArguments
    {
        private LogVerbosity logLevel;

        public MainEntryArguments()
        {
            this.DebugOption = DebugOptions.Prompt;
        }

        [ArgRequired]
        [ArgPosition(0)]
        public string Action { get; set; }

        [ArgDescription("Do not show the debug prompt AND automatically attach a debugger. Has no effect in RELEASE builds")]
        [DefaultValue(false)]
        public bool Debug { get; set; }

        [ArgDescription("Do not show the debug prompt or attach a debugger. Has no effect in RELEASE builds")]
        [DefaultValue(false)]
        public bool NoDebug { get; set; }

        [ArgIgnore]
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

        [DefaultValue(LogVerbosity.Info)]
        [ArgDescription("Set the logging. Valid values: None = 0, Error = 1, Warn = 2, Info = 3, Debug = 4, Trace = 5, Verbose = 6, All = 7")]
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

        [ArgDescription("Set the logging to be verbose. Equivalent to LogLevel = Debug = 4")]
        public bool Verbose { get; set; }

        [ArgDescription("Set the logging to very verbose. Equivalent to LogLevel = Trace = 4")]
        public bool VVerbose { get; set; }

        [ArgDescription("Set the logging to very very verbose. Equivalent to LogLevel = ALL = 7")]
        public bool VVVerbose { get; set; }

        [ArgDescription("Reduce StdOut logging to WARN and ERROR. The default is false.")]
        public bool QuietConsole { get; set; }
    }

    public enum LogVerbosity
    {
        None = 0,
        Error = 1,
        Warn = 2,
        Info = 3,
        Debug = 4,
        Trace = 5,
        Verbose = 6,
        All = 7
    }

    public class HelpArguments
    {
        [ArgPosition(1)]
        public string ActionName { get; set; }
    }

    public class SourceArguments
    {

        [ArgDescription("The source audio file to operate on")]
        [ArgExistingFile()]
        [ArgPosition(1)]
        [ArgRequired]
        public FileInfo Source { get; set; }
    }

    public class SourceAndConfigArguments : SourceArguments
    {
        [ArgDescription("The path to the config file. If not found it will attempt to use the default config file of the same name.")]
        [ArgRequired]
        [ArgPosition(2)]
        public FileInfo Config { get; set; }
    }

    public class SourceConfigOutputDirArguments : SourceAndConfigArguments
    {
        [ArgDescription("A directory to write output to")]
        [ArgExistingDirectory(createIfNotExists: true)]
        [ArgRequired]
        [ArgPosition(3)]
        public virtual DirectoryInfo Output { get; set; }

        /// <summary>
        /// Helper method used for Execute and Dev entry points. Mocks the values normally set by analysis coordinator.
        /// </summary>
        /// <param name="defaults">
        /// The default AnalysisSettings used - usually from the IAnalyzer2 interface.
        /// </param>
        /// <param name="outputIntermediate">
        /// The output Intermediate switch - true to use the default writing behavior.
        /// </param>
        /// <returns>
        /// An AnalysisSettings object.
        /// </returns>
        public virtual AnalysisSettings ToAnalysisSettings(
            AnalysisSettings defaults = null,
            bool outputIntermediate = false,
            string resultSubDirectory = null,
            dynamic configuration = null)
        {
            var analysisSettings = defaults ?? new AnalysisSettings();

            analysisSettings.ConfigFile = this.Config;

            var resultDirectory = resultSubDirectory.IsNullOrEmpty() ? this.Output : this.Output.Combine(resultSubDirectory);
            resultDirectory.Create();

            analysisSettings.AnalysisOutputDirectory = this.Output;
            analysisSettings.AnalysisTempDirectory = this.Output;

            if (outputIntermediate)
            {
                analysisSettings.AnalysisImageSaveBehavior = SaveBehavior.Always;
                analysisSettings.AnalysisDataSaveBehavior = true;
            }

            analysisSettings.Configuration = configuration ?? Yaml.Deserialise(this.Config);

            return analysisSettings;

        }
    }

    public class AnalyserArguments : SourceConfigOutputDirArguments
    {
        [ArgDescription("The start offset to start analysing from (in seconds)")]
        [ArgRange(0, double.MaxValue)]
        public double? Start { get; set; }

        [ArgDescription("The duration of each segment to analyse (seconds) - a maximum of 10 minutes")]
        [ArgRange(0, 10 * 60)]
        public double? Duration { get; set; }

        public (AnalysisSettings, SegmentSettings<FileInfo>) ToAnalysisSettings(
            AnalysisSettings defaults = null,
            bool outputIntermediate = false,
            FileSegment sourceSegment = null,
            FileSegment preparedSegment = null)
        {
            var analysisSettings = base.ToAnalysisSettings(defaults, true);

            var segment = sourceSegment ?? new FileSegment(this.Source, TimeAlignment.None);
            var segmentSettings = new SegmentSettings<FileInfo>(
                analysisSettings,
                segment,
                (analysisSettings.AnalysisOutputDirectory, analysisSettings.AnalysisTempDirectory),
                preparedSegment ?? segment);

            return (analysisSettings, segmentSettings);
        }
    }
}
