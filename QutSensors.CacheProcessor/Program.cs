using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Autofac;
using Autofac.Core;
using QutSensors.Data;
using QutSensors.Data.Providers;
using QutSensors.Data.Cache;

namespace QutSensors.CacheProcessor
{
    static class Program
    {
        static void SetupIocContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<AudioReadingManager>()
                    .WithParameter(new NamedParameter("audioSegmentationPolicy", new QutSensors.Data.Cache.SegmentCachePolicy()))
                    .WithParameter(new NamedParameter("spectrogramCachePolicy", new QutSensors.Data.Cache.SpectrogramCachePolicy()))
                    .As<IAudioReadingManager>();
            builder.RegisterType<FileSystemDataStagingProvider>().As<IDataStagingProvider>();
            builder.RegisterType<AudioTools.DShowAudioMetadataProvider>().As<IAudioMetadataProvider>();
            builder.RegisterType<CacheManager>().As<ICacheManager>();
            builder.RegisterType<CacheJobProcessor>();
            QutDependencyContainer.Instance.Container = builder.Build();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            SetupIocContainer();

            if (args.Length > 0 && args[0].ToLower() == "debug")
            {
                var service = new Service();
                service.DebugStart();
                Console.WriteLine("Press ENTER to stop processor.");
                Console.ReadLine();
                service.DebugStop();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
			    { 
				    new Service() 
			    };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}