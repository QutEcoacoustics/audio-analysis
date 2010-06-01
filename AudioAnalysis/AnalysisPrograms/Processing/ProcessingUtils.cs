// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessingUtils.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Processing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using AudioAnalysisTools;

    using QutSensors.Shared;

    /// <summary>
    /// The processing utils.
    /// </summary>
    public static class ProcessingUtils
    {
        // NOTE: if you change these file names, they also need to be changed in QutSensors.Processor.Manager

        // input files
        private const string SettingsFileName = "processing_input_settings.txt";
        private const string AudioFileName = "processing_input_audio.wav";

        // analysis program file names
        private const string ProgramOutputFinishedFileName = "output_finishedmessage.txt";
        private const string ProgramOutputResultsFileName = "output_results.xml";
        private const string ProgramOutputErrorFileName = "output_error.txt";

        /// <summary>
        /// Uses the information in Michael Towsey's AcousticEvent class to initialise an instance of the ProcessorResultTag class.
        /// </summary>
        /// <param name="ae">
        /// instance of the AcousticEvent class.
        /// </param>
        /// <param name="normalisedScore">
        /// The normalised Score.
        /// </param>
        /// <returns>Processing result tag.
        /// </returns>
        public static ProcessorResultTag GetProcessorResultTag(AcousticEvent ae, ResultProperty normalisedScore)
        {
            var prt = new ProcessorResultTag
            {
                NormalisedScore = normalisedScore,
                StartTime = (int?)Math.Round(ae.StartTime * 1000), // convert from double seconds to int milliseconds
                EndTime = (int?)Math.Round(ae.EndTime * 1000), // convert from double seconds to int milliseconds
                MinFrequency = ae.MinFreq,
                MaxFrequency = ae.MaxFreq,

                // TODO: store more info about AcousticEvents?
                ExtraDetail = ae.ResultPropertyList != null ? ae.ResultPropertyList.ToList() : null
            };

            return prt;
        }

        /// <summary>
        /// Run an analysis.
        /// </summary>
        /// <param name="args">
        /// Arguments for analysis.
        /// </param>
        internal static void Run(string[] args)
        {
            try
            {
                var analysisType = args[0];
                var rundir = args[1];
                var resourceFileFullPath = args.Count() > 2 ? args[2] : string.Empty;

                var finishedFile = new FileInfo(Path.Combine(rundir, ProgramOutputFinishedFileName));
                var errorFile = new FileInfo(Path.Combine(rundir, ProgramOutputErrorFileName));
                var resultsFile = new FileInfo(Path.Combine(rundir, ProgramOutputResultsFileName));

                var finishedMessages = new StringBuilder();
                var errorMessages = new StringBuilder();

                var isValid = Validate(args);

                if (isValid)
                {
                    IEnumerable<ProcessorResultTag> results = null;

                    try
                    {
                        results = RunAnalysis(analysisType, rundir, resourceFileFullPath);
                    }
                    catch (Exception ex)
                    {
                        errorMessages.AppendLine("Analysis-Run--Error: " + ex);
                    }

                    try
                    {
                        if (results != null && results.Count() > 0)
                        {
                            ProcessorResultTag.Write(results.ToList(), resultsFile.FullName);
                            finishedMessages.AppendLine(
                                "Analysis-Run--Results: " + results.Count() + " results available.");
                        }
                        else
                        {
                            finishedMessages.AppendLine("Analysis-Run--Results: No results available");
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMessages.AppendLine("Analysis-Run--Write-Results-Error: " + ex);
                    }
                }
                else
                {
                    PrintUsage();
                    errorMessages.AppendLine("Analysis-Run--Argument-Invalid-Error.");
                }

                // write messages
                var exitCode = 0;
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
                Console.Error.WriteLine("Error: " + ex);
            }
        }

        /// <summary>
        /// The print usage.
        /// </summary>
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

        /// <summary>
        /// Validate input parameters.
        /// </summary>
        /// <param name="args">Parameters given to program.</param>
        /// <returns>True if validation succeeded, otherwise false.</returns>
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

            if (!File.Exists(Path.Combine(args[1], SettingsFileName)))
            {
                Console.Error.WriteLine("Settings file does not exist: " + SettingsFileName);
                return false;
            }

            if (!File.Exists(Path.Combine(args[1], AudioFileName)))
            {
                Console.Error.WriteLine("Audio file does not exist: " + AudioFileName);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Run analysis and get results.
        /// </summary>
        /// <param name="analysisType">
        /// String id of type of analysis to run (see method for options).
        /// </param>
        /// <param name="runDirectory">
        /// Working directory.
        /// </param>
        /// <param name="resourceFileFullPath">Absolute path to resource file.</param>
        /// <returns>
        /// Processing Results.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// resourceFile.
        /// </exception>
        /// <exception cref="InvalidOperationException">Invaild resource file path.</exception>
        private static IEnumerable<ProcessorResultTag> RunAnalysis(string analysisType, string runDirectory, string resourceFileFullPath)
        {
            IEnumerable<ProcessorResultTag> results = null;

            var runDir = new DirectoryInfo(runDirectory);
            var settingsFile = new FileInfo(Path.Combine(runDir.FullName, SettingsFileName));
            var audioFile = new FileInfo(Path.Combine(runDir.FullName, AudioFileName));
            var resourceFile = new FileInfo(resourceFileFullPath);

            Console.WriteLine("Analysis Type: " + analysisType);

            // select analysis from name
            switch (analysisType)
            {
                // utilities
                case "aed": // acoustic event detection
                    results = ProcessingTypes.RunAed(settingsFile, audioFile);
                    break;
                case "snr": // signal to noise ratio
                    results = ProcessingTypes.RunSnr(settingsFile, audioFile);
                    break;
                case "segmentation": // segmentation
                    results = ProcessingTypes.RunSegmentation(settingsFile, audioFile);
                    break;

                // recognisers
                case "od": // Oscillation Recogniser
                    results = ProcessingTypes.RunOd(settingsFile, audioFile);
                    break;
                case "hd": // Harmonic Recogniser
                    results = ProcessingTypes.RunHd(settingsFile, audioFile);
                    break;
                case "epr": // event pattern recognition - groundparrot (in progress)
                    results = ProcessingTypes.RunEpr(settingsFile, audioFile);
                    break;
                case "spt": // spectral peak tracking (in progress)
                    results = ProcessingTypes.RunSpt(settingsFile, audioFile);
                    break;

                // require extra resources
                case "htk": // run HTK template over a recording
                    if (resourceFile.Exists)
                    {
                        results = ProcessingTypes.RunHtk(resourceFile, runDir, audioFile);
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid resource file path: " + resourceFile);
                    }

                    break;

                case "mfcc_od": // MFCCs and OD for calls haveing oscillating character
                    if (resourceFile.Exists)
                    {
                        results = ProcessingTypes.RunMfccOd(resourceFile, runDir, audioFile);
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid resource file path: " + resourceFile);
                    }

                    break;
                default:
                    Console.Error.WriteLine("Unrecognised analysis type.");
                    break;
            }

            return results;
        }
    }
}