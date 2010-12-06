// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UrlUtilities.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   This class contains utilities for calculating certain urls in the application, particularly for
//   attaching to classes from other assemblies
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using QutSensors.Data.Linq;
using QutSensors.Shared;
using QutSensors.UI.Display.Classes;

namespace QutSensors.UI.Display
{
    /// <summary>
    /// This class contains utilities for calculating certain urls in the application, particularly for 
    /// attaching to classes from other assemblies.
    /// </summary>
    public static class UrlUtilities
    {
        public static string GetDataUrl(this ReadingsFilter filter)
        {
            var queryString = filter == null ? string.Empty : filter.ToQueryString(true);
            if (!string.IsNullOrEmpty(queryString))
            {
                queryString = "?" + queryString;
            }

            return "~/UI/AudioReading/AudioReadingData.aspx" + queryString;
        }

        public static string GetViewUrl(this Processor_Job job)
        {
            return "~/Processor/ViewJob.aspx?JobID=" + job.JobID;
        }

        public static string GetViewUrl(this AudioReading reading)
        {
            // Currently this calls to the old Utilities class. The aim is to migrate all Url generation
            // utilities to this class instead.
            return GetAudioReadingHRef(reading, "aspx", false);
        }

        public static string GetViewUrl(this AudioReading reading, int? start)
        {
            if (start == null)
                return GetViewUrl(reading);
            return GetAudioReadingHRef(reading, "aspx?Offset=" + start.Value, false);
        }

        public static string GetSpectrogramUrl(this AudioReading reading, long? start, long? end, bool resolve)
        {
            var retVal = GetAudioReadingHRef(reading, "jpg", false);
            if (resolve)
                retVal = VirtualPathUtility.ToAbsolute(retVal);
            var queryString = new Dictionary<string, string>();
            if (start != null)
                queryString.Add("start", start.ToString());
            if (end != null)
                queryString.Add("end", end.ToString());
            if (queryString.Count > 0)
                retVal += "?" + queryString.ToUrlParameterString(true);
            return retVal;
        }

        public static string GetSpectrogramUrl(this AudioReading reading, long? start, long? end, bool? isWaveForm, int? height, double? ppms, bool resolve)
        {
            var retVal = GetAudioReadingHRef(reading, "jpg", false);
            if (resolve)
                retVal = VirtualPathUtility.ToAbsolute(retVal);
            var queryString = new Dictionary<string, string>();
            if (start != null)
                queryString.Add("start", start.ToString());
            if (end != null)
                queryString.Add("end", end.ToString());
            if (isWaveForm != null)
                queryString.Add("isWaveForm", isWaveForm.ToString());
            if (height != null)
                queryString.Add("height", height.ToString());
            if (ppms != null)
                queryString.Add("ppms", ppms.ToString());
            if (queryString.Count > 0)
                retVal += "?" + queryString.ToUrlParameterString(true);
            return retVal;
        }

        public static string GetSpectrogramUrl(this AudioReadingInfo reading, long? start, long? end, bool resolve)
        {
            var retVal = GetAudioReadingHRef(reading, "jpg", false);
            if (resolve)
                retVal = VirtualPathUtility.ToAbsolute(retVal);
            var queryString = new Dictionary<string, string>();
            if (start != null)
                queryString.Add("start", start.ToString());
            if (end != null)
                queryString.Add("end", end.ToString());
            if (queryString.Count > 0)
                retVal += "?" + queryString.ToUrlParameterString(true);
            return retVal;
        }

        public static string GetSensorDetailsHRef(string deploymentName)
        {
            using (var db = new QutSensorsDb())
            {
                var entityDep =
                    db.Entity_Items.Where(
                        ei =>
                        ei.Entity_MetaData.DeploymentID.HasValue && ei.Entity_MetaData.Deployment.Name == deploymentName).
                        FirstOrDefault();
                if (entityDep != null)
                {
                    return @"~/UI/Deployment/DeploymentModify.aspx?EntityDeploymentId=" + entityDep.EntityID;
                }
            }

            return null;
        }

        public static string GetSensorDetailsHRef(this Deployment deployment)
        {
            if (deployment == null)
                return null;
            return GetSensorDetailsHRef(deployment.Name);
        }

        public static string GetSensorDetailsHRef(this Deployment deployment, DateTime date, bool absolute)
        {
            if (deployment == null)
                return null;

            using (var db = new QutSensorsDb())
            {
                var hardware =
                    db.Deployments.Where(d => d.DeploymentID == deployment.DeploymentID)
                        .FirstOrDefault();

                if (hardware != null)
                {
                    var retVal = @"~/UI/Hardware/HardwareModify.aspx";

                    if (absolute)
                        retVal = VirtualPathUtility.ToAbsolute(retVal);

                    return retVal + "?HardwareId=" + hardware.HardwareID + "&Date=" + date.Date.ToString("yyyy-MM-dd");
                }
            }

            return null;
        }

        public static string GetSensorDataHRef(this Deployment deployment)
        {
            if (deployment == null)
                return null;

            using (var db = new QutSensorsDb())
            {
                var hardware =
                    db.Deployments.Where(d => d.DeploymentID == deployment.DeploymentID)
                        .FirstOrDefault();

                if (hardware != null)
                {
                    return @"~/UI/Hardware/HardwareModify.aspx?HardwareId=" + hardware.HardwareID;
                }
            }

            return null;
        }

