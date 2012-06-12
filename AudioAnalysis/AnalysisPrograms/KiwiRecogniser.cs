using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Text;

using TowseyLib;
using AudioAnalysisTools;
using Acoustics.Shared;
using Acoustics.Tools.Audio;




//Here is link to wiki page containing info about how to write Analysis techniques
//https://wiki.qut.edu.au/display/mquter/Audio+Analysis+Processing+Architecture

//HERE ARE COMMAND LINE ARGUMENTS TO PLACE IN START OPTIONS - PROPERTIES PAGE
//od  "C:\SensorNetworks\WavFiles\Canetoad\DM420010_128m_00s__130m_00s - Toads.mp3" C:\SensorNetworks\Output\OD_CaneToad\CaneToad_DetectionParams.txt events.txt
//


namespace AnalysisPrograms
{
    public class LSKiwi
    {
        //Following lines are used for the debug command line.
        // kiwi "C:\SensorNetworks\WavFiles\Kiwi\Samples\lsk-female\TOWER_20091107_07200_21.LSK.F.wav"  "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
        // kiwi "C:\SensorNetworks\WavFiles\Kiwi\Samples\lsk-male\TOWER_20091112_072000_25.LSK.M.wav"  "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
        // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_Cropped.wav"     "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
        // 8 min test recording  // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_CroppedAnd2.wav" "C:\SensorNetworks\WavFiles\Kiwi\Results_MixedTest\lskiwi_Params.txt"
        // kiwi "C:\SensorNetworks\WavFiles\Kiwi\KAPITI2_20100219_202900.wav"   "C:\SensorNetworks\WavFiles\Kiwi\Results\lskiwi_Params.txt"
        // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav"     "C:\SensorNetworks\WavFiles\Kiwi\Results_TOWER_20100208_204500\lskiwi_Params.txt"

        //public const string SOURCE_RECORDING_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav";
        //public const string WORKING_DIRECTORY     = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TOWER_20100208_204500\";
        //public const string CONFIG_PATH           = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TOWER_20100208_204500\lskiwi_Params.txt";
        //public const string ANDREWS_SELECTION_PATH= @"C:\SensorNetworks\WavFiles\Kiwi\Results_TOWER_20100208_204500\TOWER_20100208_204500_ANDREWS_SELECTIONS.csv";

        //public const string SOURCE_RECORDING_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\KAPITI2_20100219_202900.wav";
        //public const string WORKING_DIRECTORY = @"C:\SensorNetworks\WavFiles\Kiwi\Results_KAPITI2_20100219_202900\";
        //public const string CONFIG_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_KAPITI2_20100219_202900\lskiwi_Params.txt";
        //public const string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_KAPITI2_20100219_202900\KAPITI2_20100219_202900_ANDREWS_SELECTIONS.csv";

        //public const string SOURCE_RECORDING_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav";
        //public const string WORKING_DIRECTORY = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_220004\";
        //public const string CONFIG_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_220004\lskiwi_Params.txt";
        //public const string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_220004\TUITCE_20091215_220004_ANDREWS_SELECTIONS.csv";

        public const string SOURCE_RECORDING_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_210000.wav";
        public const string WORKING_DIRECTORY = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_210000\";
        public const string CONFIG_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_210000\lskiwi_Params.txt";
        public const string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_210000\TUITCE_20091215_210000_ANDREWS_SELECTIONS.csv";


        public const string ANALYSIS_NAME = "LSKiwi";
        public const double DEFAULT_activityThreshold_dB = 3.0; //used to select frames that have 3dB > background
        public const int DEFAULT_WINDOW_SIZE = 256;

        private const int COL_NUMBER = 15;
        private static Type[] COL_TYPES       = new Type[COL_NUMBER];
        private static string[] HEADERS       = new string[COL_NUMBER]; 
        private static bool[] DISPLAY_COLUMN  = new bool[COL_NUMBER];
        private static double[] COMBO_WEIGHTS = new double[COL_NUMBER]; 

        public static System.Tuple<string[],Type[], bool[]> InitOutputTableColumns()
        {
            HEADERS[0] = "count";           COL_TYPES[0] = typeof(int);     DISPLAY_COLUMN[0] = false;
            HEADERS[1] = "EvStartAbs";      COL_TYPES[1] = typeof(int);     DISPLAY_COLUMN[1] = false;
            HEADERS[2] = "EvStartMin";      COL_TYPES[2] = typeof(int);     DISPLAY_COLUMN[2] = false;
            HEADERS[3] = "EvStartSec";      COL_TYPES[3] = typeof(double);  DISPLAY_COLUMN[3] = false;
            HEADERS[4] = "SegmentDur";      COL_TYPES[4] = typeof(string);  DISPLAY_COLUMN[4] = false;
            HEADERS[5] = "Density";         COL_TYPES[5] = typeof(int);     DISPLAY_COLUMN[5] = true;
            HEADERS[6] = "Label";           COL_TYPES[6] = typeof(string);  DISPLAY_COLUMN[6] = false;
            HEADERS[7] = "EventDur";        COL_TYPES[7] = typeof(double);  DISPLAY_COLUMN[7] = false;
            HEADERS[8] = "DurScore";        COL_TYPES[8] = typeof(double);  DISPLAY_COLUMN[8] = true;
            HEADERS[9] = "HitSCore";        COL_TYPES[9] = typeof(double);  DISPLAY_COLUMN[9] = true;
            HEADERS[10] = "SnrScore";       COL_TYPES[10] = typeof(double); DISPLAY_COLUMN[10] = true;
            HEADERS[11] = "sdScore";        COL_TYPES[11] = typeof(double); DISPLAY_COLUMN[11] = true;
            HEADERS[12] = "GapScore";       COL_TYPES[12] = typeof(double); DISPLAY_COLUMN[12] = true;
            HEADERS[13] = "BWScore";        COL_TYPES[13] = typeof(double); DISPLAY_COLUMN[13] = true;
            HEADERS[14] = "KiwiScore";      COL_TYPES[14] = typeof(double); DISPLAY_COLUMN[14] = true;
            return Tuple.Create(HEADERS, COL_TYPES, DISPLAY_COLUMN);
        }


        public static bool[] LSKiwiColumns2Display()
        {
            bool[] LSKiwiColumns2Display = { false, false, true, true, true };
            return LSKiwiColumns2Display;

        }



        //Keys to recognise identifiers in PARAMETERS - INI file. 
        //public static string key_FILE_EXT        = "FILE_EXT";
        //public static string key_DO_SEGMENTATION = "DO_SEGMENTATION";
        public static string key_SEGMENT_DURATION  = "SEGMENT_DURATION";
        public static string key_SEGMENT_OVERLAP   = "SEGMENT_OVERLAP";
        
