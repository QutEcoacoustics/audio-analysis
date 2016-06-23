// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DrawLongDurationSpectrograms.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>

// <summary>
// Defines the ConcatenateIndexFiles type.
//
// Action code for this activity = "concatenateIndexFiles"

/// Activity Codes for other tasks to do with spectrograms and audio files:
/// 
/// audio2csv - Calls AnalyseLongRecording.Execute(): Outputs acoustic indices and LD false-colour spectrograms.
/// audio2sonogram - Calls AnalysisPrograms.Audio2Sonogram.Main(): Produces a sonogram from an audio file - EITHER custom OR via SOX.Generates multiple spectrogram images and oscilllations info
/// indicescsv2image - Calls DrawSummaryIndexTracks.Main(): Input csv file of summary indices. Outputs a tracks image.
/// colourspectrogram - Calls DrawLongDurationSpectrograms.Execute():  Produces LD spectrograms from matrices of indices.
/// zoomingspectrograms - Calls DrawZoomingSpectrograms.Execute():  Produces LD spectrograms on different time scales.
/// differencespectrogram - Calls DifferenceSpectrogram.Execute():  Produces Long duration difference spectrograms
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

    using AnalysisPrograms.Production;

    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.LongDurationSpectrograms;

    using PowerArgs;
    using AudioAnalysisTools;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Reflection;

    using log4net;

    using TowseyLibrary;


    /// <summary>
    /// First argument on command line to call this action is "concatenateIndexFiles"
    /// </summary>
    public static class ConcatenateIndexFiles
    {

        public class Arguments
        {
            [ArgDescription("One or more directories where the original csv files are located.")]
            public DirectoryInfo[] InputDataDirectories { get; set; }

            [ArgDescription("Directory where the output is to go.")]
            public DirectoryInfo OutputDirectory { get; set; }

            [ArgDescription("Filter string used to search for the required csv files - assumed to be in directory path.")]
            public string DirectoryFilter { get; set; }

            [ArgDescription("File stem name for output files.")]
            public string FileStemName { get; set; }

            [ArgDescription("DateTimeOffset at which concatenation begins. If null, then start with earliest available file. Can parse an ISO8601 date.")]
            public DateTimeOffset? StartDate { get; set; }

            [ArgDescription("DateTimeOffset at which concatenation ends. If null, then will be set = today's date or last available file. Can parse an ISO8601 date.")]
            public DateTimeOffset? EndDate { get; set; }

            private TimeSpan? timeSpanOffsetHint = null;

            [ArgDescription("TimeSpan offset hint required if file names do not contain time zone info. NO DEFAULT IS SET")]
            public TimeSpan? TimeSpanOffsetHint {
                get { return timeSpanOffsetHint; }
                set { timeSpanOffsetHint = value; }
            }


            //[ArgDescription("Draw images of summary and spectral indices after concatenating them")]
            internal bool DrawImages { get; set; }

            [ArgDescription("User specified file containing a list of indices and their properties.")]
            [Production.ArgExistingFile(Extension = ".yml")]
            [ArgPosition(1)]
            public FileInfo IndexPropertiesConfig { get; set; }

            private bool concatenateEverythingYouCanLayYourHandsOn = false;
            [ArgDescription("Set this true when want to concatenate longer than 24-hour recordings as in case of PNG data.")]
            public bool ConcatenateEverythingYouCanLayYourHandsOn {
                get { return concatenateEverythingYouCanLayYourHandsOn; }
                set { concatenateEverythingYouCanLayYourHandsOn = value; }
            }

            internal bool Verbose { get; set; }

        }

        /// <summary>
        /// To get to this DEV method, the FIRST AND ONLY command line argument must be "concatenateIndexFiles"
        /// </summary>
        public static Arguments Dev()
        {
            FileInfo indexPropertiesConfig = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml");

            DateTimeOffset? dtoStart = null;
            DateTimeOffset? dtoEnd = null;

            // ########################## MARINE RECORDINGS          
            // top level directory
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"Y:\Results\2015Dec14-094058 - Michael, Towsey.Indices, ICD=30.0, #70\towsey\MarineRecordings\Cornell\2013March-April"),
            //                           };
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"C:\SensorNetworks\WavFiles\MarineRecordings\Cornell\2013March-April"),
            //                           };
            //string directoryFilter = "201303";
            //string opPath = @"C:\SensorNetworks\Output\MarineSonograms\LdFcSpectrograms2013March";
            ////string opPath = @"C:\SensorNetworks\Output\MarineSonograms\LdFcSpectrograms2013April";
            //dtoStart = new DateTimeOffset(2013, 03, 01, 0, 0, 0, TimeSpan.Zero);
            //dtoEnd   = new DateTimeOffset(2013, 03, 31, 0, 0, 0, TimeSpan.Zero);
            //string opFileStem = "CornellMarine";
            //indexPropertiesConfig = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesMarineConfig.yml");


            // ########################## YVONNE'S RECORDINGS          
            // top level directory
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"Y:\Results\2015Aug06-123245 - Yvonne, Indices, ICD=60.0, #48"),
            //                             new DirectoryInfo(@"Y:\Results\2015Aug20-154235 - Yvonne, Indices, ICD=60.0, #50")
            //                           };

            DirectoryInfo[] dataDirs = { new DirectoryInfo(@"G:\SensorNetworks\Output\YvonneResults\DataFiles_62_93\2015Nov1"),
                                       };

            //below directory was to check a bug - missing 6 hours of recording
            //DirectoryInfo[] dataDirs = {
            //    new DirectoryInfo(@"Y:\Results\2015Aug06-123245 - Yvonne, Indices, ICD=60.0, #48\Yvonne\Cooloola"),
            //                           };
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"Y:\Results\2015Aug06-123245 - Yvonne, Indices, ICD=60.0, #48\Yvonne\Cooloola\2015July26\Woondum3"),
            //                           };
            //string directoryFilter = "20150725-000000+1000.wav";

            //The recording siteName is used as filter pattern to select directories. It is also used for naming the output files
            string directoryFilter = "Woondum3";
            //string directoryFilter = "GympieNP";   // this is a directory filter to locate only the required files

            string opPath = @"G:\SensorNetworks\Output\YvonneResults\ConcatenatedFiles_62_93";

            dtoStart = new DateTimeOffset(2015, 10, 26, 0, 0, 0, TimeSpan.Zero);
            dtoEnd   = new DateTimeOffset(2015, 10, 28, 0, 0, 0, TimeSpan.Zero);
            string opFileStem = directoryFilter;


            /*
            // ########################## LENN'S RECORDINGS          
            // top level directory
            DirectoryInfo[] dataDirs = { new DirectoryInfo(@"Y:\Results\2015Oct19-173501 - Lenn, Indices, ICD=60.0, #61\Berndt\Lenn\Week 1\Card1302_Box1302"),
                                       };


            // The recording siteName is used as filter pattern to select directories. It is also used for naming the output files
            string directoryFilter = "Towsey.Acoustic"; // this is a directory filter to locate only the required files
            string opFileStem = "Card1302_Box1302";
            string opPath = @"C:\SensorNetworks\Output\LennsResults";

            dtoStart = new DateTimeOffset(2015, 09, 27, 0, 0, 0, TimeSpan.Zero);
            dtoEnd = new DateTimeOffset(2015, 09, 30, 0, 0, 0, TimeSpan.Zero);
            //dtoEnd   = new DateTimeOffset(2015, 10, 11, 0, 0, 0, TimeSpan.Zero);

    */

            // ########################## STURT RECORDINGS
            // The recording siteName is used as filter pattern to select directories. It is also used for naming the output files

            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"F:\SensorNetworks\WavFiles\SturtRecordings\Thompson"), };
            //string directoryFilter = "Thompson";   // this is a directory filter to locate only the required files
            //string opFileStem = "Sturt-Thompson";
            //string opPath = @"F:\SensorNetworks\WavFiles\SturtRecordings\";

            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"F:\SensorNetworks\WavFiles\SturtRecordings\Stud"), };
            //string directoryFilter = "Stud";   // this is a directory filter to locate only the required files
            //string opFileStem = "Sturt-Stud";
            //string opPath = @"F:\SensorNetworks\WavFiles\SturtRecordings\";

            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"F:\SensorNetworks\WavFiles\SturtRecordings\Sturt1"), };
            //string directoryFilter = "Sturt1";   // this is a directory filter to locate only the required files
            //string opFileStem      = "Sturt-Sturt1";
            //string opPath = @"F:\SensorNetworks\WavFiles\SturtRecordings\";

            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"Y:\Results\2015Jul29-110950 - Jason, Towsey.Indices, ICD=60.0, #43\Sturt\2015July\Mistletoe"), };
            //string directoryFilter = "STURT2";          // this is a directory filter to locate only the required files
            //string opFileStem = "Sturt-Mistletoe";
            //string opPath = @"F:\SensorNetworks\Output\Sturt\";

            //dtoStart = new DateTimeOffset(2015, 07, 01, 0, 0, 0, TimeSpan.Zero);
            //dtoEnd = new DateTimeOffset(2015, 07, 06, 0, 0, 0, TimeSpan.Zero);




            // ########################## EDDIE GAME'S RECORDINGS
            // top level directory
            //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_4-7-15\BAR\BAR_32\";
            //string opFileStem = "TNC_Iwarame_20150704_BAR32";

            //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_4-7-15\BAR\BAR_33\";
            //string opFileStem = "TNC_Iwarame_20150704_BAR33";

            //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_4-7-15\BAR\BAR_35\";
            //string opFileStem = "TNC_Iwarame_20150704_BAR35";

            //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_7-7-15\BAR\BAR_59\";
            //string opFileStem = "TNC_Iwarame_20150707_BAR59";

            //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_9-7-15\BAR\BAR_79\";
            //string opFileStem = "TNC_Iwarame_20150709_BAR79";

            //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Yavera_8-7-15\BAR\BAR_64\";
            //string opFileStem = "TNC_Yavera_20150708_BAR64";

            //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Musiamunat_3-7-15\BAR\BAR_18\";
            //string opPath   = dataPath;
            //string directoryFilter = "Musimunat";  // this is a directory filter to locate only the required files
            //string opFileStem = "Musimunat_BAR18"; // this should be a unique site identifier
            //string opFileStem = "TNC_Musimunat_20150703_BAR18";
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(dataPath) };

            // the default set of index properties is located in the AnalysisConfig directory.
            //IndexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml".ToFileInfo();
            // However the PNG data uses an older set of index properties prior to fixing a bug!
            //FileInfo indexPropertiesConfig = new FileInfo(@"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\IndexPropertiesOLDConfig.yml");

            // ########################## END of EDDIE GAME'S RECORDINGS



            bool drawImages = true;
            if(!indexPropertiesConfig.Exists) LoggedConsole.WriteErrorLine("# indexPropertiesConfig FILE DOES NOT EXIST.");

            // DISCUSS THE FOLLOWING WITH ANTHONY
            // Anthony says we would need to serialise the class. Skip this for the moment.
            // The following location data is used only to draw the sunrise/sunset tracks on images.
            //double? latitude = null;
            //double? longitude = null;
            //var siteDescription = new SiteDescription();
            //siteDescription.SiteName = siteName;
            //siteDescription.Latitude = latitude;
            //siteDescription.Longitude = longitude;

            return new Arguments
            {
                InputDataDirectories = dataDirs,
                OutputDirectory = new DirectoryInfo(opPath),
                DirectoryFilter = directoryFilter,
                FileStemName = opFileStem,
                StartDate = dtoStart,
                EndDate = dtoEnd,
                DrawImages = drawImages,
                IndexPropertiesConfig = indexPropertiesConfig,
                ConcatenateEverythingYouCanLayYourHandsOn = false,
                TimeSpanOffsetHint = TimeSpan.FromHours(10),
                Verbose = true,
            };
            throw new NoDeveloperMethodException();
    }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Execute(Arguments arguments)
        {
            bool verbose = false; // default

            if (arguments == null)
            {
                arguments = Dev();
                verbose = true; // default is verbose if in dev mode
            }

            Log.Warn("DrawImages option hard coded to be on in this version");
            arguments.DrawImages = true;

            verbose = arguments.Verbose;
            IndexMatrices.Verbose = verbose;

            if (verbose)
            {
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine("# CONCATENATES CSV FILES CONTAINING SPECTRAL and SUMMARY INDICES.");
                LoggedConsole.WriteLine("#    IT IS ASSUMED THESE WERE OBTAINED FROM MULTIPLE SHORT AUDIO RECORDINGs");
                LoggedConsole.WriteLine(date);
                LoggedConsole.WriteLine("# Index.csv files are in directories:");
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

                LoggedConsole.WriteLine("# DIRECTORY FILTER = " + arguments.DirectoryFilter);
                LoggedConsole.WriteLine();
                //LoggedConsole.WriteLine("# Index Properties Config file: " + arguments.IndexPropertiesConfig);
            }


            // 1. PATTERN SEARCH FOR CORRECT SUBDIRECTORIES
            // Assumes that the required subdirectories have the given FILTER/SiteName somewhere in their path. 
            var subDirectories = LDSpectrogramStitching.GetSubDirectoriesForSiteData(arguments.InputDataDirectories, arguments.DirectoryFilter);
            if (subDirectories.Length == 0)
            {
                LoggedConsole.WriteErrorLine("\n\n#WARNING from method ConcatenateIndexFiles.Execute():");
                LoggedConsole.WriteErrorLine("        Subdirectory Count with given filter = ZERO");
                LoggedConsole.WriteErrorLine("        RETURNING EMPTY HANDED!");
                return;
            }

            // 2. PATTERN SEARCH FOR SUMMARY INDEX FILES.
            string pattern = "*__Towsey.Acoustic.Indices.csv";
            FileInfo[] csvFiles = IndexMatrices.GetFilesInDirectories(subDirectories, pattern);
            if (verbose)
            {
                LoggedConsole.WriteLine("# Subdirectories Count = " + subDirectories.Length);
                LoggedConsole.WriteLine("# IndexFiles.csv Count = " + csvFiles.Length);
            }

            if (csvFiles.Length == 0)
            {
                LoggedConsole.WriteErrorLine("\n\nWARNING from method ConcatenateIndexFiles.Execute():");
                LoggedConsole.WriteErrorLine("        No SUMMARY index files were found.");
                LoggedConsole.WriteErrorLine("        RETURNING EMPTY HANDED!");
                return;
            }

            // Sort the files by date and return as a dictionary: sortedDictionaryOfDatesAndFiles<DateTimeOffset, FileInfo> 
            var sortedDictionaryOfDatesAndFiles = FileDateHelpers.FilterFilesForDates(csvFiles, arguments.TimeSpanOffsetHint);


            // calculate new start date if passed value = null.
            DateTimeOffset? startDate = arguments.StartDate;
            DateTimeOffset? endDate = arguments.EndDate;
            if (startDate == null)
            {
                startDate = sortedDictionaryOfDatesAndFiles.Keys.First();
            }
            // calculate new end date if passed value = null.
            if (endDate == null)
            {
                endDate = sortedDictionaryOfDatesAndFiles.Keys.Last();
                //endDate = DateTimeOffset.UtcNow;
            }

            TimeSpan totalTimespan = (DateTimeOffset)endDate - (DateTimeOffset)startDate;
            int dayCount = totalTimespan.Days + 1; // assume last day has full 24 hours of recording available.

            if (verbose)
            {
                LoggedConsole.WriteLine("\n# Start date = " + startDate.ToString());
                LoggedConsole.WriteLine("# End   date = " + endDate.ToString());
                LoggedConsole.WriteLine(String.Format("# Elapsed time = {0:f1} hours", (dayCount * 24)));
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
            if (arguments.DrawImages)
            {
                // get the IndexGenerationData file from the first directory
                indexGenerationData = IndexGenerationData.GetIndexGenerationData(csvFiles[0].Directory);
                indexPropertiesConfig = arguments.IndexPropertiesConfig;
            }


            //TODO TODO TODO   ########################   NEED TO DEBUG THE FOLLOWING OPTION - use case of EDDIE GAME
            if (arguments.ConcatenateEverythingYouCanLayYourHandsOn)
            {
                LoggedConsole.WriteErrorLine("\n\nWARNING from method ConcatenateIndexFiles.Execute():");
                LoggedConsole.WriteErrorLine("       TODO TODO TODO   ########################   NEED TO DEBUG THE FOLLOWING OPTION");
                LoggedConsole.WriteErrorLine("       The option <ConcatenateEverythingYouCanLayYourHandsOn> has not been debugged.");
                LoggedConsole.WriteErrorLine("       Check results for accuracy.");
                LoggedConsole.WriteErrorLine("       This option is only currently used for the TNC data of Eddie Game.");
                // concatenate the summary index files
                FileInfo[] files = sortedDictionaryOfDatesAndFiles.Values.ToArray<FileInfo>();
                LDSpectrogramStitching.ConcatenateSpectralIndexFiles(subDirectories[0], indexPropertiesConfig, opDir, arguments.FileStemName);
                LDSpectrogramStitching.ConcatenateSummaryIndexFiles(subDirectories[0], indexPropertiesConfig, opDir, arguments.FileStemName);
                return;
            }



            // loop over days
            for (int d = 0; d < dayCount; d++)
            {
                var thisday = ((DateTimeOffset)startDate).AddDays(d);

                var filteredDict = LDSpectrogramStitching.GetFilesForOneDay(sortedDictionaryOfDatesAndFiles, thisday);
                if (filteredDict.Count == 0)
                {
                    LoggedConsole.WriteErrorLine("\n\nWARNING from method ConcatenateIndexFiles.Execute():");
                    LoggedConsole.WriteErrorLine("        No files of SUMMARY indices were found.");
                    LoggedConsole.WriteErrorLine("        Break cycle through days!!! ");
                    break;
                }

                // get the exact date and time
                thisday = filteredDict.Keys.First();
                LoggedConsole.WriteLine(String.Format("\n\n\nCONCATENATING DAY {0} of {1}:   {2}", (d+1), dayCount, thisday.ToString()));

                // CREATE DAY LEVEL OUTPUT DIRECTORY for this day
                string dateString = String.Format("{0}{1:D2}{2:D2}", thisday.Year, thisday.Month, thisday.Day);
                DirectoryInfo resultsDir = new DirectoryInfo(Path.Combine(opDir.FullName, arguments.FileStemName, dateString));
                if (!resultsDir.Exists) resultsDir.Create();

                string opFileStem = String.Format("{0}_{1}", arguments.FileStemName, dateString);
                var indicesFile = FilenameHelpers.AnalysisResultName(resultsDir, opFileStem, LDSpectrogramStitching.SummaryIndicesStr, LDSpectrogramStitching.CsvFileExt);
                var indicesCsvfile = new FileInfo(indicesFile);

                // CONCATENATE the SUMMARY INDEX FILES
                FileInfo[] files = filteredDict.Values.ToArray<FileInfo>();
                var summaryDict = LDSpectrogramStitching.ConcatenateSummaryIndexFiles(files, resultsDir, indicesCsvfile, indexGenerationData.IndexCalculationDuration);

                if (summaryDict.Count == 0)
                {
                    LoggedConsole.WriteErrorLine("\n\nWARNING from method ConcatenateIndexFiles.Execute():");
                    LoggedConsole.WriteErrorLine("        An empty dictionary of SUMMARY indices was returned !!! ");
                    break;
                }

                // REALITY CHECK - check for zero signal and anything else that might indicate defective signal
                List<ErroneousIndexSegments> indexErrors = ErroneousIndexSegments.DataIntegrityCheck(summaryDict, resultsDir, arguments.FileStemName);


                // DRAW SUMMARY INDEX IMAGES AND SAVE IN RESULTS DIRECTORY
                if (arguments.DrawImages)
                {
                    indexGenerationData.RecordingStartDate = thisday;
                    LDSpectrogramStitching.DrawSummaryIndexFiles(summaryDict, 
                                                                 indexGenerationData, 
                                                                 indexPropertiesConfig, 
                                                                 resultsDir, 
                                                                 siteDescription,
                                                                 indexErrors,
                                                                 verbose);
                }

                // ##############################################################################################################

                // NOW CONCATENATE SPECTRAL INDEX FILES
                string colorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN;
                string colorMap2 = SpectrogramConstants.RGBMap_BGN_POW_SPT;
                string[] keys = LdSpectrogramConfig.GetKeys(colorMap1, colorMap2);
                var spectralDict = LDSpectrogramStitching.ConcatenateSpectralIndexFilesForOneDay(subDirectories, resultsDir, arguments.FileStemName, thisday, 
                                                                         indexGenerationData.IndexCalculationDuration, keys, verbose);
                if (spectralDict.Count == 0)
                {
                    LoggedConsole.WriteErrorLine("WARNING from method ConcatenateIndexFiles.Execute():");
                    LoggedConsole.WriteErrorLine("        An empty dictionary of SPECTRAL indices was returned !!! ");
                    return;
                }

                // DRAW SPECTRAL INDEX IMAGES AND SAVE IN RESULTS DIRECTORY
                if (arguments.DrawImages)
                {
                    var sgConfig = LdSpectrogramConfig.GetDefaultConfig(colorMap1, colorMap2);
                    LDSpectrogramStitching.DrawSpectralIndexFiles(spectralDict,
                                                                  sgConfig,
                                                                  indexGenerationData, 
                                                                  indexPropertiesConfig, 
                                                                  resultsDir, 
                                                                  siteDescription,
                                                                  indexErrors,
                                                                  verbose);
                }

            } // over days

        } // Execute()



        public static void ConcatenateRibbonImages(DirectoryInfo[] dataDirs, string pattern, DirectoryInfo outputDirectory, 
                                                   string opFileStem, string title)
        {
            //get the ribon files
            FileInfo[] imageFiles = IndexMatrices.GetFilesInDirectories(dataDirs, pattern);

            var image = new Bitmap(imageFiles[0].FullName);
            int imageHt = image.Height;
            int imageCount = imageFiles.Length;
            var spacer = new Bitmap(image.Width, 1);
            Graphics canvas = Graphics.FromImage(spacer);
            canvas.Clear(Color.Gray);

            // add ribbon files to list
            var imageList = new List<Image>();
            foreach (FileInfo imageFile in imageFiles)
            {
                image = new Bitmap(imageFile.FullName);
                imageList.Add(image);
                imageList.Add(spacer);
            }

            //create composite image
            Bitmap compositeBmp = (Bitmap)ImageTools.CombineImagesVertically(imageList);

            // create left side day scale    
            Font stringFont = new Font("Arial", 16);
            imageList = new List<Image>();
            for(int i = 0; i < imageCount; i++)
            {
                image = new Bitmap(60, imageHt);
                canvas = Graphics.FromImage(image);
                string str = String.Format("{0}", i+1);
                canvas.DrawString(str, stringFont, Brushes.White, new PointF(3, 3));

                imageList.Add(image);
                imageList.Add(spacer);
            }

            //create composite image
            Bitmap compositeBmpYscale = (Bitmap)ImageTools.CombineImagesVertically(imageList);
            Bitmap[] finalImages = { compositeBmpYscale, compositeBmp, compositeBmpYscale };
            Bitmap finalComposite = (Bitmap)ImageTools.CombineImagesInLine(finalImages);

            // add title bar
            var titleBmp = new Bitmap(finalComposite.Width, 30);
            canvas = Graphics.FromImage(titleBmp);
            canvas.DrawString(title, stringFont, Brushes.White, new PointF(30, 3));

            // add title plus spacer
            spacer = new Bitmap(finalComposite.Width, 3);
            canvas = Graphics.FromImage(spacer);
            canvas.Clear(Color.Gray);
            Bitmap[] titledImages = { titleBmp, spacer, finalComposite };
            finalComposite = (Bitmap)ImageTools.CombineImagesVertically(titledImages);

            finalComposite.Save(Path.Combine(outputDirectory.FullName, opFileStem + ".png"));
            Console.WriteLine(string.Format("Final compositeBmp dimensions are width {0} by height {1}", compositeBmp.Width, compositeBmp.Height));
            Console.WriteLine(string.Format("Final number of ribbons/days = {0}", imageFiles.Length));

        } //ConcatenateRibbonImages


    }
}
