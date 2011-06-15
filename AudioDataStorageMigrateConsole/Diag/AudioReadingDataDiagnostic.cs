namespace AudioDataStorageMigrateConsole.Diag
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    using AudioTools.AudioUtlity;

    using QutSensors.Business.Storage;
    using QutSensors.Data;
    using QutSensors.Data.Linq;
    using QutSensors.Shared;
    using QutSensors.Shared.LogProviders;

    public class AudioReadingDataDiagnostic : IDisposable
    {
        private readonly FileSystemAudioDataStorage fileSystemAudioDataStorage;

        private readonly IAudioUtility audioUtility;

        private readonly ILogProvider logProvider;

        private readonly SHA1CryptoServiceProvider sha1;

        public AudioReadingDataDiagnostic(
            string logFileDir,
            FileSystemAudioDataStorage fileSystemAudioDataStorage,
            IAudioUtility audioUtility)
        {
            this.fileSystemAudioDataStorage = fileSystemAudioDataStorage;
            this.audioUtility = audioUtility;

            this.sha1 = new SHA1CryptoServiceProvider();

            this.logProvider = new MultiLogProvider(new CsvTextFileLogProvider(logFileDir, AudioReadingDiagItem.GetHeaders()), new ConsoleLogProvider());
        }

        public void Run()
        {
            long count = 0;
            Console.WriteLine("Starting audio reading diagnostic...");

            var watch = new Stopwatch();
            watch.Start();

            using (var db = new QutSensorsDb())
            {
                foreach (var reading in db.AudioReadings.AsQueryable())
                {
                    count += 1;

                    AudioReadingDiagItem diag = null;
                    Exception exTemp = null;
                    try
                    {
                        diag = this.GetInfo(reading);
                    }
                    catch (Exception ex)
                    {
                        if (diag != null)
                        {
                            diag.AddException(ex);
                        }
                        else
                        {
                            exTemp = ex;
                        }
                    }

                    if (diag == null)
                    {
                        diag = new AudioReadingDiagItem { AudioReadingId = reading.AudioReadingID };
                        if (exTemp != null)
                        {
                            diag.AddException(exTemp);
                        }

                        // record as error
                        this.logProvider.WriteEntry(LogType.Error, "See audio reading id and exceptions.", diag.ToStrings().ToArray());
                    }
                    else if (diag.GetIssues().Count() > 0)
                    {
                        diag.OverallRunningCount = count;
                        diag.OverallRunningDuration = watch.Elapsed;
                        this.logProvider.WriteEntry(
                            diag.Exceptions.Count() > 0 ? LogType.Error : LogType.Information,
                            "Diagnosis: ",
                            diag.ToStrings().ToArray());
                    }
                }
            }
            watch.Stop();
        }

        private AudioReadingDiagItem GetInfo(AudioReading reading)
        {

            if (reading == null)
            {
                throw new ArgumentNullException("reading");
            }

            var diag = GetDbInfo(reading);

            diag = this.GetFileInfo(diag, reading);

            return diag;

        }

        private static AudioReadingDiagItem GetDbInfo(AudioReading reading)
        {
            using (var db = new QutSensorsDb())
            {
                var sql =
                    @"
select 
case when [Data] is null then 'true' else 'false' end as IsDataNull,
DATALENGTH([data]) as 'DataLength', 
case when MimeType is null then 'true' else 'false' end as IsMimeTypeNull,
MimeType,
case when DataSizeBytes is null then 'true' else 'false' end as IsDataSizeBytesNull,
DataSizeBytes,
DataLocation,
[State],
case when DurationMs is null then 'true' else 'false' end as IsDurationMsNull,
DurationMs
from AudioReadings
where AudioReadingID = '" + reading.AudioReadingID + "'";

                var items =
                    db.ExecuteQuery(sql);

                if (items.Count() == 1)
                {
                    var itemDisplay =
                        items.Select(
                            i =>
                            new AudioReadingDiagItem
                            {
                                AudioReadingId = reading.AudioReadingID,
                                DbDataIsNull = bool.Parse(i["IsDataNull"].ToString()),
                                DbDataLength = i["DataLength"] == null ? 0 : (string.IsNullOrEmpty(i["DataLength"].ToString()) ? 0 : long.Parse(i["DataLength"].ToString())),
                                DbMimeTypeIsNull = bool.Parse(i["IsMimeTypeNull"].ToString()),
                                DbMimeType = i["MimeType"] == null || string.IsNullOrEmpty(i["MimeType"].ToString())
                                    ? "not given"
                                    : i["MimeType"].ToString(),
                                DbDataSizeBytesIsNull = bool.Parse(i["IsDataSizeBytesNull"].ToString()),
                                DbDataSizeBytes = i["DataSizeBytes"] == null || string.IsNullOrEmpty(i["DataSizeBytes"].ToString())
                                    ? 0
                                    : long.Parse(i["DataSizeBytes"].ToString()),
                                DbDataLocation = i["DataLocation"] == null || string.IsNullOrEmpty(i["DataLocation"].ToString())
                                    ? "not given"
                                    : i["DataLocation"].ToString(),
                                DbState = i["State"] == null || string.IsNullOrEmpty(i["State"].ToString())
                                    ? "not given"
                                    : i["State"].ToString(),
                                DbDurationMsIsNull = bool.Parse(i["IsDurationMsNull"].ToString()),
                                DbDurationMs = i["DurationMs"] == null || string.IsNullOrEmpty(i["DurationMs"].ToString())
                                    ? 0
                                    : long.Parse(i["DurationMs"].ToString()),
                            });

                    var diag = itemDisplay.First();
                    diag.DbDuration = TimeSpan.FromMilliseconds(diag.DbDurationMs);
                    return diag;
                }

                throw new InvalidOperationException("Error occured - there was not exactly one result from db query.");
            }
        }

        private AudioReadingDiagItem GetFileInfo(AudioReadingDiagItem diag, AudioReading reading)
        {
            FileInfo file = null;

            try
            {
                file = this.fileSystemAudioDataStorage.GetDataFile(reading);
            }
            catch (Exception ex)
            {
                diag.AddException(ex);
            }

            if (file != null)
            {
                diag.FileMimeType = MimeTypes.GetMimeTypeFromExtension(file.Extension);
                diag.FileExists = File.Exists(file.FullName);
                diag.FileDataSizeBytes = file.Length;

                diag.FileExtension = file.Extension;

                using (var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    diag.FileHash = Convert.ToBase64String(this.sha1.ComputeHash(fileStream));
                }

                diag.FileDuration = this.audioUtility.Duration(file, diag.FileMimeType);
                diag.FileDurationMs = (long)diag.FileDuration.TotalMilliseconds;

                // TODO: how to check if readable? Just if duration can be calculated?
            }

            return diag;
        }

        public void Dispose()
        {
            if (this.sha1 != null)
            {
                this.sha1.Dispose();
            }
        }
    }
}
