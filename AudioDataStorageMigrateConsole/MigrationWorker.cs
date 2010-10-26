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
        /// <param name="logProvider">
        /// The log Provider.
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
            ILogProvider logProvider,
            SqlFileStreamAudioDataStorage sqlAudioDataStorage,
            FileSystemAudioDataStorage fileSystemAudioDataStorage,
            IAudioUtility audioUtility)
        {
            this.logProvider = logProvider;
            this.sqlAudioDataStorage = sqlAudioDataStorage;
            this.fileSystemAudioDataStorage = fileSystemAudioDataStorage;
            this.audioUtility = audioUtility;
        }

        /// <summary>
        /// Migrate one audio reading.
        /// </summary>
        /// <returns>
        /// Migration info.
        /// </returns>
        public MigrationInfo MigrateSingleAudioReading()
        {
            var overallWatch = new Stopwatch();
            overallWatch.Start();

            var info = new MigrationInfo();

            using (var db = new QutSensorsDb())
            {
                AudioReading reading = GetAudioReadingToMigrate(db);

                if (reading == null)
                {
                    return null;
                }

                // gather all available information
                info.AudioReadingId = reading.AudioReadingID;
                info.SqlFileStreamMimeType = reading.MimeType;
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

                if (info.SqlFileStreamDataLength == 0 && fileExists && file != null && File.Exists(file.FullName) &&
                    TimeSpansWithinRange(info.FileSystemAudioDuration, info.SqlFileStreamAudioDuration) &&
                    info.SqlFileStreamMimeType == MimeTypes.GetMimeTypeFromExtension(info.FileSystemFile.Extension))
                {
                    // no data in db, file does exist, mime type/ext match, db duration and file duration within range
                    // -> data location is incorrect, fix it.
                    reading.DataLocation = AudioReadingDataLocation.FileSystem;
                    db.SubmitChanges();
                }
                else if (info.SqlFileStreamDataLength > 0 && fileExists && file != null && File.Exists(file.FullName) &&
                         info.FileSystemFile.Length < info.SqlFileStreamDataLength &&
                         info.SqlFileStreamMimeType == MimeTypes.GetMimeTypeFromExtension(info.FileSystemFile.Extension))
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
                    info.SqlFileStreamMimeType == MimeTypes.GetMimeTypeFromExtension(info.FileSystemFile.Extension) &&
                    TimeSpansWithinRange(info.FileSystemAudioDuration, info.SqlFileStreamAudioDuration)
                    )
                {
                    // file exists, data in db, file and data size match, mime types match, durations match
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
                    RecordErrorMarkAsFailed(db, reading, "Audio reading " + reading.AudioReadingID + " state can not be processed.");
                }

                overallWatch.Stop();
                info.TotalDuration = overallWatch.Elapsed;

                this.logProvider.WriteEntry(
                    LogType.Information,
                    "Migrating succesful for audio reading id " + reading.AudioReadingID + ". Info: " + info.ToString());
            }

            return info;
        }

        private static AudioReading GetAudioReadingToMigrate(QutSensorsDb db)
        {
            // export audio files starting with largest first.
            // ignore audio readings already in file system
            // 172800000 = 48 hrs
            var availableAudioReading =
                db.AudioReadings.Where(
                    ar =>
                    ar.Length.HasValue && ar.Length.Value > 0 && 
                    ar.DataLocation != AudioReadingDataLocation.FileSystem &&
                    ar.State != AudioReadingState.Uploading)
                .OrderByDescending(ar => ar.Length)
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

            this.logProvider.WriteEntry(LogType.Information, "Migrating audio reading id " + reading.AudioReadingID);

            var watch = new Stopwatch();
            watch.Start();

            bool dataToFileSuccess = ReadFromSqlFileStreamWriteToFileSystem(reading);

            watch.Stop();
            info.ReadWriteDuration = watch.Elapsed;

            if (!dataToFileSuccess)
            {
                // Writing from sql file stream to file system MUST be successful.
                string msg = "Audio Reading id " + reading.AudioReadingID +
                             " was NOT written to file system audio data storage.";

                this.RecordErrorMarkAsFailed(db, reading, msg);
            }

            /*****************
             * Get audio file info.
             *****************/

            FileInfo file = this.fileSystemAudioDataStorage.GetDataFile(reading);
            info.FileSystemFile = file;

            if (file == null || !File.Exists(file.FullName) || file.Length < 1)
            {
                // file must exist
                string msg = "Audio Reading id " + reading.AudioReadingID +
                             " exported to file, but file does not exist or contains no data.";

                this.RecordErrorMarkAsFailed(db, reading, msg);
            }

            if (info.SqlFileStreamDataLength != info.FileSystemFile.Length)
            {
                // data lengths must match
                string msg = "Audio Reading id " + reading.AudioReadingID +
                             " writen to file, but data lengths don't match. Sql FileStream " +
                             info.SqlFileStreamDataLength + ", file: " + info.FileSystemFile.Length;

                this.RecordErrorMarkAsFailed(db, reading, msg);
            }

            if (info.SqlFileStreamMimeType != MimeTypes.GetMimeTypeFromExtension(info.FileSystemFile.Extension))
            {
                // mime types must match
                string msg = "Audio Reading id " + reading.AudioReadingID +
                             " writen to file, but mime type and extension do not match. Sql FileStream " +
                             info.SqlFileStreamMimeType + ", file: " +
                             MimeTypes.GetMimeTypeFromExtension(info.FileSystemFile.Extension) + " (" +
                             info.FileSystemFile.Extension + ")";

                this.RecordErrorMarkAsFailed(db, reading, msg);
            }

            info.FileSystemAudioDuration = this.audioUtility.Duration(file, reading.MimeType);

            if (!TimeSpansWithinRange(info.FileSystemAudioDuration, info.SqlFileStreamAudioDuration))
            {
                // sqlfilestream data must match file system data
                string msg = "Audio Reading id " + reading.AudioReadingID +
                             " writen to file, but durations do not match. Sql FileStream " +
                             info.SqlFileStreamAudioDuration + ", file: " + info.FileSystemAudioDuration;

                this.RecordErrorMarkAsFailed(db, reading, msg);
            }

            /*****************
            * Update db to indicate file has been moved.
            *****************/

            // set data location
            reading.DataLocation = AudioReadingDataLocation.FileSystem;
            db.SubmitChanges();

            // remove data from sql file stream.
            ClearSqlFileStreamData(db, reading);

            return info;
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
        /// <param name="message">
        /// The message.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// <c>InvalidOperationException</c>.
        /// </exception>
        private void RecordErrorMarkAsFailed(QutSensorsDb db, AudioReading reading, string message)
        {
            reading.DataLocation = AudioReadingDataLocation.SqlFileStreamExportFailed;
            db.SubmitChanges();

            this.logProvider.WriteEntry(LogType.Error, "Migration Error: " + message);

            throw new InvalidOperationException("Migration Error: " + message);
        }
    }
}
