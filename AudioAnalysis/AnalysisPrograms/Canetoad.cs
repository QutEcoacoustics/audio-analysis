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

using TowseyLibrary;
using AudioAnalysisTools;
using AudioAnalysisTools.StandardSpectrograms;
using AudioAnalysisTools.DSP;
using AudioAnalysisTools.WavTools;

namespace AnalysisPrograms
{
    using System.Diagnostics.Contracts;

    using Acoustics.Shared.Extensions;

    using AnalysisPrograms.Production;

    using PowerArgs;

    /// <summary>
    /// NOTE: In order to detect canetoad oscillations which can reach 15 per second one requires a frame rate of at least 30 frames per second and preferably 
    /// a frame rate = 60 so that this period sits near middle of the array of DCT coefficients.
    /// The frame rate is affected by three parameters: 1) SAMPLING RATE; 2) FRAME LENGTH; 3) FRAME OVERLAP. User may wish to set SR and FRAME LENGTH should = 512 or 1024.
    /// Therefore best way to adjust frame rate is to adjust frame overlap. 
    /// Have decided on the option of auomatically calculating the frame ovelap to suit the maximum oscillation to be detected.
    /// This is written in the method OscillationDetector.CalculateRequiredFrameOverlap();
    /// Do not want the DCT length to be too long because DCT is expensive to calculate. 0.5s - 1.0s is adequate for canetoad -depends on the expected osc rate.
    /// 
    /// Analysis() method.
    /// </summary>
    public class Canetoad : IAnalyser
    {
        public class Arguments : AnalyserArguments
        {

        }

        //OTHER CONSTANTS
        public const string ANALYSIS_NAME = "Canetoad";
        public const int RESAMPLE_RATE = 17640;
        //public const int RESAMPLE_RATE = 22050;
        //public const string imageViewer = @"C:\Program Files\Windows Photo Viewer\ImagingDevices.exe";
        public const string imageViewer = @"C:\Windows\system32\mspaint.exe";


        public string DisplayName
        {
            get { return "Canetoad"; }
        }

        private static string identifier = "Towsey." + ANALYSIS_NAME;
        public string Identifier
        {
            get { return identifier; }
        }


