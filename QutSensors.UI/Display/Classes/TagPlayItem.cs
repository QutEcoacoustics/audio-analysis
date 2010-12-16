// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TagPlayItem.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using QutSensors.Shared;

namespace QutSensors.UI.Display.Classes
{
    /// <summary>
    /// Tag Play Item for displaying html tag list.
    /// </summary>
    public class TagPlayItem
    {
        /// <summary>
        /// Pixels to pad tag.
        /// </summary>
        public static readonly int PaddingPixels = 20;

        /// <summary>
        /// The pixels per millisecond.
        /// </summary>
        public static readonly double PixelsPerMillisecond = 0.045;

        /// <summary>
        /// The padding time.
        /// </summary>
        public static readonly TimeSpan PaddingTime = TimeSpan.FromMilliseconds(PaddingPixels / PixelsPerMillisecond);

        #region Spectrogram

        /// <summary>
        /// The spectrogram height in pixels.
        /// </summary>
        public static readonly int SpectrogramHeightPixels = 256;

        /// <summary>
        /// The max frequency range.
        /// </summary>
        public static readonly int SpectrogramMaxFrequency = 11025;

        /// <summary>
        /// The min frequency range.
        /// </summary>
        public static readonly int SpectrogramMinFrequency = 0;

        /// <summary>
        /// Gets SpectrogramPixelsWidth.
        /// </summary>
        public int SpectrogramPixelsWidth
        {
            get
            {
                return (PaddingPixels * 2) + TagPixelsWidth;
            }
        }

        /// <summary>
        /// Gets spectrogram duration.
        /// </summary>
        public TimeSpan SpectrogramDuration
        {
            get
            {
                return TimeSpan.FromMilliseconds(SpectrogramPixelsWidth / PixelsPerMillisecond);
            }
        }

        /// <summary>
        /// Gets SpectrogramPixelsHeight.
        /// </summary>
        public int SpectrogramPixelsHeight
        {
            get
            {
                return SpectrogramHeightPixels;
            }
        }

        /// <summary>
        /// Gets RelativePaddedEnd.
        /// </summary>
        public TimeSpan SpectrogramRelativeEnd
        {
            get
            {
                TimeSpan paddedEnd = this.TagRelativeEnd.Add(PaddingTime);

                return paddedEnd > this.AudioDuration ? this.AudioDuration : paddedEnd;
            }
        }

        /// <summary>
        /// Gets RelativePaddedStart.
        /// </summary>
        public TimeSpan SpectrogramRelativeStart
        {
            get
            {
                TimeSpan paddedStart = this.TagRelativeStart.Add(-PaddingTime);

                return paddedStart < TimeSpan.Zero ? TimeSpan.Zero : paddedStart;
            }
        }

        #endregion

        #region audio