        public static string key_FRAME_LENGTH    = "FRAME_LENGTH";
        public static string key_FRAME_OVERLAP   = "FRAME_OVERLAP";
        public static string key_MIN_HZ_MALE     = "MIN_HZ_MALE";
        public static string key_MAX_HZ_MALE     = "MAX_HZ_MALE";
        public static string key_MIN_HZ_FEMALE   = "MIN_HZ_FEMALE";
        public static string key_MAX_HZ_FEMALE   = "MAX_HZ_FEMALE";
        public static string key_DCT_DURATION    = "DCT_DURATION";
        public static string key_DCT_THRESHOLD   = "DCT_THRESHOLD";
        public static string key_MIN_PERIODICITY = "MIN_PERIOD";
        public static string key_MAX_PERIODICITY = "MAX_PERIOD";
        public static string key_MIN_DURATION    = "MIN_DURATION";
        public static string key_MAX_DURATION    = "MAX_DURATION";
        public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_DRAW_SONOGRAMS  = "DRAW_SONOGRAMS";
        public static string key_REPORT_FORMAT   = "REPORT_FORMAT";



        public static void Dev()
        {
            string title = "# SOFTWARE TO DETECT CALLS OF THE LITTLE SPOTTED KIWI (Apteryx owenii)";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            //GET COMMAND LINE ARGUMENTS
            Log.Verbosity = 1;
            string sourceRecordingPath = SOURCE_RECORDING_PATH;
            string iniPath             = CONFIG_PATH;
            string outputDir           = WORKING_DIRECTORY;
            DirectoryInfo diOutputDir  = new DirectoryInfo(outputDir);
            string tempSegmentPath     = Path.Combine(outputDir, "temp.wav"); //path location/name of extracted recording segment

            var fiSourceRecording = new FileInfo(sourceRecordingPath);
            var fiTempSegmentFile = new FileInfo(tempSegmentPath);

            Log.WriteIfVerbose("# Output dir: " + outputDir);

            string myResultsPath = outputDir + "LSKCallScores_" + Path.GetFileNameWithoutExtension(sourceRecordingPath) + ".csv";
            string reportROCPath = outputDir + "LSKRoc_Report_" + Path.GetFileNameWithoutExtension(sourceRecordingPath) + ".csv";

            InitOutputTableColumns();

            // method to calculate ROC curve results
            if (false)
            {
                var fiTheTruth = new FileInfo(ANDREWS_SELECTION_PATH);
                var fiMyResults = new FileInfo(myResultsPath);
                DataTable dt = CalculateRecallPrecision(fiMyResults, fiTheTruth);
                CsvTools.DataTable2CSV(dt, reportROCPath);
                Console.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(999);
            }

            //READ PARAMETER VALUES FROM INI FILE
            //KiwiParams kiwiParams = ReadIniFile(iniPath);
            //WriteParameters(kiwiParams);
            Dictionary<string, string> kiwiCfg = ConfigDictionary.ReadPropertiesFile(iniPath);
            double segmentDuration = ConfigDictionary.GetDouble(LSKiwi.key_SEGMENT_DURATION, kiwiCfg);
            double segmentOverlap = ConfigDictionary.GetDouble(LSKiwi.key_SEGMENT_OVERLAP, kiwiCfg); 

            // Get the file time duration
            IAudioUtility audioUtility = new MasterAudioUtility();
            var mimeType = MediaTypes.GetMediaType(fiSourceRecording.Extension);
            var duration = audioUtility.Duration(fiSourceRecording, mimeType);

            var sourceAudioDuration = audioUtility.Duration(fiSourceRecording, mimeType);
            int segmentCount = (int)Math.Round(sourceAudioDuration.TotalMinutes / segmentDuration); //convert length to minute chunks
            //int segmentDuration_ms = (int)(segmentDuration_mins * 60000) + (segmentOverlap * 1000);



            Log.WriteIfVerbose("# Recording - filename: " + Path.GetFileName(sourceRecordingPath));
            Log.WriteIfVerbose("# Recording - datetime: {0}    {1}", fiSourceRecording.CreationTime.ToLongDateString(), fiSourceRecording.CreationTime.ToLongTimeString());
            Log.WriteIfVerbose("# Recording - duration: {0}hr:{1}min:{2}s:{3}ms", duration.Hours, duration.Minutes, duration.Seconds, duration.Milliseconds);

            //SET UP THE REPORT DATATABLE
            var dataTable = DataTableTools.CreateTable(HEADERS, COL_TYPES);

            // LOOP THROUGH THE FILE
            int resampleRate = 17640;
            double startMinutes = 0.0;
            int overlap_ms = (int)Math.Floor(segmentOverlap * 1000);


            //segmentCount = 2; //FOR DEBUGGING
            // Parallelize the loop to partition the source file by segments.
            //Parallel.For(0, segmentCount, s =>
            for (int s = 0; s < segmentCount; s++)
            {
                Console.WriteLine();
                Log.WriteLine("## SAMPLE {0}:-   starts@ {1} minutes", s, startMinutes);

                int startMilliseconds = (int)(startMinutes * 60000);
                int endMilliseconds = startMilliseconds + (int)(segmentDuration * 60000) + overlap_ms;
                MasterAudioUtility.SegmentToWav(resampleRate, fiSourceRecording, new FileInfo(tempSegmentPath), startMilliseconds, endMilliseconds);
                AudioRecording recordingSegment = new AudioRecording(tempSegmentPath);
                FileInfo fiSegmentAudioFile = new FileInfo(recordingSegment.FilePath);
                TimeSpan ts = recordingSegment.Duration();

                if (ts.TotalSeconds <= 30)
                {
                    Log.WriteLine("# WARNING: Recording is less than {0} seconds long. Will ignore.", 30);
                    break;
                }
                else //do analysis
                {
                    //#############################################################################################################################################
                    DataTable results = LSKiwi.Analysis(s, fiSegmentAudioFile, kiwiCfg, diOutputDir);
                    //#############################################################################################################################################

                    //transfer acoustic event info to data table
                    Log.WriteLine("# Event count for minute {0} = {1}", startMinutes, results.Rows.Count);
                    if (results != null) 
                    {
                        string sortString = "EvStartAbs ASC";   //SORT EVENTS BY THEIR START TIME
                        DataRow[] rows = DataTableTools.SortRows(results, sortString);
                        foreach (DataRow row in rows) dataTable.ImportRow(row);
                    }
                }

                recordingSegment.Dispose();
                startMinutes += segmentDuration;
            } //end of for loop
            //); // Parallel.For()

            CsvTools.DataTable2CSV(dataTable, myResultsPath);

            // Calculate and save ROC curve results
            if (true)
            {
                var fiMyResults = new FileInfo(myResultsPath);
                var fiTheTruth = new FileInfo(ANDREWS_SELECTION_PATH);
                DataTable dt = CalculateRecallPrecision(fiMyResults, fiTheTruth);
                CsvTools.DataTable2CSV(dt, reportROCPath);
            }


            Log.WriteLine("# Finished recording:- " + Path.GetFileName(sourceRecordingPath));
            Log.WriteLine("# Output CSV at:-      " + myResultsPath);
            Console.ReadLine();
        } //Dev()



