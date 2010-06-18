// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Service.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the Service type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.Processor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceProcess;

    using QutSensors.Shared;

    /// <summary>
    /// Analysis Processor Service.
    /// </summary>
    public partial class Service : ServiceBase
    {
        private const string WorkerName = "WORKER_NAME";

        private const string WorkerNameString = "WorkerName";

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// Analysis Processor.
        /// </summary>
        public Service()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Start in debug mode - process one analysis task using console.
        /// </summary>
        public void DebugStart()
        {
            var workerName = Environment.GetEnvironmentVariable(WorkerName) ?? 
                             System.Configuration.ConfigurationManager.AppSettings[WorkerNameString] ?? 
                             Environment.MachineName;

            Console.WriteLine("Worker name: {0}", workerName);

            try
            {
                Console.WriteLine("Get work item.");
                var workItem = Manager.Instance.GetWorkItem(workerName);
                Console.WriteLine("Work item retrieved.");

                Console.WriteLine("Prepare work item.");
                var pi = Manager.Instance.Dev_PrepareItem(workItem);
                Console.WriteLine("Work item prepared.");

                Console.WriteLine("Start worker...");
                Manager.Instance.Dev_StartWorker(
                    workerName,
                    pi,
                    () =>
                        {
                            Console.WriteLine("Finished processing...");

                            Console.WriteLine("Get finished runs...");
                            var finishedRuns = Manager.Instance.GetFinishedRuns();

                            if (finishedRuns != null && finishedRuns.Count() > 0)
                            {
                                foreach (var finishedRunDir in finishedRuns)
                                {
                                    Manager.Instance.ReturnFinishedRun(finishedRunDir, workerName);
                                }

                                Console.WriteLine("Submitted " + finishedRuns.Count() + " completed runs.");
                            }
                            else
                            {
                                Console.WriteLine("No complete runs to submit.");
                            }
                        });
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: {0}", e);
            }

            /*
            var webServiceCallSuccess = Manager.Instance.ReturnComplete(
                        "TESTER",
                        0,
                        "Just Testing",
                        new List<ProcessorResultTag>
                        {
                            new ProcessorResultTag {  }
                        }
                    );
            */
        }

        /// <summary>
        /// Start service.
        /// </summary>
        /// <param name="args">
        /// String arguments.
        /// </param>
        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stop Service.
        /// </summary>
        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
            throw new NotImplementedException();
        }
    }
}
