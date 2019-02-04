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
    using System.Threading;
    using System.Threading.Tasks;
    using Acoustics.Shared.Contracts;
    using log4net;
    using Segment;

    /// <summary>
    /// Prepares, runs and completes analyses.
    ///
    /// *** DO NOT CHANGE THIS CLASS UNLESS INSTRUCTED TOO ***
    /// </summary>
    /// <remarks>
    /// <para>
    /// The process to analyze files can be complex. The overall idea is
    /// to begin with an analysis type and a list of audio objects and segments.
    /// Then those files are segmented using default settings from the analysis and
    /// possible modifications to the defaults by a user.
    /// Each segment is analyzed, and the results are put into either a purpose-created
    /// folder (which might be deleted once that analysis is complete), or a known location for later use.
    /// </para>
    /// <para>
    /// Temp files can also be stored in sub folders named by analysis name and files named by segment id
    /// when another analysis is run, the files are overwritten.
    /// </para>
    /// </remarks>
    public class AnalysisCoordinator
    {
        private const string StartingItem = "Starting item {0}: {1}.";

        private const int TaskTimeoutSeconds = 240;

        private static readonly ILog Log = LogManager.GetLogger(nameof(AnalysisCoordinator));

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
            bool uniqueDirectoryPerSegment = true,
            bool isParallel = false)
        {
            Contract.Requires<ArgumentNullException>(sourcePreparer != null, "sourcePreparer must not be null");

            this.saveIntermediateWavFiles = saveIntermediateWavFiles;

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
        /// Get analyzers using a method that is compatible with MONO environment..
        /// </summary>
        /// <param name="assembly">
        /// The assembly.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; AnalysisBase.IAnalyzer2].
        /// </returns>
        public static IEnumerable<T> GetAnalyzers<T>(Assembly assembly)
            where T : class, IAnalyser2
        {
            // to find the assembly, get the type of a class in that assembly
            // eg. typeof(MainEntry).Assembly
            var analyzerType = typeof(T);

            var types = assembly.GetTypes();
            var analyzers = types
                .Where(analyzerType.IsAssignableFrom)
                .Where(t => t.IsClass && !t.IsAbstract)
                .Select(t => Activator.CreateInstance(t) as T);

            return analyzers;
        }

        /// <summary>
        /// Gets a named output directory. For example, if <paramref name="baseDir"/> is "C:\Temp" and
        /// <paramref name="analyzer"/> is the indices analysis, the result will be "C:\Temp\Towsey.Acoustic".
        /// </summary>
        /// <param name="baseDir">The base output directory (either normal output or a temp directory).</param>
        /// <param name="analyzer">The <see cref="IAnalyser2"/> to extract the identifier from.</param>
        /// <param name="subFolders">An optional list of sub folders to append to the path.</param>
        /// <returns>A combined directory made up of all the path fragments.</returns>
        public static DirectoryInfo GetNamedDirectory(
            DirectoryInfo baseDir,
            IAnalyser2 analyzer,
            params string[] subFolders)
        {
            Contract.Requires(analyzer.NotNull(), "analyzer must be not null");
            Contract.Requires(!string.IsNullOrWhiteSpace(analyzer.Identifier), "analysisIdentifier must be set.");

            return baseDir.Combine(subFolders.Prepend(analyzer.Identifier));
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
        public AnalysisResult2[] Run<TSource>(ISegment<TSource> segment, IAnalyser2 analysis, AnalysisSettings settings)
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
           AnalysisSettings settings)
        {
            Contract.Requires(settings != null, "Settings must not be null.");
            Contract.Requires(analysis != null, "Analysis must not be null.");
            Contract.Requires(segments != null, "File Segments must not be null.");

            // do not allow the program to continue
            // if there are no possible segments to process because the original file
            // is too short.
            var tooShort = segments
                .FirstOrDefault(segment => segment.SourceMetadata.Duration < settings.AnalysisMinSegmentDuration);
            if (tooShort != null)
            {
                Log.Fatal("Provided audio recording is too short too analyze!");
                throw new AudioRecordingTooShortException(
                    "{0} is too short to analyze with current analysisSettings.AnalysisMinSegmentDuration ({1})"
                        .Format2(tooShort.Source, settings.AnalysisMinSegmentDuration));
            }

            // ensure output directory exists
            Contract.Requires(
                settings.AnalysisOutputDirectory.TryCreate(),
                $"Attempt to create AnalysisOutputDirectory failed: {settings.AnalysisOutputDirectory}");

            // try and create temp directory (returns true if already exists)
            if (!settings.IsAnalysisTempDirectoryValid)
            {
                // ensure a temp directory is always set
                Log.Warn(
                    "No temporary directory provided, using random directory: " +
                    settings.AnalysisTempDirectoryFallback);
            }

            // calculate the sub-segments of the given file segments that match what the analysis expects.
            var analysisSegments = PrepareAnalysisSegments(this.SourcePreparer, segments, settings);

            // Execute a pre analyzer hook
            Log.Info("Executing BeforeAnalyze");
            analysis.BeforeAnalyze(settings);
            Log.Debug("Completed BeforeAnalyze");

            AnalysisResult2[] results;

            // clone analysis settings for parallelism concerns:
            //  - as each iteration modifies settings. This causes hard to track down bugs
            // clones are made for sequential runs to to ensure consistency
            var settingsForThisItem = (AnalysisSettings)settings.Clone();

            Log.Info($"Analysis started in {(this.IsParallel ? "parallel" : "sequence")}.");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Analyze the sub-segments in parallel or sequentially (IsParallel property),
            // Create and delete directories and/or files as indicated by properties
            // DeleteFinished and UniqueDirectoryPerSegment
            if (this.IsParallel)
            {
                results = this.RunParallel(analysisSegments, analysis, settingsForThisItem);

                Array.Sort(results);
            }
            else
            {
                // sequential
                results = this.RunSequential(analysisSegments, analysis, settingsForThisItem);
            }

            stopwatch.Stop();

            Log.Info($"Analysis complete, took {stopwatch.Elapsed}.");

            // TODO: execute SummariseResults hook here eventually

            // delete temp directories
            // only delete directory if we are using one that was created specifically for this analysis
            settings.AnalysisTempDirectoryFallback.TryDelete(true, $"Item {settings.InstanceId}");

            return results;
        }

        private static List<ISegment<TSource>> PrepareAnalysisSegments<TSource>(
            ISourcePreparer preparer,
            ISegment<TSource>[] segments,
            AnalysisSettings settings)
        {
            // ensure all segments are valid and have source metadata set
            double duration = 0; 
            foreach (var segment in segments)
            {
                var segmentDuration = segment.EndOffsetSeconds - segment.StartOffsetSeconds;
                duration += segmentDuration;
                Contract.Requires<InvalidSegmentException>(
                    segmentDuration > 0,
                    $"Segment {segment} was invalid because end was less than start");

                Contract.Requires<InvalidSegmentException>(segment.Source != null, $"Segment {segment} source was null");

                Contract.Requires(
                    segment.SourceMetadata.NotNull(),
                    $"Segment {segment} must have metadata supplied.");

                // it should equal itself (because it is in the list) but should not equal anything else
                var matchingSegments = segments.Where(x => x.Equals(segment)).ToArray();
                Contract.Requires<InvalidSegmentException>(
                    matchingSegments.Length == 1,
                    $"Supplied segment is a duplicate of another segment. Supplied:\n{segment}\nMatches\n: {string.Join("\n-----------\n", matchingSegments.Select(x => x.ToString()))}");
            }

            Log.Info($"Analysis Coordinator will analyze a total of {duration} seconds of audio");

            // split the provided segments up into processable chunks
            var analysisSegments = preparer.CalculateSegments(segments, settings).ToArray();

            // ensure after splitting there are no identical segments

            // we use a a dictionary here because a HashSet does not support supplying an initial capacity (uggh)
            var postCutSegments = new List<ISegment<TSource>>(analysisSegments.Length);
            foreach (var analysisSegment in analysisSegments)
            {
                // check if the segment is too short... and if so, remove it
                var tooShort = analysisSegment.EndOffsetSeconds - analysisSegment.StartOffsetSeconds <
                               settings.AnalysisMinSegmentDuration.TotalSeconds;
                if (tooShort)
                {
                    Log.Warn("Analysis segment removed because it was too short " +
                             $"(less than {settings.AnalysisMinSegmentDuration}): {analysisSegment}");
                    continue;
                }

                // ensure there are no identical segments (no use processing the same piece of audio twice
                // with the same analysis!
                // warning this is an O^2 operation.
                if (postCutSegments.Any(x => x.Equals(analysisSegment)))
                {
                    Log.Warn($"A duplicate analysis segment was removed: {analysisSegment}");
                    continue;
                }

                postCutSegments.Add(analysisSegment);
            }

            return postCutSegments;
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
                if (settings.SegmentOutputDirectory.FullName == settings.SegmentTempDirectory.FullName)
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
        /// This method simply ensures that certain requirements are fulfilled by IAnalyzer2.Analyze results.
        /// It only runs when the program is built as DEBUG.
        /// </summary>
        [Conditional("DEBUG")]
        private static void ValidateResult<T>(
            AnalysisSettings preAnalysisSettings,
            AnalysisResult2 result,
            SegmentSettings<T> segmentSettings,
            TimeSpan preparedFileDuration,
            bool parallelized)
        {
            // TODO: remove if no issues - can't determine why this would have been prevented from occurring in the first place
//            if (parallelized)
//            {
//                Log.Warn("VALIDATION OF ANALYSIS RESULTS BYPASSED BECAUSE THE ANALYSIS IS IN PARALLEL!");
//                return;
//            }

            Contract.Ensures(
                result.SettingsUsed != null,
                "The settings used in the analysis must be populated in the analysis result.");
            Contract.Ensures(
                result.SegmentStartOffset == segmentSettings.SegmentStartOffset,
                "The segment start offset of the result should match the start offset that it was instructed to analyze");
            Contract.Ensures(
                Math.Abs((result.SegmentAudioDuration - preparedFileDuration).TotalMilliseconds) < 1.0,
                "The duration analyzed (reported by the analysis result) should be withing a millisecond of the provided audio file");

            if (preAnalysisSettings.AnalysisImageSaveBehavior == SaveBehavior.Always
                || preAnalysisSettings.AnalysisImageSaveBehavior == SaveBehavior.WhenEventsDetected
                && result.Events.Length > 0)
            {
                Contract.Ensures(
                    segmentSettings.SegmentImageFile.RefreshInfo().Exists,
                    "If the analysis was instructed to produce an image file, then it should exist");
            }

            Contract.Ensures(
                result.Events != null,
                "The Events array should never be null. No events should be represented by a zero length Events array.");
            if (result.Events.Length != 0 && preAnalysisSettings.AnalysisDataSaveBehavior)
            {
                Contract.Ensures(
                    result.EventsFile.RefreshInfo().Exists,
                    "If events were produced and an events file was expected, then the events file should exist");
            }

            Contract.Ensures(
                result.SummaryIndices != null,
                "The SummaryIndices array should never be null. No SummaryIndices should be represented by a zero length SummaryIndices array.");
            if (result.SummaryIndices.Length != 0 && preAnalysisSettings.AnalysisDataSaveBehavior)
            {
                Contract.Ensures(
                    result.SummaryIndicesFile.RefreshInfo().Exists,
                    "If SummaryIndices were produced and an SummaryIndices file was expected, then the SummaryIndices file should exist");
            }

            Contract.Ensures(
                result.SpectralIndices != null,
                "The SpectralIndices array should never be null. No SpectralIndices should be represented by a zero length SpectralIndices array.");
            if (result.SpectralIndices.Length != 0 && preAnalysisSettings.AnalysisDataSaveBehavior)
            {
                foreach (var spectraIndicesFile in result.SpectraIndicesFiles)
                {
                    Contract.Ensures(
                        spectraIndicesFile.RefreshInfo().Exists,
                        "If SpectralIndices were produced and SpectralIndices files were expected, then the SpectralIndices files should exist");
                }
            }

            foreach (var eventBase in result.Events)
            {
                Contract.Ensures(
                    eventBase.ResultStartSeconds >= result.SegmentStartOffset.TotalSeconds,
                    "Every event detected by this analysis should of been found within the bounds of the segment analyzed");

                // ReSharper disable CompareOfFloatsByEqualityOperator
                Contract.Ensures(
                    eventBase.EventStartSeconds == eventBase.ResultStartSeconds,
                    "The relative EventStartSeconds should equal the seconds component of StartOffset");

                // ReSharper restore CompareOfFloatsByEqualityOperator
                Contract.Ensures(
                    Math.Abs(eventBase.SegmentStartSeconds - result.SegmentStartOffset.TotalSeconds) < 0.0001,
                    "Segment start offsets must match");

                Contract.Ensures(
                    eventBase.SegmentDurationSeconds > 0.0,
                    "eventBase.SegmentDurationSeconds must be greater than 0.0");
            }

            foreach (var summaryIndexBase in result.SummaryIndices)
            {
                Contract.Ensures(
                    summaryIndexBase.ResultStartSeconds >= result.SegmentStartOffset.TotalSeconds,
                    "Every summary index generated by this analysis should of been found within the bounds of the segment analyzed");
            }

            foreach (var spectralIndexBase in result.SpectralIndices)
            {
                Contract.Ensures(
                    spectralIndexBase.ResultStartSeconds >= result.SegmentStartOffset.TotalSeconds,
                    "Every spectral index generated by this analysis should of been found within the bounds of the segment analyzed");
            }
        }

        private static (DirectoryInfo Output, DirectoryInfo Temp) PrepareSegmentDirectories<T>(IAnalyser2 analyzer, AnalysisSettings settings, ISegment<T> uniqueDirectoryPerSegment)
        {
            Contract.Requires(analyzer.NotNull(), "analysis must not be null.");
            Contract.Requires(settings != null, "settings must not be null.");
            Contract.Requires(
                settings.AnalysisOutputDirectory != null,
                $"{nameof(settings.AnalysisOutputDirectory)} is not set.");

            // create directory for analysis run
            var output = CreateRunDirectory(
                settings.AnalysisOutputDirectory,
                analyzer,
                uniqueDirectoryPerSegment);

            // create temp directory for analysis run
            var tempBase = settings.IsAnalysisTempDirectoryValid
                ? settings.AnalysisTempDirectory
                : settings.AnalysisTempDirectoryFallback;
            var temp = CreateRunDirectory(
                tempBase,
                analyzer,
                uniqueDirectoryPerSegment);

            Contract.Ensures(output != null, "SegmentOutputDirectory was not set.");
            Contract.Ensures(temp != null, "SegmentTempDirectory was not set.");
            Contract.Ensures(Directory.Exists(output.FullName), "SegmentOutputDirectory did not exist.");
            Contract.Ensures(Directory.Exists(temp.FullName), "SegmentTempDirectory did not exist.");

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
        /// <param name="unique">Whether or not we are using the unique folder scheme.</param>
        /// <returns>
        /// The created directory.
        /// </returns>
        private static DirectoryInfo CreateRunDirectory<T>(
            DirectoryInfo baseDir,
            IAnalyser2 analyzer,
            ISegment<T> unique = null)
        {
            var token = string.Empty;
            if (unique.NotNull())
            {
                token = unique.SourceMetadata.Identifier + "_" + unique.StartOffsetSeconds.ToString("000000.00") + "-" + unique.EndOffsetSeconds.ToString("000000.00");
            }

            var runDirectory = GetNamedDirectory(baseDir, analyzer, token);

            Directory.CreateDirectory(runDirectory.FullName);

            Contract.Ensures(Directory.Exists(runDirectory.FullName), "Directory was not created.");

            return runDirectory;
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
            AnalysisSettings clonedSettings)
        {
            var analysisSegmentsCount = analysisSegments.Count;
            var results = new AnalysisResult2[analysisSegmentsCount];

            // log some helper infomration
            // We're limiting this because AP.exe just uses too much RAM on smaller machines in parallel mode.
            // Ideally I'd like to be able to intelligently increase this limit based on amount of available RAM
            // but I can't find a suitable cross platform API to determine system RAM.
            var maxDegreeOfParallelism = Environment.ProcessorCount;
            Log.Debug($"Parallel analysis limited to {maxDegreeOfParallelism} concurrent processes");
            long currentConcurrency = 0;

            // much dodgy, such parallelism, so dining philosopher...
            int finished = 0;
            
            void DelegateBody(ISegment<TSource> item, ParallelLoopState state, long index)
            {
                Interlocked.Increment(ref currentConcurrency);
                Log.Debug("Analysis Coordinator concurrency:" + Interlocked.Read(ref currentConcurrency));

                var itemClosed = item;
                var indexClosed = index;

                // can't use settings as each iteration modifies settings. This causes hard to track down bugs
                // instead create a copy of the settings, and use that
                var settingsForThisItem = (AnalysisSettings)clonedSettings.Clone();

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
                Interlocked.Decrement(ref currentConcurrency);
            }

            Parallel.ForEach(analysisSegments, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, DelegateBody);

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
            AnalysisSettings clonedSettings)
        {
            var analysisSegmentsList = analysisSegments.ToList();
            var totalItems = analysisSegmentsList.Count;
            var results = new AnalysisResult2[totalItems];

            for (var index = 0; index < analysisSegmentsList.Count; index++)
            {
                Log.Debug("Starting segment {0}/{1}".Format2(index + 1, analysisSegmentsList.Count));

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
            AnalysisSettings localCopyOfSettings,
            bool parallelized)
        {
            Contract.Requires(localCopyOfSettings != null, "Settings must not be null.");
            Contract.Requires(segment != null, "File Segments must not be null.");

            // These statements are old, premised off the fact that offsets will be null... they never are anymore
            // Does that break anything?
            //var start = fileSegment.SegmentStartOffset ?? TimeSpan.Zero;
            //var end = fileSegment.SegmentEndOffset ?? fileSegment.TargetFileDuration.Value;

            // set directories
            var dirs = PrepareSegmentDirectories(analyzer, localCopyOfSettings, this.UniqueDirectoryPerSegment ? segment : null);

            // create the file for the analysis
            // save created audio file to settings.SegmentTempDirectory
            var task = this.SourcePreparer.PrepareFile(
                dirs.Temp,
                segment,
                localCopyOfSettings.SegmentMediaType,
                localCopyOfSettings.AnalysisTargetSampleRate,
                dirs.Temp,
                localCopyOfSettings.AnalysisChannelSelection,
                localCopyOfSettings.AnalysisMixDownToMono);

            // de-async this method
            var preparedFile = task.GetAwaiter().GetResult();

            var segmentSettings = new SegmentSettings<T>(localCopyOfSettings, segment, dirs, preparedFile);

            Log.Debug($"Item {segmentSettings.InstanceId} started analyzing file {segmentSettings.SegmentAudioFile.Name}.");
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // RUN the ANALYSIS
            AnalysisResult2 result = analyzer.Analyze(localCopyOfSettings, segmentSettings);

            stopwatch.Stop();
            Log.Debug($"Item {localCopyOfSettings.InstanceId} finished analyzing {segmentSettings.SegmentAudioFile.Name}, took {stopwatch.Elapsed}.");

            // add information to the results
            result.AnalysisIdentifier = analyzer.Identifier;

            // validate results (debug only & not parallel only)
            ValidateResult(
                localCopyOfSettings,
                result,
                segmentSettings,
                preparedFile.TargetFileDuration.Value,
                parallelized);

            // clean up
            CleanupAfterSegment(
                segmentSettings,
                this.UniqueDirectoryPerSegment,
                this.saveIntermediateWavFiles.ShouldSave(result.Events.Length));

            return result;
        }
    }
}