        public static string GetSensorAHQIHRef(this Deployment deployment)
        {
            if (deployment == null)
                return null;

            return "~/AHQI.ashx?Deployment=" + HttpUtility.UrlPathEncode(deployment.Name);
        }

        public static string GetResultUrl(this Processor_Result result)
        {
            return "~/Processor/JobResults.aspx?JobID=" + result.Processor_JobItem.JobID;
        }

        public static string GetAudioReadingHRef(this AudioReadingInfo reading, string extension, bool useNoiseReduction)
        {
            if (reading == null) return string.Empty;
            string deploymentName = reading.DeploymentName;
            var arId = reading.AudioReadingID;
            var time = reading.Time;

            var url = GetAudioReadingHRef(deploymentName, arId, time, extension, useNoiseReduction, null, null, false);
            return url;
        }

        public static string GetAudioReadingHRef(this AudioReading reading, string extension, bool useNoiseReduction)
        {
            if (reading == null) return string.Empty;
            string deploymentName = reading.Deployment == null ? null : reading.Deployment.Name;
            var arId = reading.AudioReadingID;
            var time = reading.Time;

            var url = GetAudioReadingHRef(deploymentName, arId, time, extension, useNoiseReduction, null, null, false);
            return url;
        }

        public static string GetAudioReadingHRef(this AudioReading reading, string extension, bool useNoiseReduction, long? start, long? end)
        {
            if (reading == null) return string.Empty;
            string deploymentName = reading.Deployment == null ? null : reading.Deployment.Name;
            var arId = reading.AudioReadingID;
            var time = reading.Time;

            var url = GetAudioReadingHRef(deploymentName, arId, time, extension, useNoiseReduction, start, end, false);
            return url;
        }

        public static string GetAudioReadingHRef(string deploymentName, Guid audioReadingId, DateTime time)
        {
            return GetAudioReadingHRef(deploymentName, audioReadingId, time, "aspx", false, null, null, false);
        }

        public static string GetAudioReadingHRef(string deploymentName, Guid id, DateTime time, string extension, bool useNoiseReduction, long? start, long? end, bool doNotResolveAsApplicationPath)
        {
            string s = ((doNotResolveAsApplicationPath) ? string.Empty : "~");
            if (deploymentName == null)
            {
                return s + "/sensors/ByID/" + id + "." + extension;
            }

            string inlineValue = useNoiseReduction ? "noise_reduced/" : string.Empty;
            string e = string.IsNullOrEmpty(extension) ? string.Empty : "." + extension;
            string queryStrings = string.Empty;
            if (start.HasValue)
            {
                queryStrings += "&start=" + start;
            }
            if (end.HasValue)
            {
                queryStrings += "&end=" + end;
            }
            if (queryStrings.Length > 0)
            {
                queryStrings = '?' + queryStrings.Substring(1); //get rid of the & char 
            }
            return s + "/sensors/" +
                   inlineValue +
                   HttpUtility.UrlEncode(deploymentName) + "/" +
                   HttpUtility.UrlEncode(time.ToString(Utilities.ReadingsDateFormat)) +
                   e + queryStrings;
        }

        /// <summary>
        /// VirtualPathUtility.ToAbsolute cannot handle a querystring. 
        /// Uses VirtualPathUtility.ToAbsolute, but trims off the querystring and adds it afterwards.
        /// </summary>
        /// <param name="path">
        /// Path to convert.
        /// </param>
        /// <param name="root">
        /// Application root.
        /// </param>
        /// <returns>Application absolute path.
        /// </returns>
        /// <remarks>
        /// It should be noted, that VirtualPathUtility.ToAbsolute() will choke and die when trying to resolve a url with querystring parameters..
        /// In other words, something like this: ~/Handlers/Imagehander.ashx?id=432432 when passed in will fail miserably.
        /// We use a custom PathHelper class to wrap VirtualPathUtility.. 
        /// Basically, it splits off the query string params, translates the root of the url and re-appends the split query string.
        /// </remarks>
        public static string ToAbsolute(string path, string root)
        {
            if (path.IndexOf('?') == -1)
            {
                path = VirtualPathUtility.ToAbsolute(path);
            }
            else
            {
                path = VirtualPathUtility.ToAbsolute(
                    path.Substring(0, path.IndexOf('?')), root) + path.Substring(path.IndexOf('?'));
            }

            return path;
        }

        /// <summary>
        /// VirtualPathUtility.ToAbsolute cannot handle a querystring. 
        /// Uses VirtualPathUtility.ToAbsolute, but trims off the querystring and adds it afterwards.
        /// </summary>
        /// <param name="path">
        /// Path to convert.
        /// </param>
        /// <returns>Application absolute path.
        /// </returns>
        /// <remarks>
        /// It should be noted, that VirtualPathUtility.ToAbsolute() will choke and die when trying to resolve a url with querystring parameters..
        /// In other words, something like this: ~/Handlers/Imagehander.ashx?id=432432 when passed in will fail miserably.
        /// We use a custom PathHelper class to wrap VirtualPathUtility.. 
        /// Basically, it splits off the query string params, translates the root of the url and re-appends the split query string.
        /// </remarks>
        public static string ToAbsolute(this string path)
        {
            if (path.IndexOf('?') == -1)
            {
                path = VirtualPathUtility.ToAbsolute(path);
            }
            else
            {
                path = VirtualPathUtility.ToAbsolute(
                    path.Substring(0, path.IndexOf('?'))) + path.Substring(path.IndexOf('?'));
            }

            return path;
        }
    }
}
