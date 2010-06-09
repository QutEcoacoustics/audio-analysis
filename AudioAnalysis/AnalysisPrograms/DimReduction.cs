using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;

namespace AnalysisPrograms
{
    class DimReduction
    {

        //CURRAWONG
        // dimred "C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged\West_Knoll_Bees_20091102-170000.mp3"  C:\SensorNetworks\Output\DIMRED\DIMRED_Params.txt  events.txt


        //Keys to recognise identifiers in PARAMETERS - INI file. 
        //public static string key_FILE_EXT    = "FILE_EXT";
        public static string key_MIN_HZ = "MIN_HZ";
        public static string key_MAX_HZ = "MAX_HZ";
        public static string key_FRAME_OVERLAP = "FRAME_OVERLAP";
        public static string key_SMOOTH_WINDOW = "SMOOTH_WINDOW";
        public static string key_MIN_DURATION = "MIN_DURATION";
        public static string key_MAX_DURATION = "MAX_DURATION";
        public static string key_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_DRAW_SONOGRAMS = "DRAW_SONOGRAMS";

        public static string eventsFile = "events.txt";


        public static void Dev(string[] args)
        {
            string title = "# DIMENSIONALITY REDUCTION OF SPECTROGRAM";
            string date = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            Log.Verbosity = 1;
            OscillationRecogniser.CheckArguments(args);


            string recordingPath = args[0];
            string iniPath = args[1];
            string outputDir = Path.GetDirectoryName(iniPath) + "\\";
            string opFName = args[2];
            string opPath = outputDir + opFName;
            Log.WriteIfVerbose("# Output folder =" + outputDir);


            //READ PARAMETER VALUES FROM INI FILE
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            Dictionary<string, string>.KeyCollection keys = dict.Keys;

            int minHz = Int32.Parse(dict[key_MIN_HZ]);
            int maxHz = Int32.Parse(dict[key_MAX_HZ]);
            double frameOverlap = Double.Parse(dict[key_FRAME_OVERLAP]);
            double smoothWindow = Double.Parse(dict[key_SMOOTH_WINDOW]);   //smoothing window (seconds) before segmentation
            double thresholdSD = Double.Parse(dict[key_THRESHOLD]);       //segmentation threshold in noise SD
            double minDuration = Double.Parse(dict[key_MIN_DURATION]);    //min duration of segment & width of smoothing window in seconds 
            //double maxDuration = Double.Parse(dict[key_MAX_DURATION]);    //max duration of segment in seconds 
            int DRAW_SONOGRAMS = Int32.Parse(dict[key_DRAW_SONOGRAMS]);   //options to draw sonogram

            Log.WriteIfVerbose("# Freq band: {0} Hz - {1} Hz.)", minHz, maxHz);
            Log.WriteIfVerbose("# Smoothing Window: {0}s.", smoothWindow);

            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(recordingPath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();

            //#############################################################################################################################################
            var results = Execute_DimRed(recording, minHz, maxHz, frameOverlap, smoothWindow, thresholdSD, minDuration);
            Log.WriteLine("# Finished detecting segments.");
            //#############################################################################################################################################

            var sonogram = results.Item1;
            //var predictedEvents = results.Item2; //contain the segments detected
            //var Q = results.Item3;
            //var oneSD_dB = results.Item4;
            //var dBThreshold = results.Item5;
            //var intensity = results.Item6;
            //Log.WriteLine("# Signal:  Duration={0}, Sample Rate={1}", sonogram.Duration, sonogram.SampleRate);
            //Log.WriteLine("# Frames:  Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f0}%, Offset={3:f1}ms, Frames/s={4:f1}",
            //                           sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
            //                          (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, frameOverlap);
            //int binCount = (int)(maxHz / sonogram.FBinWidth) - (int)(minHz / sonogram.FBinWidth) + 1;
            //Log.WriteLine("# FreqBand: {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount);
            //Log.WriteLine("# Intensity array - noise removal: Q={0:f1}dB. 1SD={1:f3}dB. Threshold={2:f3}dB.", Q, oneSD_dB, dBThreshold);
            //Log.WriteLine("# Events:  Count={0}", predictedEvents.Count());
            //int pcHIF = 100;
            //if (intensity != null)
            //{
            //    int hifCount = intensity.Count(p => p > dBThreshold); //count of high intensity frames
            //    pcHIF = 100 * hifCount / sonogram.FrameCount;
            //}

            //write event count to results file. 
            double sigDuration = sonogram.Duration.TotalSeconds;
            string fname = Path.GetFileName(recordingPath);
            //string str = String.Format("{0}\t{1}\t{2}\t{3}", fname, sigDuration, count, pcHIF);
            //StringBuilder sb = AcousticEvent.WriteEvents(predictedEvents, str);
            //FileTools.WriteTextFile(opPath, sb.ToString());

            //draw images of sonograms
            string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";
            //double min, max;
            //DataTools.MinMax(intensity, out min, out max);
            //double threshold_norm = dBThreshold / max; //min = 0.0;
            //intensity = DataTools.normalise(intensity);
            if (DRAW_SONOGRAMS > 0)
            {
                DrawSonogram(sonogram, imagePath);
            }

            Log.WriteLine("# Finished recording:- " + Path.GetFileName(recordingPath));
            Console.ReadLine();
        } //Dev()


        public static System.Tuple<BaseSonogram, double[,]> Execute_DimRed(AudioRecording recording,
            double reductionTime, int freqRedFactor, double frameOverlap, double smoothWindow, double thresholdSD, double minDuration)
        {

            //ii: MAKE SONOGRAM
            Log.WriteLine("# Start sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.WindowOverlap = frameOverlap;
            sonoConfig.SourceFName = recording.FileName;
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            recording.Dispose();

            int timeRedFactor = (int)Math.Floor(reductionTime * sonogram.FramesPerSecond);
            int freqBinCount  = sonogram.Configuration.FreqBinCount;
            int frameCount    = sonogram.FrameCount;

            int timeReducedCount = frameCount   / timeRedFactor;
            int freqReducedCount = freqBinCount / freqRedFactor;

            var reducedMatrix = new double[timeReducedCount, freqReducedCount];


            var tuple2 = System.Tuple.Create(sonogram, reducedMatrix);
            return tuple2;
        }//end Execute_DimRed


        public static void DrawSonogram(BaseSonogram sonogram, string path)
        {
            Log.WriteLine("# Start sono image.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            //double maxScore = 50.0; //assumed max posisble oscillations per second

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
                image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                //image.AddTrack(Image_Track.GetScoreTrack(segmentation, 0.0, 1.0, eventThreshold));
                //image.AddEvents(predictedEvents);
                image.Save(path);
            }
        }


        public static void CheckArguments(string[] args)
        {
            if (args.Length < 3)
            {
                Log.WriteLine("NUMBER OF COMMAND LINE ARGUMENTS = {0}", args.Length);
                foreach (string arg in args) Log.WriteLine(arg + "  ");
                Log.WriteLine("YOU REQUIRE {0} COMMAND LINE ARGUMENTS\n", 3);
                Usage();
            }
            CheckPaths(args);
        }

        /// <summary>
        /// this method checks for the existence of the two files whose paths are expected as first two arguments of the command line.
        /// </summary>
        /// <param name="args"></param>
        public static void CheckPaths(string[] args)
        {
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Cannot find recording file <" + args[0] + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
            if (!File.Exists(args[1]))
            {
                Console.WriteLine("Cannot find initialisation file: <" + args[1] + ">");
                Usage();
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
        }


        public static void Usage()
        {
            Console.WriteLine("INCORRECT COMMAND LINE.");
            Console.WriteLine("USAGE:");
            Console.WriteLine("OscillationDetection.exe recordingPath iniPath outputFileName");
            Console.WriteLine("where:");
            Console.WriteLine("recordingFileName:-(string) The path of the audio file to be processed.");
            Console.WriteLine("iniPath:-          (string) The path of the ini file containing all required parameters.");
            Console.WriteLine("outputFileName:-   (string) The name of the output file.");
            Console.WriteLine("                            By default, the output dir is that containing the ini file.");
            Console.WriteLine("");
            Console.WriteLine("\nPress <ENTER> key to exit.");
            Console.ReadLine();
            System.Environment.Exit(1);
        }   
    
    
    } //end class
}
