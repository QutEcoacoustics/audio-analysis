using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;

namespace AnalysisPrograms
{

    //Here is link to wiki page containing info about how to write Analysis techniques
    //https://wiki.qut.edu.au/display/mquter/Audio+Analysis+Processing+Architecture

    //HERE ARE COMMAND LINE ARGUMENTS TO PLACE IN START OPTIONS - PROPERTIES PAGE,  debug command line
    //for LEWIN's RAIL
    // ID, recording, template.zip, working directory.
    //kekkek C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-075040.wav C:\SensorNetworks\Templates\Template_2\KEKKEK1.zip  C:\SensorNetworks\Output\LewinsRail\
    
    class MFCC_OD_KekKek
    {

        //Keys to recognise identifiers in PARAMETERS - INI file. 
        public static string key_MIN_HZ = "MIN_FREQ";
        public static string key_MAX_HZ = "MAX_FREQ";
        public static string key_FRAME_OVERLAP = "FRAME_OVERLAP";
        public static string key_DO_MELSCALE = "DO_MELSCALE";
        public static string key_CC_COUNT = "CC_COUNT";
        public static string key_INCLUDE_DELTA = "INCLUDE_DELTA";
        public static string key_INCLUDE_DOUBLE_DELTA = "INCLUDE_DOUBLE_DELTA";
        public static string key_DCT_DURATION = "DCT_DURATION";
        public static string key_MIN_OSCIL_FREQ = "MIN_OSCIL_FREQ";
        public static string key_MAX_OSCIL_FREQ = "MAX_OSCIL_FREQ";
        public static string key_MIN_AMPLITUDE = "MIN_AMPLITUDE";
        public static string key_MIN_DURATION = "MIN_DURATION";
        public static string key_MAX_DURATION = "MAX_DURATION";
        public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_DRAW_SONOGRAMS = "DRAW_SONOGRAMS";

        public static string eventsFile = "events.txt"; 



