using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using TowseyLib;
using AudioAnalysisTools;
using QutSensors.Shared;


//HERE ARE THE FOUR (4) COMMAND LINE ARGUMENTS TO PLACE IN START OPTIONS - PROPERTIES PAGE,  debug command line
//USED TO CREATE A MFCC-OD TEMPLATE for LEWIN's RAIL
// ID, recording, template.zip, working directory.
//createtemplate_mfccod C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-075040.wav C:\SensorNetworks\Templates\Template_KEKKEK1\Template_KEKKEK1.txt  C:\SensorNetworks\Output\LewinsRail\
//
namespace AnalysisPrograms
{
    class Create_MFCC_OD_Template
    {
        //Keys to recognise identifiers in PARAMETERS - INI file. 
        public static string key_FILE_EXT  = "FILE_EXT";
        public static string key_TRAIN_DIR = "TRAIN_DIR";
        public static string key_MIN_HZ = "MIN_FREQ";
        public static string key_MAX_HZ = "MAX_FREQ";
        public static string key_FRAME_OVERLAP = "FRAME_OVERLAP";
        public static string key_DO_MELSCALE = "DO_MELSCALE";
        public static string key_CC_COUNT = "CC_COUNT";
        public static string key_DCT_DURATION = "DCT_DURATION";
        public static string key_MIN_OSCIL_FREQ = "MIN_OSCIL_FREQ";
        public static string key_MAX_OSCIL_FREQ = "MAX_OSCIL_FREQ";
        public static string key_MIN_AMPLITUDE = "MIN_AMPLITUDE";
        public static string key_MIN_DURATION = "MIN_DURATION";
        public static string key_MAX_DURATION = "MAX_DURATION";
        public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_DRAW_SONOGRAMS = "DRAW_SONOGRAMS";



        public static void Dev(string[] args)
        {
            string title = "# DETECTING LEWIN's RAIL Kek-Kek USING MFCCs and OD";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);
            Log.WriteLine("");

            //#######################################################################################################
            // KEY PARAMETERS TO CHANGE
            string templateID = "KEKKEK1";
            Log.Verbosity = 1;
            //#######################################################################################################

            CheckArguments(args);
            string testRecordingPath = args[0];
            string templatePath      = args[1];
            string workingDirectory  = args[2];


            string recordingFN = Path.GetFileName(testRecordingPath);
            string templateDir = Path.GetDirectoryName(templatePath);
            string templateFN  = Path.GetFileNameWithoutExtension(templatePath);
            string outputDir   = workingDirectory;
            string opFName     = "events.txt"; ;
            string opPath      = outputDir + opFName;

            Log.WriteLine("# CallID         =" + templateID);
            Log.WriteLine("# Test Recording =" + recordingFN);
            Log.WriteLine("# Template Dir   =" + templateDir);
            Log.WriteLine("# Working Dir    =" + workingDirectory);

            //A: SET UP DIRECTORY STRUCTURE
            //create the working directory if it does not exist
            if (!Directory.Exists(workingDirectory)) Directory.CreateDirectory(workingDirectory);
            string newTemplateDir = workingDirectory + templateFN;
            Log.WriteLine("# OP Template Dir=" + newTemplateDir);

            //B: INITIALISE CONFIG and CREATE DIRECTORY STRUCTURE
            Log.WriteLine("# Init CONFIG and creating directory structure");
            string templateFName = "Template_" + templateID + ".txt";
            string iniPath = templateDir + "\\" + templateFName;
            //string fvPath  = templateDir + "\\FV1_" + templateID + ".txt"; //feature vector path
            //double[] fv = FileTools.ReadDoubles2Vector(fvPath);

            //C: SET UP CONFIGURATION
            var config = new Configuration(iniPath);
            config.SetPair(ConfigKeys.Template.Key_TemplateDir, templateDir);
            //config.SetPair(ConfigKeys.Recording.Key_RecordingDirName, recordingFiles[0].DirectoryName);//assume all files in same dir
            config.SetPair("MODE", Mode.CREATE_NEW_TEMPLATE.ToString());
            Dictionary<string, string> dict = config.GetTable();
            Dictionary<string, string>.KeyCollection keys = dict.Keys;

           // FileTools.WriteTextFile(opPath, date + "\n# Scanning recording for Lewin's Rail Kek Kek\n# Recording file: " + recordingFN);
           // string appConfigPath = @"C:\SensorNetworks\Templates\sonogram.ini";

            //D: GET TRAINING DATA - i.e. List of Vocalisation Recordings - either paths or URIs
            string ext = dict[key_FILE_EXT];
            string trainingDir = dict[key_TRAIN_DIR];
            FileInfo[] recordingFiles = FileTools.GetFilesInDirectory(trainingDir, ext);

