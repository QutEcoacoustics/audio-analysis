// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalysisSettings.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
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
    using System.Reflection;
    using System.Runtime.Serialization;

    using Acoustics.Shared;

    using GeorgeCloney;

    using log4net;

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
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [NonSerialized]
        private static int instanceCounter = 0;

        /// <summary>
        /// Used to track instances of this class through parallelism - must be readonly.
        /// </summary>
        [NonSerialized]
        private int? instanceId = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisSettings"/> class.
        /// </summary>
        public AnalysisSettings()
        {
            this.ConfigDict = new Dictionary<string, string>();
            this.SegmentTargetSampleRate = AppConfigHelper.DefaultTargetSampleRate;
            this.SegmentMaxDuration = TimeSpan.FromMinutes(1);
            this.SegmentMinDuration = TimeSpan.FromSeconds(20);
            this.SegmentMediaType = MediaTypes.MediaTypeWav;
            this.SegmentOverlapDuration = TimeSpan.Zero;
        }

        /// <summary>
        /// Gets the instance tracking integer.
        /// </summary>
        public int InstanceId
        {
            get
            {
                if (!this.instanceId.HasValue)
                {
                    // counter increment moved out of constructor because binary serializer does not use constructors
                    instanceCounter++;
                    this.instanceId = instanceCounter;

                }

                return this.instanceId.Value;
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
        /// Gets or sets an output image file - most likely a spectrogram
        /// Analysis implementations must not set this.
        /// </summary>
        public FileInfo ImageFile { get; set; }

        /// <summary>
        /// Gets or sets the SaveBehavior.
        /// The save behavior defines whether intermediate results generated by the segment should be saved or not.
        /// </summary>
        public SaveBehavior SegmentSaveBehavior { get; set; }

        /// <summary>
        /// Gets or sets the duration for segments to overlap.
        /// This should be set to an initial value by an analysis.
        /// </summary>
        public TimeSpan SegmentOverlapDuration { get; set; }

        /// <summary>
        /// Gets or sets the ideal duration of audio for the segment being analyzed.
        /// This number represents what was requested for cutting whereas the actual
        /// duration of audio provided by <c>AudioFile</c> may differ slightly due to
        /// inaccuracies in cutting audio.
        /// </summary>
        public TimeSpan? SegmentDuration { get; set; }

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
        /// This is initially set to the value of the <c>DefaultTargetSampleRateKey</c> setting in the app.config.
        /// This used to be set by a constant in each implementation of an analysis.
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
        [Obsolete]
        public Dictionary<string, string> ConfigDict { get; set; }

        /// <summary>
        /// Gets or sets the object of other configuration properties. Should be mutually exclusive with ConfigFile.
        /// </summary>
        public dynamic Configuration { get; set; }

        /// <summary>
        /// Get or sets an object that can be used to store arbitrary configuration or options.
        /// This is useful for passing information between BeforeAnalyze and Analyze.
        /// DO NOT STORE MUTABLE STATE IN THIS OBJECT.
        /// The object provided must be serializable!
        /// </summary>
        public object AnalyzerSpecificConfiguration { get; set; }

        /// <inheritdoc/>
        public object Clone()
        {
            AnalysisSettings deepClone = this.DeepClone();
            Log.Trace("Instance Id of old: {0}, vs new {1}".Format2(this.InstanceId, deepClone.InstanceId));
            return deepClone;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Settings for {this.AudioFile.Name} with instance id {this.InstanceId} and config file {this.ConfigFile.Name}.";
        }
    }
}
