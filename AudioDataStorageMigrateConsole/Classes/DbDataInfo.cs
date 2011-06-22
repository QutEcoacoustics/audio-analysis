// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbDataInfo.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Database Data Info.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioDataStorageMigrateConsole.Classes
{
    using System;
    using System.IO;

    using QutSensors.Business.Storage;
    using QutSensors.Data.Linq;

    /// <summary>
    /// Database Data Info.
    /// </summary>
    public class DbDataInfo
    {
        private readonly SqlFileStreamAudioDataStorage sqlAudioDataStorage;

        public SqlFileStreamAudioDataStorage SqlAudioDataStorage
        {
            get
            {
                return this.sqlAudioDataStorage;
            }
        }

        public DbDataInfo(SqlFileStreamAudioDataStorage sqlAudioDataStorage)
        {
            this.sqlAudioDataStorage = sqlAudioDataStorage;
        }

        /// <summary>
        /// Get the data byte size.
        /// </summary>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <returns>
        /// Count of bytes.
        /// </returns>
        public long GetByteSize(AudioReading reading)
        {
            try
            {
                long dataLength = MigrationUtils.AudioReadingSqlFileStreamDataLength(reading);
                return dataLength;
            }
            catch
            {
            }

            return 0;
        }

        /// <summary>
        /// Check if there is data in the row.
        /// </summary>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <returns>
        /// True if data exists.
        /// </returns>
        public bool DataExists(AudioReading reading)
        {
            try
            {
                bool dataExists = MigrationUtils.AudioReadingSqlFileStreamDataExists(reading);
                return dataExists;
            }
            catch
            {
            }

            // use caution: need to default to saying there is data, just in case.
            return true;
        }

        /// <summary>
        /// Remove the data from the row.
        /// </summary>
        /// <param name="reading">
        /// The reading.
        /// </param>
        public void ClearData(AudioReading reading)
        {
            MigrationUtils.ClearSqlFileStreamData(reading);
        }

        
    }
}
