﻿namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using log4net;
    using System.Diagnostics;

    /// <summary>
    /// Prepares, runs and completes analyses.
    /// 
    /// *** DO NOT CHANGE THIS CLASS UNLESS INSTRUCTED TOO ***
    /// </summary>
    /// <remarks>
    /// <para>
    /// The process to analyse files can be a little complex. The overall idea is 
    /// to begin with an analysis type and a list of file paths and segments inside those files.
    /// Then those files are segmented using default settings from the analysis and possible modifications to the defaults by a user.
    /// Each segment is analysed, and the results are put into either a purpose-created folder (which might be deledt once that analysis is complete),
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
        private const string CancelledItem = "Cancellation requested for {0} analysis {1}. Finished item {2}: {3}.";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly bool saveIntermediateWavFiles;
        private readonly bool saveImageFiles;
        private readonly bool saveIntermediateCsvFiles;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisCoordinator"/> class.
        /// </summary>
        /// <param name="sourcePreparer">
        /// The source Preparer. The prepared files can be stored anywhere, they just need to be readable.
        /// </param>
        /// <param name="saveIntermediateWavFiles"></param>
        /// <param name="saveImageFiles"></param>
        /// <param name="saveIntermediateCsvFiles"></param>
        public AnalysisCoordinator(ISourcePreparer sourcePreparer, bool saveIntermediateWavFiles, bool saveImageFiles, bool saveIntermediateCsvFiles)
        {
            Contract.Requires(sourcePreparer != null);

            this.saveIntermediateWavFiles = saveIntermediateWavFiles;
            this.saveImageFiles = saveImageFiles;
            this.saveIntermediateCsvFiles = saveIntermediateCsvFiles;
            if (sourcePreparer == null)
            {
                throw new NullReferenceException("sourcePreparer must not be null");
            }

            this.SourcePreparer = sourcePreparer;

            this.DeleteFinished = false;
            this.SubFoldersUnique = true;
            this.IsParallel = false;
        }

        /// <summary>
        /// Gets SourcePreparer.
        /// </summary>
        public ISourcePreparer SourcePreparer { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether to delete finished runs.
        /// </summary>
        public bool DeleteFinished { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to create 
        /// uniquely named sub folders for each run, 
        /// or reuse a single folder named using the analysis name.
        /// </summary>
        public bool SubFoldersUnique { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to run in parallel.
        /// </summary>
        public bool IsParallel { get; set; }

        /// <summary>
        /// Analyse one file using the analysis and settings.
        /// </summary>
        /// <param name="file">
        /// The file.
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
        public AnalysisResult2[] Run(FileInfo file, IAnalyser2 analysis, AnalysisSettings settings)
        {
            return this.Run(new List<FileSegment>() { new FileSegment(file) }, analysis, settings);
        }

        /// <summary>
        /// Analyse one file segment using the analysis and settings.
        /// </summary>
        /// <param name="fileSegment">
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
        public AnalysisResult2[] Run(FileSegment fileSegment, IAnalyser2 analysis, AnalysisSettings settings)
        {
            return this.Run(new List<FileSegment>() {fileSegment}, analysis, settings);
        }

        /// <summary>
        /// Analyse one or more file segments using the same analysis and settings.
        /// </summary>
        /// <param name="fileSegments">
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
       public AnalysisResult2[] Run(IEnumerable<FileSegment> fileSegments, IAnalyser2 analysis, AnalysisSettings settings)
        {
            Contract.Requires(settings != null, "Settings must not be null.");
            Contract.Requires(analysis != null, "Analysis must not be null.");
            Contract.Requires(fileSegments != null, "File Segments must not be null.");
            Contract.Requires(fileSegments.All(s => s.Validate()), "File Segment must be valid.");

            // calculate the sub-segments of the given file segments that match what the analysis expects.
            var analysisSegments = this.SourcePreparer.CalculateSegments(fileSegments, settings).ToList();

            // check last segment and remove if too short
            var finalSegment = analysisSegments[analysisSegments.Count() - 1];
            var duration = finalSegment.SegmentEndOffset - finalSegment.SegmentStartOffset;
            if (duration < settings.SegmentMinDuration)
            {
                analysisSegments.Remove(finalSegment);
            }

            AnalysisResult2[] results;

            // clone analysis settings for parallelism conerns:
            //  - as each iteration modifies settings. This causes hard to track down bugs
            // clones are made for sequential runs to to ensure consistency
            var settingsForThisItem = (AnalysisSettings)settings.Clone();

            Log.InfoFormat("Analysis started in {0}.", this.IsParallel ? "parallel" : "sequence");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Analyse the sub-segments in parallel or sequentially (IsParallel property), 
            // Create and delete directories and/or files as indicated by properties
            // DeleteFinished and SubFoldersUnique
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

            // delete temp directories - may not want to do this
            // don't do this - leads to problems when running using powershell script
            //DeleteDirectory(settings.InstanceId, settings.AnalysisBaseTempDirectoryChecked);

            //if (settings.AnalysisBaseTempDirectory != null)
            //{
            //    DeleteDirectory(settings.InstanceId, settings.AnalysisBaseTempDirectory);
            //}

            return results;
        }

        /// <summary>
        /// Analyse segments of an audio file in parallel.
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
        private AnalysisResult2[] RunParallel(List<FileSegment> analysisSegments, IAnalyser2 analysis, AnalysisSettings clonedSettings)
        {
            var analysisSegmentsCount = analysisSegments.Count;
            var results = new AnalysisResult2[analysisSegmentsCount];

            // much dodgy, such parallelism, so dining philosopher...
            int finished = 0;


            Parallel.ForEach(
                analysisSegments,
                new ParallelOptions() { MaxDegreeOfParallelism = 64 },
                (item, state, index) =>
                    {
                        var itemClosed = item;
                        var indexClosed = index;

                        // can't use settings as each iteration modifies settings. This causes hard to track down bugs
                        // instead create a copy of the settings, and use that
                        var settingsForThisItem = (AnalysisSettings)clonedSettings.Clone();

                        // process item
                        var result = ProcessItem(itemClosed, analysis, settingsForThisItem, parallelised: true);
                        if (result != null)
                        {
                            results[indexClosed] = result;
                        }

                        // such dodgy - let's see if it works!
                        finished++;
                        Log.Info("Completed segment {0}/{1} - roughly {2} completed".Format2(index + 1, analysisSegments.Count, finished));
                    });

            return results;
        }

        /// <summary>
        /// Analyse segments of an audio file in sequence.
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
       private AnalysisResult2[] RunSequential(IEnumerable<FileSegment> analysisSegments, IAnalyser2 analysis, AnalysisSettings clonedSettings)
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
                var result = this.ProcessItem(item, analysis, clonedSettings, parallelised: false);
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
        /// <param name="fileSegment">
        ///     The file Segment.
        /// </param>
        /// <param name="analyser">
        ///     The analysis.
        /// </param>
        /// <param name="localCopyOfSettings">
        ///     The settings.
        /// </param>
        /// <param name="parallelised"></param>
        /// <returns>
        /// The results from the analysis.
        /// </returns>
        private AnalysisResult2 PrepareFileAndRunAnalysis(FileSegment fileSegment, IAnalyser2 analyser, AnalysisSettings localCopyOfSettings, bool parallelised)
       {
           Contract.Requires(localCopyOfSettings != null, "Settings must not be null.");
            Contract.Requires(fileSegment != null, "File Segments must not be null.");
            Contract.Requires(fileSegment.Validate(), "File Segment must be valid.");


            var start = fileSegment.SegmentStartOffset.HasValue ? fileSegment.SegmentStartOffset.Value : TimeSpan.Zero;
            var end = fileSegment.SegmentEndOffset.HasValue
                ? fileSegment.SegmentEndOffset.Value
                : fileSegment.OriginalFileDuration;
           
           // set directories
            this.PrepareDirectories(analyser, localCopyOfSettings);

            var tempDir = localCopyOfSettings.AnalysisInstanceTempDirectoryChecked;

            // create the file for the analysis
            // save created audio file to settings.AnalysisInstanceTempDirectory if given, otherwise settings.AnalysisInstanceOutputDirectory
            var preparedFile = this.SourcePreparer.PrepareFile(
                this.GetInstanceDirTempElseOutput(localCopyOfSettings),
                fileSegment.OriginalFile,
                localCopyOfSettings.SegmentMediaType,
                start,
                end,
                localCopyOfSettings.SegmentTargetSampleRate,
                tempDir, 
                mixDownToMono: true);

            var preparedFilePath = preparedFile.OriginalFile;
            var preparedFileDuration = preparedFile.OriginalFileDuration;

            // Store sample rate of original audio file in the Settings object.
            // May need original SR during the analysis, esp if have upsampled from the original SR.
            localCopyOfSettings.SampleRateOfOriginalAudioFile = preparedFile.OriginalFileSampleRate;

            localCopyOfSettings.AudioFile = preparedFilePath;
            localCopyOfSettings.SegmentStartOffset = start;

            string fileName = Path.GetFileNameWithoutExtension(preparedFile.OriginalFile.Name);

            // if user requests, save the sonogram files 
            if (this.saveImageFiles)
            {
                // save spectrogram to output dir - saving to temp dir means possibility of being overwritten
                localCopyOfSettings.ImageFile =
                    new FileInfo(Path.Combine(localCopyOfSettings.AnalysisInstanceOutputDirectory.FullName, fileName + ".png"));
            }

            // if user requests, save the intermediate csv files 
            if (this.saveIntermediateCsvFiles)
            {
                // always save csv to output dir
                localCopyOfSettings.EventsFile =
                    new FileInfo(
                        Path.Combine(
                            localCopyOfSettings.AnalysisInstanceOutputDirectory.FullName,
                            fileName + ".Events.csv"));
                localCopyOfSettings.SummaryIndicesFile =
                    new FileInfo(
                        Path.Combine(
                            localCopyOfSettings.AnalysisInstanceOutputDirectory.FullName,
                            fileName + ".Indices.csv"));
                localCopyOfSettings.SpectrumIndicesDirectory = new DirectoryInfo(localCopyOfSettings.AnalysisInstanceOutputDirectory.FullName);
            }

            Log.DebugFormat("Item {0} started analysing file {1}.", localCopyOfSettings.InstanceId, localCopyOfSettings.AudioFile.Name);
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            /* ##### RUN the ANALYSIS ################################################################ */
            AnalysisResult2 result = analyser.Analyse(localCopyOfSettings);
            /* ####################################################################################### */

            stopwatch.Stop();
            Log.DebugFormat("Item {0} finished analysing {1}, took {2}.", localCopyOfSettings.InstanceId, localCopyOfSettings.AudioFile.Name, stopwatch.Elapsed);

            // add information to the results
            result.AnalysisIdentifier = analyser.Identifier;

            // validate results (debug only & not parallel only)
            ValidateResult(localCopyOfSettings, result, start, preparedFileDuration, parallelised);

            // clean up
            if (this.DeleteFinished && this.SubFoldersUnique)
            {
                // delete the directory created for this run
                this.DeleteDirectory(localCopyOfSettings.InstanceId, localCopyOfSettings.AnalysisInstanceOutputDirectory);
            }
            else if (this.DeleteFinished && !this.SubFoldersUnique)
            {
                // delete the prepared audio file segment. Don't delete the directory - all instances use the same directory!
                if (this.saveIntermediateWavFiles)
                {
                    Log.DebugFormat("File {0} not deleted because saveIntermediateWavFiles was set to true", localCopyOfSettings.AudioFile.FullName);
                }       
                else
                {
                    try
                    {
                        File.Delete(localCopyOfSettings.AudioFile.FullName);
                        Log.DebugFormat("Item {0} deleted file {1}.", localCopyOfSettings.InstanceId, localCopyOfSettings.AudioFile.FullName);
                    }
                    catch (Exception ex)
                    {
                        // this error is not fatal, but it does mean we'll be leaving an audio file behind.
                        Log.Warn(
                            string.Format(
                                "Item {0} could not delete audio file {1}.",
                                localCopyOfSettings.InstanceId,
                                localCopyOfSettings.AudioFile.FullName),
                            ex);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// This method simply ensures that certain requirements are fullfilled by IAnalyser2.Analyse results.
        /// It only runs when the program is built as DEBUG.
        /// </summary>
       [Conditional("DEBUG")]
       private static void ValidateResult(AnalysisSettings preAnalysisSettings, AnalysisResult2 result, TimeSpan start, TimeSpan preparedFileDuration, bool parallelised)
       {
            if (parallelised)
            {
                Log.Warn("VALIDATION OF ANALYSIS RESULTS BYPASSED BECAUSE THE ANALYSIS IS IN PARALLEL!");
                return;
            }

           Debug.Assert(result.SettingsUsed != null, "The settings used in the analysis must be populated in the analysis result.");
           Debug.Assert(result.SegmentStartOffset == start, "The segmen start offset of the result should match the start offset that it was instructed to analyse");
           Debug.Assert(Math.Abs((result.SegmentAudioDuration - preparedFileDuration).TotalMilliseconds) < 1.0, "The duration analysed (reported by the analysis result) should be withing a millisecond of the provided audio file");

           if (preAnalysisSettings.ImageFile != null)
           {
               Debug.Assert(preAnalysisSettings.ImageFile.Exists, "If the analysis was instructed to produce an image file, then it should exist");
           }

           Debug.Assert(result.Events != null, "The Events array should never be null. No events should be represted by a zero length Events array.");
           if (result.Events.Length != 0 && preAnalysisSettings.EventsFile != null)
           {
               Debug.Assert(
                   result.EventsFile.Exists,
                   "If events were produced and an events file was expected, then the events file should exist");
           }

           Debug.Assert(result.SummaryIndices != null, "The SummaryIndices array should never be null. No SummaryIndices should be represented by a zero length SummaryIndices array.");
           if (result.SummaryIndices.Length != 0 && preAnalysisSettings.SummaryIndicesFile != null)
           {
               Debug.Assert(
                   result.SummaryIndicesFile.Exists,
                   "If SummaryIndices were produced and an SummaryIndices file was expected, then the SummaryIndices file should exist");
           }

           Debug.Assert(result.SpectralIndices != null, "The SpectralIndices array should never be null. No SpectralIndices should be represented by a zero length SpectralIndices array.");
           if (result.SpectralIndices.Length != 0 && preAnalysisSettings.SpectrumIndicesDirectory != null)
           {
               foreach (var spectraIndicesFile in result.SpectraIndicesFiles)
               {
                   Debug.Assert(spectraIndicesFile.Exists, "If SpectralIndices were produced and SpectralIndices files were expected, then the SpectralIndices files should exist");
               }
           }


           foreach (var eventBase in result.Events)
           {
               Debug.Assert(
                   eventBase.StartOffset >= result.SegmentStartOffset,
                   "Every event detected by this analysis should of been found within the bounds of the segment analysed");
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
                   "Every summary index generated by this analysis should of been found within the bounds of the segment analysed");
           }

           foreach (var spectralIndexBase in result.SpectralIndices)
           {
               Debug.Assert(
                   spectralIndexBase.StartOffset >= result.SegmentStartOffset,
                   "Every spectral index generated by this analysis should of been found within the bounds of the segment analysed");
           }
       }


        private void PrepareDirectories(IAnalyser2 analysis, AnalysisSettings settings)
        {
            Contract.Requires(analysis != null, "analysis must not be null.");
            Contract.Requires(settings != null, "settings must not be null.");
            Contract.Requires(settings.AnalysisBaseOutputDirectory != null, "AnalysisBaseOutputDirectory is not set.");
            Contract.Ensures(settings.AnalysisInstanceOutputDirectory != null, "AnalysisInstanceOutputDirectory was not set.");
            Contract.Ensures(Directory.Exists(settings.AnalysisInstanceOutputDirectory.FullName), "AnalysisInstanceOutputDirectory did not exist.");

            // create directory for analysis run
            settings.AnalysisInstanceOutputDirectory = this.SubFoldersUnique
                                                       ? this.CreateUniqueRunDirectory(settings.AnalysisBaseOutputDirectory, analysis.Identifier)
                                                       : this.CreateNamedRunDirectory(settings.AnalysisBaseOutputDirectory, analysis.Identifier);

            if (!Directory.Exists(settings.AnalysisInstanceOutputDirectory.FullName))
            {
                Directory.CreateDirectory(settings.AnalysisInstanceOutputDirectory.FullName);
            }

            if (settings.AnalysisBaseTempDirectory != null)
            {
                // create temp directory  for analysis run
                settings.AnalysisInstanceTempDirectory = this.SubFoldersUnique
                                                           ? this.CreateUniqueRunDirectory(settings.AnalysisBaseTempDirectory, analysis.Identifier)
                                                           : this.CreateNamedRunDirectory(settings.AnalysisBaseTempDirectory, analysis.Identifier);

                if (!Directory.Exists(settings.AnalysisInstanceTempDirectory.FullName))
                {
                    Directory.CreateDirectory(settings.AnalysisInstanceTempDirectory.FullName);
                }
            }
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
        private DirectoryInfo CreateUniqueRunDirectory(DirectoryInfo analysisBaseDirectory, string analysisIdentifier)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(analysisIdentifier), "analysisIdentifier must be set.");
            Contract.Ensures(Contract.Result<DirectoryInfo>() != null, "Directory was null.");
            Contract.Ensures(Directory.Exists(Contract.Result<DirectoryInfo>().FullName), "Directory was not created.");

            var dirName = Path.GetRandomFileName();
            var runDirectory = Path.Combine(analysisBaseDirectory.FullName, analysisIdentifier, dirName);

            var dir = new DirectoryInfo(runDirectory);
            Directory.CreateDirectory(runDirectory);
            return dir;
        }

        /// <summary>
        /// Create a directory for an analysis to be run.
        /// Will be in the form [analysisId][sep][...files...].
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
        private DirectoryInfo CreateNamedRunDirectory(DirectoryInfo analysisBaseDirectory, string analysisIdentifier)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(analysisIdentifier), "analysisIdentifier must be set.");
            Contract.Ensures(Contract.Result<DirectoryInfo>() != null, "Directory was null.");
            Contract.Ensures(Directory.Exists(Contract.Result<DirectoryInfo>().FullName), "Directory was not created.");

            var runDirectory = Path.Combine(analysisBaseDirectory.FullName, analysisIdentifier);

            var dir = new DirectoryInfo(runDirectory);
            Directory.CreateDirectory(runDirectory);
            return dir;
        }

        /// <summary>
        /// Get analysers using a method that is compatible with MONO environment..
        /// </summary>
        /// <param name="assembly">
        /// The assembly.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; AnalysisBase.IAnalyser2].
        /// </returns>
        public static IEnumerable<IAnalyser2> GetAnalysers(Assembly assembly)
        {
            // to find the assembly, get the type of a class in that assembly
            // eg. typeof(MainEntry).Assembly
            var analyserType = typeof(IAnalyser2);

            var analysers = assembly.GetTypes()
                .Where(analyserType.IsAssignableFrom)
                .Select(t => Activator.CreateInstance(t) as IAnalyser2);

            return analysers;
        }

        private AnalysisResult2 ProcessItem(FileSegment item, IAnalyser2 analysis, AnalysisSettings clonedSettings, bool parallelised)
        {
            Log.DebugFormat(StartingItem, clonedSettings.InstanceId, item);

            AnalysisResult2 result = this.PrepareFileAndRunAnalysis(item, analysis, clonedSettings, parallelised);

            return result;
        }

        private DirectoryInfo GetInstanceDirTempElseOutput(AnalysisSettings settings)
        {
            if (settings.AnalysisBaseTempDirectory != null && settings.AnalysisInstanceTempDirectory != null)
            {
                return settings.AnalysisInstanceTempDirectory;
            }
            else
            {
                return settings.AnalysisInstanceOutputDirectory;
            }
        }

        private void DeleteDirectory(int settingsInstanceId, DirectoryInfo dir)
        {
            try
            {
                Directory.Delete(dir.FullName, true);
                Log.DebugFormat("Item {0} deleted directory {1}.", settingsInstanceId, dir.FullName);
            }
            catch (Exception ex)
            {
                // this error is not fatal, but it does mean we'll be leaving a dir behind.

                Log.Warn(string.Format("Item {0} could not delete directory {1}.",
                    settingsInstanceId, dir.FullName), ex);
            }
        }
    }
}
