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
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Drawing;
    using System.Runtime.Serialization;

    /// <summary>
    /// A class of settings used by the player. All should be simple and serializable.
    /// </summary>
    [Serializable]
    [DataContract]
    public class PlayerSettings
    {
        private const bool DefaultAutomaticallySkipToNextTrack = false;
        private const string DefaultColorsLocked = "#30FF0800"; ////Color.FromArgb(48, 255, 8, 0).ToHexString(true);
        private const string DefaultColorsSelected = "#30FCFF00"; ////Color.FromArgb(48, 252, 255, 0).ToHexString(true);
        private const string DefaultColorsTag = "#3051FF00"; ////Color.FromArgb(48, 81, 255, 0).ToHexString(true);
        private const string DefaultColorsProcessorTag = "#30003EFF"; ////Color.FromArgb(48, 0, 62, 255).ToHexString(true);
        private const string DefaultColorsTagWithProcessorResultId = "#3000FF87"; /////Color.FromArgb(48, 0, 255, 135).ToHexString(true);
        private const string DefaultColorsUnsavedTag = "#30FFFFFF"; ////Color.FromArgb(48, 255, 255, 255).ToHexString(true);
        private const int DefaultDownloadsNumberForwardChunks = 2;
        private const int DefaultDownloadsNumberPreviousChunks = 1;
        private const int DefaultDownloadsRetryCount = 0;
        private const bool DefaultIsMute = false;
        private const int DefaultProfileSaveTimeMilliSeconds = 40 * 1000;
        private const bool DefaultTurnOnLooping = true;
        private const string DefaultUomFrequencyScale = "Hz";
        private const bool DefaultUomMelScale = false;
        private const string DefaultUomTimeScale = "ms";
        private const bool DefaultUseLocalAudio = false;
        private const bool DefaultUseRemoteAudio = true;
        private const double DefaultVolume = 0.9;

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
            this.UseLocalAudio = DefaultUseLocalAudio;
            this.UseRemoteAudio = DefaultUseRemoteAudio;
            this.Volume = DefaultVolume;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether AutomaticallySkipToNextTrack.
        /// </summary>
        [DataMember]
        [DefaultValue(DefaultAutomaticallySkipToNextTrack)]
        public bool AutomaticallySkipToNextTrack { get; set; }

        /// <summary>
        /// Gets or sets ColorsLocked.
        /// </summary>
        [DataMember]
        [RegularExpression(ImageExtensions.RegExHexColor, ErrorMessage = ImageExtensions.ColorRegExError)]
        [DefaultValue(DefaultColorsLocked)]
        public string ColorsLocked { get; set; }

        /// <summary>
        /// Gets or sets ColorsProcessorTag.
        /// </summary>
        [DataMember]
        [RegularExpression(ImageExtensions.RegExHexColor, ErrorMessage = ImageExtensions.ColorRegExError)]
        [DefaultValue(DefaultColorsProcessorTag)]
        public string ColorsProcessorTag { get; set; }

        /// <summary>
        /// Gets or sets ColorsSelected.
        /// </summary>
        [DataMember]
        [RegularExpression(ImageExtensions.RegExHexColor, ErrorMessage = ImageExtensions.ColorRegExError)]
        [DefaultValue(DefaultColorsSelected)]
        public string ColorsSelected { get; set; }

        /// <summary>
        /// Gets or sets ColorsTag.
        /// </summary>
        [DataMember]
        [RegularExpression(ImageExtensions.RegExHexColor, ErrorMessage = ImageExtensions.ColorRegExError)]
        [DefaultValue(DefaultColorsTag)]
        public string ColorsTag { get; set; }

        /// <summary>
        /// Gets or sets ColorsTagWithProcessorResultId.
        /// </summary>
        [DataMember]
        [RegularExpression(ImageExtensions.RegExHexColor, ErrorMessage = ImageExtensions.ColorRegExError)]
        [DefaultValue(DefaultColorsTagWithProcessorResultId)]
        public string ColorsTagWithProcessorResultId { get; set; }

        /// <summary>
        /// Gets or sets ColorsUnsavedTag.
        /// </summary>
        [DataMember]
        [RegularExpression(ImageExtensions.RegExHexColor, ErrorMessage = ImageExtensions.ColorRegExError)]
        [DefaultValue(DefaultColorsUnsavedTag)]
        public string ColorsUnsavedTag { get; set; }

        /// <summary>
        /// Gets or sets DownloadsNumberForwardChunks.
        /// </summary>
        [DataMember]
        [Range(0, 10)]
        [DefaultValue(DefaultDownloadsNumberForwardChunks)]
        public int DownloadsNumberForwardChunks { get; set; }

        /// <summary>
        /// Gets or sets DownloadsNumberPreviousChunks.
        /// </summary>
        [DataMember]
        [Range(0, 10)]
        [DefaultValue(DefaultDownloadsNumberPreviousChunks)]
        public int DownloadsNumberPreviousChunks { get; set; }

        /// <summary>
        /// Gets or sets DownloadsRetryCount.
        /// </summary>
        [DataMember]
        [Range(0, 100)]
        [DefaultValue(DefaultDownloadsRetryCount)]
        public int DownloadsRetryCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsMute.
        /// </summary>
        [DataMember]
        [DefaultValue(DefaultIsMute)]
        public bool IsMute { get; set; }

        /// <summary>
        /// Gets or sets ProfileSaveTime_ms.
        /// </summary>
        [DataMember]
        [DefaultValue(DefaultProfileSaveTimeMilliSeconds)]
        public int ProfileSaveTimeMilliSeconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether TurnOnLooping.
        /// </summary>
        [DataMember]
        [DefaultValue(DefaultTurnOnLooping)]
        public bool TurnOnLooping { get; set; }

        /// <summary>
        /// Gets or sets UomFrequencyScale.
        /// </summary>
        [DataMember]
        [StringLength(20, ErrorMessage = "The value of UomFrequencyScale must be less than 20 chars")]
        [DefaultValue(DefaultUomFrequencyScale)]
        public string UomFrequencyScale { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether UomMelScale.
        /// </summary>
        [DataMember]
        [DefaultValue(DefaultUomMelScale)]
        public bool UomMelScale { get; set; }

        /// <summary>
        /// Gets or sets UomTimeScale.
        /// </summary>
        [DataMember]
        [StringLength(20, ErrorMessage = "The value of UomTimeScale must be less than 20 chars")]
        [DefaultValue(DefaultUomTimeScale)]
        public string UomTimeScale { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use local audio files in Silverlight Isolated Storage.
        /// </summary>
        [DataMember]
        [DefaultValue(DefaultUseLocalAudio)]
        public bool UseLocalAudio { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to download audio files from web server.
        /// </summary>
        [DataMember]
        [DefaultValue(DefaultUseRemoteAudio)]
        public bool UseRemoteAudio { get; set; }

        /// <summary>
        /// Gets or sets Volume.
        /// </summary>
        [DataMember]
        [DefaultValue(DefaultVolume)]
        public double Volume { get; set; }

        #endregion

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
            yield return "UomFrequencyScale: " + this.UomFrequencyScale;
            yield return "UomMelScale: " + this.UomMelScale;
            yield return "UomTimeScale: " + this.UomTimeScale;
            yield return "UseLocalAudio: " + this.UseLocalAudio;
            yield return "UseRemoteAudio: " + this.UseRemoteAudio;
            yield return "Volume: " + this.Volume;
        }
    }
}