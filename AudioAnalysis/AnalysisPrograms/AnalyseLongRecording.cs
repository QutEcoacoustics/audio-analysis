

namespace AnalysisPrograms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.Contracts;
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

    class AnalyseLongRecording
    {
        //use the following paths for the command line.

        // MULTI-ANALYSER DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h.mp3
        //string recordingPath = @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h.mp3";
        //string configPath    = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg";
        //string outputDir     = @"C:\SensorNetworks\Output\Test1";

        // LITTLE SPOTTED KIWI3
        //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav";
        //string configPath    = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.LSKiwi3.cfg";
        //string outputDir     = @"C:\SensorNetworks\Output\LSKiwi3\";

        //ACOUSTIC INDICES
        //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav";
        //string configPath    = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.LSKiwi3.cfg";
        //string outputDir     = @"C:\SensorNetworks\Output\LSKiwi3\";

        // THE COMMAND LINES DERIVED FROM ABOVE for the <audio2csv> task. 
        //FOR  MULTI-ANALYSER and CROWS
        //audio2csv  "C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h.mp3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\Test1"
        
        //FOR  LITTLE SPOTTED KIWI3
        // audio2csv  "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.LSKiwi3.cfg" C:\SensorNetworks\Output\LSKiwi3\
        
        // FOR  ACOUSTIC INDICES
        // audio2csv  "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav"                   "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\LSKiwi3"
        
        // CHECKING 16Hz PROBLEMS
        // audio2csv  "C:\SensorNetworks\WavFiles\Frogs\Curramore\CurramoreSelection-mono16kHz.mp3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\AudioToCSV"
        // audio2csv  "C:\SensorNetworks\WavFiles\16HzRecording\CREDO1_20120607_063200.mp3"          "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\AudioToCSV"
 

        public static void Main(string[] args)
        {
            bool verbose = true;
            bool debug   = false;
#if DEBUG
            debug = true;
#endif

            if (verbose)
            {
                string title = "# PROCESS LONG RECORDING";
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine(title);
                LoggedConsole.WriteLine(date);
            }

            CheckArguments(args); // checks validity of arguments 2, 3, and 4. First argument already removed.


            string recordingPath = args[0];
            string configPath    = args[1];
            string outputDir     = args[2];

            if (verbose)
            {
                LoggedConsole.WriteLine("# Recording file:     " + Path.GetFileName(recordingPath));
                LoggedConsole.WriteLine("# Configuration file: " + configPath);
                LoggedConsole.WriteLine("# Output folder:      " + outputDir);
            }

            //1. set up the necessary files
            DirectoryInfo diSource = new DirectoryInfo(Path.GetDirectoryName(recordingPath));
            FileInfo fiSourceRecording = new FileInfo(recordingPath);
            FileInfo fiConfig = new FileInfo(configPath);
            DirectoryInfo diOP = new DirectoryInfo(outputDir);

            //2. get the analysis config dictionary
            var configuration = new ConfigDictionary(fiConfig.FullName);
            Dictionary<string, string> configDict = configuration.GetTable();

            //3. initilise AnalysisCoordinator class that will do the analysis
            bool saveIntermediateWavFiles = false;
            if (configDict.ContainsKey(Keys.SAVE_INTERMEDIATE_WAV_FILES))
                saveIntermediateWavFiles = ConfigDictionary.GetBoolean(Keys.SAVE_INTERMEDIATE_WAV_FILES, configDict);

            bool saveSonograms = false;
            if (configDict.ContainsKey(Keys.SAVE_SONOGRAMS))
                saveSonograms = ConfigDictionary.GetBoolean(Keys.SAVE_SONOGRAMS, configDict);

            
            bool doParallelProcessing = false;
            if (configDict.ContainsKey(Keys.PARALLEL_PROCESSING))
                doParallelProcessing = ConfigDictionary.GetBoolean(Keys.PARALLEL_PROCESSING, configDict);

            AnalysisCoordinator analysisCoordinator = new AnalysisCoordinator(new LocalSourcePreparer())
            {
                DeleteFinished = (!saveIntermediateWavFiles), // create and delete directories 
                IsParallel = doParallelProcessing,         // ########### PARALLEL OR SEQUENTIAL ??????????????
                SubFoldersUnique = false
            };

            //4. get the segment of audio to be analysed
            var fileSegment = new FileSegment { }; 
            if(args.Length == 3)
            {
                fileSegment = new FileSegment { OriginalFile = fiSourceRecording };
            }
            else if (args.Length == 5)
            {
                string startOffsetMins = args[3];
                string endOffsetMins = args[4];
                fileSegment = new FileSegment
                { 
                    OriginalFile = fiSourceRecording,
                    SegmentStartOffset = TimeSpan.FromMinutes(double.Parse(startOffsetMins)),
                    SegmentEndOffset   = TimeSpan.FromMinutes(double.Parse(endOffsetMins)),
                };
            }

            //5. initialise the analyser
            string analysisIdentifier = configDict[Keys.ANALYSIS_NAME];
            var analysers = AnalysisCoordinator.GetAnalysers(typeof(MainEntry).Assembly);
            IAnalyser analyser = analysers.FirstOrDefault(a => a.Identifier == analysisIdentifier);
            if (analyser == null)
            {
                LoggedConsole.WriteLine("###################################################\n");
                LoggedConsole.WriteLine("Analysis failed. UNKNOWN Analyser: <{0}>", analysisIdentifier);
                LoggedConsole.WriteLine("Available analysers are:");
                foreach (IAnalyser anal in analysers)
                {
                    LoggedConsole.WriteLine("\t  " + anal.Identifier);
                }
                LoggedConsole.WriteLine("###################################################\n");
            }


            //test conversion of events file to indices file
            //if (false)
            //{
            //    string ipPath = @"C:\SensorNetworks\Output\Test1\Towsey.MultiAnalyser\DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h_Towsey.MultiAnalyser.Events.csv";
            //    string opPath = @"C:\SensorNetworks\Output\Test1\Towsey.MultiAnalyser\DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h_Towsey.MultiAnalyser.Indices.csv";
            //    DataTable dt = CsvTools.ReadCSVToTable(ipPath, true);
            //    TimeSpan unitTime = new TimeSpan(0, 0, 60);
            //    TimeSpan source   = new TimeSpan(0, 59, 47);
            //    double dummy = 0.0;
            //    DataTable dt1 = analyser.ConvertEvents2Indices(dt, unitTime, source, dummy);
            //    CsvTools.DataTable2CSV(dt1, opPath);
            //    LoggedConsole.WriteLine("FINISHED");
            //    Console.ReadLine();
            //}

            // 6. initialise the analysis settings object
            var analysisSettings = analyser.DefaultSettings;
            analysisSettings.SetUserConfiguration(fiConfig, configDict, diOP, Keys.SEGMENT_DURATION, Keys.SEGMENT_OVERLAP);

            // 7. ####################################### DO THE ANALYSIS ###################################
            var analyserResults = analysisCoordinator.Run(new[] { fileSegment }, analyser, analysisSettings);
            //    ###########################################################################################

            // 8. PROCESS THE RESULTS
            if (analyserResults == null)
            {
                LoggedConsole.WriteLine("###################################################\n");
                LoggedConsole.WriteLine("The Analysis Run Coordinator has returned a null result.");
                LoggedConsole.WriteLine("###################################################\n");
                throw new AnalysisOptionDevilException();
            }

            // write the results to file
            DataTable datatable = ResultsTools.MergeResultsIntoSingleDataTable(analyserResults);
            if ((datatable == null) || (datatable.Rows.Count == 0))
            {
                LoggedConsole.WriteLine("###################################################\n");
                LoggedConsole.WriteLine("The Analysis Run Coordinator has returned a null data table or one with zero rows.");
                LoggedConsole.WriteLine("###################################################\n");
                throw new AnalysisOptionDevilException();
            }

            //get the duration of the original source audio file - need this to convert Events datatable to Indices Datatable
            var audioUtility = new MasterAudioUtility();
            var mimeType = MediaTypes.GetMediaType(fiSourceRecording.Extension);
            var sourceInfo = audioUtility.Info(fiSourceRecording);
            
            var op1 = ResultsTools.GetEventsAndIndicesDataTables(datatable, analyser, sourceInfo.Duration.Value);
            var eventsDatatable  = op1.Item1;
            var indicesDatatable = op1.Item2;
            int eventsCount = 0;
            if (eventsDatatable != null) eventsCount = eventsDatatable.Rows.Count;
            int indicesCount = 0;
            if (indicesDatatable != null) indicesCount = indicesDatatable.Rows.Count;
            var opdir = analyserResults.ElementAt(0).SettingsUsed.AnalysisRunDirectory;
            string fName = Path.GetFileNameWithoutExtension(fiSourceRecording.Name) + "_" + analyser.Identifier;
            var op2 = ResultsTools.SaveEventsAndIndicesDataTables(eventsDatatable, indicesDatatable, fName, opdir.FullName);

            var fiEventsCSV  = op2.Item1;
            var fiIndicesCSV = op2.Item2;

            LoggedConsole.WriteLine("\n###################################################");
            LoggedConsole.WriteLine("Finished processing " + fiSourceRecording.Name + ".");
            //LoggedConsole.WriteLine("Output  to  directory: " + diOP.FullName);
            LoggedConsole.WriteLine("\n");

            if (fiEventsCSV == null)
            {
                LoggedConsole.WriteLine("An Events CSV file was NOT returned.");
            }
            else
            {
                LoggedConsole.WriteLine("EVENTS CSV file(s) = " + fiEventsCSV.Name);
                LoggedConsole.WriteLine("\tNumber of events = " + eventsCount);
            }
            LoggedConsole.WriteLine("\n");
            if (fiIndicesCSV == null)
            {
                LoggedConsole.WriteLine("An Indices CSV file was NOT returned.");
            }
            else
            {
                LoggedConsole.WriteLine("INDICES CSV file(s) = " + fiIndicesCSV.Name);
                LoggedConsole.WriteLine("\tNumber of indices = " + indicesCount);
            }
            LoggedConsole.WriteLine("\n##### FINISHED FILE ###################################################\n");
        } // Main(string[] args)



        public static void CheckArguments(string[] args)
        {
            if ((args.Length != 3) && (args.Length != 5))
            {
                LoggedConsole.WriteLine("\nINCORRECT COMMAND LINE.");
                LoggedConsole.WriteLine("\nTHE COMMAND LINE HAS {0} ARGUMENTS", args.Length);
                foreach (string arg in args) LoggedConsole.WriteLine(arg + "  ");
                LoggedConsole.WriteLine("\nYOU REQUIRE 4 OR 6 COMMAND LINE ARGUMENTS\n");
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
            // GET FIRST THREE OBLIGATORY COMMAND LINE ARGUMENTS
            string recordingPath = args[0];
            string configPath = args[1];
            string outputDir = args[2];
            DirectoryInfo diSource = new DirectoryInfo(Path.GetDirectoryName(recordingPath));
            if (!diSource.Exists)
            {
                LoggedConsole.WriteLine("Source directory does not exist: " + diSource.FullName);

                throw new AnalysisOptionInvalidPathsException();
            }

            FileInfo fiSource = new FileInfo(recordingPath);
            if (!fiSource.Exists)
            {
                LoggedConsole.WriteLine("Source directory exists: " + diSource.FullName);
                LoggedConsole.WriteLine("\t but the source file does not exist: " + recordingPath);

                throw new AnalysisOptionInvalidPathsException();
            }

            FileInfo fiConfig = new FileInfo(configPath);
            if (!fiConfig.Exists)
            {
                LoggedConsole.WriteLine("Config file does not exist: " + fiConfig.FullName);

                throw new AnalysisOptionInvalidPathsException();
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
                    LoggedConsole.WriteLine("Output directory does not exist: " + diOP.FullName);

                    throw new AnalysisOptionInvalidPathsException();
                }
            }
        }


        public static void Usage()
        {
            LoggedConsole.WriteLine(
            @"USAGE:
            AnalysisPrograms.exe  audio2csv  audioPath  configPath  outputDirectory  startOffset  endOffset
            where:
            audio2csv:-       a literal string indicating the analysis of a long recording. Analysis type must be set in config file.
            audioPath:-       (string) Path of the audio file to be processed.
            configPath:-      (string) Path of the analysis configuration file.
            outputDirectory:- (string) Path of the output directory in which to store .csv result files.
            THE ABOVE FOUR ARGUMENTS ARE OBLIGATORY. 
            THE NEXT TWO ARGUMENTS ARE OPTIONAL:
            startOffset:      (integer) The start (minutes) of that portion of the file to be analysed.
            endOffset:        (integer) The end   (minutes) of that portion of the file to be analysed.
            IF THE LAST TWO ARGUMENTS ARE NOT INCLUDED, THE ENTIRE FILE IS ANALYSED.
            ");
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
