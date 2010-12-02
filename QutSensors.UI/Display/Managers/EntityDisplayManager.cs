// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntityDisplayManager.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the EntityDisplayManager type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Profile;
    using System.Web.Security;

    using BTR.Core.Linq;

    using QutSensors.Business.Audio;
    using QutSensors.Data;
    using QutSensors.Data.Linq;
    using QutSensors.Shared;
    using QutSensors.UI.Display.Classes;
    using QutSensors.UI.Security;

    /// <summary>
    /// Entity manager for displaying info.
    /// </summary>
    public static class EntityDisplayManager
    {
        /// <summary>
        /// Get sites in a project.
        /// </summary>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        /// <param name="startIndex">
        /// The start index.
        /// </param>
        /// <param name="projectId">
        /// The project id.
        /// </param>
        /// <returns>
        /// List of sites.
        /// </returns>
        public static IEnumerable<ProjectSitesDisplayItem> GetProjectSites(int maxItems, int startIndex, int projectId)
        {
            string currentUserName = AuthenticationHelper.CurrentUserName;

            var list = new List<ProjectSitesDisplayItem>();
            using (var db = new QutSensorsDb())
            {
                var results = EntityManager.Instance.GetDescendants(db, projectId, a => a.Entity_MetaData.Type == EntityType.Site).OrderBy(s => s.Name);

                var items = results.Skip(startIndex).Take(maxItems).ToList();

                foreach (var item in items)
                {
                    var deps = EntityManager.Instance.GetDescendants(
                        db, item.EntityID, ei1 => ei1.Entity_MetaData.DeploymentID.HasValue);

                    var query = from ar in db.AudioReadings
                                join dep in deps on ar.DeploymentID equals dep.Entity_MetaData.DeploymentID
                                select
                                    new
                                        {
                                            Id = ar.AudioReadingID,
                                            Duration = ar.Length,
                                            TagCount = db.AudioTags.Count(t => t.AudioReadingID == ar.AudioReadingID),
                                            HeardByCurrentUser =
                                    db.ReadReadings.Any(
                                        rr =>
                                        rr.AudioReadingID == ar.AudioReadingID &&
                                        rr.UserName == currentUserName),
                                            Time = ar.Time,
                                            UserId = ar.UserId
                                        };

                    var lastActivityAr = query.OrderByDescending(q => q.Time).FirstOrDefault();
                    string lastActivityString = "None";
                    if (lastActivityAr != null)
                    {
                        lastActivityString = lastActivityAr.Time.ToString("dd MMM yyyy") + " (" + lastActivityAr.Time.ToDifferenceString(DateTime.Now) + ") - ";

                        if (lastActivityAr.UserId.HasValue)
                        {
                            string displayName = AuthenticationHelper.GetUserName(lastActivityAr.UserId.Value);

                            lastActivityString += displayName + " uploaded audio data";
                        }
                        else
                        {
                            lastActivityString += "sensor upload";
                        }
                    }

                    long? totalDuration = query.Sum(l => (long?)l.Duration);
                    long? unheardDuration = query.Where(l => !l.HeardByCurrentUser).Sum(l => (long?)l.Duration);
                    long? tagCount = query.Sum(q => (long?)q.TagCount);




                    var displayItem = new ProjectSitesDisplayItem
                        {
                            Name = item.Name,
                            ProjectId = projectId,
                            SiteId = item.EntityID,
                            TotalAudioReadingDuration =
                                new TimeSpan((totalDuration.HasValue ? totalDuration.Value : 0) * TimeSpan.TicksPerMillisecond).ToReadableString(),
                            LastActivity = lastActivityString,
                            TagCount = tagCount.HasValue ? tagCount.Value : 0,
                            UnheardAudioReadingDuration =
                                new TimeSpan((unheardDuration.HasValue ? unheardDuration.Value : 0) * TimeSpan.TicksPerMillisecond).ToReadableString(),
                            ListenToAllQs =
                                new ReadingsFilter { CommaSeparatedEntityIds = item.EntityID.ToString() }.ToQueryString(
                                    true),
                            ListenToUnheardQs =
                                new ReadingsFilter { CommaSeparatedEntityIds = item.EntityID.ToString(), IsRead = false }.
                                ToQueryString(true)
                        };

                    list.Add(displayItem);
                }

                return list;
            }
        }

        /// <summary>
        /// Count sites in a project.
        /// </summary>
        /// <param name="projectId">
        /// The project id.
        /// </param>
        /// <returns>
        /// Number of sites.
        /// </returns>
        public static int GetProjectSitesCount(int projectId)
        {
            return GetProjectSites(int.MaxValue, 0, projectId).Count();
        }

        /// <summary>
        /// Get jobs in a project.
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
        /// <returns>
        /// List of jobs.
        /// </returns>
        public static IEnumerable<ProjectJobsDisplayItem> GetProjectJobs(int maxItems, int startIndex, string sortExpression, int projectId)
        {
            var list = new List<ProjectJobsDisplayItem>();
            using (var db = new QutSensorsDb())
            {
                var results = EntityManager.Instance.GetDescendants(db, projectId, a => a.Entity_MetaData.JobID.HasValue);

                switch (sortExpression)
                {
                    case "Name":
                        results = results.OrderBy(s => s.Name);
                        break;
                    case "Name DESC":
                        results = results.OrderByDescending(s => s.Name);
                        break;
                    case "Owner":
                        results = results.OrderBy(s => s.Owner.HasValue ? s.Aspnet_User.UserName : string.Empty);
                        break;
                    case "Owner DESC":
                        results = results.OrderByDescending(s => s.Owner.HasValue ? s.Aspnet_User.UserName : string.Empty);
                        break;
                }

                var items = results.Skip(startIndex).Take(maxItems).ToList();

                list = (from item in items
                        let totalJobItems = db.Processor_JobItems.Where(ji => ji.JobID == item.Entity_MetaData.JobID).Count()
                        let finishedJobItems = db.Processor_JobItems.Where(ji => ji.JobID == item.Entity_MetaData.JobID).Count(ji => ji.Status == JobStatus.Complete || ji.Status == JobStatus.Error)
                        select new ProjectJobsDisplayItem
                            {
                                Name = item.Name,
                                Owner = item.Owner.HasValue ? item.Aspnet_User.UserName : string.Empty,
                                ProjectId = projectId,
                                JobId = item.EntityID,
                                ListenToAllQs = new ReadingsFilter { CommaSeparatedJobIds = item.EntityID.ToString() }.ToQueryString(true),
                                ListenToUnheardQs = new ReadingsFilter { CommaSeparatedJobIds = item.EntityID.ToString(), IsRead = false }.ToQueryString(true),
                                ResultsQs = new ReadingsFilter { CommaSeparatedJobIds = item.EntityID.ToString(), RuiCommaSeparatedJobIds = item.EntityID.ToString() }.ToQueryString(true),
                                ProcessNewReadings = item.Entity_MetaData.Processor_Job.ProcessNewReadings ? "Yes" : "No",
                                CurrentProgress = totalJobItems == 0 ? "No tasks" : finishedJobItems + " out of " + totalJobItems + " (" + (int)(((float)finishedJobItems / (float)totalJobItems) * 100) + "%)"
                            }).ToList();
                return list;
            }
        }

        /// <summary>
        /// Count jobs in a project.
        /// </summary>
        /// <param name="projectId">
        /// The project id.
        /// </param>
        /// <returns>
        /// Number of jobs.
        /// </returns>
        public static int GetProjectJobsCount(int projectId)
        {
            return GetProjectJobs(int.MaxValue, 0, string.Empty, projectId).Count();
        }

        /// <summary>
        /// Get data sets in project.
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
        /// <returns>
        /// Data sets.
        /// </returns>
        public static IEnumerable<ProjectDataSetsDisplayItem> GetProjectDataSets(int maxItems, int startIndex, string sortExpression, int projectId)
        {
            var list = new List<ProjectDataSetsDisplayItem>();
            using (var db = new QutSensorsDb())
            {
                var results = EntityManager.Instance
                    .GetDescendants(db, projectId, ei => ei.Entity_MetaData.Filter != null && ei.Entity_MetaData.Type == EntityType.Filter)
                    .OrderBy(s => s.Name);

                switch (sortExpression)
                {
                    case "Name":
                        results = results.OrderBy(s => s.Name);
                        break;
                    case "Name DESC":
                        results = results.OrderByDescending(s => s.Name);
                        break;
                    case "Owner":
                        results = results.OrderBy(s => s.Owner.HasValue ? s.Aspnet_User.UserName : string.Empty).ThenBy(s => s.Name);
                        break;
                    case "Owner DESC":
                        results = results.OrderByDescending(s => s.Owner.HasValue ? s.Aspnet_User.UserName : string.Empty).ThenBy(s => s.Name);
                        break;
                }

                var items = results.Skip(startIndex).Take(maxItems).ToList();

                foreach (var ei in items)
                {
                    var rf = ei.Entity_MetaData.ReadingsFilter;
                    var unreadRf = rf;
                    unreadRf.IsRead = false;

                    var dispItem = new ProjectDataSetsDisplayItem
                        {
                            Name = ei.Name,
                            Owner = ei.Owner.HasValue ? ei.Aspnet_User.UserName : string.Empty,
                            ProjectId = projectId,
                            DataSetId = ei.EntityID,
                            ListenToAllQs = rf.ToQueryString(true),
                            ListenToUnheardQs = unreadRf.ToQueryString(true),
                        };

                    list.Add(dispItem);
                }

                return list;
            }
        }

        /// <summary>
        /// Get count of data sets in project.
        /// </summary>
        /// <param name="projectId">
        /// The project id.
        /// </param>
        /// <returns>
        /// Number of data sets.
        /// </returns>
        public static int GetProjectDataSetsCount(int projectId)
        {
            return GetProjectDataSets(int.MaxValue, 0, string.Empty, projectId).Count();
        }

        /// <summary>
        /// Get audio reading segments for a data set.
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
        /// <param name="dataSetEntityId">
        /// The data Set Entity Id.
        /// </param>
        /// <param name="readReadingsUserName">
        /// The read Readings User Name.
        /// </param>
        /// <param name="loggedInUserId">
        /// The logged In User Id.
        /// </param>
        /// <param name="isUserAdmin">
        /// The is User Admin.
        /// </param>
        /// <returns>
        /// Audio reading segments in data set.
        /// </returns>
        public static IEnumerable<DataSetAudioReadingDisplayItem> GetDataSetAudioReadings(int maxItems, int startIndex, string sortExpression, int dataSetEntityId, string readReadingsUserName, Guid? loggedInUserId, bool isUserAdmin)
        {
            using (var db = new QutSensorsDb())
            {
                var dataSet =
                    db.Entity_MetaDatas.Where(
                        em => em.EntityID == dataSetEntityId && em.Filter != null && em.Filter.Length > 0).
                        SingleOrDefault();

                if (dataSet == null)
                {
                    return new List<DataSetAudioReadingDisplayItem>();
                }

                var results =
                isUserAdmin ?
                ReadingsFilterManager.GetAudioReadings(db, dataSet.ReadingsFilter, readReadingsUserName, db.AudioReadings.AsQueryable(), true) :
                ReadingsFilterManager.GetAudioReadings(db, dataSet.ReadingsFilter, readReadingsUserName, loggedInUserId, true);

                switch (sortExpression)
                {
                    case "DeploymentName":
                        results = results.OrderBy(r => r.Deployment.Name);
                        break;
                    case "DeploymentName DESC":
                        results = results.OrderByDescending(r => r.Deployment.Name);
                        break;
                    case "HardwareName":
                        results = results.OrderBy(r => r.Deployment.Name);
                        break;
                    case "HardwareName DESC":
                        results = results.OrderByDescending(r => r.Deployment.Name);
                        break;
                    case "AudioReadingEnd":
                        results = results.OrderBy(r => r.Time.AddMilliseconds(r.Length.HasValue ? r.Length.Value : 0));
                        break;
                    case "AudioReadingEnd DESC":
                        results = results.OrderByDescending(r => r.Time.AddMilliseconds(r.Length.HasValue ? r.Length.Value : 0));
                        break;
                    case "AudioReadingStart":
                        results = results.OrderBy(r => r.Time);
                        break;
                    case "AudioReadingStart DESC":
                        results = results.OrderByDescending(r => r.Time);
                        break;
                    case "AudioReadingDuration":
                        results = results.OrderBy(r => r.Length);
                        break;
                    case "AudioReadingDuration DESC":
                        results = results.OrderByDescending(r => r.Length);
                        break;
                    case "State":
                        results = results.OrderBy(r => r.State).ThenBy(r => r.Time);
                        break;
                    case "State DESC":
                        results = results.OrderByDescending(r => r.State).ThenByDescending(r => r.Time);
                        break;
                    default:
                        results = results.OrderByDescending(r => r.Time);
                        break;
                }

                var items = results.Skip(startIndex).Take(maxItems);

                var segmentResults = ReadingsFilterManager.GetResults(items, dataSet.ReadingsFilter);

                var dataSetAudioReadingDisplayItems = segmentResults.Select(
                    sr => new DataSetAudioReadingDisplayItem
                        {
                            DeploymentName = sr.DeploymentName,
                            HardwareName = sr.HardwareName,

                            AudioReadingStart = sr.AudioReadingStart,
                            AudioReadingEnd = sr.AudioReadingStart.AddMilliseconds(sr.AudioReadingTotalDuration),
                            AudioReadingDuration = new TimeSpan(0, 0, 0, 0, sr.AudioReadingTotalDuration).ToReadableString(),

                            State = sr.State,

                            SegmentStartTime = sr.StartTime.HasValue ? new TimeSpan(0, 0, 0, 0, sr.StartTime.Value).ToReadableString() : "unknown",
                            SegmentEndTime = sr.EndTime.HasValue ? new TimeSpan(0, 0, 0, 0, sr.EndTime.Value).ToReadableString() : "unknown",
                            SegmentDuration = sr.EndTime.HasValue && sr.StartTime.HasValue ? new TimeSpan(0, 0, 0, 0, sr.EndTime.Value - sr.StartTime.Value).ToReadableString() : "unknown",
                        });

                return dataSetAudioReadingDisplayItems;
            }
        }

        /// <summary>
        /// Get count of audio reading segments in data set.
        /// </summary>
        /// <param name="dataSetEntityId">
        /// The data Set Entity Id.
        /// </param>
        /// <param name="readReadingsUserName">
        /// The read Readings User Name.
        /// </param>
        /// <param name="loggedInUserId">
        /// The logged In User Id.
        /// </param>
        /// <param name="isUserAdmin">
        /// The is User Admin.
        /// </param>
        /// <returns>
        /// Count of audio reading segments in data set.
        /// </returns>
        public static int GetDataSetAudioReadingsCount(int dataSetEntityId, string readReadingsUserName, Guid? loggedInUserId, bool isUserAdmin)
        {
            using (var db = new QutSensorsDb())
            {
                var dataSet =
                    db.Entity_MetaDatas.Where(
                        em => em.EntityID == dataSetEntityId && em.Filter != null && em.Filter.Length > 0).
                        SingleOrDefault();

                if (dataSet == null)
                {
                    return 0;
                }

                var results = isUserAdmin
                                  ? ReadingsFilterManager.GetAudioReadings(db, dataSet.ReadingsFilter, readReadingsUserName, db.AudioReadings.AsQueryable(), true)
                                  : ReadingsFilterManager.GetAudioReadings(db, dataSet.ReadingsFilter, readReadingsUserName, loggedInUserId, true);

                return results.Count();
            }
        }

        /// <summary>
        /// Get project name and id.
        /// </summary>
        /// <param name="db">
        /// Data Context.
        /// </param>
        /// <param name="isUserAdmin">
        /// Is the user an admin.
        /// </param>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// List of Project summary details.
        /// </returns>
        public static IEnumerable<ProjectDisplayItem> GetProjectSummaryDisplayItems(QutSensorsDb db, bool isUserAdmin, string userName, Guid userId)
        {
            IQueryable<Entity_Item> projects;

            if (isUserAdmin)
            {
                projects = db.Entity_Items.Where(ei => ei.Entity_MetaData.Type == EntityType.Project);
            }
            else
            {
                projects = EntityManager.Instance.GetEntityItemsForView(
                    db, userId != Guid.Empty ? userId : new Guid?(), ei => ei.Entity_MetaData.Type == EntityType.Project);
            }

            var orderedprojects = projects.OrderBy(ei => ei.Name);

            var items = orderedprojects.Select(p => new ProjectDisplayItem { ProjectId = p.EntityID, ProjectName = p.Name }).ToList();

            foreach (var item in items)
            {
                var access = isUserAdmin
                                 ? EntityAccessLevel.Full
                                 : EntityManager.Instance.ProjectAccessLevel(db, item.ProjectId, userId);

                item.ProjectAccessString = access == EntityAccessLevel.Readonly ? "(read&nbsp;only)" : string.Empty;
            }
            return items;
        }

        /// <summary>
        /// Get project display item.
        /// </summary>
        /// <param name="db">
        /// Data Context.
        /// </param>
        /// <param name="projectId">
        /// The project id.
        /// </param>
        /// <param name="isUserAdmin">
        /// The is User Admin.
        /// </param>
        /// <param name="userName">
        /// The user Name.
        /// </param>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <returns>
        /// Project Display Item.
        /// </returns>
        public static ProjectDisplayItem GetProjectDisplayItem(QutSensorsDb db, int projectId, bool isUserAdmin, string userName, Guid userId)
        {
            IQueryable<Entity_Item> projects;

            if (isUserAdmin)
            {
                projects = db.Entity_Items.Where(ei => ei.Entity_MetaData.Type == EntityType.Project);
            }
            else
            {
                projects = EntityManager.Instance.GetEntityItemsForView(
                    db, userId != Guid.Empty ? userId : new Guid?(), ei => ei.Entity_MetaData.Type == EntityType.Project);
            }

            var project = projects.Where(p => p.EntityID == projectId).FirstOrDefault();

            if (project == null)
            {
                return null;
            }

            // get project info
            const string DeploymentLink = "<a href='/UI/Deployment/DeploymentModify.aspx?{0}'>{1}</a>";
            const string DeploymentDisplay = "Sensors &ndash; <abbr title='Sensors functioning as expected.'>Healthy:</abbr> {0}, <abbr title='Sensors not functioning as expected.'>Unhealthy?</abbr> {1}.";

            const string DateInfo = "{0} {1:dd/MM/yyyy}<span class='smallTimeAgo'>{2}</span>";
            const string NoJobs = "No jobs.";
            const string NoJobResults = "No results from jobs.";
            const string JobDisplayCount = "Completed {0} of {1} job tasks.";
            const string NoLatestManualUpload = "No manual uploads.";

            const string NoAudioReadings = "No audio readings.";

            const string NoneHeardDuration = "Total {0} duration.";
            const string HeardDisplayDuration = "Heard {0} of {1}.";

            // not showing readings heard progress for now.
            const string NoneHeardReadings = "Total {0} audio readings.";
            const string HeardDisplayReadings = "Heard {0} of {1} audio readings.";

            const string HeardDisplayProgress = "<div class='DisplayProgressBar' percenttoshow='{0}' title='{0}%'></div>";

            var access = isUserAdmin
                                 ? EntityAccessLevel.Full
                                 : EntityManager.Instance.ProjectAccessLevel(db, project.EntityID, userId);
            var deployments = EntityManager.Instance.GetDescendants(
                db, project.EntityID, ei => ei.Entity_MetaData.DeploymentID.HasValue);
            var deploymentInfo =
                deployments.Where(d =>
                    d.Entity_MetaData.Deployment.IsActive &&
                    !d.Entity_MetaData.Deployment.IsTest &&
                    !d.Entity_MetaData.Deployment.Hardware.IsManualDevice &&
                    !(d.Entity_MetaData.Deployment.IsSensitive.HasValue && d.Entity_MetaData.Deployment.IsSensitive.Value))
                    .Select(d => d.Entity_MetaData).Select(d => new
                    {
                        DepEntityId = d.EntityID,
                        DepId = d.DeploymentID.Value,
                        Name = d.Entity_Item.Name,
                        LastContacted = d.Deployment.Hardware.LastContacted
                    }).ToList();
            var deploymentHealth =
                deploymentInfo.Select(
                    d =>
                    new
                    {
                        d.DepId,
                        Healthy = Deployment.IsHealthy(db, d.DepId),
                        d.Name,
                        d.DepEntityId,
                        LastContacted = d.LastContacted.HasValue ? d.LastContacted.Value : DateTime.MinValue
                    });
            var jobs = EntityManager.Instance.GetDescendants(db, project.EntityID, ei => ei.Entity_MetaData.JobID.HasValue);
            var jobCount = jobs.Count();
            var jobItems = jobs.SelectMany(ei => ei.Entity_MetaData.Processor_Job.Processor_JobItems);
            var mostRecentJobItem = jobItems.Where(ji => ji.Status == JobStatus.Complete).Max(ji => ji.WorkerAcceptedTimeUTC);
            var totalJobItems = jobItems.Count();
            var completeJobItems = jobItems.Count(ji => ji.Status == JobStatus.Complete || ji.Status == JobStatus.Error);
            var jobItemsCompletePercent = totalJobItems > 0 ? Math.Round(((double)completeJobItems / (double)totalJobItems) * 100.0, 0) : 0.0;
            var audioreadings = deployments.SelectMany(ei => ei.Entity_MetaData.Deployment.AudioReadings);
            var latestUploadDate = audioreadings.Where(ar => ar.Hardware.IsManualDevice).Max(ar => (DateTime?)ar.Time);
            var audioreadingsCount = audioreadings.Count();
            var heardReadingCount = audioreadings.Count(ar => db.ReadReadings.Any(rr => rr.AudioReadingID == ar.AudioReadingID && rr.UserName == userName));
            var heardReadingPercent = audioreadingsCount > 0 ? Math.Round(((double)heardReadingCount / (double)audioreadingsCount) * 100.0, 0) : 0.0;
            var audioTotalDuration = audioreadings.Sum(ar => (long?)ar.Length);
            var audioTotalDurationString = new TimeSpan(audioTotalDuration.HasValue ? (long)audioTotalDuration.Value * TimeSpan.TicksPerMillisecond : 0).ToReadableString();
            var heardDuration = audioreadings.Where(ar => db.ReadReadings.Any(rr => rr.AudioReadingID == ar.AudioReadingID && rr.UserName == userName)).Sum(ar => (long?)ar.Length);
            var heardDurationString = new TimeSpan(heardDuration.HasValue ? (long)heardDuration.Value * TimeSpan.TicksPerMillisecond : 0).ToReadableString();
            var heardDurationPercent = audioTotalDuration > 0 ? Math.Round((heardDuration.HasValue ? (double)heardDuration.Value : 0) / (audioTotalDuration.HasValue ? (double)audioTotalDuration.Value : 1) * 100.0, 0) : 0.0;

            var displayItem = new ProjectDisplayItem
            {
                DeploymentIssue =
                    isUserAdmin
                        ? string.Format(
                            DeploymentDisplay,
                            deploymentHealth.Where(d => d.Healthy.HasValue && d.Healthy.Value).Count(),
                            string.Join(
                                ", ",
                                deploymentHealth.Where(d => d.Healthy.HasValue && !d.Healthy.Value).Count() < 1 ? new string[] { "0" } :
                                deploymentHealth.Where(d => d.Healthy.HasValue && !d.Healthy.Value).Select(
                                    d =>
                                    string.Format(
                                        DeploymentLink,
                                        QutSensorsPageHelper.CreateQueryString(
                                            QutSensorsPageHelper.ENTITY_DEPLOYMENT_ID, d.DepEntityId),
                                        d.Name)).ToArray()))
                        : string.Format(
                            DeploymentDisplay,
                            deploymentHealth.Where(d => d.Healthy.HasValue && d.Healthy.Value).Count(),
                            deploymentHealth.Where(d => d.Healthy.HasValue && !d.Healthy.Value).Count()),

                HeardDisplayDuration =
                    audioreadingsCount == 0
                        ? NoAudioReadings
                        : heardReadingCount == 0
                              ? string.Format(NoneHeardDuration, audioTotalDurationString)
                              : string.Format(
                                  HeardDisplayDuration, heardDurationString, audioTotalDurationString),
                HeardDisplayDurationProgress =
                    audioreadingsCount > 0 && heardReadingCount > 0
                        ? string.Format(HeardDisplayProgress, heardDurationPercent)
                        : string.Empty,
                HeardDisplayReadings =
                    audioreadingsCount == 0
                        ? string.Empty
                        : heardReadingCount == 0
                              ? string.Format(NoneHeardReadings, audioreadingsCount)
                              : string.Format(HeardDisplayReadings, heardReadingCount, audioreadingsCount),
                HeardDisplayReadingsProgress =
                    audioreadingsCount > 0 && heardReadingCount > 0
                        ? string.Format(HeardDisplayProgress, heardReadingPercent)
                        : string.Empty,
                JobInfo =
                    jobCount == 0
                        ? NoJobs
                        : completeJobItems == 0
                              ? NoJobResults
                              : string.Format(JobDisplayCount, completeJobItems, totalJobItems),
                JobDisplayProgress =
                    jobCount > 0 && completeJobItems > 0
                        ? string.Format(HeardDisplayProgress, jobItemsCompletePercent)
                        : string.Empty,
                JobRecent =
                    mostRecentJobItem.HasValue
                        ? string.Format(
                            DateInfo,
                            "Last job activity",
                            mostRecentJobItem.Value.ToLocalTime(),
                            mostRecentJobItem.Value.ToDifferenceString(DateTime.UtcNow))
                        : string.Empty,
                MostRecentUpload =
                    latestUploadDate.HasValue
                        ? string.Format(
                            DateInfo,
                            "Latest manual upload",
                            latestUploadDate.Value,
                            latestUploadDate.Value.ToDifferenceString(DateTime.Now))
                        : NoLatestManualUpload,
                ProjectAccessString = access == EntityAccessLevel.Readonly ? "(read&nbsp;only)" : string.Empty,
                ProjectId = project.EntityID,
                ProjectName = project.Name,
                ProjectQs =
                    QutSensorsPageHelper.CreateQueryString(
                        QutSensorsPageHelper.ENTITY_PROJECT_ID, project.EntityID.ToString())
            };

            return displayItem;
        }

        /// <summary>
        /// Convert Entity Item to EntityInfo.
        /// </summary>
        /// <param name="item">
        /// The entity item.
        /// </param>
        /// <returns>
        /// EntityInfo representing Entity_Item.
        /// </returns>
        public static EntityInfo ToEntityInfo(Entity_Item item)
        {
            var info = new EntityInfo
            {
                EntityID = item.EntityID,
                Name = item.Name,
                Owner = item.Owner,
                Type = item.Entity_MetaData.Type,
                Notes = item.Entity_MetaData.Notes
            };

            if (item.Entity_MetaData.JobID.HasValue)
            {
                info.JobID = item.Entity_MetaData.JobID.Value;
            }

            if (item.Entity_MetaData.DeploymentID.HasValue)
            {
                info.DeploymentID = item.Entity_MetaData.DeploymentID.Value;
            }

            if (item.Entity_MetaData.Geography != null)
            {
                info.Latitude = (double)item.Entity_MetaData.Geography.Lat;
                info.Longitude = (double)item.Entity_MetaData.Geography.Long;
            }

            info.AnonymousAccess = item.Entity_MetaData.AnonymousAccess.HasValue && item.Entity_MetaData.AnonymousAccess.Value;

            return info;
        }

        /// <summary>
        /// Get all users and their access level to an entity.
        /// </summary>
        /// <param name="entityId">
        /// The entity id.
        /// </param>
        /// <returns>List of users with access level.
        /// </returns>
        public static IEnumerable<SecurityInfo> GetEntitySecurity(int entityId)
        {
            using (var db = new QutSensorsDb())
            {
                IQueryable<Entity_Permission> permissions = from ep in db.Entity_Permissions
                                                            where ep.EntityID == entityId
                                                            select ep;

                var members = from MembershipUser member in Membership.GetAllUsers()
                              select new { member.UserName, UserId = member.ProviderUserKey as Guid? };

                return
                    members.Select(
                        a =>
                        new SecurityInfo
                        {
                            EntityId = entityId,
                            UserName = a.UserName,
                            UserId = a.UserId,
                            AccessLevel =
                                permissions.Where(b => b.UserID == a.UserId).FirstOrDefault() == null
                                    ? EntityAccessLevel.None
                                    : permissions.Where(b => b.UserID == a.UserId).First().AccessLevel
                        }).ToArray();
            }
        }

        /// <summary>
        /// Convert Entity Items to entity Info.
        /// </summary>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// List of entity info.
        /// </returns>
        public static IEnumerable<EntityInfo> ToEntityInfoList(IEnumerable<Entity_Item> items, Guid userId)
        {
            List<EntityInfo> retVal = new List<EntityInfo>();

            foreach (Entity_Item item in items)
            {
                EntityInfo info = ToEntityInfo(item);
                using (QutSensorsDb db = new QutSensorsDb())
                {
                    info.AccessLevel = EntityManager.Instance.GetMaximumAccessLevelForUser(db, item.EntityID, userId, null);
                }
                retVal.Add(info);
            }

            return retVal;

        }

        /// <summary>
        /// Convert Entity Items to entity Info.
        /// </summary>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <returns>
        /// List of entity info.
        /// </returns>
        public static IEnumerable<EntityInfo> ToEntityInfoList(IEnumerable<Entity_Item> items)
        {
            return ToEntityInfoList(items, Guid.Empty);
        }

        /// <summary>
        /// Delete a project. Any associated entities will not be deleted.
        /// Child entities may not be accessable.
        /// </summary>
        /// <param name="projectEntityId">
        /// The project id.
        /// </param>
        /// <returns>
        /// True if project did exist and was deleted, otherwise false.
        /// </returns>
        public static bool DeleteProject(int projectEntityId)
        {
            return DeleteEntity(projectEntityId, EntityType.Project);
        }

        /// <summary>
        /// Delete a site. Any associated entities will not be deleted.
        /// Child entities may not be accessable.
        /// </summary>
        /// <param name="siteEntityId">
        /// The site entity id.
        /// </param>
        /// <returns>
        /// True if site did exist and was deleted, otherwise false.
        /// </returns>
        public static bool DeleteSite(int siteEntityId)
        {
            return DeleteEntity(siteEntityId, EntityType.Site);
        }

        /// <summary>
        /// Delete a data set. Any associated entities or jobs will not be deleted.
        /// </summary>
        /// <param name="dataSetEntityId">
        /// The data set entity id.
        /// </param>
        /// <returns>
        /// True if data set did exist and was deleted, otherwise false.
        /// </returns>
        public static bool DeleteDataSet(int dataSetEntityId)
        {
            return DeleteEntity(dataSetEntityId, EntityType.Filter);
        }

        /// <summary>
        /// Delete a job. The job must be cancelled first.
        /// </summary>
        /// <param name="jobEntityId">
        /// The job entity id.
        /// </param>
        /// <returns>
        /// True if job did exist and was deleted, otherwise false.
        /// </returns>
        public static bool DeleteJob(int jobEntityId)
        {
            /*
             * Deleting a job is a more involved process.
             * First check the entity and job are valid.
             * 
             * Then check that all job items are error, complete, or running. 
             * There can't be any ready, as the job processor 
             * will try to get/return them, causing an error once the job is deleted.
             * The job should be cancelled before calling this method.
             * There may be some items still processing when the job is cancelled, 
             * and they will cause errors when returned. 
             * We don't care about thse results, as the job has been deleted!
             * 
             * Audio tags that are linked to results must be updated to clear the result id.
             * 
             * Delete results, items, and the job, in that order.
             * 
             * Finally, delete the entity for the job.
             */

            using (var db = new QutSensorsDb())
            {
                var jobEntity =
                    db.Entity_Items.Where(
                        e =>
                        e.Entity_MetaData.Type == EntityType.Job && e.EntityID == jobEntityId &&
                        e.Entity_MetaData.JobID.HasValue).FirstOrDefault();

                if (jobEntity == null || !jobEntity.Entity_MetaData.JobID.HasValue)
                {
                    // must be valid entity, and must have job id.
                    return false;
                }

                var jobId = jobEntity.Entity_MetaData.JobID.Value;

                var job = db.Processor_Jobs.Where(j => j.JobID == jobId).FirstOrDefault();

                if (job == null)
                {
                    // must be valid job.
                    return false;
                }

                var jobItems = db.Processor_JobItems.Where(i => i.JobID == jobId);

                var itemsNotFinished = jobItems.Where(i => i.Status == JobStatus.Ready);
                if (itemsNotFinished.Count() > 0)
                {
                    // job must not contain items with status 'Ready'
                    return false;
                }

                var results = db.Processor_Results.Where(r => r.Processor_JobItem.JobID == jobId);

                var audioTagsMeta = from meta in db.AudioTags_MetaDatas
                                    join r in results.Select(re => re.ResultID) on meta.ProcessorResultID equals r into
                                        tagsLinkedResults
                                    select meta;



                // update audio tags to remove link to results.
                db.AudioTags_MetaDatas.UpdateBatch(
                    audioTagsMeta, at => new AudioTags_MetaData { ProcessorResultID = null });


                // delete results
                db.Processor_Results.DeleteAllOnSubmit(results);
                db.SubmitChanges();

                // delete job items.
                db.Processor_JobItems.DeleteAllOnSubmit(jobItems);
                db.SubmitChanges();

                // delete entity entry before deleting job due to FK.
                bool entityResult = DeleteEntity(jobEntityId, EntityType.Job);

                // return false if entity couldn't be deleted.
                if (entityResult == false)
                {
                    return false;
                }

                // delete job
                db.Processor_Jobs.DeleteOnSubmit(job);
                db.SubmitChanges();
            }

            return true;
        }

        /// <summary>
        /// Delete an entity. Any associated entities will not be deleted.
        /// Child entities may not be accessable.
        /// </summary>
        /// <param name="entityId">
        /// The project id.
        /// </param>
        /// <param name="entityType">
        /// The entity Type.
        /// </param>
        /// <returns>
        /// True if entity did exist and was deleted, otherwise false.
        /// </returns>
        private static bool DeleteEntity(int entityId, EntityType entityType)
        {
            try
            {
                using (var db = new QutSensorsDb())
                {
                    // TODO: what about entities with incorrect type?
                    var entity = db.Entity_Items
                        .Where(e => e.Entity_MetaData.Type == entityType && e.EntityID == entityId)
                        .FirstOrDefault();

                    if (entity == null)
                    {
                        return false;
                    }

                    /* 
                     * related info must be deleted as well
                     */

                    if (entity.Entity_MetaData.Type == EntityType.Project)
                    {
                        // only for projects
                        var entityPermissions = db.Entity_Permissions.Where(p => p.EntityID == entityId);
                        db.Entity_Permissions.DeleteAllOnSubmit(entityPermissions);
                        db.SubmitChanges();
                    }

                    // remove both parent and child rel'ships.
                    var entityRelationships = db.Entity_Relationships.Where(r => r.ParentID == entityId || r.ChildID == entityId);
                    db.Entity_Relationships.DeleteAllOnSubmit(entityRelationships);
                    db.SubmitChanges();

                    // remove metadata
                    var entityMetadata = db.Entity_MetaDatas.Where(m => m.EntityID == entityId);
                    db.Entity_MetaDatas.DeleteAllOnSubmit(entityMetadata);
                    db.SubmitChanges();

                    // remove item
                    db.Entity_Items.DeleteOnSubmit(entity);
                    db.SubmitChanges();

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
