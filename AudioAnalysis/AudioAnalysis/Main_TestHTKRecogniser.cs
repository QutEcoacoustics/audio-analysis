using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.Text.RegularExpressions;
using TowseyLib;


namespace AudioAnalysis
{
    public class TestHTKRecogniser
    {

        //FORCE THRESHOLDS FOR Experimentation --- set SCORE_THRESHOLD = Double.NaN  if do not want to override default values
        const double SCORE_THRESHOLD   = -50.0;  //Double.NaN
        const double QUALITY_THRESHOLD = 2.56;   //1.96 for p=0.05;  2.56 for p=0.01

        private static bool verbose = true;
        public static bool Verbose { get { return verbose; } set { verbose = value; } }
        
        
        
        /// <summary>
        /// Runs a prepared HTK template over a file
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

            TestHTKRecogniser.Verbose = false;
            //*******************************************************************************************************************
            #region Variables
            string workingDirectory = "C:\\SensorNetworks\\temp"; //set default working directory                    // ARG 0  
            //*******************************************************************************************************************
            //THE TEMPLATE
            string dir  = "C:\\SensorNetworks\\Templates\\Template_";
            //string templateName = "CURRAWONG1";
            //string templateName = "CURLEW1";
            //string templateName = "WHIPBIRD1";
            //string templateName = "CURRAWONG1";
            //string templateName = "KOALAFEMALE1";
            //string templateName = "KOALAFEMALE2";
            string templateName = "KOALAMALE1";


            //*******************************************************************************************************************
            string templateDir = dir + templateName;
            string templateFN  = templateDir + "\\" + templateName + ".zip";                           // ARG 1
            //*******************************************************************************************************************
            //THE WAV FILE TO PROCESS
            //string wavFile = "C:\\SensorNetworks\\WavFiles\\TestWaveFile\\St_Bees_Currawong_20080919-060000_13.wav";          //ARG 2
            //string wavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\West_Knoll_St_Bees_Currawong3_20080919-060000.wav";       //ARG 2
            //string wavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\Top_Knoll_St_Bees_Curlew2_20080922-030000.wav";           //ARG 2
            string wavFile = "C:\\SensorNetworks\\WavFiles\\Koala_Male\\SmallTestSet\\HoneymoonBay_StBees_20080905-001000.wav";   //ARG 2
            //string wavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\WestKnoll_StBees_KoalaBellow20080919-073000.wav";//contains currawong
            //string wavFile = @"C:\SensorNetworks\WavFiles\BridgeCreek\\cabin_GoldenWhistler_file0127_extract1.wav";
            //string wavFile = @"C:\SensorNetworks\WavFiles\Koala_Female\HoneymoonBay_StBees_20081027-023000.wav";
            //string wavFile = @"C:\SensorNetworks\WavFiles\Koala_Female\WestKnoll_StBees_20081216-213000.wav";
            //string wavFile = @"C:\SensorNetworks\WavFiles\Koala_Female\WestKnoll_StBees3_20090907-053000.wav";//mix of curlew,currawong, koalas
            //*******************************************************************************************************************

            //GET THE COMMAND LINE ARGUMENTS
            if (args.Length >= 1) workingDirectory = args[0]; //where to place output
            if (args.Length >= 2) templateFN = args[1]; //template file name
            if (args.Length == 3) wavFile = args[2]; //wav file to process
            #endregion


            //#### A: GET LIST OF LABELLED EVENTS.
            string labelsPath = @"C:\SensorNetworks\WavFiles\Koala_Male\SmallTestSet\KoalaTestData.txt";
            string labelsText;
            string filename = Path.GetFileNameWithoutExtension(wavFile);
            List<AcousticEvent> labels = AcousticEvent.GetAcousticEventsFromLabelsFile(labelsPath, filename, out labelsText);
            //List<AcousticEvent> labels = AcousticEvent.GetAcousticEventsFromLabelsFile(labelsPath, null, out labelsText);

