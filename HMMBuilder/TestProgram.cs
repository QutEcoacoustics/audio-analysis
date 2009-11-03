using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using TowseyLib;
using AudioAnalysis;


namespace HMMBuilder
{
    public class TestProgram
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Executing HMMBuilder - scanning a .wav file");

            #region Variables
            HTKConfig htkConfig = new HTKConfig();
            //SET DEFAULT VALUES FOR HTE COMMAND LINE ARGUMENTS
            htkConfig.WorkingDir = "C:\\SensorNetworks\\temp"; //set default working directory              // ARG 0  
            string templateFN    = "C:\\SensorNetworks\\Templates\\Template_CURRAWONG1\\CURRAWONG1.zip";    // ARG 1
            string testWavFile   = "C:\\SensorNetworks\\WavFiles\\TestWaveFile\\St_Bees_Currawong_20080919-060000_13.wav"; //ARG 2

            //GET THE COMMAND LINE ARGUMENTS
            if (args.Length >= 1) htkConfig.WorkingDir = args[0]; //where to place output
            if (args.Length >= 2) templateFN           = args[1]; //template file name
            if (args.Length == 3) testWavFile          = args[2]; //wav file to process

            //*******************************************************************************************************************
            //COMMENT THESE LINES BEFORE DEPLOYMENT
            //templateFN = "C:\\SensorNetworks\\Templates\\Template_CURLEW1\\CURLEW1.zip";                               //ARG 1
            //testWavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\West_Knoll_St_Bees_Currawong3_20080919-060000.wav";   //ARG 2
            //testWavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\Top_Knoll_St_Bees_Curlew2_20080922-030000.wav";         //ARG 2
            //testWavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\Honeymoon_Bay_St_Bees_KoalaBellow_20080905-001000.wav"; //ARG 2
            testWavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\WestKnoll_StBees_KoalaBellow20080919-073000.wav";//contains currawong
            //*******************************************************************************************************************


            htkConfig.CallName = Path.GetFileNameWithoutExtension(templateFN);

            //htkConfig.WorkingDir      = Directory.GetCurrentDirectory();
            htkConfig.DataDir         = htkConfig.WorkingDir + "\\data";
            htkConfig.ConfigDir       = htkConfig.WorkingDir + "\\" + htkConfig.CallName;
            htkConfig.ResultsDir      = htkConfig.WorkingDir + "\\results";
            htkConfig.HTKDir          = htkConfig.ConfigDir  + "\\HTK"; 
            htkConfig.SegmentationDir = htkConfig.ConfigDir  + "\\Segmentation";
            htkConfig.SilenceModelPath = htkConfig.SegmentationDir + "\\West_Knoll_St_Bees_Currawong1_20080923-120000.wav";
            
        
            //Console.WriteLine("CWD=" + htkConfig.WorkingDir);
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
            string target  = htkConfig.WorkingDir + "\\" + htkConfig.CallName;
            File.Copy(templateFN, zipFile, true);
            Console.WriteLine("zipFile=" + zipFile + "  target=" + target);

            ZipUnzip.UnZip(target, zipFile, true);

            //move the data/TEST file to its own directory
            Directory.CreateDirectory(htkConfig.DataDir);
            string dataFN = Path.GetFileName(testWavFile);
            File.Copy(testWavFile, htkConfig.DataDir+"\\"+ dataFN, true);

            Console.WriteLine("RESOURCES IN POSITION");
            //Console.ReadLine();

            //PREPARE THE TEST FILE AND EXTRACT FEATURES
            //write script files
            HTKHelper.WriteScriptFiles(htkConfig.DataDir, htkConfig.TestFileCode, htkConfig.TestFile, htkConfig.wavExt, htkConfig.mfcExt);
            //extract features from the test file
            HTKHelper.ExtractFeatures(htkConfig.aOptionsStr, htkConfig.MfccConfigFN, htkConfig.TestFileCode, htkConfig.HCopyExecutable); //test data
            //scan the file with HTK HMM
            HTKHelper.HVite(htkConfig.MfccConfig2FN, htkConfig.tgtDir2, htkConfig.TestFile, htkConfig.wordNet,
                            htkConfig.DictFile, htkConfig.resultTest, htkConfig.monophones, htkConfig.HViteExecutable);


            Console.WriteLine("HTK DONE");
            //Console.ReadLine();

