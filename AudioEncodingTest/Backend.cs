using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Text;

using QutSensors.Shared;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AudioEncodingTest
{
    using QutSensors.AnalysisProcessor;
    using QutSensors.ProcessorService.Analysis;

    public class Backend
    {
        public static void Start(IEnumerable<string> files, List<KeyValuePair<string, IEnumerable<String>>> options)
        {
            Console.Write("Technique,File,");
            for (int i = 0; i < options.Count; i++)
            {
                Console.Write(options[i].Key + ",");
            }
            Console.WriteLine(getResultHeading());
            foreach (string file in files)
            {
                RecursiveExecute(file, new Dictionary<string, string>(), options, 0);
            }
        }

        private static void RecursiveExecute(string file, Dictionary<string, string> settings, List<KeyValuePair<string, IEnumerable<string>>> options, int optionIndex)
        {
            if (optionIndex < options.Count)
                foreach (var option in options[optionIndex].Value)
                {
                    Dictionary<string, string> newSettings = new Dictionary<string, string>(settings);
                    newSettings[options[optionIndex].Key] = option;
                    RecursiveExecute(file, newSettings, options, optionIndex + 1);
                }
            else
            {
                Console.Write("{0},{1},", Config.AnalaysisTechnique, file);
                for (int i = 0; i < options.Count; i++)
                {
                    Console.Write(settings[options[i].Key] + ",");
                }
                Console.Write(execute(file, settings));
                Console.WriteLine();
            }
        }

        private static string execute(string file, Dictionary<string, string> options)
        {
            using (AudioTools.TempFile tmpMp3File = new AudioTools.TempFile("mp3"))
            using (AudioTools.TempFile tmpWavFile = new AudioTools.TempFile("wav"))
            {
                var encoderArgs = options.Values.Aggregate((a, b) => a + " " + b);
                Process encoder = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        Arguments = String.Format(Config.EncoderFormatString, '"' + file + '"', tmpMp3File.FileName, encoderArgs),
                        FileName = Config.Encoder,
                        UseShellExecute = false,
                        RedirectStandardError = false
                    }
                };
                encoder.Start();
                encoder.WaitForExit();

                Process decoder = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        Arguments = String.Format(Config.DecoderFormatString, tmpMp3File.FileName, tmpWavFile.FileName),
                        FileName = Config.Decoder,
                        UseShellExecute = false,
                        RedirectStandardError = false
                    }
                };
                decoder.Start();
                decoder.WaitForExit();

                var results = RunCheckResultsErrors(Config.AnalaysisTechnique, tmpWavFile.FileName, Config.AnalysisTechniqueParameterFile);
                return extractSingleResult(results);
            }
        }

        public static string getResultHeading()
        {
            return "Count,Min,Max,Average";
        }

        public static string extractSingleResult(IEnumerable<ProcessorResultTag> result)
        {
            var dataStream = result.Select(p => p.NormalisedScore.Value).Cast<double>();
            return String.Format("{0},{1},{2},{3}",
                result.Count(),
                dataStream.Min(),
                dataStream.Max(),
                dataStream.Average()
            );
        }


        private static IEnumerable<ProcessorResultTag> RunCheckResultsErrors(string analysisType, string audioFileName, string paramFileName)
        {
            return RunCheckResultsErrors(analysisType, audioFileName, paramFileName, null);
        }

        private static IEnumerable<ProcessorResultTag> RunCheckResultsErrors(string analysisType, string audioFileName, string paramFileName, string resourcesFilePath)
        {
            var resources = new TestResources(
                audioFileName, paramFileName, resourcesFilePath);

            // use local runner
            var runner = new LocalAnalysisRunner();

            // prepare item
            var preparedItem = new PreparedWorkItem
            {
                ApplicationFile = resources.AnalysisProgramsExeFile,
                WorkItemName = "debug work item",
                WorkingDirectory = resources.TestWorkingDir
            };

            preparedItem.StandardErrorFile =
                new FileInfo(Path.Combine(preparedItem.WorkingDirectory.FullName, TestResources.StderrFileName));

            preparedItem.StandardOutputFile =
                new FileInfo(Path.Combine(preparedItem.WorkingDirectory.FullName, TestResources.StdoutFileName));

            string safeDir = preparedItem.WorkingDirectory.FullName.TrimEnd('\\');

            string argString = " processing " + analysisType + " " + " \"" + safeDir + "\"";

            if (!string.IsNullOrEmpty(resourcesFilePath) && !string.IsNullOrEmpty(resourcesFilePath.Trim()) && resources.ResourceFile != null)
            {
                var safeResourceDir = resources.ResourceFile.FullName.Trim().TrimEnd('\\');
                argString += " \"" + safeResourceDir + "\"";
            }

            preparedItem.Arguments = argString;

            // run!
            int count = runner.Run(new List<PreparedWorkItem> { preparedItem });

            Assert.AreEqual(1, count);

            // collect results
            IEnumerable<FileInfo> finishedFile =
                preparedItem.WorkingDirectory.GetFiles("*.txt").Where(
                    file => file.Name == TestResources.ProgramOutputFinishedFileName);

            Assert.AreEqual(1, finishedFile.Count(), "No finished file.");

            // check for errors
            var stdError =
                preparedItem.WorkingDirectory.GetFiles("*.txt").Where(
                    file => file.Name == TestResources.StderrFileName).FirstOrDefault();

            if (stdError != null && File.Exists(stdError.FullName))
            {
                Assert.AreEqual(0, File.ReadAllText(stdError.FullName).Length, "Standard Error occured: " + File.ReadAllText(stdError.FullName));
            }

            var programError =
                preparedItem.WorkingDirectory.GetFiles("*.txt").Where(
                    file => file.Name == TestResources.ProgramOutputErrorFileName).FirstOrDefault();

            if (programError != null && File.Exists(programError.FullName))
            {
                Assert.AreEqual(0, File.ReadAllText(programError.FullName).Length, "Program Error occured: " + File.ReadAllText(programError.FullName));
            }

            finishedFile.First();
            string resultsFile = Path.Combine(
                preparedItem.WorkingDirectory.FullName, TestResources.ProgramOutputResultsFileName);
            Assert.IsTrue(File.Exists(resultsFile), "Results file does not exist.");

            List<ProcessorResultTag> results = ProcessorResultTag.Read(resultsFile);

            return results;
        }
    }
}
