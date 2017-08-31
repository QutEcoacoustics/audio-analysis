// <copyright file="EventStatisticsAnalysis.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.EventStatistics
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Remoting.Messaging;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Csv;
    using AcousticWorkbench;
    using AcousticWorkbench.Orchestration;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AnalysisBase.Segment;
    using AudioAnalysisTools.EventStatistics;
    using CsvHelper;
    using global::AcousticWorkbench;
    using global::AcousticWorkbench.Models;
    using log4net;
    using SourcePreparers;

    public partial class EventStatisticsAnalysis : AbstractStrongAnalyser
    {
        private const double AnalysisDurationSeconds = 60.0;

        private static readonly ILog Log = LogManager.GetLogger(nameof(EventStatisticsAnalysis));

        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments), "Dev() is not supported for this analysis");
            }

            Log.Info("Event statistics analysis begin");

            // validate arguments

            if (!arguments.Source.Exists)
            {
                throw new FileNotFoundException("Cannot find source file", arguments.Source.FullName);
            }

            // try an automatically find the config file
            if (arguments.Config == null)
            {
                throw new FileNotFoundException("No config file argument provided");
            }
            else if (!arguments.Config.Exists)
            {
                Log.Warn($"Config file {arguments.Config.FullName} not found... attempting to resolve config file");

                // we use .ToString() here to get the original input string - Using fullname always produces an
                // absolute path wrt to pwd... we don't want to prematurely make assumptions:
                // e.g. We require a missing absolute path to fail... that wouldn't work with .Name
                // e.g. We require a relative path to try and resolve, using .FullName would fail the first absolute 
                //    check inside ResolveConfigFile
                arguments.Config = ConfigFile.ResolveConfigFile(arguments.Config.ToString(), Directory.GetCurrentDirectory().ToDirectoryInfo());
            }

            // if a temp dir is not given, use output dir as temp dir
            if (arguments.TempDir == null)
            {
                Log.Warn("No temporary directory provided, using output directory");
                arguments.TempDir = arguments.Output;
            }

            // Remote: create an instance of our API helpers
            IApi api = arguments.WorkbenchApi.IsNullOrEmpty() ? Api.Default : Api.Parse(arguments.WorkbenchApi);

            // log some helpful messages
            Log.Info("Events file:         " + arguments.Source);
            Log.Info("Configuration file:  " + arguments.Config);
            Log.Info("Output folder:       " + arguments.Output);
            Log.Info("Temp File Directory: " + arguments.TempDir);
            Log.Info("Api:                 " + api);

            // Remote: Test we can log in to the workbench
            var auth = new AuthenticationService(api);
            Task<IAuthenticatedApi> task;
            if (arguments.AuthenticationToken.IsNotWhitespace())
            {
                Log.Debug("Using token for authentication");
                task = auth.CheckLogin(arguments.AuthenticationToken);
            }
            else
            {
                //var username = LoggedConsole.Prompt("Enter your username or email for the acoustic workbench:");
                //var password = LoggedConsole.Prompt("Enter your password for the acoustic workbench:", forPassword: true);
                //task = auth.Login(username, password);
                task = auth.Login("bioacoustics@qut.edu.au", "tsettest");
            }

            LoggedConsole.WriteWaitingLine(task, "Logging into workbench...");
            task.Wait(Service.ClientTimeout);

            var authenticatedApi = task.Result;

            Log.Info("Login success" + authenticatedApi);

            // read events/annotation file
            Log.Info("Now reading input data");

            // doing a manual CSV read here to get desired column name flexibility
            var events = Csv.ReadFromCsv<ImportedEvent>(arguments.Source, throwOnMissingField: false).ToArray();

            if (events.Length == 0)
            {
                Log.Warn("No events imported - source file empty. Exiting");
                return;
            }

            Log.Info($"Events read, {events.Length} read.");

            // need to validate the events
            var invalidEvents = events.Where(e => !e.IsValid()).ToArray();

            if (invalidEvents.Length > 0)
            {
                throw new InvalidOperationException(
                    "Invalid event detected."
                    + $" {invalidEvents.Length} events are not valid. The first invalid event is {invalidEvents[0]}");
            }

            // next gather meta data for all events
            // and transform list of events into list of segments
            var resolver = new EventMetadataResolver(
                authenticatedApi,
                AnalysisDurationSeconds,
                arguments.Parallel ? 25 : 1);
            var metadataTask = resolver.GetRemoteMetadata(events);

            // wait for 1 second per event - this should be an order of magnitude greater than what is needed
            metadataTask.Wait(TimeSpan.FromSeconds(events.Length));

            ISegment<AudioRecording>[] segments = metadataTask.Result;

            // finally time to start preparing jobs
            ISourcePreparer preparer = new RemoteSourcePreparer(authenticatedApi);

            AnalysisCoordinator coordinator = new AnalysisCoordinator(preparer, SaveBehavior.Never, true, arguments.Parallel);

            // instantiate the Analysis
            EventStatisticsAnalysis analysis = new EventStatisticsAnalysis();

            AnalysisSettings settings = analysis.DefaultSettings;
            settings.AnalysisOutputDirectory = arguments.Output;
            settings.AnalysisTempDirectory = arguments.TempDir;

            var results = coordinator.Run(segments, analysis, settings);

            Log.Warn("INCOMPLETE");

        }

        public override string DisplayName { get; } = "Event statistics calculation";

        public override string Identifier { get; } = "Ecosounds.EventStatistics";

        public string Description { get; } = "Event statistics calculation analysis used to extract critical statistics (features) from an acoustic event";

        public AnalysisSettings DefaultSettings { get; } = new AnalysisSettings()
        {
            AnalysisMaxSegmentDuration = 60.0.Seconds(),
        };

        public void BeforeAnalyze(AnalysisSettings analysisSettings)
        {
            // noop
        }

        public override AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            var segment = (RemoteSegmentWithDatum)segmentSettings.Segment;
            var importedEvent = (ImportedEvent)segment.Datum;
            var temporalRange = new Range<TimeSpan>(
                importedEvent.EventStartSeconds.Value.Seconds(),
                importedEvent.EventEndSeconds.Value.Seconds());
            var spectralRange = new Range<double>(
                importedEvent.LowFrequencyHertz.Value,
                importedEvent.HighFrequencyHertz.Value);

            var configuration = (EventStatisticsConfiguration)analysisSettings.Configuration;

            var recording = new AudioAnalysisTools.WavTools.AudioRecording(segmentSettings.SegmentAudioFile);

            var statistics = EventStatisticsCalculate.AnalyzeAudioEvent(
                recording,
                temporalRange,
                spectralRange,
                configuration);

            var result = new AnalysisResult2(analysisSettings, segmentSettings, recording.Duration());

            result.Events = statistics.AsArray();

            return result;
        }

        public override void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
            Csv.WriteToCsv(destination, results);
        }

        public override void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public override List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public override void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // noop
        }
    }
}