        /// <summary>
        /// A WRAPPER AROUND THE Execute_KiwiDetect() method
        /// Returns a DataTable
        /// The Execute_KiwiDetect() method returns a System.Tuple<BaseSonogram, Double[,], double[], double[], List<AcousticEvent>>
        /// </summary>
        /// <param name="iter"></param>
        /// <param name="config"></param>
        /// <param name="segmentAudioFile"></param>
        public static DataTable Analysis(int iter, FileInfo fiSegmentAudioFile, Dictionary<string, string> config, DirectoryInfo diOutputDir)
        {
            int minHzMale = ConfigDictionary.GetInt(LSKiwi.key_MIN_HZ_MALE, config);
            int maxHzMale = ConfigDictionary.GetInt(LSKiwi.key_MAX_HZ_MALE, config);
            int minHzFemale = ConfigDictionary.GetInt(LSKiwi.key_MIN_HZ_FEMALE, config);
            int maxHzFemale = ConfigDictionary.GetInt(LSKiwi.key_MAX_HZ_FEMALE, config);
            int frameLength = ConfigDictionary.GetInt(LSKiwi.key_FRAME_LENGTH, config);
            double frameOverlap = ConfigDictionary.GetDouble(LSKiwi.key_FRAME_OVERLAP, config);
            double dctDuration = ConfigDictionary.GetDouble(LSKiwi.key_DCT_DURATION, config);
            double dctThreshold = ConfigDictionary.GetDouble(LSKiwi.key_DCT_THRESHOLD, config);
            double minPeriod = ConfigDictionary.GetDouble(LSKiwi.key_MIN_PERIODICITY, config);
            double maxPeriod = ConfigDictionary.GetDouble(LSKiwi.key_MAX_PERIODICITY, config);
            double eventThreshold = ConfigDictionary.GetDouble(LSKiwi.key_EVENT_THRESHOLD, config);
            double minDuration = ConfigDictionary.GetDouble(LSKiwi.key_MIN_DURATION, config); //minimum event duration to qualify as species call
            double maxDuration = ConfigDictionary.GetDouble(LSKiwi.key_MAX_DURATION, config); //maximum event duration to qualify as species call
            double drawSonograms = ConfigDictionary.GetInt(LSKiwi.key_DRAW_SONOGRAMS, config);
            double segmentDuration = ConfigDictionary.GetDouble(LSKiwi.key_SEGMENT_DURATION, config); 
            double segmentStartMinute = segmentDuration * iter;

            AudioRecording recordingSegment = new AudioRecording(fiSegmentAudioFile.FullName);

            var results = LSKiwi.Execute_KiwiDetect(recordingSegment, minHzMale, maxHzMale, minHzFemale, maxHzFemale, frameLength, frameOverlap, dctDuration, dctThreshold,
                                                            minPeriod, maxPeriod, eventThreshold, minDuration, maxDuration);


            var sonogram = results.Item1;
            var hits = results.Item2;
            var scores = results.Item3;
            //var oscRates = results.Item4;
            var predictedEvents = results.Item5;

            //draw images of sonograms
            bool saveSonogram = false;
            if ((drawSonograms == 2) || ((drawSonograms == 1) && (predictedEvents.Count > 0))) saveSonogram = true;
            if (saveSonogram)
            {
                string imagePath = Path.Combine(diOutputDir.FullName, Path.GetFileNameWithoutExtension(fiSegmentAudioFile.FullName) +"_"+ (int)segmentStartMinute+ "min.png");
                Image image = DrawSonogram(sonogram, hits, scores, predictedEvents, eventThreshold);
                image.Save(imagePath, ImageFormat.Png); 
            }

            //write events to a data table to return.
            TimeSpan tsSegmentDuration = recordingSegment.Duration();
            DataTable dataTable = WriteEvents2DataTable(iter, segmentStartMinute, tsSegmentDuration, predictedEvents);
            //Log.WriteLine("# Event count for minute {0} = {1}", startMinutes, dataTable.Rows.Count);

            string sortString = "EvStartAbs ASC";
            return DataTableTools.SortTable(dataTable, sortString); //sort by start time before returning
        } //Analysis()





        /// <summary>
        /// A WRAPPER AROUND THE Execute_KiwiDetect() method
        /// Returns a DataTable
        /// The Execute_KiwiDetect() method returns a System.Tuple<BaseSonogram, Double[,], double[], double[], List<AcousticEvent>>
        /// </summary>
        /// <param name="iter"></param>
        /// <param name="config"></param>
        /// <param name="segmentAudioFile"></param>
        public static Image GetImageFromAudioSegment(FileInfo fiSegmentAudioFile, Dictionary<string, string> config)
        {
            if (config == null) return null;

            int minHzMale = ConfigDictionary.GetInt(LSKiwi.key_MIN_HZ_MALE, config);
            int maxHzMale = ConfigDictionary.GetInt(LSKiwi.key_MAX_HZ_MALE, config);
            int minHzFemale = ConfigDictionary.GetInt(LSKiwi.key_MIN_HZ_FEMALE, config);
            int maxHzFemale = ConfigDictionary.GetInt(LSKiwi.key_MAX_HZ_FEMALE, config);
            int frameLength = ConfigDictionary.GetInt(LSKiwi.key_FRAME_LENGTH, config);
            double frameOverlap = ConfigDictionary.GetDouble(LSKiwi.key_FRAME_OVERLAP, config);
            double dctDuration = ConfigDictionary.GetDouble(LSKiwi.key_DCT_DURATION, config);
            double dctThreshold = ConfigDictionary.GetDouble(LSKiwi.key_DCT_THRESHOLD, config);
            double minPeriod = ConfigDictionary.GetDouble(LSKiwi.key_MIN_PERIODICITY, config);
            double maxPeriod = ConfigDictionary.GetDouble(LSKiwi.key_MAX_PERIODICITY, config);
            double eventThreshold = ConfigDictionary.GetDouble(LSKiwi.key_EVENT_THRESHOLD, config);
            double minDuration = ConfigDictionary.GetDouble(LSKiwi.key_MIN_DURATION, config); //minimum event duration to qualify as species call
            double maxDuration = ConfigDictionary.GetDouble(LSKiwi.key_MAX_DURATION, config); //maximum event duration to qualify as species call

            AudioRecording recordingSegment = new AudioRecording(fiSegmentAudioFile.FullName);

            var results = LSKiwi.Execute_KiwiDetect(recordingSegment, minHzMale, maxHzMale, minHzFemale, maxHzFemale, frameLength, frameOverlap, dctDuration, dctThreshold,
                                                            minPeriod, maxPeriod, eventThreshold, minDuration, maxDuration);
            var sonogram = results.Item1;
            var hits = results.Item2;
            var scores = results.Item3;
            //var oscRates = results.Item4;
            var predictedEvents = results.Item5;

            // (iii) NOISE REDUCTION
            bool doNoiseReduction = false;
            doNoiseReduction = ConfigDictionary.GetBoolean(AcousticFeatures.key_DO_NOISE_REDUCTION, config);
            double sonogramBackgroundThreshold = 4.0;
            sonogramBackgroundThreshold = ConfigDictionary.GetDouble(AcousticFeatures.key_BG_NOISE_REDUCTION, config);
            if (doNoiseReduction)
            {
                var tuple = SNR.NoiseReduce(sonogram.Data, NoiseReductionType.STANDARD, sonogramBackgroundThreshold);
                sonogram.Data = tuple.Item1;   // store data matrix
            }

            Image image = DrawSonogram(sonogram, hits, scores, predictedEvents, eventThreshold);
            return image;
        } //GetImageFromAudioSegment()






