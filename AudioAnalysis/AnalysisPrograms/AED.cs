// --------------------------------------------------------------------------------------------------------------------
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

    using Acoustics.Shared;

    using AnalysisBase;

    using AnalysisPrograms.Production;

    using AudioAnalysisTools;

    using PowerArgs;

    using log4net;

    using QutSensors.AudioAnalysis.AED;

    using TowseyLib;
    using System.Diagnostics.Contracts;
    using System.Drawing;
    using System.Drawing.Imaging;

    /// <summary>
    /// Acoustic Event Detection.
    /// </summary>
    public class AED : IAnalyser
    {
        public class Arguments : AnalyserArguments
        {

        }


        // Keys to recognise identifiers in PARAMETERS - INI file. 

        /// <summary>
        /// The key_ smallarea_ threshold.
        /// </summary>
        public const string KeyBandpassMaximum = "BANDPASS_MAXIMUM";

        /// <summary>
        /// The key_ intensity_ threshold.
        /// </summary>
        public const string KeyBandpassMinimum = "BANDPASS_MINIMUM";

        /// <summary>
        /// The key_ intensity_ threshold.
        /// </summary>
        public const string KeyIntensityThreshold = "INTENSITY_THRESHOLD";

        /// <summary>
        /// The key_ smallarea_ threshold.
        /// </summary>
        public const string KeySmallareaThreshold = "SMALLAREA_THRESHOLD";

        public const int RESAMPLE_RATE = 22050;//wtf even is this shit: 17640;

        private static readonly Color aedEventColor = Color.Red;


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
                        SegmentTargetSampleRate = RESAMPLE_RATE
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
            TowseyLib.Log.Verbosity = 1;

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

            var csvEvents = ServiceStack.Text.CsvSerializer.SerializeToCsv(events);

            File.WriteAllText(destPath + ".csv", csvEvents);

            TowseyLib.Log.WriteLine("{0} events created, saved to: {1}", events.Count, destPath + ".csv");
            ////foreach (AcousticEvent ae in events)
            ////{
            ////    LoggedConsole.WriteLine(ae.TimeStart + "," + ae.Duration + "," + ae.MinFreq + "," + ae.MaxFreq);
            ////}

            GenerateImage(destPath + ".png", result.Item1, events);
            TowseyLib.Log.WriteLine("Finished");
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
            analysisResults.SegmentStartOffset = analysisSettings.StartOfSegment.HasValue ? analysisSettings.StartOfSegment.Value : TimeSpan.Zero;
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
            {
                analysisResults.EventsFile = null;
            }

            if ((analysisSettings.IndicesFile != null) && (dataTable != null))
            {
                double scoreThreshold = 0.1;
                TimeSpan unitTime = TimeSpan.FromSeconds(60); //index for each time span of i minute
                var indicesDT = ConvertEvents2Indices(dataTable, unitTime, recordingTimeSpan, scoreThreshold);
                CsvTools.DataTable2CSV(indicesDT, analysisSettings.IndicesFile.FullName);
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
                double eventStart = (double)ev[AudioAnalysisTools.Keys.EVENT_START_ABS];
                double eventScore = (double)ev[AudioAnalysisTools.Keys.EVENT_NORMSCORE];
                int timeUnit = (int)(eventStart / unitTime.TotalSeconds);
                if (eventScore != 0.0) eventsPerUnitTime[timeUnit]++;
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
            TowseyLib.Log.WriteLine("AED start");
            IEnumerable<Oblong> oblongs = AcousticEventDetection.detectEvents(
                intensityThreshold, smallAreaThreshold, bandPassMinimum, bandPassMaximum, sonogram.Data);
            TowseyLib.Log.WriteLine("AED finished");

            SonogramConfig config = sonogram.Configuration;
            double freqBinWidth = config.fftConfig.NyquistFreq / (double)config.FreqBinCount;

            List<AcousticEvent> events =
                oblongs.Select(o => {
                    var ae = new AcousticEvent(o, config.GetFrameOffset(), freqBinWidth);
                    ae.BorderColour = aedEventColor;
                    return ae;
                }).ToList();
            TowseyLib.Log.WriteIfVerbose("AED # events: " + events.Count);
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

            return new SpectralSonogram(config, recording.GetWavReader());
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
            TowseyLib.Log.WriteIfVerbose("imagePath = " + imagePath);
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

            if (dict.ContainsKey(KeyIntensityThreshold))
            {
                intensityThreshold = Convert.ToDouble(dict[KeyIntensityThreshold]);
                propertyUsageCount++;
            }

            if (dict.ContainsKey(KeySmallareaThreshold))
            {
                smallAreaThreshold = Convert.ToInt32(dict[KeySmallareaThreshold]);
                propertyUsageCount++;
            }

            if (dict.ContainsKey(KeyBandpassMaximum))
            {
                bandPassFilterMaximum = Convert.ToDouble(dict[KeyBandpassMaximum]);
                propertyUsageCount++;
            }

            if (dict.ContainsKey(KeyBandpassMinimum))
            {
                bandPassFilterMinimum = Convert.ToDouble(dict[KeyBandpassMinimum]);
                propertyUsageCount++;
            }

            TowseyLib.Log.WriteIfVerbose("Using {0} file params and {1} AED defaults", propertyUsageCount, 4 - propertyUsageCount);
        }

        /*private static void CheckArguments(string[] args)
        {
            if (args.Length < 3)
            {
                LoggedConsole.WriteErrorLine("NUMBER OF COMMAND LINE ARGUMENTS = {0}", args.Length);
                foreach (string arg in args)
                {
                    LoggedConsole.WriteError(arg + ",  ");
                }

                LoggedConsole.WriteErrorLine("YOU REQUIRE {0} COMMAND LINE ARGUMENTS\n", 3);
                Usage();
                throw new AnalysisOptionInvalidArgumentsException();
            }

            CheckPaths(args);
        }*/

        /* /// <summary>
         /// this method checks for the existence of the two files whose paths are expected as first two arguments of the command line.
         /// </summary>
         /// <param name="args">
         /// Arguments given to program.
         /// </param>
         private static void CheckPaths(string[] args)
         {
             if (!File.Exists(args[0]))
             {
                 TowseyLib.Log.WriteLine("Cannot find recording file <" + args[0] + ">");
                 throw new AnalysisOptionInvalidPathsException();
             }

             if (!File.Exists(args[1]))
             {
                 LoggedConsole.WriteLine("Cannot find initialisation file: <" + args[1] + ">");
                 Usage();
                 throw new AnalysisOptionInvalidPathsException();
             }

             var output = args[2];
             if (!Path.HasExtension(output))
             {
                 LoggedConsole.WriteLine("the output path should really lead to a file (i.e. have an extension)");
                 Usage();
                 throw new AnalysisOptionInvalidPathsException();
             }
         }
  */
        /*private static void Usage()
        {
            LoggedConsole.WriteLine(
           @"INCORRECT COMMAND LINE.
           USAGE:
           AnalysisPrograms.exe aed recordingPath iniPath outputFileName
           where:
           recordingFileName:-(string) The path of the audio file to be processed.
           iniPath:-          (string) The path of the ini file containing all required parameters.
           outputFileName:-   (string) The name of the output file.
                                       By default, the output dir is that containing the ini file.
           ");
            

            /*
            LoggedConsole.WriteLine("The arguments for AED are: wavFile [intensityThreshold smallAreaThreshold]");
            LoggedConsole.WriteLine();

            LoggedConsole.WriteLine("wavFile:            path to .wav recording.");
            LoggedConsole.WriteLine("                    eg: \"trunk\\AudioAnalysis\\AED\\Test\\matlab\\BAC2_20071015-045040.wav\"");
            LoggedConsole.WriteLine("intensityThreshold: mandatory if smallAreaThreshold specified, otherwise default used");
            LoggedConsole.WriteLine("smallAreaThreshold: mandatory if intensityThreshold specified, otherwise default used");

            *
        }*/

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
                                 AudioAnalysisTools.Keys.EVENT_NORMSCORE,
                                 AudioAnalysisTools.Keys.MAX_HZ,
                                 AudioAnalysisTools.Keys.MIN_HZ
                               };
            //                   1                2               3              4                5              6               7              8
            Type[] types = { typeof(int), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(string), typeof(double), 
                             typeof(double), typeof(double), typeof(double), typeof(double), typeof(double) };

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

                row[AudioAnalysisTools.Keys.MAX_HZ] = (double)ev.MaxFreq;
                row[AudioAnalysisTools.Keys.MIN_HZ] = (double)ev.MinFreq;

                dataTable.Rows.Add(row);
            }
            return dataTable;
        }
    }
}