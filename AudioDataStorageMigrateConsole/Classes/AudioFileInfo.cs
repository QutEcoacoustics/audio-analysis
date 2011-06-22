// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AudioFileInfo.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the AudioFileInfo type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioDataStorageMigrateConsole.Classes
{
    using System;
    using System.IO;

    using QutSensors.Business.Storage;
    using QutSensors.Data.Linq;

    /// <summary>
    /// Audio File Info.
    /// </summary>
    public class AudioFileInfo
    {
        private readonly FileSystemAudioDataStorage fileSystemAudioDataStorage;

        public FileSystemAudioDataStorage FileSystemAudioDataStorage
        {
            get
            {
                return this.fileSystemAudioDataStorage;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioFileInfo"/> class.
        /// </summary>
        /// <param name="fileSystemAudioDataStorage">
        /// The file system audio data storage.
        /// </param>
        public AudioFileInfo(FileSystemAudioDataStorage fileSystemAudioDataStorage)
        {
            this.fileSystemAudioDataStorage = fileSystemAudioDataStorage;
        }

        /// <summary>
        /// Get byte size of the audio.
        /// </summary>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <returns>
        /// Byte size of audio.
        /// </returns>
        public long GetByteSize(AudioReading reading)
        {
            try
            {
                FileInfo file = this.fileSystemAudioDataStorage.GetDataFile(reading);
                if (file != null) return file.Length;
            }
            catch
            {
            }

            return 0;
        }

        /// <summary>
        /// Check if a file exists.
        /// </summary>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <returns>
        /// True if data exists, otherwise false.
        /// </returns>
        public bool DataExists(AudioReading reading)
        {
            FileInfo file = null;

            try
            {
                file = this.fileSystemAudioDataStorage.GetDataFile(reading);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("File for audio reading does not exist"))
                {
                    return false;
                }

                throw;
            }

            return file != null && File.Exists(file.FullName);
        }
    }
}