        public static System.Tuple<BaseSonogram, Double[,], double[], double[], List<AcousticEvent>> Execute_KiwiDetect(AudioRecording recording,
            int minHzMale, int maxHzMale, int minHzFemale, int maxHzFemale, int frameLength, double frameOverlap, double dctDuration, double dctThreshold,
            double minPeriodicity, double maxPeriodicity, double eventThreshold, double minDuration, double maxDuration)
        {

            //i: MAKE SONOGRAM
            //Log.WriteLine("Make sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName    = recording.FileName;
            sonoConfig.WindowSize     = frameLength;
            sonoConfig.WindowOverlap  = frameOverlap;
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            //DO NOT DO NOISE REMOVAL BECAUSE CAN LOSE SOME KIWI INFO

            //iii: DETECT OSCILLATIONS
            bool normaliseDCT = true;
            double minOscilFreq = 1 / maxPeriodicity;  //convert max period (seconds) to oscilation rate (Herz).
            double maxOscilFreq = 1 / minPeriodicity;  //convert min period (seconds) to oscilation rate (Herz).

            //ii: CHECK FOR MALE KIWIS
            List<AcousticEvent> predictedMaleEvents;  //predefinition of results event list
            Double[,] maleHits;                       //predefinition of hits matrix - to superimpose on sonogram image
            double[] maleScores;                      //predefinition of score array
            double[] maleOscRate;
            OscillationAnalysis.Execute((SpectralSonogram)sonogram, minHzMale, maxHzMale, dctDuration, dctThreshold, normaliseDCT,
                                         minOscilFreq, maxOscilFreq, eventThreshold, minDuration, maxDuration,
                                         out maleScores, out predictedMaleEvents, out maleHits, out maleOscRate);
            if (predictedMaleEvents.Count > 0)
            {
                ProcessKiwiEvents(predictedMaleEvents, "Male LSK", maleOscRate, minDuration, maxDuration, sonogram.Data);
                //int gapThreshold = 2;                   //merge events that are closer than 2 seconds
                //AcousticEvent.MergeAdjacentEvents(predictedMaleEvents, gapThreshold);
            }

            //iii: CHECK FOR FEMALE KIWIS
            Double[,] femaleHits;                       //predefinition of hits matrix - to superimpose on sonogram image
            double[] femaleScores;                      //predefinition of score array
            double[] femaleOscRate;
            List<AcousticEvent> predictedFemaleEvents;  //predefinition of results event list
            OscillationAnalysis.Execute((SpectralSonogram)sonogram, minHzFemale, maxHzFemale, dctDuration, dctThreshold, normaliseDCT,
                                         minOscilFreq, maxOscilFreq, eventThreshold, minDuration, maxDuration,
                                         out femaleScores, out predictedFemaleEvents, out femaleHits, out femaleOscRate);
            if (predictedFemaleEvents.Count > 0)
            {
                ProcessKiwiEvents(predictedFemaleEvents, "Female LSK", femaleOscRate, minDuration, maxDuration, sonogram.Data);
                //int gapThreshold = 2;                   //merge events that are closer than 2 seconds
                //AcousticEvent.MergeAdjacentEvents(predictedFemaleEvents, gapThreshold);
            }


            //iv: MERGE MALE AND FEMALE INFO
            foreach (AcousticEvent ae in predictedFemaleEvents) predictedMaleEvents.Add(ae);
            // Merge the male and female hit matrices. Each hit matrix shows where there is an oscillation of sufficient amplitude in the correct range.
            // Values in the matrix are the oscillation rate. i.e. if OR = 2.0 = 2 oscillations per second. </param>
            Double[,] hits = DataTools.AddMatrices(maleHits, femaleHits);
            //normalise the hit values
            double delta = 2.5; //range of the oscilation rate i.e. 1-3 oscillations per second
            for (int i = 0; i < hits.GetLength(0); i++)
            {
                for (int j = 0; j < hits.GetLength(1); j++)
                {
                    hits[i, j] /= delta;
                }
            }
            //merge the two score arrays
            for (int i = 0; i < maleScores.Length; i++) if (femaleScores[i] > maleScores[i]) maleScores[i] = femaleScores[i];


            return System.Tuple.Create(sonogram, hits, maleScores, maleOscRate, predictedMaleEvents);
        } //end Execute_KiwiDetect()


        public static void ProcessKiwiEvents(List<AcousticEvent> events, string tag, double[] oscRate, double minDuration, double maxDuration, double[,] sonogramMatrix)
        {
            double[] band_dB = new double[sonogramMatrix.GetLength(0)];
            int eventCol1   = events[0].oblong.c1; 
            int eventHeight = events[0].oblong.ColWidth; //assume all events in the list have the same event height
            for (int r = 0; r < band_dB.Length; r++)
            {
                for (int c = 0; c < eventHeight; c++) band_dB[r] += sonogramMatrix[r, eventCol1 + c]; //event dB profile
            }
            for (int r = 0; r < band_dB.Length; r++) band_dB[r] /= eventHeight; //calculate average.
            
            //calculate background noise and SD for the band
            double minDecibels = -110; //min dB to consider
            double noiseThreshold_dB = 20;
            double min_dB, max_dB, modalNoise, SD_Noise;
            SNR.CalculateModalNoise(band_dB, minDecibels, noiseThreshold_dB, out min_dB, out max_dB, out modalNoise, out SD_Noise);

            foreach (AcousticEvent ae in events)
            {
                ae.Name = tag;
                int eventLength = (int)(ae.Duration * ae.FramesPerSecond);
                //int objHt = ae.oblong.RowWidth;
                //Console.WriteLine("{0}    {1} = {2}-{3}", eventLength, objHt, ae.oblong.r2, ae.oblong.r1);

                //1: calculate score for duration. Value lies in [0,1]. Shape the ends.
                double durationScore = 1.0;
                if (ae.Duration < minDuration +  5) durationScore = (ae.Duration - minDuration) / 5;
                else
                if (ae.Duration > maxDuration - 10) durationScore = (maxDuration - ae.Duration) / 10;

                //2:  %hit score = ae.
                double hitScore = ae.Score;//fraction of bins where oscillation was detected

                //3: calculate score for bandwidth of syllables
                double bandWidthScore = CalculateKiwiBandWidthScore(ae, sonogramMatrix);

                //4: get decibels through the event   
                double[] event_dB = new double[eventLength]; //dB profile for event
                for (int r = 0; r < eventLength; r++) event_dB[r] = band_dB[ae.oblong.r1 + r]; //event dB profile
                //DataTools.writeBarGraph(event_dB);

                //5 : calculate score for snr and for change in inter-syllable distance over the event.
                var tuple = KiwiPeakAnalysis(event_dB, modalNoise, SD_Noise, ae.FramesPerSecond);
                double snrScore      = tuple.Item1;
                double sdPeakScore   = tuple.Item2;
                double gapScore      = tuple.Item3;
 
                //6: DERIVE A WEIGHTED COMBINATION SCORE
                double comboScore = /*(hitScore * 0.00) +*/ (snrScore * 0.1) + (sdPeakScore * 0.1) + (gapScore * 0.3) + (bandWidthScore * 0.5); //weighted sum

                //7 add score values to acoustic event
                ae.kiwi_durationScore  = durationScore;
                ae.kiwi_hitScore       = hitScore;
                ae.kiwi_snrScore       = snrScore;
                ae.kiwi_sdPeakScore    = sdPeakScore;
                ae.kiwi_intensityScore       = gapScore;
                ae.kiwi_bandWidthScore = bandWidthScore;
                ae.Score               = comboScore;
                ae.ScoreNormalised     = comboScore;
            } //foreach (AcousticEvent ae in events)
        }//end method



