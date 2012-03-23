using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using TowseyLib;
using AudioAnalysisTools;
using AudioAnalysisTools.HTKTools;

namespace AnalysisPrograms
{
    using Acoustics.Tools.Audio;

    //INFORMATION FOR MARK
    //The CURRAWONG template under version control is located on my machine at:
    //C:\SensorNetworks\Software\AudioAnalysis\RecogniserTemplates\Template_CURRAWONG1\CURRAWONG1.zip
    //The CURLEW template under version control is located on my machine at:
    //C:\SensorNetworks\Software\AudioAnalysis\RecogniserTemplates\Template_CURLEW1\CURLEW1.zip
    //The KOALAFEMALE template under version control is located on my machine at:
    //C:\SensorNetworks\Software\AudioAnalysis\RecogniserTemplates\Template_KOALAFEMALE1



    public class HTKRecogniser
    {
        //COMMAND LINES
        //for CURRAWONG
        //htk C:\SensorNetworks\WavFiles\StBees\West_Knoll_St_Bees_Currawong3_20080919-060000.wav C:\SensorNetworks\Templates\Template_CURRAWONG2\CURRAWONG2.zip C:\SensorNetworks\temp
        //htk C:\SensorNetworks\WavFiles\StBees\West_Knoll_St_Bees_Currawong1_20080923-120000.wav C:\SensorNetworks\Templates\Template_CURRAWONG1\CURRAWONG1.zip C:\SensorNetworks\temp
        //for CURLEW
        //htk C:\SensorNetworks\WavFiles\StBees\Top_Knoll_St_Bees_Curlew1_20080922-023000.wav     C:\SensorNetworks\Templates\Template_CURLEW1\CURLEW1.zip C:\SensorNetworks\temp

        //INFO FOR THREE COMMAND LINE ARGUMENTS
        //THE WAV FILE TO PROCESS
        //string wavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\Top_Knoll_St_Bees_Curlew2_20080922-030000.wav";           //ARG 0
        //string wavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\WestKnoll_StBees_KoalaBellow20080919-073000.wav";//contains currawong
        //string wavFile = @"C:\SensorNetworks\WavFiles\BridgeCreek\\cabin_GoldenWhistler_file0127_extract1.wav";
        //string wavFile = @"C:\SensorNetworks\WavFiles\Koala_Female\HoneymoonBay_StBees_20081027-023000.wav";
        //string wavFile = @"C:\SensorNetworks\WavFiles\Koala_Female\WestKnoll_StBees_20081216-213000.wav";
        //string wavFile = @"C:\SensorNetworks\WavFiles\Koala_Female\WestKnoll_StBees3_20090907-053000.wav";//mix of curlew,currawong, koalas

        //THE TEMPLATE
        //string dir  = "C:\\SensorNetworks\\Templates\\Template_";
        //string templateName = "CURRAWONG1"  "CURLEW1"  "WHIPBIRD1"  "KOALAFEMALE1"  "KOALAMALE1";
        //string templateFN  = dir + templateName + "\\" + templateName + ".zip";                           // ARG 1
        //Example: C:\SensorNetworks\Templates\Template_CURRAWONG1\CURRAWONG1.zip
        //*******************************************************************************************************************
        //string workingDirectory = "C:\\SensorNetworks\\temp";                                            // ARG 2  
        //*******************************************************************************************************************



