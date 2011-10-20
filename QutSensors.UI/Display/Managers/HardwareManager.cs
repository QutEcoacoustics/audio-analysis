// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HardwareManager.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml.XPath;

    using QutSensors.Data;
    using QutSensors.Data.Linq;
    using QutSensors.Shared;
    using QutSensors.UI.Display.Classes;

    /// <summary>
    /// The hardware manager.
    /// </summary>
    public static class HardwareManager
    {
        /// <summary>
        /// The get site hardware.
        /// </summary>
        /// <param name="maxItems">
        /// The max Items.
        /// </param>
        /// <param name="startIndex">
        /// The start Index.
        /// </param>
        /// <param name="sortExpression">
        /// The sort Expression.
        /// </param>
        /// <param name="projectId">
        /// The project id.
        /// </param>
        /// <param name="siteId">
        /// The site id.
        /// </param>
        /// <returns>
        /// Site hardware list.
        /// </returns>
        public static IEnumerable<SiteHardware> GetSiteAutoSensors(int maxItems, int startIndex, string sortExpression, int projectId, int siteId)
        {
            var siteAutoSensors = new List<SiteHardware>();
            using (var db = new QutSensorsDb())
            {
                var results = EntityManager.Instance.GetDescendants(db, siteId, a => a.Entity_MetaData.DeploymentID.HasValue).
                        Where(ei => !ei.Entity_MetaData.Deployment.Hardware.IsManualDevice).Select(
                            ei => ei.Entity_MetaData.Deployment);


                switch (sortExpression)
                {
                    case "Name":
                        results = results.OrderBy(d => d.Name);
                        break;
                    case "Name DESC":
                        results = results.OrderByDescending(d => d.Name);
                        break;
                    case "MostRecentRecording":
                        results = results.OrderBy(d => db.AudioReadings.Where(ar => ar.DeploymentID == d.DeploymentID).Count() > 0 ? db.AudioReadings.Where(ar => ar.DeploymentID == d.DeploymentID).Max(ar => ar.Time) : new DateTime?());
                        break;
                    case "MostRecentRecording DESC":
                        results = results.OrderByDescending(d => db.AudioReadings.Where(ar => ar.DeploymentID == d.DeploymentID).Count() > 0 ? db.AudioReadings.Where(ar => ar.DeploymentID == d.DeploymentID).Max(ar => ar.Time) : new DateTime?());
                        break;
                    case "IsActive":
                        results = results.OrderBy(d => d.IsActive);
                        break;
                    case "IsActive DESC":
                        results = results.OrderByDescending(d => d.IsActive);
                        break;
                    case "IsTest":
                        results = results.OrderBy(d => d.IsActive);
                        break;
                    case "IsTest DESC":
                        results = results.OrderByDescending(d => d.IsActive);
                        break;
                    case "LastContacted":
                        results = results.OrderBy(d => d.Hardware.LastContacted);
                        break;
                    case "LastContacted DESC":
                        results = results.OrderByDescending(d => d.Hardware.LastContacted);
                        break;
                    case "Started":
                        results = results.OrderBy(d => d.DateStarted);
                        break;
                    case "Started DESC":
                        results = results.OrderByDescending(d => d.DateStarted);
                        break;
                    case "Ended":
                        results = results.OrderBy(d => d.DateEnded);
                        break;
                    case "Ended DESC":
                        results = results.OrderByDescending(d => d.DateEnded);
                        break;
                    default:
                        results = results.OrderBy(d => d.Name);
                        break;
                }

                var depList = results.Skip(startIndex).Take(maxItems).ToList();

                foreach (var dep in depList)
                {
                    var deployment = dep;
                    var audioReadingCount =
                        db.AudioReadings.Where(ar => ar.DeploymentID == deployment.DeploymentID).Count();
                    var hli = new SiteHardware
                    {
                        ProjectId = projectId,
                        SiteId = siteId,
                        HardwareId = deployment.HardwareID,
                        AudioReadingsCount = audioReadingCount,
                        LastContacted =
                            deployment.Hardware.LastContacted.HasValue
                                ? deployment.Hardware.LastContacted.Value.ToString("ddd, d MMM yyyy HH:mm:ss")
                                : "never",
                    };

                    if (audioReadingCount > 0)
                    {
                        hli.MostRecentRecording = db.AudioReadings.Where(ar => ar.DeploymentID == deployment.DeploymentID).Max(ar => ar.Time).ToString(
                            "ddd, d MMM yyyy HH:mm:ss");
                    }
                    else
                    {
                        hli.MostRecentRecording = "no audio";
                    }

                    // deployment info
                    hli.CurrentDeploymentName = deployment.Name;
                    hli.CurrentDeploymentId = deployment.DeploymentID;
                    hli.CurrentDeploymentStarted = deployment.DateStarted.ToDifferenceString(DateTime.Now);
                    hli.CurrentDeploymentEnded = deployment.DateEnded.HasValue ? deployment.DateEnded.Value.ToDifferenceString(DateTime.Now) : "never";
                    hli.IsTest = deployment.IsTest ? "Yes" : String.Empty;
                    hli.IsActive = deployment.IsActive ? "Yes" : String.Empty;

                    Entity_Item entity = EntityManager.Instance.GetDeploymentEntity(db, deployment.DeploymentID);
                    if (entity != null)
                    {
                        hli.CurrentDeploymentEntityId = entity.EntityID;
                    }


                    // device status info
                    DeviceStatus status = db.DeviceStatus.Where(ds => ds.HardwareID == deployment.HardwareID).OrderByDescending(s => s.Time).FirstOrDefault();
                    if (status != null)
                    {
                        // Freememory is in kb.
                        hli.FreeMemory = status.FreeMemory.HasValue ? (status.FreeMemory.Value / 1024).ToByteDisplay() : "--";
                        hli.PowerLevel = status.PowerLevel.HasValue ? status.PowerLevel.Value + "%" : "--";
                        hli.MemoryUsage = status.MemoryUsage.HasValue ? status.MemoryUsage.Value + "%" : "--";
                    }

                    siteAutoSensors.Add(hli);
                }
            }

            return siteAutoSensors;
        }

        /// <summary>
        /// Get count of site auto sensors for project and site.
        /// </summary>
        /// <param name="projectId">
        /// The project id.
        /// </param>
        /// <param name="siteId">
        /// The site id.
        /// </param>
        /// <returns>Number of active auto sensors in site.
        /// </returns>
        public static int GetSiteAutoSensorsCount(int projectId, int siteId)
        {
            return GetSiteAutoSensors(Int32.MaxValue, 0, String.Empty, projectId, siteId).Count();
        }

        /// <summary>
        /// The get site hardware.
        /// </summary>
        /// <param name="maxItems">
        /// The max Items.
        /// </param>
        /// <param name="startIndex">
        /// The start Index.
        /// </param>
        /// <param name="sortExpression">
        /// The sort Expression.
        /// </param>
        /// <param name="projectId">
        /// The project id.
        /// </param>
        /// <param name="siteId">
        /// The site id.
        /// </param>
        /// <returns>
        /// Site hardware list.
        /// </returns>
        public static IEnumerable<SiteHardware> GetSiteManualSensors(int maxItems, int startIndex, string sortExpression, int projectId, int siteId)
        {
            var siteManualSensors = new List<SiteHardware>();
            using (var db = new QutSensorsDb())
            {
                var results = EntityManager.Instance.GetDescendants(db, siteId, a => a.Entity_MetaData.DeploymentID.HasValue).
                        Where(ei => ei.Entity_MetaData.Deployment.Hardware.IsManualDevice).Select(
                            ei => ei.Entity_MetaData.Deployment);


                switch (sortExpression)
                {
                    case "Name":
                        results = results.OrderBy(d => d.Name);
                        break;
                    case "Name DESC":
                        results = results.OrderByDescending(d => d.Name);
                        break;
                    case "MostRecentRecording":
                        results = results.OrderBy(d => db.AudioReadings.Where(ar => ar.DeploymentID == d.DeploymentID).Count() > 0 ? db.AudioReadings.Where(ar => ar.DeploymentID == d.DeploymentID).Max(ar => ar.Time) : new DateTime?());
                        break;
                    case "MostRecentRecording DESC":
                        results = results.OrderByDescending(d => db.AudioReadings.Where(ar => ar.DeploymentID == d.DeploymentID).Count() > 0 ? db.AudioReadings.Where(ar => ar.DeploymentID == d.DeploymentID).Max(ar => ar.Time) : new DateTime?());
                        break;
                    case "IsActive":
                        results = results.OrderBy(d => d.IsActive);
                        break;
                    case "IsActive DESC":
                        results = results.OrderByDescending(d => d.IsActive);
                        break;
                    case "IsTest":
                        results = results.OrderBy(d => d.IsActive);
                        break;
                    case "IsTest DESC":
                        results = results.OrderByDescending(d => d.IsActive);
                        break;
                    case "LastContacted":
                        results = results.OrderBy(d => d.Hardware.LastContacted);
                        break;
                    case "LastContacted DESC":
                        results = results.OrderByDescending(d => d.Hardware.LastContacted);
                        break;
                    case "Started":
                        results = results.OrderBy(d => d.DateStarted);
                        break;
                    case "Started DESC":
                        results = results.OrderByDescending(d => d.DateStarted);
                        break;
                    case "Ended":
                        results = results.OrderBy(d => d.DateEnded);
                        break;
                    case "Ended DESC":
                        results = results.OrderByDescending(d => d.DateEnded);
                        break;
                    default:
                        results = results.OrderBy(d => d.Name);
                        break;
                }

                var depList = results.Skip(startIndex).Take(maxItems).ToList();

                foreach (var dep in depList)
                {
                    var deployment = dep;
                    var audioReadingCount =
                        db.AudioReadings.Where(ar => ar.DeploymentID == deployment.DeploymentID).Count();
                    var hli = new SiteHardware
                    {
                        ProjectId = projectId,
                        SiteId = siteId,
                        HardwareId = deployment.HardwareID,
                        AudioReadingsCount = audioReadingCount,
                        LastContacted =
                            deployment.Hardware.LastContacted.HasValue
                                ? deployment.Hardware.LastContacted.Value.ToString("ddd, d MMM yyyy HH:mm:ss")
                                : "never",
                    };

                    if (audioReadingCount > 0)
                    {
                        hli.MostRecentRecording = db.AudioReadings.Where(ar => ar.DeploymentID == deployment.DeploymentID).Max(ar => ar.Time).ToString(
                            "ddd, d MMM yyyy HH:mm:ss");
                    }
                    else
                    {
                        hli.MostRecentRecording = "no audio";
                    }

                    // deployment info
                    hli.CurrentDeploymentName = deployment.Name;
                    hli.CurrentDeploymentId = deployment.DeploymentID;
                    hli.CurrentDeploymentStarted = deployment.DateStarted.ToDifferenceString(DateTime.Now);
                    hli.CurrentDeploymentEnded = deployment.DateEnded.HasValue ? deployment.DateEnded.Value.ToDifferenceString(DateTime.Now) : "never";
                    hli.IsTest = deployment.IsTest ? "Yes" : String.Empty;
                    hli.IsActive = deployment.IsActive ? "Yes" : String.Empty;

                    Entity_Item entity = EntityManager.Instance.GetDeploymentEntity(db, deployment.DeploymentID);
                    if (entity != null)
                    {
                        hli.CurrentDeploymentEntityId = entity.EntityID;
                    }


                    // device status info
                    DeviceStatus status = db.DeviceStatus.Where(ds => ds.HardwareID == deployment.HardwareID).OrderByDescending(s => s.Time).FirstOrDefault();
                    if (status != null)
                    {
                        // Freememory is in kb.
                        hli.FreeMemory = status.FreeMemory.HasValue ? (status.FreeMemory.Value / 1024).ToByteDisplay() : "--";
                        hli.PowerLevel = status.PowerLevel.HasValue ? status.PowerLevel.Value + "%" : "--";
                        hli.MemoryUsage = status.MemoryUsage.HasValue ? status.MemoryUsage.Value + "%" : "--";
                    }

                    siteManualSensors.Add(hli);
                }
            }

            return siteManualSensors;
        }

        /// <summary>
        /// Get count of site auto sensors for project and site.
        /// </summary>
        /// <param name="projectId">
        /// The project id.
        /// </param>
        /// <param name="siteId">
        /// The site id.
        /// </param>
        /// <returns>Number of active auto sensors in site.
        /// </returns>
        public static int GetSiteManualSensorsCount(int projectId, int siteId)
        {
            using (var db = new QutSensorsDb())
            {
                return
                    EntityManager.Instance.GetDescendants(db, siteId, a => a.Entity_MetaData.DeploymentID.HasValue).
                        Where(ei => ei.Entity_MetaData.Deployment.Hardware.IsManualDevice).Select(
                            ei => ei.Entity_MetaData.Deployment).Count();
            }
        }

        /// <summary>
        /// The get hardware list.
        /// </summary>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        /// <param name="startIndex">
        /// The start index.
        /// </param>
        /// <param name="sortExpression">
        /// The sort expression.
        /// </param>
        /// <returns>Hardware list.
        /// </returns>
        public static IEnumerable<HardwareListItem> GetHardwareList(int maxItems, int startIndex, string sortExpression)
        {
            var hardwareListItems = new List<HardwareListItem>();
            using (var db = new QutSensorsDb())
            {
                var hardware = db.Hardware.AsQueryable();

                switch (sortExpression)
                {
                    case "HardwareName":
                        hardware = hardware.OrderBy(h => h.FriendlyName);
                        break;
                    case "HardwareName DESC":
                        hardware = hardware.OrderByDescending(h => h.FriendlyName);
                        break;
                    case "CurrentDeployment":
                        hardware =
                            hardware.OrderBy(
                                h =>
                                h.Deployments.OrderByDescending(d => d.DateStarted).FirstOrDefault() != null
                                    ? h.Deployments.OrderByDescending(d => d.DateStarted).FirstOrDefault().Name
                                    : String.Empty);
                        break;
                    case "CurrentDeployment DESC":
                        hardware =
                            hardware.OrderByDescending(
                                h =>
                                h.Deployments.OrderByDescending(d => d.DateStarted).FirstOrDefault() != null
                                    ? h.Deployments.OrderByDescending(d => d.DateStarted).FirstOrDefault().Name
                                    : String.Empty);
                        break;
                    case "CountDeployments":
                        hardware = hardware.OrderBy(h => h.Deployments.AsQueryable().Count());
                        break;
                    case "CountDeployments DESC":
                        hardware = hardware.OrderByDescending(h => h.Deployments.AsQueryable().Count());
                        break;
                    case "LastContacted":
                        hardware = hardware.OrderBy(h => h.LastContacted);
                        break;
                    case "LastContacted DESC":
                        hardware = hardware.OrderByDescending(h => h.LastContacted);
                        break;
                    case "PowerLevel":
                        hardware =
                            hardware.OrderBy(
                                h =>
                                h.DeviceStatus.OrderByDescending(d => d.Time).FirstOrDefault() != null
                                    ? h.DeviceStatus.OrderByDescending(d => d.Time).FirstOrDefault().PowerLevel
                                    : new byte?());
                        break;
                    case "PowerLevel DESC":
                        hardware =
                            hardware.OrderByDescending(
                                h =>
                                h.DeviceStatus.OrderByDescending(d => d.Time).FirstOrDefault() != null
                                    ? h.DeviceStatus.OrderByDescending(d => d.Time).FirstOrDefault().PowerLevel
                                    : new byte?());
                        break;
                    case "FreeMemory":
                        hardware =
                            hardware.OrderBy(
                                h =>
                                h.DeviceStatus.OrderByDescending(d => d.Time).FirstOrDefault() != null
                                    ? h.DeviceStatus.OrderByDescending(d => d.Time).FirstOrDefault().FreeMemory
                                    : new long?());
                        break;
                    case "FreeMemory DESC":
                        hardware =
                            hardware.OrderByDescending(
                                h =>
                                h.DeviceStatus.OrderByDescending(d => d.Time).FirstOrDefault() != null
                                    ? h.DeviceStatus.OrderByDescending(d => d.Time).FirstOrDefault().FreeMemory
                                    : new long?());
                        break;
                    case "MemoryUsage":
                        hardware =
                            hardware.OrderBy(
                                h =>
                                h.DeviceStatus.OrderByDescending(d => d.Time).FirstOrDefault() != null
                                    ? h.DeviceStatus.OrderByDescending(d => d.Time).FirstOrDefault().MemoryUsage
                                    : new long?());
                        break;
                    case "MemoryUsage DESC":
                        hardware =
                            hardware.OrderByDescending(
                                h =>
                                h.DeviceStatus.OrderByDescending(d => d.Time).FirstOrDefault() != null
                                    ? h.DeviceStatus.OrderByDescending(d => d.Time).FirstOrDefault().MemoryUsage
                                    : new long?());
                        break;
                    case "DeviceType":
                        hardware = hardware.OrderBy(h => h.IsManualDevice);
                        break;
                    case "DeviceType DESC":
                        hardware = hardware.OrderByDescending(h => h.IsManualDevice);
                        break;
                    default:
                        hardware = hardware.OrderByDescending(h => h.LastContacted);
                        break;
                }

                List<Hardware> hardwareList = hardware.Skip(startIndex).Take(maxItems).ToList();

                foreach (Hardware h1 in hardwareList)
                {
                    var hardwareItem = h1;

                    // tries to select all fields of audioreading, 
                    // end up with outofmemoryexception due to retrieving AudioReadingData.
                    var hli = new HardwareListItem
                        {
                            HardwareId = hardwareItem.HardwareID,
                            HardwareName = hardwareItem.FriendlyName,
                            DeploymentCount = db.Deployments.Where(de => de.HardwareID == hardwareItem.HardwareID).Count(),
                            AudioReadingsCount = db.AudioReadings.Where(ar => ar.HardwareID == hardwareItem.HardwareID).Count(),
                            LastContacted =
                                hardwareItem.LastContacted.HasValue
                                    ? hardwareItem.LastContacted.Value.ToString("ddd, d MMM yyyy HH:mm:ss")
                                    : "never",
                            DeviceType = hardwareItem.IsManualDevice ? "Manual" : "Auto"
                        };

                    // deployment info
                    Deployment d = hardwareItem.GetCurrentDeployment(db);
                    if (d != null)
                    {
                        hli.CurrentDeploymentName = d.Name;

                        Entity_Item entity = EntityManager.Instance.GetDeploymentEntity(db, d.DeploymentID);
                        if (entity != null)
                        {
                            hli.CurrentDeploymentEntityId = entity.EntityID;
                        }

                        hli.CurrentDeploymentId = d.DeploymentID;
                    }

                    // device status info
                    DeviceStatus status = db.DeviceStatus.Where(ds => ds.HardwareID == hardwareItem.HardwareID).OrderByDescending(s => s.Time).FirstOrDefault();
                    if (status != null)
                    {
                        hli.FreeMemory = status.FreeMemory.HasValue ? status.FreeMemory.Value + "kb" : "--";
                        hli.PowerLevel = status.PowerLevel.HasValue ? status.PowerLevel.Value + "%" : "--";
                        hli.MemoryUsage = status.MemoryUsage.HasValue ? status.MemoryUsage.Value + "%" : "--";
                    }

                    hardwareListItems.Add(hli);
                }
            }

            return hardwareListItems;
        }

        /// <summary>
        /// The get hardware list count.
        /// </summary>
        /// <returns>
        /// Number of hardware.
        /// </returns>
        public static int GetHardwareListCount()
        {
            using (var db = new QutSensorsDb())
            {
                return db.Hardware.Count();
            }
        }

        /// <summary>
        /// The get hardware list count.
        /// </summary>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        /// <param name="startIndex">
        /// The start index.
        /// </param>
        /// <param name="sortExpression">
        /// The sort expression.
        /// </param>
        /// <returns>
        /// Number of hardware.
        /// </returns>
        public static int GetHardwareListCount(int maxItems, int startIndex, string sortExpression)
        {
            return GetHardwareList(maxItems, startIndex, sortExpression).Count();
        }

        /// <summary>
        /// Get uploads in site.
        /// </summary>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        /// <param name="startIndex">
        /// The start index.
        /// </param>
        /// <param name="sortExpression">
        /// The sort expression.
        /// </param>
        /// <param name="projectId">
        /// The project id.
        /// </param>
        /// <param name="siteId">
        /// The site id.
        /// </param>
        /// <returns>
        /// Set of audio uploads in site.
        /// </returns>
        public static IEnumerable<SiteAudioUploads> GetSiteAudioUploads(int maxItems, int startIndex, string sortExpression, int projectId, int siteId)
        {
            var siteAudioUploads = new List<SiteAudioUploads>();
            using (var db = new QutSensorsDb())
            {
                var audioReadings =
                    EntityManager.Instance.GetDescendants(db, siteId, ei => ei.Entity_MetaData.DeploymentID.HasValue)
                    .SelectMany(ei => ei.Entity_MetaData.Deployment.AudioReadings).Where(ar => ar.UploadMetaData != null && ar.UserId.HasValue);

                switch (sortExpression)
                {
                    default:
                        audioReadings = audioReadings.OrderByDescending(ar => ar.Time);
                        break;
                }

                var audioReadingsList = audioReadings.Skip(startIndex).Take(maxItems).ToList();

                foreach (var arl in audioReadingsList)
                {
                    var sau = new SiteAudioUploads
                        {
                            AudioReadingTime = arl.Time,
                            MimeType = arl.MimeType,
                            SensorName = arl.Hardware.FriendlyName + ": " + arl.Deployment.Name,
                            UploadStatus = arl.State.ToString(),
							UploadFileLengthMilliseconds = new TimeSpan( 0, 0, 0, 0, arl.DurationMs.HasValue ? arl.DurationMs.Value : 0 ).ToReadableString(),
                            UploadBy = arl.UserId.HasValue ? arl.Aspnet_User.UserName : "unknown"
                        };

                    if (arl.UploadMetaData != null)
                    {
                        var fileDate = arl.UploadMetaData.XPathSelectElement("./resumeFileDate");
                        DateTime date;
                        if (fileDate != null &&
                            DateTime.TryParseExact(
                                fileDate.Value,
                                "yyyy-MM-ddTHHmmss",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out date))
                        {
                            sau.UploadDate = date.ToDifferenceString(DateTime.Now);
                        }

                        // TODO: this should use the AudioReading.Length column. Note sure how to do a datalength([Data]) in Linq to SQL.
                        var fileLength = arl.UploadMetaData.XPathSelectElement("./resumeFileTotalLength");
                        int length;
                        if (fileLength != null && Int32.TryParse(fileLength.Value, out length))
                        {
                            sau.UploadFileLengthBytes = ((long)length).ToByteDisplay();
                        }
                    }

                    siteAudioUploads.Add(sau);
                }
            }

            return siteAudioUploads;
        }

        /// <summary>
        /// Get number of audio uploads in site.
        /// </summary>
        /// <param name="projectId">
        /// The project id.
        /// </param>
        /// <param name="siteId">
        /// The site id.
        /// </param>
        /// <returns>
        /// Number of audio uploads in site.
        /// </returns>
        public static int GetSiteAudioUploadsCount(int projectId, int siteId)
        {
            using (var db = new QutSensorsDb())
            {
                return EntityManager.Instance.GetDescendants(db, siteId, ei => ei.Entity_MetaData.DeploymentID.HasValue)
                    .SelectMany(ei => ei.Entity_MetaData.Deployment.AudioReadings)
                    .Where(ar => ar.UploadMetaData != null && ar.UserId.HasValue)
                    .Count();
            }
        }

        /// <summary>
        /// Get Devicelogs for a single hardware.
        /// </summary>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        /// <param name="startIndex">
        /// The start index.
        /// </param>
        /// <param name="sortExpression">
        /// The sort expression.
        /// </param>
        /// <param name="hardwareId">
        /// The hardware id.
        /// </param>
        /// <returns>
        /// List of DeviceLogs.
        /// </returns>
        public static IEnumerable<DeviceLog> GetHardwareLogs(int maxItems, int startIndex, string sortExpression, int hardwareId)
        {
            using (var db = new QutSensorsDb())
            {
                return GetHardwareLogs(db, maxItems, startIndex, sortExpression, hardwareId).ToList();
            }
        }

        /// <summary>
        /// Get Devicelogs for a single hardware.
        /// </summary>
        /// <param name="db">
        /// Data Context.
        /// </param>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        /// <param name="startIndex">
        /// The start index.
        /// </param>
        /// <param name="sortExpression">
        /// The sort expression.
        /// </param>
        /// <param name="hardwareId">
        /// The hardware id.
        /// </param>
        /// <returns>
        /// List of DeviceLogs.
        /// </returns>
        public static IQueryable<DeviceLog> GetHardwareLogs(QutSensorsDb db, int maxItems, int startIndex, string sortExpression, int hardwareId)
        {
            var result = db.DeviceLogs.AsQueryable();

            switch (sortExpression)
            {
                case "LogTime":
                    result = result.OrderBy(a => a.Time);
                    break;
                case "LogTime DESC":
                    result = result.OrderByDescending(a => a.Time);
                    break;
                case "ErrorDetails":
                    result = result.OrderBy(a => a.ErrorDetails).ThenBy(a => a.Time);
                    break;
                case "ErrorDetails DESC":
                    result = result.OrderByDescending(a => a.ErrorDetails).ThenByDescending(a => a.Time);
                    break;
                case "Text":
                    result = result.OrderBy(a => a.Text).ThenBy(a => a.Time);
                    break;
                case "Text DESC":
                    result = result.OrderByDescending(a => a.Text).ThenByDescending(a => a.Time);
                    break;
                case "Type":
                    result = result.OrderBy(a => a.Type).ThenBy(a => a.Time);
                    break;
                case "Type DESC":
                    result = result.OrderByDescending(a => a.Type).ThenByDescending(a => a.Time);
                    break;
                default:
                    result = result.OrderByDescending(a => a.Time);
                    break;
            }

            if (hardwareId > 0)
            {
                result = result.Where(a => a.HardwareID == hardwareId);
            }

            return result.Skip(startIndex).Take(maxItems);
        }

        /// <summary>
        /// Count Devicelogs for a single hardware.
        /// </summary>
        /// <param name="hardwareId">
        /// The hardware id.
        /// </param>
        /// <returns>
        /// Number of DeviceLogs.
        /// </returns>
        public static int CountHardwareLogs(int hardwareId)
        {
            using (var db = new QutSensorsDb())
            {
                return db.DeviceLogs.Where(a => a.HardwareID == hardwareId).Count();
            }
        }

        /// <summary>
        /// The get hardware status.
        /// </summary>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        /// <param name="startIndex">
        /// The start index.
        /// </param>
        /// <param name="sortExpression">
        /// The sort expression.
        /// </param>
        /// <param name="hardwareId">
        /// The hardware id.
        /// </param>
        /// <returns>Status items.
        /// </returns>
        public static IEnumerable<DeviceStatus> GetHardwareStatus(
            int maxItems, int startIndex, string sortExpression, int hardwareId)
        {
            using (var db = new QutSensorsDb())
            {
                return GetHardwareStatus(db, maxItems, startIndex, sortExpression, hardwareId).ToList();
            }
        }

        /// <summary>
        /// The get hardware status.
        /// </summary>
        /// <param name="db">
        /// Data Context.
        /// </param>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        /// <param name="startIndex">
        /// The start index.
        /// </param>
        /// <param name="sortExpression">
        /// The sort expression.
        /// </param>
        /// <param name="hardwareId">
        /// The hardware id.
        /// </param>
        /// <returns>Status items.
        /// </returns>
        public static IQueryable<DeviceStatus> GetHardwareStatus(
            QutSensorsDb db, int maxItems, int startIndex, string sortExpression, int hardwareId)
        {
            var result = db.DeviceStatuses.AsQueryable();

            switch (sortExpression)
            {
                case "Time":
                    result = result.OrderBy(a => a.Time);
                    break;
                case "Time DESC":
                    result = result.OrderByDescending(a => a.Time);
                    break;
                case "PowerLevel":
                    result = result.OrderBy(a => a.PowerLevel).ThenBy(a => a.Time);
                    break;
                case "PowerLevel DESC":
                    result = result.OrderByDescending(a => a.PowerLevel).ThenByDescending(a => a.Time);
                    break;
                case "MemoryUsage":
                    result = result.OrderBy(a => a.MemoryUsage).ThenBy(a => a.Time);
                    break;
                case "MemoryUsage DESC":
                    result = result.OrderByDescending(a => a.MemoryUsage).ThenByDescending(a => a.Time);
                    break;
                case "FreeMemory":
                    result = result.OrderBy(a => a.FreeMemory).ThenBy(a => a.Time);
                    break;
                case "FreeMemory DESC":
                    result = result.OrderByDescending(a => a.FreeMemory).ThenByDescending(a => a.Time);
                    break;
                default:
                    result = result.OrderByDescending(a => a.Time);
                    break;
            }

            if (hardwareId > 0)
            {
                result = result.Where(a => a.HardwareID == hardwareId);
            }

            return result.Skip(startIndex).Take(maxItems);
        }

        /// <summary>
        /// The count hardware status.
        /// </summary>
        /// <param name="hardwareId">
        /// The hardware id.
        /// </param>
        /// <returns>
        /// Number of hardware status items.
        /// </returns>
        public static int CountHardwareStatus(int hardwareId)
        {
            using (var db = new QutSensorsDb())
            {
                return db.DeviceStatuses.Where(a => a.HardwareID == hardwareId).Count();
            }
        }

        /// <summary>
        /// The get hardware.
        /// </summary>
        /// <param name="sortExpression">
        /// The sort expression.
        /// </param>
        /// <returns>Get all hardware sorted.
        /// </returns>
        public static IEnumerable<Hardware> GetHardware(string sortExpression)
        {
            return GetHardware(int.MaxValue, 0, sortExpression);
        }

        /// <summary>
        /// The get hardware.
        /// </summary>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        /// <param name="startIndex">
        /// The start index.
        /// </param>
        /// <returns>
        /// Page of Hardware.
        /// </returns>
        public static IEnumerable<Hardware> GetHardware(int maxItems, int startIndex)
        {
            return GetHardware(maxItems, startIndex, string.Empty);
        }

        /// <summary>
        /// The get hardware.
        /// </summary>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        /// <param name="startIndex">
        /// The start index.
        /// </param>
        /// <param name="sortExpression">
        /// The sort expression.
        /// </param>
        /// <returns>Page of hardware.
        /// </returns>
        public static IEnumerable<Hardware> GetHardware(int maxItems, int startIndex, string sortExpression)
        {
            using (var db = new QutSensorsDb())
            {
                return GetHardware(db, maxItems, startIndex, sortExpression).ToList();
            }
        }

        /// <summary>
        /// The get hardware.
        /// </summary>
        /// <param name="db">
        /// Data Context.
        /// </param>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        /// <param name="startIndex">
        /// The start index.
        /// </param>
        /// <param name="sortExpression">
        /// The sort expression.
        /// </param>
        /// <returns>Page of hardware.
        /// </returns>
        public static IQueryable<Hardware> GetHardware(QutSensorsDb db, int maxItems, int startIndex, string sortExpression)
        {
            IQueryable<Hardware> result;

            switch (sortExpression)
            {
                case "HardwareName":
                    result = db.Hardware.OrderBy(a => a.FriendlyName);
                    break;
                case "HardwareName DESC":
                    result = db.Hardware.OrderByDescending(a => a.FriendlyName);
                    break;
                case "CountDeployments":
                    result = db.Hardware.OrderBy(a => a.Deployments.Count());
                    break;
                case "CountDeployments DESC":
                    result = db.Hardware.OrderByDescending(a => a.Deployments.Count());
                    break;
                case "CurrentDeployment":
                    result =
                        db.Hardware.OrderBy(
                            a =>
                            a.Deployments.OrderByDescending(b => b.DateStarted).FirstOrDefault() == null
                                ? string.Empty
                                : a.Deployments.OrderByDescending(b => b.DateStarted).First().Name);
                    break;
                case "CurrentDeployment DESC":
                    result =
                        db.Hardware.OrderByDescending(
                            a =>
                            a.Deployments.OrderByDescending(b => b.DateStarted).FirstOrDefault() == null
                                ? string.Empty
                                : a.Deployments.OrderByDescending(b => b.DateStarted).First().Name);
                    break;
                default:
                    result = db.Hardware.OrderBy(a => a.LastContacted);
                    break;
            }

            return result.Skip(startIndex).Take(maxItems);
        }

        /// <summary>
        /// Get the count of hardware.
        /// </summary>
        /// <returns>
        /// Count of hardware.
        /// </returns>
        public static int CountHardware()
        {
            using (var db = new QutSensorsDb())
            {
                return CountHardware(db);
            }
        }

        /// <summary>
        /// Get the count of hardware.
        /// </summary>
        /// <param name="db">
        /// Data Context.
        /// </param>
        /// <returns>
        /// Count of hardware.
        /// </returns>
        public static int CountHardware(QutSensorsDb db)
        {
            return db.Hardware.Count();
        }

        /// <summary>
        /// Get deployments.
        /// </summary>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        /// <param name="startIndex">
        /// The start index.
        /// </param>
        /// <param name="sortExpression">
        /// The sort expression.
        /// </param>
        /// <returns>
        /// List of deployments.
        /// </returns>
        public static IEnumerable<Deployment> GetDeployments(int maxItems, int startIndex, string sortExpression)
        {
            using (var db = new QutSensorsDb())
            {
                return GetDeployments(db, maxItems, startIndex, sortExpression, 0, null).ToList();
            }
        }

        /// <summary>
        /// Get deployments.
        /// </summary>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        /// <param name="startIndex">
        /// The start index.
        /// </param>
        /// <param name="sortExpression">
        /// The sort expression.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// List of deployments.
        /// </returns>
        public static IEnumerable<Deployment> GetDeployments(int maxItems, int startIndex, string sortExpression, Guid? userId)
        {
            using (var db = new QutSensorsDb())
            {
                return GetDeployments(db, maxItems, startIndex, sortExpression, 0, userId).ToList();
            }
        }

        public static IEnumerable<Deployment> GetDeployments(int maxItems, int startIndex, string sortExpression, int hardwareId)
        {
            using (var db = new QutSensorsDb())
            {
                return GetDeployments(db, maxItems, startIndex, sortExpression, hardwareId, null).ToList();
            }
        }

        public static IQueryable<Deployment> GetDeployments(QutSensorsDb db, int maxItems, int startIndex, string sortExpression, int hardwareId, Guid? userId)
        {
            IQueryable<Deployment> result;

            switch (sortExpression)
            {
                case "HardwareName":
                    result = db.Deployments.OrderBy(a => a.Hardware.FriendlyName).ThenBy(b => b.IsActive).ThenBy(a => a.AudioReadings.Max(b => b.Time));
                    break;
                case "HardwareName DESC":
                    result = db.Deployments.OrderByDescending(a => a.Hardware.FriendlyName).ThenByDescending(b => b.IsActive).ThenByDescending(a => a.AudioReadings.Max(b => b.Time));
                    break;
                case "DeploymentName":
                    result = db.Deployments.OrderBy(a => a.Name);
                    break;
                case "DeploymentName DESC":
                    result = db.Deployments.OrderByDescending(a => a.Name);
                    break;
                case "MostRecent":
                    result = db.Deployments.OrderBy(a => a.AudioReadings.Max(b => b.Time));
                    break;
                case "MostRecent DESC":
                    result = db.Deployments.OrderByDescending(a => a.AudioReadings.Max(b => b.Time));
                    break;
                case "Active":
                    result = db.Deployments.OrderBy(a => a.IsActive).ThenBy(a => a.Name);
                    break;
                case "Active DESC":
                    result = db.Deployments.OrderByDescending(a => a.IsActive).ThenByDescending(a => a.Name);
                    break;
                case "Test":
                    result = db.Deployments.OrderBy(a => a.IsTest).ThenBy(a => a.Name);
                    break;
                case "Test DESC":
                    result = db.Deployments.OrderByDescending(a => a.IsTest).ThenByDescending(a => a.Name);
                    break;
                case "LastContacted":
                    result = db.Deployments.OrderBy(a => a.Hardware.LastContacted).ThenBy(a => a.Name);
                    break;
                case "LastContacted DESC":
                    result = db.Deployments.OrderByDescending(a => a.Hardware.LastContacted).ThenByDescending(a => a.Name);
                    break;
                case "DateStarted":
                    result = db.Deployments.OrderBy(a => a.DateStarted).ThenBy(a => a.Name);
                    break;
                case "DateStarted DESC":
                    result = db.Deployments.OrderByDescending(a => a.DateStarted).ThenByDescending(a => a.Name);
                    break;
                case "DateEnded":
                    result = db.Deployments.OrderBy(a => a.DateEnded).ThenBy(a => a.Name);
                    break;
                case "DateEnded DESC":
                    result = db.Deployments.OrderByDescending(a => a.DateEnded).ThenByDescending(a => a.Name);
                    break;
                default:
                    result = db.Deployments.OrderByDescending(a => a.DateStarted).ThenBy(a => a.Name);
                    break;
            }

            if (hardwareId > 0)
            {
                result = result.Where(a => a.HardwareID == hardwareId);
            }

            if (userId.HasValue && userId != Guid.Empty)
            {
                // get projects the userId has full access to
                var projects = EntityManager.Instance.GetEntityItemsForEdit(db, userId, ei => ei.Entity_MetaData.Type == EntityType.Project);

                // get deployments in those projects
                var deployments = EntityManager.Instance.GetDescendants(db, projects.Select(a => a.EntityID), a => a.Entity_MetaData.DeploymentID.HasValue).Select(a => a.Entity_MetaData.DeploymentID.Value);

                result = result.Where(a => deployments.ToList().Contains(a.DeploymentID));
            }

            return result.Skip(startIndex).Take(maxItems);
        }

        public static int CountDeployments()
        {
            using (var db = new QutSensorsDb())
            {
                return CountDeployments(db, 0, null);
            }
        }

        public static int CountDeployments(QutSensorsDb db, int hardwareId, Guid? userId)
        {
            var query = db.Deployments.AsQueryable();

            if (hardwareId > 0)
            {
                query = query.Where(a => a.HardwareID == hardwareId);
            }

            if (userId.HasValue && userId.Value != Guid.Empty)
            {
                // get projects the userId has full access to
                var projects = EntityManager.Instance.GetEntityItemsForEdit(db, userId, ei => ei.Entity_MetaData.Type == EntityType.Project);

                // get deployments in those projects
                var deployments = EntityManager.Instance.GetDescendants(db, projects.Select(a => a.EntityID), a => a.Entity_MetaData.DeploymentID.HasValue).Select(a => a.Entity_MetaData.DeploymentID.Value);

                query = query.Where(a => deployments.ToList().Contains(a.DeploymentID));
            }

            return query.Count();
        }

        public static int CountDeployments(int hardwareId)
        {
            using (var db = new QutSensorsDb())
            {
                return CountDeployments(db, hardwareId, null);
            }
        }

        public static int CountDeployments(Guid? userId)
        {
            using (var db = new QutSensorsDb())
            {
                return CountDeployments(db, 0, userId);
            }
        }

        /// <summary>
        /// The to deployment info.
        /// </summary>
        /// <returns>deployment info.
        /// </returns>
        public static DeploymentInfo ToDeploymentInfo(Deployment deployment)
        {
            var depInfo = new DeploymentInfo
            {
                DeploymentID = deployment.DeploymentID,
                Name = !string.IsNullOrEmpty(deployment.Name) ? deployment.Name : "(unnamed deployment)",
                StartDate = deployment.DateStarted,
                EndDate = deployment.DateEnded,
                HardwareID = deployment.HardwareID
            };

            if (deployment.AudioReadings.Count > 0)
            {
                depInfo.FirstRecording = deployment.AudioReadings.OrderBy(ar => ar.Time).First().Time;
                depInfo.LatestRecording = deployment.AudioReadings.OrderBy(ar => ar.Time).Last().Time;
            }

            if (deployment.Location == null)
            {
                depInfo.Latitude = 0;
                depInfo.Longitude = 0;
            }
            else
            {
                depInfo.Latitude = deployment.Location.Lat.Value;
                depInfo.Longitude = deployment.Location.Long.Value;
            }

            depInfo.IsActive = deployment.IsActive;
            depInfo.IsTest = deployment.IsTest;

            if (deployment.Entity_MetaData.Any())
            {
                Entity_MetaData meta = deployment.Entity_MetaData.FirstOrDefault();
                if (meta != null)
                {
                    depInfo.EntityID = meta.EntityID;
                }
            }

            depInfo.Device = new DeviceInfo(deployment.Hardware.FriendlyName, deployment.HardwareID);

            return depInfo;
        }

        public static List<DeploymentInfo> ToDeploymentInfoList(IEnumerable<Deployment> items)
        {
            return items.Select(item => ToDeploymentInfo(item)).ToList();
        }

        public static IEnumerable<HardwareListItem> GetHardwareNotInSite(int maxItems, int startIndex, string sortExpression)
        {
            var hardwareItems = new List<HardwareListItem>();
            using (var db = new QutSensorsDb())
            {
                var hardware =
                    db.Entity_Items.Where(
                        ei =>
                        ei.Entity_MetaData.DeploymentID.HasValue &&
                        db.Entity_Relationships.Where(er => er.ChildID == ei.EntityID).Count() < 1 &&
                        db.Entity_Relationships.Where(er => er.ParentID == ei.EntityID).Count() < 1).Select(
                            ei => ei.Entity_MetaData.Deployment.Hardware).Distinct();

                var results = from hw in hardware.Distinct() select hw;


                // add switch for sort expression

                var items = results.Skip(startIndex).Take(maxItems).ToList();

                hardwareItems =
                    items.Select(
                        h =>
                        new HardwareListItem
                            {
                                HardwareId = h.HardwareID,
                                HardwareUniqueID = h.UniqueID,
                                HardwareName = h.FriendlyName,
                                LastContacted =
                                    h.LastContacted.HasValue
                                        ? h.LastContacted.Value.ToString("ddd, d MMM yyyy HH:mm:ss")
                                        : "never"
                            }).OrderByDescending(h => h.LastContacted).ToList();
            }

            return hardwareItems;
        }

        public static int GetHardwareNotInSiteCount()
        {
            using (var db = new QutSensorsDb())
            {
                var hardware =
                    db.Entity_Items.Where(
                        ei =>
                        ei.Entity_MetaData.DeploymentID.HasValue &&
                        db.Entity_Relationships.Where(er => er.ChildID == ei.EntityID).Count() < 1 &&
                        db.Entity_Relationships.Where(er => er.ParentID == ei.EntityID).Count() < 1).Select(
                            ei => ei.Entity_MetaData.Deployment.Hardware).Distinct();

                var results = from hw in hardware.Distinct() select hw;
                return results.Count();
            }
        }
    }
}