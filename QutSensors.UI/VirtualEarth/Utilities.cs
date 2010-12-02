// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utilities.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the Utilities type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SoulSolutions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using SoulSolutions.ClusterArticle;

    /// <summary>
    /// Utilities for Virtual Earth.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// The radius of the earth - should never change.
        /// </summary>
        private const double EarthRadius = 6378137;

        /// <summary>
        /// calulated circumference of the earth.
        /// </summary>
        private const double EarthCircum = EarthRadius * 2.0 * Math.PI;

        /// <summary>
        /// calulated half circumference of the earth.
        /// </summary>
        private const double EarthHalfCirc = EarthCircum / 2;

        /// <summary>
        /// pixels per image tile.
        /// </summary>
        private const int PixelsPerTile = 256;

        private const int MinAscii = 63;

        private const int BinaryChunkSize = 5;

        /// <summary>
        /// Convert latitude to y co-ord at specified zoom.
        /// </summary>
        /// <param name="lat">Latitude of point.</param>
        /// <param name="zoom">Specified zoom level.</param>
        /// <returns>Y co-ordinate.</returns>
        public static int LatitudeToYAtZoom(double lat, int zoom)
        {
            double arc = EarthCircum / ((1 << zoom) * PixelsPerTile);
            double sinLat = Math.Sin(DegToRad(lat));
            double metersY = EarthRadius / 2 * Math.Log((1 + sinLat) / (1 - sinLat));
            var y = (int)Math.Round((EarthHalfCirc - metersY) / arc);
            return y;
        }

        /// <summary>
        /// Convert longitude to x co-ord at specified zoom.
        /// </summary>
        /// <param name="lon">
        /// The longitude of point.
        /// </param>
        /// <param name="zoom">
        /// The zoom level.
        /// </param>
        /// <returns>
        /// X Co-ordinate.
        /// </returns>
        public static int LongitudeToXAtZoom(double lon, int zoom)
        {
            double arc = EarthCircum / ((1 << zoom) * PixelsPerTile);
            double metersX = EarthRadius * DegToRad(lon);
            var x = (int)Math.Round((EarthHalfCirc + metersX) / arc);
            return x;
        }

        /// <summary>
        /// Decode string representation of Bounds.
        /// </summary>
        /// <param name="encoded">
        /// The encoded.
        /// </param>
        /// <returns>
        /// Bounds object.
        /// </returns>
        public static Bounds DecodeBounds(string encoded)
        {
            List<LatLong> locs = DecodeLatLong(encoded);

            // OverSize the bounds to allow for rounding errors in the encoding process.
            locs[0].Lat += 0.00001;
            locs[0].Lon -= 0.00001;
            locs[1].Lat -= 0.00001;
            locs[1].Lon += 0.00001;
            return new Bounds(locs[0], locs[1]);
        }

        /// <summary>
        /// Encode list of clustered pins as string.
        /// </summary>
        /// <param name="pins">
        /// The list of clustered pins.
        /// </param>
        /// <typeparam name="T">
        /// Type of clustered pins.
        /// </typeparam>
        /// <returns>
        /// String representing clustered pins.
        /// </returns>
        public static string EncodeCluster<T>(List<ClusteredPin<T>> pins)
        {
            var encoded = new StringBuilder();

            // encode the locations
            var points = pins.Select(pin => pin.Loc).ToList();
            encoded.Append(EncodeLatLong(points));

            // encode the bounds per cluster
            foreach (var pin in pins)
            {
                encoded.Append(',');
                points = new List<LatLong> { pin.ClusterArea.NW, pin.ClusterArea.SE };
                encoded.Append(EncodeLatLong(points));
            }

            return encoded.ToString();
        }

        /// <summary>
        /// Encode list of points to string.
        /// </summary>
        /// <param name="points">
        /// The lsit of LatLong points.
        /// </param>
        /// <returns>
        /// string representation of list of LatLongs.
        /// </returns>
        public static string EncodeLatLong(List<LatLong> points)
        {
            int plat = 0;
            int plng = 0;
            int len = points.Count;

            var encodedPoints = new StringBuilder();

            for (int i = 0; i < len; ++i)
            {
                // Round to 5 decimal places and drop the decimal
                var late5 = (int)(points[i].Lat * 1e5);
                var lnge5 = (int)(points[i].Lon * 1e5);

                // encode the differences between the points
                encodedPoints.Append(EncodeSignedNumber(late5 - plat));
                encodedPoints.Append(EncodeSignedNumber(lnge5 - plng));

                // store the current point
                plat = late5;
                plng = lnge5;
            }

            return encodedPoints.ToString();
        }

        /// <summary>
        /// Decode LatLong point list.
        /// </summary>
        /// <param name="encoded">
        /// The encoded.
        /// </param>
        /// <returns>
        /// List of LatLong points.
        /// </returns>
        public static List<LatLong> DecodeLatLong(string encoded)
        {
            var locs = new List<LatLong>();

            int index = 0;
            int lat = 0;
            int lng = 0;

            int len = encoded.Length;
            while (index < len)
            {
                lat += DecodePoint(encoded, index, out index);
                lng += DecodePoint(encoded, index, out index);

                locs.Add(new LatLong((lat * 1e-5), (lng * 1e-5)));
            }

            return locs;
        }

        /// <summary>
        /// Degrees to Radians.
        /// </summary>
        /// <param name="d">Degree as double.</param>
        /// <returns>Radians from Degrees.</returns>
        private static double DegToRad(double d)
        {
            return d * Math.PI / 180.0;
        }

        /// <summary>
        /// Decode point.
        /// </summary>
        /// <param name="encoded">String representation of point.</param>
        /// <param name="startindex">Starting index.</param>
        /// <param name="finishindex">Finish index.</param>
        /// <returns>Decoded point.</returns>
        private static int DecodePoint(string encoded, int startindex, out int finishindex)
        {
            int b;
            int shift = 0;
            int result = 0;
            do
            {
                // get binary encoding
                b = Convert.ToInt32(encoded[startindex++]) - MinAscii;

                // binary shift
                result |= (b & 0x1f) << shift;

                // move to next chunk
                shift += BinaryChunkSize;

                // see if another binary value
            }
            while (b >= 0x20);

            // if negivite flip
            int dlat = ((result & 1) > 0) ? ~(result >> 1) : (result >> 1);

            // set output index
            finishindex = startindex;
            return dlat;
        }

        /// <summary>
        /// Encode Signed number.
        /// </summary>
        /// <param name="num">Number to encode.</param>
        /// <returns>Number encoded as string.</returns>
        private static string EncodeSignedNumber(int num)
        {
            // shift the binary value
            int sgnNum = num << 1;

            // if negative invert
            if (num < 0)
            {
                sgnNum = ~sgnNum;
            }

            return EncodeNumber(sgnNum);
        }

        /// <summary>
        /// Encode number.
        /// </summary>
        /// <param name="num">number to encode.</param>
        /// <returns>String representation of number.</returns>
        private static string EncodeNumber(int num)
        {
            var encodeString = new StringBuilder();
            while (num >= 0x20)
            {
                // while another chunk follows
                encodeString.Append((char)((0x20 | (num & 0x1f)) + MinAscii)); // OR value with 0x20, convert to decimal and add 63
                num >>= BinaryChunkSize; // shift to next chunk
            }

            encodeString.Append((char)(num + MinAscii));
            return encodeString.ToString();
        }
    }
}