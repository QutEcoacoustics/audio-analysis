using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using TowseyLibrary;
using AudioAnalysisTools;
using AudioAnalysisTools.Indices;
using AudioAnalysisTools.StandardSpectrograms;
using AudioAnalysisTools.DSP;
using AudioAnalysisTools.WavTools;
using Acoustics.Shared;


namespace AnalysisPrograms
{
    using PowerArgs;
    using AudioAnalysisTools.LongDurationSpectrograms;

    public class Sandpit
    {
        public const int RESAMPLE_RATE = 17640;
        public const string imageViewer = @"C:\Windows\system32\mspaint.exe";

        public class Arguments
        {
        }

        public static void Dev(Arguments arguments)
        {

            //SET VERBOSITY
            DateTime tStart = DateTime.Now;
            Log.Verbosity = 1;
            Log.WriteLine("# Start Time = " + tStart.ToString());




            if (true)  // concatenating images with a gap between them.
            {
                var inputDirectory = new DirectoryInfo(@"Z:\Italy_GianniPavan\output4\Towsey.Acoustic");
                string opFileStem = "Sassofratino_24hours_v3";
                var outputDirectory = new DirectoryInfo(@"Z:\Italy_GianniPavan\output4\");

                string[] fileEntries = Directory.GetFiles(inputDirectory.FullName);

                List<Image> images = new List<Image>();
                bool interpolateSpacer = true;
                var imagePair = new Image[2];

                // timeing protocol
                int trackHeight = 20;
                int pixelColumnsPerHour = 60;
                int minutesBetweenRecordingStarts = 30;
                TimeSpan minOffset = TimeSpan.Zero;
                TimeSpan xAxisTicInterval = TimeSpan.FromMinutes(pixelColumnsPerHour); // assume 60 pixels per hour


                foreach (string path in fileEntries)
                {
                    // filter files.
                    if(! path.EndsWith("_000.2MAPS.png")) continue;
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

                int totalWidth = compositeBmp.Width;
                Bitmap timeBmp = Image_Track.DrawTimeTrack(totalWidth, minOffset, xAxisTicInterval, totalWidth, trackHeight, "hours");

                Graphics gr = Graphics.FromImage(compositeBmp);
                int halfHeight = compositeBmp.Height / 2;

                //add in the title bars
                string title = string.Format("24 hour FALSE-COLOUR SPECTROGRAM      (scale: hours x kHz)      (colour: R-G-B = {0})         (c) QUT.EDU.AU.  ", "BGN-AVG-CVR");
                Bitmap titleBmp = Image_Track.DrawTitleTrack(totalWidth, trackHeight, title);
                int offset = 0;
                gr.DrawImage(titleBmp, 0, offset); //draw in the top time scale
                title = string.Format("24 hour FALSE-COLOUR SPECTROGRAM      (scale: hours x kHz)      (colour: R-G-B = {0})         (c) QUT.EDU.AU.  ", "ACI-ENT-EVN");
                titleBmp = Image_Track.DrawTitleTrack(totalWidth, trackHeight, title);
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


                Log.WriteLine("GOT TO HERE");
                Console.ReadLine();
                System.Environment.Exit(0);
            }







            // code to merge all files of acoustic indeces derived from 24 hours of recording,
            // problem is that Jason cuts them up into 6 hour blocks.
            if (false)
            {
                string topLevelDirectory = @"C:\SensorNetworks\Output\SERF\SERFIndices_2013April01";
                string fileStem = "SERF_20130401";
                string[] names = {"SERF_20130401_000025_000",
                                  "SERF_20130401_064604_000",
                                  "SERF_20130401_133143_000",
                                  "SERF_20130401_201721_000",
                                      };

                // MUST MAKE SURE THE BELOW ARE CONSISTENT WITH THE DATA
                int sampleRate = 17640;
                int frameWidth = 256;



                //string topLevelDirectory = @"C:\SensorNetworks\Output\SERF\SERFIndices_2013June19";
                //string fileStem = "SERF_20130619";
                //string[] names = {"SERF_20130619_000038_000",
                //                  "SERF_20130619_064615_000",
                //                  "SERF_20130619_133153_000",
                //                  "SERF_20130619_201730_000",
                //                      };




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

                foreach (string extention in fileExtentions)
                {
                    Console.WriteLine("\n\nFILE TYPE: "+extention);

                    List<string> lines = new List<string>();

                    for(int i = 0; i < dirNames.Length; i++)
                    {
                        string fName = names[i] + extention;
                        string path = Path.Combine(dirNames[i], fName);
                        var fileInfo = new FileInfo(path);
                        Console.WriteLine(path);
                        if(! fileInfo.Exists)
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
                string title = String.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
                Image titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image1.Width);
                image1 = LDSpectrogramRGB.FrameSpectrogram(image1, titleBar, minuteOffset, cs1.XInterval, cs1.YInterval);
                image1.Save(Path.Combine(dirInfo.FullName, fileStem + "." + colorMap + ".png"));

                colorMap = "BGN-AVG-VAR";
                Image image2 = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);
                title = String.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
                titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image2.Width);
                image2 = LDSpectrogramRGB.FrameSpectrogram(image2, titleBar, minuteOffset, cs1.XInterval, cs1.YInterval);
                image2.Save(Path.Combine(dirInfo.FullName, fileStem + "." + colorMap + ".png"));
                Image[] array = new Image[2];
                array[0] = image1;
                array[1] = image2;
                Image image3 = ImageTools.CombineImagesVertically(array);
                image3.Save(Path.Combine(dirInfo.FullName, fileStem + ".2MAPS.png"));

            } // end if (true)



            // experiments with clustering the spectra within spectrograms
            if (false)
            {
                SpectralClustering.Sandpit();
            } // end if (true)
            



            // INPUT FILES
            //string ipdir = @"C:\SensorNetworks\Output\Test2\Towsey.Acoustic"; //KIWI FILES
            //string ipFileName = @"TEST_TUITCE_20091215_220004";

            string ipdir = @"C:\SensorNetworks\Output\SERF\2013MonthlyAveraged"; // SERF
            //string ipdir = @"C:\SensorNetworks\Output\TestSpectrograms";
            //string ipFileName = @"Test24hSpectrogram";


            // OUTPUT FILES
            //string opdir = @"C:\SensorNetworks\Output\Test2\tTestResults";
            //string opdir = @"C:\SensorNetworks\Output\SERF\2014Feb";
            string opdir = @"C:\SensorNetworks\Output\DifferenceSpectrograms\2014March13";


            // experiments with false colour images - categorising/discretising the colours
            if (false)
            {
                Console.WriteLine("Reading image");
                //string wavFilePath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav";
                //string inputPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.colSpectrum.png";
                //string outputPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.discreteColSpectrum.png";

                string inputPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\DM420036.colSpectrum.png";
                string outputPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\DM420036.discreteColSpectrum.png";

                const int R = 0;
                const int G = 1;
                const int B = 2;
                double[,] discreteIndices = new double[12, 3]; // Ht, ACI and Ampl values in 0,1
                discreteIndices[0, R] = 0.00; discreteIndices[0, G] = 0.00; discreteIndices[0, B] = 0.00; // white
                discreteIndices[1, R] = 0.20; discreteIndices[1, G] = 0.00; discreteIndices[1, B] = 0.00; // pale blue
                discreteIndices[2, R] = 0.60; discreteIndices[2, G] = 0.20; discreteIndices[2, B] = 0.10; // medium blue

                discreteIndices[3, R] = 0.00; discreteIndices[3, G] = 0.00; discreteIndices[3, B] = 0.40; // pale yellow
                discreteIndices[4, R] = 0.00; discreteIndices[4, G] = 0.05; discreteIndices[4, B] = 0.70; // bright yellow
                discreteIndices[5, R] = 0.20; discreteIndices[5, G] = 0.05; discreteIndices[5, B] = 0.80; // yellow/green
                discreteIndices[6, R] = 0.50; discreteIndices[6, G] = 0.05; discreteIndices[6, B] = 0.50; // yellow/green
                discreteIndices[7, R] = 0.99; discreteIndices[7, G] = 0.30; discreteIndices[7, B] = 0.70; // green

                discreteIndices[8, R] = 0.10; discreteIndices[8, G] = 0.95; discreteIndices[8, B] = 0.10;    // light magenta
                discreteIndices[9, R] = 0.50; discreteIndices[9, G] = 0.95; discreteIndices[9, B] = 0.50;    // medium magenta
                discreteIndices[10, R] = 0.70; discreteIndices[10, G] = 0.95; discreteIndices[10, B] = 0.70; // dark magenta
                discreteIndices[11, R] = 0.95; discreteIndices[11, G] = 0.95; discreteIndices[11, B] = 0.95; // black

                int N = 12; // number of discrete colours
                byte[,] discreteColourValues = new byte[N, 3]; // Ht, ACI and Ampl values in 0,255
                for (int r = 0; r < discreteColourValues.GetLength(0); r++)
                {
                    for (int c = 0; c < discreteColourValues.GetLength(1); c++)
                    {
                        discreteColourValues[r, c] = (byte)Math.Floor((1 - discreteIndices[r, c]) * 255);
                    }
                }

                // set up the colour pallette.
                Color[] colourPalette = new Color[N]; //palette
                for (int c = 0; c < N; c++)
                {
                    colourPalette[c] = Color.FromArgb(discreteColourValues[c, R], discreteColourValues[c, G], discreteColourValues[c, B]);
                }

                // read in the image
                Bitmap image = ImageTools.ReadImage2Bitmap(inputPath);
                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        Color imageCol = image.GetPixel(x, y);
                        byte[] imageColorVector = new byte[3];
                        imageColorVector[0] = imageCol.R;
                        imageColorVector[1] = imageCol.G;
                        imageColorVector[2] = imageCol.B;
                        // get colour from palette closest to the existing colour
                        double[] distance = new double[N];
                        for (int c = 0; c < N; c++)
                        {
                            byte[] colourVector = new byte[3];
                            colourVector[0] = discreteColourValues[c, 0];
                            colourVector[1] = discreteColourValues[c, 1];
                            colourVector[2] = discreteColourValues[c, 2];
                            distance[c] = DataTools.EuclidianDistance(imageColorVector, colourVector);
                        }
                        int minindex, maxindex;
                        double min, max;
                        DataTools.MinMax(distance, out minindex, out maxindex, out  min, out max);

                        //if ((col.R > 200) && (col.G > 200) && (col.B > 200))
                        image.SetPixel(x, y, colourPalette[minindex]);
                    }
                }
                ImageTools.WriteBitmap2File(image, outputPath);

            } // experiments with false colour images - categorising/discretising the colours

            Log.WriteLine("# Finished!");
        }


        public static Image DrawSonogram(BaseSonogram sonogram, Plot scores, List<AcousticEvent> poi, double eventThreshold, double[,] overlay)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            if (scores != null) image.AddTrack(Image_Track.GetNamedScoreTrack(scores.data, 0.0, 1.0, scores.threshold, scores.title));
            if ((poi != null) && (poi.Count > 0))
                image.AddEvents(poi, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            if (overlay != null)
            {
                var m = MatrixTools.ThresholdMatrix2Binary(overlay, 0.5);
                image.OverlayDiscreteColorMatrix(m);
            }
            return image.GetImage();
        } //DrawSonogram()

        public static Image DrawSonogram(BaseSonogram sonogram, Plot scores, int[,] overlay)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image.OverlayDiscreteColorMatrix(overlay);
            return image.GetImage();
        } //DrawSonogram()
    }
}
