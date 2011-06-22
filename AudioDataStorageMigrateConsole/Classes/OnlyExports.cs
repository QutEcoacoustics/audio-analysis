// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OnlyExports.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Just exports data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioDataStorageMigrateConsole.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using QutSensors.Business.Storage;
    using QutSensors.Data.Linq;
    using QutSensors.Shared;
    using QutSensors.Shared.LogProviders;

    /// <summary>
    /// Just exports data.
    /// </summary>
    public class OnlyExports
    {
        private readonly ILogProvider logProvider;

        private readonly Stopwatch singleItemWatch;

        private readonly Stopwatch exportWatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="OnlyExports"/> class.
        /// </summary>
        /// <param name="logFileDir">
        /// The log file dir.
        /// </param>
        /// <param name="sqlAudioDataStorage">
        /// The sql audio data storage.
        /// </param>
        /// <param name="fileSystemAudioDataStorage">
        /// The file system audio data storage.
        /// </param>
        public OnlyExports(
            string logFileDir,
            SqlFileStreamAudioDataStorage sqlAudioDataStorage,
            FileSystemAudioDataStorage fileSystemAudioDataStorage)
        {
            this.DbDataInfo = new DbDataInfo(sqlAudioDataStorage);
            this.AudioFileInfo = new AudioFileInfo(fileSystemAudioDataStorage);

            this.singleItemWatch = new Stopwatch();
            this.exportWatch = new Stopwatch();

            var csvHeaders = new List<string>
                {
                    "AudioReadingId",
                    "FileSystemFileExists",
                    "FileSystemFileLength",
                    "FileSystemFileLengthFormatted",
                    "DatabaseDataExists",
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

        /// <summary>
        /// Gets DbDataInfo.
        /// </summary>
        public DbDataInfo DbDataInfo { get; private set; }

        /// <summary>
        /// Gets AudioFileInfo.
        /// </summary>
        public AudioFileInfo AudioFileInfo { get; private set; }

        /// <summary>
        /// Run export.
        /// </summary>
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
                    infoHolder = Process(db, reading, infoHolder);
                }
            }
        }

        private InfoHolder Process(QutSensorsDb db, AudioReading reading, InfoHolder infoHolder)
        {
            infoHolder.CountSoFar += 1;

            var msg = string.Empty;
            LogType logType = LogType.Information;

            long fileByteCount = 0;
            bool fileExists = true;

            long dbDataLength = 0;
            bool dbDataExists = true;

            TimeSpan exportTime = TimeSpan.Zero;

            this.singleItemWatch.Restart();

            try
            {
                /*
                 * Collect some info.
                 */

                // see if file exists
                fileExists = this.AudioFileInfo.DataExists(reading);

                // if a file does exist, get it's byte count)
                if (fileExists)
                {
                    fileByteCount = this.AudioFileInfo.GetByteSize(reading);
                }

                // see if there is data in the database
                dbDataExists = this.DbDataInfo.DataExists(reading);

                // if there is, get it's byte count

                if (dbDataExists)
                {
                    dbDataLength = this.DbDataInfo.GetByteSize(reading);
                }

                /*
                 * Now do the processing
                 */

                if (fileExists && dbDataExists && fileByteCount > 0 && dbDataLength > 0 && fileByteCount == dbDataLength)
                {
                    // if both file exists and db data exists, check the byte sizes match, then delete from db.
                    this.DbDataInfo.ClearData(reading);

                    // update db.
                    UpdateFileInfo(db, reading);
                    msg = "Both file exists and db data exists. The byte sizes match so deleted from db.";
                }
                else if (fileExists && dbDataExists && fileByteCount != dbDataLength)
                {
                    // if the byte sizes don't match, just record this
                    msg = "Both file exists and db data exists. The byte sizes don't match so nothing done.";
                }
                else if (!fileExists && dbDataExists)
                {
                    // if file does not exist and data does exist, export data from db.
                    this.exportWatch.Restart();

                    bool success = ReadFromSqlFileStreamWriteToFileSystem(reading);

                    this.exportWatch.Stop();
                    exportTime = this.exportWatch.Elapsed;

                    // check again to see if file exists
                    fileExists = this.AudioFileInfo.DataExists(reading);

                    if (success && fileExists)
                    {
                        UpdateFileInfo(db, reading);

                        // only remove data from db if file exists
                        this.DbDataInfo.ClearData(reading);

                        msg = "File does not exist and data does exist. Exported data from db. Updated db. Deleted data from db.";
                    }
                    else
                    {
                        msg = "File does not exist and data does exist. Tried to export data from db but it failed. Data is still in db.";
                    }

                }
                else if (!fileExists && !dbDataExists)
                {
                    msg = "File does not exist and data does not exist. PROBLEM but nothing to do about it.";
                }
                else if (fileExists && !dbDataExists)
                {
                    UpdateFileInfo(db, reading);
                    msg = "File exists and data does not exist. This is what we're after so do nothing.";
                }
            }
            catch (Exception ex)
            {
                msg += " Exception: " + ex;
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
                dbDataExists,
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
        /// <returns>
        /// ttrue if process was successful otherwise false.
        /// </returns>
        private bool ReadFromSqlFileStreamWriteToFileSystem(AudioReading reading)
        {
            bool dataToFileSuccess = false;

            try
            {
                // use sqlAudioDataStorage to get data from db using SqlFileStream.
                using (Stream dataStream = this.DbDataInfo.SqlAudioDataStorage.GetData(reading))
                using (this.DbDataInfo.SqlAudioDataStorage.SqlFileStreamWrapper)
                {
                    // use fileSystemAudioDataStorage to write file to file system
                    dataToFileSuccess = this.AudioFileInfo.FileSystemAudioDataStorage.AddData(
                        reading, dataStream, 0, true);
                }

                ////if (!dataToFileSuccess)
                ////{
                //// Writing from sql file stream to file system MUST be successful.
                ////throw new InvalidOperationException("Migration Error: Audio Reading with id " + reading.AudioReadingID + " was NOT written to file system audio data storage.");
                ////}
            }
            catch
            {
                dataToFileSuccess = false;
            }

            return dataToFileSuccess;
        }

        private void UpdateFileInfo(QutSensorsDb db, AudioReading reading)
        {
            // see if file exists
            bool fileExists = this.AudioFileInfo.DataExists(reading);

            if (fileExists)
            {
                long fileByteCount = this.AudioFileInfo.GetByteSize(reading);

                // update reading
                reading.DataLocation = AudioReadingDataLocation.FileSystem;
                reading.DataSizeBytes = fileByteCount;
                reading.State = AudioReadingState.Ready;
                db.SubmitChanges();
            }
        }

        private class InfoHolder
        {
            public long CountSoFar { get; set; }

            public TimeSpan RunningTimeSoFar { get; set; }
        }
    }
}
