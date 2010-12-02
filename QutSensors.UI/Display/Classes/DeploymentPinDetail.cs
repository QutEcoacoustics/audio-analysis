// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PinData.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the PinData type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Web;

    using QutSensors.Data.Linq;

    /// <summary>
    /// Pin Data for single deployment.
    /// </summary>
    [DataContract]
    public class DeploymentPinDetail
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentPinDetail"/> class.
        /// </summary>
        public DeploymentPinDetail()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentPinDetail"/> class.
        /// </summary>
        /// <param name="db">
        /// Data Context.
        /// </param>
        /// <param name="deployment">
        /// The deployment.
        /// </param>
        public DeploymentPinDetail(QutSensorsDb db, Deployment deployment)
        {
            this.Details = deployment.Description ?? string.Empty;
            this.SensorLink = "<a href='" + deployment.GetSensorDetailsHRef().ToAbsolute() + "'>" + deployment.Name +
                             "</a><br>";

            var status = deployment.Hardware.GetLastStatus(db);
            this.PowerGaugeUrl = status == null || status.PowerLevel == null ?
                VirtualPathUtility.ToAbsolute("~/graphics/PowerGauge.ashx") :
                VirtualPathUtility.ToAbsolute("~/graphics/PowerGauge.ashx") + "?Value=" + status.PowerLevel;

            var lastContacted = deployment.Hardware.LastContacted;
            this.LastContacted = lastContacted == null ? "Unknown" :
                string.Format("{0:dd/MM/yyyy HH:mm} ({1}) ", lastContacted.Value, lastContacted.Value.ToDifferenceString(DateTime.Now));

            this.Logs = db.DeviceLogs
                .Where(dl => dl.HardwareID == deployment.HardwareID)
                .OrderByDescending(dl => dl.Time)
                .Take(5)
                .Select(dl => new { dl.Time, dl.Text })
                .ToList()
                .Select(l => string.Format("{0:dd/MM/yyyy HH:mm} - {1}", l.Time, l.Text))
                .ToList();

            this.Health = deployment.Hardware.IsAsleep ? -2 : deployment.Health;
        }

        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        [DataMember]
        public string SensorLink { get; set; }

        /// <summary>
        /// Gets or sets Details.
        /// </summary>
        [DataMember]
        public string Details { get; set; }

        /// <summary>
        /// Gets or sets SiteUrl.
        /// </summary>
        [DataMember]
        public string SiteLink { get; set; }

        /// <summary>
        /// Gets or sets MaxIndex.
        /// </summary>
        [DataMember]
        public int MaxIndex { get; set; }

        /// <summary>
        /// Gets or sets LastContacted.
        /// </summary>
        [DataMember]
        public string LastContacted { get; set; }

        /// <summary>
        /// Gets or sets Logs.
        /// </summary>
        [DataMember]
        public List<string> Logs { get; set; }

        /// <summary>
        /// Gets or sets PowerGaugeUrl.
        /// </summary>
        [DataMember]
        public string PowerGaugeUrl { get; set; }

        /// <summary>
        /// Gets or sets Health.
        /// </summary>
        [DataMember]
        public double Health { get; set; }
    }
}