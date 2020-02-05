// <copyright file="LDSpectrogramClusters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.Collections.Generic;
    using SixLabors.ImageSharp;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Acoustics.Shared;
    using Indices;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using SixLabors.Primitives;
    using TowseyLibrary;

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
    public static class LDSpectrogramClusters
    {
        ///// <summary>
        ///// This method merges all files of acoustic indices derived from a sequence of consecutive 6 hour recording,
        ///// that have a total duration of 24 hours. This was necesarry to deal with Jason's new regime of doing 24 hour recordings
        ///// in blocks of 6 hours.
        ///// </summary>
        //public static void ConcatenateSpectralIndexFiles1()
        //{
        //    // create an array that contains the names of csv file to be read.
        //    // The file names must be in the temporal order rquired for the resulting spectrogram image.
        //    string topLevelDirectory = @"C:\SensorNetworks\Output\SERF\SERFIndices_2013April01";
        //    string fileStem = "SERF_20130401";
        //    string[] names = {"SERF_20130401_000025_000",
        //                          "SERF_20130401_064604_000",
        //                          "SERF_20130401_133143_000",
        //                          "SERF_20130401_201721_000",
        //                              };
        //    //string topLevelDirectory = @"C:\SensorNetworks\Output\SERF\SERFIndices_2013June19";
        //    //string fileStem = "SERF_20130619";
        //    //string[] names = {"SERF_20130619_000038_000",
        //    //                  "SERF_20130619_064615_000",
        //    //                  "SERF_20130619_133153_000",
        //    //                  "SERF_20130619_201730_000",
        //    //                      };

        //    // ###############################################################
        //    // VERY IMPORTANT:  MUST MAKE SURE THE BELOW ARE CONSISTENT WITH THE DATA !!!!!!!!!!!!!!!!!!!!
        //    int sampleRate = 17640;
        //    int frameWidth = 256;
        //    // ###############################################################

        //    string[] level2Dirs = {names[0]+".wav",
        //                               names[1]+".wav",
        //                               names[2]+".wav",
        //                               names[3]+".wav",
        //                              };
        //    string level3Dir = "Towsey.Acoustic";
        //    string[] dirNames = {topLevelDirectory+@"\"+level2Dirs[0]+@"\"+level3Dir,
        //                             topLevelDirectory+@"\"+level2Dirs[1]+@"\"+level3Dir,
        //                             topLevelDirectory+@"\"+level2Dirs[2]+@"\"+level3Dir,
        //                             topLevelDirectory+@"\"+level2Dirs[3]+@"\"+level3Dir
        //                            };
        //    string[] fileExtentions = { ".ACI.csv",
        //                                    ".AVG.csv",
        //                                    ".BGN.csv",
        //                                    ".CVR.csv",
        //                                    ".TEN.csv",
        //                                    ".VAR.csv",
        //                                    "_Towsey.Acoustic.Indices.csv"
        //                                  };

        //    // this loop reads in all the Indices from consecutive csv files
        //    foreach (string extention in fileExtentions)
        //    {
        //        Console.WriteLine("\n\nFILE TYPE: " + extention);

        //        List<string> lines = new List<string>();

        //        for (int i = 0; i < dirNames.Length; i++)
        //        {
        //            string fName = names[i] + extention;
        //            string path = Path.Combine(dirNames[i], fName);
        //            var fileInfo = new FileInfo(path);
        //            Console.WriteLine(path);
        //            if (!fileInfo.Exists)
        //                Console.WriteLine("ABOVE FILE DOES NOT EXIST");

        //            var ipLines = FileTools.ReadTextFile(path);
        //            if (i != 0)
        //            {
        //                ipLines.RemoveAt(0); //remove the first line
        //            }
        //            lines.AddRange(ipLines);
        //        }
        //        string opFileName = fileStem + extention;
        //        string opPath = Path.Combine(topLevelDirectory, opFileName);
        //        FileTools.WriteTextFile(opPath, lines, false);

        //    } //end of all file extentions

        //    TimeSpan minuteOffset = TimeSpan.Zero; // assume recordings start at midnight
        //    TimeSpan xScale = TimeSpan.FromMinutes(60);
        //    double backgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF;
        //    string colorMap = SpectrogramConstants.RGBMap_ACI_ENT_CVR;
        //    var cs1 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap);
        //    cs1.BaseName = fileStem;
        //    cs1.ColorMode = colorMap;
        //    cs1.BackgroundFilter = backgroundFilterCoeff;
        //    var dirInfo = new DirectoryInfo(topLevelDirectory);
        //    cs1.ReadSpectralIndices(dirInfo, fileStem); // reads all known indices files
        //    if (cs1.GetCountOfSpectrogramMatrices() == 0)
        //    {
        //        Console.WriteLine("There are no spectrogram matrices in the dictionary.");
        //        return;
        //    }
        //    cs1.DrawGreyScaleSpectrograms(dirInfo, fileStem);

        //    colorMap = SpectrogramConstants.RGBMap_ACI_ENT_CVR;
        //    Image image1 = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);

        //    int nyquist = cs1.SampleRate / 2;
        //    int herzInterval = 1000;

        //    string title = String.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
        //    Image titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image1.Width);
        //    image1 = LDSpectrogramRGB.FrameLDSpectrogram(image1, titleBar, minuteOffset, cs1.IndexCalculationDuration, cs1.XTicInterval, nyquist, herzInterval);
        //    image1.Save(Path.Combine(dirInfo.FullName, fileStem + "." + colorMap + ".png"));

        //    colorMap = "BGN-AVG-VAR";
        //    Image image2 = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);
        //    title = String.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
        //    titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image2.Width);
        //    image2 = LDSpectrogramRGB.FrameLDSpectrogram(image2, titleBar, minuteOffset, cs1.IndexCalculationDuration, cs1.XTicInterval, nyquist, herzInterval);
        //    image2.Save(Path.Combine(dirInfo.FullName, fileStem + "." + colorMap + ".png"));
        //    Image[] array = new Image[2];
        //    array[0] = image1;
        //    array[1] = image2;
        //    Image image3 = ImageTools.CombineImagesVertically(array);
        //    image3.Save(Path.Combine(dirInfo.FullName, fileStem + ".2MAPS.png"));
        //}

        /// <summary>
        /// This method rearranges the content of a false-colour spectrogram according to the acoustic cluster or acoustic state to which each minute belongs.
        /// The time scale is added in afterwards - must overwrite the previous time scale and title bar.
        /// THis method was writtent to examine the cluster content of recordings analysed by Mangalam using a 10x10 SOM.
        /// The output image was used in the paper presented by Mangalam to BDVA2015 in Tasmania. (Big data, visual analytics).
        /// </summary>
        public static void ExtractSOMClusters1()
        {
            string opDir = @"C:\SensorNetworks\Output\Mangalam_BDVA2015\";

            //string fileStem = @"BYR2_20131016";
            //string inputImagePath = @"C:\SensorNetworks\Output\Mangalam_BDVA2015\BYR2_20131016.ACI-ENT-EVN.png";
            //string clusterFile = opDir + "SE 13 Oct - Cluster-node list.csv";

            //string fileStem = @"BYR2_20131017";
            //string inputImagePath = opDir + fileStem + ".ACI-ENT-EVN.png";
            //string clusterFile = opDir + "BY2-17Oct - node_clus_map.csv";

            string fileStem = @"SERF-SE_20101013";
            string inputImagePath = @"C:\SensorNetworks\Output\Mangalam_BDVA2015\SERF-SE_20101013.ACI-ENT-EVN.png";
            string clusterFile = opDir + "SE 13 Oct - Cluster-node list.csv";

            string opFileName = fileStem + ".SOMClusters.png";

            int clusterCount = 27;  // from fuzzy c-clustering
            int nodeCount = 100; // from the 10x10 SOM
            List<Pen> pens = ImageTools.GetColorPalette(clusterCount);
            Pen whitePen = new Pen(Color.White, 1);
            Pen blackPen = new Pen(Color.Black, 1);

            //SizeF stringSize = new SizeF();
            var stringFont = Drawing.Arial12Bold;

            //Font stringFont = Drawing.Tahoma9;

            // ###############################################################
            // VERY IMPORTANT:  MUST MAKE SURE THE BELOW ARE CONSISTENT WITH THE DATA !!!!!!!!!!!!!!!!!!!!
            int sampleRate = 22050;
            int frameWidth = 256;
            int nyquist = sampleRate / 2;
            int herzInterval = 1000;
            TimeSpan minuteOffset = TimeSpan.Zero; // assume recordings start at midnight
            double backgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF;
            string colorMap = SpectrogramConstants.RGBMap_ACI_ENT_EVN;
            string title = $"SOM CLUSTERS of ACOUSTIC INDICES: recording {fileStem}";
            TimeSpan indexCalculationDuration = TimeSpan.FromSeconds(60); // seconds
            TimeSpan xTicInterval = TimeSpan.FromMinutes(60); // 60 minutes or one hour.
            int trackheight = 20;

            // ###############################################################

            // read in the assignment of cluster numbers to cluster LABEL

            string[] clusterLabel = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a" };

            // read the data file
            List<string> lines = FileTools.ReadTextFile(clusterFile);
            int lineCount = lines.Count;
            int[] clusterHistogram = new int[clusterCount];

            //read in the image
            FileInfo fi = new FileInfo(inputImagePath);
            if (!fi.Exists)
            {
                Console.WriteLine("\n\n >>>>>>>> FILE DOES NOT EXIST >>>>>>: " + fi.Name);
            }

            Console.WriteLine("Reading file: " + fi.Name);
            Image<Rgb24> ipImage = Image.Load<Rgb24>(fi.FullName);
            int imageWidth = ipImage.Width;
            int imageHt = ipImage.Height;

            // init the output image
            var opImage = Drawing.NewImage(imageWidth, imageHt, Color.Black);
            
            // construct cluster histogram
            for (int lineNumber = 0; lineNumber < lineCount; lineNumber++)
            {
                string[] words = lines[lineNumber].Split(',');
                int clusterID = int.Parse(words[2]);
                clusterHistogram[clusterID - 1]++;
            }

            // ranks cluster counts in descending order
            Tuple<int[], int[]> tuple = DataTools.SortArray(clusterHistogram);
            int[] sortOrder = tuple.Item1;

            // this loop re
            int opColumn = 0;
            int clusterStartColumn = 0;
            for (int id = 0; id < clusterCount; id++)
            {
                int sortID = sortOrder[id];

                // create node array to store column images for this cluster
                List<Image<Rgb24>>[] nodeArray = new List<Image<Rgb24>>[nodeCount];
                for (int n = 0; n < nodeCount; n++)
                {
                    nodeArray[n] = new List<Image<Rgb24>>();
                }

                Console.WriteLine("Reading CLUSTER: " + (sortID + 1) + "  Label=" + clusterLabel[sortID]);

                // read through the entire list of minutes
                for (int lineNumber = 0; lineNumber < lineCount; lineNumber++)
                {
                    if (lineNumber == 0)
                    {
                        clusterStartColumn = opColumn;
                    }

                    string[] words = lines[lineNumber].Split(',');
                    int clusterID = int.Parse(words[2]) - 1; // -1 because matlab arrays start at 1.
                    int nodeID = int.Parse(words[1]) - 1;
                    if (clusterID == sortID)
                    {
                        // get image column
                        Rectangle rectangle = new Rectangle(lineNumber, 0, 1, imageHt);
                        Image<Rgb24> column = ipImage.Clone(x => x.Crop(rectangle));

                        nodeArray[nodeID].Add(column);
                    }
                }

                
                // cycle through the nodes and get the column images.
                // the purpose is to draw the column images in order of node number
                for (int n = 0; n < nodeCount; n++)
                {
                    int imageCount = nodeArray[n].Count;
                    if (nodeArray[n].Count == 0)
                    {
                        continue;
                    }

                    opImage.Mutate(gr =>
                    {
                        for (int i = 0; i < imageCount; i++)
                        {
                            Image<Rgb24> column = nodeArray[n][i];
                            gr.DrawImage(column, new Point(opColumn, 0), 1);
                            gr.DrawLine(pens[id], opColumn, trackheight, opColumn, trackheight + trackheight);
                            gr.DrawLine(pens[id], opColumn, imageHt - trackheight, opColumn, imageHt);
                            opColumn++;
                        }
                    });

                    //gr.DrawLine(blackPen, opColumn - 1, imageHt - trackheight, opColumn - 1, imageHt - 10);
                }

                    //FileInfo fi = new FileInfo(topLevelDirectory + name);
                    //Console.WriteLine("Reading file: " + fi.Name);

                if (id >= clusterCount - 1)
                {
                    break;
                }

                opImage.Mutate(gr =>
                {
                    gr.DrawLine(whitePen, opColumn - 1, 0, opColumn - 1, imageHt - trackheight - 1);
                    gr.DrawLine(blackPen, opColumn - 1, imageHt - trackheight, opColumn - 1, imageHt);
                    gr.DrawLine(blackPen, opColumn - 1, imageHt - trackheight, opColumn - 1, imageHt);

                    int location = opColumn - ((opColumn - clusterStartColumn) / 2);
                    gr.DrawText(clusterLabel[sortID], stringFont, Color.Black, new PointF(location - 10, imageHt - 19));
                });
            }

            opImage.Mutate(gr =>
            {
                ////Draw the title bar
                Image titleBar = DrawTitleBarOfClusterSpectrogram(title, imageWidth);
                gr.DrawImage(titleBar, 0, 0);
                ////Draw the x-axis time scale bar
                //int trackHeight = 20;
                //TimeSpan fullDuration = TimeSpan.FromTicks(indexCalculationDuration.Ticks * imageWidth);
                //Image<Rgb24> timeBmp = ImageTrack.DrawTimeTrack(fullDuration, TimeSpan.Zero, imageWidth, trackHeight);

                //spgmImage = LDSpectrogramRGB.FrameLDSpectrogram(spgmImage, titleBar, minuteOffset, indexCalculationDuration, xTicInterval, nyquist, herzInterval);
                //Graphics gr = Graphics.FromImage(spgmImage);
                ////gr.Clear(Color.Black);
                //gr.DrawImage(titleBar, 0, 0); //draw in the top spectrogram
                //gr.DrawImage(timeBmp, 0, 20); //draw in the top spectrogram
                //gr.DrawImage(timeBmp, 0, imageHeight - 20); //draw in the top spectrogram
            });

            opImage.Save(Path.Combine(opDir, opFileName));
        }

        /// <summary>
        /// This method rearranges the content of a false-colour spectrogram according to the acoustic cluster or acoustic state to which each minute belongs.
        /// The time scale is added in afterwards - must overwrite the previous time scale and title bar.
        /// THis method was writtent to examine the cluster content of recordings analysed by Mangalam using a 10x10 SOM.
        /// The output image was used in the paper presented by Michael Towsey to Ecoacoustics Congress 2016, at Michigan State University.
        /// </summary>
        public static void ExtractSOMClusters2()
        {
            string opDir = @"C:\SensorNetworks\Output\Mangalam_EcoAcCongress2016\";
            string clusterFile = opDir + "Minute_cluster mapping - all.csv";

            //string inputImagePath = @"C:\SensorNetworks\Output\Mangalam_EcoAcCongress2016\SERF Spectrogram SW 2010Oct14.png";
            string inputImagePath = @"C:\SensorNetworks\Output\Mangalam_EcoAcCongress2016\SERF Spectrogram NW 2010Oct14.png";
            string fileStem = "NW_14Oct";

            //string fileStem = "SW_14Oct";
            string opFileName = fileStem + ".SOM27AcousticClusters.png";
            string title = $"SOM CLUSTERS of ACOUSTIC INDICES: recording {fileStem}";

            int clusterCount = 27;  // from Yvonne's method
            List<Pen> pens = ImageTools.GetColorPalette(clusterCount);
            Pen whitePen = new Pen(Color.White, 1);
            Pen blackPen = new Pen(Color.Black, 1);

            //SizeF stringSize = new SizeF();
            var stringFont = Drawing.Arial12Bold;

            //Font stringFont = Drawing.Tahoma9;

            // assignment of cluster numbers to cluster LABEL
            string[] clusterLabel = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a" };

            // read the data file containing cluster sequence
            List<string> lines = FileTools.ReadTextFile(clusterFile);
            string[] words = null;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartsWith(fileStem))
                {
                    words = lines[i].Split(',');
                    break;
                }
            }

            // init histogram to accumulate the cluster counts
            int[] clusterHistogram = new int[clusterCount];

            // init array of lists to know what minutes are assigned to what clusters.
            List<int>[] clusterArrays = new List<int>[clusterCount];
            for (int i = 0; i < clusterCount; i++)
            {
                clusterArrays[i] = new List<int>();
            }

            // construct cluster histogram and arrays
            for (int w = 1; w < words.Length; w++)
            {
                int clusterID = int.Parse(words[w]);
                clusterHistogram[clusterID - 1]++;
                clusterArrays[clusterID - 1].Add(w);
            }

            // ranks cluster counts in descending order
            Tuple<int[], int[]> tuple = DataTools.SortArray(clusterHistogram);
            int[] sortOrder = tuple.Item1;

            //read in the image
            FileInfo fi = new FileInfo(inputImagePath);
            if (!fi.Exists)
            {
                Console.WriteLine("\n\n >>>>>>>> FILE DOES NOT EXIST >>>>>>: " + fi.Name);
            }

            Console.WriteLine("Reading file: " + fi.Name);
            Image<Rgb24> ipImage =Image.Load<Rgb24>(fi.FullName);
            int imageWidth = ipImage.Width;
            int imageHt = ipImage.Height;

            //init the output image
            int opImageWidth = imageWidth + (2 * clusterCount);
            var opImage = Drawing.NewImage(opImageWidth, imageHt, Color.Black);

            // this loop re
            opImage.Mutate(gr =>
            {
                int opColumnNumber = 0;
                int clusterStartColumn = 0;
                for (int id = 0; id < clusterCount; id++)
                {
                    int sortID = sortOrder[id];

                    Console.WriteLine("Reading CLUSTER: " + (sortID + 1) + "  Label=" + clusterLabel[sortID]);
                    int[] minutesArray = clusterArrays[sortID].ToArray();
                    clusterStartColumn = opColumnNumber;

                    // read through the entire list of minutes
                    for (int m = 0; m < minutesArray.Length; m++)
                    {
                        // get image column
                        Rectangle rectangle = new Rectangle(minutesArray[m] - 1, 0, 1, imageHt);
                        Image<Rgb24> column = ipImage.Clone(x =>x.Crop(rectangle));
                        gr.DrawImage(column, new Point(opColumnNumber, 0), 1);
                        opColumnNumber++;
                    }

                    // draw in separators
                    gr.DrawLine(whitePen, opColumnNumber, 0, opColumnNumber, imageHt - 1);
                    opColumnNumber++;
                    gr.DrawLine(whitePen, opColumnNumber, 0, opColumnNumber, imageHt - 1);
                    opColumnNumber++;

                    // draw Cluster ID at bottom of the image
                    if (minutesArray.Length > 3)
                    {
                        Image<Rgb24> clusterIDImage = new Image<Rgb24>(minutesArray.Length,
                            SpectrogramConstants.HEIGHT_OF_TITLE_BAR - 6);
                        clusterIDImage.Mutate(g2 => { g2.Clear(Color.Black); });
                        gr.DrawImage(clusterIDImage, new Point(clusterStartColumn, imageHt - 19), 1);
                        int location = opColumnNumber - ((opColumnNumber - clusterStartColumn) / 2);
                        gr.DrawText(clusterLabel[sortID], stringFont, Color.White, new PointF(location - 10, imageHt - 19));
                    }
                }

                //Draw the title bar
                Image titleBar = DrawTitleBarOfClusterSpectrogram(title, opImageWidth - 2);
                gr.DrawImage(titleBar, new Point(1, 0), 1);
            });
            opImage.Save(Path.Combine(opDir, opFileName));
        }

        public static Image DrawTitleBarOfClusterSpectrogram(string title, int width)
        {
            var bmp = Drawing.NewImage(width, SpectrogramConstants.HEIGHT_OF_TITLE_BAR - 3, Color.Black);
            
            Pen pen = new Pen(Color.White, 1);
            var stringFont = Drawing.Arial12Bold;

            bmp.Mutate(g =>
            {
                SizeF stringSize = new SizeF();

                int X = 4;
                g.DrawText(title, stringFont, Color.Wheat, new PointF(X, 3));

                stringSize = g.MeasureString(title, stringFont);
                X += stringSize.ToSize().Width + 70;

                string text = Meta.OrganizationTag;
                stringSize = g.MeasureString(text, stringFont);
                int X2 = width - stringSize.ToSize().Width - 2;
                if (X2 > X)
                {
                    g.DrawText(text, stringFont, Color.Wheat, new PointF(X2, 3));
                }

                g.DrawLine(new Pen(Color.Gray, 1), 0, 0, width, 0); //draw upper boundary
            });

            return bmp;
        }
    }
}
