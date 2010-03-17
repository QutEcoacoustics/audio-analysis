using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using QutSensors.Shared;
using QutSensors.Processor.ProcessorService;
using Microsoft.ComputeCluster;

namespace QutSensors.Processor
{
    /// <summary>
    /// Retrieves, prepares, runs and submits analysis work items.
    /// </summary>
    public class Manager
    {
        #region Singleton

        private static readonly object syncLock = new object();

        // we don't want the compiler to emit beforefieldinit
        static Manager()
        {
        }

        // private ctor is below

        static Manager instance;
        public static Manager Instance
        {
            get
            {
                lock (syncLock)
                {
                    if (instance == null)
                        instance = new Manager();
                    return instance;
                }
            }
            set { instance = value; }
        }

        #endregion

        public DirectoryInfo DirRunBase
        {
            get
            {
                // set the run directory
                var runsFolder = System.Configuration.ConfigurationManager.AppSettings["RunDirectory"];

                if (string.IsNullOrEmpty(runsFolder) || !Directory.Exists(runsFolder))
                    throw new Exception("Analysis run directory does not exist: " + runsFolder);

                return new DirectoryInfo(runsFolder);
            }
        }

        public DirectoryInfo DirProgramBase
        {
            get
            {
                //set the programs directory
                var programsFolder = System.Configuration.ConfigurationManager.AppSettings["ProgramDirectory"];

                if (string.IsNullOrEmpty(programsFolder) || !Directory.Exists(programsFolder))
                    throw new Exception("Analysis program directory does not exist: " + programsFolder);

                return new DirectoryInfo(programsFolder);

            }
        }

