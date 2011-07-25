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
                    "SqlFileStreamAudioDurationMs",
                    "ReadWriteDuration",
                    "ReadWriteDurationFormatted",
                    "ReadWriteDurationMs",
                    "FileSystemFileExtension",
                    "FileSystemFileLength",
                    "FileSystemFileLengthFormatted",
                    "FileSystemAudioDuration",
                    "FileSystemAudioDurationFormatted",
                    "FileSystemAudioDurationMs",
                    "SingleAudioReadingProcessDuration",
                    "SingleAudioReadingProcessDurationMs",
                    "SingleAudioReadingProcessDurationFormatted",
                    "OverallRunningCount",
                    "OverallRunningDuration",
                    "OverallRunningDurationFormatted",
                    "OverallRunningDurationMs",
                };

            this.logProvider = new MultiLogProvider(new CsvTextFileLogProvider(logFileDir, csvHeaders), new ConsoleLogProvider());
        }

        /// <summary>
        /// Run migration.
        /// </summary>
        public void RunMigration()
        {
            long count = 0;

            MigrationInfo info = null;

            Console.WriteLine("Start Migration run.");

            var watch = new Stopwatch();
            watch.Start();

            try
            {
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
                        Console.WriteLine();
                        Console.WriteLine("****Migration Complete****");
                        Console.WriteLine();
                    }
                }
                while (info != null);
            }
            catch (Exception ex)
            {
                // log migration info first if able
                if (info != null)
                {
                    info.OverallRunningCount = count;
                    info.OverallRunningDuration = watch.Elapsed;
                    this.logProvider.WriteEntry(info.LogType, info.Message, info.ToStrings().ToArray());
                }

                // log eception that caused loop to exit.
                string message = " --Exception Message Causing Migration Processor Exit-- " + ex.Message;

                this.logProvider.WriteEntry(LogType.Error, message, string.Empty);
            }

            watch.Stop();
        }

        /// <summary>
        /// Get an audio reading ready to migrate.
        /// </summary>
        /// <param name="db">Data Context.</param>
        /// <returns>Audio reading or null.</returns>
        private static AudioReading GetAudioReadingToMigrate(QutSensorsDb db)
        {
            // get any audio reading that:
            // is not uploading and is not marked as export failed
            var availableAudioReading = from ar in db.AudioReadings
                                        where
                                        ar.State != AudioReadingState.Uploading &&
                                        ar.UploadType == null &&
                                        ar.DataLocation != AudioReadingDataLocation.SqlFileStreamExportFailed &&
                                        (
                                        !ar.Length.HasValue ||
                                        !ar.DataSizeBytes.HasValue ||
                                        ar.DataLocation != AudioReadingDataLocation.FileSystem
                                        )
                                        orderby ar.Length descending, ar.Time descending
                                        select ar;
            
            return availableAudioReading.FirstOrDefault();
        }

        /// <summary>
        /// Migrate one audio reading.
        /// </summary>
        /// <returns>
        /// Migration info.
        /// </returns>
        /// <exception cref="InvalidOperationException">Migration Error: Audio reading is not in a state that can be processed.</exception>
        private MigrationInfo MigrateSingleAudioReading()
        {
            var info = new MigrationInfo { LogType = LogType.Information, Message = "Migration Successful." };

            var overallWatch = new Stopwatch();
            overallWatch.Start();

            using (var db = new QutSensorsDb())
            {
                AudioReading reading = null;

                try
                {
                    reading = GetAudioReadingToMigrate(db);

                    if (reading == null)
                    {
                        Console.WriteLine("No more readings to process.");
                        // only time it returns null - no more audio readings to process.
                        return null;
                    }

                    Console.WriteLine("Starting migration for " + reading.AudioReadingID);

                    // gather all available information
                    info.AudioReadingId = reading.AudioReadingID;
                    info.SqlFileStreamMimeType = MimeTypes.Canonicalise(reading.MimeType);
                    info.SqlFileStreamAudioDuration = reading.Length.HasValue
                                                          ? TimeSpan.FromMilliseconds(reading.Length.Value)
                                                          : TimeSpan.Zero;
                    info.SqlFileStreamDataLength = MigrationUtils.AudioReadingSqlFileStreamDataLength(reading);
                    AudioReadingDataLocation location = reading.DataLocation;

                    bool fileExists = this.AudioReadingFileExists(reading);

                    FileInfo file = null;

                    if (fileExists)
                    {
                        Console.WriteLine();
                        Console.WriteLine("File already exists, getting info.");


                        file = this.fileSystemAudioDataStorage.GetDataFile(reading);
                        info.FileSystemFile = file;

                        if (file != null && File.Exists(file.FullName) && file.Length > 0)
                        {
                            try
                            {
                                info.FileSystemAudioDuration = this.audioUtility.Duration(file, reading.MimeType);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Could not get duration for audio file. Setting File duration to zero (0).");
                                Console.WriteLine(ex.Message);
                                info.FileSystemAudioDuration = TimeSpan.Zero;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Existing file is empty.");
                            info.FileSystemAudioDuration = TimeSpan.Zero;
                        }

                        Console.WriteLine();
                    }

                    /*****************
                     * first export data
                     * then calculate all that can be calulated, and store in db.
                     * audio with issues that can't be solved, record error.
                     *****************/

                    if (info.SqlFileStreamDataLength > 0 && !fileExists &&
                             location == AudioReadingDataLocation.SqlFileStream)
                    {
                        Console.WriteLine();
                        Console.WriteLine("data in db, no file, location is sql file stream");
                        Console.WriteLine("-> nothing done yet - export data");
                        Console.WriteLine();

                        info = ExportData(db, reading, info);
                    }

                    if (info.SqlFileStreamDataLength > 0 && fileExists && file != null && File.Exists(file.FullName) &&
                             info.FileSystemFile.Length < info.SqlFileStreamDataLength &&
                             MimeTypes.Canonicalise(info.SqlFileStreamMimeType) == MimeTypes.GetMimeTypeFromExtension(info.FileSystemFile.Extension) &&
                            location != AudioReadingDataLocation.FileSystem)
                    {
                        Console.WriteLine();
                        Console.WriteLine("data in db, file exists, file is smaller than sql file stream data, mime type/ext match");
                        Console.WriteLine("-> migration stopped during export, export again");
                        Console.WriteLine();

                        reading.DataLocation = AudioReadingDataLocation.SqlFileStream;
                        db.SubmitChanges();

                        // assume full file info is in db - delete file
                        File.Delete(file.FullName);

                        // export audio data.
                        info = ExportData(db, reading, info);
                    }

                    if (info.SqlFileStreamDataLength > 0 &&
                        fileExists && file != null && File.Exists(file.FullName) &&
                             info.FileSystemFile.Length == info.SqlFileStreamDataLength &&
                             MimeTypes.Canonicalise(info.SqlFileStreamMimeType) ==
                             MimeTypes.GetMimeTypeFromExtension(info.FileSystemFile.Extension) &&
                             MigrationUtils.TimeSpansWithinRange(info.FileSystemAudioDuration, info.SqlFileStreamAudioDuration))
                    {
                        Console.WriteLine();
                        Console.WriteLine("file exists, there is data in db, file and data size match, mime types match, durations match");
                        Console.WriteLine("-> remove data from db");
                        Console.WriteLine();

                        // remove data from sql file stream.
                        MigrationUtils.ClearSqlFileStreamData(reading);

                        reading.DataLocation = AudioReadingDataLocation.FileSystem;
                        db.SubmitChanges();
                    }

                    if (info.SqlFileStreamDataLength == 0 && info.FileSystemFile.Length > 0 &&
                        fileExists && file != null && File.Exists(file.FullName) &&
                        MigrationUtils.TimeSpansWithinRange(info.FileSystemAudioDuration, info.SqlFileStreamAudioDuration) &&
                        MimeTypes.Canonicalise(info.SqlFileStreamMimeType) == MimeTypes.GetMimeTypeFromExtension(info.FileSystemFile.Extension))
                    {
                        Console.WriteLine();
                        Console.WriteLine("no data in db, file does exist and has data, mime type/ext match, db duration and file duration within range");
                        Console.WriteLine("-> data location field is incorrect, fix it");
                        Console.WriteLine();

                        reading.DataLocation = AudioReadingDataLocation.FileSystem;
                        db.SubmitChanges();
                    }

                    /*
                     * by now file should be in filesystem
                     * */

                    // get and update file byte size
                    FileInfo exportedFile = this.fileSystemAudioDataStorage.GetDataFile(reading);

                    if (!reading.DataSizeBytes.HasValue || reading.DataSizeBytes.Value != exportedFile.Length)
                    {
                        reading.DataSizeBytes = exportedFile.Length;
                    }

                    // get and update audio file duration
                    TimeSpan exportedFileDuration = this.audioUtility.Duration(exportedFile, reading.MimeType);

                    if (!reading.Length.HasValue || !MigrationUtils.TimeSpansWithinRange(TimeSpan.FromMilliseconds(reading.Length.Value), exportedFileDuration))
                    {
                        reading.Length = Convert.ToInt32(exportedFileDuration.TotalMilliseconds);
                    }

                    // get and update data location
                    AudioReadingDataLocation currentDataLocation =
                        db.AudioReadings.Where(ar => ar.AudioReadingID == reading.AudioReadingID).FirstOrDefault().
                            DataLocation;

                    if (currentDataLocation != AudioReadingDataLocation.FileSystem)
                    {
                        reading.DataLocation = AudioReadingDataLocation.FileSystem;
                    }

                    // reset upload type to null
                    reading.UploadType = null;

                    // submit updates
                    db.SubmitChanges();

                    // set Data column to null if all is ok
                    if (info.FileSystemFile.Length > 0 &&
                        fileExists && file != null && File.Exists(file.FullName) &&
                        MigrationUtils.TimeSpansWithinRange(TimeSpan.FromMilliseconds(reading.Length.Value), exportedFileDuration) &&
                        reading.DataSizeBytes.Value == exportedFile.Length &&
                        location == AudioReadingDataLocation.FileSystem)
                    {
                        MigrationUtils.ClearSqlFileStreamData(reading);
                    }

                    overallWatch.Stop();
                    info.TotalDuration = overallWatch.Elapsed;
                }
                catch (Exception ex)
                {
                    info.Message = " --Exception Message-- " + ex.Message;
                    info.LogType = LogType.Error;

                    if (reading != null)
                    {
                        reading.DataLocation = AudioReadingDataLocation.SqlFileStreamExportFailed;

                        string msg = "Error: " + ex.Message;
                        if (msg.Length > 90)
                        {
                            msg = msg.Substring(0, 90);
                        }

                        reading.UploadType = msg;
                        db.SubmitChanges();
                    }
                }
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

        /// <summary>
        /// TExport audio data from db to file system.
        /// </summary>
        /// <param name="db">
        /// Data Context.
        /// </param>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <param name="info">
        /// Migration info.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// <c>InvalidOperationException</c>.
        /// </exception>
        /// <returns>
        /// Migration Info.
        /// </returns>
        private MigrationInfo ExportData(QutSensorsDb db, AudioReading reading, MigrationInfo info)
        {
            Console.WriteLine("Exporting to file.");

            /*****************
             * Export audio reading sql file stream to file.
             *****************/

            var watch = new Stopwatch();
            watch.Start();

            ReadFromSqlFileStreamWriteToFileSystem(reading);

            watch.Stop();
            info.ReadWriteDuration = watch.Elapsed;

            /*****************
             * Get audio file info.
             *****************/

            FileInfo file = this.fileSystemAudioDataStorage.GetDataFile(reading);
            info.FileSystemFile = file;

            if (file == null || !File.Exists(file.FullName) || file.Length < 1)
            {
                // file must exist
                throw new InvalidOperationException("Migration Error: Audio Reading exported to file but file does not exist or contains no data.");
            }

            if (info.SqlFileStreamDataLength != info.FileSystemFile.Length)
            {
                // data lengths must match
                const string Msg = "Migration Error: Audio Reading written to file but data lengths don't match. Sql FileStream: {0} file: {1}";

                throw new InvalidOperationException(string.Format(Msg, info.SqlFileStreamDataLength, info.FileSystemFile.Length));
            }

            if (MimeTypes.Canonicalise(info.SqlFileStreamMimeType) != MimeTypes.GetMimeTypeFromExtension(info.FileSystemFile.Extension))
            {
                // mime types must match
                const string Msg = "Migration Error: Audio Reading written to file but mime type and extension do not match. Sql FileStream: {0} file: {1} ({2})";

                throw new InvalidOperationException(
                    string.Format(
                        Msg,
                        MimeTypes.Canonicalise(info.SqlFileStreamMimeType),
                        MimeTypes.GetMimeTypeFromExtension(info.FileSystemFile.Extension),
                        info.FileSystemFile.Extension));
            }

            info.FileSystemAudioDuration = this.audioUtility.Duration(file, reading.MimeType);

            if (!MigrationUtils.TimeSpansWithinRange(info.FileSystemAudioDuration, info.SqlFileStreamAudioDuration))
            {
                // sqlfilestream data must match file system data
                const string Msg = "Migration Error: Audio Reading written to file but durations do not match. Sql FileStream: {0}  file: {1}";

                throw new InvalidOperationException(
                    string.Format(
                        Msg,
                        info.SqlFileStreamAudioDuration.ToReadableString(),
                        info.FileSystemAudioDuration.ToReadableString()));
            }

            /*****************
            * Update db to indicate file has been moved.
            *****************/

            // set data location
            reading.DataLocation = AudioReadingDataLocation.FileSystem;
            db.SubmitChanges();

            info.CopiedFromSqlFileStreamToFileSystem = true;

            return info;
        }
    }
}
