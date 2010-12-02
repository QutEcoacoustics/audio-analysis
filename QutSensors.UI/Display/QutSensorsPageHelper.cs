// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QutSensorsPageHelper.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Linq;

namespace QutSensors.UI.Display
{
    /// <summary>
    /// Helps with getting querystrings from entity web pages.
    /// </summary>
    public static class QutSensorsPageHelper
    {
        /// <summary>
        /// The entity project id.
        /// </summary>
        public const string ENTITY_PROJECT_ID = "ProjectId";

        /// <summary>
        /// The entity site id.
        /// </summary>
        public const string ENTITY_SITE_ID = "SiteId";

        /// <summary>
        /// The entity deployment id.
        /// </summary>
        public const string ENTITY_DEPLOYMENT_ID = "EntityDeploymentId";

        /// <summary>
        /// The entity filter id.
        /// </summary>
        public const string ENTITY_FILTER_ID = "FilterId";

        /// <summary>
        /// The entity job id.
        /// </summary>
        public const string ENTITY_JOB_ID = "JobId";

        /// <summary>
        /// The hardware id.
        /// </summary>
        public const string HARDWARE_ID = "HardwareId";

        /// <summary>
        /// The processor type id.
        /// </summary>
        public const string PROCESSOR_TYPE_ID = "ProcessorTypeId";

        /// <summary>
        /// The audio reading id.
        /// </summary>
        public const string AUDIO_READING_ID = "AudioReadingId";

        /// <summary>
        /// The user id.
        /// </summary>
        public const string USER_ID = "UserId";

        /// <summary>
        /// Gets EntityProjectId.
        /// </summary>
        public static int EntityProjectId
        {
            get
            {
                return GetInt(ENTITY_PROJECT_ID);
            }
        }

        /// <summary>
        /// Gets EntitySiteId.
        /// </summary>
        public static int EntitySiteId
        {
            get
            {
                return GetInt(ENTITY_SITE_ID);
            }
        }

        /// <summary>
        /// Gets EntityDeploymentId.
        /// </summary>
        public static int EntityDeploymentId
        {
            get
            {
                return GetInt(ENTITY_DEPLOYMENT_ID);
            }
        }

        /// <summary>
        /// Gets EntityFilterId.
        /// </summary>
        public static int EntityFilterId
        {
            get
            {
                return GetInt(ENTITY_FILTER_ID);
            }
        }

        /// <summary>
        /// Gets EntityJobId.
        /// </summary>
        public static int EntityJobId
        {
            get
            {
                return GetInt(ENTITY_JOB_ID);
            }
        }

        /// <summary>
        /// Gets HardwareId.
        /// </summary>
        public static int HardwareId
        {
            get
            {
                return GetInt(HARDWARE_ID);
            }
        }

        /// <summary>
        /// Gets ProcessorTypeId.
        /// </summary>
        public static int ProcessorTypeId
        {
            get
            {
                return GetInt(PROCESSOR_TYPE_ID);
            }
        }

        /// <summary>
        /// Gets AudioReadingId.
        /// </summary>
        public static Guid AudioReadingId
        {
            get
            {
                return GetGuid(AUDIO_READING_ID);
            }
        }

        /// <summary>
        /// Gets UserId.
        /// </summary>
        public static Guid UserId
        {
            get
            {
                return GetGuid(USER_ID);
            }
        }

        /// <summary>
        /// Creates a querystring given key=value pairs.
        /// </summary>
        /// <param name="items">
        /// Ordered key=value pairs. Must be a multiple of 2.
        /// </param>
        /// <returns>
        /// Querystring created from pairs of params.
        /// </returns>
        public static string CreateQueryString(params object[] items)
        {
            if (items == null || items.Length == 0 || items.Length % 2 != 0)
            {
                return string.Empty;
            }

            var qs = new StringBuilder();
            var qsItems = new List<string>(items.Where(i => i != null).Select(i => i.ToString()));

            while (qsItems.Count > 0)
            {
                string key = qsItems[0];
                string value = qsItems[1];

                qsItems.RemoveRange(0, 2);

                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value) && value != Guid.Empty.ToString() && value != "0")
                {
                    if (qs.Length > 0)
                    {
                        qs.Append("&");
                    }

                    qs.Append(key + "=" + value);
                }
            }

            return qs.ToString();
        }

        private static string GetQsValue(string name)
        {
            return WebUtilities.GetQuerystring(HttpContext.Current.Request.QueryString, name);
        }

        private static int GetInt(string idString)
        {
            int entityId;
            int.TryParse(GetQsValue(idString), out entityId);
            return entityId;
        }

        private static Guid GetGuid(string idString)
        {
            Guid id = Guid.Empty;

            try
            {
                id = new Guid(GetQsValue(idString));
            }
            catch
            {
                // catch all errors, we don't want this to fail.
            }

            return id;
        }
    }
}