            //#### B: GET LIST OF RECOGNISED EVENTS.
            List<AcousticEvent> events;
            TestHTKRecogniser.Execute(templateFN, workingDirectory, wavFile, out events);
            List<string> list = ExtractEventData(events);
            string opFile = workingDirectory + "\\results\\eventData.txt";
            FileTools.WriteTextFile(opFile, list, true);
            List<AcousticEvent> results = GetAcousticEventsFromResultsFile(opFile);

            int tp, fp, fn;
            string resultsText;
            double precision, recall, accuracy;
            AcousticEvent.CalculateAccuracy(results, labels, out tp, out fp, out fn, out precision, out recall, out accuracy,
                                              out resultsText);

            Console.WriteLine("\n\ntp={0}\tfp={1}\tfn={2}", tp, fp, fn);
            Console.WriteLine("Recall={0:f2}  Precision={1:f2}  Accuracy={2:f2}", recall, precision, accuracy);

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }// end Main()






        public static void Execute(string templatePath, string workingDirectory, string wavFile, out List<AcousticEvent> events)
        {
            string templateName = Path.GetFileNameWithoutExtension(templatePath);

            //A: SHIFT TEMPLATE TO WORKING DIRECTORY AND UNZIP IF NOT ALREADY DONE
                //string target = htkConfig.WorkingDir + "\\" + htkConfig.CallName;
                //Console.WriteLine("GOT TO HERE1");
                //ZipUnzip.UnZip(target, templateFN, true);
                //Console.WriteLine("GOT TO HERE2");
            string target = workingDirectory + "\\" + Path.GetFileNameWithoutExtension(templatePath);
            if (! Directory.Exists(target)) ZipUnzip.UnZip(target, templatePath, true);

            //B: SCAN RECORDING WITH RECOGNISER AND RETURN A RESULTS FILE
            string resultsPath;
            ScanWithHTKREcogniser(workingDirectory, templatePath, wavFile, out resultsPath);

            //C: PARSE THE RESULTS FILE TO RETURN ACOUSTIC EVENTS
            if (TestHTKRecogniser.Verbose == true)
            {
                Console.WriteLine("Parse the HMM results file and return Acoustic Events");
            }
            string templateDir = workingDirectory + "\\" + templateName; //template has been shifted
            events = GetAcousticEventsFromHTKResults(resultsPath, templateDir);

            //D: DISPLAY IN SONOGRAM
            if (TestHTKRecogniser.Verbose == true)
            {
                Console.WriteLine(" Extracted " + events.Count + " events.   Preparing sonogram to display events");
            }
            //DisplayAcousticEvents(wavFile, events, templateDir, workingDirectory);

        } //end method Execute()





        static void GetScoringParameters(string iniFile, out float HTKThreshold, out float QualityMean, out float QualitySD, out float QualityThreshold)
        {
            string key = "HTK_THRESHOLD";
            string str = FileTools.ReadPropertyFromFile(iniFile, key);
            HTKThreshold = float.Parse(str);
            Console.WriteLine("HTKThreshold= " + HTKThreshold);
            key = "DURATION_MEAN";
            str = FileTools.ReadPropertyFromFile(iniFile, key);
            QualityMean = float.Parse(str);
            Console.WriteLine("DurationMean= " + QualityMean);
            key = "DURATION_SD";
            str = FileTools.ReadPropertyFromFile(iniFile, key);
            QualitySD = float.Parse(str);
            Console.WriteLine("DurationSD= " + QualitySD);
            key = "SD_THRESHOLD";
            str = FileTools.ReadPropertyFromFile(iniFile, key);
            QualityThreshold = float.Parse(str);
            Console.WriteLine("SD_THRESHOLD= " + QualityThreshold);
        }

