using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using QutSensors.Processor;
using Microsoft.ComputeCluster;
using QutSensors.Shared;
using System.Threading;

namespace ProcessorUI
{
    public partial class MainForm : Form
    {
        // finished jobs hang arround for 5 days (TTLCompletedJobs setting).
        private string _workerName;

        private System.Timers.Timer _timer;
        private double _timerIntervalMilliseconds;

        public MainForm()
        {
            InitializeComponent();

            _workerName = System.Environment.MachineName;
            lblInfo.Text = "Name: " + _workerName;

            _timerIntervalMilliseconds = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["ActionIntervalMilliseconds"]);

            _timer = new System.Timers.Timer();
            _timer.AutoReset = true;
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(_timer_Elapsed);
            _timer.Interval = _timerIntervalMilliseconds;
            _timer.SynchronizingObject = this;


            /* // enumerate cluster settings
            foreach (NameValue item in _cluster.Parameters)
            {
                Log(this, item.Name + " = " + item.Value);
            }
            */

        }

        protected void cmdStart_Click(object sender, EventArgs e)
        {
            if (_timer.Enabled)
            {
                // timer is running
                Log(this, "Stopping...");
                cmdStart.Text = "&Start";
                _timer.Stop();

            }
            else
            {
                Log(this, "DeleteFinishedRuns: " + Manager.Instance.DeleteFinishedRuns);
                Log(this, "Started...");
                cmdStart.Text = "&Stop";

                _timer.Start();
            }
        }

        protected void Log(object sender, string log)
        {
            if (InvokeRequired)
            {
                BeginInvoke((EventHandler)delegate { Log(sender, log); });
            }
            else
            {
                if (txtLog.Text.Length > 0) txtLog.AppendText(Environment.NewLine);
                txtLog.AppendText(DateTime.Now.ToString("HH:mm:ss") + ": " + log);

                if (txtLog.Text.Length > 10000) txtLog.Text = txtLog.Text.Substring(2000);
            }
        }

        protected void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((EventHandler)delegate { _timer_Elapsed(sender, e); });
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
            Log(this, "Performing submit and retrieve...");

            using (ICluster cluster = new Cluster())
            {

                // 1. check for complete runs and send them back to the database.
                SubmitCompleteRuns();

                // 2. obtain and run more jobitems

                var maxItems = cluster.ClusterCounter.NumberOfIdleProcessors - cluster.ClusterCounter.NumberOfUnreachableProcessors;

                // get analysisItems from web service
                var workItems = GetWorkItems(cluster, maxItems);

                // create a new job on the cluster head node
                var newJob = CreateNewJob(cluster);

                if (newJob != null)
                {
                    IEnumerable<ITask> preparedTasks = new List<ITask>();

                    if (workItems != null)
                    {
                        // create ITasks from AnalysisItems
                        preparedTasks = PrepareTasks(cluster, workItems);
                        if (preparedTasks != null && preparedTasks.Count() > 0)
                        {
                            Log(this, "Add new tasks to job...");

                            // add all new ITasks to new IJob
                            foreach (var task in preparedTasks)
                            {
                                newJob.AddTask(task);
                            }

                            Log(this, preparedTasks.Count() + " tasks added to job.");

                            if (newJob.TaskCount > 0)
                            {
                                Log(this, "Queuing job...");

                                // set the max num processors based on the number of tasks in the job.
                                newJob.MaximumNumberOfProcessors = newJob.TaskCount;

                                // set the job running
                                int newJobId = Manager.Instance.PC_RunJob(cluster, newJob);

                                Log(this, "Queued new job " + newJob.Name + " with id " + newJobId + ". It contains " + preparedTasks.Count() + " tasks.");
                            }
                            else
                            {
                                Log(this, "Job not queued as it contains 0 tasks.");
                            }
                        }
                        else
                        {
                            Log(this, "No tasks prepared.");
                        }
                    }
                }
            }
        }

        private void SubmitCompleteRuns()
        {
            Log(this, "Retrieving complete runs...");

            var finishedRuns = Manager.Instance.GetFinishedRuns();

            if (finishedRuns != null && finishedRuns.Count() > 0)
            {
                foreach (var finishedRunDir in finishedRuns)
                {
                    Manager.Instance.PC_CompletedRun(finishedRunDir, _workerName);
                }

                Log(this, "Submitted " + finishedRuns.Count() + " completed runs.");
            }
            else
            {
                Log(this, "No complete runs to submit.");
            }
        }

        private IEnumerable<AnalysisWorkItem> GetWorkItems(ICluster cluster, int maxItems)
        {
            Log(this, "Get new work items...");

            var workItems = Manager.Instance.GetWorkItems(_workerName, maxItems);

            if (workItems == null || workItems.Count() == 0)
            {
                Log(this, "No new work items available from web service. " + maxItems + " processors available.");
            }
            else
            {
                Log(this, "Retrieved " + workItems.Count() + " new work items from web service. " + maxItems + " processors available.");
            }

            return workItems;
        }

        private IJob CreateNewJob(ICluster cluster)
        {
            Log(this, "Create new job...");

            IJob newJob = Manager.Instance.PC_NewJob(cluster);

            if (newJob == null)
            {
                Log(this, "Could not create new job.");
            }

            return newJob;
        }

        private IEnumerable<ITask> PrepareTasks(ICluster cluster, IEnumerable<AnalysisWorkItem> workItems)
        {
            Log(this, "Prepare tasks...");

            if (workItems == null || workItems.Count() == 0) return null;

            List<ITask> preparedTasks = new List<ITask>();
            int problemCount = 0;

            foreach (var workItem in workItems)
            {
                if (workItem != null)
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
            }


            if (preparedTasks == null || (preparedTasks.Count() == 0 && workItems.Count() > 0))
            {
                Log(this, "Problem converting AnalysisItems to ITasks - no tasks prepared.");
            }
            else
            {
                var msg = "Prepared " + preparedTasks.Count + " tasks.";
                if (problemCount > 0) msg += " " + problemCount + " tasks could not be prepared.";
                Log(this, msg);
            }

            return preparedTasks;
        }
    }
}