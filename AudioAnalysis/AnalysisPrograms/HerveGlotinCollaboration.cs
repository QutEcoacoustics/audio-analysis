namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Acoustics.Shared;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using TowseyLibrary;

    public static class HerveGlotinCollaboration
    {

        /// <summary>
        /// HERVE GLOTIN
        /// Combined audio2csv + zooming spectrogram task.
        /// This is used to analyse Herve Glotin's BIRD50 data set.
        /// ############################# IMPORTANT ########################################
        /// In order to analyse the short recordings in BIRD50 dataset, need following change to code:
        /// need to modify    AudioAnalysis.AnalysisPrograms.AcousticIndices.cs #line648
        /// need to change    SegmentMinDuration = TimeSpan.FromSeconds(20),
        /// to                SegmentMinDuration = TimeSpan.FromSeconds(1),
        /// THIS iS to analyse BIRD50 short recordings.

        /// </summary>
        public static void HiRes1()
        {
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

            string outputDir = parentDir + @"\" + learningMode;
            string imageOutputDir = parentDir + @"\" + learningMode + "Images";
            string csvDir = outputDir + @"\Towsey.Acoustic";
            string zoomOutputDir = outputDir;


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
                string name = string.Format("{0}_Species{1:d2}", idName, speciesID[i]);
                outputDir = parentDir + @"\" + learningMode + @"\" + name;
                csvDir = parentDir + @"\" + learningMode + @"\" + name + @"\Towsey.Acoustic";
                zoomOutputDir = outputDir;
                Console.WriteLine("\n\n");
                Console.WriteLine(string.Format(@">>>>{0}: File<{1}>", i, name));

                try
                {
                    // A: analyse the recording files == audio2csv.
                    var audio2csvArguments = new AnalyseLongRecordings.AnalyseLongRecording.Arguments
                    {
                        Source = recordingPath.ToFileInfo(),
                        Config = audio2csvConfigPath.ToFileInfo(),
                        Output = outputDir.ToDirectoryInfo(),
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
                    // i.e. image resolution  0.1s/px. or 600px/min
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
                        fileStem = string.Format(fileStemFormatString, (i + 1)); // training images

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
                        string sourceFileName = string.Format(idName + "__Towsey.Acoustic." + key + ".csv");
                        string sourcePath = Path.Combine(sourceDirectory.FullName, sourceFileName);
                        string nameOfParentDirectory = sourceDirectory.Parent.Name;
                        string destinationFileName = string.Format(nameOfParentDirectory + "." + key + ".csv");
                        string destinationPath = Path.Combine(destinationDirectory, destinationFileName);
                        File.Copy(sourcePath, destinationPath, true);
                    }


                } // try block
                catch (Exception e)
                {
                    LoggedConsole.WriteErrorLine(string.Format("ERROR!!!!! RECORDING {0}   FILE {1}", i, name));
                    LoggedConsole.WriteErrorLine(string.Format(e.ToString()));

                }

            } // end loop through all wav files

        } // HiRes1()


        /// <summary>
        /// HERVE GLOTIN: This is used to analyse the BIRD50 data set.
        /// Draws HIres spectrogram images AFTER indices have been calculated.
        /// This method does NOT produce acoustic indices.
        /// That is, only call this method if hires1() has already been used to produce the indices.
        /// </summary>
        public static void HiRes2()
        {
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

                string fileStem = string.Format(fileStemFormatString, (i + 1));

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
            Image histoImage = GraphsAndCharts.DrawHistogram(title, recordingDurations, 95, null, histogramWidth, 50);
            histoImage.Save(histoPath);

        } // HiRes2() produces spectrogram images


        /// <summary>
        /// This method is very similar to HiRes2().
        /// I have forgotten what the difference in purpose is!!
        /// It is a method used in February-March 2016 to analyse BIRD50 short recordings.
        /// </summary>
        public static void HiRes3()
        {
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

                string fileStem = string.Format(fileStemFormatString, (i + 1));
                Console.WriteLine("\n\n");
                Console.WriteLine(string.Format(@">>>>{0}: File<{1}>", i, fileStem));


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

        } // HiRes3() spectrogram images


        /// <summary>
        /// HERVE GLOTIN
        ///  To produce HIres spectrogram images
        ///  This is used to analyse Herve Glotin's BIRD50 data set.
        ///  Joins images of the same species
        /// </summary>
        public static void HiRes4()
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
                string outputFileName = string.Format("Species{0}.png", speciesLabel);
                string imagePath = Path.Combine(imageOutputDir, outputFileName);
                combinedImage.Save(imagePath);


            } // end for loop i

            int sum = speciesNumbers.Sum();
            Console.WriteLine("Sum of species number array = " + sum);
            bool addLineNumbers = true;
            FileTools.WriteArray2File(speciesNumbers, addLineNumbers, countsArrayFilePath);
        } // Herve Glotin's BIRD50 Dataset,  Joins images of the same species





        /// <summary>
        /// ############################# IMPORTANT ########################################
        /// THIS iS to produce histogram from Herve Glotin's SPERM WHALE BOMBYX recordings.
        /// </summary>
        public static void AnalyseBOMBYXRecordingsForSpermWhaleClicks()
        {
            string filePath = @"C:\SensorNetworks\Output\Glotin\Bombyx_SpermWhales\SpermWhaleSpikes_EntropyTimeSeries_0.1secScale.txt";
            string histoFileName = @"C:\SensorNetworks\Output\Glotin\Bombyx_SpermWhales\histogram.txt";
            string weightedHistoFileName = @"C:\SensorNetworks\Output\Glotin\Bombyx_SpermWhales\weightedHistogram.txt";


            var lines = FileTools.ReadTextFile(filePath);

            double[] entropy = new double[lines.Count];
            for (int i = 0; i < lines.Count; i++)
            {
                entropy[i] = double.Parse(lines[i]);
            }

            bool[] peaks = new bool[lines.Count];
            for (int i = 1; i < entropy.Length - 1; i++)
            {
                if ((entropy[i] > 0.3) && (entropy[i] > entropy[i - 1]) && (entropy[i] > entropy[i + 1]))
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
                        weightedHistogram[j] += ((entropy[i] + entropy[i + j]) / (double)2);
                    }
                }

            }

            FileTools.WriteArray2File(histogram, false, histoFileName);
            FileTools.WriteArray2File(weightedHistogram, weightedHistoFileName);
        }

    }
    }
