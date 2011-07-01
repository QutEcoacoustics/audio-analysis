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
            long dataLength = 0;

            try
            {
                dataLength = MigrationUtils.AudioReadingSqlFileStreamDataLength(reading);
            }
            catch
            {
                dataLength = 0;
            }

            return dataLength;
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
            // use caution: need to default to saying there is data, just in case.
            bool dataExists = true;

            try
            {
                dataExists = MigrationUtils.AudioReadingSqlFileStreamDataExists(reading);
            }
            catch
            {
                dataExists = true;
            }

            return dataExists;
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
