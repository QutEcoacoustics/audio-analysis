using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.Text.RegularExpressions;
using TowseyLib;
using AudioAnalysis;


namespace HMMBuilder
{
    public class TestHTKProgram
    {

        /// <summary>
        /// Runs a prepared HTK template over a file
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

            //*******************************************************************************************************************
            #region Variables
            string workingDirectory = "C:\\SensorNetworks\\temp"; //set default working directory          // ARG 0  
            //*******************************************************************************************************************
            //THE TEMPLATE
            string templateDir  = "C:\\SensorNetworks\\Templates\\Template_";
            //string templateName = "CURRAWONG1";
            //string templateName = "CURLEW1";
            //string templateName = "WHIPBIRD1";
            //string templateName = "CURRAWONG1";
            //string templateName = "KOALAFEMALE1";
            string templateName = "KOALAFEMALE2";
            string templateFN = templateDir + templateName + "\\" + templateName + ".zip";  // ARG 1
            //*******************************************************************************************************************
            //THE WAV FILE TO PROCESS
            //string wavFile = "C:\\SensorNetworks\\WavFiles\\TestWaveFile\\St_Bees_Currawong_20080919-060000_13.wav";          //ARG 2
            //string wavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\West_Knoll_St_Bees_Currawong3_20080919-060000.wav";       //ARG 2
            //string wavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\Top_Knoll_St_Bees_Curlew2_20080922-030000.wav";           //ARG 2
            //string wavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\Honeymoon_Bay_St_Bees_KoalaBellow_20080905-001000.wav";   //ARG 2
            //string wavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\WestKnoll_StBees_KoalaBellow20080919-073000.wav";//contains currawong
            //string wavFile = @"C:\SensorNetworks\WavFiles\BridgeCreek\\cabin_GoldenWhistler_file0127_extract1.wav";
            string wavFile = @"C:\SensorNetworks\WavFiles\Koala_Female\HoneymoonBay_StBees_20081027-023000.wav";
            //string wavFile = @"C:\SensorNetworks\WavFiles\Koala_Female\WestKnoll_StBees_20081216-213000.wav";
            //string wavFile = @"C:\SensorNetworks\WavFiles\Koala_Female\WestKnoll_StBees3_20090907-053000.wav";//mix of curlew,currawong, koalas
            //*******************************************************************************************************************

            //GET THE COMMAND LINE ARGUMENTS
            if (args.Length >= 1) workingDirectory = args[0]; //where to place output
            if (args.Length >= 2) templateFN = args[1]; //template file name
            if (args.Length == 3) wavFile = args[2]; //wav file to process

            string resultsPath;
            #endregion

            Execute(workingDirectory, templateFN, wavFile, out resultsPath);
            Console.WriteLine("FINISHED HTK SCAN OF FILE");
            //Console.ReadLine();
            
   
            //GATHER PARAMETERS FOR SCORING AND PRESENTATION OF RESULTS
            //A: GET THRESHOLDS FROM INI FILE
            string target  = workingDirectory + "\\" + templateName;
            string iniFile = target + "\\segmentation.ini";
            float scoreThreshold, qualityMean, qualitySD, qualityThreshold;
            GetScoringParameters(iniFile, out scoreThreshold, out qualityMean, out qualitySD, out qualityThreshold);
            int sampleRate, windowSize;
            GetSampleRate(iniFile, out sampleRate, out windowSize);//should be same as used to train the HMM

            //can reset thresholds here for experimentation
            scoreThreshold   = -40.0f;
            qualityThreshold = 1.96f;
            Console.WriteLine("HTK Threshold=" + scoreThreshold + "  Quality Threshold=" + qualityThreshold);


            //B: GET FRAMING PARAMETERS USED TO MAKE HMM
            string configFile = target + "\\mfccConfig.txt";
            double WindowDuration, WindowOffset; //in seconds
            int FreqMin, FreqMax;
            GetFramingParameters(configFile, out WindowDuration, out WindowOffset, out FreqMin, out FreqMax);

            //C: PARSE THE RESULTS FILE TO RETURN ACOUSTIC EVENTS
            Console.WriteLine("\nParse the HMM results file and return Acoustic Events");
            List<AcousticEvent> events = GetAcousticEvents(resultsPath, templateName, 
                                                           sampleRate, WindowDuration, WindowOffset, FreqMin, FreqMax,
                                                           qualityMean, qualitySD, scoreThreshold, qualityThreshold);



            Console.WriteLine("\nPreparing sonogram");
            AudioAnalysis.SonogramConfig sonoConfig = new SonogramConfig();
            AudioRecording ar = new AudioRecording(wavFile);
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, ar.GetWavReader());
            Console.WriteLine(" Duration=" + sonogram.Duration.TotalSeconds + " s.      Frame count=" + sonogram.FrameCount);
            bool doHighlightSubband = false; bool add1kHzLines = true;
            var image_mt = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image_mt.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            image_mt.AddTrack(Image_Track.GetSegmentationTrack(sonogram));

            //D: PARSE THE RESULTS FILE To GET SCORE ARRAY
            List<string> hmmResults = FileTools.ReadTextFile(resultsPath);
            double[] scores = ParseHmmScores(hmmResults, sonogram.FrameCount, WindowOffset, templateName,
                                             scoreThreshold, qualityMean, qualitySD, qualityThreshold);

            double thresholdFraction = 0.2;//for display purposes only. Fraction of the score track devoted to sub-threshold scores
            double[] finalScores = NormaliseScores(scores, scoreThreshold, thresholdFraction);
            image_mt.AddTrack(Image_Track.GetScoreTrack(finalScores, 0.0, 1.0, thresholdFraction));
            image_mt.AddEvents(events);


