using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;
using AudioTools.AudioUtlity;
using QutSensors.Shared.LogProviders;




//Here is link to wiki page containing info about how to write Analysis techniques
//https://wiki.qut.edu.au/display/mquter/Audio+Analysis+Processing+Architecture

//HERE ARE COMMAND LINE ARGUMENTS TO PLACE IN START OPTIONS - PROPERTIES PAGE
//od  "C:\SensorNetworks\WavFiles\Canetoad\DM420010_128m_00s__130m_00s - Toads.mp3" C:\SensorNetworks\Output\OD_CaneToad\CaneToad_DetectionParams.txt events.txt
//


namespace AnalysisPrograms
{
    class KiwiRecogniser
    {
        //Following lines are used for the debug command line.
        //CANETOAD
        //od  "C:\SensorNetworks\WavFiles\Canetoad\DM420011\DM420011_00m_00s__02m_00s.wav" C:\SensorNetworks\Output\OD_CaneToad2_DM420011_NoFilter\OD_CaneToad2_DM420011_NoFilter_Params.txt events.txt
        //od  "C:\SensorNetworks\WavFiles\Canetoad\\FromPaulRoe\canetoad_CubberlaCreek_100529.WAV"  C:\SensorNetworks\Output\OD_CaneToad_PRoe\OD_CaneToad_Params.txt events.txt
        // kiwi "C:\SensorNetworks\WavFiles\Kiwi\Samples\lsk-female\TOWER_20091107_07200_21.LSK.F.wav"  "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt" kiwiEvents.txt
        // kiwi "C:\SensorNetworks\WavFiles\Kiwi\Samples\lsk-male\TOWER_20091112_072000_25.LSK.M.wav"  "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt" kiwiEvents.txt
        // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_Cropped.wav"  "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt" kiwiEvents.txt


        

        //Keys to recognise identifiers in PARAMETERS - INI file. 
        //public static string key_FILE_EXT        = "FILE_EXT";
        //public static string key_DO_SEGMENTATION = "DO_SEGMENTATION";
        public static string key_MIN_HZ_MALE   = "MIN_HZ_MALE";
        public static string key_MAX_HZ_MALE   = "MAX_HZ_MALE";
        public static string key_MIN_HZ_FEMALE = "MIN_HZ_FEMALE";
        public static string key_MAX_HZ_FEMALE = "MAX_HZ_FEMALE";
        public static string key_FRAME_OVERLAP = "FRAME_OVERLAP";
        public static string key_DCT_DURATION    = "DCT_DURATION";
        public static string key_DCT_THRESHOLD   = "DCT_THRESHOLD";
        public static string key_MIN_OSCIL_FREQ  = "MIN_OSCIL_FREQ";
        public static string key_MAX_OSCIL_FREQ  = "MAX_OSCIL_FREQ";
        public static string key_MIN_DURATION    = "MIN_DURATION";
        public static string key_MAX_DURATION    = "MAX_DURATION";
        public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_DRAW_SONOGRAMS  = "DRAW_SONOGRAMS";

        public static string eventsFile  = "kiwiEvents.txt";


        /// <summary>
        /// a set of parameters derived from ini file
        /// </summary>
        public struct KiwiParams
        {
            public int minHzMale, maxHzMale, minHzFemale, maxHzFemale;
            public double frameOverlap, dctDuration, dctThreshold, minOscilFreq, maxOscilFreq, minDuration, maxDuration, eventThreshold; //spectral indices
            public int DRAW_SONOGRAMS;

            public KiwiParams(int _minHzMale, int _maxHzMale, int _minHzFemale, int _maxHzFemale, int _frameOverlap, double _dctDuration, double _dctThreshold,
                              double _minOscilFreq, double _maxOscilFreq, double _minDuration, double _maxDuration, double _eventThreshold, int _DRAW_SONOGRAMS)
            {
                minHzMale = _minHzMale;
                maxHzMale = _maxHzMale;
                minHzFemale = _minHzFemale;
                maxHzFemale = _maxHzFemale;
                frameOverlap = _frameOverlap;
                dctDuration = _dctDuration;
                dctThreshold = _dctThreshold;
                minOscilFreq = _minOscilFreq;
                maxOscilFreq = _maxOscilFreq;
                minDuration = _minDuration;
                maxDuration = _maxDuration;
                eventThreshold = _eventThreshold;
                DRAW_SONOGRAMS = _DRAW_SONOGRAMS; //av length of clusters > 1 frame.
            }
        }





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
            string outputDir = Path.GetDirectoryName(iniPath) + "\\"; //output directory is the one in which ini file is located.
            string opFName   = args[2];
            string opPath    = outputDir + opFName;
            Log.WriteIfVerbose("# Output folder =" + outputDir);
                       

