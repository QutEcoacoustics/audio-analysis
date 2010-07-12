using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;


//Here is link to wiki page containing info about how to write Analysis techniques
//https://wiki.qut.edu.au/display/mquter/Audio+Analysis+Processing+Architecture
//


namespace AnalysisPrograms
{
    class HarmonicRecogniser
    {
        //Following lines are used for the debug command line.
        //for CURLEW
        //hd C:\SensorNetworks\WavFiles\StBees\Top_Knoll_St_Bees_Curlew1_20080922-023000.wav C:\SensorNetworks\Output\HD_Curlew\Curlew_DetectionParams.txt events.txt
        //for FEMALE KOALA
        //hd C:\SensorNetworks\WavFiles\Koala_Female\HoneymoonBay_StBees_20081027-023000.wav C:\SensorNetworks\Output\HD_FemaleKoala\FemaleKoala_DetectionParams.txt events.txt

        //Keys to recognise identifiers in PARAMETERS - INI file. 
        public static string key_MIN_HZ          = "MIN_HZ";
        public static string key_MAX_HZ          = "MAX_HZ";
        public static string key_FRAME_OVERLAP   = "FRAME_OVERLAP";
        public static string key_DCT_DURATION    = "DCT_DURATION";
        public static string key_MIN_HARMONIC_PERIOD = "MIN_HARMONIC_PERIOD";
        public static string key_MAX_HARMONIC_PERIOD = "MAX_HARMONIC_PERIOD";
        public static string key_MIN_AMPLITUDE   = "MIN_AMPLITUDE";
        public static string key_DURATION        = "DURATION";
        public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_DRAW_SONOGRAMS  = "DRAW_SONOGRAMS";

        public static string eventsFile  = "events.txt"; 


        public static void Dev(string[] args)
        {
            string title = "# DETECTING SPECTRAL HARMONICS";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            Log.Verbosity = 1;
            CheckArguments(args);


            string recordingPath = args[0];
            string iniPath   = args[1];
            string outputDir = Path.GetDirectoryName(iniPath) + "\\";
            string opFName   = args[2];
            string opPath    = outputDir + opFName; 
                       
            Log.WriteIfVerbose("# Output folder =" + outputDir);
            Log.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            FileTools.WriteTextFile(opPath, date + "\n# Recording file: " + Path.GetFileName(recordingPath));

            //READ PARAMETER VALUES FROM INI FILE
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            Dictionary<string, string>.KeyCollection keys = dict.Keys;
            try
            {
                int minHz = Int32.Parse(dict[key_MIN_HZ]);
                int maxHz = Int32.Parse(dict[key_MAX_HZ]);
                double frameOverlap = Double.Parse(dict[key_FRAME_OVERLAP]);
                int minPeriod = Int32.Parse(dict[key_MIN_HARMONIC_PERIOD]);         //ignore harmonics whose period is below this threshold 
                int maxPeriod = Int32.Parse(dict[key_MAX_HARMONIC_PERIOD]);         //ignore harmonics whose period is above this threshold
                double minAmplitude = Double.Parse(dict[key_MIN_AMPLITUDE]);    //minimum acceptable value of a DCT coefficient
                double eventThreshold = Double.Parse(dict[key_EVENT_THRESHOLD]);
                double expectedDuration = Double.Parse(dict[key_DURATION]);         //expected duration of event in seconds 
                int DRAW_SONOGRAMS = Int32.Parse(dict[key_DRAW_SONOGRAMS]);    //options to draw sonogram

            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz.)", minHz, maxHz);
            Log.WriteIfVerbose("Bounds of harmonic period: " + minPeriod + " - " + maxPeriod + " Hz");
            Log.WriteIfVerbose("minAmplitude = " + minAmplitude);
            Log.WriteIfVerbose("Expected Duration: " + expectedDuration + " seconds");   
                    
//#############################################################################################################################################
            var results = Execute_HDDetect(recordingPath, minHz, maxHz, frameOverlap, minPeriod, maxPeriod, minAmplitude,
                                                eventThreshold, expectedDuration);
            Log.WriteLine("# Finished detecting spectral harmonic events.");
//#############################################################################################################################################

            var sonogram = results.Item1;
            var hits = results.Item2;
            var scores = results.Item3;
            var predictedEvents = results.Item4;
            Log.WriteLine("# Event Count = " + predictedEvents.Count());

            //write event count to results file.            
            WriteEventsInfo2TextFile(predictedEvents, opPath);

            if (DRAW_SONOGRAMS==2)
            {
                string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";
                DrawSonogram(sonogram, imagePath, hits, scores, predictedEvents, eventThreshold);
            }
            else
            if ((DRAW_SONOGRAMS==1) && (predictedEvents.Count > 0))
            {
                string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";
                DrawSonogram(sonogram, imagePath, hits, scores, predictedEvents, eventThreshold);
            }


            }
            catch (KeyNotFoundException ex)
            {
                Log.WriteLine("KEY NOT FOUND "+ ex.ToString());
            }


