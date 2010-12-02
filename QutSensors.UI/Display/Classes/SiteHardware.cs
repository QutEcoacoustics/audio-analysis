// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteHardware.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display.Classes
{
    using System;

    /// <summary>
    /// The site hardware.
    /// </summary>
    public struct SiteHardware
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
        /// Gets or sets HardwareId.
        /// </summary>
        public int HardwareId { get; set; }

        /// <summary>
        /// Gets or sets LastContact.
        /// </summary>
        public string LastContacted { get; set; }

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
        /// Gets or sets CurrentDeploymentStarted.
        /// </summary>
        public string CurrentDeploymentStarted { get; set; }

        /// <summary>
        /// Gets or sets CurrentDeploymentEnded.
        /// </summary>
        public string CurrentDeploymentEnded { get; set; }

        /// <summary>
        /// Gets or sets MostRecentRecording.
        /// </summary>
        public string MostRecentRecording { get; set; }

        /// <summary>
        /// Gets or sets IsActive.
        /// </summary>
        public string IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsTest.
        /// </summary>
        public string IsTest { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingsCount.
        /// </summary>
        public int AudioReadingsCount { get; set; }
    }

    /// <summary>
    /// Site audio upload.
    /// </summary>
    public class SiteAudioUploads
    {
        /// <summary>
        /// Gets or sets SensorName (hardware name and deployment name combined).
        /// </summary>
        public string SensorName { get; set; }
        public string UploadDate { get; set; }
        public string UploadBy { get; set; }
        public string UploadStatus { get; set; }
        public string UploadFileLengthMilliseconds { get; set; }
        public string UploadFileLengthBytes { get; set; }
        public string MimeType { get; set; }
        public DateTime AudioReadingTime { get; set; }
    }
}