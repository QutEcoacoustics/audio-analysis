// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalyseLongRecording.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the AnalyseLongRecording type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.AnalyseLongRecordings
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Contracts;
    using Acoustics.Tools.Audio;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Indices;
    using log4net;
    using Production;
    using SourcePreparers;

    public partial class AnalyseLongRecording
    {
        private const string ImagefileExt = "png";

        private static readonly ILog Log = LogManager.GetLogger(nameof(AnalyseLongRecording));

        /// <summary>
        /// 2. Analyses long audio recording (mp3 or wav) as per passed config file. Outputs an events.csv file AND an
        /// indices.csv file
        /// Signed off: Michael Towsey 4th December 2012
        /// </summary>
        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                throw new NoDeveloperMethodException();
            }

            LoggedConsole.WriteLine("# PROCESS LONG RECORDING");
            LoggedConsole.WriteLine("# DATE AND TIME: " + DateTime.Now);

            // 1. set up the necessary files
            var sourceAudio = arguments.Source;
            var configFile = arguments.Config.ToFileInfo();
            var outputDirectory = arguments.Output;
            var tempFilesDirectory = arguments.TempDir;

            // if a temp dir is not given, use output dir as temp dir
            if (tempFilesDirectory == null)
            {
                Log.Warn("No temporary directory provided, using output directory");
                tempFilesDirectory = outputDirectory;
            }

            // try an automatically find the config file
            if (configFile == null)
            {
                throw new FileNotFoundException("No config file argument provided");
            }
            else if (!configFile.Exists)
            {
                Log.Warn($"Config file {configFile.FullName} not found... attempting to resolve config file");

                // we use .ToString() here to get the original input string - Using fullname always produces an absolute path wrt to pwd... we don't want to prematurely make asusmptions:
                // e.g. We require a missing absolute path to fail... that wouldn't work with .Name
                // e.g. We require a relative path to try and resolve, using .FullName would fail the first absolute check inside ResolveConfigFile
                configFile = ConfigFile.Resolve(configFile.ToString(), Directory.GetCurrentDirectory().ToDirectoryInfo());
            }

            if (arguments.StartOffset.HasValue ^ arguments.EndOffset.HasValue)
            {
                throw new InvalidStartOrEndException("If StartOffset or EndOffset is specified, then both must be specified");
            }

            if (arguments.StartOffset.HasValue && arguments.EndOffset.HasValue && arguments.EndOffset.Value <= arguments.StartOffset.Value)
            {
                throw new InvalidStartOrEndException("Start offset must be less than end offset.");
            }

            LoggedConsole.WriteLine("# Recording file:      " + sourceAudio.FullName);
            LoggedConsole.WriteLine("# Configuration file:  " + configFile);
            LoggedConsole.WriteLine("# Output folder:       " + outputDirectory);
            LoggedConsole.WriteLine("# Temp File Directory: " + tempFilesDirectory);

            // optionally copy logs / config to make results easier to understand
            // TODO: remove, see https://github.com/QutEcoacoustics/audio-analysis/issues/133
            if (arguments.WhenExitCopyConfig || arguments.WhenExitCopyLog)
            {
                AppDomain.CurrentDomain.ProcessExit += (sender, args) => { Cleanup(arguments, configFile); };
            }

            // 2. initialize the analyzer
            // we're changing the way resolving config files works. Ideally, we'd like to use static type config files
            // but we can't do that unless we know which type we have to load first! Currently analyzer to load is in
            // the config file so we can't know which analyzer we can use. Thus we will change to using the file name,
            //or an argument to resolve the analyzer to load.
            // Get analysis name:
            Log.Warn("The way analysis names are determined has changed. Now either use the CLI option "
                + "`AnalysisIdentifier` or name the config file to matach an analysis name");

            IAnalyser2 analyzer = FindAndCheckAnalyser<IAnalyser2>(arguments.AnalysisIdentifier, configFile.Name);

            // 2. get the analysis config
            AnalyzerConfig configuration = analyzer.ParseConfig(configFile);

            SaveBehavior saveIntermediateWavFiles = configuration.SaveIntermediateWavFiles;
            bool saveIntermediateDataFiles = configuration.SaveIntermediateCsvFiles;
            SaveBehavior saveSonogramsImages = configuration.SaveSonogramImages;

            bool filenameDate = configuration.RequireDateInFilename;

            if (configuration[AnalysisKeys.AnalysisName].IsNotWhitespace())
            {
                Log.Warn("Your config file has `AnalysisName` set - this property is deprecated and ignored");
            }

            // AT 2018-02: changed logic so default index properties loaded if not provided
            FileInfo indicesPropertiesConfig = IndexProperties.Find(configuration, configFile);
            if (indicesPropertiesConfig == null || !indicesPropertiesConfig.Exists)
            {
                Log.Warn("IndexProperties config can not be found! Loading a default");
                indicesPropertiesConfig = ConfigFile.Default<Dictionary<string, IndexProperties>>();
            }

            LoggedConsole.WriteLine("# IndexProperties Cfg: " + indicesPropertiesConfig.FullName);

            // min score for an acceptable event
            Log.Info("Minimum event threshold has been set to " + configuration.EventThreshold);

            FileSegment.FileDateBehavior defaultBehavior = FileSegment.FileDateBehavior.Try;
            if (filenameDate)
            {
                if (!FileDateHelpers.FileNameContainsDateTime(sourceAudio.Name))
                {
                    throw new InvalidFileDateException(
                        "When RequireDateInFilename option is set, the filename of the source audio file must contain "
                        + "a valid AND UNAMBIGUOUS date. Such a date was not able to be parsed.");
                }

                defaultBehavior = FileSegment.FileDateBehavior.Required;
            }

            // 3. initilize AnalysisCoordinator class that will do the analysis
            var analysisCoordinator = new AnalysisCoordinator(
                new LocalSourcePreparer(),
                saveIntermediateWavFiles,
                false,
                arguments.Parallel);

            // 4. get the segment of audio to be analysed
            // if tiling output, specify that FileSegment needs to be able to read the date
            var fileSegment = new FileSegment(sourceAudio, arguments.AlignToMinute, null, defaultBehavior);
            var bothOffsetsProvided = arguments.StartOffset.HasValue && arguments.EndOffset.HasValue;
            if (bothOffsetsProvided)
            {
                fileSegment.SegmentStartOffset = TimeSpan.FromSeconds(arguments.StartOffset.Value);
                fileSegment.SegmentEndOffset = TimeSpan.FromSeconds(arguments.EndOffset.Value);
            }
            else
            {
                Log.Debug("Neither start nor end segment offsets provided. Therefore both were ignored.");
            }

            // 6. initialize the analysis settings object
            var analysisSettings = analyzer.DefaultSettings;
            analysisSettings.ConfigFile = configFile;
            analysisSettings.Configuration = configuration;
            analysisSettings.AnalysisOutputDirectory = outputDirectory;
            analysisSettings.AnalysisTempDirectory = tempFilesDirectory;
            analysisSettings.AnalysisDataSaveBehavior = saveIntermediateDataFiles;
            analysisSettings.AnalysisImageSaveBehavior = saveSonogramsImages;
            analysisSettings.AnalysisChannelSelection = arguments.Channels;
            analysisSettings.AnalysisMixDownToMono = arguments.MixDownToMono;

            var segmentDuration = configuration.SegmentDuration?.Seconds();
            if (!segmentDuration.HasValue)
            {
                segmentDuration = analysisSettings.AnalysisMaxSegmentDuration ?? TimeSpan.FromMinutes(1);
                Log.Warn(
                    $"Can't read `{nameof(AnalyzerConfig.SegmentDuration)}` from config file. "
                    + $"Default value of {segmentDuration} used)");
            }

            analysisSettings.AnalysisMaxSegmentDuration = segmentDuration.Value;

            var segmentOverlap = configuration.SegmentOverlap?.Seconds();
            if (!segmentOverlap.HasValue)
            {
                segmentOverlap = analysisSettings.SegmentOverlapDuration;
                Log.Warn(
                    $"Can't read `{nameof(AnalyzerConfig.SegmentOverlap)}` from config file. "
                    + $"Default value of {segmentOverlap} used)");
            }

            analysisSettings.SegmentOverlapDuration = segmentOverlap.Value;

            // set target sample rate
            var resampleRate = configuration.ResampleRate;
            if (!resampleRate.HasValue)
            {
                resampleRate = analysisSettings.AnalysisTargetSampleRate ?? AppConfigHelper.DefaultTargetSampleRate;
                Log.Warn(
                    $"Can't read {nameof(configuration.ResampleRate)} from config file. "
                    + $"Default value of {resampleRate} used)");
            }

            analysisSettings.AnalysisTargetSampleRate = resampleRate;

            Log.Info(
                $"{nameof(configuration.SegmentDuration)}={segmentDuration}, "
                + $"{nameof(configuration.SegmentOverlap)}={segmentOverlap}, "
                + $"{nameof(configuration.ResampleRate)}={resampleRate}");

            // 7. ####################################### DO THE ANALYSIS ###################################
            LoggedConsole.WriteLine("START ANALYSIS ...");
            var analyserResults = analysisCoordinator.Run(fileSegment, analyzer, analysisSettings);

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
            Debug.Assert(fileSegment.TargetFileDuration == sourceInfo.Duration);