        /// <summary>
        /// Runs a prepared HTK template over a file
        /// </summary>
        /// <param name="args"></param>
        public static void Dev(string[] args)
        {

            Log.Verbosity = 1;

            string title = "# DETECTING LOW FREQUENCY AMPLITUDE OSCILLATIONS";
            string date = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            string recordingPath = args[0];
            string templateFN = args[1];
            string workingDirectory = args[2];

            Log.WriteLine("# Recording:     " + Path.GetFileName(recordingPath));
            Log.WriteLine("# Template File: " + templateFN);
            Log.WriteLine("# Working Dir:   " + workingDirectory);

            //create the working directory if it does not exist
            if (!Directory.Exists(workingDirectory)) Directory.CreateDirectory(workingDirectory);

            //GET THE COMMAND LINE ARGUMENTS
            CheckArguments(args);

            // check to see if conversion from .MP3 to .WAV is necessary
            string sourceDir = Path.GetDirectoryName(recordingPath);
            var destinationAudioFile = Path.Combine(sourceDir, Path.GetFileNameWithoutExtension(recordingPath) + ".wav");

            Log.WriteLine("Checking to see if conversion necessary...");
            MasterAudioUtility.ConvertToWav(null, new FileInfo(recordingPath), new FileInfo(destinationAudioFile));
            if (File.Exists(destinationAudioFile))
            {
                Log.WriteLine("Wav pcm file created.");
            }
            else
            {
                Log.WriteLine("Could not get wav pcm file.");
                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();
                return;
            }

            //##############################################################################################################################
            //#### A: GET LIST OF HTK RECOGNISED EVENTS.
            var op = HTKRecogniser.Execute(destinationAudioFile, templateFN, workingDirectory);
            HTKConfig config = op.Item1;
            List<AcousticEvent> events = op.Item2;
            Log.WriteLine("# Finished scan with HTK.");
            //##############################################################################################################################

            //#### B: DISPLAY EVENTS IN SONOGRAM
            Log.WriteLine(" Extracted " + events.Count + " events.   Preparing sonogram to display events");
            DisplayAcousticEvents(recordingPath, events, config.ConfigDir, config.ResultsDir);

            //#### C: WRITE EVENTS TO A TEXT FILE
            List<string> list = ExtractEventData(events);
            FileTools.WriteTextFile(config.ResultsDir + "\\eventData.txt", list, true);

            //#### D: GET LIST OF TAGGED OR LABELLED EVENTS.
            //string labelsPath = @"C:\SensorNetworks\WavFiles\StBees\CurrawongTestData.txt";
            string labelsPath = @"C:\SensorNetworks\WavFiles\StBees\CurlewTestData.txt";
            string labelsText;
            string filename = Path.GetFileNameWithoutExtension(recordingPath);
            List<AcousticEvent> labels = AcousticEvent.GetAcousticEventsFromLabelsFile(labelsPath, filename, out labelsText);

            //#### E: CALCULATE PREDICTION ACCURACY
            int tp, fp, fn;
            string resultsText;
            double precision, recall, accuracy;
            AcousticEvent.CalculateAccuracy(events, labels, out tp, out fp, out fn, out precision, out recall, out accuracy, out resultsText);
            Log.WriteLine(" >>>>>>>>>>>>>>> tp={0}\tfp={1}\tfn={2}", tp, fp, fn);
            Log.WriteLine(" >>>>>>>>>>>>>>> Recall={0:f2}  Precision={1:f2}  Accuracy={2:f2}", recall, precision, accuracy);

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }// end Main()



        public static System.Tuple<HTKConfig, List<AcousticEvent>> Execute(string audioFile, string templatePath, string workingDirectory)
        {
            string templateName = Path.GetFileNameWithoutExtension(templatePath);

            //A: SHIFT TEMPLATE TO WORKING DIRECTORY AND UNZIP IF NOT ALREADY DONE
            string newTemplateDir = workingDirectory + templateName;
            if (!Directory.Exists(newTemplateDir)) Directory.CreateDirectory(newTemplateDir);
            ZipUnzip.UnZip(newTemplateDir, templatePath, true);

            //C: INI CONFIG and CREATE DIRECTORY STRUCTURE
            Log.WriteLine("Init CONFIG and creating directory structure");
            string iniPath = workingDirectory + templateName + "\\segmentation.ini";
            HTKConfig htkConfig = new HTKConfig(iniPath);
            Log.WriteLine("\tCONFIG=" + newTemplateDir);
            Log.WriteLine("\tDATA  =" + htkConfig.DataDir);
            Log.WriteLine("\tRESULT=" + htkConfig.ResultsDir);

            //B: move the data/TEST file to its own directory
            if (Directory.Exists(htkConfig.DataDir)) Directory.Delete(htkConfig.DataDir, true); //delete data dir if it exists
            Directory.CreateDirectory(htkConfig.DataDir);
            string dataFN = Path.GetFileName(audioFile);

            var destinationAudioFile = Path.Combine(htkConfig.DataDir, Path.GetFileNameWithoutExtension(audioFile) + ".wav");
            File.Copy(audioFile, destinationAudioFile, true);

            //D: SCAN RECORDING WITH RECOGNISER AND RETURN A RESULTS FILE
            Log.WriteLine("Executing HTK_Recogniser - scanning recording: " + dataFN);
            string resultsPath = HTKScanRecording.Execute(dataFN, workingDirectory, htkConfig);

            //E: PARSE THE RESULTS FILE TO RETURN ACOUSTIC EVENTS
            Log.WriteLine("Parse the HMM results file and return Acoustic Events");
            List<AcousticEvent> events = HTKScanRecording.GetAcousticEventsFromHTKResults(resultsPath, newTemplateDir);
            return System.Tuple.Create(htkConfig, events);
        } //end method Execute()



