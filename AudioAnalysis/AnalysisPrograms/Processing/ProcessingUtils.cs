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

    using Acoustics.Shared;

    using AudioAnalysisTools;

    /// <summary>
    /// The processing utils.
    /// </summary>
    public static class ProcessingUtils
    {
        // NOTE: if you change these file names, they also need to be changed in QutSensors.ProcessorService

        // input files
        private const string SettingsFileName = "processing_input_settings.txt";
        private const string AudioFileName = "processing_input_audio.wav";

        private const string AnalysisStderrFileName = "output_stderr.txt";
        private const string AnalysisStdoutFileName = "output_stdout.txt";

        // analysis program file names
        private const string ProgramOutputFinishedFileName = "output_finishedmessage.txt";
        private const string ProgramOutputResultsFileName = "output_results.xml";
        private const string ProgramOutputErrorFileName = "output_error.txt";
        private const string AnalysisStartedTimeFileName = "output_started_time.txt";

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
                StartTime = (int?)Math.Round(ae.TimeStart * 1000), // convert from double seconds to int milliseconds
                EndTime = (int?)Math.Round(ae.TimeEnd * 1000), // convert from double seconds to int milliseconds
                MinFrequency = ae.MinFreq,
                MaxFrequency = ae.MaxFreq,

                // TODO: store more info about AcousticEvents?
                ExtraDetail = ae.ResultPropertyList != null ? ae.ResultPropertyList.ToList() : null,
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
                if (args.Length < 2 || args.Length > 3)
                {
                    Console.WriteLine("Requires <analysistype> <rundirectory> [<resourcefile>]");
                    return;
                }

                var analysisType = args[0].Trim().ToLower();
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
                            finishedMessages.AppendLine("Analysis-Run--Results: " + results.Count() + " results available.");
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
            Console.WriteLine("This console app is used to run analyses on a compute node.");
            Console.WriteLine("It takes these parameters:");
            Console.WriteLine("\t1. [Required] 'processing' - indicates this is a processing run.");
            Console.WriteLine("\t2. [Required] Type of analysis to run.");
            Console.WriteLine("\t3. [Required] Path of run directory.");
            Console.WriteLine("\t4. [Optional] Absolute path to resource file.");
            Console.WriteLine();
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
        internal static IEnumerable<ProcessorResultTag> RunAnalysis(string analysisType, string runDirectory, string resourceFileFullPath)
        {
            IEnumerable<ProcessorResultTag> results = null;

            var runDir = new DirectoryInfo(runDirectory);
            var settingsFile = new FileInfo(Path.Combine(runDir.FullName, SettingsFileName));
            var audioFile = new FileInfo(Path.Combine(runDir.FullName, AudioFileName));

            // make sure path is valid
            FileInfo resourceFile = null;
            if (!string.IsNullOrEmpty(resourceFileFullPath) && !string.IsNullOrEmpty(resourceFileFullPath.Trim()))
            {
                try
                {
                    resourceFile = new FileInfo(resourceFileFullPath);
                }
                catch
                {
                    // don't let resource file spoil our fun...
                }
            }

            // select analysis from name
            switch (analysisType)
            {
                // utilities
                case "segment": // segmentation
                    results = ProcessingTypes.RunSegment(settingsFile, audioFile);
                    break;
                case "snr": // signal to noise ratio
                    results = ProcessingTypes.RunSnr(settingsFile, audioFile);
                    break;
                case "aed": // acoustic event detection
                    results = ProcessingTypes.RunAed(settingsFile, audioFile);
                    break;
                case "spt": // spectral peak tracking (in progress)
                    results = ProcessingTypes.RunSpt(settingsFile, audioFile);
                    break;

                // recognisers
                case "od": // Oscillation Recogniser
                    results = ProcessingTypes.RunOd(settingsFile, audioFile);
                    break;
                case "epr": // event pattern recognition - groundparrot (in progress)
                    results = ProcessingTypes.RunEpr(settingsFile, audioFile);
                    break;
                //case "spr":  // syntactic pattern recognition
                //    results = ProcessingTypes.RunSpr(settingsFile, audioFile);
                //    break;
                case "hd": // Harmonic Recogniser
                    results = ProcessingTypes.RunHd(settingsFile, audioFile);
                    break;

                // require extra resources
                case "mfcc-od": // MFCCs and OD for calls haveing oscillating character
                    if (resourceFile != null && File.Exists(resourceFile.FullName))
                    {
                        results = ProcessingTypes.RunMfccOd(settingsFile, audioFile, resourceFile, runDir);
                    }
                    else
                    {
                        var path = resourceFile != null ? resourceFile.FullName : "No path given";
                        throw new InvalidOperationException("Invalid resource file path: " + path);
                    }

                    break;
                //case "htk": // run HTK template over a recording
                //    if (resourceFile != null && File.Exists(resourceFile.FullName))
                //    {
                //        results = ProcessingTypes.RunHtk(settingsFile, audioFile, resourceFile, runDir);
                //    }
                //    else
                //    {
                //        var path = resourceFile != null ? resourceFile.FullName : "No path given";
                //        throw new InvalidOperationException("Invalid resource file path: " + path);
                //    }

                //    break;
                default:
                    Console.Error.WriteLine("Unrecognised analysis type: " + analysisType);
                    break;
            }

            return results;
        }

        /// <summary>
        /// Remove empty entries.
        /// </summary>
        /// <param name="table">
        /// The table.
        /// </param>
        /// <returns>
        /// Dictionary without empty entries.
        /// </returns>
        internal static Dictionary<string, string> RemoveEmpty(Dictionary<string, string> table)
        {
            return table
                .Where(kvp => !string.IsNullOrEmpty(kvp.Key) && !string.IsNullOrEmpty(kvp.Value))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// The check params.
        /// </summary>
        /// <param name="expected">
        /// The expected.
        /// </param>
        /// <param name="given">
        /// The given.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// <c>InvalidOperationException</c>.
        /// </exception>
        internal static void CheckParams(IEnumerable<string> expected, IEnumerable<string> given)
        {
            if (!expected.SequenceEqual(given))
            {
                var expectedNotGiven = expected.Where(e => !given.Contains(e));

                var givenNotExpected = given.Where(e => !expected.Contains(e));

                var msg = new StringBuilder(
                    "Parameters passed did not match required parameters." + Environment.NewLine);

                if (givenNotExpected.Count() > 0)
                {
                    var extraGiven = string.Join(",", givenNotExpected.ToArray());
                    msg.Append("Given but not expected: " + extraGiven + "." + Environment.NewLine);
                }

                if (expectedNotGiven.Count() > 0)
                {
                    var extraExp = string.Join(",", expectedNotGiven.ToArray());
                    msg.Append("Expected but not given: " + extraExp + "." + Environment.NewLine);

                    throw new InvalidOperationException(msg.ToString());
                }
            }
        }

        /// <summary>
        /// Validate input parameters.
        /// </summary>
        /// <param name="args">Parameters given to program.</param>
        /// <returns>True if validation succeeded, otherwise false.</returns>
        private static bool Validate(string[] args)
        {
            Console.WriteLine("Given " + args.Length + " arguments: " + Environment.NewLine + string.Join(Environment.NewLine, args));

            // validate
            if (args.Length != 2 && args.Length != 3)
            {
                Console.Error.WriteLine("Incorrect number of arguments. Given " + args.Length + ", should be 2 or 3.");
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

            if (args.Length == 3 && !File.Exists(args[2]))
            {
                Console.Error.WriteLine("Resource file was specified, but does not exist: " + args[2]);
                return false;
            }

            return true;
        }
    }
}