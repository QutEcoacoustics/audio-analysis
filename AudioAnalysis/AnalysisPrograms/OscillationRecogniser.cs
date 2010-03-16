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

//HERE ARE COMMAND LINE ARGUMENTS TO PLACE IN START OPTIONS - PROPERTIES PAGE
//     canetoad  C:\SensorNetworks\WavFiles\temp1\ C:\SensorNetworks\Output\OscillationDetection\CaneToadDetectionParams.txt
//


namespace AnalysisPrograms
{
    class OscillationRecogniser
    {
        //Following lines are used for the debug command line.
        //GECKO
        //od "C:\SensorNetworks\WavFiles\Gecko\Gecko05012010\DM420008_26m_00s__28m_00s - Gecko.mp3" C:\SensorNetworks\Output\OD_GeckoTrain\Gecko_DetectionParams.txt events.txt
        //KOALA MALE EXHALE
        //od "C:\SensorNetworks\WavFiles\Koala_Male\Recordings\KoalaMale\LargeTestSet\WestKnoll_Bees_20091103-190000.wav" C:\SensorNetworks\Output\OD_KoalaMaleExhale\KoalaMaleExhale_DetectionParams.txt events.txt
        //od "C:\SensorNetworks\WavFiles\Koala_Male\SmallTestSet\HoneymoonBay_StBees_20080905-001000.wav" C:\SensorNetworks\Output\OD_KoalaMaleExhale\KoalaMaleExhale_DetectionParams.txt events.txt
        //KOALA MALE FOREPLAY
        //od "C:\SensorNetworks\WavFiles\Koala_Male\SmallTestSet\HoneymoonBay_StBees_20080905-001000.wav" C:\SensorNetworks\Output\OD_KoalaMaleForeplay_LargeTestSet\KoalaMaleForeplay_DetectionParams.txt events.txt
        //BRIDGE CREEK
        //od "C:\SensorNetworks\WavFiles\Length1_2_4_8_16mins\BridgeCreek_1min.wav" C:\SensorNetworks\Output\TestWavDuration\DurationTest_DetectionParams.txt events.txt




        //Keys to recognise identifiers in PARAMETERS - INI file. 
        //public static string key_FILE_EXT        = "FILE_EXT";
        public static string key_MIN_HZ          = "MIN_HZ";
        public static string key_MAX_HZ          = "MAX_HZ";
        public static string key_FRAME_OVERLAP   = "FRAME_OVERLAP";
        public static string key_DCT_DURATION    = "DCT_DURATION";
        public static string key_MIN_OSCIL_FREQ  = "MIN_OSCIL_FREQ";
        public static string key_MAX_OSCIL_FREQ  = "MAX_OSCIL_FREQ";
        public static string key_MIN_AMPLITUDE   = "MIN_AMPLITUDE";
        public static string key_MIN_DURATION    = "MIN_DURATION";
        public static string key_MAX_DURATION    = "MAX_DURATION";
        public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_DRAW_SONOGRAMS  = "DRAW_SONOGRAMS";

        public static string eventsFile  = "events.txt"; 


        public static void Dev(string[] args)
        {
            string title = "# DETECTING LOW FREQUENCY AMPLITUDE OSCILLATIONS";
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

            int minHz = Int32.Parse(dict[key_MIN_HZ]);
            int maxHz = Int32.Parse(dict[key_MAX_HZ]);
            double frameOverlap = Double.Parse(dict[key_FRAME_OVERLAP]);
            double dctDuration = Double.Parse(dict[key_DCT_DURATION]);     //duration of DCT in seconds 
            int minOscilFreq = Int32.Parse(dict[key_MIN_OSCIL_FREQ]); //ignore oscillations below this threshold freq
            int maxOscilFreq = Int32.Parse(dict[key_MAX_OSCIL_FREQ]);    //ignore oscillations above this threshold freq
            double minAmplitude = Double.Parse(dict[key_MIN_AMPLITUDE]);    //minimum acceptable value of a DCT coefficient
            double eventThreshold = Double.Parse(dict[key_EVENT_THRESHOLD]);
            double minDuration = Double.Parse(dict[key_MIN_DURATION]);     //min duration of event in seconds 
            double maxDuration = Double.Parse(dict[key_MAX_DURATION]);     //max duration of event in seconds 
            int DRAW_SONOGRAMS = Int32.Parse(dict[key_DRAW_SONOGRAMS]);    //options to draw sonogram

            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz.)", minHz, maxHz);
            Log.WriteIfVerbose("Oscill bounds: " + minOscilFreq + " - " + maxOscilFreq + " Hz");
            Log.WriteIfVerbose("minAmplitude = " + minAmplitude);
            Log.WriteIfVerbose("Duration bounds: " + minDuration + " - " + maxDuration + " seconds");   
                    
//#############################################################################################################################################
            var results = Execute_ODDetect(recordingPath, minHz, maxHz, frameOverlap, dctDuration, minOscilFreq, maxOscilFreq, minAmplitude,
                                                eventThreshold, minDuration,  maxDuration);
            Log.WriteLine("# Finished detecting oscillation events.");
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

