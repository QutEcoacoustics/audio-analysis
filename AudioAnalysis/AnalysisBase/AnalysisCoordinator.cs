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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisCoordinator"/> class.
        /// </summary>
        /// <param name="sourcePreparer">
        /// The source Preparer. The prepared files can be stored anywhere, they just need to be readable.
        /// </param>
        public AnalysisCoordinator(
            ISourcePreparer sourcePreparer)
        {
            Contract.Requires(sourcePreparer != null);

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
        /// Analyse one of more file segments using the same analysis and settings.
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
        /// The results from multiple analyses.
        /// </returns>
        public IEnumerable<AnalysisResult> Run(IEnumerable<FileSegment> fileSegments, IAnalyser analysis, AnalysisSettings settings)
        {
            Contract.Requires(settings != null, "Settings must not be null.");
            Contract.Requires(analysis != null, "Analysis must not be null.");
            Contract.Requires(fileSegments != null, "File Segments must not be null.");
            Contract.Requires(fileSegments.All(s => s.Validate()), "File Segment must be valid.");

            // calculate the sub-segments of the given file segments that match what the analysis expects.
            var analysisSegments = this.SourcePreparer.CalculateSegments(fileSegments, settings).ToList();
            // check last segment and remove if too short
            var finalSegment = analysisSegments[analysisSegments.Count()-1];
            var duration = finalSegment.SegmentEndOffset - finalSegment.SegmentStartOffset;
            if (duration < settings.SegmentMinDuration) analysisSegments.Remove(finalSegment);
            var analysisSegmentsCount = analysisSegments.Count();

            // Analyse the sub-segments in parallel or sequentially (IsParallel property), 
            // Create and delete directories and/or files as indicated by properties
            // DeleteFinished and SubFoldersUnique
            if (this.IsParallel)
            {
                var results = new AnalysisResult[analysisSegmentsCount];

                Parallel.ForEach(
                    analysisSegments,
                    new ParallelOptions() { MaxDegreeOfParallelism = 64 },
                    (item, state, index) =>
                    {
                        Log.Debug("Start Parallel: Current Item: offset start: "+item.SegmentStartOffset+" offsetend: "+item.SegmentEndOffset+" file: "+item.OriginalFile);
                        // can't use settings as each iteration modifies settings. This causes hard to track down bugs
                        // instead create a copy of the settings, and use that
                        var settingsForThisItem = settings.ShallowClone();
                        
                        var result = this.PrepareFileAndRunAnalysis(item, analysis, settingsForThisItem);
                        results[index] = result;
                    });

                return results;
            }
            else // sequential
            {
                var results = new List<AnalysisResult>();
                int count = 0;
                foreach (var item in analysisSegments)
                {
                    // this can use settings, as it is modified each iteration, but this is run synchronously.
                    var result = this.PrepareFileAndRunAnalysis(item, analysis, settings);
                    results.Add(result);
                    LoggedConsole.Write(".");
                    if (count % 100 == 0) LoggedConsole.Write("#");
                    else
                    if (count % 10  == 0) LoggedConsole.Write(":");
                    else LoggedConsole.Write(".");
                    count++;
                }

                return results;
            }
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
        private AnalysisResult PrepareFileAndRunAnalysis(FileSegment fileSegment, IAnalyser analyser, AnalysisSettings settings)
        {
            Contract.Requires(settings != null,       "Settings must not be null.");
            Contract.Requires(fileSegment != null,    "File Segments must not be null.");
            Contract.Requires(fileSegment.Validate(), "File Segment must be valid.");

            Log.Debug("Starting Running Analyse for " + analyser.Identifier + " instanceId: " + settings.MyInstanceId);

            var start = fileSegment.SegmentStartOffset.HasValue ? fileSegment.SegmentStartOffset.Value : TimeSpan.Zero;
            var end   = fileSegment.SegmentEndOffset.HasValue   ? fileSegment.SegmentEndOffset.Value   : fileSegment.OriginalFileDuration;

            // create directory for analysis run
            settings.AnalysisRunDirectory = this.PrepareWorkingDirectory(analyser, settings);

            // create temp directory 
            settings.AnalysisTempRunDirectory = this.PrepareWorkingDirectory(analyser, settings, false);

             var tempFileDirectory = settings.AnalysisTempRunDirectory;

#if DEBUG
            tempFileDirectory = settings.AnalysisRunDirectory;
#endif

            Log.Warn("Using output directory: " + tempFileDirectory);

            // create the file for the analysis
            var preparedFile = this.SourcePreparer.PrepareFile(
                tempFileDirectory,
                fileSegment.OriginalFile,
                settings.SegmentMediaType,
                start,
                end,
                settings.SegmentTargetSampleRate);

            var preparedFilePath = preparedFile.OriginalFile;
            var preparedFileDuration = preparedFile.OriginalFileDuration;

            // Store sample rate of original audio file in the Settings object.
            // May need original SR during the analysis, esp if have upsampled from the original SR.
            settings.SampleRateOfOriginalAudioFile = preparedFile.OriginalFileSampleRate;
            settings.AudioFile = preparedFilePath;
            
            // Anthony: added so we knew the time of the segment we are working on (09 May 13)
            settings.StartOfSegment = start;

            //if user requests, save the sonogram files 
            if (settings.ConfigDict.ContainsKey("SAVE_SONOGRAM_FILES"))
            {
                string value = settings.ConfigDict["SAVE_SONOGRAM_FILES"].ToString();
                bool saveSonograms = false;
                saveSonograms = Boolean.Parse(value);
                if (saveSonograms)
                {
                    string fName = Path.GetFileNameWithoutExtension(preparedFile.OriginalFile.Name);
                    settings.ImageFile = new FileInfo(Path.Combine(settings.AnalysisRunDirectory.FullName, (fName + ".png")));
                }
            }

            //System.Threading.Thread.Sleep(2000);

            Log.Debug("Running Analyse for " + analyser.Identifier + " path: " + settings.AudioFile.FullName+" instanceId: " + settings.MyInstanceId);

            //##### RUN the ANALYSIS ################################################################
            var result = analyser.Analyse(settings);
            //#######################################################################################

            // add information to the results
            result.AnalysisIdentifier = analyser.Identifier;
            result.SettingsUsed = settings;
            result.SegmentStartOffset = start;
            result.AudioDuration = preparedFileDuration;

            // clean up
            if (this.DeleteFinished && this.SubFoldersUnique)
            {
                // delete the directory created for this run
                try
                {
                    Log.Debug("Attempting to delete directory " + settings.AnalysisRunDirectory.FullName + " instanceId: " + settings.MyInstanceId);
                    Directory.Delete(settings.AnalysisRunDirectory.FullName, true);
                }
                catch (Exception ex)
                {
                    // this error is not fatal, but it does mean we'll be leaving a folder behind.
                    Log.Error("Prepare file delete directory failed. instanceId: " + settings.MyInstanceId, ex);
                }
            }
            else if (this.DeleteFinished && !this.SubFoldersUnique)
            {
                // delete the prepared audio file segment
                    try
                    {
                        Log.Debug("Attempting to delete file " + settings.AudioFile.FullName + " instanceId: " + settings.MyInstanceId);
                        File.Delete(settings.AudioFile.FullName);
                    }
                    catch (Exception ex)
                    {
                        // this error is not fatal, but it does mean we'll be leaving an audio file behind.

                        Log.Error("Prepare file delete file failed. instanceId: " + settings.MyInstanceId, ex);
                    }
            }

            return result;
        }

        /// <summary>
        /// Prepare the working directory.
        /// </summary>
        /// <param name="analysis">
        /// The <paramref name="analysis"/>.
        /// </param>
        /// <param name="settings">
        /// The analysisSettings.
        /// </param>
        /// <returns>
        /// Updated analysisSettings with working directory and configuration file paths.
        /// </returns>
        private DirectoryInfo PrepareWorkingDirectory(IAnalyser analysis, AnalysisSettings settings, bool forResults = true)
        {
            Contract.Requires(analysis != null, "analysis must not be null.");
            Contract.Requires(settings != null, "settings must not be null.");
            Contract.Ensures(Contract.Result<DirectoryInfo>() != null, "Directory was null.");
            Contract.Ensures(Directory.Exists(Contract.Result<DirectoryInfo>().FullName), "Directory did not exist.");

            DirectoryInfo thisAnalysisWorkingDirectory;

            if (forResults)
            {

                thisAnalysisWorkingDirectory = this.SubFoldersUnique
                                                       ? this.CreateUniqueRunDirectory(settings.AnalysisBaseDirectory, analysis.Identifier)
                                                       : this.CreateNamedRunDirectory(settings.AnalysisBaseDirectory, analysis.Identifier);
            }
            else
            {
                thisAnalysisWorkingDirectory = this.SubFoldersUnique
                                                       ? this.CreateUniqueRunDirectory(settings.AnalysisTempBaseDirectory, analysis.Identifier)
                                                       : this.CreateNamedRunDirectory(settings.AnalysisTempBaseDirectory, analysis.Identifier);
            }

            return thisAnalysisWorkingDirectory;
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
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; AnalysisBase.IAnalyser].
        /// </returns>
        public static IEnumerable<IAnalyser> GetAnalysers(Assembly assembly)
        {
            // to find the assembly, get the type of a class in that assembly
            // eg. typeof(MainEntry).Assembly
            var analyserType = typeof(IAnalyser);

            var analysers = assembly.GetTypes()
                .Where(analyserType.IsAssignableFrom)
                .Select(t => Activator.CreateInstance(t) as IAnalyser);

            return analysers;
        }
    }
}
