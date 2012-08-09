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

            CheckArguments(args); //checks validity of the first 3 path arguments


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


                Console.WriteLine("\n##### FINISHED FILE ###################################################\n");

            return status;
        } //LoadIndicesCsvFileAndDisplayTracksImage()



        public static void CheckArguments(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("\nINCORRECT COMMAND LINE.");
                Console.WriteLine("\nTHE COMMAND LINE HAS {0} ARGUMENTS", (args.Length + 1));
                foreach (string arg in args) Console.WriteLine(arg + "  ");
                Console.WriteLine("\nYOU REQUIRE 2 COMMAND LINE ARGUMENTS\n");
                Usage();
                
                throw new AnalysisOptionInvalidArgumentsException();
            }

            CheckPaths(args);
        }

        /// <summary>
        /// this method checks validity of first three command line arguments.
        /// </summary>
        /// <param name="args"></param>
        public static void CheckPaths(string[] args)
        {
            // GET ONE OBLIGATORY COMMAND LINE ARGUMENTS
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
                  
                    throw new AnalysisOptionInvalidPathsException();
                }
            }
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
