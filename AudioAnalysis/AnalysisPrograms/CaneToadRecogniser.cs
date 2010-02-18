using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;


namespace AnalysisPrograms
{
    class CaneToadRecogniser
    {
        //default parameter values
        public static string fileExtention = "wav";         //file extention to find
        public static int minHz_default = 500;              //min of freq band
        public static int maxHz_default = 1000;             //max of freq band
        public static double frameOverlap_default = 0.75;   //default=0.50; Use 0.75 for koalas //#### IMPORTANT PARAMETER
        public static double dctDuration_default = 0.5;     //duration of DCT in seconds 
        public static int minOscilFreq_default = 10;        //ignore oscillations below this threshold freq (hz)
        public static int maxOscilFreq_default = 20;        //ignore oscillations above this threshold freq (hz)
        public static double minAmplitude_default = 0.6;    //minimum acceptable value of a DCT coefficient
        public static double eventThreshold_default = 0.40; //USE THIS TO DETERMINE FP / FN trade-off for events.
        public static bool DRAW_SONOGRAMS_default = true;
        public static string recordingDir_default = @"C:\SensorNetworks\WavFiles\Canetoad\";
        public static string outputDir_default = recordingDir_default;
        public static string iniPath_default = @"C:\SensorNetworks\Output\OscillationDetection\CaneToadDetectionParams.txt";






