namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Prepares, runs and completes analyses.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The process to analyse files can be a little complex. The overall idea is 
    /// to begin with an analysis type and a list of file paths and segments inside those files.
    /// Then those files are segmented using default settings from the analysis and possible modifications to the defaults by a user.
    /// Each segment is analysed, and the results 
    /// </para>
    /// </remarks>
    public class AnalysisCoordinator
    {
        private readonly DirectoryInfo analysisRunsDirectory;

        private readonly ISourcePreparer sourcePreparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisCoordinator"/> class.
        /// </summary>
        /// <param name="analysisRunsDirectory">
        /// The analysis runs directory.
        /// </param>
        /// <param name="sourcePreparer">
        /// The source Preparer. The prepared files can be stored anywhere, they just need to be readable.
        /// </param>
        public AnalysisCoordinator(
            DirectoryInfo analysisRunsDirectory,
            ISourcePreparer sourcePreparer)
        {
            this.analysisRunsDirectory = analysisRunsDirectory;
            this.sourcePreparer = sourcePreparer;
        }

        /// <summary>
        /// Analyse multiple files using the same settings.
        /// </summary>
        /// <param name="fileSegments">
        /// The file Segments.
        /// </param>
        /// <param name="preparerSettings">
        /// The preparer Settings.
        /// </param>
        /// <param name="analysis">
        /// The analysis.
        /// </param>
        /// <param name="analysisSettings">
        /// The analysis Settings.
        /// </param>
        /// <returns>
        /// The results from multiple analyses.
        /// </returns>
        public IEnumerable<AnalysisResult> Run(IEnumerable<FileSegment> fileSegments, PreparerSettings preparerSettings, IAnalysis analysis, AnalysisSettings analysisSettings)
        {
            var preparedFiles = this.PrepareFiles(fileSegments, preparerSettings);

            var results = new List<AnalysisResult>();

            AnalysisSettings currentAnalysisSettings;
            AnalysisResult currentResult;

            foreach (var preparedFile in preparedFiles)
            {
                currentAnalysisSettings = this.PrepareWorkingDirectory(analysis, analysisSettings);
                currentAnalysisSettings.AudioFile = preparedFile;

                currentResult = this.Analyse(analysis, currentAnalysisSettings);
                currentResult.AnalysisIdentifier = analysis.Identifier;
                currentResult.AnalysisSettingsUsed = currentAnalysisSettings;
                currentResult.PreparerSettingsUsed = preparerSettings;

                results.Add(currentResult);
            }

            return results;
        }

        /// <summary>
        /// Run an analysis over a single file.
        /// </summary>
        /// <param name="analysis">
        /// The analysis.
        /// </param>
        /// <param name="analysisSettings">
        /// The analysis Settings.
        /// </param>
        /// <returns>
        /// The Results of the analysis.
        /// </returns>
        public AnalysisResult Analyse(IAnalysis analysis, AnalysisSettings analysisSettings)
        {
            var results = analysis.Analyse(analysisSettings);
            return results;
        }

        /// <summary>
        /// Prepare the resources for an analysis.
        /// </summary>
        /// <param name="fileSegments">
        /// The file Segments.
        /// </param>
        /// <param name="preparerSettings">
        /// The preparer Settings.
        /// </param>
        /// <returns>
        /// The paths to prepared files.
        /// </returns>
        public IEnumerable<FileInfo> PrepareFiles(IEnumerable<FileSegment> fileSegments, PreparerSettings preparerSettings)
        {
            Contract.Requires(preparerSettings != null, "PreparerSettings must not be null.");
            Contract.Requires(fileSegments != null, "File Segments must not be null.");
            Contract.Requires(fileSegments.All(s => s.Validate()), "File Segments must all be valid.");
            Contract.Ensures(Contract.Result<IEnumerable<FileInfo>>().All(f => File.Exists(f.FullName)), "One or more files were not segmented.");

            return this.sourcePreparer.PrepareFiles(preparerSettings, fileSegments);
        }

        /// <summary>
        /// Prepare the working directory.
        /// </summary>
        /// <param name="analysis">
        /// The <paramref name="analysis"/>.
        /// </param>
        /// <param name="analysisSettings">
        /// The analysisSettings.
        /// </param>
        /// <returns>
        /// Updated analysisSettings with working directory and configuration file paths.
        /// </returns>
        public AnalysisSettings PrepareWorkingDirectory(IAnalysis analysis, AnalysisSettings analysisSettings)
        {
            Contract.Requires(analysis != null, "analysis must not be null.");
            Contract.Requires(analysisSettings != null, "analysisSettings must not be null.");
            Contract.Ensures(Contract.Result<AnalysisSettings>().WorkingDirectory != null, "Directory was null.");
            Contract.Ensures(Contract.Result<AnalysisSettings>().ConfigFile != null, "Config File was null.");
            Contract.Ensures(File.Exists(Contract.Result<AnalysisSettings>().ConfigFile.FullName), "Config File does not exist.");

            var thisAnalysisWorkingDirectory = this.CreateSingleRunDirectory(analysis.Identifier);

            analysisSettings.WorkingDirectory = thisAnalysisWorkingDirectory;

            var configFile = this.CreateFile(thisAnalysisWorkingDirectory);
            File.WriteAllText(configFile.FullName, analysisSettings.ConfigString);

            analysisSettings.ConfigFile = configFile;

            return analysisSettings;
        }

        /// <summary>
        /// Create a directory for an analysis to be run.
        /// Will be in the form [analysisId][sep][token].
        /// </summary>
        /// <param name="analysisIdentifier">Analysis Identifier.</param>
        /// <returns>The created directory.</returns>
        private DirectoryInfo CreateSingleRunDirectory(string analysisIdentifier)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(analysisIdentifier), "analysisIdentifier must be set.");
            Contract.Ensures(Contract.Result<DirectoryInfo>() != null, "Directory was null.");
            Contract.Ensures(Directory.Exists(Contract.Result<DirectoryInfo>().FullName), "Directory was not created.");

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[10];
                rng.GetBytes(tokenData);
                string token = Convert.ToBase64String(tokenData);

                var runDirectory = Path.Combine(this.analysisRunsDirectory.FullName, analysisIdentifier, token);

                var dir = new DirectoryInfo(runDirectory);
                return dir;
            }
        }

        /// <summary>
        /// Create an empty file with a random file name in given <paramref name="directory"/>.
        /// </summary>
        /// <param name="directory">Directory in which to create fie.</param>
        /// <returns>Created file.</returns>
        private FileInfo CreateFile(DirectoryInfo directory)
        {
            Contract.Requires(directory != null, "Directory not be null.");
            Contract.Requires(Directory.Exists(directory.FullName), "Directory must exist.");
            Contract.Ensures(Contract.Result<FileInfo>() != null, "File was null.");
            Contract.Ensures(File.Exists(Contract.Result<FileInfo>().FullName), "File does not exist.");

            var file = new FileInfo(Path.Combine(directory.FullName, Path.GetRandomFileName()));
            return file;
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
