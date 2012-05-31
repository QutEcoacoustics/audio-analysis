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
        /// Gets or sets a value indicating whether DeleteFinished.
        /// </summary>
        public bool DeleteFinished { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SubFoldersUnique.
        /// </summary>
        public bool SubFoldersUnique { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsParallel.
        /// </summary>
        public bool IsParallel { get; set; }

        /// <summary>
        /// Analyse multiple files using the same settings.
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
        public IEnumerable<AnalysisResult> Run(IEnumerable<FileSegment> fileSegments, IAnalysis analysis, AnalysisSettings settings)
        {
            var analysisSegments = this.SourcePreparer.CalculateSegments(fileSegments, settings).ToList();
            var analysisSegmentsCount = analysisSegments.Count();

            if (this.IsParallel)
            {
                var results = new AnalysisResult[analysisSegmentsCount];

                Parallel.ForEach(
                    analysisSegments,
                    (item, state, index) =>
                    {
                        var sourceFile = this.SourcePreparer.PrepareFile(
                            settings.AnalysisBaseDirectory,
                            item.OriginalFile,
                            settings.SegmentMediaType,
                            item.SegmentStartOffset.Value,
                            item.SegmentEndOffset.Value,
                            settings.SegmentTargetSampleRate);

                        settings.AudioFile = sourceFile;
                        var result = this.Analyse(analysis, settings);
                        results[index] = result;
                    });

                return results;
            }
            else
            {
                var results = new List<AnalysisResult>();
                foreach (var item in analysisSegments)
                {
                    var sourceFile = this.SourcePreparer.PrepareFile(
                            settings.AnalysisBaseDirectory,
                            item.OriginalFile,
                            settings.SegmentMediaType,
                            item.SegmentStartOffset.Value,
                            item.SegmentEndOffset.Value,
                            settings.SegmentTargetSampleRate);

                    settings.AudioFile = sourceFile;
                    var result = this.Analyse(analysis, settings);
                    results.Add(result);
                }

                return results;
            }
        }

        /// <summary>
        /// Run an analysis over a single file.
        /// </summary>
        /// <param name="analysis">
        /// The analysis.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <returns>
        /// The Results of the analysis.
        /// </returns>
        public AnalysisResult Analyse(IAnalysis analysis, AnalysisSettings settings)
        {
            AnalysisSettings currentAnalysisSettings = this.PrepareWorkingDirectory(analysis, settings);

            AnalysisResult currentResult = analysis.Analyse(settings);
            currentResult.AnalysisIdentifier = analysis.Identifier;
            currentResult.SettingsUsed = currentAnalysisSettings;

            if (this.DeleteFinished)
            {
                this.DeleteRunDirectory(currentAnalysisSettings.AnalysisRunDirectory);
            }

            return currentResult;
        }

        /// <summary>
        /// Prepare the resources for an analysis.
        /// </summary>
        /// <param name="fileSegments">
        /// The file Segments.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <returns>
        /// The paths to prepared files.
        /// </returns>
        private IEnumerable<FileInfo> PrepareFiles(IEnumerable<FileSegment> fileSegments, AnalysisSettings settings)
        {
            Contract.Requires(settings != null, "Settings must not be null.");
            Contract.Requires(fileSegments != null, "File Segments must not be null.");
            Contract.Requires(fileSegments.All(s => s.Validate()), "File Segments must all be valid.");
            Contract.Ensures(Contract.Result<IEnumerable<FileInfo>>().All(f => File.Exists(f.FullName)), "One or more files were not segmented.");

            var segmentsForAnalysis = new List<FileSegment>();

            var analysisSegments = this.SourcePreparer.CalculateSegments(fileSegments, settings).ToList();

            foreach (var file in fileSegments)
            {

                //var analysisSegment = this.SourcePreparer.PrepareFile(file.OriginalFile.FullName,);
            }

            //return this.SourcePreparer.PrepareFiles(settings, fileSegments);
            return null;
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
        private AnalysisSettings PrepareWorkingDirectory(IAnalysis analysis, AnalysisSettings settings)
        {
            Contract.Requires(analysis != null, "analysis must not be null.");
            Contract.Requires(settings != null, "settings must not be null.");
            Contract.Ensures(Contract.Result<AnalysisSettings>().AnalysisRunDirectory != null, "Directory was null.");
            Contract.Ensures(Directory.Exists(Contract.Result<AnalysisSettings>().AnalysisRunDirectory.FullName), "Directory did not exist.");
            Contract.Ensures(Contract.Result<AnalysisSettings>().ConfigFile != null, "Config File was null.");
            Contract.Ensures(File.Exists(Contract.Result<AnalysisSettings>().ConfigFile.FullName), "Config File does not exist.");

            var thisAnalysisWorkingDirectory = this.SubFoldersUnique
                                                   ? this.CreateUniqueRunDirectory(settings.AnalysisBaseDirectory, analysis.Identifier)
                                                   : this.CreateNamedRunDirectory(settings.AnalysisBaseDirectory, analysis.Identifier);

            settings.AnalysisRunDirectory = thisAnalysisWorkingDirectory;

            // config file path is already set before AnalysisCoordinator is used.
            /*
            var configFile = new FileInfo(Path.Combine(thisAnalysisWorkingDirectory.FullName, Path.GetRandomFileName() + ".txt"));
            File.WriteAllText(configFile.FullName, settings.ConfigStringInput);

            settings.ConfigFile = configFile;
            */

            return settings;
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
        /// Delete a <paramref name="directory"/>.
        /// </summary>
        /// <param name="directory">Directory to delete.</param>
        private void DeleteRunDirectory(DirectoryInfo directory)
        {
            Contract.Requires(directory != null);
            Contract.Requires(Directory.Exists(directory.FullName));
            Contract.Ensures(!Directory.Exists(directory.FullName));

            Directory.Delete(directory.FullName);
        }


    }
}
