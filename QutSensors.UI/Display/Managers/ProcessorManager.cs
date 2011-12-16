// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessorManager.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display.Managers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    using QutSensors.Data.Linq;
    using QutSensors.Shared;
    using QutSensors.UI.Display.Classes;

    /// <summary>
    /// The processor manager.
    /// </summary>
    public static class ProcessorManager
    {
        /// <summary>
        /// The get job results.
        /// </summary>
        /// <param name="jobId">
        /// The job id.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        /// <param name="startIndex">
        /// The start index.
        /// </param>
        /// <param name="sortExpression">
        /// The sort Expression.
        /// </param>
        /// <returns>
        /// Selected page of results for jobId matching filter.
        /// </returns>
        public static IEnumerable<JobResult> GetJobResults(int jobId, Expression<Func<Processor_Result, bool>> filter, int maxItems, int startIndex, string sortExpression)
        {
            using (var db = new QutSensorsDb())
            {
                return GetJobResults(db, db.Processor_Results.Where(r => r.Processor_JobItem.JobID == jobId).Where(filter), jobId, maxItems, startIndex, sortExpression).ToList();
            }
        }

        /// <summary>
        /// The count job results.
        /// </summary>
        /// <param name="jobId">
        /// The job id.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// Number of results for job matching filter.
        /// </returns>
        public static int CountJobResults(int jobId, Expression<Func<Processor_Result, bool>> filter)
        {
            using (var db = new QutSensorsDb())
            {
                var query = db.Processor_Results.Where(r => r.Processor_JobItem.JobID == jobId).Where(filter);
                return query.Where(r => r.RankingScoreValue.HasValue || r.StartTime.HasValue).Count();
            }
        }

        /// <summary>
        /// The get job results.
        /// </summary>
        /// <param name="jobId">
        /// The job id.
        /// </param>
        /// <param name="audioReadingId">
        /// The audio reading id.
        /// </param>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        /// <param name="startIndex">
        /// The start index.
        /// </param>
        /// <param name="sortExpression">
        /// The sort Expression.
        /// </param>
        /// <returns>
        /// Selected page of results for jobId matching filter for audio reading.
        /// </returns>
        public static IEnumerable<JobResult> GetJobResults(int jobId, Guid audioReadingId, int maxItems, int startIndex, string sortExpression)
        {
            using (var db = new QutSensorsDb())
            {
                var query = db.Processor_Results.Where(r => r.Processor_JobItem.JobID == jobId && r.Processor_JobItem.AudioReadingID == audioReadingId);
                var results = GetJobResults(db, query, jobId, maxItems, startIndex, sortExpression);

                return results.ToList();
            }
        }

        /// <summary>
        /// The count job results.
        /// </summary>
        /// <param name="jobId">
        /// The job id.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <param name="audioReadingId">
        /// The audio reading id.
        /// </param>
        /// <returns>
        /// Number of results.
        /// </returns>
        public static int CountJobResults(int jobId, Expression<Func<Processor_Result, bool>> filter, Guid audioReadingId)
        {
            using (var db = new QutSensorsDb())
            {
                var query =
                    db.Processor_Results.Where(
                        r => r.Processor_JobItem.JobID == jobId && r.Processor_JobItem.AudioReadingID == audioReadingId)
                        .Where(filter);

                return query.Where(r => r.RankingScoreValue.HasValue || r.StartTime.HasValue).Count();
            }
        }

        /// <summary>
        /// Get job results from Processor Results.
        /// </summary>
        /// <param name="db">
        /// Data Context.
        /// </param>
        /// <param name="results">
        /// Processor Results.
        /// </param>
        /// <param name="jobId">
        /// Processor Job Id.
        /// </param>
        /// <param name="maxItems">
        /// Max items.
        /// </param>
        /// <param name="startIndex">
        /// Start index.
        /// </param>
        /// <param name="sortExpression">
        /// sort expression.
        /// </param>
        /// <returns>
        /// Job result list.
        /// </returns>
        public static IEnumerable<JobResult> GetJobResults(QutSensorsDb db, IQueryable<Processor_Result> results, int jobId, int maxItems, int startIndex, string sortExpression)
        {
            switch (sortExpression)
            {
                case "ResultId":
                    results = results.OrderBy(r => r.ResultID);
                    break;
                case "ResultId DESC":
                    results = results.OrderByDescending(r => r.ResultID);
                    break;
                case "Score":
                    results = results.OrderBy(r => r.RankingScoreValue);
                    break;
                case "Score DESC":
                    results = results.OrderByDescending(r => r.RankingScoreValue);
                    break;
                case "ScoreName":
                    results = results.OrderBy(r => r.RankingScoreName);
                    break;
                case "ScoreName DESC":
                    results = results.OrderByDescending(r => r.RankingScoreName);
                    break;
                case "StartTime":
                    results = results.OrderBy(r => r.StartTime);
                    break;
                case "StartTime DESC":
                    results = results.OrderByDescending(r => r.StartTime);
                    break;
                case "EndTime":
                    results = results.OrderBy(r => r.EndTime);
                    break;
                case "EndTime DESC":
                    results = results.OrderByDescending(r => r.EndTime);
                    break;
                case "MinFreq":
                    results = results.OrderBy(r => r.MinFrequency);
                    break;
                case "MinFreq DESC":
                    results = results.OrderByDescending(r => r.MinFrequency);
                    break;
                case "MaxFreq":
                    results = results.OrderBy(r => r.MaxFrequency);
                    break;
                case "MaxFreq DESC":
                    results = results.OrderByDescending(r => r.MaxFrequency);
                    break;
            }

            results = results.Skip(startIndex).Take(maxItems).Where(r => r.RankingScoreValue.HasValue || r.StartTime.HasValue);

            var show = results.ToList().Select(r =>
                    new JobResult
                    {
                        AudioReadingId = r.Processor_JobItem.AudioReadingID,
                        WorkItemId = r.Processor_JobItem.JobItemID,
                        Score = r.RankingScoreValue,
                        ScoreName = r.RankingScoreName,
                        StartTime = r.StartTime,
                        EndTime = r.EndTime,
                        MinFreq = r.MinFrequency,
                        MaxFreq = r.MaxFrequency,
                        Details = r.Results != null ? r.Results.ToXmlElement() : null,
                        JobId = jobId,
                        DeploymentName = r.Processor_JobItem.AudioReading.Deployment.Name,
                        ResultId = r.ResultID
                    });

            return show.ToList();
        }

        /// <summary>
        /// The get job work items.
        /// </summary>
        /// <param name="jobId">
        /// The job id.
        /// </param>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        /// <param name="startIndex">
        /// The start index.
        /// </param>
        /// <param name="sortExpression">sort expression.</param>
        /// <returns>Work items for job.
        /// </returns>
        public static IEnumerable<JobWorkItem> GetJobWorkItems(int jobId, int maxItems, int startIndex, string sortExpression)
        {
            using (var db = new QutSensorsDb())
            {
                var job = db.Processor_Jobs.Where(j => j.JobID == jobId).SingleOrDefault();
                if (job == null)
                {
                    return new List<JobWorkItem>();
                }

                var results = db.Processor_JobItems.Where(ji => ji.JobID == jobId);

                switch (sortExpression)
                {
                    case "JobItemId":
                        results = results.OrderBy(r => r.JobItemID);
                        break;
                    case "JobItemId DESC":
                        results = results.OrderByDescending(r => r.JobItemID);
                        break;
                    case "Status":
                        results = results.OrderBy(r => r.Status);
                        break;
                    case "Status DESC":
                        results = results.OrderByDescending(r => r.Status);
                        break;
                    case "Worker":
                        results = results.OrderBy(r => r.Worker);
                        break;
                    case "Worker DESC":
                        results = results.OrderByDescending(r => r.Worker);
                        break;
                    case "AcceptedTime":
                        results = results.OrderBy(r => r.WorkerAcceptedTimeUTC);
                        break;
                    case "AcceptedTime DESC":
                        results = results.OrderByDescending(r => r.WorkerAcceptedTimeUTC);
                        break;
                    case "DeploymentName":
                        results = results.OrderBy(r => r.AudioReading.Deployment.Name);
                        break;
                    case "DeploymentName DESC":
                        results = results.OrderByDescending(r => r.AudioReading.Deployment.Name);
                        break;
                    case "StartTime":
                        results = results.OrderBy(r => r.StartTime);
                        break;
                    case "StartTime DESC":
                        results = results.OrderByDescending(r => r.StartTime);
                        break;
                    case "EndTime":
                        results = results.OrderBy(r => r.EndTime);
                        break;
                    case "EndTime DESC":
                        results = results.OrderByDescending(r => r.EndTime);
                        break;
                    case "AudioReadingTotalLength":
                        results = results.OrderBy(r => r.AudioReading.DurationMs);
                        break;
                    case "AudioReadingTotalLength DESC":
                        results = results.OrderByDescending(r => r.AudioReading.DurationMs);
                        break;
                    case "TotalJobItemResults":
                        results = results.OrderBy(r => r.Processor_Results.Count());
                        break;
                    case "TotalJobItemResults DESC":
                        results = results.OrderByDescending(r => r.Processor_Results.Count());
                        break;
                }

                var items =
                    results.Skip(startIndex).Take(maxItems).Select(ji => new { J = ji, A = ji.AudioReading }).ToList();

                var show = items.Select(ji =>
                        new JobWorkItem
                            {
                                JobItemId = ji.J.JobItemID,
                                AudioReadingId = ji.J.AudioReadingID,
                                Status = ji.J.Status,
                                Worker = ji.J.Worker,
                                AcceptedTime =
                                    ji.J.WorkerAcceptedTimeUTC.HasValue
                                        ? new DateTime(ji.J.WorkerAcceptedTimeUTC.Value.Ticks, DateTimeKind.Utc).ToString()
                                        : "Pending",
                                ItemrunDetails = ji.J.ItemRunDetails,
                                AudioReadingTotalLength =
                                    ji.A.DurationMs.HasValue
                                        ? new TimeSpan(0, 0, 0, 0, ji.A.DurationMs.Value).ToReadableString()
                                        : "unknown",
                                EndTime =
                                    ji.J.EndTime.HasValue
                                        ? new TimeSpan(0, 0, 0, 0, ji.J.EndTime.Value).ToReadableString()
                                        : "not specified",
                                StartTime =
                                    ji.J.StartTime.HasValue
                                        ? new TimeSpan(0, 0, 0, 0, ji.J.StartTime.Value).ToReadableString()
                                        : "0",
                                AudioReadingSegmentLength =
                                    !ji.A.DurationMs.HasValue
                                        ? "unknown"
                                        : new TimeSpan(0, 0, 0, 0, (ji.J.EndTime.HasValue ? ji.J.EndTime.Value :
                                            ji.A.DurationMs.Value) - (ji.J.StartTime.HasValue ? ji.J.StartTime.Value : 0)).ToReadableString(),
                                FilterQs =
                                    (new ReadingsFilter { CommaSeparatedAudioReadingIds = ji.J.AudioReadingID.ToString() }).
                                    ToQueryString(true),
                                TotalJobItemResults = db.Processor_Results.Where(pr => pr.JobItemID == ji.J.JobItemID).Count(),
                                DeploymentName =
                                    ji.A.DeploymentID.HasValue ? ji.A.Deployment.Name : "Untitled",
                            });

                return show.ToList();
            }
        }

        /// <summary>
        /// Get up to maxItems processor types from startIndex, sorted by sortExpression.
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
        /// <returns>
        /// Enumerable Processor_Type.
        /// </returns>
        public static IEnumerable<Processor_Type> GetProcessorTypes(int maxItems, int startIndex, string sortExpression)
        {
            using (var db = new QutSensorsDb())
            {
                IOrderedQueryable<Processor_Type> processorTypes = db.Processor_Types.OrderBy(pt => pt.DisplayName);

                switch (sortExpression)
                {
                    case "Name":
                        processorTypes = processorTypes.OrderBy(a => a.DisplayName);
                        break;
                    case "Name DESC":
                        processorTypes = processorTypes.OrderByDescending(a => a.DisplayName);
                        break;
                    case "ProcessorName":
                        processorTypes = processorTypes.OrderBy(a => a.ProcessorName);
                        break;
                    case "ProcessorName DESC":
                        processorTypes = processorTypes.OrderByDescending(a => a.ProcessorName);
                        break;
                    case "LatestVersion":
                        processorTypes = processorTypes.OrderBy(a => a.LatestVersion);
                        break;
                    case "LatestVersion DESC":
                        processorTypes = processorTypes.OrderByDescending(a => a.LatestVersion);
                        break;
                    case "AdditionalData":
                        processorTypes = processorTypes.OrderBy(a => a.AdditionalData);
                        break;
                    case "AdditionalData DESC":
                        processorTypes = processorTypes.OrderByDescending(a => a.AdditionalData);
                        break;
                    case "Settings":
                        processorTypes = processorTypes.OrderBy(a => a.Settings);
                        break;
                    case "Settings DESC":
                        processorTypes = processorTypes.OrderByDescending(a => a.Settings);
                        break;
                }

                return processorTypes.Skip(startIndex).Take(maxItems).ToList();
            }
        }

        /// <summary>
        /// Get the number of processor types.
        /// </summary>
        /// <returns>
        /// Number of processor types.
        /// </returns>
        public static int CountProcessorTypes()
        {
            using (var db = new QutSensorsDb())
            {
                return db.Processor_Types.Count();
            }
        }

        /// <summary>
        /// The count job work items.
        /// </summary>
        /// <param name="jobId">
        /// The job id.
        /// </param>
        /// <returns>
        /// Number of work items.
        /// </returns>
        public static int CountJobWorkItems(int jobId)
        {
            var count = 0;
            using (var db = new QutSensorsDb())
            {
                var job = db.Processor_Jobs.Where(j => j.JobID == jobId).SingleOrDefault();
                if (job != null)
                {
                    count = db.Processor_JobItems.Where(ji => ji.JobID == jobId).Count();
                }
            }

            return count;
        }

        /// <summary>
        /// Get list of audio readings in a job.
        /// </summary>
        /// <param name="jobId">
        /// The job id.
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
        /// <returns>
        /// Audio readings in job.
        /// </returns>
        public static IEnumerable<JobAudioReading> GetJobAudioReadings(int jobId, int maxItems, int startIndex, string sortExpression)
        {
            using (var db = new QutSensorsDb())
            {
                var job = db.Processor_Jobs.Where(j => j.JobID == jobId).SingleOrDefault();

                if (job == null)
                {
                    return new List<JobAudioReading>();
                }

                // ADVANCED CRYPTOGRAPHY: jiAr is a list of job items associated with each audio reading.

                var results = from ji in db.Processor_JobItems
                              where ji.JobID == jobId
                              group ji by ji.AudioReading
                                  into jiAr
                                  select new
                                      {
                                          AudioReadingId = jiAr.Key.AudioReadingID,
                                          ReadingStart = jiAr.Key.Time,
                                          ReadingLength = jiAr.Key.DurationMs,
                                          HardwareName = jiAr.Key.Hardware.FriendlyName,
                                          JobResultCount = db.Processor_Results.Where(
                                              procr => procr.Processor_JobItem.AudioReadingID == jiAr.Key.AudioReadingID &&
                                                    procr.Processor_JobItem.JobID == jobId).Count(),
                                          MaxScore = db.Processor_Results.Where(
                                              procr => procr.Processor_JobItem.AudioReadingID == jiAr.Key.AudioReadingID &&
                                                    procr.Processor_JobItem.JobID == jobId).Max(r => r.RankingScoreValue),
                                          MinScore = db.Processor_Results.Where(
                                              procr => procr.Processor_JobItem.AudioReadingID == jiAr.Key.AudioReadingID &&
                                                    procr.Processor_JobItem.JobID == jobId).Min(r => r.RankingScoreValue)
                                      };

                switch (sortExpression)
                {
                    case "ReadingStart":
                        results = results.OrderBy(r => r.ReadingStart);
                        break;
                    case "ReadingStart DESC":
                        results = results.OrderByDescending(r => r.ReadingStart);
                        break;
                    case "ReadingEnd":
                        results = results.OrderBy(r => r.ReadingLength.HasValue ? r.ReadingStart.AddMilliseconds(r.ReadingLength.Value) : r.ReadingStart);
                        break;
                    case "ReadingEnd DESC":
                        results = results.OrderByDescending(r => r.ReadingLength.HasValue ? r.ReadingStart.AddMilliseconds(r.ReadingLength.Value) : r.ReadingStart);
                        break;
                    case "ReadingDuration":
                        results = results.OrderBy(r => r.ReadingLength);
                        break;
                    case "ReadingDuration DESC":
                        results = results.OrderByDescending(r => r.ReadingLength);
                        break;
                    case "JobResultCount":
                        results = results.OrderBy(r => r.JobResultCount);
                        break;
                    case "JobResultCount DESC":
                        results = results.OrderByDescending(r => r.JobResultCount);
                        break;
                    case "MaxScore":
                        results = results.OrderBy(r => r.MaxScore);
                        break;
                    case "MaxScore DESC":
                        results = results.OrderByDescending(r => r.MaxScore);
                        break;
                    case "MinScore":
                        results = results.OrderBy(r => r.MinScore);
                        break;
                    case "MinScore DESC":
                        results = results.OrderByDescending(r => r.MinScore);
                        break;
                    default:
                        results =
                            results.OrderByDescending(r => r.MaxScore).ThenByDescending(r => r.JobResultCount).
                                ThenByDescending(r => r.ReadingStart);
                        break;
                }

                var items =
                    results.Skip(startIndex).Take(maxItems).ToList();

                var show = items.Select(i =>
                        new JobAudioReading
                        {
                            AudioReadingId = i.AudioReadingId,
                            ReadingStart = i.ReadingStart.ToString("yyy-MM-dd HH:mm:ss"),
                            ReadingEnd = i.ReadingLength.HasValue ? i.ReadingStart.AddMilliseconds(i.ReadingLength.Value).ToString("yyy-MM-dd HH:mm:ss") : "unknown",
                            ReadingDuration = i.ReadingLength.HasValue ? new TimeSpan(i.ReadingLength.Value * TimeSpan.TicksPerMillisecond).ToReadableString() : "unknown",
                            MaxScore = i.MaxScore,
                            MinScore = i.MinScore,
                            JobResultCount = i.JobResultCount
                        });

                return show.ToList();
            }
        }

        /// <summary>
        /// Count Job audio readings.
        /// </summary>
        /// <param name="jobId">
        /// The job id.
        /// </param>
        /// <returns>
        /// Number of audio readings in job.
        /// </returns>
        public static int CountJobAudioReadings(int jobId)
        {
            var count = 0;
            using (var db = new QutSensorsDb())
            {
                var job = db.Processor_Jobs.Where(j => j.JobID == jobId).SingleOrDefault();
                if (job != null)
                {
                    var query = from ji in db.Processor_JobItems
                                where ji.JobID == jobId
                                group ji by ji.AudioReading
                                    into jiAr
                                    select jiAr.Key.AudioReadingID;
                    count = query.Count();
                }
            }

            return count;
        }

        /// <summary>
        /// Delete a processor Type.
        /// </summary>
        /// <param name="processorTypeId">
        /// The processor type id.
        /// </param>
        /// <returns>
        /// True if processor type existed and was deeleted, otherwise false.
        /// </returns>
        public static bool DeleteProcessorType(int processorTypeId)
        {
            if (processorTypeId < 1) return false;

            using (var db = new QutSensorsDb())
            {
                var processorType = db.Processor_Types.Where(p => p.ProcessorTypeId == processorTypeId).FirstOrDefault();
                if (processorType != null)
                {
                    db.Processor_Types.DeleteOnSubmit(processorType);
                    db.SubmitChanges();
                    return true;
                }
            }

            return false;
        }

        private static string IndentXml(XmlNode element)
        {
            var sb = new StringBuilder();
            using (var stringWriter = new StringWriter(sb))
            using (var xmlWriter = new XmlTextWriter(stringWriter))
            {
                xmlWriter.Formatting = Formatting.Indented;

                var doc = new XmlDocument();
                doc.AppendChild(element);
                doc.Save(xmlWriter);
            }

            return sb.ToString();
        }
    }
}