// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RecognizerEntry.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the RecognizerEntry type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using Acoustics.Tools.Audio;

    using AnalysisBase;

    using AnalysisPrograms.Production;

    using AudioAnalysisTools;
    using AudioAnalysisTools.WavTools;

    using log4net;

    public class RecognizerEntry
    {
        [CustomDetailedDescription]
        public class Arguments : SourceConfigOutputDirArguments
        {
            public static string AdditionalNotes()
            {
                return "This recognizer runs any IEventRecognizer. The recognizer run is based on on the "
                    + "Identifier field and parsed from the AnalysisName field in the config file of the same name";
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public static Arguments Dev()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This entrypoint should be used for testing short files (less than 2 minutes)
        /// </summary>
        /// <param name="arguments"></param>
        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            Log.Info("Running event recognizer");

            var sourceAudio = arguments.Source;
            var configFile = arguments.Config;
            var outputDirectory = arguments.Output;

            if (configFile == null)
            {
                throw new FileNotFoundException("No config file argument provided");
            }
            else if (!configFile.Exists)
            {
                Log.Warn($"Config file {configFile.FullName} not found... attempting to resolve config file");
                configFile = ConfigFile.ResolveConfigFile(configFile.Name, Directory.GetCurrentDirectory().ToDirectoryInfo());
            }

            LoggedConsole.WriteLine("# Recording file:      " + sourceAudio.FullName);
            LoggedConsole.WriteLine("# Configuration file:  " + configFile);
            LoggedConsole.WriteLine("# Output folder:       " + outputDirectory);

            // get some basic info about the source file
            Log.Info("Querying source audio file");
            IAudioUtility audioUtility = new MasterAudioUtility();
            var info = audioUtility.Info(arguments.Source); // Get duration of the source file

            Log.Info("Reading configuration file");
            dynamic configuration = Yaml.Deserialise(configFile);
            string analysisIdentifier = configuration[AnalysisKeys.AnalysisName];

            Log.Info("Attempting to run recognizer: " + analysisIdentifier);

            // find an appropriate event IAnalyzer
            IAnalyser2 recognizer = AnalyseLongRecordings.AnalyseLongRecording.FindAndCheckAnalyser(analysisIdentifier);

            // get default settings
            AnalysisSettings analysisSettings = recognizer.DefaultSettings;

            // convert arguments to analysis settings
            analysisSettings = arguments.ToAnalysisSettings(analysisSettings, outputIntermediate: true);
            analysisSettings.Configuration = configuration;
            analysisSettings.SampleRateOfOriginalAudioFile = info.SampleRate;
            // we don't want segments, thus segment duration == total length of original file
            analysisSettings.SegmentDuration = info.Duration;
            analysisSettings.SegmentMaxDuration = info.Duration;

            if (info.SampleRate.Value != analysisSettings.SegmentTargetSampleRate)
            {
                Log.Warn("Input audio sample rate does not match target sample rate");
            }

            if (MediaTypes.CanonicaliseMediaType(info.MediaType) != MediaTypes.MediaTypeWav)
            {
                throw new ArgumentException("This entry point only supports wav file types!");
            }

            // execute actual analysis - output data will be written
            Log.Info("Running recognizer: " + analysisIdentifier);
            AnalysisResult2 results = recognizer.Analyze(analysisSettings);

            // run summarize code - output data can be written
            Log.Info("Running recognizer summary: " + analysisIdentifier);
            var fileSegment = new FileSegment(analysisSettings.AudioFile, false, true);
            recognizer.SummariseResults(
                analysisSettings,
                fileSegment,
                results.Events,
                results.SummaryIndices,
                results.SpectralIndices,
                new[] { results });

            Log.Info("Recognizer run, saving results");

            // TODO: Michael, output anything else as you wish.


            Log.Info("Recognizer complete");
        }
    }
}