        static void GetSampleRate(string iniFile, out int sr, out int wSize)
        {
            string key = "SAMPLE_RATE";
            string str = FileTools.ReadPropertyFromFile(iniFile, key);
            sr = Int32.Parse(str);
            Console.WriteLine("Sample rate= " + sr);

            key = "FRAME_SIZE";
            str = FileTools.ReadPropertyFromFile(iniFile, key);
            wSize = Int32.Parse(str);
            Console.WriteLine("Window size= " + wSize);
        }


        static void GetFramingParameters(string configFile, out double WindowSize, out double WindowOffset, out int FreqMin, out int FreqMax)
        {
            string key = "WINDOWSIZE";    //actually window duration in 100 nano-sec units
            string str = FileTools.ReadPropertyFromFile(configFile, key);
            WindowSize = Double.Parse(str) / (double)10000000;   //convert 100ns units to seconds

            Console.WriteLine("WINDOW SIZE  = " + WindowSize);
            key = "TARGETRATE";           //actually window offset duration in 100 nano-sec units
            str = FileTools.ReadPropertyFromFile(configFile, key);
            WindowOffset = Double.Parse(str) / (double)10000000; //convert 100ns units to seconds
            Console.WriteLine("WINDOW-OFFSET OR TARGET-RATE= " + WindowOffset);
            key = "LOFREQ";
            str = FileTools.ReadPropertyFromFile(configFile, key);
            FreqMin = Int32.Parse(str);
            Console.WriteLine("FREQ MIN= " + FreqMin);
            key = "HIFREQ";
            str = FileTools.ReadPropertyFromFile(configFile, key);
            FreqMax = Int32.Parse(str);
            Console.WriteLine("FREQ MAX= " + FreqMax);
        }



        /// <summary>
        /// extracts an array of scores from a list of events
        /// </summary>
        /// <param name="events"></param>
        /// <param name="frameCount">the size of the array to return</param>
        /// <param name="windowOffset"></param>
        /// <param name="targetClass"></param>
        /// <param name="scoreThreshold"></param>
        /// <param name="qualityMean"></param>
        /// <param name="qualitySD"></param>
        /// <param name="qualityThreshold"></param>
        /// <returns></returns>
        public static double[] ExtractScoreArray(List<AcousticEvent> events, string iniFile, int arraySize, string targetName)
        {

            double windowOffset = events[0].FrameOffset;
            double frameRate = 1 / windowOffset; //frames per second

            string[] files = new string[1];
            files[0] = iniFile;
            Configuration config = new Configuration(files);

            double[] scores = new double[arraySize];
            //for (int i = 0; i < arraySize; i++) scores[i] = Double.NaN; //init to NaNs.
            int count = events.Count;

            //double avScore = 0.0;
            //double avDuration = 0.0;
            //double avFrames = 0.0;
            for (int i = 0; i < count; i++)
            {
                if (!events[i].Name.Equals(targetName)) continue; //skip irrelevant events

     //           double scoreThreshold = config.GetDouble(vocalName + "HTK_THRESHOLD");
     //           double qualityMean = config.GetDouble(vocalName + "DURATION_MEAN");
     //           double qualitySD = config.GetDouble(vocalName + "DURATION_SD");
     //           double qualityThreshold = config.GetDouble("Key_SD_THRESHOLD");
                int startFrame = (int)(events[i].StartTime * frameRate);
                int endFrame       = (int)((events[i].StartTime + events[i].Duration) * frameRate);
                double frameLength = events[i].Duration * frameRate;

                //avScore    += events[i].Score;
                //avDuration += events[i].Duration;
                //avFrames   += frameLength;

                for (int s = startFrame; s <= endFrame; s++) scores[s] = events[i].NormalisedScore;
            }
            return scores;
        } //end method


