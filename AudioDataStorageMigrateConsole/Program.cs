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
    using System.Security.Cryptography;
    using System.Text;

    using AudioTools.AudioUtlity;

    using Autofac;

    using QutSensors.Business;
    using QutSensors.Business.Storage;

    /// <summary>
    /// Migrator program.
    /// </summary>
    public static class Program
    {
        private static readonly MigrationWorker Worker;

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

            Worker = new MigrationWorker(logFileDir, sqlFs, fileSys, audioutil);
        }

        /// <summary>
        /// Main program entry.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        public static void Main(string[] args)
        {
            ////GenerateMachineKey();
            Worker.RunMigration();
        }

        private static void GenerateMachineKey(params string[] argv)
        {
            int len = 128;
            if (argv.Length > 0)
            {
                len = int.Parse(argv[0]);
            }

            var buff = new byte[len / 2];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(buff);
            }

            var sb = new StringBuilder(len);
            foreach (byte t in buff)
            {
                sb.Append(string.Format("{0:X2}", t));
            }

            Console.WriteLine(sb);
            Console.ReadLine();
        }
    }
}
