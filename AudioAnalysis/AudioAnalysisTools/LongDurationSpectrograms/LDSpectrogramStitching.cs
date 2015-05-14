using AudioAnalysisTools.Indices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using TowseyLibrary;

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


            // ###############################################################
            // VERY IMPORTANT:  MUST MAKE SURE THE BELOW ARE CONSISTENT WITH THE DATA !!!!!!!!!!!!!!!!!!!!
            int sampleRate = 17640;
            int frameWidth = 256;
            // ###############################################################


            string[] level2Dirs = {names[0]+".wav",
                                       names[1]+".wav",
                                       names[2]+".wav",
                                       names[3]+".wav",
                                      };
            string level3Dir = "Towsey.Acoustic";
            string[] dirNames = {topLevelDirectory+@"\"+level2Dirs[0]+@"\"+level3Dir,
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
                string opFileName = fileStem + extention;
                string opPath = Path.Combine(topLevelDirectory, opFileName);
                FileTools.WriteTextFile(opPath, lines, false);



            } //end of all file extentions

            TimeSpan minuteOffset = TimeSpan.Zero; // assume recordings start at midnight
            TimeSpan xScale = TimeSpan.FromMinutes(60);
            double backgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF;
            string colorMap = SpectrogramConstants.RGBMap_ACI_ENT_CVR;
            var cs1 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap);
            cs1.FileName = fileStem;
            cs1.ColorMode = colorMap;
            cs1.BackgroundFilter = backgroundFilterCoeff;
            var dirInfo = new DirectoryInfo(topLevelDirectory);
            cs1.ReadCSVFiles(dirInfo, fileStem); // reads all known indices files
            if (cs1.GetCountOfSpectrogramMatrices() == 0)
            {
                Console.WriteLine("There are no spectrogram matrices in the dictionary.");
                return;
            }
            cs1.DrawGreyScaleSpectrograms(dirInfo, fileStem);

            colorMap = SpectrogramConstants.RGBMap_ACI_ENT_CVR;
            Image image1 = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);

            int nyquist = cs1.SampleRate / 2;
            int herzInterval = 1000;

            string title = String.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image1.Width);
            image1 = LDSpectrogramRGB.FrameLDSpectrogram(image1, titleBar, minuteOffset, cs1.IndexCalculationDuration, cs1.XTicInterval, nyquist, herzInterval);
            image1.Save(Path.Combine(dirInfo.FullName, fileStem + "." + colorMap + ".png"));

            colorMap = "BGN-AVG-VAR";
            Image image2 = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);
            title = String.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
            titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image2.Width);
            image2 = LDSpectrogramRGB.FrameLDSpectrogram(image2, titleBar, minuteOffset, cs1.IndexCalculationDuration, cs1.XTicInterval, nyquist, herzInterval);
            image2.Save(Path.Combine(dirInfo.FullName, fileStem + "." + colorMap + ".png"));
            Image[] array = new Image[2];
            array[0] = image1;
            array[1] = image2;
            Image image3 = ImageTools.CombineImagesVertically(array);
            image3.Save(Path.Combine(dirInfo.FullName, fileStem + ".2MAPS.png"));
        }




        /// <summary>
        /// This method merges the LDSpectrogram IMAGES derived from a sequence of consecutive 6-12 hour recording, 
        /// that have a total duration of 24 hours. This was necesarry to deal with Jason's new regime of doing 24-hour recordings 
        /// in shorter blocks of 3-12 hours. 
        /// This method differes form the above in that we are concatnating already prepared images as opposed to the index.csv files.
        /// The time scale is added in afterwards - must poverwrite the previous time scale and title bar.
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
            Bitmap timeBmp = Image_Track.DrawTimeTrack(fullDuration, TimeSpan.Zero, imageWidth, trackHeight);

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