        /// <summary>
        /// Parses the HMM scores returned by HTK and returns a set of scores in 0-1 suitable for display as a score track.
        /// </summary>
        /// <param name="results">the HTK results</param>
        /// <param name="duration">the duration of a frame</param>
        /// <param name="frameCount">number of frames in the recording</param>
        /// <param name="targetClass">Name of the target class as shown in the HTK results file</param>
        /// <returns>array of hmm scores ONLY WHERE THERE ARE HITS. All other values are set as NaN</returns>
        public static double[] ParseHmmScores(List<string> results, int frameCount, double windowOffset, string targetClass,
                                              double scoreThreshold, double qualityMean, double qualitySD, double qualityThreshold)
        {

            double frameRate = 1 / windowOffset; //frames per second

            double[] scores = new double[frameCount];
            for (int i = 0; i < frameCount; i++) scores[i] = Double.NaN; //init to NaNs.
            int count = results.Count;

            double avScore = 0.0;
            double avDuration = 0.0;
            double avFrames = 0.0;
            int hitCount = 0;

            for (int i = 0; i < count; i++)
            {
                if ((results[i] == "")            || (results[i].StartsWith("."))) continue;
                if ((results[i].StartsWith("\"")) || (results[i].StartsWith("#"))) continue;
                //Helper.ParseResultLine(results[i], out start, out end, out className, out score);
                string[] param = Regex.Split(results[i], @"\s+");
                long start       = long.Parse(param[0]);
                long end         = long.Parse(param[1]);
                string vocalName = param[2];
                float score      = float.Parse(param[3]);

                if (!vocalName.StartsWith(targetClass)) continue; //skip irrelevant lines

                hitCount++; //count hits

                //calculate hmm and quality scores
                double duration = TimeSpan.FromTicks(end - start).TotalSeconds; //call duration in seconds
                double normScore, qualityScore, frameLength;
                bool isHit;
                Helper.ComputeHit(score, duration, frameRate, qualityMean, qualitySD, scoreThreshold, qualityThreshold,
                                  out frameLength, out normScore, out qualityScore, out isHit);

                double startSec = start / (double)10000000;  //start in seconds
                double endSec   = end   / (double)10000000;  //end   in seconds
                int startFrame  = (int)(startSec * frameRate);
                int endFrame    = (int)(endSec * frameRate);

                if (TestHTKRecogniser.Verbose == true)
                {
                    Console.WriteLine("sec=" + startSec.ToString("f1") + "-" + endSec.ToString("f1") +
                                      "\t " + (endSec - startSec).ToString("f2") + "s" +
                                      "\t frames=" + frameLength.ToString("f0") +
                                      "\t score=" + score.ToString("f1") +
                                      "\t normScore=" + normScore.ToString("f1") +
                                      "\t qualityScore=" + qualityScore.ToString("f1") + "\t HIT=" + isHit);
                }

                avScore += normScore;
                avDuration += (endSec - startSec);
                avFrames += frameLength;

                if (!isHit)
                {
                    normScore = scoreThreshold + (normScore / 5);//just to have a below threshold score to show in display
                    //continue;
                }
                for (int s = startFrame; s <= endFrame; s++) scores[s] = normScore;
            }//end for all hits

            if (TestHTKRecogniser.Verbose == true)
            {
                Console.WriteLine("avNormScore=" + (avScore / hitCount).ToString("f2"));
                Console.WriteLine("av Duration=" + (avDuration / hitCount).ToString("f3") + " or " + (avFrames / hitCount).ToString("f1") + " frames.");
            }
            return scores;
        }


        public static double[] NormaliseScores(double[] scores, float threshold, double thresholdFraction)
        {

            double maxscore = -10;//maximum
            double offset = (thresholdFraction * threshold) / (1 - thresholdFraction);
            double min = threshold + offset;
            double range = Math.Abs(maxscore - min);

            int frameCount = scores.Length;
            double[] normScores = new double[frameCount]; //the final normalised scores
            for (int i = 0; i < frameCount; i++)
            {
                //normalise score between 0 - 1. Assume max score=0.000
                if (Double.IsNaN(scores[i])) normScores[i] = 0.0;
                else normScores[i] = (scores[i] - min) / range;
                if (normScores[i] > 1.0) normScores[i] = 1.0;
            }
            return normScores;
        }