        public static void Main(string[] args)
        {
            string title = "DETECTING CANE TOADS USING LOW FREQUENCY AMPLITUDE OSCILLATIONS";
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine(title + "\n");

            StringBuilder sb = new StringBuilder("DATE AND TIME:" + DateTime.Now + "\n");
            sb.Append(title + "\n");

            Log.Verbosity = 1;

            // DEFAULT PARAMETER VALUES #############################################################################################
            //string dirName = @"C:\SensorNetworks\WavFiles\temp\";
            int minHz = minHz_default;                      //koala range = 100-2000
            int maxHz = maxHz_default;
            double frameOverlap   = frameOverlap_default;    //default=0.50; Use 0.75 for koalas //#### IMPORTANT PARAMETER
            double dctDuration    = dctDuration_default;     //duration of DCT in seconds 
            int minOscilFreq      = minOscilFreq_default;    //ignore oscillations below this threshold freq
            int maxOscilFreq      = maxOscilFreq_default;    //ignore oscillations above this threshold freq
            double minAmplitude   = minAmplitude_default;    //minimum acceptable value of a DCT coefficient
            double eventThreshold = eventThreshold_default;  //use this to determine FP/FN trade-off.
            bool DRAW_SONOGRAMS   = DRAW_SONOGRAMS_default;
            string recordingDir   = recordingDir_default;
            string outputDir      = outputDir_default;
            string iniPath        = iniPath_default;

            //#######################################################################################################
            // DEAL WITH COMMAND LINE ARGUMENTS
            Console.WriteLine("NUMBER OF COMMAND LINE ARGUMENTS = {0}", args.Length);
            if (args.Length == 0) Usage();
            if (args.Length > 0) recordingDir = args[0];
            if (args.Length > 1) iniPath      = args[1];
            if (args.Length > 2) outputDir    = args[2];
            //eventThreshold = (dic.ContainsKey("id") ? Double.Parse(dic["id"]) : "default");

            //#######################################################################################################
            // OTHER VARS
            string resultsFile = "results.txt";  //1
            string eventsFile = "events.txt";   //2
            //MATCH STRING -search directory for matches to this file name
            string fileMatch = "*." + fileExtention;

            //#######################################################################################################

            if (!Directory.Exists(recordingDir))
            {
                Console.WriteLine("Cannot find directory <" + outputDir + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(999);
            }
            if (!Directory.Exists(outputDir))
            {
                Console.WriteLine("Cannot find output directory <" + outputDir + ">");
                outputDir = System.Environment.CurrentDirectory;
                Console.WriteLine("Have set output directory = <" + outputDir + ">");
                Console.WriteLine("Press <ENTER> key to continue.");
                Console.ReadLine();
            }
            else
                Log.WriteIfVerbose("output folder =" + outputDir);


            //set up the array of file paths.
            var fileNames = new List<string>();
            string[] names = Directory.GetFiles(recordingDir, fileMatch);
            foreach (string name in names) fileNames.Add(name);


            string str = String.Format("\nPARAMETER VALUES");
            Console.WriteLine(str);
            sb.Append(str + "\n");
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

            FileTools.WriteTextFile(outputDir + resultsFile, sb.ToString());

            FileTools.WriteTextFile(outputDir + eventsFile, "### " + title);


            //#######################################################################################################
            // predefinition of variables to prevent memory leaks?!
            BaseSonogram sonogram;
            List<AcousticEvent> accumulatedEvents;
            List<AcousticEvent> predictedEvents;
            StringBuilder sb1;
            StringBuilder sb3;
            SonogramConfig config;
            Double[,] hits;
            double[] scores;
            Image_MultiTrack image;
            int file_count = 0;
            int totalEvent_count = 0;
            foreach (string wavPath in fileNames) //for each recording
            {
                file_count++;

                accumulatedEvents = new List<AcousticEvent>();

                sb1 = new StringBuilder("\n\n" + file_count + " ###############################################################################\n");
                Log.WriteIfVerbose("\n\n" + file_count + " #################################################################################");

                if (!File.Exists(wavPath))
                {
                    Log.WriteIfVerbose("WARNING!!  CANNOT FIND FILE <" + wavPath + ">");
                    //                  sb1.Append("WARNING!!  CANNOT FIND FILE <" + wavPath + ">\n");
                    //Console.WriteLine("Press <ENTER> key to exit.");
                    //Console.ReadLine();
                    //System.Environment.Exit(999);
                    //continue;
                }
                else
                {
                    Log.WriteIfVerbose("wav File Path =" + wavPath);
                    //                  sb1.Append("wav File Path = <" + wavPath + ">\n");
                }

                //C: DETECT EVENTS USING OSCILLATION DETECTION - KEY PARAMETERS TO CHANGE for DETECT OSCILLATIONS
                //i: GET RECORDING
                using (AudioRecording recording = new AudioRecording(wavPath))
                {
                    if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();

                    //ii: MAKE SONOGRAM
                    config = new SonogramConfig(); //default values config
                    config.WindowOverlap = frameOverlap;
                    config.SourceFName = recording.FileName;
                    sonogram = new SpectralSonogram(config, recording.GetWavReader());

                    Console.WriteLine("\nSIGNAL PARAMETERS: Duration ={0}, Sample Rate={1}", sonogram.Duration, recording.SampleRate);
                    Console.WriteLine("FRAME  PARAMETERS: Frame Size= {0}, count={1}, duration={2:f1}ms, offset={3:f3}ms, fr/s={4:f1}",
                                               sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                              (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond);
                    Console.WriteLine("DCT    PARAMETERS: Duration={0}, #frames={1}, Search for oscillations>{2}, Frame overlap>={3}",
                                              dctDuration, (int)Math.Round(dctDuration * sonogram.FramesPerSecond), minOscilFreq, config.WindowOverlap);



                    ////iii: detect oscillations
                    predictedEvents = null;   //predefinition of results event list
                    scores = null;   //predefinition of score array
                    hits = null;   //predefinition of hits matrix - to superimpose on sonogram image
                    OscillationDetector.Execute((SpectralSonogram)sonogram, minHz, maxHz, dctDuration, minOscilFreq, maxOscilFreq,
                                                 minAmplitude, eventThreshold, out scores, out predictedEvents, out hits);

                    //write event count to results file.
                    totalEvent_count += predictedEvents.Count;
                    str = String.Format("EVENT COUNT = " + predictedEvents.Count);
                    Console.WriteLine(str);
                    sb1.Append(str + "\n");
                    FileTools.Append2TextFile(outputDir + resultsFile, sb1.ToString());
                    //sb1 = null;

                    //write detailed event info to events file.
                    sb3 = new StringBuilder("\n# " + file_count + " ########################################################################\n");
                    AcousticEvent.WriteEvents(predictedEvents, ref sb3);
                    FileTools.Append2TextFile(outputDir + eventsFile, sb3.ToString());
                    //sb3 = null;


                    //###############################################################################################");

                    //DISPLAY HITS ON SONOGRAM - THIS SECTION ORIGINALLY WRITTEN ONLY FOR OSCILLATION METHOD
                    if ((DRAW_SONOGRAMS) && (predictedEvents.Count > 0))
                    {
                        string imagePath = outputDir + Path.GetFileNameWithoutExtension(wavPath) + ".png";
                        //if (imagePath == null) return;
                        bool doHighlightSubband = false; bool add1kHzLines = true;

                        using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
                        using (image = new Image_MultiTrack(img))
                        {
                            //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
                            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                            image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, eventThreshold));
                            image.AddSuperimposedMatrix(hits);    //displays hits
                            image.AddEvents(predictedEvents);     //displays events
                            image.Save(imagePath);
                        }
                    }

                    // } //end using statement - BaseSonogram
                } //end using statement - AudioRecording

            }// end the foreach() loop over all recordings


            Console.WriteLine("\n\n#############################################################################");
            Console.WriteLine("TOTAL EVENT COUNT = " + totalEvent_count);
            sb = new StringBuilder();
            sb.Append("\n\n###########################################################################\n");
            sb.Append("TOTAL EVENT COUNT = " + totalEvent_count + "\n");
            FileTools.Append2TextFile(outputDir + eventsFile, sb.ToString());
            sb = null;

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }//end Main


        public static string GetDefaultParameterValues()
        {
            var sb = new StringBuilder();
//            # recording format WAV or MP3
//FILE_EXT=wav
//# min and max of the freq band to search
//MIN_HZ=500          
//MAX_HZ=1000
//# default=0.50; Use 0.75 for koalas 
//FRAME_OVERLAP=0.75
//# duration of DCT in seconds 
//DCT_DURATION=0.5
//# ignore oscillation rates below the min & above the max threshold OSCILLATIONS PER SECOND
//MIN_OSCIL_FREQ=10        
//MAX_OSCIL_FREQ=20
//# minimum acceptable value of a DCT coefficient
//MIN_AMPLITUDE=0.6
//# Event threshold - use this to determin FP / FN trade-off for events.
//EVENT_THRESHOLD=0.40
//# save a sonogram for each recording that contained a hit 
//DRAW_SONOGRAMS=true


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
