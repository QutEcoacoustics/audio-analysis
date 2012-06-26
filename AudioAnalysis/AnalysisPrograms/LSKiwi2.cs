using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using Acoustics.Shared;
using Acoustics.Tools;
using Acoustics.Tools.Audio;
using AnalysisBase;

using TowseyLib;
using AudioAnalysisTools;



namespace AnalysisPrograms
{
    public class  LSKiwi2: IAnalyser
    {
        //KEYS TO PARAMETERS IN CONFIG FILE
        public static string key_MIN_HZ_MALE = "MIN_HZ_MALE";
        public static string key_MAX_HZ_MALE = "MAX_HZ_MALE";
        public static string key_MIN_HZ_FEMALE = "MIN_HZ_FEMALE";
        public static string key_MAX_HZ_FEMALE = "MAX_HZ_FEMALE";

        //HEADER KEYS
        public static string key_EVENT_NAME      = AudioAnalysisTools.Keys.EVENT_NAME;
        public static string key_INTENSITY_SCORE = AudioAnalysisTools.Keys.EVENT_INTENSITY;
        public static string key_BANDWIDTH_SCORE = "BandwidthScore";
        public static string key_DELTA_SCORE     = "DeltaPeriodScore";
        public static string key_SNR_SCORE       = AudioAnalysisTools.Keys.SNR_SCORE;
        public static string key_PEAKS_SNR_SCORE = "PeaksSnrScore";
        public static string key_PEAKS_STD_SCORE = "PeaksStdScore";
        public static string key_EVENT_NORMSCORE = AudioAnalysisTools.Keys.EVENT_NORMSCORE;


        public static string[] defaultRules = {
                                           "EXCLUDE_IF_RULE=feature1_LT_0.45",
                                           "EXCLUDE_IF_RULE=feature1_GT_0.45",
                                           "WEIGHT_feature1=0.45",
                                           "WEIGHT_feature2=0.45",
                                           "WEIGHT_feature3=0.45"
        };


        //OTHER CONSTANTS
        public const string ANALYSIS_NAME = "LSKiwi2";
        public const int RESAMPLE_RATE = 17640;
        //public const int RESAMPLE_RATE = 22050;
        //public const string imageViewer = @"C:\Program Files\Windows Photo Viewer\ImagingDevices.exe";
        public const string imageViewer = @"C:\Windows\system32\mspaint.exe";


        public string DisplayName
        {
            get { return "Little Spotted Kiwi v2"; }
        }
        private static string identifier = "Towsey." + ANALYSIS_NAME;
        public string Identifier
        {
            get { return identifier; }
        }


        public static void Dev(string[] args)
        {
            //Following lines are used for the debug command line.
            // kiwi "C:\SensorNetworks\WavFiles\Kiwi\Samples\lsk-female\TOWER_20091107_07200_21.LSK.F.wav"  "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
            // kiwi "C:\SensorNetworks\WavFiles\Kiwi\Samples\lsk-male\TOWER_20091112_072000_25.LSK.M.wav"  "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
            // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_Cropped.wav"     "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
            // 8 min test recording  // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_CroppedAnd2.wav" "C:\SensorNetworks\WavFiles\Kiwi\Results_MixedTest\lskiwi_Params.txt"
            // kiwi "C:\SensorNetworks\WavFiles\Kiwi\KAPITI2_20100219_202900.wav"   "C:\SensorNetworks\WavFiles\Kiwi\Results\lskiwi_Params.txt"
            // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav"     "C:\SensorNetworks\WavFiles\Kiwi\Results_TOWER_20100208_204500\lskiwi_Params.txt"

            string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.LSKiwi2.cfg";

            //string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TOWER_20100208_204500\TOWER_20100208_204500_ANDREWS_SELECTIONS.csv";
            string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500_40m0s.wav";
            string outputDir     = @"C:\SensorNetworks\Output\LSKiwi2\Tower_20100208_204500\";

            //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\KAPITI2_20100219_202900.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\KAPITI2-20100219-202900_Airplane.mp3";
            //string outputDir     = @"C:\SensorNetworks\Output\Kiwi\KAPITI2_20100219_202900\";
            //string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_KAPITI2_20100219_202900\KAPITI2_20100219_202900_ANDREWS_SELECTIONS.csv";

            //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav";
            //string outputDir     = @"C:\SensorNetworks\Output\Kiwi\TUITCE_20091215_220004\";
            //string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_220004\TUITCE_20091215_220004_ANDREWS_SELECTIONS.csv";

            //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_210000.wav";
            //string outputDir     = @"C:\SensorNetworks\Output\LSKiwi2\TuiTce_20091215_210000\";
            //string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_210000\TUITCE_20091215_210000_ANDREWS_SELECTIONS.csv";

            string title = "# FOR DETECTION OF THE LITTLE SPOTTED KIWI - version 2 - started June 2012";
            string date = "# DATE AND TIME: " + DateTime.Now;
            Console.WriteLine(title);
            Console.WriteLine(date);
            Console.WriteLine("# Output folder:  " + outputDir);
            Console.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            var diOutputDir = new DirectoryInfo(outputDir);

            Log.Verbosity = 1;
            int startMinute = 40;
            int durationSeconds = 300; //set zero to get entire recording
            var tsStart = new TimeSpan(0, startMinute, 0); //hours, minutes, seconds
            var tsDuration = new TimeSpan(0, 0, durationSeconds); //hours, minutes, seconds
            var segmentFileStem = Path.GetFileNameWithoutExtension(recordingPath);
            var segmentFName  = string.Format("{0}_{1}min.wav", segmentFileStem, startMinute);
            var sonogramFname = string.Format("{0}_{1}min.png", segmentFileStem, startMinute);
            var eventsFname   = string.Format("{0}_{1}min.{2}.Events.csv",  segmentFileStem, startMinute, identifier);
            var indicesFname  = string.Format("{0}_{1}min.{2}.Indices.csv", segmentFileStem, startMinute, identifier);

            //var fiCsvFile = new FileInfo(restOfArgs[0]);
            //var fiConfigFile = new FileInfo(restOfArgs[1]);
            //IAnalysis analyser = new LSKiwi2();
            //analyser.ProcessCsvFile(fiCsvFile, fiConfigFile); //returns a datatable which has no relevance at this level.

            var cmdLineArgs = new List<string>();
            cmdLineArgs.Add(recordingPath);
            cmdLineArgs.Add(configPath);
            cmdLineArgs.Add(outputDir);
            cmdLineArgs.Add("-tmpwav:" + segmentFName);
            cmdLineArgs.Add("-events:" + eventsFname);
            cmdLineArgs.Add("-indices:" + indicesFname);
            cmdLineArgs.Add("-sgram:" + sonogramFname);
            cmdLineArgs.Add("-start:" + tsStart.TotalSeconds);
            cmdLineArgs.Add("-duration:" + tsDuration.TotalSeconds);

            //#############################################################################################################################################
            int status = Execute(cmdLineArgs.ToArray());
            if (status != 0)
            {
                Console.WriteLine("\n\n# FATAL ERROR. CANNOT PROCEED!");
                Console.ReadLine();
                System.Environment.Exit(99);
            }
            //#############################################################################################################################################

            string eventsPath = Path.Combine(outputDir, eventsFname);
            FileInfo fiCsvEvents = new FileInfo(eventsPath);
            if (!fiCsvEvents.Exists)
            {
                Log.WriteLine("\n\n\n############\n WARNING! Events CSV file not returned from analysis of minute {0} of file <{0}>.", startMinute, recordingPath);
            }
            else
            {
                Console.WriteLine("\n");
                DataTable dt = CsvTools.ReadCSVToTable(eventsPath, true);
                DataTableTools.WriteTable2Console(dt);
            }
            string indicesPath = Path.Combine(outputDir, indicesFname);
            FileInfo fiCsvIndices = new FileInfo(indicesPath);
            if (!fiCsvIndices.Exists)
            {
                Log.WriteLine("\n\n\n############\n WARNING! Indices CSV file not returned from analysis of minute {0} of file <{0}>.", startMinute, recordingPath);
            }
            else
            {
                Console.WriteLine("\n");
                DataTable dt = CsvTools.ReadCSVToTable(indicesPath, true);
                DataTableTools.WriteTable2Console(dt);
            }
            string imagePath = Path.Combine(outputDir, sonogramFname);
            FileInfo fiImage = new FileInfo(imagePath);
            if (fiImage.Exists)
            {
                ProcessRunner process = new ProcessRunner(imageViewer);
                process.Run(imagePath, outputDir);
            }

            Console.WriteLine("\n\n# Finished analysis:- " + Path.GetFileName(recordingPath));
            Console.ReadLine();
        } //Dev()