        public static List<string> ExtractEventData(List<AcousticEvent> events)
        {
            var list = new List<string>();
            foreach (AcousticEvent ae in events)
            {
                var sb = new StringBuilder();
                double endtime = ae.StartTime + ae.Duration;
                sb.Append(ae.Name + "\t" + ae.StartTime.ToString("f4") + "\t" +
                          endtime.ToString("f4") + "\t" + ae.Score.ToString("f4") + "\t" + ae.SourceFile);
                list.Add(sb.ToString());
            }
            return list;
        }


        public static void DisplayAcousticEvents(string wavFile, List<AcousticEvent> events, string templateDir, string outputDir)
        {
            SonogramConfig sonoConfig = new SonogramConfig();
            AudioRecording ar = new AudioRecording(wavFile);
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, ar.GetWavReader());
            Log.WriteLine(" Duration=" + sonogram.Duration.TotalSeconds + " s.      Frame count=" + sonogram.FrameCount);
            bool doHighlightSubband = false; bool add1kHzLines = true;
            var image_mt = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image_mt.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image_mt.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image_mt.AddEvents(events);

            //D: PARSE THE RESULTS FILE TO GET SCORE ARRAY
            string syllableFile = templateDir + "\\" + HTKConfig.labelListFN;
            List<string> sylNames = HTKConfig.GetSyllableNames(syllableFile);
            foreach (string name in sylNames)
            {
                double[] scores = AcousticEvent.ExtractScoreArrayFromEvents(events, sonogram.FrameCount, name);
                double thresholdFraction = 0.2; //for display purposes only. Fraction of the score track devoted to sub-threshold scores
                //double[] finalScores = NormaliseScores(scores, scoreThreshold, thresholdFraction);
                image_mt.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, thresholdFraction));
            }

            string fName = Path.GetFileNameWithoutExtension(wavFile);
            string opFile = outputDir + "\\" + fName + ".png";
            Log.WriteLine("Sonogram will be written to file: " + opFile);
            image_mt.Save(opFile);
        }



        private static void CheckArguments(string[] args)
        {
            if (args.Length != 3)
            {
                Log.WriteLine("INCORRECT COMMAND LINE.");
                Log.WriteLine("NUMBER OF COMMAND LINE ARGUMENTS = {0}", args.Length);
                foreach (string arg in args) Log.WriteLine(arg + "  ");
                Log.WriteLine("YOU REQUIRE EXACTLY {0} COMMAND LINE ARGUMENTS\n", 3);
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
                Console.WriteLine("Cannot find template file: <" + args[1] + ">");
                Usage();
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
            if (!Directory.Exists(args[2]))
            {
                Console.WriteLine("Cannot find directory: <" + args[2] + ">");
                Usage();
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
        }


        private static void Usage()
        {
            Console.WriteLine("USAGE:");
            Console.WriteLine("HTKRecogniser.exe RecordingPath TemplatePath WorkingDir");
            Console.WriteLine("where:");
            Console.WriteLine("RecordingPath:-(string) The path of the audio file to be processed.");
            Console.WriteLine("TemplatePath:- (string) The path of the template file containing all required HTK resources.");
            Console.WriteLine("WorkingDir:-   (string) The name of the working or output file.");
            Console.WriteLine("");
            Console.WriteLine("\nPress <ENTER> key to exit.");
            Console.ReadLine();
            System.Environment.Exit(1);
        }



    }//end class
}
