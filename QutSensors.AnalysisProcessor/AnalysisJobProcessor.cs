// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalysisJobProcessor.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Controls and runs analysis jobs.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.AnalysisProcessor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using QutSensors.AnalysisProcessor.ProcessorService;
    using QutSensors.AnalysisProcessor.Runners;
    using QutSensors.Shared;
    using QutSensors.Shared.LogProviders;

    /// <summary>
    /// Controls and runs analysis jobs.
    /// </summary>
    public class AnalysisJobProcessor
    {
        /// <summary>
        /// Time to wait between checks for jobs in milliseconds.
        /// </summary>
        private const int InterJobWaitPeriod = 60000; // 1 min

        private readonly ManualResetEvent stopRequestedEvent = new ManualResetEvent(false);
        private readonly ILogProvider log;
        private readonly AnalysisRunnerBase analysisRunner;
        private readonly string workerName;

        /*
         * NOTE:   if you change these file names, they also need to be changed 
         * NOTE:   in AnalysisPrograms.Processing.ProcessingUtils input files
        */

        // expected file names
        private const string SettingsFileName = "processing_input_settings.txt";
        private const string AudioFileName = "processing_input_audio.wav";

        // standard out and error
        private const string StderrFileName = "output_stderr.txt";
        private const string StdoutFileName = "output_stdout.txt";

        // analysis program file names
        private const string ProgramOutputFinishedFileName = "output_finishedmessage.txt";
        private const string ProgramOutputResultsFileName = "output_results.xml";
        private const string ProgramOutputErrorFileName = "output_error.txt";

        /// <summary>
        /// Worker thread for generating cache data.
        /// </summary>
        private Thread workerThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisJobProcessor"/> class. 
        /// </summary>
        /// <param name="executableBaseDirectory">
        /// The executable Base Directory.
        /// </param>
        /// <param name="runsDirectory">
        /// The directory containing input and output for runs.
        /// </param>
        /// <param name="log">
        /// Log Provider.
        /// </param>
        /// <param name="analysisRunner">
        /// The analysis runner.
        /// </param>
        /// <param name="workerName">
        /// The worker Name.
        /// </param>
        public AnalysisJobProcessor(
            DirectoryInfo executableBaseDirectory,
            DirectoryInfo runsDirectory,
            ILogProvider log,
            AnalysisRunnerBase analysisRunner,
            string workerName)
        {
            this.ExecutableBaseDirectory = executableBaseDirectory;
            this.RunsDirectory = runsDirectory;
            this.log = log;
            this.analysisRunner = analysisRunner;
            this.workerName = workerName;
        }

        /// <summary>
        /// Gets RunDirectory.
        /// </summary>
        public DirectoryInfo RunsDirectory { get; private set; }

        /// <summary>
        /// Gets ExecutableBaseDirectory.
        /// </summary>
        public DirectoryInfo ExecutableBaseDirectory { get; private set; }

        /// <summary>
        /// Gets a value indicating whether IsRunning.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return stopRequestedEvent.WaitOne(0);
            }
        }

        /// <summary>
        /// Start the processor.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public void Start()
        {
            if (workerThread != null)
            {
                throw new InvalidOperationException("Worker thread already started.");
            }

            workerThread = new Thread(ThreadMain);
            workerThread.Start();
        }

        /// <summary>
        /// Stop the processor.
        /// </summary>
        public void Stop()
        {
            stopRequestedEvent.Set();
        }

        /// <summary>
        /// Start Analysis Job Processor synchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Worker thread already started.
        /// </exception>
        public void StartSync()
        {
            if (workerThread != null)
            {
                throw new InvalidOperationException("Worker thread already started.");
            }

            try
            {
                if (log != null)
                {
                    log.WriteEntry(LogType.Information, "Analysis Job Processor starting (synchronously).");
                }

                var result = ProcessJobItems();

                if (log != null)
                {
                    if (result)
                    {
                        log.WriteEntry(LogType.Information, "Analysis Job Processor done. Successfully processed work items.");
                    }
                    else
                    {
                        log.WriteEntry(LogType.Error, "Analysis Job Processor done. Did not process any work items.");
                    }
                }
            }
            catch (Exception ex)
            {
                if (log != null)
                {
                    log.WriteEntry(LogType.Error, "Analysis Job Processor encountered an error running sync, and has terminated: " + ex);
                }
            }
        }

        #region Get and Return AnalysisWorkItems

        /// <summary>
        /// Get a single work item.
        /// </summary>
        /// <param name="workerName">
        /// The worker name.
        /// </param>
        /// <returns>
        /// A work item.
        /// </returns>
        private static AnalysisWorkItem GetWorkItem(string workerName)
        {
            using (var ws = new ProcessorServiceWrapper())
            {
                var item = ws.Proxy.GetWorkItem(workerName);
                return item;
            }
        }

        /// <summary>
        /// Get up to <paramref name="maxItems"/> work items.
        /// </summary>
        /// <param name="workerName">
        /// The worker name.
        /// </param>
        /// <param name="maxItems">
        /// The max items.
        /// </param>
        /// <returns>
        /// List of work items.
        /// </returns>
        private static IEnumerable<AnalysisWorkItem> GetWorkItems(string workerName, int maxItems)
        {
            using (var ws = new ProcessorServiceWrapper())
            {
                var item = ws.Proxy.GetWorkItems(workerName, maxItems);
                return item;
            }
        }

        /// <summary>
        /// Return a finished work item.
        /// </summary>
        /// <param name="workerName">
        /// The worker name.
        /// </param>
        /// <param name="jobItemId">
        /// The job item id.
        /// </param>
        /// <param name="itemRunDetails">
        /// The item run details.
        /// </param>
        /// <param name="results">
        /// The results.
        /// </param>
        private static void ReturnComplete(string workerName, int jobItemId, string itemRunDetails, List<ProcessorResultTag> results)
        {
            using (var ws = new ProcessorServiceWrapper())
            {
                ws.Proxy.ReturnWorkItemComplete(workerName, jobItemId, itemRunDetails, results.ToArray());
            }
        }

        /// <summary>
        /// Return an incomplete work item.
        /// </summary>
        /// <param name="workerName">
        /// The worker name.
        /// </param>
        /// <param name="jobItemId">
        /// The job item id.
        /// </param>
        /// <param name="itemRunDetails">
        /// The item run details.
        /// </param>
        /// <param name="errorOccurred">
        /// The error occurred.
        /// </param>
        private static void ReturnIncomplete(string workerName, int jobItemId, string itemRunDetails, bool errorOccurred)
        {
            using (var ws = new ProcessorServiceWrapper())
            {
                ws.Proxy.ReturnWorkItemIncomplete(workerName, jobItemId, itemRunDetails, errorOccurred);
            }
        }

        #endregion

        /// <summary>
        /// Main thread method.
        /// </summary>
        private void ThreadMain()
        {
            try
            {
                if (log != null)
                {
                    log.WriteEntry(LogType.Information, "Analysis Job Processor starting.");
                }

                do
                {
                    if (!ProcessJobItems())
                    {
                        // No job or error so wait double InterJobWaitPeriod for new job to process.
                        stopRequestedEvent.WaitOne(InterJobWaitPeriod);
                    }
                }
                while (!stopRequestedEvent.WaitOne(InterJobWaitPeriod));

                if (log != null)
                {
                    log.WriteEntry(LogType.Information, "Analysis Job Processor stopping.");
                }
            }
            catch (Exception ex)
            {
                if (log != null)
                {
                    log.WriteEntry(LogType.Error, "Analysis Job Processor encountered an error running async, and has terminated: " + ex);
                }
            }
            finally
            {
                stopRequestedEvent.Reset();
                workerThread = null;
            }
        }

        /// <summary>
        /// Process work items.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// Unable to generate spectrogram from job processor
        /// </exception>
        /// <returns>
        /// True if some works items were processed successfully, otherwise false.
        /// </returns>
        private bool ProcessJobItems()
        {
            try
            {
                // 1.
                // check for completed runs:
                // submitFinishedRuns if true
                // deleteFinishedRuns if true

                // finished runs are runs that ran successfully or had an error
                // complete runs ran successfully, does not matter if they had results or not.
                // failed runs had an error or did not run successfully.
                var finishedItemsBefore = ProcessFinishedItems();

                this.log.WriteEntry(LogType.Information, string.Format("Analysis Job Processor successfully returned {0} completed work items before getting new work items.", finishedItemsBefore));

                // 2.
                // get the maximum number of analysiswork items to get.
                // based on what the AnalysisRunner reports it can handle.
                var maxItems = this.analysisRunner.MaxAllocations;

                // 3.
                // get analysiswork items.
                var workitems = GetWorkItems(this.workerName, maxItems).Where(awi => awi != null);

                var preparedItems = new List<PreparedWorkItem>();

                foreach (var workItem in workitems)
                {
                    // 4. 
                    // create the folder for the run.
                    var newRunDirString = Path.Combine(
                        this.RunsDirectory.FullName, workItem.JobItemId + "-Run-" + Guid.NewGuid());

                    try
                    {
                        var name = workItem.AnalysisGenericType + " " + workItem.AnalysisGenericVersion + " " +
                                   DateTime.Now;

                        var preparedItem = new PreparedWorkItem
                            {
                                ApplicationFile = this.analysisRunner.GetExecutable(this.ExecutableBaseDirectory, workItem),
                                WorkItemName = name,
                                WorkingDirectory = Directory.CreateDirectory(newRunDirString)
                            };

                        preparedItem.StandardErrorFile =
                            new FileInfo(Path.Combine(preparedItem.WorkingDirectory.FullName, StderrFileName));

                        preparedItem.StandardOutputFile =
                            new FileInfo(Path.Combine(preparedItem.WorkingDirectory.FullName, StdoutFileName));

                        // 5. 
                        // create and write settings file.
                        var settingsFile = Path.Combine(preparedItem.WorkingDirectory.FullName, SettingsFileName);
                        File.WriteAllText(settingsFile, workItem.AnalysisRunSettings);

                        // 6.
                        // download the audio file.
                        // assume audio file is .wav. download and save audio file
                        var audioFilePath = Path.Combine(preparedItem.WorkingDirectory.FullName, AudioFileName);
                        var audioFileUrl = workItem.AudioFileUri;

                        using (var client = new System.Net.WebClient())
                        {
                            client.DownloadFile(audioFileUrl, audioFilePath);
                        }

                        // 7. create the argument string
                        // execute processing version, not dev version
                        // type of analysis to run
                        // run directory

                        var safeDir = preparedItem.WorkingDirectory.FullName.TrimEnd('\\');
                        var argString = " processing " + " " + workItem.AnalysisGenericType +
                            " \"" + safeDir + "\"";

                        if (!string.IsNullOrEmpty(workItem.AnalysisAdditionalData) &&
                            !string.IsNullOrEmpty(workItem.AnalysisAdditionalData.Trim()))
                        {
                            var safeResourceDir = preparedItem.ApplicationFile.DirectoryName.TrimEnd('\\');

                            var dir = Path.Combine(safeResourceDir, "Resources");
                            var file = Path.Combine(dir, workItem.AnalysisAdditionalData);
                            argString += " \"" + file + "\""; // resource file name
                        }

                        preparedItem.Arguments = argString;
                        preparedItems.Add(preparedItem);
                    }
                    catch
                    {
                        // don't let errors from one work item stop all work items.
                        if (Directory.Exists(newRunDirString))
                        {
                            Directory.Delete(newRunDirString, true);
                        }
                    }
                }

                // 7.
                // start the analysis items running, based on AnalysisRunner implementation.
                // cluster will return straight away, local will wait until the one item is completed.
                var numItemsRun = this.analysisRunner.Run(preparedItems);

                this.log.WriteEntry(LogType.Information, string.Format("Analysis Job Processor successfully started {0} of {1} work items.", numItemsRun, preparedItems.Count));

                // 8.
                // check for completed runs again.
                var finishedItemsAfter = ProcessFinishedItems();
                this.log.WriteEntry(LogType.Information, string.Format("Analysis Job Processor successfully returned {0} completed work items after getting new work items.", finishedItemsAfter));

                return numItemsRun > 0;
            }
            catch (Exception ex)
            {
                this.log.WriteEntry(LogType.Error, "Analysis Job Processor encountered an error processing work items: " + ex);
                return false;
            }
        }

        /// <summary>
        /// Read text from <paramref name="filePath"/>, and append to 
        /// <paramref name="itemRunDetails"/> using <paramref name="fileDescr"/> as heading.
        /// </summary>
        /// <param name="itemRunDetails">
        /// StringBuilder to append to.
        /// </param>
        /// <param name="fileDescr">
        /// Discription of file.
        /// </param>
        /// <param name="filePath">
        /// Path to file.
        /// </param>
        /// <returns>
        /// Lines from file at <paramref name="filePath"/> only.
        /// </returns>
        private static StringBuilder GetTextFromFile(StringBuilder itemRunDetails, string fileDescr, string filePath)
        {
            var sb = new StringBuilder();
            if (File.Exists(filePath))
            {
                foreach (var line in File.ReadAllLines(filePath))
                {
                    sb.AppendLine(line.Trim());
                }
            }

            if (sb.Length > 0)
            {
                itemRunDetails.AppendLine();
                itemRunDetails.AppendLine("----" + fileDescr + "----");

                itemRunDetails.AppendLine(sb.ToString());
            }

            return sb;
        }

        /// <summary>
        /// Retrieve and process finished runs.
        /// </summary>
        /// <returns>
        /// Number of finished Items sucessfully processed.
        /// </returns>
        private int ProcessFinishedItems()
        {
            var finishedDirs =
                this.RunsDirectory.GetDirectories().Where(
                    dir => dir.GetFiles("*.txt").Any(file => file.Name == ProgramOutputFinishedFileName)).ToList();

            var successfullyReturned = 0;

            foreach (var runDir in finishedDirs)
            {
                try
                {
                    // gather item run details from output text files.
                    var itemRunDetails = new StringBuilder();

                    // get jobitemId from folder name
                    int jobItemId = Convert.ToInt32(runDir.Name.Substring(0, runDir.Name.IndexOf("-")));

                    // possible files
                    var errors = GetTextFromFile(
                        itemRunDetails, "Errors", Path.Combine(runDir.FullName, ProgramOutputErrorFileName));

                    var info = GetTextFromFile(
                        itemRunDetails, "Information", Path.Combine(runDir.FullName, ProgramOutputFinishedFileName));

                    var stdOut = GetTextFromFile(
                        itemRunDetails, "Application Output", Path.Combine(runDir.FullName, StdoutFileName));

                    var stdErr = GetTextFromFile(
                        itemRunDetails, "Application Errors", Path.Combine(runDir.FullName, StderrFileName));

                    if (this.analysisRunner.SubmitFinishedRuns)
                    {
                        try
                        {
                            if (errors.Length > 0 || stdErr.Length > 0)
                            {
                                // ignore results file, send back as error
                                ReturnIncomplete(workerName, jobItemId, itemRunDetails.ToString(), true);
                            }
                            else
                            {
                                // return completed, even if there are 0 results.
                                List<ProcessorResultTag> results = null;
                                var resultsFile = Path.Combine(runDir.FullName, ProgramOutputResultsFileName);
                                if (File.Exists(resultsFile))
                                {
                                    results = ProcessorResultTag.Read(resultsFile);
                                }

                                ReturnComplete(workerName, jobItemId, itemRunDetails.ToString(), results);
                            }
                        }
                        catch (Exception ex)
                        {
                            var msg = new StringBuilder();
                            msg.AppendLine();
                            msg.AppendLine();
                            msg.AppendLine("**Error Sending Run: ");
                            msg.AppendLine(ex.ToString());

                            File.AppendAllText(
                                Path.Combine(runDir.FullName, ProgramOutputErrorFileName), msg.ToString());
                        }
                    }

                    if (this.analysisRunner.DeleteFinishedRuns)
                    {
                        if (runDir.Exists)
                        {
                            try
                            {
                                runDir.Delete(true);
                            }
                            catch (Exception ex)
                            {
                                this.log.WriteEntry(
                                    LogType.Error,
                                    "Analysis Job Processor could not delete folder '" + runDir.FullName +
                                    "' containing completed work item due to error: " + Environment.NewLine + ex);
                            }
                        }
                    }

                    successfullyReturned++;
                }
                catch (Exception ex)
                {
                    this.log.WriteEntry(
                        LogType.Error,
                        "Analysis Job Processor encountered an error processing a completed work item: '" + Environment.NewLine +
                        ex);
                }
            }

            return successfullyReturned;
        }
    }
}
