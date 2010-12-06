// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebUtilities.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using QutSensors.Business;
    using QutSensors.Data.Linq;
    using QutSensors.Shared;
    using QutSensors.UI.Display.Classes;

    /// <summary>
    /// The utilities.
    /// </summary>
    public static class WebUtilities
    {
        /// <summary>
        /// The include test deployments query string key.
        /// </summary>
        public const string IncludeTestDeploymentsQueryStringKey = "Tests";

        /// <summary>
        /// Gets ReadingsDateFormat.
        /// </summary>
        public static string ReadingsDateFormat
        {
            get
            {
                string retVal = ConfigurationManager.AppSettings["ReadingsDateFormat"];
                if (string.IsNullOrEmpty(retVal))
                {
                    retVal = "yyyyMMddTHHmmss";
                }

                return retVal;
            }
        }

        /// <summary>
        /// The get deployment.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// </returns>
        public static Deployment GetDeployment(HttpContext context)
        {
            using (var db = new QutSensorsDb())
            {
                string id = context.Request["ID"];
                if (!string.IsNullOrEmpty(id))
                {
                    Guid? deploymentID = ParsingUtilities.ParseGuid(id);
                    return db.Deployments.FirstOrDefault(d => d.DeploymentID == deploymentID);
                }

                string name = context.Request["Name"];
                if (!string.IsNullOrEmpty(name))
                {
                    return db.Deployments.FirstOrDefault(d => d.Name == name);
                }
            }

            return null;
        }

        /// <summary>
        /// The get audio reading.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// </returns>
        public static AudioReading GetAudioReading(HttpRequest request)
        {
            using (var db = new QutSensorsDb())
            {
                return GetAudioReading(db, request["ID"], request["Name"], request["Time"]);
            }
        }

        /// <summary>
        /// The get audio reading.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        /// <param name="deploymentName">
        /// The deployment name.
        /// </param>
        /// <param name="readingTime">
        /// The reading time.
        /// </param>
        /// <returns>
        /// </returns>
        public static AudioReading GetAudioReading(QutSensorsDb db, string deploymentName, DateTime readingTime)
        {
            AudioReading reading = GetAudioReading(db, null, deploymentName, readingTime);
            return reading;
        }

        /// <summary>
        /// The get audio reading linq.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// </returns>
        public static AudioReading GetAudioReadingLinq(QutSensorsDb db, HttpRequest request)
        {
            AudioReading reading = GetAudioReading(db, request["ID"], request["Name"], request["Time"]);
            return reading;
        }

        private static AudioReading GetAudioReading(
            QutSensorsDb db, string audioReadingIdString, string deploymentName, string readingTimeString)
        {
            Guid? audioReadingId = null;
            if (!string.IsNullOrEmpty(audioReadingIdString))
            {
                audioReadingId = ParsingUtilities.ParseGuid(audioReadingIdString);
            }

            DateTime readingTime = DateTime.MinValue;
            if (!string.IsNullOrEmpty(readingTimeString))
            {
                DateTime.TryParseExact(
                    readingTimeString,
                    ReadingsDateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out readingTime);
            }

            AudioReading reading = GetAudioReading(db, audioReadingId, deploymentName, readingTime);
            return reading;
        }

        /// <summary>
        /// Get audio reading using audioreadingid OR deployment and recording time.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        /// <param name="audioReadingId">
        /// The audio Reading Id.
        /// </param>
        /// <param name="deploymentName">
        /// The deployment Name.
        /// </param>
        /// <param name="readingTime">
        /// The reading Time.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// <c>NotSupportedException</c>.
        /// </exception>
        /// <returns>
        /// Requested audio reading or null;
        /// </returns>
        private static AudioReading GetAudioReading(
            QutSensorsDb db, Guid? audioReadingId, string deploymentName, DateTime readingTime)
        {
            AudioReading reading = null;

            if (audioReadingId.HasValue && audioReadingId.Value != Guid.Empty)
            {
                IQueryable<AudioReading> query = from r in db.AudioReadings
                                                 where r.AudioReadingID == audioReadingId
                                                 select r;

                reading = query.FirstOrDefault();
                if (reading != null)
                {
                    return reading;
                }
            }

            if (!string.IsNullOrEmpty(deploymentName) && readingTime != DateTime.MaxValue &&
                readingTime != DateTime.MinValue)
            {
                var query = from r in db.AudioReadings
                            where r.Time == readingTime && r.Deployment.Name == deploymentName
                            select r;

                if (query.Count() > 1)
                {
                    throw new NotSupportedException(
                        "There is more than one audio reading in deployment '" + deploymentName +
                        "' with the given start time: " + readingTime);
                }

                reading = query.FirstOrDefault();

                if (reading != null)
                {
                    return reading;
                }
            }

            return null;
        }





        #region Querystring Helpers

        /// <summary>
        /// The get querystring.
        /// </summary>
        /// <param name="querystrings">
        /// The querystrings.
        /// </param>
        /// <param name="querystringName">
        /// The querystring name.
        /// </param>
        /// <returns>
        /// The get querystring.
        /// </returns>
        public static string GetQuerystring(NameValueCollection querystrings, string querystringName)
        {
            if (querystrings != null && !string.IsNullOrEmpty(querystringName))
            {
                try
                {
                    return querystrings[querystringName];
                }
                catch
                {
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// The to query string.
        /// </summary>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <returns>
        /// The to query string.
        /// </returns>
        public static string ToQueryString(this Dictionary<string, object> items)
        {
            if (items == null || !items.Any())
            {
                return string.Empty;
            }

            return items.Select(a => a.Key + "=" + a.Value).Aggregate((a, b) => a + "+" + b);
        }

        #endregion

        #region Website Only Helpers

        private static bool? shouldLogDeviceWebServiceCalls;

        /// <summary>
        /// Gets a value indicating whether ShouldLogDeviceWebServiceCalls.
        /// </summary>
        public static bool ShouldLogDeviceWebServiceCalls
        {
            get
            {
                // protect against configuration input errors
                try
                {
                    if (shouldLogDeviceWebServiceCalls == null)
                    {
                        string o = ConfigurationManager.AppSettings["ShouldLogDeviceWebServiceCalls"];
                        shouldLogDeviceWebServiceCalls = o == null ? false : Convert.ToBoolean(o);
                    }

                    return shouldLogDeviceWebServiceCalls.Value;
                }
                catch (Exception e)
                {
                    LogError(e);
                    return false;
                }
            }
        }

        /// <summary>
        /// The log error.
        /// </summary>
        /// <param name="ex">
        /// The ex.
        /// </param>
        /// <returns>
        /// The log error.
        /// </returns>
        [MoveToBusiness("No reason for this to be here")]
        public static int LogError(Exception ex)
        {
            return ErrorLog.Insert(ex, HttpContext.Current.Request.Url.ToString());
        }



        /// <summary>
        /// A standardised way to report errors in WCF services.
        /// </summary>
        /// <param name="exception">
        /// The exception to log.
        /// </param>
        /// <param name="errorMessage">
        /// The error message to log.
        /// </param>
        /// <param name="httpContext">
        /// The http Context.
        /// </param>
        public static void HttpErrorHandler(Exception exception, string errorMessage, HttpContext httpContext)
        {
            int errNumber = ErrorLog.Insert(exception, errorMessage);
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            httpContext.Response.Headers.Add("x-Error", string.Format("An error occured ({0})", errNumber));
        }

        /// <summary>
        /// The to absolute with server.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <param name="url">
        /// virtual Path.
        /// </param>
        /// <returns>
        /// The to absolute with server.
        /// </returns>
        public static string ToAbsoluteWithServer(HttpRequest request, string virtualPath)
        {
            if (request.Url.Port != 80)
            {
                return request.Url.Scheme + "://" + request.Url.Host + ":" + request.Url.Port +
                       VirtualPathUtility.ToAbsolute(virtualPath);
            }
            else
            {
                return request.Url.Scheme + "://" + request.Url.Host + VirtualPathUtility.ToAbsolute(virtualPath);
            }
        }

        /// <summary>
        /// The get control.
        /// </summary>
        /// <param name="row">
        /// The row.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// </returns>
        public static T GetControl<T>(GridViewRow row, string id) where T : class
        {
            foreach (TableCell cell in row.Cells)
            {
                Control c = cell.FindControl(id);
                if (c != null)
                {
                    return c as T;
                }
            }

            return null;
        }

        /// <summary>
        /// Create a dropdownlist containing numbers from 0 to <paramref name="maxNum"/>.
        /// </summary>
        /// <param name="id">
        /// Drop down list id.
        /// </param>
        /// <param name="maxNum">
        /// The max num.
        /// </param>
        /// <returns>
        /// Drop Down list.
        /// </returns>
        public static DropDownList CreateDdl(string id, int maxNum)
        {
            var ddl = new DropDownList();
            ddl.ID = id;

            for (int count = 0; count < 60; count++)
            {
                string str = count.ToString();
                if (str.Length == 0)
                {
                    str = "00";
                }

                if (str.Length == 1)
                {
                    str = "0" + str;
                }

                if (count < maxNum)
                {
                    ddl.Items.Add(new ListItem(str, str));
                }
            }

            return ddl;
        }

        /// <summary>
        /// Create a dropdownlist containing numbers from 0 to <paramref name="maxNum"/>. <paramref name="selectText"/> will be at the start.
        /// </summary>
        /// <param name="id">
        /// Drop down list id.
        /// </param>
        /// <param name="maxNum">
        /// The max num.
        /// </param>
        /// <param name="selectText">
        /// The select Text.
        /// </param>
        /// <returns>
        /// Drop Down list.
        /// </returns>
        public static IEnumerable<ListItem> CreateListItems(int maxNum, string selectText)
        {
            var items = new List<ListItem>();

            items.Add(new ListItem(selectText, string.IsNullOrEmpty(selectText) ? "Select Prompt" : selectText));

            for (int count = 0; count < 60; count++)
            {
                string str = count.ToString();
                if (str.Length == 0)
                {
                    str = "00";
                }

                if (str.Length == 1)
                {
                    str = "0" + str;
                }

                if (count < maxNum)
                {
                    items.Add(new ListItem(str, str));
                }
            }

            return items;
        }



        #endregion
    }
}