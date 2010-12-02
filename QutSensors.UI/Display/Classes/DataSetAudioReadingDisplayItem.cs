// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSetAudioReadingDisplayItem.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Informaton for displaying audio reading details.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display.Classes
{
    using System;

    using QutSensors.Shared;

    /// <summary>
    /// Informaton for displaying audio reading details.
    /// </summary>
    public struct DataSetAudioReadingDisplayItem
    {
        /// <summary>
        /// Gets or sets Hardware FriendlyName.
        /// </summary>
        public string HardwareName { get; set; }

        /// <summary>
        /// Gets or sets DeploymentName.
        /// </summary>
        public string DeploymentName { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingStart.
        /// </summary>
        public DateTime AudioReadingStart { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingEnd.
        /// </summary>
        public DateTime AudioReadingEnd { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingDuration.
        /// </summary>
        public string AudioReadingDuration { get; set; }

        /// <summary>
        /// Gets or sets SegmentStartTime.
        /// </summary>
        public string SegmentStartTime { get; set; }

        /// <summary>
        /// Gets or sets SegmentEndTime.
        /// </summary>
        public string SegmentEndTime { get; set; }

        /// <summary>
        /// Gets or sets SegmentLength.
        /// </summary>
        public string SegmentDuration { get; set; }

        /// <summary>
        /// Gets or sets State of audio reading.
        /// </summary>
        public AudioReadingState State { get; set; }
    }
}
