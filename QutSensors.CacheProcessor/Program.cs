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
    using System.IO;
    using System.ServiceProcess;

    using Autofac;

    using QutSensors.Business;
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
            else if (argument.Length > 0 && argument == "debuglocal")
            {
                DebugLocal();
            }
            else if (argument.Length > 0 && argument == "splitlocal")
            {
                TestLocalMp3Split();
            }
            else if (argument.Length > 0 && argument == "splitlocalsegment")
            {
                TestLocalMp3SplitSingleSegment();
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

        private static void TestLocalMp3Split()
        {
            var mp3spltExe = @"C:\Documents and Settings\markcottmanf\My Documents\Sensor Projects\ProcessingTest\mp3splt_2.2.8_i386\mp3splt.exe";
            var mp3splt = new SplitMp3(mp3spltExe)
            {
                Mp3FileName =
                    new FileInfo("DM420003.MP3"),
                SegmentSizeMinutes = 200,
                SegmentSizeSeconds = 0,
                SegmentSizeHundredths = 0,
                WorkingDirectory =
                   new DirectoryInfo(@"C:\Documents and Settings\markcottmanf\My Documents\Sensor Projects\ProcessingTest\")
            };

            var files = mp3splt.Run();
        }

        private static void TestLocalMp3SplitSingleSegment()
        {
            var mp3spltExe = @"C:\Documents and Settings\markcottmanf\My Documents\Sensor Projects\ProcessingTest\mp3splt_2.2.8_i386\mp3splt.exe";
            var mp3splt = new SplitMp3(mp3spltExe)
            {
                Mp3FileName = new FileInfo("DM420003.MP3")
            };

            var start = 61010; // 1 min, 1 sec 10 ms
            var end = 305050; //5 min, 5 sec, 50 ms

            //var start = 12000000; // 200min
            //var end = 24000000;  //400 min

            //var start = 999;
            //var end = 300999; 

            var tempfile = @"C:\Documents and Settings\markcottmanf\My Documents\Sensor Projects\ProcessingTest\tempfile.mp3";

            var file = mp3splt.SingleSegment(tempfile, start, end);
        }

        private static void DebugLocal()
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
    }
}