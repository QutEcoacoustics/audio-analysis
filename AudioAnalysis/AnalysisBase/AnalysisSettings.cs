// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalysisSettings.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   The analysis settings for processing one audio file.
//   DO NOT CHANGE THIS CLASS UNLESS YOU ARE TOLD TO.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Acoustics.Shared;

    using GeorgeCloney;

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
    [Serializable]
    public class AnalysisSettings : ICloneable
    {

        private static int instanceCounter = 0;

        /// <summary>
        /// Used to track instances of this class through parallelism - must be readonly.
        /// </summary>
        private readonly int instanceId;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisSettings"/> class. 
        /// </summary>
        public AnalysisSettings()
        {
            instanceCounter++;
            this.instanceId = instanceCounter;
            this.ConfigDict = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the instance tracking integer.
        /// </summary>
        public int InstanceId
        {
            get
            {
                return this.instanceId;
            }
        }

        /// <summary>
        /// Gets or sets the temp directory that is the base of the folder structure that analyses can use.
        /// Anything put here will be deleted when the analysis is complete.
        /// Analysis implementations must not set this.
        /// </summary>
        public DirectoryInfo AnalysisBaseTempDirectory { get; set; }

        /// <summary>
        /// Gets a base temp directory. The directory will exist.
        /// </summary>
        public DirectoryInfo AnalysisBaseTempDirectoryChecked
        {
            get
            {
                DirectoryInfo tempDir = null;

                if (this.AnalysisBaseTempDirectory != null && Directory.Exists(this.AnalysisBaseTempDirectory.FullName))
                {
                    tempDir = this.AnalysisBaseTempDirectory;
                }
                else
                {
                    tempDir = TempFileHelper.TempDir();
                }

                if (!Directory.Exists(tempDir.FullName))
                {
                    Directory.CreateDirectory(tempDir.FullName);
                }

                return tempDir;
            }
        }

        /// <summary>
        /// Gets or sets the output directory that is the base of the folder structure that analyses can use.
        /// Analysis implementations must not set this.
        /// </summary>
        public DirectoryInfo AnalysisBaseOutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the temp directory for a single analysis run.
        /// Anything put here will be deleted when the analysis is complete.
        /// Analysis implementations must not set this.
        /// </summary>
        public DirectoryInfo AnalysisInstanceTempDirectory { get; set; }

        /// <summary>
        /// Gets a temp directory for a single run. The directory will exist.
        /// </summary>
        public DirectoryInfo AnalysisInstanceTempDirectoryChecked
        {
            get
            {
                DirectoryInfo tempDir = null;

                if (this.AnalysisInstanceTempDirectory != null && Directory.Exists(this.AnalysisInstanceTempDirectory.FullName))
                {
                    tempDir = this.AnalysisInstanceTempDirectory;
                }
                else
                {
                    tempDir = new DirectoryInfo(Path.Combine(this.AnalysisBaseTempDirectoryChecked.FullName, Path.GetRandomFileName()));
                }

                if (!Directory.Exists(tempDir.FullName))
                {
                    Directory.CreateDirectory(tempDir.FullName);
                }

                return tempDir;
            }
        }

        /// <summary>
        /// Gets or sets the output directory for a single analysis run.
        /// Analysis implementations must not set this.
        /// </summary>
        public DirectoryInfo AnalysisInstanceOutputDirectory { get; set; }

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
        /// Gets or sets the summary indices file for the analysis.
        /// </summary>
        public FileInfo SummaryIndicesFile { get; set; }

        /// <summary>
        /// Gets or sets the spectrum indices directory where spectra should be written for the analysis.
        /// </summary>
        public DirectoryInfo SpectrumIndicesDirectory { get; set; }

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
        /// TODO: a chunk of audio without the overlap is a 'segment step'.
        /// </summary>
        public TimeSpan? SegmentMinDuration { get; set; }

        /// <summary>
        /// Gets or sets the maximum audio file duration the analysis can process.
        /// This is the max duration without including overlap. Overlap will be added. This means that a segment may be larger than this value.
        /// This should be set to an initial value by an analysis.
        /// </summary>
        public TimeSpan? SegmentMaxDuration { get; set; }

        /// <summary>
        /// Gets or sets the start offset of the current analysis segment.
        /// For a large file, analyzed in minute segments, this will store Minute offsets (e.g. min 1, min 2, min 3...).
        /// </summary>
        public TimeSpan? SegmentStartOffset { get; set; }

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
        /// Gets other configuration properties. Should be mutually exclusive with ConfigFile.
        /// </summary>
        public dynamic Configuration { get; set; }

        /*
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

            newSettings.AnalysisBaseOutputDirectory = this.AnalysisBaseOutputDirectory != null ? new DirectoryInfo(this.AnalysisBaseOutputDirectory.FullName) : null;
            newSettings.AnalysisInstanceOutputDirectory = this.AnalysisInstanceOutputDirectory != null ? new DirectoryInfo(this.AnalysisInstanceOutputDirectory.FullName) : null;

            newSettings.AnalysisBaseTempDirectory = this.AnalysisBaseTempDirectory != null ? new DirectoryInfo(this.AnalysisBaseTempDirectory.FullName) : null;
            newSettings.AnalysisInstanceTempDirectory = this.AnalysisInstanceTempDirectory != null ? new DirectoryInfo(this.AnalysisInstanceTempDirectory.FullName) : null;

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
         * */

        public object Clone()
        {
            AnalysisSettings deepClone = this.DeepClone();
            return deepClone;
        }

        public override string ToString()
        {
            return string.Format(
                "Settings for {0} with instance id {1} and config file {2}.",
                this.AudioFile.Name,
                this.InstanceId,
                this.ConfigFile.Name);
        }
    }
}
