using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QutSensors.Processor;
using QutSensors.Shared;

namespace ProcessorCLI
{
    using Utilities = QutSensors.Shared.Utilities;

    class Program
    {
        public static void Main(string[] args)
        {
            RunOnce(args);
            //TestCompleteReturn();
        }

        private const string WORKER_NAME_KEY = "WorkerName";

        private static void RunOnce(string[] args)
        {
            string workerName = System.Environment.GetEnvironmentVariable("WORKER_NAME") ?? System.Configuration.ConfigurationManager.AppSettings[WORKER_NAME_KEY] ?? System.Environment.MachineName;
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
                Manager.Instance.Dev_StartWorker(workerName, pi, () =>
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

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        private static void TestCompleteReturn()
        {
            var webServiceCallSuccess = Manager.Instance.ReturnComplete(
                        "TESTER",
                        0,
                        "Just Testing",
                        new List<ProcessorResultTag>
                        {
                            new ProcessorResultTag {  }
                        }
                    );
        }

    }
}