        public static List<string> ExtractEventData(List<AcousticEvent> events)
        {
            var list = new List<string>();
            foreach (AcousticEvent ae in events)
            {
                var sb = new StringBuilder();
                double endtime = ae.StartTime + ae.Duration;
                sb.Append(ae.Name + "\t" + ae.StartTime.ToString("f4") + "\t" +
                          endtime.ToString("f4") + "\t" + ae.Score.ToString("f4") + "\t" +
                          ae.SourceFile);
                list.Add(sb.ToString());
            }
            return list;
        }




        //##########################################################################################################
        //##########################################################################################################
        //##########################################################################################################
        //##########################################################################################################
        //##########################################################################################################
        //##########################################################################################################
        //##########################################################################################################
       
        public static void ScanWithHTKREcogniser(string workingDirectory, string templateFN, string WavFile, out string resultsPath)
        {
            if (TestHTKRecogniser.Verbose == true) Console.WriteLine("Executing HMMBuilder - scanning a .wav file");

            #region Variables
            HTKConfig htkConfig  = new HTKConfig();
            htkConfig.WorkingDir = workingDirectory;  
            htkConfig.CallName   = Path.GetFileNameWithoutExtension(templateFN);
            htkConfig.DataDir    = htkConfig.WorkingDir + "\\data";
            htkConfig.ConfigDir  = htkConfig.WorkingDir + "\\" + htkConfig.CallName;
            htkConfig.ResultsDir = htkConfig.WorkingDir + "\\results";
            htkConfig.HTKDir     = htkConfig.ConfigDir + "\\HTK";
            htkConfig.SegmentationDir = htkConfig.ConfigDir + "\\Segmentation";
            htkConfig.SilenceModelPath = htkConfig.SegmentationDir + "\\West_Knoll_St_Bees_Currawong1_20080923-120000.wav";

            Console.WriteLine("CFG=" + htkConfig.ConfigDir);
            Console.WriteLine("DAT=" + htkConfig.DataDir);
            Console.WriteLine("RSL=" + htkConfig.ResultsDir);

            #endregion

            //create the working directory if it does not exist
            if (!Directory.Exists(htkConfig.WorkingDir)) Directory.CreateDirectory(htkConfig.WorkingDir);
            //delete data directory if it exists
            if (Directory.Exists(htkConfig.DataDir)) Directory.Delete(htkConfig.DataDir, true);
            Directory.CreateDirectory(htkConfig.DataDir);

            //move the data/TEST file to its own directory
            Directory.CreateDirectory(htkConfig.DataDir);
            string dataFN = Path.GetFileName(WavFile);
            File.Copy(WavFile, htkConfig.DataDir + "\\" + dataFN, true);

            //PREPARE THE TEST FILE AND EXTRACT FEATURES
            //write script files
            HTKHelper.WriteScriptFiles(htkConfig.DataDir, htkConfig.TestFileCode, htkConfig.TestFile, HTKConfig.wavExt, HTKConfig.mfcExt);
            //extract features from the test file
            HTKHelper.ExtractFeatures(htkConfig.aOptionsStr, htkConfig.MfccConfigFN, htkConfig.TestFileCode, htkConfig.HCopyExecutable); //test data
            //scan the file with HTK HMM
            HTKHelper.HVite(htkConfig.MfccConfig2FN, htkConfig.tgtDir2, htkConfig.TestFile, htkConfig.wordNet,
                            htkConfig.DictFile, htkConfig.resultTest, htkConfig.monophones, htkConfig.HViteExecutable);
            resultsPath = htkConfig.resultTest;
        }


