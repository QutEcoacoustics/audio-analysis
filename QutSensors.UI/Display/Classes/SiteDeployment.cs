// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteDeployment.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display.Classes
{
    using System;

    /// <summary>
    /// The site deployment.
    /// </summary>
    public struct SiteDeployment
    {
        /// <summary>
        /// Gets or sets ProjectId.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets SiteId.
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// Gets or sets DeploymentId.
        /// </summary>
        public Guid DeploymentId { get; set; }

        /// <summary>
        /// Gets or sets EntityDeploymentId.
        /// </summary>
        public int? EntityDeploymentId { get; set; }

        /// <summary>
        /// Gets or sets HardwareId.
        /// </summary>
        public int HardwareId { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingCount.
        /// </summary>
        public int AudioReadingCount { get; set; }

        /// <summary>
        /// Gets or sets Started.
        /// </summary>
        public DateTime? Started { get; set; }

        /// <summary>
        /// Gets or sets Ended.
        /// </summary>
        public DateTime? Ended { get; set; }

        /// <summary>
        /// Gets or sets FirstReading.
        /// </summary>
        public DateTime? FirstReading { get; set; }

        /// <summary>
        /// Gets or sets LatestReading.
        /// </summary>
        public DateTime? LatestReading { get; set; }

        /// <summary>
        /// Gets or sets Active.
        /// </summary>
        public string Active { get; set; }

        /// <summary>
        /// Gets or sets Test.
        /// </summary>
        public string Test { get; set; }

        /// <summary>
        /// Gets or sets ListenQueryString.
        /// </summary>
        public string ListenQueryString { get; set; }
    }
}