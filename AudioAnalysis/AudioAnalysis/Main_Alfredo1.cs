using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;

namespace AudioAnalysis
{
    class Main_Alfredo1
    {



        public static void Main(string[] args)
        {
            Console.WriteLine("===============================================================================================");
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("");

            if(args.Length != 2)
            {
                Console.WriteLine("USAGE: sonogram1.exe wavFilePath resultsFilePath");
                Console.WriteLine("       Press <Enter> key to exit .... ");
                Console.ReadLine();
                System.Environment.Exit(99);
                //throw new Exception() { };
            }

            string wavPath     = args[0];
            string resultsPath = args[1];
            Console.WriteLine("");


           // wavPath = @"C:\SensorNetworks\Templates\Template_8\Alfredo_results\TestMichael.wav";
           // resultsPath = @"C:\SensorNetworks\Templates\Template_8\Alfredo_results\results_TestMichael.mlf";
           // wavPath     = @"C:\SensorNetworks\Templates\Template_8\Alfredo_results\Honeymoon_Bay_St_Bees_KoalaBellow_20080905-001000.wav";
           // resultsPath = @"C:\SensorNetworks\Templates\Template_8\Alfredo_results\Honeymoon_Bay_St_Bees_KoalaBellow_20080905-001000.mlf";

            Console.WriteLine("Wav file:     " + wavPath);
            Console.WriteLine("results file: " + resultsPath);

            string opDir = Path.GetDirectoryName(wavPath);
            string fName = Path.GetFileNameWithoutExtension(wavPath);
            string opFile = opDir + "\\" + fName + ".png";


            //Configuration config = new Configuration();
            BaseSonogramConfig sonoConfig = new BaseSonogramConfig();

            AudioRecording ar = new AudioRecording(wavPath);
            Console.WriteLine();
            //Console.WriteLine("Wav sample rate=" + ar.SamplingRate);
            
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, ar.GetWavReader());
            bool doHighlightSubband = false; bool add1kHzLines = true;
            var image_mt = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image_mt.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            image_mt.AddTrack(Image_Track.GetSegmentationTrack(sonogram));


            Console.WriteLine(); 
            Console.WriteLine("Parsing the HMM results file");
            Console.WriteLine(" There must be ONE AND ONLY ONE header line.");
            Console.WriteLine(" Duration=" + sonogram.Duration.TotalSeconds + " s.      Frame count=" + sonogram.FrameCount);
            List<string> hmmResults = FileTools.ReadTextFile(resultsPath);
            double[] hmmScores = ParseHmmScores(hmmResults, sonogram.Duration, sonogram.FrameCount, "CURRAWONG");
            image_mt.AddTrack(Image_Track.GetScoreTrack(hmmScores, 8.0, 1.0));


            Console.WriteLine();
            Console.WriteLine("Sonogram will be written to file: " + opFile); 
            image_mt.Save(opFile);
            //template.SaveResultsImage(recording.GetWavData(), imagePath, result, hmmResults); //WITH HMM SCORE





            Console.WriteLine();
            Console.WriteLine("Press <Enter> key to exit .... ");
            Console.ReadLine();
        }// end Main method()


        public static double[] ParseHmmScores(List<string> results, TimeSpan duration, int frameCount, string targetClass)
        {
            //Console.WriteLine("duration.TotalSeconds=" + duration.TotalSeconds);
            double[] scores = new double[frameCount];
            int hitCount = results.Count;
            for (int i = 1; i < hitCount; i++)
            {
                //Console.WriteLine(i+ "  " + results[i]);
                if ((results[i]=="")||(results[i].StartsWith("."))) continue;
                if ((results[i].StartsWith("\"")) || (results[i].StartsWith("#"))) continue;
                string[] words = results[i].Split(' ');
                long start = long.Parse(words[0]);
                double startSec = start / (double)10000000;  //start in seconds
                long end = long.Parse(words[1]);
                double endSec = end / (double)10000000;  //start in seconds
                string className = words[2];
                double score = Double.Parse(words[3]);
                int startFrame = (int)((startSec / (double)duration.TotalSeconds) * frameCount);
                int endFrame = (int)((endSec / (double)duration.TotalSeconds) * frameCount);
                //Console.WriteLine("startSec=" + startSec + "    endSec=" + endSec + "  startFrame=" + startFrame + "    endFrame=" + endFrame);
                if (className.StartsWith(targetClass))
                    for (int s = startFrame; s <= endFrame; s++)
                    {
                        scores[s] = 5.0;
                    }
            }
            return scores;
        }



    }
}
