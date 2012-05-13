namespace AnalysisBase
{
    using System;
    using System.IO;

    /// <summary>
    /// The analysis settings for processing one audio file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The only files and folders an analysis may access are the audio file, 
    /// configuration file and any file or folder in the working directory.
    /// The working directory will be deleted after the analysis is complete.
    /// </para>
    /// </remarks>
    public class AnalysisSettings
    {
        /// <summary>
        /// Gets or sets the WorkingDirectory, which contains all files and directories required for the processing.
        /// </summary>
        public DirectoryInfo WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets the audio file for the analysis.
        /// </summary>
        public FileInfo AudioFile { get; set; }

        /// <summary>
        /// Gets or sets the configuration file.
        /// </summary>
        public FileInfo ConfigFile { get; set; }

        /// <summary>
        /// Gets or sets ConfigString.
        /// </summary>
        public string ConfigString { get; set; }
    }
}
