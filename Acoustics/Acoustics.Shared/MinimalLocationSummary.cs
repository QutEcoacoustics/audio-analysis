namespace Acoustics.Shared
{
    using System;

    /// <summary> List of the audio item recording times for a location, as determined by a filter.
	/// </summary>

	public class MinimalLocationSummary
	{
		/// <summary> Get or set the location name.
		/// </summary>

		public string LocationName { get; set; }

		/// <summary> Get or set the entity id of the location.
		/// </summary>

		public int LocationEntityId { get; set; }

		/// <summary> Get or set the total duration ofthe audio associated with this location.
		/// </summary>

		public TimeSpan TotalDuration { get; set; }

		/// <summary> Get or set the list of date-time points at which the location has associated audio.
		/// </summary>

		public DateTime[] RecordingTimes { get; set; }
	}
}
