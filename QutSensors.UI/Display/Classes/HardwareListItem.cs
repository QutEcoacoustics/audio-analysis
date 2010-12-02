// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HardwareListItem.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display.Classes
{
    using System;

    /// <summary>
    /// The hardware list item.
    /// </summary>
    public struct HardwareListItem
    {
        /// <summary>
        /// Gets or sets HardwareId.
        /// </summary>
        public int HardwareId { get; set; }

        /// <summary>
        /// Gets or sets HardwareName.
        /// </summary>
        public string HardwareName { get; set; }

        /// <summary>
        /// Gets or sets CurrentDeploymentEntityId.
        /// </summary>
        public int CurrentDeploymentEntityId { get; set; }

        /// <summary>
        /// Gets or sets CurrentDeploymentId.
        /// </summary>
        public Guid? CurrentDeploymentId { get; set; }

        /// <summary>
        /// Gets or sets CurrentDeploymentName.
        /// </summary>
        public string CurrentDeploymentName { get; set; }

        /// <summary>
        /// Gets or sets DeploymentCount.
        /// </summary>
        public int DeploymentCount { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingsCount.
        /// </summary>
        public int AudioReadingsCount { get; set; }

        /// <summary>
        /// Gets or sets LastContacted.
        /// </summary>
        public string LastContacted { get; set; }

        /// <summary>
        /// Gets or sets DeviceType.
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// Gets or sets PowerLevel.
        /// </summary>
        public string PowerLevel { get; set; }

        /// <summary>
        /// Gets or sets FreeMemory.
        /// </summary>
        public string FreeMemory { get; set; }

        /// <summary>
        /// Gets or sets MemoryUsage.
        /// </summary>
        public string MemoryUsage { get; set; }

        /// <summary>
        /// Gets or sets HardwareUniqueID.
        /// </summary>
        public string HardwareUniqueID { get; set; }
    }
}