// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UrlUtilities.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using AudioDataPrepareLocalAccessConsole;

    using QutSensors.Data.Linq;
    using QutSensors.Shared;
    using QutSensors.UI.Display.Classes;

    /// <summary>
    /// This class contains utilities for calculating certain urls in the application, particularly for 
    /// attaching to classes from other assemblies.
    /// </summary>
    public static class UrlUtilities
    {
        #region Public Methods

        /// <summary>
        /// The get audio reading h ref.
        /// </summary>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <param name="extension">
        /// The extension.
        /// </param>
        /// <param name="useNoiseReduction">
        /// The use noise reduction.
        /// </param>
        /// <returns>
        /// The get audio reading h ref.
        /// </returns>
        public static string GetAudioReadingHRef(
            this AudioReadingInfo reading, string extension, bool useNoiseReduction)
        {
            if (reading == null)
            {
                return string.Empty;
            }

            string deploymentName = reading.DeploymentName;
            Guid arId = reading.AudioReadingID;
            DateTime time = reading.Time;

            string url = GetAudioReadingHRef(
                deploymentName, arId, time, extension, useNoiseReduction, null, null, false);
            return url;
        }

        /// <summary>
        /// The get audio reading h ref.
        /// </summary>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <param name="extension">
        /// The extension.
        /// </param>
        /// <param name="useNoiseReduction">
        /// The use noise reduction.
        /// </param>
        /// <returns>
        /// The get audio reading h ref.
        /// </returns>
        public static string GetAudioReadingHRef(this AudioReading reading, string extension, bool useNoiseReduction)
        {
            if (reading == null)
            {
                return string.Empty;
            }

            string deploymentName = reading.Deployment == null ? null : reading.Deployment.Name;
            Guid arId = reading.AudioReadingID;
            DateTime time = reading.Time;

            string url = GetAudioReadingHRef(
                deploymentName, arId, time, extension, useNoiseReduction, null, null, false);
            return url;
        }

        /// <summary>
        /// The get audio reading h ref.
        /// </summary>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <param name="extension">
        /// The extension.
        /// </param>
        /// <param name="useNoiseReduction">
        /// The use noise reduction.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="end">
        /// The end.
        /// </param>
        /// <returns>
        /// The get audio reading h ref.
        /// </returns>
        public static string GetAudioReadingHRef(
            this AudioReading reading, string extension, bool useNoiseReduction, long? start, long? end)
        {
            if (reading == null)
            {
                return string.Empty;
            }

            string deploymentName = reading.Deployment == null ? null : reading.Deployment.Name;
            Guid arId = reading.AudioReadingID;
            DateTime time = reading.Time;

            string url = GetAudioReadingHRef(
                deploymentName, arId, time, extension, useNoiseReduction, start, end, false);
            return url;
        }

        /// <summary>
        /// The get audio reading h ref.
        /// </summary>
        /// <param name="deploymentName">
        /// The deployment name.
        /// </param>
        /// <param name="audioReadingId">
        /// The audio reading id.
        /// </param>
        /// <param name="time">
        /// The time.
        /// </param>
        /// <returns>
        /// The get audio reading h ref.
        /// </returns>
        public static string GetAudioReadingHRef(string deploymentName, Guid audioReadingId, DateTime time)
        {
            return GetAudioReadingHRef(deploymentName, audioReadingId, time, "aspx", false, null, null, false);
        }

        /// <summary>
        /// The get audio reading h ref.
        /// </summary>
        /// <param name="deploymentName">
        /// The deployment name.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="time">
        /// The time.
        /// </param>
        /// <param name="extension">
        /// The extension.
        /// </param>
        /// <param name="useNoiseReduction">
        /// The use noise reduction.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="end">
        /// The end.
        /// </param>
        /// <param name="doNotResolveAsApplicationPath">
        /// The do not resolve as application path.
        /// </param>
        /// <returns>
        /// The get audio reading h ref.
        /// </returns>
        public static string GetAudioReadingHRef(
            string deploymentName, 
            Guid id, 
            DateTime time, 
            string extension, 
            bool useNoiseReduction, 
            long? start, 
            long? end, 
            bool doNotResolveAsApplicationPath)
        {
            string s = (doNotResolveAsApplicationPath) ? string.Empty : "~";
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
                queryStrings = '?' + queryStrings.Substring(1); // get rid of the & char 
            }

            return s + "/sensors/" + inlineValue + HttpUtility.UrlEncode(deploymentName) + "/" +
                   HttpUtility.UrlEncode(time.ToString(Utilities.ReadingsDateFormat)) + e + queryStrings;
        }

        /// <summary>
        /// Generates the Url the player should use to download audio.
        /// Importantly it will contain no query strings - to ensure the request is cacheable.
        /// </summary>
        /// <param name="id">
        /// </param>
        /// <param name="start">
        /// </param>
        /// <param name="end">
        /// </param>
        /// <returns>
        /// The get audio reading h ref for dadi.
        /// </returns>
        public static string GetAudioReadingHRefForDadi(Guid id, long start, long end)
        {
            return "/sensors/ByID/" +
                   AudioFileUtil.GetAudioFileSegmentName(
                       id, 
                       TimeSpan.FromTicks(start * TimeSpan.TicksPerMillisecond), 
                       TimeSpan.FromTicks(end * TimeSpan.TicksPerMillisecond));
        }

        /// <summary>
        /// Generates the Url the player should use to download audio. This is designed to generate urls with replaceable strings.
        /// Importantly it will contain no query strings - to ensure the request is cacheable.
        /// </summary>
        /// <param name="id">
        /// </param>
        /// <param name="start">
        /// </param>
        /// <param name="end">
        /// </param>
        /// <returns>
        /// The get audio reading h ref for dadi.
        /// </returns>
        public static string GetAudioReadingHRefForDadi(string id, string start, string end)
        {
            return "/sensors/ByID/" + AudioFileUtil.GetAudioFileSegmentNameFormat(id, start, end);
        }

        /// <summary>
        /// The get data url.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The get data url.
        /// </returns>
        public static string GetDataUrl(this ReadingsFilter filter)
        {
            string queryString = filter == null ? string.Empty : filter.ToQueryString(true);
            if (!string.IsNullOrEmpty(queryString))
            {
                queryString = "?" + queryString;
            }

            return "~/UI/AudioReading/AudioReadingData.aspx" + queryString;
        }

        /// <summary>
        /// The get result url.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// The get result url.
        /// </returns>
        public static string GetResultUrl(this Processor_Result result)
        {
            return "~/Processor/JobResults.aspx?JobID=" + result.Processor_JobItem.JobID;
        }

        /// <summary>
        /// The get sensor ahqih ref.
        /// </summary>
        /// <param name="deployment">
        /// The deployment.
        /// </param>
        /// <returns>
        /// The get sensor ahqih ref.
        /// </returns>
        public static string GetSensorAHQIHRef(this Deployment deployment)
        {
            if (deployment == null)
            {
                return null;
            }

            return "~/AHQI.ashx?Deployment=" + HttpUtility.UrlPathEncode(deployment.Name);
        }

        /// <summary>
        /// The get sensor data h ref.
        /// </summary>
        /// <param name="deployment">
        /// The deployment.
        /// </param>
        /// <returns>
        /// The get sensor data h ref.
        /// </returns>
        public static string GetSensorDataHRef(this Deployment deployment)
        {
            if (deployment == null)
            {
                return null;
            }

            using (var db = new QutSensorsDb())
            {
                Deployment hardware =
                    db.Deployments.Where(d => d.DeploymentID == deployment.DeploymentID).FirstOrDefault();

                if (hardware != null)
                {
                    return @"~/UI/Hardware/HardwareModify.aspx?HardwareId=" + hardware.HardwareID;
                }
            }

            return null;
        }

        /// <summary>
        /// The get sensor details h ref.
        /// </summary>
        /// <param name="deploymentName">
        /// The deployment name.
        /// </param>
        /// <returns>
        /// The get sensor details h ref.
        /// </returns>
        public static string GetSensorDetailsHRef(string deploymentName)
        {
            using (var db = new QutSensorsDb())
            {
                Entity_Item entityDep =
                    db.Entity_Items.Where(
                        ei =>
                        ei.Entity_MetaData.DeploymentID.HasValue && ei.Entity_MetaData.Deployment.Name == deploymentName)
                        .FirstOrDefault();
                if (entityDep != null)
                {
                    return @"~/UI/Deployment/DeploymentModify.aspx?EntityDeploymentId=" + entityDep.EntityID;
                }
            }

            return null;
        }

        /// <summary>
        /// The get sensor details h ref.
        /// </summary>
        /// <param name="deployment">
        /// The deployment.
        /// </param>
        /// <returns>
        /// The get sensor details h ref.
        /// </returns>
        public static string GetSensorDetailsHRef(this Deployment deployment)
        {
            if (deployment == null)
            {
                return null;
            }

            return GetSensorDetailsHRef(deployment.Name);
        }

        /// <summary>
        /// The get sensor details h ref.
        /// </summary>
        /// <param name="deployment">
        /// The deployment.
        /// </param>
        /// <param name="date">
        /// The date.
        /// </param>
        /// <param name="absolute">
        /// The absolute.
        /// </param>
        /// <returns>
        /// The get sensor details h ref.
        /// </returns>
        public static string GetSensorDetailsHRef(this Deployment deployment, DateTime date, bool absolute)
        {
            if (deployment == null)
            {
                return null;
            }

            using (var db = new QutSensorsDb())
            {
                Deployment hardware =
                    db.Deployments.Where(d => d.DeploymentID == deployment.DeploymentID).FirstOrDefault();

                if (hardware != null)
                {
                    string retVal = @"~/UI/Hardware/HardwareModify.aspx";

                    if (absolute)
                    {
                        retVal = VirtualPathUtility.ToAbsolute(retVal);
                    }

                    return retVal + "?HardwareId=" + hardware.HardwareID + "&Date=" + date.Date.ToString("yyyy-MM-dd");
                }
            }

            return null;
        }

        /// <summary>
        /// The get spectrogram url.
        /// </summary>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="end">
        /// The end.
        /// </param>
        /// <param name="resolve">
        /// The resolve.
        /// </param>
        /// <returns>
        /// The get spectrogram url.
        /// </returns>
        public static string GetSpectrogramUrl(this AudioReading reading, long? start, long? end, bool resolve)
        {
            string retVal = GetAudioReadingHRef(reading, "jpg", false);
            if (resolve)
            {
                retVal = VirtualPathUtility.ToAbsolute(retVal);
            }

            var queryString = new Dictionary<string, string>();
            if (start != null)
            {
                queryString.Add("start", start.ToString());
            }

            if (end != null)
            {
                queryString.Add("end", end.ToString());
            }

            if (queryString.Count > 0)
            {
                retVal += "?" + queryString.ToUrlParameterString(true);
            }

            return retVal;
        }

        /// <summary>
        /// The get spectrogram url.
        /// </summary>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="end">
        /// The end.
        /// </param>
        /// <param name="isWaveForm">
        /// The is wave form.
        /// </param>
        /// <param name="height">
        /// The height.
        /// </param>
        /// <param name="ppms">
        /// The ppms.
        /// </param>
        /// <param name="resolve">
        /// The resolve.
        /// </param>
        /// <returns>
        /// The get spectrogram url.
        /// </returns>
        public static string GetSpectrogramUrl(
            this AudioReading reading, long? start, long? end, bool? isWaveForm, int? height, double? ppms, bool resolve)
        {
            string retVal = GetAudioReadingHRef(reading, "jpg", false);
            if (resolve)
            {
                retVal = VirtualPathUtility.ToAbsolute(retVal);
            }

            var queryString = new Dictionary<string, string>();
            if (start != null)
            {
                queryString.Add("start", start.ToString());
            }

            if (end != null)
            {
                queryString.Add("end", end.ToString());
            }

            if (isWaveForm != null)
            {
                queryString.Add("isWaveForm", isWaveForm.ToString());
            }

            if (height != null)
            {
                queryString.Add("height", height.ToString());
            }

            if (ppms != null)
            {
                queryString.Add("ppms", ppms.ToString());
            }

            if (queryString.Count > 0)
            {
                retVal += "?" + queryString.ToUrlParameterString(true);
            }

            return retVal;
        }

        /// <summary>
        /// The get spectrogram url.
        /// </summary>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="end">
        /// The end.
        /// </param>
        /// <param name="resolve">
        /// The resolve.
        /// </param>
        /// <returns>
        /// The get spectrogram url.
        /// </returns>
        public static string GetSpectrogramUrl(this AudioReadingInfo reading, long? start, long? end, bool resolve)
        {
            string retVal = GetAudioReadingHRef(reading, "jpg", false);
            if (resolve)
            {
                retVal = VirtualPathUtility.ToAbsolute(retVal);
            }

            var queryString = new Dictionary<string, string>();
            if (start != null)
            {
                queryString.Add("start", start.ToString());
            }

            if (end != null)
            {
                queryString.Add("end", end.ToString());
            }

            if (queryString.Count > 0)
            {
                retVal += "?" + queryString.ToUrlParameterString(true);
            }

            return retVal;
        }

        /// <summary>
        /// The get view url.
        /// </summary>
        /// <param name="job">
        /// The job.
        /// </param>
        /// <returns>
        /// The get view url.
        /// </returns>
        public static string GetViewUrl(this Processor_Job job)
        {
            return "~/Processor/ViewJob.aspx?JobID=" + job.JobID;
        }

        /// <summary>
        /// The get view url.
        /// </summary>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <returns>
        /// The get view url.
        /// </returns>
        public static string GetViewUrl(this AudioReading reading)
        {
            // Currently this calls to the old Utilities class. The aim is to migrate all Url generation
            // utilities to this class instead.
            return GetAudioReadingHRef(reading, "aspx", false);
        }

        /// <summary>
        /// The get view url.
        /// </summary>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <returns>
        /// The get view url.
        /// </returns>
        public static string GetViewUrl(this AudioReading reading, int? start)
        {
            if (start == null)
            {
                return GetViewUrl(reading);
            }

            return GetAudioReadingHRef(reading, "aspx?Offset=" + start.Value, false);
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
        /// <returns>
        /// Application absolute path.
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
                path = VirtualPathUtility.ToAbsolute(path.Substring(0, path.IndexOf('?')), root) +
                       path.Substring(path.IndexOf('?'));
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
        /// <returns>
        /// Application absolute path.
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
                path = VirtualPathUtility.ToAbsolute(path.Substring(0, path.IndexOf('?'))) +
                       path.Substring(path.IndexOf('?'));
            }

            return path;
        }

        #endregion
    }
}