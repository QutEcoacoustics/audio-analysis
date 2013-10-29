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
    using Acoustics.Shared.Extensions;

    using AnalysisPrograms.Production;

    using PowerArgs;

    public class IndicesCsv2Display
    {
        public class Arguments
        {
            [ArgDescription("The source csv file to operate on")]
            [Production.ArgExistingFile(Extension = ".csv")]
            [ArgPosition(0)]
            public FileInfo InputCsv { get; set; }

            // Note: not required
            [ArgDescription("The path to the config file")]
            [Production.ArgExistingFile]
            public FileInfo Config { get; set; }

            [ArgDescription("A file path to write output image to")]
            [ArgNotExistingFile(Extension = ".png")]
            [ArgRequired]
            public FileInfo Output { get; set; }
        }

        //use the following for the command line for the <indicesCsv2Image> task. 
        //indicesCsv2Image  "C:\SensorNetworks\Output\SunshineCoast\Site1\Towsey.MultiAnalyser\DM420036_Towsey.MultiAnalyser.Indices.csv"            "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg"  C:\SensorNetworks\Output\SunshineCoast\Site1\Towsey.MultiAnalyser\DM420036_Towsey.MultiAnalyser.IndicesNEW.png
        //indicesCsv2Image  "C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\IndicesCsv2Image\DM420044_20111020_000000_Towsey.Acoustic.Indices.csv" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"       C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\IndicesCsv2Image\DM420044_20111020_000000_Towsey.Acoustic.Indices.png


        // indicesCsv2Image  "C:\SensorNetworks\Output\LSKiwi3\Towsey.Acoustic\TOWER_20100208_204500_Towsey.Acoustic.Indices.csv"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"   "C:\SensorNetworks\Output\LSKiwi3\Towsey.Acoustic\TOWER_20100208_204500_Towsey.Acoustic.Indices.png

        private static Arguments Dev()
        {
            throw new NoDeveloperMethodException();
        }

        /// <summary>
        /// Loads a csv file for visualisation and displays TracksImage
        /// </summary>
        /// <param name="arguments"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public static void Main(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            bool verbose = true;

            /*ATA
            CheckArguments(args); //checks validity of the three path arguments
 
            string csvPath    = args[0];
            string configPath = args[1];
            string imagePath  = args[2]; */

            if (arguments.Config == null)
            {
                LoggedConsole.WriteLine("### WARNING: Config file is not provided - using defaults");
            }

            arguments.Output.CreateParentDirectories();

            if (verbose)
            {
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine("# MAKE AN IMAGE FROM A CSV FILE OF INDICES DERIVED FROM AN AUDIO RECORDING");
                LoggedConsole.WriteLine(date);
                LoggedConsole.WriteLine("# Input  audio  file: " + arguments.InputCsv);
                LoggedConsole.WriteLine("# Configuration file: " + arguments.Config);
                LoggedConsole.WriteLine("# Output image  file: " + arguments.Output);
            }

            string analysisIdentifier = null;
            if (arguments.Config.Exists)
            {
                var configuration = new ConfigDictionary(arguments.Config);
                Dictionary<string, string> configDict = configuration.GetTable();
                analysisIdentifier = configDict[Keys.ANALYSIS_NAME];
            }

            var outputDTs = Tuple.Create(new DataTable(), new DataTable() );

            var analysers = AnalysisCoordinator.GetAnalysers(typeof(MainEntry).Assembly);
            IAnalyser analyser = analysers.FirstOrDefault(a => a.Identifier == analysisIdentifier);
            if (analyser == null)
            {
                LoggedConsole.WriteLine("\nWARNING: Analysis name not recognized: " + analysisIdentifier);
                LoggedConsole.WriteLine("\t\t Will construct default image");
                outputDTs = DisplayIndices.ProcessCsvFile(arguments.InputCsv);
            }
            else
            {
                outputDTs = analyser.ProcessCsvFile(arguments.InputCsv, arguments.Config);

            }
            analyser = null;

            //DataTable dtRaw = output.Item1;
            DataTable dt2Display = outputDTs.Item2;
            if (dt2Display == null)
            {
                throw new InvalidOperationException("Data table is null - cannot continue");
            }

            // #########################################################################################################
            // Convert datatable to image
            bool normalisedDisplay = false;
            string fileName = Path.GetFileNameWithoutExtension(arguments.Output.Name);
            string title = String.Format("(c) QUT.EDU.AU - SOURCE:{0};  ", fileName);
            Bitmap tracksImage = DisplayIndices.ConstructVisualIndexImage(dt2Display, title, normalisedDisplay, arguments.Output);
            // #########################################################################################################

            if (tracksImage == null)
            {
                LoggedConsole.WriteLine("\nWARNING: Null image returned from DisplayIndices.ConstructVisualIndexImage(dt2Display, title, normalisedDisplay, imagePath);");
                throw new AnalysisOptionDevilException();
            }

        } // Main();


        /*ATA
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
        } // Usage()*/


    } //class
}