            Log.WriteLine("# Finished recording:- " + Path.GetFileName(recordingPath));
            Console.ReadLine();
        } //Dev()


        public static System.Tuple<BaseSonogram, Double[,], double[], List<AcousticEvent>> Execute_ODDetect(string wavPath,
            int minHz, int maxHz, double frameOverlap, double dctDuration, int minOscilFreq, int maxOscilFreq, double minAmplitude,
            double eventThreshold, double minDuration, double maxDuration)
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

            Log.WriteIfVerbose("DctDuration=" + dctDuration + "sec.  (# frames=" + (int)Math.Round(dctDuration * sonogram.FramesPerSecond) + ")");
            Log.WriteIfVerbose("EventThreshold=" + eventThreshold);
            Log.WriteLine("Start OD event detection");

            //iii: DETECT OSCILLATIONS
            List<AcousticEvent> predictedEvents;  //predefinition of results event list
            double[] scores;                      //predefinition of score array
            Double[,] hits;                       //predefinition of hits matrix - to superimpose on sonogram image
            OscillationAnalysis.Execute((SpectralSonogram)sonogram, minHz, maxHz, dctDuration, minOscilFreq, maxOscilFreq,
                                         minAmplitude, eventThreshold, minDuration, maxDuration, out scores, out predictedEvents, out hits);

            return System.Tuple.Create(sonogram, hits, scores, predictedEvents);

        }//end CaneToadRecogniser


        static void DrawSonogram(BaseSonogram sonogram, string path, double[,] hits, double[] scores, List<AcousticEvent> predictedEvents, double eventThreshold)
        {
            Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
                image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, eventThreshold));
                image.AddSuperimposedMatrix(hits);
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


        // TODO this is duplicated with the ini file?
        //public static string GetDefaultParameterValues()
        //{
        //    var sb = new StringBuilder("# LIST OF DEFAULT PARAMETER VALUES FOR CANETOAD RECOGNITION\n");
        //    sb.Append("# min and max of the freq band to search\n");
        //    sb.Append("MIN_HZ=500   \n");       
        //    sb.Append("MAX_HZ=1000\n");
        //    sb.Append("# default=0.50; Use 0.75 for koalas \n");
        //    sb.Append("FRAME_OVERLAP=0.75\n");
        //    sb.Append("# duration of DCT in seconds \n");
        //    sb.Append("DCT_DURATION=0.5\n");
        //    sb.Append("# ignore oscillation rates below the min & above the max threshold OSCILLATIONS PER SECOND\n");
        //    sb.Append("MIN_OSCIL_FREQ=10 \n");       
        //    sb.Append("MAX_OSCIL_FREQ=20\n");
        //    sb.Append("# minimum acceptable value of a DCT coefficient\n");
        //    sb.Append("MIN_AMPLITUDE=0.6\n");
        //    sb.Append("# Event threshold - use this to determin FP / FN trade-off for events.\n");
        //    sb.Append("EVENT_THRESHOLD=0.40\n");
        //    sb.Append("# save a sonogram for each recording that contained a hit \n");
        //    sb.Append("DRAW_SONOGRAMS=true\n");
        //    return sb.ToString();
        //}


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
