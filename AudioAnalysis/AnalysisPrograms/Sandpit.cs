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
using AudioAnalysisTools.LongDurationSpectrograms;
using AudioAnalysisTools.DSP;
using AudioAnalysisTools.WavTools;
using Acoustics.Shared;

namespace AnalysisPrograms
{
    using PowerArgs;


    /// <summary>
    /// Activity Code for this class:= sandpit
    ///
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
    /// </summary>
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

            if (false)  // 
            {
                CubeHelix.DrawTestImage();
                LoggedConsole.WriteLine("FINSIHED");
            }

            if (false)  // construct 3Dimage of audio
            {
                //TowseyLibrary.Matrix3D.TestMatrix3dClass();
                LDSpectrogram3D.Main(null);
                LoggedConsole.WriteLine("FINSIHED");
            }

            if (false)  // call SURF image Feature extraction
            {
                //SURFFeatures.SURF_TEST();
                SURFAnalysis.Main(null);
                LoggedConsole.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(0);
            }


            if (false)  // do test of SNR calculation
            {
                //Audio2InputForConvCNN.Main(null);
                Audio2InputForConvCNN.ProcessMeriemsDataset();
                //SNR.Calculate_SNR_ofXueyans_data();
                LoggedConsole.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(0);
            }




            if (false)  // do test of new moving average method
            {
                DataTools.TEST_FilterMovingAverage();
            }


            if (false)
            {
                ImageTools.TestCannyEdgeDetection();
            }


            if (false)
            {
                //HoughTransform.Test1HoughTransform();
                HoughTransform.Test2HoughTransform();
            }


            if (false)  // used to test structure tensor code.
            {
                StructureTensor.Test1StructureTensor();
                StructureTensor.Test2StructureTensor();
                Log.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(0);
            }


            /// used to caluclate eigen values and singular valuse
            if (false)
            {

                SvdAndPca.TestEigenValues();
                Log.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(0);
            }


            if (false)  // test examples of wavelets
            {
                WaveletTransformContinuous.ExampleOfWavelets_1();
                //WaveletPacketDecomposition.ExampleOfWavelets_1();
                Log.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(0);
            }


            if (false)  // do 2D-FFT of an image.
            {
                FFT2D.TestFFT2D();
                Log.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(0);
            }



            if (false)  // concatenating spectrogram images with gaps between them.
            {
                LDSpectrogramStitching.StitchPartialSpectrograms();
                LDSpectrogramStitching.StitchPartialSpectrograms();

                Log.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(0);
            }


            // code to merge all files of acoustic indeces derived from 24 hours of recording,
            if (false)
            {
                //LDSpectrogramStitching.ConcatenateSpectralIndexFiles1(); //DEPRACATED
                //LDSpectrogramStitching.ConcatenateSpectralIndexImages();
                //LDSpectrogramClusters.ExtractSOMClusters();
            } // end if (true)


            // PAPUA NEW GUINEA DATA
            // concatenating csv files of spectral and summary indices
            if (false)
            {
                // top level directory
                //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_4-7-15\BAR\BAR_32\";
                //string opFileStem = "TNC_Iwarame_20150704_BAR32";

                //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_4-7-15\BAR\BAR_33\";
                //string opFileStem = "TNC_Iwarame_20150704_BAR33";

                //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_4-7-15\BAR\BAR_35\";
                //string opFileStem = "TNC_Iwarame_20150704_BAR35";

                //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_7-7-15\BAR\BAR_59\";
                //string opFileStem = "TNC_Iwarame_20150707_BAR59";

                //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_9-7-15\BAR\BAR_79\";
                //string opFileStem = "TNC_Iwarame_20150709_BAR79";

                //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Yavera_8-7-15\BAR\BAR_64\";
                //string opFileStem = "TNC_Yavera_20150708_BAR64";

                string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Musiamunat_3-7-15\BAR\BAR_18\";
                string opFileStem = "TNC_Musiamunat_20150703_BAR18";


                DirectoryInfo[] dataDir = { new DirectoryInfo(dataPath) };

                string indexPropertiesConfigPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\IndexPropertiesOLDConfig.yml";
                FileInfo indexPropertiesConfigFileInfo = new FileInfo(indexPropertiesConfigPath);

                // string outputDirectory = @"C:\SensorNetworks\Output\Test\TNC";
                var opDir = new DirectoryInfo(dataPath);
                LDSpectrogramStitching.ConcatenateAllIndexFiles(dataDir, indexPropertiesConfigFileInfo, opDir, opFileStem);

            }


            // testing TERNARY PLOTS using spectral indices
            if (false)
            {
                string[] keys = { "ACI", "ENT", "EVN" };
                //string[] keys = { "BGN", "POW", "EVN"};

                FileInfo[] indexFiles = { new FileInfo(@"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\GympieNP\20150622\GympieNP_20150622__"+keys[0]+".csv"),
                                          new FileInfo(@"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\GympieNP\20150622\GympieNP_20150622__"+keys[1]+".csv"),
                                          new FileInfo(@"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\GympieNP\20150622\GympieNP_20150622__"+keys[2]+".csv")
                };
                FileInfo opImage = new FileInfo(@"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\GympieNP\20150622\GympieNP_20150622_TernaryPlot.png");

                var matrixDictionary = IndexMatrices.ReadSummaryIndexFiles(indexFiles, keys);

                string indexPropertiesConfigPath = @"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults" + @"\IndexPropertiesConfig.yml";
                FileInfo indexPropertiesConfigFileInfo = new FileInfo(indexPropertiesConfigPath);
                Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indexPropertiesConfigFileInfo);
                dictIP = InitialiseIndexProperties.FilterIndexPropertiesForSpectralOnly(dictIP);

