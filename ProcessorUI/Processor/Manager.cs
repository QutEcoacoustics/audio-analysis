// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Manager.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Retrieves, prepares, runs and submits analysis work items.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.Processor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Microsoft.ComputeCluster;
    using Microsoft.Practices.EnterpriseLibrary.Logging;

    using QutSensors.Processor.ProcessorService;
    using QutSensors.Shared;

    /// <summary>
    /// Retrieves, prepares, runs and submits analysis work items.
    /// </summary>
    public class Manager
    {
        // NOTE: if you change these file names, they also need to be changed in AnalysisPrograms.Processing.ProcessingUtils

        // input files
        private const string SettingsFileName = "processing_input_settings.txt";
        private const string AudioFileName = "processing_input_audio.wav";

        // standard out and error
        private const string StderrFileName = "output_stderr.txt";
        private const string StdoutFileName = "output_stdout.txt";

        // analysis program file names
        private const string ProgramOutputFinishedFileName = "output_finishedmessage.txt";
        private const string ProgramOutputResultsFileName = "output_results.xml";
        private const string ProgramOutputErrorFileName = "output_error.txt";

        #region Singleton Pattern

        private Manager()
        {
        }

        internal class ManagerSingleton
        {
            static ManagerSingleton() { }
            internal static readonly Manager instance = new Manager();
        }

        public static Manager Instance
        {
            get
            {
                return ManagerSingleton.instance;
            }
        }

        #endregion

        /// <summary>
        /// Gets the dir run base.
        /// </summary>
        /// <exception cref="DirectoryNotFoundException">
        /// <c>DirectoryNotFoundException</c>.
        /// </exception>
        public DirectoryInfo DirRunBase
        {
            get
            {
                // set the run directory
                var runsFolder = System.Configuration.ConfigurationManager.AppSettings["RunDirectory"];

                if (string.IsNullOrEmpty(runsFolder) || !Directory.Exists(runsFolder))
                    throw new DirectoryNotFoundException("Analysis run directory does not exist: " + runsFolder);

                return new DirectoryInfo(runsFolder);
            }
        }

        /// <summary>
        /// Gets the dir program base.
        /// </summary>
        /// <exception cref="Exception">
        /// <c>Exception</c>.
        /// </exception>
        public DirectoryInfo DirProgramBase
        {
            get
            {
                // set the programs directory
                var programsFolder = System.Configuration.ConfigurationManager.AppSettings["ProgramDirectory"];

                if (string.IsNullOrEmpty(programsFolder) || !Directory.Exists(programsFolder))
                {
                    throw new Exception("Analysis program directory does not exist: " + programsFolder);
                }

                return new DirectoryInfo(programsFolder);
            }
        }

        /// <summary>
        /// Gets ProgramName.
        /// </summary>
        public string ProgramName
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["ProgramName"] ?? "AnalysisPrograms.exe";
            }
        }

        /// <summary>
        /// Gets UserName.
        /// </summary>
        public string UserName
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["JobRunUserName"];
            }
        }

        /// <summary>
        /// Gets Password.
        /// </summary>
        public string Password
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["JobRunPassword"];
            }
        }

        /// <summary>
        /// Gets a value indicating whether DeleteFinishedRuns.
        /// </summary>
        public bool DeleteFinishedRuns
        {
            get
            {
                return Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["DeleteFinishedRuns"]);
            }
        }

        private readonly object currentlyRetrievingFinishedRunsLock = new object();

        private bool currentlyRetrievingFinishedRuns;

        /// <summary>
        /// Gets a value indicating whether Currently Retrieving Finished Runs.
        /// </summary>
        public bool CurrentlyRetrievingFinishedRuns
        {
            get
            {
                lock (this.currentlyRetrievingFinishedRunsLock)
                {
                    return this.currentlyRetrievingFinishedRuns;
                }
            }

            private set
            {
                lock (this.currentlyRetrievingFinishedRunsLock)
                {
                    this.currentlyRetrievingFinishedRuns = value;
                }
            }
        }

        #region Common

        /// <summary>
        /// Prepare folders and files for a new run.
        /// </summary>
        /// <param name="workItem">
        /// The work item.
        /// </param>
        /// <returns>
        /// Directory for new run.
        /// </returns>
        public DirectoryInfo PrepareNewRun(AnalysisWorkItem workItem)
        {
            if (workItem == null) return null;

            var newRunDirString = DirRunBase.FullName + "\\" + workItem.JobItemId + "-Run-" + Guid.NewGuid();

            try
            {
                // create new run folder
                var newRunDir = Directory.CreateDirectory(newRunDirString);

                // create settings file
                var settingsFile = newRunDir.FullName + "\\" + SettingsFileName;
                File.WriteAllText(settingsFile, workItem.AnalysisRunSettings);

                // audio file location
                var audioFile = newRunDir.FullName + "\\" + AudioFileName;
                var audioFileUrl = workItem.AudioFileUri;

                // audio file must be wav file
                if (!workItem.AudioFileUri.AbsoluteUri.EndsWith("wav") && workItem.AudioFileUri.AbsoluteUri.Contains('.'))
                {
                    audioFileUrl = new Uri(workItem.AudioFileUri.AbsoluteUri.Substring(0, workItem.AudioFileUri.AbsoluteUri.LastIndexOf('.')) + ".wav");
                }

                // download and save audio file
                var client = new System.Net.WebClient();
                client.DownloadFile(audioFileUrl, audioFile);

                return newRunDir;
            }
            catch (Exception ex)
            {
                if (Directory.Exists(newRunDirString))
                {
                    Directory.Delete(newRunDirString, true);
                }

                Log(ex);
            }

            return null;
        }

        public string CreateArgumentString(AnalysisWorkItem item, DirectoryInfo runDirectory)
        {
            if (item == null || runDirectory == null) return string.Empty;

            var argString =
                " processing" + // execute cluster version, not dev version
                " " + item.AnalysisGenericType + // type of analysis to run
                " \"" + runDirectory.FullName + "\""; // run directory

            if (!string.IsNullOrEmpty(item.AnalysisAdditionalData.Trim()))
            {
                argString += " \"" + Shared.Utilities.ExecutingDirectory + "\\Resources\\" + item.AnalysisAdditionalData + "\""; // resource file name
            }

            return argString;
        }

        private StringBuilder GetTextFromFile(string fileDescr, string filePath, StringBuilder itemRunDetails)
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

        public void ReturnFinishedRun(DirectoryInfo runDir, string workerName)
        {
            if (CurrentlyRetrievingFinishedRuns)
            {
                this.Log("Already retrieving finished runs (ReturnFinishedRun(" + runDir.FullName + ", " + workerName + ")).");
                return;
            }

            CurrentlyRetrievingFinishedRuns = true;

            var itemRunDetails = new StringBuilder();

            // get jobitemId from folder name
            int jobItemId = Convert.ToInt32(runDir.Name.Substring(0, runDir.Name.IndexOf("-")));

            // possible files
            var errors = GetTextFromFile("Errors", Path.Combine(runDir.FullName, ProgramOutputErrorFileName), itemRunDetails);
            var info = GetTextFromFile("Information", Path.Combine(runDir.FullName, ProgramOutputFinishedFileName), itemRunDetails);

            var stdOut = GetTextFromFile("Application Output", Path.Combine(runDir.FullName, StdoutFileName), itemRunDetails);
            var stdErr = GetTextFromFile("Application Errors", Path.Combine(runDir.FullName, StderrFileName), itemRunDetails);

            try
            {
                bool webServiceCallSuccess;
                if (errors.Length > 0 || stdErr.Length > 0)
                {
                    // ignore results file, send back as error
                    webServiceCallSuccess = this.ReturnIncomplete(
                        workerName,
                        jobItemId,
                        itemRunDetails.ToString(),
                        true);
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

                    webServiceCallSuccess = this.ReturnComplete(
                        workerName,
                        jobItemId,
                        itemRunDetails.ToString(),
                        results);
                }

                if (webServiceCallSuccess)
                {
                    // delete run directory
                    if (runDir.Exists && DeleteFinishedRuns)
                    {
                        runDir.Delete(true);
                    }
                }
                else
                {
                    var msg = new StringBuilder();
                    msg.AppendLine();
                    msg.AppendLine();
                    msg.AppendLine("**Error Sending Run via webservice. ");

                    File.AppendAllText(Path.Combine(runDir.FullName, ProgramOutputErrorFileName), msg.ToString());
                }

                CurrentlyRetrievingFinishedRuns = false;
            }
            catch (Exception ex)
            {
                var msg = new StringBuilder();
                msg.AppendLine();
                msg.AppendLine();
                msg.AppendLine("**Error Sending Run: ");
                msg.AppendLine(ex.ToString());

                File.AppendAllText(Path.Combine(runDir.FullName, ProgramOutputErrorFileName), msg.ToString());

                Log(ex);
                CurrentlyRetrievingFinishedRuns = false;
            }
        }

        public IEnumerable<DirectoryInfo> GetFinishedRuns()
        {
            if (CurrentlyRetrievingFinishedRuns)
            {
                this.Log("Already retrieving finished runs (in GetFinishedRuns()).");
                return new List<DirectoryInfo>();
            }

            CurrentlyRetrievingFinishedRuns = true;

            var finishedDirs = this.DirRunBase.GetDirectories()
                .Where(dir => dir.GetFiles("*.txt")
                    .Any(file => file.Name == ProgramOutputFinishedFileName || file.Name == StderrFileName)).ToList();

            CurrentlyRetrievingFinishedRuns = false;

            return finishedDirs;
        }

        /// <summary>
        /// Log error or warning message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Log(object message)
        {
            if (Logger.IsLoggingEnabled())
            {
                Logger.Write(message);
            }
        }

        #endregion

        #region Web service

        public AnalysisWorkItem GetWorkItem(string workerName)
        {
            using (var ws = new ProcessorServiceWrapper())
            {
                try
                {
                    var item = ws.Proxy.GetWorkItem(new GetWorkItemRequest(workerName));
                    return item.GetWorkItemResult;
                }
                catch (Exception ex)
                {
                    Log(ex);
                    return null;
                }
            }
        }

        public IEnumerable<AnalysisWorkItem> GetWorkItems(string workerName, int maxItems)
        {
            using (var ws = new ProcessorServiceWrapper())
            {
                try
                {
                    var item = ws.Proxy.GetWorkItems(new GetWorkItemsRequest(workerName, maxItems));
                    return item.GetWorkItemsResult;
                }
                catch (Exception ex)
                {
                    Log(ex);
                    return null;
                }
            }
        }

        public bool ReturnComplete(string workerName, int jobItemId, string itemRunDetails, List<ProcessorResultTag> results)
        {
            using (var ws = new ProcessorServiceWrapper())
            {
                try
                {
                    var item = ws.Proxy.ReturnWorkItemComplete(new ReturnWorkItemCompleteRequest(workerName, jobItemId, itemRunDetails, results));
                    return true;
                }
                catch (Exception ex)
                {
                    Log(ex);
                    return false;
                }
            }
        }

        public bool ReturnIncomplete(string workerName, int jobItemId, string itemRunDetails, bool errorOccurred)
        {
            using (var ws = new ProcessorServiceWrapper())
            {
                try
                {
                    var item = ws.Proxy.ReturnWorkItemIncomplete(new ReturnWorkItemIncompleteRequest(workerName, jobItemId, itemRunDetails, errorOccurred));
                    return true;
                }
                catch (Exception ex)
                {
                    Log(ex);
                    return false;
                }
            }
        }

        #endregion

        #region Processing Cluster

        /// <summary>
        /// Create a new job on the cluster.
        /// </summary>
        /// <param name="cluster">Existing cluster to create job.</param>
        /// <returns>New job.</returns>
        public IJob PC_NewJob(ICluster cluster)
        {
            var job = cluster.CreateJob();
            job.IsExclusive = false;

            job.MinimumNumberOfProcessors = 1;
            job.Name = "Processor " + DateTime.Now;
            job.Project = "QUT Sensors";

            return job;
        }

        public int PC_RunJob(ICluster cluster, IJob job)
        {
            var jobId = cluster.AddJob(job);

            // job is owned by specified user
            cluster.SetJobCredentials(jobId, UserName, Password);

            // submit job as specified user
            cluster.SubmitJob(jobId, UserName, Password, false, 0);
            return jobId;
        }

        public ITask PC_CreateTask(ICluster cluster)
        {
            ITask task = cluster.CreateTask();
            task.IsExclusive = false;
            task.IsRerunnable = false;

            task.MaximumNumberOfProcessors = 1;
            task.MinimumNumberOfProcessors = 1;

            return task;
        }

        public ITask PC_PrepareTask(ICluster cluster, AnalysisWorkItem item)
        {
            var newRunDir = PrepareNewRun(item);
            if (newRunDir == null || !newRunDir.Exists)
            {
                this.Log("Problem creating '" + newRunDir + "' or downloading audio.");
                return null;
            }

            // program file from version
            FileInfo programFile;
            try
            {
                if (string.IsNullOrEmpty(item.AnalysisGenericVersion))
                {
                    this.Log("Null or empty AnalysisGenericVersion.");
                    return null;
                }

                programFile = new FileInfo(DirProgramBase.FullName + "\\" + item.AnalysisGenericVersion + "\\" + ProgramName);
                if (!programFile.Exists)
                {
                    this.Log("Problem finding program file '" + programFile + "'.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                this.Log("Problem finding program file." + Environment.NewLine + ex);
                return null;
            }

            var programArgs = CreateArgumentString(item, newRunDir);
            if (string.IsNullOrEmpty(programArgs))
            {
                this.Log("Problem creating program argument string.");
                return null;
            }

            var task = PC_CreateTask(cluster);

            task.Name = item.AnalysisGenericType + " " + item.AnalysisGenericVersion + " " + DateTime.Now;
            task.WorkDirectory = programFile.DirectoryName;

            task.CommandLine = programFile.Name + " " + programArgs;
            ////task.CommandLine = programArgs;

            task.Stderr = Path.Combine(newRunDir.FullName, StderrFileName);
            task.Stdout = Path.Combine(newRunDir.FullName, StdoutFileName);

            return task;
        }

        public void PC_CompletedRun(DirectoryInfo runDir, string workerName)
        {
            ReturnFinishedRun(runDir, workerName);
        }

        #endregion

        #region Development

        public void Dev_StartWorker(string workerName, ProcessItem pi, Action onComplete)
        {
            if (pi != null)
            {
                // start processing
                pi.Start((exitCode) => { Dev_WorkerCompleted(pi, workerName, exitCode); onComplete(); });
            }
            else
            {
                onComplete();
            }
        }

        public ProcessItem Dev_PrepareItem(AnalysisWorkItem workItem)
        {
            if (workItem != null)
            {
                var newRunDir = PrepareNewRun(workItem);
                var programFile = new FileInfo(DirProgramBase.FullName + "\\" + ProgramName); // always use debug build
                var programArgs = CreateArgumentString(workItem, newRunDir);

                // create new processor
                return new ProcessItem(workItem, programFile, newRunDir, programArgs);
            }

            return null;
        }

        private void Dev_WorkerCompleted(ProcessItem pi, string workerName, int exitCode)
        {
            if (pi != null)
            {
                File.WriteAllText(Path.Combine(pi.RunDir.FullName, StdoutFileName), pi.OutputData);
                File.WriteAllText(Path.Combine(pi.RunDir.FullName, StderrFileName), pi.ErrorData);

                if (!string.IsNullOrEmpty(workerName))
                {
                    ReturnFinishedRun(pi.RunDir, workerName);
                }
            }
        }

        #endregion
    }
}
