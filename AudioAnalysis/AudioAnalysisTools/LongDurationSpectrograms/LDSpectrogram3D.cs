// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LDSpectrogram3D.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   This class generates three dimensional false-colour spectrograms of long duration audio recordings.
//   The three dimensions are: 1) Y-axis = frequency bin; X-axis = time of day (either 1435 minutes or 24 hours); Z-axis = consecutive days through year.
//   It does not calculate the indices but reads them from pre-calculated values in csv files.
//
//   Important properties are:
//   All the arguments can be passed through a config file.
//   Create the config file through an instance of the class LDSpectrogramConfig
//   and then call config.WritConfigToYAML(FileInfo path).
//   Then pass that path to the above static method.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using Acoustics.Shared;
    using Indices;
    using log4net;
    using StandardSpectrograms;
    using TowseyLibrary;

    /// <summary>
    /// This class generates false-colour spectrograms of long duration audio recordings.
    /// Important properties are:
    /// 1) the colour map which maps three acoutic indices to RGB.
    /// 2) The scale of the x and y axes which are dtermined by the sample rate, frame size etc.
    /// In order to create false colour spectrograms, copy the method
    ///         public static void DrawFalseColourSpectrograms(LDSpectrogramConfig configuration)
    /// All the arguments can be passed through a config file.
    /// Create the config file throu an instance of the class LDSpectrogramConfig
    /// and then call config.WritConfigToYAML(FileInfo path).
    /// Then pass that path to the above static method.
    /// </summary>
    public static class LdSpectrogram3D
    {
        public const string KeyYear = "Year";
        public const string KeyDayOfYear = "DayOfYear";
        public const string KeyMinOfDay = "MinOfDay";
        public const string KeyFreqBin = "FreqBin";

        public const int TotalMinutesInDay = 1440;
        public const int TotalDaysInYear = 365;

        [Obsolete("See https://github.com/QutBioacoustics/audio-analysis/issues/134")]
        private static Arguments Dev()
        {
            DateTime time = DateTime.Now;
            string datestamp = $"{time.Year}{time.Month:d2}{time.Day:d2}";
            var dev = new Arguments();
            dev.IndexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml"
                .ToFileInfo();
            dev.BrisbaneSunriseDatafile = @"C:\SensorNetworks\OutputDataSets\SunRiseSet\SunriseSet2013Brisbane.csv".ToFileInfo();
            dev.InputDir = @"C:\SensorNetworks\OutputDataSets\SERF - November 2013 Download".ToDirectoryInfo();
            dev.TableDir = (@"C:\SensorNetworks\OutputDataSets\Spectrograms3D\").ToDirectoryInfo();
            dev.OutputDir = (@"C:\SensorNetworks\Output\FalseColourSpectrograms\Spectrograms3D\" /* + datestamp*/)
                .ToDirectoryInfo();
            dev.SampleRate = 17640;
            dev.FrameSize = 512;
            dev.Verbose = true;
            return dev;
        }

        // use the following paths for the command line for the <audio2sonogram> task.
        public class Arguments
        {
            public FileInfo IndexPropertiesConfig { get; set; }

            public FileInfo BrisbaneSunriseDatafile { get; set; }

            public bool Verbose { get; set; }

            public DirectoryInfo InputDir { get; set; }

            //public FileInfo     SonoConfig { get; set; }
            public DirectoryInfo TableDir { get; set; } // intermediate storage for pivot table files

            public DirectoryInfo OutputDir { get; set; }

            public int SampleRate { get; set; }

            public int FrameSize { get; set; }

            public static string Description()
            {
                return "Reads Pivot-table files to create 3-D spectrograms.";
            }

            public static string AdditionalNotes()
            {
                return "Nothing to add.";
            }
        }

        /// <summary>
        /// This method used to construct slices out of implicit 3-D spectrograms.
        /// As of December 2014 it contains hard coded variables just to get it working.
        /// </summary>
        public static void Main(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            if (!arguments.OutputDir.Exists)
            {
                arguments.OutputDir.Create();
            }

            const string title = "# READ LD data table files to prepare a 3D Spectrogram";
            string dateNow = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(title);
            LoggedConsole.WriteLine(dateNow);
            LoggedConsole.WriteLine("# Index    Properties: " + arguments.IndexPropertiesConfig.Name);
            LoggedConsole.WriteLine("# Input     directory: " + arguments.InputDir.Name);

            //LoggedConsole.WriteLine("# SonogramConfig:   " + arguments.SonoConfig.Name);
            LoggedConsole.WriteLine("# Table     directory: " + arguments.TableDir.Name);
            LoggedConsole.WriteLine("# Output    directory: " + arguments.OutputDir.Name);
            LoggedConsole.WriteLine("# Analysis SampleRate: " + arguments.SampleRate);
            LoggedConsole.WriteLine("# Analysis  FrameSize: " + arguments.FrameSize);

            bool verbose = arguments.Verbose;

            // 1. set up the necessary files
            DirectoryInfo inputDirInfo = arguments.InputDir;
            DirectoryInfo dataTableDirInfo = arguments.TableDir;
            DirectoryInfo opDir = arguments.OutputDir;
            // FileInfo configFile = arguments.SonoConfig;
            FileInfo indexPropertiesConfig = arguments.IndexPropertiesConfig;
            FileInfo sunriseSetData = arguments.BrisbaneSunriseDatafile;
            int sampleRate = arguments.SampleRate;
            int frameSize = arguments.FrameSize;
            int nyquistFreq = sampleRate / 2;
            int freqBinCount = frameSize / 2;
            double freqBinWidth = nyquistFreq / (double)freqBinCount;

            // 2. convert spectral indices to a data table - need only do this once
            // ### IMPORTANT ######################################################################################################################
            // Uncomment the next line when converting spectral indices to a data table for the first time.
            // It calls method to read in index spectrograms and combine all the info into one index table per day
            //SpectralIndicesToAndFromTable.ReadAllSpectralIndicesAndWriteToDataTable(indexPropertiesConfig, inputDirInfo, dataTableDirInfo);

            // ############ use next seven lines to obtain slices at constant DAY OF YEAR
            string key = KeyDayOfYear;
            int step = 1;
            int firstIndex = 71;
            int maxSliceCount = TotalDaysInYear + 1;
            var xInterval = TimeSpan.FromMinutes(60); // one hour intervals = 60 pixels
            int rowId = 3; // FreqBin
            int colId = 2; // MinOfDay

            // ############ use next seven lines to obtain slices at constant FREQUENCY
            //string key = keyFreqBin;
            //int step = 100;
            //int firstIndex = 0;
            //int maxSliceCount = nyquistFreq;
            //var XInterval = TimeSpan.FromMinutes(60);
            //int rowID = 1; // DayOfYear
            //int colID = 2; // MinOfDay

            // ############ use next seven lines to obtain slices at constant MINUTE OF DAY
            //string key = keyMinOfDay;
            //int step = 5;
            //int firstIndex = 0;
            //int maxSliceCount = LDSpectrogram3D.Total_Minutes_In_Day;
            //var XInterval = TimeSpan.FromDays(30.4); // average days per month
            //int rowID = 3; // FreqBin
            //int colID = 1; // DayOfYear

            // These are the column names and order in the csv data strings.
            // Year, DayOfYear, MinOfDay, FreqBin, ACI, AVG, BGN, CVR, TEN, VAR
            string colorMap = "ACI-TEN-CVR";
            int redId = 4; // ACI
            int grnId = 8; // TEN
            int bluId = 7; // CVR
            int year = 2013;

            for (int sliceId = firstIndex; sliceId < maxSliceCount; sliceId += step)
            {
                // DEFINE THE SLICE
                //sliceID = 300; // Herz
                int arrayId = sliceId;
                if (key == "FreqBin")
                {
                    arrayId = (int)Math.Round(sliceId / (double)freqBinWidth);
                }

                var fileStem = string.Format("SERF_2013_" + key + "_{0:d4}", arrayId);

                // 3. Read a data slice from the data table files
                List<string> data;
                var outputFileName = string.Format("{0}.csv", fileStem);
                var path = Path.Combine(opDir.FullName, outputFileName);
                if (File.Exists(path))
                {
                    data = FileTools.ReadTextFile(path);
                }
                else
                {
                    if (key == KeyDayOfYear)
                    {
                        data = GetDaySlice(dataTableDirInfo, year, arrayId);
                    }
                    else
                    {
                        data = GetDataSlice(dataTableDirInfo, key, arrayId);
                    }

                    FileTools.WriteTextFile(path, data);
                }

                // 4. Read the yaml file describing the Index Properties
                Dictionary<string, IndexProperties> dictIp = IndexProperties.GetIndexProperties(indexPropertiesConfig);
                dictIp = InitialiseIndexProperties.FilterIndexPropertiesForSpectralOnly(dictIp);

                // 5. Convert data slice to image
                string[] indexNames = colorMap.Split('-');
                IndexProperties ipRed = dictIp[indexNames[0]];
                IndexProperties ipGrn = dictIp[indexNames[1]];
                IndexProperties ipBlu = dictIp[indexNames[2]];
                Image image = GetImageSlice(key, data, rowId, colId, redId, grnId, bluId, ipRed, ipGrn, ipBlu, freqBinCount);

                // 6. frame the image and save
                image = Frame3DSpectrogram(image, key, arrayId, year, colorMap, xInterval, nyquistFreq, sliceId, sunriseSetData);

                // 7. save the image
                outputFileName = string.Format("{0}.png", fileStem);
                path = Path.Combine(opDir.FullName, outputFileName);
                image.Save(path);
            } // end loop through slices
        } // end Main()

        public static Image GetImageSlice(string key, List<string> data, int rowId, int colId, int redId, int grnId, int bluId,
                                           IndexProperties ipRed, IndexProperties ipGrn, IndexProperties ipBlu, int freqBinCount)
        {
            Bitmap image = null;
            switch (key)
            {
                case KeyFreqBin:
                    image = new Bitmap(TotalMinutesInDay, TotalDaysInYear);
                    break;
                case KeyDayOfYear:
                    image = new Bitmap(TotalMinutesInDay, freqBinCount);
                    break;
                case KeyMinOfDay:
                    image = new Bitmap(TotalDaysInYear, freqBinCount);
                    break;
            }

            int maxRgbValue = 255;

            foreach (string line in data)
            {
                string[] fields = line.Split(',');
                int row = int.Parse(fields[rowId]);
                int col = int.Parse(fields[colId]);

                // these images must be inverted to show zero Hz at bottom
                if ((key == KeyMinOfDay) || (key == KeyDayOfYear))
                {
                    row = freqBinCount - 1 - row;
                }

                var redValue = double.Parse(fields[redId]);
                var grnValue = double.Parse(fields[grnId]);
                var bluValue = double.Parse(fields[bluId]);

                redValue = ipRed.NormaliseValue(redValue);
                grnValue = ipGrn.NormaliseValue(1 - grnValue); // temporal entropy
                bluValue = ipBlu.NormaliseValue(bluValue);

                // de-demphasize the background small values
                //MatrixTools.FilterBackgroundValues(matrix, this.BackgroundFilter);

                var r = Convert.ToInt32(Math.Max(0, redValue * maxRgbValue));
                var g = Convert.ToInt32(Math.Max(0, grnValue * maxRgbValue));
                var b = Convert.ToInt32(Math.Max(0, bluValue * maxRgbValue));
                var colour = Color.FromArgb(r, g, b);
                image.SetPixel(col, row, colour);
            }

            Graphics gr = Graphics.FromImage(image);
            gr.DrawRectangle(new Pen(Color.DarkGray), 0, 0, image.Width - 1, image.Height - 1);
            return image;
        }

        public static Image Frame3DSpectrogram(Image image, string key, int value, int year, string colorMap, TimeSpan xInterval, int nyquistFreq, int unitValue, FileInfo sunriseSetData)
        {
            if (key == KeyDayOfYear)
            {
                var title = string.Format("SPECTROGRAM (hours x Herz): {0}={1}      (R-G-B={2})", key, value, colorMap, unitValue);
                var titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image.Width);
                return FrameSliceOf3DSpectrogram_DayOfYear(image, titleBar, year, value, xInterval, unitValue, sunriseSetData, nyquistFreq);
            }
            else
            if (key == KeyFreqBin)
            {
                var title = string.Format("SPECTROGRAM (hours x months): {0}={1}      (R-G-B={2})", key, value, colorMap, unitValue);
                var titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image.Width);
                return FrameSliceOf3DSpectrogram_ConstantFreq(image, titleBar, xInterval, unitValue, sunriseSetData, nyquistFreq);
            }
            else
            if (key == KeyMinOfDay)
            {
                var title = string.Format("SPECTROGRAM (months x Herz): {0}={1}       (R-G-B={2})", key, value, colorMap, unitValue);
                var titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image.Width);
                return FrameSliceOf3DSpectrogram_ConstantMin((Bitmap)image, titleBar, nyquistFreq, unitValue, sunriseSetData);
            }

            return null;
        }

        public static Image FrameSliceOf3DSpectrogram_DayOfYear(Image bmp1, Image titleBar, int year, int dayOfYear, TimeSpan xInterval, int herzValue, FileInfo sunriseSetData, int nyquistFreq)
        {
            Bitmap suntrack = SunAndMoon.AddSunTrackToImage(bmp1.Width, sunriseSetData, year, dayOfYear);

            Graphics g = Graphics.FromImage(bmp1);
            Pen pen = new Pen(Color.White);
            Font stringFont = new Font("Arial", 12);
            //Font stringFont = new Font("Tahoma", 9);

            DateTime theDate = new DateTime(year, 1, 1).AddDays(dayOfYear - 1);
            string dateString = string.Format("{0} {1} {2:d2}", year, DataTools.MonthNames[theDate.Month - 1], theDate.Day);
            g.DrawString(dateString, stringFont, Brushes.Wheat, new PointF(10, 3));

            TimeSpan xAxisPixelDuration = TimeSpan.FromSeconds(60);
            var minuteOffset = TimeSpan.Zero;
            double secondsDuration = xAxisPixelDuration.TotalSeconds * bmp1.Width;
            TimeSpan fullDuration = TimeSpan.FromSeconds(secondsDuration);

            // init frequency scale
            int herzInterval = 1000;
            int frameSize = bmp1.Height;
            var freqScale = new DSP.FrequencyScale(nyquistFreq, frameSize, herzInterval);

            SpectrogramTools.DrawGridLinesOnImage((Bitmap)bmp1, minuteOffset, fullDuration, xInterval, freqScale);

            int trackHeight = 20;
            int imageHt = bmp1.Height + trackHeight + trackHeight + trackHeight;
            var xAxisTicInterval = TimeSpan.FromMinutes(60); // assume 60 pixels per hour
            var timeScale24Hour = ImageTrack.DrawTimeTrack(fullDuration, minuteOffset, xAxisTicInterval, bmp1.Width, trackHeight, "hours");

            var imageList = new List<Image>();
            imageList.Add(titleBar);
            imageList.Add(timeScale24Hour);
            imageList.Add(suntrack);
            imageList.Add(bmp1);
            imageList.Add(timeScale24Hour);
            Image compositeBmp = ImageTools.CombineImagesVertically(imageList.ToArray());

            // trackHeight = compositeBmp.Height;
            // Bitmap timeScale12Months = ImageTrack.DrawYearScaleVertical(40, trackHeight);
            // Bitmap freqScale = DrawFreqScale_vertical(40, trackHeight, HerzValue, nyquistFreq);

            imageList = new List<Image>();

            // imageList.Add(timeScale12Months);
            imageList.Add(compositeBmp);

            // imageList.Add(freqScale);
            compositeBmp = ImageTools.CombineImagesInLine(imageList.ToArray());

            return compositeBmp;
        }

        public static Image FrameSliceOf3DSpectrogram_ConstantFreq(Image bmp1, Image titleBar, TimeSpan xInterval, int herzValue, FileInfo sunriseSetData, int nyquistFreq)
        {
            SunAndMoon.AddSunRiseSetLinesToImage((Bitmap)bmp1, sunriseSetData, 0 , 365, 1);// assume full year and 1px/day

            var g = Graphics.FromImage(bmp1);
            var pen = new Pen(Color.White);
            var stringFont = new Font("Arial", 12);
            var str = $"Freq = {herzValue} Hz";
            g.DrawString(str, stringFont, Brushes.Wheat, new PointF(10, 7));

            var xAxisPixelDuration = TimeSpan.FromSeconds(60);
            var startOffset = TimeSpan.Zero;
            double secondsDuration = xAxisPixelDuration.TotalSeconds * bmp1.Width;
            var fullDuration = TimeSpan.FromSeconds(secondsDuration);

            // init frequency scale
            int herzInterval = 1000;
            int frameSize = bmp1.Height;
            var freqScale = new DSP.FrequencyScale(nyquistFreq, frameSize, herzInterval);

            SpectrogramTools.DrawGridLinesOnImage((Bitmap)bmp1, startOffset, fullDuration, xInterval, freqScale);

            int trackHeight = 20;
            var xAxisTicInterval = TimeSpan.FromMinutes(60); // assume 60 pixels per hour
            var timeScale24Hour = ImageTrack.DrawTimeTrack(fullDuration, startOffset, xAxisTicInterval, bmp1.Width, trackHeight, "hours");

            var imageList = new List<Image> {titleBar, timeScale24Hour, bmp1, timeScale24Hour};
            var compositeBmp = ImageTools.CombineImagesVertically(imageList.ToArray());
            if (compositeBmp == null)
            {
                throw new ArgumentNullException(nameof(compositeBmp));
            }

            trackHeight = compositeBmp.Height;
            Bitmap timeScale12Months = ImageTrack.DrawYearScaleVertical(40, trackHeight);
            Bitmap freqScaleImage = DrawFreqScale_vertical(40, trackHeight, herzValue, nyquistFreq);

            imageList = new List<Image> {timeScale12Months, compositeBmp, freqScaleImage };
            compositeBmp = ImageTools.CombineImagesInLine(imageList.ToArray());

            return compositeBmp;
        }

        // mark off Y-axis frequency scale.
        public static Bitmap DrawFreqScale_vertical(int yoffset, int trackHeight, int herzValue, int nyquistFreq)
        {
            double herzPerPixel = nyquistFreq / (double)(trackHeight - yoffset);
            double gridInterval = 1000 / herzPerPixel;
            int ymark = trackHeight - (int)Math.Round(herzValue / herzPerPixel);
            int gridCount = nyquistFreq / 1000;

            int xoffset = 10;
            int trackWidth = 45;
            Bitmap bmp = new Bitmap(trackWidth, trackHeight);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.LightGray);
            g.FillRectangle(Brushes.Black, xoffset, 0, bmp.Width - 1, bmp.Height - 1);

            Pen grayPen = new Pen(Color.DarkGray);
            Pen whitePen = new Pen(Color.White);
            Font stringFont = new Font("Arial", 9);
            g.DrawString("Herz", stringFont, Brushes.Yellow, new PointF(xoffset + 2, 6)); //draw label
            g.DrawString("Scale", stringFont, Brushes.Yellow, new PointF(xoffset, 19)); //draw label
            g.DrawLine(whitePen, xoffset, yoffset, trackWidth, yoffset);
            g.DrawLine(whitePen, xoffset, yoffset - 1, trackWidth, yoffset - 1);

            for (int i = 1; i <= gridCount; i++) //for pixels in the line
            {
                int y = trackHeight - (int)Math.Round(i * gridInterval);
                g.DrawLine(whitePen, xoffset, y, trackWidth, y);
            } // end over all pixels

            // draw the current herz mark
            Pen yellowPen = new Pen(Color.Yellow);
            g.DrawLine(yellowPen, xoffset, ymark,     trackWidth, ymark);
            g.DrawLine(yellowPen, xoffset, ymark - 1, trackWidth, ymark - 1);
            g.DrawString(herzValue.ToString(), stringFont, Brushes.White, new PointF(xoffset + 1, ymark - 14)); //draw time

            // g.DrawLine(whitePen, 0, daysInYear + offset, trackWidth, daysInYear + offset);
            // g.DrawLine(whitePen, 0, offset, trackWidth, offset);          //draw lower boundary
            // g.DrawLine(whitePen, duration, 0, duration, trackHeight - 1);//draw right end boundary

            // g.DrawString(title, stringFont, Brushes.White, new PointF(duration + 4, 3));
            return bmp;
        }

        public static Image FrameSliceOf3DSpectrogram_ConstantMin(Bitmap bmp1, Image titleBar, int nyquistFreq, int minuteOfDay, FileInfo sunriseSetData)
        {
            int imageWidth = bmp1.Width;
            int imageHeight = bmp1.Height;

            Graphics g = Graphics.FromImage(bmp1);
            Pen pen = new Pen(Color.White);
            Font stringFont = new Font("Arial", 12);

            TimeSpan time = TimeSpan.FromMinutes((double)minuteOfDay);
            string str = string.Format("Time = {0}h:{1}m", time.Hours, time.Minutes);
            g.DrawString(str, stringFont, Brushes.Wheat, new PointF(10, 7));

            int binCount = 512;
            if (imageHeight <= 256)
            {
                binCount = 256;
            }

            int lineCount = nyquistFreq / 1000;
            double gridLineInterval = 1000 / (nyquistFreq / (double)binCount);

            for (int i = 1; i <= lineCount; i++)
            {
                int y = imageHeight - (int)Math.Round(i * gridLineInterval);
                for (int x = 1; x < imageWidth; x++)
                {
                    bmp1.SetPixel(x, y, Color.White);
                    bmp1.SetPixel(x - 1, y, Color.Black);
                    x++;
                }
            }

            AddDaylightMinutesToImage(bmp1, sunriseSetData, minuteOfDay);

            //TimeSpan xAxisPixelDuration = TimeSpan.FromSeconds(60);
            //var minOffset = TimeSpan.Zero;
            //SpectrogramTools.DrawGridLinesOnImage((Bitmap)bmp1, minOffset, X_interval, xAxisPixelDuration, 120, 10);

            const int trackHeight = 20;
            var timeScale12Months = ImageTrack.DrawYearScale_horizontal(imageWidth, trackHeight);
            var imageList = new List<Image> {titleBar, timeScale12Months, bmp1, timeScale12Months};
            var compositeBmp = ImageTools.CombineImagesVertically(imageList.ToArray());

            //imageWidth = compositeBmp.Height;
            //imageList = new List<Image>();
            //imageList.Add(timeScale12Months);
            //imageList.Add(compositeBmp);
            //compositeBmp = ImageTools.CombineImagesInLine(imageList.ToArray());

            return compositeBmp;
        }

        public static void AddDaylightMinutesToImage(Bitmap image, FileInfo sunriseSetData, int minuteOfDay)
        {
            if (minuteOfDay < 180)
            {
                return;
            }

            if (minuteOfDay > 1260)
            {
                return;
            }

            var lines = FileTools.ReadTextFile(sunriseSetData.FullName);

            for (int i = 1; i <= 365; i++) // skip header
            {
                string[] fields = lines[i].Split(',');

                /*
                 the sunrise data has the below line format
                 DayOfyear	Date	Astro start	Astro end	Naut start	Naut end	Civil start	Civil end	Sunrise	  Sunset
                    1	      1-Jan-13	3:24 AM	     8:19 PM	3:58 AM	     7:45 PM	4:30 AM	    7:13 PM	    4:56 AM	  6:47 PM
                */

                int dayOfYear = int.Parse(fields[0]);
                string[] sunriseArray = fields[6].Split(' ');
                string[] sunsetArray = fields[7].Split(' ');
                sunriseArray = sunriseArray[0].Split(':');
                sunsetArray = sunsetArray[0].Split(':');
                int sunriseMinute = (int.Parse(sunriseArray[0]) * 60) + int.Parse(sunriseArray[1]);
                int sunsetMinute = (int.Parse(sunsetArray[0]) * 60) + int.Parse(sunsetArray[1]) + 720;

                if ((minuteOfDay >= sunriseMinute) && (minuteOfDay <= sunsetMinute))
                {
                    image.SetPixel(dayOfYear, 0, Color.Yellow);
                    image.SetPixel(dayOfYear, 1, Color.Yellow);
                    image.SetPixel(dayOfYear, 2, Color.Yellow);
                }
            }
        }

        /// <summary>
        /// This method reads a single file containg a single day of index values.
        /// The method assumes that the file name has following structure:  XXXXX_YYYYMMDD.SpectralIndices.PivotTable.csv
        /// </summary>
        public static List<string> GetDaySlice(DirectoryInfo dataTableDir, int year, int dayOfYear)
        {
            string key = KeyDayOfYear;
            LoggedConsole.WriteLine("GetDataSlice() for DayOfYear=" + dayOfYear + ": ");

            //get structure of the first file name in the directory
            FileInfo[] fileList = dataTableDir.GetFiles();
            FileInfo file = fileList[0];

            // split the file name into component parts
            string[] nameParts = file.Name.Split('.');
            string[] stemParts = nameParts[0].Split('_');

            var date = new DateTime(year, 1, 1).AddDays(dayOfYear - 1);

            //string stem = stemParts[0] + "_" + date.Year + date.Month + date.Day + "." + nameParts[1] + "." + nameParts[2] + nameParts[3];
            string stem =
                $"{stemParts[0]}_{date.Year:d4}{date.Month:d2}{date.Day:d2}.{nameParts[1]}.{nameParts[2]}.{nameParts[3]}";

            string path = Path.Combine(dataTableDir.FullName, stem);
            if (!File.Exists(path))
            {
                return new List<string>();
            }

            List<string> list = FileTools.ReadSelectedLinesOfCsvFile(path, key, dayOfYear);
            if (list == null)
            {
                return new List<string>();
            }

            return list;
        }

        public static List<string> GetDataSlice(DirectoryInfo dataTableDir, string key, int value)
        {
            LoggedConsole.Write("GetDataSlice() " + key + "=" + value + ": ");
            List<string> outputList = new List<string>();

            FileInfo[] fileList = dataTableDir.GetFiles();
            foreach (FileInfo file in fileList)
            {
                Console.Write("."); // so impatient user knows something is happening!

                List<string> list = FileTools.ReadSelectedLinesOfCsvFile(file.FullName, key, value);
                if (list == null)
                {
                    continue;
                }

                outputList.AddRange(list);
            }

            LoggedConsole.WriteLine();
            return outputList;
        }

        private static Dictionary<string, string> GetConfiguration(FileInfo configFile)
        {
            dynamic configuration = Yaml.Deserialise(configFile);

            var configDict = new Dictionary<string, string>((Dictionary<string, string>)configuration)
            {
                [AnalysisKeys.AddAxes] = ((bool?)configuration[AnalysisKeys.AddAxes] ?? true).ToString(),
                [AnalysisKeys.AddSegmentationTrack] = configuration[AnalysisKeys.AddSegmentationTrack] ?? true,
                [AnalysisKeys.AddTimeScale] = (string)configuration[AnalysisKeys.AddTimeScale] ?? "true",
                [AnalysisKeys.AddAxes] = (string)configuration[AnalysisKeys.AddAxes] ?? "true",
                [AnalysisKeys.AddSegmentationTrack] =
                (string)configuration[AnalysisKeys.AddSegmentationTrack] ?? "true",
            };

            return configDict;
        }

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //############################################################################################################################################################
        //############################################################################################################################################################

        public static double[,] NormaliseSpectrogramMatrix(IndexProperties indexProperties, double[,] matrix, double backgroundFilterCoeff)
        {
            matrix = MatrixTools.NormaliseInZeroOne(matrix, indexProperties.NormMin, indexProperties.NormMax);
            matrix = MatrixTools.FilterBackgroundValues(matrix, backgroundFilterCoeff); // to de-demphasize the background small values
            return matrix;
        }

        ///// <summary>
        ///// This method started 27-11-2014 to process consecutive days of acoustic indices data for 3-D spectrograms.
        ///// </summary>
        //public static void Main_DISCONTINUED_ButMayStillContainUsefulCode(Arguments arguments)
        //{
        //    if (arguments == null)
        //    {
        //        arguments = Dev();
        //    }

        //    if (!arguments.Output.Exists)
        //    {
        //        arguments.Output.Create();
        //    }

        //    const string Title = "# READ LD Spectrogram csv files to prepare a 3D Spectrogram";
        //    string dateNow = "# DATE AND TIME: " + DateTime.Now;
        //    LoggedConsole.WriteLine(Title);
        //    LoggedConsole.WriteLine(dateNow);
        //    LoggedConsole.WriteLine("# Input directory: " + arguments.Source.Name);
        //    LoggedConsole.WriteLine("# Configure  file: " + arguments.Config.Name);
        //    LoggedConsole.WriteLine("# Output directry: " + arguments.Output.Name);

        //    bool verbose = arguments.Verbose;

        //    // 1. set up the necessary files
        //    DirectoryInfo inputDirInfo = arguments.Source;
        //    FileInfo configFile = arguments.Config;
        //    DirectoryInfo opDir = arguments.Output;

        //    // 2. get the config dictionary
        //    var configDict = GetConfiguration(configFile);
        //    // print out the parameters
        //    if (verbose)
        //    {
        //        LoggedConsole.WriteLine("\nPARAMETERS");
        //        foreach (var kvp in configDict)
        //        {
        //            LoggedConsole.WriteLine("{0}  =  {1}", kvp.Key, kvp.Value);
        //        }
        //    }

        //    // COMPONENT FILES IN DIRECTORY HAVE THIS STRUCTURE
        //    //SERF_20130915_201727_000.wav\Towsey.Acoustic\SERF_20130915_201727_000.ACI.csv; SERF_20130915_201727_000.BGN.csv etc

        //    // #################################################################
        //    // ## To save a lot of mucking around, enter the first and last date of the recordings in list.
        //    // #################################################################

        //    // date time of first recording = 20130314_000021_000
        //    DateTime startDate = new DateTime(2013, 03, 14, 00, 00, 21);
        //    // date time of last  recording = 20131010_201733_000
        //    DateTime endDate   = new DateTime(2013, 10, 10, 20, 17, 33);

        //    int startDayOfyear = startDate.DayOfYear;
        //    int endDayOfyear   = endDate.DayOfYear;
        //    int totalDays = endDayOfyear - startDayOfyear + 1;
        //    // total number of minutes in one day
        //    int totalMinutes = 1440;

        //    // location to write the yaml config file for producing long duration spectrograms
        //    FileInfo fiSpectrogramConfig = new FileInfo(Path.Combine(opDir.FullName, "LDSpectrogramConfig.yml"));
        //    // Initialise the default Yaml Config file
        //    var config = new LdSpectrogramConfig("null", inputDirInfo, opDir); // default values have been set
        //    int totalFreqBins = config.FrameLength / 2;
        //    // write the yaml file to config
        //    config.WriteConfigToYaml(fiSpectrogramConfig);

        //    // read the yaml Config file describing the Index Properties
        //    FileInfo indexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml".ToFileInfo();
        //    Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indexPropertiesConfig);
        //    dictIP = InitialiseIndexProperties.GetDictionaryOfSpectralIndexProperties(dictIP);

        //    // set up the 3D matrix to store results.
        //    var matrix3D = new Matrix3D("min", totalMinutes, "freq", totalFreqBins, "day", totalDays);

        //    int count = 0;
        //    DirectoryInfo[] dirList = inputDirInfo.GetDirectories();
        //    foreach (DirectoryInfo dir in dirList)
        //    {
        //        string targetFileName = dir.Name;
        //        string[] nameArray = targetFileName.Split('_');
        //        string date = nameArray[1];
        //        string time = nameArray[2];
        //        int year   = Int32.Parse(date.Substring(0, 4));
        //        int month  = Int32.Parse(date.Substring(4, 2));
        //        int day    = Int32.Parse(date.Substring(6, 2));
        //        int hour   = Int32.Parse(time.Substring(0, 2));
        //        int minute = Int32.Parse(time.Substring(2, 2));
        //        int second = Int32.Parse(time.Substring(4, 2));

        //        DateTime thisDate = new DateTime(year, month, day, hour, minute, second);

        //        int thisDayOfYear   = thisDate.DayOfYear;
        //        int thisStartMinute = thisDate.Minute;
        //        int dayIndex = thisDayOfYear - startDayOfyear;

        //        // get target file name without extention
        //        nameArray = targetFileName.Split('.');
        //        targetFileName = nameArray[0];
        //        string targetDirectory = dir.FullName + @"\Towsey.Acoustic";

        //        //Write the default Yaml Config file for producing long duration spectrograms and place in the output directory
        //        config = new LdSpectrogramConfig(targetFileName, inputDirInfo, opDir); // default values have been set
        //        // write the yaml file to config
        //        config.WriteConfigToYaml(fiSpectrogramConfig);
        //        // read the yaml file to a LdSpectrogramConfig object
        //        LdSpectrogramConfig configuration = LdSpectrogramConfig.ReadYamlToConfig(fiSpectrogramConfig);
        //        configuration.InputDirectoryInfo = targetDirectory.ToDirectoryInfo();

        //        // These parameters manipulate the colour map and appearance of the false-colour spectrogram
        //        //string colorMap1 = configuration.ColourMap1 ?? SpectrogramConstants.RGBMap_BGN_AVG_CVR;   // assigns indices to RGB
        //        string colorMap2 = configuration.ColourMap2 ?? SpectrogramConstants.RGBMap_ACI_ENT_EVN;   // assigns indices to RGB

        //        double backgroundFilterCoeff = (double?)configuration.BackgroundFilterCoeff ?? SpectrogramConstants.BACKGROUND_FILTER_COEFF;
        //        //double  colourGain = (double?)configuration.ColourGain ?? SpectrogramConstants.COLOUR_GAIN;  // determines colour saturation

        //        // These parameters describe the frequency and time scales for drawing the X and Y axes on the spectrograms
        //        TimeSpan minuteOffset = configuration.AnalysisStartOffset;   // default = zero minute of day i.e. midnight
        //        TimeSpan xScale = configuration.XAxisTicInterval; // default is one minute spectra i.e. 60 per hour
        //        int sampleRate = configuration.SampleRate;
        //        int frameWidth = configuration.FrameLength;

        //        var cs1 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap2);
        //        cs1.BaseName = configuration.BaseName;
        //        cs1.BackgroundFilter = backgroundFilterCoeff;
        //        cs1.SetSpectralIndexProperties(dictIP); // set the relevant dictionary of index properties

        //        // reads all known files spectral indices
        //        Logger.Info("Reading spectra files from disk");
        //        cs1.ReadSpectralIndices(configuration.InputDirectoryInfo, configuration.BaseName);

        //        if (cs1.GetCountOfSpectrogramMatrices() == 0)
        //        {
        //            LoggedConsole.WriteLine("No spectrogram matrices in the dictionary. Spectrogram files do not exist?");
        //            return;
        //        }

        //        // get the ACI matrix oriented as per image
        //        double[,] ACImatrix = cs1.GetNormalisedSpectrogramMatrix("ACI");

        //        //int thisEndMinute = thisStartMinute + ACImatrix.GetLength(0);
        //        //for (int Y = thisStartMinute; Y <= thisEndMinute; Y++)
        //        for (int Y = 0; Y < ACImatrix.GetLength(0); Y++) // freq bins
        //        {
        //            for (int X = 0; X < ACImatrix.GetLength(1); X++)  // minutes
        //            {
        //                matrix3D.SetValue(thisStartMinute + X, Y, dayIndex, ACImatrix[Y, X]);
        //            }
        //        }

        //        // for DEBUG
        //        count++;
        //        if(count >=20) break;

        //    } // foreach (DirectoryInfo dir in dirList)

        //    for (int Z = 0; Z < 20; Z++)
        //    {
        //        float[,] m = matrix3D.GetMatrix("ZZ", Z);

        //        string fileName = string.Format("sg.{0:d2}.ACI.png", Z);
        //        string path = Path.Combine(opDir.FullName, fileName);
        //        ImageTools.DrawMatrix(MatrixTools.ConvertMatrixOfFloat2Double(m), path);
        //    }
        //}
    }
}