            //READ PARAMETER VALUES FROM INI FILE
            KiwiParams kiwiParams = ReadIniFile(iniPath);

            double startMinutes = 0.0;
            double durationMinutes = 7.5;
            int overlap = 10000;
            AudioRecording recording = GetSegmentFromAudioRecording(recordingPath, startMinutes, durationMinutes, overlap);
                    
//#############################################################################################################################################
            Log.WriteLine("# Looking for kiwi oscillation events.");
            var results = Execute_KiwiDetect(recording, kiwiParams.minHzMale, kiwiParams.maxHzMale, kiwiParams.minHzFemale, kiwiParams.maxHzFemale, kiwiParams.frameOverlap,
                                             kiwiParams.dctDuration, kiwiParams.dctThreshold, kiwiParams.minOscilFreq, kiwiParams.maxOscilFreq,
                                             kiwiParams.eventThreshold, kiwiParams.minDuration, kiwiParams.maxDuration);
//#############################################################################################################################################

            var sonogram = results.Item1;
            var hits = results.Item2;
            var scores = results.Item3;
            var predictedEvents = results.Item4;
            Log.WriteLine("# Event count = " + predictedEvents.Count());

            //write event count to results file. 
            double sigDuration = sonogram.Duration.TotalSeconds;
            string fname = Path.GetFileName(recordingPath);
            int count = predictedEvents.Count;
            //string str = String.Format("#RecordingName\tDuration(sec)\t#Ev\tCompT(ms)\t%hiFrames\n{0}\t{1}\t{2}\t{3}\t{4}\n", fname, sigDuration, count, analysisDuration.TotalMilliseconds, pcHIF);
            string str = String.Format("{0}\t{1}\t{2}", fname, sigDuration, count);
            StringBuilder sb = AcousticEvent.WriteEvents(predictedEvents, str);
            FileTools.WriteTextFile(opPath, sb.ToString());


            //draw images of sonograms
            string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";
            if (kiwiParams.DRAW_SONOGRAMS == 2)
            {
                DrawSonogram(sonogram, imagePath, hits, scores, predictedEvents, kiwiParams.eventThreshold);
            }
            else
                if ((kiwiParams.DRAW_SONOGRAMS == 1) && (predictedEvents.Count > 0))
            {
                DrawSonogram(sonogram, imagePath, hits, scores, predictedEvents, kiwiParams.eventThreshold);
            }

            Log.WriteLine("# Finished recording:- " + Path.GetFileName(recordingPath));
            Console.ReadLine();
        } //Dev()


        public static KiwiParams ReadIniFile(string iniPath)
        {
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            Dictionary<string, string>.KeyCollection keys = dict.Keys;

            KiwiParams kiwiParams; // st
            //bool doSegmentation = Boolean.Parse(dict[key_DO_SEGMENTATION]);
            kiwiParams.minHzMale = Int32.Parse(dict[key_MIN_HZ_MALE]);
            kiwiParams.maxHzMale = Int32.Parse(dict[key_MAX_HZ_MALE]);
            kiwiParams.minHzFemale = Int32.Parse(dict[key_MIN_HZ_FEMALE]);
            kiwiParams.maxHzFemale = Int32.Parse(dict[key_MAX_HZ_FEMALE]);
            kiwiParams.frameOverlap = Double.Parse(dict[key_FRAME_OVERLAP]);
            kiwiParams.dctDuration = Double.Parse(dict[key_DCT_DURATION]);       //duration of DCT in seconds 
            kiwiParams.dctThreshold = Double.Parse(dict[key_DCT_THRESHOLD]);      //minimum acceptable value of a DCT coefficient
            kiwiParams.minOscilFreq = Double.Parse(dict[key_MIN_OSCIL_FREQ]);     //ignore oscillations below this threshold freq
            kiwiParams.maxOscilFreq = Double.Parse(dict[key_MAX_OSCIL_FREQ]);     //ignore oscillations above this threshold freq
            kiwiParams.minDuration = Double.Parse(dict[key_MIN_DURATION]);       //min duration of event in seconds 
            kiwiParams.maxDuration = Double.Parse(dict[key_MAX_DURATION]);       //max duration of event in seconds 
            kiwiParams.eventThreshold = Double.Parse(dict[key_EVENT_THRESHOLD]);  //min score for an acceptable event
            kiwiParams.DRAW_SONOGRAMS = Int32.Parse(dict[key_DRAW_SONOGRAMS]);      //options to draw sonogram

            Log.WriteIfVerbose("Male   Freq band: {0} Hz - {1} Hz.)", kiwiParams.minHzMale, kiwiParams.maxHzMale);
            Log.WriteIfVerbose("Female Freq band: {0} Hz - {1} Hz.)", kiwiParams.minHzFemale, kiwiParams.maxHzFemale);
            Log.WriteIfVerbose("Oscillation bounds: {0:f1}-{1:f1} Hz", kiwiParams.minOscilFreq, kiwiParams.maxOscilFreq);
            Log.WriteIfVerbose("minAmplitude = " + kiwiParams.dctThreshold);
            Log.WriteIfVerbose("Duration bounds: " + kiwiParams.minDuration + " - " + kiwiParams.maxDuration + " seconds");   
            return kiwiParams;
        }



