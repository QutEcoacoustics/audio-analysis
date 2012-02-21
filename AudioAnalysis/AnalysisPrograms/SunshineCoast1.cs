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



        public static void Dev(string[] args)
        {
            string title = "# SOFTWARE TO EXTRACT ACOUSTIC INDICES FROM SUNSHINE COAST DATA";
            string date = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            //GET COMMAND LINE ARGUMENTS
            Log.Verbosity = 1;
            CheckArguments(args);
            string recordingPath = args[0];
            string iniPath = args[1];
            string outputDir = Path.GetDirectoryName(iniPath) + "\\"; //output directory is the one in which ini file is located.


            //READ PARAMETER VALUES FROM INI FILE
            Log.WriteIfVerbose("  ");
            AcousticIndices.Parameters parameters = AcousticIndices.ReadIniFile(iniPath, Log.Verbosity);
            Log.WriteIfVerbose("  ");

            // Set up the file and get info
            SpecificWavAudioUtility audioUtility = SpecificWavAudioUtility.Create();
            var fileInfo = new FileInfo(recordingPath);
            var mimeType = QutSensors.Shared.MediaTypes.GetMediaType(fileInfo.Extension);
            //var dateInfo = fileInfo.CreationTime;
            var duration = audioUtility.Duration(fileInfo, mimeType);
            Log.WriteIfVerbose("# Recording - filename: " + Path.GetFileName(recordingPath));
            Log.WriteIfVerbose("# Recording - datetime: {0}    {1}", fileInfo.CreationTime.ToLongDateString(), fileInfo.CreationTime.ToLongTimeString());
            Log.WriteIfVerbose("# Recording - duration: {0}hr:{1}min:{2}s:{3}ms", duration.Hours, duration.Minutes, duration.Seconds, duration.Milliseconds);
            Log.WriteIfVerbose("# Output to  directory: " + outputDir);


            //SET UP THE REPORT FILE
            string reportfileName = outputDir + "AcousticIndices_" + Path.GetFileNameWithoutExtension(recordingPath);
            if (parameters.reportFormat.Equals("CSV")) reportfileName += ".csv";
            else reportfileName += ".txt";
            AcousticIndices.WriteHeaderToReportFile(reportfileName, parameters.reportFormat);

            // LOOP THROUGH THE FILE
            //initialse counters
            DateTime tStart = DateTime.Now;
            Log.WriteLine(tStart);


            int overlap_ms = (int)Math.Floor(parameters.segmentOverlap * 1000);
            for (int s = 0; s < Int32.MaxValue; s++)
            {
                double startMinutes = s * parameters.segmentDuration;

                Console.WriteLine("\n\n");
                Log.WriteLine("## SAMPLE {0}:-   starts@ {1} minutes", s, startMinutes);
                int startMilliseconds = (int)(startMinutes * 60000);
                int endMilliseconds = startMilliseconds + (int)(parameters.segmentDuration * 60000) + overlap_ms;
                AudioRecording recording = AudioRecording.GetSegmentFromAudioRecording(recordingPath, startMilliseconds, endMilliseconds, parameters.resampleRate, outputDir);
                //string segmentDuration = DataTools.Time_ConvertSecs2Mins(recording.GetWavReader().Time.TotalSeconds);
                double segmentDuration = recording.GetWavReader().Time.TotalSeconds;
                int sampleCount = recording.GetWavReader().Samples.Length; //get recording length to determine if long enough
                int minLength = 3 * parameters.frameLength; //ignore recordings shorter than three frames
                if (sampleCount <= minLength)
                {
                    Log.WriteLine("# WARNING: Recording is only {0} samples long (i.e. less than three frames). Will ignore.", sampleCount);
                    //Console.ReadLine();
                    //System.Environment.Exit(666);
                    break;
                }
                //Log.WriteLine("Signal Duration: " + segmentDuration + "seconds");

                //#############################################################################################################################################
                //iii: EXTRACT INDICES   Default windowDuration = 128 samples @ 22050 Hz = 5.805ms, @ 11025 Hz = 11.61ms.
                //     EXTRACT INDICES   Default windowDuration = 256 samples @ 22050 Hz = 11.61ms, @ 11025 Hz = 23.22ms, @ 17640 Hz = 18.576ms.
                var results = AcousticIndices.ExtractIndices(recording, parameters.frameLength, parameters.lowFreqBound);

                AcousticIndices.Indices2 indices = results.Item1;
                AcousticIndices.WriteIndicesToReportFile(reportfileName, parameters.reportFormat, s, startMinutes, segmentDuration, indices);

                //#############################################################################################################################################

                recording.Dispose();            

                //draw images of sonograms
                string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + "_" + startMinutes.ToString() + "min.png";
                if ((parameters.DRAW_SONOGRAMS == 2) || (parameters.DRAW_SONOGRAMS == 1))
                {
                    //DrawSonogram(sonogram, imagePath, hits, scores, null, predictedEvents, parameters.eventThreshold);
                }

                startMinutes += parameters.segmentDuration;
            } //end of for loop

            Log.WriteLine("# Finished recording:- " + Path.GetFileName(recordingPath));

            AcousticIndices.VISUALIZE_CSV_DATA(reportfileName);


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
