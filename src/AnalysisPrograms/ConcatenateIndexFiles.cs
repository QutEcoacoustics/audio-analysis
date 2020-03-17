// <copyright file="ConcatenateIndexFiles.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

// Defines the ConcatenateIndexFiles type.
// Action code for this activity = "concatenateIndexFiles"

// Activity Codes for other tasks to do with spectrograms and audio files:
// audio2csv - Calls AnalyseLongRecording.Execute(): Outputs acoustic indices and LD false-color spectrograms.
// audio2sonogram - Calls AnalysisPrograms.Audio2Sonogram.Main(): Produces a sonogram from an audio file - EITHER custom OR via SOX.Generates multiple spectrogram images and oscillations info
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

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using SixLabors.ImageSharp;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Csv;
    using Acoustics.Shared.ImageSharp;
    using AnalysisPrograms.Production;
    using AnalysisPrograms.Production.Arguments;
    using AnalysisPrograms.Production.Validation;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.StandardSpectrograms;
    using log4net;
    using McMaster.Extensions.CommandLineUtils;
    using Production;
    using Production.Arguments;
    using Production.Validation;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;
    using Acoustics.Shared.Contracts;
    using Path = System.IO.Path;

    /// <summary>
    /// First argument on command line to call this action is "concatenateIndexFiles"
    ///
    /// NOTE: This code was last tested on 2016 October 10. Both tests passed.
    /// </summary>
    public static class ConcatenateIndexFiles
    {
        public const string CommandName = "ConcatenateIndexFiles";
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Command(
            CommandName,
            Description = "[BETA] Concatenates multiple consecutive AcousticIndex.csv files. Caution required!!")]
        public class Arguments : SubCommandBase
        {
            [Argument(
                0,
                Description = "One or more directories where the original csv files are located.")]
            public DirectoryInfo[] InputDataDirectories { get; set; }

            [Obsolete("Originally hack to get around power-args limitation, can probably be removed soon")]
            [Option(
                CommandOptionType.SingleValue,
                Description = "One directory where the original csv files are located. This option exists as an alternative to input data directories")]
            public DirectoryInfo InputDataDirectory { get; set; }

            [Option(
                CommandOptionType.SingleValue,
                Description = "Directory where the output is to go.")]
            [DirectoryExistsOrCreate(createIfNotExists: true)]
            [LegalFilePath]
            public DirectoryInfo OutputDirectory { get; set; }

            [Option(Description =
                "Used to get the required data.csv files, which are assumed to be in a matching dir or sub-directory. E.g. use name of audio file suffix e.g.: `*.wav`. The default is `*.wav`")]
            public string DirectoryFilter { get; set; } = "*.wav";

            [Option(
                CommandOptionType.SingleValue,
                Description = "File stem name for output files.")]
            public string FileStemName { get; set; }

            [Option(
                CommandOptionType.SingleValue,
                Description = "DateTimeOffset (inclusive) at which concatenation begins. If null, then start with earliest available file. Can parse an ISO8601 date.")]
            public DateTimeOffset? StartDate { get; set; }

            [Option(
                CommandOptionType.SingleValue,
                Description = "DateTimeOffset (exclusive) at which concatenation ends. If null, then will be set = today's date or last available file. Can parse an ISO8601 date.")]
            public DateTimeOffset? EndDate { get; set; }

            [Option(
                CommandOptionType.SingleValue,
                Description = "TimeSpan offset hint required if file names do not contain time zone info. NO DEFAULT IS SET",
                ShortName = "z")]
            public TimeSpan? TimeSpanOffsetHint { get; set; }

            [Option(
                CommandOptionType.NoValue,
                Description = "Draw false-color spectrograms after concatenating index files",
                ShortName = "")]
            public bool DrawImages { get; set; } = true;

            [Option(
                Description = "The mapping of indices to color channel in false-color spectrogram 1",
                ShortName = "")]
            public string ColorMap1 { get; set; }

            [Option(
                Description = "The mapping of indices to color channel in false-color spectrogram 2",
                ShortName = "")]
            public string ColorMap2 { get; set; }

            [Option(
                Description = "User specified file containing a list of indices and their properties.",
                ShortName = "ip")]
            [ExistingFile(Extension = ".yml")]
            [LegalFilePath]
            public string IndexPropertiesConfig { get; set; }

            [Option(
                Description = "Config file for drawing the false colour spectrograms.",
                ShortName = "fcs")]
            [ExistingFile(Extension = ".yml")]
            [LegalFilePath]
            public string FalseColourSpectrogramConfig { get; set; }

            [Option(
                CommandOptionType.NoValue,
                Description = "Set true only when concatenating more than 24-hours of data into one image",
                LongName = "concatenate-everything",
                ShortName = "")]
            public bool ConcatenateEverythingYouCanLayYourHandsOn { get; set; }

            [Option(Description = "How to render gaps in a recording. Valid options: `" + nameof(ConcatMode.TimedGaps) + "` (default), `" + nameof(ConcatMode.NoGaps) + "`, `" + nameof(ConcatMode.EchoGaps) + "`")]
            public ConcatMode GapRendering { get; set; }

            [Option(
                Description = "One or more directories where the RECOGNIZER event scores are located in csv files. This is optional",
                ShortName = "")]
            public string[] EventDataDirectories { get; set; }

            [Option(Description = "Used only to get Event Recognizer files.", ShortName = "")]
            public string EventFilePattern { get; set; }

            public override Task<int> Execute(CommandLineApplication app)
            {
                ConcatenateIndexFiles.Execute(this);
                return this.Ok();
            }
        }

        /// <summary>
        /// Concatenation is designed only for the output from a "Towsey.Acoustic" analysis.
        /// </summary>
        public static void Execute(Arguments arguments)
        {
            // Get the currently available spectral indices
            string[] keys = LDSpectrogramRGB.GetArrayOfAvailableKeys();

            if (arguments == null)
            {
                throw new NoDeveloperMethodException();
            }

            Log.Warn(@"
!
!   THIS IS A BETA COMMAND.
!   It generally works but only for very narrow scenarios. Your mileage *will* vary.
!
!   DO NOT USE THE OUTPUT INDICES FOR QUANTITATIVE ANALYSIS.
!");

            if (arguments.InputDataDirectory != null)
            {
                arguments.InputDataDirectories =
                    (arguments.InputDataDirectories ?? new DirectoryInfo[0]).Concat(new[] { arguments.InputDataDirectory }).ToArray();
            }

            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine("# CONCATENATES CSV FILES CONTAINING SPECTRAL and SUMMARY INDICES.");
            LoggedConsole.WriteLine("#    IT IS ASSUMED THESE WERE OBTAINED FROM MULTIPLE SHORT AUDIO RECORDINGs");
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Index.csv files are in directories:");
            var inputDirs = arguments.InputDataDirectories;
            foreach (var dir in inputDirs)
            {
                LoggedConsole.WriteLine("    {0}", dir.FullName);
            }

            var output = arguments.OutputDirectory;
            LoggedConsole.WriteLine("# Output directory: " + output.FullName);
            LoggedConsole.WriteLine("# DIRECTORY FILTER = " + arguments.DirectoryFilter);
            LoggedConsole.WriteLine();

            //LoggedConsole.WriteLine("# Index Properties Config file: " + arguments.IndexPropertiesConfig);

            // 1. PATTERN SEARCH FOR CORRECT SUBDIRECTORIES
            // Assumes that the required subdirectories have the given FILTER/SiteName somewhere in their path.
            var searchOption = SearchOption.AllDirectories;
            //var searchOption = SearchOption.TopDirectoryOnly;
            var subDirectories = LdSpectrogramStitching.GetSubDirectoriesForSiteData(inputDirs, arguments.DirectoryFilter, searchOption);
            if (subDirectories.Length == 0)
            {
                LoggedConsole.WriteErrorLine("\n\n#Error from method ConcatenateIndexFiles.Execute():");
                LoggedConsole.WriteErrorLine("        Subdirectory Count with given filter = ZERO");
                LoggedConsole.WriteErrorLine("        RETURNING EMPTY HANDED!");
                throw new MissingDataException("Could not find any sub directories from input directories:" + inputDirs.FormatList());
            }

            // 2. PATTERN SEARCH FOR SUMMARY INDEX FILES.
            string pattern = "*_Towsey.Acoustic.Indices.csv";
            FileInfo[] csvFiles = IndexMatrices.GetFilesInDirectories(subDirectories, pattern);
            LoggedConsole.WriteLine("# Subdirectories Count = " + subDirectories.Length);
            LoggedConsole.WriteLine("# IndexFiles.csv Count = " + csvFiles.Length);

            if (csvFiles.Length == 0)
            {
                LoggedConsole.WriteErrorLine("\n\nError from method ConcatenateIndexFiles.Execute():");
                LoggedConsole.WriteErrorLine("        No SUMMARY index files were found.");
                LoggedConsole.WriteErrorLine("        RETURNING EMPTY HANDED!");
                throw new MissingDataException($"Could not find any files matching `{pattern}` in:" + subDirectories.FormatList());
            }

            // Sort the files by date and return as a dictionary: sortedDictionaryOfDatesAndFiles<DateTimeOffset, FileInfo>
            var sortedDictionaryOfDatesAndFiles = FileDateHelpers.FilterFilesForDates(csvFiles, arguments.TimeSpanOffsetHint);

            // Set default start and end dates to first and last available dates.
            DateTimeOffset startDate = sortedDictionaryOfDatesAndFiles.Keys.First();
            DateTimeOffset endDate = sortedDictionaryOfDatesAndFiles.Keys.Last();
            if (!arguments.ConcatenateEverythingYouCanLayYourHandsOn)
            {
                // concatenate in 24 hour blocks
                if (arguments.StartDate != null)
                {
                    startDate = arguments.StartDate.Value;
                }

                if (arguments.EndDate != null)
                {
                    endDate = arguments.EndDate.Value;
                }

                if (startDate >= endDate)
                {
                    LoggedConsole.WriteErrorLine("# The End Date must be greater than the Start Date when ConcatenateEverythingYouCanLayYourHandsOn = false.");
                    throw new ArgumentException("FATAL ERROR: End Date must be greater than the Start Date.");
                }
            }

            var startDateTimeOffset = startDate;

            LoggedConsole.WriteLine("\n# Start date = " + startDate.ToString());
            LoggedConsole.WriteLine("# End   date = " + endDate.ToString());
            LoggedConsole.WriteLine("# Time Zone  = " + arguments.TimeSpanOffsetHint.ToString());

            LoggedConsole.WriteLine("# WARNING: A sunrise/sunset data file does not exist for time zone >> " + arguments.TimeSpanOffsetHint.ToString());

            // create top level output directory if it does not exist.
            DirectoryInfo opDir = output;
            if (!opDir.Exists)
            {
                output.Create();
            }

            if (arguments.FileStemName.IsNullOrEmpty())
            {
                arguments.FileStemName = arguments.InputDataDirectories.First().Name;
                Log.Warn($"FileStemName was empty had a default value of `{arguments.FileStemName}` was used");
            }

            string outputFileStem = arguments.FileStemName;

            // SET UP DEFAULT SITE LOCATION INFO --  DISCUSS IWTH ANTHONY
            // The following location data is used only to draw the sunrise/sunset tracks on images.
            // But sun tracks now depracated.
            double? latitude = null;
            double? longitude = null;
            var siteDescription = new SiteDescription
            {
                SiteName = arguments.FileStemName,
                Latitude = latitude,
                Longitude = longitude,
            };

            // the following are required if drawing the index images
            IndexGenerationData indexGenerationData = null;
            FileInfo indexPropertiesConfig;
            if (arguments.IndexPropertiesConfig.IsNullOrEmpty())
            {
                indexPropertiesConfig = ConfigFile.Default<IndexPropertiesCollection>();
                Log.Warn($"IndexPropertiesConfig file not provided, using default: {indexPropertiesConfig}");
            }
            else
            {
                indexPropertiesConfig = ConfigFile.Resolve(arguments.IndexPropertiesConfig.ToString());
            }

            FileInfo ldSpectrogramConfigFile = null;
            LdSpectrogramConfig ldSpectrogramConfig = null;
            if (arguments.FalseColourSpectrogramConfig.IsNullOrEmpty())
            {
                ldSpectrogramConfigFile = ConfigFile.Default<LdSpectrogramConfig>();
                Log.Warn($"FalseColourSpectrogramConfig file not provided, using default: {ldSpectrogramConfig}");
            }
            else
            {
                ldSpectrogramConfigFile = ConfigFile.Resolve(arguments.FalseColourSpectrogramConfig.ToString());
            }

            if (arguments.DrawImages)
            {
                // get the IndexGenerationData file from the first directory
                indexGenerationData = IndexGenerationData.GetIndexGenerationData(csvFiles[0].Directory);
                if (indexGenerationData.RecordingStartDate == null)
                {
                    indexGenerationData.RecordingStartDate = startDate;
                }

                indexPropertiesConfig = arguments.IndexPropertiesConfig.ToFileInfo();

                // prepare the LDFC spgm config file or set up a default config
                // WARNING: This default config is used when testing. If you alter these defaults, Unit Test results may be affected.
                ldSpectrogramConfig = (ldSpectrogramConfigFile?.Exists ?? false)
                    ? LdSpectrogramConfig.ReadYamlToConfig(ldSpectrogramConfigFile)
                    : new LdSpectrogramConfig();

                // the user should have provided ColorMap arguments which we insert here
                if (arguments.ColorMap1.NotNull())
                {
                    ldSpectrogramConfig.ColorMap1 = arguments.ColorMap1;
                }

                if (arguments.ColorMap2.NotNull())
                {
                    ldSpectrogramConfig.ColorMap2 = arguments.ColorMap2;
                }
            }

            // ################################ ConcatenateEverythingYouCanLayYourHandsOn = true
            DirectoryInfo resultsDir;
            if (arguments.ConcatenateEverythingYouCanLayYourHandsOn)
            {
                var totalTimespan = endDate - startDate;
                LoggedConsole.WriteLine("# Total duration of available recording = " + totalTimespan.ToString());

                if (totalTimespan > TimeSpan.FromDays(3))
                {
                    LoggedConsole.WriteErrorLine("# WARNING: You are attempting to concatenate MORE THAN three days of recording!!!!");
                    LoggedConsole.WriteErrorLine("# WARNING: This is not a good idea!!!!!");
                }

                string dateString = $"{startDateTimeOffset.Year}{startDateTimeOffset.Month:D2}{startDateTimeOffset.Day:D2}";
                resultsDir = new DirectoryInfo(Path.Combine(opDir.FullName, arguments.FileStemName, dateString));
                if (!resultsDir.Exists)
                {
                    resultsDir.Create();
                }

                // ###### FIRST CONCATENATE THE SUMMARY INDICES, DRAW IMAGES AND SAVE IN RESULTS DIRECTORY
                var summaryIndexFiles = sortedDictionaryOfDatesAndFiles.Values.ToArray<FileInfo>();

                var concatenatedSummaryIndices = LdSpectrogramStitching.ConcatenateAllSummaryIndexFiles(summaryIndexFiles, resultsDir, indexGenerationData, outputFileStem);
                WriteSummaryIndexFile(resultsDir, outputFileStem, AcousticIndices.TowseyAcoustic, concatenatedSummaryIndices);

                // WARNING: call to this method only returns a fixed list of indices.
                var dictionaryOfSummaryIndices = SummaryIndexValues.ConvertToDictionaryOfSummaryIndices(concatenatedSummaryIndices);

                // REALITY CHECK - check for continuous zero indices or anything else that might indicate defective signal,
                //                 incomplete analysis of recordings, recording gaps or file joins.
                var gapsAndJoins = GapsAndJoins.DataIntegrityCheck(concatenatedSummaryIndices, arguments.GapRendering);
                GapsAndJoins.WriteErrorsToFile(gapsAndJoins, resultsDir, outputFileStem);

                if (arguments.DrawImages)
                {
                    TimeSpan start = ((DateTimeOffset)indexGenerationData.RecordingStartDate).TimeOfDay;
                    string startTime = $"{start.Hours:d2}{start.Minutes:d2}h";
                    string imageTitle = $"SOURCE: \"{outputFileStem}\".     Starts at {startTime}                       {Meta.OrganizationTag}";
                    Image<Rgb24> tracksImage = IndexDisplay.DrawImageOfSummaryIndices(
                            IndexProperties.GetIndexProperties(indexPropertiesConfig),
                            dictionaryOfSummaryIndices,
                            imageTitle,
                            indexGenerationData.IndexCalculationDuration,
                            indexGenerationData.RecordingStartDate,
                            gapsAndJoins);

                    var imagePath = FilenameHelpers.AnalysisResultPath(resultsDir, outputFileStem, "SummaryIndices", "png");
                    tracksImage.Save(imagePath);
                }

                LoggedConsole.WriteLine("# Finished summary indices. Now start spectral indices.");

                // ###### NOW CONCATENATE THE SPECTRAL INDICES, DRAW IMAGES AND SAVE IN RESULTS DIRECTORY
                var dictionaryOfSpectralIndices1 = LdSpectrogramStitching.ConcatenateAllSpectralIndexFiles(subDirectories, keys, indexGenerationData);
                gapsAndJoins.AddRange(GapsAndJoins.DataIntegrityCheck(dictionaryOfSpectralIndices1, arguments.GapRendering));

                // Calculate the index distribution statistics and write to a json file. Also save as png image
                var indexDistributions = IndexDistributions.WriteSpectralIndexDistributionStatistics(dictionaryOfSpectralIndices1, resultsDir, outputFileStem);

                if (arguments.DrawImages)
                {
                    Tuple<Image<Rgb24>, string>[] tuple = LDSpectrogramRGB.DrawSpectrogramsFromSpectralIndices(
                            subDirectories[0],
                            resultsDir,
                            ldSpectrogramConfig,
                            indexPropertiesConfig,
                            indexGenerationData,
                            outputFileStem,
                            AcousticIndices.TowseyAcoustic,
                            dictionaryOfSpectralIndices1,
                            /*summaryIndices = */null,
                            indexDistributions,
                            siteDescription,
                            sunriseDataFile: null,
                            segmentErrors: gapsAndJoins,
                            imageChrome: ImageChrome.With);
                }

                WriteSpectralIndexFiles(resultsDir, outputFileStem, AcousticIndices.TowseyAcoustic, dictionaryOfSpectralIndices1);
                return;
            }

            // ################################ ConcatenateEverythingYouCanLayYourHandsOn = false
            // ################################ That is, CONCATENATE DATA in BLOCKS of 24 hours

            var startDateOffset = startDateTimeOffset.Floor(TimeSpan.FromDays(1));
            var endDateOffset = endDate.Ceiling(TimeSpan.FromDays(1));
            int dayCount = (int)Math.Ceiling((endDateOffset - startDateOffset).TotalDays);
            LoggedConsole.WriteLine("# Day  count = " + dayCount);
            /* Previously used the following line BUT the assumption proved to be a bug, not a feature.
            // int dayCount = timespan.Days + 1; // This assumes that the last day has full 24 hours of recording available.
            // LoggedConsole.WriteLine($"# Elapsed time = {totalTimespan.TotalHours:f1} hours or {dayCount} days");
            */

            // loop over days
            for (int d = 0; d < dayCount; d++)
            {
                var thisday = startDateOffset.AddDays(d);
                LoggedConsole.WriteLine($"\n\n\nCONCATENATING DAY {d + 1} of {dayCount}:   {thisday}");

                FileInfo[] indexFiles = LdSpectrogramStitching.GetFileArrayForOneDay(sortedDictionaryOfDatesAndFiles, thisday);
                if (indexFiles.Length == 0)
                {
                    LoggedConsole.WriteErrorLine("\n\nWARNING from method ConcatenateIndexFiles.Execute():");
                    LoggedConsole.WriteErrorLine($"        No files of SUMMARY indices were found for day {thisday}. Ignore this day.");
                    continue;
                }

                // CREATE DAY LEVEL OUTPUT DIRECTORY for this day
                string format = "yyyyMMdd";
                string dateString = thisday.ToString(format);
                resultsDir = new DirectoryInfo(Path.Combine(opDir.FullName, arguments.FileStemName, dateString));
                if (!resultsDir.Exists)
                {
                    resultsDir.Create();
                }

                var outputBaseName = $"{arguments.FileStemName}_{dateString}";

                // Recalculate <thisDay> to include the start time - not just the date. This is for time scale on false-colour spectrograms.
                if (FileDateHelpers.FileNameContainsDateTime(indexFiles[0].Name, out var dt, arguments.TimeSpanOffsetHint))
                {
                    thisday = dt;
                } // else <thisday> will not contain the start time of the day.

                // CONCATENATE the SUMMARY INDEX FILES
                var concatenatedSummaryIndices = LdSpectrogramStitching.ConcatenateAllSummaryIndexFiles(indexFiles, resultsDir, indexGenerationData, outputBaseName);
                WriteSummaryIndexFile(resultsDir, outputBaseName, AcousticIndices.TowseyAcoustic, concatenatedSummaryIndices);

                // WARNING: call to this method only returns a fixed list of indices.
                var summaryDict = SummaryIndexValues.ConvertToDictionaryOfSummaryIndices(concatenatedSummaryIndices);

                if (summaryDict == null)
                {
                    continue;
                }

                // REALITY CHECK - check for zero signal and anything else that might indicate defective signal
                List<GapsAndJoins> indexErrors = GapsAndJoins.DataIntegrityCheck(concatenatedSummaryIndices, arguments.GapRendering);
                GapsAndJoins.WriteErrorsToFile(indexErrors, resultsDir, outputBaseName);

                // DRAW SUMMARY INDEX IMAGES AND SAVE IN RESULTS DIRECTORY
                if (arguments.DrawImages)
                {
                    indexGenerationData.RecordingStartDate = thisday;
                    LdSpectrogramStitching.DrawSummaryIndexFiles(
                        summaryDict,
                        indexGenerationData,
                        indexPropertiesConfig,
                        resultsDir,
                        siteDescription,
                        sunriseDatafile: null,
                        erroneousSegments: indexErrors);
                }

                LoggedConsole.WriteLine("# Finished summary indices. Now start spectral indices.");

                // ##############################################################################################################

                // NOW CONCATENATE SPECTRAL INDEX FILES
                //Filter the array of Directories to get the correct dates
                var spectralSubdirectories = FileDateHelpers.FilterDirectoriesForDates(subDirectories, arguments.TimeSpanOffsetHint);
                var dirArray = LdSpectrogramStitching.GetDirectoryArrayForOneDay(spectralSubdirectories, thisday);

                if (dirArray.Length == 0)
                {
                    LoggedConsole.WriteErrorLine("\n\nWARNING from method ConcatenateIndexFiles.Execute():");
                    LoggedConsole.WriteErrorLine($"        No directories of Spectral indices were found for {thisday}.");
                    continue;
                }

                var dictionaryOfSpectralIndices2 = LdSpectrogramStitching.ConcatenateAllSpectralIndexFiles(dirArray, keys, indexGenerationData);
                if (dictionaryOfSpectralIndices2.Count == 0)
                {
                    LoggedConsole.WriteErrorLine("WARNING from method ConcatenateIndexFiles.Execute():");
                    LoggedConsole.WriteErrorLine("        An empty dictionary of SPECTRAL indices was returned !!! ");
                    continue;
                }

                // Calculate the index distribution statistics and write to a json file. Also save as png image
                var indexDistributions = IndexDistributions.WriteSpectralIndexDistributionStatistics(dictionaryOfSpectralIndices2, resultsDir, outputBaseName);

                // DRAW SPECTRAL INDEX IMAGES AND SAVE IN RESULTS DIRECTORY
                if (arguments.DrawImages)
                {
                    Tuple<Image<Rgb24>, string>[] tuple = LDSpectrogramRGB.DrawSpectrogramsFromSpectralIndices(
                        subDirectories[0],
                        resultsDir,
                        ldSpectrogramConfig,
                        indexPropertiesConfig,
                        indexGenerationData,
                        outputBaseName,
                        AcousticIndices.TowseyAcoustic,
                        dictionaryOfSpectralIndices2,
                        /*summaryIndices = */null,
                        indexDistributions,
                        siteDescription,
                        sunriseDataFile: null,
                        segmentErrors: indexErrors,
                        imageChrome: ImageChrome.With);

                    if (!arguments.EventDataDirectories.IsNullOrEmpty())
                    {
                        var candidateFiles = IndexMatrices.GetFilesInDirectories(arguments.EventDataDirectories.Select(FileInfoExtensions.ToDirectoryInfo).ToArray(), arguments.EventFilePattern);
                        var sortedDictionaryOfEventFiles = FileDateHelpers.FilterFilesForDates(candidateFiles, arguments.TimeSpanOffsetHint);
                        var eventFiles = LdSpectrogramStitching.GetFileArrayForOneDay(sortedDictionaryOfEventFiles, thisday);

                        //int lineCount = 0;
                        var output2 = new List<string>();
                        foreach (var file in eventFiles)
                        {
                            var lines = FileTools.ReadTextFile(file.FullName);
                            lines.RemoveAt(0); // ignore header
                            output2.AddRange(lines);

                            //lineCount += lines.Count;
                            //Console.WriteLine($"  # events = {lines.Count}");
                        }

                        var indexArray = ConvertEventsToSummaryIndices(output2);

                        DataTools.Normalise(indexArray, 2, out var normalisedScores, out var normalisedThreshold);

                        //var plot = new Plot("Cane Toad", normalisedScores, normalisedThreshold);
                        var recognizerTrack = GraphsAndCharts.DrawGraph("Canetoad events", normalisedScores, 32);
                        var imageFilePath = Path.Combine(resultsDir.FullName, outputFileStem + "_" + dateString + "__2Maps" + ".png");
                        var twoMaps = Image.Load<Rgb24>(imageFilePath);
                        var imageList = new [] { twoMaps, recognizerTrack };
                        var compositeBmp = (Image<Rgb24>)ImageTools.CombineImagesVertically(imageList);
                        var imagePath2 = Path.Combine(resultsDir.FullName, outputFileStem + "_" + dateString + ".png");
                        compositeBmp.Save(imagePath2);
                    }
                }

                WriteSpectralIndexFiles(resultsDir, outputBaseName, AcousticIndices.TowseyAcoustic, dictionaryOfSpectralIndices2);
                LoggedConsole.WriteLine("     Completed Spectral Indices");
            } // over days
        } // Execute()

        public static List<FileInfo> WriteSpectralIndexFiles(DirectoryInfo destination, string fileNameBase, string identifier, Dictionary<string, double[,]> results)
        {
            Log.Info("\t\tWriting spectral indices");

            var spectralIndexFiles = new List<FileInfo>(results.Count);

            foreach (var kvp in results)
            {
                // write spectrogram to disk as CSV file
                var filename = FilenameHelpers.AnalysisResultPath(destination, fileNameBase, identifier + "." + kvp.Key, "csv").ToFileInfo();
                spectralIndexFiles.Add(filename);
                Csv.WriteMatrixToCsv(filename, kvp.Value, TwoDimensionalArray.Rotate90ClockWise);
            }

            Log.Debug("Finished writing spectral indices");
            return spectralIndexFiles;
        }

        public static void WriteSummaryIndexFile(DirectoryInfo destination, string baseName, string identifier, IEnumerable<SummaryIndexValues> summaryIndices)
        {
            var indicesFile = FilenameHelpers.AnalysisResultPath(destination, baseName, identifier + "." + FilenameHelpers.StandardIndicesSuffix, "csv");
            var indicesCsvfile = new FileInfo(indicesFile);
            Csv.WriteToCsv(indicesCsvfile, summaryIndices);
        }

        /// <summary>
        /// This method is designed only to read in Spectrogram ribbons for Georgia marine recordings.
        /// Used to prepare images for Aaron Rice.
        /// </summary>
        public static void ConcatenateRibbonImages(DirectoryInfo[] dataDirs, string pattern, DirectoryInfo outputDirectory, string opFileStem, string title, SunAndMoon.SunMoonTides[] tidalInfo = null)
        {
            //get the ribon files
            FileInfo[] imageFiles = IndexMatrices.GetFilesInDirectories(dataDirs, pattern);

            DateTimeOffset dto = new DateTimeOffset(2013, 3, 1, 0, 0, 0, TimeSpan.Zero);
            TimeSpan oneday = new TimeSpan(24, 0, 0);

            var image = Image.Load<Rgb24>(imageFiles[0].FullName);

            int imageHt = image.Height;
            int imageCount = imageFiles.Length;
            var spacer = new Image<Rgb24>(image.Width, 1);
            spacer.Mutate(canvas => { canvas.Clear(Color.Gray); });

            // add ribbon files to list
            var imageList = new List<Image<Rgb24>>();
            foreach (FileInfo imageFile in imageFiles)
            {
                image = Image.Load<Rgb24>(imageFile.FullName);

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
            var compositeBmp = (Image<Rgb24>)ImageTools.CombineImagesVertically(imageList);

            // create left side day scale
            var stringFont = Drawing.Arial16;
            imageList = new List<Image<Rgb24>>();

            for (int i = 0; i < imageCount; i++)
            {
                image = new Image<Rgb24>(60, imageHt);
                image.Mutate(canvas =>
                {
                    var str = $"{i + 1}";
                    canvas.DrawText(str, stringFont, Color.White, new PointF(3, 3));
                });

                imageList.Add(image);
                imageList.Add(spacer);
            }

            //create composite image
            var compositeBmpYscale = (Image<Rgb24>)ImageTools.CombineImagesVertically(imageList);
            var finalImages = new [] { compositeBmpYscale, compositeBmp, compositeBmpYscale };
            var finalComposite = (Image<Rgb24>)ImageTools.CombineImagesInLine(finalImages);

            // add title bar
            var titleBmp = new Image<Rgb24>(finalComposite.Width, 30);
            titleBmp.Mutate(canvas => { canvas.DrawText(title, stringFont, Color.White, new PointF(30, 3)); });

            // add title plus spacer
            spacer = new Image<Rgb24>(finalComposite.Width, 3);
            spacer.Mutate(canvas =>
            {
                canvas.Clear(Color.Gray);
            });

            var titledImages = new []{ titleBmp, spacer, finalComposite };
            finalComposite = (Image<Rgb24>)ImageTools.CombineImagesVertically(titledImages);

            finalComposite.Save(Path.Combine(outputDirectory.FullName, opFileStem + ".png"));
            Console.WriteLine($"Final compositeBmp dimensions are width {compositeBmp.Width} by height {compositeBmp.Height}");
            Console.WriteLine($"Final number of ribbons/days = {imageFiles.Length}");
        } //ConcatenateRibbonImages

        public static void AddTidalInfo(Image<Rgb24> image, SunAndMoon.SunMoonTides[] tidalInfo, DateTimeOffset dto)
        {
            var yellowPen = new Pen(Color.Yellow, 1);
            var cyanPen = new Pen(Color.Lime, 2);
            var whitePen = new Pen(Color.White, 2);
            image.Mutate(spgCanvas =>
            {

                foreach (SunAndMoon.SunMoonTides smt in tidalInfo)
                {
                    if (smt.Date == dto)
                    {
                        foreach (KeyValuePair<string, DateTimeOffset> kvp in smt.dictionary)
                        {
                            string key = kvp.Key;
                            DateTimeOffset dto2 = kvp.Value;
                            var thisPen = yellowPen;
                            if (key == SunAndMoon.SunMoonTides.HIGHTIDE)
                            {
                                thisPen = cyanPen;
                            }
                            else if (key == SunAndMoon.SunMoonTides.LOWTIDE)
                            {
                                thisPen = whitePen;
                            }

                            int minute =
                                (int)Math.Round(dto2.TimeOfDay.TotalMinutes *
                                                2); //IMPORTANT multiply by 2 because scale = 30s/px.
                            spgCanvas.DrawLine(thisPen, minute, 0, minute, image.Height);
                        }
                    }
                }
            });
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

            var image = GraphsAndCharts.DrawGraph("Canetoad events", indexArray, 100);

            string title = $"Canetoad events: {opFileStem}                       Max value={maxValue:f0}";
            var titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, indexArray.Length);

            string firstFileName = csvFiles[0].Name;
            DateTimeOffset startTime = DataTools.Time_ConvertDateString2DateTime(firstFileName);
            var duration = new TimeSpan(0, indexArray.Length, 0);

            int trackHeight = 20;
            Image<Rgb24> timeBmp1 = ImageTrack.DrawTimeRelativeTrack(duration, indexArray.Length, trackHeight);
            Image<Rgb24> timeBmp2 = ImageTrack.DrawTimeTrack(duration, startTime, indexArray.Length, trackHeight);

            var imageList = new [] { titleBar, timeBmp1, image, timeBmp2 };
            var compositeBmp = (Image<Rgb24>)ImageTools.CombineImagesVertically(imageList);

            string imagePath = Path.Combine(outputDirectory.FullName, opFileStem + ".png");
            compositeBmp.Save(imagePath);
        }

        public static double[] ConvertEventsToSummaryIndices(List<string> events)
        {
            // Assume that each line in events = one event
            // assume one minute resolution for events index
            // TimeSpan? offsetHint = new TimeSpan(9, 0, 0);
            // int unitTime = 60; // one minute resolution

            // get start and end time from first and last file name
            string line = events[0];
            string[] fields = line.Split(',');
            string fstartFileName = fields[14];

            //string[] times = fileName.Split('_');
            //string startOffset = fields[15];
            DateTimeOffset startTime = DataTools.Time_ConvertDateString2DateTime(fstartFileName);

            // get last event
            line = events[events.Count - 1];
            fields = line.Split(',');
            string endFileName = fields[14];
            DateTimeOffset endTime = DataTools.Time_ConvertDateString2DateTime(endFileName);

            string[] parts = endFileName.Split('_');
            int addOn = int.Parse(parts[2].Substring(0, parts[2].Length - 3));

            // get duration whole minutes
            TimeSpan duration = endTime - startTime;
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
                eventsPerUnitTime[minuteId]++;

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
                    AnalysisIdealSegmentDuration = absolute ? unitTime : duration
                };

                indices[i] = newIndex;
            }
            */

            //return indices;
            return eventsPerUnitTime;
        }

        /// <summary>
        /// Test data derived from ZuZana's INDONESIAN RECORDINGS, recording site 2. Obtained July 2016.
        /// This tests concatenation when ConcatenateEverythingYouCanLayYourHandsOn = true
        /// This test was set up October 2016. The test was transfered to this separate TESTMETHOD in April 2017.
        /// </summary>
        public static void TESTMETHOD_ConcatenateIndexFilesTest1()
        {
            // Set the drive: work = G; home = E
            string drive = "G";

            // top level directory
            var dataDirs = $"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Data\\Indonesia_2\\";

            var outputDir = $"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Test3_Output";

            var falseColourSpgConfig = $"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Data\\ConcatTest_SpectrogramFalseColourConfig.yml";

            var arguments = new Arguments
            {
                InputDataDirectory = dataDirs.ToDirectoryInfo(),
                OutputDirectory = outputDir.ToDirectoryInfo(),
                DirectoryFilter = "*.wav",
                FileStemName = "Indonesia2016",

                // NOTE: When (ConcatenateEverythingYouCanLayYourHandsOn = true), the start and end dates are ignored.
                // However they must be either null or parsible.
                StartDate = new DateTimeOffset(2016, 07, 27, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2016, 07, 25, 0, 0, 0, TimeSpan.Zero),

                IndexPropertiesConfig = $"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Data\\ConcatTest_IndexPropertiesConfig.yml",
                FalseColourSpectrogramConfig = falseColourSpgConfig,
                ColorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN,
                ColorMap2 = "BGN-PMN-SPT", // This color map dates pre-May 2017.
                ConcatenateEverythingYouCanLayYourHandsOn = true,
                TimeSpanOffsetHint = TimeSpan.FromHours(8),
                DrawImages = true,

                // following used to add in a recognizer score track
                EventDataDirectories = null,
                EventFilePattern = null,
            };

            Execute(arguments);

            Log.Success("Completed concatenation test where ConcatenateEverythingYouCanLayYourHandsOn = true");
            Console.WriteLine("\n\n");
        }

        /// <summary>
        /// Test data derived from ZuZana's INDONESIAN RECORDINGS, recording site 2. Obtained July 2016.
        /// TEST 2: Do test of CONCATENATE A 24 hour BLOCK of DATA
        ///         That is, ConcatenateEverythingYouCanLayYourHandsOn = false
        /// This test was set up October 2016. The test was transfered to this separate TESTMETHOD in April 2017.
        /// </summary>
        public static void TESTMETHOD_ConcatenateIndexFilesTest2()
        {
            // Set the drive: work = G; home = E
            string drive = "G";

            // top level directory
            var dataDirs = $"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Data\\Indonesia_2\\";
            var outputDir = $"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Test4_Output";

            // var falseColourSpgConfig = new FileInfo($"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Data\\ConcatTest_SpectrogramFalseColourConfig.yml");
            // if set null will use the default for testing.
            string falseColourSpgConfig = null;

            var arguments = new Arguments
            {
                InputDataDirectory = dataDirs.ToDirectoryInfo(),
                OutputDirectory = outputDir.ToDirectoryInfo(),
                DirectoryFilter = "*.wav",
                FileStemName = "Indonesia2016",
                StartDate = new DateTimeOffset(2016, 07, 25, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2016, 07, 25, 0, 0, 0, TimeSpan.Zero),
                IndexPropertiesConfig = $"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Data\\ConcatTest_IndexPropertiesConfig.yml",
                FalseColourSpectrogramConfig = falseColourSpgConfig,
                ColorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN,
                ColorMap2 = "BGN-PMN-SPT", // This color map dates pre-May 2017.
                ConcatenateEverythingYouCanLayYourHandsOn = false, // 24 hour blocks only
                TimeSpanOffsetHint = TimeSpan.FromHours(8),
                DrawImages = true,

                // following used to add in a recognizer score track
                EventDataDirectories = null,
                EventFilePattern = null,
            };

            Execute(arguments);

            Log.Success("Completed concatenation test where ConcatenateEverythingYouCanLayYourHandsOn = false");
            Console.WriteLine("\n\n");
        }

        /// <summary>
        /// Test data derived from ZuZana's INDONESIAN RECORDINGS, recording site 2. Obtained July 2016.
        /// TEST 3: Do test of CONCATENATE A 24 hour BLOCK of DATA
        ///         That is, ConcatenateEverythingYouCanLayYourHandsOn = false
        /// HOWEVER, NOTE that the start and end dates are set = null.
        /// In this situation the default behaviour is to concatenate the earliest to the last dates found in 24 hour blocks.
        /// This test was set up October 2016. The test was transfered to this separate TESTMETHOD in April 2017.
        /// </summary>
        public static void TESTMETHOD_ConcatenateIndexFilesTest3()
        {
            // Set the drive: work = G; home = E
            string drive = "G";

            // top level directory
            var dataDirs = $"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Data\\Indonesia_2\\";
            var outputDir = @"C:\SensorNetworks\SoftwareTests\TestConcatenation\Test5_Output";
            var falseColourSpgConfig = $"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Data\\ConcatTest_SpectrogramFalseColourConfig.yml";

            var arguments = new Arguments
            {
                InputDataDirectory = dataDirs.ToDirectoryInfo(),
                OutputDirectory = outputDir.ToDirectoryInfo(),
                DirectoryFilter = "*.wav",
                FileStemName = "Indonesia2016",
                StartDate = null,
                EndDate = null,
                IndexPropertiesConfig = $"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Data\\ConcatTest_IndexPropertiesConfig.yml",
                FalseColourSpectrogramConfig = falseColourSpgConfig,
                ColorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN,
                ColorMap2 = "BGN-PMN-SPT", // This color map dates pre-May 2017.
                ConcatenateEverythingYouCanLayYourHandsOn = false, // 24 hour blocks only
                TimeSpanOffsetHint = TimeSpan.FromHours(8),
                DrawImages = true,

                // following used to add in a recognizer score track
                EventDataDirectories = null,
                EventFilePattern = null,
            };

            Execute(arguments);

            Log.Success("Completed concatenation test where ConcatenateEverythingYouCanLayYourHandsOn = false");
            Console.WriteLine("\n\n");
        }

        /// <summary>
        /// Test data derived from ZuZana's INDONESIAN RECORDINGS, recording site 2. Obtained July 2016.
        /// This tests concatenation when ConcatenateEverythingYouCanLayYourHandsOn = true
        /// It works with a reduced data set that will be used for UNIT TESTING, 13th April 2017.
        /// </summary>
        public static void TESTMETHOD_ConcatenateIndexFilesTest4()
        {
            // Set the drive: work = G; home = E
            string drive = "G";

            var zipFile = new FileInfo($"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Data\\Indonesia_2Reduced.zip");
            var dataDir = new DirectoryInfo($"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Data\\Delete");
            ZipFile.ExtractToDirectory(zipFile.FullName, dataDir.FullName);

            // top level directory
            var dataDirs = new[]
            {
                dataDir.FullName + "\\Indonesia_2Reduced",

                //new DirectoryInfo($"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Data\\Indonesia_2Reduced"),
                //new DirectoryInfo($"{drive}:\\Work\\GitHub\\audio-analysis\\Acoustics\\Acoustics.Test\\TestResources\\Concatenation\\Indonesia20160726"),
            };

            var outputDir = $"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Test7_Output";
            var falseColourSpgConfig = $"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Data\\ConcatTest_SpectrogramFalseColourConfig.yml";

            var arguments = new Arguments
            {
                InputDataDirectories = dataDirs.Select(FileInfoExtensions.ToDirectoryInfo).ToArray(),
                OutputDirectory = outputDir.ToDirectoryInfo(),
                DirectoryFilter = "*.wav",
                FileStemName = "Indonesia2016",

                // NOTE: When (ConcatenateEverythingYouCanLayYourHandsOn = true), the start and end dates are ignored.
                // However they must be either null or parsible.
                StartDate = new DateTimeOffset(2016, 07, 27, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2016, 07, 22, 0, 0, 0, TimeSpan.Zero),

                IndexPropertiesConfig = $"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Data\\ConcatTest_IndexPropertiesConfig.yml",
                FalseColourSpectrogramConfig = falseColourSpgConfig,
                ColorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN,
                ColorMap2 = "BGN-PMN-SPT", // This color map dates pre-May 2017.
                ConcatenateEverythingYouCanLayYourHandsOn = true,
                TimeSpanOffsetHint = TimeSpan.FromHours(8),
                DrawImages = true,

                // following used to add in a recognizer score track
                EventDataDirectories = null,
                EventFilePattern = null,
            };

            Execute(arguments);

            Log.Success("Completed Concatenation Test 4 where ConcatenateEverythingYouCanLayYourHandsOn = true");
            Console.WriteLine("\n\n");
        }
    }
}
