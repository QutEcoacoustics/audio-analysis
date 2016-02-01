using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using TowseyLibrary;

namespace AudioAnalysisTools
{
    public static class SunAndMoon
    {
        public class SunMoonTides
        {
            public const string LOWTIDE  = "LowTide";
            public const string HIGHTIDE = "HighTide";
            public const string SUNRISE  = "Sunrise";
            public const string SUNSET   = "Sunset";
            public DateTimeOffset Date { get; set; }
            //public DateTimeOffset LowTide { get; set; }
            //public DateTimeOffset HighTide { get; set; }
            //public DateTimeOffset Sunrise { get; set; }
            //public DateTimeOffset Sunset { get; set; }

            public Dictionary<string, DateTimeOffset> dictionary = new Dictionary<string, DateTimeOffset>(); 

        }


        public static FileInfo BrisbaneSunriseDatafile = @"C:\SensorNetworks\OutputDataSets\SunRiseSet\SunriseSet2013Brisbane.csv".ToFileInfo();


        /// <summary>
        /// The data for establishing the exact startDTO for the phase of moon in 2010 was obtained at the folowing website:
        /// http://www.timeanddate.com/moon/phases/australia/brisbane
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
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
                remainderCycle = 1 + remainderCycle;

            return remainderCycle;
        }


        /// <summary>
        /// the moon phsae value is assumed to be between 0 and 1. 0 = new moon. 1 = newmoon. 0.5 = full moon.
        /// </summary>
        /// <param name="phase"></param>
        /// <returns></returns>
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
        /// This is a quick HACK!!!!!!!
        /// 
        /// returns an astronomical track depending on site etc.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="dateTimeOffset"></param>
        /// <param name="siteName"></param>
        /// <returns></returns>
        public static Bitmap AddSunTrackToImage(int width, DateTimeOffset? dateTimeOffset, string siteName, double? latitude, double? longitude)
        {
            if (siteName == null)
            {
                return null;
            }

            if (siteName.StartsWith("Gympie") || siteName.StartsWith("Woondum3") || siteName.StartsWith("SERF"))
            {
                int dayOfYear = ((DateTimeOffset)dateTimeOffset).DayOfYear;
                double moonPhase = SunAndMoon.GetPhaseOfMoon((DateTimeOffset)dateTimeOffset);
                string strMoonPhase = SunAndMoon.ConvertMoonPhaseToString(moonPhase);
                Bitmap suntrack = SunAndMoon.AddSunTrackToImage(width, SunAndMoon.BrisbaneSunriseDatafile, dayOfYear, strMoonPhase);
                return suntrack;
            }

            return null;
        }

        public static Bitmap AddSunTrackToImage(int width, DateTimeOffset? dateTimeOffset, SiteDescription siteDescription)
        {
            return AddSunTrackToImage(width, dateTimeOffset, siteDescription.SiteName, siteDescription.Latitude, siteDescription.Longitude);        
        }

