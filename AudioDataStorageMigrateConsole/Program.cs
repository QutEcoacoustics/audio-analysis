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
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    using AudioDataStorageMigrateConsole.Diag;

    using AudioTools.AudioUtlity;
    using AudioTools.WavAudio;

    using Autofac;

    using QutSensors.Business;
    using QutSensors.Business.Audio;
    using QutSensors.Business.Storage;
    using QutSensors.Shared;
    using QutSensors.Shared.LogProviders;

    /// <summary>
    /// Migrator program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main program entry.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        public static void Main(string[] args)
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

            // var sqlFs = QutDependencyContainer.Instance.Container.Resolve<SqlFileStreamAudioDataStorage>();
            var fileSys = QutDependencyContainer.Instance.Container.Resolve<FileSystemAudioDataStorage>();
            var audioutil = QutDependencyContainer.Instance.Container.Resolve<IAudioUtility>();

            //Worker = new MigrationWorker(logFileDir, sqlFs, fileSys, audioutil);

            using (var diag = new AudioReadingDataDiagnostic(logFileDir, fileSys, audioutil))
            {
                diag.Run();
            }

            ////GenerateMachineKey("64");

            ////GetSpectrogram();

            ////GenerateSpectrograms();

            //Worker.RunMigration();

            Console.WriteLine();
            Console.WriteLine("Done, press any key to close...");
            Console.ReadLine();

            //var asr = new AudioStreamReader();
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
                @"C:\QutSensors\trunk\WebFrontend\UI\Learning\Resources";

            ISignalToImage web = new WebSignalToImage();

            ISignalToImage towsey = new TowseySignalToImage();

            ILogProvider log = new TextFileLogProvider(dir);

            var watch = new Stopwatch();

            foreach (FileInfo file in new DirectoryInfo(dir).GetFiles("*.mp3"))
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
        }

        private static void GenerateSpectrograms()
        {
            var queue = new Queue<DirectoryInfo>();

            queue.Enqueue(new DirectoryInfo(@"C:\QutSensors\trunk\WebFrontend\UI\Learning\Resources\Eastern_Dwarf_Tree_Frog"));

            while (queue.Count > 0)
            {
                var nextDir = queue.Dequeue();

                foreach (FileInfo file in nextDir.GetFiles("*.mp3"))
                {
                    SaveSpectrogram(file);
                }

                foreach (DirectoryInfo dir in nextDir.GetDirectories())
                {
                    queue.Enqueue(dir);
                }
            }
        }

        private static void SaveSpectrogram(FileInfo audioFile)
        {
            ISignalToImage web = new WebSignalToImage();
            var audioUtility = QutDependencyContainer.Instance.Container.Resolve<IAudioUtility>();

            string uniqueString = Guid.NewGuid().ToString().Substring(0, 4);

            FileInfo outputAudioFile =
                new FileInfo(
                    Path.Combine(
                        audioFile.DirectoryName,
                        Path.GetFileNameWithoutExtension(audioFile.Name) + "-" + uniqueString + "." +
                        MimeTypes.ExtWav));

            audioUtility.Convert(
                audioFile,
                MimeTypes.GetMimeTypeFromExtension(audioFile.Extension),
                outputAudioFile,
                MimeTypes.MimeTypeWav);

            byte[] bytes = File.ReadAllBytes(outputAudioFile.FullName);

            using (var image = web.Spectrogram(bytes))
            {
                image.Save(
                    Path.Combine(
                        audioFile.DirectoryName,
                        Path.GetFileNameWithoutExtension(outputAudioFile.Name) + "-" + uniqueString + "-web.jpg"),
                    ImageFormat.Jpeg);
            }

            // delete outputAudioFile
            if (File.Exists(outputAudioFile.FullName))
            {
                File.Delete(outputAudioFile.FullName);
            }
        }

        /*
select COUNT(*) as count, datalocation, cast(SUM(cast(length as bigint))/1000/60/60 as varchar(100))+' hrs' as totalduration, MimeType,
cast(MIN([Length])/1000 as varchar(100))+'s' as minduration, cast(MAX([Length])/1000 as varchar(100))+'s' as maxduration,
(case when [State] = 'uploading' then 'not ready' else 'ready' end) as [state],
min([Time]), MAX([Time])
from audioreadings
group by datalocation, (case when [State] = 'uploading' then 'not ready' else 'ready' end), MimeType, (case when [Length] is null OR [Length] < 1 then 1 else 0 end)
order by DATALocation, MAX([Length]) desc, MimeType
    */
    }
}
