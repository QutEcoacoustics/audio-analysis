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
    public class Frogs : IAnalyser
    {
        
        public static string key_FROG_DATA = "FROG_DATA_FILE";


        //OTHER CONSTANTS
        public const string ANALYSIS_NAME = "Frogs";
        public const int RESAMPLE_RATE = 17640;
        //public const int RESAMPLE_RATE = 22050;
        //public const string imageViewer = @"C:\Program Files\Windows Photo Viewer\ImagingDevices.exe";
        public const string imageViewer = @"C:\Windows\system32\mspaint.exe";


        public string DisplayName
        {
            get { return ANALYSIS_NAME; }
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
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Adelotus_brevis_TuskedFrog_BridgeCreek.wav";   // NEGATIVE walking on dry leaves
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min646.wav";   //NEGATIVE  rain
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min599.wav";   //NEGATIVE  rain
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min602.wav";   //NEGATIVE  rain
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Noise\BAC3_20070924-153657_noise.wav";               // NEGATIVE  noise
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\FrogPond_Samford_SE_555_20101023-000000.mp3";  // FROGs AT SAMFORD
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Crinia_signifera_july08.wav";                  // Crinia signifera
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Frogs_BridgeCreek_Night_Extract1-31-00.mp3";   // FROGs at Bridgecreek

            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Compilation6_Mono.mp3";                          // FROG COMPILATION
            string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Curramore\CorramoreSelection-mono16kHz.mp3";       // Curramore COMPILATION


            string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Frogs.cfg";
            string outputDir  = @"C:\SensorNetworks\Output\Frogs\";
            //COMMAND LINE
            //AnalysisPrograms.exe Rheobatrachus "C:\SensorNetworks\WavFiles\Frogs\Rheobatrachus_silus_MONO.wav" C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.RheobatrachusSilus.cfg" "C:\SensorNetworks\Output\Frogs\"

            string title = "# FOR DETECTION OF 'FROG SPECIES' ";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(title);
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Output folder:  " + outputDir);
            LoggedConsole.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            var diOutputDir = new DirectoryInfo(outputDir);



            int startMinute = 0;
            //int startMinute = 1;
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
            Execute(cmdLineArgs.ToArray());
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
                ProcessRunner process = new ProcessRunner(imageViewer);
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
        public static void Execute(string[] args)
        {
            if (args.Length < 4)
            {
                LoggedConsole.WriteLine("You require at least 4 command line arguments after the analysis option.");
                Usage();
                throw new AnalysisOptionInvalidArgumentsException();
            }

            // GET FIRST THREE OBLIGATORY COMMAND LINE ARGUMENTS
            string recordingPath = args[0];
            string configPath    = args[1];
            string outputDir     = args[2];
           
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

            //get the data file to identify frog calls. Check it exists and then store full path in dictionary. 
            string frogParametersPath = analysisSettings.ConfigDict[key_FROG_DATA];
            FileInfo fi_Frog = new FileInfo(Path.Combine(Path.GetDirectoryName(configPath), frogParametersPath));
            if (!fi_Frog.Exists)
            {
                LoggedConsole.WriteLine("INVALID PATH: " + fi_Frog.FullName);
                LoggedConsole.WriteLine("The config file must contain the name of a valid .csv file (containing frog call parameters) located in same directory as the .cfg file.");
                LoggedConsole.WriteLine("For example, use Key/Value pair:  FROG_DATA_FILE=FrogDataAndCompilationFile.csv");
                throw new AnalysisOptionInvalidPathsException();
            }
            analysisSettings.ConfigDict[key_FROG_DATA] = fi_Frog.FullName; // store full path in the dictionary.


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
                    int s = int.Parse(parts[1]);
                    tsDuration = new TimeSpan(0, 0, s);
                    if (tsDuration.TotalMinutes > 10)
                    {
                        LoggedConsole.WriteLine("Segment duration cannot exceed 10 minutes.");

                        throw new AnalysisOptionInvalidDurationException();
                    }
                }
            }

            //EXTRACT THE REQUIRED RECORDING SEGMENT
            FileInfo sourceF = new FileInfo(recordingPath);
            FileInfo tempF   = analysisSettings.AudioFile;
            if (tempF.Exists) tempF.Delete();
            if (tsDuration.TotalSeconds == 0)   //Process entire file
            {
                AudioFilePreparer.PrepareFile(sourceF, tempF, new AudioUtilityRequest { SampleRate = RESAMPLE_RATE });
                //var fiSegment = AudioFilePreparer.PrepareFile(diOutputDir, fiSourceFile, , Human2.RESAMPLE_RATE);
            }
            else
            {
                AudioFilePreparer.PrepareFile(sourceF, tempF,new AudioUtilityRequest {SampleRate = RESAMPLE_RATE, OffsetStart = tsStart, OffsetEnd = tsStart.Add(tsDuration)});
                //var fiSegmentOfSourceFile = AudioFilePreparer.PrepareFile(diOutputDir, new FileInfo(recordingPath), MediaTypes.MediaTypeWav, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(3), RESAMPLE_RATE);
            }

            //DO THE ANALYSIS
            //#############################################################################################################################################
            IAnalyser analyser = new Frogs();
            AnalysisResult result = analyser.Analyse(analysisSettings);
            DataTable dt = result.Data;
            //#############################################################################################################################################

            //ADD IN ADDITIONAL INFO TO TABLE
            if (dt != null)
            {
                AddContext2Table(dt, tsStart, result.AudioDuration);
                CsvTools.DataTable2CSV(dt, analysisSettings.EventsFile.FullName);
                //DataTableTools.WriteTable(augmentedTable);
            }
            else
            {
                throw new InvalidOperationException("Data table is null");
            }
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
                    //ev.Name = analysisName; //TEMPORARY DISABLE
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
            int frameSize = 32;
            double windowOverlap   = 0.3;
            int xCorrelationLength = 256;   //for Xcorrelation   - 256 frames @801 = 320ms, almost 1/3 second.
            //int xCorrelationLength = 128;   //for Xcorrelation   - 128 frames @801 = 160ms, almost 1/6 second.
            //int xCorrelationLength = 64;   //for Xcorrelation   - 64 frames @128 = 232ms, almost 1/4 second.
            //int xCorrelationLength = 16;   //for Xcorrelation   - 16 frames @128 = 232ms, almost 1/4 second.
            double dBThreshold = 12.0;

            // read frog data to datatable
            var dt = CsvTools.ReadCSVToTable(configDict[key_FROG_DATA], true); // read file contining parameters of frog calls to a table



            double intensityThreshold = Double.Parse(configDict[Keys.INTENSITY_THRESHOLD]); //in 0-1
            double minDuration = Double.Parse(configDict[Keys.MIN_DURATION]);     // seconds
            double maxDuration = Double.Parse(configDict[Keys.MAX_DURATION]);     // seconds
            double minPeriod   = Double.Parse(configDict[Keys.MIN_PERIODICITY]);  // seconds
            double maxPeriod   = Double.Parse(configDict[Keys.MAX_PERIODICITY]);  // seconds

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
            sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("STANDARD");   //must do noise removal
            TimeSpan tsRecordingtDuration = recording.Duration();
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;
            double frameOffset = sonoConfig.GetFrameOffset(sr);
            double framesPerSecond = 1 / frameOffset;

            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            recording.Dispose();

            //var dBArray = sonogram.DecibelsPerFrame;
            var peaks = DataTools.GetPeakValues(sonogram.DecibelsPerFrame);

            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);

            int nhLimit = 3; //limit of neighbourhood around maximum
            var maxFreqArray = new int[rowCount]; //array (one element per frame) indicating which freq bin has max amplitude.
            var hitsMatrix   = new double[rowCount, colCount];
            for (int r = nhLimit; r < rowCount - nhLimit; r++)
            {
                if (peaks[r] < dBThreshold) continue;
                //find local freq maxima and store in freqArray & hits matrix.
                for (int nh = -nhLimit; nh < nhLimit; nh++)
                {
                    double[] spectrum = MatrixTools.GetRow(sonogram.Data, r+nh);
                    spectrum[0] = 0.0; // set DC = 0.0 just in case it is max.
                    int maxFreqbin = DataTools.GetMaxIndex(spectrum);
                    if (spectrum[maxFreqbin] > dBThreshold) //only record spectral peak if it is above threshold.
                    {
                        maxFreqArray[r + nh] = maxFreqbin;
                        //if ((spectrum[maxFreqbin] > dBThreshold) && (sonogram.Data[r, maxFreqbin] >= sonogram.Data[r - 1, maxFreqbin]) && (sonogram.Data[r, maxFreqbin] >= sonogram.Data[r + 1, maxFreqbin]))
                        hitsMatrix[r + nh, maxFreqbin] = 1.0;
                    }
                }
            }

            //iii: GET TRACKS
            var tracks = SpectralTrack.GetSpectraltracks(maxFreqArray, framesPerSecond, freqBinWidth);

            //double threshold = 9.0;
            double severity = 0.5;

            double dynamicRange = 60; // deciBels above background noise. BG noise has already been removed from each bin.
            // convert sonogram to a list of frequency bin arrays
            var listOfFrequencyBins = SonogramTools.Sonogram2ListOfFreqBinArrays(sonogram, dynamicRange);

            int minFrameLength = SpectralTrack.ConvertMilliseconds2FrameCount(SpectralTrack.MIN_TRACK_DURATION, framesPerSecond);
            for (int i = tracks.Count-1; i >= 0; i--)
            {
                tracks[i].CropTrack(listOfFrequencyBins, severity);
                //track.CropTrack(sonogram, threshold);
                if (tracks[i].Length < minFrameLength) tracks.Remove(tracks[i]);
            } // foreach track


            foreach (SpectralTrack track in tracks) // find any periodicity in the track and calculate its score.
            {
                SpectralTrack.DetectTrackPeriodicity(track, xCorrelationLength, listOfFrequencyBins, sonogram.FramesPerSecond);
            } // foreach track

            int topBin = SpectralTrack.UpperTrackBound(freqBinWidth);
            var plots = CreateScorePlots(tracks, rowCount, topBin);

            //iv: CONVERT TRACKS TO ACOUSTIC EVENTS
            List<AcousticEvent> frogEvents = SpectralTrack.ConvertTracks2Events(tracks);

            // v: GET FROG IDs
            //var frogEvents = new List<AcousticEvent>();
            foreach (AcousticEvent ae in frogEvents)
            {
                double oscRate = 1 / ae.Periodicity;
                // ae.DominantFreq
                // ae.Score
                // ae.Duration
                //ClassifyFrogEvent(ae);
                string[] names = ClassifyFrogEvent(ae.DominantFreq, oscRate, dt);
                ae.Name = names[0];
                ae.Name2 = names[1];
            }

            return System.Tuple.Create(sonogram, hitsMatrix, plots, frogEvents, tsRecordingtDuration);
        } //Analysis()



        public static string[] ClassifyFrogEvent(double freq, double oscRate, DataTable frogDataTable)
        {
            int rowCount = frogDataTable.Rows.Count;

            List<double> data = new List<double>();
            data.Add(freq);
            data.Add(oscRate);

            double[] probScore = new double[rowCount];

            for (int i = 0; i < rowCount; i++)
            {
                DataRow row = frogDataTable.Rows[i];

                List<double> targets = new List<double>();
                targets.Add((double)((int)row["DominantFreq-Hz"]));
                targets.Add((double)((int)row["OscRate-cyclesPerSec"]));


                probScore[i] = GetNaiveBayesScore(targets.ToArray(), data.ToArray());
            }

            int id = DataTools.GetMaxIndex(probScore);
            DataRow row1 = frogDataTable.Rows[id];
            string[] names = new string[2];
            names[0] = (string)row1["LatinName"];
            names[1] = (string)row1["CommonName"];

            return names;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double GetNaiveBayesScore(double[] targetMeans, double[] data)
        {
            double SD_FACTOR = 10.0; // used to estimate the SD of the distribution associated with a single feature value 
            double dataProb = 1.0;
            for (int i = 0; i < data.Length; i++)
            {
                double targetSD = targetMeans[i] / SD_FACTOR;
                double featureZScore = (data[i] - targetMeans[i]) / targetSD;
                double featureProb = NormalDist.zScore2pValue(Math.Abs(featureZScore));
                dataProb *= featureProb;
            }
            return dataProb;
        }



        public static void ClassifyFrogEvent(AcousticEvent ae)
        {
            double oscRate = 1 / ae.Periodicity;

            if ((ae.DominantFreq > 3350) && (ae.DominantFreq < 3550) && (oscRate > 160) && (oscRate < 190) && (ae.Score > 0.20) && (ae.Duration > 0.05) && (ae.Duration < 0.3))
            {
                ae.Name = "Assa darlingtoni";
            }
            else
            if ((ae.DominantFreq > 4600) && (ae.DominantFreq < 4900) && (oscRate > 15) && (oscRate < 25) && (ae.Score > 0.20) && (ae.Duration > 0.05) && (ae.Duration < 0.5))
            {
                ae.Name = "Crinia deserticola";
            }
            else
            if ((ae.DominantFreq > 2600) && (ae.DominantFreq < 2800) && (oscRate > 20) && (oscRate < 30) && (ae.Score > 0.50) && (ae.Duration > 0.05) && (ae.Duration < 0.5))
            {
                ae.Name = "Crinia signifera";
            }
            else
            if ((ae.DominantFreq > 2100) && (ae.DominantFreq < 2300) && (oscRate > 145) && (oscRate < 165) && (ae.Score > 0.50) && (ae.Duration > 0.5))
            {
                ae.Name = "Cyclorana brevipes";
            }
            else
            if ((ae.DominantFreq > 600) && (ae.DominantFreq < 700) && (oscRate > 10) && (oscRate < 14) && (ae.Score > 1.00) && (ae.Duration > 0.3))
            {
                    ae.Name = "Heleioporus australiacus";
            }
            else
            if ((ae.DominantFreq > 1350) && (ae.DominantFreq < 1650) && (oscRate > 40) && (oscRate < 80) && (ae.Score > 0.3) && (ae.Duration > 0.3))
            {
                ae.Name = "GBH"; // the oscillation rate of the GBF increases from slow to fast, 30 - 80
            }
            else
            if ((ae.DominantFreq > 650) && (ae.DominantFreq < 750) && (oscRate > 13) && (oscRate < 15) && (ae.Score > 1.00) && (ae.Duration > 0.3))
            {
                ae.Name = "Lechriodus fletcheri";
            }
            else
            if ((ae.DominantFreq > 1400) && (ae.DominantFreq < 1700) && (oscRate > 68) && (oscRate < 88) && (ae.Score > 0.15) && (ae.Duration > 0.1))
            {
                ae.Name = "Limnodynastes fletcheri";
            }
            else
            if ((ae.DominantFreq > 1600) && (ae.DominantFreq < 1900) && (oscRate > 14) && (oscRate < 20) && (ae.Score > 0.50) && (ae.Duration > 0.1) && (ae.Duration < 0.5))
            {
                ae.Name = "Limnodynastes tasmaniensis";
            }
            else
            if ((ae.DominantFreq > 1100) && (ae.DominantFreq < 1500) && (oscRate > 110) && (oscRate < 150) && (ae.Score > 0.3) && (ae.Duration > 0.2) && (ae.Duration < 1.0))
            {
                ae.Name = "Litoria aurea";
            }
            else
            if ((ae.DominantFreq > 2900) && (ae.DominantFreq < 3200) && (oscRate > 9) && (oscRate < 12) && (ae.Score > 1.00) && (ae.Duration > 0.2) && (ae.Duration < 1.5))
            {
                ae.Name = "Litoria brevipalmata";
            }
            else
            if ((ae.DominantFreq > 1500) && (ae.DominantFreq < 1700) && (oscRate > 35) && (oscRate < 55) && (ae.Score > 0.50) && (ae.Duration > 0.2) && (ae.Duration < 1.5))
            {
                ae.Name = "Litoria citropa";
            }
            else
            if ((ae.DominantFreq > 2700) && (ae.DominantFreq < 3000) && (oscRate > 90) && (oscRate < 120) && (ae.Score > 1.00) && (ae.Duration > 0.2) && (ae.Duration < 1.0))
            {
                ae.Name = "Litoria gracilenta";
            }
            else
            if ((ae.DominantFreq > 960) && (ae.DominantFreq < 1300) && (oscRate > 10) && (oscRate < 16) && (ae.Score > 1.00) && (ae.Duration > 0.1))
            {
                ae.Name = "Litoria lesueuri";
            }
            else
            if ((ae.DominantFreq > 2050) && (ae.DominantFreq < 2200) && (oscRate > 35) && (oscRate < 45) && (ae.Score > 1.00) && (ae.Duration > 0.1) && (ae.Duration < 0.5))
            {
                ae.Name = "Litoria littlejohni";
            }
            else
            if ((ae.DominantFreq > 2650) && (ae.DominantFreq < 2950) && (oscRate > 14) && (oscRate < 20) && (ae.Score > 1.00) && (ae.Duration > 0.1) && (ae.Duration < 1.5))
            {
                ae.Name = "Litoria olongburensis";
            }
            else
            if ((ae.DominantFreq > 1750) && (ae.DominantFreq < 2050) && (oscRate > 15) && (oscRate < 25) && (ae.Score > 1.00) && (ae.Duration > 0.5) && (ae.Duration < 2.0))
            {
                ae.Name = "Litoria peronii";
            }
            else
            if ((ae.DominantFreq > 800) && (ae.DominantFreq < 1100) && (oscRate > 62) && (oscRate < 82) && (ae.Score > 0.50) && (ae.Duration > 0.3))
            {
                ae.Name = "Mixophyes fleayi";
            }
            else
            if ((ae.DominantFreq > 900) && (ae.DominantFreq < 1200) && (oscRate > 125) && (oscRate < 145) && (ae.Score >= 0.20) )
            {
                ae.Name = "Mixophyes fasciolatus";
            }
            else
            if ((ae.DominantFreq > 700) && (ae.DominantFreq < 800) && (oscRate > 80) && (oscRate < 90) && (ae.Score > 0.20) && (ae.Duration > 0.2))
            {
                ae.Name = "Mixophyes iteratus";
            }
            else
            if ((ae.DominantFreq > 1350) && (ae.DominantFreq < 1550) && (oscRate > 19) && (oscRate < 23) && (ae.Score > 1.00) && (ae.Duration > 0.1))
            {
                ae.Name = "Neobatrachus sudelli";
            }
            else
            if ((ae.DominantFreq > 2100) && (ae.DominantFreq < 2400) && (oscRate > 90) && (oscRate < 120) && (ae.Score > 0.05) && (ae.Duration > 0.1))
            {
                ae.Name = "Paracrinia haswelli";
            }
            else
            if ((ae.DominantFreq > 450) && (ae.DominantFreq < 650) && (oscRate > 100) && (oscRate < 130) && (ae.Score > 0.5) && (ae.Duration > 0.01))
            {
                ae.Name = "Philoria kundagungan";
            }
            else
            if ((ae.DominantFreq > 400) && (ae.DominantFreq < 600) && (oscRate > 62) && (oscRate < 82) && (ae.Score > 0.5) && (ae.Duration > 0.1))
            {
                ae.Name = "Philoria loveridgei";
            }
            else
            if ((ae.DominantFreq > 960) && (ae.DominantFreq < 1250) && (oscRate > 20) && (oscRate < 45) && (ae.Score > 0.5) && (ae.Duration > 0.05))
            {
                ae.Name = "Philoria sphagnicolus";
            }
            else
            if ((ae.DominantFreq > 2650) && (ae.DominantFreq < 2950) && (oscRate > 67) && (oscRate < 85) && (ae.Score > 0.2) && (ae.Duration > 0.1))
            {
                ae.Name = "Pseudophryne australis";
            }
            else
            if ((ae.DominantFreq > 2300) && (ae.DominantFreq < 2600) && (oscRate > 45) && (oscRate < 55) && (ae.Score > 0.50) && (ae.Duration > 0.2))
            {
                ae.Name = "Pseudophryne coriacea";
            }
            else
            if ((ae.DominantFreq > 2400) && (ae.DominantFreq < 2700) && (oscRate > 40) && (oscRate < 50) && (ae.Score > 0.5) && (ae.Duration > 0.2))
            {
                ae.Name = "Pseudophryne raveni";
            }
            else
            if ((ae.DominantFreq > 2100) && (ae.DominantFreq < 2400) && (oscRate > 35) && (oscRate < 48) && (ae.Score > 1.00) && (ae.Duration > 0.4))
            {
                ae.Name = "Uperoleia fusca";
            }
            else
            if ((ae.DominantFreq > 2300) && (ae.DominantFreq < 2500) && (oscRate > 135) && (oscRate < 155) && (ae.Score > 0.30) && (ae.Duration > 0.1) && (ae.Duration < 0.5))
            {
                ae.Name = "Uperoleia laevigata";
            }
            else
            if ((ae.DominantFreq > 2100) && (ae.DominantFreq < 2300) && (oscRate > 27) && (oscRate < 37) && (ae.Score > 0.30) && (ae.Duration < 0.2))
            {
                ae.Name = "Uperoleia rugosa";
            }
            else
            if ((ae.DominantFreq > 2700) && (ae.DominantFreq < 3000) && (oscRate > 80) && (oscRate < 100) && (ae.Score > 0.10) && (ae.Duration < 1.0))
            {
                ae.Name = "Uperoleia tyleri";
            }
        }

        public static List<Plot> CreateScorePlots(List<SpectralTrack> tracks, int rowCount, int topBin)
        {
            double herzPerBin = tracks[0].herzPerBin;
            //init score arrays
            var scores = new List<double[]>();
            for (int c = 1; c <= topBin; c++) //ignore DC bin
            {
                var array = new double[rowCount];
                scores.Add(array);
            }

            //add in track scores
            foreach (SpectralTrack track in tracks)
            {
                int sampleStart = track.StartFrame;
                int bin = (int)Math.Round(track.AverageBin) - 1; //get the tracks frequency i.e. bin number
                if(bin >= topBin) continue;
                int length = track.periodicity.Length;
                int periodicityStart = sampleStart + (track.Length/3);
                for (int r = 0; r < length; r++) // for each position in track
                {
                    scores[bin][periodicityStart + r] = track.periodicityScore[r];
                }
            }

            double intensityThreshold = 0.25;
            var plots = new List<Plot>();
            for (int c = 1; c < topBin; c++) //ignore DC bin
            {
                double[] filteredScores = DataTools.filterMovingAverage(scores[c-1], 3);
                filteredScores = DataTools.NormaliseInZeroOne(filteredScores, 0, 2.0); //## ABSOLUTE NORMALISATION 0-0.75 #####################################
                string title = ((int)Math.Round(c * herzPerBin)).ToString();
                plots.Add(new Plot(title + " Hz", filteredScores, intensityThreshold));
            }
            return plots;
        } // CreateScorePlots()


        static Image DrawSonogram(BaseSonogram sonogram, double[,] hits, List<Plot> plots, List<AcousticEvent> predictedEvents)
        {
            bool doHighlightSubband = false; bool add1kHzLines = false;
            //int maxFreq = sonogram.NyquistFrequency / 2;
            //int maxFreq = 6000;
            //Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(maxFreq, 1, doHighlightSubband, add1kHzLines));
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));

            //System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            //img.Save(@"C:\SensorNetworks\temp\testimage1.png", System.Drawing.Imaging.ImageFormat.Png);

            //Image_MultiTrack image = new Image_MultiTrack(img);
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            if (plots != null)
                foreach (Plot plot in plots) image.AddTrack(Image_Track.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title));
            //if (hits != null) image.OverlayRainbowTransparency(hits);
            //if (hits != null) image.OverlayRedTransparency(hits);
            if (hits != null) image.OverlayRedMatrix(hits, 1.0);
            if (predictedEvents.Count > 0) image.AddEvents(predictedEvents, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            return image.GetImage();
        } //DrawSonogram()




        public static DataTable WriteEvents2DataTable(List<AcousticEvent> predictedEvents)
        {
            if (predictedEvents == null) return null;
            string[] headers = { AudioAnalysisTools.Keys.EVENT_COUNT,        //1
                                 AudioAnalysisTools.Keys.EVENT_START_MIN,    //2
                                 AudioAnalysisTools.Keys.EVENT_START_SEC,    //3
                                 AudioAnalysisTools.Keys.EVENT_START_ABS,    //4
                                 AudioAnalysisTools.Keys.SEGMENT_TIMESPAN,   //5
                                 AudioAnalysisTools.Keys.EVENT_DURATION,     //6
                                 //AudioAnalysisTools.Keys.EVENT_INTENSITY,
                                 AudioAnalysisTools.Keys.EVENT_NAME,         //7
                                 AudioAnalysisTools.Keys.DOMINANT_FREQUENCY,
                                 AudioAnalysisTools.Keys.OSCILLATION_RATE,
                                 AudioAnalysisTools.Keys.EVENT_SCORE,
                                 AudioAnalysisTools.Keys.EVENT_NORMSCORE 

                               };
            //                   1                2               3              4                5              6               7              8
            Type[] types = { typeof(int), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(string), typeof(double), 
                             typeof(double), typeof(double), typeof(double) };

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
                row[AudioAnalysisTools.Keys.DOMINANT_FREQUENCY] = (double)ev.DominantFreq;
                row[AudioAnalysisTools.Keys.OSCILLATION_RATE] = 1 / (double)ev.Periodicity;
                row[AudioAnalysisTools.Keys.EVENT_SCORE] = (double)ev.Score;      //Score
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


        /// <summary>
        /// NOTE: EDIT THE "Default" string to describethat indicates analysis type.
        /// </summary>
        public static void Usage()
        {
            LoggedConsole.WriteLine("USAGE:");
            LoggedConsole.WriteLine("AnalysisPrograms.exe  " + ANALYSIS_NAME + "  audioPath  configPath  outputDirectory  startOffset  endOffset");
            LoggedConsole.WriteLine(
            @"
            where:
            ANALYSIS_NAME:-   (string) Identifies the analysis type.
            audioPath:-       (string) Path of the audio file to be processed.
            configPath:-      (string) Path of the analysis configuration file.
            outputDirectory:- (string) Path of the output directory in which to store .csv result files.
            THE ABOVE THREE ARGUMENTS ARE OBLIGATORY. 
            THE NEXT TWO ARGUMENTS ARE OPTIONAL:
            startOffset:      (integer) The start (minutes) of that portion of the file to be analysed.
            endOffset:        (integer) The end   (minutes) of that portion of the file to be analysed.
            IF THE LAST TWO ARGUMENTS ARE NOT INCLUDED, THE ENTIRE FILE IS ANALYSED.
            ");
        }



    } //end class Frogs
}