        /// <summary>
        /// A WRAPPER AROUND THE analyser.Analyse(analysisSettings) METHOD
        /// To be called as an executable with command line arguments.
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="configPath"></param>
        /// <param name="outputPath"></param>
        public static int Execute(string[] args)
        {
            int status = 0;
            if (args.Length < 4)
            {
                Console.WriteLine("Require at least 4 command line arguments.");
                status = 1;
                return status;
            }
            //GET FIRST THREE OBLIGATORY COMMAND LINE ARGUMENTS
            string recordingPath = args[0];
            string configPath = args[1];
            string outputDir = args[2];
            FileInfo fiSource = new FileInfo(recordingPath);
            if (!fiSource.Exists)
            {
                Console.WriteLine("Source file does not exist: " + recordingPath);
                status = 2;
                return status;
            }
            FileInfo fiConfig = new FileInfo(configPath);
            if (!fiConfig.Exists)
            {
                Console.WriteLine("Config file does not exist: " + configPath);
                status = 2;
                return status;
            }
            DirectoryInfo diOP = new DirectoryInfo(outputDir);
            if (!diOP.Exists)
            {
                Console.WriteLine("Output directory does not exist: " + outputDir);
                status = 2;
                return status;
            }

            //INIT SETTINGS
            AnalysisSettings analysisSettings = new AnalysisSettings();
            analysisSettings.ConfigFile = fiConfig;
            analysisSettings.AnalysisRunDirectory = diOP;
            analysisSettings.AudioFile = null;
            analysisSettings.EventsFile = null;
            analysisSettings.IndicesFile = null;
            analysisSettings.ImageFile = null;
            TimeSpan tsStart = new TimeSpan(0, 0, 0);
            TimeSpan tsDuration = new TimeSpan(0, 0, 0);
            var configuration = new ConfigDictionary(analysisSettings.ConfigFile.FullName);
            analysisSettings.ConfigDict = configuration.GetTable();


            //PROCESS REMAINDER OF THE OPTIONAL COMMAND LINE ARGUMENTS
            for (int i = 3; i < args.Length; i++)
            {
                string[] parts = args[i].Split(':');
                if (parts[0].StartsWith("-tmpwav"))
                {
                    var outputWavPath = Path.Combine(outputDir, parts[1]);
                    analysisSettings.AudioFile = new FileInfo(outputWavPath);
                }
                else
                    if (parts[0].StartsWith("-events"))
                    {
                        string eventsPath = Path.Combine(outputDir, parts[1]);
                        analysisSettings.EventsFile = new FileInfo(eventsPath);
                    }
                    else
                        if (parts[0].StartsWith("-indices"))
                        {
                            string indicesPath = Path.Combine(outputDir, parts[1]);
                            analysisSettings.IndicesFile = new FileInfo(indicesPath);
                        }
                        else
                            if (parts[0].StartsWith("-sgram"))
                            {
                                string sonoImagePath = Path.Combine(outputDir, parts[1]);
                                analysisSettings.ImageFile = new FileInfo(sonoImagePath);
                            }
                            else
                                if (parts[0].StartsWith("-start"))
                                {
                                    int s = Int32.Parse(parts[1]);
                                    tsStart = new TimeSpan(0, 0, s);
                                }
                                else
                                    if (parts[0].StartsWith("-duration"))
                                    {
                                        int s = Int32.Parse(parts[1]);
                                        tsDuration = new TimeSpan(0, 0, s);
                                        if (tsDuration.TotalMinutes > 10)
                                        {
                                            Console.WriteLine("Segment duration cannot exceed 10 minutes.");
                                            status = 3;
                                            return status;
                                        }
                                    }
            }

            //EXTRACT THE REQUIRED RECORDING SEGMENT
            FileInfo tempF = analysisSettings.AudioFile;
            if (tsDuration.TotalSeconds == 0)   //Process entire file
            {
                AudioFilePreparer.PrepareFile(fiSource, tempF, RESAMPLE_RATE);
                //var fiSegment = AudioFilePreparer.PrepareFile(diOutputDir, fiSourceFile, , Human2.RESAMPLE_RATE);
            }
            else
            {
                AudioFilePreparer.PrepareFile(fiSource, tempF, RESAMPLE_RATE, tsStart, tsStart.Add(tsDuration));
                //var fiSegmentOfSourceFile = AudioFilePreparer.PrepareFile(diOutputDir, new FileInfo(recordingPath), MediaTypes.MediaTypeWav, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(3), RESAMPLE_RATE);
            }

            //DO THE ANALYSIS
            //#############################################################################################################################################
            IAnalyser analyser = new LSKiwi2();
            AnalysisResult result = analyser.Analyse(analysisSettings);
            DataTable dt = result.Data;
            //#############################################################################################################################################

            //ADD IN ADDITIONAL INFO TO RESULTS TABLE
            if (dt != null)
            {
                AddContext2Table(dt, tsStart, result.AudioDuration);
                CsvTools.DataTable2CSV(dt, analysisSettings.EventsFile.FullName);
                //DataTableTools.WriteTable(augmentedTable);
            }
            else
            {
                return -993;  //error!!
            }

            return status;
        }

