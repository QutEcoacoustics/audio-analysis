using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioDataStorageMigrateConsole.ExportAndVerify
{
    using System.IO;
    using System.Security.Cryptography;

    using AudioDataStorageMigrateConsole.Diag;

    using QutSensors.Business.Storage;
    using QutSensors.Data.Linq;
    using QutSensors.Shared.LogProviders;

    /// <summary>
    /// Simple class to export data from db, compare exported data and 
    /// data in db, then remove data in db if export was successful.
    /// Uses the data length and sha1 hash.
    /// </summary>
    public class ExportVerifyRunner
    {
        private readonly SHA1CryptoServiceProvider sha1;

        private readonly ILogProvider logProvider;

        private readonly SqlFileStreamAudioDataStorage sqlAudioDataStorage;

        private readonly FileSystemAudioDataStorage fileSystemAudioDataStorage;

        public ExportVerifyRunner(string logFileDir,
            SqlFileStreamAudioDataStorage sqlAudioDataStorage,
            FileSystemAudioDataStorage fileSystemAudioDataStorage
           )
        {
            this.sha1 = new SHA1CryptoServiceProvider();

            this.sqlAudioDataStorage = sqlAudioDataStorage;
            this.fileSystemAudioDataStorage = fileSystemAudioDataStorage;

            this.logProvider = new MultiLogProvider(new CsvTextFileLogProvider(logFileDir, AudioReadingDiagItem.GetHeaders()), new ConsoleLogProvider());

        }

        public class DataInfo
        {
            public long Length { get; set; }

            public string Sha1Hash { get; set; }
        }

        public DataInfo GetFileInfo(FileInfo file)
        {
            var info = new DataInfo { Length = file.Length };

            using (var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // TODO: will need to hash each 8000 byte segment separately to match sql server hashbytes function limitations.
                info.Sha1Hash = Convert.ToBase64String(this.sha1.ComputeHash(fileStream));
            }

            return info;
        }

        public DataInfo GetDbInfo(AudioReading reading)
        {
            var info = new DataInfo
                {
                    Length = MigrationUtils.AudioReadingSqlFileStreamDataLength(reading),
                    Sha1Hash = MigrationUtils.AudioReadingSqlFileStreamDataSha1Hash(reading)
                };

            return info;
        }
    }
}
