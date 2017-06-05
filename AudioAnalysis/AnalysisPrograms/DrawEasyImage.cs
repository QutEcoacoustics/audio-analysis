// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DrawEasyImage.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

// <summary>
// Defines the ConcatenateIndexFiles type.
//
// Action code for this activity = "drawEasyImage"

/// Activity Codes for other tasks to do with spectrograms and audio files:
///
/// audio2csv - Calls AnalyseLongRecording.Execute(): Outputs acoustic indices and LD false-colour spectrograms.
/// audio2sonogram - Calls AnalysisPrograms.Audio2Sonogram.Main(): Produces a sonogram from an audio file - EITHER custom OR via SOX.Generates multiple spectrogram images and oscilllations info
/// indicescsv2image - Calls DrawSummaryIndexTracks.Main(): Input csv file of summary indices. Outputs a tracks image.
/// colourspectrogram - Calls DrawLongDurationSpectrograms.Execute():  Produces LD spectrograms from matrices of indices.
/// zoomingspectrograms - Calls DrawZoomingSpectrograms.Execute():  Produces LD spectrograms on different time scales.
/// differencespectrogram - Calls DifferenceSpectrogram.Execute():  Produces Long duration difference spectrograms
/// concatenateIndexFiles - Concatenates  all the index files in a 24 hour period. Used wherever partial recordings must be stitched to make whole day.
///
/// audiofilecheck - Writes information about audio files to a csv file.
/// snr - Calls SnrAnalysis.Execute():  Calculates signal to noise ratio.
/// audiocutter - Cuts audio into segments of desired length and format
/// createfoursonograms
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.IO;
    using System.Linq;

    using Acoustics.Shared;

    using Production;

    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.LongDurationSpectrograms;

    using PowerArgs;
    using AudioAnalysisTools;
    using System.Collections.Generic;
    using TowseyLibrary;
    using System.Drawing;


    /// <summary>
    /// First argument on command line to call this action is "drawEasyImage"
    /// </summary>
    public static class DrawEasyImage
    {

        public class Arguments
        {
            [ArgDescription("One or more directories where the original csv files are located.")]
            public DirectoryInfo[] InputDataDirectories { get; set; }

            [ArgDescription("Directory where the output is to go.")]
            public DirectoryInfo OutputDirectory { get; set; }

            [ArgDescription("Filter string used to search for the required csv files - assumed to be in directory path.")]
            public string FileFilter { get; set; }

            [ArgDescription("File stem name for output files.")]
            public string FileStemName { get; set; }

            [ArgDescription("The start DateTime.")]
            public DateTimeOffset? StartDate { get; set; }

            [ArgDescription("The end DateTime at which concatenation ends. If missing|null, then will be set = today's date or last available file.")]
            public DateTimeOffset? EndDate { get; set; }

            public TimeSpan? timeSpanOffsetHint = new TimeSpan(10, 0, 0);
            [ArgDescription("TimeSpan offset hint required if file names do not contain time zone info. Set default to east coast Australia")]
            public TimeSpan? TimeSpanOffsetHint {
                get { return this.timeSpanOffsetHint; }
                set { this.timeSpanOffsetHint = value; }
            }


            //[ArgDescription("User specified file containing a list of indices and their properties.")]
            //[Production.ArgExistingFile(Extension = ".yml")]
            //[ArgPosition(1)]
            internal FileInfo IndexPropertiesConfig { get; set; }

            public FileInfo BrisbaneSunriseDatafile { get; set; }

        }

        /// <summary>
        /// To get to this DEV method, the FIRST AND ONLY command line argument must be "drawEasyImage"
        /// </summary>
        public static Arguments Dev()
        {
            DateTimeOffset? dtoStart = null;
            DateTimeOffset? dtoEnd = null;

            FileInfo indexPropertiesConfig = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfigForEasyImages.yml");
            FileInfo sunrisesetData = new FileInfo(@"C:\SensorNetworks\OutputDataSets\SunRiseSet\SunriseSet2013Brisbane.csv");

            // ########################## CSV FILES CONTAINING SUMMARY INDICES IN 24 hour BLOCKS
            // top level directory
            string opFileStem = "GympieNP-2015";
            DirectoryInfo[] dataDirs = { new DirectoryInfo(@"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\GympieNP"), };

            //string opFileStem = "Woondum3-2015";
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\Woondum3"),   };


            // The filter pattern finds summary index files
            string fileFilter = "*SummaryIndices.csv";
            string opPath = @"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults";

            dtoStart = new DateTimeOffset(2015, 06, 22, 0, 0, 0, TimeSpan.Zero);
            dtoEnd   = new DateTimeOffset(2015, 10, 11, 0, 0, 0, TimeSpan.Zero);

            if(!indexPropertiesConfig.Exists) LoggedConsole.WriteErrorLine("# indexPropertiesConfig FILE DOES NOT EXIST.");

            return new Arguments
            {
                InputDataDirectories = dataDirs,
                OutputDirectory = new DirectoryInfo(opPath),
                FileFilter      = fileFilter,
                FileStemName    = opFileStem,
                StartDate       = dtoStart,
                EndDate         = dtoEnd,
                IndexPropertiesConfig = indexPropertiesConfig,
                BrisbaneSunriseDatafile = sunrisesetData,
            };
            throw new NoDeveloperMethodException();
    }

        public static void Execute(Arguments arguments)
        {
            bool verbose = false; // default

            if (arguments == null)
            {
                arguments = Dev();
                verbose = true; // assume verbose if in dev mode
            }

            if (verbose)
            {
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine("\n# DRAW an EASY IMAGE from consecutive days of SUMMARY INDICES in CSV files.");
                LoggedConsole.WriteLine("#    IT IS ASSUMED THAT THE CSV files are already concatenated into 24 hour files.");
                LoggedConsole.WriteLine(date);
                LoggedConsole.WriteLine("# Summary Index.csv files are in directories:");
                foreach (DirectoryInfo dir in arguments.InputDataDirectories)
                {
                    LoggedConsole.WriteLine("    {0}", dir.FullName);
                }
                LoggedConsole.WriteLine("# Output directory: " + arguments.OutputDirectory.FullName);
                if (arguments.StartDate == null)
                    LoggedConsole.WriteLine("# Start date = NULL (No argument provided). Will revise start date ....");
                else
                    LoggedConsole.WriteLine("# Start date = " + arguments.StartDate.ToString());

                if (arguments.EndDate == null)
                    LoggedConsole.WriteLine("# End   date = NULL (No argument provided). Will revise end date ....");
                else
                    LoggedConsole.WriteLine("# End   date = " + arguments.EndDate.ToString());

                LoggedConsole.WriteLine("# FILE FILTER = " + arguments.FileFilter);
                LoggedConsole.WriteLine();
                //LoggedConsole.WriteLine("# Index Properties Config file: " + arguments.IndexPropertiesConfig);
            }


            // PATTERN SEARCH FOR SUMMARY INDEX FILES.
            //string pattern = "*__Towsey.Acoustic.Indices.csv";
            FileInfo[] csvFiles = IndexMatrices.GetFilesInDirectories(arguments.InputDataDirectories, arguments.FileFilter);
            if (verbose)
            {
                //LoggedConsole.WriteLine("# Subdirectories Count = " + subDirectories.Length);
                LoggedConsole.WriteLine("# SummaryIndexFiles.csv Count = " + csvFiles.Length);
            }

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

            if (verbose)
            {
                LoggedConsole.WriteLine("\n# Start date = " + startDate.ToString());
                LoggedConsole.WriteLine("# End   date = " + endDate.ToString());
                LoggedConsole.WriteLine(string.Format("# Elapsed time = {0:f1} hours", (dayCount * 24)));
                LoggedConsole.WriteLine("# Day  count = " + dayCount + " (inclusive of start and end days)");
                LoggedConsole.WriteLine("# Time Zone  = " + arguments.TimeSpanOffsetHint.ToString());
            }

            // create top level output directory if it does not exist.
            DirectoryInfo opDir = arguments.OutputDirectory;
            if (!opDir.Exists) arguments.OutputDirectory.Create();

            // SET UP DEFAULT SITE LOCATION INFO    --  DISCUSS IWTH ANTHONY
            // The following location data is used only to draw the sunrise/sunset tracks on images.
            double? latitude = null;
            double? longitude = null;
            var siteDescription = new SiteDescription();
            siteDescription.SiteName = arguments.FileStemName;
            siteDescription.Latitude = latitude;
            siteDescription.Longitude = longitude;

            // the following required if drawing the index images
            IndexGenerationData indexGenerationData = null;
            FileInfo indexPropertiesConfig = null;

            // require IndexGenerationData and indexPropertiesConfig for drawing
            //indexGenerationData = IndexGenerationData.GetIndexGenerationData(csvFiles[0].Directory);
            indexPropertiesConfig = arguments.IndexPropertiesConfig;
            Dictionary<string, IndexProperties> listOfIndexProperties = IndexProperties.GetIndexProperties(indexPropertiesConfig);
            Tuple<List<string>, List<double[]>> tuple = CsvTools.ReadCSVFile(csvFiles[0].FullName);
            var names = tuple.Item1;

            // default EASY indices
            int redID = 3;  // backgroundNoise
            //int grnID = 4;  //SNR
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
            var bitmap = new Bitmap(colCount, rowCount);
            var colour = Color.Yellow;
            int currentRow = 0;
            var oneDay = TimeSpan.FromHours(24);
            int graphWidth = colCount;
            int trackHeight = 20;
            Pen whitePen = new Pen(Color.White);
            //Pen grayPen = new Pen(Color.Gray);
            Font stringFont = new Font("Arial", 8);
            string[] monthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"};


            // for drawing the y-axis scale
            int scaleWidth = trackHeight + 7;
            var yAxisScale = new Bitmap(scaleWidth, rowCount + (2 * trackHeight));
            Graphics g = Graphics.FromImage(yAxisScale);
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
                        bitmap.SetPixel(c, currentRow, Color.Gray);
                        bitmap.SetPixel(c, nextRow,    Color.Gray);
                    }
                    for (int c = 0; c < scaleWidth; c++)
                    {
                        yAxisScale.SetPixel(c, currentRow + trackHeight, Color.Gray);
                        yAxisScale.SetPixel(c, nextRow + trackHeight, Color.Gray);
                    }
                    string month = monthNames[thisday.Month-1];
                    if (thisday.Month == 1) // January
                    {
                        g.DrawString(thisday.Year.ToString(), stringFont, Brushes.White, new PointF(0, nextRow + trackHeight + 1)); //draw time
                        g.DrawString(month, stringFont, Brushes.White, new PointF(1, nextRow + trackHeight + 11)); //draw time
                    }
                    else {
                        g.DrawString(month, stringFont, Brushes.White, new PointF(1, nextRow + trackHeight + 1)); //draw time
                    }

                    currentRow += 2;
                }

                // get the exact date and time
                LoggedConsole.WriteLine(string.Format("READING DAY {0} of {1}:   {2}", (d+1), dayCount, thisday.ToString()));

                // CREATE DAY LEVEL OUTPUT DIRECTORY for this day
                string dateString = string.Format("{0}{1:D2}{2:D2}", thisday.Year, thisday.Month, thisday.Day);

                string opFileStem = string.Format("{0}_{1}", arguments.FileStemName, dateString);
                //var indicesFile = FilenameHelpers.AnalysisResultPath(resultsDir, opFileStem, LDSpectrogramStitching.SummaryIndicesStr, LDSpectrogramStitching.CsvFileExt);

                tuple = CsvTools.ReadCSVFile(csvFiles[d].FullName);
                var arrays = tuple.Item2;

                var redArray = arrays[redID];
                var grnArray = arrays[grnID];
                var bluArray = arrays[bluID];

                // NormaliseMatrixValues the indices
                redArray = DataTools.NormaliseInZeroOne(redArray, redIndexProps.NormMin, redIndexProps.NormMax);
                grnArray = DataTools.NormaliseInZeroOne(grnArray, grnIndexProps.NormMin, grnIndexProps.NormMax);
                bluArray = DataTools.NormaliseInZeroOne(bluArray, bluIndexProps.NormMin, bluIndexProps.NormMax);
                double transformedValue;

                for (int c = 0; c < colCount; c++)
                {
                    for (int r = 0; r < dayPixelHeight; r++)
                    {
                        //transformedValue = Math.Sqrt(redArray[c]);
                        transformedValue = redArray[c] * redArray[c]; // square the value
                        int redVal = (int)Math.Round(transformedValue * 255);
                        if (redVal < 0) redVal = 0;
                        else
                        if (redVal > 255) redVal = 255;

                        //transformedValue = Math.Sqrt(grnArray[c]);
                        transformedValue = grnArray[c] * grnArray[c]; // square the value
                        int grnVal = (int)Math.Round(transformedValue * 255);
                        if (grnVal < 0) grnVal = 0;
                        else
                        if (grnVal > 255) grnVal = 255;

                        //transformedValue = Math.Sqrt(bluArray[c]);
                        transformedValue = bluArray[c] * bluArray[c]; // square the value
                        int bluVal = (int)Math.Round(transformedValue * 255);
                        if (bluVal < 0) bluVal = 0;
                        else
                        if (bluVal > 255) bluVal = 255;
                        bitmap.SetPixel(c, (currentRow + r), Color.FromArgb(redVal, grnVal, bluVal));

                    }
                } // over all columns

                currentRow += dayPixelHeight;

                if (thisday.Day % 7 == 0)
                {
                    for (int c = 0; c < colCount; c++)
                    {
                        bitmap.SetPixel(c, currentRow, Color.Gray);
                    }
                    currentRow++;
                }

            } // over days


            // draw on civil dawn and dusk lines
            int startdayOfYear = ((DateTimeOffset)startDate).DayOfYear;
            int endDayOfYear   = ((DateTimeOffset)endDate).DayOfYear;
            SunAndMoon.AddSunRiseSetLinesToImage((Bitmap)bitmap, arguments.BrisbaneSunriseDatafile, startdayOfYear, endDayOfYear, dayPixelHeight);

            // add the time scales
            Bitmap timeBmp1 = Image_Track.DrawTimeRelativeTrack(oneDay, graphWidth, trackHeight);
            var imageList = new List<Image>();
            imageList.Add(timeBmp1);
            imageList.Add(bitmap);
            imageList.Add(timeBmp1);
            Bitmap compositeBmp1 = (Bitmap)ImageTools.CombineImagesVertically(imageList);

            imageList = new List<Image>();
            imageList.Add(yAxisScale);
            imageList.Add(compositeBmp1);
            Bitmap compositeBmp2 = (Bitmap)ImageTools.CombineImagesInLine(imageList);

            // indices used for image
            string indicesDescription = $"{redIndexProps.Name}|{grnIndexProps.Name}|{bluIndexProps.Name}";
            string startString = $"{startDate.Value.Year}/{startDate.Value.Month}/{startDate.Value.Day}";
            string   endString = $"{endDate.Value.Year}/{endDate.Value.Month}/{endDate.Value.Day}";
            string title = $"EASY:   {arguments.FileStemName}    From {startString} to {endString}                          Indices: {indicesDescription}";
            Bitmap titleBar = Image_Track.DrawTitleTrack(compositeBmp2.Width, trackHeight, title);
            imageList = new List<Image>();
            imageList.Add(titleBar);
            imageList.Add(compositeBmp2);
            compositeBmp2 = (Bitmap)ImageTools.CombineImagesVertically(imageList);
            var outputFileName = Path.Combine(opDir.FullName, arguments.FileStemName + "." + rep + ".EASY.png");
            compositeBmp2.Save(outputFileName);



        } // Execute()
    }
}
