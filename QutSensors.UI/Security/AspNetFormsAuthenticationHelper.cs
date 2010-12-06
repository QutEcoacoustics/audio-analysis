// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AspNetFormsAuthenticationHelper.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Summary description for AspNetFormsAuthenticationHelper
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Security
{
    using System;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Profile;
    using System.Web.Security;

    using QutSensors.Data.Linq;

    /// <summary>
    /// Helper methods for website authentication.
    /// </summary>
    public static class AuthenticationHelper
    {
        /// <summary>
        /// Gets a value indicating whether the current user is authenticated.
        /// </summary>
        public static bool IsAuthenticated
        {
            get
            {
                return HttpContext.Current.User.Identity.IsAuthenticated;
            }
        }

        /// <summary>
        /// Gets the current MembershipUser.
        /// </summary>
        public static MembershipUser CurrentMembershipUser
        {
            get
            {
                return IsAuthenticated ? Membership.GetUser(false) : null;
            }
        }

        /// <summary>
        /// Gets current UserId.
        /// </summary>
        public static Guid? CurrentUserId
        {
            get
            {
                var user = CurrentMembershipUser;
                if (user == null)
                {
                    return null;
                }

                return user.ProviderUserKey as Guid?;
            }
        }

        /// <summary>
        /// Gets Username of current user. Anonymous users have a GUID as username (AnonymousID).
        /// </summary>
        public static string CurrentUserName
        {
            get
            {
                return HttpContext.Current.User.Identity.IsAuthenticated ?
                    HttpContext.Current.User.Identity.Name :
                    HttpContext.Current.Request.AnonymousID;
            }
        }

        /// <summary>
        /// Gets Principal of CurrentUser.
        /// </summary>
        public static IPrincipal CurrentUser
        {
            get
            {
                return HttpContext.Current.User.Identity.IsAuthenticated ?
                    HttpContext.Current.User : null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user in an administrator.
        /// </summary>
        public static bool IsCurrentUserAdmin
        {
            get
            {
                return CurrentUser != null && CurrentUser.IsInRole(AdministratorRoleName);
            }
        }

        /// <summary>
        /// Gets AdministratorRoleName.
        /// </summary>
        public static string AdministratorRoleName
        {
            get
            {
                var adminName = System.Configuration.ConfigurationManager.AppSettings["AdministratorRoleName"];
                return !string.IsNullOrEmpty(adminName) ? adminName : "administrators";
            }
        }

        /// <summary>
        /// Gets EveryoneRoleName.
        /// </summary>
        public static string EveryoneRoleName
        {
            get
            {
                return "everyone";
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user is an administrator.
        /// </summary>
        /// <param name="user">
        /// User Principal.
        /// </param>
        /// <returns>
        /// True if user is an admin, otherwise false.
        /// </returns>
        public static bool IsAdmin(this IPrincipal user)
        {
            return user != null && user.IsInRole(AdministratorRoleName);
        }

        /// <summary>
        /// Gets a value indicating whether the current user is an administrator.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <returns>
        /// True if user is an admin, otherwise false.
        /// </returns>
        public static bool IsAdmin(string userName)
        {
            return Roles.IsUserInRole(userName, AdministratorRoleName);
        }

        /// <summary>
        /// Get the user id for a user name.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <returns>
        /// User id for username or Guid.Empty.
        /// </returns>
        public static Guid GetUserId(string username)
        {
            var user = Membership.GetUser(username);
            return user != null ?
                new Guid(user.ProviderUserKey.ToString()) : Guid.Empty;
        }

        /// <summary>
        /// Get the user name if present, otherwise user name for a user id.
        /// </summary>
        /// <param name="guid">
        /// User id guid.
        /// </param>
        /// <returns>
        /// Display name if present, otherwise user name.
        /// </returns>
        public static string GetUserName(Guid guid)
        {
            var user = Membership.GetUser(guid);

            if (user != null && !string.IsNullOrEmpty(user.UserName))
            {
                var profile = QutSensorsUserProfile.GetUserProfile(user.UserName);

                return profile != null ? profile.DisplayName : user.UserName;
            }

            return string.Empty;
        }

        /// <summary>
        /// 401 Unauthorized: 
        /// An anonymous user tried to do something they can't.
        /// User will be prompted to log in.
        /// </summary>
        /// <param name="attemptedUrl">
        /// The attempted Url.
        /// </param>
        public static void ActionUnauthorised(string attemptedUrl)
        {
            var msg = CurrentUserName + " did not have authorisation to access requested page (401)";

            if (!string.IsNullOrEmpty(attemptedUrl))
            {
                msg += ": " + attemptedUrl;
            }

            var ex = new UnauthorizedAccessException(msg + ".");

            ErrorLog.Insert(ex, "Error logged from AuthenticationHelper.ActionUnauthorised");

            FormsAuthentication.RedirectToLoginPage();

            // required response.end call to make sure the rest of the page does not load.
            // http://www.neilpullinger.co.uk/2007/07/always-use-responseend-after.html
            HttpContext.Current.Response.End();
        }

        /// <summary>
        /// 403 Forbidden: 
        /// An authenticated user tried to do something they do not have permission to do.
        /// </summary>
        /// <param name="attemptedUrl">Url that was forbidden.</param>
        public static void ActionForbidden(string attemptedUrl)
        {
            var msg = CurrentUserName + " tried to access a forbidden page (403)";

            if (!string.IsNullOrEmpty(attemptedUrl))
            {
                msg += ": " + attemptedUrl;
            }

            var ex = new UnauthorizedAccessException(msg + ".");

            ErrorLog.Insert(ex, "Error logged from AuthenticationHelper.ActionForbidden");

            HttpContext.Current.Response.Redirect("~/Error/Error_403.aspx", true);
        }

        /// <summary>
        /// 404 Not Found:
        /// A page or requested item could not be found.
        /// </summary>
        /// <param name="attemptedUrl">
        /// The attempted Url that does not exist.
        /// </param>
        public static void ActionNotFound(string attemptedUrl)
        {
            var msg = CurrentUserName + " tried to access a page that does not exist (404)";

            if (!string.IsNullOrEmpty(attemptedUrl))
            {
                msg += ": " + attemptedUrl;
            }

            var ex = new Exception(msg + ".");

            ErrorLog.Insert(ex, "Error logged from AuthenticationHelper.ActionNotFound");

            HttpContext.Current.Response.Redirect("~/Error/Error_404.aspx", true);
        }

        /// <summary>
        /// 400 Bad Request:
        /// The request contains bad syntax or cannot be fulfilled.
        /// </summary>
        /// <param name="attemptedUrl">
        /// The attempted Url.
        /// </param>
        public static void ActionBadRequest(string attemptedUrl)
        {
            var msg = CurrentUserName + " tried to access a page using a bad request (400)";

            if (!string.IsNullOrEmpty(attemptedUrl))
            {
                msg += ": " + attemptedUrl;
            }

            var ex = new Exception(msg + ".");

            ErrorLog.Insert(ex, "Error logged from AuthenticationHelper.ActionBadRequest");

            HttpContext.Current.Response.Redirect("~/Error/Error_400.aspx", true);
        }
    }

    /// <summary>
    /// Checks if a user is logged in, and redirects to the log in page if not.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class IsLoggedIn : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsLoggedIn"/> class.
        /// </summary>
        public IsLoggedIn()
        {
            if (!AuthenticationHelper.IsAuthenticated)
            {
                FormsAuthentication.RedirectToLoginPage();
            }
        }
    }
}
