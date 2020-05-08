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
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using log4net;
    using ObjectCloner.Extensions;

    /// <summary>
    /// The analysis settings for processing one audio file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All members in this class are prefixed either with. <code>Segment</code> or. <code>Analysis</code>.
    /// Members prefixed with Segment change per segment.
    /// Members prefixed with Analysis are invariant for the Analysis.
    /// </para>
    /// This class MUST be deeply serializable as it crosses serialization boundaries.
    /// <para>
    /// </para>
    /// <para>
    /// The only files and folders an analysis may access are the audio file,
    /// configuration file and any file or folder in the working directory.
    /// The working directory may be deleted after the analysis is complete.
    /// </para>
    /// </remarks>
    public class AnalysisSettings : ICloneable
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(typeof(AnalysisSettings));

        [NonSerialized]
        private static int instanceCounter = 0;

        // TODO CORE: IOC this so readonly avoidance hack not needed
        // needs to be read only for modifications in AnalysisCoordinatorTests
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
#pragma warning disable IDE0044 // Add readonly modifier
        private string fallbackTempDirectory;
#pragma warning restore IDE0044 // Add readonly modifier

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
            this.AnalysisMixDownToMono = true;
            this.AnalysisTargetSampleRate = AppConfigHelper.DefaultTargetSampleRate;
            this.AnalysisMaxSegmentDuration = TimeSpan.FromMinutes(1);
            this.AnalysisMinSegmentDuration = TimeSpan.FromSeconds(20);
            this.fallbackTempDirectory = TempFileHelper.TempDir(ensureNew: true).FullName;
            this.SegmentOverlapDuration = TimeSpan.Zero;
            this.SegmentMediaType = MediaTypes.MediaTypeWav;
            this.AnalysisDataSaveBehavior = false;
            this.AnalysisImageSaveBehavior = SaveBehavior.Never;
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
        /// The contents of this directory may be deleted after the analysis is finished.
        /// If this value is null, AnalysisCoordinator will fallback to AnalysisTempDirectoryFallback.
        /// Analysis implementations must not set this.
        /// </summary>
        public DirectoryInfo AnalysisTempDirectory { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="AnalysisTempDirectory"/> is not null and exists.
        /// </summary>
        public bool IsAnalysisTempDirectoryValid => this.AnalysisTempDirectory?.TryCreate() == true;

        /// <summary>
        /// Gets a base temp directory. The directory will exist and it will be unique.
        /// Anything put here will be deleted when the analysis is complete.
        /// </summary>
        public DirectoryInfo AnalysisTempDirectoryFallback
        {
            get
            {
                if (!Directory.Exists(this.fallbackTempDirectory))
                {
                    Directory.CreateDirectory(this.fallbackTempDirectory);
                }

                return this.fallbackTempDirectory.ToDirectoryInfo();
            }
        }

        /// <summary>
        /// Gets or sets the ChannelSelection array - a list of channels to extract from the audio.
        /// </summary>
        public int[] AnalysisChannelSelection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to mix all selected channels down into one mono channel.
        /// </summary>
        public bool AnalysisMixDownToMono { get; set; }

        /// <summary>
        /// Gets or sets the output directory that is the base of the folder structure that analyses can use.
        /// Analysis implementations must not set this.
        /// </summary>
        public DirectoryInfo AnalysisOutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the AnalysisImageSaveBehavior for images
        /// The save behavior defines whether intermediate results generated by the segment should be saved or not.
        /// </summary>
        public SaveBehavior AnalysisImageSaveBehavior { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether intermediate data files should be saved.
        /// The save behavior defines whether intermediate results generated by the segment should be saved or not.
        /// </summary>
        public bool AnalysisDataSaveBehavior { get; set; }

        /// <summary>
        /// Gets or sets the minimum audio file duration the analysis can process.
        /// This is the min duration without including overlap. Overlap will be added.
        /// This should be set to an initial value by an analysis.
        /// This value is used in <see cref="AnalysisCoordinator.PrepareAnalysisSegments{TSource}"/>
        /// TODO: a chunk of audio without the overlap is a 'segment step'.
        /// </summary>
        public TimeSpan AnalysisMinSegmentDuration { get; set; }

        /// <summary>
        /// Gets or sets the maximum audio file duration the analysis can process.
        /// This is the max duration without including overlap. Overlap will be added. This means that a segment may be larger than this value.
        /// This should be set to an initial value by an analysis.
        /// </summary>
        public TimeSpan? AnalysisMaxSegmentDuration { get; set; }

        /// <summary>
        /// Gets or sets the duration for segments to overlap.
        /// This should be set to an initial value by an analysis.
        /// </summary>
        public TimeSpan SegmentOverlapDuration { get; set; }

        /// <summary>
        /// Gets or sets the media type the analysis expects.
        /// This should be set to an initial value by an analysis.
        /// </summary>
        public string SegmentMediaType { get; set; }

        /// <summary>
        /// Gets or sets the audio sample rate the analysis expects (in hertz).
        /// This is initially set to the value of the <c>DefaultTargetSampleRateKey</c> setting in the AP.Settings.json.
        /// This used to be set by a constant in each implementation of an analysis.
        /// A null value indicates that no sample rate modification will be done.
        /// </summary>
        public int? AnalysisTargetSampleRate { get; set; }

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
        public Config Configuration { get; set; }

        /// <summary>
        /// Gets or sets an object that can be used to store arbitrary configuration or options.
        /// This is useful for passing information between BeforeAnalyze and Analyze.
        /// DO NOT STORE MUTABLE STATE IN THIS OBJECT.
        /// The object provided must be serializable!.
        /// </summary>
        public object AnalysisAnalyzerSpecificConfiguration { get; set; }

        /// <remarks>
        /// Does a deep clone.
        /// </remarks>
        public object Clone()
        {
            AnalysisSettings deepClone = this.DeepClone<AnalysisSettings>();
            Log.Trace("Instance Id of old: {0}, vs new {1}".Format2(this.InstanceId, deepClone.InstanceId));
            return deepClone;
        }

        public override string ToString()
        {
            return $"{nameof(AnalysisSettings)} " +
                   $"with instance id {this.InstanceId} and config file {this.ConfigFile?.Name ?? "<no config>"}.";
        }
    }
}