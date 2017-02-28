// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DrawLongDurationSpectrograms.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
// Defines the ConcatenateIndexFiles type.
//
// Action code for this activity = "concatenateIndexFiles"
// Activity Codes for other tasks to do with spectrograms and audio files:
// 
// audio2csv - Calls AnalyseLongRecording.Execute(): Outputs acoustic indices and LD false-colour spectrograms.
// audio2sonogram - Calls AnalysisPrograms.Audio2Sonogram.Main(): Produces a sonogram from an audio file - EITHER custom OR via SOX.Generates multiple spectrogram images and oscilllations info
// indicescsv2image - Calls DrawSummaryIndexTracks.Main(): Input csv file of summary indices. Outputs a tracks image.
// colourspectrogram - Calls DrawLongDurationSpectrograms.Execute():  Produces LD spectrograms from matrices of indices.
// zoomingspectrograms - Calls DrawZoomingSpectrograms.Execute():  Produces LD spectrograms on different time scales.
// differencespectrogram - Calls DifferenceSpectrogram.Execute():  Produces Long duration difference spectrograms
//
// audiofilecheck - Writes information about audio files to a csv file.
// snr - Calls SnrAnalysis.Execute():  Calculates signal to noise ratio.
// audiocutter - Cuts audio into segments of desired length and format
// createfoursonograms 
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Web.UI;
using Acoustics.Shared.Csv;

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
    /// 
    /// NOTE: This code was last tested on 2016 October 10. Both tests passed.
    /// </summary>
    public static class ConcatenateIndexFiles
    {

        public class Arguments
        {
            [ArgDescription("One or more directories where the original csv files are located.")]
            public DirectoryInfo[] InputDataDirectories { get; set; }

            [ArgDescription("One directory where the original csv files are located. This option exists as well as a hack to get around commas in paths conflicting with PowerArgs' array parsing feature.")]
            public DirectoryInfo InputDataDirectory { get; set; }

            [ArgDescription("Directory where the output is to go.")]
            public DirectoryInfo OutputDirectory { get; set; }

            [ArgDescription("Used to get the required data.csv files, which are assumed to be in a matching dir or subdirectory. E.g. use name of audio file suffix e.g.: *.wav")]
            public string DirectoryFilter { get; set; }

            [ArgDescription("File stem name for output files.")]
            public string FileStemName { get; set; }

            [ArgDescription("DateTimeOffset at which concatenation begins. If null, then start with earliest available file. Can parse an ISO8601 date.")]
            public DateTimeOffset? StartDate { get; set; }

            [ArgDescription("DateTimeOffset at which concatenation ends. If null, then will be set = today's date or last available file. Can parse an ISO8601 date.")]
            public DateTimeOffset? EndDate { get; set; }

            [ArgDescription("TimeSpan offset hint required if file names do not contain time zone info. NO DEFAULT IS SET")]
            public TimeSpan? TimeSpanOffsetHint { get; set; }

            [ArgDescription("Draw false-colour spectrograms after concatenating index files")]
            internal bool DrawImages { get; set; } = true;

            [ArgDescription("User specified file containing a list of indices and their properties.")]
            [Production.ArgExistingFile(Extension = ".yml")]
            public FileInfo IndexPropertiesConfig { get; set; }

            [ArgDescription("Config file for drawing the false colour spectrograms.")]
            [Production.ArgExistingFile(Extension = ".yml")]
            public FileInfo FalseColourSpectrogramConfig { get; set; }

            [ArgDescription("User specified file containing times of sunrise & sunset for recording location. Must be correct format!")]
            [Production.ArgExistingFile(Extension = ".csv")]
            public FileInfo SunRiseDataFile { get; set; }

            [ArgDescription("Set true only when concatenating more than 24-hours of data into one image - e.g. PNG/Indonesian data.")]
            public bool ConcatenateEverythingYouCanLayYourHandsOn { get; set; }

            [ArgDescription("One or more directories where the RECOGNIZER event scores are located in csv files. This is optional")]
            public DirectoryInfo[] EventDataDirectories { get; set; }

            [ArgDescription("Used only to get Event Recognizer files.")]
            public string EventFilePattern { get; set; }
            
            [ArgDescription("Default = false. For use by software manager only.")]
            internal bool DoTest { get; set; }
            [ArgDescription("Directory containing benchmark TEST files. For use by software manager only.")]
            public DirectoryInfo TestDirectory { get; set; }

            internal bool Verbose { get; set; }

        }

        /// <summary>
        /// To get to this DEV method, the FIRST AND ONLY command line argument must be "concatenateIndexFiles"
        /// </summary>
        public static Arguments Dev()
        {
            // set the default values here
            var indexPropertiesConfig = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml");
            var timeSpanOffsetHint = TimeSpan.FromHours(10); // default = Brisbane time
            bool drawImages = true;
            bool doTest = true;

            DateTimeOffset? dtoStart = null;
            DateTimeOffset? dtoEnd = null;
            DirectoryInfo[] eventDirs = null;

            // The drive: local = C; work = G; home = E
            string drive = "C";

            /*
                        // ########################## TESTING OF CONCATENATION 
                        // Test data derived from ZuZZana's INDONESIAN RECORDINGS, recording site 2. Obtained July 2016. THis teste set up October 2016.
                        // The drive: work = G; home = E
                        // top level directory
                        DirectoryInfo[] dataDirs = { new DirectoryInfo($"{drive}:\\SensorNetworks\\SoftwareTests\\Test_Concatenation\\Data\\Indonesia_2\\"),
                                                   };
                        string directoryFilter = "*.wav";  // this is a directory filter to locate only the required files
                        string testPath = $"{drive}:\\SensorNetworks\\SoftwareTests\\Test_Concatenation\\ExpectedOutput\\";
                        indexPropertiesConfig = new FileInfo($"{drive}:\\SensorNetworks\\SoftwareTests\\Test_Concatenation\\Data\\Concat_TEST_IndexPropertiesConfig.yml");
                        var falseColourSpgConfig = new FileInfo($"{drive}:\\SensorNetworks\\SoftwareTests\\Test_Concatenation\\Data\\TEST_SpectrogramFalseColourConfig.yml");
                        timeSpanOffsetHint = TimeSpan.FromHours(8);
                        FileInfo sunriseDatafile = null;
                        doTest = true;
                        // ########################## TEST 1 CONCATENATION 
                        //string opFileStem = "Concat_Test1"; // this should be a unique site identifier
                        //string opPath = $"{drive}:\\SensorNetworks\\SoftwareTests\\Test_Concatenation\\Test1_Output\\";
                        //bool concatenateEverythingYouCanLayYourHandsOn = true;
                        // ########################## TEST 2 CONCATENATION 
                        string opFileStem = "Concat_Test2";
                        string opPath = $"{drive}:\\SensorNetworks\\SoftwareTests\\Test_Concatenation\\Test2_Output\\";
                        bool concatenateEverythingYouCanLayYourHandsOn = false; // 24 hour blocks only
                        dtoStart = new DateTimeOffset(2016, 07, 25, 0, 0, 0, TimeSpan.Zero);
                        dtoEnd = new DateTimeOffset(2016, 07, 25, 0, 0, 0, TimeSpan.Zero);
                        // ########################## END of TEST ARGUMENTS
            */

            // ################################ CONCATENATE GROOTE DATA 
            // This data derived from Groote recordings I brought back from JCU, July 2016.
            // top level directory
            //DirectoryInfo[] dataDirs = { new DirectoryInfo($"{drive}:\\SensorNetworks\\Output\\Frogs\\Canetoad\\2016Oct28-174219 - Michael, Towsey.Indices, #120\\SD Card A"),
            DirectoryInfo[] dataDirs = { new DirectoryInfo($"G:\\SensorNetworks\\OutputDataSets\\GrooteAcousticIndices_Job120\\SD Card A"),
                                                   };
            string directoryFilter = "*.wav";  // this is a directory filter to locate only the required files
            string testPath = $"{drive}:\\SensorNetworks\\SoftwareTests\\Test_Concatenation\\ExpectedOutput";
            var falseColourSpgConfig = new FileInfo($"{drive}:\\SensorNetworks\\SoftwareTests\\Test_Concatenation\\Data\\TEST_SpectrogramFalseColourConfig.yml");
            timeSpanOffsetHint = TimeSpan.FromHours(9.5);
            FileInfo sunriseDatafile = null;
            doTest = false;
            string opFileStem = "ConcatGrooteJCU";
            string opPath = $"{drive}:\\SensorNetworks\\Output\\Frogs\\Canetoad\\ConcatGroote_Job120";
            bool concatenateEverythingYouCanLayYourHandsOn = false; // 24 hour blocks only
            // start and end dates INCLUSIVE
            dtoStart = new DateTimeOffset(2016, 08, 03, 0, 0, 0, TimeSpan.Zero);
            dtoEnd   = new DateTimeOffset(2016, 08, 03, 0, 0, 0, TimeSpan.Zero);

            eventDirs = new DirectoryInfo[1];
            eventDirs[0] = new DirectoryInfo(@"G:\SensorNetworks\OutputDataSets\GrooteCaneToad_Job120\\SD Card A");
            string eventFilePattern = "*_Towsey.RhinellaMarina.Events.csv";



            //// ########################## MARINE RECORDINGS          
            //// top level directory
            ////DirectoryInfo[] dataDirs = { new DirectoryInfo(@"Y:\Results\2015Dec14-094058 - Michael, Towsey.Indices, ICD=30.0, #70\towsey\MarineRecordings\Cornell\2013March-April"),
            ////                           };
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"C:\SensorNetworks\WavFiles\MarineRecordings\Cornell\2013March-April"),
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

            //            DirectoryInfo[] dataDirs = { new DirectoryInfo(@"G:\SensorNetworks\Output\YvonneResults\DataFiles_62_93\2015Nov1"),
            //                                       };

            //below directory was to check a bug - missing 6 hours of recording
            //DirectoryInfo[] dataDirs = {
            //    new DirectoryInfo(@"Y:\Results\2015Aug06-123245 - Yvonne, Indices, ICD=60.0, #48\Yvonne\Cooloola"),
            //                           };
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"Y:\Results\2015Aug06-123245 - Yvonne, Indices, ICD=60.0, #48\Yvonne\Cooloola\2015July26\Woondum3"),
            //                           };
            //string directoryFilter = "20150725-000000+1000.wav";

            //The recording siteName is used as filter pattern to select directories. It is also used for naming the output files
            //            string directoryFilter = "Woondum3";
            //string directoryFilter = "GympieNP";   // this is a directory filter to locate only the required files

            //            string opPath = @"G:\SensorNetworks\Output\YvonneResults\ConcatenatedFiles_62_93";
            //
            //            dtoStart = new DateTimeOffset(2015, 10, 26, 0, 0, 0, TimeSpan.Zero);
            //            dtoEnd   = new DateTimeOffset(2015, 10, 28, 0, 0, 0, TimeSpan.Zero);
            //            string opFileStem = directoryFilter;

            // string sunriseDatafile = @"C:\SensorNetworks\OutputDataSets\SunRiseSet\SunriseSet2013Brisbane.csv";

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




            // ########################## EDDIE GAME'S PNG RECORDINGS
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
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(dataPath) };
            //string opPath   = dataPath;
            //string directoryFilter = "Musimunat";  // this is a directory filter to locate only the required files
            //string opFileStem = "Musimunat_BAR18"; // this should be a unique site identifier
            //string opFileStem = "TNC_Musimunat_20150703_BAR18";

            // the default set of index properties is located in the AnalysisConfig directory.
            //IndexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml".ToFileInfo();
            // However the PNG data uses an older set of index properties prior to fixing a bug!
            //FileInfo indexPropertiesConfig = new FileInfo(@"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\IndexPropertiesOLDConfig.yml");


            // ########################## GRIFFITH - SIMON/TOBY FRESH-WATER RECORDINGS          
            // top level directory
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"F:\AvailaeFolders\Griffith\Toby\20160201_FWrecordings\Site1"),
            //                           };
            //string directoryFilter = "Site2";
            //string opPath = @"F:\AvailaeFolders\Griffith\Toby\20160201_FWrecordings";
            ////string opPath = @"C:\SensorNetworks\Output\MarineSonograms\LdFcSpectrograms2013April";
            //dtoStart = new DateTimeOffset(2015, 07, 09, 0, 0, 0, TimeSpan.Zero);
            //dtoEnd = new DateTimeOffset(2015, 07, 10, 0, 0, 0, TimeSpan.Zero);
            //string opFileStem = "Site1_20150709";

            // ########################## END of GRIFFITH - SIMON/TOBY FRESH-WATER RECORDINGS


            if (!indexPropertiesConfig.Exists) LoggedConsole.WriteErrorLine("# indexPropertiesConfig FILE DOES NOT EXIST.");

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
                TestDirectory = new DirectoryInfo(testPath),
                FileStemName = opFileStem,
                StartDate = dtoStart,
                EndDate = dtoEnd,
                IndexPropertiesConfig = indexPropertiesConfig,
                FalseColourSpectrogramConfig = falseColourSpgConfig,
                ConcatenateEverythingYouCanLayYourHandsOn = concatenateEverythingYouCanLayYourHandsOn,
                TimeSpanOffsetHint = timeSpanOffsetHint,
                SunRiseDataFile = sunriseDatafile,
                DrawImages = drawImages,
                Verbose = true,
                DoTest = doTest,
                // following used to add in a recognizer score track
                EventDataDirectories = eventDirs,
                EventFilePattern = eventFilePattern,
            };
            throw new NoDeveloperMethodException();
    }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Execute(Arguments arguments)
        {
            string analysisType = "Towsey.Acoustic";

            // deal with verbosity
            bool verbose = false; // default
            if (arguments == null)
            {
                arguments = Dev();
                verbose = true; // default is verbose if in dev mode
            }
            verbose = arguments.Verbose;
            IndexMatrices.Verbose = verbose;

            if (arguments.InputDataDirectory != null)
            {
                arguments.InputDataDirectories =
                    (arguments.InputDataDirectories ?? new DirectoryInfo[0]).Concat(new[] {arguments.InputDataDirectory}).ToArray();
            }


            //Log.Warn("DrawImages option hard coded to be on in this version");
            //arguments.DrawImages = true;


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
            string pattern = "*_Towsey.Acoustic.Indices.csv";
            FileInfo[] csvFiles = IndexMatrices.GetFilesInDirectories(subDirectories, pattern);
            LoggedConsole.WriteLine("# Subdirectories Count = " + subDirectories.Length);
            LoggedConsole.WriteLine("# IndexFiles.csv Count = " + csvFiles.Length);

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

            var dateTimeOffset = (DateTimeOffset)startDate;
            TimeSpan totalTimespan = (DateTimeOffset)endDate - dateTimeOffset;
            int dayCount = totalTimespan.Days + 1; // assume last day has full 24 hours of recording available.

            if (verbose)
            {
                LoggedConsole.WriteLine("\n# Start date = " + startDate.ToString());
                LoggedConsole.WriteLine("# End   date = " + endDate.ToString());
                LoggedConsole.WriteLine($"# Elapsed time = {totalTimespan.TotalHours:f1} hours");
                LoggedConsole.WriteLine("# Time Zone  = " + arguments.TimeSpanOffsetHint.ToString());

                if ((arguments.SunRiseDataFile != null) && (arguments.SunRiseDataFile.Exists))
                {
                    LoggedConsole.WriteLine("# Sunrise/sunset data file = " + arguments.TimeSpanOffsetHint.ToString());
                }
                else
                {
                    LoggedConsole.WriteLine("####### WARNING ####### The sunrise/sunset data file does not exist >> " + arguments.TimeSpanOffsetHint.ToString());
                }
            }

            // create top level output directory if it does not exist.
            DirectoryInfo opDir = arguments.OutputDirectory;
            if (!opDir.Exists) arguments.OutputDirectory.Create();
            string opFileStem = arguments.FileStemName;



            // SET UP DEFAULT SITE LOCATION INFO    --  DISCUSS IWTH ANTHONY
            // The following location data is used only to draw the sunrise/sunset tracks on images.
            double? latitude = null;
            double? longitude = null;
            var siteDescription = new SiteDescription
            {
                SiteName = arguments.FileStemName,
                Latitude = latitude,
                Longitude = longitude
            };

            // the following are required if drawing the index images
            IndexGenerationData indexGenerationData = null;
            FileInfo indexPropertiesConfig = null;
            LdSpectrogramConfig ldSpectrogramConfig = null;

            if (arguments.DrawImages)
            {
                // get the IndexGenerationData file from the first directory
                indexGenerationData = IndexGenerationData.GetIndexGenerationData(csvFiles[0].Directory);
                if (indexGenerationData.RecordingStartDate == null) indexGenerationData.RecordingStartDate = startDate;

                indexPropertiesConfig = arguments.IndexPropertiesConfig;

                // prepare the false-colour spgm config file
                if (arguments.FalseColourSpectrogramConfig.Exists)
                {
                    ldSpectrogramConfig = LdSpectrogramConfig.ReadYamlToConfig(arguments.FalseColourSpectrogramConfig);

                    // TODO TODO TODO TODO    
                    // this next line is necessary because TimeSpan is not read correctly from yaml.
                    ldSpectrogramConfig.XAxisTicInterval = TimeSpan.FromMinutes(60);
                    ldSpectrogramConfig.ColourGain = 1.0;
                    ldSpectrogramConfig.ColourFilter = 1.0;
                }
                else // set up a default config
                {
                    ldSpectrogramConfig = new LdSpectrogramConfig
                    {
                        XAxisTicInterval = SpectrogramConstants.X_AXIS_TIC_INTERVAL,
                        YAxisTicInterval = 1000,
                        //ColorMap1 = "ACI-TEN-CVR",
                        //ColorMap2 = "BGN-AVG-VAR",
                        ColorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN,
                        ColorMap2 = SpectrogramConstants.RGBMap_BGN_POW_SPT,
                    };
                }


            }




            // ################################ ConcatenateEverythingYouCanLayYourHandsOn = true
            DirectoryInfo resultsDir = null;
            if (arguments.ConcatenateEverythingYouCanLayYourHandsOn)
            {
                string dateString = $"{dateTimeOffset.Year}{dateTimeOffset.Month:D2}{dateTimeOffset.Day:D2}";
                resultsDir = new DirectoryInfo(Path.Combine(opDir.FullName, arguments.FileStemName, dateString));
                if (!resultsDir.Exists) resultsDir.Create();

                // ###### FIRST CONCATENATE THE SUMMARY INDICES, DRAW IMAGES AND SAVE IN RESULTS DIRECTORY
                var summaryIndexFiles = sortedDictionaryOfDatesAndFiles.Values.ToArray<FileInfo>();

                var dictionaryOfSummaryIndices = LDSpectrogramStitching.ConcatenateAllSummaryIndexFiles(summaryIndexFiles, resultsDir, indexGenerationData, opFileStem);
                // REALITY CHECK - check for continuous zero indices or anything else that might indicate defective signal or incomplete analysis of recordings
                List<ErroneousIndexSegments> indexErrors = ErroneousIndexSegments.DataIntegrityCheck(dictionaryOfSummaryIndices, resultsDir, arguments.FileStemName);

                
                if (arguments.DrawImages)
                {

                    TimeSpan start = ((DateTimeOffset)indexGenerationData.RecordingStartDate).TimeOfDay;
                    string startTime = $"{start.Hours:d2}{start.Minutes:d2}h";
                    string imageTitle = $"SOURCE: \"{opFileStem}\".     Starts at {startTime}                       (c) QUT.EDU.AU";
                    Bitmap tracksImage =
                            IndexDisplay.DrawImageOfSummaryIndices(
                            IndexProperties.GetIndexProperties(indexPropertiesConfig),
                            dictionaryOfSummaryIndices,
                            imageTitle,
                            indexGenerationData.IndexCalculationDuration,
                            indexGenerationData.RecordingStartDate);

                    var imagePath = FilenameHelpers.AnalysisResultPath(resultsDir, opFileStem, "SummaryIndices", "png");
                    tracksImage.Save(imagePath);
                }
                
                // ###### THEN CONCATENATE THE SPECTRAL INDICES, DRAW IMAGES AND SAVE IN RESULTS DIRECTORY
                var dictionaryOfSpectralIndices1 = LDSpectrogramStitching.ConcatenateAllSpectralIndexFiles(subDirectories, resultsDir, indexPropertiesConfig, indexGenerationData, opFileStem);

                // The currently available sepctral indices are  "ACI", "ENT", "EVN", "BGN", "POW", "CLS", "SPT", "RHZ", "CVR"
                // RHZ, SPT and CVR correlated with POW and do not add much. Currently use SPT.  Do not use CLS. Not particularly useful.
                if (arguments.DrawImages)
                {
                    //string filename = "20160724_121922_continuous1";
                    //Dictionary<string, IndexDistributions.SpectralStats> indexDistributions = IndexDistributions.ReadSpectralIndexDistributionStatistics(resultsDir, filename);

                    Tuple<Image, string>[] tuple = LDSpectrogramRGB.DrawSpectrogramsFromSpectralIndices(
                            subDirectories[0],
                            resultsDir,
                            ldSpectrogramConfig,
                            indexPropertiesConfig,
                            indexGenerationData,
                            opFileStem,
                            analysisType,
                            dictionaryOfSpectralIndices1,
                            /*summaryIndices = */null,
                            /*indexDistributions*/ null,
                            siteDescription,
                            arguments.SunRiseDataFile,
                            indexErrors,
                            ImageChrome.With);
                }

                WriteSpectralIndexFiles(resultsDir, opFileStem, analysisType, dictionaryOfSpectralIndices1);

                if (arguments.DoTest)
                {
                    // THis is test 1.
                    var expectedTestFile1 = new FileInfo(Path.Combine(arguments.TestDirectory.FullName, "Concat_Test1__SummaryIndexStatistics.EXPECTED.json"));
                    var expectedTestFile2 = new FileInfo(Path.Combine(arguments.TestDirectory.FullName, "Concat_Test1__SpectralIndexStatistics.EXPECTED.json"));

                    var trialFile1 = new FileInfo(Path.Combine(resultsDir.FullName, opFileStem + "__SummaryIndexStatistics.json"));
                    var trialFile2 = new FileInfo(Path.Combine(resultsDir.FullName, opFileStem + "__SpectralIndexStatistics.json"));

                    string testName1 = "test1";
                    TestTools.FileEqualityTest(testName1, trialFile1, expectedTestFile1);

                    string testName2 = "test2";
                    TestTools.FileEqualityTest(testName2, trialFile2, expectedTestFile2);
                }

                Log.Success("Complete ConcatenateEverythingYouCanLayYourHandsOn");
                return;
            } // ConcatenateEverythingYouCanLayYourHandsOn






            // ############################# CONCATENATE in 24 hour BLOCKS of DATA  ### ConcatenateEverythingYouCanLayYourHandsOn = false
            LoggedConsole.WriteLine($"# Elapsed time = {totalTimespan.TotalHours:f1} hours or {dayCount} days");
            LoggedConsole.WriteLine("# Day  count = " + dayCount + " (inclusive of start and end days)");
            LoggedConsole.WriteLine("# Time Zone  = " + arguments.TimeSpanOffsetHint.ToString());

            // loop over days
            for (int d = 0; d < dayCount; d++)
            {
                var thisday = dateTimeOffset.AddDays(d);
                LoggedConsole.WriteLine($"\n\n\nCONCATENATING DAY {(d + 1)} of {dayCount}:   {thisday.ToString()}");

                FileInfo[] indexFiles = LDSpectrogramStitching.GetFileArrayForOneDay(sortedDictionaryOfDatesAndFiles, thisday);
                if (indexFiles.Length == 0)
                {
                    LoggedConsole.WriteErrorLine("\n\nWARNING from method ConcatenateIndexFiles.Execute():");
                    LoggedConsole.WriteErrorLine("        No files of SUMMARY indices were found.");
                    LoggedConsole.WriteErrorLine("        Break cycle through days!!! ");
                    break;
                }

                // CREATE DAY LEVEL OUTPUT DIRECTORY for this day
                string format = "yyyyMMdd";
                string dateString = thisday.ToString(format);
                resultsDir = new DirectoryInfo(Path.Combine(opDir.FullName, arguments.FileStemName, dateString));
                if (!resultsDir.Exists) resultsDir.Create();

                var opFileStem1 = $"{arguments.FileStemName}_{dateString}";
                
                // Recalculate <thisDay> to include the start time - not just the date. This is for time scale on false-colour spectrograms.
                DateTimeOffset dt;
                if (FileDateHelpers.FileNameContainsDateTime(indexFiles[0].Name, out dt, arguments.TimeSpanOffsetHint))
                {
                    thisday = dt;
                } // else <thisday> will not contain the start time of the day.

                // CONCATENATE the SUMMARY INDEX FILES
                var summaryDict = LDSpectrogramStitching.ConcatenateAllSummaryIndexFiles(indexFiles, resultsDir, indexGenerationData, opFileStem1);

                if (summaryDict == null)
                {
                    break;
                }

                // REALITY CHECK - check for zero signal and anything else that might indicate defective signal
                List<ErroneousIndexSegments> indexErrors = ErroneousIndexSegments.DataIntegrityCheck(summaryDict, resultsDir, opFileStem1);


                // DRAW SUMMARY INDEX IMAGES AND SAVE IN RESULTS DIRECTORY
                if (arguments.DrawImages)
                {
                    indexGenerationData.RecordingStartDate = thisday;
                    LDSpectrogramStitching.DrawSummaryIndexFiles(summaryDict, 
                                                                 indexGenerationData, 
                                                                 indexPropertiesConfig, 
                                                                 resultsDir, 
                                                                 siteDescription,
                                                                 arguments.SunRiseDataFile,
                                                                 indexErrors,
                                                                 verbose);
                }

                // ##############################################################################################################

                // NOW CONCATENATE SPECTRAL INDEX FILES
                //Filter the array of Directories to get the correct dates
                var spectralSubdirectories = FileDateHelpers.FilterDirectoriesForDates(subDirectories, arguments.TimeSpanOffsetHint);
                var dirArray = LDSpectrogramStitching.GetDirectoryArrayForOneDay(spectralSubdirectories, thisday);

                if (dirArray.Length == 0)
                {
                    LoggedConsole.WriteErrorLine("\n\nWARNING from method ConcatenateIndexFiles.Execute():");
                    LoggedConsole.WriteErrorLine("        No directories of Spectral indices were found.   Break cycle through days!!! ");
                    break;
                }

                var dictionaryOfSpectralIndices2 = LDSpectrogramStitching.ConcatenateAllSpectralIndexFiles(dirArray, resultsDir, indexPropertiesConfig, indexGenerationData, opFileStem1);
                if (dictionaryOfSpectralIndices2.Count == 0)
                {
                    LoggedConsole.WriteErrorLine("WARNING from method ConcatenateIndexFiles.Execute():");
                    LoggedConsole.WriteErrorLine("        An empty dictionary of SPECTRAL indices was returned !!! ");
                    return;
                }

                // DRAW SPECTRAL INDEX IMAGES AND SAVE IN RESULTS DIRECTORY
                if (arguments.DrawImages)
                {
                    LDSpectrogramRGB.DrawSpectrogramsFromSpectralIndices(
                        subDirectories[0],
                        resultsDir,
                        ldSpectrogramConfig,
                        indexPropertiesConfig,
                        indexGenerationData,
                        opFileStem1,
                        analysisType,
                        dictionaryOfSpectralIndices2,
                        /*summaryIndices = */null,
                        /*indexDistributions*/ null,
                        siteDescription,
                        arguments.SunRiseDataFile,
                        indexErrors,
                        ImageChrome.With);


                    if (arguments.EventDataDirectories != null)
                    {
                        var candidateFiles = IndexMatrices.GetFilesInDirectories(arguments.EventDataDirectories, arguments.EventFilePattern);
                        var sortedDictionaryOfEventFiles = FileDateHelpers.FilterFilesForDates(candidateFiles, arguments.TimeSpanOffsetHint);
                        var eventFiles = LDSpectrogramStitching.GetFileArrayForOneDay(sortedDictionaryOfEventFiles, thisday);

                        //int lineCount = 0;
                        var output = new List<string>();
                        foreach (FileInfo file in eventFiles)
                        {
                            var lines = FileTools.ReadTextFile(file.FullName);
                            lines.RemoveAt(0); // ignore header
                            output.AddRange(lines);
                            //lineCount += lines.Count;
                            //Console.WriteLine($"  # events = {lines.Count}"); 
                        }
                        var indexArray = ConcatenateIndexFiles.ConvertEventsToSummaryIndices(output);

                        double[] normalisedScores;
                        double normalisedThreshold;
                        DataTools.Normalise(indexArray, 2, out normalisedScores, out normalisedThreshold);
                        //var plot = new Plot("Cane Toad", normalisedScores, normalisedThreshold);
                        Image recognizerTrack = ImageTools.DrawGraph("Canetoad events", normalisedScores, 32);
                        string imageFilePath = Path.Combine(resultsDir.FullName, opFileStem + "_"+ dateString + "__2Maps" + ".png");
                        Image twoMaps = ImageTools.ReadImage2Bitmap(imageFilePath);
                        var imageList = new List<Image> { twoMaps, recognizerTrack };
                        Bitmap compositeBmp = (Bitmap)ImageTools.CombineImagesVertically(imageList);

                        string imagePath2 = Path.Combine(resultsDir.FullName, opFileStem + "_" + dateString + ".png");
                        compositeBmp.Save(imagePath2);
                    }
                }

                WriteSpectralIndexFiles(subDirectories[0], opFileStem1, analysisType, dictionaryOfSpectralIndices2);

            } // over days

            if (arguments.DoTest)
            {
                var dto = (DateTimeOffset)arguments.StartDate;
                var date = $"{dto.Year}{dto.Month:D2}{dto.Day:D2}";
                var fileName = arguments.FileStemName + "_" + date;
                // THis is test 2.
                var expectedTestFile1 = new FileInfo(Path.Combine(arguments.TestDirectory.FullName, "Concat_Test2__SummaryIndexStatistics.EXPECTED.json"));
                var expectedTestFile2 = new FileInfo(Path.Combine(arguments.TestDirectory.FullName, "Concat_Test2__SpectralIndexStatistics.EXPECTED.json"));

                var trialFile1 = new FileInfo(Path.Combine(resultsDir.FullName, fileName + "__SummaryIndexStatistics.json"));
                var trialFile2 = new FileInfo(Path.Combine(resultsDir.FullName, fileName + "__SpectralIndexStatistics.json"));

                string testName1 = "test1";
                TestTools.FileEqualityTest(testName1, trialFile1, expectedTestFile1);

                string testName2 = "test2";
                TestTools.FileEqualityTest(testName2, trialFile2, expectedTestFile2);
            }

            Log.Success("Complete");
        } // Execute()


        public static List<FileInfo> WriteSpectralIndexFiles(DirectoryInfo destination, string fileNameBase, string identifier, Dictionary<string, double[,]> results)
        {

            Log.Info("Writing spectral indices");
            
            var spectralIndexFiles = new List<FileInfo>(results.Count);

            foreach (var kvp in results)
            {
                // write spectrogram to disk as CSV file
                var filename = FilenameHelpers.AnalysisResultPath(destination, fileNameBase, identifier + "." + kvp.Key, "csv").ToFileInfo();
                spectralIndexFiles.Add(filename);
                Csv.WriteMatrixToCsv(filename, kvp.Value, TwoDimensionalArray.ColumnMajorFlipped);
            }

            Log.Debug("Finished writing spectral indices");
            return spectralIndexFiles;
        }


        /// <summary>
        /// This method is designed only to read in Spectrogram ribbons for Georgia marine recordings. 
        /// Used to prepare images for Aaron Rice.
        /// </summary>
        /// <param name="dataDirs"></param>
        /// <param name="pattern"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="opFileStem"></param>
        /// <param name="title"></param>
        /// <param name="tidalInfo"></param>
        public static void ConcatenateRibbonImages(DirectoryInfo[] dataDirs, string pattern, DirectoryInfo outputDirectory, 
                                                   string opFileStem, string title, SunAndMoon.SunMoonTides[] tidalInfo = null)
        {
            //get the ribon files
            FileInfo[] imageFiles = IndexMatrices.GetFilesInDirectories(dataDirs, pattern);

            DateTimeOffset dto = new DateTimeOffset(2013, 3, 1, 0, 0, 0, TimeSpan.Zero);
            TimeSpan oneday = new TimeSpan(24, 0, 0);

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

                // draw on the tidal and sun info IFF available.
                if (tidalInfo != null)
                {
                    AddTidalInfo(image, tidalInfo, dto);
                }
                dto = dto.Add(oneday);
                Console.WriteLine(dto.ToString());

                imageList.Add(image);
                imageList.Add(spacer);
            }

            //create composite image
            var compositeBmp = (Bitmap)ImageTools.CombineImagesVertically(imageList);

            // create left side day scale    
            var stringFont = new Font("Arial", 16);
            imageList = new List<Image>();
            for(int i = 0; i < imageCount; i++)
            {
                image = new Bitmap(60, imageHt);
                canvas = Graphics.FromImage(image);
                var str = $"{i + 1}";
                canvas.DrawString(str, stringFont, Brushes.White, new PointF(3, 3));

                imageList.Add(image);
                imageList.Add(spacer);
            }

            //create composite image
            var compositeBmpYscale = (Bitmap)ImageTools.CombineImagesVertically(imageList);
            Image[] finalImages = { compositeBmpYscale, compositeBmp, compositeBmpYscale };
            var finalComposite = (Bitmap)ImageTools.CombineImagesInLine(finalImages);

            // add title bar
            var titleBmp = new Bitmap(finalComposite.Width, 30);
            canvas = Graphics.FromImage(titleBmp);
            canvas.DrawString(title, stringFont, Brushes.White, new PointF(30, 3));

            // add title plus spacer
            spacer = new Bitmap(finalComposite.Width, 3);
            canvas = Graphics.FromImage(spacer);
            canvas.Clear(Color.Gray);
            Image[] titledImages = { titleBmp, spacer, finalComposite };
            finalComposite = (Bitmap)ImageTools.CombineImagesVertically(titledImages);

            finalComposite.Save(Path.Combine(outputDirectory.FullName, opFileStem + ".png"));
            Console.WriteLine($"Final compositeBmp dimensions are width {compositeBmp.Width} by height {compositeBmp.Height}");
            Console.WriteLine($"Final number of ribbons/days = {imageFiles.Length}");

        } //ConcatenateRibbonImages


        static void AddTidalInfo(Bitmap image, SunAndMoon.SunMoonTides[] tidalInfo, DateTimeOffset dto)
        {
            Pen yellowPen = new Pen(Brushes.Yellow);
            Pen cyanPen   = new Pen(Brushes.Lime, 2);
            Pen whitePen  = new Pen(Brushes.White, 2);
            Graphics spgCanvas = Graphics.FromImage(image);

            foreach (SunAndMoon.SunMoonTides smt in tidalInfo)
            {
                if (smt.Date == dto)
                {
                    foreach (KeyValuePair<string, DateTimeOffset> kvp in smt.dictionary)
                    {
                        string key = kvp.Key;
                        DateTimeOffset dto2 = kvp.Value;
                        var thisPen = yellowPen;
                        if (key == SunAndMoon.SunMoonTides.HIGHTIDE) thisPen = cyanPen;
                        else if (key == SunAndMoon.SunMoonTides.LOWTIDE) thisPen = whitePen;

                        int minute = (int)Math.Round(dto2.TimeOfDay.TotalMinutes * 2); //IMPORTANT multiply by 2 because scale = 30s/px.
                        spgCanvas.DrawLine(thisPen, minute, 0, minute, image.Height);
                    }
                }
            }
        }



        public static void ConcatenateAcousticEventFiles(DirectoryInfo[] dataDirs, string pattern, DirectoryInfo outputDirectory, string opFileStem)
        {
            //get the csv files
            FileInfo[] csvFiles = IndexMatrices.GetFilesInDirectories(dataDirs, pattern);
            int lineCount = 0;
            var output = new List<string>();

            foreach (FileInfo file in csvFiles)
            {
                var lines = FileTools.ReadTextFile(file.FullName);
                lines.RemoveAt(0);
                output.AddRange(lines);
                lineCount += lines.Count;
                Console.WriteLine($"  # events = {lines.Count}"); // ignore header
            }

            Console.WriteLine($"Final number of FILES = {csvFiles.Length}");
            Console.WriteLine($"Final number of lines = {lineCount}");
            Console.WriteLine($"Final number of lines = {output.Count}");

            var indexArray = ConvertEventsToSummaryIndices(output);
            Console.WriteLine($"Final number of events  = {indexArray.Sum()}");
            double maxValue = indexArray.Max();
            Console.WriteLine($"Max Value in any minute = {maxValue}");
            indexArray = DataTools.normalise(indexArray);

            Image image = ImageTools.DrawGraph("Canetoad events", indexArray, 100);

            string title = string.Format("Canetoad events: {0}                       Max value={1:f0}", opFileStem, maxValue);
            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, indexArray.Length);

            string firstFileName = csvFiles[0].Name;
            DateTimeOffset startTime = DataTools.Time_ConvertDateString2DateTime(firstFileName);
            var duration = new TimeSpan(0, indexArray.Length, 0);

            int trackHeight = 20;
            Bitmap timeBmp1 = Image_Track.DrawTimeRelativeTrack(duration, indexArray.Length, trackHeight);
            //Bitmap timeBmp2 = (Bitmap)timeBmp1.Clone();
            Bitmap timeBmp2 = Image_Track.DrawTimeTrack(duration, startTime, indexArray.Length, trackHeight);

            var imageList = new List<Image> { titleBar, timeBmp1, image, timeBmp2 };
            Bitmap compositeBmp = (Bitmap)ImageTools.CombineImagesVertically(imageList);

            string imagePath = Path.Combine(outputDirectory.FullName, opFileStem + ".png");
            compositeBmp.Save(imagePath);


        } //ConcatenateAcousticEventFiles


        public static double[] ConvertEventsToSummaryIndices(List<string> events)
        {
            // Assume that each line in events = one event
            // assume one minute resolution for events index 
            TimeSpan? offsetHint = new TimeSpan(9, 0, 0);
            //int unitTime = 60; // one minute resolution

            // get start and end time from first and last file name
            string line = events[0];
            string[] fields = line.Split(',');
            string fstartFileName = fields[14];
            //string[] times = fileName.Split('_');
            //string startOffset = fields[15];
            DateTimeOffset startTime = DataTools.Time_ConvertDateString2DateTime(fstartFileName);

            // get last event
            line = events[events.Count-1];
            fields = line.Split(',');
            string endFileName = fields[14];
            DateTimeOffset endTime = DataTools.Time_ConvertDateString2DateTime(endFileName);

            string[] parts = endFileName.Split('_');
            int addOn = int.Parse(parts[2].Substring(0, parts[2].Length-3));
            //var addOnTime = new DateTime(0, 0, 0, 0, addOn, 0);

            TimeSpan duration = endTime - startTime;
            // get whole minutes
            int minuteCount = addOn + (int)Math.Ceiling(duration.TotalMinutes) + 1;

            // to store event counts
            var eventsPerUnitTime = new double[minuteCount];

            foreach (var line1 in events)
            {
                string[] fieldArray = line1.Split(',');
                // note: absolute determines what value is used
                // EventStartSeconds (relative to segment)
                // StartOffset (relative to recording)

                string fileName = fieldArray[14];
                DateTimeOffset evTime = DataTools.Time_ConvertDateString2DateTime(fileName);
                TimeSpan elapsedTime = evTime - startTime;

                parts = fileName.Split('_');
                int addOn1 = int.Parse(parts[2].Substring(0, parts[2].Length - 3));
                int minuteId = addOn1 + (int)Math.Round(elapsedTime.TotalMinutes);
                eventsPerUnitTime[minuteId] ++;

                // Console.WriteLine($"minuteId={minuteId}  elapsedTimeFromStart.Minutes={elapsedTime.TotalMinutes}");

                //double eventStart = ev.StartOffset.TotalSeconds : ev.EventStartSeconds;
                //var timeUnit = (int)(eventStart / unitTime.TotalSeconds);

                /*
                // NOTE: eventScore filter replaced with greater then as opposed to not equal to
                if (eventScore >= 0.0)
                {
                    eventsPerUnitTime[timeUnit]++;
                }

                if (eventScore > scoreThreshold)
                {
                    bigEvsPerUnitTime[timeUnit]++;
                }
                */
            }

            /*
            var indices = new SummaryIndexBase[eventsPerUnitTime.Length];

            for (int i = 0; i < eventsPerUnitTime.Length; i++)
            {
                var newIndex = new EventIndex
                {
                    StartOffset = unitTime.Multiply(i),
                    EventsTotal = eventsPerUnitTime[i],
                    EventsTotalThresholded = bigEvsPerUnitTime[i],
                    SegmentDuration = absolute ? unitTime : duration
                };

                indices[i] = newIndex;
            }
            */

            //return indices;
            return eventsPerUnitTime;
        }

    }
}
