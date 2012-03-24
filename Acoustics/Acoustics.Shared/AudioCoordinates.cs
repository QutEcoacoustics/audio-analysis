namespace Acoustics.Shared
{
    using System;

    /// <summary> 
    /// Represents a specific location within a filter.
    /// </summary>
    public class AudioCoordinates
    {
        /// <summary> 
        /// Gets or sets the Entity Id of the site.
        /// </summary>
        public int LocationEntityId { get; set; }

        /// <summary> 
        /// Gets or sets the date of the recording.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary> 
        /// Gets or sets the time of the recording.
        /// </summary>
        public TimeSpan Time { get; set; }
    }
}