        private static readonly object imageWriteLock = new object();

        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            //var configuration = new ConfigDictionary(analysisSettings.ConfigFile.FullName);
            //Dictionary<string, string> configDict = configuration.GetTable();
            var fiAudioF    = analysisSettings.AudioFile;
            var diOutputDir = analysisSettings.AnalysisRunDirectory;

            var analysisResults = new AnalysisResult();
            analysisResults.AnalysisIdentifier = this.Identifier;
            analysisResults.SettingsUsed = analysisSettings;
            analysisResults.Data = null;

            //######################################################################
            var results = Analysis(fiAudioF, analysisSettings.ConfigDict);
            //######################################################################

            if (results == null) return analysisResults; //nothing to process 
            var sonogram = results.Item1;
            var hits = results.Item2;
            var scores = results.Item3;
            var predictedEvents = results.Item4;
            var recordingTimeSpan = results.Item5;
            analysisResults.AudioDuration = recordingTimeSpan;

            DataTable dataTableOfEvents = null;

            if ((predictedEvents != null) && (predictedEvents.Count != 0))
            {
                string analysisName = analysisSettings.ConfigDict[AudioAnalysisTools.Keys.ANALYSIS_NAME];
                string fName = Path.GetFileNameWithoutExtension(fiAudioF.Name);
                foreach (AcousticEvent ev in predictedEvents)
                {
                    ev.SourceFileName = fName;
                    //ev.Name = analysisName; //name was added previously
                    ev.SourceFileDuration = recordingTimeSpan.TotalSeconds;
                }
                //write events to a data table to return.
                dataTableOfEvents = WriteEvents2DataTable(predictedEvents);
                string sortString = AudioAnalysisTools.Keys.EVENT_START_SEC + " ASC";
                dataTableOfEvents = DataTableTools.SortTable(dataTableOfEvents, sortString); //sort by start time before returning
            }

            if ((analysisSettings.EventsFile != null) && (dataTableOfEvents != null))
            {
                CsvTools.DataTable2CSV(dataTableOfEvents, analysisSettings.EventsFile.FullName);
            }

            if ((analysisSettings.IndicesFile != null) && (dataTableOfEvents != null))
            {
                double scoreThreshold = 0.1;
                TimeSpan unitTime = TimeSpan.FromSeconds(60); //index for each time span of i minute
                var indicesDT = ConvertEvents2Indices(dataTableOfEvents, unitTime, recordingTimeSpan, scoreThreshold);
                CsvTools.DataTable2CSV(indicesDT, analysisSettings.IndicesFile.FullName);
            }

            //save image of sonograms
            if ((sonogram != null) && (analysisSettings.ImageFile != null))
            {
                var fileExists = File.Exists(analysisSettings.ImageFile.FullName);
                string imagePath = analysisSettings.ImageFile.FullName;
                double eventThreshold = 0.1;
                Image image = DrawSonogram(sonogram, hits, scores, predictedEvents, eventThreshold);
                //image.Save(imagePath, ImageFormat.Png);

                lock (imageWriteLock)
                {
                    //try
                    //{
                    image.Save(analysisSettings.ImageFile.FullName, ImageFormat.Png);
                    //}
                    //catch (Exception ex)
                    //{

                    //}
                }
            }

            analysisResults.Data = dataTableOfEvents;
            analysisResults.ImageFile = analysisSettings.ImageFile;
            analysisResults.AudioDuration = recordingTimeSpan;
            //result.DisplayItems = { { 0, "example" }, { 1, "example 2" }, }
            //result.OutputFiles = { { "exmaple file key", new FileInfo("Where's that file?") } }
            return analysisResults;
        } //Analyse()




