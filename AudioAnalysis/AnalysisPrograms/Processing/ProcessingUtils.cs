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

        internal static void Run(string[] args)
        {
            // validate
            if (
                args.Length != 5 ||                         // must be 5 args
                // first arg must be valid analysis type
                !File.Exists(args[1]) ||                    // settings file must exist
                !File.Exists(args[2])                       // audio file must exist
                // results file name
                // finished file name
            )
            {
                Console.WriteLine("Num of Args: " + args.Length);
                Console.WriteLine("Settings File: " + args[1]);
                Console.WriteLine("Exists: " + File.Exists(args[1]));
                Console.WriteLine("Audio File: " + args[2]);
                Console.WriteLine("Exists: " + File.Exists(args[2]));
                Console.WriteLine();
                Console.WriteLine("Arguments: " + string.Join(" , ", args));
                Console.WriteLine();

                // check trust
                var filePerm = new System.Security.Permissions.FileIOPermission(
                    System.Security.Permissions.FileIOPermissionAccess.Read,
                    System.Security.AccessControl.AccessControlActions.View,
                    args[1]
                    );

                var accessSettings = System.Security.SecurityManager.IsGranted(filePerm);
                Console.WriteLine("Access: " + accessSettings + " To: " + filePerm.ToString());


                PrintUsage();
            }
            else
            {
                RunAnalysis(args[0], args[1], args[2], args[3], args[4]);
            }
        }

        internal static void PrintUsage()
        {
            Console.WriteLine("This console app is used to run analyses on the processing cluster.");
            Console.WriteLine("It requires exactly six parameters:");
            Console.WriteLine("\t1. 'processing' - indicates this is a processing run.");
            Console.WriteLine("\t2. Type of analysis to run.");
            Console.WriteLine("\t3. Path to settings file.");
            Console.WriteLine("\t4. Path to audio file.");
            Console.WriteLine("\t5. Name of results output file.");
            Console.WriteLine("\t6. Name of finished output file.");
            Console.WriteLine();
            Console.WriteLine("Press a key to continue...");
        }

        private static void RunAnalysis(string analysisType, string pathToSettingsFile, string pathToAudioFile, string resultsFileName, string finishedFileName)
        {

            IEnumerable<ProcessorResultTag> results = null;

            var finishedPath = Path.GetDirectoryName(pathToSettingsFile) + "\\" + finishedFileName;
            var finishedMessage = new StringBuilder();
            Console.WriteLine("Analysis Type: " + analysisType);

            try
            {
                // select analysis from name
                switch (analysisType)
                {
                    case "aed":  //acoustic event detection
                        results = ProcessingTypes.RunAED(new FileInfo(pathToSettingsFile), new FileInfo(pathToAudioFile));
                        break;
                    case "od":   //Oscillation Recogniser
                        results = ProcessingTypes.RunOD(new FileInfo(pathToSettingsFile), new FileInfo(pathToAudioFile));
                        break;
                    case "epr": //event pattern recognition - groundparrot
                        results = ProcessingTypes.RunEPR(new FileInfo(pathToAudioFile));
                        break;
                    case "snr":   //signal to noise ratio
                        // not used yet
                        Console.WriteLine("not used yet.");
                        break;
                    case "htk":   //run an HTK template over a recording
                        // not used yet
                        Console.WriteLine("not used yet.");
                        break;
                    case "spt": // spectral peak tracking
                        // not used yet
                        Console.WriteLine("not used yet.");
                        break;
                    default:
                        Console.WriteLine("Unrecognised analysis type.");
                        PrintUsage();
                        break;
                }

            }
            catch (Exception ex)
            {
                finishedMessage.AppendLine("***AP-Analysis-Run-Error: " + ex.ToString());
            }

            // write results and messages
            var resultsPath = Path.GetDirectoryName(pathToSettingsFile) + "\\" + resultsFileName;

            try
            {

                if (results != null)
                {
                    // results file
                    ProcessorResultTag.Write(results.ToList(), resultsPath);

                    // finished file
                    finishedMessage.AppendLine("***AP-ExitCode: 0");
                    File.WriteAllText(finishedPath, finishedMessage.ToString());

                    Environment.Exit(0);
                }
                else
                {
                    // finished file
                    finishedMessage.AppendLine("***AP-ExitCode: 1");
                    finishedMessage.AppendLine("***AP-Results: No results available");
                    File.WriteAllText(finishedPath, finishedMessage.ToString());
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                // finished file
                finishedMessage.AppendLine("***AP-Write-File-Error: " + ex.ToString());
                finishedMessage.AppendLine("***AP-ExitCode: 2");
                File.WriteAllText(finishedPath, finishedMessage.ToString());
                Environment.Exit(2);
            }


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
