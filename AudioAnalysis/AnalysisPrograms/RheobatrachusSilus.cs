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
    public class RheobatrachusSilus : IAnalyser
    {
        //KEYS TO PARAMETERS IN CONFIG FILE
        public static string key_ANALYSIS_NAME = "ANALYSIS_NAME";
        public static string key_CALL_DURATION = "CALL_DURATION";
        public static string key_DECIBEL_THRESHOLD = "DECIBEL_THRESHOLD";
        public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_INTENSITY_THRESHOLD = "INTENSITY_THRESHOLD";
        public static string key_SEGMENT_DURATION = "SEGMENT_DURATION";
        public static string key_SEGMENT_OVERLAP = "SEGMENT_OVERLAP";
        public static string key_RESAMPLE_RATE = "RESAMPLE_RATE";
        public static string key_FRAME_LENGTH = "FRAME_LENGTH";
        public static string key_FRAME_OVERLAP = "FRAME_OVERLAP";
        public static string key_NOISE_REDUCTION_TYPE = "NOISE_REDUCTION_TYPE";
        public static string key_UPPERFREQBAND_TOP = "UPPERFREQBAND_TOP";
        public static string key_UPPERFREQBAND_BTM = "UPPERFREQBAND_BTM";
        public static string key_LOWERFREQBAND_TOP = "LOWERFREQBAND_TOP";
        public static string key_LOWERFREQBAND_BTM = "LOWERFREQBAND_BTM";
        public static string key_MIN_AMPLITUDE  = "MIN_AMPLITUDE";
        public static string key_MIN_DURATION   = "MIN_DURATION";
        public static string key_MAX_DURATION   = "MAX_DURATION";
        public static string key_MIN_PERIOD     = "MIN_PERIOD";
        public static string key_MAX_PERIOD     = "MAX_PERIOD";
        public static string key_DRAW_SONOGRAMS = "DRAW_SONOGRAMS";

        //KEYS TO OUTPUT EVENTS and INDICES
        public static string key_COUNT     = "count";
        public static string key_SEGMENT_TIMESPAN = "SegTimeSpan";
        public static string key_START_ABS = "EvStartAbs";
        public static string key_START_MIN = "EvStartMin";
        public static string key_START_SEC = "EvStartSec";
        public static string key_CALL_DENSITY = "CallDensity";
        public static string key_CALL_SCORE = "CallScore";
        public static string key_EVENT_TOTAL= "# events";


        //OTHER CONSTANTS
        public const string ANALYSIS_NAME = "RheobatrachusSilus";
        //public const int RESAMPLE_RATE = 17640;
        public const int RESAMPLE_RATE = 22050;
        //public const string imageViewer = @"C:\Program Files\Windows Photo Viewer\ImagingDevices.exe";
        public const string imageViewer = @"C:\Windows\system32\mspaint.exe";


        public string DisplayName
        {
            get { return "Gastric Broooding Frog - Rheobatrachus"; }
        }

        private static string identifier = "Towsey." + ANALYSIS_NAME;
        public string Identifier
        {
            get { return identifier; }
        }

        public static void Dev(string[] args)
        {
            Log.Verbosity = 1;
            bool debug = false;
#if DEBUG
            debug = true;
#endif

            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Rheobatrachus_silus_MONO.wav";  //POSITIVE
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Adelotus_brevis_TuskedFrog_BridgeCreek.wav";   //NEGATIVE walking on dry leaves
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min646.wav";   //NEGATIVE  rain
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min599.wav";   //NEGATIVE  rain
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min602.wav";   //NEGATIVE  rain
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Noise\BAC3_20070924-153657_noise.wav";  //NEGATIVE  noise
            string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Compilation6_Mono.mp3";  //FROG COMPILATION
            string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.RheobatrachusSilus.cfg";
            string outputDir     = @"C:\SensorNetworks\Output\Frogs\";
            //COMMAND LINE
            //AnalysisPrograms.exe Rheobatrachus "C:\SensorNetworks\WavFiles\Frogs\Rheobatrachus_silus_MONO.wav" C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.RheobatrachusSilus.cfg" "C:\SensorNetworks\Output\Frogs\"

            string title = "# FOR DETECTION OF 'Rheobatrachus silus' using CROSS-CORRELATION & FFT";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(title);
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Output folder:  " + outputDir);
            LoggedConsole.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            var diOutputDir = new DirectoryInfo(outputDir);



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
            cmdLineArgs.Add("-tmpwav:"   + segmentFName);
            cmdLineArgs.Add("-events:"   + eventsFname);
            cmdLineArgs.Add("-indices:"  + indicesFname);
            cmdLineArgs.Add("-sgram:"    + sonogramFname);
            cmdLineArgs.Add("-start:"    + tsStart.TotalSeconds);
            cmdLineArgs.Add("-duration:" + tsDuration.TotalSeconds);
            
            //#############################################################################################################################################
            int status = Execute(cmdLineArgs.ToArray());
            if (status != 0)
            {
                LoggedConsole.WriteLine("\n\n# EXECUTE RETURNED ERROR STATUS. CANNOT PROCEED!");

                if (debug)
                {
                    Console.ReadLine();
                    //System.Environment.Exit(99);
                }
                return;
            }
            //#############################################################################################################################################


            string eventsPath = Path.Combine(outputDir, eventsFname);
            FileInfo fiCsvEvents = new FileInfo(eventsPath);
            if (! fiCsvEvents.Exists)
            {
                Log.WriteLine("\n\n\n############\n WARNING! Events CSV file not returned from analysis of minute {0} of file <{0}>.", startMinute, recordingPath);
            }
            else
            {
                LoggedConsole.WriteLine("\n");
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
                LoggedConsole.WriteLine("\n");
                DataTable dt = CsvTools.ReadCSVToTable(indicesPath, true);
                DataTableTools.WriteTable2Console(dt);
            }
            string imagePath = Path.Combine(outputDir, sonogramFname);
            FileInfo fiImage = new FileInfo(imagePath);
            if (fiImage.Exists)
            {
                TowseyLib.ProcessRunner process = new TowseyLib.ProcessRunner(imageViewer);
                process.Run(imagePath, outputDir);
            }

            LoggedConsole.WriteLine("\n##### FINISHED FILE ###################################################\n");
            return;
        } //Dev()



        /// <summary>
        /// A WRAPPER AROUND THE Analysis() METHOD
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
                LoggedConsole.WriteLine("Require at least 4 command line arguments.");
                status = 1;
                return status;
            }
            //GET FIRST THREE OBLIGATORY COMMAND LINE ARGUMENTS
            string recordingPath = args[0];
            string configPath = args[1];
            string outputDir = args[2];
           
            // INIT SETTINGS
            AnalysisSettings analysisSettings = new AnalysisSettings();
            analysisSettings.ConfigFile       = new FileInfo(configPath);
            analysisSettings.AnalysisRunDirectory = new DirectoryInfo(outputDir); 
            analysisSettings.AudioFile   = null;
            analysisSettings.EventsFile  = null;
            analysisSettings.IndicesFile = null;
            analysisSettings.ImageFile   = null;
            TimeSpan tsStart    = new TimeSpan(0, 0, 0);
            TimeSpan tsDuration = new TimeSpan(0, 0, 0);
            var configuration = new ConfigDictionary(analysisSettings.ConfigFile.FullName);
            analysisSettings.ConfigDict = configuration.GetTable();

            //PROCESS REMAINDER OF COMMAND LINE ARGUMENTS
            for (int i = 3; i < args.Length; i++)
            {
                string[] parts = args[i].Split(':');
                if (parts[0].StartsWith("-tmpwav"))
                {
                    var outputWavPath   = Path.Combine(outputDir, parts[1]);
                    analysisSettings.AudioFile  = new FileInfo(outputWavPath);
                } else
                if (parts[0].StartsWith("-events"))
                {
                    string eventsPath = Path.Combine(outputDir, parts[1]);
                    analysisSettings.EventsFile = new FileInfo(eventsPath);
                } else
                if (parts[0].StartsWith("-indices"))
                {
                    string indicesPath = Path.Combine(outputDir, parts[1]);
                    analysisSettings.IndicesFile = new FileInfo(indicesPath);
                } else
                if (parts[0].StartsWith("-sgram"))
                {
                    string sonoImagePath = Path.Combine(outputDir, parts[1]);
                    analysisSettings.ImageFile = new FileInfo(sonoImagePath);
                } else
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
                        LoggedConsole.WriteLine("Segment duration cannot exceed 10 minutes.");
                        status = 3;
                        return status;
                    }
                }
            }

            //EXTRACT THE REQUIRED RECORDING SEGMENT
            FileInfo sourceF = new FileInfo(recordingPath);
            FileInfo tempF   = analysisSettings.AudioFile;
            if (tempF.Exists) tempF.Delete();
            if (tsDuration.TotalSeconds == 0)   //Process entire file
            {
                AudioFilePreparer.PrepareFile(sourceF, tempF, new AudioUtilityRequest { TargetSampleRate = RESAMPLE_RATE }, analysisSettings.AnalysisTempBaseDirectoryChecked);
                //var fiSegment = AudioFilePreparer.PrepareFile(diOutputDir, fiSourceFile, , Human2.RESAMPLE_RATE);
            }
            else
            {
                AudioFilePreparer.PrepareFile(sourceF, tempF, new AudioUtilityRequest { TargetSampleRate = RESAMPLE_RATE, OffsetStart = tsStart, OffsetEnd = tsStart.Add(tsDuration) }, analysisSettings.AnalysisTempBaseDirectoryChecked);
                //var fiSegmentOfSourceFile = AudioFilePreparer.PrepareFile(diOutputDir, new FileInfo(recordingPath), MediaTypes.MediaTypeWav, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(3), RESAMPLE_RATE);
            }

            //DO THE ANALYSIS
            //#############################################################################################################################################
            IAnalyser analyser = new RheobatrachusSilus();
            AnalysisResult result = analyser.Analyse(analysisSettings);
            DataTable dt = result.Data;
            if (dt == null) return 6;
            //#############################################################################################################################################

            //ADD IN ADDITIONAL INFO TO TABLE
            AddContext2Table(dt, tsStart, tsDuration);
            CsvTools.DataTable2CSV(dt, analysisSettings.EventsFile.FullName);
            //DataTableTools.WriteTable(dt);

            return status;
        } //Execute()




        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            var fiAudioF    = analysisSettings.AudioFile;
            var diOutputDir = analysisSettings.AnalysisRunDirectory;

            var result = new AnalysisResult();
            result.AnalysisIdentifier = this.Identifier;
            result.SettingsUsed = analysisSettings;
            result.Data = null;

            //######################################################################
            var results = Analysis(fiAudioF, analysisSettings.ConfigDict);
            //######################################################################

            if (results == null) return result; //nothing to process 
            var sonogram = results.Item1;
            var hits = results.Item2;
            var scores = results.Item3;
            var predictedEvents = results.Item4;
            var recordingTimeSpan = results.Item5;

            DataTable dataTable = null;

            if (predictedEvents != null)
            {
                string analysisName = analysisSettings.ConfigDict[AudioAnalysisTools.Keys.ANALYSIS_NAME];
                string fName = Path.GetFileNameWithoutExtension(fiAudioF.Name);
                foreach (AcousticEvent ev in predictedEvents)
                {
                    ev.SourceFileName = fName;
                    ev.Name = analysisName;
                    ev.SourceFileDuration = recordingTimeSpan.TotalSeconds;
                }
                //write events to a data table to return.
                dataTable = WriteEvents2DataTable(predictedEvents);
                string sortString = Keys.EVENT_START_ABS + " ASC";
                dataTable = DataTableTools.SortTable(dataTable, sortString); //sort by start time before returning
            }

            if ((analysisSettings.EventsFile != null) && (dataTable != null))
            {
                CsvTools.DataTable2CSV(dataTable, analysisSettings.EventsFile.FullName);
            }
            else
                result.EventsFile = null;

            if ((analysisSettings.IndicesFile != null) && (dataTable != null))
            {
                double scoreThreshold = 0.01;
                if (analysisSettings.ConfigDict.ContainsKey(Keys.INTENSITY_THRESHOLD))
                    scoreThreshold = ConfigDictionary.GetDouble(Keys.INTENSITY_THRESHOLD, analysisSettings.ConfigDict);
                TimeSpan unitTime = TimeSpan.FromSeconds(60); //index for each time span of i minute
                var indicesDT = ConvertEvents2Indices(dataTable, unitTime, recordingTimeSpan, scoreThreshold);
                CsvTools.DataTable2CSV(indicesDT, analysisSettings.IndicesFile.FullName);
            }
            else
                result.IndicesFile = null;

            //save image of sonograms
            if (analysisSettings.ImageFile != null)
            {
                string imagePath = analysisSettings.ImageFile.FullName;
                Image image = DrawSonogram(sonogram, hits, scores, predictedEvents);
                image.Save(imagePath, ImageFormat.Png);
            }

            result.Data = dataTable;
            result.ImageFile = analysisSettings.ImageFile;
            result.AudioDuration = recordingTimeSpan;
            //result.DisplayItems = { { 0, "example" }, { 1, "example 2" }, }
            //result.OutputFiles = { { "exmaple file key", new FileInfo("Where's that file?") } }
            return result;
        } //Analyse()




        /// <summary>
        /// ################ THE KEY ANALYSIS METHOD
        /// Returns a DataTable
        /// </summary>
        /// <param name="fiSegmentOfSourceFile"></param>
        /// <param name="configDict"></param>
        /// <param name="diOutputDir"></param>
        public static System.Tuple<BaseSonogram, Double[,], List<Plot>, List<AcousticEvent>, TimeSpan>
                                                                                   Analysis(FileInfo fiSegmentOfSourceFile, Dictionary<string, string> configDict)
        {
            //set default values - ignore those set by user
            int frameSize        = 128;
            double windowOverlap = 0.5;

            //int upperBandMinHz = Int32.Parse(configDict[key_UPPERFREQBAND_BTM]);
            //int upperBandMaxHz = Int32.Parse(configDict[key_UPPERFREQBAND_TOP]);
            //int lowerBandMinHz = Int32.Parse(configDict[key_LOWERFREQBAND_BTM]);
            //int lowerBandMaxHz = Int32.Parse(configDict[key_LOWERFREQBAND_TOP]);
            //double decibelThreshold = Double.Parse(configDict[key_DECIBEL_THRESHOLD]); ;   //dB
            double intensityThreshold = Double.Parse(configDict[key_INTENSITY_THRESHOLD]); //in 0-1
            double minDuration = Double.Parse(configDict[key_MIN_DURATION]);  // seconds
            double maxDuration = Double.Parse(configDict[key_MAX_DURATION]);  // seconds
            double minPeriod   = Double.Parse(configDict[key_MIN_PERIOD]);      // seconds
            double maxPeriod   = Double.Parse(configDict[key_MAX_PERIOD]);      // seconds

            AudioRecording recording = new AudioRecording(fiSegmentOfSourceFile.FullName);
            if (recording == null)
            {
                LoggedConsole.WriteLine("AudioRecording == null. Analysis not possible.");
                return null;
            }

            //i: MAKE SONOGRAM
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.WindowSize = frameSize;
            sonoConfig.WindowOverlap = windowOverlap;
            //sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("NONE");
            sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("STANDARD");
            TimeSpan tsRecordingtDuration = recording.Duration();
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;
            double frameOffset = sonoConfig.GetFrameOffset(sr);
            double framesPerSecond = 1 / frameOffset;

            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);
            recording.Dispose();

            //#############################################################################################################################################
            //window    sr          frameDuration   frames/sec  hz/bin  64frameDuration hz/64bins       hz/128bins
            // 1024     22050       46.4ms          21.5        21.5    2944ms          1376hz          2752hz
            // 256      17640       14.5ms          68.9        68.9    ms          hz          hz
            // 512      17640       29.0ms          34.4        34.4    ms          hz          hz
            // 1024     17640       58.0ms          17.2        17.2    3715ms          1100hz          2200hz
            // 2048     17640       116.1ms          8.6         8.6    7430ms           551hz          1100hz

            //The Xcorrelation-FFT technique requires number of bins to scan to be power of 2.
            // Assuming sr=17640 and window=256, then binWidth = 68.9Hz and 1500Hz = bin 21.7..
            // Therefore do a Xcorrelation between bins 21 and 22.
            // Number of frames to span must power of 2. Try 16 frames which covers 232ms - almost 1/4 second.

            int midHz    = 1500;
            int lowerBin = (int)(midHz / freqBinWidth) + 1;  //because bin[0] = DC
            int upperBin = lowerBin + 4;
            int lowerHz = (int)Math.Floor((lowerBin-1) * freqBinWidth);
            int upperHz = (int)Math.Ceiling((upperBin - 1) * freqBinWidth); ;

            //ALTERNATIVE IS TO USE THE AMPLITUDE SPECTRUM
            //var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, sr, frameSize, windowOverlap);
            //double[,] matrix = results2.Item3;  //amplitude spectrogram. Note that column zero is the DC or average energy value and can be ignored.
            //double[] avAbsolute = results2.Item1; //average absolute value over the minute recording
            ////double[] envelope = results2.Item2;
            //double windowPower = results2.Item4;

            double[] lowerArray = MatrixTools.GetColumn(sonogram.Data, lowerBin);
            double[] upperArray = MatrixTools.GetColumn(sonogram.Data, upperBin);
            //upperArray = lowerArray;
            lowerArray = DataTools.NormaliseInZeroOne(lowerArray, 0, 60); //## ABSOLUTE NORMALISATION 0-60 dB #######################################################################
            upperArray = DataTools.NormaliseInZeroOne(upperArray, 0, 60); //## ABSOLUTE NORMALISATION 0-60 dB #######################################################################

            int step = (int)(framesPerSecond / 40); //take one/tenth second steps
            int stepCount = rowCount / step;
            int sampleLength = 32; //16 frames = 232ms - almost 1/4 second.
            double[] intensity   = new double[rowCount];
            double[] periodicity = new double[rowCount]; 

            //######################################################################
            //ii: DO THE ANALYSIS AND RECOVER SCORES

            for (int i = 0; i < stepCount; i++)
            {
                int start = step * i;
                double[] lowerSubarray = DataTools.Subarray(lowerArray, start, sampleLength);
                double[] upperSubarray = DataTools.Subarray(upperArray, start, sampleLength);
                if ((lowerSubarray == null) || (upperSubarray == null)) break;
                if ((lowerSubarray.Length != sampleLength) || (upperSubarray.Length != sampleLength)) break;
                var spectrum = CrossCorrelation.CrossCorr(lowerSubarray, upperSubarray);
                //DataTools.writeBarGraph(spectrum);
                int zeroCount = 2;
                for (int s = 0; s < zeroCount; s++) spectrum[s] = 0.0;  //in real data these bins are dominant and hide other frequency content
                //spectrum = DataTools.NormaliseArea(spectrum);
                int maxId = DataTools.GetMaxIndex(spectrum);
                double period = 2 * sampleLength / (double)maxId / framesPerSecond; //convert maxID to period in seconds
                if ((period < minPeriod) || (period > maxPeriod)) continue;
                for (int j = 0; j < sampleLength; j++) //lay down score for sample length
                {
                    if (intensity[start + j] < spectrum[maxId]) intensity[start + j] = spectrum[maxId];
                    periodicity[start + j] = period;
                }
            }

            //iii: CONVERT SCORES TO ACOUSTIC EVENTS
            intensity = DataTools.filterMovingAverage(intensity, 3);
            intensity = DataTools.NormaliseInZeroOne(intensity, 0, 0.5); //## ABSOLUTE NORMALISATION 0-0.5 #######################################################################

            List<AcousticEvent> predictedEvents = AcousticEvent.ConvertScoreArray2Events(intensity, lowerHz, upperHz, sonogram.FramesPerSecond, freqBinWidth,
                                                                                         intensityThreshold, minDuration, maxDuration);
            CropEvents(predictedEvents, upperArray);
            var hits = new double[rowCount, colCount];

            var plots = new List<Plot>();
            //plots.Add(new Plot("lowerArray", DataTools.Normalise(lowerArray, 0, 100), 10.0));
            //plots.Add(new Plot("lowerArray", DataTools.Normalise(lowerArray, 0, 100), 10.0));
            //plots.Add(new Plot("lowerArray", DataTools.normalise(lowerArray), 0.25));
            //plots.Add(new Plot("upperArray", DataTools.normalise(upperArray), 0.25));
            //plots.Add(new Plot("intensity",  DataTools.normalise(intensity), intensityThreshold));
            plots.Add(new Plot("intensity", intensity, intensityThreshold));

            return System.Tuple.Create(sonogram, hits, plots, predictedEvents, tsRecordingtDuration);
        } //Analysis()

        public static void CropEvents(List<AcousticEvent> events, double[] intensity)
        {
            double severity = 0.1;
            int length = intensity.Length;

            foreach (AcousticEvent ev in events)
            {
                int start = ev.oblong.r1;
                int end   = ev.oblong.r2;
                double[] subArray = DataTools.Subarray(intensity, start, end-start+1);
                int[] bounds = DataTools.Peaks_CropLowAmplitude(subArray, severity);

                int newMinRow = start + bounds[0];
                int newMaxRow = start + bounds[1];
                if (newMaxRow >= length) newMaxRow = length - 1;

                Oblong o = new Oblong(newMinRow, ev.oblong.c1, newMaxRow, ev.oblong.c2);
                ev.oblong = o;
                ev.TimeStart = newMinRow * ev.FrameOffset;
                ev.TimeEnd   = newMaxRow * ev.FrameOffset;
            }
        }


        static Image DrawSonogram(BaseSonogram sonogram, double[,] hits, List<Plot> plots, List<AcousticEvent> predictedEvents)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            //int maxFreq = sonogram.NyquistFrequency / 2;
            int maxFreq = 6000;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(maxFreq, 1, doHighlightSubband, add1kHzLines));

            //System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            //img.Save(@"C:\SensorNetworks\temp\testimage1.png", System.Drawing.Imaging.ImageFormat.Png);

            //Image_MultiTrack image = new Image_MultiTrack(img);
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            if (plots != null)
                foreach (Plot plot in plots) image.AddTrack(Image_Track.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title));
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
                                 //AudioAnalysisTools.Keys.EVENT_INTENSITY,
                                 AudioAnalysisTools.Keys.EVENT_NAME,
                                 AudioAnalysisTools.Keys.EVENT_SCORE,
                                 AudioAnalysisTools.Keys.EVENT_NORMSCORE 

                               };
            //                   1                2               3              4                5              6               7              8
            Type[] types = { typeof(int), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), /*typeof(double), */ typeof(string), 
                             typeof(double), typeof(double) };

            var dataTable = DataTableTools.CreateTable(headers, types);
            if (predictedEvents.Count == 0) return dataTable;

            foreach (var ev in predictedEvents)
            {
                DataRow row = dataTable.NewRow();
                row[AudioAnalysisTools.Keys.EVENT_START_ABS] = (double)ev.TimeStart;  //Set now - will overwrite later
                row[AudioAnalysisTools.Keys.EVENT_START_SEC] = (double)ev.TimeStart;  //EvStartSec
                row[AudioAnalysisTools.Keys.EVENT_DURATION] = (double)ev.Duration;   //duration in seconds
                //row[AudioAnalysisTools.Keys.EVENT_INTENSITY] = (double)ev.kiwi_intensityScore;   //
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
            Type[] types = { typeof(int), typeof(int), typeof(int) };
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

    } //end class RheobatrachusSilus
}
