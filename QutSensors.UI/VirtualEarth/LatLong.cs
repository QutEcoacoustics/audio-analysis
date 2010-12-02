// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LatLong.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the LatLong type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SoulSolutions.ClusterArticle
{
    /// <summary>
    /// Latitude and Longitude representation.
    /// </summary>
    public class LatLong
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LatLong"/> class.
        /// </summary>
        public LatLong()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LatLong"/> class.
        /// </summary>
        /// <param name="lat">
        /// Latitude of point.
        /// </param>
        /// <param name="lon">
        /// longitude of point.
        /// </param>
        public LatLong(double lat, double lon)
        {
            this.Lat = lat;
            this.Lon = lon;
        }

        /// <summary>
        /// Gets or sets Lat.
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// Gets or sets Lon.
        /// </summary>
        public double Lon { get; set; }

        /// <summary>
        /// Check if the LatLong is within the given Bounds.
        /// </summary>
        /// <param name="bounds">
        /// Bounds to check.
        /// </param>
        /// <returns>
        /// True if LatLong is within Bounds, otherwise false.
        /// </returns>
        public bool IsInBounds(Bounds bounds)
        {
            return (this.Lat >= bounds.SE.Lat) &&
                (this.Lat <= bounds.NW.Lat) &&
                (this.Lon >= bounds.NW.Lon) &&
                (this.Lon <= bounds.SE.Lon);
        }
    }
}