        public static AudioRecording GetAudioRecording(string recordingPath)
        {
            //OLD CODE
            //AudioRecording recording = new AudioRecording(recordingPath);
            //if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();

            SpecificWavAudioUtility audioUtility = SpecificWavAudioUtility.Create();
            audioUtility.SoxAudioUtility.ResampleQuality = SoxAudioUtility.SoxResampleQuality.VeryHigh; //Options: Low, Medium, High, VeryHigh 
            audioUtility.SoxAudioUtility.TargetSampleRateHz = 17640;
            audioUtility.SoxAudioUtility.ReduceToMono = true;
            audioUtility.SoxAudioUtility.UseSteepFilter = true;
            //##### ######  IMPORTANT NOTE 1 :: THE EFFECT OF THE ABOVE RESAMPLING PARAMETERS IS TO SET NYQUIST = SAMPLERATE / 2 Hz.
            //##### ######  IMPORTANT NOTE 2 :: THE RESULTING SIGNAL ARRAY VARIES SLIGHTLY FOR EVERY LOADING - NOT SURE WHY? A STOCHASTOIC COMPONENT TO FILTER? 
            //##### ######                               BUT IT HAS THE EFFECT THAT STATISTICS VARY SLIGHTLY FOR EACH RUN OVER THE SAME FILE.
            audioUtility.LogLevel = LogType.Error;  //Options: None, Fatal, Error, Debug, 
            AudioRecording recording = new AudioRecording(recordingPath, audioUtility);

            return recording;
        }

        public static AudioRecording GetSegmentFromAudioRecording(string recordingPath, double startMinutes, double durationMinutes, int overlap)
        {
            SpecificWavAudioUtility audioUtility = SpecificWavAudioUtility.Create();
            audioUtility.SoxAudioUtility.ResampleQuality = SoxAudioUtility.SoxResampleQuality.VeryHigh; //Options: Low, Medium, High, VeryHigh 
            audioUtility.SoxAudioUtility.TargetSampleRateHz = 17640;
            audioUtility.SoxAudioUtility.ReduceToMono = true;
            audioUtility.SoxAudioUtility.UseSteepFilter = true;
            //##### ######  IMPORTANT NOTE 1 :: THE EFFECT OF THE ABOVE RESAMPLING PARAMETERS IS TO SET NYQUIST = SAMPLERATE / 2 Hz.
            //##### ######  IMPORTANT NOTE 2 :: THE RESULTING SIGNAL ARRAY VARIES SLIGHTLY FOR EVERY LOADING - NOT SURE WHY? A STOCHASTOIC COMPONENT TO FILTER? 
            //##### ######                               BUT IT HAS THE EFFECT THAT STATISTICS VARY SLIGHTLY FOR EACH RUN OVER THE SAME FILE.
            audioUtility.LogLevel = LogType.Error;  //Options: None, Fatal, Error, Debug, 

            FileInfo inFile = new FileInfo(recordingPath);
            FileInfo outFile = new FileInfo(@"C:\SensorNetworks\WavFiles\Kiwi\Samples\test.wav");
            int startMilliseconds = (int)(startMinutes * 60000);
            int endMilliseconds   = startMilliseconds + (int)(durationMinutes * 60000) + overlap;

            SpecificWavAudioUtility.GetSingleSegment(audioUtility, inFile, outFile, startMilliseconds, endMilliseconds);
            AudioRecording recording = new AudioRecording(outFile.FullName, audioUtility);

            //AudioRecording recording = new AudioRecording(recordingPath, audioUtility);

            return recording;
        }

