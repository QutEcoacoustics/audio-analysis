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


        public static void Main(string[] args)
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
            string startOffsetMins = args[3];
            string endOffsetMins = args[4];

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

            var fileSegment = new FileSegment { 
                OriginalFile = fiSourceRecording,
                SegmentStartOffset = TimeSpan.FromMinutes(double.Parse(startOffsetMins)),
                SegmentEndOffset = TimeSpan.FromMinutes(double.Parse(endOffsetMins)),
            };
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

            if (datatable != null)
            {
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
            }
            else
            {
                Console.WriteLine("###################################################\n");
                Console.WriteLine("No results");
                Console.WriteLine("###################################################\n");
            }
            //return status;
            return;
        }

        public static int CheckArguments(string[] args)
        {
            int argumentCount = 5;
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
                bool success = true;
                try
                {
                    Directory.CreateDirectory(outputDir);
                    success = Directory.Exists(outputDir);
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


//rem run the analysis, note: needs multi analysis build of analysis programs.exe
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site3\DM420037.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site3"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site4\DM420062.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site4"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site5\DM420050.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site5"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site6\DM420048.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site6"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site9\DM420039.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site9"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site10\DM420049.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site10"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site11\DM420057.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site11"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site12\DM420041.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site12"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site13\DM420054.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site13"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site14\DM420053.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site14"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site22\DM420040.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site22"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site24\DM420002.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site24"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site25\DM420012.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site25"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site27\DM420029.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site27"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site28\DM420009.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site28"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site29\DM420016.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site29"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site30\DM420015.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site30"

//pause




//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	0	30
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	30	60
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	60	90
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	90	120
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	120	150
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	150	180
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	180	210
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	210	240
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	240	270
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	270	300
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	300	330
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	330	360
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	360	390
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	390	420
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	420	450
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	450	480
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	480	510
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	510	540
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	540	570
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	570	600
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	600	630
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	630	660
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	660	690
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	690	720
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	720	750
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	750	780
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	780	810
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	810	840
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	840	870
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	870	900
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	900	930
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	930	960
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	960	990
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	990	1020
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	1020	1050
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	1050	1080
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	1080	1110
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	1110	1140
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	1140	1170
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	1170	1200
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	1200	1230
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	1230	1260
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	1260	1290
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	1290	1320
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	1320	1350
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	1350	1380
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	1380	1410
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser"	1410	1440


//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 0 30
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 30 60
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 60 90
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 90 120
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 120 150
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 150 180
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 180 210
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 210 240
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 240 270
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 270 300
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 300 330
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 330 360
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 360 390
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 390 420
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 420 450
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 450 480
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 480 510
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 510 540
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 540 570
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 570 600
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 600 630
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 630 660
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 660 690
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 690 720
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 720 750
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 750 780
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 780 810
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 810 840
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 840 870
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 870 900
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 900 930
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 930 960
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 960 990
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 990 1020
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 1020 1050
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 1050 1080
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 1080 1110
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 1110 1140
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 1140 1170
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 1170 1200
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 1200 1230
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 1230 1260
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 1260 1290
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 1290 1320
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 1320 1350
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 1350 1380
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 1380 1410
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1" 1410 1440
