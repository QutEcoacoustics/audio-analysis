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

    using QutSensors.Business;
    using QutSensors.Business.Analysis;
    using QutSensors.Business.Audio;
    using QutSensors.Business.Cache;
    using QutSensors.Business.Providers;
    using QutSensors.Shared.LogProviders;

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
            else if (args.Length > 0 && args[0].ToLower() == "debuglocal")
            {
                // TODO: set 'ConversionFolder' appsettings value to valid directory.
                var file =
                    @"C:\Documents and Settings\markcottmanf\My Documents\Sensor Projects\ProcessingTest\DM420003.MP3";

                var maxSegmentDurationMs = 1000 * 60 * 20; // 20 min

                var duration = new TimeSpan(23, 54, 59);

                var local = new LocalCacheJobProcessor(
                    file, maxSegmentDurationMs,
                    new TextFileLogProvider(@"C:\Documents and Settings\markcottmanf\My Documents\Sensor Projects\ProcessingTest\"), 
                    Convert.ToInt64(duration.TotalMilliseconds));

                local.Start();
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
            builder.RegisterType<DShowAudioMetadataProvider>().As<IAudioMetadataProvider>();
            builder.RegisterType<CacheManager>().As<ICacheManager>();
            builder.RegisterType<CacheJobProcessor>();
            builder.RegisterType<AudioTransformer>().As<IAudioTransformer>();
            QutDependencyContainer.Instance.Container = builder.Build();
        }
    }
}