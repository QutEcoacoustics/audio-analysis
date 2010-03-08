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
                args.Length != 3 ||                         // must be 3 args
                // first arg must be valid analysis type
                !File.Exists(args[1]) ||                    // settings file must exist
                !File.Exists(args[2])                       // audio file must exist
            )
            {
                PrintUsage();
            }
            else
            {
                RunAnalysis(args[0], args[1], args[2]);
            }
        }

        internal static void PrintUsage()
        {
            Console.WriteLine("This console app is used to run analyses on the processing cluster.");
            Console.WriteLine("It requires exactly four parameters:");
            Console.WriteLine("\t1. 'processing' - indicates this is a processing run.");
            Console.WriteLine("\t2. Type of analysis to run.");
            Console.WriteLine("\t3. Path to settings file.");
            Console.WriteLine("\t4. Path to audio file.");
            Console.WriteLine();
            Console.WriteLine("Press a key to continue...");

            Console.ReadLine();
        }

        private static void RunAnalysis(string analysisType, string pathToSettingsFile, string pathToAudioFile)
        {
            IEnumerable<ProcessorResultTag> results = null;

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
                    break;
                default:
                    Console.WriteLine("Unrecognised analysis type.");
                    PrintUsage();
                    break;
            }

            // write results and messages
            if (results != null)
            {
                var resultsPath = Path.GetDirectoryName(pathToSettingsFile) + "\\results.xml";
                var messagePath = Path.GetDirectoryName(pathToSettingsFile) + "\\usermessage.txt";

                ProcessorResultTag.Write(results, resultsPath);
            }
        }

        /// <summary>
        /// Uses the information in Michael Towsey's AcousticEvent class to initialise an instance of the ProcessorResultTag class.
        /// </summary>
        /// <param name="ae">instance of the AcousticEvent class</param>
        /// <returns></returns>
        public static ProcessorResultTag GetProcessorResultTag(AcousticEvent ae)
        {
            var prt = new ProcessorResultTag()
            {
                NormalisedScore = new ResultProperty()
                {
                    Key = ae.Name,
                    Value = ae.NormalisedScore,
                    Info = new SerializableDictionary<string, string>() { { "Description", "Normalised score" } }
                },
                StartTime = (int?)Math.Round(ae.StartTime * 1000),
                EndTime = (int?)Math.Round(ae.EndTime * 1000),
                MinFrequency = (int?)ae.MinFreq,
                MaxFrequency = (int?)ae.MaxFreq,
                ExtraDetail = new System.Collections.ObjectModel.Collection<ResultProperty>(ae.ResultPropertyList.ToList()) //TODO: store more info about AcousticEvents?
            };

            return prt;
        }

    }
}
