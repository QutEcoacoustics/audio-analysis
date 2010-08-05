// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Service.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the Service type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.AnalysisProcessor
{
    using System;
    using System.IO;
    using System.ServiceProcess;

    using QutSensors.AnalysisProcessor.Runners;
    using QutSensors.Shared.LogProviders;

    /// <summary>
    /// Analysis Processor Service.
    /// </summary>
    public partial class Service : ServiceBase
    {
        private AnalysisJobProcessor processor;

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
        /// <param name="exeBaseDir">
        /// The exe Base Dir.
        /// </param>
        /// <param name="runsDir">
        /// The runs Dir.
        /// </param>
        public void DebugStart(DirectoryInfo exeBaseDir, DirectoryInfo runsDir)
        {
            if (processor == null)
            {
                processor = new AnalysisJobProcessor(
                    exeBaseDir,
                    runsDir,
                    new MultiLogProvider(new TextFileLogProvider(), new ConsoleLogProvider()),
                    new LocalAnalysisRunner(),
                    "DEBUG_WORKER");
            }

            processor.StartSync();
        }

        /// <summary>
        /// Start service.
        /// </summary>
        /// <param name="args">
        /// String arguments.
        /// </param>
        protected override void OnStart(string[] args)
        {
            if (processor == null)
            {
                var workerName = !string.IsNullOrEmpty(Environment.MachineName)
                                     ? Environment.MachineName
                                     : "Analysis Job Processor Runner";

                var exeBaseDir = new DirectoryInfo(System.Configuration.ConfigurationManager.AppSettings["ProgramBaseDirectory"]);
                var runsDir = new DirectoryInfo(System.Configuration.ConfigurationManager.AppSettings["RunDirectory"]);

                var username = System.Configuration.ConfigurationManager.AppSettings["JobRunUserName"];
                var password = System.Configuration.ConfigurationManager.AppSettings["JobRunPassword"];

                processor = new AnalysisJobProcessor(
                    exeBaseDir,
                    runsDir,
                    new MultiLogProvider(new EventLogProvider(EventLog), new TextFileLogProvider()),
                    new ClusterAnalysisRunner(username, password),
                    workerName);
            }

            processor.Start();
        }

        /// <summary>
        /// Stop Service.
        /// </summary>
        protected override void OnStop()
        {
            processor.Stop();
        }
    }
}
