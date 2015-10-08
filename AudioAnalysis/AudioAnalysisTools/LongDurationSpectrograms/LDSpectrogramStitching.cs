using AudioAnalysisTools.Indices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TowseyLibrary;
using AnalysisBase.ResultBases;
using Acoustics.Shared;
using Acoustics.Shared.Csv;

namespace AudioAnalysisTools.LongDurationSpectrograms
{


    /// <summary>
    /// This class contains two methods:  (1) StitchPartialSpectrograms()   and    (2) ConcatenateSpectralIndexFiles()
    /// 
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
    public static class LDSpectrogramStitching
    {

        // CONSTANT STRINGS
        public const string CsvFileExt = "csv";
        public const string ImgFileExt = "png";

        public const string SummaryIndicesStr  = "SummaryIndices";
        public const string SpectralIndicesStr = "SpectralIndices";


        public static DirectoryInfo[] GetSubDirectoriesForSiteData(DirectoryInfo[] topLevelDataDirectories, string site)
        {
            //string dateString = String.Format("{0}{1:D2}{2:D2}", dto.Year, dto.Month, dto.Day);
            string searchPattern = "*" + site + "*";

            // PATTERN SEARCH FOR CORRECT SUBDIRECTORIES
            // Assumes that the required files are subdirectories of given site. 
            List<string> dirList = new List<string>();
            foreach (DirectoryInfo dir in topLevelDataDirectories)
            {
                string[] dirs = Directory.GetDirectories(dir.FullName, searchPattern, SearchOption.AllDirectories);
                dirList.AddRange(dirs);
            }

            var dataDirectories = new List<DirectoryInfo>();
            foreach (string path in dirList)
            {
                dataDirectories.Add(new DirectoryInfo(path));
            }
            return dataDirectories.ToArray();
        }



        /// <summary>
        /// sorts a list of files by the date assumed to be encoded in their file names
        /// and then returns the list as a sorted dictionary with file DateTime as the keys.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="offsetHint"></param>
        /// <returns></returns>
        public static SortedDictionary<DateTimeOffset, FileInfo> FilterFilesForDates(FileInfo[] files, TimeSpan? offsetHint = null)
        {
            var datesAndFiles = new SortedDictionary<DateTimeOffset, FileInfo>();
            foreach (var file in files)
            {
                DateTimeOffset parsedDate;
                if (FileDateHelpers.FileNameContainsDateTime(file.Name, out parsedDate, offsetHint))
                {
                    datesAndFiles.Add(parsedDate, file);
                }
            }

            // use following lines to get first and last date from returned dictionary
            //DateTimeOffset firstdate = datesAndFiles[datesAndFiles.Keys.First()];
            //DateTimeOffset lastdate  = datesAndFiles[datesAndFiles.Keys.Last()];
            return datesAndFiles;
        }




        /// <summary>
        /// Use this concatenation method when you only want to concatenate the files for a fixed single day.
        /// The files to be concatenated must be somewhere in the subdirectory structure of the passed list of data directories
        /// </summary>
        /// <param name="directories"></param>
        /// <param name="opDir"></param>
        /// <param name="site"></param>
        /// <param name="dto"></param>
        public static Dictionary<string, double[,]> ConcatenateSpectralIndexFilesForOneDay(DirectoryInfo[] directories,
                                         DirectoryInfo opDir,
                                         string filestem,
                                         DateTimeOffset dto)
        {
            // 1. PATTERN SEARCH FOR CORRECT CSV FILES
            // Assumes that the required files are subdirectories of given site. 
            // Read them into a dictionary
            string colorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN;
            string colorMap2 = SpectrogramConstants.RGBMap_BGN_POW_EVN;
            string[] keys = LdSpectrogramConfig.GetKeys(colorMap1, colorMap2);
            string filePrefix = null;
            // Concatenate the csv files
            var dictionary = LDSpectrogramStitching.ConcatenateSpectralIndexFiles(directories.ToArray(), filePrefix, dto, keys);

            // 2. SAVE SPECTRAL INDEX DATA as CSV file TO OUTPUT DIRECTORY
            string dateString = String.Format("{0}{1:D2}{2:D2}", dto.Year, dto.Month, dto.Day);
            string opFileStem = String.Format("{0}_{1}", filestem, dateString);
            TwoDimensionalArray orient = TwoDimensionalArray.ColumnMajor;
            foreach (var key in keys)
            {
                var filename = FilenameHelpers.AnalysisResultName(opDir, opFileStem, key, "csv").ToFileInfo();
                Csv.WriteMatrixToCsv(filename, dictionary[key], orient);
            }

            return dictionary;
        }



