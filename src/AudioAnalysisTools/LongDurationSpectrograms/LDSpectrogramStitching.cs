// <copyright file="LDSpectrogramStitching.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools.StandardSpectrograms;
    using Indices;
    using TowseyLibrary;

    /// <summary>
    /// This class used to contain only two methods:  (1) StitchPartialSpectrograms()   and    (2) ConcatenateSpectralIndexFiles()
    /// Now it contains several versions to concatenate Index files. This is because there are now several use cases.
    ///
    ///
    /// Here are the original two methods:
    /// (1) StitchPartialSpectrograms()
    /// This method stitches together images and/or indices derived from a sequence of short recordings with gaps between them.
    /// It was written to deal with a set of recordings with protocol of Gianna Pavan (10 minutes every 30 minutes).
    ///
    /// The following Powershell command was constructed by Anthony to do the analysis and join the sequence of images so derived:
    /// Y:\Italy_GianniPavan\Sassofratino1day | % {& "C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisPrograms\bin\Release\AnalysisPrograms.exe" audio2csv -so ($_.FullName) -o "Y:\Italy_GianniPavan\output" -c "C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.Parallel.yml" }
    /// where:
    ///         Y:\Italy_GianniPavan\Sassofratino1day   is the directory containing recordings
    ///         | = a pipe
    ///         % = foreach{}  = perform the operation in curly brackets on each item piped from the directory.
    ///         & "C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisPrograms\bin\Release\AnalysisPrograms.exe"  = runs an executable
    ///         audio2csv = first command line argument which determines the "activity" performed
    ///         -so ($_.FullName)  = the input file
    ///         -o "Y:\Italy_GianniPavan\output" = the output directory
    ///         -c "PATH\Towsey.Acoustic.Parallel.yml" is the config file
    ///
    /// The following PowerShell command was used by Anthony to stitch together a sequence of spectrogam images without any gap between them.
    /// It requires ImageMagick software to be installed: i.e. C:\Program Files\ImageMagick-6.8.9-Q16\montage.exe
    /// Y:\Italy_GianniPavan\output\Towsey.Acoustic> & "C:\Program Files\ImageMagick-6.8.9-Q16\montage.exe" -mode concatenate -tile x1 *2MAP* "..\..\merge.png"
    ///
    ///
    /// (2) ConcatenateSpectralIndexFiles()
    /// This method was written to deal with a new recording protocol in which 24 hours of recording are made in 4 blocks of 6 hours each.
    /// It merges all files of acoustic indices derived from a sequence of consecutive 6 hour recording, into one file. It then creates the images.
    /// </summary>
    public static class LdSpectrogramStitching
    {
        // CONSTANT STRINGS
        public const string CsvFileExt = "csv";
        public const string ImgFileExt = "png";

        public const string SummaryIndicesStr = "SummaryIndices";
        public const string SpectralIndicesStr = "SpectralIndices";

        public static DirectoryInfo[] GetSubDirectoriesForSiteData(IEnumerable<DirectoryInfo> topLevelDataDirectories, string site)
        {
            //string dateString = String.Format("{0}{1:D2}{2:D2}", dto.Year, dto.Month, dto.Day);
            string searchPattern = "*" + site + "*";

            return topLevelDataDirectories
                .SelectMany(dir => dir.GetDirectories(searchPattern, SearchOption.AllDirectories))
                .ToArray();
        }

        /// <summary>
        /// ONLY Use this concatenation method when you want to concatenate the files for a fixed single day.
        /// The files to be concatenated must be somewhere in the subdirectory structure of the passed list of data directories
        /// Read them into a dictionary
        /// MOST RECENT METHOD TO CONCATENATE Spectral INDEX.CSV FILES - Early September 2015.
        /// It is designed to deal with Yvonne's case where want to concatenate files distributed over arbitrary directories.
        /// It only merges files for the passed fixed date. i.e only 24 hours
        /// </summary>
        public static void DrawSpectralIndexFiles(
            Dictionary<string, double[,]> dictionary,
            LdSpectrogramConfig sgConfig,
            IndexGenerationData indexGenerationData,
            FileInfo indexPropertiesConfigFileInfo,
            DirectoryInfo opDir,
            SiteDescription siteDescription,
            FileInfo sunriseDataFile = null,
            List<GapsAndJoins> segmentErrors = null)
        {
            // derive new indices such as sqrt(POW), NCDI etc -- main reason for this is to view what their distributions look like.
            dictionary = IndexMatrices.AddDerivedIndices(dictionary);

            // Calculate the index distribution statistics and write to a json file. Also save as png image
            if (indexGenerationData.RecordingStartDate != null)
            {
                DateTimeOffset dto = (DateTimeOffset)indexGenerationData.RecordingStartDate;
                string dateString = $"{dto.Year}{dto.Month:D2}{dto.Day:D2}";
                string opFileStem = $"{siteDescription.SiteName}_{dateString}";

                var indexDistributions = IndexDistributions.WriteSpectralIndexDistributionStatistics(dictionary, opDir, opFileStem);

                //SummaryIndexBase[] summaryIndices = null;
                string analysisType = "Towsey.Acoustic";

                Tuple<Image, string>[] tuple = LDSpectrogramRGB.DrawSpectrogramsFromSpectralIndices(
                    opDir, // topLevelDirectories[0], // this should not be required but it is - because things have gotten complicated !
                    opDir,
                    sgConfig,
                    indexPropertiesConfigFileInfo,
                    indexGenerationData,
                    opFileStem,
                    analysisType,
                    dictionary,
                    null, //summaryIndices,
                    indexDistributions,
                    siteDescription,
                    sunriseDataFile,
                    segmentErrors,
                    ImageChrome.With);
            }
        }

        // ####################################  SPECTRAL INDEX METHODS ABOVE HERE  ##################################

        // ####################################  SUMMARY  INDEX METHODS BELOW HERE  ##################################

        public static FileInfo[] GetFileArrayForOneDay(SortedDictionary<DateTimeOffset, FileInfo> dict, DateTimeOffset dto)
        {
            var keys = dict.Keys;
            var matchFiles = new List<FileInfo>();
            foreach (var key in keys)
            {
                if ((dto.Year == key.Year) && (dto.DayOfYear == key.DayOfYear))
                {
                    matchFiles.Add(dict[key]);
                }
            }

            FileInfo[] array = matchFiles.ToArray<FileInfo>();
            return array;
        }

        public static DirectoryInfo[] GetDirectoryArrayForOneDay(SortedDictionary<DateTimeOffset, DirectoryInfo> dict, DateTimeOffset dto)
        {
            var keys = dict.Keys;
            var matchFiles = new List<DirectoryInfo>();
            foreach (var key in keys)
            {
                if ((dto.Year == key.Year) && (dto.DayOfYear == key.DayOfYear))
                {
                    matchFiles.Add(dict[key]);
                }
            }

            DirectoryInfo[] array = matchFiles.ToArray<DirectoryInfo>();
            return array;
        }

        public static void DrawSummaryIndexFiles(
            Dictionary<string, double[]> dictionaryOfCsvColumns,
            IndexGenerationData indexGenerationData,
            FileInfo indexPropertiesConfigFileInfo,
            DirectoryInfo opDir,
            SiteDescription siteDescription,
            FileInfo sunriseDatafile = null,
            List<GapsAndJoins> erroneousSegments = null, // info if have fatal errors i.e. no signal
            bool verbose = false)
        {
            var dto = (DateTimeOffset)indexGenerationData.RecordingStartDate;

            string dateString = $"{dto.Year}{dto.Month:D2}{dto.Day:D2}";
            string opFileStem = $"{siteDescription.SiteName}_{dateString}";

            // Calculate the index distribution statistics and write to a json file. Also save as png image
            var indexDistributions = IndexDistributions.WriteSummaryIndexDistributionStatistics(dictionaryOfCsvColumns, opDir, opFileStem);

            var start = ((DateTimeOffset)indexGenerationData.RecordingStartDate).TimeOfDay;
            string startTime = $"{start.Hours:d2}{start.Minutes:d2}h";
            if (start.Hours == 0 && start.Minutes == 0)
            {
                startTime = "midnight";
            }

            string titletext =
                $"SOURCE: \"{opFileStem}\".     Starts at {startTime}                       {Meta.OrganizationTag}";
            Bitmap tracksImage = IndexDisplay.DrawImageOfSummaryIndices(
                                 IndexProperties.GetIndexProperties(indexPropertiesConfigFileInfo),
                                 dictionaryOfCsvColumns,
                                 titletext,
                                 indexGenerationData.IndexCalculationDuration,
                                 indexGenerationData.RecordingStartDate,
                                 sunriseDatafile,
                                 erroneousSegments,
                                 verbose);
            var imagePath = FilenameHelpers.AnalysisResultPath(opDir, opFileStem, SummaryIndicesStr, ImgFileExt);
            tracksImage.Save(imagePath);
        }

        // ##############################################################################################################
        // ######################### METHODS FOR STITCHING TNC - EDDIE GAME's DATA
        // ######################### CONCATENATE EVERYTHING

        /// <summary>
        /// RECENT METHOD TO CONCATENATE Spectral INDEX.CSV FILES - August 2015. Revised Septermber 2016
        /// Was written to deal with  EDDIE GAME PNG data where the files to be concatenated are all in one top level directory.
        /// This method merges all files of spectral indices in the passed directories.
        /// The total length of the concatenated files can exceed 24 hours - limited by memory!
        /// </summary>
        public static Dictionary<string, double[,]> ConcatenateAllSpectralIndexFiles(DirectoryInfo[] directories, string[] keys, IndexGenerationData indexGenerationData)
        {
            string analysisType = "Towsey.Acoustic";
            var dictionaryOfSpectralIndices = IndexMatrices.GetSpectralIndexFilesAndConcatenate(directories, analysisType, keys, indexGenerationData, true);

            if (dictionaryOfSpectralIndices.Count == 0)
            {
                LoggedConsole.WriteErrorLine("WARNING from method LDSpectrogramStitching.ConcatenateSpectralIndexFiles() !!!");
                LoggedConsole.WriteErrorLine("        An empty dictionary of spectral indices was returned !!! ");
                return null;
            }

            // now add in derived indices i.e. POW, NCDI etc
            // dictionaryOfSpectralIndices = IndexMatrices.AddDerivedIndices(dictionaryOfSpectralIndices);
            return dictionaryOfSpectralIndices;
        }

        /// <summary>
        /// Joins summary indices csv files together.
        /// This method merges ALL the passed files of acoustic indices
        /// It is assumed you are concatenating a sequence of consecutive short recordings.
        /// </summary>
        public static List<SummaryIndexValues> ConcatenateAllSummaryIndexFiles(
            FileInfo[] summaryIndexFiles,
            DirectoryInfo opDir,
            IndexGenerationData indexGenerationData,
            string outputFileBaseName)
        {
            var indexResolution = indexGenerationData.IndexCalculationDuration;

            var summaryIndices = IndexMatrices.ConcatenateSummaryIndexFilesWithTimeCheck(summaryIndexFiles, indexResolution);
            if (summaryIndices.Count == 0)
            {
                LoggedConsole.WriteErrorLine("WARNING: LDSpectrogramStitching.ConcatenateAllSummaryIndexFiles(): Empty List of SUMMARY indices returned!");
                return null;
            }

            // check length of data and make adjustments if required.
            // NOTHING done with this info at the moment. Could be used to truncate data to 24 hours.
            //int totalRowMinutes = (int)Math.Round(summaryIndices.Count() * indexResolution.TotalMinutes);

            // write out the list of data file names to JSON file.
            var arrayOfFileNames = summaryIndices.Select(x => x.FileName).ToArray();
            var path = FilenameHelpers.AnalysisResultPath(opDir, outputFileBaseName, "FileNames", "json");
            Json.Serialise(new FileInfo(path), arrayOfFileNames);

            return summaryIndices;
        }

        public static Dictionary<string, double[]> ConvertToDictionaryOfSummaryIndices(List<SummaryIndexValues> summaryIndices)
        {
            // Now add in derived indices i.e. NCDI etc
            // Decided NOT to do this anymore
            // dictionaryOfSummaryIndices = IndexMatrices.AddDerivedIndices(dictionaryOfSummaryIndices);

            // Put SUMMARY indices into dictionary. TODO need to generalise the following method
            // ################# WARNING: THIS METHOD ONLY GETS A "HARD CODED" LIST OF SUMMARY INDICES. See the method.
            var dictionaryOfSummaryIndices = IndexMatrices.GetDictionaryOfSummaryIndices(summaryIndices);

            // return the dictionary - it will be used later to produce an index tracks image.
            return dictionaryOfSummaryIndices;
        }

        // ##############################################################################################################
        // ######################### ORIGINAL METHOD FOR STITCHING  Gianna Pavan's DATA (10 minutes every 30 minutes)

        /// <summary>
        /// This method stitches together spectrogram images derived from consecutive shorter recordings over a 24 hour period.
        /// Currently set for the recording protocol of Gianna Pavan (10 minutes every 30 minutes).
        ///
        /// Call this method from Sandpit or where ever!
        ///
        /// IMPORTANT NOTE: This method does NOT check to see if the images are in temporal order.
        ///                 A SORT line should be inserted somewhere
        /// </summary>
        public static void StitchPartialSpectrograms()
        {
            //######################################################
            // ********************* set the below parameters
            var inputDirectory = new DirectoryInfo(@"Z:\Italy_GianniPavan\output4\Towsey.Acoustic");
            string opFileStem = "Sassofratino_24hours_v3";
            var outputDirectory = new DirectoryInfo(@"Z:\Italy_GianniPavan\output4\");

            // a filter to select images to be stitched
            string endString = "_000.2MAPS.png";

            // recording protocol
            int minutesBetweenRecordingStarts = 30;
            TimeSpan minOffset = TimeSpan.Zero; // assume first recording in sequence started at midnight

            // X-axis timescale
            int pixelColumnsPerHour = 60;
            int trackHeight = IndexDisplay.DefaultTrackHeight;

            // ********************* set the above parameters
            //######################################################

            string[] fileEntries = Directory.GetFiles(inputDirectory.FullName);
            var images = new List<Image>();
            bool interpolateSpacer = true;
            var imagePair = new Image[2];

            TimeSpan xAxisTicInterval = TimeSpan.FromMinutes(pixelColumnsPerHour); // assume 60 pixels per hour

            // loop through all files in the required directory
            foreach (string path in fileEntries)
            {
                // filter files.
                if (!path.EndsWith(endString))
                {
                    continue;
                }

                var image = new Bitmap(path);
                int spacerWidth = minutesBetweenRecordingStarts - image.Width;

                if (interpolateSpacer)
                {
                    var spacer = new Bitmap(spacerWidth, image.Height);
                    imagePair[0] = image;
                    imagePair[1] = spacer;
                    image = (Bitmap)ImageTools.CombineImagesInLine(imagePair);
                }

                images.Add(image);
            }

            var compositeBmp = ImageTools.CombineImagesInLine(images.ToArray());

            var fullDuration = TimeSpan.FromMinutes(compositeBmp.Width);
            var timeBmp = ImageTrack.DrawTimeTrack(fullDuration, minOffset, xAxisTicInterval, compositeBmp.Width, trackHeight, "hours");

            var gr = Graphics.FromImage(compositeBmp);
            int halfHeight = compositeBmp.Height / 2;

            //add in the title bars
            string title = $"24 hour FALSE-COLOUR SPECTROGRAM      (scale: hours x kHz)      (colour: R-G-B = BGN-AVG-CVR)         {Meta.OrganizationTag}  ";
            var titleBmp = ImageTrack.DrawTitleTrack(compositeBmp.Width, trackHeight, title);
            int offset = 0;
            gr.DrawImage(titleBmp, 0, offset); //draw in the top time scale
            title = $"24 hour FALSE-COLOUR SPECTROGRAM      (scale: hours x kHz)      (colour: R-G-B = ACI-ENT-EVN)         {Meta.OrganizationTag}  ";
            titleBmp = ImageTrack.DrawTitleTrack(compositeBmp.Width, trackHeight, title);
            offset = halfHeight;
            gr.DrawImage(titleBmp, 0, offset); //draw in the top time scale

            //add in the timescale tracks
            offset = trackHeight;
            gr.DrawImage(timeBmp, 0, offset); //draw in the top time scale
            offset = compositeBmp.Height - trackHeight;
            gr.DrawImage(timeBmp, 0, offset); //draw in the top time scale
            offset = halfHeight - trackHeight;
            gr.DrawImage(timeBmp, 0, offset); //draw in the top time scale
            offset = halfHeight + trackHeight;
            gr.DrawImage(timeBmp, 0, offset); //draw in the top time scale

            compositeBmp.Save(Path.Combine(outputDirectory.FullName, opFileStem + ".png"));
        }
    }
}
