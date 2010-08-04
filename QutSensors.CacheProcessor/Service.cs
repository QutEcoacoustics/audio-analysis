// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Service.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the Service type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.CacheProcessor
{
    using System.ServiceProcess;

    using Autofac;

    using QutSensors.Data;
    using QutSensors.Shared.LogProviders;

    /// <summary>
    /// Cache Processor Service.
    /// </summary>
    public partial class Service : ServiceBase
    {
        private CacheJobProcessor processor;

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// Cache Processor.
        /// </summary>
        public Service()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Start in debug mode - use WebDev process.
        /// </summary>
        public void DebugStart()
        {
            if (processor == null)
            {
                processor =
                    CreateProcessor(new MultiLogProvider(new EventLogProvider(EventLog), new ConsoleLogProvider()));
            }

            processor.Start();
        }

        /// <summary>
        /// Stop debug mode.
        /// </summary>
        public void DebugStop()
        {
            processor.Stop();
        }

        /// <summary>
        /// Service Start.
        /// </summary>
        /// <param name="args">
        /// String args.
        /// </param>
        protected override void OnStart(string[] args)
        {
            if (processor == null)
            {
                processor =
                    CreateProcessor(new MultiLogProvider(new EventLogProvider(EventLog), new TextFileLogProvider()));
            }

            processor.Start();
        }

        /// <summary>
        /// Service Stop.
        /// </summary>
        protected override void OnStop()
        {
            processor.Stop();
        }

        private static CacheJobProcessor CreateProcessor(ILogProvider log)
        {
            return QutDependencyContainer.Instance.Container.Resolve<CacheJobProcessor>(new[] { new NamedParameter("log", log) });
        }
    }
}