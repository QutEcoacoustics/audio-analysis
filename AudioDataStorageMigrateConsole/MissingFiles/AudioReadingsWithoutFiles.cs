using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QutSensors.Shared.LogProviders;
using System.Diagnostics;
using QutSensors.Business.Storage;
using AudioDataStorageMigrateConsole.Classes;
using QutSensors.Data.Linq;

namespace AudioDataStorageMigrateConsole.MissingFiles
{
    public class AudioReadingsWithoutFiles
    {
        private readonly ILogProvider logProvider;

        private readonly Stopwatch singleItemWatch;

        private readonly Stopwatch exportWatch;

        public AudioReadingsWithoutFiles(
            string logFileDir,
            FileSystemAudioDataStorage fileSystemAudioDataStorage)
        {
            this.AudioFileInfo = new AudioFileInfo(fileSystemAudioDataStorage);

            this.singleItemWatch = new Stopwatch();
            this.exportWatch = new Stopwatch();

            var csvHeaders = new List<string>
                {
                    "AudioReadingId",
                    "FileSystemFileExistsBefore",
                    "FileSystemFileLengthBefore",
                    "FileSystemFileLengthFormattedBefore",
                    "FileSystemFileExistsAfter",
                    "FileSystemFileLengthAfter",
                    "FileSystemFileLengthFormattedAfter",
                    "DatabaseDataExistsBefore",
                    "DatabaseDataLengthBefore",
                    "DatabaseDataLengthFormattedBefore",
                    "DatabaseDataExistsAfter",
                    "DatabaseDataLengthAfter",
                    "DatabaseDataLengthFormattedAfter",
                    "ReadWriteDuration",
                    "ReadWriteDurationFormatted",
                    "ReadWriteDurationMs",
                    "SingleAudioReadingProcessDuration",
                    "SingleAudioReadingProcessDurationFormatted",
                    "SingleAudioReadingProcessDurationMs",
                    "OverallRunningCount",
                    "OverallRunningDuration",
                    "OverallRunningDurationFormatted",
                    "OverallRunningDurationMs",
                };

            this.logProvider = new MultiLogProvider(new CsvTextFileLogProvider(logFileDir, csvHeaders), new ConsoleLogProvider());
        }

        /// <summary>
        /// Gets DbDataInfo.
        /// </summary>
        public DbDataInfo DbDataInfo { get; private set; }

        /// <summary>
        /// Gets AudioFileInfo.
        /// </summary>
        public AudioFileInfo AudioFileInfo { get; private set; }

        /// <summary>
        /// Run checks.
        /// </summary>
        public void Run()
        {
            using (var db = new QutSensorsDb())
            {
                // exclude all with state = uploading. these may need to be fixed manually.
                // !(ar.State == AudioReadingState.Uploading && ar.UploadStartUTC.HasValue)
                var audioreadings =  db.AudioReadings.OrderByDescending(  ar => ar.Length).ThenByDescending(ar => ar.Time);

                foreach (var reading in audioreadings)
                {
                    //infoHolder = Process(db, reading, infoHolder);
                }
            }
        }
    }
}
