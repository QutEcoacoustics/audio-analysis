// <copyright file="SiteDescription.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class SiteDescription
    {
        /// <summary>
        /// Site name
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// Site name
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Latitude of the site
        /// </summary>
        public double? Latitude { get; set; }


        /// <summary>
        /// Longitude of the site
        /// </summary>
        public double? Longitude { get; set; }

    }
}
