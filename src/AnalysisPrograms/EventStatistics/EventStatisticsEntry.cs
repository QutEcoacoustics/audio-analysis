// <copyright file="EventStatisticsEntry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.EventStatistics
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Csv;
    using AcousticWorkbench.Orchestration;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AnalysisBase.Segment;
    using AudioAnalysisTools.EventStatistics;
    using global::AcousticWorkbench;
    using global::AcousticWorkbench.Models;
    using log4net;
    using Production;
    using SourcePreparers;

    public partial class EventStatisticsEntry
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(EventStatisticsAnalysis));

        public static async Task<int> ExecuteAsync(Arguments arguments)
        {
            Log.Info("Event statistics analysis begin");

            // validate arguments

            var input = arguments.Source;
            var config = arguments.Config.ToFileInfo();

            if (!input.Exists)
            {
                throw new FileNotFoundException("Cannot find source file", input.FullName);
            }

            // try an automatically find the config file
            if (config == null)
            {
                throw new FileNotFoundException("No config file argument provided");
            }
            else if (!config.Exists)
            {
                Log.Warn($"Config file {config.FullName} not found... attempting to resolve config file");

                // we use  the original input string - Using FileInfo fullname always produces an
                // absolute path wrt to pwd... we don't want to prematurely make assumptions:
                // e.g. We require a missing absolute path to fail... that wouldn't work with .Name
                // e.g. We require a relative path to try and resolve, using .FullName would fail the first absolute
                //    check inside ResolveConfigFile
                config = ConfigFile.Resolve(arguments.Config, Directory.GetCurrentDirectory().ToDirectoryInfo());
            }

            // if a temp dir is not given, use output dir as temp dir
            if (arguments.TempDir == null)
            {
                Log.Warn("No temporary directory provided, using backup directory");
            }

            // Remote: create an instance of our API helpers
            IApi api = arguments.WorkbenchApi.IsNullOrEmpty() ? Api.Default : Api.Parse(arguments.WorkbenchApi);

            // log some helpful messages
            Log.Info("Events file:         " + input);
            Log.Info("Configuration file:  " + config);
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
                var username = LoggedConsole.Prompt("Enter your username or email for the acoustic workbench:");
                var password = LoggedConsole.Prompt("Enter your password for the acoustic workbench:", forPassword: true);
                task = auth.Login(username, password);

                //task = auth.Login("bioacoustics@qut.edu.au", "tsettest");
            }

            LoggedConsole.WriteWaitingLine(task, "Logging into workbench...");

            var authenticatedApi = await task.TimeoutAfter(Service.ClientTimeout).ConfigureAwait(false);

            Log.Info("Login success" + authenticatedApi);

            // read events/annotation file
            Log.Info("Now reading input data");

            // Read events from provided CSV file.
            // Also tag them with an order index to allow sorting in the same order as they were provided to us.
            var events = Csv
                .ReadFromCsv<ImportedEvent>(input, throwOnMissingField: false)
                .Select(
                    (x, i) =>
                    {
                        x.Order = i;
                        return x;
                    })
                .ToArray();

            if (events.Length == 0)
            {
                Log.Warn("No events imported - source file empty. Exiting");
                return ExceptionLookup.NoData;
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
                PaddingFunction,
                arguments.Parallel ? 25 : 1);
            var metadataTask = resolver.GetRemoteMetadata(events);

            // wait for 1 second per event - this should be an order of magnitude greater than what is needed
            ISegment<AudioRecording>[] segments = await metadataTask.TimeoutAfter(events.Length);

            Log.Info($"Metadata collected, preparing to start analysis");

            // finally time to start preparing jobs
            ISourcePreparer preparer = new RemoteSourcePreparer(authenticatedApi, allowSegmentcutting: false);

            AnalysisCoordinator coordinator = new AnalysisCoordinator(
                preparer,
                SaveBehavior.Never,
                uniqueDirectoryPerSegment: false,
                isParallel: arguments.Parallel);

            // instantiate the Analysis
            EventStatisticsAnalysis analysis = new EventStatisticsAnalysis();

            // derserialize the config file
            var configuration = analysis.ParseConfig(config);

            AnalysisSettings settings = analysis.DefaultSettings;
            settings.AnalysisOutputDirectory = arguments.Output;
            settings.AnalysisTempDirectory = arguments.TempDir;
            settings.Configuration = configuration;

            var results = coordinator.Run(segments, analysis, settings);

            var allEvents = results.SelectMany(es => es.Events).ToArray();

            var eventsWithErrors = allEvents.Count(x => ((EventStatistics)x).Error);
            if (eventsWithErrors > 0)
            {
                Log.Warn($"Errors occurred when calculating statistics for {eventsWithErrors} events.");
            }

            Log.Trace("Sorting event statistics results");
            Array.Sort(allEvents);

            Log.Info("Executing summary");

            // TODO: implement if needed
            analysis.SummariseResults(settings, null, allEvents, null, null, results);

            Log.Debug("Summary complete");

            var instanceOutputDirectory =
                AnalysisCoordinator.GetNamedDirectory(settings.AnalysisOutputDirectory, analysis);

            var resultName = FilenameHelpers.AnalysisResultPath(
                instanceOutputDirectory,
                input,
                analysis.Identifier,
                "csv");

            // NOTE: we are only saving event files
            Log.Info($"Writing results to {resultName}");
            analysis.WriteEventsFile(resultName.ToFileInfo(), allEvents.AsEnumerable());
            Log.Debug("Writing events completed");

            var summaryStats = new
            {
                numberEvents = allEvents.Length,
                durationEvents = allEvents.Sum(x => ((EventStatistics)x).EventDurationSeconds),
                numberRecordings = allEvents.Select(x => ((EventStatistics)x).AudioRecordingId).Distinct().Count(),
                durationAudioProcessed = results.Sum(x => x.SegmentAudioDuration.TotalSeconds),
                remoteAudioDownloaded = (preparer as RemoteSourcePreparer)?.TotalBytesRecieved,
            };
            Log.Info("Summary statistics:\n" + Json.SerializeToString(summaryStats));

            Log.Success("Event statistics analysis complete!");

            return ExceptionLookup.Ok;
        }

        /// <summary>
        /// Add this much paddding to each acoustic event.
        /// Returns the total padding, half of which will be symmetrically to either side, or assymertrically if
        /// boundary effects occur.
        /// </summary>
        private static double PaddingFunction(double eventDurationSeconds)
        {
            const double minimum = 10;
            return Math.Max(minimum, Math.Ceiling(eventDurationSeconds * 2.0));
        }
    }
}