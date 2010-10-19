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

    using AudioTools.AudioUtlity;

    using QutSensors.Business.Storage;
    using QutSensors.Data.Linq;
    using QutSensors.Shared;
    using QutSensors.Data;
    using QutSensors.Shared.LogProviders;

    using BTR.Core.Linq;

    /// <summary>
    /// Migration worker migrates audio from Sql Server Db to file system.
    /// </summary>
    public class MigrationWorker
    {
        private readonly SqlFileStreamAudioDataStorage sqlAudioDataStorage;

        private readonly FileSystemAudioDataStorage fileSystemAudioDataStorage;

        private readonly IAudioUtility audioUtility;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationWorker"/> class.
        /// </summary>
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
            SqlFileStreamAudioDataStorage sqlAudioDataStorage,
            FileSystemAudioDataStorage fileSystemAudioDataStorage,
            IAudioUtility audioUtility)
        {
            this.sqlAudioDataStorage = sqlAudioDataStorage;
            this.fileSystemAudioDataStorage = fileSystemAudioDataStorage;
            this.audioUtility = audioUtility;
        }

        /// <summary>
        /// Migrate one audio reading from SQL Server Db to file system.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <c>InvalidOperationException</c>.
        /// </exception>
        /// <returns>
        /// The migration info.
        /// </returns>
        public MigrationInfo Migrate()
        {
            var overallWatch = new Stopwatch();
            overallWatch.Start();

            var info = new MigrationInfo();

            using (var db = new QutSensorsDb())
            {
                // 1. get audio reading id not migrated yet
                AudioReading reading = GetAvailableDbAudioReading(db);

                if (reading == null)
                {
                    return null;
                }

                if (this.AudioReadingFileExists(reading))
                {
                    // audio reading should both be available to move and have an existing file.
                    throw new InvalidOperationException(
                        "Audio Reading id " + reading.AudioReadingID +
                        " was available to migrate from db, but file already exists. No action taken.");
                }

                info.AudioReadingId = reading.AudioReadingID;
                info.SqlFileStreamMimeType = reading.MimeType;

                info.SqlFileStreamAudioDuration = reading.Length.HasValue
                                                      ? TimeSpan.FromMilliseconds(reading.Length.Value)
                                                      : TimeSpan.Zero;

                info.SqlFileStreamDataLength = AudioReadingDataLength(db, reading);

                var watch = new Stopwatch();
                watch.Start();

                bool dataToFileSuccess = ReadFromSqlFileStreamWriteToFileSystem(reading);

                watch.Stop();
                info.ReadWriteDuration = watch.Elapsed;

                if (!dataToFileSuccess)
                {
                    // Writing from sql file stream to file system MUST be successful.
                    throw new InvalidOperationException(
                        "Audio Reading id " + reading.AudioReadingID +
                        " was NOT successfully written to file system audio data storage.");
                }

                // 4. update db to indicate file has been moved.
                reading.DataLocation = AudioReadingDataLocation.FileSystem;
                db.SubmitChanges();

                FileInfo file = this.fileSystemAudioDataStorage.GetDataFile(reading);

                info.FileSystemFile = file;
                info.FileSystemAudioDuration = this.audioUtility.Duration(file, reading.MimeType);
            }

            overallWatch.Stop();
            info.TotalDuration = overallWatch.Elapsed;

            return info;
        }

        private static AudioReading GetAvailableDbAudioReading(QutSensorsDb db)
        {
            var availableAudioReading =
                db.AudioReadings.Where(
                    ar =>
                    ar.Length.HasValue && ar.Length.Value > 0 &&
                    ar.DataLocation == AudioReadingDataLocation.SqlFileStream).FirstOrDefault();

            if (availableAudioReading != null)
            {
                return availableAudioReading;
            }

            return null;
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

        private static long AudioReadingDataLength(QutSensorsDb db, AudioReading reading)
        {
            // long.ToByteDisplay();

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
    }
}