        public static System.Tuple<BaseSonogram, Double[,], double[], List<AcousticEvent>> Execute_KiwiDetect(AudioRecording recording, 
            /*bool doSegmentation,*/ int minHzMale, int maxHzMale, int minHzFemale, int maxHzFemale, double frameOverlap, double dctDuration, double dctThreshold, 
            double minOscilFreq, double maxOscilFreq, double eventThreshold, double minDuration, double maxDuration)
        {
            //i: GET RECORDING


            //ii: MAKE SONOGRAM
            Log.WriteLine("Make sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.WindowOverlap = frameOverlap;
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.WindowSize = 2048;
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            recording.Dispose();
            Log.WriteLine("Signal: Duration={0}, Sample Rate={1}", sonogram.Duration, recording.SampleRate);
            Log.WriteLine("Frames: Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f0}%, Offset={3:f1}ms, Frames/s={4:f1}",
                                       sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                      (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, frameOverlap);
            int binCount_male   = (int)(maxHzMale / sonogram.FBinWidth) - (int)(minHzMale / sonogram.FBinWidth) + 1;
            int binCount_female = (int)(maxHzFemale / sonogram.FBinWidth) - (int)(minHzFemale / sonogram.FBinWidth) + 1;
            Log.WriteIfVerbose("Male   Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHzMale, maxHzMale, binCount_male);
            Log.WriteIfVerbose("Female Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHzFemale, maxHzFemale, binCount_female);

            Log.WriteIfVerbose("DctDuration=" + dctDuration + "sec.  (# frames=" + (int)Math.Round(dctDuration * sonogram.FramesPerSecond) + ")");
            Log.WriteIfVerbose("Score threshold for oscil events=" + eventThreshold);
            Log.WriteLine("Start oscillation detection");

            //iii: DETECT OSCILLATIONS
            bool normaliseDCT = true;

            //CHECK FOR MALE KIWIS
            List<AcousticEvent> predictedMaleEvents;  //predefinition of results event list
            Double[,] maleHits;                       //predefinition of hits matrix - to superimpose on sonogram image
            double[] maleScores;                      //predefinition of score array
            OscillationAnalysis.Execute((SpectralSonogram)sonogram, minHzMale, maxHzMale, dctDuration, dctThreshold, normaliseDCT,
                                         minOscilFreq, maxOscilFreq, eventThreshold, minDuration, maxDuration,
                                         out maleScores, out predictedMaleEvents, out maleHits);
            foreach (AcousticEvent ae in predictedMaleEvents) ae.Name = "Male LSK";
            //CHECK FOR FEMALE KIWIS
            Double[,] femaleHits;                       //predefinition of hits matrix - to superimpose on sonogram image
            double[] femaleScores;                      //predefinition of score array
            List<AcousticEvent> predictedFemaleEvents;  //predefinition of results event list
            OscillationAnalysis.Execute((SpectralSonogram)sonogram, minHzFemale, maxHzFemale, dctDuration, dctThreshold, normaliseDCT,
                                         minOscilFreq, maxOscilFreq, eventThreshold, minDuration, maxDuration,
                                         out femaleScores, out predictedFemaleEvents, out femaleHits);
            foreach (AcousticEvent ae in predictedFemaleEvents) ae.Name = "Female LSK";

            //merge the two lists of events
            foreach (AcousticEvent ae in predictedFemaleEvents) predictedMaleEvents.Add(ae);
            //merge the two hit matrices
            Double[,] hits = DataTools.AddMatrices(maleHits, femaleHits);
            //merge the two score arrays
            for (int i = 0; i < maleScores.Length; i++) if (femaleScores[i] > maleScores[i]) maleScores[i] = femaleScores[i];

            return System.Tuple.Create(sonogram, hits, maleScores, predictedMaleEvents);

        }//end Execute_KiwiDetect()


        public static void DrawSonogram(BaseSonogram sonogram, string path, double[,] hits, double[] scores,
                                        List<AcousticEvent> predictedEvents, double eventThreshold)
        {
            Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            //double maxScore = 50.0; //assumed max posisble oscillations per second

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, eventThreshold));
                double maxScore = 16.0;
                image.AddSuperimposedMatrix(hits, maxScore);
                image.AddEvents(predictedEvents);
                image.Save(path);
            }
        }


        public static void CheckArguments(string[] args)
        {
            if (args.Length != 3)
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
            Console.WriteLine("KiwiDetect.exe recordingPath iniPath outputFileName");
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
