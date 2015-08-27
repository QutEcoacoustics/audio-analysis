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



        /// <summary>
        /// Use this concatenation method when you want to concatenate every file within the passed data directory
        /// </summary>
        /// <param name="dataDir"></param>
        /// <param name="indexPropertiesConfigFileInfo"></param>
        /// <param name="opDir"></param>
        /// <param name="opFileStem"></param>
        public static void ConcatenateIndexFiles(DirectoryInfo dataDir, 
                                                 FileInfo indexPropertiesConfigFileInfo, 
                                                 DirectoryInfo opDir,
                                                 string opFileStem,
                                                 double? latitude = null,
                                                 double? longitude = null)
        {
            LDSpectrogramStitching.ConcatenateSpectralIndexFiles(dataDir, indexPropertiesConfigFileInfo, opDir, opFileStem);
            LDSpectrogramStitching.ConcatenateSummaryIndexFiles(dataDir, indexPropertiesConfigFileInfo, opDir, opFileStem);
        }


        /// <summary>
        /// Use this concatenation method when you only want to concatenate the files only for a fixed single day.
        /// The files to be concatenated must be somewhere in the subdirectory structure of the passed list of data directories
        /// </summary>
        /// <param name="topLevelDataDirectories"></param>
        /// <param name="indexPropertiesConfigFileInfo"></param>
        /// <param name="opDir"></param>
        /// <param name="site"></param>
        /// <param name="dto"></param>
        public static int ConcatenateIndexFiles(DirectoryInfo[] topLevelDataDirectories,
                                         FileInfo indexPropertiesConfigFileInfo,
                                         DirectoryInfo opDir,
                                         string site,
                                         DateTimeOffset dto,
                                         double? latitude = null,
                                         double? longitude = null)
        {
            int returnStatus = 0;

            // 1. PATTERN SEARCH FOR CORRECT SUBDIRECTORIES
            // Assumes that the required subdirectories have given site somewhere in their path. 
            var dataDirectories = GetSubDirectoriesForSiteData(topLevelDataDirectories, site);

            // 2. PATTERN SEARCH FOR CORRECT CSV FILES
            // Assumes that the required files are subdirectories of given site. 
            // Read them into a dictionary
            string colorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN;
            string colorMap2 = SpectrogramConstants.RGBMap_BGN_POW_EVN;
            string[] keys = LdSpectrogramConfig.GetKeys(colorMap1, colorMap2);
            string filePrefix = null;
            // Concatenate the csv files
            var dictionary1 = LDSpectrogramStitching.ConcatenateSpectralIndexFiles(dataDirectories.ToArray(), filePrefix, dto, keys);
            if (dictionary1.Count == 0)
            {
                LoggedConsole.WriteErrorLine("WARNING from method LDSpectrogramStitching.ConcatenateIndexFiles() !!!");
                LoggedConsole.WriteErrorLine("        An empty dictionary of spectral indices was returned !!! ");
                return returnStatus = 1;
            }

            // 3. CREATE OUTPUT DIRECTORY
            string dateString = String.Format("{0}{1:D2}{2:D2}", dto.Year, dto.Month, dto.Day);
            DirectoryInfo resultsDir = new DirectoryInfo(Path.Combine(opDir.FullName, site, dateString));
            if (!resultsDir.Exists) resultsDir.Create();


            // 4. SAVE SPECTRAL INDEX DATA as CSV file TO OUTPUT DIRECTORY
            string opFileStem = String.Format("{0}_{1}", site, dateString);
            TwoDimensionalArray orient = TwoDimensionalArray.ColumnMajor;
            // TODO TODO TODO TODO TODO -- THERE ARE PROBLEMS WITH ORIENTATION OF THE WRITTEN FILES. COLUMNS END UP WRITTEN IN REVERSE ORDER
            foreach (var key in keys)
            {
                var filename = FilenameHelpers.AnalysisResultName(resultsDir, opFileStem, key, "csv").ToFileInfo();
                Csv.WriteMatrixToCsv(filename, dictionary1[key], orient);
            }


            // 5. DRAW SPECTRAL INDEX IMAGES AND SAVE IN OUTPUT DIRECTORY
            LDSpectrogramStitching.DrawSpectralIndexFiles(dataDirectories.ToArray(), dictionary1, indexPropertiesConfigFileInfo, resultsDir, site, dto);


            // ==========================================================================


            // 6. PATTERN SEARCH FOR CORRECT SUMMARY CSV FILES AND READ INTO DICTIONARY
            // Assumes that the required files are subdirectories of given site. 
            // Read them into a dictionary - also written at same time
            var dictionary2 = LDSpectrogramStitching.ConcatenateSummaryIndexFiles(dataDirectories.ToArray(), resultsDir, site, dto);
            if (dictionary2.Count == 0)
            {
                LoggedConsole.WriteErrorLine("WARNING from method LDSpectrogramStitching.ConcatenateIndexFiles() !!!");
                LoggedConsole.WriteErrorLine("        An empty dictionary of summary indices was returned !!! ");
                return returnStatus = 1;
            }

            // 7. DRAW SUMMARY INDEX IMAGES AND SAVE IN OUTPUT DIRECTORY
            LDSpectrogramStitching.DrawSummaryIndexFiles(dataDirectories.ToArray(), dictionary2, indexPropertiesConfigFileInfo, resultsDir, site, dto);

            return returnStatus;
        }


        public static DirectoryInfo[] GetSubDirectoriesForSiteData(DirectoryInfo[] topLevelDataDirectories, string site)
        {
            //string dateString = String.Format("{0}{1:D2}{2:D2}", dto.Year, dto.Month, dto.Day);
            string searchPattern = site + "*";

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
        /// Assumes that the required spectral index files can be found using search patterns that utilise the filePrefix, 
        /// passed date, the analysis type and the passed keys.  
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
        /// MOST RECENT METHOD TO CONCATENATE Spectral INDEX.CSV FILES - Late August 2015.
        /// It is designed to deal with Yvonne's case where want to concatenate files distributed over arbitrary directories.
        /// It only merges files for the passed fixed date. i.e only 24 hours 
        /// </summary>
        public static void DrawSpectralIndexFiles(DirectoryInfo[] topLevelDirectories,
                                                 Dictionary<string, double[,]> dictionary,
                                                 FileInfo indexPropertiesConfigFileInfo,
                                                 DirectoryInfo opDir,
                                                 string site,
                                                 DateTimeOffset dto,
                                                 double? latitude = null,
                                                 double? longitude = null)
        {
            // derive new indices such as sqrt(POW), NCDI etc -- main reason for this is to view what their distributions look like.
            dictionary = IndexMatrices.AddDerivedIndices(dictionary);

            // Calculate the index distribution statistics and write to a json file. Also save as png image
            string dateString = String.Format("{0}{1:D2}{2:D2}", dto.Year, dto.Month, dto.Day);
            string opFileStem = String.Format("{0}_{1}", site, dateString);

            var indexDistributions = IndexDistributions.WriteSpectralIndexDistributionStatistics(dictionary, opDir, opFileStem);

            // We want to recover the indexGenerationData and insert new info
            IndexGenerationData indexGenerationData = GetIndexGenerationData(topLevelDirectories[0]);
            indexGenerationData.RecordingStartDate = dto;

            //throw new NotImplementedException("DISABLED ON PURPOSE");
            var siteDescription = new SiteDescription();
            siteDescription.SiteName = site;
            siteDescription.Latitude = latitude;
            siteDescription.Longitude = longitude;

            SummaryIndexBase[] summaryIndices = null;
            bool returnChromelessImages = false;
            string analysisType = "Towsey.Acoustic";

            Tuple<Image, string>[] tuple = LDSpectrogramRGB.DrawSpectrogramsFromSpectralIndices(
            topLevelDirectories[0], // this should not be required but it is - because things have gotten complicated !
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




        /// <summary>
        /// RECENT METHOD TO CONCATENATE Spectral INDEX.CSV FILES - August 2015.
        /// Was written to deal with PNG data where the files to be concatenated are all in one top level directory.
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
            IndexGenerationData indexGenerationData = GetIndexGenerationDataAndAddStartTime(files[0].Directory, files[0].Name);

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
        /// This method merges ALL files of acoustic indices (in any and all subdirectories of the passed topLevelDirectory) 
        /// It is assumed you are concatneating a sequence of consecutive shorter recordings.
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
            var summaryDataTuple  = IndexMatrices.GetSummaryIndexFilesAndConcatenate(files);
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
            IndexGenerationData indexGenerationData = GetIndexGenerationDataAndAddStartTime(files[0].Directory, files[0].Name);
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



        public static Dictionary<string, double[]> ConcatenateSummaryIndexFiles(DirectoryInfo[] topLevelDirectories, DirectoryInfo opDir, string site, DateTimeOffset dto)
        {
            string csvFileExt = "csv";
            string indexType = "SummaryIndices";
            string pattern = "*__Towsey.Acoustic.Indices.csv";

            string dateString = String.Format("{0}{1:D2}{2:D2}", dto.Year, dto.Month, dto.Day);
            string opFileStem = String.Format("{0}_{1}", site, dateString);

            string fileStemPattern = dateString + pattern;
            FileInfo[] files = IndexMatrices.GetFilesInDirectories(topLevelDirectories, fileStemPattern);

            if (files.Length == 0)
            {
                return null;
            }
            var summaryDataTuple = IndexMatrices.GetSummaryIndexFilesAndConcatenate(files);
            string[] headers = summaryDataTuple.Item1;
            double[,] summaryIndices = summaryDataTuple.Item2;
            Dictionary<string, double[]> dictionaryOfCsvColumns = IndexMatrices.ConvertCsvData2DictionaryOfColumns(headers, summaryIndices);

            if (dictionaryOfCsvColumns.Count == 0)
            {
                LoggedConsole.WriteErrorLine("WARNING from method LDSpectrogramStitching.ConcatenateSummaryIndexFiles() !!!");
                LoggedConsole.WriteErrorLine("        An empty dictionary of SUMMARY indices was returned !!! ");
                return null;
            }


            var indicesFile = FilenameHelpers.AnalysisResultName(opDir, opFileStem, indexType, csvFileExt);
            var indicesCsvfile = new FileInfo(indicesFile);
            //serialiseFunc(indicesFile, results);
            //Csv.WriteMatrixToCsv(indicesCsvfile, summaryIndices);
            CsvTools.WriteMatrix2CSV(summaryIndices, headers, indicesCsvfile);

            // insert some transformed data columns etc
            dictionaryOfCsvColumns = IndexMatrices.AddDerivedIndices(dictionaryOfCsvColumns);

            return dictionaryOfCsvColumns;
        }


        public static void DrawSummaryIndexFiles(DirectoryInfo[] topLevelDirectories,
                                                Dictionary<string, double[]> dictionaryOfCsvColumns,
                                                FileInfo indexPropertiesConfigFileInfo,
                                                DirectoryInfo opDir,
                                                string site,
                                                DateTimeOffset dto,
                                                double? latitude = null,
                                                double? longitude = null)
        {
            string imgFileExt = "png";
            string indexType = "SummaryIndices";

            string dateString = String.Format("{0}{1:D2}{2:D2}", dto.Year, dto.Month, dto.Day);
            string opFileStem = String.Format("{0}_{1}", site, dateString);

            // Calculate the index distribution statistics and write to a json file. Also save as png image
            var indexDistributions = IndexDistributions.WriteSummaryIndexDistributionStatistics(dictionaryOfCsvColumns, opDir, opFileStem);

            // get the IndexGenerationData file from the first directory
            IndexGenerationData indexGenerationData = GetIndexGenerationData(topLevelDirectories[0]);
            indexGenerationData.RecordingStartDate  = dto;

            var siteDescription = new SiteDescription();
            siteDescription.SiteName = opFileStem;
            siteDescription.Latitude  = latitude;
            siteDescription.Longitude = longitude;

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
            var imagePath = FilenameHelpers.AnalysisResultName(opDir, opFileStem, indexType, imgFileExt);
            tracksImage.Save(imagePath);
        }





        /// <summary>
        /// Returns the index generation data from file in passed directory.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static IndexGenerationData GetIndexGenerationData(DirectoryInfo directory)
        {
            string pattern = "*__" + IndexGenerationData.FileNameFragment + ".json";
            //FileInfo igdFile = IndexMatrices.GetFilesInDirectory(directory.FullName, pattern).Single();
            FileInfo[] igdFiles = IndexMatrices.GetFilesInDirectory(directory.FullName, pattern);
            IndexGenerationData indexGenerationData = Json.Deserialise<IndexGenerationData>(igdFiles[0]);
            return indexGenerationData;
        }

        public static IndexGenerationData GetIndexGenerationDataAndAddStartTime(DirectoryInfo directory, string fileName)
        {
            var indexGenerationData = GetIndexGenerationData(directory);

            // Get the start time from the file name.
            DateTimeOffset startTime = IndexMatrices.GetFileStartTime(fileName);
            indexGenerationData.RecordingStartDate = startTime;
            return indexGenerationData;
        }


    }
}
