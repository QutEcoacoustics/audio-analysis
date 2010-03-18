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
        private static readonly int NUM_ARGS_REQUIRED = 7;

        internal static void Run(string[] args)
        {
            try
            {
                bool isValid = Validate(args);

                if (isValid)
                {
                    IEnumerable<ProcessorResultTag> results = null;
                    var resultsFile = new FileInfo(Path.Combine(args[2], args[4]));
                    var finishedFile = new FileInfo(Path.Combine(args[2], args[5]));
                    var errorFile = new FileInfo(Path.Combine(args[2], args[6]));
                    var finishedMessages = new StringBuilder();
                    var errorMessages = new StringBuilder();


                    try
                    {
                        results = RunAnalysis(args[0], args[1], args[2], args[3]);
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
                else
                {
                    PrintUsage();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error: " + ex.ToString());
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
            Console.WriteLine("\t4. Name of settings file.");
            Console.WriteLine("\t5. Name of audio file.");
            Console.WriteLine("\t6. Name of results output file.");
            Console.WriteLine("\t7. Name of finished output file.");
            Console.WriteLine("\t8. Name of error output file.");
            Console.WriteLine();
        }

        private static bool Validate(string[] args)
        {
            Console.WriteLine("Given " + args.Length + " arguments: " + string.Join(" , ", args));

            bool isValid = true;

            // validate
            if (args.Length != NUM_ARGS_REQUIRED)
            {
                Console.WriteLine("Inncorrect number of arguments. Given " + args.Length + ", require 'processing' " + NUM_ARGS_REQUIRED + ".");
                isValid = false;
            }

            if (!Directory.Exists(args[1]))
            {
                Console.WriteLine("Directory does not exist: " + args[1]);
                isValid = false;
            }

            if (!File.Exists(args[2]))
            {
                Console.WriteLine("File does not exist: " + args[1]);
                isValid = false;
            }

            if (!File.Exists(args[3]))
            {
                Console.WriteLine("File does not exist: " + args[1]);
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Run analysis and get results.
        /// </summary>
        /// <param name="analysisType"></param>
        /// <param name="runDirectory"></param>
        /// <param name="settingsFileName"></param>
        /// <param name="audioFileName"></param>
        /// <returns></returns>
        private static IEnumerable<ProcessorResultTag> RunAnalysis(string analysisType, string runDirectory, string settingsFileName, string audioFileName)
        {
            IEnumerable<ProcessorResultTag> results = null;

            DirectoryInfo runDir = new DirectoryInfo(runDirectory);
            var settingsFile = new FileInfo(Path.Combine(runDir.FullName, settingsFileName));
            var audioFile = new FileInfo(Path.Combine(runDir.FullName, audioFileName));

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
                    Console.WriteLine("Unrecognised analysis type.");
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
