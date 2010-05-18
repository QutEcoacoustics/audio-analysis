// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AED.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the AED type.
// </summary>
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

    /// <summary>Acoustic Event Detection.</summary>
    public class AED
    {
        // Keys to recognise identifiers in PARAMETERS - INI file. 
        public static string key_INTENSITY_THRESHOLD = "INTENSITY_THRESHOLD";
        public static string key_SMALLAREA_THRESHOLD = "SMALLAREA_THRESHOLD";

        /// <summary>Detection method for development.
        /// </summary>
        /// <param name="args">
        /// Arguments given to program.
        /// </param>
        public static void Dev(string[] args)
        {
            var date = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine("# Running acoustic event detection.");
            Log.WriteLine(date);
            Log.Verbosity = 1;

            CheckArguments(args);

            var recordingPath = args[0];
            var iniPath = args[1];
            var outputDir = Path.GetDirectoryName(iniPath) + "\\";
            var opFName = args[2];
            var opPath = outputDir + opFName;

            Log.WriteIfVerbose("# Output folder =" + outputDir);
            Log.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            FileTools.WriteTextFile(opPath, date + "\n# Recording file: " + Path.GetFileName(recordingPath));

            // READ PARAMETER VALUES FROM INI FILE
            var config = new Configuration(iniPath);
            var dict = config.GetTable();

            var intensityThreshold = Default.intensityThreshold;
            var smallAreaThreshold = Default.smallAreaThreshold;

            if (dict.ContainsKey(key_INTENSITY_THRESHOLD) && dict.ContainsKey(key_SMALLAREA_THRESHOLD))
            {
                intensityThreshold = Convert.ToDouble(dict[key_INTENSITY_THRESHOLD]);
                smallAreaThreshold = Convert.ToInt32(dict[key_SMALLAREA_THRESHOLD]);
            }
            else
            {
                Log.WriteIfVerbose("Using AED defaults");
            }

            var result = Detect(recordingPath, intensityThreshold, smallAreaThreshold);
            var events = result.Item2;

            Console.WriteLine();
            foreach (var ae in events)
            {
                Console.WriteLine(ae.StartTime + "," + ae.Duration + "," + ae.MinFreq + "," + ae.MaxFreq);
            }

            Console.WriteLine();

            GenerateImage(recordingPath, outputDir, result.Item1, events);
            Log.WriteLine("Finished");
        }

        /// <summary>
        /// Detect using audio file.
        /// </summary>
        /// <param name="wavFilePath">path to audio file.</param>
        /// <param name="intensityThreshold">Intensity threshold.</param>
        /// <param name="smallAreaThreshold">Small area threshold.</param>
        /// <returns>Sonogram and Acoustic events.</returns>
        public static Tuple<BaseSonogram, List<AcousticEvent>> Detect(string wavFilePath, double intensityThreshold, int smallAreaThreshold)
        {
            var sonogram = FileToSonogram(wavFilePath);
            var events = Detect(sonogram, intensityThreshold, smallAreaThreshold);
            return Tuple.Create(sonogram, events);
        }

        /// <summary>
        /// Detect events using sonogram.
        /// </summary>
        /// <param name="sonogram">Existing sonogram.</param>
        /// <param name="intensityThreshold">Intensity threshold.</param>
        /// <param name="smallAreaThreshold">Small area threshold.</param>
        /// <returns>Acoustic events.</returns>
        public static List<AcousticEvent> Detect(BaseSonogram sonogram, double intensityThreshold, int smallAreaThreshold)
        {
            Log.WriteLine("intensityThreshold = " + intensityThreshold);
            Log.WriteLine("smallAreaThreshold = " + smallAreaThreshold);

            Log.WriteLine("AED start");
            var oblongs = AcousticEventDetection.detectEvents(intensityThreshold, smallAreaThreshold, sonogram.Data);
            Log.WriteLine("AED finished");

            var config = sonogram.Configuration;
            var freqBinWidth = config.fftConfig.NyquistFreq / (double)config.FreqBinCount;

            var events = oblongs.Select(o => new AcousticEvent(o, config.GetFrameOffset(), freqBinWidth)).ToList();
            Log.WriteIfVerbose("AED # events: " + events.Count);
            return events;
        }

        /// <summary>
        /// Create a sonogram from a wav audio file.
        /// </summary>
        /// <param name="wavFilePath">path to audio file.</param>
        /// <returns>Sonogram from audio.</returns>
        public static BaseSonogram FileToSonogram(string wavFilePath)
        {
            var recording = new AudioRecording(wavFilePath);
            if (recording.SampleRate != 22050)
            {
                recording.ConvertSampleRate22kHz();
            }

            var config = new SonogramConfig
                {
                    NoiseReductionType = NoiseReductionType.NONE
                };

            return new SpectralSonogram(config, recording.GetWavReader());
        }

        /// <summary>
        /// Create and save sonogram image.
        /// </summary>
        /// <param name="wavFilePath">path to audio file.</param>
        /// <param name="outputFolder">Working directory.</param>
        /// <param name="sonogram">Existing sonogram.</param>
        /// <param name="events">Acoustic events.</param>
        public static void GenerateImage(string wavFilePath, string outputFolder, BaseSonogram sonogram, List<AcousticEvent> events)
        {
            var imagePath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(wavFilePath) + ".png");
            Log.WriteIfVerbose("imagePath = " + imagePath);
            var image = new Image_MultiTrack(sonogram.GetImage(false, true));
            ////image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            ////image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.Image.Width));
            ////image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image.AddEvents(events);
            image.Save(imagePath);
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
        /// <param name="args">Arguments given to program.</param>
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
            Console.WriteLine("iniPath:-          (string) The path of the ini file containing all required parameters.");
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
    }
}
