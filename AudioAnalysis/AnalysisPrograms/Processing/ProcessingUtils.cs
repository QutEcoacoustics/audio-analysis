using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using QutSensors.Shared;
using System.Xml.Serialization;
using AudioAnalysisTools;

namespace AnalysisPrograms.Processing
{
    public static class ProcessingUtils
    {
        // input files
        private const string SETTINGS_FILE_NAME = "input_settings.txt";
        private const string AUDIO_FILE_NAME = "input_audio.wav";

        // standard out and error
        private const string STDERR_FILE_NAME = "output_stderr.txt";
        private const string STDOUT_FILE_NAME = "output_stdout.txt";

        // analysis program file names
        private const string PROGRAM_OUTPUT_FINISHED_FILE_NAME = "output_finishedmessage.txt";
        private const string PROGRAM_OUTPUT_RESULTS_FILE_NAME = "output_results.xml";
        private const string PROGRAM_OUTPUT_ERROR_FILE_NAME = "output_error.txt";

        internal static void Run(string[] args)
        {
            try
            {
                var rundir = args[1];
                var analysisType = args[0];

                var finishedFile = new FileInfo(Path.Combine(rundir, PROGRAM_OUTPUT_FINISHED_FILE_NAME));
                var errorFile = new FileInfo(Path.Combine(rundir, PROGRAM_OUTPUT_ERROR_FILE_NAME));
                var resultsFile = new FileInfo(Path.Combine(rundir, PROGRAM_OUTPUT_RESULTS_FILE_NAME));

                var finishedMessages = new StringBuilder();
                var errorMessages = new StringBuilder();


                bool isValid = Validate(args);

                if (isValid)
                {

                    IEnumerable<ProcessorResultTag> results = null;

                    try
                    {
                        results = RunAnalysis(analysisType, rundir);
                    }
                    catch (Exception ex)
                    {
                        errorMessages.AppendLine("Analysis-Run--Error: " + ex.ToString());
                    }


                    try
                    {
                        if (results != null && results.Count() > 0)
                        {
                            ProcessorResultTag.Write(results.ToList(), resultsFile.FullName);
                            finishedMessages.AppendLine("Analysis-Run--Results: " + results.Count() + " results available.");
                        }
                        else
                        {
                            finishedMessages.AppendLine("Analysis-Run--Results: No results available");
                        }

                    }
                    catch (Exception ex)
                    {
                        errorMessages.AppendLine("Analysis-Run--Write-Results-Error: " + ex.ToString());
                    }

                }
                else
                {
                    PrintUsage();
                    errorMessages.AppendLine("Analysis-Run--Argument-Invalid-Error.");
                }

                //write messages
                int exitCode = 0;
                if (errorMessages.Length > 0)
                {
                    exitCode = 1;
                    File.WriteAllText(errorFile.FullName, errorMessages.ToString());
                }

                finishedMessages.AppendLine("Analysis-Run--Exit-Code: " + exitCode);
                File.WriteAllText(finishedFile.FullName, finishedMessages.ToString());
                Environment.Exit(exitCode);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("Error: " + ex.ToString());
            }
        }

        internal static void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine("This console app is used to run analyses on a compute node in the processing cluster.");
            Console.WriteLine("It requires these parameters:");
            Console.WriteLine("\t1. 'processing' - indicates this is a processing run.");
            Console.WriteLine("\t2. Type of analysis to run.");
            Console.WriteLine("\t3. Path of run directory.");
            Console.WriteLine();
        }

        private static bool Validate(string[] args)
        {
            Console.WriteLine("Given " + args.Length + " arguments: " + string.Join(" , ", args));

            // validate
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Incorrect number of arguments given: " + args.Length + ".");
                return false;
            }

            if (!Directory.Exists(args[1]))
            {
                Console.Error.WriteLine("Run directory does not exist: " + args[1]);
                return false;
            }

            if (!File.Exists(Path.Combine(args[1], SETTINGS_FILE_NAME)))
            {
                Console.Error.WriteLine("Settings file does not exist: " + SETTINGS_FILE_NAME);
                return false;
            }

            if (!File.Exists(Path.Combine(args[1], AUDIO_FILE_NAME)))
            {
                Console.Error.WriteLine("Audio file does not exist: " + AUDIO_FILE_NAME);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Run analysis and get results.
        /// </summary>
        /// <param name="analysisType"></param>
        /// <param name="runDirectory"></param>
        /// <param name="settingsFileName"></param>
        /// <param name="audioFileName"></param>
        /// <returns></returns>
        private static IEnumerable<ProcessorResultTag> RunAnalysis(string analysisType, string runDirectory)
        {
            IEnumerable<ProcessorResultTag> results = null;

            DirectoryInfo runDir = new DirectoryInfo(runDirectory);
            var settingsFile = new FileInfo(Path.Combine(runDir.FullName, SETTINGS_FILE_NAME));
            var audioFile = new FileInfo(Path.Combine(runDir.FullName, AUDIO_FILE_NAME));

            Console.WriteLine("Analysis Type: " + analysisType);

            // select analysis from name
            switch (analysisType)
            {
                case "aed":  //acoustic event detection
                    results = ProcessingTypes.RunAED(settingsFile, audioFile);
                    break;
                case "od":   //Oscillation Recogniser
                    results = ProcessingTypes.RunOD(settingsFile, audioFile);
                    break;
                case "epr": //event pattern recognition - groundparrot
                    results = ProcessingTypes.RunEPR(audioFile);
                    break;
                case "snr":   //signal to noise ratio
                    // not used yet
                    Console.WriteLine("not used yet...");
                    break;
                case "htk":   //run an HTK template over a recording
                    // not used yet
                    Console.WriteLine("not used yet...");
                    break;
                case "spt": // spectral peak tracking
                    // not used yet
                    Console.WriteLine("not used yet...");
                    break;
                default:
                    Console.Error.WriteLine("Unrecognised analysis type.");
                    break;
            }

            return results;
        }

        /// <summary>
        /// Uses the information in Michael Towsey's AcousticEvent class to initialise an instance of the ProcessorResultTag class.
        /// </summary>
        /// <param name="ae">instance of the AcousticEvent class</param>
        /// <returns></returns>
        public static ProcessorResultTag GetProcessorResultTag(AcousticEvent ae, ResultProperty normalisedScore)
        {
            var prt = new ProcessorResultTag()
            {
                NormalisedScore = normalisedScore,
                StartTime = (int?)Math.Round(ae.StartTime * 1000),
                EndTime = (int?)Math.Round(ae.EndTime * 1000),
                MinFrequency = (int?)ae.MinFreq,
                MaxFrequency = (int?)ae.MaxFreq,
                ExtraDetail = ae.ResultPropertyList != null ?
                    ae.ResultPropertyList.ToList() //TODO: store more info about AcousticEvents?
                    : null
            };

            return prt;
        }

    }
}