        /// <summary>
        ///
        /// </summary>
        /// <param name="ae">an acoustic event</param>
        /// <param name="dbArray">The sequence of frame dB over the event</param>
        /// <returns></returns>
        public static System.Tuple<double, double, double> KiwiPeakAnalysis(double[] dbArray, double modalNoise, double SD_Noise, double framesPerSecond)
        {
            //1: SMOOTH the array.  Smoothing window of 1/4 second because shortest kiwi periodicity = 0.25 seconds
            int smoothWindow = (int)Math.Ceiling(framesPerSecond * 0.25);  
            if (smoothWindow % 2 == 0) smoothWindow += 1; //make an odd number
            dbArray = DataTools.filterMovingAverage(dbArray, smoothWindow);

            bool[] peaks   = DataTools.GetPeaks(dbArray);

            //remove peaks below the threshold AND make list of peaks > threshold
            double dBThreshold = modalNoise + (2.0 * SD_Noise); //threshold = 2* sd of the background noise in the entire recording
            List<double> peakDB = new List<double>();
            for (int i = 0; i < dbArray.Length; i++)
            {
                if (dbArray[i] < dBThreshold) peaks[i] = false;
                else if (peaks[i]) peakDB.Add(dbArray[i]);
            }
            int peakCount = DataTools.CountTrues(peaks);
            if (peakCount == 0)
                return System.Tuple.Create(0.0, 0.0, 0.0); //no values can be calculated with zero-1 peaks

            //PROCESS PEAK DECIBELS
            double avPeakDB, sdPeakDB;
            NormalDist.AverageAndSD(peakDB.ToArray(), out avPeakDB, out sdPeakDB);
            double snr_dB = avPeakDB - modalNoise;
            //normalise the snr score
            double snrScore = 0.0;
            if (snr_dB > 9.0) snrScore = 1.0; else if (snr_dB > 1.0) snrScore = (snr_dB - 1) * 0.125; //two thresholds - 1 dB and 9 dB 
            //normalise the standard deviation of the peak decibels
            if (peakCount < 3)
                return System.Tuple.Create(snrScore, 0.0, 0.0); //no sd can be calculated with < 3 peaks
            double sdPeakScore = 2 / sdPeakDB; //set 2-3 dB as a threshold
            if (sdPeakScore > 1.0) sdPeakScore = 1.0;

            //PROCESS PEAK GAPS
            //if (peakCount < 3)
            //    return System.Tuple.Create(snrScore, sdPeakScore, 0.0); //no gaps can be calculated

            List<int> peakGaps = DataTools.GapLengths(peaks);
            int[] observedFrameGaps = peakGaps.ToArray();
            //double gapDelta = 0;
            //double gapPath = 0;
            double maxDelta_seconds = 0.5; //max permitted change in gap from one peak to next.
            int maxDelta = (int)Math.Ceiling(maxDelta_seconds / framesPerSecond);
            int correctDeltaCount = 0;
            int gapLength = observedFrameGaps[0];
            for (int i = 1; i < observedFrameGaps.Length; i++)
            {
                double gapDelta = (observedFrameGaps[i] - observedFrameGaps[i - 1]);
                if ((gapDelta >= 0) && (gapDelta <= maxDelta)) correctDeltaCount++;
                gapLength += observedFrameGaps[i];
                //gapDelta +=         (observedFrameGaps[i] - observedFrameGaps[i-1]);
                //gapPath  += Math.Abs(observedFrameGaps[i] - observedFrameGaps[i-1]);
            }
            //gapDelta /= (observedFrameGaps.Length - 1);    //convert to average
            //gapPath  /= (observedFrameGaps.Length - 1);    //convert to average
            //double gapScore = gapDelta / gapPath;
            //if (gapDelta <= 0.0) gapScore = 0.0; //gap delta for KIWIs must be positive
            double gapScore = 0.0;
            if (observedFrameGaps.Length != 0)
            {
                double avGapLength_seconds = (gapLength / (double)observedFrameGaps.Length) / framesPerSecond;
                gapScore = correctDeltaCount / (double)(observedFrameGaps.Length - 2);
                if ((avGapLength_seconds < 0.2) || (avGapLength_seconds > 1.2)) gapScore = 0.0;
                //else gapScore = (gapScore-0.5) * 2; 
            }

            //JUST IN CASE !!
            if (Double.IsNaN(gapScore))      { gapScore = 0.0; Console.WriteLine("gapScore is NaN"); }    //####### DEBUG TODO - TOO MANY gapScores are NaN
            if (Double.IsInfinity(gapScore)) { gapScore = 0.0; Console.WriteLine("gapScore is Infinity"); }
            if (Double.IsNaN(snrScore)) { snrScore = 0.0; Console.WriteLine("snrScore is NaN"); }
            if (Double.IsNaN(sdPeakScore)) { sdPeakScore = 0.0; Console.WriteLine("sdPeakScore is NaN"); }
            return System.Tuple.Create(snrScore, sdPeakScore, gapScore);
        }


        /// <summary>
        /// Checkes that the passed acoustic event does not have acoustic activity that spills outside the kiwi bandwidth.
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
                for (int c = 0; c < halfHt; c++) upper_dB[r]  += sonogramMatrix[ae.oblong.r1 + r, ae.oblong.c2 + c + buffer];
                for (int c = 0; c < halfHt; c++) lower_dB[r]  += sonogramMatrix[ae.oblong.r1 + r, ae.oblong.c1 - halfHt - buffer + c];
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



        //##################################################################################################################################################
        //##################################################################################################################################################
        //##################################################################################################################################################
        //NEXT THREE METHODS NOT USED ANYMORE BUT MIGHT SCAVANGE CODE LATER


