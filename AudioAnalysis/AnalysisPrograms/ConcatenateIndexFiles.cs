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

            [ArgDescription("DateTime at which concatenation begins. If null, then start with earliest available file.")]
            public DateTimeOffset? StartDate { get; set; }

            [ArgDescription("Number of days after StartDate to process. If null then do every day available.")]
            public int? NumberOfDays { get; set; }

            //[ArgDescription("User specified file containing a list of indices and their properties.")]
            //[Production.ArgExistingFile(Extension = ".yml")]
            //[ArgPosition(1)]
            //public FileInfo IndexPropertiesConfig { get; set; }
        }

        /// <summary>
        /// To get to this DEV method, the FIRST AND ONLY command line argument must be "concatenateIndexFiles"
        /// </summary>
        public static Arguments Dev()
        {
            // top level directory
            DirectoryInfo[] dataDirs = { new DirectoryInfo(@"Y:\Results\2015Aug06-123245 - Yvonne, Indices, ICD=60.0, #48"),
                                         new DirectoryInfo(@"Y:\Results\2015Aug20-154235 - Yvonne, Indices, ICD=60.0, #50") };

            string opPath = @"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults";

            // The recording siteName is used as filter pattern 
            // It is also used for naming the output files
            //string siteName = "Woondum3";
            string siteName = "GympieNP";

            var dtoStart = new DateTimeOffset(2015, 6, 22, 0, 0, 0, TimeSpan.Zero);
            int dayCount = 1;


            return new Arguments
            {
                InputDataDirectories = dataDirs,
                OutputDirectory = new DirectoryInfo(opPath),
                SiteName = siteName,
                StartDate = dtoStart,
                NumberOfDays = dayCount
                // use the default set of index properties in the AnalysisConfig directory.
                //IndexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml".ToFileInfo(),
            };
            throw new NoDeveloperMethodException();
    }

        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();

                bool verbose = true; // assume verbose if in dev mode
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
                    LoggedConsole.WriteLine("# Start date = " + arguments.StartDate.ToString());
                    LoggedConsole.WriteLine("# Number of Days = " + arguments.NumberOfDays);
                    LoggedConsole.WriteLine();
                    //LoggedConsole.WriteLine("# Index Properties Config file: " + arguments.IndexPropertiesConfig);
                }
            }

            string csvFileExt = "csv";
            string indexType = "SummaryIndices";
            bool drawImages = true;

            // create top level output directory if it does not exist.
            DirectoryInfo opDir = arguments.OutputDirectory;
            if (!opDir.Exists) arguments.OutputDirectory.Create();

            // The following location data is used only to draw the sunrise/sunset tracks.
            double? latitude = null;
            double? longitude = null;
            var siteDescription = new SiteDescription();
            siteDescription.SiteName = arguments.SiteName;
            siteDescription.Latitude = latitude;
            siteDescription.Longitude = longitude;

            int dayCount = 1000; // a large number
            if(arguments.NumberOfDays != null)
                dayCount = (int)arguments.NumberOfDays;

            // assume StartDate not null TODO!!
            //if (arguments.StartDate == null)

            // loop over days
            for (int d = 0; d < dayCount; d++)
            {
                // assumes DateTimeOffset not = null!! TODO 
                var thisday = ((DateTimeOffset)arguments.StartDate).AddDays(d);
                LoggedConsole.WriteLine("\n\n\nCONCATENATING DAY: " + thisday.ToString());

                // CREATE DAY LEVEL OUTPUT DIRECTORY for this day
                string dateString = String.Format("{0}{1:D2}{2:D2}", thisday.Year, thisday.Month, thisday.Day);
                DirectoryInfo resultsDir = new DirectoryInfo(Path.Combine(opDir.FullName, arguments.SiteName, dateString));
                if (!resultsDir.Exists) resultsDir.Create();

                FileInfo[] files = LDSpectrogramStitching.GetSummaryIndexFilesForOneDay(arguments.InputDataDirectories, arguments.SiteName, thisday);
                if (files.Length == 0)
                {
                    LoggedConsole.WriteErrorLine("WARNING from method ConcatenateIndexFiles.Execute():");
                    LoggedConsole.WriteErrorLine("        No files of SUMMARY indices were found !!! ");
                    break;
                }

                string opFileStem = String.Format("{0}_{1}", arguments.SiteName, dateString);
                var indicesFile = FilenameHelpers.AnalysisResultName(opDir, opFileStem, indexType, csvFileExt);
                var indicesCsvfile = new FileInfo(indicesFile);

                var summaryDict = LDSpectrogramStitching.ConcatenateSummaryIndexFiles(files, resultsDir, indicesCsvfile);

                if (summaryDict.Count == 0)
                {
                    LoggedConsole.WriteErrorLine("WARNING from method ConcatenateIndexFiles.Execute():");
                    LoggedConsole.WriteErrorLine("        An empty dictionary of SUMMARY indices was returned !!! ");
                    break;
                }

                // DRAW SUMMARY INDEX IMAGES AND SAVE IN RESULTS DIRECTORY
                // get the IndexGenerationData file from the first directory
                IndexGenerationData indexGenerationData = IndexGenerationData.GetIndexGenerationData(arguments.InputDataDirectories[0]);
                indexGenerationData.RecordingStartDate = thisday;
                if (drawImages)
                {
                    var indexPropertiesConfig = new FileInfo(Path.Combine(opDir.FullName, "IndexPropertiesConfig.yml"));
                    LDSpectrogramStitching.DrawSummaryIndexFiles(summaryDict, indexGenerationData, indexPropertiesConfig, resultsDir, siteDescription);
                }

                // ##############################################################################################################

                // NOW CONCATENATE SPECTRAL INDEX FILES
                var spectralDict = LDSpectrogramStitching.ConcatenateSpectralIndexFilesForOneDay(arguments.InputDataDirectories, resultsDir, arguments.SiteName, thisday);

                if (spectralDict.Count == 0)
                {
                    LoggedConsole.WriteErrorLine("WARNING from method ConcatenateIndexFiles.Execute():");
                    LoggedConsole.WriteErrorLine("        An empty dictionary of SPECTRAL indices was returned !!! ");
                    return;
                }

                // DRAW SPECTRAL INDEX IMAGES AND SAVE IN RESULTS DIRECTORY
                if (drawImages)
                {
                    var indexPropertiesConfig = new FileInfo(Path.Combine(opDir.FullName, "IndexPropertiesConfig.yml"));
                    LDSpectrogramStitching.DrawSpectralIndexFiles(spectralDict, indexGenerationData, indexPropertiesConfig, resultsDir, siteDescription);
                }

            } // over days

        } // Execute()
    }
}
