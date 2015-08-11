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
        ///         /// SHOULD BE DEPRACATED!
        /// This method merges all files of acoustic indices derived from a sequence of consecutive 6 hour recording, 
        /// that have a total duration of 24 hours. This was necesarry to deal with Jason's new regime of doing 24 hour recordings 
        /// in blocks of 6 hours. 
        /// </summary>
        public static void ConcatenateSpectralIndexFiles1()
        {
            // create an array that contains the names of csv file to be read.
            // The file names must be in the temporal order rquired for the resulting spectrogram image.
            string topLevelDirectory = @"C:\SensorNetworks\Output\SERF\SERFIndices_2013April01";
            string fileStem = "SERF_20130401";
            string[] names = {"SERF_20130401_000025_000",
                              "SERF_20130401_064604_000",
                              "SERF_20130401_133143_000",
                              "SERF_20130401_201721_000",
                                      };
            //string topLevelDirectory = @"C:\SensorNetworks\Output\SERF\SERFIndices_2013June19";
            //string fileStem = "SERF_20130619";
            //string[] names = {"SERF_20130619_000038_000",
            //                  "SERF_20130619_064615_000",
            //                  "SERF_20130619_133153_000",
            //                  "SERF_20130619_201730_000",
            //                      };

            string indexPropertiesConfigPath = @"C:\SensorNetworks\Output\SERF\SERFIndices_2013April01\IndexPropertiesOLDConfig.yml";


            string outputDirectory = @"C:\SensorNetworks\Output\SERF\SERFIndices_2013April01";
            DirectoryInfo dirInfo = new DirectoryInfo(outputDirectory);

            var ldSpectrogramConfig = new LdSpectrogramConfig
            {
                XAxisTicInterval = SpectrogramConstants.X_AXIS_TIC_INTERVAL,
                YAxisTicInterval = 1000,
                ColorMap1 = "ACI-TEN-CVR",
                ColorMap2 = "BGN-AVG-VAR",
                //ColorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN,
                //ColorMap2 = SpectrogramConstants.RGBMap_BGN_POW_EVN,
                //ColorMap2 = SpectrogramConstants.RGBMap_BGN_POW_CVR,
            };

            //string colorMap2 = SpectrogramConstants.RGBMap_BGN_AVG_CVR;
            //string[] keys = {"ACI", "TEN", "CVR", "BGN", "AVG", "VAR"};
            string[] keys = ldSpectrogramConfig.GetKeys();

            string analysisType = "Towsey.Acoustic";

            // ###############################################################
            // VERY IMPORTANT:  MUST MAKE SURE THE BELOW ARE CONSISTENT WITH THE DATA !!!!!!!!!!!!!!!!!!!!
            int sampleRate = 17640;
            int frameWidth = 256;
            int nyquist = sampleRate / 2;
            int herzInterval = 1000;
            // ###############################################################


            FileInfo[] paths = ConcatenateSpectralIndexFiles(topLevelDirectory, fileStem, names);
            Dictionary<string, double[,]> indexSpectrograms = IndexMatrices.ReadCSVFiles(paths, keys);

            FileInfo indexPropertiesConfigFileInfo  = new FileInfo(indexPropertiesConfigPath);

            var icdPath = Path.Combine(topLevelDirectory, fileStem + "__" + IndexGenerationData.FileNameFragment + ".json"); 
            FileInfo icdFileInfo = icdPath.ToFileInfo();
            IndexGenerationData indexGenerationData = Json.Deserialise<IndexGenerationData>(icdFileInfo);



            Dictionary<string, IndexDistributions.SpectralStats> indexDistributions = null;
            SummaryIndexBase[] summaryIndices = null;
            bool returnChromelessImages = false;

            Tuple<Image, string>[] tuple =  LDSpectrogramRGB.DrawSpectrogramsFromSpectralIndices(
            dirInfo,
            dirInfo,
            ldSpectrogramConfig,
            indexPropertiesConfigFileInfo,
            indexGenerationData,
            fileStem,
            analysisType,
            indexSpectrograms,
            summaryIndices,
            indexDistributions,
            returnChromelessImages);

        }




        /// <summary>
        ///         /// SHOULD BE DEPRACATED!
        /// This method merges all files of acoustic indices derived from a sequence of consecutive 6 hour recording, 
        /// that have a total duration of 24 hours. This was necesarry to deal with Jason's new regime of doing 24 hour recordings 
        /// in blocks of 6 hours. 
        /// </summary>
        public static FileInfo[] ConcatenateSpectralIndexFiles(string topLevelDirectory, string fileStem, string[] names)
        {
            string[] level2Dirs = { names[0]+".wav",
                                    names[1]+".wav",
                                    names[2]+".wav",
                                    names[3]+".wav",
                                  };
            string level3Dir = "Towsey.Acoustic";
            string[] dirNames = {   topLevelDirectory+@"\"+level2Dirs[0]+@"\"+level3Dir,
                                    topLevelDirectory+@"\"+level2Dirs[1]+@"\"+level3Dir,
                                    topLevelDirectory+@"\"+level2Dirs[2]+@"\"+level3Dir,
                                    topLevelDirectory+@"\"+level2Dirs[3]+@"\"+level3Dir
                                };
            string[] fileExtentions = { ".ACI.csv",
                                        ".AVG.csv",
                                        ".BGN.csv",
                                        ".CVR.csv",
                                        ".TEN.csv",
                                        ".VAR.csv",
                                        "_Towsey.Acoustic.Indices.csv"
                                        };
            var paths = new List<FileInfo>();

            // this loop reads in all the Indices from consecutive csv files
            foreach (string extention in fileExtentions)
            {
                Console.WriteLine("\n\nFILE TYPE: " + extention);

                List<string> lines = new List<string>();

                for (int i = 0; i < dirNames.Length; i++)
                {
                    string fName = names[i] + extention;
                    string path = Path.Combine(dirNames[i], fName);
                    var fileInfo = new FileInfo(path);
                    Console.WriteLine(path);
                    if (!fileInfo.Exists)
                        Console.WriteLine("ABOVE FILE DOES NOT EXIST");

                    var ipLines = FileTools.ReadTextFile(path);
                    if (i != 0)
                    {
                        ipLines.RemoveAt(0); //remove the first line
                    }
                    lines.AddRange(ipLines);
                }
                string opFileName = fileStem + "__Towsey.Acoustic" + extention;
                string opPath = Path.Combine(topLevelDirectory, opFileName);
                FileTools.WriteTextFile(opPath, lines, false);

                paths.Add(new FileInfo(opPath));
            } //end of all file extentions

            return paths.ToArray();
        }



        /// <summary>
        /// MOST RECENT METHOD TO CONCATENATE INDEX.CSV FILES - August 2015.
        /// This method merges all files of acoustic indices derived from a sequence of consecutive 1/2 to 6 hour recordings, 
        /// that have a total duration of 24 hours. This was necessary to deal with the new regime of doing 24 hour recordings 
        /// in conseutive short segments. 
        /// IMPORTANT NOTE: THIS METHOD DOES NOT CHECK FOR TEMPORAL GAPS BETWEEN THE STITCHED CSV FILES!
        ///                 SEE METHOD ABOVE WHICH DOES CHECK -- StitchPartialSpectrograms()
        /// </summary>
        public static void ConcatenateSpectralIndexFiles2(DirectoryInfo topLevelDirectory,
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
            var dictionary = IndexMatrices.GetSpectralIndexFilesAndConcatenate(path, keys);

            // get first file name from sorted list
            string pattern = "*ACI.csv";
            FileInfo[] files = IndexMatrices.GetFilesInDirectory(topLevelDirectory.FullName, pattern);

            // get the IndexGenerationData file from the first directory
            DirectoryInfo firstDirectory = files[0].Directory;
            pattern = "*__" + IndexGenerationData.FileNameFragment + ".json";
            FileInfo igdFile = IndexMatrices.GetFilesInDirectory(firstDirectory.FullName, pattern).Single();
            IndexGenerationData indexGenerationData = Json.Deserialise<IndexGenerationData>(igdFile);

            // Get the start time from the first file in sort list.
            // UNNECESARRY CODE????? Anthony has this one already done somewhere else!!!!!!!!!!!!!!!!!!
            pattern = @"20\d\d\d\d\d\d_\d\d\d\d\d\d_";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            Match m = r.Match(files[0].Name); // get name of first file

            Group group = m.Groups[0];
            string[] array = group.ToString().Split('_');
            int year = Int32.Parse(array[0].Substring(0, 4));
            int mnth = Int32.Parse(array[0].Substring(4, 2));
            int day = Int32.Parse(array[0].Substring(6, 2));
            int hour = Int32.Parse(array[1].Substring(0, 2));
            int min = Int32.Parse(array[1].Substring(2, 2));
            int sec = Int32.Parse(array[1].Substring(4, 2));
            DateTimeOffset startTime = new DateTimeOffset(year, mnth, day, hour, min, sec, TimeSpan.Zero);
            indexGenerationData.RecordingStartDate = startTime;

            Dictionary<string, IndexDistributions.SpectralStats> indexDistributions = null;
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
            returnChromelessImages);
        }



        /// <summary>
        /// MOST RECENT METHOD TO CONCATENATE SUMMARY INDEX.CSV FILES - August 2015.
        /// This method merges all files of acoustic indices derived from a sequence of consecutive 1/2 to 6 hour recordings, 
        /// that have a total duration of 24 hours. This was necessary to deal with the new regime of doing 24 hour recordings 
        /// in conseutive short segments. 
        /// </summary>
        public static void ConcatenateSummaryIndexFiles(DirectoryInfo topLevelDirectory,
                                                          FileInfo indexPropertiesConfig,
                                                          DirectoryInfo opDir,
                                                          string opFileStem)
        {
            string ImagefileExt = "png";

            //20150704_143000_Day1__Towsey.Acoustic.Indices
            string analysisType = "__Towsey.Acoustic.Indices.csv";
            string pattern = "*" + analysisType;

            var ldSpectrogramConfig = new LdSpectrogramConfig
            {
                XAxisTicInterval = SpectrogramConstants.X_AXIS_TIC_INTERVAL,
                YAxisTicInterval = 1000,
                ColorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN,
                ColorMap2 = SpectrogramConstants.RGBMap_BGN_POW_EVN,
            };
            string path = topLevelDirectory.FullName;

            FileInfo[] files = IndexMatrices.GetFilesInDirectory(path, pattern);
            var summaryDataTuple  = IndexMatrices.GetSummaryIndexFilesAndConcatenate(files);
            string[] headers = summaryDataTuple.Item1;
            double[,] summaryIndices = summaryDataTuple.Item2;
            Dictionary<string, double[]> dictionaryOfCsvColumns = IndexMatrices.ConvertCsvData2DictionaryOfColumns(headers, summaryIndices);


            var indicesFile    = FilenameHelpers.AnalysisResultName(topLevelDirectory, opFileStem, "Indices", "csv");
            var indicesCsvfile = new FileInfo(indicesFile);
            //serialiseFunc(indicesFile, results);
            //Csv.WriteMatrixToCsv(indicesCsvfile, summaryIndices);
            CsvTools.WriteMatrix2CSV(summaryIndices, headers, indicesCsvfile);

            // get the IndexGenerationData file from the first directory
            DirectoryInfo firstDirectory = files[0].Directory;
            pattern = "*__" + IndexGenerationData.FileNameFragment + ".json";
            FileInfo igdFile = IndexMatrices.GetFilesInDirectory(firstDirectory.FullName, pattern).Single();
            IndexGenerationData indexGenerationData = Json.Deserialise<IndexGenerationData>(igdFile);

            // Get the start time from the first file in sort list.
            // UNNECESARRY CODE????? Anthony has this one already done somewhere else!!!!!!!!!!!!!!!!!!
            pattern = @"20\d\d\d\d\d\d_\d\d\d\d\d\d_";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            Match m = r.Match(files[0].Name); // get name of first file

            Group group = m.Groups[0];
            string[] array = group.ToString().Split('_');
            int year = Int32.Parse(array[0].Substring(0, 4));
            int mnth = Int32.Parse(array[0].Substring(4, 2));
            int day = Int32.Parse(array[0].Substring(6, 2));
            int hour = Int32.Parse(array[1].Substring(0, 2));
            int min = Int32.Parse(array[1].Substring(2, 2));
            int sec = Int32.Parse(array[1].Substring(4, 2));
            DateTimeOffset startTime = new DateTimeOffset(year, mnth, day, hour, min, sec, TimeSpan.Zero);
            indexGenerationData.RecordingStartDate = startTime;

            //Dictionary<string, IndexDistributions.SpectralStats> indexDistributions = null;
            //SummaryIndexBase[] summaryIndices = null;

            //string fileName = Path.GetFileNameWithoutExtension(fileNameBase);
            string fileName = opFileStem;
            string imageTitle = string.Format("SOURCE:{0},   (c) QUT;  ", fileName);
            Bitmap tracksImage =
                DrawSummaryIndices.DrawImageOfSummaryIndices(
                    IndexProperties.GetIndexProperties(indexPropertiesConfig),
                    dictionaryOfCsvColumns,
                    imageTitle);
            var imagePath = FilenameHelpers.AnalysisResultName(topLevelDirectory, fileName, "Indices", ImagefileExt);
            tracksImage.Save(imagePath);
        }




        /// <summary>
        /// SHOULD BE DEPRACATED!
        /// This method merges the LDSpectrogram IMAGES derived from a sequence of consecutive 6-12 hour recording, 
        /// that have a total duration of 24 hours. This was necesarry to deal with Jason's new regime of doing 24-hour recordings 
        /// in shorter blocks of 3-12 hours. 
        /// This method differs from the above in that we are concatnating already prepared images as opposed to the index.csv files.
        /// The time scale is added in afterwards - must over-write the previous time scale and title bar.
        /// </summary>
        public static void ConcatenateSpectralIndexImages()
        {
            // create an array that contains the names of csv file to be read.
            // The file names must be in the temporal order rquired for the resulting spectrogram image.

            //string topLevelDirectory = @"Y:\Results\2015May07-121245 - SERF MtByron SunnyCoast\Mt Byron\Creek 1\";
            //string fileStem =  "BYR2_20131016";
            //string[] names = {@"BYR2_20131016_000000.wav\Towsey.Acoustic\BYR2_20131016_000000__ACI-ENT-EVN.png",
            //                  @"BYR2_20131016_133121.wav\Towsey.Acoustic\BYR2_20131016_133121__ACI-ENT-EVN.png",
            //                 };

            //string topLevelDirectory = @"Y:\Results\2015May07-121245 - SERF MtByron SunnyCoast\Mt Byron\Creek 1\";
            //string fileStem = "BYR2_20131017";
            //string[] names = {@"BYR2_20131017_000000.wav\Towsey.Acoustic\BYR2_20131017_000000__ACI-ENT-EVN.png",
            //                  @"BYR2_20131017_133121.wav\Towsey.Acoustic\BYR2_20131017_133121__ACI-ENT-EVN.png",
            //                 };
            string topLevelDirectory = @"Y:\Results\2015May07-121245 - SERF MtByron SunnyCoast\Mt Byron\PRA\";
            string fileStem = "BYR4_20131017";
            string[] names = {@"BYR4_20131017_000000.wav\Towsey.Acoustic\BYR4_20131017_000000__ACI-ENT-EVN.png",
                              @"BYR4_20131017_064544.wav\Towsey.Acoustic\BYR4_20131017_064544__ACI-ENT-EVN.png",
                              @"BYR4_20131017_133128.wav\Towsey.Acoustic\BYR4_20131017_133128__ACI-ENT-EVN.png",
                              @"BYR4_20131017_201713.wav\Towsey.Acoustic\BYR4_20131017_201713__ACI-ENT-EVN.png",
                             };

            string opDir = @"C:\SensorNetworks\Output\Mangalam_BDVA2015";

            // ###############################################################
            // VERY IMPORTANT:  MUST MAKE SURE THE BELOW ARE CONSISTENT WITH THE DATA !!!!!!!!!!!!!!!!!!!!
            int sampleRate = 22050;
            int frameWidth = 256;
            int nyquist    = sampleRate / 2;
            int herzInterval = 1000;
            TimeSpan minuteOffset = TimeSpan.Zero; // assume recordings start at midnight
            double backgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF;
            string colorMap = SpectrogramConstants.RGBMap_ACI_ENT_CVR;
            string title = String.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
            TimeSpan indexCalculationDuration = TimeSpan.FromSeconds(60); // seconds
            TimeSpan xTicInterval = TimeSpan.FromMinutes(60); // 60 minutes or one hour.
            // ###############################################################

            List<Image> imageList = new List<Image>();
            // this loop reads in all the file names
            foreach (string name in names)
            {
                FileInfo fi = new FileInfo(topLevelDirectory + name);
                Console.WriteLine("Reading file: " + fi.Name);
                Image image = ImageTools.ReadImage2Bitmap(fi.FullName);
                imageList.Add(image);
            } //end of all file names

            Image spgmImage = ImageTools.CombineImagesInLine(imageList);
            int imageWidth  = spgmImage.Width;
            int imageHeight = spgmImage.Height;


            //Draw the title bar
            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, imageWidth);
            //Draw the x-axis time scale bar
            int trackHeight = 20;
            TimeSpan fullDuration = TimeSpan.FromTicks(indexCalculationDuration.Ticks * imageWidth);
            Bitmap timeBmp = Image_Track.DrawTimeTrack(fullDuration, null, imageWidth, trackHeight);

            //spgmImage = LDSpectrogramRGB.FrameLDSpectrogram(spgmImage, titleBar, minuteOffset, indexCalculationDuration, xTicInterval, nyquist, herzInterval);
            Graphics gr = Graphics.FromImage(spgmImage);
            //gr.Clear(Color.Black);
            gr.DrawImage(titleBar, 0, 0); //draw in the top spectrogram
            gr.DrawImage(timeBmp, 0, 20); //draw in the top spectrogram
            gr.DrawImage(timeBmp, 0, imageHeight - 20); //draw in the top spectrogram

            spgmImage.Save(Path.Combine(opDir, fileStem + "." + colorMap + ".png"));
        }


    }
}
