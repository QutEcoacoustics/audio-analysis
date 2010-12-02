// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorLogItem.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace QutSensors.UI.Display.Classes
{
    /// <summary>
    /// ErrorLogItem.
    /// </summary>
    public class ErrorLogItem
    {
        /// <summary>
        /// Gets or sets ErrorId.
        /// </summary>
        public long ErrorId { get; set; }

        /// <summary>
        /// Gets or sets Summary.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets Detail.
        /// </summary>
        public string Detail { get; set; }

        /// <summary>
        /// Gets or sets Time.
        /// </summary>
        public DateTime Time { get; set; }
    }
}