// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RecognizerEntry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the RecognizerEntry type.
//
// NOTE:  The action type to call a recognizer is "EventRecognizer".
//         The action name should be the first argument on the command line.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Tools;
    using AnalysisBase;
    using AnalysisBase.Extensions;

    using AnalysisPrograms.AnalyseLongRecordings;

    using AudioAnalysisTools;
    using log4net;
    using McMaster.Extensions.CommandLineUtils;
    using Production;
    using Production.Arguments;

    public class RecognizerEntry
    {
        public const string CommandName = "EventRecognizer";
        private const string Description =
            "The entry point for all species or event recognizers. Only to be used on short recordings (< 2 mins) that exactly match the code assumptions (e.g. correct format, channels, sample rate)." +
            "This recognizer runs any IEventRecognizer. The recognizer run" +
            "follows the same rules as " + AnalyseLongRecording.CommandName;

        [Command(
            CommandName,
            Description = Description)]
        public class Arguments : SourceConfigOutputDirArguments
        {
            [Option(Description = "Sets the name of the analysis to run. If not set, analysis identifer is parsed from the config file name.")]
            public string AnalysisIdentifier { get; set; }

            public override Task<int> Execute(CommandLineApplication app)
            {
                RecognizerEntry.Execute(this);

                return this.Ok();
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(nameof(RecognizerEntry));

        /// <summary>
        /// This entrypoint should be used for testing short files (less than 2 minutes)
        /// </summary>
        public static void Execute(Arguments arguments)
        {
            MainEntry.WarnIfDevleoperEntryUsed("EventRecognizer entry does not do any audio maniuplation.");
            Log.Info("Running event recognizer");

            var sourceAudio = arguments.Source;
            var configFile = arguments.Config.ToFileInfo();
            var outputDirectory = arguments.Output;

            if (configFile == null)
            {
                throw new FileNotFoundException("No config file argument provided");
            }
            else if (!configFile.Exists)
            {
                Log.Warn($"Config file {configFile.FullName} not found... attempting to resolve config file");
                configFile = ConfigFile.Resolve(configFile.Name, Directory.GetCurrentDirectory().ToDirectoryInfo());
            }

            LoggedConsole.WriteLine("# Recording file:      " + sourceAudio.FullName);
            LoggedConsole.WriteLine("# Configuration file:  " + configFile);
            LoggedConsole.WriteLine("# Output folder:       " + outputDirectory);

            // find an appropriate event IAnalyzer
            IAnalyser2 recognizer = AnalyseLongRecording.FindAndCheckAnalyser<IEventRecognizer>(
                arguments.AnalysisIdentifier,
                configFile.Name);

            Log.Info("Attempting to run recognizer: " + recognizer.Identifier);

            Log.Info("Reading configuration file");
            Config configuration = ConfigFile.Deserialize<RecognizerBase.RecognizerConfig>(configFile);

            // get default settings
            AnalysisSettings analysisSettings = recognizer.DefaultSettings;

            // convert arguments to analysis settings
            analysisSettings = arguments.ToAnalysisSettings(
                analysisSettings,
                outputIntermediate: true,
                resultSubDirectory: recognizer.Identifier,
                configuration: configuration);

            // Enable this if you want the Config file ResampleRate parameter to work.
            // Generally however the ResampleRate should remain at 22050Hz for all recognizers.
            //analysisSettings.AnalysisTargetSampleRate = (int) configuration[AnalysisKeys.ResampleRate];

            // get transform input audio file - if needed
            Log.Info("Querying source audio file");
            var audioUtilityRequest = new AudioUtilityRequest()
            {
                TargetSampleRate = analysisSettings.AnalysisTargetSampleRate,
            };
            var preparedFile = AudioFilePreparer.PrepareFile(
                outputDirectory,
                sourceAudio,
                MediaTypes.MediaTypeWav,
                audioUtilityRequest,
                outputDirectory);

            var source = preparedFile.SourceInfo.ToSegment();
            var prepared = preparedFile.TargetInfo.ToSegment(FileSegment.FileDateBehavior.None);
            var segmentSettings = new SegmentSettings<FileInfo>(
                analysisSettings,
                source,
                (analysisSettings.AnalysisOutputDirectory, analysisSettings.AnalysisTempDirectory),
                prepared);

            if (preparedFile.TargetInfo.SampleRate.Value != analysisSettings.AnalysisTargetSampleRate)
            {
                Log.Warn("Input audio sample rate does not match target sample rate");
            }

            // Execute a pre analyzer hook
            recognizer.BeforeAnalyze(analysisSettings);

            // execute actual analysis - output data will be written
            Log.Info("Running recognizer: " + recognizer.Identifier);
            AnalysisResult2 results = recognizer.Analyze(analysisSettings, segmentSettings);

            // run summarize code - output data can be written
            Log.Info("Running recognizer summary: " + recognizer.Identifier);
            recognizer.SummariseResults(
                analysisSettings,
                source,
                results.Events,
                results.SummaryIndices,
                results.SpectralIndices,
                new[] { results });

            //Log.Info("Recognizer run, saving extra results");
            // TODO: Michael, output anything else as you wish.

            Log.Debug("Clean up temporary files");
            if (source.Source.FullName != prepared.Source.FullName)
            {
                prepared.Source.Delete();
            }

            int eventCount = results?.Events?.Length ?? 0;
            Log.Info($"Number of detected events: {eventCount}");
            Log.Success(recognizer.Identifier + " recognizer has completed");
        }
    }
}
