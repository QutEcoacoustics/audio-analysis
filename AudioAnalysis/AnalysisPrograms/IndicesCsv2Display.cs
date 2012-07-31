using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

using AnalysisBase;
using AudioAnalysisTools;

using TowseyLib;

namespace AnalysisPrograms
{
    class IndicesCsv2Display
    {

        //use the following for the command line for the <indicesCsv2Image> task. 
        //indicesCsv2Image  "C:\SensorNetworks\Output\SunshineCoast\Site1\Towsey.MultiAnalyser\DM420036_Towsey.MultiAnalyser.Indices.csv" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg"  C:\SensorNetworks\Output\SunshineCoast\Site1\Towsey.MultiAnalyser\DM420036_Towsey.MultiAnalyser.IndicesNEW.png

        /// <summary>
        /// loads a csv file for visualisation
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int Main(string[] args)
        {
            int status = 0;
            bool verbose = true;

            if (CheckArguments(args) != 0) //checks validity of the first 3 path arguments
            {
                Console.WriteLine("\nPress <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }

            string csvPath    = args[0];
            string configPath = args[1];
            string imagePath  = args[2];


            if (verbose)
            {
                string date = "# DATE AND TIME: " + DateTime.Now;
                Console.WriteLine("# MAKE A IMAGE FROM CSV FILE OF ACOUSTIC INDICES");
                Console.WriteLine(date);
                Console.WriteLine("# Input  audio file: " + Path.GetFileName(csvPath));
                Console.WriteLine("# Output image file: " + imagePath);
            }


            var fiConfig = new FileInfo(configPath);
            var configuration = new ConfigDictionary(fiConfig.FullName);
            Dictionary<string, string> configDict = configuration.GetTable();

            string analysisIdentifier = configDict[Keys.ANALYSIS_NAME];
            var analysers = AnalysisCoordinator.GetAnalysers(typeof(MainEntry).Assembly);
            IAnalyser analyser = analysers.FirstOrDefault(a => a.Identifier == analysisIdentifier);
            if (analyser == null)
            {
                Console.WriteLine("\nWARNING: Could not construct image from CSV file. Analysis name not recognized: " + analysisIdentifier);
                Console.WriteLine("\nPress <ENTER> key to exit.");
                Console.ReadLine();
                return 1;
            }

            //#########################################################################################################
            var output = analyser.ProcessCsvFile(new FileInfo(csvPath), fiConfig);
            if (output == null) return 3;
            //DataTable dtRaw = output.Item1;
            DataTable dt2Display = output.Item2;
            analyser = null;
            bool normalisedDisplay = true;
            string fileName = Path.GetFileNameWithoutExtension(imagePath);
            string title = String.Format("(c) Queensland University of Technology.   SOURCE:{0};  ", fileName);
            Bitmap tracksImage = DisplayIndices.ConstructVisualIndexImage(dt2Display, title, normalisedDisplay, imagePath);
            //#########################################################################################################

            if (verbose)
            {
                Console.WriteLine("\n##### FINISHED FILE ###################################################\n");
                Console.ReadLine();
            }

            return status;
        } //LoadIndicesCsvFileAndDisplayTracksImage()



        public static int CheckArguments(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("\nINCORRECT COMMAND LINE.");
                Console.WriteLine("\nTHE COMMAND LINE HAS {0} ARGUMENTS", (args.Length+1));
                foreach (string arg in args) Console.WriteLine(arg + "  ");
                Console.WriteLine("\nYOU REQUIRE 4 COMMAND LINE ARGUMENTS\n");
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
            //GET THREE OBLIGATORY COMMAND LINE ARGUMENTS
            string csvPath    = args[0];
            string configPath = args[1];
            string outputPath = args[2];
            DirectoryInfo diSource = new DirectoryInfo(Path.GetDirectoryName(csvPath));
            if (!diSource.Exists)
            {
                Console.WriteLine("Source directory does not exist: " + diSource.FullName);
                status = 2;
                return status;
            }
            FileInfo fiSource = new FileInfo(csvPath);
            if (!fiSource.Exists)
            {
                Console.WriteLine("Source directory exists: " + diSource.FullName);
                Console.WriteLine("\t but the source file does not exist: " + csvPath);
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
                    Console.WriteLine("Output directory does not exist and could not be created: " + diOP.FullName);
                    status = 2;
                    return status;
                }
            }
            return status;
        }


        public static void Usage()
        {
            Console.WriteLine("USAGE:");
            Console.WriteLine("AnalysisPrograms.exe  indicesCsv2Display  csvPath  configPath  outputPath");
            Console.WriteLine("where:");
            Console.WriteLine("indicesCsv2Display:- (string) a short string that selects the process to be performed.");
            Console.WriteLine("input  csv  File:-   (string) Path of the indices.csv file to be processed.");
            Console.WriteLine("configuration File:- (string) Path of the configuration file containing relevant parameters.");
            Console.WriteLine("output File:-        (string) Path of the image.png file.");
//            Console.WriteLine("The following argument is OPTIONAL.");
//            Console.WriteLine("verbosity:   (boolean) true/false");
            Console.WriteLine("");
        } // Usage()

    } //class
}
