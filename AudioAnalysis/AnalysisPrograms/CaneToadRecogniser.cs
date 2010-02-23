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
    class CaneToadAnalysis
    {
        public static string key_FILE_EXT        = "FILE_EXT";
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


        /// <summary>
        /// for use in compiling a stand alone application 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            Manage_CaneToadRecogniser(args);
        }

        public static void Manage_CaneToadRecogniser(string[] args)
        {
            string title = "# DETECTING CANE TOADS USING LOW FREQUENCY AMPLITUDE OSCILLATIONS";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            StringBuilder sb = new StringBuilder(title + "\n" + date+ "\n");
            Log.WriteLine(sb.ToString());

            Log.Verbosity = 1;

            if (args.Length < 2)
            {
                Log.WriteLine("NUMBER OF COMMAND LINE ARGUMENTS = {0}", args.Length);
                Log.WriteLine("YOU REQUIRE AT LEAST {0} COMMAND LINE ARGUMENTS\n", 2);
                Usage();
            }

            //CHECK THE PATHS
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Cannot find file <" + args[0] + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
            if (!File.Exists(args[1]))
            {
                Usage();
                Console.WriteLine();
                Console.WriteLine("Cannot find initialisation file: <" + args[1] + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }

            string recordingPath = args[0];
            string iniPath = args[1];
            string outputDir = Path.GetDirectoryName(iniPath) + "\\";
            
            Log.WriteIfVerbose("\nOutput folder =" + outputDir);
            sb.Append("# RECORDING: " + Path.GetFileName(recordingPath));
            FileTools.WriteTextFile(outputDir + CaneToadAnalysis.eventsFile, sb.ToString());

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
            bool DRAW_SONOGRAMS = bool.Parse(dict[key_DRAW_SONOGRAMS]);

            var results = DetectOscillations(recordingPath, minHz, maxHz, frameOverlap, dctDuration, minOscilFreq, maxOscilFreq, minAmplitude,
                                                eventThreshold, minDuration,  maxDuration);
            var sonogram = results.Item1;
            var hits = results.Item2;
            var scores = results.Item3;
            var predictedEvents = results.Item4;
            Log.WriteLine("Event Count = " + predictedEvents.Count());

            //write event count to results file.            
            WriteEventsInfo2TextFile(predictedEvents, outputDir + CaneToadAnalysis.eventsFile);

            //if ((DRAW_SONOGRAMS) && (predictedEvents.Count > 0))
            {
                string imagePath = outputDir + Path.GetFileNameWithoutExtension(iniPath) + ".png";
                bool doHighlightSubband = false; bool add1kHzLines = true;

                using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
                using (Image_MultiTrack image = new Image_MultiTrack(img))
                {
                    //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                    image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
                    image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                    // TODO fix eventThreshold once ini file parameters are moved out
                    //image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, eventThreshold));
                    image.AddSuperimposedMatrix(hits);
                    image.AddEvents(predictedEvents);
                    image.Save(imagePath);
                }
            }

            Log.WriteLine("Finished!");
            Console.ReadLine();
        } //Manage_CaneToadRecogniser()


        public static System.Tuple<BaseSonogram, Double[,], double[], List<AcousticEvent>> DetectOscillations(string wavPath,
            int minHz, int maxHz, double frameOverlap, double dctDuration, int minOscilFreq, int maxOscilFreq, double minAmplitude,
            double eventThreshold, double minDuration, double maxDuration)
        {
            Log.WriteIfVerbose("minHz = " + minHz);
            Log.WriteIfVerbose("maxHz = " + maxHz);
            Log.WriteIfVerbose("frameOverlap = " + frameOverlap);
            Log.WriteIfVerbose("dctDuration = " + dctDuration);
            Log.WriteIfVerbose("minOscilFreq = " + minOscilFreq);
            Log.WriteIfVerbose("maxOscilFreq = " + maxOscilFreq);
            Log.WriteIfVerbose("minAmplitude = " + minAmplitude);
            Log.WriteIfVerbose("eventThreshold = " + eventThreshold);
            Log.WriteIfVerbose("minDuration = " + minDuration);
            Log.WriteIfVerbose("maxDuration = " + maxDuration);                       

            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(wavPath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();

            //ii: MAKE SONOGRAM
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.WindowOverlap = frameOverlap;
            sonoConfig.SourceFName = recording.FileName;
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());

            Log.WriteLine("SIGNAL PARAMETERS: Duration ={0}, Sample Rate={1}", sonogram.Duration, recording.SampleRate);
            Log.WriteLine("FRAME  PARAMETERS: Frame Size= {0}, count={1}, duration={2:f1}ms, offset={3:f3}ms, fr/s={4:f1}",
                                       sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                      (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond);
            Log.WriteLine("DCT    PARAMETERS: #frames={0}", (int)Math.Round(dctDuration * sonogram.FramesPerSecond));

            //iii: DETECT OSCILLATIONS
            List<AcousticEvent> predictedEvents;  //predefinition of results event list
            double[] scores;                      //predefinition of score array
            Double[,] hits;                       //predefinition of hits matrix - to superimpose on sonogram image
            OscillationDetector.Execute((SpectralSonogram)sonogram, minHz, maxHz, dctDuration, minOscilFreq, maxOscilFreq,
                                         minAmplitude, eventThreshold, minDuration, maxDuration, out scores, out predictedEvents, out hits);
            Log.WriteLine("Finished detecting oscillation events.");

            return System.Tuple.Create(sonogram, hits, scores, predictedEvents);

        }//end CaneToadRecogniser

        static void WriteEventsInfo2TextFile(List<AcousticEvent>predictedEvents, string path)
        {
            StringBuilder sb = new StringBuilder("# EVENT COUNT = " + predictedEvents.Count() + "\n\n");
            AcousticEvent.WriteEvents(predictedEvents, ref sb);
            sb.Append("#############################################################################\n");
            FileTools.Append2TextFile(path, sb.ToString());
        }

        // TODO this is duplicated with the ini file?
        public static string GetDefaultParameterValues()
        {
            var sb = new StringBuilder("# LIST OF DEFAULT PARAMETER VALUES FOR CANETOAD RECOGNITION\n");
            sb.Append("# min and max of the freq band to search\n");
            sb.Append("MIN_HZ=500   \n");       
            sb.Append("MAX_HZ=1000\n");
            sb.Append("# default=0.50; Use 0.75 for koalas \n");
            sb.Append("FRAME_OVERLAP=0.75\n");
            sb.Append("# duration of DCT in seconds \n");
            sb.Append("DCT_DURATION=0.5\n");
            sb.Append("# ignore oscillation rates below the min & above the max threshold OSCILLATIONS PER SECOND\n");
            sb.Append("MIN_OSCIL_FREQ=10 \n");       
            sb.Append("MAX_OSCIL_FREQ=20\n");
            sb.Append("# minimum acceptable value of a DCT coefficient\n");
            sb.Append("MIN_AMPLITUDE=0.6\n");
            sb.Append("# Event threshold - use this to determin FP / FN trade-off for events.\n");
            sb.Append("EVENT_THRESHOLD=0.40\n");
            sb.Append("# save a sonogram for each recording that contained a hit \n");
            sb.Append("DRAW_SONOGRAMS=true\n");
            return sb.ToString();
        }


        private static void Usage()
        {
            Console.WriteLine("USAGE:");
            Console.WriteLine("detectCanetoads.exe recordingDir iniPath outputDir");
            Console.WriteLine("where:");
            Console.WriteLine("recordingDir:-   (string) the directory containing the audio files to be processed.");
            Console.WriteLine("iniPath:-        (string) path of the ini file containing all required paramters");
            Console.WriteLine("outputDir:-      (string) the directory where output is to be placed. This is optional.");
            Console.WriteLine("                          The default output dir is that containing the ini file.");
            Console.WriteLine("");
            Console.WriteLine("Here are the default paramter values:-");
            Console.WriteLine(GetDefaultParameterValues());
            Console.WriteLine("\nPress <ENTER> key to exit.");
            Console.ReadLine();
            System.Environment.Exit(999);
        }

    }
}