        public static List<AcousticEvent> GetAcousticEventsFromHTKResults(string resultsPath, string target)
        {
            //A: GET CONFIG INFO USED TO MAKE HMM
            string iniFile  = target + "\\" + HTKConfig.segmentationIniFN;
            string configFile = target + "\\" + HTKConfig.mfccConfigFN;
            string syllableFile = target + "\\" + HTKConfig.labelListFN;
            List<string> targetClasses = HTKConfig.GetSyllableNames(syllableFile);

            //set up Config class and read in the threshold values from the .ini file
            string[] files = new string[1];
            files[0] = iniFile;
            Configuration config = new Configuration(files);
            int sampleRate = config.GetInt(HTKConfig.Key_SAMPLE_RATE); //should be same as used to train the HMM

            //get the framing parmaters - required for calculating time/freq scale
            double windowDuration, windowOffset; //in seconds
            int FreqMin, FreqMax;                //in Herz
            GetFramingParameters(configFile, out windowDuration, out windowOffset, out FreqMin, out FreqMax);


            //read in the results file
            List<string> results = null;
            int count = 0;
            string sourceFile = null; 
            try
            {
                results = FileTools.ReadTextFile(resultsPath);
                string secondLine = results[1].Substring(1, results[1].Length - 2);//read name of source file and remove quotes
                sourceFile = Path.GetFileNameWithoutExtension(secondLine);
                count = results.Count; //number of lines in results file
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR IN METHOD TestHTKRecogniser.GetAcousticEventsFromHTKResults(string resultsPath, string target)");
                Console.WriteLine("Failed to read header of results file: " + resultsPath);
                Console.WriteLine(e.ToString());
            }

            //init a list of events
            List<AcousticEvent> events = new List<AcousticEvent>();

            //calculate time and freq scale
            double frameRate = 1 / windowOffset; //frames per second
            int windowSize = (int)Math.Floor(windowDuration * sampleRate);
            int windowSampleOffset = (int)Math.Floor(windowOffset * sampleRate);
            int hitCount = 0;

            //write header line to Console
            if (TestHTKRecogniser.Verbose == true) Console.WriteLine("#\tcallName\tframes\tscore\tsc/fr\tquality\thit?");

            for (int i = 0; i < count; i++)
            {
                if ((results[i] == "") || (results[i].StartsWith("."))) continue;
                if ((results[i].StartsWith("\"")) || (results[i].StartsWith("#"))) continue;
                //Helper.ParseResultLine(results[i], out start, out end, out className, out score);
                string[] param = Regex.Split(results[i], @"\s+");
                long start = long.Parse(param[0]);
                long end = long.Parse(param[1]);
                string vocalName = param[2];
                float callScore = float.Parse(param[3]);

                if (!targetClasses.Contains(vocalName)) continue; //skip irrelevant lines of HTK results file

                hitCount++; //count hits
                double duration = TimeSpan.FromTicks(end - start).TotalSeconds; //call duration in seconds

                double scoreThreshold = config.GetDouble(vocalName + "_HTK_THRESHOLD");
                double qualityMean = config.GetDouble(vocalName + "_DURATION_MEAN");
                double qualitySD = config.GetDouble(vocalName + "_DURATION_SD");
                double qualityThreshold = config.GetDouble("Key_SD_THRESHOLD");
                //convert default thresholds to user supplied values
                if (!Double.IsNaN(TestHTKRecogniser.SCORE_THRESHOLD)) scoreThreshold = (float)TestHTKRecogniser.SCORE_THRESHOLD;
                if (!Double.IsNaN(TestHTKRecogniser.QUALITY_THRESHOLD)) qualityThreshold = (float)TestHTKRecogniser.QUALITY_THRESHOLD;
                //Console.WriteLine("HTK Threshold=" + scoreThreshold + "  Quality Threshold=" + qualityThreshold);


                //calculate hmm and quality scores
                double frameScore, qualityScore, frameLength;
                bool isHit;
                Helper.ComputeHit(callScore, duration, frameRate, qualityMean, qualitySD, scoreThreshold, qualityThreshold,
                                  out frameLength, out frameScore, out qualityScore, out isHit);

                if (!isHit) continue;  //ignore non-hits
                double startSec = start / (double)10000000;  //convert start(100ns units) to seconds
                var acEvent = new AcousticEvent(startSec, duration, FreqMin, FreqMax);
                acEvent.Name = vocalName;
                if (!isHit) frameScore = scoreThreshold + (frameScore / 5); //reduce score below threshold
                acEvent.SetScores(frameScore, scoreThreshold, -20);
                acEvent.SourceFile = sourceFile;
                acEvent.SetTimeAndFreqScales(sampleRate, windowSize, windowSampleOffset);
                acEvent.Tag = isHit;
                events.Add(acEvent);

                if (TestHTKRecogniser.Verbose == true)
                {
                    Console.WriteLine(hitCount + "\t" + vocalName +
                                      "\t" + frameLength.ToString("f0") +
                                      "\t" + callScore.ToString("f1") +
                                      "\t" + frameScore.ToString("f1") +
                                      "\t" + qualityScore.ToString("f1") + "\t" + isHit);
                }

            }//end for all hits
            return events;
        }


