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


    ///This class is a combination of analysers
    /// When adding a new analyser to this class need to modify two methods:
    ///    1) ConvertEvents2Indices();  and
    ///    2) Analyse()
    ///
    /// As of 20 June 2012 this class includes three analysers: crow, human, machine.
    /// As of 22 June 2012 this class includes five  analysers: crow, human, machine, canetoad, koala-male.

    public class MultiAnalyser : IAnalyser
    {
        //OTHER CONSTANTS
        public const string ANALYSIS_NAME = "MultiAnalyser";
        public const int RESAMPLE_RATE = 17640;
        //public const int RESAMPLE_RATE = 22050;
        //public const string imageViewer = @"C:\Program Files\Windows Photo Viewer\ImagingDevices.exe";
        public const string imageViewer = @"C:\Windows\system32\mspaint.exe";

        public static string[] analysisTitles = { Human2.ANALYSIS_NAME, Crow.ANALYSIS_NAME, PlanesTrainsAndAutomobiles.ANALYSIS_NAME, Canetoad.ANALYSIS_NAME, KoalaMale.ANALYSIS_NAME };


        public string DisplayName
        {
            get { return "Multiple analyses"; }
        }

        private static string identifier = "Towsey." + ANALYSIS_NAME;
        public string Identifier
        {
            get { return identifier; }
        }

        public static void Dev(string[] args)
        {
            //HUMAN
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Crows\Crows111216-001Mono5-7min.mp3";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\PramukSpeech_20090615.wav"; //WARNING: RECORDING IS 44 MINUTES LONG. NEEDT TO SAMPLE
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\Wimmer_DM420011.wav";         
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\DM420036_min452Speech.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\DM420036_min465Speech.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\BAC2_20071018-143516_speech.wav";
            string recordingPath = @"C:\SensorNetworks\WavFiles\Human\Planitz.wav";
            //MACHINES
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\DM420036_min465Speech.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Machines\DM420036_min173Airplane.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Machines\DM420036_min449Airplane.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Machines\DM420036_min700Airplane.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Machines\DM420036_min757PLANE.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Machines\KAPITI2-20100219-202900_Airplane.mp3";
            //CROW
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Crows\Cassandra111216-001Mono5-7min.mp3";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Crows\DM420036_min430Crows.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Crows\DM420036_min646Crows.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h.mp3";

            //KOALA MALE
            //string recordingPath = @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\HoneymoonBay_StBees_20080905-001000.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\HoneymoonBay_StBees_20080909-013000.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\TopKnoll_StBees_20080909-003000.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\TopKnoll_StBees_VeryFaint_20081221-003000.wav";
            //CANETOAD
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100529_16bitPCM.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100530_2_16bitPCM.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100530_1_16bitPCM.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Canetoad\RuralCanetoads_9Jan\toads_rural_9jan2010\toads_rural1_16.mp3";



            string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg";
            string outputDir  = @"C:\SensorNetworks\Output\MultiAnalyser\";

            string title = "# RUNS MULTIPLE ANALYSES";
            string date = "# DATE AND TIME: " + DateTime.Now;
            Console.WriteLine(title);
            Console.WriteLine(date);
            Console.WriteLine("# Output folder:  " + outputDir);
            Console.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            var diOutputDir = new DirectoryInfo(outputDir);

            Log.Verbosity = 1;
            int startMinute = 0;
            int durationSeconds = 60; //set zero to get entire recording
            var tsStart = new TimeSpan(0, startMinute, 0); //hours, minutes, seconds
            var tsDuration = new TimeSpan(0, 0, durationSeconds); //hours, minutes, seconds
            var segmentFileStem = Path.GetFileNameWithoutExtension(recordingPath);
            var segmentFName  = string.Format("{0}_{1}min.wav", segmentFileStem, startMinute);
            var sonogramFname = string.Format("{0}_{1}min.png", segmentFileStem, startMinute);
            var eventsFname   = string.Format("{0}_{1}min.{2}.Events.csv",  segmentFileStem, startMinute, identifier);
            var indicesFname  = string.Format("{0}_{1}min.{2}.Indices.csv", segmentFileStem, startMinute, identifier);

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
            DirectoryInfo diSource = new DirectoryInfo(Path.GetDirectoryName(recordingPath));
            if (!diSource.Exists)
            {
                Console.WriteLine("Source directory does not exist: " + diSource.FullName);
                status = 2;
                return status;
            }
            FileInfo fiSource = new FileInfo(recordingPath);
            if (!fiSource.Exists)
            {
                Console.WriteLine("Source directory exists: " + diSource.FullName);
                Console.WriteLine("\t but the source file does not exist: " + recordingPath);
                status = 2;
                return status;
            }
            FileInfo fiConfig = new FileInfo(configPath);
            if (!fiConfig.Exists)
            {
                Console.WriteLine("Config file does not exist: " + fiConfig.FullName);
                status = 2;
                return status;
            }
            DirectoryInfo diOP = new DirectoryInfo(outputDir);
            if (!diOP.Exists)
            {
                Console.WriteLine("Output directory does not exist: " + diOP.FullName);
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
            var configuration = new ConfigDictionary(fiConfig.FullName);
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
            IAnalyser analyser = new MultiAnalyser();
            AnalysisResult result = analyser.Analyse(analysisSettings);
            DataTable dt = result.Data;
            //#############################################################################################################################################

            //ADD IN ADDITIONAL INFO TO RESULTS TABLE
            if (dt != null)
            {
                AddContext2Table(dt, tsStart, result.AudioDuration);
                CsvTools.DataTable2CSV(dt, analysisSettings.EventsFile.FullName);
                //DataTableTools.WriteTable(dt);
            }

            return status;
        }



        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            //var configuration = new ConfigDictionary(analysisSettings.ConfigFile.FullName);
            //Dictionary<string, string> configDict = configuration.GetTable();
            var configDict = analysisSettings.ConfigDict;
            string frameLength = null;
            if (configDict.ContainsKey(Keys.FRAME_LENGTH)) 
                 frameLength = (Int32.Parse(configDict[Keys.FRAME_LENGTH]).ToString()); 

            var fiAudioF    = analysisSettings.AudioFile;
            var diOutputDir = analysisSettings.AnalysisRunDirectory;

            var analysisResults = new AnalysisResult();
            analysisResults.AnalysisIdentifier = this.Identifier;
            analysisResults.SettingsUsed = analysisSettings;
            analysisResults.Data = null;

            Dictionary<string, string> newDict;
            BaseSonogram sonogram = null;
            DataTable dataTable = null;
            var events = new List<AcousticEvent>();
            double[,] hits = null;
            var recordingTimeSpan = new TimeSpan();
            var scores = new List<Plot>();

            //######################################################################
            //HUMAN
            //######################################################################
            newDict = new Dictionary<string, string>();
            string filter = "HUMAN";
            var keysFiltered= DictionaryTools.FilterKeysInDictionary(configDict, filter);

            foreach (string key in keysFiltered) //derive new dictionary for human
            {
                string newKey = key.Substring(6);
                newDict.Add(newKey, configDict[key]);
            }
            newDict.Add(Keys.ANALYSIS_NAME, Human2.ANALYSIS_NAME);
            if (frameLength != null) 
                newDict.Add(Keys.FRAME_LENGTH, frameLength); 

            var results1 = Human2.Analysis(fiAudioF, newDict);
            if (results1 != null)
            {
                sonogram = results1.Item1;
                hits = results1.Item2;
                scores.Add(results1.Item3);
                if (results1.Item4 != null)
                {
                    foreach (AcousticEvent ae in results1.Item4)
                    {
                        ae.Name = Human2.ANALYSIS_NAME;
                        events.Add(ae);
                    }
                }
                recordingTimeSpan = results1.Item5;
            }
            //######################################################################
            //CROW
            //######################################################################
            newDict = new Dictionary<string, string>();
            filter = "CROW";
            keysFiltered = DictionaryTools.FilterKeysInDictionary(configDict, filter);

            foreach (string key in keysFiltered)  //derive new dictionary for crow
            {
                string newKey = key.Substring(5);
                newDict.Add(newKey, configDict[key]);
            }
            newDict.Add(Keys.ANALYSIS_NAME, Crow.ANALYSIS_NAME);
            if (frameLength != null)
                newDict.Add(Keys.FRAME_LENGTH, frameLength);
            
            var results2 = Crow.Analysis(fiAudioF, newDict);
            if (results2 != null)
            {
                if (sonogram == null) sonogram = results2.Item1;
                hits = MatrixTools.AddMatrices(hits, results2.Item2);
                scores.Add(results2.Item3);
                if (results2.Item4 != null)
                {
                    foreach (AcousticEvent ae in results2.Item4)
                    {
                        ae.Name = Crow.ANALYSIS_NAME;
                        events.Add(ae);
                    }
                }
                recordingTimeSpan = results2.Item5;
            }
            //######################################################################
            //MACHINES
            //######################################################################
            newDict = new Dictionary<string, string>();
            filter = "MACHINE";
            keysFiltered = DictionaryTools.FilterKeysInDictionary(configDict, filter);

            foreach (string key in keysFiltered)
            {
                string newKey = key.Substring(8);
                newDict.Add(newKey, configDict[key]);
            }
            newDict.Add(Keys.ANALYSIS_NAME, PlanesTrainsAndAutomobiles.ANALYSIS_NAME);
            if (frameLength != null)
                newDict.Add(Keys.FRAME_LENGTH, frameLength); 

            var results3 = PlanesTrainsAndAutomobiles.Analysis(fiAudioF, newDict);
            if (results3 != null)
            {
                if (sonogram == null) sonogram = results3.Item1;
                hits = MatrixTools.AddMatrices(hits, results3.Item2);
                scores.Add(results3.Item3);
                if (results3.Item4 != null)
                {
                    foreach (AcousticEvent ae in results3.Item4)
                    {
                        ae.Name = PlanesTrainsAndAutomobiles.ANALYSIS_NAME;
                        events.Add(ae);
                    }
                }
                recordingTimeSpan = results3.Item5;
            }
            //######################################################################
            //CANETOAD
            //######################################################################
            newDict = new Dictionary<string, string>();
            filter = "CANETOAD";
            keysFiltered = DictionaryTools.FilterKeysInDictionary(configDict, filter);

            foreach (string key in keysFiltered)  //derive new dictionary for crow
            {
                string newKey = key.Substring(9);
                newDict.Add(newKey, configDict[key]);
            }
            newDict.Add(Keys.ANALYSIS_NAME, Canetoad.ANALYSIS_NAME);
            if (frameLength != null)
                newDict.Add(Keys.FRAME_LENGTH, frameLength);

            var results4 = Canetoad.Analysis(fiAudioF, newDict);
            if (results4 != null)
            {
                if (sonogram == null) sonogram = results4.Item1;
                //hits = MatrixTools.AddMatrices(hits, results4.Item2);
                scores.Add(results4.Item3);
                if (results4.Item4 != null)
                {
                    foreach (AcousticEvent ae in results4.Item4)
                    {
                        ae.Name = Canetoad.ANALYSIS_NAME;
                        events.Add(ae);
                    }
                }
                recordingTimeSpan = results4.Item5;
            }
            //######################################################################
            //KOALA-MALE
            //######################################################################
            newDict = new Dictionary<string, string>();
            filter = "KOALAMALE";
            keysFiltered = DictionaryTools.FilterKeysInDictionary(configDict, filter);

            foreach (string key in keysFiltered)  //derive new dictionary for crow
            {
                string newKey = key.Substring(10);
                newDict.Add(newKey, configDict[key]);
            }
            newDict.Add(Keys.ANALYSIS_NAME, KoalaMale.ANALYSIS_NAME);
            if (frameLength != null)
                newDict.Add(Keys.FRAME_LENGTH, frameLength);

            var results5 = KoalaMale.Analysis(fiAudioF, newDict);
            if (results5 != null)
            {
                if (sonogram == null) sonogram = results5.Item1;
                //hits = MatrixTools.AddMatrices(hits, results5.Item2);
                scores.Add(results5.Item3);
                if (results5.Item4 != null)
                {
                    foreach (AcousticEvent ae in results5.Item4)
                    {
                        ae.Name = KoalaMale.ANALYSIS_NAME;
                        events.Add(ae);
                    }
                }
                recordingTimeSpan = results5.Item5;
            }
            //######################################################################


            if ((events != null) && (events.Count != 0))
            {
                string analysisName = configDict[Keys.ANALYSIS_NAME];
                string fName = Path.GetFileNameWithoutExtension(fiAudioF.Name);
                foreach (AcousticEvent ev in events)
                {
                    ev.SourceFileName = fName;
                    //ev.Name = analysisName;
                    ev.SourceFileDuration = recordingTimeSpan.TotalSeconds;
                }
                //write events to a data table to return.
                dataTable = WriteEvents2DataTable(events);
                string sortString = Keys.EVENT_START_SEC + " ASC";
                dataTable = DataTableTools.SortTable(dataTable, sortString); //sort by start time before returning
            }

            if ((analysisSettings.EventsFile != null) && (dataTable != null))
            {
                CsvTools.DataTable2CSV(dataTable, analysisSettings.EventsFile.FullName);
            }

            if ((analysisSettings.IndicesFile != null) && (dataTable != null))
            {
                double scoreThreshold = 0.1;
                TimeSpan unitTime = TimeSpan.FromSeconds(60); //index for each time span of i minute
                var indicesDT = ConvertEvents2Indices(dataTable, unitTime, recordingTimeSpan, scoreThreshold);
                CsvTools.DataTable2CSV(indicesDT, analysisSettings.IndicesFile.FullName);
            }

            //save image of sonograms
            if (analysisSettings.ImageFile != null)
            {
                string imagePath = analysisSettings.ImageFile.FullName;
                double eventThreshold = 0.1;
                Image image = DrawSonogram(sonogram, hits, scores, events, eventThreshold);
                image.Save(imagePath, ImageFormat.Png);
            }

            analysisResults.Data = dataTable;
            analysisResults.ImageFile = analysisSettings.ImageFile;
            analysisResults.AudioDuration = recordingTimeSpan;
            //result.DisplayItems = { { 0, "example" }, { 1, "example 2" }, }
            //result.OutputFiles = { { "exmaple file key", new FileInfo("Where's that file?") } }
            return analysisResults;
        } //Analyse()


        static Image DrawSonogram(BaseSonogram sonogram, double[,] hits, List<Plot> scores, List<AcousticEvent> predictedEvents, double eventThreshold)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            int maxFreq = sonogram.NyquistFrequency / 2;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(maxFreq, 1, doHighlightSubband, add1kHzLines));

            //System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            //img.Save(@"C:\SensorNetworks\temp\testimage1.png", System.Drawing.Imaging.ImageFormat.Png);

            //Image_MultiTrack image = new Image_MultiTrack(img);
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            if (scores != null) 
                for (int i = 0; i < scores.Count; i++)
                {
                    scores[i].ScaleDataArray(sonogram.FrameCount);
                    image.AddTrack(Image_Track.GetNamedScoreTrack(scores[i].data, 0.0, 1.0, scores[i].threshold, scores[i].title));
                }
            //if (hits != null) image.OverlayRedTransparency(hits);
            if (hits != null) image.OverlayRainbowTransparency(hits);
            if ((predictedEvents != null) && (predictedEvents.Count > 0))
                image.AddEvents(predictedEvents, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
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
                                 //AudioAnalysisTools.Keys.EVENT_INTENSITY,
                                 AudioAnalysisTools.Keys.EVENT_NAME,
                                 AudioAnalysisTools.Keys.EVENT_SCORE,
                                 AudioAnalysisTools.Keys.EVENT_NORMSCORE 

                               };
            //                   1                2               3              4                5              6               7              8
            Type[] types = { typeof(int), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(string), typeof(double), typeof(double) };

            var dataTable = DataTableTools.CreateTable(headers, types);
            if (predictedEvents.Count == 0) return dataTable;

            foreach (var ev in predictedEvents)
            {
                DataRow row = dataTable.NewRow();
                row[AudioAnalysisTools.Keys.EVENT_START_SEC] = (double)ev.TimeStart;  //EvStartSec
                row[AudioAnalysisTools.Keys.EVENT_START_ABS] = (double)ev.TimeStart;  //EvStartAbs - OVER-WRITE LATER
                row[AudioAnalysisTools.Keys.EVENT_DURATION] = (double)ev.Duration;   //duratio in seconds
                row[AudioAnalysisTools.Keys.EVENT_NAME] = (string)ev.Name;   //
                row[AudioAnalysisTools.Keys.EVENT_NORMSCORE] = (double)ev.ScoreNormalised;
                row[AudioAnalysisTools.Keys.EVENT_SCORE] = (double)ev.Score;      //Score
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
            int[] human_EventsPerUnitTime = new int[unitCount]; //to store event counts
            int[] crow__EventsPerUnitTime = new int[unitCount]; //to store counts
            int[] machinEventsPerUnitTime = new int[unitCount]; //to store counts
            int[] koala_EventsPerUnitTime = new int[unitCount]; //to store counts
            int[] canetdEventsPerUnitTime = new int[unitCount]; //to store counts



            foreach (DataRow ev in dt.Rows)
            {
                double eventStart = (double)ev[AudioAnalysisTools.Keys.EVENT_START_ABS];
                double eventScore = (double)ev[AudioAnalysisTools.Keys.EVENT_NORMSCORE];
                int timeUnit = (int)(eventStart / unitTime.TotalSeconds);

                string eventName = (string)ev[AudioAnalysisTools.Keys.EVENT_NAME];
                if(eventName == Human2.ANALYSIS_NAME)
                {
                    if (eventScore != 0.0) human_EventsPerUnitTime[timeUnit]++;
                } else if(eventName == Crow.ANALYSIS_NAME)
                {
                    if (eventScore != 0.0) crow__EventsPerUnitTime[timeUnit]++;
                }
                else if (eventName == PlanesTrainsAndAutomobiles.ANALYSIS_NAME)
                {
                    if (eventScore != 0.0) machinEventsPerUnitTime[timeUnit]++;
                }
                else if (eventName == KoalaMale.ANALYSIS_NAME)
                {
                    if (eventScore != 0.0) koala_EventsPerUnitTime[timeUnit]++;
                }
                else if (eventName == Canetoad.ANALYSIS_NAME)
                {
                    if (eventScore != 0.0) canetdEventsPerUnitTime[timeUnit]++;
                }
            }

            string[] headers = { AudioAnalysisTools.Keys.START_MIN, "HumanEvents", "CrowEvents", "MachineEvents", "KoalaEvents", "CanetoadEvents" };
            Type[]   types   = { typeof(int),                        typeof(int),  typeof(int),   typeof(int),     typeof(int),    typeof(int) };
            var newtable = DataTableTools.CreateTable(headers, types);

            for (int i = 0; i < unitCount; i++)
            {
                int unitID = (int)(i * unitTime.TotalMinutes);
                newtable.Rows.Add(unitID, human_EventsPerUnitTime[i], crow__EventsPerUnitTime[i], machinEventsPerUnitTime[i],
                                          koala_EventsPerUnitTime[i], canetdEventsPerUnitTime[i]);
            }
            return newtable;
        }

        public static void AddContext2Table(DataTable dt, TimeSpan segmentStartMinute, TimeSpan recordingTimeSpan)
        {
            if (dt == null) return;

            if (!dt.Columns.Contains(Keys.SEGMENT_TIMESPAN)) dt.Columns.Add(AudioAnalysisTools.Keys.SEGMENT_TIMESPAN, typeof(double));
            if (!dt.Columns.Contains(Keys.EVENT_START_ABS)) dt.Columns.Add(AudioAnalysisTools.Keys.EVENT_START_ABS, typeof(double));
            if (!dt.Columns.Contains(Keys.EVENT_START_MIN)) dt.Columns.Add(AudioAnalysisTools.Keys.EVENT_START_MIN, typeof(double));
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


        public string DefaultConfiguration
        {
            get
            {
                return string.Empty;
            }
        }


        public AnalysisSettings DefaultSettings
        {
            get
            {
                return new AnalysisSettings
                {
                    SegmentMaxDuration = TimeSpan.FromMinutes(1),
                    SegmentMinDuration = TimeSpan.FromSeconds(30),
                    SegmentMediaType = MediaTypes.MediaTypeWav,
                    SegmentOverlapDuration = TimeSpan.Zero,
                    SegmentTargetSampleRate = AnalysisTemplate.RESAMPLE_RATE
                };
            }
        }

    } //end class MultiAnalyser
}
