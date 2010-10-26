// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Migrator program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioDataStorageMigrateConsole
{
    using System;
    using System.Configuration;

    using AudioTools.AudioUtlity;

    using Autofac;

    using QutSensors.Business;
    using QutSensors.Business.Storage;
    using QutSensors.Shared.LogProviders;

    /// <summary>
    /// Migrator program.
    /// </summary>
    public class Program
    {
        private static readonly MigrationWorker Worker;

        private static readonly ILogProvider LogProvider;

        /// <summary>
        /// Initializes static members of the <see cref="Program"/> class.
        /// </summary>
        static Program()
        {
            string ffmpegExe = ConfigurationManager.AppSettings["AudioUtilityFfmpegExe"];
            string wvunpackExe = ConfigurationManager.AppSettings["AudioUtilityWvunpackExe"];
            string mp3SpltExe = ConfigurationManager.AppSettings["AudioUtilityMp3SpltExe"];

            string audioDataStorageDir = ConfigurationManager.AppSettings["AudioDataStorageDir"];

            string tempFileUploadDir = ConfigurationManager.AppSettings["UploadFolder"];

            string logFileDir = ConfigurationManager.AppSettings["LogFileDir"];

            QutDependencyContainer.Instance.BuildIoCContainer(
                ffmpegExe,
                wvunpackExe,
                mp3SpltExe,
                audioDataStorageDir,
                tempFileUploadDir);

            var sqlFs = QutDependencyContainer.Instance.Container.Resolve<SqlFileStreamAudioDataStorage>();
            var fileSys = QutDependencyContainer.Instance.Container.Resolve<FileSystemAudioDataStorage>();
            var audioutil = QutDependencyContainer.Instance.Container.Resolve<IAudioUtility>();

            LogProvider = new MultiLogProvider(new TextFileLogProvider(logFileDir), new ConsoleLogProvider());

            Worker = new MigrationWorker(LogProvider, sqlFs, fileSys, audioutil);
        }

        /// <summary>
        /// Main program entry.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        public static void Main(string[] args)
        {
            bool successfulRun;
            MigrationInfo info = null;

            do
            {
                successfulRun = true;

                try
                {
                    info = Worker.MigrateSingleAudioReading();
                }
                catch
                {
                    successfulRun = false;
                }

                // only stop when info is null and there was no error.
            }
            while (!(info == null && successfulRun));
        }
    }
}
