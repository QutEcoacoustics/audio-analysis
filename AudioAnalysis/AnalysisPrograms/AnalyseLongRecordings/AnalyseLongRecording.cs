// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalyseLongRecording.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the AnalyseLongRecording type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.AnalyseLongRecordings
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Acoustics.Shared;
    using Acoustics.Tools.Audio;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AnalysisRunner;

    using AudioAnalysisTools;
    using AudioAnalysisTools.Indices;

    using log4net;

    public partial class AnalyseLongRecording
    {
        private const string ImagefileExt = ".png";

        private const string FinishedMessage = @"

###################################################
Finished processing audio file: {0}.
Output  to  directory: {1}


##### FINISHED FILE ###################################################
";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            LoggedConsole.WriteLine("# PROCESS LONG RECORDING");
            LoggedConsole.WriteLine("# DATE AND TIME: " + DateTime.Now);

            // 1. set up the necessary files
            var sourceAudio = arguments.Source;
            var configFile = arguments.Config;
            var outputDirectory = arguments.Output;
            var tempFilesDirectory = arguments.TempDir;

            // if a temp dir is not given, use output dir as temp dir
            if (tempFilesDirectory == null)
            {
                Log.Warn("No temporary directory provided, using output directory");
                tempFilesDirectory = arguments.Output;
            }

            LoggedConsole.WriteLine("# Recording file:     " + sourceAudio.Name);
            LoggedConsole.WriteLine("# Configuration file: " + configFile);
            LoggedConsole.WriteLine("# Output folder:      " + outputDirectory);
            LoggedConsole.WriteLine("# Temp File Directory:      " + tempFilesDirectory);

            // 2. get the analysis config dictionary
            dynamic configuration = Yaml.Deserialise(configFile);

            bool saveIntermediateWavFiles = (bool?)configuration[AnalysisKeys.SaveIntermediateWavFiles] ?? false;
            bool saveSonograms = (bool?)configuration[AnalysisKeys.SaveSonograms] ?? false;
            bool displayCsvImage = (bool?)configuration[AnalysisKeys.DisplayCsvImage] ?? false;
            bool doParallelProcessing = (bool?)configuration[AnalysisKeys.ParallelProcessing] ?? false;
            string analysisIdentifier = configuration[AnalysisKeys.AnalysisName];

            double scoreThreshold = 0.2; // min score for an acceptable event
            scoreThreshold = configuration[AnalysisKeys.EventThreshold] ?? scoreThreshold;

            /* var configuration = new ConfigDictionary(configFile.FullName);
             Dictionary<string, string> configDict = configuration.GetTable();

            
             bool saveIntermediateWavFiles = false;
             if (configDict.ContainsKey(AnalysisKeys.SAVE_INTERMEDIATE_WAV_FILES))
                 saveIntermediateWavFiles = ConfigDictionary.GetBoolean(AnalysisKeys.SAVE_INTERMEDIATE_WAV_FILES,
                     configDict);

             bool saveSonograms;
             if (configDict.ContainsKey(AnalysisKeys.SAVE_SONOGRAMS))
                 saveSonograms = ConfigDictionary.GetBoolean(AnalysisKeys.SAVE_SONOGRAMS, configDict);

             bool displayCSVImage = false;
             if (configDict.ContainsKey(AnalysisKeys.DISPLAY_CSV_IMAGE))
                 displayCSVImage = ConfigDictionary.GetBoolean(AnalysisKeys.DISPLAY_CSV_IMAGE, configDict);

             bool doParallelProcessing = false;
             if (configDict.ContainsKey(AnalysisKeys.PARALLEL_PROCESSING))
                 doParallelProcessing = ConfigDictionary.GetBoolean(AnalysisKeys.PARALLEL_PROCESSING, configDict);
            
             if (analysisSettings.ConfigDict.ContainsKey(AnalysisKeys.EVENT_THRESHOLD))
             {
                 scoreThreshold = double.Parse(analysisSettings.ConfigDict[AnalysisKeys.EVENT_THRESHOLD]);
             }
             */

            // 3. initilise AnalysisCoordinator class that will do the analysis
            var analysisCoordinator = new AnalysisCoordinator(new LocalSourcePreparer(), saveIntermediateWavFiles, saveSonograms, displayCsvImage)
            {
                DeleteFinished = (!saveIntermediateWavFiles), // create and delete directories 
                IsParallel = doParallelProcessing,
                SubFoldersUnique = false
            };

            // 4. get the segment of audio to be analysed
            var fileSegment = new FileSegment { OriginalFile = sourceAudio };
            var bothOffsetsProvided = arguments.StartOffset.HasValue && arguments.EndOffset.HasValue;
            if (bothOffsetsProvided)
            {
                fileSegment.SegmentStartOffset = TimeSpan.FromSeconds(arguments.StartOffset.Value);
                fileSegment.SegmentEndOffset = TimeSpan.FromSeconds(arguments.EndOffset.Value);
            }
            else
            {
                Log.Warn("Both offsets were not provided, thus all ignored");
            }

            // 5. initialise the analyser
            var analyser = FindAndCheckAnalyser(analysisIdentifier);

            // 6. initialise the analysis settings object
            var analysisSettings = analyser.DefaultSettings;
            analysisSettings.ConfigFile = configFile;
            analysisSettings.SourceFile = sourceAudio;
            analysisSettings.AnalysisBaseOutputDirectory = outputDirectory;
            analysisSettings.AnalysisBaseTempDirectory = tempFilesDirectory;

            // #SEGMENT_DURATION=minutes, SEGMENT_OVERLAP=seconds   FOR EXAMPLE: SEGMENT_DURATION=5  and SEGMENT_OVERLAP=10
            // set the segment offset i.e. time between consecutive segment starts - the key used for this in config file = "SEGMENT_DURATION"
            try
            {
                int rawDuration = configuration[AnalysisKeys.SegmentDuration];
                analysisSettings.SegmentMaxDuration = TimeSpan.FromMinutes(rawDuration);
            }
            catch (Exception ex)
            {
                Log.Warn("Can't read SegmentMaxDuration from config file (exceptions squashed, default value used)", ex);
                analysisSettings.SegmentMaxDuration = null;
            }

            // set overlap
            try
            {
                int rawOverlap = configuration[AnalysisKeys.SegmentOverlap];
                analysisSettings.SegmentOverlapDuration = TimeSpan.FromSeconds(rawOverlap);
            }
            catch (Exception ex)
            {
                Log.Warn("Can't read SegmentOverlapDuration from config file (exceptions squahsed, default value used)", ex);
                analysisSettings.SegmentOverlapDuration = TimeSpan.Zero;
            }

            // 7. ####################################### DO THE ANALYSIS ###################################
            LoggedConsole.WriteLine("STARTING ANALYSIS ...");
            var analyserResults = analysisCoordinator.Run(fileSegment, analyser, analysisSettings);

            // ##############################################################################################
            // 8. PROCESS THE RESULTS
            LoggedConsole.WriteLine(string.Empty);
            if (analyserResults == null)
            {
                LoggedConsole.WriteErrorLine("###################################################\n");
                LoggedConsole.WriteErrorLine("The Analysis Run Coordinator has returned a null result.");
                LoggedConsole.WriteErrorLine("###################################################\n");
                throw new AnalysisOptionDevilException();
            }

            // Merge and correct main result types
            EventBase[] mergedEventResults = ResultsTools.MergeResults(analyserResults, ar => ar.Events, ResultsTools.CorrectEvent);
            SummaryIndexBase[] mergedIndicesResults = ResultsTools.MergeResults(analyserResults, ar => ar.SummaryIndices, ResultsTools.CorrectSummaryIndex);
            SpectrumBase[] mergedSpectrumResults = ResultsTools.MergeResults(analyserResults, ar => ar.SpectralIndices, ResultsTools.CorrectSpectrumIndex);

            // not an exceptional state, do not throw exception
            if (mergedEventResults != null && mergedEventResults.Length == 0)
            {
                LoggedConsole.WriteWarnLine("The analysis produced no EVENTS (mergedResults had zero count)");
            }
            if (mergedIndicesResults != null && mergedIndicesResults.Length == 0)
            {
                LoggedConsole.WriteWarnLine("The analysis produced no Summary INDICES (mergedResults had zero count)");
            }
            if (mergedSpectrumResults != null && mergedSpectrumResults.Length == 0)
            {
                LoggedConsole.WriteWarnLine("The analysis produced no Spectral INDICES (merged results had zero count)");
            }


            // 9. CREATE SUMMARY INDICES IF NECESSARY (FROM EVENTS)
#if DEBUG
            // get the duration of the original source audio file - need this to convert Events datatable to Indices Datatable
            var audioUtility = new MasterAudioUtility(tempFilesDirectory);
            var mimeType = MediaTypes.GetMediaType(sourceAudio.Extension);
            var sourceInfo = audioUtility.Info(sourceAudio);

            // updated by reference all the way down in LocalSourcePreparer
            Debug.Assert(fileSegment.OriginalFileDuration == sourceInfo.Duration);
#endif
            var duration = fileSegment.OriginalFileDuration;

            // AT: disabled - I can't think of a good reason for this to be here!
            /*// increase the threshold - used to display number of high scoring events
            scoreThreshold *= 3;
            if (scoreThreshold > 1.0)
            {
                scoreThreshold = 1.0;
            }*/

            ResultsTools.ConvertEventsToIndices(analyser, mergedEventResults, ref mergedIndicesResults, duration, scoreThreshold);
            int eventsCount = mergedEventResults == null ? 0 : mergedEventResults.Length;
            int numberOfRowsOfIndices = mergedIndicesResults == null ? 0 : mergedIndicesResults.Length;


            // 10. Allow analysers to post-process

            // TODO: remove results directory if possible
            var resultsDirectory = analyserResults.First().SettingsUsed.AnalysisInstanceOutputDirectory;
            Debug.Assert(analysisSettings.AnalysisInstanceOutputDirectory == resultsDirectory);
            Debug.Assert(analysisSettings.SourceFile == fileSegment.OriginalFile);

            analyser.SummariseResults(analysisSettings, fileSegment, mergedEventResults, mergedIndicesResults, mergedSpectrumResults, analyserResults);


            // 11. SAVE THE RESULTS
            string fileNameBase = Path.GetFileNameWithoutExtension(sourceAudio.Name) + "_" + analyser.Identifier;

            var eventsFile = ResultsTools.SaveEvents(analyser, fileNameBase, resultsDirectory, mergedEventResults);
            var indicesFile = ResultsTools.SaveSummaryIndices(analyser, fileNameBase, resultsDirectory, mergedIndicesResults);
            var spectraFile = ResultsTools.SaveSpectralIndices(analyser, fileNameBase, resultsDirectory, mergedSpectrumResults);

            // 12. Convert summary indices to image
            var indicesPropertiesConfig = FindIndicesConfig.Find(configuration, arguments.Config);

            string fileName = Path.GetFileNameWithoutExtension(indicesFile.Name);
            string imageTitle = string.Format("SOURCE:{0},   (c) QUT;  ", fileName);
            Bitmap tracksImage = DrawSummaryIndices.DrawImageOfSummaryIndices(IndexProperties.GetIndexProperties(indicesPropertiesConfig), indicesFile, imageTitle);
            var imagePath = Path.Combine(resultsDirectory.FullName, fileName + ImagefileExt);
            tracksImage.Save(imagePath);

            // 13. wrap up, write stats
            LoggedConsole.WriteLine("INDICES CSV file(s) = " + indicesFile.Name);
            LoggedConsole.WriteLine("\tNumber of rows (i.e. minutes) in CSV file of indices = " +
                                    numberOfRowsOfIndices);
            LoggedConsole.WriteLine(string.Empty);

            if (eventsFile == null)
            {
                LoggedConsole.WriteLine("An Events CSV file was NOT returned.");
            }
            else
            {
                LoggedConsole.WriteLine("EVENTS CSV file(s) = " + eventsFile.Name);
                LoggedConsole.WriteLine("\tNumber of events = " + eventsCount);
            }


            LoggedConsole.WriteLine(FinishedMessage, sourceAudio.Name, resultsDirectory.FullName);
        }

        private static IAnalyser2 FindAndCheckAnalyser(string analysisIdentifier)
        {
            var analysers = AnalysisCoordinator.GetAnalysers(typeof(MainEntry).Assembly).ToList();
            IAnalyser2 analyser = analysers.FirstOrDefault(a => a.Identifier == analysisIdentifier);
            if (analyser == null)
            {
                LoggedConsole.WriteLine("###################################################\n");
                LoggedConsole.WriteLine("Analysis failed. UNKNOWN Analyser: <{0}>", analysisIdentifier);
                LoggedConsole.WriteLine("Available analysers are:");
                foreach (IAnalyser2 anal in analysers)
                {
                    LoggedConsole.WriteLine("\t  " + anal.Identifier);
                }
                LoggedConsole.WriteLine("###################################################\n");

                throw new Exception("Cannot find a valid IAnalyser2");
            }
            return analyser;
        }
    }
}