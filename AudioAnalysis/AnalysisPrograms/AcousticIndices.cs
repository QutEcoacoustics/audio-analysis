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
    public class AcousticIndices : IAnalysis
    {
        //TASK IDENTIFIERS
        public const string task_ANALYSE  = "analysis";
        public const string task_LOAD_CSV = "loadCsv";

        //KEYS TO PARAMETERS IN CONFIG FILE
        public static string key_ANALYSIS_NAME = "ANALYSIS_NAME";
        public static string key_CALL_DURATION = "CALL_DURATION";
        public static string key_DECIBEL_THRESHOLD = "DECIBEL_THRESHOLD";
        public static string key_DRAW_SONOGRAMS = "DRAW_SONOGRAMS";
        public static string key_DISPLAY_COLUMNS = "DISPLAY_COLUMNS";
        public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_FRAME_LENGTH = "FRAME_LENGTH";
        public static string key_FRAME_OVERLAP = "FRAME_OVERLAP";
        public static string key_INTENSITY_THRESHOLD = "INTENSITY_THRESHOLD";
        public static string key_MIN_HZ = "MIN_HZ";
        public static string key_MAX_HZ = "MAX_HZ";
        public static string key_MIN_GAP = "MIN_GAP";
        public static string key_MAX_GAP = "MAX_GAP";
        public static string key_MIN_AMPLITUDE = "MIN_AMPLITUDE";
        public static string key_MIN_DURATION = "MIN_DURATION";
        public static string key_MAX_DURATION = "MAX_DURATION";
        public static string key_NOISE_REDUCTION_TYPE = "NOISE_REDUCTION_TYPE";
        public static string key_RESAMPLE_RATE = "RESAMPLE_RATE";
        public static string key_SEGMENT_DURATION = "SEGMENT_DURATION";
        public static string key_SEGMENT_OVERLAP = "SEGMENT_OVERLAP";

        //KEYS TO OUTPUT EVENTS and INDICES
        public static string key_COUNT     = "count";
        //public static string key_SEGMENT_TIMESPAN = "SegTimeSpan";
        public static string key_AV_AMPLITUDE = "avAmp-dB";
        //public static string key_START_ABS = "EvStartAbs";
        //public static string key_START_MIN = "EvStartMin";
        //public static string key_START_SEC = "EvStartSec";
        public static string key_CALL_DENSITY = "CallDensity";
        public static string key_CALL_SCORE = "CallScore";
        public static string key_EVENT_TOTAL= "# events";


        //OTHER CONSTANTS
        public const string ANALYSIS_NAME = "AcousticIndices";
        public const int RESAMPLE_RATE = 17640;
        //public const int RESAMPLE_RATE = 22050;
        //public const string imageViewer = @"C:\Program Files\Windows Photo Viewer\ImagingDevices.exe";
        public const string imageViewer = @"C:\Windows\system32\mspaint.exe";


        public string DisplayName
        {
            get { return "Acoustic Indices"; }
        }

        public string Identifier
        {
            get { return "Towsey." + ANALYSIS_NAME; }
        }


        public static void Dev(string[] args)
        {
            string recordingPath = @"C:\SensorNetworks\WavFiles\Human\Planitz.wav";
            string configPath = @"C:\SensorNetworks\Output\AcousticIndices\Indices.cfg";
            string outputDir = @"C:\SensorNetworks\Output\AcousticIndices\";
            string csvPath = @"C:\SensorNetworks\Output\AcousticIndices\AcousticIndices.csv";

            string title = "# FOR EXTRACTION OF Acoustic Indices";
            string date = "# DATE AND TIME: " + DateTime.Now;
            Console.WriteLine(title);
            Console.WriteLine(date);
            Console.WriteLine("# Output folder:  " + outputDir);
            Console.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            var diOutputDir = new DirectoryInfo(outputDir);

            Log.Verbosity = 1;
            int startMinute = 0;
            int durationSeconds = 0; //set zero to get entire recording
            var tsStart = new TimeSpan(0, startMinute, 0); //hours, minutes, seconds
            var tsDuration = new TimeSpan(0, 0, durationSeconds); //hours, minutes, seconds
            var segmentFileStem = Path.GetFileNameWithoutExtension(recordingPath);
            var segmentFName = string.Format("{0}_converted.wav", segmentFileStem);
            var sonogramFname = string.Format("{0}_{1}min.png", segmentFileStem, startMinute);
            var eventsFname = string.Format("{0}_Events{1}min.csv", segmentFileStem, startMinute);
            var indicesFname = string.Format("{0}_Indices{1}min.csv", segmentFileStem, startMinute);

            var cmdLineArgs = new List<string>();
            if (false)
            {
                cmdLineArgs.Add(task_ANALYSE);
                cmdLineArgs.Add(recordingPath);
                cmdLineArgs.Add(configPath);
                cmdLineArgs.Add(outputDir);
                cmdLineArgs.Add("-tmpwav:" + segmentFName);
                cmdLineArgs.Add("-indices:" + indicesFname);
                cmdLineArgs.Add("-start:" + tsStart.TotalSeconds);
                cmdLineArgs.Add("-duration:" + tsDuration.TotalSeconds);
            }
            if (true)
            {
                //string indicesImagePath = "some path or another";
                cmdLineArgs.Add(task_LOAD_CSV);
                cmdLineArgs.Add(csvPath);
                cmdLineArgs.Add(configPath);
                //cmdLineArgs.Add(indicesImagePath);
            }

            //#############################################################################################################################################
            int status = Execute(cmdLineArgs.ToArray());
            if (status != 0)
            {
                Console.WriteLine("\n\n# FATAL ERROR. CANNOT PROCEED!");
                Console.ReadLine();
                System.Environment.Exit(99);
            }
            //#############################################################################################################################################

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
            //string imagePath = Path.Combine(outputDir, sonogramFname);
            //FileInfo fiImage = new FileInfo(imagePath);
            //if (fiImage.Exists)
            //{
            //    ProcessRunner process = new ProcessRunner(imageViewer);
            //    process.Run(imagePath, outputDir);
            //}

            Console.WriteLine("\n\n# Finished analysis:- " + Path.GetFileName(recordingPath));
            Console.ReadLine();
        } //Dev()

        /// <summary>
        /// Directs task to the appropriate method based on the first argument in the command line string.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int Execute(string[] args)
        {
            int status = 0;
            if (args.Length < 2)
            {
                Console.WriteLine("ERROR: You have called the AanalysisPrograms.MainEntry() method without command line arguments.");
                Console.WriteLine("Require at least 2 command line arguments.");
                status = 1;
                return status;
            }
            else
            {
                string[] restOfArgs = args.Skip(1).ToArray();
                switch (args[0])
                {
                    case task_ANALYSE:      // perform the analysis task
                        ExecuteAnalysis(restOfArgs);
                        break;
                    case task_LOAD_CSV:     // loads a csv file for visualisation
                        string[] defaultColumns2Display = { "avAmp-dB","snr-dB","bg-dB","activity","segCount","avSegDur","hfCover","mfCover","lfCover","H[ampl]","H[avSpectrum]","#clusters","avClustDur" };
                        var fiCsvFile    = new FileInfo(restOfArgs[0]);
                        var fiConfigFile = new FileInfo(restOfArgs[1]);
                        //var fiImageFile  = new FileInfo(restOfArgs[2]); //path to which to save image file.
                        IAnalysis analyser = new AcousticIndices();
                        var dataTables = analyser.ProcessCsvFile(fiCsvFile, fiConfigFile);
                        //returns two datatables, the second of which is to be converted to an image (fiImageFile) for display
                        break;
                    default:
                        Console.WriteLine("Task unrecognised>>>" + args[0]);
                        return 999;
                } //switch
            } //if-else
            return status;
        } //Execute()


        /// <summary>
        /// A WRAPPER AROUND THE analyser.Analyse(analysisSettings) METHOD
        /// To be called as an executable with command line arguments.
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="configPath"></param>
        /// <param name="outputPath"></param>
        public static int ExecuteAnalysis(string[] args)
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
                Console.WriteLine("Source file does not exist: " + recordingPath);
                status = 2;
                return status;
            }
            DirectoryInfo diOP = new DirectoryInfo(outputDir);
            if (!diOP.Exists)
            {
                Console.WriteLine("Output directory does not exist: " + recordingPath);
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
                if (parts[0].StartsWith("-indices"))
                {
                  string indicesPath = Path.Combine(outputDir, parts[1]);
                   analysisSettings.IndicesFile = new FileInfo(indicesPath);
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
                }//if
            } //for

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
            IAnalysis analyser = new AcousticIndices();
            AnalysisResult result = analyser.Analyse(analysisSettings);
            DataTable dt = result.Data;
            //#############################################################################################################################################

            //ADD IN ADDITIONAL INFO TO RESULTS TABLE
            if (dt != null)
            {
                int iter = 0; //dummy - iteration number would ordinarily be available at this point.
                int startMinute = (int)tsStart.TotalMinutes;
                AddContext2Table(dt, iter, startMinute, result.AudioDuration);
                //DataTable augmentedTable = AddContext2Table(dt, tsStart, result.AudioDuration);
                CsvTools.DataTable2CSV(dt, analysisSettings.IndicesFile.FullName);
                //DataTableTools.WriteTable2Console(dt);
            }

            return status;
        }



        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            var configuration = new ConfigDictionary(analysisSettings.ConfigFile.FullName);
            Dictionary<string, string> configDict = configuration.GetTable();
            //string key_GET_ANNOTATED_SONOGRAM = "ANNOTATE_SONOGRAM";
            //configDict.Add(key_GET_ANNOTATED_SONOGRAM, Boolean.FalseString);
            //if (analysisSettings.ImageFile != null) configDict[key_GET_ANNOTATED_SONOGRAM] = Boolean.TrueString;
            var fiAudioF    = analysisSettings.AudioFile;
            var diOutputDir = analysisSettings.AnalysisRunDirectory;

            var analysisResults = new AnalysisResult();
            analysisResults.AnalysisIdentifier = this.Identifier;
            analysisResults.SettingsUsed = analysisSettings;
            analysisResults.Data = null;

            //######################################################################
            var results = AcousticIndicesExtraction.Analysis(fiAudioF, configDict);
            //######################################################################

            if (results == null) return analysisResults; //nothing to process 
            analysisResults.Data          = results.Item1;
            analysisResults.AudioDuration = results.Item2;
            var sonogram                  = results.Item3;
            var hits                      = results.Item4;
            var scores                    = results.Item5;

            if (sonogram != null)
            {
                string imagePath = Path.Combine(diOutputDir.FullName, analysisSettings.ImageFile.Name);
                var image = DrawSonogram(sonogram, hits, scores);
                //var fiImage = new FileInfo(imagePath);
                //if (fiImage.Exists) fiImage.Delete();
                image.Save(imagePath, ImageFormat.Png);
                analysisResults.ImageFile = new FileInfo(imagePath);
            }

            if ((analysisSettings.IndicesFile != null) && (analysisResults.Data != null))
            {
                CsvTools.DataTable2CSV(analysisResults.Data, analysisSettings.IndicesFile.FullName);
            }
            return analysisResults;
        } //Analyse()


        static Image DrawSonogram(BaseSonogram sonogram, double[,] hits, List<double[]> scores)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            int maxFreq = sonogram.NyquistFrequency;
            //int maxFreq = sonogram.NyquistFrequency / 2;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(maxFreq, 1, doHighlightSubband, add1kHzLines));

            //System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            //img.Save(@"C:\SensorNetworks\temp\testimage1.png", System.Drawing.Imaging.ImageFormat.Png);

            //Image_MultiTrack image = new Image_MultiTrack(img);
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            double eventThreshold = 0.2;
            if (scores != null) foreach(double[] track in scores) image.AddTrack(Image_Track.GetScoreTrack(track, 0.0, 1.0, eventThreshold));
            if (hits != null) image.OverlayRainbowTransparency(hits);
            return image.GetImage();
        } //DrawSonogram()




        public static void AddContext2Table(DataTable dt, int count, int segmentStartMinute, TimeSpan recordingTimeSpan)
        {

            foreach (DataRow row in dt.Rows)
            {
                row[AcousticIndicesExtraction.header_count]           = count;
                row[AcousticIndicesExtraction.header_startMin]        = segmentStartMinute;
                row[AcousticIndicesExtraction.header_SecondsDuration] = recordingTimeSpan.TotalSeconds;
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
                    SegmentMediaType   = MediaTypes.MediaTypeWav,
                    SegmentOverlapDuration  = TimeSpan.Zero,
                    SegmentTargetSampleRate = AnalysisTemplate.RESAMPLE_RATE
                };
            }
        }




        public Tuple<DataTable, DataTable> ProcessCsvFile(FileInfo fiCsvFile, FileInfo fiConfigFile)
        {
            var configuration = new ConfigDictionary(fiConfigFile.FullName);
            Dictionary<string, string> configDict = configuration.GetTable();
            List<string> displayHeaders = configDict[key_DISPLAY_COLUMNS].Split(',').ToList();

            bool addColumnOfweightedIndices = true;
            DataTable dt = CsvTools.ReadCSVToTable(fiCsvFile.FullName, true);
            if ((dt == null) || (dt.Rows.Count == 0)) return null;

            dt = DataTableTools.SortTable(dt, AcousticIndicesExtraction.header_count + " ASC");

            double[] weightedIndices = null;
            if (addColumnOfweightedIndices)
            {
                double[] comboWts = AcousticIndicesExtraction.GetComboWeights();
                weightedIndices = AcousticIndicesExtraction.GetArrayOfWeightedAcousticIndices(dt, comboWts);
                string colName = "WeightedIndex";
                displayHeaders.Add(colName);
                DataTableTools.AddColumn2Table(dt, colName, weightedIndices);
            }

            DataTable table2Display = ProcessDataTableForDisplayOfColumnValues(dt, displayHeaders);
            return System.Tuple.Create(dt, table2Display);
        } // ProcessCsvFile()



        /// <summary>
        /// takes a data table of indices and converts column values to values in [0,1].
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DataTable ProcessDataTableForDisplayOfColumnValues(DataTable dt, List<string> headers2Display)
        {
            string[] headers = DataTableTools.GetColumnNames(dt);
            List<string> originalHeaderList = headers.ToList();
            List<string> newHeaders = new List<string>();

            List<double[]> newColumns = new List<double[]>();
            // double[] processedColumn = null;

            for (int i = 0; i < headers2Display.Count; i++)
            {
                string header = headers2Display[i];
                if (!originalHeaderList.Contains(header)) continue;

                List<double> values = DataTableTools.Column2ListOfDouble(dt, header); //get list of values
                if ((values == null) || (values.Count == 0)) continue;

                double min = 0;
                double max = 1;
                if (header.Equals(AcousticIndicesExtraction.header_count))
                {
                    newColumns.Add(DataTools.normalise(values.ToArray())); //normalise all values in [0,1]
                    newHeaders.Add(header);
                }
                else if (header.Equals(AcousticIndicesExtraction.header_avAmpdB))
                {
                    min = -50;
                    max = -5;
                    newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                    newHeaders.Add(AcousticIndicesExtraction.header_avAmpdB + "  (-50..-5dB)");
                }
                else if (header.Equals(AcousticIndicesExtraction.header_snrdB))
                {
                    min = 5;
                    max = 50;
                    newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                    newHeaders.Add(AcousticIndicesExtraction.header_snrdB + "  (5..50dB)");
                }
                else if (header.Equals(AcousticIndicesExtraction.header_avSegDur))
                {
                    min = 0.0;
                    max = 500.0; //av segment duration in milliseconds
                    newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                    newHeaders.Add(AcousticIndicesExtraction.header_avSegDur + "  (0..500ms)");
                }
                else if (header.Equals(AcousticIndicesExtraction.header_bgdB))
                {
                    min = -50;
                    max = -5;
                    newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                    newHeaders.Add(AcousticIndicesExtraction.header_bgdB + "  (-50..-5dB)");
                }
                else if (header.Equals(AcousticIndicesExtraction.header_avClustDur))
                {
                    min = 50.0; //note: minimum cluster length = two frames = 2*frameDuration
                    max = 200.0; //av segment duration in milliseconds
                    newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                    newHeaders.Add(AcousticIndicesExtraction.header_avClustDur + "  (50..200ms)");
                }
                else if (header.Equals(AcousticIndicesExtraction.header_lfCover))
                {
                    min = 0.1; //
                    max = 1.0; //
                    newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                    newHeaders.Add(AcousticIndicesExtraction.header_lfCover + "  (10..100%)");
                }
                else if (header.Equals(AcousticIndicesExtraction.header_mfCover))
                {
                    min = 0.0; //
                    max = 0.9; //
                    newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                    newHeaders.Add(AcousticIndicesExtraction.header_mfCover + "  (0..90%)");
                }
                else if (header.Equals(AcousticIndicesExtraction.header_hfCover))
                {
                    min = 0.0; //
                    max = 0.9; //
                    newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                    newHeaders.Add(AcousticIndicesExtraction.header_hfCover + "  (0..90%)");
                }
                else if (header.Equals(AcousticIndicesExtraction.header_HAmpl))
                {
                    min = 0.5; //
                    max = 1.0; //
                    newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                    newHeaders.Add(AcousticIndicesExtraction.header_HAmpl + "  (0.5..1.0)");
                }
                else if (header.Equals(AcousticIndicesExtraction.header_HAvSpectrum))
                {
                    min = 0.2; //
                    max = 1.0; //
                    newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                    newHeaders.Add(AcousticIndicesExtraction.header_HAvSpectrum + "  (0.2..1.0)");
                }
                else //default is to normalise in [0,1]
                {
                    newColumns.Add(DataTools.normalise(values.ToArray())); //normalise all values in [0,1]
                    newHeaders.Add(header);
                }
            }

            //convert type int to type double due to normalisation
            Type[] types = new Type[newHeaders.Count];
            for (int i = 0; i < newHeaders.Count; i++) types[i] = typeof(double);
            var processedtable = DataTableTools.CreateTable(newHeaders.ToArray(), types, newColumns);

            return processedtable;
        }


        public DataTable ConvertEvents2Indices(DataTable dt, TimeSpan unitTime, TimeSpan timeDuration, double scoreThreshold)
        {
            return null;
        }



        public string DefaultConfiguration
        {
            get
            {
                return string.Empty;
            }
        }



    } //end class AcousticIndices
}
