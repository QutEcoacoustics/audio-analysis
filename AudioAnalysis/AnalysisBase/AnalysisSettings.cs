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
    /// <para>
    /// If the analysis expects particular files or folders to exist apart from 
    /// audio file and configuration file, they must be created during the preparation step.
    /// 
    /// </para>
    /// </remarks>
    public class AnalysisSettings
    {
        /// <summary>
        /// Gets or sets the name to display for the analysis.
        /// </summary>
        public string AnalysisName { get; set; }

        /// <summary>
        /// Gets or sets the duration for segments to overlap when the original audio file is longer than <see cref="SegmentMaxDuration"/>.
        /// </summary>
        public TimeSpan SegmentOverlapDuration { get; set; }

        /// <summary>
        /// Gets or sets the maximum audio file duration the analysis can process.
        /// </summary>
        public TimeSpan SegmentMaxDuration { get; set; }

        /// <summary>
        /// Gets or sets the audio sample rate the analysis expects (in hertz).
        /// </summary>
        public int SegmentTargetSampleRate { get; set; }

        /// <summary>
        /// Gets or sets the WorkingDirectory, which contains all files and directories required for the processing.
        /// </summary>
        public DirectoryInfo WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets the audio file to process.
        /// </summary>
        public FileInfo AudioFile { get; set; }

        /// <summary>
        /// Gets or sets the configuration file.
        /// </summary>
        public FileInfo ConfigFile { get; set; }
    }
}