        /// <summary>
        /// Assumes that the required spectral index files can be found using search patterns that utilise 
        /// the filePrefix,  the passed dateTimeOffset, the analysis type and the passed keys.  
        /// </summary>
        /// <param name="topLevelDirectories"></param>
        /// <param name="filePrefix"></param>
        /// <param name="dto"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static Dictionary<string, double[,]> ConcatenateSpectralIndexFiles(DirectoryInfo[] topLevelDirectories,
                                                         string filePrefix,
                                                         DateTimeOffset dto,
                                                         string[] keys)
        {
            string analysisType = "Towsey.Acoustic";

            string dateString = String.Format("{0}{1:D2}{2:D2}", dto.Year, dto.Month, dto.Day);
            string opFileStem = dateString;
            if (filePrefix != null)
                opFileStem = String.Format("{0}_{1}", filePrefix, dateString);

            string fileStemPattern = dateString + "*__" + analysisType;
            var dictionary = IndexMatrices.GetSpectralIndexFilesAndConcatenate(topLevelDirectories, fileStemPattern, keys);
            return dictionary;
        }



        /// <summary>
        /// MOST RECENT METHOD TO CONCATENATE Spectral INDEX.CSV FILES - Early September 2015.
        /// It is designed to deal with Yvonne's case where want to concatenate files distributed over arbitrary directories.
        /// It only merges files for the passed fixed date. i.e only 24 hours 
        /// </summary>
        public static void DrawSpectralIndexFiles(Dictionary<string, double[,]> dictionary,
                                                  IndexGenerationData indexGenerationData,
                                                  FileInfo indexPropertiesConfigFileInfo,
                                                  DirectoryInfo opDir,
                                                  SiteDescription siteDescription)
        {
            // derive new indices such as sqrt(POW), NCDI etc -- main reason for this is to view what their distributions look like.
            dictionary = IndexMatrices.AddDerivedIndices(dictionary);

            // Calculate the index distribution statistics and write to a json file. Also save as png image
            DateTimeOffset dto = (DateTimeOffset)indexGenerationData.RecordingStartDate;
            string dateString = String.Format("{0}{1:D2}{2:D2}", dto.Year, dto.Month, dto.Day);
            string opFileStem = String.Format("{0}_{1}", siteDescription.SiteName, dateString);

            var indexDistributions = IndexDistributions.WriteSpectralIndexDistributionStatistics(dictionary, opDir, opFileStem);

            SummaryIndexBase[] summaryIndices = null;
            bool returnChromelessImages = false;
            string analysisType = "Towsey.Acoustic";

            Tuple<Image, string>[] tuple = LDSpectrogramRGB.DrawSpectrogramsFromSpectralIndices(
            opDir, // topLevelDirectories[0], // this should not be required but it is - because things have gotten complicated !
            opDir,
            LdSpectrogramConfig.GetDefaultConfig(),
            indexPropertiesConfigFileInfo,
            indexGenerationData,
            opFileStem,
            analysisType,
            dictionary,
            summaryIndices,
            indexDistributions,
            siteDescription,
            returnChromelessImages);
        }


        // ####################################  SPECTRAL INDEX METHODS ABOVE HERE  ##################################

        // ####################################  SUMMARY  INDEX METHODS BELOW HERE  ##################################



