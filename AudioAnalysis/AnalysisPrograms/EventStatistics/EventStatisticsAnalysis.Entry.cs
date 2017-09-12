// <copyright file="EventStatisticsAnalysis.Entry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.EventStatistics
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Csv;
    using AcousticWorkbench.Orchestration;
    using AnalysisBase;
    using AnalysisBase.Segment;
    using AudioAnalysisTools.EventStatistics;
    using global::AcousticWorkbench;
    using global::AcousticWorkbench.Models;
    using SourcePreparers;

    public partial class EventStatisticsAnalysis
    {
        public static void Execute(Arguments arguments)
        {
            MainEntry.ExecuteAsync(ExecuteAsync, arguments);
        }

        public static async Task ExecuteAsync(Arguments arguments)
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
                arguments.Config = ConfigFile.ResolveConfigFile(
                    arguments.Config.ToString(),
                    Directory.GetCurrentDirectory().ToDirectoryInfo());
            }

            // if a temp dir is not given, use output dir as temp dir
            if (arguments.TempDir == null)
            {
                Log.Warn("No temporary directory provided, using backup directory");
            }

            // Remote: create an instance of our API helpers
            IApi api = arguments.WorkbenchApi.IsNullOrEmpty() ? Api.Default : Api.Parse(arguments.WorkbenchApi);

            // log some helpful messages
            Log.Info("Events file:         " + arguments.Source);
            Log.Info("Configuration file:  " + arguments.Config);
            Log.Info("Output folder:       " + arguments.Output);
            Log.Info("Temp File Directory: " + arguments.TempDir);
            Log.Info("Api:                 " + api);

            // derserialize the config file
            var configuration = Yaml.Deserialise<EventStatisticsConfiguration>(arguments.Config);

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

            var authenticatedApi = await task.TimeoutAfter(Service.ClientTimeout).ConfigureAwait(false);

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
            // NOTE: to save on I/O sometimes if events share the same audio block, then multiple events will be
            // bundled into the same analysis segment.
            var resolver = new EventMetadataResolver(
                authenticatedApi,
                AnalysisDurationSeconds,
                arguments.Parallel ? 25 : 1);
            var metadataTask = resolver.GetRemoteMetadata(events);

            // wait for 1 second per event - this should be an order of magnitude greater than what is needed
            ISegment<AudioRecording>[] segments = await metadataTask.TimeoutAfter(events.Length);

            // finally time to start preparing jobs
            ISourcePreparer preparer = new RemoteSourcePreparer(authenticatedApi, allowSegmentcutting: false);

            AnalysisCoordinator coordinator = new AnalysisCoordinator(
                preparer,
                SaveBehavior.Never,
                true,
                arguments.Parallel);

            // instantiate the Analysis
            EventStatisticsAnalysis analysis = new EventStatisticsAnalysis();

            AnalysisSettings settings = analysis.DefaultSettings;
            settings.AnalysisOutputDirectory = arguments.Output;
            settings.AnalysisTempDirectory = arguments.TempDir;
            settings.Configuration = configuration;

            var results = coordinator.Run(segments, analysis, settings);

            Log.Info("Executing summary");

            // TODO: implement if needed
            analysis.SummariseResults(settings, null, null, null, null, results);

            Log.Debug("Summary complete");

            var instanceOutputDirectory =
                AnalysisCoordinator.GetNamedDirectory(settings.AnalysisOutputDirectory, analysis);

            var resultName = FilenameHelpers.AnalysisResultPath(
                instanceOutputDirectory,
                arguments.Source,
                analysis.Identifier,
                "csv");

            // NOTE: we are only saving event files
            Log.Info($"Writing results to {resultName}");
            analysis.WriteEventsFile(resultName.ToFileInfo(), results.SelectMany(es => es.Events));
            Log.Debug("Writing events completed");

            Log.Success("Event statistics analysis complete!");
        }
    }
}