        public static void Dev(Arguments arguments)
        {
            bool executeDev = arguments == null;
            if (executeDev)
            {
                arguments = new Arguments();
                string recordingPath =
                    @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100529_16bitPCM.wav";
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100530_2_16bitPCM.wav";
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100530_1_16bitPCM.wav";
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Canetoad\RuralCanetoads_9Jan\toads_rural_9jan2010\toads_rural1_16.mp3";

                string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Canetoad.cfg";
                string outputDir = @"C:\SensorNetworks\Output\Canetoad\";
                //string csvPath       = @"C:\SensorNetworks\Output\Test\TEST_Indices.csv";

                string title = "# FOR DETECTION OF CANETOAD using DCT OSCILLATION DETECTION";
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine(title);
                LoggedConsole.WriteLine(date);
                LoggedConsole.WriteLine("# Output folder:  " + outputDir);
                LoggedConsole.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
                var diOutputDir = new DirectoryInfo(outputDir);

                Log.Verbosity = 1;
                int startMinute = 0;
                int durationSeconds = 0; //set zero to get entire recording
                var tsStart = TimeSpan.FromMinutes(startMinute); //hours, minutes, seconds
                var tsDuration = TimeSpan.FromSeconds(durationSeconds); //hours, minutes, seconds
                var segmentFileStem = Path.GetFileNameWithoutExtension(recordingPath);
                var segmentFName = string.Format("{0}_{1}min.wav", segmentFileStem, startMinute);
                var sonogramFname = string.Format("{0}_{1}min.png", segmentFileStem, startMinute);
                var eventsFname = string.Format("{0}_{1}min.{2}.Events.csv", segmentFileStem, startMinute, identifier);
                var indicesFname = string.Format("{0}_{1}min.{2}.Indices.csv", segmentFileStem, startMinute, identifier);

                if (true)
                {
                    arguments.Source = recordingPath.ToFileInfo();
                    arguments.Config = configPath.ToFileInfo();
                    arguments.Output = outputDir.ToDirectoryInfo();
                    arguments.TmpWav = segmentFName;
                    arguments.Events = eventsFname;
                    arguments.Indices = indicesFname;
                    arguments.Sgram = sonogramFname;
                    arguments.Start = tsStart.TotalSeconds;
                    arguments.Duration = tsDuration.TotalSeconds;
                }

                if (false)
                {
                    // loads a csv file for visualisation
                    //string indicesImagePath = "some path or another";
                    //var fiCsvFile    = new FileInfo(restOfArgs[0]);
                    //var fiConfigFile = new FileInfo(restOfArgs[1]);
                    //var fiImageFile  = new FileInfo(restOfArgs[2]); //path to which to save image file.
                    //IAnalysis analyser = new AnalysisTemplate();
                    //var dataTables = analyser.ProcessCsvFile(fiCsvFile, fiConfigFile);
                    //returns two datatables, the second of which is to be converted to an image (fiImageFile) for display
                }
            }
            
            Execute(arguments);


            if (executeDev)
            {
                var csvEvents = arguments.Output.CombineFile(arguments.Events);  
                if (!csvEvents.Exists)
                {
                    Log.WriteLine(
                        "\n\n\n############\n WARNING! Events CSV file not returned from analysis of minute {0} of file <{0}>.",
                        arguments.Start.Value,
                        arguments.Source.FullName);
                }
                else
                {
                    LoggedConsole.WriteLine("\n");
                    DataTable dt = CsvTools.ReadCSVToTable(csvEvents.FullName, true);
                    DataTableTools.WriteTable2Console(dt);
                }
                var csvIndicies = arguments.Output.CombineFile(arguments.Indices);
                if (!csvIndicies.Exists)
                {
                    Log.WriteLine(
                        "\n\n\n############\n WARNING! Indices CSV file not returned from analysis of minute {0} of file <{0}>.",
                        arguments.Start.Value,
                        arguments.Source.FullName);
                }
                else
                {
                    LoggedConsole.WriteLine("\n");
                    DataTable dt = CsvTools.ReadCSVToTable(csvIndicies.FullName, true);
                    DataTableTools.WriteTable2Console(dt);
                }
                var image = arguments.Output.CombineFile(arguments.Sgram);
                if (image.Exists)
                {
                    TowseyLibrary.ProcessRunner process = new TowseyLibrary.ProcessRunner(imageViewer);
                    process.Run(image.FullName, arguments.Output.FullName);
                }

                LoggedConsole.WriteLine("\n\n# Finished analysis:- " + arguments.Source.FullName);
            }
        } //Dev()



        /// <summary>
        /// A WRAPPER AROUND THE analyser.Analyse(analysisSettings) METHOD
        /// To be called as an executable with command line arguments.
        /// </summary>
        public static void Execute(Arguments arguments)
        {
            Contract.Requires(arguments != null);

            AnalysisSettings analysisSettings = arguments.ToAnalysisSettings();
            TimeSpan tsStart = TimeSpan.FromSeconds(arguments.Start ?? 0);
            TimeSpan tsDuration = TimeSpan.FromSeconds(arguments.Duration ?? 0);

            //EXTRACT THE REQUIRED RECORDING SEGMENT
            FileInfo tempF = analysisSettings.AudioFile;
            if (tsDuration == TimeSpan.Zero)   //Process entire file
            {
                AudioFilePreparer.PrepareFile(arguments.Source, tempF, new AudioUtilityRequest { TargetSampleRate = RESAMPLE_RATE }, analysisSettings.AnalysisBaseTempDirectoryChecked);
                //var fiSegment = AudioFilePreparer.PrepareFile(diOutputDir, fiSourceFile, , Human2.RESAMPLE_RATE);
            }
            else
            {
                AudioFilePreparer.PrepareFile(arguments.Source, tempF, new AudioUtilityRequest { TargetSampleRate = RESAMPLE_RATE, OffsetStart = tsStart, OffsetEnd = tsStart.Add(tsDuration) }, analysisSettings.AnalysisBaseTempDirectoryChecked);
                //var fiSegmentOfSourceFile = AudioFilePreparer.PrepareFile(diOutputDir, new FileInfo(recordingPath), MediaTypes.MediaTypeWav, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(3), RESAMPLE_RATE);
            }

            //DO THE ANALYSIS
            //#############################################################################################################################################
            IAnalyser analyser = new Canetoad();
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
                throw new InvalidOperationException("Data table is null");
            }
        }



        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            var fiAudioF = analysisSettings.AudioFile;
            var diOutputDir = analysisSettings.AnalysisInstanceOutputDirectory;

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