#endif
            var duration = fileSegment.TargetFileDuration.Value;

            ResultsTools.ConvertEventsToIndices(
                analyzer,
                mergedEventResults,
                ref mergedIndicesResults,
                duration,
                configuration.EventThreshold);
            int eventsCount = mergedEventResults?.Length ?? 0;
            int numberOfRowsOfIndices = mergedIndicesResults?.Length ?? 0;

            // 10. Allow analysers to post-process

            // TODO: remove results directory if possible
            var instanceOutputDirectory =
                AnalysisCoordinator.GetNamedDirectory(analysisSettings.AnalysisOutputDirectory, analyzer);

            // 11. IMPORTANT - this is where IAnalyser2's post processor gets called.
            // Produces all spectrograms and images of SPECTRAL INDICES.
            // Long duration spectrograms are drawn IFF analysis type is Towsey.Acoustic
            analyzer.SummariseResults(analysisSettings, fileSegment, mergedEventResults, mergedIndicesResults, mergedSpectralIndexResults, analyserResults);

            // 12. SAVE THE RESULTS
            string fileNameBase = Path.GetFileNameWithoutExtension(sourceAudio.Name);

            var eventsFile = ResultsTools.SaveEvents(analyzer, fileNameBase, instanceOutputDirectory, mergedEventResults);
            var indicesFile = ResultsTools.SaveSummaryIndices(analyzer, fileNameBase, instanceOutputDirectory, mergedIndicesResults);
            var spectraFile = ResultsTools.SaveSpectralIndices(analyzer, fileNameBase, instanceOutputDirectory, mergedSpectralIndexResults);

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

                // this arbitrary amount of data.
                if (mergedIndicesResults.Length > 5000)
                {
                    Log.Warn("Summary Indices Image not able to be drawn - there are too many indices to render");
                }
                else
                {
                    var basename = Path.GetFileNameWithoutExtension(fileNameBase);
                    string imageTitle = $"SOURCE:{basename},   {Meta.OrganizationTag};  ";

                    // Draw Tracks-Image of Summary indices
                    // set time scale resolution for drawing of summary index tracks
                    TimeSpan timeScale = TimeSpan.FromSeconds(0.1);
                    Bitmap tracksImage =
                        IndexDisplay.DrawImageOfSummaryIndices(
                            IndexProperties.GetIndexProperties(indicesPropertiesConfig),
                            indicesFile,
                            imageTitle,
                            timeScale,
                            fileSegment.TargetFileStartDate);
                    var imagePath = FilenameHelpers.AnalysisResultPath(instanceOutputDirectory, basename, "SummaryIndices", ImagefileExt);
                    tracksImage.Save(imagePath);
                }
            }

            // 14. wrap up, write stats
            LoggedConsole.WriteLine("INDICES CSV file(s) = " + (indicesFile?.Name ?? "<<No indices result, no file!>>"));
            LoggedConsole.WriteLine("\tNumber of rows (i.e. minutes) in CSV file of indices = " + numberOfRowsOfIndices);
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

            Log.Success($"Analysis Complete.\nSource={sourceAudio.Name}\nOutput={instanceOutputDirectory.FullName}");
        }

        public static T FindAndCheckAnalyser<T>(string analysisIdentifier, string partialIdentifier)
            where T : class, IAnalyser2
        {
            string searchName;
            if (analysisIdentifier.IsNotWhitespace())
            {
                searchName = analysisIdentifier;
                Log.Debug($"Searching for exact analysis identifier name {searchName} (from a CLI option)");
            }
            else
            {
                // split name (e.g. "Towsey.Acoustics.Zooming.yml") on periods
                var fragments = partialIdentifier.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);

                Contract.Requires<CommandLineArgumentException>(
                    fragments.Length >= 2,
                    $"We need at least two segments to search for an analyzer, supplied name `{partialIdentifier}` is insufficient.");

                // assume indentifier (e.g. "Towsey.Acoustic") in first two segments
                searchName = fragments[0] + "." + fragments[1];
                Log.Debug($"Searching for partial analysis identifier name. `{searchName}` extracted from `{partialIdentifier}`");
            }

            var analysers = AnalysisCoordinator.GetAnalyzers<T>(typeof(MainEntry).Assembly).ToList();
            T analyser = analysers.FirstOrDefault(a => a.Identifier == searchName);
            if (analyser == null)
            {
                var error = $"We can not determine what analysis you want to run. We tried to search for \"{searchName}\"";
                LoggedConsole.WriteError(error);
                var knownAnalyzers = analysers.Aggregate(string.Empty, (a, i) => a + $"  {i.Identifier}\n");
                LoggedConsole.WriteLine("Available analysers are:\n" + knownAnalyzers);

                throw new ValidationException($"Cannot find an IAnalyser2 with the name `{searchName}`");
            }

            Log.Info($"Using analyzer {analyser.Identifier}");

            return analyser;
        }

        /// <summary>
        /// Generic organization of resources after a run
        /// </summary>
        private static void Cleanup(Arguments args, FileInfo configFile)
        {
            if (args.WhenExitCopyConfig)
            {
                configFile.CopyTo(Path.Combine(args.Output.FullName, Path.GetFileName(args.Config)), true);
            }

            if (args.WhenExitCopyLog)
            {
                var logDirectory = LoggedConsole.LogFolder;
                var logFile = Path.Combine(logDirectory, "log.txt");

                File.Copy(logFile, Path.Combine(args.Output.FullName, "log.txt"), true);
            }
        }
    }
}