        /// <summary>
        /// ################ THE KEY ANALYSIS METHOD
        /// Returns a DataTable
        /// </summary>
        /// <param name="fiSegmentOfSourceFile"></param>
        /// <param name="configDict"></param>
        /// <param name="diOutputDir"></param>
        public static System.Tuple<BaseSonogram, Double[,], List<double[]>, List<AcousticEvent>, TimeSpan>
                                                                                   Analysis(FileInfo fiSegmentOfSourceFile, Dictionary<string, string> config)
        {
            int minHzMale = ConfigDictionary.GetInt(LSKiwi1.key_MIN_HZ_MALE, config);
            int maxHzMale = ConfigDictionary.GetInt(LSKiwi1.key_MAX_HZ_MALE, config);
            int minHzFemale = ConfigDictionary.GetInt(LSKiwi1.key_MIN_HZ_FEMALE, config);
            int maxHzFemale = ConfigDictionary.GetInt(LSKiwi1.key_MAX_HZ_FEMALE, config);
            int frameLength = ConfigDictionary.GetInt(LSKiwi1.key_FRAME_LENGTH, config);
            double frameOverlap = ConfigDictionary.GetDouble(LSKiwi1.key_FRAME_OVERLAP, config);
            //double dctDuration = ConfigDictionary.GetDouble(LSKiwi1.key_DCT_DURATION, config);
            //double dctThreshold = ConfigDictionary.GetDouble(LSKiwi1.key_DCT_THRESHOLD, config);
            double minPeriod = ConfigDictionary.GetDouble(LSKiwi1.key_MIN_PERIODICITY, config);
            double maxPeriod = ConfigDictionary.GetDouble(LSKiwi1.key_MAX_PERIODICITY, config);
            double eventThreshold = ConfigDictionary.GetDouble(Keys.EVENT_THRESHOLD, config);
            double minDuration = ConfigDictionary.GetDouble(LSKiwi1.key_MIN_DURATION, config); //minimum event duration to qualify as species call
            double maxDuration = ConfigDictionary.GetDouble(LSKiwi1.key_MAX_DURATION, config); //maximum event duration to qualify as species call

            AudioRecording recording = new AudioRecording(fiSegmentOfSourceFile.FullName);
            if (recording == null)
            {
                Console.WriteLine("AudioRecording == null. Analysis not possible.");
                return null;
            }
            TimeSpan tsRecordingtDuration = recording.Duration();

            //i: MAKE SONOGRAM
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.WindowSize = frameLength;
            sonoConfig.WindowOverlap = frameOverlap;
            sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD; //MUST DO NOISE REMOVAL
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            
            //DETECT MALE KIWI
            var resultsMale = DetectKiwi(sonogram, minHzMale, maxHzMale, /*dctDuration, dctThreshold,*/  minPeriod, maxPeriod, eventThreshold, minDuration, maxDuration);
            var scoresM = resultsMale.Item1;
            var hitsM = resultsMale.Item2;
            var predictedEventsM = resultsMale.Item3;
            foreach (AcousticEvent ev in predictedEventsM) ev.Name = "LSK(m)";
            //DETECT FEMALE KIWI
            var resultsFemale = DetectKiwi(sonogram, minHzFemale, maxHzFemale,/* dctDuration, dctThreshold,*/ minPeriod, maxPeriod, eventThreshold, minDuration, maxDuration);
            var scoresF = resultsFemale.Item1;
            var hitsF = resultsFemale.Item2;
            var predictedEventsF = resultsFemale.Item3;
            foreach (AcousticEvent ev in predictedEventsF) ev.Name = "LSK(f)";

            //combine the male and female results
            hitsM = MatrixTools.AddMatrices(hitsM, hitsF);
            foreach (AcousticEvent ev in predictedEventsF) predictedEventsM.Add(ev);
            foreach (double[] array in scoresF) scoresM.Add(array);

            return System.Tuple.Create(sonogram, hitsM, scoresM, predictedEventsM, tsRecordingtDuration);
        } //Analysis()

