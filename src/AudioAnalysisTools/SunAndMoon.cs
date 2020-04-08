// <copyright file="SunAndMoon.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Acoustics.Shared.ImageSharp;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;

    [Obsolete("This class is not generalizeable and shouldn't be used until it can be made so.")]
    public static class SunAndMoon
    {
        public class SunMoonTides
        {
            public const string LOWTIDE = "LowTide";
            public const string HIGHTIDE = "HighTide";
            public const string SUNRISE = "Sunrise";
            public const string SUNSET = "Sunset";

            public DateTimeOffset Date { get; set; }

            //public DateTimeOffset LowTide { get; set; }
            //public DateTimeOffset HighTide { get; set; }
            //public DateTimeOffset Sunrise { get; set; }
            //public DateTimeOffset Sunset { get; set; }

            public Dictionary<string, DateTimeOffset> Dictionary = new Dictionary<string, DateTimeOffset>();
        }

        public static FileInfo BrisbaneSunriseDatafile = @"C:\SensorNetworks\OutputDataSets\SunRiseSet\SunriseSet2013Brisbane.csv".ToFileInfo();

        /// <summary>
        /// The data for establishing the exact startDTO for the phase of moon in 2010 was obtained at the folowing website:
        /// http://www.timeanddate.com/moon/phases/australia/brisbane.
        ///
        /// </summary>
        public static double GetPhaseOfMoon(DateTimeOffset dto)
        {
            double lunarCycle = 29.53; // one lunar cycle in days

            // a known new moon in Brisbane
            var startDTO = new DateTimeOffset(2010, 1, 15, 17, 11, 0, TimeSpan.Zero);

            // the below line tests case of a known full moon PRIOR to the startDTO.
            // Did this to check negative cycles - It works!
            //dto = new DateTimeOffset(2008, 7, 18, 17, 59, 0, TimeSpan.Zero); // a known full moon in Brisbane

            double totalElapsedDays = (dto - startDTO).TotalDays;
            double cycles = totalElapsedDays / lunarCycle;
            double remainderCycle = cycles - Math.Truncate(cycles);

            if (remainderCycle < 0)
            {
                remainderCycle = 1 + remainderCycle;
            }

            return remainderCycle;
        }

        /// <summary>
        /// the moon phsae value is assumed to be between 0 and 1. 0 = new moon. 1 = newmoon. 0.5 = full moon.
        /// </summary>
        public static string ConvertMoonPhaseToString(double phase)
        {
            string moon = "New moon";
            int intPhase = (int)Math.Round(phase * 16);

            if (intPhase < 1)
            {
                moon = "New moon";
            }
            else if (intPhase < 3)
            {
                moon = "Waxing Crescent Moon";
            }
            else if (intPhase < 5)
            {
                moon = "First Quarter Moon";
            }
            else if (intPhase < 7)
            {
                moon = "Waxing Gibbous Moon";
            }
            else if (intPhase < 9)
            {
                moon = "Full Moon";
            }
            else if (intPhase < 11)
            {
                moon = "Waning Gibbous Moon";
            }
            else if (intPhase < 13)
            {
                moon = "Third Quarter Moon";
            }
            else if (intPhase < 15)
            {
                moon = "Waning Crescent Moon";
            }

            return moon;
        }

        /// <summary>
        /// TODO TODO  work on this method using website and javascript referred to by Anthony. (suncalc.net)
        /// This method requires a properly formatted csv file containing sunrise/sunset data.
        /// </summary>
        public static Image<Rgb24> AddSunTrackToImage(int width, DateTimeOffset? dateTimeOffset, FileInfo sunriseDatafile)
        {
            // AT: I DON'T UNDERSTAND THIS CODE! I CAN'T FIX IT
            LoggedConsole.WriteWarnLine("\t\tERROR: Sun track generation disabled - broken in long ago merge");
            return null;

            //            if (!sunriseDatafile.Exists)
            //                return null;
            //<<<<<<< HEAD
            //            }
            //
            //            if (siteName.StartsWith("Gympie") || siteName.StartsWith("Woondum3") || siteName.StartsWith("SERF"))
            //            {
            //                int dayOfYear = ((DateTimeOffset)dateTimeOffset).DayOfYear;
            //                double moonPhase = SunAndMoon.GetPhaseOfMoon((DateTimeOffset)dateTimeOffset);
            //                string strMoonPhase = SunAndMoon.ConvertMoonPhaseToString(moonPhase);
            //                throw new NotSupportedException("THE FOLLOWING FAILS IN PRODUCTION");
            //                Image<Rgb24> suntrack = SunAndMoon.AddSunTrackToImage(width, SunAndMoon.BrisbaneSunriseDatafile, dayOfYear, strMoonPhase);
            //                return suntrack;
            //            }
            //
            //            return null;
            //        }
            //
            //        public static Image<Rgb24> AddSunTrackToImage(int width, DateTimeOffset? dateTimeOffset, SiteDescription siteDescription)
            //        {
            //            return AddSunTrackToImage(width, dateTimeOffset, siteDescription.SiteName, siteDescription.Latitude, siteDescription.Longitude);
            //=======
            //            int year = ((DateTimeOffset)dateTimeOffset).Year;
            //            int dayOfYear = ((DateTimeOffset)dateTimeOffset).DayOfYear;
            //            double moonPhase = SunAndMoon.GetPhaseOfMoon((DateTimeOffset)dateTimeOffset);
            //            string strMoonPhase = SunAndMoon.ConvertMoonPhaseToString(moonPhase);
            //            Image<Rgb24> suntrack = SunAndMoon.AddSunTrackToImage(width, sunriseDatafile, year, dayOfYear, strMoonPhase);
            //            return suntrack;
            //>>>>>>> master
        }

        /// <summary>
        /// This method assumes that the argument "dayValue" will not take zero value i.e. dayValue=1 represents January 1st.
        /// </summary>
        public static Image<Rgb24> AddSunTrackToImage(int width, FileInfo sunriseSetData, int year, int dayValue, string moonPhase = null)
        {
            int trackHeight = 18;
            Image<Rgb24> image = new Image<Rgb24>(width, trackHeight);

            bool leapYear = false;
            if (year % 4 == 0)
            {
                leapYear = true;
            }

            // A hack to deal with leap years and other astronomical catastrophes.
            if (leapYear && dayValue > 59)
            {
                dayValue -= 1;
            }

            List<string> lines = FileTools.ReadTextFile(sunriseSetData.FullName);
            if (lines == null)
            {
                return null;
            }

            // data file not long enough to contain the requierd day of year
            if (lines.Count - 1 < dayValue)
            {
                return null; // -1 because first line is header.
            }

            // if there is an exception in reading the sunrise/sunset data file then return null;
            try
            {
                string[] fields = lines[dayValue].Split(',');

                // the sunrise/sunset data has the following line format
                // DayOfyear    Date    Astro start Astro end   Naut start  Naut end    Civil start Civil end   Sunrise   Sunset
                //    1       1-Jan-13  3:24 AM      8:19 PM    3:58 AM      7:45 PM    4:30 AM     7:13 PM     4:56 AM   6:47 PM

                string[] nautiRiseArray = fields[4].Split(' ');
                string[] nautiSetArray = fields[5].Split(' ');
                nautiRiseArray = nautiRiseArray[0].Split(':');
                nautiSetArray = nautiSetArray[0].Split(':');
                int nautiRiseMinute = (int.Parse(nautiRiseArray[0]) * 60) + int.Parse(nautiRiseArray[1]);
                int nautiSetMinute = (int.Parse(nautiSetArray[0]) * 60) + int.Parse(nautiSetArray[1]) + 720;
                int nautiDayLength = nautiSetMinute - nautiRiseMinute + 1;

                string[] civilRiseArray = fields[6].Split(' ');
                string[] civilSetArray = fields[7].Split(' ');
                civilRiseArray = civilRiseArray[0].Split(':');
                civilSetArray = civilSetArray[0].Split(':');
                int civilRiseMinute = (int.Parse(civilRiseArray[0]) * 60) + int.Parse(civilRiseArray[1]);
                int civilSetMinute = (int.Parse(civilSetArray[0]) * 60) + int.Parse(civilSetArray[1]) + 720;
                int civilDayLength = civilSetMinute - civilRiseMinute + 1;

                string[] sunriseArray = fields[8].Split(' ');

                string[] sunsetArray = fields[9].Split(' ');
                sunriseArray = sunriseArray[0].Split(':');
                sunsetArray = sunsetArray[0].Split(':');
                int sunriseMinute = (int.Parse(sunriseArray[0]) * 60) + int.Parse(sunriseArray[1]);
                int sunsetMinute = (int.Parse(sunsetArray[0]) * 60) + int.Parse(sunsetArray[1]) + 720;
                int sunDayLength = sunsetMinute - sunriseMinute + 1;

                image.Mutate(g =>
                {
                    Color cbgn = Color.FromRgb(0, 0, 35);
                    g.Clear(cbgn);
                    g.FillRectangle(Brushes.Solid(Color.Gray), nautiRiseMinute, 1, nautiDayLength, trackHeight - 2);
                    g.FillRectangle(Brushes.Solid(Color.LightSalmon), civilRiseMinute, 1, civilDayLength, trackHeight - 2);
                    g.FillRectangle(Brushes.Solid(Color.SkyBlue), sunriseMinute, 1, sunDayLength, trackHeight - 2);

                    if (moonPhase != null)
                    {
                        var font = Drawing.Arial9;
                        g.DrawText(moonPhase, font, Color.White, new PointF(5, 1));
                    }
                });
            }
            catch
            {
                // error reading the sunrise/sunset data file
                return null;
            }

            return image;
        }

        public static void AddSunRiseSetLinesToImage(Image<Rgb24> image, FileInfo sunriseSetData, int startdayOfYear, int endDayOfYear, int pixelStep)
        {
            List<string> lines = FileTools.ReadTextFile(sunriseSetData.FullName);
            int imageRow = 0;

            //  skip header
            for (int i = startdayOfYear; i <= endDayOfYear; i++)
            {
                string[] fields = lines[i].Split(',');

                // the sunrise data hasthe below line format
                // DayOfyear    Date    Astro start Astro end   Naut start  Naut end    Civil start Civil end   Sunrise   Sunset
                //    1       1-Jan-13  3:24 AM      8:19 PM    3:58 AM      7:45 PM    4:30 AM     7:13 PM     4:56 AM   6:47 PM

                int dayOfYear = int.Parse(fields[0]);
                string[] sunriseArray = fields[6].Split(' ');
                string[] sunsetArray = fields[7].Split(' ');
                sunriseArray = sunriseArray[0].Split(':');
                sunsetArray = sunsetArray[0].Split(':');
                int sunriseMinute = (int.Parse(sunriseArray[0]) * 60) + int.Parse(sunriseArray[1]);
                int sunsetMinute = (int.Parse(sunsetArray[0]) * 60) + int.Parse(sunsetArray[1]) + 720;
                for (int px = 0; px < pixelStep; px++)
                {
                    image[sunriseMinute, imageRow] = Color.White;
                    image[sunsetMinute, imageRow] = Color.White;
                    imageRow++;
                }

                // this is a hack to deal with inserting weekly gridlines rather than overwriting the image.
                // This was done for EASY images but not for 3D!!
                // ONE DAY THIS WILL HAVE TO BE FIXED!
                if (dayOfYear % 7 == 0 || dayOfYear % 30 == 0)
                {
                    imageRow++;
                }
            }
        }

        public static DateTimeOffset ParseString2DateTime(string dtString)
        {
            // assume that first eight digits constitute a date string.
            int year = int.Parse(dtString.Substring(0, 4));
            int mnth = int.Parse(dtString.Substring(4, 2));
            int day = int.Parse(dtString.Substring(6, 2));

            // assume skip digit and then next six digits constitute a time of day string.
            int hour = int.Parse(dtString.Substring(9, 2));
            int min = int.Parse(dtString.Substring(11, 2));
            int sec = int.Parse(dtString.Substring(13, 2));

            //?? TODO TODO TODO TODO CANNOT GET DATE TIME STIRNG TO PARSE
            DateTimeOffset dto = new DateTimeOffset(year, mnth, day, hour, min, sec, TimeSpan.Zero);

            //Acoustics.Shared.FileDateHelpers.FileNameContainsDateTime(dtString, out dto);
            return dto;
        }

        /// <summary>
        /// This method is a quick and dirty hack to rad in some tidal info about Georgia Coast and add into spectrogram images.
        /// The method is not efficient - just enough to the job done!.
        /// </summary>
        public static SunMoonTides[] ReadGeorgiaTidalInformation(FileInfo file)
        {
            List<string> lines = FileTools.ReadTextFile(file.FullName);
            int lineCount = lines.Count;
            var list = new List<SunMoonTides>();
            var tides = new SunMoonTides();

            //  skip header
            for (int i = 1; i < lineCount; i++)
            {
                if (lines[i].Length < 6)
                {
                    continue;
                }

                string[] fields = lines[i].Split(' ');

                // the Georgia tidal data has the below line format
                // 2013-03-01 Fri  6:50 AM EST   Sunrise
                // 2013 - 03 - 01 Fri 10:13 AM EST    7.2 feet High Tide
                // 2013 - 03 - 01 Fri  4:34 PM EST   -0.6 feet Low Tide
                // 2013 - 03 - 01 Fri  6:21 PM EST   Sunset

                string[] dateFields = fields[0].Split('-');
                int year = int.Parse(dateFields[0]);
                int month = int.Parse(dateFields[1]);
                int day = int.Parse(dateFields[2]);
                var dateTime = new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);

                tides = new SunMoonTides();
                tides.Date = dateTime;

                string time = fields[2];
                string ampm = fields[3];
                string est = fields[4];
                if (time == string.Empty)
                {
                    time = fields[3];
                    ampm = fields[4];
                    est = fields[5];
                }

                string[] hrminFields = time.Split(':');
                int hour = int.Parse(hrminFields[0]);
                int minute = int.Parse(hrminFields[1]);

                var dto = new DateTimeOffset(year, month, day, hour, minute, 0, TimeSpan.Zero);
                if (ampm == "PM" && hour < 12)
                {
                    dto = dto.AddHours(12);
                }
                else
                if (ampm == "AM" && hour == 12)
                {
                    dto = dto.AddHours(-12);
                }

                // correct for daylight saving
                if (est == "EDT")
                {
                    dto = dto.AddHours(-1);
                }

                if (fields[fields.Length - 1] == SunMoonTides.SUNRISE)
                {
                    //var dto = new DateTimeOffset();
                    tides.Dictionary.Add(SunMoonTides.SUNRISE, dto);
                }
                else
                if (fields[fields.Length - 1] == SunMoonTides.SUNSET)
                {
                    //var dto = new DateTimeOffset();
                    tides.Dictionary.Add(SunMoonTides.SUNRISE, dto);
                }
                else
                if (fields[fields.Length - 2] == "Low" && fields[fields.Length - 1] == "Tide")
                {
                    //var dto = new DateTimeOffset();
                    tides.Dictionary.Add(SunMoonTides.LOWTIDE, dto);
                }
                else
                if (fields[fields.Length - 2] == "High" && fields[fields.Length - 1] == "Tide")
                {
                    //var dto = new DateTimeOffset();
                    tides.Dictionary.Add(SunMoonTides.HIGHTIDE, dto);
                }

                list.Add(tides);
            } // for

            return list.ToArray();
        } // ReadGeorgiaTidalInformation()
    }
}