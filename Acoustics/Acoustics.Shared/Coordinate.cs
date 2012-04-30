namespace Acoustics.Shared
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net;
    using System.Web.Script.Serialization;

    /// <summary>
    /// Coordinate.
    /// </summary>
    public struct Coordinate
    {
        private double latitude;
        private double longitude;

        /// <summary>
        /// Initializes a new instance of the <see cref="Coordinate"/> struct.
        /// </summary>
        /// <param name="latitude">
        /// The latitude.
        /// </param>
        /// <param name="longitude">
        /// The longitude.
        /// </param>
        public Coordinate(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        /// <summary>
        /// Gets or sets Latitude.
        /// </summary>
        public double Latitude
        {
            get
            {
                return this.latitude;
            }

            set
            {
                this.latitude = value;
            }
        }

        /// <summary>
        /// Gets or sets Longitude.
        /// </summary>
        public double Longitude
        {
            get
            {
                return this.longitude;
            }

            set
            {
                this.longitude = value;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="coordinate">
        /// The coordinate.
        /// </param>
        /// <returns>
        /// </returns>
        public static string GetLocation(Coordinate coordinate)
        {
            using (var client = new WebClient())
            {
                // for bing maps
                var bingMapsUrl =
                    string.Format(
                        "http://dev.virtualearth.net/REST/v1/Locations/{0},{1}?includeEntityTypes=Neighborhood,PopulatedPlace&includeNeighborhood=1&key={2}",
                        coordinate.Latitude,
                        coordinate.Longitude,
                        AppConfigHelper.GetString("BingMaps.Key"));

                JavaScriptSerializer jss = new JavaScriptSerializer();
                jss.RegisterConverters(new JavaScriptConverter[] { new DynamicJsonConverter() });

                // TODO: get place name from request

                return string.Empty;
            }
        }
    }

    public class DynamicJsonConverter : JavaScriptConverter
    {
        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            if (type == typeof(object))
            {
                
            }

            return null;
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get { return new ReadOnlyCollection<Type>(new List<Type>(new Type[] { typeof(object) })); }
        }
    }
}