        public static System.Tuple<List<double[]>, double[,], List<AcousticEvent>> DetectKiwi(BaseSonogram sonogram, int minHz, int maxHz, 
                                    /* double dctDuration, double dctThreshold, */ double minPeriod, double maxPeriod, double eventThreshold, double minDuration, double maxDuration)
        {
            int step = (int)Math.Round(sonogram.FramesPerSecond); //take one second steps
            int sampleLength = 32; //32 frames = 1.85 seconds.   64 frames (i.e. 3.7 seconds) is to long a sample - require stationarity.

            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);
            double minFramePeriod = minPeriod * sonogram.FramesPerSecond;
            double maxFramePeriod = maxPeriod * sonogram.FramesPerSecond;

            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);


            //#############################################################################################################################################
            //window    sr          frameDuration   frames/sec  hz/bin  64frameDuration hz/64bins       hz/128bins
            // 1024     22050       46.4ms          21.5        21.5    2944ms          1376hz          2752hz
            // 1024     17640       58.0ms          17.2        17.2    3715ms          1100hz          2200hz
            // 2048     17640       116.1ms          8.6         8.6    7430ms           551hz          1100hz
            double[] fullArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, minBin, (rowCount - 1), minBin + 130);
            var result1 = CrossCorrelation.DetectXcorrelationInTwoArrays(fullArray, fullArray, step, sampleLength, minFramePeriod, maxFramePeriod);
            double[] intensity1 = result1.Item1;
            double[] periodicity1 = result1.Item2;
            intensity1 = DataTools.filterMovingAverage(intensity1, 11);

            //#############################################################################################################################################
            //double[] lowerArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, minBin, (rowCount - 1), minBin + 65);
            //double[] upperArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, minBin+66, (rowCount - 1), minBin+130);
            //int actualMaxHz     = (int)Math.Round((minBin+130) * sonogram.FBinWidth);
            //var result2 = CrossCorrelation.DetectXcorrelationInTwoArrays(lowerArray, upperArray, step, sampleLength, minFramePeriod, maxFramePeriod);
            //double[] intensity2   = result2.Item1;
            //double[] periodicity2 = result2.Item2;
            //intensity2 = DataTools.filterMovingAverage(intensity2, 5);

            //#############################################################################################################################################
            //minFramePeriod = 4;  
            //maxFramePeriod = 14;
            //var return3 = Gratings.ScanArrayForGratingPattern(fullArray, (int)minFramePeriod, (int)maxFramePeriod, 4, step);
            //var return3 = Gratings.ScanArrayForGratingPattern(fullArray, step, 4, 4);
            //var return4 = Gratings.ScanArrayForGratingPattern(fullArray, step, 4, 5);
            //var return5 = Gratings.ScanArrayForGratingPattern(fullArray, step, 4, 8);
            //var return6 = Gratings.ScanArrayForGratingPattern(fullArray, step, 4, 10);
            //var return7 = Gratings.ScanArrayForGratingPattern(fullArray, step, 4, 12);

            //#############################################################################################################################################
            //bool normaliseDCT = true;
            //Double[,] maleHits;                       //predefinition of hits matrix - to superimpose on sonogram image
            //double[] maleScores;                      //predefinition of score array
            //double[] maleOscRate;
            //List<AcousticEvent> predictedMaleEvents;
            //double minOscilFreq = 1 / maxPeriod;  //convert max period (seconds) to oscilation rate (Herz).
            //double maxOscilFreq = 1 / minPeriod;  //convert min period (seconds) to oscilation rate (Herz).
            //OscillationAnalysis.Execute((SpectralSonogram)sonogram, minHz, maxHz, dctDuration, dctThreshold, normaliseDCT,
            //                             minOscilFreq, maxOscilFreq, eventThreshold, minDuration, maxDuration,
            //                             out maleScores, out predictedMaleEvents, out maleHits, out maleOscRate);            
            
            //iii: CONVERT SCORES TO ACOUSTIC EVENTS
            List<AcousticEvent> events = AcousticEvent.ConvertScoreArray2Events(intensity1, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth,
                                                                                         eventThreshold, minDuration, maxDuration);

            CropEvents(events, fullArray, minDuration);
            CalculateAvIntensityScore(events, intensity1);
            CalculateDeltaPeriodScore(events, periodicity1, minFramePeriod, maxFramePeriod);
            CalculateBandWidthScore(events, sonogram.Data);
            CalculatePeaksScore(events, fullArray);
            //FilterEvents(events);
            CalculateWeightedEventScore(events);

            // PREPARE HITS MATRIX
            var hits = new double[rowCount, colCount];
            double range = maxFramePeriod - minFramePeriod;
            for (int r = 0; r < rowCount; r++)
            {
                if (intensity1[r] > eventThreshold)
                for (int c = minBin; c < maxBin; c++)
                {
                    hits[r, c] = (periodicity1[r] - minFramePeriod) / range; //normalisation
                }
            }

            periodicity1 = CropArrayToEvents(events, periodicity1); //for display only

            var scores = new List<double[]>();
            scores.Add(DataTools.normalise(fullArray));
            //scores.Add(DataTools.normalise(upperArray));
            //scores.Add(DataTools.normalise(lowerArray));
            scores.Add(DataTools.normalise(intensity1));
            scores.Add(DataTools.normalise(periodicity1));
            //scores.Add(DataTools.normalise(intensity2));
            //scores.Add(DataTools.normalise(return3));
            //scores.Add(DataTools.normalise(return4));
            //scores.Add(DataTools.normalise(return5));
            //scores.Add(DataTools.normalise(return6));
            //scores.Add(DataTools.normalise(return7));
            //scores.Add(DataTools.normalise(maleScores));
            //scores.Add(DataTools.normalise(maleOscRate));
            return System.Tuple.Create(scores, hits, events);
        }

        public static void CropEvents(List<AcousticEvent> events, double[] activity, double minDurationInSeconds)
        {
            double severity = 0.2;
            int length = activity.Length;

            foreach (AcousticEvent ev in events)
            {
                int start = ev.oblong.r1;
                int end = ev.oblong.r2;
                double[] subArray = DataTools.Subarray(activity, start, end - start + 1);
                int[] bounds = DataTools.Peaks_CropLowAmplitude(subArray, severity);

                int newMinRow = start + bounds[0];
                int newMaxRow = start + bounds[1];
                if (newMaxRow >= length) newMaxRow = length - 1;

                Oblong o = new Oblong(newMinRow, ev.oblong.c1, newMaxRow, ev.oblong.c2);
                ev.oblong = o;
                ev.TimeStart = newMinRow  * ev.FrameOffset;
                ev.TimeEnd   = newMaxRow  * ev.FrameOffset;
                ev.Duration  = ev.TimeEnd - ev.TimeStart;
                //int frameCount = (int)Math.Round(ev.Duration / ev.FrameOffset); 
            }
            for (int i = events.Count - 1; i >= 0; i--) if (events[i].Duration < minDurationInSeconds) events.Remove(events[i]);
        }

        public static double[] CropArrayToEvents(List<AcousticEvent> events, double[] array)
        {
            int length = array.Length;
            double[] returnArray = new double[length];
            foreach (AcousticEvent ev in events)
            {
                int start = ev.oblong.r1;
                int end = ev.oblong.r2;
                for (int i = start; i < end; i++) returnArray[i] = array[i];
            }
            return returnArray;
        }

        public static void CalculateAvIntensityScore(List<AcousticEvent> events, double[] intensity)
        {
            //periodicity score is the average intensity response to the periodicity - i.e. amplitude of the FFT
            foreach (AcousticEvent ev in events)
            {
                int start = ev.oblong.r1;
                int end = ev.oblong.r2;
                int length = end - start + 1;
                double avIntensity = 0.0;
                for (int i = start; i <= end; i++) avIntensity += intensity[i];
                ev.kiwi_intensityScore = avIntensity / (double)length;
            }

        }

        public static void CalculateDeltaPeriodScore(List<AcousticEvent> events, double[] periodicity, double  minFramePeriod, double maxFramePeriod)
        {
            double halfPeriodicityRange = (maxFramePeriod - minFramePeriod) / 2;

            foreach (AcousticEvent ev in events)
            {
                int start = ev.oblong.r1;
                int end = ev.oblong.r2;
                int halfEventLength = (end - start + 1) / 2;
                double[] subArray = DataTools.Subarray(periodicity, start, halfEventLength);
                double startAv = 0.0;
                for(int i = 0; i < 5; i++) startAv += subArray[i];
                double endAv   = 0.0;
                for (int i = subArray.Length-5; i < subArray.Length; i++) endAv += subArray[i];
                double delta = (endAv - startAv) / (double)5; //get the average of 5 values
                ev.kiwi_deltaPeriodScore = delta / halfPeriodicityRange; //normalisation;
            }
        }


        public static void CalculateBandWidthScore(List<AcousticEvent> events, double[,] sonogramMatrix)
        {
            foreach (AcousticEvent ev in events)
            {
                ev.kiwi_bandWidthScore = CalculateKiwiBandWidthScore(ev, sonogramMatrix);
            }
        }


        /// <summary>
        /// Checks that the passed acoustic event does not have acoustic activity that spills outside the kiwi bandwidth.
        /// </summary>
        /// <param name="ae"></param>
        /// <param name="sonogramMatrix"></param>
        /// <returns></returns>
        public static double CalculateKiwiBandWidthScore(AcousticEvent ae, double[,] sonogramMatrix)
        {
            int eventLength = (int)Math.Round(ae.Duration * ae.FramesPerSecond);
            double[] event_dB = new double[eventLength]; //dB profile for event
            double[] upper_dB = new double[eventLength]; //dB profile for bandwidth above event
            double[] lower_dB = new double[eventLength]; //dB profile for bandwidth below event
            int eventHt = ae.oblong.ColWidth;
            int halfHt = eventHt / 2;
            int buffer = 20; //avoid this margin around the event
            //get acoustic activity within the event bandwidth and above it.
            for (int r = 0; r < eventLength; r++)
            {
                for (int c = 0; c < eventHt; c++) event_dB[r] += sonogramMatrix[ae.oblong.r1 + r, ae.oblong.c1 + c]; //event dB profile
                for (int c = 0; c < halfHt; c++)  upper_dB[r] += sonogramMatrix[ae.oblong.r1 + r, ae.oblong.c2 + c + buffer];
                for (int c = 0; c < halfHt; c++)  lower_dB[r] += sonogramMatrix[ae.oblong.r1 + r, ae.oblong.c1 - halfHt - buffer + c];
                //for (int c = 0; c < eventHt; c++) noiseReducedMatrix[ae.oblong.r1 + r, ae.oblong.c1 + c]     = 20.0; //mark matrix
                //for (int c = 0; c < eventHt; c++) noiseReducedMatrix[ae.oblong.r1 + r, ae.oblong.c2 + 5 + c] = 40.0; //mark matrix
            }
            for (int r = 0; r < eventLength; r++) event_dB[r] /= eventHt; //calculate average.
            for (int r = 0; r < eventLength; r++) upper_dB[r] /= halfHt;
            for (int r = 0; r < eventLength; r++) lower_dB[r] /= halfHt;

            //event_dB = DataTools.normalise(event_dB);
            //upper_dB = DataTools.normalise(upper_dB);

            double upperCC = DataTools.CorrelationCoefficient(event_dB, upper_dB);
            double lowerCC = DataTools.CorrelationCoefficient(event_dB, lower_dB);
            if (upperCC < 0.0) upperCC = 0.0;
            if (lowerCC < 0.0) lowerCC = 0.0;
            double CCscore = upperCC + lowerCC;
            if (CCscore > 1.0) CCscore = 1.0;
            return 1 - CCscore;
        }



        public static void CalculatePeaksScore(List<AcousticEvent> events, double[] dBArray)
        {
            foreach (AcousticEvent ev in events)
            {
                int start = ev.oblong.r1;
                int end = ev.oblong.r2;
                int eventLength = end - start + 1;
                double[] subArray = DataTools.Subarray(dBArray, start, eventLength);
                var tuple = KiwiPeakAnalysis(ev, subArray);
                double snrScore = tuple.Item1 / 20.0;  //snrScore - 20.0dB is normalisation factor
                if (snrScore > 1.0) snrScore = 1.0;
                ev.kiwi_snrScore = snrScore;  //snrScore - 20.0dB is normalisation factor
                double sdPeakScore = tuple.Item1 / tuple.Item2 / 20.0; //sdPeakScore = av/sd / 20.0.   20=normalisation factor
                if (sdPeakScore > 1.0) sdPeakScore = 1.0;
                ev.kiwi_sdPeakScore = sdPeakScore;  
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ae">an acoustic event</param>
        /// <param name="dbArray">The sequence of frame dB over the event</param>
        /// <returns></returns>
        public static System.Tuple<double, double> KiwiPeakAnalysis(AcousticEvent ae, double[] dbArray)
        {

            //dbArray = DataTools.filterMovingAverage(dbArray, 3);
            bool[] peaks = DataTools.GetPeaks(dbArray);  //locate the peaks
            double[] peakValues = new double[dbArray.Length];
            for (int i = 0; i < dbArray.Length; i++)
            {
                if(peaks[i])  peakValues[i] = dbArray[i];
            }

            //take the top N peaks
            int N = 5;
            double[] topNValues = new double[N];
            for (int p = 0; p < N; p++)
            {
                int maxID = DataTools.GetMaxIndex(peakValues);
                topNValues[p] = peakValues[maxID];
                peakValues[maxID] = 0.0;
            }        
            //PROCESS PEAK DECIBELS
            double avPeakDB, sdPeakDB;
            NormalDist.AverageAndSD(topNValues, out avPeakDB, out sdPeakDB);
            return System.Tuple.Create(avPeakDB, sdPeakDB);
        }



        public static void CalculateWeightedEventScore(List<AcousticEvent> events)
        {
            foreach (AcousticEvent ev in events)
            {
                //double comboScore = (snrScore * 0.1)        +   (sdPeakScore * 0.1)         + (ev.kiwi_intensityScore * 0.1) + (periodicityScore * 0.3) + (bandWidthScore * 0.5); //weighted sum
                ev.ScoreNormalised = (ev.kiwi_snrScore * 0.05) + (ev.kiwi_sdPeakScore * 0.25) + (ev.kiwi_intensityScore * 0.2) + (ev.kiwi_deltaPeriodScore * 0.3) + (ev.kiwi_bandWidthScore * 0.2); 
            }
        }





        static Image DrawSonogram(BaseSonogram sonogram, double[,] hits, List<double[]> scores, List<AcousticEvent> predictedEvents, double eventThreshold)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            int maxFreq = sonogram.NyquistFrequency / 2;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(maxFreq, 1, doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            if (scores != null)
            {
                foreach(double[] array in scores) image.AddTrack(Image_Track.GetScoreTrack(array, 0.0, 1.0, eventThreshold));
            }
            if (hits != null) image.OverlayRainbowTransparency(hits);
            if (predictedEvents.Count > 0) image.AddEvents(predictedEvents, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            return image.GetImage();
        } //DrawSonogram()




        public static DataTable WriteEvents2DataTable(List<AcousticEvent> predictedEvents)
        {
            if (predictedEvents == null) return null;
            string[] headers = { AudioAnalysisTools.Keys.EVENT_COUNT,
                                 AudioAnalysisTools.Keys.EVENT_START_MIN,
                                 AudioAnalysisTools.Keys.EVENT_START_SEC, 
                                 AudioAnalysisTools.Keys.EVENT_START_ABS,
                                 AudioAnalysisTools.Keys.SEGMENT_TIMESPAN,
                                 AudioAnalysisTools.Keys.EVENT_DURATION, 
                                 AudioAnalysisTools.Keys.EVENT_INTENSITY,
                                 AudioAnalysisTools.Keys.EVENT_NAME,
                                 key_BANDWIDTH_SCORE,
                                 key_DELTA_SCORE,
                                 key_PEAKS_SNR_SCORE,
                                 key_PEAKS_STD_SCORE,
                                 AudioAnalysisTools.Keys.EVENT_SCORE,
                                 AudioAnalysisTools.Keys.EVENT_NORMSCORE 

                               };
            //                   1                2               3              4                5              6               7              8
            Type[] types = { typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(string), 
                             typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double) };

            var dataTable = DataTableTools.CreateTable(headers, types);
            if (predictedEvents.Count == 0) return dataTable;

            foreach (var ev in predictedEvents)
            {
                DataRow row = dataTable.NewRow();
                row[AudioAnalysisTools.Keys.EVENT_START_ABS] = (double)ev.TimeStart;  //Set now - will overwrite later
                row[AudioAnalysisTools.Keys.EVENT_START_SEC] = (double)ev.TimeStart;  //EvStartSec
                row[AudioAnalysisTools.Keys.EVENT_DURATION]  = (double)ev.Duration;   //duratio in seconds
                row[AudioAnalysisTools.Keys.EVENT_INTENSITY] = (double)ev.kiwi_intensityScore;   //
                row[AudioAnalysisTools.Keys.EVENT_NAME]      = (string)ev.Name;   //
                row[key_BANDWIDTH_SCORE]                     = (double)ev.kiwi_bandWidthScore;  
                row[key_DELTA_SCORE]                         = (double)ev.kiwi_deltaPeriodScore;
                row[key_PEAKS_SNR_SCORE]                     = (double)ev.kiwi_snrScore;
                row[key_PEAKS_STD_SCORE]                     = (double)ev.kiwi_sdPeakScore;
                row[AudioAnalysisTools.Keys.EVENT_NORMSCORE] = (double)ev.ScoreNormalised;
                row[AudioAnalysisTools.Keys.EVENT_SCORE]     = (double)ev.Score;      //Score
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }



        /// <summary>
        /// Converts a DataTable of events to a datatable where one row = one minute of indices
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public DataTable ConvertEvents2Indices(DataTable dt, TimeSpan unitTime, TimeSpan sourceDuration, double scoreThreshold)
        {
            if (dt == null) return null;

            if ((sourceDuration == null) || (sourceDuration == TimeSpan.Zero)) return null;
            double units = sourceDuration.TotalSeconds / unitTime.TotalSeconds;
            int unitCount = (int)(units / 1);   //get whole minutes
            if (units % 1 > 0.0) unitCount += 1; //add fractional minute
            int[] eventsPerUnitTime = new int[unitCount]; //to store event counts
            int[] bigEvsPerUnitTime = new int[unitCount]; //to store counts of high scoring events

            foreach (DataRow ev in dt.Rows)
            {
                double eventStart = (double)ev[AudioAnalysisTools.Keys.EVENT_START_SEC];
                double eventScore = (double)ev[AudioAnalysisTools.Keys.EVENT_NORMSCORE];
                int timeUnit = (int)(eventStart / unitTime.TotalSeconds);
                eventsPerUnitTime[timeUnit]++;
                if (eventScore > scoreThreshold) bigEvsPerUnitTime[timeUnit]++;
            }

            string[] headers = { AudioAnalysisTools.Keys.START_MIN, AudioAnalysisTools.Keys.EVENT_TOTAL, ("#Ev>" + scoreThreshold) };
            Type[]   types   = { typeof(int), typeof(int), typeof(int) };
            var newtable = DataTableTools.CreateTable(headers, types);

            for (int i = 0; i < eventsPerUnitTime.Length; i++)
            {
                int unitID = (int)(i * unitTime.TotalMinutes);
                newtable.Rows.Add(unitID, eventsPerUnitTime[i], bigEvsPerUnitTime[i]);
            }
            return newtable;
        }

        public static void AddContext2Table(DataTable dt, TimeSpan segmentStartMinute, TimeSpan recordingTimeSpan)
        {
            if (!dt.Columns.Contains(Keys.SEGMENT_TIMESPAN)) dt.Columns.Add(AudioAnalysisTools.Keys.SEGMENT_TIMESPAN, typeof(double));
            if (!dt.Columns.Contains(Keys.EVENT_START_ABS))  dt.Columns.Add(AudioAnalysisTools.Keys.EVENT_START_ABS,  typeof(double));
            if (!dt.Columns.Contains(Keys.EVENT_START_MIN))  dt.Columns.Add(AudioAnalysisTools.Keys.EVENT_START_MIN,  typeof(double));
            double start = segmentStartMinute.TotalSeconds;
            foreach (DataRow row in dt.Rows)
            {
                row[AudioAnalysisTools.Keys.SEGMENT_TIMESPAN] = recordingTimeSpan.TotalSeconds;
                row[AudioAnalysisTools.Keys.EVENT_START_ABS] = start + (double)row[AudioAnalysisTools.Keys.EVENT_START_SEC];
                row[AudioAnalysisTools.Keys.EVENT_START_MIN] = start;
            }
        } //AddContext2Table()
        
     public Tuple<DataTable, DataTable> ProcessCsvFile(FileInfo fiCsvFile, FileInfo fiConfigFile)
     {
         DataTable dt = CsvTools.ReadCSVToTable(fiCsvFile.FullName, true); //get original data table
         if ((dt == null) || (dt.Rows.Count == 0)) return null;
         //get its column headers
         var dtHeaders = new List<string>();
         var dtTypes = new List<Type>();
         foreach (DataColumn col in dt.Columns)
         {
             dtHeaders.Add(col.ColumnName);
             dtTypes.Add(col.DataType);
         }

         List<string> displayHeaders = null;
         //check if config file contains list of display headers
         if (fiConfigFile != null)
         {
             var configuration = new ConfigDictionary(fiConfigFile.FullName);
             Dictionary<string, string> configDict = configuration.GetTable();
             if (configDict.ContainsKey(Keys.DISPLAY_COLUMNS))
                 displayHeaders = configDict[Keys.DISPLAY_COLUMNS].Split(',').ToList();
         }
         //if config file does not exist or does not contain display headers then use the original headers
         if (displayHeaders == null) displayHeaders = dtHeaders; //use existing headers if user supplies none.

         //now determine how to display tracks in display datatable
         Type[] displayTypes = new Type[displayHeaders.Count];
         bool[] canDisplay = new bool[displayHeaders.Count];
         for (int i = 0; i < displayTypes.Length; i++)
         {
             displayTypes[i] = typeof(double);
             canDisplay[i] = false;
             if (dtHeaders.Contains(displayHeaders[i])) canDisplay[i] = true;
         }

         DataTable table2Display = DataTableTools.CreateTable(displayHeaders.ToArray(), displayTypes);
         foreach (DataRow row in dt.Rows)
         {
             DataRow newRow = table2Display.NewRow();
             for (int i = 0; i < canDisplay.Length; i++)
             {
                 if (canDisplay[i]) newRow[displayHeaders[i]] = row[displayHeaders[i]];
                 else newRow[displayHeaders[i]] = 0.0;
             }
             table2Display.Rows.Add(newRow);
         }

         //order the table if possible
         if (dt.Columns.Contains(AudioAnalysisTools.Keys.EVENT_START_ABS))
         {
             dt = DataTableTools.SortTable(dt, AudioAnalysisTools.Keys.EVENT_START_ABS + " ASC");
         }
         else if (dt.Columns.Contains(AudioAnalysisTools.Keys.EVENT_COUNT))
         {
             dt = DataTableTools.SortTable(dt, AudioAnalysisTools.Keys.EVENT_COUNT + " ASC");
         }
         else if (dt.Columns.Contains(AudioAnalysisTools.Keys.INDICES_COUNT))
         {
             dt = DataTableTools.SortTable(dt, AudioAnalysisTools.Keys.INDICES_COUNT + " ASC");
         }
         else if (dt.Columns.Contains(AudioAnalysisTools.Keys.START_MIN))
         {
             dt = DataTableTools.SortTable(dt, AudioAnalysisTools.Keys.START_MIN + " ASC");
         }

         table2Display = NormaliseColumnsOfDataTable(table2Display);
         return System.Tuple.Create(dt, table2Display);
     } // ProcessCsvFile()



     /// <summary>
     /// takes a data table of indices and normalises column values to values in [0,1].
     /// </summary>
     /// <param name="dt"></param>
     /// <returns></returns>
     public static DataTable NormaliseColumnsOfDataTable(DataTable dt)
     {
         string[] headers = DataTableTools.GetColumnNames(dt);
         string[] newHeaders = new string[headers.Length];

         List<double[]> newColumns = new List<double[]>();

         for (int i = 0; i < headers.Length; i++)
         {
             double[] values = DataTableTools.Column2ArrayOfDouble(dt, headers[i]); //get list of values
             if ((values == null) || (values.Length == 0)) continue;

             double min = 0;
             double max = 1;
             if (headers[i].Equals(Keys.AV_AMPLITUDE))
             {
                 min = -50;
                 max = -5;
                 newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                 newHeaders[i] = headers[i] + "  (-50..-5dB)";
             }
             else //default is to normalise in [0,1]
             {
                 newColumns.Add(DataTools.normalise(values)); //normalise all values in [0,1]
                 newHeaders[i] = headers[i];
             }
         } //for loop

         //convert type int to type double due to normalisation
         Type[] types = new Type[newHeaders.Length];
         for (int i = 0; i < newHeaders.Length; i++) types[i] = typeof(double);
         var processedtable = DataTableTools.CreateTable(newHeaders, types, newColumns);
         return processedtable;
     }


        public AnalysisSettings DefaultSettings
        {
            get
            {
                return new AnalysisSettings
                {
                    SegmentMaxDuration = TimeSpan.FromMinutes(1),
                    SegmentMinDuration = TimeSpan.FromSeconds(30),
                    SegmentMediaType   = MediaTypes.MediaTypeWav,
                    SegmentOverlapDuration  = TimeSpan.Zero,
                    SegmentTargetSampleRate = AnalysisTemplate.RESAMPLE_RATE
                };
            }
        }


        public string DefaultConfiguration
        {
            get
            {
                return string.Empty;
            }
        }

    } //end class LSKiwi2
}
