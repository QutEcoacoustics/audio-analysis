namespace SoulSolutions.ClusterArticle
{
    /// <summary>
    /// Represents a bounded map area using a rectange based on a topleft (NW and bottom right (SE) latitude/longitude
    /// </summary>
    public class Bounds
    {
        #region Fields

        private LatLong nW;
        private LatLong sE;

        #endregion

        public Bounds(LatLong nW, LatLong sE)
        {
            NW = nW;
            SE = sE;
        }

        public Bounds()
        {
            //Set values to opposite to allow any new values to override
            NW = new LatLong(-90, 180);
            SE = new LatLong(90, -180);
        }

        #region Properties

        public LatLong NW
        {
            get { return nW; }
            set { nW = value; }
        }

        public LatLong SE
        {
            get { return sE; }
            set { sE = value; }
        }

        #endregion

        /// <summary>
        /// Expands the current bounds to include the supplied bounds
        /// </summary>
        /// <param name="bounds">the latitude/longitude to be included</param>
        public void IncludeInBounds(Bounds bounds)
        {
            if (bounds.SE.Lat < SE.Lat)
                SE.Lat = bounds.SE.Lat;
            if (bounds.NW.Lat > NW.Lat)
                NW.Lat = bounds.NW.Lat;
            if (bounds.SE.Lon > SE.Lon)
                SE.Lon = bounds.SE.Lon;
            if (bounds.NW.Lon < NW.Lon)
                NW.Lon = bounds.NW.Lon;
        }
    }
}