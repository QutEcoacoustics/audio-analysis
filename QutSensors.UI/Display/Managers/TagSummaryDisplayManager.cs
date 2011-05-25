using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QutSensors.UI.Display.Managers
{
    using System.Web.Security;

    using QutSensors.Data.Linq;
    using QutSensors.Shared;
    using QutSensors.UI.Display.Classes;

    public class TagSummaryDisplayManager : IDisposable
    {
        private QutSensorsDb db;

        public TagSummaryDisplayManager()
        {
            db = new QutSensorsDb();
        }

        private IQueryable<AudioTag> GetUserTags(MembershipUser user)
        {
            IQueryable<AudioTag> totalTagsForUser;
            if (user.ProviderUserKey != null)
            {

                totalTagsForUser = db.AudioTags
                    .Where(a => a.CreatedBy.ToLower() == user.UserName.ToLower() ||
                        a.OldCreatedBy.ToLower() == user.UserName.ToLower() ||
                        a.CreatedBy.ToLower() == user.ProviderUserKey.ToString().ToLower() ||
                        a.OldCreatedBy.ToLower() == user.ProviderUserKey.ToString().ToLower());
            }
            else
            {
                totalTagsForUser = db.AudioTags
                    .Where(a => a.CreatedBy.ToLower() == user.UserName.ToLower() ||
                        a.OldCreatedBy.ToLower() == user.UserName.ToLower());
            }

            return totalTagsForUser;
        }

        public int TotalTags()
        {
            return db.AudioTags.Count();
        }

        public int TotalTags(MembershipUser user)
        {
            return GetUserTags(user).Count();
        }

        public int TotalUniqueTags()
        {
            return db.AudioTags.Select(t => t.Tag).Distinct().Count();
        }

        public int TotalUniqueTags(MembershipUser user)
        {
            return GetUserTags(user).Select(t => t.Tag).Distinct().Count();
        }

        public IEnumerable<TagAggregateItem> TopTags(int count)
        {
            var tagsGrouped =
              db.AudioTags.GroupBy(at => at.Tag).Select(
                  groupedtag => new TagAggregateItem() { TagName = groupedtag.Key, Count = groupedtag.Count() });

            // order will be same within equal counts for bottom and top
            var tags = tagsGrouped.OrderByDescending(a => a.Count).ThenByDescending(a => a.TagName).Take(count);

            return tags;
        }

        public IEnumerable<TagAggregateItem> TopTags(int count, MembershipUser user)
        {
            var tagsGrouped =
              GetUserTags(user).GroupBy(at => at.Tag).Select(
                  groupedtag => new TagAggregateItem() { TagName = groupedtag.Key, Count = groupedtag.Count() });

            // order will be same within equal counts for bottom and top
            var tags = tagsGrouped.OrderByDescending(a => a.Count).ThenByDescending(a => a.TagName).Take(count);

            return tags;
        }

        public IOrderedQueryable<AudioTag> LongestTags()
        {
            return db.AudioTags.OrderByDescending(t => t.EndTime - t.StartTime);
        }

        public IOrderedQueryable<AudioTag> LongestTags(MembershipUser user)
        {
            return GetUserTags(user).OrderByDescending(t => t.EndTime - t.StartTime);
        }

        public IOrderedQueryable<AudioTag> ShortestTags()
        {
            return db.AudioTags.OrderBy(t => t.EndTime - t.StartTime);
        }

        public IOrderedQueryable<AudioTag> ShortestTags(MembershipUser user)
        {
            return GetUserTags(user).OrderBy(t => t.EndTime - t.StartTime);
        }

        public IEnumerable<TagAggregateItem> BottomTags(int count)
        {
            var tagsGrouped =
              db.AudioTags.GroupBy(at => at.Tag).Select(
                  groupedtag => new TagAggregateItem() { TagName = groupedtag.Key, Count = groupedtag.Count() });

            // order will be same within equal counts for bottom and top
            var tags = tagsGrouped.OrderBy(a => a.Count).ThenBy(a => a.TagName).Take(count);

            return tags;
        }

        public IEnumerable<TagAggregateItem> BottomTags(int count, MembershipUser user)
        {
            var tagsGrouped =
              GetUserTags(user).GroupBy(at => at.Tag).Select(
                  groupedtag => new TagAggregateItem() { TagName = groupedtag.Key, Count = groupedtag.Count() });

            // order will be same within equal counts for bottom and top
            var tags = tagsGrouped.OrderBy(a => a.Count).ThenBy(a => a.TagName).Take(count);

            return tags;
        }



        public void Dispose()
        {
            db.Dispose();
        }
    }
}
