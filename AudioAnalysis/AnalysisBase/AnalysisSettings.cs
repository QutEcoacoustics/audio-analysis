namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.IO;

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

        static int instanceCounter = 0;
        int myInstanceId;

        public AnalysisSettings()
        {
            instanceCounter++;
            myInstanceId = instanceCounter;
        }

        public int MyInstanceId { get { return myInstanceId; } }

        /// <summary>
        /// Gets or sets the directory that is the base of the folder structure that analyses can use.
        /// Anything put here will be deleted when the analysis is complete.
        /// Analysis implementations must not set this.
        /// </summary>
        public DirectoryInfo AnalysisTempBaseDirectory { get; set; }

        /// <summary>
        /// Gets or sets the directory for a single analysis run.
        /// Anything put here will be deleted when the analysis is complete.
        /// Analysis implementations must not set this.
        /// </summary>
        public DirectoryInfo AnalysisTempRunDirectory { get; set; }

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
        /// Gets or sets the original source file from which audio segments are extracted for analysis.
        /// </summary>
        public FileInfo SourceFile { get; set; }

        /// <summary>
        /// Gets or sets the audio file for the analysis.
        /// Analysis implementations must not set this.
        /// </summary>
        public FileInfo AudioFile { get; set; }

        /// <summary>
        /// Gets or sets the events file for the analysis.
        /// </summary>
        public FileInfo EventsFile { get; set; }

        /// <summary>
        /// Gets or sets the indices file for the analysis.
        /// </summary>
        public FileInfo IndicesFile { get; set; }

        /// <summary>
        /// Gets or sets the audio file for the analysis.
        /// Analysis implementations must not set this.
        /// </summary>
        public FileInfo ImageFile { get; set; }

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
        /// Gets or sets the sample rate of the original audio file from which segment was extracted.
        /// THIS IS A HACK!!! IT IS A WAY OF STORING INFORMATION THAT WE WANT 
        /// TO PASS DOWN INTO THE ANALYSIS LEVEL
        /// </summary>
        public int? SampleRateOfOriginalAudioFile { get; set; }

        /// <summary>
        /// Gets or sets the media type the analysis expects.
        /// This should be set to an initial value by an analysis.
        /// </summary>
        public string SegmentMediaType { get; set; }

        /// <summary>
        /// Gets or sets the configuration file to use to run the analysis.
        /// </summary>
        public FileInfo ConfigFile { get; set; }

        /// <summary>
        /// Gets or sets the config dictionary.
        /// </summary>
        public Dictionary<string, string> ConfigDict { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fiConfig"></param>
        /// <param name="dict"></param>
        /// <param name="diOutputDir"></param>
        /// <param name="key_SEGMENT_DURATION"></param>
        /// <param name="key_SEGMENT_OVERLAP"></param>
        public void SetUserConfiguration(DirectoryInfo tempFileDir, FileInfo fiConfig, Dictionary<string, string> dict, DirectoryInfo diOutputDir, string key_SEGMENT_DURATION, string key_SEGMENT_OVERLAP)
        {
            this.ConfigFile = fiConfig;
            this.ConfigDict = dict;
            this.AnalysisBaseDirectory = diOutputDir;
            if (tempFileDir == null)
            {
                this.AnalysisTempBaseDirectory = new DirectoryInfo(Path.GetTempPath());
            }
            else
            {
                this.AnalysisTempBaseDirectory = tempFileDir;
            }

            //#SEGMENT_DURATION=minutes, SEGMENT_OVERLAP=seconds   FOR EXAMPLE: SEGMENT_DURATION=5  and SEGMENT_OVERLAP=10

            //set the segment offset i.e. time between consecutive segment starts - the key used for this in config file = "SEGMENT_DURATION"
            if (this.ConfigDict.ContainsKey(key_SEGMENT_DURATION))
            {
                string value = dict.TryGetValue(key_SEGMENT_DURATION, out value) ? value : null;
                int segmentOffsetMinutes;
                if (int.TryParse(value, out segmentOffsetMinutes))
                    this.SegmentMaxDuration = TimeSpan.FromMinutes(segmentOffsetMinutes);
                else
                {
                    this.SegmentMaxDuration = null;
                    Console.WriteLine("############### WARNING #############");
                    Console.WriteLine("ERROR READING USER CONFIGURATION FILE");
                    Console.WriteLine("\tINVALID KVP: key={0}, value={1}", key_SEGMENT_DURATION, value);
                }
            }

            // set overlap
            if (this.ConfigDict.ContainsKey(key_SEGMENT_OVERLAP))
            {
                string value = dict.TryGetValue(key_SEGMENT_OVERLAP, out value) ? value : null;
                int segmentOverlapSeconds;
                if (int.TryParse(value, out segmentOverlapSeconds))
                    this.SegmentOverlapDuration = TimeSpan.FromSeconds(segmentOverlapSeconds);
                else
                    this.SegmentOverlapDuration = TimeSpan.Zero;
            }
        } //SetUserConfiguration()

        /// <summary>
        /// Creates a clone of this AnalysisSettings object.
        /// 
        /// </summary>
        /// <returns>A clone of this AnalysisSettings object.</returns>
        /// <remarks>
        /// see: http://msdn.microsoft.com/en-us/library/system.icloneable.aspx
        /// see: http://msdn.microsoft.com/en-us/library/system.object.memberwiseclone.aspx
        /// </remarks>
        public AnalysisSettings ShallowClone()
        {
            // all these must copy the VALUE not the REFERENCE!!!!
            // TODO: If lots more properties are added, this needs to be changed.
            var newSettings = new AnalysisSettings();

            newSettings.AnalysisBaseDirectory = this.AnalysisBaseDirectory != null ? new DirectoryInfo(this.AnalysisBaseDirectory.FullName) : null;
            newSettings.AnalysisRunDirectory = this.AnalysisRunDirectory != null ? new DirectoryInfo(this.AnalysisRunDirectory.FullName) : null;

            newSettings.AnalysisTempBaseDirectory = this.AnalysisTempBaseDirectory != null ? new DirectoryInfo(this.AnalysisTempBaseDirectory.FullName) : null;
            newSettings.AnalysisTempRunDirectory = this.AnalysisTempRunDirectory != null ? new DirectoryInfo(this.AnalysisTempRunDirectory.FullName) : null;

            newSettings.SourceFile = this.SourceFile != null ? new FileInfo(this.SourceFile.FullName) : null;
            newSettings.AudioFile = this.AudioFile != null ? new FileInfo(this.AudioFile.FullName) : null;
            newSettings.EventsFile = this.EventsFile != null ? new FileInfo(this.EventsFile.FullName) : null;
            newSettings.IndicesFile = this.IndicesFile != null ? new FileInfo(this.IndicesFile.FullName) : null;
            newSettings.ImageFile = this.ImageFile != null ? new FileInfo(this.ImageFile.FullName) : null;

            newSettings.SegmentOverlapDuration = TimeSpan.FromTicks(this.SegmentOverlapDuration.Ticks);
            newSettings.SegmentMinDuration = SegmentMinDuration.HasValue ? TimeSpan.FromTicks(this.SegmentMinDuration.Value.Ticks) : new TimeSpan?();
            newSettings.SegmentMaxDuration = SegmentMaxDuration.HasValue ? TimeSpan.FromTicks(this.SegmentMaxDuration.Value.Ticks) : new TimeSpan?();

            newSettings.StartOfSegment = StartOfSegment.HasValue ? TimeSpan.FromTicks(this.StartOfSegment.Value.Ticks) : new TimeSpan?();

            newSettings.SegmentTargetSampleRate = this.SegmentTargetSampleRate;
            newSettings.SampleRateOfOriginalAudioFile = this.SampleRateOfOriginalAudioFile.HasValue ? this.SampleRateOfOriginalAudioFile.Value : new int?();
            newSettings.SegmentMediaType = this.SegmentMediaType;

            newSettings.ConfigFile = new FileInfo(this.ConfigFile.FullName);
            newSettings.ConfigDict = new Dictionary<string, string>(this.ConfigDict);

            return newSettings;
        }

        public TimeSpan? StartOfSegment { get; set; }
    }
}
