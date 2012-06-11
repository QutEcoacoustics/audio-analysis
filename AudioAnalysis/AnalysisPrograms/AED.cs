// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AED.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using AudioAnalysisTools;

    using QutSensors.AudioAnalysis.AED;

    using TowseyLib;

    /// <summary>
    /// Acoustic Event Detection.
    /// </summary>
    public class AED
    {
        // Keys to recognise identifiers in PARAMETERS - INI file. 
        #region Constants and Fields

        /// <summary>
        /// The key_ smallare a_ threshold.
        /// </summary>
        public static string KeyBandpassMaximum = "BANDPASS_MAXIMUM";

        /// <summary>
        /// The key_ intensit y_ threshold.
        /// </summary>
        public static string KeyBandpassMinimum = "BANDPASS_MINIMUM";

        /// <summary>
        /// The key_ intensit y_ threshold.
        /// </summary>
        public static string KeyIntensityThreshold = "INTENSITY_THRESHOLD";

        /// <summary>
        /// The key_ smallare a_ threshold.
        /// </summary>
        public static string KeySmallareaThreshold = "SMALLAREA_THRESHOLD";

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
            Log.WriteLine("AED start");
            IEnumerable<Oblong> oblongs = AcousticEventDetection.detectEvents(
                intensityThreshold, smallAreaThreshold, bandPassMinimum, bandPassMaximum, sonogram.Data);
            Log.WriteLine("AED finished");

            SonogramConfig config = sonogram.Configuration;
            double freqBinWidth = config.fftConfig.NyquistFreq / (double)config.FreqBinCount;

            List<AcousticEvent> events =
                oblongs.Select(o => new AcousticEvent(o, config.GetFrameOffset(), freqBinWidth)).ToList();
            Log.WriteIfVerbose("AED # events: " + events.Count);
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
            Log.WriteLine("# Running acoustic event detection.");
            Log.WriteLine(date);
            Log.Verbosity = 1;

            CheckArguments(args);

            string recordingPath = args[0];
            string iniPath = args[1];
            string outputDir = Path.GetDirectoryName(iniPath) + "\\";
            string opFName = args[2];
            string opPath = outputDir + opFName;

            Log.WriteIfVerbose("# Output folder =" + outputDir);
            Log.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            FileTools.WriteTextFile(opPath, date + "\n# Recording file: " + Path.GetFileName(recordingPath));

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

            Console.WriteLine();
            foreach (AcousticEvent ae in events)
            {
                Console.WriteLine(ae.TimeStart + "," + ae.Duration + "," + ae.MinFreq + "," + ae.MaxFreq);
            }

            Console.WriteLine();

            GenerateImage(recordingPath, outputDir, result.Item1, events);
            Log.WriteLine("Finished");
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
            Log.WriteIfVerbose("imagePath = " + imagePath);
            var image = new Image_MultiTrack(sonogram.GetImage(false, true));

            ////image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            ////image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.Image.Width));
            ////image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image.AddEvents(events, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount); 
            image.Save(imagePath);
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

            Log.WriteIfVerbose("Using {0} file params and {1} AED defaults", propertyUsageCount, 4 - propertyUsageCount);
        }

        private static void CheckArguments(string[] args)
        {
            if (args.Length < 3)
            {
                Log.WriteLine("NUMBER OF COMMAND LINE ARGUMENTS = {0}", args.Length);
                foreach (string arg in args)
                {
                    Log.WriteLine(arg + "  ");
                }

                Log.WriteLine("YOU REQUIRE {0} COMMAND LINE ARGUMENTS\n", 3);
                Usage();
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
                Console.WriteLine("Cannot find recording file <" + args[0] + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                Environment.Exit(1);
            }

            if (!File.Exists(args[1]))
            {
                Console.WriteLine("Cannot find initialisation file: <" + args[1] + ">");
                Usage();
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                Environment.Exit(1);
            }
        }

        private static void Usage()
        {
            Console.WriteLine("INCORRECT COMMAND LINE.");
            Console.WriteLine("USAGE:");
            Console.WriteLine("AnalysisPrograms.exe aed recordingPath iniPath outputFileName");
            Console.WriteLine("where:");
            Console.WriteLine("recordingFileName:-(string) The path of the audio file to be processed.");
            Console.WriteLine(
                "iniPath:-          (string) The path of the ini file containing all required parameters.");
            Console.WriteLine("outputFileName:-   (string) The name of the output file.");
            Console.WriteLine("                            By default, the output dir is that containing the ini file.");
            Console.WriteLine();
            Console.WriteLine("\nPress <ENTER> key to exit.");
            Console.ReadLine();
            Environment.Exit(1);

            /*
            Console.WriteLine("The arguments for AED are: wavFile [intensityThreshold smallAreaThreshold]");
            Console.WriteLine();

            Console.WriteLine("wavFile:            path to .wav recording.");
            Console.WriteLine("                    eg: \"trunk\\AudioAnalysis\\AED\\Test\\matlab\\BAC2_20071015-045040.wav\"");
            Console.WriteLine("intensityThreshold: mandatory if smallAreaThreshold specified, otherwise default used");
            Console.WriteLine("smallAreaThreshold: mandatory if intensityThreshold specified, otherwise default used");
            Environment.Exit(1);
            */
        }

        #endregion
    }
}