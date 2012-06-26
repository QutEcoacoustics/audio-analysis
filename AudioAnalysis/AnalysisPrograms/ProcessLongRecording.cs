using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
//using System.Threading.Tasks;


//using Acoustics.Shared;
using Acoustics.Tools;
//using Acoustics.Tools.Audio;
using AnalysisBase;
using TowseyLib;
using System.Threading.Tasks;
using AnalysisRunner;
using Acoustics.Tools.Audio;
using Acoustics.Shared;
using AudioBrowser;
using AudioAnalysisTools;
using System.Diagnostics.Contracts;


namespace AnalysisPrograms
{
    class AnalyseLongRecording
    {
        //use the following paths for the command line.
        string recordingPath = @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h.mp3";
        string configPath    = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg";
        string outputDir     = @"C:\SensorNetworks\Output\Test1";
        // THE COMMAND LINE
        //C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h.mp3  C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg  C:\SensorNetworks\Output\Test1 
        //"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h.mp3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\Test1"

        public const int RESAMPLE_RATE = 17640;


        public static int Main(string[] args)
        {
            int status = 0;
            string title = "# PROCESS LONG RECORDING";
            string date = "# DATE AND TIME: " + DateTime.Now;
            bool verbose = true;
            if (verbose)
            {
                Console.WriteLine(title);
                Console.WriteLine(date);
            }

            if (CheckArguments(args) != 0)
            {
                Console.WriteLine("\nPress <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }

            string recordingPath = args[0];
            string configPath    = args[1];
            string outputDir     = args[2];

            if (verbose)
            {
                Console.WriteLine("# Output folder =" + outputDir);
                Console.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            }

            DirectoryInfo diSource = new DirectoryInfo(Path.GetDirectoryName(recordingPath));
            FileInfo fiSourceRecording = new FileInfo(recordingPath);
            FileInfo fiConfig = new FileInfo(configPath);
            var configuration = new ConfigDictionary(fiConfig.FullName);
            DirectoryInfo diOP = new DirectoryInfo(outputDir);
             
            // run the analysis
            AnalysisCoordinator coord = new AnalysisCoordinator(new LocalSourcePreparer())
            {
                DeleteFinished = false,
                //DeleteFinished = true,
                IsParallel = true,
                SubFoldersUnique = false
            };

            var fileSegment = new FileSegment { OriginalFile = fiSourceRecording };
            IAnalyser analyser = new MultiAnalyser();

            //test conversion of events file to indies file
            if (false)
            {
                string ipPath = @"C:\SensorNetworks\Output\Test1\Towsey.MultiAnalyser\DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h_Towsey.MultiAnalyser.Events.csv";
                string opPath = @"C:\SensorNetworks\Output\Test1\Towsey.MultiAnalyser\DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h_Towsey.MultiAnalyser.Indices.csv";
                DataTable dt = CsvTools.ReadCSVToTable(ipPath, true);
                TimeSpan unitTime = new TimeSpan(0, 0, 60);
                TimeSpan source   = new TimeSpan(0, 59, 47);
                double dummy = 0.0;
                DataTable dt1 = analyser.ConvertEvents2Indices(dt, unitTime, source, dummy);
                CsvTools.DataTable2CSV(dt1, opPath);
                Console.WriteLine("FINISHED");
                Console.ReadLine();
            }


            var analysisSettings = analyser.DefaultSettings;
            analysisSettings.ConfigFile = fiConfig;
            analysisSettings.AnalysisBaseDirectory = diOP;
            analysisSettings.ConfigDict = configuration.GetTable();

            var analyserResults = coord.Run(new[] { fileSegment }, analyser, analysisSettings);
            
            // write the results to file
            DataTable datatable = TempTools.MergeResultsIntoSingleDataTable(analyserResults);

            //get the duration of the original source audio file - need this to convert Events datatable to Indices Datatable
            var audioUtility = new MasterAudioUtility(analysisSettings.SegmentTargetSampleRate, SoxAudioUtility.SoxResampleQuality.VeryHigh);
            var mimeType = MediaTypes.GetMediaType(fiSourceRecording.Extension);
            var sourceDuration = audioUtility.Duration(fiSourceRecording, mimeType);

            var op1 = TempTools.GetEventsAndIndicesDataTables(datatable, analyser, sourceDuration);
            var eventsDatatable = op1.Item1;
            var indicesDatatable = op1.Item2;
            int eventsCount = 0;
            if (eventsDatatable != null) eventsCount = eventsDatatable.Rows.Count;
            int indicesCount = 0;
            if (indicesDatatable != null) indicesCount = indicesDatatable.Rows.Count;
            var opdir = analyserResults.ElementAt(0).SettingsUsed.AnalysisRunDirectory;
            string fName = Path.GetFileNameWithoutExtension(fiSourceRecording.Name) + "_" + analyser.Identifier;
            var op2 = TempTools.SaveEventsAndIndicesDataTables(eventsDatatable, indicesDatatable, fName, opdir.FullName);        

            var fiEventsCSV = op2.Item1;
            var fiIndicesCSV = op2.Item2;

            Console.WriteLine("###################################################");
            Console.WriteLine("Finished processing " + fiSourceRecording.Name + ".");
            Console.WriteLine("Output  to  directory: " + diOP.FullName);
            if (fiEventsCSV != null)
            {
                Console.WriteLine("EVENTS CSV file(s) = " + fiEventsCSV.Name);
                Console.WriteLine("\tNumber of events = " + eventsCount);
            }
            if (fiIndicesCSV != null)
            {
                Console.WriteLine("INDICES CSV file(s) = " + fiIndicesCSV.Name);
                Console.WriteLine("\tNumber of indices = " + indicesCount);
            }
            Console.WriteLine("###################################################\n");

            return status;
        }

        public static int CheckArguments(string[] args)
        {
            int argumentCount = 3;
            if (args.Length != argumentCount)
            {
                Console.WriteLine("THE COMMAND LINE HAS {0} ARGUMENTS", args.Length);
                foreach (string arg in args) Console.WriteLine(arg + "  ");
                Console.WriteLine("YOU REQUIRE {0} COMMAND LINE ARGUMENTS\n", argumentCount);
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
            string outputDir = args[2];
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
            DirectoryInfo diOP = new DirectoryInfo(outputDir);
            if (!diOP.Exists)
            {
                Console.WriteLine("Output directory does not exist: " + diOP.FullName);
                status = 2;
                return status;
            }
            return status;
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
        }

    } //class AnalyseLongRecording
}