        public string ProgramName
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["ProgramName"] ?? "AnalysisPrograms.exe";
            }
        }

        public string UserName
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["JobRunUserName"];
            }
        }

        public string Password
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["JobRunPassword"];
            }
        }

        private const string SETTINGS_FILE_NAME = "input_settings.txt";
        private const string AUDIO_FILE_NAME = "input_audio.wav";
        private const string CLUSTER_STDERR_FILE_NAME = "output_stderr.txt";
        private const string CLUSTER_STDOUT_FILE_NAME = "output_stdout.txt";

        // analysis program file names
        private const string PROGRAM_OUTPUT_FINISHED_FILE_NAME = "output_finishedmessage.txt";
        private const string PROGRAM_OUTPUT_RESULTS_FILE_NAME = "output_results.xml";

        /// <summary>
        /// Private ctor for Singleton pattern
        /// </summary>
        private Manager()
        {

        }


        #region Common

        public DirectoryInfo PrepareNewRun(AnalysisWorkItem workItem)
        {
            if (workItem == null) return null;

            var newRunDirString = DirRunBase.FullName + "\\" + workItem.JobItemId.ToString() + "-Run-" + Guid.NewGuid().ToString();

            try
            {

                // create new run folder
                var newRunDir = Directory.CreateDirectory(newRunDirString);


                // create settings file
                var settingsFile = newRunDir.FullName + "\\" + SETTINGS_FILE_NAME;
                File.WriteAllText(settingsFile, workItem.AnalysisRunSettings);


                // audio file location
                var audioFile = newRunDir.FullName + "\\" + AUDIO_FILE_NAME;

                // audio file must be wav file
                var audioFileUrl = workItem.AudioFileUri;
                if (!workItem.AudioFileUri.AbsoluteUri.EndsWith("wav") && workItem.AudioFileUri.AbsoluteUri.Contains('.'))
                {
                    audioFileUrl = new Uri(workItem.AudioFileUri.AbsoluteUri.Substring(0, workItem.AudioFileUri.AbsoluteUri.LastIndexOf('.')) + ".wav");
                }

                // download and save audio file
                var client = new System.Net.WebClient();
                client.DownloadFile(audioFileUrl, audioFile);

                return newRunDir;

            }
            catch
            {
                if (Directory.Exists(newRunDirString))
                {
                    Directory.Delete(newRunDirString, true);
                }
            }

            return null;
        }

        public FileInfo GetProgramFile(string version)
        {
            // program file from version

            // get program file location
            var programFile = DirProgramBase.FullName + "\\" + version + "\\" + ProgramName;

            // while testing, directly use debug version of AnalysisPrograms.exe
            //var programFile = DirProgramBase.FullName + "\\" + ProgramName;

            return new FileInfo(programFile);
        }

        public string CreateArgumentString(AnalysisWorkItem item, DirectoryInfo runDirectory)
        {

            return
                " processing " + // execute cluster version, not dev version
                " " + item.AnalysisGenericType + " " +// type of analysis to run
                " \"" + runDirectory.FullName + "\\" + SETTINGS_FILE_NAME + "\" " + // full path to settings file
                " \"" + runDirectory.FullName + "\\" + AUDIO_FILE_NAME + "\" " + // full path to audio file
                " \"" + PROGRAM_OUTPUT_RESULTS_FILE_NAME + "\" " + // results file name
                " \"" + PROGRAM_OUTPUT_FINISHED_FILE_NAME + "\" " // finished file name
                ;

            //return "dir /Q \"" + runDirectory.FullName + "\"";
        }



        public AnalysisWorkItem GetWorkItem(string workerName)
        {
            using (var ws = new ProcessorServiceWrapper())
            {
                try
                {
                    var item = ws.Proxy.GetWorkItem(new GetWorkItemRequest(workerName));
                    return item.GetWorkItemResult;
                }
                catch
                {
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
                catch
                {
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
                catch
                {
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
                catch
                {
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
            job.MaximumNumberOfProcessors = cluster.ClusterCounter.TotalNumberOfProcessors;
            job.MinimumNumberOfProcessors = 1;
            job.Name = "Processor " + DateTime.Now.ToString();
            job.Project = "QUT Sensors";

            return job;
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
            if (newRunDir == null || !newRunDir.Exists) return null;

            var programFile = GetProgramFile(item.AnalysisGenericVersion);
            if (programFile == null || !programFile.Exists) return null;

            var programArgs = CreateArgumentString(item, newRunDir);
            if (string.IsNullOrEmpty(programArgs)) return null;

            var task = PC_CreateTask(cluster);

            task.Name = item.AnalysisGenericType + " " + item.AnalysisGenericVersion + " " + DateTime.Now.ToString();
            task.WorkDirectory = programFile.DirectoryName;

            task.CommandLine = programFile.Name + " " + programArgs;
            //task.CommandLine = programArgs;

            task.Stderr = newRunDir.FullName + "\\" + CLUSTER_STDERR_FILE_NAME;
            task.Stdout = newRunDir.FullName + "\\" + CLUSTER_STDOUT_FILE_NAME;

            return task;
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

        public IJob PC_GetJob(ICluster cluster, int jobId)
        {
            return cluster.GetJob(jobId);
        }

        private object _timestamp;

        public ITask PC_GetFinishedTask(ICluster cluster, int jobId)
        {
            return cluster.CheckAnyTask(jobId, ref _timestamp);
        }

        public IEnumerable<DirectoryInfo> PC_GetFinishedRuns()
        {
            var finishedDirs = new List<DirectoryInfo>();

            foreach (var dir in DirRunBase.GetDirectories())
            {
                foreach (var file in dir.GetFiles("*.txt"))
                {
                    if (file.Name == PROGRAM_OUTPUT_FINISHED_FILE_NAME)
                    {
                        finishedDirs.Add(dir);
                        break;
                    }
                }
            }

            return finishedDirs.ToList();
        }

        public void PC_CompletedRun(DirectoryInfo runDir, string workerName)
        {
            var itemRunDetails = new StringBuilder();

            //get jobitemId from folder name
            int jobItemId = Convert.ToInt32(runDir.Name.Substring(0, runDir.Name.IndexOf("-")));

            try
            {
                // get output
                var runDirString = runDir.FullName + "\\";
                var resultFile = runDirString + PROGRAM_OUTPUT_RESULTS_FILE_NAME;
                var finishedFile = runDirString + PROGRAM_OUTPUT_FINISHED_FILE_NAME;
                var stderrFile = runDirString + CLUSTER_STDERR_FILE_NAME;
                var stdoutFile = runDirString + CLUSTER_STDOUT_FILE_NAME;

                if (File.Exists(finishedFile)) itemRunDetails.AppendLine("-->Finished Information: " + File.ReadAllText(finishedFile));
                if (File.Exists(stdoutFile)) itemRunDetails.AppendLine("-->Standard Out: " + File.ReadAllText(stdoutFile));
                if (File.Exists(stderrFile)) itemRunDetails.AppendLine("-->Standard Error: " + File.ReadAllText(stderrFile));

                if (!File.Exists(resultFile) || !File.Exists(finishedFile))
                {
                    this.ReturnIncomplete(
                        workerName,
                        jobItemId,
                        itemRunDetails.ToString(),
                        File.Exists(stderrFile)
                    );
                }
                else if (File.Exists(resultFile))
                {
                    this.ReturnComplete(
                        workerName,
                        jobItemId,
                        itemRunDetails.ToString(),
                        ProcessorResultTag.Read(resultFile)
                    );

                }

                // delete run directory
                if (Directory.Exists(runDirString))
                {
                    //Directory.Delete(runDirString, true);
                }
            }
            catch (Exception ex)
            {
                itemRunDetails.AppendLine("**Error Sending Completed: " + ex.ToString());

                this.ReturnIncomplete(
                        workerName,
                        jobItemId,
                        itemRunDetails.ToString(),
                        true
                    );
            }
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
                var programFile = GetProgramFile(workItem.AnalysisGenericVersion);
                var programArgs = CreateArgumentString(workItem, newRunDir);

                // create new processor
                return new ProcessItem(workItem, programFile, newRunDir, programArgs);
            }

            return null;
        }

        private void Dev_WorkerCompleted(ProcessItem pi, string workerName, int exitCode)
        {
            var itemRunDetails = new StringBuilder();

            try
            {
                // retrieve output

                var resultFile = pi.RunDir.FullName + "\\" + PROGRAM_OUTPUT_RESULTS_FILE_NAME;
                var finishedFile = pi.RunDir.FullName + "\\" + PROGRAM_OUTPUT_FINISHED_FILE_NAME;

                itemRunDetails.AppendLine("**Exit Code: " + exitCode);
                if (File.Exists(finishedFile)) itemRunDetails.AppendLine("**Finished Information: " + File.ReadAllText(finishedFile));
                if (!string.IsNullOrEmpty(pi.OutputData)) itemRunDetails.AppendLine("**Output: " + pi.OutputData);
                if (!string.IsNullOrEmpty(pi.ErrorData)) itemRunDetails.AppendLine("**Error: " + pi.ErrorData);


                if (!File.Exists(resultFile) || exitCode != 0)
                {
                    this.ReturnIncomplete(
                        workerName,
                        pi.WorkItem.JobItemId,
                        itemRunDetails.ToString(),
                        exitCode != 0
                    );
                }
                else if (File.Exists(resultFile))
                {
                    this.ReturnComplete(
                        workerName,
                        pi.WorkItem.JobItemId,
                        itemRunDetails.ToString(),
                        ProcessorResultTag.Read(resultFile)
                    );

                }

                // delete run directory
                if (Directory.Exists(pi.RunDir.FullName))
                {
                    //Directory.Delete(pi.RunDir.FullName, true);
                }
            }
            catch (Exception ex)
            {
                itemRunDetails.AppendLine("**Error Sending Completed: " + ex.ToString());

                this.ReturnIncomplete(
                        workerName,
                        pi.WorkItem.JobItemId,
                        itemRunDetails.ToString(),
                        true
                    );
            }
        }

        #endregion

    }
}