            string resultsDir = workingDirectory + "\\results";
            string fName = Path.GetFileNameWithoutExtension(wavFile);
            string opFile = resultsDir + "\\" + fName + ".png";
            Console.WriteLine("\nSonogram will be written to file: " + opFile);
            image_mt.Save(opFile); 

            Console.WriteLine("FINISHED!");
            Console.ReadLine();
        }// end Main()


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



        static List<AcousticEvent> GetAcousticEvents(string resultsPath, string targetClass, int sampleRate,
                                                     double windowDuration, double windowOffset, int FreqMin, int FreqMax,
                                                     double qualityMean, double qualitySD, double scoreThreshold, double qualityThreshold)
        {
            List<string> results = FileTools.ReadTextFile(resultsPath);

            List<AcousticEvent> events = new List<AcousticEvent>();
            int count = results.Count; //number of lines in results file

            double frameRate       = 1 / windowOffset; //frames per second
            int windowSize         = (int)Math.Floor(windowDuration * sampleRate);
            int windowSampleOffset = (int)Math.Floor(windowOffset   * sampleRate);
            int hitCount = 0;

            for (int i = 0; i < count; i++)
            {
                if ((results[i] == "") || (results[i].StartsWith("."))) continue;
                if ((results[i].StartsWith("\"")) || (results[i].StartsWith("#"))) continue;
                //Helper.ParseResultLine(results[i], out start, out end, out className, out score);
                string[] param = Regex.Split(results[i], @"\s+");
                long start = long.Parse(param[0]);
                long end = long.Parse(param[1]);
                string vocalName = param[2];
                float score = float.Parse(param[3]);

                if (!vocalName.StartsWith(targetClass)) continue; //skip irrelevant lines
                hitCount++; //count hits

                double duration  = TimeSpan.FromTicks(end - start).TotalSeconds; //call duration in seconds

                //calculate hmm and quality scores
                double normScore, qualityScore, frameLength;
                bool isHit;
                Helper.ComputeHit(score, duration, frameRate, qualityMean, qualitySD, scoreThreshold, qualityThreshold,
                                  out frameLength, out normScore, out qualityScore, out isHit);

                if (isHit) //init an acoustic event
                {
                    double startSec = start / (double)10000000;  //convert start(100ns units) to seconds
                    var acEvent = new AcousticEvent(startSec, duration, FreqMin, FreqMax);
                    acEvent.SetNormalisedScore(normScore, scoreThreshold, -20);
                    acEvent.SetTimeAndFreqScales(sampleRate, windowSize, windowSampleOffset);
                    events.Add(acEvent);
                }

                Console.WriteLine("hitCount=" + hitCount +
                                  "\t frames=" + frameLength.ToString("f0") +
                                  "\t score=" + score.ToString("f1") +
                                  "\t normScore=" + normScore.ToString("f1") +
                                  "\t qualityScore=" + qualityScore.ToString("f1") + "\t HIT=" + isHit);


            }//end for all hits
            return events;
        }



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
                int startFrame = (int)(startSec * frameRate);
                int endFrame = (int)(endSec * frameRate);
                //double frameLength = (endSec - startSec) * frameRate;

                Console.WriteLine("sec=" + startSec.ToString("f1") + "-" + endSec.ToString("f1") +
                                  "\t " + (endSec - startSec).ToString("f2") +"s"+
                                  "\t frames=" + frameLength.ToString("f0") +
                                  "\t score=" + score.ToString("f1") +
                                  "\t normScore=" + normScore.ToString("f1")+
                                  "\t qualityScore=" + qualityScore.ToString("f1") + "\t HIT=" + isHit);

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
            Console.WriteLine("avNormScore=" + (avScore / hitCount).ToString("f2"));
            Console.WriteLine("av Duration=" + (avDuration / hitCount).ToString("f3") +" or " + (avFrames/hitCount).ToString("f1")+" frames.");
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


        //##########################################################################################################
        //##########################################################################################################
        //##########################################################################################################
        //##########################################################################################################
        //##########################################################################################################
        //##########################################################################################################
        //##########################################################################################################
       
        public static void Execute(string workingDirectory, string templateFN, string WavFile, out string resultsPath)
        {
            Console.WriteLine("Executing HMMBuilder - scanning a .wav file");

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

            //shift template to working directory and unzip
            string zipFile = htkConfig.WorkingDir + "\\" + Path.GetFileName(templateFN);
            string target = htkConfig.WorkingDir + "\\" + htkConfig.CallName;
            File.Copy(templateFN, zipFile, true);
            Console.WriteLine("zipFile=" + zipFile + "  target=" + target);

            ZipUnzip.UnZip(target, zipFile, true);

            //move the data/TEST file to its own directory
            Directory.CreateDirectory(htkConfig.DataDir);
            string dataFN = Path.GetFileName(WavFile);
            File.Copy(WavFile, htkConfig.DataDir + "\\" + dataFN, true);

            //PREPARE THE TEST FILE AND EXTRACT FEATURES
            //write script files
            HTKHelper.WriteScriptFiles(htkConfig.DataDir, htkConfig.TestFileCode, htkConfig.TestFile, htkConfig.wavExt, htkConfig.mfcExt);
            //extract features from the test file
            HTKHelper.ExtractFeatures(htkConfig.aOptionsStr, htkConfig.MfccConfigFN, htkConfig.TestFileCode, htkConfig.HCopyExecutable); //test data
            //scan the file with HTK HMM
            HTKHelper.HVite(htkConfig.MfccConfig2FN, htkConfig.tgtDir2, htkConfig.TestFile, htkConfig.wordNet,
                            htkConfig.DictFile, htkConfig.resultTest, htkConfig.monophones, htkConfig.HViteExecutable);
            resultsPath = htkConfig.resultTest;
        }


    }//end class
}
