// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Coordinate.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Web.Script.Serialization;

    using Microsoft.SqlServer.Types;

    //using Microsoft.SqlServer.Types;

    /// <summary>
    /// Coordinate.
    /// </summary>
    public struct Coordinate
    {
        #region Constants and Fields

        /// <summary>
        /// Means approximately 1100m.
        /// </summary>
        public const int AccuracyLimit = 2;

        private const string AccuracyWarning =
            "The accuracy of the measurements in this class have been artificially modified for privacy reasons. Do not rely on this information.";

        private static readonly double accuracySmallest = 1.0 / Math.Pow(10, AccuracyLimit);

        private double latitude;

        private double longitude;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Coordinate"/> struct. This is a public facing class meant for deserialisation. The serializable parts of this spec are designed NOT to give pin-point lat and longs. The degree of accuracy is determined from the 
        /// <code>
        /// AccuracyLimit
        /// </code>
        /// constant. Choices for valid values can be found: http://en.wikipedia.org/wiki/Decimal_degrees.
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

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets Latitude.
        /// </summary>
        public double Latitude
        {
            get
            {
                return this.Obsfucate(this.latitude);
            }
        }

        /// <summary>
        /// Gets Latitude.
        /// </summary>
        [IgnoreDataMember]
        public double LatitudeExact
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
                return this.Obsfucate(this.longitude);
            }
        }

        /// <summary>
        /// Gets or sets Longitude.
        /// </summary>
        [IgnoreDataMember]
        public double LongitudeExact
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
        /// Gets Warning.
        /// </summary>
        public string Warning
        {
            get
            {
                return AccuracyWarning;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get location.
        /// </summary>
        /// <param name="coordinate">
        /// The coordinate. 
        /// </param>
        /// <returns>
        /// The get location.
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

        /// <summary>
        /// The op_ implicit.
        /// </summary>
        /// <param name="g">
        /// The g.
        /// </param>
        /// <returns>
        /// </returns>
        public static implicit operator Coordinate(SqlGeography g)
        {
            return new Coordinate(g.Lat.Value, g.Long.Value);
        }

        #endregion

        #region Methods

        private double Obsfucate(double bearing)
        {
            // rather than round to the corner of the bounding box,
            // add half of the smallest accuracy to get a point in the middle
            var mid = accuracySmallest / 2.0;
            double p = Math.Round(bearing, AccuracyLimit, MidpointRounding.AwayFromZero);

            // if it was rounded up, subtract to get back into the same grid square
            if (p > bearing)
            {
                return p - mid;
            }
            else
            {
                return p + mid;
            }
        }

        #endregion
    }

    /// <summary>
    /// The dynamic json converter.
    /// </summary>
    public class DynamicJsonConverter : JavaScriptConverter
    {
        #region Public Properties

        /// <summary>
        /// Gets SupportedTypes.
        /// </summary>
        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                return new ReadOnlyCollection<Type>(new List<Type>(new[] { typeof(object) }));
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The deserialize.
        /// </summary>
        /// <param name="dictionary">
        /// The dictionary.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="serializer">
        /// The serializer.
        /// </param>
        /// <returns>
        /// The deserialize.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public override object Deserialize(
            IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }

            if (type == typeof(object))
            {
            }

            return null;
        }

        /// <summary>
        /// The serialize.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <param name="serializer">
        /// The serializer.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}