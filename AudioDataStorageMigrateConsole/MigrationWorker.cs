// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationWorker.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Migration worker migrates audio from Sql Server Db to file system.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioDataStorageMigrateConsole
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using AudioTools;
    using AudioTools.AudioUtlity;

    using QutSensors.Business.Storage;
    using QutSensors.Data;
    using QutSensors.Data.Linq;
    using QutSensors.Shared;
    using QutSensors.Shared.LogProviders;

    /// <summary>
    /// Migration worker migrates audio from Sql Server Db to file system.
    /// </summary>
    public class MigrationWorker
    {
        private readonly SqlFileStreamAudioDataStorage sqlAudioDataStorage;

        private readonly FileSystemAudioDataStorage fileSystemAudioDataStorage;

        private readonly IAudioUtility audioUtility;

        private readonly ILogProvider logProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationWorker"/> class.
        /// </summary>
        /// <param name="logFileDir">
        /// The log File Dir.
        /// </param>
        /// <param name="sqlAudioDataStorage">
        /// The sql File Stream Audio Data Storage.
        /// </param>
        /// <param name="fileSystemAudioDataStorage">
        /// The file System Audio Data Storage.
        /// </param>
        /// <param name="audioUtility">
        /// The audio Utility.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public MigrationWorker(
            string logFileDir,
            SqlFileStreamAudioDataStorage sqlAudioDataStorage,
            FileSystemAudioDataStorage fileSystemAudioDataStorage,
            IAudioUtility audioUtility)
        {
            this.sqlAudioDataStorage = sqlAudioDataStorage;
            this.fileSystemAudioDataStorage = fileSystemAudioDataStorage;
            this.audioUtility = audioUtility;

            var csvHeaders = new List<string>
                {
                    "AudioReadingId",
                    "SqlFileStreamMimeType",
                    "SqlFileStreamDataLength",
                    "SqlFileStreamDataLengthFormatted",
                    "SqlFileStreamAudioDuration",
                    "SqlFileStreamAudioDurationFormatted",
                    "ReadWriteDuration",
                    "FileSystemFileExtension",
                    "FileSystemFileLength",
                    "FileSystemFileLengthFormatted",
                    "FileSystemAudioDuration",
                    "FileSystemAudioDurationFormatted",
                    "SingleAudioReadingProcessDuration",
                    "OverallRunningCount",
                    "OverallRunningDuration",
                    "OverallRunningDurationFormatted",
                };

            this.logProvider = new MultiLogProvider(new CsvTextFileLogProvider(logFileDir, csvHeaders), new ConsoleLogProvider());
        }

        /// <summary>
        /// Run migration.
        /// </summary>
        public void RunMigration()
        {
            long count = 0;

            MigrationInfo info;

            var watch = new Stopwatch();
            watch.Start();

            // only stop when info is null - nothing more to process.
            do
            {
                count += 1;
                info = this.MigrateSingleAudioReading();

                if (info != null)
                {
                    info.OverallRunningCount = count;
                    info.OverallRunningDuration = watch.Elapsed;
                    this.logProvider.WriteEntry(info.LogType, info.Message, info.ToStrings().ToArray());
                }
            }
            while (info != null);

            watch.Stop();
        }

        /// <summary>
        /// Get an audio reading ready to migrate.
        /// </summary>
        /// <param name="db">Data Context.</param>
        /// <returns>Audio reading or null.</returns>
        private static AudioReading GetAudioReadingToMigrate(QutSensorsDb db)
        {
            // export audio files starting with largest first.
            // only consider audio readings in sql file stream
            // sqlfilestreamexportfailed will need to be dealt with manually.
            // 172800000 = 48 hrs
            var availableAudioReading =
                db.AudioReadings.Where(
                    ar =>
                    ar.Length.HasValue && ar.Length.Value > 0 &&
                    ar.DataLocation == AudioReadingDataLocation.SqlFileStream &&
                    ar.State != AudioReadingState.Uploading)
                .OrderByDescending(ar => ar.Length)
                .ThenByDescending(ar => ar.Time)
                .FirstOrDefault();

            return availableAudioReading;
        }

        /// <summary>
        /// Get audio reading sql file stream data length.
        /// </summary>
        /// <param name="db">
        /// Data Context.
        /// </param>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Audio reading id must be set.
        /// </exception>
        /// <returns>
        /// The audio reading sql file stream data length.
        /// </returns>
        private static long AudioReadingSqlFileStreamDataLength(QutSensorsDb db, AudioReading reading)
        {
            if (reading == null || reading.AudioReadingID == Guid.Empty)
            {
                throw new ArgumentException("Audio reading id must be set.");
            }

            var items = db.ExecuteQuery(@"
SELECT top 1 ar.[AudioReadingID], datalength(ar.[Data]) as [DataLength] 
FROM AudioReadings ar
WHERE ar.[AudioReadingID] = '" + reading.AudioReadingID + "';");

            if (items.Count() > 0)
            {
                var check = items.Select(i => new
                {
                    Id = Guid.Parse(i["AudioReadingID"].ToString()),
                    DataLength = long.Parse((i["DataLength"] ?? 0).ToString())
                });

                var item = check.FirstOrDefault();

                if (item != null && item.Id == reading.AudioReadingID)
                {
                    return item.DataLength;
                }
            }

            return 0;
        }

        /// <summary>
        /// Check that time spans match or are close enough.
        /// </summary>
        /// <param name="ts1">Time Span 1.</param>
        /// <param name="ts2">Time Span 2.</param>
        /// <returns>True if time spans are within range.</returns>
        private static bool TimeSpansWithinRange(TimeSpan ts1, TimeSpan ts2)
        {
            TimeSpan difference;

            if (ts1 > ts2)
            {
                difference = ts1 - ts2;
            }
            else
            {
                difference = ts2 - ts1;
            }

            return difference <= TimeSpan.FromMilliseconds(800);
        }

        /// <summary>
        /// Clear sql file stream data.
        /// </summary>
        /// <param name="db">
        /// Data Context.
        /// </param>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Audio reading id must be set.
        /// </exception>
        /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
        private static void ClearSqlFileStreamData(QutSensorsDb db, AudioReading reading)
        {
            if (reading == null || reading.AudioReadingID == Guid.Empty)
            {
                throw new ArgumentException("Audio reading id must be set.");
            }

            // parameters use braces eg. {0} {1}
            int rowsAffected = db.ExecuteCommand(
                "Update [AudioReadings] Set [Data] = 0x Where [AudioReadingID] =  {0}",
                reading.AudioReadingID.ToString());

            if (rowsAffected != 1)
            {
                throw new InvalidOperationException("Updated " + rowsAffected + " rows instead of 1.");
            }
        }

        /// <summary>
        /// Record error and mark as failed.
        /// </summary>
        /// <param name="db">
        /// Data Context.
        /// </param>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <param name="format">
        /// The message.
        /// </param>
        /// <param name="formatArgs">
        /// The format Args.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// <c>InvalidOperationException</c>.
        /// </exception>
        private static void RecordErrorMarkAsFailed(QutSensorsDb db, AudioReading reading, string format, params object[] formatArgs)
        {
            reading.DataLocation = AudioReadingDataLocation.SqlFileStreamExportFailed;
            db.SubmitChanges();

            throw new InvalidOperationException("Migration Error: " + string.Format(format, formatArgs));
        }

        /// <summary>
        /// Migrate one audio reading.
        /// </summary>
        /// <returns>
        /// Migration info.
        /// </returns>
        private MigrationInfo MigrateSingleAudioReading()
        {
            var info = new MigrationInfo { LogType = LogType.Information, Message = "Migration Successful." };

            try
            {
                var overallWatch = new Stopwatch();
                overallWatch.Start();

                using (var db = new QutSensorsDb())
                {
                    AudioReading reading = GetAudioReadingToMigrate(db);

                    if (reading == null)
                    {
                        // only time it returns null - no more audio readings to process.
                        return null;
                    }

                    // gather all available information
                    info.AudioReadingId = reading.AudioReadingID;
                    info.SqlFileStreamMimeType = MimeTypes.Canonicalise(reading.MimeType);
                    info.SqlFileStreamAudioDuration = reading.Length.HasValue
                                                          ? TimeSpan.FromMilliseconds(reading.Length.Value)
                                                          : TimeSpan.Zero;
                    info.SqlFileStreamDataLength = AudioReadingSqlFileStreamDataLength(db, reading);
                    AudioReadingDataLocation location = reading.DataLocation;

                    bool fileExists = this.AudioReadingFileExists(reading);

                    FileInfo file = null;

                    if (fileExists)
                    {
                        file = this.fileSystemAudioDataStorage.GetDataFile(reading);
                        info.FileSystemFile = file;
                        info.FileSystemAudioDuration = this.audioUtility.Duration(file, reading.MimeType);
                    }

                    /*****************
                     * decide what to do
                     *****************/

                    if (info.SqlFileStreamDataLength == 0 && info.FileSystemFile.Length > 0 &&
                        fileExists && file != null && File.Exists(file.FullName) &&
                        TimeSpansWithinRange(info.FileSystemAudioDuration, info.SqlFileStreamAudioDuration) &&
                        MimeTypes.Canonicalise(info.SqlFileStreamMimeType) == MimeTypes.GetMimeTypeFromExtension(info.FileSystemFile.Extension))
                    {
                        // no data in db, file does exist and has data, mime type/ext match, db duration and file duration within range
                        // -> data location is incorrect, fix it.
                        reading.DataLocation = AudioReadingDataLocation.FileSystem;
                        db.SubmitChanges();
                    }
                    else if (info.SqlFileStreamDataLength > 0 && fileExists && file != null && File.Exists(file.FullName) &&
                             info.FileSystemFile.Length < info.SqlFileStreamDataLength &&
                             MimeTypes.Canonicalise(info.SqlFileStreamMimeType) ==
                             MimeTypes.GetMimeTypeFromExtension(info.FileSystemFile.Extension))
                    {
                        // data in db, file exists, file is smaller than sql file stream data, mime type/ext match
                        // -> migration stopped during export
                        reading.DataLocation = AudioReadingDataLocation.SqlFileStream;
                        db.SubmitChanges();

                        // assume full file info is in db - delete file
                        File.Delete(file.FullName);

                        // export audio data.
                        info = ExportData(db, reading, info);
                    }
                    else if (fileExists && file != null && File.Exists(file.FullName) &&
                             info.SqlFileStreamDataLength > 0 &&
                             info.FileSystemFile.Length == info.SqlFileStreamDataLength &&
                             MimeTypes.Canonicalise(info.SqlFileStreamMimeType) ==
                             MimeTypes.GetMimeTypeFromExtension(info.FileSystemFile.Extension) &&
                             TimeSpansWithinRange(info.FileSystemAudioDuration, info.SqlFileStreamAudioDuration))
                    {
                        // file exists, there is data in db, file and data size match, mime types match, durations match
                        // -> remove data from db.
                        reading.DataLocation = AudioReadingDataLocation.FileSystem;
                        db.SubmitChanges();

                        // remove data from sql file stream.
                        ClearSqlFileStreamData(db, reading);
                    }
                    else if (info.SqlFileStreamDataLength > 0 && !fileExists &&
                             location == AudioReadingDataLocation.SqlFileStream)
                    {
                        // data in db, no file, location is sql file stream
                        // -> export data
                        info = ExportData(db, reading, info);
                    }
                    else
                    {
                        RecordErrorMarkAsFailed(
                            db,
                            reading,
                            "Audio reading is not in a state that can be processed.");
                    }

                    overallWatch.Stop();
                    info.TotalDuration = overallWatch.Elapsed;
                }
            }
            catch (Exception ex)
            {
                info.Message = " --Exception Message-- " + ex.Message;
                info.LogType = LogType.Error;
            }

            return info;
        }

        private bool AudioReadingFileExists(AudioReading reading)
        {
            try
            {
                FileInfo file = this.fileSystemAudioDataStorage.GetDataFile(reading);
                return file != null && File.Exists(file.FullName);
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private bool ReadFromSqlFileStreamWriteToFileSystem(AudioReading reading)
        {
            bool dataToFileSuccess;

            // 2. use sqlAudioDataStorage to get data from db using SqlFileStream.
            using (Stream dataStream = this.sqlAudioDataStorage.GetData(reading))
            using (this.sqlAudioDataStorage.SqlFileStreamWrapper)
            {
                // 3. use fileSystemAudioDataStorage to write file to file system
                dataToFileSuccess = this.fileSystemAudioDataStorage.AddData(reading, dataStream, 0, true);
            }

            return dataToFileSuccess;
        }

        private MigrationInfo ExportData(QutSensorsDb db, AudioReading reading, MigrationInfo info)
        {
            /*****************
             * Export audio reading sql file stream to file.
             *****************/

            var watch = new Stopwatch();
            watch.Start();

            bool dataToFileSuccess = ReadFromSqlFileStreamWriteToFileSystem(reading);

            watch.Stop();
            info.ReadWriteDuration = watch.Elapsed;

            if (!dataToFileSuccess)
            {
                // Writing from sql file stream to file system MUST be successful.
                RecordErrorMarkAsFailed(db, reading, "Audio Reading was NOT written to file system audio data storage.");
            }

            /*****************
             * Get audio file info.
             *****************/

            FileInfo file = this.fileSystemAudioDataStorage.GetDataFile(reading);
            info.FileSystemFile = file;

            if (file == null || !File.Exists(file.FullName) || file.Length < 1)
            {
                // file must exist
                RecordErrorMarkAsFailed(db, reading, "Audio Reading exported to file but file does not exist or contains no data.");
            }

            if (info.SqlFileStreamDataLength != info.FileSystemFile.Length)
            {
                // data lengths must match
                const string Msg = "Audio Reading writen to file but data lengths don't match. Sql FileStream: {0} file: {1}";

                RecordErrorMarkAsFailed(db, reading, Msg, info.SqlFileStreamDataLength, info.FileSystemFile.Length);
            }

            if (MimeTypes.Canonicalise(info.SqlFileStreamMimeType) != MimeTypes.GetMimeTypeFromExtension(info.FileSystemFile.Extension))
            {
                // mime types must match
                const string Msg = "Audio Reading writen to file but mime type and extension do not match. Sql FileStream: {0} file: {1} ({2})";

                RecordErrorMarkAsFailed(
                    db,
                    reading,
                    Msg,
                    MimeTypes.Canonicalise(info.SqlFileStreamMimeType),
                    MimeTypes.GetMimeTypeFromExtension(info.FileSystemFile.Extension),
                    info.FileSystemFile.Extension);
            }

            info.FileSystemAudioDuration = this.audioUtility.Duration(file, reading.MimeType);

            if (!TimeSpansWithinRange(info.FileSystemAudioDuration, info.SqlFileStreamAudioDuration))
            {
                // sqlfilestream data must match file system data
                const string Msg = "Audio Reading writen to file but durations do not match. Sql FileStream: {0}  file: {1}";

                RecordErrorMarkAsFailed(
                    db,
                    reading,
                    Msg,
                    info.SqlFileStreamAudioDuration.ToReadableString(),
                    info.FileSystemAudioDuration.ToReadableString());
            }

            /*****************
            * Update db to indicate file has been moved.
            *****************/

            // set data location
            reading.DataLocation = AudioReadingDataLocation.FileSystem;
            db.SubmitChanges();

            // remove data from sql file stream.
            ClearSqlFileStreamData(db, reading);

            info.CopiedFromSqlFileStreamToFileSystem = true;

            return info;
        }
    }
}
