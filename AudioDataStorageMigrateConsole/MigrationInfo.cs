// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationInfo.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioDataStorageMigrateConsole
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using QutSensors.Shared.LogProviders;

    /// <summary>
    /// The migration info.
    /// </summary>
    public class MigrationInfo
    {
        #region Properties

        /// <summary>
        /// Gets or sets FileSystemAudioDuration.
        /// </summary>
        public TimeSpan FileSystemAudioDuration { get; set; }

        /// <summary>
        /// Gets or sets ReadWriteDuration.
        /// </summary>
        public TimeSpan ReadWriteDuration { get; set; }

        /// <summary>
        /// Gets or sets SqlFileStreamAudioDuration.
        /// </summary>
        public TimeSpan SqlFileStreamAudioDuration { get; set; }

        /// <summary>
        /// Gets or sets SqlFileStreamDataLength.
        /// </summary>
        public long SqlFileStreamDataLength { get; set; }

        /// <summary>
        /// Gets or sets TotalDuration.
        /// </summary>
        public TimeSpan TotalDuration { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingId.
        /// </summary>
        public Guid AudioReadingId { get; set; }

        /// <summary>
        /// Gets or sets FileSystemFile.
        /// </summary>
        public FileInfo FileSystemFile { get; set; }

        /// <summary>
        /// Gets or sets SqlFileStreamMimeType.
        /// </summary>
        public string SqlFileStreamMimeType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Copied From Sql FileStream To File System.
        /// </summary>
        public bool CopiedFromSqlFileStreamToFileSystem { get; set; }

        /// <summary>
        /// Gets or sets Message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets LogType.
        /// </summary>
        public LogType LogType { get; set; }

        /// <summary>
        /// Gets or sets OverallRunningCount.
        /// </summary>
        public long OverallRunningCount { get; set; }

        /// <summary>
        /// Gets or sets OverallRunningDuration.
        /// </summary>
        public TimeSpan OverallRunningDuration { get; set; }

        #endregion

        /// <summary>
        /// Migration Info to String.
        /// </summary>
        /// <returns>
        /// String representation of Migration info.
        /// </returns>
        public override string ToString()
        {
            return string.Format(
                "{0}, Sql File Stream: {1}, {2}, {3}, {4}, {5}, Export Duration: {6}, File System: {7}, {8}, {9}, {10}, {11}, Total Duration: {12}",
                this.AudioReadingId,
                this.SqlFileStreamMimeType,
                this.SqlFileStreamDataLength,
                this.SqlFileStreamDataLength.ToByteDisplay(),
                this.SqlFileStreamAudioDuration,
                this.SqlFileStreamAudioDuration.ToReadableString(),
                this.ReadWriteDuration,
                this.FileSystemFile.Extension,
                this.FileSystemFile.Length,
                this.FileSystemFile.Length.ToByteDisplay(),
                this.FileSystemAudioDuration,
                this.FileSystemAudioDuration.ToReadableString(),
                this.TotalDuration);
        }

        /// <summary>
        /// Get Migration info properties as string array.
        /// </summary>
        /// <returns>
        /// String array of migraiton properties.
        /// </returns>
        public IEnumerable<object> ToStrings()
        {
            return new List<object>
                {
                     this.AudioReadingId,
                    this.SqlFileStreamMimeType,
                    this.SqlFileStreamDataLength,
                    this.SqlFileStreamDataLength.ToByteDisplay(),
                    this.SqlFileStreamAudioDuration,
                    this.SqlFileStreamAudioDuration.ToReadableString(),
                    this.ReadWriteDuration,
                    this.FileSystemFile != null ? this.FileSystemFile.Extension : "unknown",
                    this.FileSystemFile != null ? this.FileSystemFile.Length.ToString() : "unknown",
                    this.FileSystemFile != null ? this.FileSystemFile.Length.ToByteDisplay() : "unknown",
                    this.FileSystemAudioDuration,
                    this.FileSystemAudioDuration.ToReadableString(),
                    this.TotalDuration,
                    this.OverallRunningCount,
                    this.OverallRunningDuration,
                    this.OverallRunningDuration.ToReadableString(),
                };
        }
    }
}