// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainForm.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the MainForm type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ProcessorUI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;

    using Microsoft.ComputeCluster;

    using QutSensors.Processor;
    using QutSensors.Shared;

    /// <summary>
    /// Processor UI Form.
    /// </summary>
    public partial class MainForm : Form
    {
        private const string ActionIntervalMilliseconds = "ActionIntervalMilliseconds";

        // finished jobs hang arround for 5 days (TTLCompletedJobs setting).
        private readonly string workerName;

        private readonly System.Timers.Timer timer;
        private readonly double timerIntervalMilliseconds;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            this.workerName = Environment.MachineName;
            lblWorkerName.Text = this.workerName;

            try
            {
                using (var ws = new ProcessorServiceWrapper())
                {
                    lbWebserviceUrl.Text = ws.Proxy.Endpoint.Address.Uri.AbsoluteUri.Substring(0, 40) + "...";
                }
            }
            catch
            {
                lbWebserviceUrl.Text = "unknown";
            }

            this.timerIntervalMilliseconds = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings[ActionIntervalMilliseconds]);

            this.timer = new System.Timers.Timer
                {
                    AutoReset = true
                };

            this.timer.Elapsed += this.TimerElapsed;
            this.timer.Interval = this.timerIntervalMilliseconds;
            this.timer.SynchronizingObject = this;

            /* // enumerate cluster settings
            foreach (NameValue item in _cluster.Parameters)
            {
                Log(this, item.Name + " = " + item.Value);
            }
            */
        }

        /// <summary>
        /// Start the processor.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// Event Arguments.
        /// </param>
        protected void CmdStartClick(object sender, EventArgs e)
        {
            if (this.timer.Enabled)
            {
                // timer is running
                Log(this, "Stopping...");
                cmdStart.Text = "&Start";
                this.timer.Stop();
            }
            else
            {
                Log(this, "DeleteFinishedRuns: " + Manager.Instance.DeleteFinishedRuns);
                Log(this, "Started...");
                cmdStart.Text = "&Stop";

                // start straight away
                var doWork = new WaitCallback(TickAction);
                ThreadPool.QueueUserWorkItem(doWork);

                this.timer.Start();
            }
        }

        /// <summary>
        /// Log an event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="log">
        /// Message to log.
        /// </param>
        protected void Log(object sender, string log)
        {
            if (InvokeRequired)
            {
                BeginInvoke((EventHandler)delegate { Log(sender, log); });
            }
            else
            {
                if (txtLog.Text.Length > 0)
                {
                    txtLog.AppendText(Environment.NewLine);
                }

                txtLog.AppendText(DateTime.Now.ToString("HH:mm:ss") + ": " + log);

                if (txtLog.Text.Length > 10000)
                {
                    txtLog.Text = txtLog.Text.Substring(2000);
                }
            }
        }

        /// <summary>
        /// Timer elapsed event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// Elapsed event arguments.
        /// </param>
        protected void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((EventHandler)delegate { this.TimerElapsed(sender, e); });
            }
            else
            {
                var doWork = new WaitCallback(TickAction);
                ThreadPool.QueueUserWorkItem(doWork);
            }
        }

        private void TickAction(object obj)
        {
            // action to take when _timer 'ticks'
            Log(this, "*** Performing submit and retrieve... ***");

            using (ICluster cluster = new Cluster())
            {
                // 1. check for complete runs and send them back to the database.
                SubmitCompleteRuns();

                // 2. obtain and run more jobitems
                var maxItems = cluster.ClusterCounter.NumberOfIdleProcessors -
                               cluster.ClusterCounter.NumberOfUnreachableProcessors;

                // get analysisItems from web service
                var workItems = GetWorkItems(maxItems);

                if (workItems != null && workItems.Count() > 0)
                {
                    // create a new job on the cluster head node
                    var newJob = CreateNewJob(cluster);

                    if (newJob != null)
                    {
                        // create ITasks from AnalysisItems
                        var preparedTasks = this.PrepareTasks(cluster, workItems);
                        if (preparedTasks != null && preparedTasks.Count() > 0)
                        {
                            this.Log(this, "Add new tasks to job...");

                            // add all new ITasks to new IJob
                            foreach (var task in preparedTasks)
                            {
                                newJob.AddTask(task);
                            }

                            this.Log(this, string.Format("{0} tasks added to job.", preparedTasks.Count()));

                            if (newJob.TaskCount > 0)
                            {
                                this.Log(this, "Queuing job...");

                                // set the max num processors based on the number of tasks in the job.
                                newJob.MaximumNumberOfProcessors = newJob.TaskCount;

                                // set the job running
                                int newJobId = Manager.Instance.PC_RunJob(cluster, newJob);

                                this.Log(this, string.Format("Queued new job {0} with id {1} containing {2} tasks.", newJob.Name, newJobId, preparedTasks.Count()));
                            }
                            else
                            {
                                this.Log(this, "Job not queued as it contains no tasks.");
                            }
                        }
                        else
                        {
                            this.Log(this, "No tasks prepared.");
                        }
                    }
                }
            }

            Log(this, "*** Submit and retrieve complete. ***" + Environment.NewLine + Environment.NewLine);
        }

        private void SubmitCompleteRuns()
        {
            Log(this, "Retrieving complete runs...");

            var finishedRuns = Manager.Instance.GetFinishedRuns();

            if (finishedRuns != null && finishedRuns.Count() > 0)
            {
                foreach (var finishedRunDir in finishedRuns)
                {
                    Manager.Instance.PC_CompletedRun(finishedRunDir, this.workerName);
                }

                Log(this, string.Format("Submitted {0} completed runs.", finishedRuns.Count()));
            }
            else
            {
                Log(this, "No complete runs to submit.");
            }
        }

        private IEnumerable<AnalysisWorkItem> GetWorkItems(int maxItems)
        {
            Log(this, "Get new work items...");

            var workItems = Manager.Instance.GetWorkItems(this.workerName, maxItems);

            if (workItems == null || workItems.Count() == 0)
            {
                Log(this, "No new work items available from web service.");
                return null;
            }

            Log(this, string.Format("Retrieved {0} new work items from web service. {1} processors available.", workItems.Count(), maxItems));
            return workItems;
        }

        private IJob CreateNewJob(ICluster cluster)
        {
            Log(this, "Create new job...");

            IJob newJob = Manager.Instance.PC_NewJob(cluster);

            if (newJob == null)
            {
                const string Msg = "Could not create new job.";
                Log(this, Msg);
                Manager.Instance.Log(Msg);
            }

            return newJob;
        }

        private IEnumerable<ITask> PrepareTasks(ICluster cluster, IEnumerable<AnalysisWorkItem> workItems)
        {
            Log(this, "Prepare tasks...");

            if (workItems == null || workItems.Count() == 0) return null;

            var preparedTasks = new List<ITask>();
            int problemCount = 0;

            foreach (var workItem in workItems)
            {
                var item = Manager.Instance.PC_PrepareTask(cluster, workItem);
                if (item != null)
                {
                    preparedTasks.Add(item);
                }
                else
                {
                    problemCount++;
                }
            }

            var msg = string.Format("Prepared {0} out of {1} tasks.", preparedTasks.Count, workItems.Count());
            if (problemCount > 0)
            {
                msg += string.Format(" {0} tasks were not prepared due to errors.", problemCount);
            }

            Log(this, msg);

            return preparedTasks;
        }
    }
}