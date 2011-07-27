using System;
using System.Collections.Generic;
using System.Linq;

using QutSensors.Shared.LogProviders;
using System.Diagnostics;
using QutSensors.Business.Storage;
using QutSensors.Data.Linq;

namespace AudioDataStorageMigrateConsole.MissingFiles
{
    using System.IO;

    public class AudioReadingsWithoutFiles
    {
        private readonly FileSystemAudioDataStorage fileSystemAudioDataStorage;

        private readonly ILogProvider logProvider;

        private readonly Stopwatch singleItemWatch;

        public AudioReadingsWithoutFiles(string logFileDir,
            FileSystemAudioDataStorage fileSystemAudioDataStorage)
        {
            this.fileSystemAudioDataStorage = fileSystemAudioDataStorage;

            this.singleItemWatch = new Stopwatch();

            var csvHeaders = new List<string>
                {
                    "AudioReadingId",
                    "FileSystemFileExists",
                    "FileSystemFileLength",
                    "FileSystemFileLengthFormatted",
                    "DatabaseDataIsNull",
                    "DatabaseDataLength",
                    "DatabaseDataLengthFormatted",
                    "ReadWriteDuration",
                    "ReadWriteDurationFormatted",
                    "ReadWriteDurationMs",
                    "SingleAudioReadingProcessDuration",
                    "SingleAudioReadingProcessDurationFormatted",
                    "SingleAudioReadingProcessDurationMs"
                };

            this.logProvider = new MultiLogProvider(new CsvTextFileLogProvider(logFileDir, csvHeaders), new ConsoleLogProvider());

        }

        public void Run()
        {
            using (var db = new QutSensorsDb())
            {
                var audioreadings = db.AudioReadings.OrderByDescending(ar => ar.Length).ThenByDescending(ar => ar.Time);

                foreach (var reading in audioreadings)
                {
                    Check(reading);
                }
            }
        }

        private void Check(AudioReading reading)
        {
            var msg = string.Empty;
            LogType logType = LogType.Information;

            long fileByteCount = 0;
            bool fileExists = false;

            long dbDataByteCount = 0;
            bool dbDataExists = false;

            TimeSpan exportTime = TimeSpan.Zero;

            this.singleItemWatch.Restart();

            try
            {
                FileInfo file = null;
                try
                {
                    file = this.fileSystemAudioDataStorage.GetDataFile(reading);
                }
                catch (InvalidOperationException ioex)
                {
                    if (ioex.Message.Contains("File for audio reading does not exist."))
                    {
                        file = null;
                    }
                }

                // see if file exists
                fileExists = file != null && File.Exists(file.FullName);

                // if a file does exist, get it's byte count
                if (fileExists)
                {
                    fileByteCount = file.Length;
                }

                // see if there is data in the database
                dbDataExists = MigrationUtils.AudioReadingSqlFileStreamDataExists(reading);

                // if there is, get it's byte count
                if (dbDataExists)
                {
                    dbDataByteCount = MigrationUtils.AudioReadingSqlFileStreamDataLength(reading);
                }
            }
            catch (Exception ex)
            {
                msg = "Exception: " + ex;
                logType = LogType.Error;
            }

            this.singleItemWatch.Stop();

            bool bothExistByteCountMismatch = fileExists && dbDataExists && fileByteCount != dbDataByteCount;

            // only log if file does not exist, or both exist but byte sizes don't match
            if (!fileExists || bothExistByteCountMismatch)
            {
                string extraMsg = !fileExists
                                      ? "File does not exist."
                                      : bothExistByteCountMismatch ? "Both exist but byte sizes don't match." : "Other.";

                this.logProvider.WriteEntry(
                    logType,
                    msg + " -- " + extraMsg,
                    reading.AudioReadingID,
                    fileExists,
                    fileByteCount,
                    fileByteCount.ToByteDisplay(),
                    dbDataExists,
                    dbDataByteCount,
                    dbDataByteCount.ToByteDisplay(),
                    exportTime,
                    exportTime.ToReadableString(),
                    exportTime.TotalMilliseconds,
                    this.singleItemWatch.Elapsed,
                    this.singleItemWatch.Elapsed.ToReadableString(),
                    this.singleItemWatch.Elapsed.TotalMilliseconds);
            }
        }
    }
}
