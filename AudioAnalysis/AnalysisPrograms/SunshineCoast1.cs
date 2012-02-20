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
        // sunshinecoast1 "C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav"     "C:\SensorNetworks\WavFiles\SunshineCoast\acousticIndices_Params.txt"



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
            Log.WriteIfVerbose("# Output dir: " + outputDir);


            //READ PARAMETER VALUES FROM INI FILE
            RichnessIndices2.Parameters parameters = ReadIniFile(iniPath);

            // Get the file time duration
            SpecificWavAudioUtility audioUtility = SpecificWavAudioUtility.Create();
            var fileInfo = new FileInfo(recordingPath);
            var mimeType = QutSensors.Shared.MediaTypes.GetMediaType(fileInfo.Extension);
            //var dateInfo = fileInfo.CreationTime;
            var duration = audioUtility.Duration(fileInfo, mimeType);
            Log.WriteIfVerbose("# Recording - filename: " + Path.GetFileName(recordingPath));
            Log.WriteIfVerbose("# Recording - datetime: {0}    {1}", fileInfo.CreationTime.ToLongDateString(), fileInfo.CreationTime.ToLongTimeString());
            Log.WriteIfVerbose("# Recording - duration: {0}hr:{1}min:{2}s:{3}ms", duration.Hours, duration.Minutes, duration.Seconds, duration.Milliseconds);

            //SET UP THE REPORT FILE
            string reportfileName = outputDir + "AcousticIndices_" + Path.GetFileNameWithoutExtension(recordingPath);
            if (parameters.reportFormat.Equals("CSV")) reportfileName += ".csv";
            else reportfileName += ".txt";
            WriteHeaderToReportFile(reportfileName, parameters.reportFormat);

            // LOOP THROUGH THE FILE
            //initialse counters
            int fileCount = 0;
            double elapsedTime = 0.0;
            DateTime tStart = DateTime.Now;


            double startMinutes = 0.0;
            int overlap = (int)Math.Floor(parameters.segmentOverlap * 1000);
            for (int s = 0; s < Int32.MaxValue; s++)
            {

                startMinutes += parameters.segmentDuration;
                Console.WriteLine("\n\n");
                Log.WriteLine("## SAMPLE {0}:-   starts@ {1} minutes", s, startMinutes);

                AudioRecording recording = GetSegmentFromAudioRecording(recordingPath, startMinutes, parameters.segmentDuration, overlap, outputDir);
                string segmentDuration = DataTools.Time_ConvertSecs2Mins(recording.GetWavReader().Time.TotalSeconds);
                //Log.WriteLine("Signal Duration: " + segmentDuration);
                int sampleCount = recording.GetWavReader().Samples.Length;
                int minLength = 3 * parameters.frameLength;
                if (sampleCount <= minLength)
                {
                    Log.WriteLine("# WARNING: Recording is less than {0} samples (three frames) long. Will ignore.", sampleCount);
                    //Console.ReadLine();
                    //System.Environment.Exit(666);
                    break;
                }

                //#############################################################################################################################################
                //iii: EXTRACT INDICES   Default windowDuration = 128 samples @ 22050Hz = 5.805ms, @ 11025kHz = 11.61ms.
                var results = RichnessIndices2.ExtractIndices(recording);

                elapsedTime += recording.GetWavReader().Time.TotalMinutes;
                RichnessIndices2.Indices2 indices = results.Item1;


                WriteIndicesToReportFile();

                var values = String.Format(RichnessIndices2._FORMAT_STRING,
                    fileCount, elapsedTime, recording.FileName, indices.avSig_dB, indices.snr, indices.bgNoise,
                    indices.activity, indices.segmentCount, indices.avSegmentDuration, indices.spectralCover, indices.entropyOfAmpl,
                    indices.entropyOfPeakFreqDistr, indices.entropyOfAvSpectrum, indices.entropyOfVarianceSpectra1,
                    indices.clusterCount, indices.avClusterDuration);
                FileTools.Append2TextFile(opPath, values);
                //#############################################################################################################################################

                recording.Dispose();
                var sonogram = results.Item1;
                var hits = results.Item2;
                var scores = results.Item3;
                //var oscRates = results.Item4;
                var predictedEvents = results.Item5;
                Log.WriteLine("# Event count = " + predictedEvents.Count());

                //write events to results file. 
                double sigDuration = sonogram.Duration.TotalSeconds;
                string fname = Path.GetFileName(recordingPath);
                int count = predictedEvents.Count;

                StringBuilder sb = WriteEvents(startMinutes, sigDuration, count, predictedEvents, parameters.reportFormat);
                if (sb.Length > 1)
                {
                    sb.Remove(sb.Length - 2, 2); //remove the last endLine to prevent line gaps.
                    FileTools.Append2TextFile(reportfileName, sb.ToString());
                }


                //draw images of sonograms
                string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + "_" + startMinutes.ToString() + "min.png";
                if ((parameters.DRAW_SONOGRAMS == 2) || ((parameters.DRAW_SONOGRAMS == 1) && (predictedEvents.Count > 0)))
                {
                    DrawSonogram(sonogram, imagePath, hits, scores, null, predictedEvents, parameters.eventThreshold);
                }

                startMinutes += parameters.segmentDuration;
            } //end of for loop

            Log.WriteLine("# Finished recording:- " + Path.GetFileName(recordingPath));
            Console.ReadLine();
        } //Dev()




        public static void WriteHeaderToReportFile(string reportfileName, string parmasFile_Separator)
        {
            string reportSeparator = "\t";
            if (parmasFile_Separator.Equals("CSV")) reportSeparator = ",";
            const string[] _HEADER = {"count", "minutes", "FileName", "avAmp", "snr-dB", "bg-dB", "activity", "segCount", "avSegLngth", "spCover", "1-H[ampl]", 
                                      "H[peakFreq]", "H[avSpectrum]", "H1[diffSpectra]", "#clusters", "avClustLngth"};
            string _FORMAT_STRING = "{1}{0}{2:f3}{0}{3}{0}{4:f2}{0}{5:f2}{0}{6:f2}{0}{7:f2}{0}{8}{0}{9:f2}{0}{10:f4}{0}{11:f4}{0}{12:f4}{0}{13:f4}{0}{14:f4}{0}{15}{0}{16}";
            string line = String.Format(_FORMAT_STRING, reportSeparator, _HEADER);
            FileTools.WriteTextFile(reportfileName, line);
        }

        public static void WriteIndicesToReportFile(string opPath, string parmasFile_Separator, int fileCount,  elapsedTime, string fileName,
                                                    RichnessIndices2.Indices2 indices)
        {

            string reportSeparator = "\t";
            if (parmasFile_Separator.Equals("CSV")) reportSeparator = ",";

            string _FORMAT_STRING = "{1}{0}{2:f3}{0}{3}{0}{4:f2}{0}{5:f2}{0}{6:f2}{0}{7:f2}{0}{8}{0}{9:f2}{0}{10:f4}{0}{11:f4}{0}{12:f4}{0}{13:f4}{0}{14:f4}{0}{15}{0}{16}";

            //string duration = DataTools.Time_ConvertSecs2Mins(segmentDuration);
            string line = String.Format(_FORMAT_STRING, reportSeparator,
                                       fileCount, elapsedTime, fileName, indices.avSig_dB, indices.snr, indices.bgNoise,
                                       indices.activity, indices.segmentCount, indices.avSegmentDuration, indices.spectralCover, indices.entropyOfAmpl,
                                       indices.entropyOfPeakFreqDistr, indices.entropyOfAvSpectrum, indices.entropyOfVarianceSpectra1,
                                       indices.clusterCount, indices.avClusterDuration);
            FileTools.Append2TextFile(opPath, line);
        }




        public static AudioRecording GetAudioRecording(string recordingPath)
        {
            //OLD CODE
            //AudioRecording recording = new AudioRecording(recordingPath);
            //if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();

            SpecificWavAudioUtility audioUtility = SpecificWavAudioUtility.Create();
            audioUtility.SoxAudioUtility.ResampleQuality = SoxAudioUtility.SoxResampleQuality.VeryHigh; //Options: Low, Medium, High, VeryHigh 
            audioUtility.SoxAudioUtility.TargetSampleRateHz = 17640;
            audioUtility.SoxAudioUtility.ReduceToMono = true;
            audioUtility.SoxAudioUtility.UseSteepFilter = true;
            //##### ######  IMPORTANT NOTE 1 :: THE EFFECT OF THE ABOVE RESAMPLING PARAMETERS IS TO SET NYQUIST = SAMPLERATE / 2 Hz.
            //##### ######  IMPORTANT NOTE 2 :: THE RESULTING SIGNAL ARRAY VARIES SLIGHTLY FOR EVERY LOADING - NOT SURE WHY? A STOCHASTOIC COMPONENT TO FILTER? 
            //##### ######                               BUT IT HAS THE EFFECT THAT STATISTICS VARY SLIGHTLY FOR EACH RUN OVER THE SAME FILE.
            audioUtility.LogLevel = LogType.Error;  //Options: None, Fatal, Error, Debug, 
            AudioRecording recording = new AudioRecording(recordingPath, audioUtility);

            return recording;
        }

        public static AudioRecording GetSegmentFromAudioRecording(string recordingPath, double startMinutes, double durationMinutes, int overlap, string opDir)
        {
            SpecificWavAudioUtility audioUtility = SpecificWavAudioUtility.Create();
            audioUtility.SoxAudioUtility.ResampleQuality = SoxAudioUtility.SoxResampleQuality.VeryHigh; //Options: Low, Medium, High, VeryHigh 
            audioUtility.SoxAudioUtility.TargetSampleRateHz = 17640;
            audioUtility.SoxAudioUtility.ReduceToMono = true;
            audioUtility.SoxAudioUtility.UseSteepFilter = true;
            //##### ######  IMPORTANT NOTE 1 :: THE EFFECT OF THE ABOVE RESAMPLING PARAMETERS IS TO SET NYQUIST = SAMPLERATE / 2 Hz.
            //##### ######  IMPORTANT NOTE 2 :: THE RESULTING SIGNAL ARRAY VARIES SLIGHTLY FOR EVERY LOADING - NOT SURE WHY? A STOCHASTOIC COMPONENT TO FILTER? 
            //##### ######                               BUT IT HAS THE EFFECT THAT STATISTICS VARY SLIGHTLY FOR EACH RUN OVER THE SAME FILE.
            audioUtility.LogLevel = LogType.Error;  //Options: None, Fatal, Error, Debug, 

            FileInfo inFile = new FileInfo(recordingPath);
            //FileInfo outFile = new FileInfo(@"C:\SensorNetworks\WavFiles\Kiwi\Samples\test.wav");
            FileInfo outFile = new FileInfo(opDir + @"temp.wav");
            int startMilliseconds = (int)(startMinutes * 60000);
            int endMilliseconds = startMilliseconds + (int)(durationMinutes * 60000) + overlap;

            SpecificWavAudioUtility.GetSingleSegment(audioUtility, inFile, outFile, startMilliseconds, endMilliseconds);
            AudioRecording recording = new AudioRecording(outFile.FullName, audioUtility);

            return recording;
        }


        public static RichnessIndices2.Parameters ReadIniFile(string iniPath)
        {
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            Dictionary<string, string>.KeyCollection keys = dict.Keys;

            RichnessIndices2.Parameters paramaters; // st
            paramaters.segmentDuration = Double.Parse(dict[RichnessIndices2.key_SEGMENT_DURATION]);
            paramaters.segmentOverlap  = Double.Parse(dict[RichnessIndices2.key_SEGMENT_OVERLAP]);
            paramaters.minHzMale       = Int32.Parse(dict[RichnessIndices2.key_MIN_HZ_MALE]);
            paramaters.maxHzMale       = Int32.Parse(dict[RichnessIndices2.key_MAX_HZ_MALE]);
            paramaters.minHzFemale = Int32.Parse(dict[RichnessIndices2.key_MIN_HZ_FEMALE]);
            paramaters.maxHzFemale = Int32.Parse(dict[RichnessIndices2.key_MAX_HZ_FEMALE]);
            paramaters.frameLength = Int32.Parse(dict[RichnessIndices2.key_FRAME_LENGTH]);
            paramaters.frameOverlap = Double.Parse(dict[RichnessIndices2.key_FRAME_OVERLAP]);
            paramaters.dctDuration = Double.Parse(dict[RichnessIndices2.key_DCT_DURATION]);        //duration of DCT in seconds 
            paramaters.dctThreshold = Double.Parse(dict[RichnessIndices2.key_DCT_THRESHOLD]);      //minimum acceptable value of a DCT coefficient
            paramaters.minPeriodicity = Double.Parse(dict[RichnessIndices2.key_MIN_PERIODICITY]);  //ignore oscillations with period below this threshold
            paramaters.maxPeriodicity = Double.Parse(dict[RichnessIndices2.key_MAX_PERIODICITY]);  //ignore oscillations with period above this threshold
            paramaters.minDuration = Double.Parse(dict[RichnessIndices2.key_MIN_DURATION]);        //min duration of event in seconds 
            paramaters.maxDuration = Double.Parse(dict[RichnessIndices2.key_MAX_DURATION]);        //max duration of event in seconds 
            paramaters.eventThreshold = Double.Parse(dict[RichnessIndices2.key_EVENT_THRESHOLD]);  //min score for an acceptable event
            paramaters.DRAW_SONOGRAMS = Int32.Parse(dict[RichnessIndices2.key_DRAW_SONOGRAMS]);    //options to draw sonogram
            paramaters.reportFormat = dict[RichnessIndices2.key_REPORT_FORMAT];                  //options are TAB or COMMA separator 

            Log.WriteIfVerbose("# PARAMETER SETTINGS:");
            Log.WriteIfVerbose("Segment size: Duration = {0} minutes;  Overlap = {1} seconds.", paramaters.segmentDuration, paramaters.segmentOverlap);
            Log.WriteIfVerbose("Male   Freq band: {0} Hz - {1} Hz.)", paramaters.minHzMale, paramaters.maxHzMale);
            Log.WriteIfVerbose("Female Freq band: {0} Hz - {1} Hz.)", paramaters.minHzFemale, paramaters.maxHzFemale);
            Log.WriteIfVerbose("Periodicity bounds: {0:f1}sec - {1:f1}sec", paramaters.minPeriodicity, paramaters.maxPeriodicity);
            Log.WriteIfVerbose("minAmplitude = " + paramaters.dctThreshold);
            Log.WriteIfVerbose("Duration bounds: " + paramaters.minDuration + " - " + paramaters.maxDuration + " seconds");
            Log.WriteIfVerbose("####################################################################################");
            //Log.WriteIfVerbose("Male   Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHzMale, maxHzMale, binCount_male);
            //Log.WriteIfVerbose("Female Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHzFemale, maxHzFemale, binCount_female);
            //Log.WriteIfVerbose("DctDuration=" + dctDuration + "sec.  (# frames=" + (int)Math.Round(dctDuration * sonogram.FramesPerSecond) + ")");
            //Log.WriteIfVerbose("Score threshold for oscil events=" + eventThreshold);
            return paramaters;
        }




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
            Console.WriteLine("KiwiDetect.exe recordingPath iniPath outputFileName");
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
