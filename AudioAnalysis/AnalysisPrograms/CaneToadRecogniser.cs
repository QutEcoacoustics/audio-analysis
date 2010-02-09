using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools; 


namespace AudioAnalysis
{
    class CaneToadRecogniser
    {


        public static string fileExtention = "mp3";         //file extention to find
        public static int minHz_default = 500;              //min of freq band
        public static int maxHz_default = 1000;             //max of freq band
        public static double frameOverlap_default = 0.75;   //default=0.50; Use 0.75 for koalas //#### IMPORTANT PARAMETER
        public static double dctDuration_default = 0.5;     //duration of DCT in seconds 
        public static int minOscilFreq_default = 10;        //ignore oscillations below this threshold freq (hz)
        public static int maxOscilFreq_default = 20;        //ignore oscillations above this threshold freq (hz)
        public static double minAmplitude_default = 0.6;    //minimum acceptable value of a DCT coefficient
        public static double eventThreshold_default = 0.40; //USE THIS TO DETERMINE FP / FN trade-off for events.
        public static bool DRAW_SONOGRAMS = false;





        public static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("DETECTING CANE TOADS USING LOW FREQUENCY AMPLITUDE OSCILLATIONS\n");

            StringBuilder sb = new StringBuilder("DATE AND TIME:" + DateTime.Now + "\n");
            sb.Append("DETECTING CANE TOADS USING LOW FREQUENCY AMPLITUDE OSCILLATIONS\n");

            Log.Verbosity = 1;

            // DEFAULT PARAMETER VALUES #############################################################################################
            string dirName = @"C:\SensorNetworks\WavFiles\Canetoad\";
            int minHz = minHz_default;                      //koala range = 100-2000
            int maxHz = maxHz_default;
            double frameOverlap = frameOverlap_default;     //default=0.50; Use 0.75 for koalas //#### IMPORTANT PARAMETER
            double dctDuration = dctDuration_default;       //duration of DCT in seconds 
            int minOscilFreq = minOscilFreq_default;        //ignore oscillations below this threshold freq
            int maxOscilFreq = maxOscilFreq_default;        //ignore oscillations above this threshold freq
            double minAmplitude = minAmplitude_default;     //minimum acceptable value of a DCT coefficient
            double eventThreshold = eventThreshold_default; //use this to determine FP/FN trade-off.
         
            //#######################################################################################################
            // DEAL WITH COMMAND LINE ARGUMENTS
            Console.WriteLine("NUMBER OF COMMAND LINE ARGUMENTS = {0}", args.Length);
            if (args.Length == 0) Usage();
            if (args.Length > 0) dirName = args[0];
            if (args.Length > 1) fileExtention = args[1];
            if (args.Length > 2) minHz = Int32.Parse(args[2]);
            if (args.Length > 3) maxHz = Int32.Parse(args[3]);
            if (args.Length > 4) frameOverlap   = Double.Parse(args[4]);
            if (args.Length > 5) eventThreshold = Double.Parse(args[5]);

            //#######################################################################################################
            // OTHER VARS
            string outputFolder = dirName;
            string resultsFile = "results.txt";  //1
            string eventsFile  = "events.txt";   //2
            //MATCH STRING -search directory for matches to this file name
            string fileMatch     = "*." + fileExtention;

            //#######################################################################################################

