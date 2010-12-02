// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClusteredPin.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   A clustered pin is the basic object required to plot on the VE map.
//   It has a location, a type and a bounds that it represents.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SoulSolutions.ClusterArticle
{
    using System.Collections.Generic;

    /// <summary>
    /// A clustered pin is the basic object required to plot on the VE map.
    /// It has a location, a type and a bounds that it represents.
    /// </summary>
    /// <typeparam name="T">
    /// Type of Clustered pin.
    /// </typeparam>
    public class ClusteredPin<T>
    {
        #region Fields

        private int _pixelX;
        private int _pixelY;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusteredPin{T}"/> class.
        /// </summary>
        public ClusteredPin()
            : this(null, new Bounds())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusteredPin{T}"/> class.
        /// </summary>
        /// <param name="loc">
        /// LatLong location.
        /// </param>
        public ClusteredPin(LatLong loc)
            : this(loc, new Bounds(loc, loc))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusteredPin{T}"/> class.
        /// </summary>
        /// <param name="loc">
        /// LatLong location.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public ClusteredPin(LatLong loc, T value)
            : this(loc, new Bounds(loc, loc))
        {
            Values.Add(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusteredPin{T}"/> class.
        /// </summary>
        /// <param name="loc">
        /// LatLong location.
        /// </param>
        /// <param name="clusterArea">
        /// The cluster area.
        /// </param>
        public ClusteredPin(LatLong loc, Bounds clusterArea)
        {
            Values = new List<T>();
            this._pixelX = -1;
            this._pixelY = -1;
            this.Loc = loc;
            this.ClusterArea = clusterArea;
        }

        #region Properties

        /// <summary>
        /// Gets or sets Loc.
        /// </summary>
        public LatLong Loc { get; set; }

        /// <summary>
        /// Gets or sets ClusterArea.
        /// </summary>
        public Bounds ClusterArea { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsClustered.
        /// </summary>
        public bool IsClustered { get; set; }

        /// <summary>
        /// Gets or sets Values.
        /// </summary>
        public List<T> Values { get; set; }

        #endregion

        /// <summary>
        /// Adds a pin to the cluster.
        /// </summary>
        /// <param name="newPin">
        /// the pin to add.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public void AddPin(ClusteredPin<T> newPin, List<T> value)
        {
            if (this.Loc == null)
            {
                this.Loc = newPin.Loc;
            }

            this.ClusterArea.IncludeInBounds(newPin.ClusterArea);
            Values.AddRange(value);
        }

        /// <summary>
        /// Gets the x pixel location of the pin for the given zoomlevel
        /// location is stored. Assumption is made the zoomlevel does not change for the pin.
        /// </summary>
        /// <param name="zoomLevel">the current zoomlevel of the map.</param>
        /// <returns>the x pixel location of the pin.</returns>
        public int GetPixelX(int zoomLevel)
        {
            if (this._pixelX < 0)
            {
                this._pixelX = Utilities.LongitudeToXAtZoom(this.Loc.Lon, zoomLevel);
            }

            return this._pixelX;
        }

        /// <summary>
        /// Gets the y pixel location of the pin for the given zoomlevel
        /// location is stored. Assumption is made the zoomlevel does not change for the pin.
        /// </summary>
        /// <param name="zoomLevel">the current zoomlevel of the map.</param>
        /// <returns>the y pixel location of the pin.</returns>
        public int GetPixelY(int zoomLevel)
        {
            if (this._pixelY < 0)
            {
                this._pixelY = Utilities.LongitudeToXAtZoom(this.Loc.Lat, zoomLevel);
            }

            return this._pixelY;
        }
    }
}