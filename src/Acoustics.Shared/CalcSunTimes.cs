// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalcSunTimes.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the SunTimes type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Sun Times.
    /// </summary>
    public sealed class SunTimes
    {
        private readonly object mLock = new object();

        private const double mDR = Math.PI / 180;
        private const double mK1 = 15 * mDR * 1.0027379;

        private readonly int[] mRiseTimeArr = new int[2] { 0, 0 };
        private readonly int[] mSetTimeArr = new int[2] { 0, 0 };
        private double mRizeAzimuth = 0.0;
        private double mSetAzimuth = 0.0;

        private readonly double[] mSunPositionInSkyArr = new double[2] { 0.0, 0.0 };
        private readonly double[] mRightAscentionArr = new double[3] { 0.0, 0.0, 0.0 };
        private readonly double[] mDecensionArr = new double[3] { 0.0, 0.0, 0.0 };
        private readonly double[] mVHzArr = new double[3] { 0.0, 0.0, 0.0 };

        private bool mIsSunrise = false;
        private bool mIsSunset = false;

        private SunTimes()
        {
        }

        public static SunTimes Instance { get; } = new SunTimes();

        public abstract class Coords
        {
            protected internal int mDegrees = 0;
            protected internal int mMinutes = 0;
            protected internal int mSeconds = 0;

            public double ToDouble()
            {
                return this.Sign() * (this.mDegrees + ((double)this.mMinutes / 60) + ((double)this.mSeconds / 3600));
            }

            protected internal abstract int Sign();
        }

        public class LatitudeCoords : Coords
        {
            public enum Direction
            {
                North,
                South,
            }

            protected internal Direction mDirection = Direction.North;

            public LatitudeCoords(int degrees, int minutes, int seconds, Direction direction)
            {
                this.mDegrees = degrees;
                this.mMinutes = minutes;
                this.mSeconds = seconds;
                this.mDirection = direction;
            }

            protected internal override int Sign()
            {
                return this.mDirection == Direction.North ? 1 : -1;
            }
        }

        public class LongitudeCoords : Coords
        {
            public enum Direction
            {
                East,
                West,
            }

            protected internal Direction mDirection = Direction.East;

            public LongitudeCoords(int degrees, int minutes, int seconds, Direction direction)
            {
                this.mDegrees = degrees;
                this.mMinutes = minutes;
                this.mSeconds = seconds;
                this.mDirection = direction;
            }

            protected internal override int Sign()
            {
                return this.mDirection == Direction.East ? 1 : -1;
            }
        }

        /// <summary>
        /// Calculate sunrise and sunset times. Returns false if time zone and longitude are incompatible.
        /// </summary>
        /// <param name="lat">Latitude coordinates.</param>
        /// <param name="lon">Longitude coordinates.</param>
        /// <param name="date">Date for which to calculate.</param>
        /// <param name="riseTime">Sunrise time (output)</param>
        /// <param name="setTime">Sunset time (output)</param>
        /// <param name="isSunrise">Whether or not the sun rises at that day</param>
        /// <param name="isSunset">Whether or not the sun sets at that day</param>
        public bool CalculateSunRiseSetTimes(LatitudeCoords lat, LongitudeCoords lon, DateTime date,
                                                ref DateTime riseTime, ref DateTime setTime,
                                                ref bool isSunrise, ref bool isSunset)
        {
            return this.CalculateSunRiseSetTimes(lat.ToDouble(), lon.ToDouble(), date, ref riseTime, ref setTime, ref isSunrise, ref isSunset);
        }

        /// <summary>
        /// Calculate sunrise and sunset times. Returns false if time zone and longitude are incompatible.
        /// </summary>
        /// <param name="lat">Latitude in decimal notation.</param>
        /// <param name="lon">Longitude in decimal notation.</param>
        /// <param name="date">Date for which to calculate.</param>
        /// <param name="riseTime">Sunrise time (output)</param>
        /// <param name="setTime">Sunset time (output)</param>
        /// <param name="isSunrise">Whether or not the sun rises at that day</param>
        /// <param name="isSunset">Whether or not the sun sets at that day</param>
        public bool CalculateSunRiseSetTimes(double lat, double lon, DateTime date,
                                                ref DateTime riseTime, ref DateTime setTime,
                                                ref bool isSunrise, ref bool isSunset)
        {
            lock (this.mLock) // lock for thread safety
            {
                double zone = -(int)Math.Round(TimeZone.CurrentTimeZone.GetUtcOffset(date).TotalSeconds / 3600);
                double jd = this.GetJulianDay(date) - 2451545;  // Julian day relative to Jan 1.5, 2000

                if (this.Sign(zone) == this.Sign(lon) && zone != 0)
                {
                    Debug.Print("WARNING: time zone and longitude are incompatible!");
                    return false;
                }

                lon = lon / 360;
                double tz = zone / 24;
                double ct = (jd / 36525) + 1;                                 // centuries since 1900.0
                double t0 = this.LocalSiderealTimeForTimeZone(lon, jd, tz);      // local sidereal time

                // get sun position at start of day
                jd += tz;
                this.CalculateSunPosition(jd, ct);
                double ra0 = this.mSunPositionInSkyArr[0];
                double dec0 = this.mSunPositionInSkyArr[1];

                // get sun position at end of day
                jd += 1;
                this.CalculateSunPosition(jd, ct);
                double ra1 = this.mSunPositionInSkyArr[0];
                double dec1 = this.mSunPositionInSkyArr[1];

                // make continuous
                if (ra1 < ra0)
                {
                    ra1 += 2 * Math.PI;
                }

                // initialize
                this.mIsSunrise = false;
                this.mIsSunset = false;

                this.mRightAscentionArr[0] = ra0;
                this.mDecensionArr[0] = dec0;

                // check each hour of this day
                for (int k = 0; k < 24; k++)
                {
                    this.mRightAscentionArr[2] = ra0 + ((k + 1) * (ra1 - ra0) / 24);
                    this.mDecensionArr[2] = dec0 + ((k + 1) * (dec1 - dec0) / 24);
                    this.mVHzArr[2] = this.TestHour(k, zone, t0, lat);

                    // advance to next hour
                    this.mRightAscentionArr[0] = this.mRightAscentionArr[2];
                    this.mDecensionArr[0] = this.mDecensionArr[2];
                    this.mVHzArr[0] = this.mVHzArr[2];
                }

                riseTime = new DateTime(date.Year, date.Month, date.Day, this.mRiseTimeArr[0], this.mRiseTimeArr[1], 0);
                setTime = new DateTime(date.Year, date.Month, date.Day, this.mSetTimeArr[0], this.mSetTimeArr[1], 0);

                isSunset = true;
                isSunrise = true;

                // neither sunrise nor sunset
                if (!this.mIsSunrise && !this.mIsSunset)
                {
                    if (this.mVHzArr[2] < 0)
                    {
                        isSunrise = false; // Sun down all day
                    }
                    else
                    {
                        isSunset = false; // Sun up all day
                    }
                }

                // sunrise or sunset
                else
                {
                    if (!this.mIsSunrise)
                    {
                        // No sunrise this date
                        isSunrise = false;
                    }
                    else if (!this.mIsSunset)
                    {
                        // No sunset this date
                        isSunset = false;
                    }
                }

                return true;
            }
        }

        private int Sign(double value)
        {
            int rv = 0;

            if (value > 0.0)
            {
                rv = 1;
            }
            else if (value < 0.0)
            {
                rv = -1;
            }
            else
            {
                rv = 0;
            }

            return rv;
        }

        // Local Sidereal Time for zone
        private double LocalSiderealTimeForTimeZone(double lon, double jd, double z)
        {
            double s = 24110.5 + (8640184.812999999 * jd / 36525) + (86636.6 * z) + (86400 * lon);
            s = s / 86400;
            s = s - Math.Floor(s);
            return s * 360 * mDR;
        }

        // determine Julian day from calendar date
        // (Jean Meeus, "Astronomical Algorithms", Willmann-Bell, 1991)
        private double GetJulianDay(DateTime date)
        {
            int month = date.Month;
            int day = date.Day;
            int year = date.Year;

            bool gregorian = year < 1583 ? false : true;

            if (month == 1 || month == 2)
            {
                year = year - 1;
                month = month + 12;
            }

            double a = Math.Floor((double)year / 100);
            double b = 0;

            if (gregorian)
            {
                b = 2 - a + Math.Floor(a / 4);
            }
            else
            {
                b = 0.0;
            }

            double jd = Math.Floor(365.25 * (year + 4716))
                       + Math.Floor(30.6001 * (month + 1))
                       + day + b - 1524.5;

            return jd;
        }

        // sun's position using fundamental arguments
        // (Van Flandern & Pulkkinen, 1979)
        private void CalculateSunPosition(double jd, double ct)
        {
            double g, lo, s, u, v, w;

            lo = 0.779072 + (0.00273790931 * jd);
            lo = lo - Math.Floor(lo);
            lo = lo * 2 * Math.PI;

            g = 0.993126 + (0.0027377785 * jd);
            g = g - Math.Floor(g);
            g = g * 2 * Math.PI;

            v = 0.39785 * Math.Sin(lo);
            v = v - (0.01 * Math.Sin(lo - g));
            v = v + (0.00333 * Math.Sin(lo + g));
            v = v - (0.00021 * ct * Math.Sin(lo));

            u = 1 - (0.03349 * Math.Cos(g));
            u = u - (0.00014 * Math.Cos(2 * lo));
            u = u + (0.00008 * Math.Cos(lo));

            w = -0.0001 - (0.04129 * Math.Sin(2 * lo));
            w = w + (0.03211 * Math.Sin(g));
            w = w + (0.00104 * Math.Sin((2 * lo) - g));
            w = w - (0.00035 * Math.Sin((2 * lo) + g));
            w = w - (0.00008 * ct * Math.Sin(g));

            // compute sun's right ascension
            s = w / Math.Sqrt(u - (v * v));
            this.mSunPositionInSkyArr[0] = lo + Math.Atan(s / Math.Sqrt(1 - (s * s)));

            // ...and declination
            s = v / Math.Sqrt(u);
            this.mSunPositionInSkyArr[1] = Math.Atan(s / Math.Sqrt(1 - (s * s)));
        }

        // test an hour for an event
        private double TestHour(int k, double zone, double t0, double lat)
        {
            double[] ha = new double[3];
            double a, b, c, d, e, s, z;
            double time;
            int hr, min;
            double az, dz, hz, nz;

            ha[0] = t0 - this.mRightAscentionArr[0] + (k * mK1);
            ha[2] = t0 - this.mRightAscentionArr[2] + (k * mK1) + mK1;

            ha[1] = (ha[2] + ha[0]) / 2;    // hour angle at half hour
            this.mDecensionArr[1] = (this.mDecensionArr[2] + this.mDecensionArr[0]) / 2;  // declination at half hour

            s = Math.Sin(lat * mDR);
            c = Math.Cos(lat * mDR);
            z = Math.Cos(90.833 * mDR);    // refraction + sun semidiameter at horizon

            if (k <= 0)
            {
                this.mVHzArr[0] = (s * Math.Sin(this.mDecensionArr[0])) + (c * Math.Cos(this.mDecensionArr[0]) * Math.Cos(ha[0])) - z;
            }

            this.mVHzArr[2] = (s * Math.Sin(this.mDecensionArr[2])) + (c * Math.Cos(this.mDecensionArr[2]) * Math.Cos(ha[2])) - z;

            if (this.Sign(this.mVHzArr[0]) == this.Sign(this.mVHzArr[2]))
            {
                return this.mVHzArr[2];  // no event this hour
            }

            this.mVHzArr[1] = (s * Math.Sin(this.mDecensionArr[1])) + (c * Math.Cos(this.mDecensionArr[1]) * Math.Cos(ha[1])) - z;

            a = (2 * this.mVHzArr[0]) - (4 * this.mVHzArr[1]) + (2 * this.mVHzArr[2]);
            b = (-3 * this.mVHzArr[0]) + (4 * this.mVHzArr[1]) - this.mVHzArr[2];
            d = (b * b) - (4 * a * this.mVHzArr[0]);

            if (d < 0)
            {
                return this.mVHzArr[2];  // no event this hour
            }

            d = Math.Sqrt(d);
            e = (-b + d) / (2 * a);

            if (e > 1 || e < 0)
            {
                e = (-b - d) / (2 * a);
            }

            time = k + e + (1 / (double)120); // time of an event

            hr = (int)Math.Floor(time);
            min = (int)Math.Floor((time - hr) * 60);

            hz = ha[0] + (e * (ha[2] - ha[0]));                 // azimuth of the sun at the event
            nz = -Math.Cos(this.mDecensionArr[1]) * Math.Sin(hz);
            dz = (c * Math.Sin(this.mDecensionArr[1])) - (s * Math.Cos(this.mDecensionArr[1]) * Math.Cos(hz));
            az = Math.Atan2(nz, dz) / mDR;
            if (az < 0)
            {
                az = az + 360;
            }

            if (this.mVHzArr[0] < 0 && this.mVHzArr[2] > 0)
            {
                this.mRiseTimeArr[0] = hr;
                this.mRiseTimeArr[1] = min;
                this.mRizeAzimuth = az;
                this.mIsSunrise = true;
            }

            if (this.mVHzArr[0] > 0 && this.mVHzArr[2] < 0)
            {
                this.mSetTimeArr[0] = hr;
                this.mSetTimeArr[1] = min;
                this.mSetAzimuth = az;
                this.mIsSunset = true;
            }

            return this.mVHzArr[2];
        }
    }
}