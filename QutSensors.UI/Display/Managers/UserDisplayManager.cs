// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserDisplayManager.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the UserDisplayManager type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Security;

    using QutSensors.UI.Display.Classes;
    using QutSensors.UI.Security;

    /// <summary>Manages Users and Roles.
    /// </summary>
    public static class UserDisplayManager
    {
        #region users

        /// <summary>
        /// Get users for display in table.
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
        /// <returns>User display items.
        /// </returns>
        public static IEnumerable<UserDisplayItem> GetUsers(int maxItems, int startIndex, string sortExpression)
        {
            var users = Membership.GetAllUsers().Cast<MembershipUser>().Select(mu => new UserDisplayItem
            {
                UserId = mu.ProviderUserKey == null ? Guid.Empty : (Guid)mu.ProviderUserKey,
                UserName = mu.UserName,
                UserDisplayName = QutSensorsUserProfile.GetUserProfile(mu.UserName).DisplayName,
                Email = mu.Email,
                IsLockedOut = mu.IsLockedOut ? "Yes" : "No",
                IsApproved = mu.IsApproved ? "Yes" : "No",
                IsOnline = mu.IsOnline ? "Yes" : "No",
                LastLogin = mu.LastLoginDate.ToString("yyyy-MM-dd") + " (" + mu.LastLoginDate.ToDifferenceString(DateTime.Now) + ")",
                LastOnline = mu.LastActivityDate.ToString("yyyy-MM-dd") + " (" + mu.LastActivityDate.ToDifferenceString(DateTime.Now) + ")",
            });

            switch (sortExpression)
            {
                case "UserName":
                    users = users.OrderBy(u => u.UserName);
                    break;
                case "UserName DESC":
                    users = users.OrderByDescending(u => u.UserName);
                    break;
                case "Email":
                    users = users.OrderBy(u => u.Email);
                    break;
                case "Email DESC":
                    users = users.OrderByDescending(u => u.Email);
                    break;
                case "IsApproved":
                    users = users.OrderBy(u => u.IsApproved);
                    break;
                case "IsApproved DESC":
                    users = users.OrderByDescending(u => u.IsApproved);
                    break;
                case "IsLockedOut":
                    users = users.OrderBy(u => u.IsLockedOut);
                    break;
                case "IsLockedOut DESC":
                    users = users.OrderByDescending(u => u.IsLockedOut);
                    break;
                case "IsOnline":
                    users = users.OrderBy(u => u.IsOnline);
                    break;
                case "IsOnline DESC":
                    users = users.OrderByDescending(u => u.IsOnline);
                    break;
                case "LastLogin":
                    users = users.OrderBy(u => u.LastLogin);
                    break;
                case "LastLogin DESC":
                    users = users.OrderByDescending(u => u.LastLogin);
                    break;
                case "LastOnline":
                    users = users.OrderBy(u => u.LastOnline);
                    break;
                case "LastOnline DESC":
                    users = users.OrderByDescending(u => u.LastOnline);
                    break;
                default:
                    users = users.OrderByDescending(u => u.LastOnline);
                    break;
            }

            return users.Skip(startIndex).Take(maxItems);
        }

        /// <summary>
        /// Get users for display in table.
        /// </summary>
        /// <param name="sortExpression">
        /// The sort expression.
        /// </param>
        /// <returns>User display items.
        /// </returns>
        public static IEnumerable<UserDisplayItem> GetUsers(string sortExpression)
        {
            return GetUsers(int.MaxValue, 0, sortExpression);
        }

        /// <summary>
        /// Get user count.
        /// </summary>
        /// <returns>
        /// Number of users.
        /// </returns>
        public static int GetUserListCount()
        {
            return GetUsers(int.MaxValue, 0, string.Empty).Count();
        }

        #endregion

        #region roles

        /// <summary>
        /// Get role names paged.
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
        /// One page of roles names.
        /// </returns>
        public static IEnumerable<string> GetRoles(int maxItems, int startIndex, string sortExpression)
        {
            var roles = Roles.GetAllRoles().AsEnumerable();

            switch (sortExpression)
            {
                case "RoleName":
                    roles = roles.OrderBy(r => r);
                    break;
                case "RoleName DESC":
                    roles = roles.OrderByDescending(r => r);
                    break;
            }

            return roles.Skip(startIndex).Take(maxItems);
        }

        /// <summary>
        /// Get roles names.
        /// </summary>
        /// <param name="sortExpression">
        /// The sort expression.
        /// </param>
        /// <returns>
        /// Role names.
        /// </returns>
        public static IEnumerable<string> GetRoles(string sortExpression)
        {
            return GetRoles(int.MaxValue, 0, sortExpression);
        }

        /// <summary>
        /// Get role count.
        /// </summary>
        /// <returns>
        /// Number of roles.
        /// </returns>
        public static int GetRoleListCount()
        {
            return GetRoles(int.MaxValue, 0, string.Empty).Count();
        }

        #endregion
    }
}
