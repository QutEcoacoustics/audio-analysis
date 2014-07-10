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
    using System.Diagnostics.Contracts;

    using Acoustics.Shared.Extensions;

    using AnalysisPrograms.Production;

    public class LewinsRail3 : IAnalyser
    {
        public class Arguments : AnalyserArguments
        {
        }

        // KEYS TO PARAMETERS IN CONFIG FILE
        public const string KeyAnalysisName = AnalysisKeys.AnalysisName;
        public const string KeyCallDuration = AnalysisKeys.CallDuration;
        public const string KeyDecibelThreshold = AnalysisKeys.DecibelThreshold;
        public const string KeyEventThreshold = AnalysisKeys.EventThreshold;
        public const string KeyIntensityThreshold = AnalysisKeys.IntensityThreshold;
        public const string KeySegmentDuration = AnalysisKeys.SegmentDuration;
        public const string KeySegmentOverlap = AnalysisKeys.SegmentOverlap;
        public const string KeyResampleRate = AnalysisKeys.ResampleRate;
        public const string KeyFrameLength = AnalysisKeys.FrameLength;
        public const string KeyFrameOverlap = AnalysisKeys.FrameOverlap;
        public const string KeyNoiseReductionType = AnalysisKeys.NoiseReductionType;
        public const string KeyUpperfreqbandTop = "UpperFreqBandTop";
        public const string KeyUpperfreqbandBtm = "UpperFreqBandBottom";
        public const string KeyLowerfreqbandTop = "LowerFreqBandTop";
        public const string KeyLowerfreqbandBtm = "LowerFreqBandBottom";
        public const string KeyMinAmplitude  = AnalysisKeys.MinAmplitude;
        public const string KeyMinDuration   = AnalysisKeys.MinDuration;
        public const string KeyMaxDuration   = AnalysisKeys.MaxDuration;
        public const string KeyMinPeriod     = AnalysisKeys.MinPeriodicity;
        public const string KeyMaxPeriod = AnalysisKeys.MaxPeriodicity;
        public const string KeyDrawSonograms = AnalysisKeys.KeyDrawSonograms;

        // KEYS TO OUTPUT EVENTS and INDICES
        public const string KeyCount     = "count";
        public const string KeySegmentTimespan = "SegTimeSpan";
        public const string KeyStartAbs = "EvStartAbs";
        public const string KeyStartMin = "EvStartMin";
        public const string KeyStartSec = "EvStartSec";
        public const string KeyCallDensity = "CallDensity";
        public const string KeyCallScore = "CallScore";
        public const string KeyEventTotal = "# events";


        // OTHER CONSTANTS
        public const string AnalysisName = "LewinsRail3";
        public const int ResampleRate = 17640;
        ////public const int RESAMPLE_RATE = 22050;
        ////public const string imageViewer = @"C:\Program Files\Windows Photo Viewer\ImagingDevices.exe";
        public const string ImageViewer = @"C:\Windows\system32\mspaint.exe";


        public string DisplayName
        {
            get { return "Lewins Rail"; }
        }

        public string Identifier
        {
            get { return "Towsey." + AnalysisName; }
        }

        public static void Dev(Arguments arguments)
        {
            var executeDev = arguments == null;
            if (executeDev)
            {
                //string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav";
                //string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-084607.wav";
                //string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-062040.wav";
                string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-075040.wav";
                //string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav";
                string configPath = @"C:\SensorNetworks\Output\LewinsRail\LewinsRail.cfg";
                string outputDir = @"C:\SensorNetworks\Output\LewinsRail\";

                string title = "# FOR DETECTION OF LEWIN'S RAIL using CROSS-CORRELATION & FFT";
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine(title);
                LoggedConsole.WriteLine(date);
                LoggedConsole.WriteLine("# Output folder:  " + outputDir);
                LoggedConsole.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
                var diOutputDir = new DirectoryInfo(outputDir);

                Log.Verbosity = 1;
                int startMinute = 0;
                int durationSeconds = 60; //set zero to get entire recording
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
                    TowseyLibrary.ProcessRunner process = new TowseyLibrary.ProcessRunner(ImageViewer);
                    process.Run(image.FullName, arguments.Output.FullName);
                }

                LoggedConsole.WriteLine("\n\n# Finished analysis:- " + arguments.Source.FullName);
            }
        }



        /// <summary>
        /// A WRAPPER AROUND THE Analysis() METHOD
        /// To be called as an executable with command line arguments.
        /// </summary>
        public static void Execute(Arguments arguments)
        {
            Contract.Requires(arguments != null);


            AnalysisSettings analysisSettings = arguments.ToAnalysisSettings();
            TimeSpan tsStart = TimeSpan.FromSeconds(arguments.Start ?? 0);
            TimeSpan tsDuration = TimeSpan.FromSeconds(arguments.Duration ?? 0);

            //EXTRACT THE REQUIRED RECORDING SEGMENT
            FileInfo sourceF = arguments.Source;
            FileInfo tempF   = analysisSettings.AudioFile;
            if (tsDuration == TimeSpan.Zero)   //Process entire file
            {
                AudioFilePreparer.PrepareFile(sourceF, tempF, new AudioUtilityRequest { TargetSampleRate = ResampleRate }, analysisSettings.AnalysisBaseTempDirectoryChecked);
                //var fiSegment = AudioFilePreparer.PrepareFile(diOutputDir, fiSourceFile, , Human2.RESAMPLE_RATE);
            }
            else
            {
                AudioFilePreparer.PrepareFile(sourceF, tempF, new AudioUtilityRequest { TargetSampleRate = ResampleRate, OffsetStart = tsStart, OffsetEnd = tsStart.Add(tsDuration) }, analysisSettings.AnalysisBaseTempDirectoryChecked);
                //var fiSegmentOfSourceFile = AudioFilePreparer.PrepareFile(diOutputDir, new FileInfo(recordingPath), MediaTypes.MediaTypeWav, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(3), RESAMPLE_RATE);
            }

            //DO THE ANALYSIS
            //#############################################################################################################################################
            IAnalyser analyser = new LewinsRail3();
            AnalysisResult result = analyser.Analyse(analysisSettings);
            DataTable dt = result.Data;
            //#############################################################################################################################################

            //ADD IN ADDITIONAL INFO TO TABLE
            AddContext2Table(dt, tsStart, tsDuration);
            CsvTools.DataTable2CSV(dt, analysisSettings.EventsFile.FullName);
            //DataTableTools.WriteTable(dt);
        }

        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            var configuration = new ConfigDictionary(analysisSettings.ConfigFile.FullName);
            Dictionary<string, string> configDict = configuration.GetTable();
            var fiAudioF    = analysisSettings.AudioFile;
            var diOutputDir = analysisSettings.AnalysisInstanceOutputDirectory;

            var result = new AnalysisResult();
            result.AnalysisIdentifier = this.Identifier;
            result.SettingsUsed = analysisSettings;
            result.Data = null;

            //######################################################################
            var results = Analysis(fiAudioF, configDict);
            //######################################################################

            if (results == null) return result; //nothing to process 
            var sonogram = results.Item1;
            var hits = results.Item2;
            var scores = results.Item3;
            var predictedEvents = results.Item4;
            var recordingTimeSpan = results.Item5;

            DataTable dataTable = null;

            if ((predictedEvents != null) && (predictedEvents.Count != 0))
            {
                string analysisName = configDict[KeyAnalysisName];
                string fName = Path.GetFileNameWithoutExtension(fiAudioF.Name);
                foreach (AcousticEvent ev in predictedEvents)
                {
                    ev.SourceFileName = fName;
                    ev.Name = analysisName;
                    ev.SourceFileDuration = recordingTimeSpan.TotalSeconds;
                }
                //write events to a data table to return.
                dataTable = WriteEvents2DataTable(predictedEvents);
                string sortString = KeyStartSec + " ASC";
                dataTable = DataTableTools.SortTable(dataTable, sortString); //sort by start time before returning
            }

            if ((analysisSettings.EventsFile != null) && (dataTable != null))
            {
                CsvTools.DataTable2CSV(dataTable, analysisSettings.EventsFile.FullName);
            }

            if ((analysisSettings.SummaryIndicesFile != null) && (dataTable != null))
            {
                double scoreThreshold = 0.01;
                TimeSpan unitTime = TimeSpan.FromSeconds(60); //index for each time span of i minute
                var indicesDT = ConvertEvents2Indices(dataTable, unitTime, recordingTimeSpan, scoreThreshold);
                CsvTools.DataTable2CSV(indicesDT, analysisSettings.SummaryIndicesFile.FullName);
            }

            //save image of sonograms
            if (analysisSettings.ImageFile != null)
            {
                string imagePath = analysisSettings.ImageFile.FullName;
                double eventThreshold = 0.1;
                Image image = DrawSonogram(sonogram, hits, scores, predictedEvents, eventThreshold);
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
        public static System.Tuple<BaseSonogram, Double[,], double[], List<AcousticEvent>, TimeSpan>
                                                                                   Analysis(FileInfo fiSegmentOfSourceFile, Dictionary<string, string> configDict)
        {
            //set default values - ignor those set by user
            int frameSize = 1024;
            double windowOverlap = 0.0;

            int upperBandMinHz = Int32.Parse(configDict[KeyUpperfreqbandBtm]);
            int upperBandMaxHz = Int32.Parse(configDict[KeyUpperfreqbandTop]);
            int lowerBandMinHz = Int32.Parse(configDict[KeyLowerfreqbandBtm]);
            int lowerBandMaxHz = Int32.Parse(configDict[KeyLowerfreqbandTop]);
            double decibelThreshold = Double.Parse(configDict[KeyDecibelThreshold]); ;   //dB
            double intensityThreshold = Double.Parse(configDict[KeyIntensityThreshold]); //in 0-1
            double minDuration = Double.Parse(configDict[KeyMinDuration]);  // seconds
            double maxDuration = Double.Parse(configDict[KeyMaxDuration]);  // seconds
            double minPeriod = Double.Parse(configDict[KeyMinPeriod]);  // seconds
            double maxPeriod = Double.Parse(configDict[KeyMaxPeriod]);  // seconds

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
            double framesPerSecond = freqBinWidth;



            //#############################################################################################################################################
            //window    sr          frameDuration   frames/sec  hz/bin  64frameDuration hz/64bins       hz/128bins
            // 1024     22050       46.4ms          21.5        21.5    2944ms          1376hz          2752hz
            // 1024     17640       58.0ms          17.2        17.2    3715ms          1100hz          2200hz
            // 2048     17640       116.1ms          8.6         8.6    7430ms           551hz          1100hz

            //the Xcorrelation-FFT technique requires number of bins to scan to be power of 2.
            //assuming sr=17640 and window=1024, then  64 bins span 1100 Hz above the min Hz level. i.e. 500 to 1600
            //assuming sr=17640 and window=1024, then 128 bins span 2200 Hz above the min Hz level. i.e. 500 to 2700

            int upperBandMinBin = (int)Math.Round(upperBandMinHz / freqBinWidth) + 1;
            int upperBandMaxBin = (int)Math.Round(upperBandMaxHz / freqBinWidth) + 1;
            int lowerBandMinBin = (int)Math.Round(lowerBandMinHz / freqBinWidth) + 1;
            int lowerBandMaxBin = (int)Math.Round(lowerBandMaxHz / freqBinWidth) + 1;

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);
            recording.Dispose();

            //ALTERNATIVE IS TO USE THE AMPLITUDE SPECTRUM
            //var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, sr, frameSize, windowOverlap);
            //double[,] matrix = results2.Item3;  //amplitude spectrogram. Note that column zero is the DC or average energy value and can be ignored.
            //double[] avAbsolute = results2.Item1; //average absolute value over the minute recording
            ////double[] envelope = results2.Item2;
            //double windowPower = results2.Item4;

            double[] lowerArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, lowerBandMinBin, (rowCount - 1), lowerBandMaxBin);
            double[] upperArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, upperBandMinBin, (rowCount - 1), upperBandMaxBin);

            int step = (int)Math.Round(framesPerSecond); //take one second steps
            int stepCount = rowCount / step;
            int sampleLength = 64; //64 frames = 3.7 seconds. Suitable for Lewins Rail.
            double[] intensity   = new double[rowCount];
            double[] periodicity = new double[rowCount]; 

            //######################################################################
            //ii: DO THE ANALYSIS AND RECOVER SCORES
            for (int i = 0; i < stepCount; i++)
            {
                int start = step * i;
                double[] lowerSubarray = DataTools.Subarray(lowerArray, start, sampleLength);
                double[] upperSubarray = DataTools.Subarray(upperArray, start, sampleLength);
                if ((lowerSubarray.Length != sampleLength) || (upperSubarray.Length != sampleLength)) break;
                var spectrum = CrossCorrelation.CrossCorr(lowerSubarray, upperSubarray);
                int zeroCount = 3;
                for (int s = 0; s < zeroCount; s++) spectrum[s] = 0.0;  //in real data these bins are dominant and hide other frequency content
                spectrum = DataTools.NormaliseArea(spectrum);
                int maxId = DataTools.GetMaxIndex(spectrum);
                double period = 2 * sampleLength / (double)maxId / framesPerSecond; //convert maxID to period in seconds
                if ((period < minPeriod) || (period > maxPeriod)) continue;
                for (int j = 0; j < sampleLength; j++) //lay down score for sample length
                {
                    if (intensity[start + j] < spectrum[maxId]) intensity[start + j] = spectrum[maxId];
                    periodicity[start + j] = period;
                }
            }
            //######################################################################

            //iii: CONVERT SCORES TO ACOUSTIC EVENTS
            intensity = DataTools.filterMovingAverage(intensity, 5);
            List<AcousticEvent> predictedEvents = AcousticEvent.ConvertScoreArray2Events(intensity, lowerBandMinHz, upperBandMaxHz, sonogram.FramesPerSecond, freqBinWidth,
                                                                                         intensityThreshold, minDuration, maxDuration);
            CropEvents(predictedEvents, upperArray);
            var hits = new double[rowCount, colCount];

            return System.Tuple.Create(sonogram, hits, intensity, predictedEvents, tsRecordingtDuration);
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


        static Image DrawSonogram(BaseSonogram sonogram, double[,] hits, double[] scores, List<AcousticEvent> predictedEvents, double eventThreshold)
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
            if (scores != null) image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 0.5, eventThreshold));
            //if (hits != null) image.OverlayRedTransparency(hits);
            if (hits != null) image.OverlayRainbowTransparency(hits);
            if (predictedEvents.Count > 0) image.AddEvents(predictedEvents, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            return image.GetImage();
        } //DrawSonogram()




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
            Type[] types = { typeof(int), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(string), 
                             typeof(double), typeof(double) };

            var dataTable = DataTableTools.CreateTable(headers, types);
            if (predictedEvents.Count == 0) return dataTable;

            foreach (var ev in predictedEvents)
            {
                DataRow row = dataTable.NewRow();
                row[AudioAnalysisTools.AnalysisKeys.EventStartAbs] = (double)ev.TimeStart;  //Set now - will overwrite later
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
                if (configDict.ContainsKey(AnalysisKeys.DisplayColumns))
                    displayHeaders = configDict[AnalysisKeys.DisplayColumns].Split(',').ToList();
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
            if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.EventStartAbs))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.EventStartAbs + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.EventCount))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.EventCount + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.KeyRankOrder))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.KeyRankOrder + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.KeyStartMinute))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.KeyStartMinute + " ASC");
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
                if (headers[i].Equals(AnalysisKeys.KeyAvSignalAmplitude))
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
                    SegmentTargetSampleRate = AnalysisTemplate.ResampleRate
                };
            }
        }


    } //end class LewinsRail3
}