        public static FileInfo[] GetSummaryIndexFilesForOneDay(DirectoryInfo[] directories, DateTimeOffset dto)
        {
            string pattern = "*__Towsey.Acoustic.Indices.csv";
           // LDSpectrogramStitching.IndexType, LDSpectrogramStitching.CsvFileExt

            string dateString = String.Format("{0}{1:D2}{2:D2}", dto.Year, dto.Month, dto.Day);
            //string opFileStem = String.Format("{0}_{1}", site, dateString);

            // 2. PATTERN SEARCH FOR CORRECT SUMMARY CSV FILES AND READ INTO DICTIONARY
            // Assumes that the required files are subdirectories of given site. 
            string fileStemPattern = dateString + pattern;
            FileInfo[] files = IndexMatrices.GetFilesInDirectories(directories, fileStemPattern);
            return files;
        }

        public static FileInfo[] GetSummaryIndexFilesForOneDay(SortedDictionary<DateTimeOffset, FileInfo> dict, DateTimeOffset dto)
        {
            string pattern = "Towsey.Acoustic.Indices.csv";
            // LDSpectrogramStitching.IndexType, LDSpectrogramStitching.CsvFileExt
            var keys = dict.Keys;
            var matchFiles = new List<FileInfo>();
            foreach (var key in keys)
            {
                if ((dto.Year == key.Year) && (dto.DayOfYear == key.DayOfYear))
                {
                    FileInfo file = dict[key];
                    if(file.Name.EndsWith(pattern))
                    {
                        matchFiles.Add(file);
                    }
                }
            }
            return matchFiles.ToArray();
        }

        public static SortedDictionary<DateTimeOffset, FileInfo> GetFilesForOneDay(SortedDictionary<DateTimeOffset, FileInfo> dict, DateTimeOffset dto)
        {
            var keys = dict.Keys;
            var matchFiles = new SortedDictionary<DateTimeOffset, FileInfo>();
            foreach (var key in keys)
            {
                if ((dto.Year == key.Year) && (dto.DayOfYear == key.DayOfYear))
                {
                    matchFiles.Add(key, dict[key]);
                }
            }
            return matchFiles;
        }


        public static Dictionary<string, double[]> ConcatenateSummaryIndexFiles(FileInfo[] files, DirectoryInfo opDir, FileInfo indicesCsvfile)
        {
            // the following method call assumes 24 hour long data i.e. trims length to 1440 minutes.
            var summaryDataTuple = IndexMatrices.GetSummaryIndexFilesAndConcatenateWithTimeCheck(files);
            string[] headers = summaryDataTuple.Item1;
            double[,] summaryIndices = summaryDataTuple.Item2;
            Dictionary<string, double[]> dictionaryOfCsvColumns = IndexMatrices.ConvertCsvData2DictionaryOfColumns(headers, summaryIndices);

            if (dictionaryOfCsvColumns.Count == 0)
            {
                LoggedConsole.WriteErrorLine("WARNING from method LDSpectrogramStitching.ConcatenateSummaryIndexFiles() !!!");
                LoggedConsole.WriteErrorLine("        An empty dictionary of SUMMARY indices was returned !!! ");
                return null;
            }


            //serialiseFunc(indicesFile, results);
            //Csv.WriteMatrixToCsv(indicesCsvfile, summaryIndices);
            CsvTools.WriteMatrix2CSV(summaryIndices, headers, indicesCsvfile);

            // insert some transformed data columns etc
            dictionaryOfCsvColumns = IndexMatrices.AddDerivedIndices(dictionaryOfCsvColumns);

            return dictionaryOfCsvColumns;
        }


