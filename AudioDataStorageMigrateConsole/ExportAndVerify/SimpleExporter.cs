using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioDataStorageMigrateConsole.ExportAndVerify
{
    using System.Diagnostics;
    using System.IO;
    using System.Security.Cryptography;

    using QutSensors.Business.Storage;
    using QutSensors.Data.Linq;
    using QutSensors.Shared;
    using QutSensors.Shared.LogProviders;

    /// <summary>
    /// Simple Exporter that checks if audio file exists and id there is data in db.
    /// Ensures that files exist for all audio readings in db.
    /// </summary>
    public class SimpleExporter : IDisposable
    {
        private readonly SqlFileStreamAudioDataStorage sqlAudioDataStorage;

        private readonly FileSystemAudioDataStorage fileSystemAudioDataStorage;

        private readonly ILogProvider logProvider;

        private readonly Stopwatch singleItemWatch;

        private readonly Stopwatch exportWatch;

        private readonly SHA1CryptoServiceProvider sha1;

        public SimpleExporter(
            string logFileDir,
            SqlFileStreamAudioDataStorage sqlAudioDataStorage,
            FileSystemAudioDataStorage fileSystemAudioDataStorage)
        {
            this.sqlAudioDataStorage = sqlAudioDataStorage;
            this.fileSystemAudioDataStorage = fileSystemAudioDataStorage;

            this.singleItemWatch = new Stopwatch();
            this.exportWatch = new Stopwatch();

            this.sha1 = new SHA1CryptoServiceProvider();

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
                    "SingleAudioReadingProcessDurationMs",
                    "OverallRunningCount",
                    "OverallRunningDuration",
                    "OverallRunningDurationFormatted",
                    "OverallRunningDurationMs",
                };

            this.logProvider = new MultiLogProvider(new CsvTextFileLogProvider(logFileDir, csvHeaders), new ConsoleLogProvider());
        }

        public void Run()
        {
            using (var db = new QutSensorsDb())
            {
                var infoHolder = new InfoHolder { CountSoFar = 0, RunningTimeSoFar = TimeSpan.Zero };

                var audioreadings =
                    db.AudioReadings.Where(ar => ar.State != AudioReadingState.Uploading).OrderByDescending(
                        ar => ar.Length).ThenByDescending(ar => ar.Time);

                foreach (var reading in audioreadings)
                {
                    infoHolder = Process(reading, infoHolder);
                }
            }
        }

        private string HashLikeSqlSha1Extended(FileInfo file)
        {
            string hash = string.Empty;
            using (var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // TODO: will need to hash each 8000 byte segment separately to match sql server hashbytes function limitations.
                hash = Convert.ToBase64String(this.sha1.ComputeHash(fileStream));
            }
            return hash;
        }

        private string HashSqlExtended(AudioReading reading)
        {
            return string.Empty;
        }

        private class InfoHolder
        {
            public long CountSoFar { get; set; }
            public TimeSpan RunningTimeSoFar { get; set; }
        }

        private InfoHolder Process(AudioReading reading, InfoHolder infoHolder)
        {
            infoHolder.CountSoFar += 1;

            var msg = string.Empty;
            LogType logType = LogType.Information;

            long fileByteCount = 0;
            bool fileExists = false;

            long dbDataLength = 0;
            bool dbDataIsNull = false;

            TimeSpan exportTime = TimeSpan.Zero;

            this.singleItemWatch.Restart();

            try
            {
                FileInfo file = this.fileSystemAudioDataStorage.GetDataFile(reading);

                // see if file exists
                fileExists = file != null && File.Exists(file.FullName);

                // if a file does exist, get it's byte count
                if (fileExists)
                {
                    fileByteCount = file.Length;
                }

                // see if there is data in the database
                dbDataIsNull = !MigrationUtils.AudioReadingSqlFileStreamDataExists(reading);

                // if there is, get it's byte count

                if (!dbDataIsNull)
                {
                    dbDataLength = MigrationUtils.AudioReadingSqlFileStreamDataLength(reading);
                }

                // if both file exists and data is not null, not sure what's up so do nothing.

                // if file does not exist and data is not null, export data from db.

                if (!fileExists && !dbDataIsNull)
                {
                    this.exportWatch.Restart();

                    ReadFromSqlFileStreamWriteToFileSystem(reading);

                    this.exportWatch.Stop();
                    exportTime = this.exportWatch.Elapsed;
                }

                // if file does not exist and data is null, there is a problem.

                // if file exists and data is null, this is what we're after so do nothing.

                if (fileExists && !dbDataIsNull)
                {
                    msg = "both file exists and data is not null, not sure what's up so do nothing";
                }
                else if (!fileExists && !dbDataIsNull)
                {
                    msg = "file does not exist and data is not null, export data from db";
                }
                else if (!fileExists && dbDataIsNull)
                {
                    msg = "file does not exist and data is null, there is a problem";
                }
                else if (fileExists && dbDataIsNull)
                {
                    msg = "file exists and data is null, this is what we're after so do nothing";
                }
            }
            catch (Exception ex)
            {
                msg = "Exception: " + ex;
                logType = LogType.Error;
            }

            this.singleItemWatch.Stop();
            infoHolder.RunningTimeSoFar += this.singleItemWatch.Elapsed;

            // log
            this.logProvider.WriteEntry(
                logType,
                msg,
                reading.AudioReadingID,
                fileExists,
                fileByteCount,
                fileByteCount.ToByteDisplay(),
                dbDataIsNull,
                dbDataLength,
                dbDataLength.ToByteDisplay(),
                exportTime,
                exportTime.ToReadableString(),
                exportTime.TotalMilliseconds,
                this.singleItemWatch.Elapsed,
                this.singleItemWatch.Elapsed.ToReadableString(),
                this.singleItemWatch.Elapsed.TotalMilliseconds,
                infoHolder.CountSoFar,
                infoHolder.RunningTimeSoFar,
                infoHolder.RunningTimeSoFar.ToReadableString(),
                infoHolder.RunningTimeSoFar.TotalMilliseconds);

            return infoHolder;
        }

        /// <summary>
        /// The read from sql file stream write to file system.
        /// </summary>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// <c>InvalidOperationException</c>.
        /// </exception>
        private void ReadFromSqlFileStreamWriteToFileSystem(AudioReading reading)
        {
            bool dataToFileSuccess;

            // 2. use sqlAudioDataStorage to get data from db using SqlFileStream.
            using (Stream dataStream = this.sqlAudioDataStorage.GetData(reading))
            using (this.sqlAudioDataStorage.SqlFileStreamWrapper)
            {
                // 3. use fileSystemAudioDataStorage to write file to file system
                dataToFileSuccess = this.fileSystemAudioDataStorage.AddData(reading, dataStream, 0, true);
            }

            if (!dataToFileSuccess)
            {
                // Writing from sql file stream to file system MUST be successful.
                throw new InvalidOperationException("Migration Error: Audio Reading with id " + reading.AudioReadingID + " was NOT written to file system audio data storage.");
            }
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (this.sqlAudioDataStorage != null)
            {
                this.sqlAudioDataStorage.Dispose();
            }
        }

        #endregion
    }
}