        /// <summary>
        /// This method assumes that the argument "dayValue" will not take zero value i.e. dayValue=1 represents January 1st.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="sunriseSetData"></param>
        /// <param name="dayValue"></param>
        /// <returns></returns>
        public static Bitmap AddSunTrackToImage(int width, FileInfo sunriseSetData, int dayValue, string moonPhase = null)
        {
            // if dayValue >= 366, then set dayValue = 365  
            // i.e. a rough hack to deal with leap years and other astronomical catastrophes.
            if (dayValue >= 366) dayValue = 365;

            List<string> lines = FileTools.ReadTextFile(sunriseSetData.FullName);
            int trackHeight = 18;
            Bitmap image = new Bitmap(width, trackHeight);

            // if not leap year will not have 366 days
            if ((lines.Count - 1) <= dayValue) return image;

            string[] fields = lines[dayValue].Split(',');
            // the sunrise/sunset data has the following line format
            // DayOfyear	Date	Astro start	Astro end	Naut start	Naut end	Civil start	Civil end	Sunrise	  Sunset
            //    1	      1-Jan-13	3:24 AM	     8:19 PM	3:58 AM	     7:45 PM	4:30 AM	    7:13 PM	    4:56 AM	  6:47 PM

            string[] nautiRiseArray = fields[4].Split(' ');
            string[] nautiSetArray = fields[5].Split(' ');
            nautiRiseArray = nautiRiseArray[0].Split(':');
            nautiSetArray = nautiSetArray[0].Split(':');
            int nautiRiseMinute = (Int32.Parse(nautiRiseArray[0]) * 60) + Int32.Parse(nautiRiseArray[1]);
            int nautiSetMinute = (Int32.Parse(nautiSetArray[0]) * 60) + Int32.Parse(nautiSetArray[1]) + 720;
            int nautiDayLength = nautiSetMinute - nautiRiseMinute + 1;

            string[] civilRiseArray = fields[6].Split(' ');
            string[] civilSetArray = fields[7].Split(' ');
            civilRiseArray = civilRiseArray[0].Split(':');
            civilSetArray = civilSetArray[0].Split(':');
            int civilRiseMinute = (Int32.Parse(civilRiseArray[0]) * 60) + Int32.Parse(civilRiseArray[1]);
            int civilSetMinute = (Int32.Parse(civilSetArray[0]) * 60) + Int32.Parse(civilSetArray[1]) + 720;
            int civilDayLength = civilSetMinute - civilRiseMinute + 1;

            string[] sunriseArray = fields[8].Split(' ');

            string[] sunsetArray = fields[9].Split(' ');
            sunriseArray = sunriseArray[0].Split(':');
            sunsetArray = sunsetArray[0].Split(':');
            int sunriseMinute = (Int32.Parse(sunriseArray[0]) * 60) + Int32.Parse(sunriseArray[1]);
            int sunsetMinute = (Int32.Parse(sunsetArray[0]) * 60) + Int32.Parse(sunsetArray[1]) + 720;
            int sunDayLength = sunsetMinute - sunriseMinute + 1;

            Graphics g = Graphics.FromImage(image);
            Color cbgn = Color.FromArgb(0, 0, 35);
            g.Clear(cbgn);
            g.FillRectangle(Brushes.Gray, nautiRiseMinute, 1, nautiDayLength, trackHeight - 2);
            g.FillRectangle(Brushes.LightSalmon, civilRiseMinute, 1, civilDayLength, trackHeight - 2);
            g.FillRectangle(Brushes.SkyBlue, sunriseMinute, 1, sunDayLength, trackHeight - 2);

            if (moonPhase != null)
            {
                Font font = new Font("Arial", 9);
                g.DrawString(moonPhase, font, Brushes.White, new PointF(5, 1));
            }

            return image;
        }

        public static void AddSunRiseSetLinesToImage(Bitmap image, FileInfo sunriseSetData, int startdayOfYear, int endDayOfYear, int pixelStep)
        {
            List<string> lines = FileTools.ReadTextFile(sunriseSetData.FullName);
            int imageRow = 0;
            for (int i = startdayOfYear; i <= endDayOfYear; i++) // skip header
            {
                string[] fields = lines[i].Split(',');
                // the sunrise data hasthe below line format
                // DayOfyear	Date	Astro start	Astro end	Naut start	Naut end	Civil start	Civil end	Sunrise	  Sunset
                //    1	      1-Jan-13	3:24 AM	     8:19 PM	3:58 AM	     7:45 PM	4:30 AM	    7:13 PM	    4:56 AM	  6:47 PM

                int dayOfYear = Int32.Parse(fields[0]);
                string[] sunriseArray = fields[6].Split(' ');
                string[] sunsetArray = fields[7].Split(' ');
                sunriseArray = sunriseArray[0].Split(':');
                sunsetArray = sunsetArray[0].Split(':');
                int sunriseMinute = (Int32.Parse(sunriseArray[0]) * 60) + Int32.Parse(sunriseArray[1]);
                int sunsetMinute = (Int32.Parse(sunsetArray[0]) * 60) + Int32.Parse(sunsetArray[1]) + 720;
                for (int px = 0; px < pixelStep; px++)
                {
                    image.SetPixel(sunriseMinute, imageRow, Color.White);
                    image.SetPixel(sunsetMinute,  imageRow, Color.White);
                    imageRow++;
                }

                // this is a hack to deal with inserting weekly gridlines rather than overwriting the image.
                // This was done for EASY images but not for 3D!!
                // ONE DAY THIS WILL HAVE TO BE FIXED! 
                if ((dayOfYear % 7 == 0)|| (dayOfYear % 30 == 0))
                {
                    imageRow++;
                }
            }

        }