                foreach (string key in keys)
                {
                    IndexProperties indexProperties = dictIP[key];
                    double min = indexProperties.NormMin;
                    double max = indexProperties.NormMax;
                    matrixDictionary[key] = MatrixTools.NormaliseInZeroOne(matrixDictionary[key], min, max);
                    //matrix = MatrixTools.FilterBackgroundValues(matrix, this.BackgroundFilter); // to de-demphasize the background small values
                }
                Image image = TernaryPlots.DrawTernaryPlot(matrixDictionary, keys);
                image.Save(opImage.FullName);
            }




            // testing directory search and file search 
            if (false)
            {
                string[] topLevelDirs =
                {
                    @"C:\temp\DirA",
                    @"C:\temp\DirB"
                };

                string sitePattern = "Subdir2";
                string dayPattern = "F2*.txt";

                List<string> dirList = new List<string>();
                foreach (string dir in topLevelDirs)
                {
                    string[] dirs = Directory.GetDirectories(dir, sitePattern, SearchOption.AllDirectories);
                    dirList.AddRange(dirs);
                }

                List<FileInfo> fileList = new List<FileInfo>();
                foreach (string subdir in topLevelDirs)
                {
                    var files = IndexMatrices.GetFilesInDirectory(subdir, dayPattern);

                    fileList.AddRange(files);
                }

                Console.WriteLine("The number of directories is {0}.", dirList.Count);
                foreach (string dir in dirList)
                {

                    Console.WriteLine(dir);
                }

                Console.WriteLine("The number of files is {0}.", fileList.Count);
                foreach (FileInfo file in fileList)
                {

                    Console.WriteLine(file.FullName);
                }
            }

            // experiments with clustering the spectra within spectrograms
            if (false)
            {
                SpectralClustering.Sandpit();
            } // end if (true)


            // experiments with false colour images - categorising/discretising the colours
            if (false)
            {
                LDSpectrogramDiscreteColour.DiscreteColourSpectrograms();
            }


            // Concatenate marine spectrogram ribbons and add tidal info if available.
            if (false)
            {

                DirectoryInfo[] dataDirs = { new DirectoryInfo(@"C:\SensorNetworks\Output\MarineSonograms\LdFcSpectrograms2013March\CornellMarine"),
                                             new DirectoryInfo(@"C:\SensorNetworks\Output\MarineSonograms\LdFcSpectrograms2013April\CornellMarine")
                                           };

                DirectoryInfo outputDirectory = new DirectoryInfo(@"C:\SensorNetworks\Output\MarineSonograms");
                string title = "Marine Spectrograms - 15km off Georgia Coast, USA.    Day 1= 01/March/2013      (Low tide=white; High tide=lime)";
                //indexPropertiesConfig = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesMarineConfig.yml");

                //string match = @"CornellMarine_*__ACI-ENT-EVN.SpectralRibbon.png";
                //string opFileStem = "CornellMarine.ACI-ENT-EVN.SpectralRibbon.2013MarchApril";

                string match = @"CornellMarine_*__BGN-POW-EVN.SpectralRibbon.png";
                string opFileStem = "CornellMarine.BGN-POW-EVN.SpectralRibbon.2013MarchApril";

                FileInfo tidalDataFile = new FileInfo(@"C:\SensorNetworks\OutputDataSets\GeorgiaTides2013.txt");
                //SunAndMoon.SunMoonTides[] tidalInfo = null;
                SunAndMoon.SunMoonTides[] tidalInfo = SunAndMoon.ReadGeorgiaTidalInformation(tidalDataFile);



                ConcatenateIndexFiles.ConcatenateRibbonImages(dataDirs, match, outputDirectory, opFileStem, title, tidalInfo);
            }


            // Concatenate three images for Dan Stowell.
            if (false)  // 
            {
                var imageDirectory = new DirectoryInfo(@"H:\Documents\SensorNetworks\MyPapers\2016_QMUL_SchoolMagazine");
                string fileName1 = @"TNC_Musiamunat_20150702_BAR10__ACI-ENT-EVNCropped.png";
                string fileName2 = @"GympieNP_20150701__ACI-ENT-EVN.png";
                string fileName3 = @"Sturt-Mistletoe_20150702__ACI-ENT-EVN - Corrected.png";
                var image1Path = new FileInfo(Path.Combine(imageDirectory.FullName, fileName1));
                var image2Path = new FileInfo(Path.Combine(imageDirectory.FullName, fileName2));
                var image3Path = new FileInfo(Path.Combine(imageDirectory.FullName, fileName3));

                var imageList = new List<Image>();


                imageList.Add(Bitmap.FromFile(image1Path.FullName));
                imageList.Add(Bitmap.FromFile(image2Path.FullName));
                imageList.Add(Bitmap.FromFile(image3Path.FullName));

                Image combinedImage = ImageTools.CombineImagesVertically(imageList);

                string fileName = String.Format("ThreeLongDurationSpectrograms.png");
                combinedImage.Save(Path.Combine(imageDirectory.FullName, fileName));
            }


            // Concatenate twelve images for Simon and Toby
            if (false)  // 
            {
                var imageDirectory = new DirectoryInfo(@"F:\AvailaeFolders\Griffith\Toby\20160201_FWrecordings\Site1Images");
                var imageFiles = imageDirectory.GetFiles();
                var imageList = new List<Image>();

                foreach (FileInfo file in imageFiles)
                {
                    imageList.Add(Bitmap.FromFile(file.FullName));
                }

                Image combinedImage = ImageTools.CombineImagesInLine(imageList);

                string fileName = String.Format("Site1.png");
                combinedImage.Save(Path.Combine(imageDirectory.FullName, fileName));
            }



            // Concatenate images for Karl-Heinz Frommolt
            if (false)  // 
            {
                string parentDir = @"C:\SensorNetworks\Output\Frommolt";
                DirectoryInfo dataDir = new DirectoryInfo(parentDir + @"\AnalysisOutput\mono");
                var imageDirectory = new DirectoryInfo(parentDir + @"\ConcatImageOutput");

                //string indexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfigHiRes.yml";

                string channel = "channel_0";
                //string dirMatch = "Monitoring_Rosin_2012*T*+0200_.merged.wav.channel_0.wav"; 
                string dirMatch = "Monitoring_Rosin_2012*T220000+0200_.merged.wav.channel_0.wav"; 
                 DirectoryInfo[] subDirectories = dataDir.GetDirectories(dirMatch, SearchOption.AllDirectories);

                //string fileMatch = @"*__ACI-ENT-EVN.png";
                string fileMatch = @"*__2Maps.png";

                var imageList = new List<Image>();

                FileInfo[] imageFiles = subDirectories[10].GetFiles(fileMatch, SearchOption.AllDirectories);
                Image image = Bitmap.FromFile(imageFiles[0].FullName);

                int width = 1;
                int height = image.Height;
                Bitmap spacerImage = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(spacerImage);
                g.Clear(Color.DarkGray);


                for (int d= 0; d < subDirectories.Length; d++)
                {
                    imageFiles = subDirectories[d].GetFiles(fileMatch, SearchOption.AllDirectories);

                    image = Bitmap.FromFile(imageFiles[0].FullName);
                    imageList.Add(image);
                    imageList.Add(spacerImage);
                }

                Image combinedImage = ImageTools.CombineImagesInLine(imageList);
                string fileName = String.Format(channel+".png");
                combinedImage.Save(Path.Combine(imageDirectory.FullName, fileName));
            }




            //HERVE GLOTIN
            // Combined audio2csv + zooming spectrogram task.
            // This is used to analyse Herve Glotin's BIRD50 data set.
            if (false)
            {
                // ############################# IMPORTANT ########################################
                // In order to analyse the short recordings in BIRD50 dataset, need following change to code:
                // need to modify    AudioAnalysis.AnalysisPrograms.AcousticIndices.cs #line648
                // need to change    SegmentMinDuration = TimeSpan.FromSeconds(20),  
                // to                SegmentMinDuration = TimeSpan.FromSeconds(1),
                // THIS iS to analyse BIRD50 short recordings.
                string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\TEST_7min_artificial.wav";

                //// HERVE GLOTIN BIRD50 TRAINING RECORDINGS
                //DirectoryInfo dataDir = new DirectoryInfo(@"D:\SensorNetworks\WavFiles\Glotin\Bird50\AmazonBird50_training_input");
                //string parentDir = @"C:\SensorNetworks\Output\BIRD50";
                //string speciesLabelsFile = parentDir + @"\AmazonBird50_training_output.csv";
                //int speciesCount = 50;
                //////set file name format -depends on train or test. E.g.  "ID0003";     
                //string fileStemFormatString = "ID{0:d4}";   // for training files
                //string indexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfigHiRes.yml";
                //string learningMode = "Train";

                //// HERVE GLOTIN BIRD50 TESTING RECORDINGS
                DirectoryInfo dataDir = new DirectoryInfo(@"D:\SensorNetworks\WavFiles\Glotin\Bird50\AmazonBird50_testing_input");
                string parentDir = @"C:\SensorNetworks\Output\BIRD50";
                string speciesLabelsFile = null;
                int speciesCount = 50;
                ////set file name format -depends on train or test. E.g.  "ID0003";     
                string fileStemFormatString = "ID1{0:d3}"; // for testing files
                string indexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfigHiRes.yml";
                string learningMode = "Test";


                // HERVE GLOTIN BOMBYX WHALE RECORDINGS
                //DirectoryInfo dataDir = new DirectoryInfo(@"C:\SensorNetworks\WavFiles\WhaleFromGlotin");
                //string parentDir = @"C:\SensorNetworks\Output\Glotin\Bombyx_SpermWhales";
                //string speciesLabelsFile = null;
                //int speciesCount = 0;
                //////set file name format -depends on train or test. E.g.  "ID0003";     
                //string fileStemFormatString = null;
                ////string fileStemFormatString = "ID1{0:d3}"; // for testing files
                //string indexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfigHiResGianniPavan.yml";
                //string learningMode = "Train";


                // GIANNI PAVAN SASSAFRAS RECORDINGS
                //DirectoryInfo dataDir = new DirectoryInfo(@"C:\SensorNetworks\WavFiles\GianniPavan\SABIOD - TEST SASSOFRATINO");
                //string parentDir = @"C:\SensorNetworks\Output\GianniPavan";
                //string speciesLabelsFile = null;
                //int speciesCount = 0;
                //string fileStemFormatString = null;
                //string indexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfigHiResGianniPavan.yml";
                //string learningMode = "Train";


                // ######################################################################

                string outputDir      = parentDir + @"\" + learningMode;
                string imageOutputDir = parentDir + @"\" + learningMode + "Images";
                string csvDir         = outputDir + @"\Towsey.Acoustic";
                string zoomOutputDir  = outputDir;


                string audio2csvConfigPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.AcousticHiRes.yml";
                string hiResZoomConfigPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramHiResConfig.yml";


                FileInfo[] wavFiles = { new FileInfo(recordingPath) };

                // comment next two lines when debugging a single recording file
                string match = @"*.wav";
                wavFiles = dataDir.GetFiles(match, SearchOption.AllDirectories);


                // READ IN THE SPECIES LABELS FILE AND SET UP THE DATA
                string[] fileID = new string[wavFiles.Length];
                int[] speciesID = new int[speciesCount];
                if (speciesLabelsFile != null)
                {
                    BirdClefExperiment1.ReadGlotinsSpeciesLabelFile(speciesLabelsFile, wavFiles.Length, out fileID, out speciesID);
                }
                else // make seperate species name for each file
                {
                    speciesID = new int[wavFiles.Length];
                }


                //LOOP THROUGH ALL WAV FILES
                //for (int i = 538; i < 539; i++)
                //for (int i = 0; i < 8; i++)
                for (int i = 0; i < wavFiles.Length; i++)
                {
                    FileInfo file = wavFiles[i];
                    recordingPath = file.FullName;
                    string idName = Path.GetFileNameWithoutExtension(file.FullName);
                    string name = String.Format("{0}_Species{1:d2}", idName, speciesID[i]);
                    outputDir = parentDir + @"\" + learningMode + @"\" + name;
                    csvDir    = parentDir + @"\" + learningMode + @"\" + name + @"\Towsey.Acoustic";
                    zoomOutputDir = outputDir;
                    Console.WriteLine("\n\n");
                    Console.WriteLine(String.Format(@">>>>{0}: File<{1}>", i, name));

                    try
                    {
                        // A: analyse the recording files == audio2csv.
                        var audio2csvArguments = new AnalyseLongRecordings.AnalyseLongRecording.Arguments
                        {
                            Source = recordingPath.ToFileInfo(),
                            Config = audio2csvConfigPath.ToFileInfo(),
                            Output = outputDir.ToDirectoryInfo()
                        };

                        if (!audio2csvArguments.Source.Exists)
                        {
                            LoggedConsole.WriteWarnLine(" >>>>>>>>>>>> WARNING! The Source Recording file cannot be found! This will cause an exception.");
                        }
                        if (!audio2csvArguments.Config.Exists)
                        {
                            LoggedConsole.WriteWarnLine(" >>>>>>>>>>>> WARNING! The Configuration file cannot be found! This will cause an exception.");
                        }
                        AnalyseLongRecordings.AnalyseLongRecording.Execute(audio2csvArguments);


                        // B: Concatenate the summary indices and produce images
                        // Use the Zoomingspectrograms action.

                        // need to find out how long the recording is.
                        string fileName = Path.GetFileNameWithoutExtension(audio2csvArguments.Source.FullName);
                        string testFileName = fileName + @"__Towsey.Acoustic.ACI.csv";
                        List<string> data = FileTools.ReadTextFile(Path.Combine(csvDir, testFileName));
                        int lineCount = data.Count - 1;  // -1 for header.
                        int imageWidth = lineCount;
                        //assume scale is index calculation duration = 0.1s
                        // i.e. image resolution  0.1s/px.
                        double focalMinute = (double)lineCount / 600 / 2;
                        if (focalMinute < 0.016666) focalMinute = 0.016666; // shortest recording = 1 second.



                        var zoomingArguments = new DrawZoomingSpectrograms.Arguments
                        {
                            // use the default set of index properties in the AnalysisConfig directory.
                            SourceDirectory = csvDir.ToDirectoryInfo(),
                            Output = zoomOutputDir.ToDirectoryInfo(),
                            SpectrogramTilingConfig = hiResZoomConfigPath.ToFileInfo(),

                            // draw a focused multi-resolution pyramid of images
                            ZoomAction = DrawZoomingSpectrograms.Arguments.ZoomActionType.Focused,
                            //FocusMinute = (int)focalMinute,
                        };

                        LoggedConsole.WriteLine("# Spectrogram Zooming config  : " + zoomingArguments.SpectrogramTilingConfig);
                        LoggedConsole.WriteLine("# Input Directory             : " + zoomingArguments.SourceDirectory);
                        LoggedConsole.WriteLine("# Output Directory            : " + zoomingArguments.Output);

                        var common = new ZoomCommonArguments();
                        common.SuperTilingConfig = Yaml.Deserialise<SuperTilingConfig>(zoomingArguments.SpectrogramTilingConfig);
                        var indexPropertiesPath = IndexProperties.Find(common.SuperTilingConfig, zoomingArguments.SpectrogramTilingConfig);
                        LoggedConsole.WriteLine("Using index properties file: " + indexPropertiesPath.FullName);
                        common.IndexProperties = IndexProperties.GetIndexProperties(indexPropertiesPath);

                        // get the indexDistributions and the indexGenerationData AND the //common.OriginalBasename
                        common.CheckForNeededFiles(zoomingArguments.SourceDirectory);
                        // Create directory if not exists
                        if (!zoomingArguments.Output.Exists)
                        {
                            zoomingArguments.Output.Create();
                        }

                        ZoomFocusedSpectrograms.DrawStackOfZoomedSpectrograms(zoomingArguments.SourceDirectory,
                                                                              zoomingArguments.Output,
                                                                              common,
                                                                              TimeSpan.FromMinutes(focalMinute),
                                                                              imageWidth);



                        // DRAW THE VARIOUS IMAGES
                        string fileStem = fileName;
                        if (fileStemFormatString != null)
                            fileStem = String.Format(fileStemFormatString, (i + 1)); // training images

                        var LDFCSpectrogramArguments = new DrawLongDurationSpectrograms.Arguments
                        {
                            // use the default set of index properties in the AnalysisConfig directory.
                            InputDataDirectory = csvDir.ToDirectoryInfo(),
                            OutputDirectory = imageOutputDir.ToDirectoryInfo(),
                            IndexPropertiesConfig = indexPropertiesConfig.ToFileInfo(),
                        };
                        // Create directory if not exists
                        if (!LDFCSpectrogramArguments.OutputDirectory.Exists)
                        {
                            LDFCSpectrogramArguments.OutputDirectory.Create();
                        }

                        // there are two possible tasks
                        // 1: draw the aggregated grey scale spectrograms 
                        int secDuration = DrawLongDurationSpectrograms.DrawAggregatedSpectrograms(LDFCSpectrogramArguments, fileStem);

                        // 2: draw the coloured ridge spectrograms 
                        secDuration = DrawLongDurationSpectrograms.DrawRidgeSpectrograms(LDFCSpectrogramArguments, fileStem);

                        // copy files
                        // POW, EVN, SPT, RHZ, RVT, RPS, RNG
                        string[] copyArray = { "POW", "EVN", "SPT", "RHZ", "RVT", "RPS", "RNG" };
                        DirectoryInfo sourceDirectory = new DirectoryInfo(csvDir);
                        string destinationDirectory = parentDir + @"\TrainingClassifier";
                        foreach (string key in copyArray)
                        {
                            // ID0002__Towsey.Acoustic.BGN.csv    fileName += @"__Towsey.Acoustic.ACI.csv";
                            string sourceFileName = String.Format(idName + "__Towsey.Acoustic." + key + ".csv");
                            string sourcePath = Path.Combine(sourceDirectory.FullName, sourceFileName);
                            string nameOfParentDirectory = sourceDirectory.Parent.Name;
                            string destinationFileName = String.Format(nameOfParentDirectory + "." + key + ".csv");
                            string destinationPath = Path.Combine(destinationDirectory, destinationFileName);
                            File.Copy(sourcePath, destinationPath, true);
                        }


                    } // try block
                    catch (Exception e)
                    {
                        LoggedConsole.WriteErrorLine(String.Format("ERROR!!!!! RECORDING {0}   FILE {1}", i, name));
                        LoggedConsole.WriteErrorLine(String.Format(e.ToString()));

                    }

                } // end loop through all wav files


            }  // END combined audio2csv + zooming spectrogram task.


            //HERVE GLOTIN
            // To produce HIres spectrogram images
            // This is used to analyse Herve Glotin's BIRD50 data set.
            if (false)
            {
                // In order to analyse the short recordings in BIRD50 dataset, need following change to code:
                // need to modify    AudioAnalysis.AnalysisPrograms.AcousticIndices.cs #line648
                // need to change    SegmentMinDuration = TimeSpan.FromSeconds(20),  
                // to                SegmentMinDuration = TimeSpan.FromSeconds(1),
                // THIS iS to analyse BIRD50 short recordings.
                string histoDir = @"C:\SensorNetworks\Output\BIRD50";
                string histoPath = Path.Combine(histoDir, "TrainingRecordingDurations.png");
                //string histoPath = Path.Combine(histoDir, "TestingRecordingDurations.png");
                // set up  histogram of recording durations
                int histogramWidth = 600; // equivalent to ten minutes at 0.1 second resolution
                int[] recordingDurations = new int[histogramWidth];


                // set up IP and OP directories
                string inputDir = @"C:\SensorNetworks\Output\BIRD50\Training";
                string imageOutputDir = @"C:\SensorNetworks\Output\BIRD50\TrainingImages";
                //string imageOutputDir = @"C:\SensorNetworks\Output\BIRD50\TestingRidgeImages";
                string indexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfigHiRes.yml";

                // comment next two lines when debugging a single recording file
                var inputDirInfo = new DirectoryInfo(inputDir);
                //string match = @"*.wav";
                //DirectoryInfo[] directories = inputDirInfo.GetDirectories(match, SearchOption.AllDirectories);
                DirectoryInfo[] directories = inputDirInfo.GetDirectories();
                int count = directories.Length;
                //count = 3;

                //string fileStem = "ID0003";      //\ID0001\Towsey.Acoustic\
                string fileStemFormatString = "ID{0:d4}"; // for training files
                //string fileStemFormatString = "ID1{0:d3}"; // for testing files

                for (int i = 0; i < count; i++)
                {

                    string fileStem = String.Format(fileStemFormatString, (i + 1));

                    string dataDir = directories[i].FullName + @"\Towsey.Acoustic\";
                    //string imageOutputDir = inputDir + @"\" + fileStem;

                    var LDFCSpectrogramArguments = new DrawLongDurationSpectrograms.Arguments
                    {
                        // use the default set of index properties in the AnalysisConfig directory.
                        InputDataDirectory = dataDir.ToDirectoryInfo(),
                        OutputDirectory = imageOutputDir.ToDirectoryInfo(),
                        IndexPropertiesConfig = indexPropertiesConfig.ToFileInfo(),
                    };


                    // there are two possible tasks
                    // 1: draw the aggregated grey scale spectrograms 
                    int secDuration = DrawLongDurationSpectrograms.DrawAggregatedSpectrograms(LDFCSpectrogramArguments, fileStem);

                    // 2: draw the coloured ridge spectrograms 
                    DrawLongDurationSpectrograms.DrawRidgeSpectrograms(LDFCSpectrogramArguments, fileStem);


                    if (secDuration >= recordingDurations.Length) secDuration = recordingDurations.Length - 1;
                    recordingDurations[secDuration]++;
                }
                string title = "Recording Duration: Width = " + histogramWidth + "secs";
                Image histoImage = ImageTools.DrawHistogram(title, recordingDurations, 95, null, histogramWidth, 50);
                histoImage.Save(histoPath);

            } // Herve Glotin's BIRD50 Dataset,   HIres spectrogram images




            if (false)
            {
                // In order to analyse the short recordings in BIRD50 dataset, need following change to code:
                // need to modify    AudioAnalysis.AnalysisPrograms.AcousticIndices.cs #line648
                // need to change    SegmentMinDuration = TimeSpan.FromSeconds(20),  
                // to                SegmentMinDuration = TimeSpan.FromSeconds(1),
                // THIS iS to analyse BIRD50 short recordings.
                string histoDir = @"C:\SensorNetworks\Output\BIRD50";
                string histoPath = Path.Combine(histoDir, "TrainingRecordingDurations.png");
                //string histoPath = Path.Combine(histoDir, "TestingRecordingDurations.png");
                // set up  histogram of recording durations
                //int histogramWidth = 600; // equivalent to ten minutes at 0.1 second resolution
                int totalRecordingLength = 0;


                // set up IP and OP directories
                string inputDir = @"C:\SensorNetworks\Output\BIRD50\Training";
                string imageOutputDir = @"C:\SensorNetworks\Output\BIRD50\TrainingImagesTEMP";
                //string imageOutputDir = @"C:\SensorNetworks\Output\BIRD50\TestingRidgeImages";
                string indexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfigHiRes.yml";

                // comment next two lines when debugging a single recording file
                var inputDirInfo = new DirectoryInfo(inputDir);
                //string match = @"*.wav";
                //DirectoryInfo[] directories = inputDirInfo.GetDirectories(match, SearchOption.AllDirectories);
                DirectoryInfo[] directories = inputDirInfo.GetDirectories();
                int count = directories.Length;
                //count = 619;

                //string fileStem = "ID0003";      //\ID0001\Towsey.Acoustic\
                string fileStemFormatString = "ID{0:d4}"; // for training files
                //string fileStemFormatString = "ID1{0:d3}"; // for testing files

                for (int i = 0; i < count; i++)
                {

                    string fileStem = String.Format(fileStemFormatString, (i + 1));
                    Console.WriteLine("\n\n");
                    Console.WriteLine(String.Format(@">>>>{0}: File<{1}>", i, fileStem));




                    string dataDir = directories[i].FullName + @"\Towsey.Acoustic\";
                    //string imageOutputDir = inputDir + @"\" + fileStem;

                    var LDFCSpectrogramArguments = new DrawLongDurationSpectrograms.Arguments
                    {
                        // use the default set of index properties in the AnalysisConfig directory.
                        InputDataDirectory = dataDir.ToDirectoryInfo(),
                        OutputDirectory = imageOutputDir.ToDirectoryInfo(),
                        IndexPropertiesConfig = indexPropertiesConfig.ToFileInfo(),
                    };


                    // there are two possible tasks
                    // 1: draw the aggregated grey scale spectrograms 
                    int rowCount = DrawLongDurationSpectrograms.DrawAggregatedSpectrograms(LDFCSpectrogramArguments, fileStem);

                    // 2: draw the coloured ridge spectrograms 
                    //DrawLongDurationSpectrograms.DrawRidgeSpectrograms(LDFCSpectrogramArguments, fileStem);


                    //if (secDuration >= recordingDurations.Length)
                    //    secDuration = recordingDurations.Length - 1;
                    totalRecordingLength += rowCount;
                    Console.WriteLine("Recording length = " + rowCount);
                }
                //string title = "Recording Duration: Width = " + histogramWidth + "secs";
                //Image histoImage = ImageTools.DrawHistogram(title, recordingDurations, 95, null, histogramWidth, 50);
                //histoImage.Save(histoPath);
                Console.WriteLine("\nTotal recording length = " + totalRecordingLength);
                Console.WriteLine("Av recording length = " + (totalRecordingLength / (double)count));

            } // Herve Glotin's BIRD50 Dataset,   HIres spectrogram images




            //HERVE GLOTIN
            // To produce HIres spectrogram images
            // This is used to analyse Herve Glotin's BIRD50 data set.
            //   Joins images of the same species
            if (false)
            {
                // set up IP and OP directories
                string inputDir = @"C:\SensorNetworks\Output\BIRD50\Testing";
                string imageInputDir = @"C:\SensorNetworks\Output\BIRD50\TrainingRidgeImages";
                string imageOutputDir = @"C:\SensorNetworks\Output\BIRD50\TrainingSpeciesImages"; //
                //string imageOutputDir = @"C:\SensorNetworks\Output\BIRD50\TestingRidgeImages";
                string speciesLabelsFile = @"C:\SensorNetworks\Output\BIRD50\AmazonBird50_training_output.csv";
                string countsArrayFilePath = @"C:\SensorNetworks\Output\BIRD50\AmazonBird50_training_Counts.txt";
                int speciesCount = 50;
                int count = 924; //trainingCount
                //int count = 375; //testCount
                //count = 3;

                // READ IN THE SPECIES LABELS FILE AND SET UP THE DATA
                string[] fileID = new string[count];
                int[] speciesID = new int[speciesCount];
                BirdClefExperiment1.ReadGlotinsSpeciesLabelFile(speciesLabelsFile, count, out fileID, out speciesID);


                // INIT array of species counts
                int[] speciesNumbers = new int[speciesCount];

                // loop through all 50 species
                for (int i = 0; i < speciesCount; i++)
                {
                    int speciesLabel = i + 1;
                    Console.WriteLine("Species " + speciesLabel);

                    // set up the image list for one species
                    List<Image> imageList = new List<Image>();


                    for (int j = 0; j < count; j++)
                    {
                        if (speciesID[j] != speciesLabel) continue;

                        // get the file name
                        FileInfo file = new FileInfo(Path.Combine(imageInputDir, fileID[j] + ".Ridges.png"));
                        Image bmp = ImageTools.ReadImage2Bitmap(file.FullName);
                        imageList.Add(bmp);


                        speciesNumbers[i]++;
                    } // end for loop j

                    Image combinedImage = ImageTools.CombineImagesVertically(imageList, 900);
                    string outputFileName = String.Format("Species{0}.png", speciesLabel);
                    string imagePath = Path.Combine(imageOutputDir, outputFileName);
                    combinedImage.Save(imagePath);


                } // end for loop i

                int sum = speciesNumbers.Sum();
                Console.WriteLine("Sum of species number array = " + sum);
                bool addLineNumbers = true;
                FileTools.WriteArray2File(speciesNumbers, addLineNumbers, countsArrayFilePath);


            } // Herve Glotin's BIRD50 Dataset,  Joins images of the same species


            //HERVE GLOTIN
            // To produce observe feature spectra or SPECTRAL FEATURE TEMPLATES for each species
            // This is used to analyse Herve Glotin's BIRD50 data set.
            if (true)
            {
                BirdClefExperiment1.Execute(null);
            } // Herve Glotin's BIRD50 Dataset




            if (false)
            {
                // ############################# IMPORTANT ########################################
                // THIS iS to produce histogram from Herve Glotin's SPERM WHALE BOMBYX recordings.
                string filePath = @"C:\SensorNetworks\Output\Glotin\Bombyx_SpermWhales\SpermWhaleSpikes_EntropyTimeSeries_0.1secScale.txt";
                string histoFileName = @"C:\SensorNetworks\Output\Glotin\Bombyx_SpermWhales\histogram.txt";
                string weightedHistoFileName = @"C:\SensorNetworks\Output\Glotin\Bombyx_SpermWhales\weightedHistogram.txt";


                var lines = FileTools.ReadTextFile(filePath);

                double[] entropy = new double[lines.Count];
                for (int i = 0; i < lines.Count; i++)
                {
                    entropy[i] = Double.Parse(lines[i]);
                }

                bool[] peaks = new bool[lines.Count];
                for (int i = 1; i < entropy.Length-1; i++)
                {
                    if ((entropy[i] > 0.3) && (entropy[i] > entropy[i-1]) && (entropy[i] > entropy[i+1]))
                    {
                        peaks[i] = true;
                    }
                }

                int searchWindow = 16;  // i.e. 1.6 seconds given time scale = 0.1s per value.
                int[] histogram = new int[searchWindow];
                double[] weightedHistogram = new double[searchWindow];
                int peakCount = 0;
                for (int i = 0; i < peaks.Length - searchWindow; i++)
                {
                    if (!peaks[i]) continue;
                    peakCount++;

                    //accumulate peak distances in histogram
                    for (int j = 1; j < searchWindow; j++)
                    {
                        if (peaks[i + j])
                        {
                            histogram[j] += 1;
                            weightedHistogram[j] += ((entropy[i] + entropy[i+j])/(double)2);
                        }
                    }

                }

                FileTools.WriteArray2File(histogram, false, histoFileName);
                FileTools.WriteArray2File(weightedHistogram, weightedHistoFileName);


            } //if (true)
            


            // To CALCULATE MUTUAL INFORMATION BETWEEN SPECIES DISTRIBUTION AND FREQUENCY INFO
            // This method calculates a seperate value of MI for each frequency bin
            // See the next method for single value of MI that incorporates all freq bins combined.
            if (false)
            {
                // set up IP and OP directories
                string parentDir = @"C:\SensorNetworks\Output\BIRD50";
                string key = "RHZ";  //"RHZ";
                int valueResolution = 6;
                string miFileName = parentDir + @"\MutualInformation."+ valueResolution + "catNoSkew." + key + ".txt";
                //double[] bounds = { 0.0, 3.0, 6.0 };
                //double[] bounds = { 0.0, 2.0, 4.0, 8.0 };
                double[] bounds = { 0.0, 2.0, 4.0, 6.0, 8.0, 10.0 }; // noSkew
                //double[] bounds = { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 10.0 };
                //double[] bounds = { 0.0, 1.0, 2.0, 4.0, 6.0, 10.0 }; // skew left
                //double[] bounds = { 0.0, 2.0, 4.0, 5.0, 6.0, 8.0 }; // skew centre
                //double[] bounds = { 0.0, 2.0, 4.0, 6.0, 8.0, 10.0 }; // noSkew

                string inputDir = parentDir + @"\TrainingCSV";
                int speciesNumber = 50;

                string speciesCountFile = parentDir + @"\AmazonBird50_training_Counts.txt"; //
                var lines = FileTools.ReadTextFile(speciesCountFile);
                int[] speciesCounts = new int[speciesNumber];
                for (int i = 0; i < speciesNumber; i++)
                {
                    string[] words = lines[i].Split(','); 
                    speciesCounts[i] = Int32.Parse(words[1]);
                }
                double Hspecies = DataTools.Entropy_normalised(speciesCounts);
                Console.WriteLine("Species Entropy = " + Hspecies);

                int freqBinCount = 256;
                int reducedBinCount = freqBinCount;
                //int reductionFactor = 1;
                //reducedBinCount = freqBinCount / reductionFactor;
                reducedBinCount = 100 + (156 / 2); // exotic style
                // data structure to contain probability info
                int[,,] probSgivenF = new int[reducedBinCount, speciesNumber, valueResolution];

                DirectoryInfo inputDirInfo = new DirectoryInfo(inputDir);
                string pattern = "*." + key + ".csv";
                FileInfo[] filePaths = inputDirInfo.GetFiles(pattern);

                // read through all the files
                int fileCount = filePaths.Length;
                //fileCount = 3;
                for (int i = 0; i < fileCount; i++)
                {
                    //ID0001_Species01.EVN.csv
                    char[] delimiters = { '.', 's' };
                    string fileName = filePaths[i].Name;
                    string[] parts = fileName.Split(delimiters);
                    int speciesID = Int32.Parse(parts[1]);
                    double[,] matrix = null;
                    if (filePaths[i].Exists)
                    {
                        int binCount;
                        matrix = IndexMatrices.ReadSpectrogram(filePaths[i], out binCount);

                        // column reduce the matrix
                        // try max pooling
                        //matrix = Sandpit.MaxPoolMatrixColumns(matrix, reducedBinCount);
                        //matrix = Sandpit.MaxPoolMatrixColumnsByFactor(matrix, reductionFactor);
                        matrix = BirdClefExperiment1.ExoticMaxPoolingMatrixColumns(matrix, reducedBinCount);
                    }

                    Console.WriteLine("Species ID = " + speciesID);

                    int rowCount = matrix.GetLength(0);
                    reducedBinCount = matrix.GetLength(1);

                    // calculate the conditional probabilities
                    // set up data structure to contain probability info
                    for (int r = 0; r < rowCount; r++)
                    {
                        var rowVector = MatrixTools.GetRow(matrix, r);
                        for (int c = 0; c < reducedBinCount; c++)
                        {
                            int valueCategory = 0;
                            for (int bound = 1; bound < bounds.Length; bound++)
                            {
                                if (rowVector[c] > bounds[bound]) valueCategory = bound;
                            }
                            probSgivenF[c, speciesID-1, valueCategory] ++;
                        }
                    }
                }

                // now process info in probabilities in data structure
                double[] mi = new double[reducedBinCount];
                for (int i = 0; i < reducedBinCount; i++)
                {
                    var m = new double[speciesNumber, valueResolution];


                    for (int r = 0; r < speciesNumber; r++)
                    {
                        for (int c = 0; c < valueResolution; c++)
                        {
                            m[r, c] = probSgivenF[i, r, c];
                        }
                    }
                    double[]  array = DataTools.Matrix2Array(m);
                    double entropy = DataTools.Entropy_normalised(array);
                    mi[i] = entropy;
                }

                for (int i = 0; i < reducedBinCount; i++)
                {
                    Console.WriteLine(String.Format("Bin{0}  {1}", i, mi[i]));
                }
                FileTools.WriteArray2File(mi, miFileName);

            } // CALCULATE MUTUAL INFORMATION



            // test 3-D matrix to array
            if (false)
            {
                double value = 0;
                var M3d = new double[2,2,2];
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            M3d[i, j, k] = value;
                            value += 1.0;
                        }
                    }
                }
                double[] array = DataTools.Matrix2Array(M3d);
            }


            // test MUTUAL INFORMATION
            if (false)
            {
                var M = new int[4, 4];
                M[0, 0] = 4; M[0, 1] = 2; M[0, 2] = 1; M[0, 3] = 1;
                M[1, 0] = 2; M[1, 1] = 4; M[1, 2] = 1; M[1, 3] = 1;
                M[2, 0] = 2; M[2, 1] = 2; M[2, 2] = 2; M[2, 3] = 2;
                M[3, 0] = 8; M[3, 1] = 0; M[3, 2] = 0; M[3, 3] = 0;
                double MI = DataTools.MutualInformation(M);
            }


            // EXPERIMENTS WITH HERVE
            // To CALCULATE MUTUAL INFORMATION BETWEEN SPECIES DISTRIBUTION AND FREQUENCY INFO
            // this method calculates a single MI value for the entire frequency band
            if (false)
            {
                // set up IP and OP directories
                string parentDir = @"C:\SensorNetworks\Output\BIRD50";
                string key = "POW";  //"RHZ";
                int valueCategoryCount = 10;
                string miFileName = parentDir + @"\MutualInformation." + valueCategoryCount + "cats." + key + ".txt";

                //double[] bounds = { 5.0 };
                //double[] bounds = { 0.0, 4.0, 6.0 };
                //double[] bounds = { 0.0, 2.0, 4.0, 8.0 };
                //double[] bounds = { 0.0, 2.0, 4.0, 6.0, 8.0, 10.0 }; // noSkew
                //double[] bounds = { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 10.0 };
                //double[] bounds = { 0.0, 2.0, 4.0, 6.0, 8.0, 10.0, 12.0, 14.0, 16.0, 18.0 };
                double[] bounds = { 0.0, 4.0, 8.0, 12.0, 16.0, 20.0, 24.0, 28.0, 32.0, 36.0 };
                //double[] bounds = { 0.0, 1.0, 2.0, 4.0, 6.0, 10.0 }; // skew left
                //double[] bounds = { 0.0, 2.0, 4.0, 5.0, 6.0, 8.0 }; // skew centre
                //double[] bounds = { 0.0, 2.0, 4.0, 6.0, 8.0, 10.0 }; // noSkew

                string inputDir = parentDir + @"\TrainingCSV";

                // read Herve's file of metadata
                int speciesNumber = 50;
                string speciesCountFile = parentDir + @"\AmazonBird50_training_Counts.txt"; //
                var lines = FileTools.ReadTextFile(speciesCountFile);
                int[] speciesCounts = new int[speciesNumber];
                for (int i = 0; i < speciesNumber; i++)
                {
                    string[] words = lines[i].Split(',');
                    speciesCounts[i] = Int32.Parse(words[1]);
                }
                double Hspecies = DataTools.Entropy_normalised(speciesCounts);
                Console.WriteLine("Species Entropy = " + Hspecies);


                // set up the input data
                int freqBinCount = 256;
                int reducedBinCount = freqBinCount;

                // standard matrix reduction
                int minBin = 9;
                //int maxBin = 233;
                int maxBin = 218;
                reducedBinCount = maxBin - minBin + 1;

                // frequency bins used to reduce dimensionality of the 256 spectral values.
                //int startBin = 8;
                //int maxOf2Bin = 117;
                //int maxOf3Bin = 182;
                //int endBin = 234;
                //double[] testArray = new double[256];
                ////for (int i = 0; i < testArray.Length; i++) testArray[i] = i;
                //double[] reducedArray = Sandpit.MaxPoolingLimited(testArray, startBin, maxOf2Bin, maxOf3Bin, endBin);
                //int reducedBinCount = reducedArray.Length;

                // other ways to reduce the spectrum length
                //int reductionFactor = 1;
                //reducedBinCount = freqBinCount / reductionFactor;
                //reducedBinCount = 100 + (156 / 2); // exotic style


                // Length of the Input feature vector 
                int featureVectorLength = reducedBinCount * valueCategoryCount;

                // data structure to contain probability info
                //int[,,] probSgivenF = new int[reducedBinCount, speciesNumber, valueResolution];
                int[,] probSgivenF = new int[featureVectorLength, speciesNumber];
                int[] decibelDistribution = new int[100];

                DirectoryInfo inputDirInfo = new DirectoryInfo(inputDir);
                string pattern = "*." + key + ".csv";
                FileInfo[] filePaths = inputDirInfo.GetFiles(pattern);

                // read through all the files
                int fileCount = filePaths.Length;
                //fileCount = 3;
                for (int i = 0; i < fileCount; i++)
                {
                    //ID0001_Species01.EVN.csv
                    char[] delimiters = { '.', 's' };
                    string fileName = filePaths[i].Name;
                    string[] parts = fileName.Split(delimiters);
                    int speciesID = Int32.Parse(parts[1]);
                    //Console.WriteLine("Species ID = " + speciesID);
                    // show user something is happening
                    Console.Write(".");

                    double[,] matrix = null;
                    if (filePaths[i].Exists)
                    {
                        int binCount;
                        matrix = IndexMatrices.ReadSpectrogram(filePaths[i], out binCount);

                        // column reduce the matrix
                        matrix = BirdClefExperiment1.ReduceMatrixColumns(matrix, minBin, maxBin);

                        // try max pooling
                        //matrix = Sandpit.MaxPoolingLimited(matrix, startBin, maxOf2Bin, maxOf3Bin, endBin, reducedBinCount);
                        //matrix = Sandpit.MaxPoolMatrixColumns(matrix, reducedBinCount);
                        //matrix = Sandpit.MaxPoolMatrixColumnsByFactor(matrix, reductionFactor);
                        //matrix = Sandpit.ExoticMaxPoolingMatrixColumns(matrix, reducedBinCount);
                    }

                    int rowCount = matrix.GetLength(0);
                    reducedBinCount = matrix.GetLength(1);


                    // calculate the conditional probabilities
                    // set up data structure to contain probability info
                    //int threshold = 0;
                    for (int r = 0; r < rowCount; r++) // for all time
                    {
                        var rowVector = MatrixTools.GetRow(matrix, r);
                        for (int c = 0; c < reducedBinCount; c++) // for all freq bins
                        {
                            double dBvalue = rowVector[c];
                            decibelDistribution[(int)Math.Floor(dBvalue)]++;

                            // use this line when have only a single binary variable
                            //if (dBvalue > threshold)
                            //{
                            //    probSgivenF[c, speciesID - 1] ++;
                            //    decibelDistribution[dBvalue]++;
                            //}
                            

                            // use next six lines when variable can have >=3 discrete values 
                            int valueCategory = 0;
                            for (int bound = 1; bound < bounds.Length; bound++)
                            {
                                if (dBvalue > bounds[bound]) valueCategory = bound;
                            }
                            int newIndex = (valueCategory * reducedBinCount) + c;
                            probSgivenF[newIndex, speciesID - 1]++;
                        }
                    }
                } // over all files

                // Now have the entire data in one structure.
                // Next process inf// in probabilities in data structure
                //int[] array = DataTools.Matrix2Array(probSgivenF);
                //double entropy = DataTools.Entropy_normalised(array);
                double MI = DataTools.MutualInformation(probSgivenF);


                Console.WriteLine(String.Format("\n\nFeature {0};  Category Count {1}", key, valueCategoryCount));
                Console.WriteLine(String.Format("Mutual Info = {0}", MI));

                //for (int i = 0; i < decibelDistribution.Length; i++)
                //{
                //    Console.WriteLine(String.Format("dB{0}  {1}", i, decibelDistribution[i])); 
                //}
                double sum = decibelDistribution.Sum();
                Console.WriteLine(String.Format("Dist sum = {0}", sum));

                double threshold = sum / 2;
                double median = 0;
                int medianIndex = 0;
                for (int i = 0; i < decibelDistribution.Length; i++)
                {
                    median += decibelDistribution[i];
                    if (median >= threshold)
                    {
                        medianIndex = i;
                        break;
                    }
                }
                Console.WriteLine(String.Format("Median occurs at {0} ", medianIndex)); 

                //for (int i = 0; i < reducedBinCount; i++)
                //{
                //Console.WriteLine(String.Format("Bin{0}  {1}", i, mi[i]));
                //}
                //FileTools.WriteArray2File(mi, miFileName);

            } // CALCULATE MUTUAL INFORMATION




            Console.WriteLine("# Finished Sandpit Task!");
            Console.ReadLine();
            System.Environment.Exit(0);
        }


    }
}
