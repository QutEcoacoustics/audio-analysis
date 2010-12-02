// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobAudioReading.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display.Classes
{
    using System;

    /// <summary>
    /// The job audio reading.
    /// </summary>
    public class JobAudioReading
    {
        /// <summary>
        /// Gets or sets AudioReadingId.
        /// </summary>
        public Guid AudioReadingId { get; set; }

        /// <summary>
        /// Gets or sets ReadingStart.
        /// </summary>
        public string ReadingStart { get; set; }

        /// <summary>
        /// Gets or sets ReadingLength.
        /// </summary>
        public string ReadingEnd { get; set; }

        public string ReadingDuration { get; set; }

        /// <summary>
        /// Gets or sets number of results for the audio reading.
        /// </summary>
        public int JobResultCount { get; set; }

        /// <summary>
        /// Gets or sets max score.
        /// </summary>
        public double? MaxScore { get; set; }

        /// <summary>
        /// Gets or sets Min Score.
        /// </summary>
        public double? MinScore { get; set; }
    }
}