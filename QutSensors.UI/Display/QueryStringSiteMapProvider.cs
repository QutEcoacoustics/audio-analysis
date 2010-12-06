// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryStringSiteMapProvider.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Web;

    using QutSensors.Data.Linq;

    /// <summary>
    /// Allows pages to specify a reliance on one or more querystrings.
    /// </summary>
    public class QueryStringSiteMapProvider : XmlSiteMapProvider
    {
        /// <summary>
        /// The initialize.
        /// </summary>
        /// <param name="name">
        /// Site map Name.
        /// </param>
        /// <param name="attributes">
        /// The attributes.
        /// </param>
        public override void Initialize(string name, NameValueCollection attributes)
        {
            base.Initialize(name, attributes);
            this.SiteMapResolve += SmartSiteMapProviderSiteMapResolve;
        }

        private static SiteMapNode SmartSiteMapProviderSiteMapResolve(object sender, SiteMapResolveEventArgs e)
        {
            if (SiteMap.CurrentNode == null)
            {
                return null;
            }

            var temp = SiteMap.CurrentNode.Clone(true);

            // begin at current node and traverse through parents
            var tempNode = temp;
            while (tempNode != null)
            {
                var properties = GetSiteMapNode(tempNode, e.Context);
                tempNode.Url = properties.Url;
                tempNode.Title = properties.Title;
                tempNode = tempNode.ParentNode;
            }

            return temp;
        }

        private static SiteMapNode GetSiteMapNode(SiteMapNode node, HttpContext context)
        {
            var newNode = new SiteMapNode(node.Provider, node.Key)
                {
                    Title = node.Title,
                    Description = node.Description,
                    Url = node.Url
                };

            // required for link
            if (node["requiredForLink"] == null)
            {
                return newNode;
            }

            var requiredLinks = node["requiredForLink"].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            var queryStringNameValues = context.Request.QueryString;

            if (!requiredLinks.All(s => queryStringNameValues.AllKeys.Contains(s)))
            {
                newNode.Url = string.Empty;
                return newNode;
            }

            // set title 
            var newTitle = string.Empty;

            if (requiredLinks.Contains(QutSensorsPageHelper.EntityProjectIdString) && string.IsNullOrEmpty(newTitle))
                newTitle = "Project: " + GetEntityName(context.Request.QueryString[QutSensorsPageHelper.EntityProjectIdString]);

            if (requiredLinks.Contains(QutSensorsPageHelper.EntitySiteIdString) && string.IsNullOrEmpty(newTitle))
                newTitle = "Site: " + GetEntityName(context.Request.QueryString[QutSensorsPageHelper.EntitySiteIdString]);

            if (requiredLinks.Contains(QutSensorsPageHelper.EntityFilterIdString) && string.IsNullOrEmpty(newTitle))
                newTitle = "Data Set: " + GetEntityName(context.Request.QueryString[QutSensorsPageHelper.EntityFilterIdString]);

            if (requiredLinks.Contains(QutSensorsPageHelper.EntityJobIdString) && string.IsNullOrEmpty(newTitle))
                newTitle = "Job: " + GetEntityName(context.Request.QueryString[QutSensorsPageHelper.EntityJobIdString]);

            if (requiredLinks.Contains(QutSensorsPageHelper.EntityDeploymentIdString) && string.IsNullOrEmpty(newTitle))
                newTitle = "Deployment: " + GetEntityName(context.Request.QueryString[QutSensorsPageHelper.EntityDeploymentIdString]);

            newNode.Title = string.IsNullOrEmpty(newTitle) ? node.Title : newTitle;
            newNode.Description = newNode.Title;

            newNode.Url = node.Url;
            if (!newNode.Url.EndsWith("?")) newNode.Url += "?";

            if (node["optionalForLink"] != null)
            {
                var optionalLinks = node["optionalForLink"].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            
                if (optionalLinks.Any()) 
                    requiredLinks = requiredLinks.Concat(optionalLinks);
            }

            if (requiredLinks.Any())
            {
                newNode.Url += string.Join("&", requiredLinks.Select(s => s + "=" + queryStringNameValues[s]).ToArray());
            }

            return newNode;
        }

        private static string GetEntityName(string entityIdString)
        {
            if (string.IsNullOrEmpty(entityIdString))
                return string.Empty;

            int entityId;
            if (!int.TryParse(entityIdString, out entityId))
            {
                return string.Empty;
            }

            if (entityId > 0)
            {
                using (var db = new QutSensorsDb())
                {
                    var entity = db.Entity_Items.Where(ei => ei.EntityID == entityId).SingleOrDefault();
                    return entity != null ? entity.Name : string.Empty;
                }
            }

            return string.Empty;
        }
    }
}