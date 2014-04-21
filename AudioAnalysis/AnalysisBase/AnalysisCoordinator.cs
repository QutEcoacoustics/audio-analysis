using AnalysisBase.StrongAnalyser;

namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using log4net;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// Prepares, runs and completes analyses.
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
        private readonly bool saveIntermediateWavFiles;
        private readonly bool saveImageFiles;
        private readonly bool saveIntermediateCsvFiles;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string startingItem = "Starting item {0}: {1}.";
        private static string cancelledItem = "Cancellation requested for {0} analysis {1}. Finished item {2}: {3}.";

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisCoordinator"/> class.
        /// </summary>
        /// <param name="sourcePreparer">
        /// The source Preparer. The prepared files can be stored anywhere, they just need to be readable.
        /// </param>
        public AnalysisCoordinator(ISourcePreparer sourcePreparer, bool saveIntermediateWavFiles, bool saveImageFiles, bool saveIntermediateCsvFiles)
        {
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
        public IEnumerable<AnalysisResult2> Run(FileInfo file, IAnalyser2 analysis, AnalysisSettings settings)
        {
            return Run(new List<FileSegment>() {new FileSegment() {OriginalFile = file}}, analysis, settings);
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
        public IEnumerable<AnalysisResult2> Run(FileSegment fileSegment, IAnalyser2 analysis, AnalysisSettings settings)
        {
            return Run(new List<FileSegment>() {fileSegment}, analysis, settings);
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
        public IEnumerable<AnalysisResult2> Run(IEnumerable<FileSegment> fileSegments, IAnalyser2 analysis,
            AnalysisSettings settings)
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

            IEnumerable<AnalysisResult2> results;

            Log.DebugFormat("Analysis started in {0}.", this.IsParallel ? "parallel" : "sequence");

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            // Analyse the sub-segments in parallel or sequentially (IsParallel property), 
            // Create and delete directories and/or files as indicated by properties
            // DeleteFinished and SubFoldersUnique
            if (this.IsParallel)
            {
                results = RunParallel(analysisSegments, analysis, settings);
            }
            else // sequential
            {
                results = RunSequential(analysisSegments, analysis, settings);
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
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <returns>
        /// The analysis results.
        /// </returns>
        private IEnumerable<AnalysisResult2> RunParallel(IEnumerable<FileSegment> analysisSegments, IAnalyser2 analysis,
            AnalysisSettings settings)
        {
            var analysisSegmentsCount = analysisSegments.Count();
            var results = new AnalysisResult2[analysisSegmentsCount];

            Parallel.ForEach(
                analysisSegments,
                new ParallelOptions() {MaxDegreeOfParallelism = 64},
                (item, state, index) =>
                {
                    var item1 = item;
                    var index1 = index;

                    // can't use settings as each iteration modifies settings. This causes hard to track down bugs
                    // instead create a copy of the settings, and use that
                    var settingsForThisItem = (AnalysisSettings) settings.Clone();

                    // finished items
                    var finishedItems = results.Count(i => i != null);

                    // process item
                    var result = ProcessItem(item1, analysis, settingsForThisItem);
                    if (result != null)
                    {
                        results[index1] = result;
                    }

                    // check for cancellation
                    //if (this.CancellationPending)
                    //{
                    //    Log.InfoFormat(cancelledItem, "parallel", analysis.Identifier, settingsForThisItem.InstanceId, item1);
                    //    state.Break();
                    //}
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
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <returns>
        /// The analysis results.
        /// </returns>
        private IEnumerable<AnalysisResult2> RunSequential(IEnumerable<FileSegment> analysisSegments,
            IAnalyser2 analysis, AnalysisSettings settings)
        {
            var results = new List<AnalysisResult2>();
            var analysisSegmentsList = analysisSegments.ToList();
            var totalItems = analysisSegmentsList.Count;

            for (var index = 0; index < analysisSegmentsList.Count; index++)
            {
                var item = analysisSegmentsList[index];

                // process item
                // this can use settings, as it is modified each iteration, but this is run synchronously.
                var result = ProcessItem(item, analysis, settings);
                if (result != null)
                {
                    results.Add(result);
                }

                // check for cancellation
                //if (this.CancellationPending)
                //{
                //    Log.WarnFormat(cancelledItem, "sequential", analysis.Identifier, settings.InstanceId, item);
                //    break;
                //}
            }

            return results;
        }

        /// <summary>
        /// Prepare the resources for an analysis, and the run the analysis.
        /// </summary>
        /// <param name="fileSegment">
        /// The file Segment.
        /// </param>
        /// <param name="analyser">
        /// The analysis.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <returns>
        /// The results from the analysis.
        /// </returns>
        private AnalysisResult2 PrepareFileAndRunAnalysis(FileSegment fileSegment, IAnalyser2 analyser,
            AnalysisSettings settings)
        {
            Contract.Requires(settings != null, "Settings must not be null.");
            Contract.Requires(fileSegment != null, "File Segments must not be null.");
            Contract.Requires(fileSegment.Validate(), "File Segment must be valid.");


            var start = fileSegment.SegmentStartOffset.HasValue ? fileSegment.SegmentStartOffset.Value : TimeSpan.Zero;
            var end = fileSegment.SegmentEndOffset.HasValue
                ? fileSegment.SegmentEndOffset.Value
                : fileSegment.OriginalFileDuration;

            // set directories
            this.PrepareDirectories(analyser, settings);

            var tempDir = settings.AnalysisInstanceTempDirectoryChecked;

            // create the file for the analysis
            // save created audio file to settings.AnalysisInstanceTempDirectory if given, otherwise settings.AnalysisInstanceOutputDirectory
            var preparedFile = this.SourcePreparer.PrepareFile(
                GetInstanceDirTempElseOutput(settings),
                fileSegment.OriginalFile,
                settings.SegmentMediaType,
                start,
                end,
                settings.SegmentTargetSampleRate,
                tempDir);

            var preparedFilePath = preparedFile.OriginalFile;
            var preparedFileDuration = preparedFile.OriginalFileDuration;

            // Store sample rate of original audio file in the Settings object.
            // May need original SR during the analysis, esp if have upsampled from the original SR.
            settings.SampleRateOfOriginalAudioFile = preparedFile.OriginalFileSampleRate;

            settings.AudioFile = preparedFilePath;
            settings.SegmentStartOffset = start;

            string fileName = Path.GetFileNameWithoutExtension(preparedFile.OriginalFile.Name);

            //if user requests, save the sonogram files 
            if (saveImageFiles)
            {
                // save spectrogram to output dir - saving to temp dir means possibility of being overwritten
                settings.ImageFile =
                    new FileInfo(Path.Combine(settings.AnalysisInstanceOutputDirectory.FullName, fileName + ".png"));
            }
            

            //if user requests, save the intermediate csv files 
            if (saveIntermediateCsvFiles)
            {
                // always save csv to output dir
                settings.EventsFile =
                    new FileInfo(Path.Combine(settings.AnalysisInstanceOutputDirectory.FullName,
                        fileName + ".Events.csv"));
                settings.SummaryIndicesFile =
                    new FileInfo(Path.Combine(settings.AnalysisInstanceOutputDirectory.FullName,
                        fileName + ".Indices.csv"));
                settings.SpectrumIndicesDirectory = new DirectoryInfo(settings.AnalysisInstanceOutputDirectory.FullName);
            }
            

            Log.DebugFormat("Item {0} started analysing file {1}.", settings.InstanceId, settings.AudioFile.Name);
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            //##### RUN the ANALYSIS ################################################################
            AnalysisResult2 result = analyser.Analyse(settings);
            //#######################################################################################

            stopwatch.Stop();
            Log.DebugFormat("Item {0} finished analysing {1}, took {2}.", settings.InstanceId, settings.AudioFile.Name,
                stopwatch.Elapsed);

            // add information to the results
            result.AnalysisIdentifier = analyser.Identifier;

            // validate results (debug only)
            ValidateResult(settings, result, start, preparedFileDuration);

            // clean up
            if (this.DeleteFinished && this.SubFoldersUnique)
            {
                // delete the directory created for this run
                DeleteDirectory(settings.InstanceId, settings.AnalysisInstanceOutputDirectory);
            }
            else if (this.DeleteFinished && !this.SubFoldersUnique)
            {
                // delete the prepared audio file segment. Don't delete the directory - all instances use the same directory!
                if (saveIntermediateWavFiles)
                {
                    Log.DebugFormat("File {0} not deleted because saveIntermediateWavFiles was set to true",
                        settings.AudioFile.FullName);
                }
                else
                {
                    try
                    {
                        File.Delete(settings.AudioFile.FullName);
                        Log.DebugFormat("Item {0} deleted file {1}.", settings.InstanceId, settings.AudioFile.FullName);
                    }
                    catch (Exception ex)
                    {
                        // this error is not fatal, but it does mean we'll be leaving an audio file behind.
                        Log.Warn(string.Format("Item {0} could not delete audio file {1}.",
                            settings.InstanceId, settings.AudioFile.FullName), ex);
                    }
                }
            }

            return result;
        }

        [Conditional("DEBUG")]
        private static void ValidateResult(AnalysisSettings settings, AnalysisResult2 result, TimeSpan start,
            TimeSpan preparedFileDuration)
        {
            Debug.Assert(result.SettingsUsed != null);
            Debug.Assert(result.SegmentStartOffset == start);
            Debug.Assert(result.AudioDuration == preparedFileDuration);
            Debug.Assert(settings.ImageFile != null && settings.ImageFile.Exists);
            if (result.Events != null)
            {
                Debug.Assert(settings.EventsFile != null && result.EventsFile.Exists);
            }
            if (result.SummaryIndices != null)
            {
                Debug.Assert(settings.SummaryIndicesFile != null && result.SummaryIndicesFile.Exists);
            }
            if (result.SpectralIndices != null)
            {
                foreach (var spectraIndicesFile in result.SpectraIndicesFiles)
                {
                    Debug.Assert(spectraIndicesFile.Exists);
                }
            }
        }

        private void PrepareDirectories(IAnalyser2 analysis, AnalysisSettings settings)
        {
            Contract.Requires(analysis != null, "analysis must not be null.");
            Contract.Requires(settings != null, "settings must not be null.");
            Contract.Requires(settings.AnalysisBaseOutputDirectory != null, "AnalysisBaseOutputDirectory is not set.");
            Contract.Ensures(settings.AnalysisInstanceOutputDirectory != null,
                "AnalysisInstanceOutputDirectory was not set.");
            Contract.Ensures(Directory.Exists(settings.AnalysisInstanceOutputDirectory.FullName),
                "AnalysisInstanceOutputDirectory did not exist.");

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
            var analyserType = typeof (IAnalyser2);

            var analysers = assembly.GetTypes()
                .Where(analyserType.IsAssignableFrom)
                .Select(t => Activator.CreateInstance(t) as IAnalyser2);

            return analysers;
        }

        private AnalysisResult2 ProcessItem(FileSegment item, IAnalyser2 analysis, AnalysisSettings settings)
        {
            Log.DebugFormat(startingItem, settings.InstanceId, item);

            AnalysisResult2 result = null;

            //try
            //{
            result = this.PrepareFileAndRunAnalysis(item, analysis, settings);

            var progressString = string.Format("Successfully analysed {0} using {1}.", item, analysis.Identifier);
            //}
            //catch (Exception ex)
            //{
            //// try to get all the results up to the exception
            //DataTable datatable = ResultsTools.MergeResultsIntoSingleDataTable(results);
            //var op1 = ResultsTools.GetEventsAndIndicesDataTables(datatable, analyser, TimeSpan.Zero);
            //var eventsDatatable = op1.Item1;
            //var indicesDatatable = op1.Item2;
            //var opdir = results.ElementAt(0).SettingsUsed.AnalysisRunDirectory;
            //string fName = Path.GetFileNameWithoutExtension(audioFile.Name) + "_" + analyser.Identifier;
            //var op2 = ResultsTools.SaveEventsAndIndicesDataTables(eventsDatatable, indicesDatatable, fName, opdir.FullName);

            //Log.Error(string.Format("Item {0}: Error processing {1}. Error: {2}.", settings.InstanceId, item, ex.Message), ex);
            //throw;
            //}

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