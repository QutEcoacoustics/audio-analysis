// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TagDisplayManager.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Tag Display Manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using QutSensors.Data;
    using QutSensors.Data.Linq;
    using QutSensors.UI.Display.Classes;
    using QutSensors.UI.Security;

    /// <summary>
    /// Tag Display Manager.
    /// </summary>
    public static class TagDisplayManager
    {
        /// <summary>
        /// Get tag completion list for partial text. Can be accesed by admin or normal user or anon.
        /// </summary>
        /// <param name="partialText">
        /// The prefix text.
        /// </param>
        /// <param name="count">
        /// Maximum number of tags.
        /// </param>
        /// <returns>
        /// List of matching tags. 
        /// </returns>
        public static IEnumerable<string> GetAudioTags(string partialText, int count)
        {
            using (var db = new QutSensorsDb())
            {
                IQueryable<AudioTag> ats;

                if (AuthenticationHelper.IsCurrentUserAdmin)
                {
                    ats = db.AudioTags.AsQueryable();
                }
                else
                {
                    ats =
                        EntityManager.Instance.GetEntityItemsForView(
                            db, AuthenticationHelper.CurrentUserId, ei => ei.Entity_MetaData.DeploymentID.HasValue).
                            SelectMany(ei => ei.Entity_MetaData.Deployment.AudioReadings).SelectMany(ar => ar.AudioTags);
                }

                return ats
                    .Where(at => at.Tag.ToLower().Contains(partialText.Trim().ToLower()))
                    .OrderByDescending(at => at.Tag)
                    .Take(count)
                    .Select(at => at.Tag)
                    .Distinct()
                    .ToArray();
            }
        }

        /// <summary>
        /// Get tag completion list for partial text. Can be accesed by admin or normal user or anon.
        /// Returns only reference tags.
        /// </summary>
        /// <param name="partialText">
        /// The prefix text.
        /// </param>
        /// <param name="count">
        /// Maximum number of tags.
        /// </param>
        /// <returns>
        /// List of matching tags. 
        /// </returns>
        public static IEnumerable<string> GetAudioRefTags(string partialText, int count)
        {
            using (var db = new QutSensorsDb())
            {
                IQueryable<AudioTag> ats;

                if (AuthenticationHelper.IsCurrentUserAdmin)
                {
                    ats = db.AudioTags.AsQueryable();
                }
                else
                {
                    ats =
                        EntityManager.Instance.GetEntityItemsForView(
                            db, AuthenticationHelper.CurrentUserId, ei => ei.Entity_MetaData.DeploymentID.HasValue).
                            SelectMany(ei => ei.Entity_MetaData.Deployment.AudioReadings).SelectMany(ar => ar.AudioTags);
                }

                return ats
                    .Where(at => at.Tag.ToLower().Contains(partialText.Trim().ToLower()))
                    .Where(at => at.AudioTags_MetaData.ReferenceTag.HasValue && at.AudioTags_MetaData.ReferenceTag.Value)
                    .OrderByDescending(at => at.Tag)
                    .Take(count)
                    .Select(at => at.Tag)
                    .Distinct()
                    .ToArray();
            }
        }

        /// <summary>
        /// Get aggregate tag count. Only accessed by admin.
        /// </summary>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        /// <param name="startIndex">
        /// The start Index.
        /// </param>
        /// <param name="sortExpression">
        /// The sort Expression.
        /// </param>
        /// <returns>
        /// List of aggregate tags.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <c>InvalidOperationException</c>.
        /// </exception>
        public static IEnumerable<TagAggregateItem> GetAudioTagsAggregate(int maxItems, int startIndex, string sortExpression)
        {
            if (!AuthenticationHelper.IsCurrentUserAdmin)
            {
                throw new InvalidOperationException(
                    "Must be admin to access. User id: " + AuthenticationHelper.CurrentUserId);
            }

            using (var db = new QutSensorsDb())
            {
                var query = db.AudioTags.GroupBy(at => at.Tag);

                switch (sortExpression)
                {
                    case "TagName":
                        query = query.OrderBy(a => a.Key).ThenByDescending(a => a.Count());
                        break;
                    case "TagName DESC":
                        query = query.OrderByDescending(a => a.Key).ThenByDescending(a => a.Count());
                        break;
                    case "Count":
                        query = query.OrderBy(a => a.Count()).ThenBy(a => a.Key);
                        break;
                    case "Count DESC":
                        query = query.OrderByDescending(a => a.Count()).ThenBy(a => a.Key);
                        break;
                    default:
                        query = query.OrderBy(a => a.Key).ThenByDescending(a => a.Count());
                        break;
                }

                var queryItems = query.Select(q => new
                    {
                        Tag = q.Key,
                        Count = q.Count()
                    });

                if (maxItems > 0 && queryItems.Count() > 0)
                {
                    var items = queryItems
                        .Skip(startIndex)
                        .Take(maxItems)
                        .ToList()
                        .Select(t => new TagAggregateItem
                            {
                                TagName = t.Tag,
                                Count = t.Count
                            });

                    return items;
                }

                return new List<TagAggregateItem>();
            }
        }

        /// <summary>
        /// Get count of audio tags aggregate.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <c>InvalidOperationException</c>.
        /// </exception>
        /// <returns>
        /// The number of aggregate audio tags.
        /// </returns>
        public static int CountAudioTagsAggregate()
        {
            if (!AuthenticationHelper.IsCurrentUserAdmin)
            {
                throw new InvalidOperationException(
                    "Must be admin to access. User id: " + AuthenticationHelper.CurrentUserId);
            }

            using (var db = new QutSensorsDb())
            {
                var query = db.AudioTags
                    .GroupBy(at => at.Tag)
                    .Select(groupedTags => new
                    {
                        Tag = groupedTags.Key,
                        Count = groupedTags.Count()
                    });

                int count = query.Count();
                return count;
            }
        }

        /// <summary>
        /// Update one tag name to another.
        /// </summary>
        /// <param name="count">
        /// The count.
        /// </param>
        /// <param name="tagName">
        /// The tag Name.
        /// </param>
        /// <param name="oldValueTagName">
        /// The old Value Tag Name.
        /// </param>
        public static void UpdateAudioTagAggregate(int count, string tagName, string oldValueTagName)
        {
            if (count > 0 && !string.IsNullOrEmpty(tagName) && !string.IsNullOrEmpty(oldValueTagName) && tagName != oldValueTagName)
            {
                // update all tags with 'oldValueTagName' to 'tagName'
                using (var db = new QutSensorsDb())
                {
                    int rowsAffected = db.ExecuteCommand("UPDATE AudioTags SET Tag = {0} WHERE Tag = {1}", tagName, oldValueTagName);
                }
            }
            else if (count > 0 && !string.IsNullOrEmpty(tagName) && string.IsNullOrEmpty(oldValueTagName) && tagName != oldValueTagName)
            {
                // for updating tags that are empty string.
                using (var db = new QutSensorsDb())
                {
                    int rowsAffected = db.ExecuteCommand("UPDATE AudioTags SET Tag = {0} WHERE Tag IS NULL OR Tag =''", tagName);
                }
            }
        }

        /// <summary>
        /// Get audio reference tags for display as list.
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
        /// <param name="partialTagName">
        /// The partial Tag Name.
        /// </param>
        /// <returns>
        /// List of audio tags for html player.
        /// </returns>
        public static IEnumerable<TagPlayItem> GetAudioRefTags(int maxItems, int startIndex, string sortExpression, string partialTagName)
        {
            using (var db = new QutSensorsDb())
            {
                var query = db.AudioTags.AsQueryable();

                Guid? user = AuthenticationHelper.CurrentUserId;

                // restrict to ref. tags
                query = query.Where(at => at.AudioTags_MetaData.ReferenceTag.HasValue && at.AudioTags_MetaData.ReferenceTag.Value);

                if (!AuthenticationHelper.IsCurrentUserAdmin)
                {
                    // only show ref tags that user can access.
                    var deps =
                        EntityManager.Instance.GetEntityItemsForView(
                            db, user, (ei) => ei.Entity_MetaData.DeploymentID.HasValue).Select(
                                (d) => d.Entity_MetaData.Deployment);

                    query = from a in query
                            join d in deps on a.AudioReading.Deployment.DeploymentID equals d.DeploymentID
                            select a;
                }

                if (!string.IsNullOrEmpty(partialTagName))
                {
                    query = query.Where(at => at.Tag.Contains(partialTagName));
                }

                switch (sortExpression)
                {
                    case "TagName":
                        query = query.OrderBy(a => a.Tag).ThenBy(a => a.AudioReading.Deployment.Name);
                        break;
                    case "TagName DESC":
                        query = query.OrderByDescending(a => a.Tag).ThenBy(a => a.AudioReading.Deployment.Name);
                        break;
                    case "RelativeStart":
                        query = query.OrderBy(a => a.StartTime).ThenBy(a => a.Tag).ThenBy(a => a.AudioReading.Deployment.Name);
                        break;
                    case "RelativeStart DESC":
                        query = query.OrderByDescending(a => a.StartTime).ThenBy(a => a.Tag).ThenBy(a => a.AudioReading.Deployment.Name);
                        break;
                    case "RelativeEnd":
                        query = query.OrderBy(a => a.EndTime).ThenBy(a => a.Tag).ThenBy(a => a.AudioReading.Deployment.Name);
                        break;
                    case "RelativeEnd DESC":
                        query = query.OrderByDescending(a => a.EndTime).ThenBy(a => a.Tag).ThenBy(a => a.AudioReading.Deployment.Name);
                        break;
                    case "AbsoluteStart":
                        query = query.OrderBy(a => a.AudioReading.Time.AddMilliseconds(a.StartTime)).ThenBy(a => a.Tag).ThenBy(a => a.AudioReading.Deployment.Name);
                        break;
                    case "AbsoluteStart DESC":
                        query = query.OrderByDescending(a => a.AudioReading.Time.AddMilliseconds(a.StartTime)).ThenBy(a => a.Tag).ThenBy(a => a.AudioReading.Deployment.Name);
                        break;
                    case "AbsoluteEnd":
                        query = query.OrderBy(a => a.AudioReading.Time.AddMilliseconds(a.EndTime)).ThenBy(a => a.Tag).ThenBy(a => a.AudioReading.Deployment.Name);
                        break;
                    case "AbsoluteEnd DESC":
                        query = query.OrderByDescending(a => a.AudioReading.Time.AddMilliseconds(a.EndTime)).ThenBy(a => a.Tag).ThenBy(a => a.AudioReading.Deployment.Name);
                        break;
                    case "DeploymentName":
                        query = query.OrderBy(a => a.AudioReading.Deployment.Name).ThenBy(a => a.Tag);
                        break;
                    case "DeploymentName DESC":
                        query = query.OrderByDescending(a => a.AudioReading.Deployment.Name).ThenBy(a => a.Tag);
                        break;
                    default:
                        query = query.OrderBy(a => a.Tag).ThenBy(a => a.AudioReading.Deployment.Name);
                        break;
                }

                query = query.Skip(startIndex).Take(maxItems);

                var queryItems = query.Select(q => new TagPlayItem
                {
                    AudioId = q.AudioReadingID,
                    AudioAbsoluteStart = q.AudioReading.Time,
                    AudioDuration = q.AudioReading.Length.HasValue ? TimeSpan.FromMilliseconds(q.AudioReading.Length.Value) : TimeSpan.Zero,
                    TagId = q.AudioTagID,
                    DeploymentName = q.AudioReading.Deployment.Name,
                    TagIsReference = q.AudioTags_MetaData.ReferenceTag.HasValue ? q.AudioTags_MetaData.ReferenceTag.Value : false,
                    TagRelativeEnd = TimeSpan.FromMilliseconds(q.EndTime),
                    TagRelativeStart = TimeSpan.FromMilliseconds(q.StartTime),
                    TagName = q.Tag,
                    TagFrequencyMax = Convert.ToInt32(q.EndFrequency),
                    TagFrequencyMin = Convert.ToInt32(q.StartFrequency)
                }).ToList();

                return queryItems;
            }
        }

        /// <summary>
        /// Get count of reference tags.
        /// </summary>
        /// <param name="partialTagName">
        /// The partial Tag Name.
        /// </param>
        /// <returns>
        /// Number of reference tags.
        /// </returns>
        public static int CountAudioRefTags(string partialTagName)
        {
            using (var db = new QutSensorsDb())
            {
                var query = db.AudioTags.AsQueryable();

                Guid? user = AuthenticationHelper.CurrentUserId;

                // restrict to ref. tags
                query = query.Where(at => at.AudioTags_MetaData.ReferenceTag.HasValue && at.AudioTags_MetaData.ReferenceTag.Value);

                if (!AuthenticationHelper.IsCurrentUserAdmin)
                {
                    // only show ref tags that user can access.
                    var deps =
                        EntityManager.Instance.GetEntityItemsForView(
                            db, user, (ei) => ei.Entity_MetaData.DeploymentID.HasValue).Select(
                                (d) => d.Entity_MetaData.Deployment);

                    query = from a in query
                            join d in deps on a.AudioReading.Deployment.DeploymentID equals d.DeploymentID
                            select a;
                }

                if (!string.IsNullOrEmpty(partialTagName))
                {
                    query = query.Where(at => at.Tag.Contains(partialTagName));
                }

                return query.Count();
            }
        }

        /// <summary>
        /// Get Audio Tag Count Info.
        /// </summary>
        /// <param name="sortExpression">
        /// The sort expression.
        /// </param>
        /// <param name="intervalMinutes">
        /// The interval minutes.
        /// </param>
        /// <returns>
        /// Tag Count Info list.
        /// </returns>
        public static IEnumerable<TagCountInfo> GetAudioTagCountInfo(string sortExpression, int intervalMinutes)
        {
            if (intervalMinutes < 1)
            {
                intervalMinutes = 30;
            }

            if (string.IsNullOrEmpty(sortExpression))
            {
                sortExpression = "FromCalculated";
            }

            using (var db = new QutSensorsDb())
            {
                string query =
                    @"
declare @interval int = {0}
select count(*) as TagCount, 
((datepart(hour,dateadd(millisecond,StartTime,a.[Time]))*60) + datepart(minute,dateadd(millisecond,StartTime,a.[Time])))/@interval*@interval as FromCalculated,
(((datepart(hour,dateadd(millisecond,StartTime,a.[Time]))*60) + datepart(minute,dateadd(millisecond,StartTime,a.[Time])))/@interval) *@interval+@interval as ToCalculated,
min((datepart(hour,dateadd(millisecond,StartTime,a.[Time]))*60) + datepart(minute,dateadd(millisecond,StartTime,a.[Time]))) as FromData,
MAX((datepart(hour,dateadd(millisecond,StartTime,a.[Time]))*60) + datepart(minute,dateadd(millisecond,StartTime,a.[Time]))) as ToData
from AudioTags t 
left outer join AudioReadings a on t.AudioReadingID = a.AudioReadingID
group by 
((datepart(hour,dateadd(millisecond,StartTime,a.[Time]))*60) + datepart(minute,dateadd(millisecond,StartTime,a.[Time])))/@interval
order by {1}
";

                var items = db.ExecuteQuery(string.Format(query, intervalMinutes, sortExpression));

                if (items.Count() > 0)
                {
                    var itemDisplay =
                        items.Select(
                            i =>
                            new TagCountInfo
                                {
                                    TagCount = int.Parse((i["TagCount"] ?? string.Empty).ToString()),
                                    FromCalculated =
                                        TimeSpan.FromMinutes(int.Parse((i["FromCalculated"] ?? string.Empty).ToString())),
                                    ToCalculated =
                                        TimeSpan.FromMinutes(int.Parse((i["ToCalculated"] ?? string.Empty).ToString())),
                                    FromData =
                                        TimeSpan.FromMinutes(int.Parse((i["FromData"] ?? string.Empty).ToString())),
                                    ToData = TimeSpan.FromMinutes(int.Parse((i["ToData"] ?? string.Empty).ToString()))
                                }).ToList();

                    return itemDisplay;
                }

                return null;
            }
        }

        /// <summary>
        /// Count Audio Tag Count Info.
        /// </summary>
        /// <param name="intervalMinutes">
        /// The interval minutes.
        /// </param>
        /// <returns>
        /// Count of Audio Tag Count Info.
        /// </returns>
        public static int CountAudioTagCountInfo(int intervalMinutes)
        {
            return GetAudioTagCountInfo("TagCount DESC", intervalMinutes).Count();
        }
    }
}
