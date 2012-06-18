namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

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
            // analyse each sub-segment in parallel or not (IsParallel property), 
            // creating and deleting directories and/or files as indicated by properties
            // DeleteFinished and SubFoldersUnique

            var analysisSegments = this.SourcePreparer.CalculateSegments(fileSegments, settings).ToList();
            var analysisSegmentsCount = analysisSegments.Count();

            if (this.IsParallel)
            {
                var results = new AnalysisResult[analysisSegmentsCount];

                Parallel.ForEach(
                    analysisSegments,
                    (item, state, index) =>
                    {
                        var result = this.PrepareFileAndRunAnalysis(item, analysis, settings);
                        results[index] = result;
                    });

                return results;
            }
            else
            {
                var results = new List<AnalysisResult>();
                foreach (var item in analysisSegments)
                {
                    var result = this.PrepareFileAndRunAnalysis(item, analysis, settings);
                    results.Add(result);
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
        /// <param name="analysis">
        /// The analysis.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <returns>
        /// The results from the analysis.
        /// </returns>
        private AnalysisResult PrepareFileAndRunAnalysis(FileSegment fileSegment, IAnalyser analysis, AnalysisSettings settings)
        {
            Contract.Requires(settings != null, "Settings must not be null.");
            Contract.Requires(fileSegment != null, "File Segments must not be null.");
            Contract.Requires(fileSegment.Validate(), "File Segment must be valid.");

            var start = fileSegment.SegmentStartOffset.HasValue ? fileSegment.SegmentStartOffset.Value : TimeSpan.Zero;
            var end = fileSegment.SegmentEndOffset.HasValue ? fileSegment.SegmentEndOffset.Value : fileSegment.OriginalFileDuration;

            // create directory for analysis run
            settings.AnalysisRunDirectory = this.PrepareWorkingDirectory(analysis, settings);

            // create the file for the analysis
            var preparedFile = this.SourcePreparer.PrepareFile(
                settings.AnalysisRunDirectory,
                fileSegment.OriginalFile,
                settings.SegmentMediaType,
                start,
                end,
                settings.SegmentTargetSampleRate);

            var preparedFilePath = preparedFile.OriginalFile;
            var preparedFileDuration = preparedFile.OriginalFileDuration;

            settings.AudioFile = preparedFilePath;

            // run the analysis
            var result = analysis.Analyse(settings);

            // add information to the results
            result.AnalysisIdentifier = analysis.Identifier;
            result.SettingsUsed = settings;
            result.SegmentStartOffset = start;
            result.AudioDuration = preparedFileDuration;

            // clean up
            if (this.DeleteFinished && this.SubFoldersUnique)
            {
                // delete the directory created for this run
                try
                {
                    Directory.Delete(settings.AnalysisRunDirectory.FullName, true);
                }
                catch (Exception ex)
                {
                    // this error is not fatal, but it does mean we'll be leaving a folder behind.
                }
            }
            else if (this.DeleteFinished && !this.SubFoldersUnique)
            {
                // delete the prepared audio file segment
                try
                {
                    File.Delete(settings.AudioFile.FullName);
                }
                catch (Exception ex)
                {
                    // this error is not fatal, but it does mean we'll be leaving an audio file behind.
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
        private DirectoryInfo PrepareWorkingDirectory(IAnalyser analysis, AnalysisSettings settings)
        {
            Contract.Requires(analysis != null, "analysis must not be null.");
            Contract.Requires(settings != null, "settings must not be null.");
            Contract.Ensures(Contract.Result<DirectoryInfo>() != null, "Directory was null.");
            Contract.Ensures(Directory.Exists(Contract.Result<DirectoryInfo>().FullName), "Directory did not exist.");

            var thisAnalysisWorkingDirectory = this.SubFoldersUnique
                                                   ? this.CreateUniqueRunDirectory(settings.AnalysisBaseDirectory, analysis.Identifier)
                                                   : this.CreateNamedRunDirectory(settings.AnalysisBaseDirectory, analysis.Identifier);

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
    }
}
