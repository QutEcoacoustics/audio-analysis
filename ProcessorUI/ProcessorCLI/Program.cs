using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using QutSensors.Processor;
using QutSensors.Processor.WebServices;

namespace ProcessorCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            string workerName = System.Environment.GetEnvironmentVariable("WORKER_NAME") ?? Settings.WorkerName ?? System.Environment.MachineName;
            Console.WriteLine("Worker name: {0}", workerName);

            try
            {
                ProcessorJobItemDescription item = Manager.GetJobItem(workerName);
                if (item == null)
                {
                    Console.WriteLine("Failed to get item");
                    return;
                }

                TimeSpan? duration;
                bool success = Manager.ProcessItem(item, workerName, out duration);
                Console.WriteLine(success ? "OK" : "Failed to process");
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: {0}", e.ToString());
            }
        }
    }
}
