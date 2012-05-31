namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public enum AnalysisMode {
        None = 0,
        Efficient = 1, // data table only
        Display = 2, // image only
        Everything = 3 // everything!
    }

    /// <summary>
    /// The analysis settings for processing one audio file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The only files and folders an analysis may access are the audio file, 
    /// configuration file and any file or folder in the working directory.
    /// The working directory may be deleted after the analysis is complete.
    /// </para>
    /// </remarks>
    public class AnalysisSettings
    {
        /// <summary>
        /// Gets RunDirectoryString.
        /// </summary>
        public static string AnalysisBaseDirectoryString
        {
            get
            {
                return "AnalysisSettings.analysisBaseDirectory";
            }
        }

        /// <summary>
        /// Gets BaseRunDirectoryString.
        /// </summary>
        public static string AnalysisRunDirectoryString
        {
            get
            {
                return "AnalysisSettings.analysisRunDirectory";
            }
        }

        /// <summary>
        /// Gets AudioFileString.
        /// </summary>
        public static string AudioFileString
        {
            get
            {
                return "AnalysisSettings.audioFile";
            }
        }

        /// <summary>
        /// Gets SegmentOverlapDurationString.
        /// </summary>
        public static string SegmentOverlapDurationString
        {
            get
            {
                return "AnalysisSettings.SegmentOverlapDuration";
            }
        }

        /// <summary>
        /// Gets SegmentMinDurationString.
        /// </summary>
        public static string SegmentMinDurationString
        {
            get
            {
                return "AnalysisSettings.SegmentMinDuration";
            }
        }

        /// <summary>
        /// Gets SegmentMaxDurationString.
        /// </summary>
        public static string SegmentMaxDurationString
        {
            get
            {
                return "AnalysisSettings.SegmentMaxDuration";
            }
        }

        /// <summary>
        /// Gets SegmentTargetSampleRateString.
        /// </summary>
        public static string SegmentTargetSampleRateString
        {
            get
            {
                return "AnalysisSettings.SegmentTargetSampleRate";
            }
        }

        /// <summary>
        /// Gets SegmentMediaTypeString.
        /// </summary>
        public static string SegmentMediaTypeString
        {
            get
            {
                return "AnalysisSettings.SegmentMediaType";
            }
        }

        /// <summary>
        /// Gets ConfigStringInputString.
        /// </summary>
        public static string ConfigStringInputString
        {
            get
            {
                return "AnalysisSettings.configStringInput";
            }
        }

        /// <summary>
        /// Gets ConfigFileString.
        /// </summary>
        public static string ConfigFileString
        {
            get
            {
                return "AnalysisSettings.configFile";
            }
        }

        /// <summary>
        /// Gets or sets the directory that is the base of the folder structure that analyses can use.
        /// Analysis implementations must not set this.
        /// </summary>
        public DirectoryInfo AnalysisBaseDirectory { get; set; }

        /// <summary>
        /// Gets or sets the directory for a single analysis run.
        /// Analysis implementations must not set this.
        /// </summary>
        public DirectoryInfo AnalysisRunDirectory { get; set; }

        /// <summary>
        /// Gets or sets the audio file for the analysis.
        /// Analysis implementations must not set this.
        /// </summary>
        public FileInfo AudioFile { get; set; }

        /// <summary>
        /// Gets or sets the duration for segments to overlap.
        /// This should be set to an initial value by an analysis.
        /// </summary>
        public TimeSpan SegmentOverlapDuration { get; set; }

        /// <summary>
        /// Gets or sets the minimum audio file duration the analysis can process.
        /// This is the min duration without including overlap. Overlap will be added.
        /// This should be set to an initial value by an analysis.
        /// TODO: a chunk of audio without the overlap is a 'segmentstep'.
        /// </summary>
        public TimeSpan? SegmentMinDuration { get; set; }

        /// <summary>
        /// Gets or sets the maximum audio file duration the analysis can process.
        /// This is the max duration without including overlap. Overlap will be added. This means that a segment may be larger than this value.
        /// This should be set to an initial value by an analysis.
        /// </summary>
        public TimeSpan? SegmentMaxDuration { get; set; }

        /// <summary>
        /// Gets or sets the audio sample rate the analysis expects (in hertz).
        /// This should be set to an initial value by an analysis.
        /// </summary>
        public int SegmentTargetSampleRate { get; set; }

        /// <summary>
        /// Gets or sets the media type the analysis expects.
        /// This should be set to an initial value by an analysis.
        /// </summary>
        public string SegmentMediaType { get; set; }

        /// <summary>
        /// Gets or sets the configuration file to use to run the analysis.
        /// Analysis implementations must not set this. Use ConfigStringInput to set the content of the file instead.
        /// </summary>
        public FileInfo ConfigFile { get; set; }

        /// <summary>
        /// Gets or sets analysis mode (this is just a hint).
        /// </summary>
        public AnalysisMode AnalysisRunMode { get; set; }
    }
}
