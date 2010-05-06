using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Autofac;
using QutSensors.Data;
using Autofac.Core;

namespace QutSensors.CacheProcessor
{
    public partial class Service : ServiceBase
    {
        CacheJobProcessor processor;

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (processor == null)
                processor = CreateProcessor(new MultiLogProvider(new EventLogProvider(EventLog), new TextFileLogProvider()));
            processor.Start();
        }

        protected override void OnStop()
        {
            processor.Stop();
        }

        public void DebugStart()
        {
            if (processor == null)
                processor = CreateProcessor(new MultiLogProvider(new EventLogProvider(EventLog), new ConsoleLogProvider()));
            processor.Start();
        }

        public void DebugStop()
        {
            processor.Stop();
        }

        static CacheJobProcessor CreateProcessor(ILogProvider log)
        {
            return QutDependencyContainer.Instance.Container.Resolve<CacheJobProcessor>(new[] { new NamedParameter("log", log) });
        }
    }
}