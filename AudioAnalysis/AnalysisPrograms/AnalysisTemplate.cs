﻿using System;
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
    using Acoustics.Shared.Extensions;

    using AnalysisPrograms.Production;

    using AudioAnalysisTools.Indices;

    public class AnalysisTemplate : IAnalyser
    {
        //OTHER CONSTANTS
        public const string AnalysisName = "Default";
        public const int ResampleRate = 17640;
        //public const int RESAMPLE_RATE = 22050;
        //public const string imageViewer = @"C:\Program Files\Windows Photo Viewer\ImagingDevices.exe";
        public const string ImageViewer = @"C:\Windows\system32\mspaint.exe";


        public string DisplayName
        {
            get { return "Default Analysis"; }
        }

        public string Identifier
        {
            get { return "Towsey." + AnalysisName; }
        }


        public class Arguments : AnalyserArguments
        {
        }

        public static void Dev(Arguments arguments)
        {
            var executeDev = arguments == null;

            if (executeDev)
            {
                string recordingPath = @"C:\SensorNetworks\WavFiles\Human\Planitz.wav";
                string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Template.cfg";
                string outputDir = @"C:\SensorNetworks\Output\Test\";
                //string csvPath    = @"C:\SensorNetworks\Output\Test\TEST_Indices.csv";

                int startMinute = 0;
                int durationSeconds = 0; //set zero to get entire recording
                var tsStart = new TimeSpan(0, startMinute, 0); //hours, minutes, seconds
                var tsDuration = new TimeSpan(0, 0, durationSeconds); //hours, minutes, seconds
                var segmentFileStem = Path.GetFileNameWithoutExtension(recordingPath);
                var segmentFName = string.Format("{0}_{1}min.wav", segmentFileStem, startMinute);
                var sonogramFname = string.Format("{0}_{1}min.png", segmentFileStem, startMinute);
                var eventsFname = string.Format("{0}_{1}min.{2}.Events.csv", segmentFileStem, startMinute, "Towsey." + AnalysisName);
                var indicesFname = string.Format("{0}_{1}min.{2}.Indices.csv", segmentFileStem, startMinute, "Towsey." + AnalysisName);

                arguments = new Arguments
                {
                    Source = recordingPath.ToFileInfo(),
                    Config = configPath.ToFileInfo(),
                    Output = outputDir.ToDirectoryInfo(),
                    TmpWav = segmentFName,
                    Events = eventsFname,
                    Indices = indicesFname,
                    Sgram = sonogramFname,
                    Start = tsStart.TotalSeconds,
                    Duration = tsDuration.TotalSeconds
                };

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

            string title = "# FOR DETECTION OF ############ using TECHNIQUE ########";
            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(title);
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Output folder:  " + arguments.Output);
            LoggedConsole.WriteLine("# Recording file: " + arguments.Source.Name);

            Log.Verbosity = 1;

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
                    TowseyLibrary.ProcessRunner process = new TowseyLibrary.ProcessRunner(ImageViewer);
                    process.Run(image.FullName, arguments.Output.FullName);
                }

                LoggedConsole.WriteLine("\n\n# Finished analysis:- " + arguments.Source.FullName);
            }
        }



        /// <summary>
        /// A WRAPPER AROUND THE analyser.Analyze(analysisSettings) METHOD
        /// To be called as an executable with command line arguments.
        /// Use this when you want to analyse only a short segment of recording i.e. 1-2 miniutes
        /// </summary>
        public static void Execute(Arguments arguments)
        {
            AnalysisSettings analysisSettings = arguments.ToAnalysisSettings();
            TimeSpan tsStart = TimeSpan.FromSeconds(arguments.Start ?? 0);
            TimeSpan tsDuration = TimeSpan.FromSeconds(arguments.Duration ?? 0);

            // EXTRACT THE REQUIRED RECORDING SEGMENT
            FileInfo fiSource = analysisSettings.SourceFile;
            FileInfo tempF = analysisSettings.AudioFile;
            if (tempF.Exists) { tempF.Delete(); }

            // GET INFO ABOUT THE SOURCE and the TARGET files - esp need the sampling rate
            AudioUtilityModifiedInfo beforeAndAfterInfo;

            if (tsDuration == TimeSpan.Zero) // Process entire file
            {
                beforeAndAfterInfo = AudioFilePreparer.PrepareFile(fiSource, tempF, new AudioUtilityRequest { TargetSampleRate = AnalysisTemplate.ResampleRate }, analysisSettings.AnalysisBaseTempDirectoryChecked);
            }
            else
            {
                beforeAndAfterInfo = AudioFilePreparer.PrepareFile(fiSource, tempF, new AudioUtilityRequest { TargetSampleRate = AnalysisTemplate.ResampleRate, OffsetStart = tsStart, OffsetEnd = tsStart.Add(tsDuration) }, analysisSettings.AnalysisBaseTempDirectoryChecked);
            }

            // Store source sample rate - may need during the analysis if have upsampled the source.
            analysisSettings.SampleRateOfOriginalAudioFile = beforeAndAfterInfo.SourceInfo.SampleRate;

            // DO THE ANALYSIS
            //#############################################################################################################################################
            IAnalyser analyser = new AnalysisTemplate();
            AnalysisResult result = analyser.Analyse(analysisSettings);
            DataTable dt = result.Data;
            if (dt == null) { throw new InvalidOperationException("Data table of results is null"); }
            //#############################################################################################################################################

            // ADD IN ADDITIONAL INFO TO RESULTS TABLE
            AddContext2Table(dt, tsStart, result.AudioDuration);
            CsvTools.DataTable2CSV(dt, analysisSettings.EventsFile.FullName);
            // DataTableTools.WriteTable(augmentedTable);

        }

        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            //var configuration = new ConfigDictionary(analysisSettings.ConfigFile.FullName);
            //Dictionary<string, string> configDict = configuration.GetTable();
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

            //if ((predictedEvents != null) && (predictedEvents.Count != 0))
            if (predictedEvents != null)
            {
                string analysisName = analysisSettings.ConfigDict[AudioAnalysisTools.AnalysisKeys.AnalysisName];
                string fName = Path.GetFileNameWithoutExtension(fiAudioF.Name);
                foreach (AcousticEvent ev in predictedEvents)
                {
                    ev.FileName = fName;
                    ev.Name = analysisName;
                    ev.SegmentDuration = recordingTimeSpan;
                }
                //write events to a data table to return.
                dataTable = WriteEvents2DataTable(predictedEvents);
                string sortString = AnalysisKeys.EventStartAbs + " ASC";
                dataTable = DataTableTools.SortTable(dataTable, sortString); //sort by start time before returning
            }

            if ((analysisSettings.EventsFile != null) && (dataTable != null))
            {
                CsvTools.DataTable2CSV(dataTable, analysisSettings.EventsFile.FullName);
            }
            else
                analysisResults.EventsFile = null;

            if ((analysisSettings.SummaryIndicesFile != null) && (dataTable != null))
            {
                double scoreThreshold = 0.01;
                if (analysisSettings.ConfigDict.ContainsKey(AnalysisKeys.IntensityThreshold))
                    scoreThreshold = ConfigDictionary.GetDouble(AnalysisKeys.IntensityThreshold, analysisSettings.ConfigDict);
                TimeSpan unitTime = TimeSpan.FromSeconds(60); //index for each time span of i minute
                var indicesDT = ConvertEvents2Indices(dataTable, unitTime, recordingTimeSpan, scoreThreshold);
                CsvTools.DataTable2CSV(indicesDT, analysisSettings.SummaryIndicesFile.FullName);
            }
            else
                analysisResults.IndicesFile = null;

            //save image of sonograms
            if (analysisSettings.SegmentSaveBehavior.ShouldSave(analysisResults.Data.Rows.Count))
            {
                string imagePath = analysisSettings.ImageFile.FullName;
                double eventThreshold = 0.1;
                Image image = SpectrogramTools.Sonogram2Image(sonogram, analysisSettings.ConfigDict, hits, scores, predictedEvents, eventThreshold);
                image.Save(imagePath, ImageFormat.Png);
            }
            else
                analysisResults.ImageFile = null;

            analysisResults.Data = dataTable;
            analysisResults.ImageFile = analysisSettings.ImageFile;
            analysisResults.AudioDuration = recordingTimeSpan;
            //result.DisplayItems = { { 0, "example" }, { 1, "example 2" }, }
            //result.OutputFiles = { { "exmaple file key", new FileInfo("Where's that file?") } }
            return analysisResults;
        } 

        /// <summary>
        /// ################ THE KEY ANALYSIS METHOD
        /// Returns a DataTable
        /// </summary>
        /// <param name="fiSegmentOfSourceFile"></param>
        /// <param name="configDict"></param>
        /// <param name="diOutputDir"></param>
        public static Tuple<BaseSonogram, double[,], List<Plot>, List<AcousticEvent>, TimeSpan> Analysis(FileInfo fiSegmentOfSourceFile, Dictionary<string, string> configDict)
        {
            //set default values - ignore those set by user
            int frameSize = 1024;
            double windowOverlap = 0.0;

            int minHz = int.Parse(configDict[AnalysisKeys.MinHz]);
            double intensityThreshold = double.Parse(configDict[AnalysisKeys.IntensityThreshold]); //in 0-1
            //double minDuration = Double.Parse(configDict[Keys.MIN_DURATION]);  // seconds
            //double maxDuration = Double.Parse(configDict[Keys.MAX_DURATION]);  // seconds
            double minDuration = 0.0;
            double maxDuration = 0.0;

            AudioRecording recording = new AudioRecording(fiSegmentOfSourceFile.FullName);
            if (recording == null)
            {
                LoggedConsole.WriteLine("AudioRecording == null. Analysis not possible.");
                return null;
            }
            TimeSpan tsRecordingDuration = recording.Duration();
            double minRecordingDuration = 15;
            if (tsRecordingDuration.TotalSeconds < minRecordingDuration)
            {
                LoggedConsole.WriteLine("Audio recording must be at least {0} seconds long for analysis.", minRecordingDuration);
                return null;
            }

            //i: MAKE SONOGRAM
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.BaseName;
            sonoConfig.WindowSize = frameSize;
            sonoConfig.WindowOverlap = windowOverlap;
            //sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("NONE");
            sonoConfig.NoiseReductionType = SNR.KeyToNoiseReductionType("STANDARD");
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;
            double framesPerSecond = freqBinWidth;



            //#############################################################################################################################################
            //window    sr          frameDuration   frames/sec  hz/bin  64frameDuration hz/64bins       hz/128bins
            // 1024     22050       46.4ms          21.5        21.5    2944ms          1376hz          2752hz
            // 1024     17640       58.0ms          17.2        17.2    3715ms          1100hz          2200hz
            // 2048     17640       116.1ms          8.6         8.6    7430ms           551hz          1100hz

            //the Xcorrelation-FFT technique requires number of bins to scan to be power of 2.
            //assuming sr=17640 and window=1024, then  64 bins span 1100 Hz above the min Hz level. i.e. 500 to 1600
            //assuming sr=17640 and window=1024, then 128 bins span 2200 Hz above the min Hz level. i.e. 500 to 2700
            int numberOfBins = 64;
            int minBin = (int)Math.Round(minHz / freqBinWidth) + 1;
            int maxbin = minBin + numberOfBins - 1;
            int maxHz = (int)Math.Round(minHz + (numberOfBins * freqBinWidth));

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);

            double[,] subMatrix = MatrixTools.Submatrix(sonogram.Data, 0, minBin, (rowCount - 1), maxbin);

            //ALTERNATIVE IS TO USE THE AMPLITUDE SPECTRUM
            //var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, sr, frameSize, windowOverlap);
            //double[,] matrix = results2.Item3;  //amplitude spectrogram. Note that column zero is the DC or average energy value and can be ignored.
            //double[] avAbsolute = results2.Item1; //average absolute value over the minute recording
            ////double[] envelope = results2.Item2;
            //double windowPower = results2.Item4;


            //######################################################################
            //ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            var results = System.Tuple.Create(new double[100], new double[100]);
            double[] scoreArray1 = results.Item1;
            double[] scoreArray2 = results.Item2;
            //######################################################################

            var plots = new List<Plot>();
            plots.Add(new Plot("title", scoreArray1, 0.2)); //or get the analysis to pass back plots
            var hits = new double[rowCount, colCount];

            //iii: CONVERT SCORES TO ACOUSTIC EVENTS
            List<AcousticEvent> predictedEvents = AcousticEvent.ConvertScoreArray2Events(scoreArray1, minHz, maxHz, sonogram.FramesPerSecond, freqBinWidth,
                                                                                         intensityThreshold, minDuration, maxDuration);
            return System.Tuple.Create(sonogram, hits, plots, predictedEvents, tsRecordingDuration);
        }

        //static Image DrawSonogram(BaseSonogram sonogram, double[,] hits, List<Plot> scores, List<AcousticEvent> predictedEvents, double eventThreshold)
        //{
        //    bool doHighlightSubband = false; bool add1kHzLines = true;
        //    int maxFreq = sonogram.NyquistFrequency / 2;
        //    Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(maxFreq, 1, doHighlightSubband, add1kHzLines));

        //    //System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
        //    //img.Save(@"C:\SensorNetworks\temp\testimage1.png", System.Drawing.Imaging.ImageFormat.Png);

        //    //Image_MultiTrack image = new Image_MultiTrack(img);
        //    image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
        //    image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
        //    //if (scores != null) image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, eventThreshold));
        //    if (scores != null)
        //    {
        //        foreach (Plot plot in scores)
        //            image.AddTrack(Image_Track.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
        //    }
        //    if (hits != null) image.OverlayRainbowTransparency(hits);
        //    if (predictedEvents.Count > 0) image.AddEvents(predictedEvents, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
        //    return image.GetImage();
        //} //DrawSonogram()

        public static DataTable WriteEvents2DataTable(List<AcousticEvent> predictedEvents)
        {
            if (predictedEvents == null) return null;
            string[] headers = { AudioAnalysisTools.AnalysisKeys.EventCount,
                                 AudioAnalysisTools.AnalysisKeys.EventStartMin,
                                 AudioAnalysisTools.AnalysisKeys.EventStartSec, 
                                 AudioAnalysisTools.AnalysisKeys.EventStartAbs,
                                 AudioAnalysisTools.AnalysisKeys.KeySegmentDuration,
                                 AudioAnalysisTools.AnalysisKeys.EventDuration, 
                                 AudioAnalysisTools.AnalysisKeys.EventIntensity,
                                 AudioAnalysisTools.AnalysisKeys.EventName,
                                 AudioAnalysisTools.AnalysisKeys.EventScore,
                                 AudioAnalysisTools.AnalysisKeys.EventNormscore 

                               };
            //                   1                2               3              4                5              6               7              8
            Type[] types = { typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(string), 
                             typeof(double), typeof(double) };

            var dataTable = DataTableTools.CreateTable(headers, types);
            if (predictedEvents.Count == 0) return dataTable;

            foreach (var ev in predictedEvents)
            {
                DataRow row = dataTable.NewRow();
                row[AudioAnalysisTools.AnalysisKeys.EventStartSec] = (double)ev.TimeStart;  //EvStartSec
                row[AudioAnalysisTools.AnalysisKeys.EventDuration] = (double)ev.Duration;   //duratio in seconds
                row[AudioAnalysisTools.AnalysisKeys.EventIntensity] = (double)ev.kiwi_intensityScore;   //
                row[AudioAnalysisTools.AnalysisKeys.EventName] = (string)ev.Name;   //
                row[AudioAnalysisTools.AnalysisKeys.EventNormscore] = (double)ev.ScoreNormalised;
                row[AudioAnalysisTools.AnalysisKeys.EventScore] = (double)ev.Score;      //Score
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
                double eventStart = (double)ev[AudioAnalysisTools.AnalysisKeys.EventStartSec];
                double eventScore = (double)ev[AudioAnalysisTools.AnalysisKeys.EventNormscore];
                int timeUnit = (int)(eventStart / unitTime.TotalSeconds);
                eventsPerUnitTime[timeUnit]++;
                if (eventScore > scoreThreshold) bigEvsPerUnitTime[timeUnit]++;
            }

            string[] headers = { AudioAnalysisTools.AnalysisKeys.KeyStartMinute, AudioAnalysisTools.AnalysisKeys.EventTotal, ("#Ev>" + scoreThreshold) };
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
            if (dt == null) return;

            if (!dt.Columns.Contains(AnalysisKeys.KeySegmentDuration)) dt.Columns.Add(AudioAnalysisTools.AnalysisKeys.KeySegmentDuration, typeof(double));
            if (!dt.Columns.Contains(AnalysisKeys.EventStartAbs)) dt.Columns.Add(AudioAnalysisTools.AnalysisKeys.EventStartAbs, typeof(double));
            if (!dt.Columns.Contains(AnalysisKeys.EventStartMin)) dt.Columns.Add(AudioAnalysisTools.AnalysisKeys.EventStartMin, typeof(double));
            double start = segmentStartMinute.TotalSeconds;
            foreach (DataRow row in dt.Rows)
            {
                row[AudioAnalysisTools.AnalysisKeys.KeySegmentDuration] = recordingTimeSpan.TotalSeconds;
                row[AudioAnalysisTools.AnalysisKeys.EventStartAbs] = start + (double)row[AudioAnalysisTools.AnalysisKeys.EventStartSec];
                row[AudioAnalysisTools.AnalysisKeys.EventStartMin] = start;
            }
        }

        /// <summary>
        /// This method should no longer be used. 
        /// It depends on use of the DataTable class which ceased when Anthony did a major refactor in mid-2014.
        /// </summary>
        /// <param name="fiCsvFile"></param>
        /// <param name="fiConfigFile"></param>
        /// <returns></returns>
        public Tuple<DataTable, DataTable> ProcessCsvFile(FileInfo fiCsvFile, FileInfo fiConfigFile)
        {
            //THIS METHOD HAS BEEn DEPRACATED
            //return DrawSummaryIndices.ProcessCsvFile(fiCsvFile, fiConfigFile);
            return null;
        }

        /// <summary>
        /// takes a data table of indices and normalises column values to values in [0,1].
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        //public static DataTable NormaliseColumnsOfDataTable(DataTable dt)
        //{
        //    string[] headers = DataTableTools.GetColumnNames(dt);
        //    string[] newHeaders = new string[headers.Length];

        //    List<double[]> newColumns = new List<double[]>();

        //    for (int i = 0; i < headers.Length; i++)
        //    {
        //        double[] values = DataTableTools.Column2ArrayOfDouble(dt, headers[i]); //get list of values
        //        if ((values == null) || (values.Length == 0)) continue;

        //        double min = 0;
        //        double max = 1;
        //        if (headers[i].Equals(Keys.AV_AMPLITUDE))
        //        {
        //            min = -50;
        //            max = -5;
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders[i] = headers[i] + "  (-50..-5dB)";
        //        }
        //        else //default is to normalise in [0,1]
        //        {
        //            newColumns.Add(DataTools.normalise(values)); //normalise all values in [0,1]
        //            newHeaders[i] = headers[i];
        //        }
        //    } //for loop

        //    //convert type int to type double due to normalisation
        //    Type[] types = new Type[newHeaders.Length];
        //    for (int i = 0; i < newHeaders.Length; i++) types[i] = typeof(double);
        //    var processedtable = DataTableTools.CreateTable(newHeaders, types, newColumns);
        //    return processedtable;
        //}


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
                    SegmentTargetSampleRate = AnalysisTemplate.ResampleRate
                };
            }
        }
    }
}
