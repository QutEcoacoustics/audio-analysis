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
    using System.ServiceProcess;

    using Autofac;

    using QutSensors.Data;
    using QutSensors.Data.Cache;
    using QutSensors.Data.Providers;

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
                var servicesToRun = new ServiceBase[] { new Service() };
                ServiceBase.Run(servicesToRun);
            }
        }

        /// <summary>
        /// Set up Inversion of Control Container.
        /// </summary>
        private static void SetupIocContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<AudioReadingManager>()
                    .WithParameter(new NamedParameter("audioSegmentationPolicy", new SegmentCachePolicy()))
                    .WithParameter(new NamedParameter("spectrogramCachePolicy", new SpectrogramCachePolicy()))
                    .As<IAudioReadingManager>();
            builder.RegisterType<FileSystemDataStagingProvider>().As<IDataStagingProvider>();
            builder.RegisterType<AudioTools.DShowAudioMetadataProvider>().As<IAudioMetadataProvider>();
            builder.RegisterType<CacheManager>().As<ICacheManager>();
            builder.RegisterType<CacheJobProcessor>();
            QutDependencyContainer.Instance.Container = builder.Build();
        }
    }
}