        public static void DrawSummaryIndexFiles(Dictionary<string, double[]> dictionaryOfCsvColumns,
                                                IndexGenerationData indexGenerationData,
                                                FileInfo indexPropertiesConfigFileInfo,
                                                DirectoryInfo opDir,
                                                SiteDescription siteDescription)
        {
            DateTimeOffset dto = (DateTimeOffset)indexGenerationData.RecordingStartDate;

            string dateString = String.Format("{0}{1:D2}{2:D2}", dto.Year, dto.Month, dto.Day);
            string opFileStem = String.Format("{0}_{1}", siteDescription.SiteName, dateString);

            // Calculate the index distribution statistics and write to a json file. Also save as png image
            var indexDistributions = IndexDistributions.WriteSummaryIndexDistributionStatistics(dictionaryOfCsvColumns, opDir, opFileStem);

            TimeSpan start = ((DateTimeOffset)indexGenerationData.RecordingStartDate).TimeOfDay;
            string startTime = string.Format("{0:d2}{1:d2}h", start.Hours, start.Minutes);
            if((start.Hours == 0) && (start.Minutes == 0)) startTime = "midnight";
            string titletext = string.Format("SOURCE: \"{0}\".     Starts at {1}                       (c) QUT.EDU.AU", opFileStem, startTime);
            Bitmap tracksImage = DrawSummaryIndices.DrawImageOfSummaryIndices(
                                 IndexProperties.GetIndexProperties(indexPropertiesConfigFileInfo),
                                 indexGenerationData,
                                 dictionaryOfCsvColumns,
                                 titletext,
                                 siteDescription);
            var imagePath = FilenameHelpers.AnalysisResultName(opDir, opFileStem, SummaryIndicesStr, ImgFileExt);
            tracksImage.Save(imagePath);
        }


        // ##############################################################################################################
        // ######################### METHOD FOR STITCHING EDDIE GAME's DATA 
        // ######################### CONCATENATE EVRYTHING

        /// <summary>
        /// Use this concatenation method when you want to concatenate EVERY index file within the passed data directories
        /// Used for eddie Game recordings
        /// </summary>
        /// <param name="dataDir"></param>
        /// <param name="indexPropertiesConfigFileInfo"></param>
        /// <param name="opDir"></param>
        /// <param name="opFileStem"></param>
        public static void ConcatenateAllIndexFiles(DirectoryInfo[] dataDirs,
                                                 FileInfo indexPropertiesConfigFileInfo,
                                                 DirectoryInfo opDir,
                                                 string opFileStem,
                                                 double? latitude = null,
                                                 double? longitude = null)
        {
            LDSpectrogramStitching.ConcatenateSpectralIndexFiles(dataDirs[0], indexPropertiesConfigFileInfo, opDir, opFileStem);
            LDSpectrogramStitching.ConcatenateSummaryIndexFiles(dataDirs[0], indexPropertiesConfigFileInfo, opDir, opFileStem);
        }



