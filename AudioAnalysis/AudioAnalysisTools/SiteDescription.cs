using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysisTools
{
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