        public static void Dev(string[] args)
        {
            Log.WriteLine("\n");
            string title = "# DETECTING LEWIN's RAIL Kek-Kek USING MFCCs and OD";
            string date = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            Log.Verbosity = 1;
            CheckArguments(args);


            string recordingPath    = args[0];
            string templatePath     = args[1];
            string workingDirectory = args[2];
            string recordingFN      = Path.GetFileName(recordingPath);
            string outputDir        = workingDirectory;
            string opFName          = "events.txt"; ;
            string opPath           = outputDir + opFName;
            string templateFN       = Path.GetFileNameWithoutExtension(templatePath);
            
            Log.WriteLine("# Recording:     " + recordingFN);
            Log.WriteLine("# Working Dir:   " + workingDirectory);
            Log.WriteLine("# Output folder: " + outputDir);
            FileTools.WriteTextFile(opPath, date + "\n# Scanning recording for Lewin's Rail Kek Kek\n# Recording file: " + recordingFN);


            //A: SHIFT TEMPLATE TO WORKING DIRECTORY AND UNZIP IF NOT ALREADY DONE
            string templateName = Path.GetFileNameWithoutExtension(templatePath);
            //create the working directory if it does not exist
            if (!Directory.Exists(workingDirectory)) Directory.CreateDirectory(workingDirectory);
            string newTemplateDir = workingDirectory + templateName;
            ZipUnzip.UnZip(newTemplateDir, templatePath, true);

            //B: READ INI/CONFIG and CREATE DIRECTORY STRUCTURE
            Log.WriteLine("# Init CONFIG and creating directory structure");
            Log.WriteLine("# New Template Dir: " + newTemplateDir);
            string iniPath = workingDirectory + templateFN + "\\Template_" + templateFN + ".txt";
            //read feature vector
            string fvPath  = workingDirectory + templateFN + "\\FV1_" + templateFN + ".txt"; //feature vector path
            double[] fv = FileTools.ReadDoubles2Vector(fvPath);

            //C: SET UP CONFIGURATION
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            //Dictionary<string, string>.KeyCollection keys = dict.Keys;

            int windowSize = Int32.Parse(dict["FRAME_SIZE"]);
            double frameOverlap = Double.Parse(dict[key_FRAME_OVERLAP]);
            NoiseReductionType nrt = SNR.Key2NoiseReductionType(dict["NOISE_REDUCTION_TYPE"]);
            double dynamicRange = Double.Parse(dict["DYNAMIC_RANGE"]);
            int minHz           = Int32.Parse(dict[key_MIN_HZ]);
            int maxHz           = Int32.Parse(dict[key_MAX_HZ]);
            int ccCount         = Int32.Parse(dict[key_CC_COUNT]);           //Number of mfcc coefficients
            bool doMelScale     =  Boolean.Parse(dict[key_DO_MELSCALE]);       
            bool includeDelta       = Boolean.Parse(dict[key_INCLUDE_DELTA]);
            bool includeDoubleDelta = Boolean.Parse(dict[key_INCLUDE_DOUBLE_DELTA]);
            double dctDuration  = Double.Parse(dict[key_DCT_DURATION]);      //duration of DCT in seconds 
            int minOscilFreq    = Int32.Parse(dict[key_MIN_OSCIL_FREQ]);     //ignore oscillations below this threshold freq
            int maxOscilFreq    = Int32.Parse(dict[key_MAX_OSCIL_FREQ]);     //ignore oscillations above this threshold freq
            double minAmplitude = Double.Parse(dict[key_MIN_AMPLITUDE]);     //minimum acceptable value of a DCT coefficient
            double eventThreshold = Double.Parse(dict[key_EVENT_THRESHOLD]);
            double minDuration  = Double.Parse(dict[key_MIN_DURATION]);      //min duration of event in seconds 
            double maxDuration  = Double.Parse(dict[key_MAX_DURATION]);      //max duration of event in seconds 
            int DRAW_SONOGRAMS  = Int32.Parse(dict[key_DRAW_SONOGRAMS]);     //options to draw sonogram


            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz.", minHz, maxHz);
            Log.WriteIfVerbose("Oscill bounds: " + minOscilFreq + " - " + maxOscilFreq + " Hz");
            Log.WriteIfVerbose("minAmplitude = " + minAmplitude);
            Log.WriteIfVerbose("Duration bounds: " + minDuration + " - " + maxDuration + " seconds");

            //#############################################################################################################################################
            var results = Execute_CallDetect(recordingPath, minHz, maxHz, windowSize, frameOverlap, nrt, dynamicRange, 
                                  doMelScale, ccCount, includeDelta, includeDoubleDelta,
                                  fv, dctDuration, minOscilFreq, maxOscilFreq, minAmplitude, eventThreshold, minDuration, maxDuration);
            Log.WriteLine("# Finished detecting Lewin's Rail calls.");
            //#############################################################################################################################################

            var sonogram = results.Item1;
            var scores = results.Item2;
            var predictedEvents = results.Item3;
            var avMatrix = results.Item4;
            Log.WriteLine("# Event Count = " + predictedEvents.Count());

            //write event count to results file.            
            WriteEventsInfo2TextFile(predictedEvents, opPath);

            if (DRAW_SONOGRAMS == 2)
            {
                string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";
                DrawSonogram(sonogram, imagePath, scores, predictedEvents, eventThreshold);
                ImageTools.DrawMatrix(avMatrix, outputDir+"\\acousticVectors.jpg");
            }
            else
            if ((DRAW_SONOGRAMS == 1) && (predictedEvents.Count > 0))
            {
                string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";
                DrawSonogram(sonogram, imagePath, scores, predictedEvents, eventThreshold);
            }

            Log.WriteLine("# Finished analysis of recording:- " + Path.GetFileName(recordingPath));
            Console.ReadLine();
        } //Dev()


