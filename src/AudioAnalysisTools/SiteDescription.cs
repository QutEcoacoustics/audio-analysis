// <copyright file="SiteDescription.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    public class SiteDescription
    {
        /// <summary>
        /// Gets or sets site name
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// Gets or sets site name
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Gets or sets latitude of the site
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets longitude of the site
        /// </summary>
        public double? Longitude { get; set; }
    }
}