            //E: CREATE resources according to parameters in ini file.
            Log.WriteLine("# RESOURCE 1: Extract Feature Vector from recordings");
            var tuple = FVExtractor.ExtractSingleFV(recordingFiles, dict);
            double[] fv    = tuple.Item1;
            double[] noise = tuple.Item2;
 
            //STEP THREE: Extract template
            //Log.WriteIfVerbose("\nSTEP THREE: Create language model");
            //template.CreateLanguageModel(config);
            //STEP FOUR: Save template
            Log.WriteIfVerbose("\nSTEP FOUR: Save template");
            string opTemplatePath = templateDir + "\\"+ templateFName;
            string opDir = Path.GetDirectoryName(opTemplatePath);
            if (!Directory.Exists(opDir)) Directory.CreateDirectory(opDir);

            if (File.Exists(opTemplatePath)) File.Copy(opTemplatePath, opTemplatePath + "OLD.txt", true); //overwrite

//            Save(new StreamWriter(opTemplatePath), opDir);
//            template.SonogramConfig.NoiseReductionType = ConfigKeys.NoiseReductionType.STANDARD;    //reset noise reduction type for normal use
//            template.mode = Mode.READ_EXISTING_TEMPLATE;  //reset mode for normal use

            //B: CREATE ZIPPED VERSION OF TEMPLATE
            Log.WriteLine("# STEP B: CREATE ZIPPED RESOURCES");
            ZipUnzip.ZipDirectory(newTemplateDir, templatePath);

            //C: READ IN SERIALISED TEMPLATE
            Log.WriteLine("# STEP C: READ ZIPPED TEMPLATE" + templatePath);

            //D: LOAD TEMPLATE INTO RECOGNISER
            Log.WriteLine("STEP D: VERIFY TEMPLATE: LOAD IT INTO RECOGNISER");
//            var recogniser = new Recogniser(template2 as Template_CCAuto); //GET THE TYPE

            //E: VERIFY TEMPLATE: SCAN SINGLE RECORDING and SAVE RESULTS IMAGE
            Log.WriteLine("# STEP E: VERIFY TEMPLATE: SCAN SINGLE RECORDING " + templatePath);
            Log.WriteLine("# Recording:     " + recordingFN);

            string wavDirName; string wavFileName;
            WavChooser.ChooseWavFile(out wavDirName, out wavFileName); //WARNING! CHOOSE WAV FILE
            string wavPath = wavDirName + wavFileName + ".wav";        //set the .wav file in method ChooseWavFile()
            AudioRecording recording = new AudioRecording(wavPath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();

//            var result = recogniser.Analyse(recording);


  //          int samplingRate = template2.SonogramConfig.FftConfig.SampleRate;
    //        int windowSize = template2.SonogramConfig.WindowSize;
      //      int windowOffset = (int)Math.Floor(windowSize * template2.SonogramConfig.WindowOverlap);
        //    bool doMelScale = template2.SonogramConfig.DoMelScale;
          //  int minF = (int)template2.SonogramConfig.MinFreqBand;
            //int maxF = (int)template2.SonogramConfig.MaxFreqBand;

//            var events = result.GetAcousticEvents(samplingRate, windowSize, windowOffset, doMelScale, minF, maxF);
//            string imagePath = Path.Combine(templateDir, "RESULTS_" + Path.GetFileNameWithoutExtension(recording.FileName) + ".png");
//            template2.SaveSyllablesAndResultsImage(recording.GetWavReader(), imagePath, result, events);
//            int count = 0;
//            foreach (AcousticEvent e in events)
            //{
            //    count++;
            //    string key = result.RankingScoreName;
            //    ResultProperty item = result.GetEventProperty(key, e);
            //    Console.WriteLine("Hit Event (" + count + ")  score=" + item.Value.ToString());
            //}

            //F: TEST TEMPLATE ON MULTIPLE VOCALISATIONS
            var testDirectories = new List<String>();
            testDirectories.Add(@"C:\SensorNetworks\Templates\Template_3\TestSetTrue");
            testDirectories.Add(@"C:\SensorNetworks\Templates\Template_3\TestSetFalse");
        //    Main_TestSerialTemplateOnCallFiles.ScanTestDirectories(template2, testDirectories);


            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
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
            Console.WriteLine("Create_MFCC_OD_Template.exe recordingPath iniPath outputFileName");
            Console.WriteLine("where:");
            Console.WriteLine("recordingFileName:-(string) The path to audio file used to test template.");
            Console.WriteLine("iniPath:-          (string) The path to ini file containing parameters used to create template.");
            Console.WriteLine("workingDir:-       (string) Dir for placing resources.");
            Console.WriteLine("                            By default, the workingDir dir is that containing the ini file.");
            Console.WriteLine("");
            Console.WriteLine("\nPress <ENTER> key to exit.");
            Console.ReadLine();
            System.Environment.Exit(1);
        }



    }
}
