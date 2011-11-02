using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using TowseyLib;
using AudioTools.AudioUtlity;
using AudioAnalysisTools;
using QutSensors.Shared.LogProviders;





namespace AnalysisPrograms
{
    class SpeciesAccumulationCurve
    {

        static string HEADER = "sample,additional,total";


        public static void Dev(string[] args)
        {

            //SET VERBOSITY
            DateTime datetime = DateTime.Now;
            Log.Verbosity = 1;
            Log.WriteLine("# Start Time = " + datetime.ToString());


            //i: Set up the dir and file names
            string inputDir = @"C:\SensorNetworks\WavFiles\SpeciesRichness\";
            string inputfile = "SE_13102010_Full Day_AndTotals.csv";
            string occurenceFile = inputDir + inputfile;
            Log.WriteLine("Directory:          " + inputDir);
            Log.WriteLine("Selected file:      " + inputfile);

            string outputDir = inputDir;
            Log.WriteIfVerbose("# Output folder =" + outputDir);
            string opFileName = "Results_ARI_" + FileTools.TimeStamp2FileName(datetime) + ".csv";
            string opPath = outputDir + opFileName; // .csv file

            //READ CSV FILE TO MASSAGE DATA
            var results1 = READ_OCCURENCE_CSV_DATA(occurenceFile);
            List<string> speciesList = results1.Item1;
            //the speciesList contains 62 species names from columns 3 to 64 i.e. 62 species.
            byte[,] occurenceMatrix = results1.Item2;

            if (false)
            {
                Console.ReadLine();
                Environment.Exit(666);
            }


            //init counters
            int fileCount = 0;
            double elapsedTime = 0.0;
            DateTime tStart = DateTime.Now;

            Console.WriteLine("\n\n");
            Log.WriteLine("###### " + (++fileCount) + " #### Process Recording: " + inputfile + " ###############################");


            DateTime tEnd = DateTime.Now;
            TimeSpan duration = tEnd - tStart;
            Log.WriteLine("# Elapsed Time = " + duration.TotalSeconds);
            Log.WriteLine("# Finished everything!");
            Console.ReadLine();
        } //DEV()

        /// <summary>
        /// EXECUTABLE - To CALL THIS METHOD MUST EDIT THE MainEntry.cs FILE
        /// extracts acoustic richness indices from a single recording.
        /// </summary>
        /// <param name="args"></param>
        public static void Executable(string[] args)
        {
            DateTime tStart = DateTime.Now;
            //SET VERBOSITY
            Log.Verbosity = 0;
            bool doStoreImages = false;
            CheckArguments(args);

            string recordingPath = args[0];
            string opPath = args[1];

            //i: Set up the dir and file names
            string recordingDir = Path.GetDirectoryName(recordingPath);
            string outputDir = Path.GetDirectoryName(opPath);
            string fileName = Path.GetFileName(recordingPath);

            //init counters
            double elapsedTime = 0.0;
            int fileCount = 1;

            //write header to results file
            if (!File.Exists(opPath))
            {
                FileTools.WriteTextFile(opPath, HEADER);
            }
            else //calculate file number and total elapsed time so far
            {
                List<string> text = FileTools.ReadTextFile(opPath);  //read results file
                string[] lastLine = text[text.Count - 1].Split(','); // read and split the last line
                if (!lastLine[0].Equals("count")) Int32.TryParse(lastLine[0], out fileCount);
                fileCount++;
                if (!lastLine[1].Equals("minutes")) Double.TryParse(lastLine[1], out elapsedTime);
            }

            //Console.WriteLine("\n\n");
            Log.WriteLine("###### " + fileCount + " #### Process Recording: " + fileName + " ###############################");


            DateTime tEnd = DateTime.Now;
            TimeSpan duration = tEnd - tStart;
            Log.WriteLine("###### Elapsed Time = " + duration.TotalSeconds + " #####################################\n");
        } //EXECUTABLE()


        //#########################################################################################################################################################




        public static System.Tuple<List<string>, byte[,]> READ_OCCURENCE_CSV_DATA(string occurenceFile)
        {
            int startColumn = 3;
            int endColumn = 64;
            List<string> text = FileTools.ReadTextFile(occurenceFile);  // read occurence file
            List<string> speciesList = new List<string>();
            string[] line = text[0].Split(',');                    // read and split the first line
            for (int j = startColumn; j <= endColumn; j++) speciesList.Add(line[j]);

            byte[,]  occurenceMatrix = new byte[text.Count - 1, line.Length];
            byte[] speciesCount = new byte[text.Count - 1];
            for (int i = 1; i < text.Count; i++)
            {
                line = text[i].Split(',');                    // read and split the first line
                for (int j = startColumn; j <= endColumn; j++)
                {
                    if (line[j].StartsWith("1")) occurenceMatrix[i, j] = 1;
                }
                speciesCount[i-1] = Byte.Parse(line[endColumn+2]);
            }
            //the speciesList contains 62 species names from columns 3 to 64 i.e. 62 species.

            //now cross check that all is OK
            for (int i = 0; i < occurenceMatrix.GetLength(0); i++)
            {
                int sum = 0;
                for (int j = 0; j < occurenceMatrix.GetLength(1); j++) sum += occurenceMatrix[i,j];

                if (speciesCount[i] != sum) Console.WriteLine("WARNING: ROW {0}: Matrix row sum != Species count i.e. {1} != {2}", i, speciesCount[i], sum);
            }

            return Tuple.Create(speciesList, occurenceMatrix);
        }


        public static void CheckArguments(string[] args)
        {
            int argumentCount = 2;
            if (args.Length != argumentCount)
            {
                Log.WriteLine("THE COMMAND LINE HAS {0} ARGUMENTS", args.Length);
                foreach (string arg in args) Log.WriteLine(arg + "  ");
                Log.WriteLine("YOU REQUIRE {0} COMMAND LINE ARGUMENTS\n", argumentCount);
                Usage();
            }
            CheckPaths(args);
        }

        /// <summary>
        /// this method checks for the existence of a file and directory expected as two arguments of the command line.
        /// </summary>
        /// <param name="args"></param>
        public static void CheckPaths(string[] args)
        {
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Cannot find recording file <" + args[0] + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
            string opDir = Path.GetDirectoryName(args[1]);
            if (!Directory.Exists(opDir))
            {
                Console.WriteLine("Cannot find output directory: <" + opDir + ">");
                Usage();
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
        }


        public static void Usage()
        {
            Console.WriteLine("INCORRECT COMMAND LINE.");
            Console.WriteLine("USAGE:");
            Console.WriteLine("SpeciesAccumulation.exe inputFilePath outputFilePath");
            Console.WriteLine("where:");
            Console.WriteLine("inputFileName:- (string) Path of the input  file to be processed.");
            Console.WriteLine("outputFileName:-(string) Path of the output file to store results.");
            Console.WriteLine("");
            Console.WriteLine("\nPress <ENTER> key to exit.");
            Console.ReadLine();
            System.Environment.Exit(1);
        }



    }
}
