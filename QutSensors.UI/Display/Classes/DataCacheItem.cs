// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCacheItem.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using QutSensors.Shared;

namespace QutSensors.UI.Display.Classes
{
    /// <summary>
    /// Data Cache Item.
    /// </summary>
    public class DataCacheItem
    {
        /// <summary>
        /// Gets or sets CreatedTime.
        /// </summary>
        public string CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets End.
        /// </summary>
        public string End { get; set; }

        /// <summary>
        /// Gets or sets Duration.
        /// </summary>
        public string Duration { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingId.
        /// </summary>
        public string AudioReadingIdQs { get; set; }

        /// <summary>
        /// Gets or sets JobId.
        /// </summary>
        public int JobId { get; set; }

        /// <summary>
        /// Gets or sets JobItemId.
        /// </summary>
        public int JobItemId { get; set; }

        /// <summary>
        /// Gets or sets LastAccessed.
        /// </summary>
        public string LastAccessed { get; set; }

        /// <summary>
        /// Gets or sets MimeType.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets ProcessingStartTime.
        /// </summary>
        public string ProcessingStartTime { get; set; }

        /// <summary>
        /// Gets or sets Start.
        /// </summary>
        public string Start { get; set; }

        /// <summary>
        /// Gets or sets Status.
        /// </summary>
        public CacheJobItemStatus Status { get; set; }

        /// <summary>
        /// Gets or sets TimeSpentGenerating.
        /// </summary>
        public string TimeSpentGenerating { get; set; }

        /// <summary>
        /// Gets or sets Type.
        /// </summary>
        public string Type { get; set; }
    }
}