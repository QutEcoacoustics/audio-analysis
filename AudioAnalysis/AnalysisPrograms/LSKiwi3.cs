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
    public class LSKiwi3 : IAnalyser
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
        public static string key_GRID_SCORE      = "GridScore";
        public static string key_CHIRP_SCORE     = "ChirpScore";
        public static string key_EVENT_NORMSCORE = AudioAnalysisTools.Keys.EVENT_NORMSCORE;
        
        public static string[] defaultRules = {
                                           "EXCLUDE_IF_RULE="+key_BANDWIDTH_SCORE+"_LT_0.30",
                                           "EXCLUDE_IF_RULE=feature1_GT_0.45",
                                           "WEIGHT_feature1=0.45",
                                           "WEIGHT_feature2=0.45",
                                           "WEIGHT_feature3=0.45"
        };


        //OTHER CONSTANTS
        public const string ANALYSIS_NAME = "LSKiwi3";
        public const int RESAMPLE_RATE = 17640;
        //public const int RESAMPLE_RATE = 22050;
        //public const string imageViewer = @"C:\Program Files\Windows Photo Viewer\ImagingDevices.exe";
        public const string imageViewer = @"C:\Windows\system32\mspaint.exe";


        public string DisplayName
        {
            get { return "Little Spotted Kiwi v3"; }
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

            string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.LSKiwi3.cfg";

            //string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TOWER_20100208_204500\TOWER_20100208_204500_ANDREWS_SELECTIONS.csv";
            string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500_40m0s.wav";
            string outputDir     = @"C:\SensorNetworks\Output\LSKiwi3\";

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

            string title = "# FOR DETECTION OF THE LITTLE SPOTTED KIWI - version 3 - started 23rd June 2012";
            string date = "# DATE AND TIME: " + DateTime.Now;
            Console.WriteLine(title);
            Console.WriteLine(date);
            Console.WriteLine("# Output folder:  " + outputDir);
            Console.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            var diOutputDir = new DirectoryInfo(outputDir);

            //Log.Verbosity = 1;
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

            //string fName = Path.GetFileNameWithoutExtension(recordingPath);
            DirectoryInfo diOP = new DirectoryInfo(outputDir);
            if (!diOP.Exists)
            {
                //diOP.CreateSubdirectory(fName);
                Console.WriteLine("Output directory did not exist!");
                Console.WriteLine("     Create new directory: " + outputDir);
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
            IAnalyser analyser = new LSKiwi3();
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
            //else
            //{
            //    return -993;  //error!!
            //}

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
                double eventThreshold = ConfigDictionary.GetDouble(Keys.EVENT_THRESHOLD, analysisSettings.ConfigDict);
                TimeSpan unitTime = TimeSpan.FromSeconds(60); //index for each time span of one minute
                var indicesDT = ConvertEvents2Indices(dataTableOfEvents, unitTime, recordingTimeSpan, eventThreshold);
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
        public static System.Tuple<BaseSonogram, Double[,], List<Plot>, List<AcousticEvent>, TimeSpan>
                                                                                   Analysis(FileInfo fiSegmentOfSourceFile, Dictionary<string, string> config)
        {
            int minHzMale = ConfigDictionary.GetInt(LSKiwi1.key_MIN_HZ_MALE, config);
            int maxHzMale = ConfigDictionary.GetInt(LSKiwi1.key_MAX_HZ_MALE, config);
            int minHzFemale = ConfigDictionary.GetInt(LSKiwi1.key_MIN_HZ_FEMALE, config);
            int maxHzFemale = ConfigDictionary.GetInt(LSKiwi1.key_MAX_HZ_FEMALE, config);
            int frameLength = ConfigDictionary.GetInt(LSKiwi1.key_FRAME_LENGTH, config);
            double frameOverlap = ConfigDictionary.GetDouble(LSKiwi1.key_FRAME_OVERLAP, config);
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
            if (tsRecordingtDuration.TotalSeconds < 15)
            {
                Console.WriteLine("Audio recording must be atleast 15 seconds long for analysis.");
                return null;
            }


            //i: MAKE SONOGRAM
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.WindowSize = frameLength;
            sonoConfig.WindowOverlap = frameOverlap;
            sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD; //MUST DO NOISE REMOVAL - XCORR only works well if do noise removal
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            
            //DETECT MALE KIWI
            var resultsMale = DetectKiwi(sonogram, minHzMale, maxHzMale,  minPeriod, maxPeriod, eventThreshold, minDuration, maxDuration);
            var scoresM = resultsMale.Item1;
            var hitsM = resultsMale.Item2;
            var predictedEventsM = resultsMale.Item3;
            foreach (AcousticEvent ev in predictedEventsM) ev.Name = "LSK(m)";

            //DETECT FEMALE KIWI
            var resultsFemale = DetectKiwi(sonogram, minHzFemale, maxHzFemale, minPeriod, maxPeriod, eventThreshold, minDuration, maxDuration);
            var scoresF = resultsFemale.Item1;
            var hitsF = resultsFemale.Item2;
            var predictedEventsF = resultsFemale.Item3;
            foreach (AcousticEvent ev in predictedEventsF) ev.Name = "LSK(f)";

            //combine the male and female results
            hitsM = MatrixTools.AddMatrices(hitsM, hitsF);
            foreach (AcousticEvent ev in predictedEventsF) predictedEventsM.Add(ev);
            foreach (Plot plot in scoresF) scoresM.Add(plot);

            return System.Tuple.Create(sonogram, hitsM, scoresM, predictedEventsM, tsRecordingtDuration);
        } //Analysis()

        public static System.Tuple<List<Plot>, double[,], List<AcousticEvent>> DetectKiwi(BaseSonogram sonogram, int minHz, int maxHz, double minPeriod, double maxPeriod, 
                                                                                              double eventThreshold, double minDuration, double maxDuration)
        {
            int step = (int)Math.Round(sonogram.FramesPerSecond); //take one second steps
            //#############################################################################################################################################
            //                                                          (---frame duration --)  
            //window    sr          frameDuration   frames/sec  hz/bin   32       64      128      hz/64bins       hz/128bins
            // 1024     22050       46.4ms            21.5      21.5            2944ms              1376hz          2752hz
            // 1024     17640       58.0ms            17.2      17.2    1.85s   3.715s    7.42s     1100hz          2200hz  More than 3s is too long a sample - require stationarity.
            // 2048     17640       116.1ms            8.6       8.6    3.71s   7.430s   14.86s      551hz          1100hz
            //@frame size = 1024: 
            //@frame size = 2048: 32 frames = 1.85 seconds.   64 frames  (i.e. 3.7 seconds) is to long a sample - require stationarity.
            int sampleLength = 64; //assuming window = 1024
            if (sonogram.Configuration.WindowSize == 2048) sampleLength = 32;

            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);
            double minFramePeriod = minPeriod * sonogram.FramesPerSecond;
            double maxFramePeriod = maxPeriod * sonogram.FramesPerSecond;

            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);
            int bandWidth = (int)((maxHz - minHz) / sonogram.FBinWidth);

            double[,] subMatrix = MatrixTools.Submatrix(sonogram.Data, 0, minBin, (rowCount - 1), minBin + bandWidth);
            double[] dBArray    = MatrixTools.GetRowAverages(subMatrix);
            var result1 = CrossCorrelation.DetectXcorrelationInTwoArrays(dBArray, dBArray, step, sampleLength, minFramePeriod, maxFramePeriod);
            double[] intensity1 = result1.Item1;
            double[] periodicity1 = result1.Item2;
            intensity1 = DataTools.filterMovingAverage(intensity1, 5);
            //#############################################################################################################################################
            
            //iii MAKE array showing locations of dB peaks and periodicity at that point
            bool[] peaks = DataTools.GetPeaks(dBArray); 
            double[] peakPeriodicity = new double[dBArray.Length];
            for (int r = 0; r < dBArray.Length; r++)
            {
                if ((peaks[r])&&(intensity1[r] > eventThreshold))
                {
                    peakPeriodicity[r] = periodicity1[r];
                }
            }

            double[] gridScore        = CalculateGridScore(dBArray, peakPeriodicity);
            double[] deltaPeriodScore = CalculateDeltaPeriodScore( periodicity1, sonogram.FramesPerSecond);
            double[] chirps           = CalculateKiwiChirpScore(dBArray, peakPeriodicity, subMatrix);
            double[] chirpScores      = ConvertChirpsToScoreArray(chirps, dBArray, sonogram.FramesPerSecond);
            double[] bandWidthScore   = CalculateKiwiBandWidthScore(sonogram, minHz, maxHz, peakPeriodicity);

            double[] comboScore = new double[dBArray.Length];
            for (int r = 0; r < dBArray.Length; r++)
            {
                comboScore[r] = (intensity1[r] * 0.3) + (gridScore[r] * 0.2) + (deltaPeriodScore[r] * 0.2) + (chirpScores[r] * 0.3);
            }

            //iii: CONVERT SCORES TO ACOUSTIC EVENTS
            var events = LSKiwi3.ConvertScoreArray2Events(intensity1, gridScore, deltaPeriodScore, chirpScores, comboScore, bandWidthScore, 
                                                          minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth,
                                                          eventThreshold, minDuration, maxDuration);

            CropEvents(events, dBArray, minDuration);
            //CalculateAvIntensityScore(events, intensity1);
            //CalculateDeltaPeriodScore(events, periodicity1, minFramePeriod, maxFramePeriod);
            //CalculateBandWidthScore(events, sonogram.Data);
            //CalculatePeaksScore(events, dBArray);

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

            var scores = new List<Plot>();
            scores.Add(new Plot("Decibels",           DataTools.normalise(dBArray), 0.1));
            scores.Add(new Plot("Xcorrelation score", DataTools.normalise(intensity1), 0.1));
            scores.Add(new Plot("Delta Period Score", DataTools.normalise(deltaPeriodScore), 0.1));
            scores.Add(new Plot("Grid Score",         DataTools.normalise(gridScore), 0.1));
            scores.Add(new Plot("Chirps", chirps, 0.5));
            scores.Add(new Plot("Chirp Score", chirpScores, 0.1));
            scores.Add(new Plot("Bandwidth Score", bandWidthScore, 0.1));
            scores.Add(new Plot("Combo Score", comboScore, eventThreshold));
            return System.Tuple.Create(scores, hits, events);
        }

        //public static double[] NormalisePeriodicity(double[] periodicity, double minFramePeriod, double maxFramePeriod)
        //{
        //    double range = maxFramePeriod - minFramePeriod;
        //    for (int i = 0; i < periodicity.Length; i++)
        //    {
        //        //if (i > 100) 
        //        //    Console.WriteLine("{0}      {1}",  periodicity[i], ((periodicity[i] - minFramePeriod) / range));
        //        if (periodicity[i] <= 0.0) continue;
        //        periodicity[i] = (periodicity[i] - minFramePeriod) / range;
        //    }
        //    return periodicity;
        //}


        public static double[] CalculateGridScore(double[] dBArray, double[] peakPeriodicity)
        {
            int length = dBArray.Length;
            var gridScore = new double[length];
            int numberOfCycles = 4;

            for (int i = 0; i < peakPeriodicity.Length; i++)
            {
                if (peakPeriodicity[i] <= 0.0) continue;
                //calculate grid score

                int cyclePeriod = (int)peakPeriodicity[i];
                int minPeriod = cyclePeriod - 2;
                int maxPeriod = cyclePeriod + 2;
                double score = 0.0;
                int scoreLength = 0;
                for (int p = minPeriod; p <= maxPeriod; p++)
                {
                    int segmentLength = numberOfCycles * p;
                    double[] extract = DataTools.Subarray(dBArray, i, segmentLength);
                    if (extract == null) return gridScore; // reached end of array

                    double[] reducedSegment = Gratings.ReduceArray(extract, p, numberOfCycles);
                    double pScore = Gratings.DetectPeriod2Grating(reducedSegment);
                    if (pScore > score)
                    {
                        score = pScore;
                        scoreLength = segmentLength;
                    }
                }

                //transfer score to output array
                for (int x = 0; x < scoreLength; x++) 
                    if (gridScore[i + x] < score) gridScore[i + x] = score;
            }
            return gridScore;
        }


        //Delta score is number of frames over which there is a gradual increase in period.
        public static double[] CalculateDeltaPeriodScore(double[] periodicity, double framesPerSecond)
        {
            int maxSeconds = 20;
            double maxCount = maxSeconds * framesPerSecond; //use as a normalising factor
            double[] deltaScore = new double[periodicity.Length];
            int count = 0;
            int start = 0;
            double startPeriodicity = 0.0;
            for (int i = 1; i < periodicity.Length; i++)
            {
                if (periodicity[i] == 0.0)
                {
                    count = 0;
                    start = i;
                    startPeriodicity = 0.0;
                    continue;
                }
                count ++;
                //IF there is drop in period AND periodicity has increased THEN calculate a delta score
                if ((periodicity[i] < periodicity[i-1] ) && (periodicity[i - 1] > startPeriodicity))
                {
                        double score = count / maxCount;
                        if (score > 1.0) score = 1.0;
                        for (int j = start; j < i; j++)
                        {
                            deltaScore[j] = score;
                        }
                    count = 0;
                    start = i;
                    startPeriodicity = periodicity[i];
                }
            }
            return deltaScore;
        }

        public static double[] CalculateKiwiChirpScore(double[] dBArray, double[] peakPeriodicity, double[,] matrix)
        {
            int length = dBArray.Length;
            double[] chirpScore = new double[length];
            for (int i = 1; i < dBArray.Length - 1; i++)
            {
                if (peakPeriodicity[i] == 0.0) continue;
                //have a peak get spectra before and after
                double[] spectrumM1 = MatrixTools.GetRow(matrix, i - 1);
                double[] spectrumP1 = MatrixTools.GetRow(matrix, i + 1);
                spectrumM1 = DataTools.filterMovingAverage(spectrumM1, 5);
                spectrumP1 = DataTools.filterMovingAverage(spectrumP1, 5);
                double[] peakValuesM1 = DataTools.GetPeakValues(spectrumM1);
                double[] peakValuesP1 = DataTools.GetPeakValues(spectrumP1);
                int[] peakLocationsM1 = DataTools.GetOrderedPeakLocations(peakValuesM1, 2);
                int[] peakLocationsP1 = DataTools.GetOrderedPeakLocations(peakValuesP1, 2);
                double avLocationM1 = peakLocationsM1.Average();
                double avLocationP1 = peakLocationsP1.Average();
                double score = avLocationP1 - avLocationM1;
                double normalisingRange = 60.0;
                if (score < 0.0) chirpScore[i] = 0.0;
                else if (score > normalisingRange) chirpScore[i] = 1.0;
                else chirpScore[i] = score / normalisingRange;
            }
            return chirpScore;
        }


        public static double[] ConvertChirpsToScoreArray(double[] chirps, double[] dBArray, double framesPerSecond)
        {
            int length = dBArray.Length;
            double[] chirpScores = new double[length];
            int secondsSpan = 10;
            int span = (int)(secondsSpan * framesPerSecond);
            int step = (int)(5 * framesPerSecond);
            for (int r = 0; r < length - span; r++)
            {
                double score = 0.0;
                for (int i = 0; i < span; i++)
                {
                    score += chirps[r + i];
                }
                score /= (double)secondsSpan; // get a density per second
                if (score > 1.0) score = 1.0;

                for (int i = 0; i < span; i++)
                {
                    if (score > chirpScores[r + i]) chirpScores[r + i] = score;
                }
                r += step;
            }

            return chirpScores;
        }


        /// Checks acoustic activity that spills outside the kiwi bandwidth.
        /// use the periodicity array to cut down comoputaiton.
        /// Only look where we already know there is periodicity.
        public static double[] CalculateKiwiBandWidthScore(BaseSonogram sonogram, int minHz, int maxHz, double[] peakPeriodicity)
        {
            int frameCount = sonogram.FrameCount;
            double sonogramDuration = sonogram.FrameOffset * frameCount;
            var scores = new double[frameCount];
            int secondsSpan = 10;
            TimeSpan span = new TimeSpan(0, 0, secondsSpan);
            int frameSpan = (int)Math.Round(secondsSpan * sonogram.FramesPerSecond);
            for (int r = 1; r < frameCount - frameSpan; r++) 
            {
                if (peakPeriodicity[r] == 0.0) continue;
                TimeSpan start = new TimeSpan(0, 0, (int)(r / sonogram.FramesPerSecond));
                TimeSpan end = start + span;
                double score = CalculateKiwiBandWidthScore(sonogram, start, end, minHz, maxHz);
                for (int i = 0; i < frameSpan; i++)
                    if (scores[r + i] < score) scores[r + i] = score;
            }
            return scores;
        }
        /// <summary>
        /// Checks acoustic activity that spills outside the kiwi bandwidth.
        /// </summary>
        /// <returns></returns>
        public static double CalculateKiwiBandWidthScore(BaseSonogram sonogram, TimeSpan start, TimeSpan end, int minHz, int maxHz)
        {
            double[,] m = sonogram.Data;

            //set the time dimension
            int startFrame = (int)Math.Round(start.TotalSeconds * sonogram.FramesPerSecond);
            int endFrame   = (int)Math.Round(end.TotalSeconds   * sonogram.FramesPerSecond);
            //if (endFrame >= m.GetLength(1)) return 0.0;  //end of spectrum
            //int span = (int)Math.Round((end - start).TotalSeconds * sonogram.FramesPerSecond);
            int span = endFrame - startFrame + 1;

            //set the frequency dimension
            int minBin = (int)Math.Round(minHz / sonogram.FBinWidth);
            int maxBin = (int)Math.Round(maxHz / sonogram.FBinWidth);
            //int bandHt = (int)Math.Round((maxHz - minHz) / sonogram.FBinWidth);
            int bandHt = maxBin - minBin + 1;
            int halfHt = bandHt / 2;
            int hzBuffer = 150;
            int buffer = (int)Math.Round(hzBuffer / sonogram.FBinWidth); //avoid this margin around the main band

            //init the activity arrays
            double[] band_dB  = new double[span]; //dB profile for kiwi band
            double[] upper_dB = new double[span]; //dB profile for band above kiwi band
            double[] lower_dB = new double[span]; //dB profile for band below kiwi band

            //get acoustic activity within the kiwi bandwidth and above it.
            for (int r = 0; r < span; r++)
            {
                for (int c = 0; c < bandHt; c++) band_dB[r]  += m[startFrame + r, minBin + c]; //event dB profile
                for (int c = 0; c < halfHt; c++) upper_dB[r] += m[startFrame + r, maxBin + c + buffer];
                for (int c = 0; c < halfHt; c++) lower_dB[r] += m[startFrame + r, minBin - halfHt - buffer + c];
            }
            for (int r = 0; r < span; r++) band_dB[r]  /= bandHt; //calculate averagesS.
            for (int r = 0; r < span; r++) upper_dB[r] /= halfHt;
            for (int r = 0; r < span; r++) lower_dB[r] /= halfHt;

            double upperCC = DataTools.CorrelationCoefficient(band_dB, upper_dB);
            double lowerCC = DataTools.CorrelationCoefficient(band_dB, lower_dB);
            if (upperCC < 0.0) upperCC = 0.0;
            if (lowerCC < 0.0) lowerCC = 0.0;
            double CCscore = upperCC + lowerCC;
            if (CCscore > 1.0) CCscore = 1.0;
            return 1 - CCscore;
        }



        public static List<AcousticEvent> ConvertScoreArray2Events(
                                          double[] intensity, double[] gridScore, double[] deltaPeriodScore, double[] chirpScores, 
                                          double[] comboScore, double[] bwScore, 
                                          int minHz, int maxHz, double framesPerSec, double freqBinWidth,
                                          double scoreThreshold, double minDuration, double maxDuration)
        {
            int count = comboScore.Length;
            var events = new List<AcousticEvent>();
            //double maxPossibleScore = 5 * scoreThreshold; // used to calculate a normalised score bewteen 0 - 1.0 
            bool isHit = false;
            double frameOffset = 1 / framesPerSec; // frame offset in fractions of second
            double startTime = 0.0;
            int startFrame = 0;

            //for filtering acoustic events
            List<string[]> excludeRules = LSKiwi1.GetExcludeRules(defaultRules);

            // pass over all frames
            for (int i = 0; i < count; i++) 
            {
                if ((isHit == false) && (comboScore[i] >= scoreThreshold))//start of an event
                {
                    isHit = true;
                    startTime = i * frameOffset;
                    startFrame = i;
                }
                else  // check for the end of an event
                    if ((isHit == true) && (comboScore[i] <= scoreThreshold)) // this is end of an event, so initialise it
                    {
                        isHit = false;

                        double endTime = i * frameOffset;
                        double duration = endTime - startTime;
                        if ((duration < minDuration) || (duration > maxDuration)) continue; //skip events with duration outside defined limits
                        AcousticEvent ev = new AcousticEvent(startTime, duration, minHz, maxHz);
                        ev.SetTimeAndFreqScales(framesPerSec, freqBinWidth); //need time scale for later cropping of events

                        //
                        ev.kiwi_intensityScore   = CalculateAverageEventScore(ev, intensity);
                        ev.kiwi_gridScore        = CalculateAverageEventScore(ev, gridScore);
                        ev.kiwi_deltaPeriodScore = CalculateAverageEventScore(ev, deltaPeriodScore);
                        ev.kiwi_chirpScore       = CalculateAverageEventScore(ev, chirpScores);
                        ev.Score                 = CalculateAverageEventScore(ev, comboScore);
                        ev.ScoreNormalised       = ev.Score;  //assume score already nomalised
                        if (ev.ScoreNormalised > 1.0) ev.ScoreNormalised = 1.0;

                        //int frameCount = i - startFrame + 1;

                        // obtain an average score for the duration of the event.
                        //double av = 0.0;
                        //for (int n = startFrame; n <= i; n++) av += comboScore[n];

                        //find max score and its time - also calculate bandwidth score
                        //double bandwidthScore = 0.0;
                        //double maxComboSocre = -double.MaxValue;
                        //for (int n = startFrame; n <= i; n++)
                        //{
                        //    if (comboScore[n] > maxComboSocre)
                        //    {
                        //        maxComboSocre = comboScore[n];
                        //        ev.Score_MaxInEvent = maxComboSocre;
                        //        ev.Score_TimeOfMaxInEvent = n * frameOffset;
                        //    }
                        //    bandwidthScore += bwScore[n];
                        //}
                        //bandwidthScore /= frameCount;

                        ev.kiwi_bandWidthScore = CalculateAverageEventScore(ev, bwScore);

                        ev = FilterEvent(ev, excludeRules);
                        events.Add(ev);
                    }
            } //end of pass over all frames
            return events;
        }//end method ConvertScoreArray2Events()


        public static double CalculateAverageEventScore(AcousticEvent ae, double[] scoreArray)
        {
            int start  = ae.oblong.r1;
            int end    = ae.oblong.r2;
            if (end > scoreArray.Length) end = scoreArray.Length - 1;
            int length = end - start + 1;
            double sum = 0.0;
            for (int i = start; i <= end; i++) sum += scoreArray[i];
            return sum / (double)length;
        }

        public static AcousticEvent FilterEvent(AcousticEvent ae, List<string[]> rules)
        {
            //discount the normalised score by the bandwidth score.
            ae.ScoreNormalised *= ae.kiwi_bandWidthScore;

            //loop through exclusion rules - DO NOT DELETE events - set score to zero so can check later what is happening.
            foreach (string[] rule in rules)
            {
                string feature = rule[0];
                string op = rule[1];
                double value = Double.Parse(rule[2]);
                if ((feature == LSKiwi2.key_BANDWIDTH_SCORE) && (op == "LT") && (ae.kiwi_bandWidthScore < value))
                {
                    ae.kiwi_bandWidthScore = 0.0;
                    ae.ScoreNormalised     = 0.0;
                    return ae;
                }
                else
                if ((feature == LSKiwi2.key_BANDWIDTH_SCORE) && (op == "GT") && (ae.kiwi_bandWidthScore > value))
                {
                    ae.kiwi_bandWidthScore = 0.0;
                    ae.ScoreNormalised = 0.0;
                    return ae;
                }
                    //else
                    //    if ((feature == LSKiwi2.key_INTENSITY_SCORE) && (op == "LT") && (ae.kiwi_INTENSITY_SCORE < value)) return null;
                    //    else
                    //        if ((feature == LSKiwi2.key_INTENSITY_SCORE) && (op == "GT") && (ae.kiwi_INTENSITY_SCORE > value)) return null;
            }
            return ae;
        }


        public static void CropEvents(List<AcousticEvent> events, double[] activity, double minDurationInSeconds)
        {
            double croppingSeverity = 0.2;
            int length = activity.Length;

            foreach (AcousticEvent ev in events)
            {
                int start = ev.oblong.r1;
                int end = ev.oblong.r2;
                double[] subArray = DataTools.Subarray(activity, start, end - start + 1);
                int[] bounds = DataTools.Peaks_CropLowAmplitude(subArray, croppingSeverity);

                int newMinRow = start + bounds[0];
                int newMaxRow = start + bounds[1];
                if (newMaxRow >= length) newMaxRow = length - 1;

                ev.oblong = null;
                ev.TimeStart = newMinRow * ev.FrameOffset;
                ev.TimeEnd = newMaxRow * ev.FrameOffset;
                ev.Duration = ev.TimeEnd - ev.TimeStart;
                //int frameCount = (int)Math.Round(ev.Duration / ev.FrameOffset); 
            }
            for (int i = events.Count - 1; i >= 0; i--) if (events[i].Duration < minDurationInSeconds) events.Remove(events[i]);
        }



        static Image DrawSonogram(BaseSonogram sonogram, double[,] hits, List<Plot> scores, List<AcousticEvent> predictedEvents, double eventThreshold)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            int maxFreq = sonogram.NyquistFrequency / 2;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(maxFreq, 1, doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            if (scores != null)
            {
                foreach(Plot plot in scores) 
                    image.AddTrack(Image_Track.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
            }
            if (hits != null) image.OverlayRainbowTransparency(hits);
            if (predictedEvents.Count > 0) image.AddEvents(predictedEvents, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            return image.GetImage();
        } //DrawSonogram()




        public static DataTable WriteEvents2DataTable(List<AcousticEvent> predictedEvents)
        {
            if (predictedEvents == null) return null;
            string[] headers = { AudioAnalysisTools.Keys.EVENT_COUNT,     //1
                                 AudioAnalysisTools.Keys.EVENT_START_MIN, //2
                                 AudioAnalysisTools.Keys.EVENT_START_SEC, //3
                                 AudioAnalysisTools.Keys.EVENT_START_ABS, //4
                                 AudioAnalysisTools.Keys.SEGMENT_TIMESPAN,//5
                                 AudioAnalysisTools.Keys.EVENT_NAME,      //6
                                 AudioAnalysisTools.Keys.EVENT_DURATION,  //7
                                 AudioAnalysisTools.Keys.EVENT_INTENSITY, //8
                                 key_GRID_SCORE,                          //9   
                                 key_DELTA_SCORE,                         //10
                                 key_CHIRP_SCORE,                         //11  
                                 key_BANDWIDTH_SCORE,                     //12
                                 AudioAnalysisTools.Keys.EVENT_SCORE,     //13
                                 AudioAnalysisTools.Keys.EVENT_NORMSCORE  //14 
                               };
            //                   1                2               3              4                5              6               7              8
            Type[] types = { typeof(int), typeof(double), typeof(double), typeof(double), typeof(double), typeof(string), typeof(double), typeof(double), 
            //                   9                10              11               12              13             14
                             typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double) };

            var dataTable = DataTableTools.CreateTable(headers, types);
            if (predictedEvents.Count == 0) return dataTable;

            foreach (var ev in predictedEvents)
            {
                DataRow row = dataTable.NewRow();
                row[AudioAnalysisTools.Keys.EVENT_START_ABS] = (double)ev.TimeStart;  //Set now - will overwrite later
                row[AudioAnalysisTools.Keys.EVENT_START_SEC] = (double)ev.TimeStart;  //EvStartSec
                row[AudioAnalysisTools.Keys.EVENT_DURATION]  = (double)ev.Duration;   //duratio in seconds
                row[AudioAnalysisTools.Keys.EVENT_NAME]      = (string)ev.Name;       //
                row[AudioAnalysisTools.Keys.EVENT_INTENSITY] = (double)ev.kiwi_intensityScore;   //
                row[key_BANDWIDTH_SCORE]                     = (double)ev.kiwi_bandWidthScore;  
                row[key_DELTA_SCORE]                         = (double)ev.kiwi_deltaPeriodScore;
                row[key_GRID_SCORE]                          = (double)ev.kiwi_gridScore;
                row[key_CHIRP_SCORE]                         = (double)ev.kiwi_chirpScore;
                row[AudioAnalysisTools.Keys.EVENT_SCORE]     = (double)ev.Score;      //Score
                row[AudioAnalysisTools.Keys.EVENT_NORMSCORE] = (double)ev.ScoreNormalised;
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

    } //end class LSKiwi3
}
