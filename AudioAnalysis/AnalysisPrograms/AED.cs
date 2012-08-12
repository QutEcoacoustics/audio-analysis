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

    using AudioAnalysisTools;

    using QutSensors.AudioAnalysis.AED;

    using ServiceStack.Text;

    using TowseyLib;

    using log4net;

    /// <summary>
    /// Acoustic Event Detection.
    /// </summary>
    public class AED : IAnalyser
    {
        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // Keys to recognise identifiers in PARAMETERS - INI file. 
        #region Constants and Fields

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

        #endregion

        #region Public Methods

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
            string wavFilePath, 
            double intensityThreshold, 
            int smallAreaThreshold, 
            double bandPassMinimum, 
            double bandPassMaximum)
        {
            BaseSonogram sonogram = FileToSonogram(wavFilePath);
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
                oblongs.Select(o => new AcousticEvent(o, config.GetFrameOffset(), freqBinWidth)).ToList();
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
            string wavFilePath, double intensityThreshold, int smallAreaThreshold)
        {
            // TODO fix constants
            return Detect(wavFilePath, intensityThreshold, smallAreaThreshold, 0, 11025);
        }

        /// <summary>
        /// Detection method for development.
        /// </summary>
        /// <param name="args">
        /// Arguments given to program.
        /// </param>
        public static void Dev(string[] args)
        {
            string date = "# DATE AND TIME: " + DateTime.Now;
            Log.Info("# Running acoustic event detection.");
            Log.Info(date);
            TowseyLib.Log.Verbosity = 1;

            CheckArguments(args);

            string recordingPath = args[0];
            string iniPath = args[1];
            string outputDir = Path.GetDirectoryName(iniPath) + "\\";
            string opFName = args[2];
            string opPath = outputDir + opFName;

            ////Log.WriteIfVerbose("# Output folder =" + outputDir);
            ////Log.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            ////FileTools.WriteTextFile(opPath, date + "\n# Recording file: " + Path.GetFileName(recordingPath));
            TowseyLib.Log.WriteLine("WARN: output file writing disabled in build");

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

            string destPathBase = outputDir + Path.GetFileNameWithoutExtension(recordingPath);
            string destPath = destPathBase;
            var inc = 0;
            while (File.Exists(destPath + ".csv"))
            {
                inc++;
                destPath = destPathBase + "_{0:000}".FormatWith(inc);
            }

            var csvEvents = CsvSerializer.SerializeToCsv(events);
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

            ////image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, image.));
            ////image.AddTrack(Image_Track.GetWavEnvelopeTrack(sonogram, image.sonogramImage.Width));
            ////image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
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

        #endregion

        #region Methods

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
            string iniPath, 
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

        private static void CheckArguments(string[] args)
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
        }

        /// <summary>
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

        private static void Usage()
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

            */
        }

        #endregion

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
                        SegmentMediaType = MediaTypes.MediaTypeWav,
                        SegmentOverlapDuration = TimeSpan.Zero
                    };
            }
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
            throw new NotImplementedException();
        }

        public Tuple<DataTable, DataTable> ProcessCsvFile(FileInfo fiCsvFile, FileInfo fiConfigFile)
        {
            throw new NotImplementedException();
        }

        public DataTable ConvertEvents2Indices(DataTable dt, TimeSpan unitTime, TimeSpan timeDuration, double scoreThreshold)
        {
            throw new NotImplementedException();
        }
    }
}