        /// <summary>
        /// calculates score for change in inter-syllable distance over the KIWI event
        /// </summary>
        /// <param name="ae">an acoustic event</param>
        /// <param name="oscRate"></param>
        /// <returns></returns>
        public static double CalculateKiwiDeltaISDscore(AcousticEvent ae, double[] oscRate)
        {

            //double[] ANDREW_KIWI_GAPS = {0.25,0.27,0.30,0.34,0.38,0.41,0.45,0.47,0.49,0.51,0.54,0.57,0.59,0.62,0.63,0.66,0.68,0.70,0.72,0.75,0.77,0.79,0.79,0.80,0.82,0.83,0.84,0.85};//seconds
            //double[] andrewFrameGaps = new double[ANDREW_KIWI_GAPS.Length];
            //for (int i = 0; i < ANDREW_KIWI_GAPS.Length; i++) andrewFrameGaps[i] = ANDREW_KIWI_GAPS[i] * framesPerSecond;
            //double avAndrewDelta_seconds = 0.0;
            //for (int i = 1; i < ANDREW_KIWI_GAPS.Length; i++) avAndrewDelta_seconds += (ANDREW_KIWI_GAPS[i] - ANDREW_KIWI_GAPS[i - 1]);
            //double avAndrewDelta_Frames = avAndrewDelta_seconds / (ANDREW_KIWI_GAPS.Length-1) * framesPerSecond;


            //double[] array = DataTools.Subarray(oscRate, ae.oblong.r1, ae.oblong.RowWidth);
            //DataTools.writeBarGraph(array);
            int eventLength = (int)Math.Round(ae.Duration * ae.FramesPerSecond);
            int onetenth = eventLength / 10;
            int onefifth = eventLength / 5;
            int sevenTenth = eventLength * 7 / 10;
            int startOffset = ae.oblong.r1 + onetenth;
            int endOffset = ae.oblong.r1 + sevenTenth;
            double startISD = 0; //Inter-Syllable Distance in seconds
            double endISD = 0; //Inter-Syllable Distance in seconds
            for (int i = 0; i < onefifth; i++)
            {
                startISD += (1 / oscRate[startOffset + i]); //convert oscilation rates to inter-syllable distance i.e. periodicity.
                endISD += (1 / oscRate[endOffset + i]);
            }
            double deltaISD = (endISD - startISD) / onefifth; //get average change in inter-syllable distance
            double deltaScore = 0.0;
            if ((deltaISD >= -0.1) && (deltaISD <= 0.2)) deltaScore = (3.3333 * (deltaISD - 0.1)) + 0.3333;  //y=mx+c where c=0.333 and m=3.333
            else
                if (deltaISD > 0.2) deltaScore = 1.0;
            return deltaScore;
        }

        public static double CalculateKiwiEntropyScore(AcousticEvent ae, double[,] noiseReducedMatrix, double peakThreshold)
        {
            int eventLength = (int)Math.Round(ae.Duration * ae.FramesPerSecond);
            double[] event_dB = new double[eventLength]; //dB profile for event
            int eventHeight = ae.oblong.ColWidth;
            //int halfHt = eventHt / 2;
            //get acoustic activity within the event bandwidth and above it.
            for (int r = 0; r < eventLength; r++)
            {
                for (int c = 0; c < eventHeight; c++) event_dB[r] += noiseReducedMatrix[ae.oblong.r1 + r, ae.oblong.c1 + c]; //event dB profile
            }
            for (int r = 0; r < eventLength; r++) event_dB[r] /= eventHeight; //calculate average.

            bool[] peaks = DataTools.GetPeaks(event_dB);
            //int peakCount = DataTools.CountTrues(peaks);
            //DataTools.writeBarGraph(event_dB);

            //for (int r = 0; r < eventLength; r++) if (event_dB[r] < peakThreshold) peaks[r] = false;
            int peakCount2 = DataTools.CountTrues(peaks);
            int expectedPeakCount = (int)(ae.Duration * 0.8); //calculate expected number of peaks given event duration
            //if (peakCount2 == 0) return 1.0;                //assume that energy is dispersed
            if (peakCount2 < expectedPeakCount) return 0.0;   //assume that energy is concentrated

            //set up histogram of peak energies
            double[] histogram = new double[peakCount2];
            int count = 0;
            for (int r = 0; r < eventLength; r++)
            {
                if (peaks[r])
                {
                    histogram[count] = event_dB[r];
                    count++;
                }
            }
            histogram = DataTools.Normalise2Probabilites(histogram);
            double normFactor = Math.Log(histogram.Length) / DataTools.ln2;  //normalize for length of the array
            double entropy = DataTools.Entropy(histogram) / normFactor;
            return entropy;
        }

        public static double CalculateKiwiPeakPeriodicityScore(AcousticEvent ae, double[,] noiseReducedMatrix, double peakThreshold)
        {
            int eventLength = (int)Math.Round(ae.Duration * ae.FramesPerSecond);
            double[] event_dB = new double[eventLength]; //dB profile for event
            int eventHeight = ae.oblong.ColWidth;
            //get acoustic activity within the event bandwidth
            for (int r = 0; r < eventLength; r++)
            {
                for (int c = 0; c < eventHeight; c++) event_dB[r] += noiseReducedMatrix[ae.oblong.r1 + r, ae.oblong.c1 + c]; //event dB profile
            }
            //for (int r = 0; r < eventLength; r++) event_dB[r] /= eventHeight; //calculate average.

            event_dB = DataTools.filterMovingAverage(event_dB, 3);
            bool[] peaks = DataTools.GetPeaks(event_dB);


            //DataTools.writeBarGraph(event_dB);

            //for (int r = 0; r < eventLength; r++) if (event_dB[r] < peakThreshold) peaks[r] = false;
            //int peakCount2 = DataTools.CountTrues(peaks);
            //int expectedPeakCount = (int)ae.Duration;         //calculate expected number of peaks given event duration

            var tuple = DataTools.Periodicity_MeanAndSD(event_dB);
            double mean = tuple.Item1;
            double sd   = tuple.Item2;
            int peakCount = tuple.Item3;

            double score = 0.0;
            if (peakCount > (int)Math.Round(ae.Duration*1.2)) return score;

            double ratio = sd / mean;
            if (ratio < 0.333) score = 1.0;
            else if (ratio > 1.0) score = 0.0;
            else score = 1 - (ratio - 0.3) / 0.666;
            return score;
        }
        //ABOVE THREE METHODS NOT USED ANYMORE
        //##################################################################################################################################################
        //##################################################################################################################################################
        //##################################################################################################################################################




        public static Tuple<DataTable, DataTable, bool[]> ProcessCsvFile(FileInfo fiCsvFile)
        {
            DataTable dt = CsvTools.ReadCSVToTable(fiCsvFile.FullName, true);
            if ((dt == null) || (dt.Rows.Count == 0)) return null;

            dt = DataTableTools.SortTable(dt, "count ASC");
            List<bool> columns2Display = LSKiwiColumns2Display().ToList();
            DataTable processedtable = ProcessDataTableForDisplayOfColumnValues(dt);
            return System.Tuple.Create(dt, processedtable, columns2Display.ToArray());
        } // ProcessCsvFile()


