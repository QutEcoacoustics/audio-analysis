using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace AudioDataStorageMigrateConsole.Diag
{
    public class AudioReadingDiagItem
    {
        public Guid AudioReadingId { get; set; }

        public string DbMimeType { get; set; }

        public bool DbMimeTypeIsNull { get; set; }

        public string FileMimeType { get; set; }

        public bool DbDataIsNull { get; set; }

        public bool FileExists { get; set; }

        public long DbDataLength { get; set; }

        public long DbDataSizeBytes { get; set; }

        public bool DbDataSizeBytesIsNull { get; set; }

        public long FileDataSizeBytes { get; set; }

        public string DbDataLocation { get; set; }

        public string DbState { get; set; }

        public TimeSpan DbDuration { get; set; }

        public long DbDurationMs { get; set; }

        public bool DbDurationMsIsNull { get; set; }

        public TimeSpan FileDuration { get; set; }

        public long FileDurationMs { get; set; }

        public string FileExtension { get; set; }

        public string FileHash { get; set; }

        /// <summary>
        /// Gets or sets OverallRunningCount.
        /// </summary>
        public long OverallRunningCount { get; set; }

        /// <summary>
        /// Gets or sets OverallRunningDuration.
        /// </summary>
        public TimeSpan OverallRunningDuration { get; set; }

        private readonly List<string> exceptions = new List<string>();

        public IEnumerable<string> Exceptions
        {
            get
            {
                return exceptions.AsReadOnly();
            }
        }

        public void AddException(Exception ex)
        {
            this.exceptions.Add(ex.Message);
        }

        public bool HasProblems()
        {
            return this.exceptions.Count > 0 ||
                this.DbMimeTypeIsNull ||
                this.DbMimeType != this.FileMimeType ||
                !this.DbDataIsNull ||
                !this.FileExists ||
                this.DbDataSizeBytesIsNull ||
                this.DbDataLength != this.DbDataSizeBytes ||
                this.DbDataLength != this.FileDataSizeBytes ||
                this.DbDataSizeBytes != this.FileDataSizeBytes ||
                string.IsNullOrEmpty(this.DbDataLocation) ||
                this.DbDataLocation.ToLower() != "filesystem" ||
                this.DbDurationMsIsNull ||
                this.DbDurationMs != this.FileDurationMs;

         // TODO: include file hash matching hash of data exported from sql filestream col.
        }

        /// <summary>
        /// Migration Info to String.
        /// </summary>
        /// <returns>
        /// String representation of Migration info.
        /// </returns>
        public override string ToString()
        {
            return string.Join(", ", ToStrings());
        }

        /// <summary>
        /// Get Migration info properties as string array.
        /// </summary>
        /// <returns>
        /// String array of migraiton properties.
        /// </returns>
        public IEnumerable<object> ToStrings()
        {
            var csvRow = new List<string>
                {
                    AudioReadingId.ToString(),
                    
                    DbMimeType,
                    DbMimeTypeIsNull.ToString(),
                    FileMimeType,

                    DbDataIsNull.ToString(),
                    FileExists.ToString(),
                    DbDataLength.ToString(),
                    DbDataLength.ToByteDisplay(),
                    DbDataSizeBytes.ToString(),
                    DbDataSizeBytes.ToByteDisplay(),
                    DbDataSizeBytesIsNull.ToString(),
                    FileDataSizeBytes.ToString(),
                    FileDataSizeBytes.ToByteDisplay(),

                    DbDataLocation,
                    DbState,
                    

                    DbDuration.ToString(),
                    DbDuration.ToReadableString(),
                    DbDurationMs.ToString(),
                    DbDurationMsIsNull.ToString(),
                    FileDuration.ToString(),
                    FileDuration.ToReadableString(),
                    FileDurationMs.ToString(),

                    FileExtension,
                    FileHash,

                    OverallRunningCount.ToString(),
                    OverallRunningDuration.ToString(),
                    OverallRunningDuration.ToReadableString(),
                    OverallRunningDuration.TotalMilliseconds.ToString()
                };
            return csvRow;

        }

        public static IEnumerable<string> GetHeaders()
        {
            var csvHeaders = new List<string>
                {
                    "AudioReadingId",
                    
                    "DbMimeType",
                    "DbMimeTypeIsNull",
                    "FileMimeType",

                    "DbDataIsNull",
                    "FileExists",
                    "DbDataLength",
                    "DbDataLengthFormatted",
                    "DbDataSizeBytes",
                    "DbDataSizeBytesFormatted",
                    "DbDataSizeBytesIsNull",
                    "FileDataSizeBytes",
                    "FileDataSizeBytesFormatted",

                    "DbDataLocation",
                    "DbState",
                    

                    "DbDuration",
                    "DbDurationFormatted",
                    "DbDurationMs",
                    "DbDurationMsIsNull",
                    "FileDuration",
                    "FileDurationFormatted",
                    "FileDurationMs",

                    "FileExtension",
                    "FileHash",

                    "OverallRunningCount",
                    "OverallRunningDuration",
                    "OverallRunningDurationFormatted",
                    "OverallRunningDurationMs",
                };
            return csvHeaders;
        }
    }
}
