// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DrawEasyImage.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

// <summary>
// Action code for this activity = "drawEasyImage"
// </summary>

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using SixLabors.ImageSharp;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.ImageSharp;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using McMaster.Extensions.CommandLineUtils;
    using Production.Arguments;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;
    using Path = System.IO.Path;

    /// <summary>
    /// First argument on command line to call this action is "drawEasyImage"
    /// </summary>
    public static class DrawEasyImage
    {
        public const string CommandName = "DrawEasyImage";

        [Command(
            CommandName,
            Description = "[UNMAINTAINED] Produces image derived from concatenation of multiple consecutive index files. Suitable for years of recorded data.")]
        public class Arguments : SubCommandBase
        {
            [Option(
                CommandOptionType.MultipleValue,
                Description = "One or more directories where the original csv files are located.")]
            public string[] InputDataDirectories { get; set; }

            [Option(Description = "Directory where the output is to go.")]
            [LegalFilePath]
            public string OutputDirectory { get; set; }

            [Option(
                Description = "Filter string used to search for the required csv files - assumed to be in directory path.",
                ShortName = "")]
            public string FileFilter { get; set; }

            [Option(
                Description = "File stem name for output files.",
                ShortName = "")]
            public string FileStemName { get; set; }

            [Option(
                CommandOptionType.SingleValue,
                Description = "The start DateTime.")]
            public DateTimeOffset? StartDate { get; set; }

            [Option(
                CommandOptionType.SingleValue,
                Description = "The end DateTime at which concatenation ends. If missing|null, then will be set = today's date or last available file.")]
            public DateTimeOffset? EndDate { get; set; }

            [Option(
                CommandOptionType.SingleValue,
                Description = "TimeSpan offset hint required if file names do not contain time zone info. Set default to east coast Australia",
                ShortName = "z")]
            public TimeSpan? TimeSpanOffsetHint { get; set; } = new TimeSpan(10, 0, 0);

            //[ArgDescription("User specified file containing a list of indices and their properties.")]
            //[Production.ArgExistingFile(Extension = ".yml")]
            //[ArgPosition(1)]
            internal string IndexPropertiesConfig { get; set; }

            public string BrisbaneSunriseDatafile { get; set; }

            public override Task<int> Execute(CommandLineApplication app)
            {
                DrawEasyImage.Execute(this);
                return this.Ok();
            }
        }

        public static void Execute(Arguments arguments)
        {
            var inputDirs = arguments.InputDataDirectories.Select(FileInfoExtensions.ToDirectoryInfo);
            var output = arguments.OutputDirectory.ToDirectoryInfo();

            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine("\n# DRAW an EASY IMAGE from consecutive days of SUMMARY INDICES in CSV files.");
            LoggedConsole.WriteLine("#    IT IS ASSUMED THAT THE CSV files are already concatenated into 24 hour files.");
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Summary Index.csv files are in directories:");
            foreach (DirectoryInfo dir in inputDirs)
            {
                LoggedConsole.WriteLine("    {0}", dir.FullName);
            }

            LoggedConsole.WriteLine("# Output directory: " + output);
            if (arguments.StartDate == null)
            {
                LoggedConsole.WriteLine("# Start date = NULL (No argument provided). Will revise start date ....");
            }
            else
            {
                LoggedConsole.WriteLine("# Start date = " + arguments.StartDate.ToString());
            }

            if (arguments.EndDate == null)
            {
                LoggedConsole.WriteLine("# End   date = NULL (No argument provided). Will revise end date ....");
            }
            else
            {
                LoggedConsole.WriteLine("# End   date = " + arguments.EndDate.ToString());
            }

            LoggedConsole.WriteLine("# FILE FILTER = " + arguments.FileFilter);
            LoggedConsole.WriteLine();

            // PATTERN SEARCH FOR SUMMARY INDEX FILES.
            //string pattern = "*__Towsey.Acoustic.Indices.csv";
            FileInfo[] csvFiles = IndexMatrices.GetFilesInDirectories(inputDirs.ToArray(), arguments.FileFilter);

            //LoggedConsole.WriteLine("# Subdirectories Count = " + subDirectories.Length);
            LoggedConsole.WriteLine("# SummaryIndexFiles.csv Count = " + csvFiles.Length);

            if (csvFiles.Length == 0)
            {
                LoggedConsole.WriteErrorLine("\n\nWARNING from method DrawEasyImage.Execute():");
                LoggedConsole.WriteErrorLine("        No SUMMARY index files were found.");
                LoggedConsole.WriteErrorLine("        RETURNING EMPTY HANDED!");
                return;
            }

            // Sort the files by date and return as a dictionary: sortedDictionaryOfDatesAndFiles<DateTimeOffset, FileInfo>
            //var sortedDictionaryOfDatesAndFiles = LDSpectrogramStitching.FilterFilesForDates(csvFiles, arguments.TimeSpanOffsetHint);

            // calculate new start date if passed value = null.
            DateTimeOffset? startDate = arguments.StartDate;
            DateTimeOffset? endDate = arguments.EndDate;

            TimeSpan totalTimespan = (DateTimeOffset)endDate - (DateTimeOffset)startDate;
            int dayCount = totalTimespan.Days + 1; // assume last day has full 24 hours of recording available.

            LoggedConsole.WriteLine("\n# Start date = " + startDate.ToString());
            LoggedConsole.WriteLine("# End   date = " + endDate.ToString());
            LoggedConsole.WriteLine(string.Format("# Elapsed time = {0:f1} hours", dayCount * 24));
            LoggedConsole.WriteLine("# Day  count = " + dayCount + " (inclusive of start and end days)");
            LoggedConsole.WriteLine("# Time Zone  = " + arguments.TimeSpanOffsetHint.ToString());

            // create top level output directory if it does not exist.
            DirectoryInfo opDir = output;
            if (!opDir.Exists)
            {
                opDir.Create();
            }

            // SET UP DEFAULT SITE LOCATION INFO    --  DISCUSS IWTH ANTHONY
            // The following location data is used only to draw the sunrise/sunset tracks on images.
            double? latitude = null;
            double? longitude = null;
            var siteDescription = new SiteDescription();
            siteDescription.SiteName = arguments.FileStemName;
            siteDescription.Latitude = latitude;
            siteDescription.Longitude = longitude;

            // the following required if drawing the index images
            FileInfo indexPropertiesConfig = null;

            // require IndexGenerationData and indexPropertiesConfig for drawing
            //indexGenerationData = IndexGenerationData.GetIndexGenerationData(csvFiles[0].Directory);
            indexPropertiesConfig = arguments.IndexPropertiesConfig.ToFileInfo();
            Dictionary<string, IndexProperties> listOfIndexProperties = IndexProperties.GetIndexProperties(indexPropertiesConfig);
            Tuple<List<string>, List<double[]>> tuple = CsvTools.ReadCSVFile(csvFiles[0].FullName);
            var names = tuple.Item1;

            // default EASY indices
            int redID = 3;  // backgroundNoise
            int grnID = 5; // avSNROfActiveframes
            int bluID = 7;   // events per second
            string rep = @"bgn-avsnr-evn";

            // ACI Ht Hpeaks EASY indices
            if (false)
            {
                redID = 11;  // ACI
                grnID = 12;  // Ht

                //bluID = 13;  // HavgSp
                //bluID = 14;  // Hvariance
                //bluID = 15;  // Hpeaks
                bluID = 16;  // Hcov

                //bluID = 7;  // SPT
                rep = @"aci-ht-hcov";

                //rep = @"aci-ht-spt";
            }

            // LF, MF, HF
            if (true)
            {
                redID = 10;  // LF
                grnID = 9;   // MF
                bluID = 8;   // HF
                rep = @"lf-mf-hf";
            }

            IndexProperties redIndexProps = listOfIndexProperties[names[redID]];
            IndexProperties grnIndexProps = listOfIndexProperties[names[grnID]];
            IndexProperties bluIndexProps = listOfIndexProperties[names[bluID]];

            int dayPixelHeight = 4;
            int rowCount = (dayPixelHeight * dayCount) + 35; // +30 for grid lines
            int colCount = 1440;
            var bitmap = new Image<Rgb24>(colCount, rowCount);
            var colour = Color.Yellow;
            int currentRow = 0;
            var oneDay = TimeSpan.FromHours(24);
            int graphWidth = colCount;
            int trackHeight = 20;
            var stringFont = Drawing.Arial8;
            string[] monthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            // for drawing the y-axis scale
            int scaleWidth = trackHeight + 7;
            var yAxisScale = new Image<Rgb24>(scaleWidth, rowCount + (2 * trackHeight));
            yAxisScale.Mutate(g =>
            {
                g.Clear(Color.Black);

                // loop over days
                for (int d = 0; d < dayCount; d++)
                {
                    var thisday = ((DateTimeOffset)startDate).AddDays(d);

                    if (thisday.Day == 1)
                    {
                        int nextRow = currentRow + 1;
                        for (int c = 0; c < colCount; c++)
                        {
                            bitmap[c, currentRow] = Color.Gray;
                            bitmap[c, nextRow] = Color.Gray;
                        }

                        for (int c = 0; c < scaleWidth; c++)
                        {
                            yAxisScale[c, currentRow + trackHeight] = Color.Gray;
                            yAxisScale[c, nextRow + trackHeight] = Color.Gray;
                        }

                        string month = monthNames[thisday.Month - 1];
                        if (thisday.Month == 1) // January
                        {
                            g.DrawText(thisday.Year.ToString(), stringFont, Color.White,
                                new PointF(0, nextRow + trackHeight + 1)); //draw time
                            g.DrawText(month, stringFont, Color.White,
                                new PointF(1, nextRow + trackHeight + 11)); //draw time
                        }
                        else
                        {
                            g.DrawText(month, stringFont, Color.White,
                                new PointF(1, nextRow + trackHeight + 1)); //draw time
                        }

                        currentRow += 2;
                    }

                    // get the exact date and time
                    LoggedConsole.WriteLine($"READING DAY {d + 1} of {dayCount}:   {thisday.ToString()}");

                    // CREATE DAY LEVEL OUTPUT DIRECTORY for this day
                    string dateString = $"{thisday.Year}{thisday.Month:D2}{thisday.Day:D2}";

                    tuple = CsvTools.ReadCSVFile(csvFiles[d].FullName);
                    var arrays = tuple.Item2;

                    var redArray = arrays[redID];
                    var grnArray = arrays[grnID];
                    var bluArray = arrays[bluID];

                    // NormaliseMatrixValues the indices
                    redArray = DataTools.NormaliseInZeroOne(redArray, redIndexProps.NormMin, redIndexProps.NormMax);
                    grnArray = DataTools.NormaliseInZeroOne(grnArray, grnIndexProps.NormMin, grnIndexProps.NormMax);
                    bluArray = DataTools.NormaliseInZeroOne(bluArray, bluIndexProps.NormMin, bluIndexProps.NormMax);

                    for (int c = 0; c < colCount; c++)
                    {
                        for (int r = 0; r < dayPixelHeight; r++)
                        {
                            //transformedValue = Math.Sqrt(redArray[c]);
                            var transformedValue = redArray[c] * redArray[c];
                            int redVal = (int)Math.Round(transformedValue * 255);
                            if (redVal < 0)
                            {
                                redVal = 0;
                            }
                            else if (redVal > 255)
                            {
                                redVal = 255;
                            }

                            //transformedValue = Math.Sqrt(grnArray[c]);
                            transformedValue = grnArray[c] * grnArray[c]; // square the value
                            int grnVal = (int)Math.Round(transformedValue * 255);
                            if (grnVal < 0)
                            {
                                grnVal = 0;
                            }
                            else if (grnVal > 255)
                            {
                                grnVal = 255;
                            }

                            //transformedValue = Math.Sqrt(bluArray[c]);
                            transformedValue = bluArray[c] * bluArray[c]; // square the value
                            int bluVal = (int)Math.Round(transformedValue * 255);
                            if (bluVal < 0)
                            {
                                bluVal = 0;
                            }
                            else if (bluVal > 255)
                            {
                                bluVal = 255;
                            }

                            bitmap[c, currentRow + r] = Color.FromRgb((byte)redVal, (byte)grnVal, (byte)bluVal);
                        }
                    } // over all columns

                    currentRow += dayPixelHeight;

                    if (thisday.Day % 7 == 0)
                    {
                        for (int c = 0; c < colCount; c++)
                        {
                            bitmap[c, currentRow] = Color.Gray;
                        }

                        currentRow++;
                    }
                } // over days
            });
            // draw on civil dawn and dusk lines
            int startdayOfYear = ((DateTimeOffset)startDate).DayOfYear;
            int endDayOfYear = ((DateTimeOffset)endDate).DayOfYear;
            SunAndMoon.AddSunRiseSetLinesToImage(bitmap, arguments.BrisbaneSunriseDatafile.ToFileInfo(), startdayOfYear, endDayOfYear, dayPixelHeight);

            // add the time scales
            Image<Rgb24> timeBmp1 = ImageTrack.DrawTimeRelativeTrack(oneDay, graphWidth, trackHeight);
            var imageList = new [] { timeBmp1, bitmap, timeBmp1 };
            Image<Rgb24> compositeBmp1 = (Image<Rgb24>)ImageTools.CombineImagesVertically(imageList);

            imageList = new [] { yAxisScale, compositeBmp1 };
            Image<Rgb24> compositeBmp2 = (Image<Rgb24>)ImageTools.CombineImagesInLine(imageList);

            // indices used for image
            string indicesDescription = $"{redIndexProps.Name}|{grnIndexProps.Name}|{bluIndexProps.Name}";
            string startString = $"{startDate.Value.Year}/{startDate.Value.Month}/{startDate.Value.Day}";
            string endString = $"{endDate.Value.Year}/{endDate.Value.Month}/{endDate.Value.Day}";
            string title = $"EASY:   {arguments.FileStemName}    From {startString} to {endString}                          Indices: {indicesDescription}";
            Image<Rgb24> titleBar = ImageTrack.DrawTitleTrack(compositeBmp2.Width, trackHeight, title);
            imageList = new [] { titleBar, compositeBmp2 };
            compositeBmp2 = (Image<Rgb24>)ImageTools.CombineImagesVertically(imageList);
            var outputFileName = Path.Combine(opDir.FullName, arguments.FileStemName + "." + rep + ".EASY.png");
            compositeBmp2.Save(outputFileName);
        } // Execute()
    }
}