        public static void DisplayAcousticEvents(string wavFile, List<AcousticEvent> events, string target, string outputDir)
        {
            SonogramConfig sonoConfig = new SonogramConfig();
            AudioRecording ar = new AudioRecording(wavFile);
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, ar.GetWavReader());
            if (TestHTKRecogniser.Verbose == true) 
                Console.WriteLine(" Duration=" + sonogram.Duration.TotalSeconds + " s.      Frame count=" + sonogram.FrameCount);
            bool doHighlightSubband = false; bool add1kHzLines = true;
            var image_mt = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image_mt.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            image_mt.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image_mt.AddEvents(events);

            //D: PARSE THE RESULTS FILE TO GET SCORE ARRAY
            string iniFile = target + "\\" + HTKConfig.segmentationIniFN;
            string syllableFile = target + "\\" + HTKConfig.labelListFN;
            List<string> sylNames = HTKConfig.GetSyllableNames(syllableFile);
            foreach (string name in sylNames)
            {
                double[] scores = ExtractScoreArray(events, iniFile, sonogram.FrameCount, name);
                double thresholdFraction = 0.2; //for display purposes only. Fraction of the score track devoted to sub-threshold scores
                //double[] finalScores = NormaliseScores(scores, scoreThreshold, thresholdFraction);
                image_mt.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, thresholdFraction));
            }

            string resultsDir = outputDir + "\\results";
            string fName = Path.GetFileNameWithoutExtension(wavFile);
            string opFile = resultsDir + "\\" + fName + ".png";
            if (TestHTKRecogniser.Verbose == true) Console.WriteLine("\nSonogram will be written to file: " + opFile);
            image_mt.Save(opFile); 
        }



        public static List<AcousticEvent> GetAcousticEventsFromResultsFile(string path)
        {
            var events = new List<AcousticEvent>();
            List<string> lines = FileTools.ReadTextFile(path);
            int minFreq = 0; //dummy value - never to be used
            int maxfreq = 0; //dummy value - never to be used
            foreach (string line in lines)
            {
                string[] words = Regex.Split(line, @"\t");
                string name = words[0];
                double start = Double.Parse(words[1]);
                double end = Double.Parse(words[2]);
                double score = Double.Parse(words[3]);
                string file = words[4];
                //Console.WriteLine("{0,10}{1,6:f1} ...{2,6:f1}{3,10:f1}", name, start, end, score);
                var ae   = new AcousticEvent(start, (end - start), minFreq, maxfreq);
                ae.Score = score;
                ae.Name  = name;
                ae.SourceFile = file;
                events.Add(ae);
            }
            return events;
        } //end method GetLabelsInFile(List<string> labels, string file)


    }//end class
}