        /// <summary>
        /// RECENT METHOD TO CONCATENATE Spectral INDEX.CSV FILES - August 2015.
        /// Was written to deal with  EDDIE GAME PNG data where the files to be concatenated are all in one top level directory.
        /// This method merges all files of acoustic indices derived from a sequence of consecutive 1/2 to 6 hour recordings
        /// The total length of the concatenated files can exceed 24 hours - limited by memory! 
        /// This was necessary to deal with the new regime of doing 24 hour recordings in consecutive short segments. 
        /// IMPORTANT NOTE: THIS METHOD DOES NOT CHECK FOR TEMPORAL GAPS BETWEEN THE STITCHED CSV FILES!
        ///                 SEE METHOD ABOVE WHICH DOES CHECK -- StitchPartialSpectrograms()
        /// </summary>
        public static void ConcatenateSpectralIndexFiles(DirectoryInfo topLevelDirectory,
                                                          FileInfo indexPropertiesConfigFileInfo,
                                                          DirectoryInfo opDir,
                                                          string opFileStem)
        {
            string analysisType = "Towsey.Acoustic";

            var ldSpectrogramConfig = new LdSpectrogramConfig
            {
                XAxisTicInterval = SpectrogramConstants.X_AXIS_TIC_INTERVAL,
                YAxisTicInterval = 1000,
                //ColorMap1 = "ACI-TEN-CVR",
                //ColorMap2 = "BGN-AVG-VAR",
                ColorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN,
                ColorMap2 = SpectrogramConstants.RGBMap_BGN_POW_EVN,
            };
            // string[] keys = { "ACI", "ENT", "EVN", "BGN", "POW" };
            string[] keys = ldSpectrogramConfig.GetKeys();
            string path = topLevelDirectory.FullName;

            // assume that the last 8 digits of the passed filename contain a date. 
            string date = opFileStem.Substring(opFileStem.Length - 8);
            string fileStemPattern = date + "*__" + analysisType;
            var dictionary = IndexMatrices.GetSpectralIndexFilesAndConcatenate(path, fileStemPattern, keys);
            if (dictionary.Count == 0)
            {
                LoggedConsole.WriteErrorLine("WARNING from method LDSpectrogramStitching.ConcatenateSpectralIndexFiles() !!!");
                LoggedConsole.WriteErrorLine("        An empty dictionary of spectral indices was returned !!! ");
            }

            // now add in derived indices i.e. POW, NCDI etc
            dictionary = IndexMatrices.AddDerivedIndices(dictionary);

            // Calculate the index distribution statistics and write to a json file. Also save as png image
            var indexDistributions = IndexDistributions.WriteSpectralIndexDistributionStatistics(dictionary, opDir, opFileStem);


            // get first file name from sorted list
            string pattern = "*ACI.csv";
            FileInfo[] files = IndexMatrices.GetFilesInDirectory(topLevelDirectory.FullName, pattern);

            // get the IndexGenerationData file from the first directory
            IndexGenerationData indexGenerationData = IndexGenerationData.GetIndexGenerationDataAndAddStartTime(files[0].Directory, files[0].Name);

            var siteDescription = new SiteDescription();
            siteDescription.SiteName = opFileStem;

            SummaryIndexBase[] summaryIndices = null;
            bool returnChromelessImages = false;

            Tuple<Image, string>[] tuple = LDSpectrogramRGB.DrawSpectrogramsFromSpectralIndices(
            topLevelDirectory,
            opDir,
            ldSpectrogramConfig,
            indexPropertiesConfigFileInfo,
            indexGenerationData,
            opFileStem,
            analysisType,
            dictionary,
            summaryIndices,
            indexDistributions,
            siteDescription,
            returnChromelessImages);
        }


