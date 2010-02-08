using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.Text.RegularExpressions;
using TowseyLib;
using AudioTools;
using AudioAnalysisTools;
using AudioAnalysisTools.HTKTools;


namespace HMMBuilder
{
    public class TestProgram
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Executing HMMBuilder - scanning a .wav file");

            #region Variables
            HTKConfig htkConfig = new HTKConfig();
            //htkConfig.WorkingDir      = Directory.GetCurrentDirectory();

            //SET THESE DEFAULT VALUES FOR THE COMMAND LINE ARGUMENTS
            htkConfig.WorkingDir = "C:\\SensorNetworks\\temp"; //set default working directory             // ARG 0  
            //string templateFN = "C:\\SensorNetworks\\Templates\\Template_CURRAWONG1\\CURRAWONG1.zip";    // ARG 1
            //string templateFN = "C:\\SensorNetworks\\Templates\\Template_CURLEW1\\CURLEW1.zip";          // ARG 1
            string templateFN = "C:\\SensorNetworks\\Templates\\Template_WHIPBIRD1\\WHIPBIRD1.zip";    // ARG 1
            string testWavFile = "C:\\SensorNetworks\\WavFiles\\TestWaveFile\\St_Bees_Currawong_20080919-060000_13.wav"; //ARG 2

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
            //testWavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\WestKnoll_StBees_KoalaBellow20080919-073000.wav";//contains currawong
            testWavFile = @"C:\SensorNetworks\WavFiles\BridgeCreek\\cabin_GoldenWhistler_file0127_extract1.wav";
            //*******************************************************************************************************************


            htkConfig.CallName = Path.GetFileNameWithoutExtension(templateFN);

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
            HTKHelper.WriteScriptFiles(htkConfig.DataDir, htkConfig.TestFileCode, htkConfig.TestFile, HTKConfig.wavExt, HTKConfig.mfcExt);
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
            float HTKThreshold = float.Parse(str);
            Console.WriteLine("HTKThreshold= " + HTKThreshold);
            key = "DURATION_MEAN";
            str = FileTools.ReadPropertyFromFile(target + "\\segmentation.ini", key);
            float QualityMean = float.Parse(str);
            Console.WriteLine("DurationMean= " + QualityMean);
            key = "DURATION_SD";
            str = FileTools.ReadPropertyFromFile(target + "\\segmentation.ini", key);
            float QualitySD = float.Parse(str);
            Console.WriteLine("DurationSD= " + QualitySD);
            key = "SD_THRESHOLD";
            str = FileTools.ReadPropertyFromFile(target + "\\segmentation.ini", key);
            float QualityThreshold = float.Parse(str);
            Console.WriteLine("SD_THRESHOLD= " + QualityThreshold);


            Console.WriteLine("\nParsing the HMM results file");
            List<string> hmmResults = FileTools.ReadTextFile(htkConfig.resultTest);//There must be ONE AND ONLY ONE header line.

            Console.WriteLine("\nPreparing sonogram");
            AudioAnalysisTools.SonogramConfig sonoConfig = new SonogramConfig();
            AudioRecording ar = new AudioRecording(testWavFile);
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, ar.GetWavReader());
            Console.WriteLine(" Duration=" + sonogram.Duration.TotalSeconds + " s.      Frame count=" + sonogram.FrameCount);
            bool doHighlightSubband = false; bool add1kHzLines = true;
            var image_mt = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image_mt.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            image_mt.AddTrack(Image_Track.GetSegmentationTrack(sonogram));

            //can reset thresholds here for experimentation
            //HTKThreshold = -100.0f;
            //QualityThreshold = 40.0f;

            Console.WriteLine("HTKThreshold=" + HTKThreshold + "    QualityThreshold=" + QualityThreshold + "   frameRate=" + sonogram.FramesPerSecond);
            double[] scores = ParseHmmScores(hmmResults, sonogram.FrameCount, sonogram.FramesPerSecond, htkConfig.CallName,
                                             HTKThreshold, QualityMean, QualitySD, QualityThreshold);

            double thresholdFraction = 0.2;//for display purposes only. Fraction of the score track devoted to sub-threshold scores
            double[] finalScores = NormaliseScores(scores, HTKThreshold, thresholdFraction);
            image_mt.AddTrack(Image_Track.GetScoreTrack(finalScores, 0.0, 1.0, thresholdFraction));


            string fName  = Path.GetFileNameWithoutExtension(testWavFile);
            string opFile = htkConfig.ResultsDir + "\\" + fName + ".png";
            Console.WriteLine("\nSonogram will be written to file: " + opFile);
            image_mt.Save(opFile);

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
        public static double[] ParseHmmScores(List<string> results, int frameCount, double frameRate, string targetClass,
                                              double scoreThreshold, double qualityMean, double qualitySD, double qualityThreshold)
        {
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
                    normScore = scoreThreshold + (normScore / 5);
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
            HTKHelper.WriteScriptFiles(htkConfig.DataDir, htkConfig.TestFileCode, htkConfig.TestFile, HTKConfig.wavExt, HTKConfig.mfcExt);
            //extract features from the test file
            HTKHelper.ExtractFeatures(htkConfig.aOptionsStr, htkConfig.MfccConfigFN, htkConfig.TestFileCode, htkConfig.HCopyExecutable); //test data
            //scan the file with HTK HMM
            HTKHelper.HVite(htkConfig.MfccConfig2FN, htkConfig.tgtDir2, htkConfig.TestFile, htkConfig.wordNet,
                            htkConfig.DictFile, htkConfig.resultTest, htkConfig.monophones, htkConfig.HViteExecutable);

            Console.WriteLine("FINISHED!");
        }


    }//end class
}
