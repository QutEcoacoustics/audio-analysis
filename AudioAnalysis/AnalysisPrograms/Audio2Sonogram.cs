using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Acoustics.Tools;
using Acoustics.Tools.Audio;
using Acoustics.Shared;
using AnalysisBase;
using AnalysisRunner;
using AudioAnalysisTools;
using TowseyLib;


namespace AnalysisPrograms
{
    class Audio2Sonogram
    {
        //use the following paths for the command line for the <Audio2Sonogram> task. 

        // audio2sonogram "C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Sonogram.cfg"  C:\SensorNetworks\Output\Sonograms\BAC1_20071008-081607.png 0   0  true

        //public const int DEFAULT_SAMPLE_RATE = 22050;
        
        public static int Main(string[] args)
        {
            int status = 0;

            if (CheckArguments(args) != 0) //checks validity of the first 3 path arguments
            {
                Console.WriteLine("\nPress <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }

            string recordingPath = args[0];
            string configPath    = args[1];
            string outputPath    = args[2];

            TimeSpan startOffsetMins = TimeSpan.Zero;
            TimeSpan endOffsetMins   = TimeSpan.Zero;

            if (args.Length >= 5)
            {
                startOffsetMins = TimeSpan.FromMinutes(double.Parse(args[3]));
                endOffsetMins   = TimeSpan.FromMinutes(double.Parse(args[4]));
            }

            bool verbose = false;
            if (args.Length == 6)
            {
                verbose = bool.Parse(args[5]);
                if (verbose)
                {
                    string title = "# MAKE A SONOGRAM FROM AUDIO RECORDING";
                    string date  = "# DATE AND TIME: " + DateTime.Now;
                    Console.WriteLine(title);
                    Console.WriteLine(date);
                    Console.WriteLine("# Input  audio file: " + Path.GetFileName(recordingPath));
                    Console.WriteLine("# Output image file: " + outputPath);
                }
            }
            

            //1. set up the necessary files
            DirectoryInfo diSource = new DirectoryInfo(Path.GetDirectoryName(recordingPath));
            FileInfo fiSourceRecording = new FileInfo(recordingPath);
            FileInfo fiConfig = new FileInfo(configPath);
            FileInfo fiImage  = new FileInfo(outputPath);

            //2. get the config dictionary
            var configuration = new ConfigDictionary(fiConfig.FullName);
            Dictionary<string, string> configDict = configuration.GetTable();

            if (verbose)
            {
                Console.WriteLine("\nPARAMETERS");
                foreach (KeyValuePair<string, string> kvp in configDict)
                {
                    Console.WriteLine("{0}  =  {1}", kvp.Key, kvp.Value);
                }
            }

            //3: GET RECORDING
            FileInfo fiOutputSegment = fiSourceRecording;
            if (!((startOffsetMins == TimeSpan.Zero) && (endOffsetMins == TimeSpan.Zero)))
            {
                TimeSpan buffer = new TimeSpan(0, 0, 0);
                fiOutputSegment = new FileInfo(Path.Combine(Path.GetDirectoryName(outputPath), "tempWavFile.wav"));
                AudioRecording.ExtractSegment(fiSourceRecording, startOffsetMins, endOffsetMins, buffer, configDict, fiOutputSegment);
            }

            //###### get sonogram image ##############################################################################################
            if ((configDict.ContainsKey(Keys.MAKE_SOX_SONOGRAM)) && (ConfigDictionary.GetBoolean(Keys.MAKE_SOX_SONOGRAM, configDict)))
            {
                status = SonogramTools.MakeSonogramWithSox(fiOutputSegment, configDict, fiImage);
            }
            else
            {
                using (Image image = SonogramTools.Audio2SonogramImage(fiOutputSegment, configDict))
                {
                    if (image == null) return 1;
                    if (fiImage.Exists) fiImage.Delete();
                    image.Save(fiImage.FullName, ImageFormat.Png);
                }
            }
            //###### get sonogram image ##############################################################################################

            if (verbose)
            {
                Console.WriteLine("\n##### FINISHED FILE ###################################################\n");
                Console.ReadLine();
            }

            return status;
        } //Main(string[] args)



        public static int CheckArguments(string[] args)
        {
            if ((args.Length != 5) && (args.Length != 6))
            {
                Console.WriteLine("\nINCORRECT COMMAND LINE.");
                Console.WriteLine("\nTHE COMMAND LINE HAS {0} ARGUMENTS", args.Length);
                foreach (string arg in args) Console.WriteLine(arg + "  ");
                Console.WriteLine("\nYOU REQUIRE 5 OR 6 COMMAND LINE ARGUMENTS\n");
                Usage();
                return 666;
            }
            if (CheckPaths(args) != 0) return 999;
            return 0;
        }

        /// <summary>
        /// this method checks validity of first three command line arguments.
        /// </summary>
        /// <param name="args"></param>
        public static int CheckPaths(string[] args)
        {
            int status = 0;
            //GET FIRST THREE OBLIGATORY COMMAND LINE ARGUMENTS
            string recordingPath = args[0];
            string configPath = args[1];
            string outputPath = args[2];
            DirectoryInfo diSource = new DirectoryInfo(Path.GetDirectoryName(recordingPath));
            if (!diSource.Exists)
            {
                Console.WriteLine("Source directory does not exist: " + diSource.FullName);
                status = 2;
                return status;
            }
            FileInfo fiSource = new FileInfo(recordingPath);
            if (!fiSource.Exists)
            {
                Console.WriteLine("Source directory exists: " + diSource.FullName);
                Console.WriteLine("\t but the source file does not exist: " + recordingPath);
                status = 2;
                return status;
            }
            FileInfo fiConfig = new FileInfo(configPath);
            if (!fiConfig.Exists)
            {
                Console.WriteLine("Config file does not exist: " + fiConfig.FullName);
                status = 2;
                return status;
            }
            
            DirectoryInfo diOP = new DirectoryInfo(Path.GetDirectoryName(outputPath));
            if (!diOP.Exists)
            {
                bool success = true;
                
                try
                {
                    Directory.CreateDirectory(diOP.FullName);
                    success = Directory.Exists(diOP.FullName);
                }
                catch
                {
                    success = false;
                }

                if (!success)
                {
                    Console.WriteLine("Output directory does not exist: " + diOP.FullName);
                    status = 2;
                    return status;
                }
            }
            return status;
        }


        public static void Usage()
        {
            Console.WriteLine("USAGE:");
            Console.WriteLine("AnalysisPrograms.exe  audio2sonogram  audioPath  configPath  outputDirectory  startOffset  endOffset");
            Console.WriteLine("where:");
            Console.WriteLine("audio2sonogram:- (string) a short string that selects the analysis/process to be performed.");
            Console.WriteLine("input  audio  File:- (string) Path of the audio file to be processed.");
            Console.WriteLine("configuration File:- (string) Path of the analysis configuration file.");
            Console.WriteLine("output   Directory:- (string) Path of the output directory in which to store .csv result files.");
            Console.WriteLine("startOffset: (integer) The start (minutes) of that portion of the file to be analysed.");
            Console.WriteLine("endOffset:   (integer) The end   (minutes) of that portion of the file to be analysed.");
            Console.WriteLine("In order to analyse the entire file, set start and end times both equal to zero.");
            Console.WriteLine("The following argument is OPTIONAL.");
            Console.WriteLine("verbosity:   (boolean) true/false");
            Console.WriteLine("");
        }

    } //class Audio2Sonogram
}


