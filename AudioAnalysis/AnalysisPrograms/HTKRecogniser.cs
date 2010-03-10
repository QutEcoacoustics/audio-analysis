using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using TowseyLib;
using AudioAnalysisTools;
using AudioAnalysisTools.HTKTools;


namespace AnalysisPrograms
{
    public class HTKRecogniser
    {
        //COMMAND LINES
//htk C:\SensorNetworks\WavFiles\TestWaveFile\St_Bees_Currawong_20080919-060000_13.wav C:\SensorNetworks\Templates\Template_CURRAWONG1\CURRAWONG1.zip C:\SensorNetworks\temp



        //FORCE THRESHOLDS FOR Experimentation --- set SCORE_THRESHOLD = Double.NaN  if do not want to override default values
        const double SCORE_THRESHOLD   = -50.0;  //Double.NaN
        const double QUALITY_THRESHOLD = 2.56;   //1.96 for p=0.05;  2.56 for p=0.01

        private static bool verbose = true;
        public static bool Verbose { get { return verbose; } set { verbose = value; } }
        
        
        
        /// <summary>
        /// Runs a prepared HTK template over a file
        /// </summary>
        /// <param name="args"></param>
        public static void Dev(string[] args)
        {
            //THE WAV FILE TO PROCESS
            //string wavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\West_Knoll_St_Bees_Currawong3_20080919-060000.wav";       //ARG 0
            //string wavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\Top_Knoll_St_Bees_Curlew2_20080922-030000.wav";           //ARG 0
            //string wavFile = "C:\\SensorNetworks\\WavFiles\\StBees\\WestKnoll_StBees_KoalaBellow20080919-073000.wav";//contains currawong
            //string wavFile = @"C:\SensorNetworks\WavFiles\BridgeCreek\\cabin_GoldenWhistler_file0127_extract1.wav";
            //string wavFile = @"C:\SensorNetworks\WavFiles\Koala_Female\HoneymoonBay_StBees_20081027-023000.wav";
            //string wavFile = @"C:\SensorNetworks\WavFiles\Koala_Female\WestKnoll_StBees_20081216-213000.wav";
            //string wavFile = @"C:\SensorNetworks\WavFiles\Koala_Female\WestKnoll_StBees3_20090907-053000.wav";//mix of curlew,currawong, koalas
            
            //THE TEMPLATE
            //string dir  = "C:\\SensorNetworks\\Templates\\Template_";
            //string templateName = "CURRAWONG1";
            //string templateName = "CURLEW1";
            //string templateName = "WHIPBIRD1";
            //string templateName = "CURRAWONG1";
            //string templateName = "KOALAFEMALE1";
            //string templateName = "KOALAFEMALE2";
            //string templateName = "KOALAMALE1";
            //*******************************************************************************************************************
            //string templateDir = dir + templateName;
            //string templateFN  = templateDir + "\\" + templateName + ".zip";                           // ARG 1
            //*******************************************************************************************************************

            //string workingDirectory = "C:\\SensorNetworks\\temp"; //set default working directory                    // ARG 2  
            //*******************************************************************************************************************

            Log.Verbosity = 1;

            string title = "# DETECTING LOW FREQUENCY AMPLITUDE OSCILLATIONS";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            //GET THE COMMAND LINE ARGUMENTS
            CheckArguments(args);


            string recordingPath      = args[0];
            string templateFN         = args[1];
            string workingDirectory   = args[2];
            
            string templateDir = Path.GetDirectoryName(templateFN);
            Log.WriteLine("# Recording file: "    + Path.GetFileName(recordingPath));
            Log.WriteLine("# Template Dir: "      + templateDir);
            Log.WriteLine("# Working Directory =" + workingDirectory);
            //FileTools.WriteTextFile(opPath, date + "\n# Recording file: " + Path.GetFileName(recordingPath));




            //##############################################################################################################################
            //#### A: GET LIST OF HTK RECOGNISED EVENTS.
            List<AcousticEvent> events = HTKRecogniser.Execute(recordingPath, templateFN, workingDirectory);
            Log.WriteLine("# Finished scan with HTK.");

            //##############################################################################################################################

            //#### B: GET LIST OF TAGGED OR LABELLED EVENTS.
            string labelsPath = @"C:\SensorNetworks\WavFiles\Koala_Male\SmallTestSet\KoalaTestData.txt";
            string labelsText;
            string filename = Path.GetFileNameWithoutExtension(recordingPath);
            List<AcousticEvent> labels = AcousticEvent.GetAcousticEventsFromLabelsFile(labelsPath, filename, out labelsText);

            //#### C: DISPLAY EVENTS IN SONOGRAM
            Log.WriteLine(" Extracted " + events.Count + " events.   Preparing sonogram to display events");
            DisplayAcousticEvents(recordingPath, events, templateDir, workingDirectory);

            //#### D: WRITE EVENTS TO A TEXT FILE
            List<string> list = ExtractEventData(events);
            string opFile = workingDirectory + "\\results\\eventData.txt";
            FileTools.WriteTextFile(opFile, list, true);

            //#### E: CALCULATE PREDICTIOn ACCURACY
            int tp, fp, fn;
            string resultsText;
            double precision, recall, accuracy;
            //List<AcousticEvent> results = HTKScanRecording.GetAcousticEventsFromResultsFile(opFile);
//            AcousticEvent.CalculateAccuracy(results, labels, out tp, out fp, out fn, out precision, out recall, out accuracy,
//                                              out resultsText);
            AcousticEvent.CalculateAccuracy(events, labels, out tp, out fp, out fn, out precision, out recall, out accuracy,
                                              out resultsText);

            Console.WriteLine("\n\ntp={0}\tfp={1}\tfn={2}", tp, fp, fn);
            Console.WriteLine("Recall={0:f2}  Precision={1:f2}  Accuracy={2:f2}", recall, precision, accuracy);

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }// end Main()






        public static List<AcousticEvent> Execute(string wavFile, string templatePath, string workingDirectory)
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
            string resultsPath = HTKScanRecording.Execute(wavFile, workingDirectory, templatePath);

            //C: PARSE THE RESULTS FILE TO RETURN ACOUSTIC EVENTS
            Log.WriteLine("Parse the HMM results file and return Acoustic Events");
            string templateDir = workingDirectory + "\\" + templateName; //template has been shifted
            List<AcousticEvent> events = HTKScanRecording.GetAcousticEventsFromHTKResults(resultsPath, templateDir);
            return events;
        } //end method Execute()







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


        public static void DisplayAcousticEvents(string wavFile, List<AcousticEvent> events, string target, string outputDir)
        {
            SonogramConfig sonoConfig = new SonogramConfig();
            AudioRecording ar = new AudioRecording(wavFile);
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, ar.GetWavReader());
            if (HTKRecogniser.Verbose == true) 
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
                double[] scores = AcousticEvent.ExtractScoreArrayFromEvents(events, sonogram.FrameCount, name);
                double thresholdFraction = 0.2; //for display purposes only. Fraction of the score track devoted to sub-threshold scores
                //double[] finalScores = NormaliseScores(scores, scoreThreshold, thresholdFraction);
                image_mt.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, thresholdFraction));
            }

            string resultsDir = outputDir + "\\results";
            string fName = Path.GetFileNameWithoutExtension(wavFile);
            string opFile = resultsDir + "\\" + fName + ".png";
            if (HTKRecogniser.Verbose == true) Console.WriteLine("\nSonogram will be written to file: " + opFile);
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