        /// <summary>
        /// MOST RECENT METHOD TO CONCATENATE SUMMARY INDEX.CSV FILES - August 2015.
        /// WRITTEN FOR EDDIE GAME's DATA
        /// This method merges ALL files of acoustic indices (in any and all subdirectories of the passed topLevelDirectory) 
        /// It is assumed you are concatneating a sequence of consecutive shorter recordings.
        /// NOTE WARNING with method call to IndexMatrices.GetSummaryIndexFilesAndConcatenateWithTimeCheck(files);
        /// </summary>
        public static void ConcatenateSummaryIndexFiles(DirectoryInfo topLevelDirectory,
                                                          FileInfo indexPropertiesConfig,
                                                          DirectoryInfo opDir,
                                                          string opFileStem)
        {
            string csvFileExt = "csv";
            string imgFileExt = "png";

            string pattern = "*__Towsey.Acoustic.Indices.csv";
            string indexType = "SummaryIndices";

            // assume that the last 8 digits of the passed filename contain a date. 
            string date = opFileStem.Substring(opFileStem.Length - 8);
            string fileStemPattern = date + pattern;
            FileInfo[] files = IndexMatrices.GetFilesInDirectory(topLevelDirectory.FullName, fileStemPattern);

            // the following method call assumes 24 hour long data i.e. trims length to 1440 minutes.
            var summaryDataTuple = IndexMatrices.GetSummaryIndexFilesAndConcatenateWithTimeCheck(files);
            string[] headers = summaryDataTuple.Item1;
            double[,] summaryIndices = summaryDataTuple.Item2;
            Dictionary<string, double[]> dictionaryOfCsvColumns = IndexMatrices.ConvertCsvData2DictionaryOfColumns(headers, summaryIndices);
            if (dictionaryOfCsvColumns.Count == 0)
            {
                LoggedConsole.WriteErrorLine("WARNING from method LDSpectrogramStitching.ConcatenateSummaryIndexFiles() !!!");
                LoggedConsole.WriteErrorLine("        An empty dictionary of SUMMARY indices was returned !!! ");
            }


            // now add in derived indices i.e. NCDI etc
            dictionaryOfCsvColumns = IndexMatrices.AddDerivedIndices(dictionaryOfCsvColumns);


            // Calculate the index distribution statistics and write to a json file. Also save as png image
            var indexDistributions = IndexDistributions.WriteSummaryIndexDistributionStatistics(dictionaryOfCsvColumns, opDir, opFileStem);

            var indicesFile = FilenameHelpers.AnalysisResultName(opDir, opFileStem, indexType, csvFileExt);
            var indicesCsvfile = new FileInfo(indicesFile);
            //serialiseFunc(indicesFile, results);
            //Csv.WriteMatrixToCsv(indicesCsvfile, summaryIndices);
            CsvTools.WriteMatrix2CSV(summaryIndices, headers, indicesCsvfile);

            // get the IndexGenerationData file from the first directory
            IndexGenerationData indexGenerationData = IndexGenerationData.GetIndexGenerationDataAndAddStartTime(files[0].Directory, files[0].Name);
            TimeSpan start = ((DateTimeOffset)indexGenerationData.RecordingStartDate).TimeOfDay;
            string startTime = string.Format("{0:d2}{1:d2}h", start.Hours, start.Minutes);
            string imageTitle = string.Format("SOURCE: \"{0}\".     Starts at {1}                       (c) QUT.EDU.AU", opFileStem, startTime);
            Bitmap tracksImage =
                DrawSummaryIndices.DrawImageOfSummaryIndices(
                    IndexProperties.GetIndexProperties(indexPropertiesConfig),
                    indexGenerationData,
                    dictionaryOfCsvColumns,
                    imageTitle);
            var imagePath = FilenameHelpers.AnalysisResultName(opDir, opFileStem, indexType, imgFileExt);
            tracksImage.Save(imagePath);
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
            int trackHeight = DrawSummaryIndices.DefaultTrackHeight;
            // ********************* set the above parameters
            //######################################################

            string[] fileEntries = Directory.GetFiles(inputDirectory.FullName);
            List<Image> images = new List<Image>();
            bool interpolateSpacer = true;
            var imagePair = new Image[2];

            TimeSpan xAxisTicInterval = TimeSpan.FromMinutes(pixelColumnsPerHour); // assume 60 pixels per hour

            // loop through all files in the required directory 
            foreach (string path in fileEntries)
            {
                // filter files.
                if (!path.EndsWith(endString)) continue;
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
            Image compositeBmp = ImageTools.CombineImagesInLine(images.ToArray());

            TimeSpan fullDuration = TimeSpan.FromMinutes(compositeBmp.Width);
            Bitmap timeBmp = Image_Track.DrawTimeTrack(fullDuration, minOffset, xAxisTicInterval, compositeBmp.Width, trackHeight, "hours");

            Graphics gr = Graphics.FromImage(compositeBmp);
            int halfHeight = compositeBmp.Height / 2;

            //add in the title bars
            string title = string.Format("24 hour FALSE-COLOUR SPECTROGRAM      (scale: hours x kHz)      (colour: R-G-B = {0})         (c) QUT.EDU.AU.  ", "BGN-AVG-CVR");
            Bitmap titleBmp = Image_Track.DrawTitleTrack(compositeBmp.Width, trackHeight, title);
            int offset = 0;
            gr.DrawImage(titleBmp, 0, offset); //draw in the top time scale
            title = string.Format("24 hour FALSE-COLOUR SPECTROGRAM      (scale: hours x kHz)      (colour: R-G-B = {0})         (c) QUT.EDU.AU.  ", "ACI-ENT-EVN");
            titleBmp = Image_Track.DrawTitleTrack(compositeBmp.Width, trackHeight, title);
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