            Log.WriteLine("# Finished recording:- " + Path.GetFileName(recordingPath));
            //Console.ReadLine();
        } //Dev()



        public static System.Tuple<BaseSonogram, Double[,], double[], List<AcousticEvent>> Execute_HDDetect(string wavPath,
            int minHz, int maxHz, double frameOverlap, int minOscilFreq, int maxOscilFreq, double minAmplitude,
            double eventThreshold, double expectedDuration)
        {
            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(wavPath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            int sr = recording.SampleRate;

            //ii: MAKE SONOGRAM
            Log.WriteLine("Start sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.WindowOverlap = frameOverlap;
            sonoConfig.SourceFName = recording.FileName;
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            recording.Dispose();
            Log.WriteLine("Signal: Duration={0}, Sample Rate={1}", sonogram.Duration, sr);
            Log.WriteLine("Frames: Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f0}%, Offset={3:f1}ms, Frames/s={4:f1}",
                                       sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                      (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, frameOverlap);
            int binCount = (int)(maxHz / sonogram.FBinWidth) - (int)(minHz / sonogram.FBinWidth) + 1;
            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount);

            Log.WriteIfVerbose("EventThreshold=" + eventThreshold);
            Log.WriteLine("Start harmonic event detection");

            //iii: DETECT HARMONICS
            //bool normaliseDCT = true;
            List<AcousticEvent> predictedEvents;  //predefinition of results event list
            double[] scores;                      //predefinition of score array
            Double[,] hits;                       //predefinition of hits matrix - to superimpose on sonogram image
            HarmonicAnalysis.Execute((SpectralSonogram)sonogram, minHz, maxHz, minOscilFreq, maxOscilFreq,
                                         minAmplitude, eventThreshold, expectedDuration, out scores, out predictedEvents, out hits);

            return System.Tuple.Create(sonogram, hits, scores, predictedEvents);

        }//end CaneToadRecogniser


        static void DrawSonogram(BaseSonogram sonogram, string path, double[,] hits, double[] scores, List<AcousticEvent> predictedEvents, double eventThreshold)
        {
            Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            double maxScore = 2000.0;

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, eventThreshold));
                image.AddSuperimposedMatrix(hits, maxScore);
                image.AddEvents(predictedEvents);
                image.Save(path);
            }
        }


        static void WriteEventsInfo2TextFile(List<AcousticEvent>predictedEvents, string path)
        {
            StringBuilder sb = new StringBuilder("# EVENT COUNT = " + predictedEvents.Count() + "\n");
            AcousticEvent.WriteEvents(predictedEvents, ref sb);
            sb.Append("#############################################################################");
            FileTools.Append2TextFile(path, sb.ToString());
        }


        private static void CheckArguments(string[] args)
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
        private static void CheckPaths(string[] args)
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


        private static void Usage()
        {
            Console.WriteLine("INCORRECT COMMAND LINE.");
            Console.WriteLine("USAGE:");
            Console.WriteLine("HarmonicRecogniser.exe recordingPath iniPath outputFileName");
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

    } //end class HarmonicRecogniser
}
