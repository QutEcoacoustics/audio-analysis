using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;
using QutSensors.Shared;


//HERE ARE THE FOUR (4) COMMAND LINE ARGUMENTS TO PLACE IN START OPTIONS - PROPERTIES PAGE,  debug command line
//USED TO CREATE A MFCC-OD TEMPLATE for LEWIN's RAIL
// ID, recording, template.zip, working directory.
//createtemplate_mfccod C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-075040.wav C:\SensorNetworks\Templates\Template_KEKKEK1\Template_KEKKEK1.txt  C:\SensorNetworks\Output\MFCC_LewinsRail\
//
namespace AnalysisPrograms
{
    class Create_MFCC_OD_Template
    {
        //Keys to recognise identifiers in PARAMETERS - INI file. 
        public static string key_TEMPLATE_ID = "TEMPLATE_ID";
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
            Log.Verbosity = 1;
            //#######################################################################################################

            CheckArguments(args);
            string testRecordingPath = args[0];
            string templatePath      = args[1];
            string testDirectory     = args[2]; //used for testing completed template.

            string templateDir = Path.GetDirectoryName(templatePath);
            string templateFN  = Path.GetFileNameWithoutExtension(templatePath);

            //A: INITIALISE CONFIG
            Log.WriteLine("# STEP A: READ CONFIG");
            Log.WriteLine("# Template Dir   =" + templateDir);
            var config = new Configuration(templatePath);
            Dictionary<string, string> dict = config.GetTable();
            //Dictionary<string, string>.KeyCollection keys = dict.Keys;
            string templateID = dict[key_TEMPLATE_ID];
            Log.WriteLine("# CallID         =" + templateID);

            //B: GET TRAINING DATA - i.e. List of Vocalisation Recordings - either paths or URIs
            Log.WriteLine("# STEP B: COLLECT TRAINING FILES");
            string ext = dict[key_FILE_EXT];
            string trainingDir = dict[key_TRAIN_DIR];
            FileInfo[] recordingFiles = FileTools.GetFilesInDirectory(trainingDir, ext);

            //C: CREATE resources according to parameters in ini file.
            Log.WriteLine("# STEP C: CREATE RESOURCES");
            var tuple = FVExtractor.ExtractSingleFV(recordingFiles, dict);
            double[] fv    = tuple.Item1;
            double[] modalNoise_Fullband = tuple.Item2;
            double[] modalNoise_Subband  = tuple.Item3;

            //D: SAVE THE RESOURCES AND ZIP
            Log.WriteLine("# STEP D: SAVE THE RESOURCES");
            string fvPath = templateDir + "\\FV1_" + templateID + ".txt";
            FileTools.WriteArray2File_Formatted(fv, fvPath, "f8");
            string noisePath = templateDir + "\\modalNoiseFullband_" + templateID + ".txt";
            FileTools.WriteArray2File_Formatted(modalNoise_Fullband, noisePath, "f8");
            noisePath = templateDir + "\\modalNoiseSubband_" + templateID + ".txt";
            FileTools.WriteArray2File_Formatted(modalNoise_Subband, noisePath, "f8");
            Log.WriteLine("# STEP E: ZIP THE RESOURCES");
            DirectoryInfo parent = Directory.GetParent(templateDir); //place zipped file in parent directory
            string zipPath = parent.FullName + "\\" + templateID + ".zip";
            ZipUnzip.ZipDirectory(templateDir, zipPath);

            //E: VERIFY THE TEMPLATE
            string recordingFN = Path.GetFileName(testRecordingPath);
            Log.WriteLine("# STEP F: VERIFY TEMPLATE: " + templatePath);
            Log.WriteLine("# SCAN TEST RECORDING:     " + recordingFN);
            Log.WriteLine("# WORKING DIRECTORY:       " + testDirectory);
            string[] arguments = new string[3];
            arguments[0] = testRecordingPath;
            arguments[1] = zipPath;
            arguments[2] = testDirectory; //working directory for verification.
            MFCC_OD_KekKek.Dev(arguments);

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
