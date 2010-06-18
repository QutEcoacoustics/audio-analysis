// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ProcessorCLI
{
    using System;
    using System.ServiceProcess;

    using QutSensors.Processor;

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
                var service = new Service();
                service.DebugStart();
                Console.WriteLine("Press enter to exit.");
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