        public static DateTimeOffset ParseString2DateTime(string dtString)
        {
            // assume that first eight digits constitute a date string.
            int year = Int32.Parse(dtString.Substring(0, 4));
            int mnth = Int32.Parse(dtString.Substring(4, 2));
            int day = Int32.Parse(dtString.Substring(6, 2));
            // assume skip digit and then next six digits constitute a time of day string.
            int hour = Int32.Parse(dtString.Substring(9, 2));
            int min = Int32.Parse(dtString.Substring(11, 2));
            int sec = Int32.Parse(dtString.Substring(13, 2));

            //?? TODO TODO TODO TODO CANNOT GET DATE TIME STIRNG TO PARSE
            DateTimeOffset dto = new DateTimeOffset(year, mnth, day, hour, min, sec, TimeSpan.Zero);
            //Acoustics.Shared.FileDateHelpers.FileNameContainsDateTime(dtString, out dto);
            return dto;
        }

        /// <summary>
        /// This method is a quick and dirty hack to rad in some tidal info about Georgia Coast and add into spectrogram images.
        /// The method is not efficient - just enough to the job done!
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static SunMoonTides[] ReadGeorgiaTidalInformation(FileInfo file)
        {
            List<string> lines = FileTools.ReadTextFile(file.FullName);
            int lineCount = lines.Count;
            var list = new List<SunMoonTides>();
            var tides = new SunMoonTides();

            for (int i = 1; i < lineCount; i++) // skip header
            {
                if (lines[i].Length < 6) continue;

                string[] fields = lines[i].Split(' ');

                // the Georgia tidal data has the below line format
                // 2013-03-01 Fri  6:50 AM EST   Sunrise
                // 2013 - 03 - 01 Fri 10:13 AM EST    7.2 feet High Tide
                // 2013 - 03 - 01 Fri  4:34 PM EST   -0.6 feet Low Tide
                // 2013 - 03 - 01 Fri  6:21 PM EST   Sunset

                string[] dateFields = fields[0].Split('-');
                int year = Int32.Parse(dateFields[0]);
                int month = Int32.Parse(dateFields[1]);
                int day = Int32.Parse(dateFields[2]);
                var dateTime = new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);

                tides = new SunMoonTides();
                tides.Date = dateTime;

                string time = fields[2];
                string ampm = fields[3];
                string est  = fields[4];
                if (time == "")
                {
                    time = fields[3];
                    ampm = fields[4];
                    est  = fields[5];
                }
                string[] hrminFields = time.Split(':');
                int hour = Int32.Parse(hrminFields[0]);
                int minute = Int32.Parse(hrminFields[1]);

                var dto = new DateTimeOffset(year, month, day, hour, minute, 0, TimeSpan.Zero );
                if ((ampm == "PM") && (hour < 12)) dto = dto.AddHours(12);
                else 
                if ((ampm == "AM") && (hour == 12)) dto = dto.AddHours(-12);

                // correct for daylight saving
                if (est == "EDT") dto = dto.AddHours(-1); 



                if (fields[fields.Length-1] == SunMoonTides.SUNRISE)
                {
                    //var dto = new DateTimeOffset();
                    tides.dictionary.Add(SunMoonTides.SUNRISE, dto);
                }
                else
                if (fields[fields.Length-1] == SunMoonTides.SUNSET)
                {
                    //var dto = new DateTimeOffset();
                    tides.dictionary.Add(SunMoonTides.SUNRISE, dto);
                }
                else
                if ((fields[fields.Length-2] == "Low") && (fields[fields.Length-1] == "Tide"))
                {

                    //var dto = new DateTimeOffset();
                    tides.dictionary.Add(SunMoonTides.LOWTIDE, dto);
                }
                else
                if ((fields[fields.Length - 2] == "High") && (fields[fields.Length - 1] == "Tide"))
                {
                    //var dto = new DateTimeOffset();
                    tides.dictionary.Add(SunMoonTides.HIGHTIDE, dto);
                }

                list.Add(tides);
            } // for


            return list.ToArray();
        } // ReadGeorgiaTidalInformation()

    }
}
