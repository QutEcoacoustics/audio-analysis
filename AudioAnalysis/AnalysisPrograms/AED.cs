﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AED.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Diagnostics.Contracts;
    using System.Drawing;
    using System.Drawing.Imaging;

    using Acoustics.Shared;
    using AnalysisBase;
    using AnalysisPrograms.Production;

    using AudioAnalysisTools;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.WavTools;

    using PowerArgs;
    using log4net;
    using QutSensors.AudioAnalysis.AED;

    using TowseyLibrary;

    /// <summary>
    /// Acoustic Event Detection.
    /// </summary>
    public class AED : IAnalyser
    {
        public class Arguments : AnalyserArguments
        {

        }

        public const int ResampleRate = 22050;

        private static readonly Color AedEventColor = Color.Red;


        /// <summary>
        /// Gets the name to display for the analysis.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return "AED";
            }
        }

        /// <summary>
        /// Gets Identifier.
        /// </summary>
        public string Identifier
        {
            get
            {
                return "MQUTeR.AED";
            }
        }

        /// <summary>
        /// Gets the initial (default) settings for the analysis.
        /// </summary>
        public AnalysisSettings DefaultSettings
        {
            get
            {
                return new AnalysisSettings()
                    {
                        SegmentMaxDuration = TimeSpan.FromMinutes(1),
                        SegmentMinDuration = TimeSpan.FromSeconds(20),
                        SegmentMediaType = MediaTypes.MediaTypeWav,
                        SegmentOverlapDuration = TimeSpan.Zero,
                        SegmentTargetSampleRate = ResampleRate
                    };
            }
        }

        public static Arguments Dev(object obj)
        {
            throw new NotImplementedException();
        }

        public static void Execute(AED.Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev(arguments);
            }

            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine("# Running acoustic event detection.");
            LoggedConsole.WriteLine(date);
            TowseyLibrary.Log.Verbosity = 1;

            ////CheckArguments(args);

            ////string recordingPath = args[0];
            var recordingPath = arguments.Source;

            ////string iniPath = args[1];
            var iniPath = arguments.Config;


            //string outputDir = Path.GetDirectoryName(iniPath) + "\\";
            var outputDir = iniPath.Directory;

            //string opFName = args[2];
            var outputFileName = arguments.Output;

            //string opPath = outputDir + opFName;
            var outputPath = Path.Combine(outputDir.FullName, outputFileName.FullName);

            ////Log.WriteIfVerbose("# Output folder =" + outputDir);
            ////Log.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            ////FileTools.WriteTextFile(opPath, date + "\n# Recording file: " + Path.GetFileName(recordingPath));
            LoggedConsole.WriteWarnLine("Output file writing disabled in build");

            // READ PARAMETER VALUES FROM INI FILE
            double intensityThreshold;
            double bandPassFilterMaximum;
            double bandPassFilterMinimum;
            int smallAreaThreshold;
            GetAedParametersFromConfigFileOrDefaults(
                iniPath,
                out intensityThreshold,
                out bandPassFilterMaximum,
                out bandPassFilterMinimum,
                out smallAreaThreshold);

            // TODO: fix constants
            Tuple<BaseSonogram, List<AcousticEvent>> result = Detect(
                recordingPath, intensityThreshold, smallAreaThreshold, bandPassFilterMinimum, bandPassFilterMaximum);
            List<AcousticEvent> events = result.Item2;

            string destPathBase = Path.Combine(outputDir.FullName, Path.GetFileNameWithoutExtension(recordingPath.Name));
            string destPath = destPathBase;
            var inc = 0;
            while (File.Exists(destPath + ".csv"))
            {
                inc++;
                destPath = destPathBase + "_{0:000}".FormatWith(inc);
            }

            Csv.WriteToCsv((destPath + ".csv").ToFileInfo() , events);

            TowseyLibrary.Log.WriteLine("{0} events created, saved to: {1}", events.Count, destPath + ".csv");
            ////foreach (AcousticEvent ae in events)
            ////{
            ////    LoggedConsole.WriteLine(ae.TimeStart + "," + ae.Duration + "," + ae.MinFreq + "," + ae.MaxFreq);
            ////}

            GenerateImage(destPath + ".png", result.Item1, events);
            TowseyLibrary.Log.WriteLine("Finished");
        }


        /// <summary>
        /// Run analysis using the given analysis settings.
        /// </summary>
        /// <param name="analysisSettings">
        /// The analysis Settings.
        /// </param>
        /// <returns>
        /// The results of the analysis.
        /// </returns>
        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            var fiAudioF = analysisSettings.AudioFile;
            var diOutputDir = analysisSettings.AnalysisInstanceOutputDirectory;

            var analysisResults = new AnalysisResult();
            analysisResults.AnalysisIdentifier = this.Identifier;
            analysisResults.SettingsUsed = analysisSettings;
            analysisResults.SegmentStartOffset = analysisSettings.SegmentStartOffset.HasValue ? analysisSettings.SegmentStartOffset.Value : TimeSpan.Zero;
            analysisResults.Data = null;

            // READ PARAMETER VALUES FROM INI FILE
            double intensityThreshold;
            double bandPassFilterMaximum;
            double bandPassFilterMinimum;
            int smallAreaThreshold;
            GetAedParametersFromConfigFileOrDefaults(
                analysisSettings.ConfigFile,
                out intensityThreshold,
                out bandPassFilterMaximum,
                out bandPassFilterMinimum,
                out smallAreaThreshold);

            //######################################################################
            var results = Detect(fiAudioF, intensityThreshold, smallAreaThreshold, bandPassFilterMinimum, bandPassFilterMaximum);
            //######################################################################

            if (results == null)
            {
                //nothing to process
                return analysisResults; 
            }

            var sonogram = results.Item1;
            var predictedEvents = results.Item2;

            TimeSpan recordingTimeSpan;
            using (AudioRecording recording = new AudioRecording(fiAudioF.FullName))
            {
                recordingTimeSpan = recording.Duration();
            }

            DataTable dataTable = null;

            if ((predictedEvents != null) && (predictedEvents.Count != 0))
            {
                string analysisName = analysisSettings.ConfigDict[AudioAnalysisTools.AnalysisKeys.AnalysisName];
                string fName = Path.GetFileNameWithoutExtension(fiAudioF.Name);
                foreach (AcousticEvent ev in predictedEvents)
                {
                    ev.SourceFileName = fName;
                    ev.Name = analysisName;
                    ev.SourceFileDuration = recordingTimeSpan.TotalSeconds;
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
            {
                analysisResults.EventsFile = null;
            }

            if ((analysisSettings.SummaryIndicesFile != null) && (dataTable != null))
            {
                double scoreThreshold = 0.1;
                TimeSpan unitTime = TimeSpan.FromSeconds(60); //index for each time span of i minute
                var indicesDT = ConvertEvents2Indices(dataTable, unitTime, recordingTimeSpan, scoreThreshold);
                CsvTools.DataTable2CSV(indicesDT, analysisSettings.SummaryIndicesFile.FullName);
            }
            else
            {
                analysisResults.IndicesFile = null;
            }

            //save image of sonograms
            if ((sonogram != null) && (analysisSettings.ImageFile != null))
            {
                string imagePath = analysisSettings.ImageFile.FullName;
                double eventThreshold = 0.0;
                Image image = DrawSonogram(sonogram, predictedEvents, eventThreshold);
                image.Save(imagePath, ImageFormat.Png);
                analysisResults.ImageFile = analysisSettings.ImageFile;
            }
            else
            {
                analysisResults.ImageFile = null;
            }

            analysisResults.Data = dataTable;
            analysisResults.AudioDuration = recordingTimeSpan;

            return analysisResults;
        }

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
        }

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
                double eventStart = (double)ev[AudioAnalysisTools.AnalysisKeys.EventStartAbs];
                double eventScore = (double)ev[AudioAnalysisTools.AnalysisKeys.EventNormscore];
                int timeUnit = (int)(eventStart / unitTime.TotalSeconds);
                if (eventScore != 0.0) eventsPerUnitTime[timeUnit]++;
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

        /// <summary>
        /// Detect using audio file.
        /// </summary>
        /// <param name="wavFilePath">
        /// path to audio file.
        /// </param>
        /// <param name="intensityThreshold">
        /// Intensity threshold.
        /// </param>
        /// <param name="smallAreaThreshold">
        /// Small area threshold.
        /// </param>
        /// <param name="bandPassMinimum">
        /// The band Pass Minimum.
        /// </param>
        /// <param name="bandPassMaximum">
        /// The band Pass Maximum.
        /// </param>
        /// <returns>
        /// Sonogram and Acoustic events.
        /// </returns>
        public static Tuple<BaseSonogram, List<AcousticEvent>> Detect(
            FileInfo wavFilePath,
            double intensityThreshold,
            int smallAreaThreshold,
            double bandPassMinimum,
            double bandPassMaximum)
        {
            BaseSonogram sonogram = FileToSonogram(wavFilePath.FullName);
            List<AcousticEvent> events = Detect(
                sonogram, intensityThreshold, smallAreaThreshold, bandPassMinimum, bandPassMaximum);
            return Tuple.Create(sonogram, events);
        }

        /// <summary>
        /// Detect events using sonogram.
        /// </summary>
        /// <param name="sonogram">
        /// Existing sonogram.
        /// </param>
        /// <param name="intensityThreshold">
        /// Intensity threshold.
        /// </param>
        /// <param name="smallAreaThreshold">
        /// Small area threshold.
        /// </param>
        /// <param name="bandPassMinimum">
        /// The band Pass Minimum.
        /// </param>
        /// <param name="bandPassMaximum">
        /// The band Pass Maximum.
        /// </param>
        /// <returns>
        /// Acoustic events.
        /// </returns>
        public static List<AcousticEvent> Detect(
            BaseSonogram sonogram,
            double intensityThreshold,
            int smallAreaThreshold,
            double bandPassMinimum,
            double bandPassMaximum)
        {
            TowseyLibrary.Log.WriteLine("AED start");
            IEnumerable<Oblong> oblongs = AcousticEventDetection.detectEvents(
                intensityThreshold, smallAreaThreshold, bandPassMinimum, bandPassMaximum, sonogram.Data);
            TowseyLibrary.Log.WriteLine("AED finished");

            SonogramConfig config = sonogram.Configuration;
            double freqBinWidth = config.fftConfig.NyquistFreq / (double)config.FreqBinCount;

            List<AcousticEvent> events =
                oblongs.Select(o => {
                    var ae = new AcousticEvent(o, config.GetFrameOffset(), freqBinWidth);
                    ae.BorderColour = AedEventColor;
                    return ae;
                }).ToList();
            TowseyLibrary.Log.WriteIfVerbose("AED # events: " + events.Count);
            return events;
        }

        /// <summary>
        /// The detect.
        /// </summary>
        /// <param name="wavFilePath">
        /// The wav file path.
        /// </param>
        /// <param name="intensityThreshold">
        /// The intensity threshold.
        /// </param>
        /// <param name="smallAreaThreshold">
        /// The small area threshold.
        /// </param>
        /// <returns>
        /// </returns>
        public static List<AcousticEvent> Detect(
            BaseSonogram wavFilePath, double intensityThreshold, int smallAreaThreshold)
        {
            // TODO fix constants
            return Detect(wavFilePath, intensityThreshold, smallAreaThreshold, 0, 11025);
        }

        /// <summary>
        /// The detect.
        /// </summary>
        /// <param name="wavFilePath">
        /// The wav file path.
        /// </param>
        /// <param name="intensityThreshold">
        /// The intensity threshold.
        /// </param>
        /// <param name="smallAreaThreshold">
        /// The small area threshold.
        /// </param>
        /// <returns>
        /// </returns>
        public static Tuple<BaseSonogram, List<AcousticEvent>> Detect(
            FileInfo wavFilePath, double intensityThreshold, int smallAreaThreshold)
        {
            // TODO fix constants
            return Detect(wavFilePath, intensityThreshold, smallAreaThreshold, 0, 11025);
        }

        /// <summary>
        /// Create a sonogram from a wav audio file.
        /// </summary>
        /// <param name="wavFilePath">
        /// path to audio file.
        /// </param>
        /// <returns>
        /// Sonogram from audio.
        /// </returns>
        public static BaseSonogram FileToSonogram(string wavFilePath)
        {
            var recording = new AudioRecording(wavFilePath);
            if (recording.SampleRate != 22050)
            {
                recording.ConvertSampleRate22kHz();
            }

            var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.NONE };

            return new SpectrogramStandard(config, recording.WavReader);
        }

        /// <summary>
        /// Create and save sonogram image.
        /// </summary>
        /// <param name="imagePath"> </param>
        /// <param name="sonogram">
        /// Existing sonogram.
        /// </param>
        /// <param name="events">
        /// Acoustic events.
        /// </param>
        public static void GenerateImage(
            string imagePath, BaseSonogram sonogram, List<AcousticEvent> events)
        {
            TowseyLibrary.Log.WriteIfVerbose("imagePath = " + imagePath);
            var image = new Image_MultiTrack(sonogram.GetImage(false, true));

            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            ////image.AddTrack(Image_Track.GetWavEnvelopeTrack(sonogram, image.sonogramImage.Width));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image.AddEvents(events, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            image.Save(imagePath);
        }

        /// <summary>
        /// Create and save sonogram image.
        /// </summary>
        /// <param name="wavFilePath">
        /// path to audio file.
        /// </param>
        /// <param name="outputFolder">
        /// Working directory.
        /// </param>
        /// <param name="sonogram">
        /// Existing sonogram.
        /// </param>
        /// <param name="events">
        /// Acoustic events.
        /// </param>
        public static void GenerateImage(
            string wavFilePath, string outputFolder, BaseSonogram sonogram, List<AcousticEvent> events)
        {
            string imagePath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(wavFilePath) + ".png");
            GenerateImage(imagePath, sonogram, events);
        }

        public static Image DrawSonogram(BaseSonogram sonogram, List<AcousticEvent> events, double eventThreshold)
        {
            var image = new Image_MultiTrack(sonogram.GetImage(false, true));
            
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            ////image.AddTrack(Image_Track.GetWavEnvelopeTrack(sonogram, image.sonogramImage.Width));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image.AddEvents(events, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);

            return image.GetImage();
        }

        /// <summary>
        /// The get aed parameters from config file or defaults.
        /// </summary>
        /// <param name="iniPath">
        /// The ini path.
        /// </param>
        /// <param name="intensityThreshold">
        /// The intensity threshold.
        /// </param>
        /// <param name="bandPassFilterMaximum">
        /// The band pass filter maximum.
        /// </param>
        /// <param name="bandPassFilterMinimum">
        /// The band pass filter minimum.
        /// </param>
        /// <param name="smallAreaThreshold">
        /// The small area threshold.
        /// </param>
        internal static void GetAedParametersFromConfigFileOrDefaults(
            FileInfo iniPath,
            out double intensityThreshold,
            out double bandPassFilterMaximum,
            out double bandPassFilterMinimum,
            out int smallAreaThreshold)
        {
            var config = new ConfigDictionary(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            int propertyUsageCount = 0;

            intensityThreshold = Default.intensityThreshold;
            smallAreaThreshold = Default.smallAreaThreshold;
            bandPassFilterMaximum = Default.bandPassMaxDefault;
            bandPassFilterMinimum = Default.bandPassMinDefault;

            if (dict.ContainsKey(AnalysisKeys.KeyAedIntensityThreshold))
            {
                intensityThreshold = Convert.ToDouble(dict[AnalysisKeys.KeyAedIntensityThreshold]);
                propertyUsageCount++;
            }

            if (dict.ContainsKey(AnalysisKeys.KeyAedSmallAreaThreshold))
            {
                smallAreaThreshold = Convert.ToInt32(dict[AnalysisKeys.KeyAedSmallAreaThreshold]);
                propertyUsageCount++;
            }

            if (dict.ContainsKey(AnalysisKeys.KeyBandpassMaximum))
            {
                bandPassFilterMaximum = Convert.ToDouble(dict[AnalysisKeys.KeyBandpassMaximum]);
                propertyUsageCount++;
            }

            if (dict.ContainsKey(AnalysisKeys.KeyBandpassMinimum))
            {
                bandPassFilterMinimum = Convert.ToDouble(dict[AnalysisKeys.KeyBandpassMinimum]);
                propertyUsageCount++;
            }

            TowseyLibrary.Log.WriteIfVerbose("Using {0} file params and {1} AED defaults", propertyUsageCount, 4 - propertyUsageCount);
        }

        public static DataTable WriteEvents2DataTable(List<AcousticEvent> predictedEvents)
        {
            if (predictedEvents == null) return null;
            string[] headers = { AudioAnalysisTools.AnalysisKeys.EventCount,        //1
                                 AudioAnalysisTools.AnalysisKeys.EventStartMin,    //2
                                 AudioAnalysisTools.AnalysisKeys.EventStartSec,    //3
                                 AudioAnalysisTools.AnalysisKeys.EventStartAbs,    //4
                                 AudioAnalysisTools.AnalysisKeys.KeySegmentDuration,   //5
                                 AudioAnalysisTools.AnalysisKeys.EventDuration,     //6
                                 //AudioAnalysisTools.Keys.EVENT_INTENSITY,
                                 AudioAnalysisTools.AnalysisKeys.EventName,         //7
                                 AudioAnalysisTools.AnalysisKeys.DominantFrequency,
                                 AudioAnalysisTools.AnalysisKeys.OscillationRate,
                                 AudioAnalysisTools.AnalysisKeys.EventScore,
                                 AudioAnalysisTools.AnalysisKeys.EventNormscore,
                                 AudioAnalysisTools.AnalysisKeys.MaxHz,
                                 AudioAnalysisTools.AnalysisKeys.MinHz
                               };
            //                   1                2               3              4                5              6               7              8
            Type[] types = { typeof(int), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(string), typeof(double), 
                             typeof(double), typeof(double), typeof(double), typeof(double), typeof(double) };

            var dataTable = DataTableTools.CreateTable(headers, types);
            if (predictedEvents.Count == 0) return dataTable;

            foreach (var ev in predictedEvents)
            {
                DataRow row = dataTable.NewRow();
                row[AudioAnalysisTools.AnalysisKeys.EventStartAbs] = (double)ev.TimeStart;  //Set now - will overwrite later
                row[AudioAnalysisTools.AnalysisKeys.EventStartSec] = (double)ev.TimeStart;  //EvStartSec
                row[AudioAnalysisTools.AnalysisKeys.EventDuration] = (double)ev.Duration;   //duration in seconds
                //row[AudioAnalysisTools.Keys.EVENT_INTENSITY] = (double)ev.kiwi_intensityScore;   //
                row[AudioAnalysisTools.AnalysisKeys.EventName] = (string)ev.Name;   //
                row[AudioAnalysisTools.AnalysisKeys.DominantFrequency] = (double)ev.DominantFreq;
                row[AudioAnalysisTools.AnalysisKeys.OscillationRate] = 1 / (double)ev.Periodicity;
                row[AudioAnalysisTools.AnalysisKeys.EventScore] = (double)ev.Score;      //Score
                row[AudioAnalysisTools.AnalysisKeys.EventNormscore] = (double)ev.ScoreNormalised;

                row[AudioAnalysisTools.AnalysisKeys.MaxHz] = (double)ev.MaxFreq;
                row[AudioAnalysisTools.AnalysisKeys.MinHz] = (double)ev.MinFreq;

                dataTable.Rows.Add(row);
            }
            return dataTable;
        }
    }
}