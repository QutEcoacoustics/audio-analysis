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
    using System.IO;

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
    }
}