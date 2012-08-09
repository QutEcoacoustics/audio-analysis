using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using AnalysisBase;
using TowseyLib;


namespace AnalysisPrograms
{
    class AnalysesAvailable
    {

        //use the following for the command line for the <analysesAvailable> task. 
        //AnalysisPrograms.exe analysesAvailable  C:\SensorNetworks\Output\temp\analysesAvailable.txt

        /// <summary>
        /// returns a text file of the available analyses
        /// Also writes those analyses to Console.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int Main(string[] args)
        {
            bool debug = false;
            int status = 0;
            bool verbose = true;

            if (CheckArguments(args) != 0) //checks validity of the first 3 path arguments
            {
                if (debug)
                {
                    Console.WriteLine("\nPress <ENTER> key to exit.");
                    Console.ReadLine();
                }
                System.Environment.Exit(1);
            }

            string outputPath = args[0];


            if (verbose)
            {
                string date = "# DATE AND TIME: " + DateTime.Now;
                Console.WriteLine("# WRITE FILE OF THE AVAILABLE ANALYSIS IDENTIFIERS.");
                Console.WriteLine(date);
                Console.WriteLine("# Output  file: " + outputPath);
                Console.WriteLine("\n#######################################################################\n");
            }


            //#########################################################################################################
            var list = new List<string>();
            var analysers = AnalysisCoordinator.GetAnalysers(typeof(MainEntry).Assembly);
            foreach (IAnalyser analyser in analysers)
            {
                Console.WriteLine(analyser.Identifier);
                list.Add(analyser.Identifier);
            }
            FileTools.WriteTextFile(outputPath, list);
            //#########################################################################################################

            if (debug)
            {
                Console.WriteLine("\n##### FINISHED FILE ###################################################\n");
                Console.ReadLine();
            }

            return status;
        } //LoadIndicesCsvFileAndDisplayTracksImage()



        public static int CheckArguments(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("\nINCORRECT COMMAND LINE.");
                Console.WriteLine("\nTHE COMMAND LINE HAS {0} ARGUMENTS", (args.Length + 1));
                foreach (string arg in args) Console.WriteLine(arg + "  ");
                Console.WriteLine("\nYOU REQUIRE 2 COMMAND LINE ARGUMENTS\n");
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
            //GET ONE OBLIGATORY COMMAND LINE ARGUMENTS
            string outputPath = args[0];
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
            Console.WriteLine("AnalysisPrograms.exe  analysesAvailable  outputPath");
            Console.WriteLine("where:");
            Console.WriteLine("analysesAvailable:- (string) a short string that identifies the process to be performed.");
            Console.WriteLine("output File:-       (string) Path of the analysisIdentifiers.txt file.");
            //            Console.WriteLine("The following argument is OPTIONAL.");
            //            Console.WriteLine("verbosity:   (boolean) true/false");
            Console.WriteLine("");
        } // Usage()

    }
}
