using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;
using AudioTools.AudioUtlity;
using QutSensors.Shared.LogProviders;


namespace AnalysisPrograms
{
    class SunshineCoast1
    {
        // 3 hr test file  // sunshinecoast1 "C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav"     "C:\SensorNetworks\WavFiles\SunshineCoast\acousticIndices_Params.txt"
        //8 min test file  // sunshinecoast1 "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_CroppedAnd2.wav" "C:\SensorNetworks\WavFiles\SunshineCoast\acousticIndices_Params.txt"
        //SCC file site 4  // sunshinecoast1 "Y:\Sunshine Coast\Site4\DM420062.mp3" "C:\SensorNetworks\WavFiles\SunshineCoast\acousticIndices_Params.txt"
        //SCC file site 4  // sunshinecoast1 "\\hpc-fs.qut.edu.au\staging\availae\Sunshine Coast\Site4\DM420062.mp3" "C:\SensorNetworks\WavFiles\SunshineCoast\acousticIndices_Params.txt"

        public static void Dev(string[] args)
        {
            string title = "# SOFTWARE TO EXTRACT ACOUSTIC INDICES FROM SUNSHINE COAST DATA";
            string date = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            //GET COMMAND LINE ARGUMENTS
            Log.Verbosity = 1;
            CheckArguments(args);
            string sourceRecordingPath = args[0];
            string iniPath = args[1];
            string outputDir = Path.GetDirectoryName(iniPath) + "\\"; //output directory is the one in which ini file is located.

            string outputSegmentPath = Path.Combine(outputDir, @"temp.wav"); //path name of the temporary segment files extracted from long recording



            //READ PARAMETER VALUES FROM INI FILE
            Log.WriteIfVerbose("  ");
            AcousticIndices.Parameters parameters = AcousticIndices.ReadIniFile(iniPath, Log.Verbosity);
            Log.WriteIfVerbose("  ");

            // Set up the file and get info
            SpecificWavAudioUtility audioUtility = SpecificWavAudioUtility.Create();
            var fileInfo = new FileInfo(sourceRecordingPath);
            var mimeType = QutSensors.Shared.MediaTypes.GetMediaType(fileInfo.Extension);
            //var dateInfo = fileInfo.CreationTime;
            var duration = audioUtility.Duration(fileInfo, mimeType);
            double minCount = (duration.TotalMinutes); //convert length to minute chunks
            int segmentCount = (int)Math.Round(minCount / parameters.segmentDuration); //convert length to minute chunks
            Log.WriteIfVerbose("# Recording - filename: " + Path.GetFileName(sourceRecordingPath));
            Log.WriteIfVerbose("# Recording - datetime: {0}    {1}", fileInfo.CreationTime.ToLongDateString(), fileInfo.CreationTime.ToLongTimeString());
            Log.WriteIfVerbose("# Recording - duration: {0}hr:{1}min:{2}s:{3}ms", duration.Hours, duration.Minutes, duration.Seconds, duration.Milliseconds);
            Log.WriteIfVerbose("# Recording - duration: {0} minutes", duration.TotalMinutes);
            Log.WriteIfVerbose("# Recording - minutes: {0:f3}   segments: {1}", minCount, segmentCount);
            Log.WriteIfVerbose("# Output to  directory: " + outputDir);

            AcousticIndices.ScanRecording(sourceRecordingPath, outputDir, parameters);

            Log.WriteLine("# Finished extracting indices from source recording:- " + Path.GetFileName(sourceRecordingPath));

            //AcousticIndices.AddColumnOfWeightedIndicesToCSVFile(reportfileName, columnHeader, opFileName);
            //AcousticIndices.VISUALIZE_CSV_DATA(reportfileName);


            Log.WriteLine("# Finished visualization and  EVERYTHING");
            Console.ReadLine();
        } //Dev()



        public static void CheckArguments(string[] args)
        {
            if (args.Length != 2)
            {
                Log.WriteLine("NUMBER OF COMMAND LINE ARGUMENTS = {0}", args.Length);
                foreach (string arg in args) Log.WriteLine(arg + "  ");
                Log.WriteLine("YOU REQUIRE {0} COMMAND LINE ARGUMENTS\n", 2);
                Usage();
            }
            CheckPaths(args);
        }

        /// <summary>
        /// this method checks for the existence of the two files whose paths are expected as first two arguments of the command line.
        /// </summary>
        /// <param name="args"></param>
        public static void CheckPaths(string[] args)
        {
            string dir = Path.GetPathRoot(args[0]);
            if (!Directory.Exists(dir))
            {
                Console.WriteLine("Cannot find root <" + dir + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
            else Console.WriteLine("Root dir = <" + dir + ">");

            dir = Path.GetDirectoryName(args[0]);
            if (!Directory.Exists(dir))
            {
                Console.WriteLine("Cannot find directory <" + dir + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Cannot find recording file <" + args[0] + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
            if (!File.Exists(args[1]))
            {
                Console.WriteLine("Cannot find initialisation file: <" + args[1] + ">");
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
            Console.WriteLine("SunshineCoast1.exe recordingPath");
            Console.WriteLine("where:");
            Console.WriteLine("recordingFileName:-(string) The path of the audio file to be processed.");
            Console.WriteLine("iniPath:-          (string) The path of the ini file containing all required parameters.");
            Console.WriteLine();
            Console.WriteLine("NOTE: By default, the output dir is that containing the ini file.");
            Console.WriteLine("");
            Console.WriteLine("\nPress <ENTER> key to exit.");
            Console.ReadLine();
            System.Environment.Exit(1);
        }


    }
}