            DataTable dataTable = null;

            if ((predictedEvents != null) && (predictedEvents.Count != 0))
            {
                string analysisName = analysisSettings.ConfigDict[AudioAnalysisTools.AnalysisKeys.ANALYSIS_NAME];
                string fName = Path.GetFileNameWithoutExtension(fiAudioF.Name);
                foreach (AcousticEvent ev in predictedEvents)
                {
                    ev.SourceFileName = fName;
                    ev.Name = analysisName;
                    ev.SourceFileDuration = recordingTimeSpan.TotalSeconds;
                }
                //write events to a data table to return.
                dataTable = WriteEvents2DataTable(predictedEvents);
                string sortString = AnalysisKeys.EVENT_START_ABS + " ASC";
                dataTable = DataTableTools.SortTable(dataTable, sortString); //sort by start time before returning
            }

            if ((analysisSettings.EventsFile != null) && (dataTable != null))
            {
                CsvTools.DataTable2CSV(dataTable, analysisSettings.EventsFile.FullName);
            }
            else
                analysisResults.EventsFile = null;


            if ((analysisSettings.IndicesFile != null) && (dataTable != null))
            {
                double scoreThreshold = 0.1;
                TimeSpan unitTime = TimeSpan.FromSeconds(60); //index for each time span of i minute
                var indicesDT = ConvertEvents2Indices(dataTable, unitTime, recordingTimeSpan, scoreThreshold);
                CsvTools.DataTable2CSV(indicesDT, analysisSettings.IndicesFile.FullName);
            }
            else
                analysisResults.IndicesFile = null;


            //save image of sonograms
            if ((sonogram != null) && (analysisSettings.ImageFile != null))
            {
                string imagePath = analysisSettings.ImageFile.FullName;
                double eventThreshold = 0.1;
                Image image = DrawSonogram(sonogram, hits, scores, predictedEvents, eventThreshold);
                image.Save(imagePath, ImageFormat.Png);
                analysisResults.ImageFile = analysisSettings.ImageFile;
            }
            else
                analysisResults.ImageFile = null;

