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

        public IEnumerable<string> GetIssues()
        {
            if (this.exceptions.Count > 0) yield return this.exceptions.Count + " exceptions";
            if (this.DbMimeTypeIsNull) yield return "Mime type column is null";
            if (this.DbMimeType != this.FileMimeType) yield return "Db mime type does not match file mime type";
            if (!this.DbDataIsNull) yield return "Data in db";
            if (!this.FileExists) yield return "Audio file does not exist";
            if (this.DbDataSizeBytesIsNull) yield return "DataSizeBytes column is null";
            if (!this.DbDataIsNull && this.DbDataLength != this.DbDataSizeBytes) yield return "Db contains data and the data length does not match db DataSizeBytes";
            if (!this.DbDataIsNull && this.DbDataLength != this.FileDataSizeBytes) yield return "DB contains data and the data length does not match file size bytes";
            if (this.DbDataSizeBytes != this.FileDataSizeBytes) yield return "DB DataSizeBytes does not match file size bytes";
            if (string.IsNullOrEmpty(this.DbDataLocation)) yield return "DB DataLocation is null or empty";
            if (!string.IsNullOrEmpty(this.DbDataLocation) && this.DbDataLocation.ToLower() != "filesystem") yield return "Db data location is not 'filesystem', it is '" + this.DbDataLocation + "'";
            if (this.DbDurationMsIsNull) yield return "Db duration is null";
            if (this.FileDurationMs < 1) yield return "File duration is less than 1ms";
            if (this.DbDurationMs != this.FileDurationMs) yield return "Db duration does not match file duation";

            // TODO: include file hash matching hash of data exported from sql filestream col.
        }

        /// <summary>
        /// Properties to String.
        /// </summary>
        /// <returns>
        /// String representation of Properties.
        /// </returns>
        public override string ToString()
        {
            return string.Join(", ", ToStrings());
        }

        /// <summary>
        /// Get properties as string array.
        /// </summary>
        /// <returns>
        /// String array of properties.
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

                    string.Join(" | ",Exceptions),
                    string.Join(" | ",this.GetIssues()),

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

                    "Exceptions",
                    "Reasons",

                    "OverallRunningCount",
                    "OverallRunningDuration",
                    "OverallRunningDurationFormatted",
                    "OverallRunningDurationMs",
                };
            return csvHeaders;
        }
    }
}
