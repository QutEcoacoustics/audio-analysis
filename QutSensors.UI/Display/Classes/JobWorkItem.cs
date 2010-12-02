// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobWorkItem.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display.Classes
{
    using System;

    using QutSensors.Shared;

    /// <summary>
    /// The job work item.
    /// </summary>
    public struct JobWorkItem
    {
        /// <summary>
        /// Gets or sets JobItemId.
        /// </summary>
        public int JobItemId { get; set; }

        /// <summary>
        /// Gets or sets JobId.
        /// </summary>
        public int JobId { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingId.
        /// </summary>
        public Guid AudioReadingId { get; set; }

        /// <summary>
        /// Gets or sets Status.
        /// </summary>
        public JobStatus Status { get; set; }

        /// <summary>
        /// Gets or sets Worker.
        /// </summary>
        public string Worker { get; set; }

        /// <summary>
        /// Gets or sets AcceptedTime.
        /// </summary>
        public string AcceptedTime { get; set; }

        // = ji.WorkerAcceptedTimeUTC.HasValue ? ji.WorkerAcceptedTimeUTC.Value.ToLocalTime().ToString() : "Pending",
        /// <summary>
        /// Gets or sets ItemrunDetails.
        /// </summary>
        public string ItemrunDetails { get; set; }

        /// <summary>
        /// Gets or sets StartTime.
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// Gets or sets EndTime.
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingSegmentLength.
        /// </summary>
        public string AudioReadingSegmentLength { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingTotalLength.
        /// </summary>
        public string AudioReadingTotalLength { get; set; }

        /// <summary>
        /// Gets or sets TotalJobItemResults.
        /// </summary>
        public int TotalJobItemResults { get; set; }

        /// <summary>
        /// Gets or sets FilterQs.
        /// </summary>
        public string FilterQs { get; set; }

        /// <summary>
        /// Gets or sets DeploymentName.
        /// </summary>
        public string DeploymentName { get; set; }
    }
}