// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClusterAnalysisRunner.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the ClusterAnalysisRunner type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.AnalysisProcessor.Runners
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.ComputeCluster;

    using QutSensors.Shared;

    /// <summary>
    /// Runs analysis items on MS Processing Cluster.
    /// </summary>
    public class ClusterAnalysisRunner : AnalysisRunnerBase
    {
        private readonly string userName;

        private readonly string password;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterAnalysisRunner"/> class.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        public ClusterAnalysisRunner(string userName, string password)
        {
            this.userName = userName;
            this.password = password;
        }

        /// <summary>
        /// Gets the maxmimum number of items this runner can allocate at this time.
        /// </summary>
        /// <returns>Maximum number of items.</returns>
        public override int MaxAllocations
        {
            get
            {
                try
                {
                    using (ICluster cluster = new Cluster())
                    {
                        return cluster.ClusterCounter.NumberOfIdleProcessors -
                               cluster.ClusterCounter.NumberOfUnreachableProcessors;
                    }
                }
                catch
                {
                    return 0;
                }
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

            using (ICluster cluster = new Cluster())
            {
                // create and set cluster job details.
                var job = cluster.CreateJob();
                job.IsExclusive = false;
                job.MinimumNumberOfProcessors = 1;
                job.Name = "Processor " + DateTime.Now;
                job.Project = "QUT Sensors";

                foreach (var item in workItems)
                {
                    try
                    {
                        ITask task = cluster.CreateTask();
                        task.IsExclusive = false;
                        task.IsRerunnable = false;

                        task.MaximumNumberOfProcessors = 1;
                        task.MinimumNumberOfProcessors = 1;

                        task.Name = item.WorkItemName;
                        task.WorkDirectory = item.WorkingDirectory.FullName;

                        task.CommandLine = item.ApplicationFile.FullName + " " + item.Arguments;

                        task.Stderr = item.StandardErrorFile.FullName;
                        task.Stdout = item.StandardOutputFile.FullName;

                        job.AddTask(task);
                    }
                    catch
                    {
                        // log error?
                        // task count will be less than workItems count.
                    }
                }

                if (job.TaskCount == 0)
                {
                    return 0;
                }

                job.MaximumNumberOfProcessors = job.TaskCount;

                // add job to cluster - job does not start, just so cluster knows it is there.
                var jobId = cluster.AddJob(job);

                // job is owned by specified user
                cluster.SetJobCredentials(jobId, userName, password);

                // submit job as specified user - starts the tasks in the job when processors are available.
                cluster.SubmitJob(jobId, userName, password, false, 0);

                return job.TaskCount;
            }
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
        /// <exception cref="FileNotFoundException">
        /// Executable file is required.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <c>InvalidOperationException</c>.
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

            // check version info
            if (string.IsNullOrEmpty(analysisWorkItem.AnalysisGenericVersion))
            {
                throw new InvalidOperationException("'AnalysisGenericVersion' was not given.");
            }

            // check executable exists at location specified.
            var dir = Path.Combine(executableBaseDirectory.FullName, analysisWorkItem.AnalysisGenericVersion);
            var programFile = new FileInfo(Path.Combine(dir, this.AnalysisProgramsExeFileName));
            if (!programFile.Exists)
            {
                throw new FileNotFoundException("Executable file is required.", programFile.FullName);
            }

            return programFile;
        }
    }
}