            //GET THRESHOLDS FROM INI FILE
            string key = "HTK_THRESHOLD";
            string str = FileTools.ReadPropertyFromFile(target + "\\segmentation.ini", key);
            //float threshold = -2500f;
            float HTKThreshold = float.Parse(str);
            Console.WriteLine("HTKThreshold= " + HTKThreshold);
            key = "DURATION_MEAN";
            str = FileTools.ReadPropertyFromFile(target + "\\segmentation.ini", key);
            float DurationMean = float.Parse(str);
            Console.WriteLine("DurationMean= " + DurationMean);
            key = "DURATION_SD";
            str = FileTools.ReadPropertyFromFile(target + "\\segmentation.ini", key);
            float DurationSD = float.Parse(str);
            Console.WriteLine("DurationSD= " + DurationSD);
            key = "SD_THRESHOLD";
            str = FileTools.ReadPropertyFromFile(target + "\\segmentation.ini", key);
            float SD_THRESHOLD = float.Parse(str);
            Console.WriteLine("SD_THRESHOLD= " + SD_THRESHOLD);


            Console.WriteLine("\n\nParsing the HMM results file");
            Console.WriteLine(" There must be ONE AND ONLY ONE header line.");
            List<string> hmmResults = FileTools.ReadTextFile(htkConfig.resultTest);

