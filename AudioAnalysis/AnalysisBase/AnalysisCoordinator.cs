// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalysisCoordinator.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Prepares, runs and completes analyses.
//   *** DO NOT CHANGE THIS CLASS UNLESS INSTRUCTED TOO ***
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Acoustics.Shared.Contracts;
    using log4net;
    using SegmentAnalysis;

    /// <summary>
    /// Prepares, runs and completes analyses.
    ///
    /// *** DO NOT CHANGE THIS CLASS UNLESS INSTRUCTED TOO ***
    /// </summary>
    /// <remarks>
    /// <para>
    /// The process to analyze files can be a little complex. The overall idea is
    /// to begin with an analysis type and a list of file paths and segments inside those files.
    /// Then those files are segmented using default settings from the analysis and possible modifications to the defaults by a user.
    /// Each segment is analyzed, and the results are put into either a purpose-created folder (which might be deledt once that analysis is complete),
    /// or a known location for later use.
    /// </para>
    /// <para>
    /// temp files can also be stored in sub folders named by analysis name and files named by segment id
    /// when another analysis is run, the files are overwritten.
    /// </para>
    /// </remarks>
    public class AnalysisCoordinator
    {
        private const string StartingItem = "Starting item {0}: {1}.";
        private const string CanceledItem = "Cancellation requested for {0} analysis {1}. Finished item {2}: {3}.";

        private static readonly ILog Log = LogManager.GetLogger(nameof(AnalysisCoordinator));

        private readonly SaveBehavior saveImageFiles;
        private readonly bool saveIntermediateDataFiles;

        private readonly SaveBehavior saveIntermediateWavFiles;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisCoordinator"/> class but also allows for advanced
        /// channel mapping options.
        /// </summary>
        /// <param name="sourcePreparer">The source preparer to use</param>
        /// <param name="saveIntermediateWavFiles">Defines when intermediate WAVE files should be saved</param>
        /// <param name="saveImageFiles">Defines when intermediate image files should be saved</param>
        /// <param name="saveIntermediateDataFiles">Defines if intermediate data files should be saved</param>
        /// <param name="isParallel">Whether or not to run the analysis with multiple threads</param>
        /// <param name="uniqueDirectoryPerSegment">Whether or not to create unique directories per segment (in both temp and output directories)</param>
        public AnalysisCoordinator(
            ISourcePreparer sourcePreparer,
            SaveBehavior saveIntermediateWavFiles,
            SaveBehavior saveImageFiles,
            bool saveIntermediateDataFiles,
            bool uniqueDirectoryPerSegment = true,
            bool isParallel = false)
        {
            Contract.Requires<ArgumentNullException>(sourcePreparer != null, "sourcePreparer must not be null");

            this.saveIntermediateWavFiles = saveIntermediateWavFiles;
            this.saveImageFiles = saveImageFiles;
            this.saveIntermediateDataFiles = saveIntermediateDataFiles;

            this.SourcePreparer = sourcePreparer;
            this.UniqueDirectoryPerSegment = uniqueDirectoryPerSegment;
            this.IsParallel = isParallel;
        }

        /// <summary>
        /// Gets SourcePreparer.
        /// </summary>
        public ISourcePreparer SourcePreparer { get; }

        /// <summary>
        /// Gets a value indicating whether to create
        /// uniquely named sub directories for each run,
        /// or reuse a single folder named using the analysis name.
        /// Applies to both temp and output directories.
        /// </summary>
        public bool UniqueDirectoryPerSegment { get; }

        /// <summary>
        /// Gets a value indicating whether to run in parallel.
        /// </summary>
        public bool IsParallel { get; }

        /// <summary>
        /// Get analysers using a method that is compatible with MONO environment..
        /// </summary>
        /// <param name="assembly">
        /// The assembly.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; AnalysisBase.IAnalyser2].
        /// </returns>
        public static IEnumerable<IAnalyser2> GetAnalyzers(Assembly assembly)
        {
            // to find the assembly, get the type of a class in that assembly
            // eg. typeof(MainEntry).Assembly
            var analyzerType = typeof(IAnalyser2);

            var analyzers = assembly.GetTypes()
                .Where(analyzerType.IsAssignableFrom)
                .Where(t => t.IsClass && !t.IsAbstract)
                .Select(t => Activator.CreateInstance(t) as IAnalyser2);

            return analyzers;
        }

        /// <summary>
        /// Analyze one file segment using the analysis and settings.
        /// </summary>
        /// <param name="segment">
        /// The file Segment.
        /// </param>
        /// <param name="analysis">
        /// The analysis.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <returns>
        /// The analysis results.
        /// </returns>
        public AnalysisResult2[] Run<TSource>(ISegment<TSource> segment, IAnalyser2 analysis, AnalysisSettingsBase settings)
        {
            return this.Run(new[] { segment }, analysis, settings);
        }

        /// <summary>
        /// Analyze one or more file segments using the same analysis and settings.
        /// Note each segment could be sourced from separate original audio files!
        /// If using a remote source preparer the segments could even be downloaded from a remote source!
        /// </summary>
        /// <param name="segments">
        /// The file Segments.
        /// </param>
        /// <param name="analysis">
        /// The analysis.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <returns>
        /// The analysis results.
        /// </returns>
        public AnalysisResult2[] Run<TSource>(
           ISegment<TSource>[] segments,
           IAnalyser2 analysis,
           AnalysisSettingsBase settings)
        {
            Contract.Requires(settings != null, "Settings must not be null.");
            Contract.Requires(analysis != null, "Analysis must not be null.");
            Contract.Requires(segments != null, "File Segments must not be null.");

            // do not allow the program to continue
            // if there are no possible segments to process because the original file
            // is too short.
            var tooShort = segments.FirstOrDefault(segment => segment.SourceMetadata.DurationSeconds < settings.AnalysisMinSegmentDuration);
            if (tooShort != null)
            {
                Log.Fatal("Provided audio recording is too short too analyze!");
                throw new AudioRecordingTooShortException(
                    "{0} is too short to analyze with current analysisSettings.AnalysisMinSegmentDuration ({1})"
                        .Format2(tooShort.Source, settings.AnalysisMinSegmentDuration));
            }

            // try and create temp directory (returns true if already exists)
            if (!settings.IsAnalysisTempDirectoryValid)
            {
                // ensure a temp directory is always set
                Log.Warn(
                    "No temporary directory provided, using random directory: " +
                    settings.AnalysisTempDirectoryFallback);
            }

            // calculate the sub-segments of the given file segments that match what the analysis expects.
            var analysisSegments = this.PrepareAnalysisSegments(segments, settings);

            AnalysisResult2[] results;

            // clone analysis settings for parallelism concerns:
            //  - as each iteration modifies settings. This causes hard to track down bugs
            // clones are made for sequential runs to to ensure consistency
            var settingsForThisItem = (AnalysisSettingsBase)settings.Clone();

            Log.InfoFormat("Analysis started in {0}.", this.IsParallel ? "parallel" : "sequence");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Analyze the sub-segments in parallel or sequentially (IsParallel property),
            // Create and delete directories and/or files as indicated by properties
            // DeleteFinished and UniqueDirectoryPerSegment
            if (this.IsParallel)
            {
                results = this.RunParallel(analysisSegments, analysis, settingsForThisItem);

                // TODO: determine if this is bad because we do not do it for sequential as well!
                Array.Sort(results);
            }
            else
            {
                // sequential
                results = this.RunSequential(analysisSegments, analysis, settingsForThisItem);
            }

            stopwatch.Stop();

            Log.DebugFormat("Analysis complete, took {0}.", stopwatch.Elapsed);

            // delete temp directories
            // only delete directory if we are using one that was created specifically for this analysis
            settings.AnalysisTempDirectoryFallback.TryDelete(true, $"Item {settings.InstanceId}");

            return results;
        }

        private List<ISegment<TSource>> PrepareAnalysisSegments<TSource>(ISegment<TSource>[] segments, AnalysisSettingsBase settings)
        {
            var analysisSegments = this.SourcePreparer.CalculateSegments(segments, settings).ToList();

            // check for any segment that is too short and remove if too short
            var shortSegments = analysisSegments
                .Where(x => x.EndOffsetSeconds - x.StartOffsetSeconds <
                            settings.AnalysisMinSegmentDuration.TotalSeconds);
            foreach (var segment in shortSegments)
            {
                Log.Warn("Analysis segment removed because it was too short " +
                         $"(less than {settings.AnalysisMinSegmentDuration}): {segment}");
                analysisSegments.Remove(segment);
            }

            // ensure all segments are valid
            foreach (var segment in analysisSegments)
            {
                Contract.Requires<InvalidSegmentException>(
                    segment.EndOffsetSeconds - segment.StartOffsetSeconds > 0,
                    $"Segment {segment} wa invalid because end was less than start");

                Contract.Requires<InvalidSegmentException>(segment.Source != null, $"Segment {segment} source was null");
            }

            return analysisSegments;
        }

        private static void CleanupAfterSegment<TSource>(
            SegmentSettings<TSource> settings,
            bool hasUniqueDirectoryPerSegment,
            bool shouldSaveAudioSegment)
        {
            Debug.Assert(
                settings.SegmentAudioFile.DirectoryName == settings.SegmentTempDirectory.FullName,
                "The used audio file should be saved in the segment temp directory (but wasn't).");

            int id = settings.InstanceId;

            // delete the prepared audio file segment or move it to output folder if we are keeping it
            if (shouldSaveAudioSegment)
            {
                Log.Debug(
                    $"Item {id} moved file {settings.SegmentAudioFile.FullName} to output because " +
                    $"saveIntermediateWavFiles was not set to {nameof(SaveBehavior.Never)}");

                var destination = Path.Combine(
                    settings.SegmentOutputDirectory.FullName,
                    settings.SegmentAudioFile.Name);
                settings.SegmentAudioFile.MoveTo(destination);
            }
            else
            {
                // failing is not fatal, but it does mean we'll be leaving a file behind.
                settings.SegmentAudioFile.TryDelete($"AnalysisSettings Item: {id}");
            }

            // if there's a unique directory per segment we just delete folder
            // however if it is shared we can only delete resources inside it (see above)
            // as all instances use the same directory!
            if (hasUniqueDirectoryPerSegment)
            {
                if (settings.SegmentOutputDirectory == settings.SegmentTempDirectory)
                {
                    Log.Debug(
                        "Not deleting segment temp directory because it is identical to segment output directory");
                }
                else
                {
                    // delete the directory created for this run
                    // Failing is not fatal, but it does mean we'll be leaving a dir behind.
                    settings.SegmentTempDirectory.TryDelete(recursive: true, message: $"AnalysisSettings Item: {id}");
                }
            }
        }

        /// <summary>
        /// This method simply ensures that certain requirements are fulfilled by IAnalyser2.Analyze results.
        /// It only runs when the program is built as DEBUG.
        /// </summary>
        [Conditional("DEBUG")]
        private static void ValidateResult(
            AnalysisSettings preAnalysisSettings,
            AnalysisResult2 result,
            TimeSpan start,
            TimeSpan preparedFileDuration,
            bool parallelized)
        {
            if (parallelized)
            {
                Log.Warn("VALIDATION OF ANALYSIS RESULTS BYPASSED BECAUSE THE ANALYSIS IS IN PARALLEL!");
                return;
            }

            Debug.Assert(
                result.SettingsUsed != null,
                "The settings used in the analysis must be populated in the analysis result.");
            Debug.Assert(
                result.SegmentStartOffset == start,
                "The segment start offset of the result should match the start offset that it was instructed to analyze");
            Debug.Assert(
                Math.Abs((result.SegmentAudioDuration - preparedFileDuration).TotalMilliseconds) < 1.0,
                "The duration analyzed (reported by the analysis result) should be withing a millisecond of the provided audio file");

            if (preAnalysisSettings.SegmentSettings.SegmentImageFile != null)
            {
                if (preAnalysisSettings.AnalysisSaveBehavior == SaveBehavior.Always
                    || (preAnalysisSettings.AnalysisSaveBehavior == SaveBehavior.WhenEventsDetected
                        && result.Events.Length > 0))
                {
                    Debug.Assert(
                        preAnalysisSettings.SegmentSettings.SegmentImageFile.Exists,
                        "If the analysis was instructed to produce an image file, then it should exist");
                }
            }

            Debug.Assert(
                result.Events != null,
                "The Events array should never be null. No events should be represented by a zero length Events array.");
            if (result.Events.Length != 0 && preAnalysisSettings.SegmentSettings.SegmentEventsFile != null)
            {
                Debug.Assert(
                    result.EventsFile.Exists,
                    "If events were produced and an events file was expected, then the events file should exist");
            }

            Debug.Assert(
                result.SummaryIndices != null,
                "The SummaryIndices array should never be null. No SummaryIndices should be represented by a zero length SummaryIndices array.");
            if (result.SummaryIndices.Length != 0 && preAnalysisSettings.SegmentSettings.SegmentSummaryIndicesFile != null)
            {
                Debug.Assert(
                    result.SummaryIndicesFile.Exists,
                    "If SummaryIndices were produced and an SummaryIndices file was expected, then the SummaryIndices file should exist");
            }

            Debug.Assert(
                result.SpectralIndices != null,
                "The SpectralIndices array should never be null. No SpectralIndices should be represented by a zero length SpectralIndices array.");
            if (result.SpectralIndices.Length != 0 && preAnalysisSettings.SegmentSettings.SegmentSpectrumIndicesDirectory != null)
            {
                foreach (var spectraIndicesFile in result.SpectraIndicesFiles)
                {
                    Debug.Assert(
                        spectraIndicesFile.Exists,
                        "If SpectralIndices were produced and SpectralIndices files were expected, then the SpectralIndices files should exist");
                }
            }

            foreach (var eventBase in result.Events)
            {
                Debug.Assert(
                    eventBase.StartOffset >= result.SegmentStartOffset,
                    "Every event detected by this analysis should of been found within the bounds of the segment analyzed");
                Debug.Assert(
                    Math.Abs((eventBase.EventStartSeconds % 60.0) - (eventBase.StartOffset.TotalSeconds % 60)) < 0.001,
                    "The relative EventStartSeconds should equal the seconds component of StartOffset");
                Debug.Assert(
                    eventBase.SegmentStartOffset == result.SegmentStartOffset,
                    "Segment start offsets must match");
            }

            foreach (var summaryIndexBase in result.SummaryIndices)
            {
                Debug.Assert(
                    summaryIndexBase.StartOffset >= result.SegmentStartOffset,
                    "Every summary index generated by this analysis should of been found within the bounds of the segment analyzed");
            }

            foreach (var spectralIndexBase in result.SpectralIndices)
            {
                Debug.Assert(
                    spectralIndexBase.StartOffset >= result.SegmentStartOffset,
                    "Every spectral index generated by this analysis should of been found within the bounds of the segment analyzed");
            }
        }

        private static (DirectoryInfo Output, DirectoryInfo Temp) PrepareSegmentDirectories<T>(string analysisIdentifier, AnalysisSettingsBase settings, ISegment<T> uniqueDirectoryPerSegment)
        {
            Contract.Requires(analysisIdentifier.IsNotEmpty(), "analysis must not be null.");
            Contract.Requires(settings != null, "settings must not be null.");
            Contract.Requires(
                settings.AnalysisOutputDirectory != null,
                $"{nameof(settings.AnalysisOutputDirectory)} is not set.");

            // create directory for analysis run
            var output = CreateRunDirectory(
                settings.AnalysisOutputDirectory,
                analysisIdentifier,
                uniqueDirectoryPerSegment);

            // create temp directory for analysis run
            var temp = CreateRunDirectory(
                settings.IsAnalysisTempDirectoryValid ? settings.AnalysisTempDirectory : settings.AnalysisTempDirectoryFallback,
                analysisIdentifier,
                uniqueDirectoryPerSegment);

            Contract.Ensures(output != null, "SegmentOutputDirectory was not set.");
            Contract.Ensures(temp != null, "SegmentOutputDirectory was not set.");
            Contract.Ensures(Directory.Exists(output.FullName), "SegmentOutputDirectory did not exist.");
            Contract.Ensures(Directory.Exists(temp.FullName), "SegmentOutputDirectory did not exist.");

            return (output, temp);
        }

        /// <summary>
        /// Create a directory for an analysis to be run.
        /// Will be in the form [analysisId][sep][token][sep][...files...].
        /// </summary>
        /// <param name="analysisBaseDirectory">
        /// The analysis Base Directory.
        /// </param>
        /// <param name="analysisIdentifier">
        /// Analysis Identifier.
        /// </param>
        /// <returns>
        /// The created directory.
        /// </returns>
        private static DirectoryInfo CreateRunDirectory<T>(DirectoryInfo analysisBaseDirectory, string analysisIdentifier, ISegment<T> unique = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(analysisIdentifier), "analysisIdentifier must be set.");

            var token = string.Empty;
            if (unique.NotNull())
            {
                token = unique.Source + unique.StartOffsetSeconds.ToString("000000.00");
            }

            var runDirectory = Path.Combine(analysisBaseDirectory.FullName, analysisIdentifier, token);

            var dir = new DirectoryInfo(runDirectory);
            Directory.CreateDirectory(runDirectory);

            Contract.Ensures(Directory.Exists(dir.FullName), "Directory was not created.");

            return dir;
        }

        /// <summary>
        /// Analyze segments of an audio file in parallel.
        /// </summary>
        /// <param name="analysisSegments">
        /// The analysis Segments.
        /// </param>
        /// <param name="analysis">
        /// The analysis.
        /// </param>
        /// <param name="clonedSettings">
        /// The settings.
        /// </param>
        /// <returns>
        /// The analysis results.
        /// </returns>
        private AnalysisResult2[] RunParallel<TSource>(
            ICollection<ISegment<TSource>> analysisSegments,
            IAnalyser2 analysis,
            AnalysisSettingsBase clonedSettings)
        {
            var analysisSegmentsCount = analysisSegments.Count;
            var results = new AnalysisResult2[analysisSegmentsCount];

            // much dodgy, such parallelism, so dining philosopher...
            int finished = 0;

            void DelegateBody(ISegment<TSource> item, ParallelLoopState state, long index)
            {
                var itemClosed = item;
                var indexClosed = index;

                // can't use settings as each iteration modifies settings. This causes hard to track down bugs
                // instead create a copy of the settings, and use that
                var settingsForThisItem = (AnalysisSettingsBase)clonedSettings.Clone();

                // process item
                Log.DebugFormat(StartingItem, settingsForThisItem.InstanceId, itemClosed);

                var result = this.PrepareFileAndAnalyzeSegment(itemClosed, analysis, settingsForThisItem, true);
                if (result != null)
                {
                    results[indexClosed] = result;
                }

                // such dodgy - let's see if it works!
                finished++;
                Log.Info($"Completed segment {index + 1}/{analysisSegments.Count} - roughly {finished} completed");
            }

            Parallel.ForEach(analysisSegments, new ParallelOptions { MaxDegreeOfParallelism = 64 }, DelegateBody);

            return results;
        }

        /// <summary>
        /// Analyze segments of an audio file in sequence.
        /// </summary>
        /// <param name="analysisSegments">
        /// The analysis Segments.
        /// </param>
        /// <param name="analysis">
        /// The analysis.
        /// </param>
        /// <param name="clonedSettings">
        /// The settings.
        /// </param>
        /// <returns>
        /// The analysis results.
        /// </returns>
        private AnalysisResult2[] RunSequential<TSource>(
            ICollection<ISegment<TSource>> analysisSegments,
            IAnalyser2 analysis,
            AnalysisSettingsBase clonedSettings)
        {
            var analysisSegmentsList = analysisSegments.ToList();
            var totalItems = analysisSegmentsList.Count;
            var results = new AnalysisResult2[totalItems];

            for (var index = 0; index < analysisSegmentsList.Count; index++)
            {
                Log.Debug("Starting segment {0}/{1}".Format2(index, analysisSegmentsList.Count));

                var item = analysisSegmentsList[index];

                // process item
                // this can use settings, as it is modified each iteration, but this is run synchronously.
                Log.DebugFormat(StartingItem, clonedSettings.InstanceId, item);

                var result = this.PrepareFileAndAnalyzeSegment(item, analysis, clonedSettings, false);
                if (result != null)
                {
                    results[index] = result;
                }

                Log.Info("Completed segment {0}/{1}".Format2(index + 1, analysisSegmentsList.Count));
            }

            return results;
        }

        /// <summary>
        /// Prepare the resources for an analysis, and the run the analysis.
        /// </summary>
        /// <param name="segment">The segment to analyze.</param>
        /// <param name="analyzer">The analysis.</param>
        /// <param name="localCopyOfSettings">The settings.</param>
        /// <param name="parallelized">
        /// Set to true if this method was invoked in a parallelized context.
        /// </param>
        /// <returns>
        /// The results from the analysis.
        /// </returns>
        private AnalysisResult2 PrepareFileAndAnalyzeSegment<T>(
            ISegment<T> segment,
            IAnalyser2 analyzer,
            AnalysisSettingsBase localCopyOfSettings,
            bool parallelized)
        {
            Contract.Requires(localCopyOfSettings != null, "Settings must not be null.");
            Contract.Requires(segment != null, "File Segments must not be null.");

            // These statements are old, premised off the fact that offsets will be null... they never are anymore
            // Does that break anything?
            //var start = fileSegment.SegmentStartOffset ?? TimeSpan.Zero;
            //var end = fileSegment.SegmentEndOffset ?? fileSegment.TargetFileDuration.Value;

            // set directories
            var dirs = PrepareSegmentDirectories(analyzer.Identifier, localCopyOfSettings, this.UniqueDirectoryPerSegment ? segment : null);

            // create the file for the analysis
            // save created audio file to settings.SegmentTempDirectory
            var task = this.SourcePreparer.PrepareFile(
                dirs.Temp,
                segment.Source,
                localCopyOfSettings.SegmentMediaType,
                segment.StartOffsetSeconds.Seconds(),
                segment.EndOffsetSeconds.Seconds(),
                localCopyOfSettings.AnalysisTargetSampleRate,
                dirs.Temp,
                localCopyOfSettings.AnalysisChannelSelection,
                localCopyOfSettings.AnalysisMixDownToMono);

            // de-async this method
            task.Wait();

            var preparedFile = task.Result;

            var preparedFilePath = preparedFile.TargetFile;
            var preparedFileDuration = preparedFile.TargetFileDuration.Value;

            // Store sample rate of original audio file in the Settings object.
            // May need original SR during the analysis, esp if have upsampled from the original SR.
            localCopyOfSettings.SampleRateOfOriginalAudioFile = fileSegment.TargetFileSampleRate;

            //localCopyOfSettings.SegmentSettings.SegmentAudioFile = preparedFilePath;
            //localCopyOfSettings.SegmentSettings.SegmentStartOffset = start;
            //localCopyOfSettings.SegmentSettings.AnalysisIdealSegmentDuration = end - start;

            var segmentSettings = new SegmentSettings<T>(localCopyOfSettings, segment, dirs, preparedFile);

            string fileName = Path.GetFileNameWithoutExtension(preparedFile.TargetFile.Name);

            localCopyOfSettings.AnalysisSaveBehavior = this.saveImageFiles;

            // if user requests, save the sonogram files
            if (this.saveImageFiles != SaveBehavior.Never)
            {
                // save spectrogram to output dir - saving to temp dir means possibility of being overwritten
                localCopyOfSettings.SegmentSettings.SegmentImageFile =
                    new FileInfo(Path.Combine(
                        localCopyOfSettings.SegmentSettings.SegmentOutputDirectory.FullName,
                        fileName + ".png"));
            }

            Log.DebugFormat($"Item {segmentSettings.InstanceId} started analysing file {segmentSettings.SegmentAudioFile.Name}.");
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // RUN the ANALYSIS
            AnalysisResult2 result = analyzer.Analyze(localCopyOfSettings);

            stopwatch.Stop();
            Log.DebugFormat("Item {0} finished analysing {1}, took {2}.", localCopyOfSettings.InstanceId,
                localCopyOfSettings.SegmentSettings.SegmentAudioFile.Name, stopwatch.Elapsed);

            // add information to the results
            result.AnalysisIdentifier = analyzer.Identifier;

            // validate results (debug only & not parallel only)
            ValidateResult(localCopyOfSettings, result, start, preparedFileDuration, parallelized);

            // clean up
            CleanupAfterSegment(localCopyOfSettings, this.UniqueDirectoryPerSegment, this.saveIntermediateWavFiles.ShouldSave(result.Events.Length));

            return result;
        }
    }
}
