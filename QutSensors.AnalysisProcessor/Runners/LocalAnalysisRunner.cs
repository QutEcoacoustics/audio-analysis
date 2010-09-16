// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocalAnalysisRunner.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Runs analysis items on the local machine using the console.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.AnalysisProcessor.Runners
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using QutSensors.Shared;

    /// <summary>
    /// Runs analysis items on the local machine using the console.
    /// </summary>
    public class LocalAnalysisRunner : AnalysisRunnerBase
    {
        /// <summary>
        /// Gets the maxmimum number of items this runner can allocate at this time.
        /// </summary>
        /// <returns>Maximum number of items.</returns>
        public override int MaxAllocations
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Gets a value indicating whether SubmitFinishedRuns.
        /// </summary>
        public override bool SubmitFinishedRuns
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether DeleteFinishedRuns.
        /// </summary>
        public override bool DeleteFinishedRuns
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Complete analysis work item preparation and run analysis work items. 
        /// </summary>
        /// <param name="workItems">
        /// The Work Items.
        /// </param>
        /// <returns>
        /// Total number of analysis work items started.
        /// </returns>
        public override int Run(IEnumerable<PreparedWorkItem> workItems)
        {
            workItems = workItems.Where(i => i != null);
            if (workItems.Count() == 0)
            {
                return 0;
            }

            int successCount = 0;

            foreach (var workItem in workItems)
            {
                try
                {
                    PreparedWorkItem item = workItem;
                    using (var worker = new Process())
                    {
                        worker.StartInfo = new ProcessStartInfo
                            {
                                CreateNoWindow = true,
                                RedirectStandardError = true,
                                RedirectStandardInput = true,
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                WindowStyle = ProcessWindowStyle.Hidden,
                                WorkingDirectory = item.WorkingDirectory.FullName,
                                Arguments = item.Arguments,
                                FileName = item.ApplicationFile.FullName,
                            };

                        worker.EnableRaisingEvents = true;

                        worker.ErrorDataReceived += (sender, eventArgs) =>
                            {
                                if (!string.IsNullOrEmpty(eventArgs.Data))
                                {
                                    File.AppendAllText(
                                        item.StandardErrorFile.FullName, eventArgs.Data + Environment.NewLine);
                                }
                            };

                        worker.OutputDataReceived += (sender, eventArgs) =>
                            {
                                if (!string.IsNullOrEmpty(eventArgs.Data))
                                {
                                    File.AppendAllText(
                                        item.StandardOutputFile.FullName, eventArgs.Data + Environment.NewLine);
                                }
                            };

                        worker.Start();

                        worker.BeginErrorReadLine();
                        worker.BeginOutputReadLine();

                        worker.WaitForExit();
                    }

                    successCount++;
                }
                catch
                {
                    // log error?
                    // worker just won't run.
                }
            }

            return successCount;
        }

        /// <summary>
        /// Get location of executable program.
        /// </summary>
        /// <param name="executableBaseDirectory">
        /// The executable Base Directory.
        /// </param>
        /// <param name="analysisWorkItem">
        /// The analysis work items.
        /// </param>
        /// <returns>
        /// FileInfo for executable program.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <c>InvalidOperationException</c>.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Executable file is required.
        /// </exception>
        public override FileInfo GetExecutable(DirectoryInfo executableBaseDirectory, AnalysisWorkItem analysisWorkItem)
        {
            // get base folder for executable file.
            if (executableBaseDirectory == null)
            {
                throw new InvalidOperationException("Analysis program base directory is null.");
            }

            if (!executableBaseDirectory.Exists)
            {
                throw new InvalidOperationException("Analysis program directory does not exist: " + executableBaseDirectory.FullName);
            }

            // check executable exists at location specified.
            var programFile = new FileInfo(Path.Combine(executableBaseDirectory.FullName, this.AnalysisProgramsExeFileName));
            if (!programFile.Exists)
            {
                throw new FileNotFoundException("Executable file is required.", programFile.FullName);
            }

            return programFile;
        }
    }
}
