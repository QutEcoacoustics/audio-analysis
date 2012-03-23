namespace Acoustics.Shared
{
    using System;

    /// <summary>
    /// Details for a single filter result for a ReadingsFilter search.
    /// </summary>
    public class FilterResult
    {
        /// <summary>
        /// Gets or sets Audio Reading Id for the reading this result represents.
        /// </summary>
        public Guid AudioReadingId { get; set; }

        /// <summary>
        /// Gets or sets Hardware Id of the hardware that recorded this audio reading.
        /// </summary>
        public int HardwareId { get; set; }

        /// <summary>
        /// Gets or sets Deployment Id for deployment that recorded this audio reading.
        /// </summary>
        public Guid? DeploymentId { get; set; }

		/// <summary> Get or set the site id from which the associated audio reading was obtained.
		/// </summary>

		public Guid? SiteId { get; set; }

        /// <summary>
        /// Gets or sets Date and time recording of the audio reading began.
        /// </summary>
        public DateTime AudioReadingStart { get; set; }

        /// <summary>
        /// Gets or sets Start time of this result in milliseconds, relative to the audio reading start.
        /// </summary>
        public int? StartTime { get; set; }

        /// <summary>
        /// Gets or sets End time of this result in milliseconds, relative to the audio reading start.
        /// </summary>
        public int? EndTime { get; set; }

        /// <summary>
        /// Gets or sets total duration of the audio reading.
        /// </summary>
        public int AudioReadingTotalDuration { get; set; }

        /// <summary>
        /// Gets or sets State of audio reading.
        /// </summary>
        public AudioReadingState State { get; set; }

        /// <summary>
        /// Gets or sets Hardware FriendlyName.
        /// </summary>
        public string HardwareName { get; set; }

        /// <summary>
        /// Gets or sets DeploymentName.
        /// </summary>
        public string DeploymentName { get; set; }
    }
}
