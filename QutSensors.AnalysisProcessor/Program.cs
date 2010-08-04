// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.AnalysisProcessor
{
    using System;
    using System.IO;
    using System.ServiceProcess;

    /// <summary>
    /// Analysis Job Processor Program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">
        /// Program Arguments.
        /// </param>
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].ToLower() == "debug")
            {
                if (args.Length != 3)
                {
                    Console.WriteLine("Running in debug mode requires 3 arguments:");
                    Console.WriteLine("(1) 'debug' ");
                    Console.WriteLine("(2) Executable Program Base Directory ");
                    Console.WriteLine("(3) Run top level directory ");
                    return;
                }

                var service = new Service();
                service.DebugStart(new DirectoryInfo(args[1]), new DirectoryInfo(args[2]));
                Console.WriteLine("Program done. Press enter to exit.");
                Console.ReadLine();
            }
            else
            {
                var servicesToRun = new ServiceBase[] { new Service() };
                ServiceBase.Run(servicesToRun);
            }
        }
    }
}
