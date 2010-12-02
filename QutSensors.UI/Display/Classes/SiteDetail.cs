// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteDetails.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Contains details of site and deployments for sensor map.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display.Classes
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using QutSensors.Data;
    using QutSensors.Data.Linq;

    using SoulSolutions.ClusterArticle;

    /// <summary>
    /// Contains details of site and deployments for sensor map.
    /// </summary>
    public class SiteDetail
    {
        public string SiteLink { get; set; }

        /// <summary>
        /// Gets or sets Location.
        /// </summary>
        public LatLong Location { get; set; }

        /// <summary>
        /// Gets Pins.
        /// </summary>
        public IEnumerable<DeploymentPinDetail> Deployments { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteDetail"/> class.
        /// Populates Pins with Deployment PinData.
        /// </summary>
        /// <param name="db">
        /// Data Context.
        /// </param>
        /// <param name="siteEntityId">
        /// The site entity id.
        /// </param>
        /// <param name="siteName">
        /// The site name.
        /// </param>
        public SiteDetail(QutSensorsDb db, int siteEntityId, string siteName)
        {
            var deps = EntityManager.Instance.GetDescendants(
                db,
                siteEntityId,
                ei =>
                    ei.Entity_MetaData.DeploymentID.HasValue
                    && ei.Entity_MetaData.Deployment.IsActive
                    && !ei.Entity_MetaData.Deployment.Hardware.IsManualDevice
                    && !(ei.Entity_MetaData.Deployment.IsSensitive.HasValue && ei.Entity_MetaData.Deployment.IsSensitive.Value))
                .OrderBy(ei => ei.Name)
                .Select(ei => ei.Entity_MetaData.Deployment)
                .ToList();

            var url = VirtualPathUtility.ToAbsolute("~/UI/Site/SiteDisplay.aspx") + "?" +
                      QutSensorsPageHelper.CreateQueryString(QutSensorsPageHelper.ENTITY_SITE_ID, siteEntityId);
            var siteLink = "Site: <a href='" + url + "'>" + siteName + "</a><br>";
            this.SiteLink = siteLink;

            this.Deployments = deps.Select(d => new DeploymentPinDetail(db, d) { SiteLink = this.SiteLink }).ToList();
        }
    }
}