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
    using System.IO;
    using System.Linq;
    using System.ServiceProcess;

    using Autofac;

    using QutSensors.Business;
    using QutSensors.Business.Audio;
    using QutSensors.Business.Cache;
    using QutSensors.Business.Providers;
    using QutSensors.Business.Storage;

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
            builder.RegisterType<AudioToolDirectShow>().As<IAudioTool>();
            builder.RegisterType<CacheTransformer>();
            builder.RegisterType<TowseySignalToImage>().As<ISignalToImage>();

            // this can be changed to MigrateAudioDataStorage or FileSystemAudioDataStorage as needed for extracting
            // audio from db.
            builder.RegisterType<SqlFileStreamAudioDataStorage>().As<IAudioDataStorage>();

            QutDependencyContainer.Instance.Container = builder.Build();
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