            analysisResults.Data = dataTable;
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
        public static Tuple<BaseSonogram, double[,], Plot, List<AcousticEvent>, TimeSpan> Analysis(FileInfo fiSegmentOfSourceFile, Dictionary<string, string> configDict)
        {
            int minHz = int.Parse(configDict[AnalysisKeys.MIN_HZ]);
            int maxHz = int.Parse(configDict[AnalysisKeys.MAX_HZ]);
            //double frameOverlap = Double.Parse(configDict[Keys.FRAME_OVERLAP]);    //BETTER TO CALUCLATE THIS. IGNORE USER!
            double dctDuration = double.Parse(configDict[AnalysisKeys.DCT_DURATION]);       //duration of DCT in seconds 
            double dctThreshold = double.Parse(configDict[AnalysisKeys.DCT_THRESHOLD]);      //minimum acceptable value of a DCT coefficient
            int minOscilFreq = int.Parse(configDict[AnalysisKeys.MIN_OSCIL_FREQ]);      //ignore oscillations below this threshold freq
            int maxOscilFreq = int.Parse(configDict[AnalysisKeys.MAX_OSCIL_FREQ]);      //ignore oscillations above this threshold freq
            double minDuration = double.Parse(configDict[AnalysisKeys.MIN_DURATION]);       //min duration of event in seconds 
            double maxDuration = double.Parse(configDict[AnalysisKeys.MAX_DURATION]);       //max duration of event in seconds 
            double eventThreshold = double.Parse(configDict[AnalysisKeys.EVENT_THRESHOLD]);  //min score for an acceptable event

            AudioRecording recording = new AudioRecording(fiSegmentOfSourceFile.FullName);
            if (recording == null)
            {
                LoggedConsole.WriteLine("AudioRecording == null. Analysis not possible.");
                return null;
            }

            int frameSize = 1024; //seems to work
            double windowOverlap = OscillationDetector.CalculateRequiredFrameOverlap(recording.SampleRate, frameSize, maxOscilFreq);


            //i: MAKE SONOGRAM
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.WindowSize = frameSize;
            sonoConfig.WindowOverlap = windowOverlap;
            sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("NONE");
            //sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("STANDARD");
            TimeSpan tsRecordingtDuration = recording.Duration();
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;

            //#############################################################################################################################################
            //window    sr          frameDuration   frames/sec  hz/bin  64frameDuration hz/64bins       hz/128bins
            // 1024     22050       46.4ms          21.5        21.5    2944ms          1376hz          2752hz
            // 1024     17640       58.0ms          17.2        17.2    3715ms          1100hz          2200hz
            // 2048     17640       116.1ms          8.6         8.6    7430ms           551hz          1100hz

            //int minBin = (int)Math.Round(minHz / freqBinWidth) + 1;
            //int maxbin = minBin + numberOfBins - 1;

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.GetWavReader());
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);
            recording.Dispose();
            //double[,] subMatrix = MatrixTools.Submatrix(sonogram.Data, 0, minBin, (rowCount - 1), maxbin);

            //######################################################################
            //ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            double[] scores;                      //predefinition of score array
            List<AcousticEvent> events;
            double[,] hits;
            OscillationDetector.Execute((SpectrogramStandard)sonogram, minHz, maxHz, dctDuration, minOscilFreq, maxOscilFreq, dctThreshold, eventThreshold,
                                        minDuration, maxDuration, out scores, out events, out hits);

            //######################################################################
            Plot plot = new Plot(Canetoad.ANALYSIS_NAME, scores, eventThreshold);
            return System.Tuple.Create(sonogram, hits, plot, events, tsRecordingtDuration);
        } //Analysis()



        static Image DrawSonogram(BaseSonogram sonogram, double[,] hits, Plot scores, List<AcousticEvent> predictedEvents, double eventThreshold)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            int maxFreq = sonogram.NyquistFrequency / 2;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(maxFreq, 1, doHighlightSubband, add1kHzLines));

            //System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            //img.Save(@"C:\SensorNetworks\temp\testimage1.png", System.Drawing.Imaging.ImageFormat.Png);

            //Image_MultiTrack image = new Image_MultiTrack(img);
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            if (scores != null) image.AddTrack(Image_Track.GetNamedScoreTrack(scores.data, 0.0, 1.0, scores.threshold, scores.title));
            //if (hits != null) image.OverlayRedTransparency(hits);
            if (hits != null) image.OverlayRainbowTransparency(hits);
            if ((predictedEvents != null) && (predictedEvents.Count > 0))
                image.AddEvents(predictedEvents, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            return image.GetImage();
        } //DrawSonogram()


        public static DataTable WriteEvents2DataTable(List<AcousticEvent> predictedEvents)
        {
            if (predictedEvents == null) return null;
            string[] headers = { AudioAnalysisTools.AnalysisKeys.EVENT_COUNT,
                                 AudioAnalysisTools.AnalysisKeys.EVENT_START_MIN,
                                 AudioAnalysisTools.AnalysisKeys.EVENT_START_SEC, 
                                 AudioAnalysisTools.AnalysisKeys.EVENT_START_ABS,
                                 AudioAnalysisTools.AnalysisKeys.KEY_SegmentDuration,
                                 AudioAnalysisTools.AnalysisKeys.EVENT_DURATION, 
                                 AudioAnalysisTools.AnalysisKeys.EVENT_INTENSITY,
                                 AudioAnalysisTools.AnalysisKeys.EVENT_NAME,
                                 AudioAnalysisTools.AnalysisKeys.EVENT_SCORE,
                                 AudioAnalysisTools.AnalysisKeys.EVENT_NORMSCORE 

                               };
            //                   1                2               3              4                5              6               7              8
            Type[] types = { typeof(int), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(string), 
                             typeof(double), typeof(double) };

            var dataTable = DataTableTools.CreateTable(headers, types);
            if (predictedEvents.Count == 0) return dataTable;

            foreach (var ev in predictedEvents)
            {
                DataRow row = dataTable.NewRow();
                row[AudioAnalysisTools.AnalysisKeys.EVENT_START_ABS] = (double)ev.TimeStart;  //Set now - will overwrite later
                row[AudioAnalysisTools.AnalysisKeys.EVENT_START_SEC] = (double)ev.TimeStart;  //EvStartSec
                row[AudioAnalysisTools.AnalysisKeys.EVENT_DURATION] = (double)ev.Duration;   //duratio in seconds
                row[AudioAnalysisTools.AnalysisKeys.EVENT_INTENSITY] = (double)ev.kiwi_intensityScore;   //
                row[AudioAnalysisTools.AnalysisKeys.EVENT_NAME] = (string)ev.Name;   //
                row[AudioAnalysisTools.AnalysisKeys.EVENT_NORMSCORE] = (double)ev.ScoreNormalised;
                row[AudioAnalysisTools.AnalysisKeys.EVENT_SCORE] = (double)ev.Score;      //Score
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
            if ((sourceDuration == null) || (sourceDuration == TimeSpan.Zero)) return null;
            double units = sourceDuration.TotalSeconds / unitTime.TotalSeconds;
            int unitCount = (int)(units / 1);   //get whole minutes
            if (units % 1 > 0.0) unitCount += 1; //add fractional minute
            int[] eventsPerUnitTime = new int[unitCount]; //to store event counts
            int[] bigEvsPerUnitTime = new int[unitCount]; //to store counts of high scoring events

            foreach (DataRow ev in dt.Rows)
            {
                double eventStart = (double)ev[AudioAnalysisTools.AnalysisKeys.EVENT_START_ABS];
                double eventScore = (double)ev[AudioAnalysisTools.AnalysisKeys.EVENT_NORMSCORE];
                int timeUnit = (int)(eventStart / unitTime.TotalSeconds);
                if (eventScore != 0.0) eventsPerUnitTime[timeUnit]++;
                if (eventScore > scoreThreshold) bigEvsPerUnitTime[timeUnit]++;
            }

            string[] headers = { AudioAnalysisTools.AnalysisKeys.KEY_StartMinute, AudioAnalysisTools.AnalysisKeys.EVENT_TOTAL, ("#Ev>" + scoreThreshold) };
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
            if (!dt.Columns.Contains(AnalysisKeys.KEY_SegmentDuration)) dt.Columns.Add(AudioAnalysisTools.AnalysisKeys.KEY_SegmentDuration, typeof(double));
            if (!dt.Columns.Contains(AnalysisKeys.EVENT_START_ABS)) dt.Columns.Add(AudioAnalysisTools.AnalysisKeys.EVENT_START_ABS, typeof(double));
            if (!dt.Columns.Contains(AnalysisKeys.EVENT_START_MIN)) dt.Columns.Add(AudioAnalysisTools.AnalysisKeys.EVENT_START_MIN, typeof(double));
            double start = segmentStartMinute.TotalSeconds;
            foreach (DataRow row in dt.Rows)
            {
                row[AudioAnalysisTools.AnalysisKeys.KEY_SegmentDuration] = recordingTimeSpan.TotalSeconds;
                row[AudioAnalysisTools.AnalysisKeys.EVENT_START_ABS] = start + (double)row[AudioAnalysisTools.AnalysisKeys.EVENT_START_SEC];
                row[AudioAnalysisTools.AnalysisKeys.EVENT_START_MIN] = start;
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
                if (configDict.ContainsKey(AnalysisKeys.DISPLAY_COLUMNS))
                    displayHeaders = configDict[AnalysisKeys.DISPLAY_COLUMNS].Split(',').ToList();
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
            if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.EVENT_START_ABS))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.EVENT_START_ABS + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.EVENT_COUNT))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.EVENT_COUNT + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.KEY_RankOrder))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.KEY_RankOrder + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.KEY_StartMinute))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.KEY_StartMinute + " ASC");
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
                if (headers[i].Equals(AnalysisKeys.KEY_AvSignalAmplitude))
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


    } //end class Canetoad
}
