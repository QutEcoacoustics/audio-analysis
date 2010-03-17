using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QutSensors.Processor;

namespace ProcessorCLI
{
    class Program
    {
        private const string WORKER_NAME_KEY = "WorkerName";

        public static void Main(string[] args)
        {
            var path = @"\\131.181.206.254\Sensors\AnlaysisRoot\task add command.txt";
            var exists = System.IO.File.Exists(path);

            Console.WriteLine();

            /*
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
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: {0}", e.ToString());
            }
            Console.WriteLine("Press enter to exit.");
            
             
             */
            Console.ReadLine();

        }
    }
}
