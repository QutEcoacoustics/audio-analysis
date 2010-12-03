// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QutSensorsUserProfile.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Security
{
    using System;
    using System.ComponentModel;
    using System.Web.Profile;
    using System.Web.Security;

    using YAF.Classes.Utils;

    /// <summary>
    /// The qut sensors user profile.
    /// </summary>
    public class QutSensorsUserProfile : YafUserProfile
    {
        #region Properties

        /// <summary>
        /// Gets or sets DisplayName.
        /// </summary>
        [SettingsAllowAnonymous(false)]
        public virtual string DisplayName
        {
            get
            {
                return (string)this.GetPropertyValue("DisplayName");
            }

            set
            {
                this.SetPropertyValue("DisplayName", value);
            }
        }

        /// <summary>
        /// Gets or sets PlayerSettings.
        /// </summary>
        [SettingsAllowAnonymous(true)]
        public virtual PlayerSettings PlayerSettings
        {
            get
            {
                var settings = this.GetPropertyValue("PlayerSettings") as PlayerSettings ??
                               new PlayerSettings { TurnOnLooping = true, Volume = 0.9, IsMute = false };

                return settings;
            }

            set
            {
                this.SetPropertyValue("PlayerSettings", value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether ProfileAlreadyMigrated.
        /// </summary>
        [SettingsAllowAnonymous(true)]
        [DefaultValue(false)]
        public virtual bool ProfileAlreadyMigrated
        {
            get
            {
                return (bool)this.GetPropertyValue("ProfileAlreadyMigrated");
            }

            set
            {
                this.SetPropertyValue("ProfileAlreadyMigrated", value);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The get user profile.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <returns>
        /// Profile for <paramref name="username"/>.
        /// </returns>
        public static QutSensorsUserProfile GetUserProfile(string username)
        {
            return Create(username) as QutSensorsUserProfile;
        }

        /// <summary>
        /// The get user profile.
        /// </summary>
        /// <returns>
        /// Profile for currently logged in user.
        /// </returns>
        public static QutSensorsUserProfile GetUserProfile()
        {
            var user = Membership.GetUser();

            if (user != null)
            {
                return Create(user.UserName, true) as QutSensorsUserProfile;
            }

            return null;
        }

        /// <summary>
        /// The get user profile.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="isAuthenticated">
        /// The is authenticated.
        /// </param>
        /// <returns>
        /// User profile.
        /// </returns>
        public static QutSensorsUserProfile GetUserProfile(string username, bool isAuthenticated)
        {
            return Create(username, isAuthenticated) as QutSensorsUserProfile;
        }

        #endregion
    }
}