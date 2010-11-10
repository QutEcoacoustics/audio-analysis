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
    using System.Diagnostics;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    using AudioTools.AudioUtlity;

    using Autofac;

    using QutSensors.Business;
    using QutSensors.Business.Audio;
    using QutSensors.Business.Storage;
    using QutSensors.Shared.LogProviders;

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
            ////GenerateMachineKey("64");
            ////GetSpectrogram();
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

            File.WriteAllText(@"C:\key.txt", sb.ToString());
            Console.WriteLine(sb);
            Console.ReadLine();
        }

        private static void GetSpectrogram()
        {
            string dir =
                @"C:\Documents and Settings\markcottmanf\My Documents\Sensor Projects\SensorsTrunk\QutSensors.Test\TestData";

            ISignalToImage web = new WebSignalToImage();

            ISignalToImage towsey = new TowseySignalToImage();

            ILogProvider log = new TextFileLogProvider(dir);

            var watch = new Stopwatch();

            foreach (FileInfo file in new DirectoryInfo(dir).GetFiles("*.wav"))
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(file.FullName);

                    watch.Restart();

                    using (var image = web.Spectrogram(bytes))
                    {
                        watch.Stop();
                        log.WriteEntry(LogType.Information, "Web: {0} - {1}", Path.GetFileNameWithoutExtension(file.FullName), watch.Elapsed);

                        image.Save(Path.Combine(dir, Path.GetFileNameWithoutExtension(file.FullName) + "-web.jpg"), ImageFormat.Jpeg);
                    }

                    watch.Restart();

                    using (var image = towsey.Spectrogram(bytes))
                    {
                        watch.Stop();
                        log.WriteEntry(LogType.Information, "Towsey: {0} - {1}", Path.GetFileNameWithoutExtension(file.FullName), watch.Elapsed);

                        image.Save(Path.Combine(dir, Path.GetFileNameWithoutExtension(file.FullName) + "-towsey.jpg"), ImageFormat.Jpeg);
                    }
                }
                catch (Exception ex)
                {
                    log.WriteEntry(LogType.Error, "Error reading {0} - {1}", file.Name, ex.Message);
                }
            }

            Console.ReadLine();
        }

        /*
        select COUNT(*), datalocation, cast(SUM(cast(length as bigint))/1000/60/60 as varchar(100))+' hrs', MimeType
from audioreadings
group by datalocation, mimetype
order by DATALocation, MimeType
    */
    }
}
