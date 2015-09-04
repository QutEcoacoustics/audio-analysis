// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DrawLongDurationSpectrograms.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the DrawLongDurationSpectrograms type.
//
// Action code for this analysis = ColourSpectrogram
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

    using Acoustics.Shared;

    using AnalysisPrograms.Production;

    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.LongDurationSpectrograms;

    using PowerArgs;
    using AudioAnalysisTools;


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
            public string SiteName { get; set; }

            [ArgDescription("File stem name for output files.")]
            public string FileStemName { get; set; }

            [ArgDescription("DateTime at which concatenation begins. If null, then start with earliest available file.")]
            public DateTimeOffset? StartDate { get; set; }

            [ArgDescription("DateTime at which concatenation ends. If missing|null, then will be set = today's date or last available file.")]
            public DateTimeOffset? EndDate { get; set; }

            //[ArgDescription("Draw images of summary and spectral indices after concatenating them")]
            internal bool DrawImages { get; set; }

            //[ArgDescription("User specified file containing a list of indices and their properties.")]
            //[Production.ArgExistingFile(Extension = ".yml")]
            //[ArgPosition(1)]
            internal FileInfo IndexPropertiesConfig { get; set; }
        }

        /// <summary>
        /// To get to this DEV method, the FIRST AND ONLY command line argument must be "concatenateIndexFiles"
        /// </summary>
        public static Arguments Dev()
        {
            // ########################## YVONNE'S RECORDINGS
            // top level directory
            //FileInfo indexPropertiesConfig = new FileInfo(@"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\IndexPropertiesConfig.yml");
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"Y:\Results\2015Aug06-123245 - Yvonne, Indices, ICD=60.0, #48"),
            //                             new DirectoryInfo(@"Y:\Results\2015Aug20-154235 - Yvonne, Indices, ICD=60.0, #50") };
            //string opPath = @"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults";
            // The recording siteName is used as filter pattern. It is also used for naming the output files
            //string siteName = "Woondum3";
            //string siteName = "GympieNP";
            //string fileStemName = siteName;


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

            FileInfo indexPropertiesConfig = new FileInfo(@"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\IndexPropertiesOLDConfig.yml");
            string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Musiamunat_3-7-15\BAR\BAR_18\";
            string opPath   = dataPath;
            string siteName = "Musimunat";
            string fileStemName = "Musimunat_BAR18";
            //string opFileStem = "TNC_Musimunat_20150703_BAR18";
            DirectoryInfo[] dataDirs = { new DirectoryInfo(dataPath) };
            // ########################## END of EDDIE GAME'S RECORDINGS




            //var dtoStart = new DateTimeOffset(2015, 6, 22, 0, 0, 0, TimeSpan.Zero);
            //var dtoEnd   = new DateTimeOffset(2015, 6, 22, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset? dtoStart = null;
            DateTimeOffset? dtoEnd = null;

            bool drawImages = true;
             if(!indexPropertiesConfig.Exists) LoggedConsole.WriteErrorLine("# indexPropertiesConfig FILE DOES NOT EXIST.");

            // DISCUSS THE FOLLOWING WITH ANTHONY
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
                SiteName  = siteName,
                FileStemName = fileStemName,
                StartDate = dtoStart,
                EndDate   = dtoEnd,
                //SiteDescription = siteDescription,
                DrawImages = drawImages,
                IndexPropertiesConfig = indexPropertiesConfig,
                // use the default set of index properties in the AnalysisConfig directory.
                //IndexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml".ToFileInfo(),
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
                    LoggedConsole.WriteLine("# Start date = NULL");
                else
                    LoggedConsole.WriteLine("# Start date = " + arguments.StartDate.ToString());

                if (arguments.EndDate == null)
                    LoggedConsole.WriteLine("# End date = NULL");
                else
                    LoggedConsole.WriteLine("# End   date = " + arguments.EndDate.ToString());

                LoggedConsole.WriteLine("# SITE FILTER = " + arguments.SiteName);
                LoggedConsole.WriteLine();
                //LoggedConsole.WriteLine("# Index Properties Config file: " + arguments.IndexPropertiesConfig);
            }


            // 1. PATTERN SEARCH FOR CORRECT SUBDIRECTORIES
            // Assumes that the required subdirectories have the given site name somewhere in their path. 
            var subDirectories = LDSpectrogramStitching.GetSubDirectoriesForSiteData(arguments.InputDataDirectories, arguments.SiteName);
            if (subDirectories.Length == 0)
            {
                LoggedConsole.WriteErrorLine("\n# WARNING: Subdirectory Count = ZERO");
                LoggedConsole.WriteErrorLine("\n# RETURNING EMPTY HANDED!");
                return;
            }

            // 2. PATTERN SEARCH FOR SUMMARY INDEX FILES. Do this because want to determine earliest date of files.
            string pattern = "*__Towsey.Acoustic.Indices.csv";
            FileInfo[] csvFiles = IndexMatrices.GetFilesInDirectories(subDirectories, pattern);

            if (verbose)
            {
                LoggedConsole.WriteLine("# Subdirectory Count = " + subDirectories.Length);
                LoggedConsole.WriteLine("# Indices.csv  Count = " + csvFiles.Length);
                LoggedConsole.WriteLine("# First  file   name = " + csvFiles[0].Name);
                LoggedConsole.WriteLine("# Last   file   name = " + csvFiles[csvFiles.Length - 1].Name);
            }

            var startendDTO = LDSpectrogramStitching.GetStartAndEndDateTimes(csvFiles);

            // calculate start date if passed value = null.
            DateTimeOffset? startDate = arguments.StartDate;
            if (startDate == null)
            {
                LoggedConsole.WriteLine("# Revising start date ... ");
                startDate = startendDTO[0];
            }
            // calculate end date if passed value = null.
            DateTimeOffset? endDate = arguments.EndDate;
            if (endDate == null)
            {
                LoggedConsole.WriteLine("# Revising end date ... ");
                endDate = startendDTO[1];
                //endDate = DateTimeOffset.UtcNow;
            }

            TimeSpan timespan = (DateTimeOffset)endDate - (DateTimeOffset)startDate;
            int dayCount = timespan.Days;

            if (verbose)
            {
                LoggedConsole.WriteLine("\n# REVISED START AND END DAYS:");
                LoggedConsole.WriteLine("# Start date = " + startDate.ToString());
                LoggedConsole.WriteLine("# End   date = " + endDate.ToString());
                LoggedConsole.WriteLine("# Day  count = " + dayCount);
                LoggedConsole.WriteLine();
            }

            if (dayCount == 0)
            {
                LoggedConsole.WriteErrorLine("\nNUMBER OF DAYS TO PROCESS = ZERO. Must process one day. Day count has been set = 1.");
                dayCount = 1;
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


            // loop over days
            for (int d = 0; d < dayCount; d++)
            {
                // TODO TODO TODO TODO    Fix up time of day on subsequent passes through day loop.
                // TIME OF DAY should be changed to midnight.

                var thisday = ((DateTimeOffset)startDate).AddDays(d);
                FileInfo[] files = LDSpectrogramStitching.GetSummaryIndexFilesForOneDay(subDirectories, thisday);
                if (files.Length == 0)
                {
                    LoggedConsole.WriteErrorLine("\n\nWARNING from method ConcatenateIndexFiles.Execute():");
                    LoggedConsole.WriteErrorLine("        No files of SUMMARY indices were found.");
                    LoggedConsole.WriteErrorLine("        Break cycling through days!!! ");
                    break;
                }
                LoggedConsole.WriteLine("\n\n\nCONCATENATING DAY: " + thisday.ToString());

                // CREATE DAY LEVEL OUTPUT DIRECTORY for this day
                string dateString = String.Format("{0}{1:D2}{2:D2}", thisday.Year, thisday.Month, thisday.Day);
                DirectoryInfo resultsDir = new DirectoryInfo(Path.Combine(opDir.FullName, arguments.FileStemName, dateString));
                if (!resultsDir.Exists) resultsDir.Create();

                string opFileStem = String.Format("{0}_{1}", arguments.FileStemName, dateString);
                var indicesFile = FilenameHelpers.AnalysisResultName(resultsDir, opFileStem, LDSpectrogramStitching.SummaryIndicesStr, LDSpectrogramStitching.CsvFileExt);
                var indicesCsvfile = new FileInfo(indicesFile);

                var summaryDict = LDSpectrogramStitching.ConcatenateSummaryIndexFiles(files, resultsDir, indicesCsvfile);
                if (summaryDict.Count == 0)
                {
                    LoggedConsole.WriteErrorLine("\n\nWARNING from method ConcatenateIndexFiles.Execute():");
                    LoggedConsole.WriteErrorLine("        An empty dictionary of SUMMARY indices was returned !!! ");
                    break;
                }

                // DRAW SUMMARY INDEX IMAGES AND SAVE IN RESULTS DIRECTORY
                if (arguments.DrawImages)
                {
                    indexGenerationData.RecordingStartDate = thisday;
                    LDSpectrogramStitching.DrawSummaryIndexFiles(summaryDict, indexGenerationData, indexPropertiesConfig, resultsDir, siteDescription);
                }

                // ##############################################################################################################

                // NOW CONCATENATE SPECTRAL INDEX FILES
                var spectralDict = LDSpectrogramStitching.ConcatenateSpectralIndexFilesForOneDay(subDirectories, resultsDir, arguments.FileStemName, thisday);
                if (spectralDict.Count == 0)
                {
                    LoggedConsole.WriteErrorLine("WARNING from method ConcatenateIndexFiles.Execute():");
                    LoggedConsole.WriteErrorLine("        An empty dictionary of SPECTRAL indices was returned !!! ");
                    return;
                }

                // DRAW SPECTRAL INDEX IMAGES AND SAVE IN RESULTS DIRECTORY
                if (arguments.DrawImages)
                {
                    LDSpectrogramStitching.DrawSpectralIndexFiles(spectralDict, indexGenerationData, indexPropertiesConfig, resultsDir, siteDescription);
                }

            } // over days

        } // Execute()
    }
}