        /// <summary>
        /// Gets or sets Date and Time of start of audio.
        /// </summary>
        public DateTime AudioAbsoluteStart { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingDuration.
        /// </summary>
        public TimeSpan AudioDuration { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingId.
        /// </summary>
        public Guid AudioId { get; set; }

        /// <summary>
        /// Gets or sets DeploymentName.
        /// </summary>
        public string DeploymentName { get; set; }

        #endregion

        #region tag

        /// <summary>
        /// Gets AbsoluteEnd.
        /// </summary>
        public DateTime TagAbsoluteEnd
        {
            get
            {
                return this.AudioAbsoluteStart.Add(this.TagRelativeEnd);
            }
        }

        /// <summary>
        /// Gets AbsoluteStart.
        /// </summary>
        public DateTime TagAbsoluteStart
        {
            get
            {
                return this.AudioAbsoluteStart.Add(this.TagRelativeStart);
            }
        }

        /// <summary>
        /// Gets Duration of tag.
        /// </summary>
        public TimeSpan TagDuration
        {
            get
            {
                return (this.TagRelativeEnd - this.TagRelativeStart).Duration();
            }
        }

        /// <summary>
        /// Gets or sets MaxFrequency (largest, highest).
        /// </summary>
        public int TagFrequencyMax { get; set; }

        /// <summary>
        /// Gets or sets MinFrequency (smallest, lowest).
        /// </summary>
        public int TagFrequencyMin { get; set; }

        /// <summary>
        /// Gets FrequencyRange.
        /// </summary>
        public int TagFrequencyRange
        {
            get
            {
                return Math.Abs(this.TagFrequencyMax - this.TagFrequencyMin);
            }
        }

        /// <summary>
        /// Gets or sets AudioTagId.
        /// </summary>
        public int TagId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this tag is a reference tag.
        /// </summary>
        public bool TagIsReference { get; set; }

        /// <summary>
        /// Gets or sets Tag name.
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// Gets or sets tag end time relative to audio start.
        /// </summary>
        public TimeSpan TagRelativeEnd { get; set; }

        /// <summary>
        /// Gets or sets tag start time relative to audio start.
        /// </summary>
        public TimeSpan TagRelativeStart { get; set; }

        /// <summary>
        /// Gets TagPixelsTop.
        /// </summary>
        public int TagPixelsTop
        {
            get
            {
                return
                    Convert.ToInt32(
                        ((SpectrogramMaxFrequency - this.TagFrequencyMax) / (double)SpectrogramMaxFrequency) *
                        SpectrogramHeightPixels);
            }
        }

        /// <summary>
        /// Gets TagPixelsLeft.
        /// </summary>
        public int TagPixelsLeft
        {
            get
            {
                return
                    Convert.ToInt32(Math.Min(PaddingPixels, TagRelativeStart.TotalMilliseconds * PixelsPerMillisecond));
            }
        }

        /// <summary>
        /// Gets TagPixelsWidth.
        /// </summary>
        public int TagPixelsWidth
        {
            get
            {
                return Convert.ToInt32(this.TagDuration.TotalMilliseconds * PixelsPerMillisecond);
            }
        }

        /// <summary>
        /// Gets TagPixelsHeight.
        /// </summary>
        public int TagPixelsHeight
        {
            get
            {
                return Convert.ToInt32((this.TagFrequencyRange / (double)SpectrogramMaxFrequency) * SpectrogramHeightPixels);
            }
        }

        #endregion

        /// <summary>
        /// Get Url for Image.
        /// </summary>
        /// <returns>
        /// Image url.
        /// </returns>
        public string UrlImage()
        {
            return string.Format(
                "/Spectrogram.ashx?height={0}&ppms={1}&ID={2}&start={3}&end={4}",
                SpectrogramHeightPixels,
                PixelsPerMillisecond,
                this.AudioId,
                Convert.ToInt64(this.SpectrogramRelativeStart.TotalMilliseconds),
                Convert.ToInt64(this.SpectrogramRelativeEnd.TotalMilliseconds));
        }

        /// <summary>
        /// Get audio url.
        /// </summary>
        /// <param name="mimeType">
        /// The mime type.
        /// </param>
        /// <returns>
        /// Audio url.
        /// </returns>
        public string UrlAudioMimeType(string mimeType)
        {
            return this.UrlAudioExt(MimeTypes.GetExtension(mimeType));
        }

        /// <summary>
        /// Get audio url.
        /// </summary>
        /// <param name="ext">
        /// The extension.
        /// </param>
        /// <returns>
        /// Audio url.
        /// </returns>
        public string UrlAudioExt(string ext)
        {
            string canonExt = MimeTypes.GetExtension(MimeTypes.GetMimeTypeFromExtension(ext));

            // /sensors/ByID/
            // "/AudioReading.ashx?ID={0}&Type={1}&start={2}&end={3}",

            return string.Format(
                "/sensors/ByID/{0}.{1}?start={2}&end={3}",
                this.AudioId,
                canonExt,
                Convert.ToInt64(this.SpectrogramRelativeStart.TotalMilliseconds),
                Convert.ToInt64(this.SpectrogramRelativeEnd.TotalMilliseconds));
        }

        /// <summary>
        /// Get a link to the sub item that contains the tag.
        /// </summary>
        /// <returns>
        /// Player url.
        /// </returns>
        public string UrlPlayerSegment()
        {
            // subitemtoload identified by start time of segment
            var segmentSize = TimeSpan.FromMinutes(6).TotalMilliseconds;

            // get left over from dividing by segment size 
            var remainder = TagRelativeStart.TotalMilliseconds % segmentSize;
            var relativeStart = TagRelativeStart.TotalMilliseconds - remainder;


            return
                string.Format(
                    "/UI/AudioReading/AudioReadingData.aspx?AudioReadingIds={0}&subitemtoload={1}&jumptotimeoffsetms={2}",
                    AudioId,
                    relativeStart,
                    remainder);
        }
    }
}