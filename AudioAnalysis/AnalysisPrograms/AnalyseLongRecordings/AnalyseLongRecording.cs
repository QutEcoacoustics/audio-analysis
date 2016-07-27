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
    using System.Configuration;
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

    using AnalysisPrograms.Production;

    using AnalysisRunner;

    using AudioAnalysisTools;
    using AudioAnalysisTools.Indices;

    using log4net;
    using log4net.Repository.Hierarchy;


    public partial class AnalyseLongRecording
    {
        private const string ImagefileExt = "png";

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

            // try an automatically find the config file
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
            LoggedConsole.WriteLine("# Temp File Directory: " + tempFilesDirectory);

            // optionally copy logs / config to make results easier to understand
            if (arguments.WhenExitCopyConfig || arguments.WhenExitCopyLog)
            {
                AppDomain.CurrentDomain.ProcessExit += (sender, args) => { Cleanup(arguments, configFile); };
            }

            // 2. get the analysis config
            dynamic configuration = Yaml.Deserialise(configFile);

            bool saveIntermediateWavFiles = (bool?)configuration[AnalysisKeys.SaveIntermediateWavFiles] ?? false;
            bool saveIntermediateCsvFiles = (bool?)configuration[AnalysisKeys.SaveIntermediateCsvFiles] ?? false;
            bool saveSonogramsImages = (bool?)configuration[AnalysisKeys.SaveSonogramImages] ?? false;
            bool doParallelProcessing = (bool?)configuration[AnalysisKeys.ParallelProcessing] ?? false;
            
            bool filenameDate = (bool?)configuration[AnalysisKeys.RequireDateInFilename] ?? false;

            string analysisIdentifier = configuration[AnalysisKeys.AnalysisName];
            FileInfo indicesPropertiesConfig = IndexProperties.Find(configuration, arguments.Config);

            if (indicesPropertiesConfig == null || !indicesPropertiesConfig.Exists)
            {
                Log.Warn("IndexProperties config can not be found! This will result in an exception if it is needed later on.");
            }
            else
            {
                LoggedConsole.WriteLine("# IndexProperties Cfg: " + indicesPropertiesConfig.FullName);
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

            if (filenameDate)
            {
                if (!FileDateHelpers.FileNameContainsDateTime(sourceAudio.Name))
                {
                    throw new InvalidFileDateException("When RequireDateInFilename option is set, the filename of the source audio file must contain a valid AND UNAMBIGUOUS date. Such a date was not able to be parsed.");
                }
            }

            // 3. initilise AnalysisCoordinator class that will do the analysis
            var analysisCoordinator = new AnalysisCoordinator(new LocalSourcePreparer(), saveIntermediateWavFiles, saveSonogramsImages, saveIntermediateCsvFiles, arguments.Channels, arguments.MixDownToMono)
            {
                // create and delete directories
                DeleteFinished = !saveIntermediateWavFiles,  
                IsParallel = doParallelProcessing,
                SubFoldersUnique = false
            };

            // 4. get the segment of audio to be analysed
            // if tiling output, specify that FileSegment needs to be able to read the date
            var fileSegment = new FileSegment(sourceAudio, filenameDate, true); 
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

            // Execute a pre analyzer hook
            analyser.BeforeAnalyze(analysisSettings);

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

            // 11. IMPORTANT - this is where IAnalyser2's post processer gets called.
            // Produces all spectrograms and images of SPECTRAL INDICES.
            // Long duration spectrograms are drawn IFF analysis type is Towsey.Acoustic
            analyser.SummariseResults(analysisSettings, fileSegment, mergedEventResults, mergedIndicesResults, mergedSpectralIndexResults, analyserResults);


            // 12. SAVE THE RESULTS
            string fileNameBase = Path.GetFileNameWithoutExtension(sourceAudio.Name);

            var eventsFile  = ResultsTools.SaveEvents(analyser, fileNameBase, instanceOutputDirectory, mergedEventResults);
            var indicesFile = ResultsTools.SaveSummaryIndices(analyser, fileNameBase, instanceOutputDirectory, mergedIndicesResults);
            var spectraFile = ResultsTools.SaveSpectralIndices(analyser, fileNameBase, instanceOutputDirectory, mergedSpectralIndexResults);

            // 13. THIS IS WHERE SUMMARY INDICES ARE PROCESSED
            //     Convert summary indices to black and white tracks image
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

                // this arbitrary amount - a sheer guess... who knows if it will work.
                if (mergedIndicesResults.Length > 5000)
                {
                    Log.Warn("Summary Indices Image not able to be drawn - there are too many indices to render");
                }
                else
                {
                    var basename = Path.GetFileNameWithoutExtension(fileNameBase);
                    string imageTitle = $"SOURCE:{basename},   (c) QUT;  ";

                    Bitmap tracksImage =
                        IndexDisplay.DrawImageOfSummaryIndices(
                            IndexProperties.GetIndexProperties(indicesPropertiesConfig),
                            indicesFile,
                            imageTitle,
                            analysisSettings.SegmentMaxDuration.Value,
                            fileSegment.OriginalFileStartDate);
                    var imagePath = FilenameHelpers.AnalysisResultName(instanceOutputDirectory, basename, "Indices", ImagefileExt);
                    tracksImage.Save(imagePath);
                }
            }

            // 14. wrap up, write stats
            LoggedConsole.WriteLine(
                "INDICES CSV file(s) = " + (indicesFile?.Name ?? "<<No indices result, no file!>>"));
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

        /// <summary>
        /// Generic organization of resources after a run
        /// </summary>
        /// <param name="args"></param>
        /// <param name="configFile"></param>
        private static void Cleanup(Arguments args, FileInfo configFile)
        {
            if (args.WhenExitCopyConfig)
            {
                configFile.CopyTo(Path.Combine(args.Output.FullName, args.Config.Name), true);
            }

            if (args.WhenExitCopyLog)
            {
                var logDirectory = ConfigFile.LogFolder;
                var logFile = Path.Combine(logDirectory, "log.txt");

                File.Copy(logFile, Path.Combine(args.Output.FullName, "log.txt"), true);
            }
        }
    }
}
