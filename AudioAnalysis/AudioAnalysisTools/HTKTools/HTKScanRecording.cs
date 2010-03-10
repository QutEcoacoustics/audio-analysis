using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using TowseyLib;



namespace AudioAnalysisTools.HTKTools
{
    public class HTKScanRecording
    {

        //The following two thresholds are used to modify the results delivered by the HMM.
        //Their use was described in the Songscope paper. They provide a further check on the output form the HMM.
        //FORCE THRESHOLDS FOR Experimentation --- set = Double.NaN  if do not want to override default values
        const double SCORE_THRESHOLD = -50.0;
        const double QUALITY_THRESHOLD = 2.56;   //1.96 for p=0.05;  2.56 for p=0.01




        public static string Execute(string WavFile, string workingDirectory, string templateFN)
        {
            Console.WriteLine("Executing HTK_Recogniser - scanning a .wav file");

            #region Variables
            HTKConfig htkConfig = new HTKConfig();
            htkConfig.WorkingDir = workingDirectory;
            htkConfig.CallName = Path.GetFileNameWithoutExtension(templateFN);
            htkConfig.DataDir = htkConfig.WorkingDir + "\\data";
            htkConfig.ConfigDir = htkConfig.WorkingDir + "\\" + htkConfig.CallName;
            htkConfig.ResultsDir = htkConfig.WorkingDir + "\\results";
            htkConfig.HTKDir = htkConfig.ConfigDir + "\\HTK";
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
            string target = htkConfig.WorkingDir + "\\" + htkConfig.CallName;
            ZipUnzip.UnZip(target, templateFN, true);

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
            return htkConfig.resultTest;
        }



        public static void GetScoringParameters(string iniFile, out float HTKThreshold, out float QualityMean, out float QualitySD, out float QualityThreshold)
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

        public static void GetSampleRate(string iniFile, out int sr, out int wSize)
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

        public static void GetFramingParameters(string configFile, out double WindowSize, out double WindowOffset, out int FreqMin, out int FreqMax)
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

                //calculate hmm and quality scores
                double duration = TimeSpan.FromTicks(end - start).TotalSeconds; //call duration in seconds
                double normScore, qualityScore, frameLength;
                bool isHit;
                Helper.ComputeHit(score, duration, frameRate, qualityMean, qualitySD, scoreThreshold, qualityThreshold,
                                  out frameLength, out normScore, out qualityScore, out isHit);

                double startSec = start / (double)10000000;  //start in seconds
                double endSec = end / (double)10000000;  //end   in seconds
                int startFrame = (int)(startSec * frameRate);
                int endFrame = (int)(endSec * frameRate);
                //double frameLength = (endSec - startSec) * frameRate;

                Log.WriteLine("sec=" + startSec.ToString("f1") + "-" + endSec.ToString("f1") +
                                  "\t " + (endSec - startSec).ToString("f2") + "s" +
                                  "\t frames=" + frameLength.ToString("f0") +
                                  "\t score=" + score.ToString("f1") +
                                  "\t normScore=" + normScore.ToString("f1") +
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
            Log.WriteLine("avNormScore=" + (avScore / hitCount).ToString("f2"));
            Log.WriteLine("av Duration=" + (avDuration / hitCount).ToString("f3") + " or " + (avFrames / hitCount).ToString("f1") + " frames.");
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


        public static List<AcousticEvent> GetAcousticEventsFromHTKResults(string resultsPath, string target)
        {
            //A: GET CONFIG INFO USED TO MAKE HMM
            string iniFile = target + "\\" + HTKConfig.segmentationIniFN;
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
            HTKScanRecording.GetFramingParameters(configFile, out windowDuration, out windowOffset, out FreqMin, out FreqMax);


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
            Log.WriteLine("#\tcallName\tframes\tscore\tsc/fr\tquality\thit?");

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
                if (!Double.IsNaN(HTKScanRecording.SCORE_THRESHOLD)) scoreThreshold = (float)HTKScanRecording.SCORE_THRESHOLD;
                if (!Double.IsNaN(HTKScanRecording.QUALITY_THRESHOLD)) qualityThreshold = (float)HTKScanRecording.QUALITY_THRESHOLD;
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

                Log.WriteLine(hitCount + "\t" + vocalName +
                                      "\t" + frameLength.ToString("f0") +
                                      "\t" + callScore.ToString("f1") +
                                      "\t" + frameScore.ToString("f1") +
                                      "\t" + qualityScore.ToString("f1") + "\t" + isHit);

            }//end for all hits
            return events;
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
                var ae = new AcousticEvent(start, (end - start), minFreq, maxfreq);
                ae.Score = score;
                ae.Name = name;
                ae.SourceFile = file;
                events.Add(ae);
            }
            return events;
        } //end method GetLabelsInFile(List<string> labels, string file)

    }
}
