// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerSettings.cs" company="MQUTeR">
//   -
// </copyright>
// <author> Anthony  Truskinger</author>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Drawing;
    using System.Runtime.Serialization;

    /// <summary>
    /// A class of settings used by the player. All should be simple and serializable.
    /// NOTE: @Mark: A DefaultValueAttribute will not cause a member to be automatically initialized with the attribute's value. You must set the initial value in your code.
    /// </summary>
    [Serializable]
    [DataContract]
    public class PlayerSettings
    {
        #region Constants and Fields

        private const bool DefaultAutomaticallySkipToNextTrack = false;

        private const int DefaultDownloadsNumberForwardChunks = 2;

        private const int DefaultDownloadsNumberPreviousChunks = 1;

        private const int DefaultDownloadsRetryCount = 0;

        private const bool DefaultIsMute = false;

        private const double DefaultNavigatorSizeMilliSeconds = 6 * 60 * 1000;

        private const int DefaultProfileSaveTimeMilliSeconds = 40 * 1000;

        private const bool DefaultTurnOnLooping = true;

        private const string DefaultUomFrequencyScale = "Hz";

        private const bool DefaultUomMelScale = false;

        private const string DefaultUomTimeScale = "ms";

        private const bool DefaultUseLocalFiles = false;

        private const bool DefaultUseRemoteFiles = true;

        private const double DefaultVolume = 0.9;

        private static readonly string DefaultColorsLocked = Color.FromArgb(48, 255, 8, 0).ToHexString(true);

        private static readonly string DefaultColorsProcessorTag = Color.FromArgb(48, 0, 62, 255).ToHexString(true);

        private static readonly string DefaultColorsSelected = Color.FromArgb(48, 252, 255, 0).ToHexString(true);

        private static readonly string DefaultColorsTag = Color.FromArgb(48, 81, 255, 0).ToHexString(true);

        private static readonly string DefaultColorsTagWithProcessorResultId =
            Color.FromArgb(48, 0, 255, 135).ToHexString(true);

        private static readonly string DefaultColorsUnsavedTag = Color.FromArgb(48, 255, 255, 255).ToHexString(true);

        private const bool DefaultTagListShowReferenceOnly = false;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerSettings"/> class.
        /// </summary>
        public PlayerSettings()
        {
            this.AutomaticallySkipToNextTrack = DefaultAutomaticallySkipToNextTrack;
            this.ColorsLocked = DefaultColorsLocked;
            this.ColorsProcessorTag = DefaultColorsProcessorTag;
            this.ColorsSelected = DefaultColorsSelected;
            this.ColorsTag = DefaultColorsTag;
            this.ColorsTagWithProcessorResultId = DefaultColorsTagWithProcessorResultId;
            this.ColorsUnsavedTag = DefaultColorsUnsavedTag;
            this.DownloadsNumberForwardChunks = DefaultDownloadsNumberForwardChunks;
            this.DownloadsNumberPreviousChunks = DefaultDownloadsNumberPreviousChunks;
            this.DownloadsRetryCount = DefaultDownloadsRetryCount;
            this.IsMute = DefaultIsMute;
            this.ProfileSaveTimeMilliSeconds = DefaultProfileSaveTimeMilliSeconds;
            this.TurnOnLooping = DefaultTurnOnLooping;
            this.UomFrequencyScale = DefaultUomFrequencyScale;
            this.UomMelScale = DefaultUomMelScale;
            this.UomTimeScale = DefaultUomTimeScale;
            this.UseLocalFiles = DefaultUseLocalFiles;
            this.UseRemoteFiles = DefaultUseRemoteFiles;
            this.Volume = DefaultVolume;
            this.NavigatorSizeMilliSeconds = DefaultNavigatorSizeMilliSeconds;
            this.TagListShowReferenceOnly = DefaultTagListShowReferenceOnly;
        }

        [DataMember]
        public bool TagListShowReferenceOnly { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether AutomaticallySkipToNextTrack.
        /// </summary>
        [DataMember]
        public bool AutomaticallySkipToNextTrack { get; set; }

        /// <summary>
        /// Gets or sets ColorsLocked.
        /// </summary>
        [DataMember]
        [RegularExpression(ImageExtensions.RegExHexColor, ErrorMessage = ImageExtensions.ColorRegExError)]
        public string ColorsLocked { get; set; }

        /// <summary>
        /// Gets or sets ColorsProcessorTag.
        /// </summary>
        [DataMember]
        [RegularExpression(ImageExtensions.RegExHexColor, ErrorMessage = ImageExtensions.ColorRegExError)]
        public string ColorsProcessorTag { get; set; }

        /// <summary>
        /// Gets or sets ColorsSelected.
        /// </summary>
        [DataMember]
        [RegularExpression(ImageExtensions.RegExHexColor, ErrorMessage = ImageExtensions.ColorRegExError)]
        public string ColorsSelected { get; set; }

        /// <summary>
        /// Gets or sets ColorsTag.
        /// </summary>
        [DataMember]
        [RegularExpression(ImageExtensions.RegExHexColor, ErrorMessage = ImageExtensions.ColorRegExError)]
        public string ColorsTag { get; set; }

        /// <summary>
        /// Gets or sets ColorsTagWithProcessorResultId.
        /// </summary>
        [DataMember]
        [RegularExpression(ImageExtensions.RegExHexColor, ErrorMessage = ImageExtensions.ColorRegExError)]
        public string ColorsTagWithProcessorResultId { get; set; }

        /// <summary>
        /// Gets or sets ColorsUnsavedTag.
        /// </summary>
        [DataMember]
        [RegularExpression(ImageExtensions.RegExHexColor, ErrorMessage = ImageExtensions.ColorRegExError)]
        public string ColorsUnsavedTag { get; set; }

        /// <summary>
        /// Gets or sets DownloadsNumberForwardChunks.
        /// </summary>
        [DataMember]
        [Range(0, 10)]
        public int DownloadsNumberForwardChunks { get; set; }

        /// <summary>
        /// Gets or sets DownloadsNumberPreviousChunks.
        /// </summary>
        [DataMember]
        [Range(0, 10)]
        public int DownloadsNumberPreviousChunks { get; set; }

        /// <summary>
        /// Gets or sets DownloadsRetryCount.
        /// </summary>
        [DataMember]
        [Range(0, 100)]
        public int DownloadsRetryCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsMute.
        /// </summary>
        [DataMember]
        public bool IsMute { get; set; }

        /// <summary>
        /// Gets or sets the size of the Navigation Window used in the player (the top slidy bar).
        /// </summary>
        [DataMember]
        [Range(2 * 60 * 1000, 20 * 60 * 1000)]
        public double NavigatorSizeMilliSeconds { get; set; }

        /// <summary>
        /// Gets or sets ProfileSaveTime_ms.
        /// </summary>
        [DataMember]
        public int ProfileSaveTimeMilliSeconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether TurnOnLooping.
        /// </summary>
        [DataMember]
        public bool TurnOnLooping { get; set; }

        /// <summary>
        /// Gets or sets UomFrequencyScale.
        /// </summary>
        [DataMember]
        [StringLength(20, ErrorMessage = "The value of UomFrequencyScale must be less than 20 chars")]
        public string UomFrequencyScale { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether UomMelScale.
        /// </summary>
        [DataMember]
        public bool UomMelScale { get; set; }

        /// <summary>
        /// Gets or sets UomTimeScale.
        /// </summary>
        [DataMember]
        [StringLength(20, ErrorMessage = "The value of UomTimeScale must be less than 20 chars")]
        public string UomTimeScale { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use local audio files in Silverlight Isolated Storage.
        /// </summary>
        [DataMember]
        public bool UseLocalFiles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to download audio files from web server.
        /// </summary>
        [DataMember]
        public bool UseRemoteFiles { get; set; }

        /// <summary>
        /// Gets or sets Volume.
        /// </summary>
        [DataMember]
        public double Volume { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get desciption strings.
        /// </summary>
        /// <returns>
        /// Enumerable of strings.
        /// </returns>
        public IEnumerable<string> ToDescription()
        {
            yield return "AutomaticallySkipToNextTrack: " + this.AutomaticallySkipToNextTrack;
            yield return "ColorsLocked: " + this.ColorsLocked;
            yield return "ColorsProcessorTag: " + this.ColorsProcessorTag;
            yield return "ColorsSelected: " + this.ColorsSelected;
            yield return "ColorsTag: " + this.ColorsTag;
            yield return "ColorsTagWithProcessorResultId: " + this.ColorsTagWithProcessorResultId;
            yield return "ColorsUnsavedTag: " + this.ColorsUnsavedTag;
            yield return "DownloadsNumberForwardChunks: " + this.DownloadsNumberForwardChunks;
            yield return "DownloadsNumberPreviousChunks: " + this.DownloadsNumberPreviousChunks;
            yield return "DownloadsRetryCount: " + this.DownloadsRetryCount;
            yield return "IsMute: " + this.IsMute;
            yield return "ProfileSaveTimeMilliSeconds: " + this.ProfileSaveTimeMilliSeconds;
            yield return "TurnOnLooping: " + this.TurnOnLooping;
            yield return "TagListShowReferenceOnly: " + this.TagListShowReferenceOnly;
            yield return "UomFrequencyScale: " + this.UomFrequencyScale;
            yield return "UomMelScale: " + this.UomMelScale;
            yield return "UomTimeScale: " + this.UomTimeScale;
            yield return "UseLocalFiles: " + this.UseLocalFiles;
            yield return "UseRemoteFiles: " + this.UseRemoteFiles;
            yield return "Volume: " + this.Volume;
        }

        #endregion
    }
}