// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RecognizerEntry.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
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
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Tools;
    using AnalysisBase;

    using AnalysisPrograms.Production;
    using AudioAnalysisTools;
    using log4net;

    public class RecognizerEntry
    {
        [CustomDetailedDescription]
        public class Arguments : SourceConfigOutputDirArguments
        {
            public static string AdditionalNotes()
            {
                return "This recognizer runs any IEventRecognizer. The recognizer run is based on the "
                    + "Identifier field and parsed from the AnalysisName field in the config file of the same name";
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public static Arguments Dev()
        {
            // The MULTI-RECOGNISER
            // Canetoad, Litoria fallax and Limnodynastes convex.
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\3mile_creek_dam_-_Herveys_Range_1076_248366_20130305_001700_30.wav";
            //string outputPath = @"C:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016Sept\Multi";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Ecosounds.MultiRecognizer.yml";

            //Ardea insignis (The White-bellied Herron
            string recordingPath = @"G:\SensorNetworks\WavFiles\Bhutan\Heron_commonCall_16.wav";
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Bhutan\Both call types of Heron.wav";
            string outputPath = @"G:\SensorNetworks\Output\Bhutan\";
            string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.ArdeaInsignis.yml";

            // Canetoad
            //string recordingPath = @"C:\Work\GitHub\recognizer-tests\tests\species\Rhinella_marina\data\CaneToad_Gympie_resampled.wav";   // Positive call   
            //string recordingPath = @"C:\Work\GitHub\recognizer-tests\tests\species\Rhinella_marina\data\Lwotjulumensis_trill_bickerton_20131212_214430.wav";    // Positive call
            //string recordingPath = @"C:\Work\GitHub\recognizer-tests\tests\species\Rhinella_marina\data\TruckMotor_20150603_004248.wav"; // Negative call
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Canetoad\FalsePositives\FalsePosFromPaul_2015-06-02-031015_downsampled.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Canetoad\FalsePositives\FalsePosFromPaul_2015-06-03-004248_downsampled.wav";
            //string outputPath = @"C:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016Sept\Canetoad";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Towsey.RhinellaMarina.yml";

            //Crinia tinnula
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\Crinia\CriniaTinnula.wav";
            //string outputPath = @"C:\SensorNetworks\Output\Frogs\TestOfRecognisers-2016October";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Stark.CriniaTinnula.yml";

            // Cyclorana novaehollandiae
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\28 Cyclorana novaehollandiae.mp3";
            //string outputPath = @"C:\SensorNetworks\Output\Frogs\TestOfRecognisers-2016Sept\Test";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.CycloranaNovaeholl.yml";

            // Lewin's Rail  --  Lewinia pectoralis
            //string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-084607.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-062040.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-075040.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav";
            //string configPath    = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Towsey.LewiniaPectoralis.yml";
            //string outputPath     = @"C:\SensorNetworks\Output\LewinsRail\";
            // LEWIN'S RAIL TEST
            //string recordingPath = @"E:\SensorNetworks\SoftwareTests\Test_LewinsRail\Data\BAC2_20071008-085040.wav";
            //string configPath = @"E:\SensorNetworks\SoftwareTests\Test_LewinsRail\Data\Towsey.LewiniaPectoralis.yml";
            //string outputPath = @"E:\SensorNetworks\SoftwareTests\Test_LewinsRail\";

            // Limnodynastes convex
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\3mile_creek_dam_-_Herveys_Range_1076_248366_20130305_001700_30.wav";
            //string outputPath = @"C:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016Sept\LimnoConvex";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Towsey.LimnodynastesConvex.yml";

            // Litoria bicolor
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\Bickerton\bicolor_bickerton_island_1013_255205_20131211_191621_30_0.wav";
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\Bickerton\bicolor_bickerton_island_1013_255205_20131211_195821_30_0.wav";
            //string outputPath = @"G:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016October\";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Towsey.LitoriaBicolor.yml";

            // Litoria fallax
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\53 Litoria fallax.mp3";
            //string outputPath = @"C:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016Sept\Test";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.LitoriaFallax.yml";

            // Litoria nasuta
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\43 Litoria nasuta.mp3";
            //string outputPath = @"C:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016Sept\Test";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.LitoriaNasuta.yml";

            // Litoria olongburensis
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\TEST_16000Hz_LitoriaOlongburensis.wav";
            //string outputPath = @"C:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016Sept\Canetoad";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Canetoad.yml";

            // Litoria rothii.
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\49 Litoria rothii.mp3";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\69 Litoria rothii.mp3";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\LitoriaWotjulumensisAndRothii\bickerton_island_1013_255205_20131211_194041_30_0.wav";
            //string outputPath = @"C:\SensorNetworks\Output\Frogs\TestOfRecognisers-2016Sept\Test";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.LitoriaRothii.yml";

            // Litoria rubella
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\50 Litoria rubella.mp3";
            //string outputPath = @"C:\SensorNetworks\Output\Frogs\TestOfRecognisers-2016Sept\Test";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.LitoriaRubella.yml";

            // Platyplectrum ornatum
            //string recordingPath = @"E:\SensorNetworks\WavFiles\Frogs\PlatyplectrumSp\p_ornatum_bickerton_island_1013_255599_20140213_214500_30_0.wav";
            //string outputPath = @"E:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016October\";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Towsey.PlatyplectrumOrnatum.yml";

            // Uperoleia inundata
            //string recordingPath = @"E:\SensorNetworks\WavFiles\Frogs\UperoleiaSp\u_inundata_bickerton_island_1013_255713_20140112_212900_30_0.wav";
            //string recordingPath = @"E:\SensorNetworks\WavFiles\Frogs\UperoleiaSp\u_inundata_bickerton_island_1013_255713_20140112_213030_30_0.wav";
            //string configPath    = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.UperoleiaInundata.yml";
            //string outputPath    = @"E:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016October";

            // Uperoleia mimula
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\23 Uperoleia mimula.mp3";
            //string outputPath = @"C:\SensorNetworks\Output\Frogs\TestOfRecognisers-2016Sept\Test";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.UperoleiaMimula.yml";

            // Fresh water cat fish
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Freshwater\BlueCatfish_LonePine_ChrisAfterFiltering.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Freshwater\BlueCatfish_LonePine_LeftChannel_First60s.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Freshwater\BlueCatfish_LonePine_LeftChannel.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Freshwater\BlueCatfish_LonePine_ChrisFilteredLeftChFirst60s.wav";
            //string outputPath = @"C:\SensorNetworks\Output\FreshWater";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.IctalurusFurcatus.yml";

            var arguments = new Arguments
            {
                Source = recordingPath.ToFileInfo(),
                Config = configPath.ToFileInfo(),
                Output = outputPath.ToDirectoryInfo()
            };

            //// #########  NOTE: All other parameters are set in the .yml file assigned to configPath variable above.
            //if (!arguments.Source.Exists)
            //{
            //    Log.Warn(" >>>>>>>>>>>> WARNING! The Source Recording file cannot be found! This will cause an exception.");
            //}
            //if (!arguments.Config.Exists)
            //{
            //    Log.Warn(" >>>>>>>>>>>> WARNING! The Configuration file cannot be found! This will cause an exception.");
            //}

            return arguments;
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
                arguments.Config = configFile = ConfigFile.ResolveConfigFile(configFile.Name, Directory.GetCurrentDirectory().ToDirectoryInfo());
            }

            LoggedConsole.WriteLine("# Recording file:      " + sourceAudio.FullName);
            LoggedConsole.WriteLine("# Configuration file:  " + configFile);
            LoggedConsole.WriteLine("# Output folder:       " + outputDirectory);


            Log.Info("Reading configuration file");
            dynamic configuration = Yaml.Deserialise(configFile);
            string analysisIdentifier = configuration[AnalysisKeys.AnalysisName];

            Log.Info("Attempting to run recognizer: " + analysisIdentifier);

            // find an appropriate event IAnalyzer
            IAnalyser2 recognizer = AnalyseLongRecordings.AnalyseLongRecording.FindAndCheckAnalyser(analysisIdentifier);

            // get default settings
            AnalysisSettings analysisSettings = recognizer.DefaultSettings;

            // convert arguments to analysis settings
            analysisSettings = arguments.ToAnalysisSettings(analysisSettings, outputIntermediate: true, resultSubDirectory: recognizer.Identifier);
            analysisSettings.Configuration = configuration;

            // get transform input audio file - if needed
            Log.Info("Querying source audio file");
            var audioUtilityRequest = new AudioUtilityRequest()
            {
                TargetSampleRate = analysisSettings.SegmentTargetSampleRate
            };
            var preparedFile = AudioFilePreparer.PrepareFile(
                arguments.Output,
                arguments.Source,
                MediaTypes.MediaTypeWav,
                audioUtilityRequest,
                arguments.Output);

            analysisSettings.AudioFile = preparedFile.TargetInfo.SourceFile;
            analysisSettings.SampleRateOfOriginalAudioFile = preparedFile.SourceInfo.SampleRate;
            // we don't want segments, thus segment duration == total length of original file
            analysisSettings.SegmentDuration = preparedFile.TargetInfo.Duration;
            analysisSettings.SegmentMaxDuration = preparedFile.TargetInfo.Duration;
            analysisSettings.SegmentStartOffset = TimeSpan.Zero;

            if (preparedFile.TargetInfo.SampleRate.Value != analysisSettings.SegmentTargetSampleRate)
            {
                Log.Warn("Input audio sample rate does not match target sample rate");
            }


            // Execute a pre analyzer hook
            recognizer.BeforeAnalyze(analysisSettings);

            // execute actual analysis - output data will be written
            Log.Info("Running recognizer: " + analysisIdentifier);
            AnalysisResult2 results = recognizer.Analyze(analysisSettings);

            // run summarize code - output data can be written
            Log.Info("Running recognizer summary: " + analysisIdentifier);
            var fileSegment = new FileSegment(analysisSettings.AudioFile, preparedFile.SourceInfo.SampleRate.Value, preparedFile.SourceInfo.Duration.Value);
            recognizer.SummariseResults(
                analysisSettings,
                fileSegment,
                results.Events,
                results.SummaryIndices,
                results.SpectralIndices,
                new[] { results });

            //Log.Info("Recognizer run, saving extra results");
            // TODO: Michael, output anything else as you wish.

            Log.Debug("Clean up temporary files");
            if (analysisSettings.SourceFile.FullName != analysisSettings.AudioFile.FullName)
            {
                analysisSettings.AudioFile.Delete();
            }

            Log.Success(recognizer.Identifier + " recognizer has completed");
        }
    }
}
