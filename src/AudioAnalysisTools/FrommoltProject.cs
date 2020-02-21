// <copyright file="FrommoltProject.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using SixLabors.ImageSharp;
    using System.IO;
    using Acoustics.Shared;
    using Acoustics.Shared.ImageSharp;
    using Indices;
    using SixLabors.Fonts;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;
    using Path = System.IO.Path;

    public static class FrommoltProject
    {
        public static void ConcatenateDays()
        {
            DirectoryInfo parentDir = new DirectoryInfo(@"C:\SensorNetworks\Output\Frommolt");
            DirectoryInfo dataDir = new DirectoryInfo(parentDir + @"\AnalysisOutput\mono");
            var imageDirectory = new DirectoryInfo(parentDir + @"\ConcatImageOutput");

            //string indexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfigHiRes.yml";
            DateTimeOffset? startDate = new DateTimeOffset(2012, 03, 29, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset? endDate = new DateTimeOffset(2012, 06, 20, 0, 0, 0, TimeSpan.Zero);
            var timeSpanOffsetHint = new TimeSpan(01, 0, 0);

            //string fileSuffix = @"2Maps.png";
            //string fileSuffix = @"ACI-ENT-EVN.png";
            // WARNING: POW was removed in December 2018
            string fileSuffix = @"BGN-POW-EVN.png";

            TimeSpan totalTimespan = (DateTimeOffset)endDate - (DateTimeOffset)startDate;
            int dayCount = totalTimespan.Days + 1; // assume last day has full 24 hours of recording available.

            bool verbose = true;
            if (verbose)
            {
                LoggedConsole.WriteLine("\n# Start date = " + startDate.ToString());
                LoggedConsole.WriteLine("# End   date = " + endDate.ToString());
                LoggedConsole.WriteLine($"# Elapsed time = {dayCount * 24:f1} hours");
                LoggedConsole.WriteLine("# Day  count = " + dayCount + " (inclusive of start and end days)");
                LoggedConsole.WriteLine("# Time Zone  = " + timeSpanOffsetHint.ToString());
            }

            //string dirMatch = "Monitoring_Rosin_2012*T*+0200_.merged.wav.channel_0.wav";
            string stem = "Monitoring_Rosin_2012????T??0000+0200_.merged.wav.channel_";
            string dirMatch = stem + "?.wav";
            DirectoryInfo[] subDirectories = dataDir.GetDirectories(dirMatch, SearchOption.AllDirectories);

            string format = "yyyyMMdd";
            string startDay = ((DateTimeOffset)startDate).ToString(format);

            //string fileMatch = stem + "?__" + fileSuffix;
            //FileInfo[] files = IndexMatrices.GetFilesInDirectories(subDirectories, fileMatch);

            // Sort the files by date and return as a dictionary: sortedDictionaryOfDatesAndFiles<DateTimeOffset, FileInfo>
            //var sortedDictionaryOfDatesAndFiles = FileDateHelpers.FilterFilesForDates(files, timeSpanOffsetHint);

            //following needed if a day is missing.
            int defaultDayWidth = 20;
            int defaultDayHeight = 300;

            var brush = Color.White;
            Font stringFont = Drawing.Tahoma12;

            var list = new List<Image<Rgb24>>();

            // loop over days
            for (int d = 0; d < dayCount; d++)
            {
                Console.WriteLine($"Day {d} of {dayCount} days");
                var thisday = ((DateTimeOffset)startDate).AddDays(d);
                string date = thisday.ToString(format);

                stem = "Monitoring_Rosin_" + date + "T??0000+0200_.merged.wav.channel_";
                string fileMatch = stem + "?__" + fileSuffix;
                FileInfo[] files = IndexMatrices.GetFilesInDirectories(subDirectories, fileMatch);
                if (files.Length == 0)
                {
                    Image<Rgb24> gapImage = new Image<Rgb24>(defaultDayWidth, defaultDayHeight);
                    gapImage.Mutate(g5 =>
                    {
                        g5.Clear(Color.Gray);
                        g5.DrawText("Day", stringFont, brush, new PointF(2, 5));
                        g5.DrawText("missing", stringFont, brush, new PointF(2, 35));
                    });

                    list.Add(gapImage);

                    continue;
                }

                // Sort the files by date and return as a dictionary: sortedDictionaryOfDatesAndFiles<DateTimeOffset, FileInfo>
                //var sortedDictionaryOfDatesAndFiles = FileDateHelpers.FilterFilesForDates(files, timeSpanOffsetHint);

                var image = ConcatenateFourChannelImages(files, imageDirectory, fileSuffix, date);

                defaultDayHeight = image.Height;
                list.Add(image);
            }

            var combinedImage = ImageTools.CombineImagesInLine(list);

            Image<Rgb24> labelImage1 = new Image<Rgb24>(combinedImage.Width, 24);
            labelImage1.Mutate(g1 =>
            {
                g1.Clear(Color.Black);
                g1.DrawText(fileSuffix, stringFont, brush, new PointF(2, 2));
            });

            //labelImage1.Save(Path.Combine(imageDirectory.FullName, suffix1));
            combinedImage.Mutate(g => { g.DrawImage(labelImage1, 0, 0); });
            string fileName = string.Format(startDay + "." + fileSuffix);
            combinedImage.Save(Path.Combine(imageDirectory.FullName, fileName));
        }

        public static Image<Rgb24> ConcatenateFourChannelImages(FileInfo[] imageFiles, DirectoryInfo imageDirectory, string fileSuffix, string date)
        {
            // get first image to find its dimensions
            var image = (Image<Rgb24>)Image.Load(imageFiles[0].FullName);

            var brush = Color.White;
            Font stringFont = Drawing.Tahoma12;

            //create spacer image
            int width = 1;
            int height = image.Height;
            Image<Rgb24> spacerImage = new Image<Rgb24>(width, height);
            spacerImage.Mutate(g => {
                g.Clear(Color.DarkGray);
            });

            // init output list of images
            var fourChannelList = new List<Image<Rgb24>>();
            for (int channel = 0; channel < 4; channel++)
            {
                var imageList = new List<Image<Rgb24>>();

                //   Monitoring_Rosin_20120329T000000 + 0200_.merged.wav.channel_0__2Maps.png;
                string fileMatch = $@"0000+0200_.merged.wav.channel_{channel}__{fileSuffix}";

                foreach (FileInfo imageFile in imageFiles)
                {
                    if (!imageFile.Name.EndsWith(fileMatch))
                    {
                        continue;
                    }

                    image = (Image<Rgb24>)Image.Load(imageFile.FullName);
                    imageList.Add(image);
                    imageList.Add(spacerImage);
                }

                imageList.Add(spacerImage);
                imageList.Add(spacerImage);
                var concatImage = ImageTools.CombineImagesInLine(imageList);
                concatImage.Mutate(g =>
                {
                    string chn = $"ch{channel + 1}";
                    g.DrawText(chn, stringFont, brush, new PointF(2, 40));
                });

                fourChannelList.Add(concatImage);
            }

            var combinedImage = ImageTools.CombineImagesVertically(fourChannelList);
            return combinedImage;
        }
    }
}
