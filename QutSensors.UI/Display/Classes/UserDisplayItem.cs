// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserDisplayItem.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display.Classes
{
    using System;

    /// <summary>
    /// The user display item.
    /// </summary>
    public struct UserDisplayItem
    {
        /// <summary>
        /// Gets or sets UserId.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets UserName.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets UserDisplayName.
        /// </summary>
        public string UserDisplayName { get; set; }

        /// <summary>
        /// Gets or sets Email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets IsApproved.
        /// </summary>
        public string IsApproved { get; set; }

        /// <summary>
        /// Gets or sets IsLockedOut.
        /// </summary>
        public string IsLockedOut { get; set; }

        /// <summary>
        /// Gets or sets IsOnline.
        /// </summary>
        public string IsOnline { get; set; }

        /// <summary>
        /// Gets or sets LastOnline.
        /// </summary>
        public string LastOnline { get; set; }

        /// <summary>
        /// Gets or sets LastLogin.
        /// </summary>
        public string LastLogin { get; set; }
    }
}