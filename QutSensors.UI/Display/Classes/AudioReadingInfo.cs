// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AudioReadingInfo.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display.Classes
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The audio reading info.
    /// </summary>
    public class AudioReadingInfo
    {
        /// <summary>
        /// Gets or sets AudioReadingID.
        /// </summary>
        public Guid AudioReadingID { get; set; }

        /// <summary>
        /// Gets or sets HardwareID.
        /// </summary>
        public int HardwareID { get; set; }

        /// <summary>
        /// Gets or sets DeploymentID.
        /// </summary>
        public Guid? DeploymentID { get; set; }

        /// <summary>
        /// Gets or sets DeploymentName.
        /// </summary>
        public string DeploymentName { get; set; }

        /// <summary>
        /// Gets or sets Time.
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Gets or sets AudioTagNames.
        /// </summary>
        public IEnumerable<string> AudioTagNames { get; set; }

        /// <summary>
        /// Gets or sets MimeType.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets DurationMS.
        /// </summary>
        public int? DurationMS { get; set; }

        /// <summary>
        /// Gets or sets Duration.
        /// </summary>
        public TimeSpan? Duration
        {
            get
            {
                if (this.DurationMS != null)
                {
                    return TimeSpan.FromMilliseconds(this.DurationMS.Value);
                }

                return null;
            }

            set
            {
                if (value == null)
                {
                    this.DurationMS = null;
                }
                else
                {
                    this.DurationMS = (int)value.Value.TotalMilliseconds;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether HasBeenRead.
        /// </summary>
        public bool HasBeenRead { get; set; }

        /// <summary>
        /// Gets or sets EnvironmentalIndex.
        /// </summary>
        public double? EnvironmentalIndex { get; set; }
    }
}