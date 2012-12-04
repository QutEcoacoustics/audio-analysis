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
        //indicesCsv2Image  "C:\SensorNetworks\Output\SunshineCoast\Site1\Towsey.MultiAnalyser\DM420036_Towsey.MultiAnalyser.Indices.csv"            "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg"  C:\SensorNetworks\Output\SunshineCoast\Site1\Towsey.MultiAnalyser\DM420036_Towsey.MultiAnalyser.IndicesNEW.png
        //indicesCsv2Image  "C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\IndicesCsv2Image\DM420044_20111020_000000_Towsey.Acoustic.Indices.csv" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"       C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\IndicesCsv2Image\DM420044_20111020_000000_Towsey.Acoustic.Indices.png


        // indicesCsv2Image  "C:\SensorNetworks\Output\LSKiwi3\Towsey.Acoustic\TOWER_20100208_204500_Towsey.Acoustic.Indices.csv"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"   "C:\SensorNetworks\Output\LSKiwi3\Towsey.Acoustic\TOWER_20100208_204500_Towsey.Acoustic.Indices.png


        /// <summary>
        /// Loads a csv file for visualisation and displays TracksImage
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static void Main(string[] args)
        {
            bool verbose = true;

            CheckArguments(args); //checks validity of the three path arguments
 
            string csvPath    = args[0];
            string configPath = args[1];
            string imagePath  = args[2];


            if (verbose)
            {
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine("# MAKE AN IMAGE FROM A CSV FILE OF INDICES DERIVED FROM AN AUDIO RECORDING");
                LoggedConsole.WriteLine(date);
                LoggedConsole.WriteLine("# Input  audio  file: " + csvPath);
                LoggedConsole.WriteLine("# Configuration file: " + configPath);
                LoggedConsole.WriteLine("# Output image  file: " + imagePath);
            }

            string analysisIdentifier = null;
            var fiConfig = new FileInfo(configPath);
            if (fiConfig.Exists)
            {
                var configuration = new ConfigDictionary(fiConfig.FullName);
                Dictionary<string, string> configDict = configuration.GetTable();
                analysisIdentifier = configDict[Keys.ANALYSIS_NAME];
            }
            else
            {
                LoggedConsole.WriteLine("\nWARNING: Config file does not exist: " + fiConfig.FullName);
            }
            var outputDTs = Tuple.Create(new DataTable(), new DataTable() );

            var analysers = AnalysisCoordinator.GetAnalysers(typeof(MainEntry).Assembly);
            IAnalyser analyser = analysers.FirstOrDefault(a => a.Identifier == analysisIdentifier);
            if (analyser == null)
            {
                LoggedConsole.WriteLine("\nWARNING: Analysis name not recognized: " + analysisIdentifier);
                LoggedConsole.WriteLine("\t\t Will construct default image");
                outputDTs = DisplayIndices.ProcessCsvFile(new FileInfo(csvPath));
            }
            else
            {
                outputDTs = analyser.ProcessCsvFile(new FileInfo(csvPath), fiConfig);

            }
            analyser = null;

            //DataTable dtRaw = output.Item1;
            DataTable dt2Display = outputDTs.Item2;
            if (dt2Display == null) throw new AnalysisOptionInvalidPathsException();

            // #########################################################################################################
            // Convert datatable to image
            bool normalisedDisplay = false;
            string fileName = Path.GetFileNameWithoutExtension(imagePath);
            string title = String.Format("(c) QUT, Brisbane.   SOURCE:{0};  ", fileName);
            Bitmap tracksImage = DisplayIndices.ConstructVisualIndexImage(dt2Display, title, normalisedDisplay, imagePath);
            // #########################################################################################################

            if (tracksImage == null)
            {
                LoggedConsole.WriteLine("\nWARNING: Null image returned from DisplayIndices.ConstructVisualIndexImage(dt2Display, title, normalisedDisplay, imagePath);");
                throw new AnalysisOptionDevilException();
            }

        } // Main();



        public static void CheckArguments(string[] args)
        {
            if (args.Length != 3)
            {
                LoggedConsole.WriteLine("\nINCORRECT COMMAND LINE.");
                LoggedConsole.WriteLine("\nTHE COMMAND LINE HAS {0} ARGUMENTS", (args.Length+1));
                foreach (string arg in args) LoggedConsole.WriteLine(arg + "  ");
                LoggedConsole.WriteLine("\nYOU REQUIRE 4 COMMAND LINE ARGUMENTS\n");
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
            // GET THREE OBLIGATORY COMMAND LINE ARGUMENTS
            string csvPath    = args[0];
            string configPath = args[1];
            string outputPath = args[2];
            DirectoryInfo diSource = new DirectoryInfo(Path.GetDirectoryName(csvPath));
            if (!diSource.Exists)
            {
                LoggedConsole.WriteLine("Source directory does not exist: " + diSource.FullName);
                
                throw new AnalysisOptionInvalidPathsException();
            }

            FileInfo fiSource = new FileInfo(csvPath);
            if (!fiSource.Exists)
            {
                LoggedConsole.WriteLine("Source directory exists: " + diSource.FullName);
                LoggedConsole.WriteLine("\t but the source file does not exist: " + csvPath);
                
                throw new AnalysisOptionInvalidPathsException();
            }

            FileInfo fiConfig = new FileInfo(configPath);
            if (!fiConfig.Exists)
            {
                LoggedConsole.WriteLine("### WARNING: Config file does not exist: " + fiConfig.FullName); // LET THIS BE OK. Proceed anyway with default 
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
                    LoggedConsole.WriteLine("Output directory does not exist and could not be created: " + diOP.FullName);
                    throw new AnalysisOptionInvalidPathsException();
                }
            }
        } // CheckPaths()


        public static void Usage()
        {
            LoggedConsole.WriteLine("USAGE:");
            LoggedConsole.WriteLine("AnalysisPrograms.exe  indicesCsv2Image  csvPath  configPath  outputPath");
            LoggedConsole.WriteLine("where:");
            LoggedConsole.WriteLine("indicesCsv2Image:-   (string) this is a literal string that selects the analytical process to be performed.");
            LoggedConsole.WriteLine("input  csv  File:-   (string) Path of the indices.csv file to be processed.");
            LoggedConsole.WriteLine("configuration File:- (string) Path of the configuration file containing relevant parameters.");
            LoggedConsole.WriteLine("output File:-        (string) Path of the image.png file for output.");
            LoggedConsole.WriteLine("");
        } // Usage()

    } //class
}
