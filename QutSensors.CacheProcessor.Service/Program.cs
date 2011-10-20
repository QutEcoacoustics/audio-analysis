// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.CacheProcessor
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.ServiceProcess;

    using QutSensors.Business;

    /// <summary>
    /// Cache Job Processor Program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">
        /// Program Arguments.
        /// </param>
        public static void Main(string[] args)
        {
            RunService(args);
            ////Export();
        }

        private static void RunService(string[] args)
        {
            SetupIocContainer();

            var argument = string.Empty;

            if (args != null && args.Length == 1)
            {
                argument = args[0].ToLower();
            }

            if (argument.Length > 0 && argument == "debug")
            {
                var service = new Service();
                service.DebugStart();
                Console.WriteLine("Press ENTER to stop processor.");
                Console.ReadLine();
                service.DebugStop();
            }
            else
            {
                var servicesToRun = new ServiceBase[] { new Service() };
                ServiceBase.Run(servicesToRun);
            }
        }

        /// <summary>
        /// Set up Inversion of Control Container.
        /// </summary>
        private static void SetupIocContainer()
        {
            string ffmpegExe = ConfigurationManager.AppSettings["AudioUtilityFfmpegExeFullPath"];
            string wvunpackExe = ConfigurationManager.AppSettings["AudioUtilityWvunpackExeFullPath"];
            string mp3SpltExe = ConfigurationManager.AppSettings["AudioUtilityMp3SpltExeFullPath"];

            string audioDataStorageDir = ConfigurationManager.AppSettings["AudioDataStorageDir"];

            QutDependencyContainer.Instance.BuildIoCContainer(ffmpegExe, wvunpackExe, mp3SpltExe, audioDataStorageDir);
        }

        private static void Export()
        {
            const string ConversionfolderKey = "ConversionFolder";

            var conversionPath = ConfigurationManager.AppSettings.AllKeys.Contains(ConversionfolderKey)
                                     ? ConfigurationManager.AppSettings[ConversionfolderKey]
                                     : string.Empty;

            CacheUtilities.Export(36, conversionPath);
        }
    }
}