            if (!Directory.Exists(dirName))
            {
                Console.WriteLine("Cannot find directory <" + outputFolder + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(999);
            }
            if (!Directory.Exists(outputFolder))
            {
                Console.WriteLine("Cannot find output directory <" + outputFolder + ">");
                outputFolder = System.Environment.CurrentDirectory;
                Console.WriteLine("Have set output directory = <" + outputFolder + ">");
                Console.WriteLine("Press <ENTER> key to continue.");
                Console.ReadLine();
            }
            else
                Log.WriteIfVerbose("output folder =" + outputFolder);


            //set up the array of file paths.
            var fileNames = new List<string>();
            string[] names = Directory.GetFiles(dirName, fileMatch);
            foreach (string name in names) fileNames.Add(name);


            string str = String.Format("\nPARAMETER VALUES");
            Console.WriteLine(str);
            sb.Append(str+"\n");
            str = String.Format("\nNUMBER OF FILES IN DIRECTORY MATCHING REGEX \\\\{0}\\\\  = {1}", fileMatch, fileNames.Count);
            Console.WriteLine(str);
            sb.Append(str + "\n");
            str = String.Format("FRAME OVERLAP = {0} (Determines time scale of sonogram)", frameOverlap);
            Console.WriteLine(str);
            sb.Append(str + "\n");
            str = String.Format("FREQ BAND: {0} - {1} Hz.", minHz, maxHz);
            Console.WriteLine(str);
            sb.Append(str + "\n");
            str = String.Format("DCT DURATION: {0} seconds. Min-max Oscill Freq: {1}-{2}", dctDuration, minOscilFreq, maxOscilFreq);
            Console.WriteLine(str);
            sb.Append(str + "\n");
            str = String.Format("DCT amplitude threshold: {0}", minAmplitude);
            Console.WriteLine(str);
            sb.Append(str + "\n");
            str = String.Format("Score Threshold: {0}. (Determines fn/fp tradeoff)", eventThreshold);
            Console.WriteLine(str);
            sb.Append(str + "\n");

            FileTools.WriteTextFile(outputFolder + resultsFile, sb.ToString());
            sb = null;


            //#######################################################################################################
            int file_count = 0;
            int totalEvent_count = 0;
            foreach (string wavPath in fileNames) //for each recording
            {
                file_count++;

                //List<AcousticEvent> accumulatedEvents = new List<AcousticEvent>();

                StringBuilder sb1 = new StringBuilder("\n\n" + file_count + " ###############################################################################################\n");
                Log.WriteIfVerbose("\n\n" + file_count + " ###############################################################################################");

                if (!File.Exists(wavPath))
                {
                    Log.WriteIfVerbose("WARNING!!  CANNOT FIND FILE <" + wavPath + ">");
                    sb1.Append("WARNING!!  CANNOT FIND FILE <" + wavPath + ">\n");
                    //Console.WriteLine("Press <ENTER> key to exit.");
                    //Console.ReadLine();
                    //System.Environment.Exit(999);
                    //continue;
                }
                else
                {
                    Log.WriteIfVerbose("wav File Path =" + wavPath);
                    sb1.Append("wav File Path = <" + wavPath + ">\n");
                }

                //C: DETECT EVENTS USING OSCILLATION DETECTION - KEY PARAMETERS TO CHANGE for DETECT OSCILLATIONS
                //i: GET RECORDING
                AudioRecording recording = new AudioRecording(wavPath);
                if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
                
                //ii: MAKE SONOGRAM
                var config = new SonogramConfig();//default values config
                config.WindowOverlap = frameOverlap; 
                config.SourceFName   = recording.FileName;
                BaseSonogram sonogram = new SpectralSonogram(config, recording.GetWavReader());
                
                Console.WriteLine("\nSIGNAL PARAMETERS: Duration ={0}, Sample Rate={1}", sonogram.Duration, recording.SampleRate);
                Console.WriteLine("FRAME  PARAMETERS: Frame Size= {0}, count={1}, duration={2:f1}ms, offset={3:f3}ms, fr/s={4:f1}",
                                           sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                          (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond);
                Console.WriteLine("DCT    PARAMETERS: Duration={0}, #frames={1}, Search for oscillations>{2}, Frame overlap>={3}",
                                          dctDuration, (int)Math.Round(dctDuration * sonogram.FramesPerSecond), minOscilFreq, config.WindowOverlap);
                
                //iii: detect oscillations
                List<AcousticEvent> predictedEvents = null; //predefinition of results event list
                double[] scores = null;                     //predefinition of score array
                Double[,] hits = null;                      //predefinition of hits matrix - to superimpose on sonogram image
                OscillationDetector.Execute((SpectralSonogram)sonogram, minHz, maxHz, dctDuration, minOscilFreq, maxOscilFreq, 
                                             minAmplitude, eventThreshold, out scores, out predictedEvents, out hits);

                //write event count to results file.
                totalEvent_count += predictedEvents.Count;
                str = String.Format("EVENT COUNT = " + predictedEvents.Count);
                Console.WriteLine(str);
                sb1.Append(str + "\n");
                FileTools.Append2TextFile(outputFolder + resultsFile, sb1.ToString());
                sb1 = null;

                //write detailed event info to events file.
                StringBuilder sb3 = new StringBuilder();
                AcousticEvent.WriteEvents(predictedEvents, ref sb3);
                FileTools.Append2TextFile(outputFolder + eventsFile, sb3.ToString());
                //accumulatedEvents.AddRange(predictedEvents);



                //###############################################################################################");
                
                //DISPLAY HITS ON SONOGRAM - THIS SECTION ORIGINALLY WRITTEN ONLY FOR OSCILLATION METHOD
                if ((DRAW_SONOGRAMS) && (predictedEvents.Count > 0))
                {
                    string imagePath = outputFolder + Path.GetFileNameWithoutExtension(wavPath) + ".png";
                    if (imagePath == null) return;
                    bool doHighlightSubband = false; bool add1kHzLines = true;
                    var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
                    image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
                    image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                    image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, eventThreshold));
                    image.AddSuperimposedMatrix(hits);    //displays hits
                    image.AddEvents(predictedEvents);     //displays events
                    image.Save(imagePath);
                    image = null; //checking for a memory leak
                }

                recording = null; //checking for a memory leak
                sonogram  = null; //checking for a memory leak

            }// end the foreach() loop over all recordings


            Console.WriteLine("\n\n###############################################################################################");
            Console.WriteLine("TOTAL EVENT COUNT = " + totalEvent_count);
            StringBuilder sb2 = new StringBuilder();
            sb.Append("\n\n###############################################################################################\n");
            sb.Append("TOTAL EVENT COUNT = " + totalEvent_count + "\n");
            FileTools.Append2TextFile(outputFolder + eventsFile, sb2.ToString());
            sb2 = null;

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }//end Main


        private static void Usage()
        {
            Console.WriteLine("USAGE:");
            Console.WriteLine("DetectCanetoads  dir  mp3|wav  minHz  maxHz  frameOverlap  eventThreshold");
            Console.WriteLine("where:");
            Console.WriteLine("dir:-            (string) the directory containing the audio files to be processed.");
            Console.WriteLine("mp3|wav:-        (string) the file extention to recognise. Only wav and mp3 files can be processed. Default= mp3");
            Console.WriteLine("minHz:-          (int)    the minimum bound of freq band to search. Default= 300");
            Console.WriteLine("maxHz:-          (int)    the maximum bound of freq band to search. Default=1300");
            Console.WriteLine("frameOverlap:-   (double) fractional overlap of frames. Default=0.75. Values in [0.0-1.0].");
            Console.WriteLine("eventThreshold:- (double) fraction of bins in freq band required to oscillate for a hit. Default=0.4. Values in [0.0-1.0].");
            Console.WriteLine("\n Only the first argument is obligatory. Default values will be used for missing arguments.");
            Console.WriteLine(" The event threshold (6th argument) is adaptive. The 6th argument sets upper limit to event threshold.");
            Console.WriteLine("\nPress <ENTER> key to exit.");
            Console.ReadLine();
            System.Environment.Exit(999);
        }

    }
}