        public static System.Tuple<BaseSonogram, double[], List<AcousticEvent>, double[,]> Execute_CallDetect(string wavPath,
            int minHz, int maxHz, int windowSize, double frameOverlap, NoiseReductionType nrt, double dynamicRange, 
            bool doMelScale, int ccCount, bool includeDelta, bool includeDoubleDelta,

            double[] fv, double dctDuration, int minOscilFreq, int maxOscilFreq, 
            double minAmplitude, double eventThreshold, double minDuration, double maxDuration)
        {
            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(wavPath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            int sr = recording.SampleRate;

            //ii: MAKE SONOGRAM
            Log.WriteLine("Start sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config - especially full band width
            //now set required values in config
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.WindowSize = windowSize;
            sonoConfig.WindowOverlap = frameOverlap;
            sonoConfig.DoMelScale = doMelScale;
            sonoConfig.NoiseReductionType = nrt;
            sonoConfig.DynamicRange = dynamicRange;

            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            recording.Dispose();
            Log.WriteLine("Signal: Duration={0}, Sample Rate={1}", sonogram.Duration, sr);
            Log.WriteLine("Frames: Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f0}%, Offset={3:f1}ms, Frames/s={4:f1}",
                                       sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                      (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, frameOverlap*100);
            int binCount = (int)(maxHz / sonogram.FBinWidth) - (int)(minHz / sonogram.FBinWidth) + 1;

            //iii: EXTRACT CEPSTROGRAM - MFCC coefficients 
            Log.WriteIfVerbose("Freqs : {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount);
            Log.WriteIfVerbose("MFCCs : doMelScale=" + doMelScale + ";  ccCount=" + ccCount + ";  includeDelta=" + includeDelta + ";  includeDoubleDelta=" + includeDoubleDelta);
            var tuple =((SpectralSonogram)sonogram).GetCepstrogram(minHz, maxHz, doMelScale, ccCount);
            double[,] m = tuple.Item1;

            //iv:  REPLACE THE dB ARRAY for full bandwidth by array initialized to 0.5 (an average value)
            //THIS IS IN PLACE OF REMOVING THE dB array altogether OR CALCULATING SUB-BAND dB array.
            //BOTH THESE OPTIONS ARE TOO TIME CONSUMING IN PRESENT CIRCUMSTANCE.
            //double[] dB = sonogram.DecibelsNormalised;
            double[] dB = new double[sonogram.FrameCount];
            for (int i=0; i< sonogram.FrameCount; i++) dB[i] = 0.5;

            //v: calculate the full ACOUSTIC VECTORS ie including decibel and deltas, etc
            m = Speech.AcousticVectors(m, dB, includeDelta, includeDoubleDelta);

            //vi: GET FEATURE VECTOR SCORES
            double[] scores = GetTemplateScores(m, fv, includeDelta, includeDoubleDelta);
            double Q;
            scores = SNR.NoiseSubtractMode(scores, out Q);
            Log.WriteLine("Noise removal, Q={0:f3}",Q);
            //normalise scores rather than calculate Z-scores.
            //scores = NormalDist.CalculateZscores(scores, this.NoiseAv, this.NoiseSd);
            scores = DataTools.normalise(scores);

            //vii: DETECT OSCILLATIONS
            Log.WriteIfVerbose("DctDuration=" + dctDuration + "sec.  (# frames=" + (int)Math.Round(dctDuration * sonogram.FramesPerSecond) + ")");
            Log.WriteIfVerbose("EventThreshold=" + eventThreshold);
            int dctLength = (int)Math.Round(sonogram.FramesPerSecond * dctDuration);
            double[] oscillationScores = DetectOscillations(scores, dctDuration, dctLength, minOscilFreq, maxOscilFreq, minAmplitude);

            //viii:  EXTRACT ACOUSTIC EVENTS
            //double[] oscFreq = OscillationAnalysis.GetODFrequency(m, minHz, maxHz, sonogram.FBinWidth);
            List<AcousticEvent> predictedEvents = ConvertScores2Events(oscillationScores, minHz, maxHz, sonogram.FramesPerSecond,
                                       sonogram.FBinWidth, eventThreshold, minDuration, maxDuration, sonogram.Configuration.SourceFName);

            return System.Tuple.Create(sonogram, oscillationScores, predictedEvents, m);

        }//end Execute_CallDetect



        static double[] GetTemplateScores(Double[,] m, Double[] fv, bool includeDelta, bool includeDoubleDelta)
        {
            int frameCount   = m.GetLength(0);
            int featureCount = m.GetLength(1);
            double[] scores = new double[frameCount];
            //extract relevant part of the feature vector.
            int length = 13;
            int skip = length * 3;
            if (includeDelta)       length += 13;
            if (includeDoubleDelta) length += 13; 
            double[] trimFV = new double[length];
            for (int i = 0; i < length; i++) trimFV[i] = fv[skip+i];
            double[] normFV = DataTools.DiffFromMean(trimFV); //required for cross correlation

            for (int r = 0; r < frameCount; r++)
            {
                double[] v = DataTools.GetRow(m, r);
                scores[r] = DataTools.DotProduct(normFV, DataTools.DiffFromMean(v));  // Cross-correlation coeff
            }
            scores = DataTools.normalise(scores);
            return scores;
        }


        public static double[] DetectOscillations(double[] scores, double dctDuration, int dctLength, int minOscilFreq, int maxOscilFreq, double minAmplitude)
        {

              double[,] cosines = Speech.Cosines(dctLength, dctLength); //set up the cosine coefficients
              int L = scores.Length;
              double[] oscillationScores = new double[L];
              //following two lines write matrix of cos values for checking.
              //string fPath = @"C:\SensorNetworks\Sonograms\cosines.txt";
              //FileTools.WriteMatrix2File_Formatted(cosines, fPath, "F3");

              //following two lines write bmp image of cos values for checking.
              //string fPath = @"C:\SensorNetworks\Output\cosines.bmp";
              //ImageTools.DrawMatrix(cosines, fPath);

              for (int r = 0; r < L - dctLength; r++)
              {
                  var array = new double[dctLength];
                  for (int i = 0; i < dctLength; i++) array[i] = scores[r+i];

                  double[] dct = Speech.DCT(array, cosines);
                  for (int i = 0; i < dctLength; i++) dct[i] = Math.Abs(dct[i]);//convert to absolute values
                  for (int i = 0; i < 5; i++)         dct[i] = 0.0; //remove low freq oscillations from consideration
                  dct = DataTools.normalise2UnitLength(dct);
                  
                  int indexOfMaxValue = DataTools.GetMaxIndex(dct);
                  double oscilFreq = indexOfMaxValue / dctDuration * 0.5; //Times 0.5 because index = Pi and not 2Pi

                  //DataTools.MinMax(dct, out min, out max);
                  //      DataTools.writeBarGraph(dct);

                  //mark DCT location with oscillation freq, only if oscillation freq is in correct range and amplitude
                  if ((oscilFreq >= minOscilFreq) && (oscilFreq <= maxOscilFreq) && (dct[indexOfMaxValue] > minAmplitude))
                  {
                      for (int i = 0; i < dctLength; i++) oscillationScores[r + i] = oscilFreq;
                  }
                  r += 1; //skip positions
              }

              return oscillationScores;
          }



        /// <summary>
        /// Converts the Oscillation Detector score array to a list of AcousticEvents. 
        /// </summary>
        /// <param name="scores">the array of OD scores</param>
        /// <param name="minHz">lower freq bound of the acoustic event</param>
        /// <param name="maxHz">upper freq bound of the acoustic event</param>
        /// <param name="framesPerSec">the time scale required by AcousticEvent class</param>
        /// <param name="freqBinWidth">the freq scale required by AcousticEvent class</param>
        /// <param name="maxThreshold">OD score must exceed this threshold to count as an event</param>
        /// <param name="minDuration">duration of event must exceed this to count as an event</param>
        /// <param name="maxDuration">duration of event must be less than this to count as an event</param>
        /// <param name="fileName">name of source file to be added to AcousticEvent class</param>
        /// <returns></returns>
        public static List<AcousticEvent> ConvertScores2Events(double[] scores, int minHz, int maxHz,
                                                               double framesPerSec, double freqBinWidth,
                                                               double threshold, double minDuration, double maxDuration, string fileName)
        {
            int count = scores.Length;
            var events = new List<AcousticEvent>();
            bool isHit = false;
            double frameOffset = 1 / framesPerSec;
            double startTime = 0.0;
            int startFrame = 0;

            for (int i = 0; i < count; i++)//pass over all frames
            {
                if ((isHit == false) && (scores[i] >= threshold))//start of an event
                {
                    isHit = true;
                    startTime = i * frameOffset;
                    startFrame = i;
                }
                else  //check for the end of an event
                    if ((isHit == true) && (scores[i] < threshold))//this is end of an event, so initialise it
                    {
                        isHit = false;
                        double endTime = i * frameOffset;
                        double duration = endTime - startTime;
                        if ((duration < minDuration) || (duration > maxDuration)) continue; //skip events with duration shorter than threshold
                        AcousticEvent ev = new AcousticEvent(startTime, duration, minHz, maxHz);
                        ev.Name = "OscillationEvent"; //default name
                        //ev.SetTimeAndFreqScales(22050, 512, 128);
                        ev.SetTimeAndFreqScales(framesPerSec, freqBinWidth);
                        ev.SourceFile = fileName;
                        //obtain average score.
                        double av = 0.0;
                        for (int n = startFrame; n <= i; n++) av += scores[n];
                        ev.Score = av / (double)(i - startFrame + 1);
                        //calculate average oscillation freq and assign to ev.Score2 
                        ev.Score2Name = "OscRate"; //score2 name
                        av = 0.0;
                        //for (int n = startFrame; n <= i; n++) av += oscFreq[n];
                        ev.Score2 = av / (double)(i - startFrame + 1);
                        events.Add(ev);
                    }
            } //end of pass over all frames
            return events;
        }//end method ConvertScores2Events()



        static void DrawSonogram(BaseSonogram sonogram, string path, double[] scores, List<AcousticEvent> predictedEvents, double eventThreshold)
        {
            Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            //double maxScore = 50.0; //assumed max posisble oscillations per second

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
                image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, eventThreshold));
                image.AddEvents(predictedEvents);
                image.Save(path);
            }
        }


        static void WriteEventsInfo2TextFile(List<AcousticEvent> predictedEvents, string path)
        {
            StringBuilder sb = new StringBuilder("# EVENT COUNT = " + predictedEvents.Count() + "\n");
            AcousticEvent.WriteEvents(predictedEvents, ref sb);
            sb.Append("#############################################################################");
            FileTools.Append2TextFile(path, sb.ToString());
        }


        /// <summary>
        /// JUST A TEMPORARY METHOD WHILE SETTING UP THIS CLASS
        /// These params will be used to detect a kek-kek in the output file from MFCCs 
        /// </summary>
        private static void AppendNewParams(string iniPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# duration of DCT in seconds");
            sb.AppendLine("DCT_DURATION=2.0");
            sb.AppendLine("# ignore oscillation rates below the min & above the max threshold OSCILLATIONS PER SECOND");
            sb.AppendLine("MIN_OSCIL_FREQ=4");
            sb.AppendLine("MAX_OSCIL_FREQ=6");
            sb.AppendLine("# minimum acceptable value of a DCT coefficient");
            sb.AppendLine("MIN_AMPLITUDE=0.6");
            sb.AppendLine("# Minimum duration for the length of a true event.");
            sb.AppendLine("MIN_DURATION=2.0");
            sb.AppendLine("# Maximum duration for the length of a true event.");
            sb.AppendLine("MAX_DURATION=10.0");
            sb.AppendLine("# Event threshold - use this to determin FP / FN trade-off for events.");
            sb.AppendLine("EVENT_THRESHOLD=0.20");
            sb.AppendLine("# save a sonogram for each recording that contained a hit");
            sb.AppendLine("DRAW_SONOGRAMS=2");
            FileTools.Append2TextFile(iniPath, sb.ToString());
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
            Console.WriteLine("KekKek_MFCC_OD.exe recordingPath iniPath outputFileName");
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

    }
}
