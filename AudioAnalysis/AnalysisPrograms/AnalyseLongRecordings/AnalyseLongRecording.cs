﻿// --------------------------------------------------------------------------------------------------------------------
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
    using log4net.Repository.Hierarchy;


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

            LoggedConsole.WriteLine("# Recording file:      " + sourceAudio.FullName);
            LoggedConsole.WriteLine("# Configuration file:  " + configFile);
            LoggedConsole.WriteLine("# Output folder:       " + outputDirectory);
            LoggedConsole.WriteLine("# Temp File Directory: " + tempFilesDirectory);


            if (!sourceAudio.Exists)
            {
                Log.Warn(" >>>>>>>>>>>> WARNING! The Source Recording file cannot be found! This will cause an exception.");
            }

            // 2. get the analysis config
            dynamic configuration = Yaml.Deserialise(configFile);

            bool saveIntermediateWavFiles = (bool?)configuration[AnalysisKeys.SaveIntermediateWavFiles] ?? false;
            bool saveIntermediateCsvFiles = (bool?)configuration[AnalysisKeys.SaveIntermediateCsvFiles] ?? false;
            bool saveSonogramsImages      = (bool?)configuration[AnalysisKeys.SaveSonogramImages] ?? false;
            
            // There's no reason for this to be here
            ////bool displayCsvImage = (bool?)configuration[AnalysisKeys.DisplayCsvImage] ?? false;
            bool doParallelProcessing = (bool?)configuration[AnalysisKeys.ParallelProcessing] ?? false;
            string analysisIdentifier = configuration[AnalysisKeys.AnalysisName];
            FileInfo indicesPropertiesConfig = FindIndicesConfig.Find(configuration, arguments.Config);

            if (indicesPropertiesConfig == null || !indicesPropertiesConfig.Exists)
            {
                Log.Warn("IndexProperties config can not be found! This will result in an exception if it is needed later on.");
            }

            // min score for an acceptable event
            double scoreThreshold = 0.2;
            if (((double?)configuration[AnalysisKeys.EventThreshold]) != null)
            {
                scoreThreshold = (double)configuration[AnalysisKeys.EventThreshold];
                Log.Info("Minimum event threshold has been set to " + scoreThreshold);

            }
            else
            {
                Log.Warn("Minimum event threshold has been set to the default: " + scoreThreshold);
            }

            // 3. initilise AnalysisCoordinator class that will do the analysis
            var analysisCoordinator = new AnalysisCoordinator(new LocalSourcePreparer(), saveIntermediateWavFiles, saveSonogramsImages, saveIntermediateCsvFiles)
            {
                // create and delete directories
                DeleteFinished = !saveIntermediateWavFiles,  
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
                Log.Warn("Neither start nor end segment offsets provided. Therefore ignored");
            }

            // 5. initialise the analyser
            var analyser = FindAndCheckAnalyser(analysisIdentifier);

            // 6. initialise the analysis settings object
            var analysisSettings = analyser.DefaultSettings;
            analysisSettings.ConfigFile = configFile;
            analysisSettings.Configuration = configuration;
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
                analysisSettings.SegmentMaxDuration = TimeSpan.FromMinutes(1.0);
                Log.Warn("Can't read SegmentMaxDuration from config file (exceptions squashed, default value of " + analysisSettings.SegmentMaxDuration + " used)");
            }

            // set IndexCalculationDuration i.e. duration of a subsegment
            try
            {
                double indexCalculationDuration = configuration[AnalysisKeys.IndexCalculationDuration];
                analysisSettings.IndexCalculationDuration = TimeSpan.FromSeconds(indexCalculationDuration);
            }
            catch (Exception ex)
            {
                analysisSettings.IndexCalculationDuration = TimeSpan.FromSeconds(60);
                Log.Warn("Cannot read IndexCalculationDuration from config file (Exceptions squashed. Used default value = " + analysisSettings.IndexCalculationDuration + ")");
            }

            // set background noise neighbourhood
            try
            {
                int bgnNh = configuration[AnalysisKeys.BGNoiseNeighbourhood];
                analysisSettings.BGNoiseNeighbourhood = TimeSpan.FromSeconds(bgnNh);
            }
            catch (Exception ex)
            {
                analysisSettings.BGNoiseNeighbourhood = analysisSettings.SegmentMaxDuration;
                Log.Warn("Cannot read BGNNeighbourhood from config file (Exceptions squashed. Used default value = " + analysisSettings.BGNoiseNeighbourhood + ")");
            }

            // set target sample rate
            try
            {
                int rawSampleRate = configuration[AnalysisKeys.ResampleRate];
                analysisSettings.SegmentTargetSampleRate = rawSampleRate;
            }
            catch (Exception ex)
            {
                Log.Warn("Can't read SegmentTargetSampleRate from config file (exceptions squashed, default value  of " + analysisSettings.SegmentTargetSampleRate + " used)");
            }


            //var config = analysisSettings.Configuration;
            //var indicesPropertiesConfig = FindIndicesConfig.Find(config, analysisSettings.ConfigFile);
            var indexProperties = IndexProperties.GetIndexProperties(indicesPropertiesConfig);
            AudioAnalysisTools.Indices.SpectralIndexValues.CheckExistenceOfSpectralIndexValues(indexProperties);




            // 7. ####################################### DO THE ANALYSIS ###################################
            LoggedConsole.WriteLine("STARTING ANALYSIS ...");
            var analyserResults = analysisCoordinator.Run(fileSegment, analyser, analysisSettings);

            // ##############################################################################################
            // 8. PROCESS THE RESULTS
            LoggedConsole.WriteLine(string.Empty);
            LoggedConsole.WriteLine("START PROCESSING RESULTS ...");
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
            SpectralIndexBase[] mergedSpectralIndexResults = ResultsTools.MergeResults(analyserResults, ar => ar.SpectralIndices, ResultsTools.CorrectSpectrumIndex);

            // not an exceptional state, do not throw exception
            if (mergedEventResults != null && mergedEventResults.Length == 0)
            {
                LoggedConsole.WriteWarnLine("The analysis produced no EVENTS (mergedResults had zero count)");
            }
            if (mergedIndicesResults != null && mergedIndicesResults.Length == 0)
            {
                LoggedConsole.WriteWarnLine("The analysis produced no Summary INDICES (mergedResults had zero count)");
            }
            if (mergedSpectralIndexResults != null && mergedSpectralIndexResults.Length == 0)
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

            ResultsTools.ConvertEventsToIndices(analyser, mergedEventResults, ref mergedIndicesResults, duration, scoreThreshold);
            int eventsCount = mergedEventResults == null ? 0 : mergedEventResults.Length;
            int numberOfRowsOfIndices = mergedIndicesResults == null ? 0 : mergedIndicesResults.Length;


            // 10. Allow analysers to post-process

            // TODO: remove results directory if possible
            var instanceOutputDirectory = analyserResults.First().SettingsUsed.AnalysisInstanceOutputDirectory;

            // this allows the summariser to write results to the same output directory as each analysis segment
            analysisSettings.AnalysisInstanceOutputDirectory = instanceOutputDirectory;
            Debug.Assert(analysisSettings.AnalysisInstanceOutputDirectory == instanceOutputDirectory, "The instance result directory should be the same as the base analysis directory");
            Debug.Assert(analysisSettings.SourceFile == fileSegment.OriginalFile);

            // Important - this is where IAnalyser2's post processer gets called. I.e. Long duration spectrograms are drawn IFF analysis type is Towsey.Acoustic
            analyser.SummariseResults(analysisSettings, fileSegment, mergedEventResults, mergedIndicesResults, mergedSpectralIndexResults, analyserResults);


            // 11. SAVE THE RESULTS
            string fileNameBase = Path.GetFileNameWithoutExtension(sourceAudio.Name) + "_" + analyser.Identifier;

            var eventsFile = ResultsTools.SaveEvents(analyser, fileNameBase, instanceOutputDirectory, mergedEventResults);
            var indicesFile = ResultsTools.SaveSummaryIndices(analyser, fileNameBase, instanceOutputDirectory, mergedIndicesResults);
            var spectraFile = ResultsTools.SaveSpectralIndices(analyser, fileNameBase, instanceOutputDirectory, mergedSpectralIndexResults);

            // 12. Convert summary indices to tracks (black and white rows) image
            if (mergedIndicesResults == null)
            {
                Log.Info("No summary indices produced");
            }
            else
            {
                if (indicesPropertiesConfig == null || !indicesPropertiesConfig.Exists)
                {
                    throw new InvalidOperationException("Cannot process indices without an index configuration file, the file could not be found!");
                }

                // this arbitary amount - a sheer guess... who knows if it will work.
                if (mergedIndicesResults.Length > 5000)
                {
                    Log.Warn("Summary Indices Image not able to be drawn - there are too many indices to render");
                }
                else
                {
                    string fileName = Path.GetFileNameWithoutExtension(indicesFile.Name);
                    string imageTitle = string.Format("SOURCE:{0},   (c) QUT;  ", fileName);
                    Bitmap tracksImage =
                        DrawSummaryIndices.DrawImageOfSummaryIndices(
                            IndexProperties.GetIndexProperties(indicesPropertiesConfig),
                            indicesFile,
                            imageTitle);
                    var imagePath = Path.Combine(instanceOutputDirectory.FullName, fileName + ImagefileExt);
                    tracksImage.Save(imagePath);
                }
            }

            // 13. wrap up, write stats
            LoggedConsole.WriteLine(
                "INDICES CSV file(s) = " + (indicesFile == null ? "<<No indices result, no file!>>" : indicesFile.Name));
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


            LoggedConsole.WriteLine(FinishedMessage, sourceAudio.Name, instanceOutputDirectory.FullName);
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