        /// <summary>
        /// takes a data table of indices and converts column values to values in [0,1].
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DataTable ProcessDataTableForDisplayOfColumnValues(DataTable dt)
        {
            List<double[]> columns    = DataTableTools.ListOfColumnValues(dt);
            List<double[]> newColumns = new List<double[]>();
            for (int i = 0; i < columns.Count; i++)
            {
                double[] processedColumn = DataTools.normalise(columns[i]); //normalise all values in [0,1]
                newColumns.Add(processedColumn);
            }
            string[] headers = DataTableTools.GetColumnNames(dt);
            Type[] types = DataTableTools.GetColumnTypes(dt);
            for (int i = 0; i < columns.Count; i++)
            {
                if (types[i] == typeof(int)) types[i] = typeof(double);
                else
                if (types[i] == typeof(Int32)) types[i] = typeof(double);
            }
            var processedtable = DataTableTools.CreateTable(headers, types, newColumns);

            return processedtable;
        }


        public static DataTable WriteEvents2DataTable(int count, double segmentStartMinute, TimeSpan tsSegmentDuration, List<AcousticEvent> predictedEvents)
        {
            if (predictedEvents == null) return null;
            var dataTable = DataTableTools.CreateTable(HEADERS, COL_TYPES);
            if (predictedEvents.Count == 0) return dataTable;
            foreach (var kiwiEvent in predictedEvents)
            {
                int segmentStartSec = (int)(segmentStartMinute * 60);
                int eventStartAbsoluteSec   = (int)(segmentStartSec + kiwiEvent.TimeStart);
                int eventStartMin = eventStartAbsoluteSec / 60;
                int eventStartSec = eventStartAbsoluteSec % 60;
                string segmentDuration = DataTools.Time_ConvertSecs2Mins(tsSegmentDuration.TotalSeconds);

                DataRow row = dataTable.NewRow();
                row[HEADERS[0]] = count;                   //count
                row[HEADERS[1]] = eventStartAbsoluteSec;   //EvStartAbsolute - from start of source ifle
                row[HEADERS[2]] = eventStartMin;           //EvStartMin
                row[HEADERS[3]] = eventStartSec;           //EvStartSec
                row[HEADERS[4]] = segmentDuration;         //segmentDur
                row[HEADERS[5]] = predictedEvents.Count;   //Density
                row[HEADERS[6]] = kiwiEvent.Name;          //Label
                row[HEADERS[7]] = kiwiEvent.Duration;      //EventDur
                row[HEADERS[8]] = kiwiEvent.kiwi_durationScore;   //DurScore
                row[HEADERS[9]] = kiwiEvent.kiwi_hitScore;        //HitScore
                row[HEADERS[10]] = kiwiEvent.kiwi_snrScore;        //SnrScore
                row[HEADERS[11]] = kiwiEvent.kiwi_sdPeakScore;     //sdScore
                row[HEADERS[12]] = kiwiEvent.kiwi_intensityScore;        //GapScore
                row[HEADERS[13]] = kiwiEvent.kiwi_bandWidthScore;  //BWScore
                row[HEADERS[14]] = kiwiEvent.ScoreNormalised;      //Weighted combination Score
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }



        //##################################################################################################################################################

        //public static Tuple<DataTable, double[]> ProcessCsvFile(FileInfo fiCsvFile)
        //{
        //    //KiwiRecogniser.InitOutputTableColumns(); //initialise just in case have not been before now.
        //    DataTable dt = CsvTools.ReadCSVToTable(fiCsvFile.FullName, true);//LOAD CSV FILE
        //    if ((dt == null) || (dt.Rows.Count == 0)) return null;

        //    //return last column as the one for color display
        //    string[] headers = DataTableTools.GetColumnNames(dt);
        //    double[] array = DataTableTools.Column2ListOfDouble(dt, headers[headers.Length - 1]).ToArray(); 
        //    return System.Tuple.Create(dt, array);
        //}


        /// <summary>
        /// Converts a DataTable of Kiwi events into a datatable where one row = one minute of events
        /// WARNING: TODO: This method needs to be checked! Maybe putting events into the wrong minute.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DataTable ConvertListOfKiwiEvents2TemporalList(DataTable dt)
        {
            dt = DataTableTools.SortTable(dt, "EvStartAbs ASC"); //must ensure a sort

            double scoreThreshold = 0.25;
            int timeScale = 60; //i.e. 60 seconds per output row.
            string[] headers = { "count", "minute", "# Events", ("#Ev>" + scoreThreshold), "KiwiScore" };
            Type[] types = { typeof(int), typeof(int), typeof(int), typeof(int), typeof(double) };
            var newtable = DataTableTools.CreateTable(headers, types);

            int prevMinuteStart = 0;
            int minuteStart = 0;
            int eventcount = 0;
            int eventCountThresholded = 0;
            double kiwiScore = 0.0;
            foreach (DataRow kiwievent in dt.Rows)
            {
                int eventStart        = (int)kiwievent["EvStartAbs"];
                double eventKiwiScore = (double)kiwievent["KiwiScore"];
                if (eventKiwiScore > kiwiScore) kiwiScore = eventKiwiScore;
                minuteStart = eventStart / timeScale;
                
                if (minuteStart > prevMinuteStart)
                {
                    // fill in missing minutes
                    for (int i = prevMinuteStart + 1; i < minuteStart; i++ ) 
                        newtable.Rows.Add(i, i, 0, 0, 0.0);
                    newtable.Rows.Add(minuteStart, minuteStart, eventcount, eventCountThresholded, kiwiScore);
                    prevMinuteStart = minuteStart;
                    eventcount = 1;
                    if (eventKiwiScore > scoreThreshold) eventCountThresholded = 1;
                    else                                 eventCountThresholded = 0;
                    kiwiScore = 0.0;
                }
                else
                {
                    eventcount++;
                    if (eventKiwiScore > scoreThreshold) eventCountThresholded++;
                }
            }
            newtable.Rows.Add(minuteStart, minuteStart, eventcount, eventCountThresholded, kiwiScore); //add in last minute
            CsvTools.DataTable2CSV(newtable, @"C:\SensorNetworks\WavFiles\Kiwi\Results_TOWER_20100208_204500\DELETE_ME.csv");
            return newtable;
        }


        public static Image DrawSonogram(BaseSonogram sonogram, double[,] hits, double[] scores, 
                                         List<AcousticEvent> predictedEvents, double eventThreshold)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, eventThreshold));
            double maxScore = 16.0;
            image.AddSuperimposedMatrix(hits, maxScore);
            image.AddEvents(predictedEvents, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount); 
            return image.GetImage();
        }


        public static DataTable CalculateRecallPrecision(FileInfo fiMyResults, FileInfo fiTheTruth)
        {
            string[] ROC_HEADERS = { "startSec", "min",         "secOffset", "hitScore",     "snrScore",     "sdScore",        "gapScore",      "bwScore",    "comboScore", "Quality",       "Sex",        "Harmonics",      "TP",     "FP",       "FN"};
            Type[] ROC_COL_TYPES = { typeof(int),typeof(string), typeof(int), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(int), typeof(string), typeof(int), typeof(int), typeof(int), typeof(int) };

            //ANDREW'S HEADERS:          Selection,        View,     Channel, Begin Time (s),  End Time (s), Low Freq (Hz),High Freq (Hz),    Begin File,    Species,        Sex,       Harmonics,   Quality
            Type[] ANDREWS_TYPES = {typeof(string),typeof(string),typeof(int),typeof(double),typeof(double),typeof(double),typeof(double),typeof(string),typeof(string),typeof(string),typeof(int),typeof(int) };
        
            bool isFirstRowHeader = true;
            var dtADResults = CsvTools.ReadCSVToTable(fiTheTruth.FullName,  isFirstRowHeader, ANDREWS_TYPES);  //AD = Andrew Digby
            var dtMyResults = CsvTools.ReadCSVToTable(fiMyResults.FullName, isFirstRowHeader, COL_TYPES);      //MY = Michael Towsey
            //string colName  = "Species"; 
            //string value    = "LSK";
            //DataTableTools.DeleteRows(dtADResults, colName, value); //delete rows where Species name is not "LSK"
            var dtOutput = DataTableTools.CreateTable(ROC_HEADERS, ROC_COL_TYPES);
            int TP = 0;
            int FP = 0;
            int FN = 0;

            foreach(DataRow myRow in dtMyResults.Rows)
            {
                int    myStart  = (int)myRow[HEADERS[6]];
                double hitScore = (double)myRow[HEADERS[11]];
                double snrScore = (double)myRow[HEADERS[12]];
                double sdScore = (double)myRow[HEADERS[13]]; //sdPeakScore
                double gapScore = (double)myRow[HEADERS[14]];
                double bandWidthScore = (double)myRow[HEADERS[15]];
                double comboScore = (double)myRow[HEADERS[16]];

                //################################################################################
                // the following line experiments with different weightings other than the default
                comboScore = (hitScore * 0.0) + (snrScore * 0.1) + (sdScore * 0.1) + (gapScore * 0.3) + (bandWidthScore * 0.5); //weighted sum


                DataRow opRow = dtOutput.NewRow();
                opRow["startSec"]  = myStart;
                opRow["min"]       = (string)myRow[1];
                opRow["secOffset"] = (int)Math.Round((double)myRow[5]);
                opRow["hitScore"]  = hitScore;
                opRow["snrScore"]  = snrScore;
                opRow["sdScore"]   = sdScore;
                opRow["gapScore"]  = gapScore;
                opRow["bwScore"]   = bandWidthScore;
                opRow["comboScore"] = comboScore;
                opRow["Quality"] = -99;
                opRow["Sex"] = "-";
                opRow["Harmonics"] = 0;
                opRow["TP"] = 0;
                opRow["FP"] = 0;
                opRow["FN"] = 0;

                bool isTP = false;
                foreach (DataRow adRow in dtADResults.Rows) 
                {
                    double adStart = (double)adRow["Begin Time (s)"];
                    if ((adStart >= (myStart - 10)) && (adStart <= (myStart + 10))) //myStart is within 10 seconds of adStart THERFORE TRUE POSTIIVE
                    {
                        isTP = true;
                        adRow["Begin Time (s)"] = Double.NaN; //mark so that will not use again 
                        opRow["Quality"] = adRow["Quality"];
                        opRow["Sex"] = adRow["Sex"];
                        opRow["Harmonics"] = adRow["Harmonics"];
                        break;
                    }
                } //foreach - AD loop
                if (isTP)
                {
                    opRow["TP"] = 1;
                    TP++;
                }
                else //FALSE POSITIVE
                {
                    opRow["FP"] = 1;
                    FP++;
                }
                dtOutput.Rows.Add(opRow);
            } //foreach - MY loop

            //now add in the false negatives
            foreach (DataRow adRow in dtADResults.Rows)
            {
                double adStart = (double)adRow["Begin Time (s)"];
                if (! Double.IsNaN(adStart))
                {
                    DataRow row = dtOutput.NewRow();
                    row["startSec"] = (int)Math.Round(adStart);
                    row["min"]      = "not calculated";
                    row["secOffset"] = 0;
                    row["hitScore"] = 0.0;
                    row["snrScore"] = 0.0;
                    row["sdScore"]  = 0.0;
                    row["gapScore"] = 0.0;
                    row["bwScore"]  = 0.0;
                    row["comboScore"] = 0.0;
                    row["Quality"] = adRow["Quality"];
                    row["Sex"] = adRow["Sex"];
                    row["Harmonics"] = adRow["Harmonics"];
                    row["TP"] = 0;
                    row["FP"] = 0;
                    row["FN"] = 1;
                    dtOutput.Rows.Add(row);
                    FN++;
                }
            }

            double recall = TP / (double)(TP + FN);
            double specificity = TP / (double)(TP + FP);
            Console.WriteLine("TP={0},  FP={1},  FN={2}", TP, FP, FN);
            Console.WriteLine("RECALL={0:f3},  SPECIFICITY={1:f3}", recall, specificity);

            string sortString = "comboScore desc";
            //string sortString = "startSec desc";
            dtOutput = DataTableTools.SortTable(dtOutput, sortString);

            ROCCurve(dtOutput, dtADResults.Rows.Count); //write ROC area above curve


            return dtOutput;
        }


        public static void ROCCurve(DataTable dt, int countOfTargetTrues)
        {
            double previousRecall = 0.0;
            int cumulativeTP = 0; 
            int cumulativeFP = 0;
            double area = 0.0;  //area under the ROC curve
            List<double> curveValues = new List<double>();
            double maxAccuracy = 0.0;
            double precisionAtMax = 0.0;
            double recallAtMax = 0.0;
            double scoreAtMax = 0.0;
            double precisionAt30 = 0.0;
            double recallAt30 = 0.0;
            double scoreAt30 = 0.0;


            int count = 0;
            foreach (DataRow row in dt.Rows)
            {
                int value = (int)row["TP"];
                if (value == 1) cumulativeTP++;
                else
                    if ((int)row["FP"] == 1) cumulativeFP++;
                double recall = cumulativeTP / (double)countOfTargetTrues;
                double precision = cumulativeTP / (double)(cumulativeTP + cumulativeFP);
                double accuracy = (recall + precision) / (double)2;
                if (accuracy > maxAccuracy)
                {
                    maxAccuracy = accuracy;
                    recallAtMax = recall;
                    precisionAtMax = precision;
                    scoreAtMax = (double)row["comboScore"];
                }
                count++;
                if (count == 30)
                {
                    recallAt30 = recall;
                    precisionAt30 = precision;
                    scoreAt30 = (double)row["comboScore"];
                }

                double delta = precision * (recall - previousRecall);
                area += delta;
                if (delta > 0.0) curveValues.Add(delta);
                previousRecall = recall;
            }
            DataTools.writeBarGraph(curveValues.ToArray());
            Console.WriteLine("Area under ROC curve = {0:f4}", area);
            Console.WriteLine("Max accuracy={0:f3};  where recall={1:f3}, precision={2:f3} for score threshold={3:f3}", maxAccuracy, recallAtMax, precisionAtMax, scoreAtMax);
            Console.WriteLine("At 30 samples: recall={0:f3},  precision={1:f3},  at score={2:f3}", recallAt30, precisionAt30, scoreAt30);
        }

    } //end class
}
