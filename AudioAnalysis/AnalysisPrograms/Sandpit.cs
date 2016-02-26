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
                string dayPattern  = "F2*.txt";

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



            //HERVE GLOTIN
            // Combined audio2csv + zooming spectrogram task.
            // This is used to analyse Herve Glotin's BIRD50 data set.
            if (true)
            {
                // ############################# IMPORTANT ########################################
                // In order to analyse the short recordings in BIRD50 dataset, need following change to code:
                // need to modify    AudioAnalysis.AnalysisPrograms.AcousticIndices.cs #line648
                // need to change    SegmentMinDuration = TimeSpan.FromSeconds(20),  
                // to                SegmentMinDuration = TimeSpan.FromSeconds(1),
                // THIS iS to analyse BIRD50 short recordings.

                int speciesCount = 50;
                DirectoryInfo dataDir = new DirectoryInfo(@"D:\SensorNetworks\WavFiles\Glotin\Bird50\AmazonBird50_training_input");
                //DirectoryInfo dataDir = new DirectoryInfo(@"D:\SensorNetworks\WavFiles\Glotin\Bird50\AmazonBird50_testing_input");

                string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\TEST_7min_artificial.wav";
                string parentDir     = @"C:\SensorNetworks\Output\BIRD50";
                string outputDir  = parentDir + @"\Training";
                string csvDir  = outputDir + @"\Towsey.Acoustic";
                string zoomOutputDir = outputDir;
                string imageOutputDir = parentDir + @"\TrainingImages";
                string speciesLabelsFile = parentDir + @"\AmazonBird50_training_output.csv";

                // set file name format - depends on train or test
                //string fileStem = "ID0003";      //\ID0001\Towsey.Acoustic\
                string fileStemFormatString = "ID{0:d4}"; // for training files
                //string fileStemFormatString = "ID1{0:d3}"; // for testing files

                string audio2csvConfigPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.AcousticHiRes.yml";
                string hiResZoomConfigPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramHiResConfig.yml";


                FileInfo[] wavFiles = { new FileInfo(recordingPath) };

                // comment next two lines when debugging a single recording file
                string match = @"*.wav";
                wavFiles = dataDir.GetFiles(match, SearchOption.AllDirectories);


                // READ IN THE SPECIES LABELS FILE AND SET UP THE DATA
                string[] fileID = new string[wavFiles.Length];
                int[] speciesID = new int[speciesCount];
                ReadGlotinsSpeciesLabelFile(speciesLabelsFile, wavFiles.Length, out fileID, out speciesID);



                //LOOP THROUGH ALL WAV FILES
                //for (int i = 538; i < 539; i++)
                //for (int i = 0; i < 8; i++)
                for (int i = 0; i < wavFiles.Length; i++)
                {
                    FileInfo file = wavFiles[i];
                    recordingPath = file.FullName;
                    string idName = Path.GetFileNameWithoutExtension(file.FullName);
                    string name = String.Format("{0}_Species{1:d2}", idName, speciesID[i]);
                    outputDir = parentDir + @"\Training\" + name;
                    csvDir    = parentDir + @"\Training\" + name + @"\Towsey.Acoustic";
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
                        fileName += @"__Towsey.Acoustic.ACI.csv";
                        List<string> data = FileTools.ReadTextFile(Path.Combine(csvDir, fileName));
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
                        string fileStem = String.Format(fileStemFormatString, (i + 1)); // training images
                        string indexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfigHiRes.yml";

                        var LDFCSpectrogramArguments = new DrawLongDurationSpectrograms.Arguments
                        {
                            // use the default set of index properties in the AnalysisConfig directory.
                            InputDataDirectory = csvDir.ToDirectoryInfo(),
                            OutputDirectory = imageOutputDir.ToDirectoryInfo(),
                            IndexPropertiesConfig = indexPropertiesConfig.ToFileInfo(),
                        };

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
                        foreach(string key in copyArray)
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
                int histogramWidth = 600;
                int[] recordingDurations = new int[histogramWidth];


                // set up IP and OP directories
                string inputDir = @"C:\SensorNetworks\Output\BIRD50\Testing";
                //string imageOutputDir = @"C:\SensorNetworks\Output\BIRD50\TrainingImages";
                string imageOutputDir = @"C:\SensorNetworks\Output\BIRD50\TestingRidgeImages";
                string indexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfigHiRes.yml";
                //int count = 924; //trainingCount
                int count = 375; //testCount
                //count = 3;

                //string fileStem = "ID0003";      //\ID0001\Towsey.Acoustic\
                //string fileStemFormatString = "ID{0:d4}"; // for training files
                string fileStemFormatString = "ID1{0:d3}"; // for testing files

                for (int i = 1; i <= count; i++)
                {

                    string fileStem = String.Format(fileStemFormatString, i); // training images
                    //string fileStem = String.Format("ID{0:d4}", i); // training images
                    //string fileStem = String.Format("ID1{0:d3}", i); // testing images

                    string dataDir = inputDir + @"\" + fileStem + @"\Towsey.Acoustic\";
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
                //int secDuration = DrawLongDurationSpectrograms.DrawAggregatedSpectrograms(LDFCSpectrogramArguments, fileStem);

                // 2: draw the coloured ridge spectrograms 
                int secDuration = DrawLongDurationSpectrograms.DrawRidgeSpectrograms(LDFCSpectrogramArguments, fileStem);


                    if (secDuration >= recordingDurations.Length) secDuration = recordingDurations.Length - 1;
                recordingDurations[secDuration]++;                    
            }
                string title = "Recording Duration: Width = "+ histogramWidth + "secs"; 
                Image histoImage =  ImageTools.DrawHistogram(title, recordingDurations, 95, null, histogramWidth, 50);
                histoImage.Save(histoPath);

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
                ReadGlotinsSpeciesLabelFile(speciesLabelsFile, count, out fileID, out speciesID);


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
                        FileInfo file = new FileInfo(Path.Combine(imageInputDir, fileID[j]+ ".Ridges.png"));
                        Image bmp = ImageTools.ReadImage2Bitmap(file.FullName);
                        imageList.Add(bmp);


                        speciesNumbers[i] ++;
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





            Console.WriteLine("# Finished Sandpit Task!");
            Console.ReadLine();
            System.Environment.Exit(0);
        }


        private static void ReadGlotinsSpeciesLabelFile(string speciesLabelsFile, int count, out string[] fileID, out int[] speciesID)
        { 
            // READ IN THE SPECIES LABELS FILE AND SET UP THE DATA
            var lines = FileTools.ReadTextFile(speciesLabelsFile);

            speciesID = new int[lines.Count];
            fileID = new string[lines.Count];

            if (lines.Count != count)
            {
                Console.WriteLine("lineCount != count    {0}  !=  {1}", lines.Count, count);
                return;
            }

            for (int i = 0; i<lines.Count; i++)
            {
                string[] words = lines[i].Split(',');
                fileID[i] = words[0];
                speciesID[i] = Int32.Parse(words[1]);
            }
        } // ReadGlotinsSpeciesLabelFile()




    }
}