            Console.WriteLine("\nPreparing sonogram");
            AudioAnalysis.SonogramConfig sonoConfig = new SonogramConfig();
            AudioRecording ar = new AudioRecording(testWavFile);
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, ar.GetWavReader());
            Console.WriteLine(" Duration=" + sonogram.Duration.TotalSeconds + " s.      Frame count=" + sonogram.FrameCount);
            bool doHighlightSubband = false; bool add1kHzLines = true;
            var image_mt = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image_mt.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            image_mt.AddTrack(Image_Track.GetSegmentationTrack(sonogram));

            double[] hmmScores = ParseHmmScores(hmmResults, sonogram.Duration, sonogram.FrameCount, htkConfig.CallName);
            Console.WriteLine();
            double[] qualityScores = ParseQualityScores(hmmResults, sonogram.Duration, sonogram.FrameCount, htkConfig.CallName,
                                                        DurationMean, DurationSD);

            double thresholdFraction = 0.2;//for display purposes only. Fraction of the score track devoted to sub-threshold scores
            double[] finalScores = MergeScores(hmmScores, qualityScores, HTKThreshold, thresholdFraction, SD_THRESHOLD);
            image_mt.AddTrack(Image_Track.GetScoreTrack(finalScores, 0.0, 1.0, thresholdFraction));


            string fName = Path.GetFileNameWithoutExtension(testWavFile);
            string opFile = htkConfig.ResultsDir + "\\" + fName + ".png";
            Console.WriteLine("\nSonogram will be written to file: " + opFile);
            image_mt.Save(opFile);
            //template.SaveResultsImage(recording.GetWavData(), imagePath, result, hmmResults); //WITH HMM SCORE

            Console.WriteLine("FINISHED!");
            Console.ReadLine();
        }// end Main()



        /// <summary>
        /// Parses the HMM scores returned by HTK and returns a set of scores in 0-1 suitable for display as a score track.
        /// </summary>
        /// <param name="results">the HTK results</param>
        /// <param name="duration">the duration of a frame</param>
        /// <param name="frameCount">number of frames in the recording</param>
        /// <param name="targetClass">Name of the target class as shown in the HTK results file</param>
        /// <returns>array of hmm scores ONLY WHERE THERE ARE HITS. All other values are set as NaN</returns>
        public static double[] ParseHmmScores(List<string> results, TimeSpan duration, int frameCount, string targetClass)
        {
            double[] scores = new double[frameCount];
            for (int i = 0; i < frameCount; i++) scores[i] = Double.NaN; //init to NaNs.
            int hitCount = results.Count;
            for (int i = 1; i < hitCount; i++)
            {
                if ((results[i] == "")            || (results[i].StartsWith("."))) continue;
                if ((results[i].StartsWith("\"")) || (results[i].StartsWith("#"))) continue;
                long start;
                long end;
                string className;
                double score;
                Helper.ParseResultLine(results[i], out start, out end, out className, out score);
                double startSec = start / (double)10000000;  //start in seconds
                double endSec   = end   / (double)10000000;  //end   in seconds

                int startFrame = (int)((startSec / (double)duration.TotalSeconds) * frameCount);
                int endFrame   = (int)((endSec   / (double)duration.TotalSeconds) * frameCount);
                if (className.StartsWith(targetClass))
                {
                    Console.WriteLine("sec=" + startSec.ToString("f1") + " - " + endSec.ToString("f1") +
                                      "  frames=" + startFrame + "-" + endFrame + "  score=" + score.ToString("f0"));

                    for (int s = startFrame; s <= endFrame; s++) scores[s] = score;
                }
            }
            return scores;
        }

        /// <summary>
        /// Parses the HMM scores returned by HTK and returns a set of scores in 0-1 suitable for display as a score track.
        /// </summary>
        /// <param name="results">the HTK results</param>
        /// <param name="duration">the duration of a frame</param>
        /// <param name="frameCount">number of frames in the recording</param>
        /// <param name="targetClass">Name of the target class as shown in the HTK results file</param>
        /// <param name="threshold">the HTK score threshold</param>
        /// <param name="thresholdFraction">threshold fraction between 0.0-1.0. Helps with display of score track.</param>
        /// <returns></returns>
        public static double[] ParseQualityScores(List<string> results, TimeSpan duration, int frameCount, string targetClass,
                                                  float mean, float sd)
        {
            double[] quality = new double[frameCount]; //to store quality scores
            int hitCount = results.Count;
            for (int i = 1; i < hitCount; i++)
            {
                //Console.WriteLine(i+ "  " + results[i]);
                if ((results[i] == "") || (results[i].StartsWith("."))) continue;
                if ((results[i].StartsWith("\"")) || (results[i].StartsWith("#"))) continue;
                long start;
                long end;
                string className;
                double score; //ignored - just want time duration of the hit
                Helper.ParseResultLine(results[i], out start, out end, out className, out score);
                double startSec = start / (double)10000000;  //start in seconds
                double endSec = end / (double)10000000;  //start in seconds
                double span = TimeSpan.FromTicks(end - start).TotalSeconds; //duration in seconds

                double zscore = (span - mean) / sd;

                int startFrame = (int)((startSec / (double)duration.TotalSeconds) * frameCount);
                int endFrame = (int)((endSec / (double)duration.TotalSeconds) * frameCount);
                if (className.StartsWith(targetClass))
                {
                    Console.WriteLine("sec=" + startSec.ToString("f1") + " - " + endSec.ToString("f1") +
                                      "  frames=" + startFrame + "-" + endFrame + "  score=" + score.ToString("f0") + "  zscore=" + zscore.ToString("f3"));

                    for (int s = startFrame; s <= endFrame; s++)
                    {
                        quality[s] = zscore;
                    }
                }
            }
            return quality;
        }

        public static double[] MergeScores(double[] hmmScores, double[] qualityScores, float htkThreshold, double thresholdFraction, float qualityThreshold)
        {
            double offset = (thresholdFraction * htkThreshold) / (1 - thresholdFraction);
            //Console.WriteLine("offset=" + offset);
            double min = htkThreshold + offset;
            Console.WriteLine("\nmin=" + min);

            int frameCount = hmmScores.Length;
            double[] normScores = new double[frameCount]; //the final normalised scores
            for (int i = 0; i < frameCount; i++)
            {
                //normalise score between 0 - 1. Assume max score=0.000
                if (Double.IsNaN(hmmScores[i])) normScores[i] = 0.0;
                else                            normScores[i] = (hmmScores[i] - min) / Math.Abs(min); 
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
       
        public static void Execute(string workingDirectory, string templateFN, string WavFile)
        {
            Console.WriteLine("Executing HMMBuilder - scanning a .wav file");

            #region Variables
            HTKConfig htkConfig = new HTKConfig();
            htkConfig.WorkingDir = workingDirectory;

            //float threshold = -2500f;
            htkConfig.CallName = Path.GetFileNameWithoutExtension(templateFN);

            //htkConfig.WorkingDir      = Directory.GetCurrentDirectory();
            htkConfig.DataDir = htkConfig.WorkingDir + "\\data";
            htkConfig.ConfigDir = htkConfig.WorkingDir + "\\" + htkConfig.CallName;
            htkConfig.ResultsDir = htkConfig.WorkingDir + "\\results";
            htkConfig.HTKDir = htkConfig.ConfigDir + "\\HTK";
            htkConfig.SegmentationDir = htkConfig.ConfigDir + "\\Segmentation";
            htkConfig.SilenceModelPath = htkConfig.SegmentationDir + "\\West_Knoll_St_Bees_Currawong1_20080923-120000.wav";


            //Console.WriteLine("CWD=" + htkConfig.WorkingDir);
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

            Console.WriteLine("FINISHED!");
        }


    }//end class
}
