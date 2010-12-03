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
    /// </summary>
    [Serializable]
    [DataContract]
    public class PlayerSettings
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerSettings"/> class.
        /// </summary>
        public PlayerSettings()
        {
            this.AutomaticallySkipToNextTrack = false;
            this.ColorsLocked = Color.FromArgb(48, 255, 8, 0).ToHexString(true);
            this.ColorsSelected = Color.FromArgb(48, 252, 255, 0).ToHexString(true);
            this.ColorsTag = Color.FromArgb(48, 81, 255, 0).ToHexString(true);
            this.ColorsProcessorTag = Color.FromArgb(48, 0, 62, 255).ToHexString(true);
            this.ColorsTagWithProcessorResultId = Color.FromArgb(48, 0, 255, 135).ToHexString(true);
            this.ColorsUnsavedTag = Color.FromArgb(48, 255, 255, 255).ToHexString(true);
            this.DownloadsNumberForwardChunks = 2;
            this.DownloadsNumberPreviousChunks = 1;
            this.DownloadsRetryCount = 0;
            this.IsMute = false;
            this.ProfileSaveTimeMilliSeconds = 40 * 1000;
            this.TurnOnLooping = true;
            this.UomFrequencyScale = "Hz";
            this.UomMelScale = false;
            this.UomTimeScale = "ms";
            this.Volume = 0.9;
        }

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
        /// Gets or sets Volume.
        /// </summary>
        [DataMember]
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
            yield return "Volume: " + this.Volume;
        }
    }
}