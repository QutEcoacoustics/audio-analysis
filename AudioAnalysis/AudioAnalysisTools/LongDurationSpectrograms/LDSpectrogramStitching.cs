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
    /// This class used to contain only two methods:  (1) StitchPartialSpectrograms()   and    (2) ConcatenateSpectralIndexFiles()
    /// Now it contains several versions to concatenate Index files. HTis is because there are now several use cases.
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
        /// ONLY Use this concatenation method when you want to concatenate the files for a fixed single day.
        /// The files to be concatenated must be somewhere in the subdirectory structure of the passed list of data directories
        /// Read them into a dictionary
        /// </summary>
        /// <param name="directories"></param>
        /// <param name="opDir"></param>
        /// <param name="site"></param>
        /// <param name="dto"></param>
        //public static Dictionary<string, double[,]> ConcatenateSpectralIndexFilesForOneDay(DirectoryInfo[] directories,
        //                                 DirectoryInfo opDir,
        //                                 string filestem,
        //                                 DateTimeOffset dto,
        //                                 IndexGenerationData indexGenerationData,
        //                                 string[] keys,
        //                                 bool verbose = false)
        //{
        //    // 1. PATTERN SEARCH FOR CORRECT CSV FILES
        //    string analysisType = "Towsey.Acoustic";
        //    string dateString = String.Format("{0}{1:D2}{2:D2}", dto.Year, dto.Month, dto.Day);

        //    string fileStemPattern = "*" + dateString + "*__" + analysisType;
        //    var dictionary = IndexMatrices.GetSpectralIndexFilesAndConcatenate(directories, fileStemPattern, keys, indexGenerationData, verbose);

        //    // 2. SAVE SPECTRAL INDEX DATA as CSV file TO OUTPUT DIRECTORY
        //    string opFileStem = String.Format("{0}_{1}", filestem, dateString);
        //    //TwoDimensionalArray orient = TwoDimensionalArray.ColumnMajor;
        //    TwoDimensionalArray orient = TwoDimensionalArray.ColumnMajorFlipped;
        //    foreach (var key in keys)
        //    {
        //        var filename = FilenameHelpers.AnalysisResultName(opDir, opFileStem, key, "csv").ToFileInfo();
        //        Csv.WriteMatrixToCsv(filename, dictionary[key], orient);
        //    }

        //    return dictionary;
        //}


        /// <summary>
        /// MOST RECENT METHOD TO CONCATENATE Spectral INDEX.CSV FILES - Early September 2015.
        /// It is designed to deal with Yvonne's case where want to concatenate files distributed over arbitrary directories.
        /// It only merges files for the passed fixed date. i.e only 24 hours 
        /// </summary>
        public static void DrawSpectralIndexFiles(Dictionary<string, double[,]> dictionary,
                                                  LdSpectrogramConfig sgConfig,
                                                  IndexGenerationData indexGenerationData,
                                                  FileInfo indexPropertiesConfigFileInfo,
                                                  DirectoryInfo opDir,
                                                  SiteDescription siteDescription,
                                                  FileInfo sunriseDataFile = null,
                                                  List<ErroneousIndexSegments> segmentErrors = null,
                                                  bool verbose = false)
        {
            // derive new indices such as sqrt(POW), NCDI etc -- main reason for this is to view what their distributions look like.
            dictionary = IndexMatrices.AddDerivedIndices(dictionary);

            // Calculate the index distribution statistics and write to a json file. Also save as png image
            DateTimeOffset dto = (DateTimeOffset)indexGenerationData.RecordingStartDate;
            string dateString = String.Format("{0}{1:D2}{2:D2}", dto.Year, dto.Month, dto.Day);
            string opFileStem = String.Format("{0}_{1}", siteDescription.SiteName, dateString);

            var indexDistributions = IndexDistributions.WriteSpectralIndexDistributionStatistics(dictionary, opDir, opFileStem);

            SummaryIndexBase[] summaryIndices = null;
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
            summaryIndices,
            indexDistributions,
            siteDescription,
            sunriseDataFile,
            segmentErrors,
            ImageChrome.With,
            verbose);
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



        public static Dictionary<string, double[]> ConcatenateSummaryIndexFiles_DEPRACATED(FileInfo[] summaryIndexFiles, 
                                                                            DirectoryInfo opDir, 
                                                                            FileInfo indicesCsvfile, 
                                                                            IndexGenerationData indexGenerationData)
        {
            var indexResolution = indexGenerationData.IndexCalculationDuration;

            var summaryIndices = IndexMatrices.ConcatenateSummaryIndexFilesWithTimeCheck(summaryIndexFiles, indexResolution);

            // write out the list of file names to JSON ifle
            var fileNames = summaryIndices.Select(x => x.FileName).ToArray();
            FileInfo path = new FileInfo(indicesCsvfile + "_FileNames.json");
            Json.Serialise(path, fileNames);

            //now put summary indices into a dictionary. WARNING: THIS METHOD ONLY GETS FIXED LIST OF INDICES.
            var dictionaryOfsummaryIndices = IndexMatrices.GetDictionaryOfSummaryIndices(summaryIndices);
            if (dictionaryOfsummaryIndices.Count == 0)
            {
                LoggedConsole.WriteErrorLine("WARNING from method LDSpectrogramStitching.ConcatenateSummaryIndexFiles() !!!");
                LoggedConsole.WriteErrorLine("        An empty dictionary of SUMMARY indices was returned !!! ");
                return null;
            }


            //serialiseFunc(indicesFile, results);
            //Csv.WriteMatrixToCsv(indicesCsvfile, summaryIndices);
            CsvTools.WriteDictionaryOfDoubles2CSV(dictionaryOfsummaryIndices, indicesCsvfile);

            // now add in derived indices i.e. NCDI etc
            //dictionaryOfsummaryIndices = IndexMatrices.AddDerivedIndices(dictionaryOfsummaryIndices);

            return dictionaryOfsummaryIndices;
        }


        public static void DrawSummaryIndexFiles(Dictionary<string, double[]> dictionaryOfCsvColumns,
                                                IndexGenerationData indexGenerationData,
                                                FileInfo indexPropertiesConfigFileInfo,
                                                DirectoryInfo opDir,
                                                SiteDescription siteDescription,
                                                FileInfo sunriseDatafile = null,
                                                List<ErroneousIndexSegments> erroneousSegments = null, // info if have fatal errors i.e. no signal
                                                bool verbose = false
            )
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
            Bitmap tracksImage = IndexDisplay.DrawImageOfSummaryIndices(
                                 IndexProperties.GetIndexProperties(indexPropertiesConfigFileInfo),
                                 dictionaryOfCsvColumns,
                                 titletext,
                                 indexGenerationData.IndexCalculationDuration,
                                 indexGenerationData.RecordingStartDate,
                                 sunriseDatafile,
                                 erroneousSegments,
                                 verbose);
            var imagePath = FilenameHelpers.AnalysisResultName(opDir, opFileStem, SummaryIndicesStr, ImgFileExt);
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
        public static Dictionary<string, double[,]> ConcatenateAllSpectralIndexFiles(DirectoryInfo[] directories, 
                                                         DirectoryInfo opDir, 
                                                         FileInfo indexPropertiesConfig,
                                                         IndexGenerationData indexGenerationData,
                                                         string opFileStem)
        {
            string[] keys = {"ACI", "ENT", "EVN", "BGN", "POW", "CLS", "SPT", "RHZ", "CVR"};

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

            // Calculate the index distribution statistics and write to a json file. Also save as png image
            var indexDistributions = IndexDistributions.WriteSpectralIndexDistributionStatistics(dictionaryOfSpectralIndices, opDir, opFileStem);

            return dictionaryOfSpectralIndices;
        }


        /// <summary>
        /// MOST RECENT METHOD TO CONCATENATE SUMMARY INDEX.CSV FILES - August 2015. Revised september 2016
        /// WRITTEN FOR THE NATURE CONSERVANCY DATA
        /// This method merges ALL the passed files of acoustic indices 
        /// It is assumed you are concatenating a sequence of consecutive short recordings.
        /// </summary>
        public static Dictionary<string, double[]> ConcatenateAllSummaryIndexFiles(FileInfo[] summaryIndexFiles,
                                                        DirectoryInfo opDir,
                                                        IndexGenerationData indexGenerationData,
                                                        string opFileStem)
        {
            var indexResolution = indexGenerationData.IndexCalculationDuration;

            var summaryIndices = IndexMatrices.ConcatenateSummaryIndexFilesWithTimeCheck(summaryIndexFiles, indexResolution);

            // check length of data and make adjustments if required.
            int totalRowMinutes = (int)Math.Round(summaryIndices.Count() * indexResolution.TotalMinutes);
            // NOTHING done with this info at the moment. Could be used to truncate data to 24 hours.

            // write out the list of data file names to JSON file.
            var arrayOfFileNames = summaryIndices.Select(x => x.FileName).ToArray();
            string indexType = "SummaryIndex";
            var path = FilenameHelpers.AnalysisResultName(opDir, opFileStem, indexType, "FileNames.json");
            Json.Serialise(new FileInfo(path), arrayOfFileNames);

            //now put summary indices into a dictionary. WARNING: THIS METHOD ONLY GETS FIXED LIST OF INDICES.
            var dictionaryOfSummaryIndices = IndexMatrices.GetDictionaryOfSummaryIndices(summaryIndices);
            if (dictionaryOfSummaryIndices.Count == 0)
            {
                LoggedConsole.WriteErrorLine("WARNING from method LDSpectrogramStitching.ConcatenateSummaryIndexFiles() !!!");
                LoggedConsole.WriteErrorLine("        An empty dictionary of SUMMARY indices was returned !!! ");
                return null;
            }

            // now add in derived indices i.e. NCDI etc
            // dictionaryOfSummaryIndices = IndexMatrices.AddDerivedIndices(dictionaryOfSummaryIndices);

            // Calculate the index distribution statistics and write to a json file. Also save as png image
            var indexDistributions = IndexDistributions.WriteSummaryIndexDistributionStatistics(dictionaryOfSummaryIndices, opDir, opFileStem);
            string csvFileExt = "csv";

            var indicesFile = FilenameHelpers.AnalysisResultName(opDir, opFileStem, indexType, csvFileExt);
            var indicesCsvfile = new FileInfo(indicesFile);
            //serialiseFunc(indicesFile, results);
            //Csv.WriteMatrixToCsv(indicesCsvfile, summaryIndices);
            //CsvTools.WriteMatrix2CSV(summaryIndices, headers, indicesCsvfile);
            CsvTools.WriteDictionaryOfDoubles2CSV(dictionaryOfSummaryIndices, indicesCsvfile);

            return dictionaryOfSummaryIndices;
        }




        /// <summary>
        /// There can be issues with this method because images are not at same dpi.
        /// https://msdn.microsoft.com/en-us/library/system.drawing.bitmap.setresolution(v=vs.110).aspx
        /// I.e. resolution = 96dpi rather than 120 dpi
        /// 
        /// If having resolution problems i.e. the bitmap does not draw at the correct size into the larger Graphics canvas,
        ///  then may need to comment out the line: ((Bitmap)image).SetResolution(96, 96);
        /// </summary>
        public static void ConcatenateFalsecolourSpectrograms()
        {
            //DirectoryInfo dirInfo = new DirectoryInfo(@"G:\Documents\Karlina\BickertonIsSpectrograms_2013Dec-2014Jun");
            DirectoryInfo dirInfo = new DirectoryInfo(@"G:\Documents\Karlina\Bickerton 20131212_20140104Copy");
            FileInfo[] files = dirInfo.GetFiles();
            //FileInfo opPath = new FileInfo(@"G:\Documents\Karlina\BickertonIsSpectrograms_2013Dec-2014Jun.png");
            FileInfo opPath = new FileInfo(@"G:\Documents\Karlina\BickertonIsSpectrograms_2013Dec-2014Jan.png");

            double verticalScaleReduction = 0.4;
            int width = 785;
            System.Drawing.Image spacer = new Bitmap(width, 8);
            // float standardresolution = 96;
            float standardresolution = ((Bitmap)spacer).VerticalResolution;
            System.Drawing.Graphics g = Graphics.FromImage(spacer);
            g.Clear(Color.LightGray);

            var imageList = new List<Image>();
            foreach (FileInfo file in files)
            {
                System.Drawing.Image image = ImageTools.ReadImage2Bitmap(file.FullName);
                float verticalresolution = ((Bitmap)image).VerticalResolution;
                float horizontalresolution = ((Bitmap)image).HorizontalResolution;
                ((Bitmap)image).SetResolution(standardresolution, (float)(verticalresolution / verticalScaleReduction)); 
                imageList.Add(image);
                imageList.Add(spacer);
            }

            var opImage = ImageTools.CombineImagesVertically(imageList);
            opImage.Save(opPath.FullName);
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
