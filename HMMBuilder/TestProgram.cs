using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using TowseyLib;
using AudioAnalysis;


namespace HMMBuilder
{
    class TestProgram
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
            if (args.Length >= 1) htkConfig.WorkingDir = args[0];
            if (args.Length >= 2) templateFN           = args[1];
            if (args.Length == 3) testWavFile          = args[2];

            //*******************************************************************************************************************
            //COMMENT THESE LINES BEFORE DEPLOYMENT
            templateFN = "C:\\SensorNetworks\\Templates\\Template_CURLEW1\\CURLEW1.zip";                               // ARG 1
            testWavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\West_Knoll_St_Bees_Currawong3_20080919-060000.wav"; //ARG 2
            //testWavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\Top_Knoll_St_Bees_Curlew2_20080922-030000.wav";       //ARG 2
            //testWavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\Honeymoon_Bay_St_Bees_KoalaBellow_20080905-001000.wav"; //ARG 2
            //testWavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\WestKnoll_StBees_KoalaBellow20080919-073000.wav";//contains currawong
            //*******************************************************************************************************************


            float threshold = -2500f;
            htkConfig.CallName = Path.GetFileNameWithoutExtension(templateFN);

            //htkConfig.WorkingDir      = Directory.GetCurrentDirectory();
            htkConfig.DataDir         = htkConfig.WorkingDir + "\\data";
            htkConfig.ConfigDir       = htkConfig.WorkingDir + "\\" + htkConfig.CallName;
            htkConfig.ResultsDir      = htkConfig.WorkingDir + "\\results";
            htkConfig.SilenceModelPath = htkConfig.ConfigDir + "\\SilenceModel\\West_Knoll_St_Bees_Currawong1_20080923-120000.wav";
            htkConfig.HTKDir          = htkConfig.ConfigDir  + "\\HTK"; 
            htkConfig.SegmentationDir = htkConfig.ConfigDir  + "\\Segmentation";
            
        
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

            //PREPARE THE TEST FILE AND EXTRACT FEATURES
            //write script files
            HTKHelper.WriteScriptFiles(htkConfig.DataDir, htkConfig.TestFileCode, htkConfig.TestFile, htkConfig.wavExt, htkConfig.mfcExt);
            //extract features from the test file
            HTKHelper.ExtractFeatures(htkConfig.aOptionsStr, htkConfig.MfccConfigFN, htkConfig.TestFileCode, htkConfig.HCopyExecutable); //test data
            //scan the file with HTK HMM
            HTKHelper.HVite(htkConfig.MfccConfig2FN, htkConfig.tgtDir2, htkConfig.TestFile, htkConfig.wordNet,
                            htkConfig.DictFile, htkConfig.resultTest, htkConfig.monophones, htkConfig.HViteExecutable);




            Console.WriteLine("\n\nParsing the HMM results file");
            Console.WriteLine(" There must be ONE AND ONLY ONE header line.");
            List<string> hmmResults = FileTools.ReadTextFile(htkConfig.resultTest);

            Console.WriteLine("\nPreparing sonogram");
            AudioAnalysis.BaseSonogramConfig sonoConfig = new BaseSonogramConfig();
            AudioRecording ar = new AudioRecording(testWavFile);
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, ar.GetWavReader());
            Console.WriteLine(" Duration=" + sonogram.Duration.TotalSeconds + " s.      Frame count=" + sonogram.FrameCount);
            bool doHighlightSubband = false; bool add1kHzLines = true;
            var image_mt = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image_mt.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            image_mt.AddTrack(Image_Track.GetSegmentationTrack(sonogram));

            double thresholdFraction = 0.2;
            double[] hmmScores = ParseHmmScores(hmmResults, sonogram.Duration, sonogram.FrameCount, htkConfig.CallName,
                                                threshold, thresholdFraction);
            image_mt.AddTrack(Image_Track.GetScoreTrack(hmmScores, 1.0, 0.2));


            Console.WriteLine();
            string fName = Path.GetFileNameWithoutExtension(testWavFile);
            string opFile = htkConfig.ResultsDir + "\\" + fName + ".png";
            Console.WriteLine("Sonogram will be written to file: " + opFile);
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
        /// <param name="threshold">the HTK score threshold</param>
        /// <param name="thresholdFraction">threshold fraction between 0.0-1.0. Helps with display of score track.</param>
        /// <returns></returns>
        public static double[] ParseHmmScores(List<string> results, TimeSpan duration, int frameCount, string targetClass,
                                              float threshold, double thresholdFraction)
        {
            double offset = (thresholdFraction * threshold) / (1 - thresholdFraction);
            //Console.WriteLine("offset=" + offset);
            double min = threshold + offset;
            //Console.WriteLine("min=" + min);

            //Console.WriteLine("duration.TotalSeconds=" + duration.TotalSeconds);
            double[] scores = new double[frameCount];
            int hitCount = results.Count;
            for (int i = 1; i < hitCount; i++)
            {
                //Console.WriteLine(i+ "  " + results[i]);
                if ((results[i] == "") || (results[i].StartsWith("."))) continue;
                if ((results[i].StartsWith("\"")) || (results[i].StartsWith("#"))) continue;
                string[] words = results[i].Split(' ');
                long start = long.Parse(words[0]);
                double startSec = start / (double)10000000;  //start in seconds
                long end = long.Parse(words[1]);
                double endSec = end / (double)10000000;  //start in seconds
                string className = words[2];
                double score = Double.Parse(words[3]);

                score = (score - min) / Math.Abs(min); //normalise score between 0 - 1.
                if (score < min) score = min;

                int startFrame = (int)((startSec / (double)duration.TotalSeconds) * frameCount);
                int endFrame = (int)((endSec / (double)duration.TotalSeconds) * frameCount);
                //Console.WriteLine("startSec=" + startSec + "    endSec=" + endSec + "  startFrame=" + startFrame + "    endFrame=" + endFrame);
                if (className.StartsWith(targetClass))
                    for (int s = startFrame; s <= endFrame; s++)
                    {
                        scores[s] = score;
                    }
            }
            return scores;
        }



    }
}
