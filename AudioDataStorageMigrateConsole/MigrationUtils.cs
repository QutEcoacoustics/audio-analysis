// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationUtils.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Migration Utilities.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioDataStorageMigrateConsole
{
    using System;
    using System.Linq;

    using QutSensors.Data;
    using QutSensors.Data.Linq;

    /// <summary>
    /// Migration Utilities.
    /// </summary>
    public static class MigrationUtils
    {
        /// <summary>
        /// Get audio reading sql file stream data length.
        /// </summary>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Audio reading id must be set.
        /// </exception>
        /// <returns>
        /// The audio reading sql file stream data length.
        /// </returns>
        /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><c>reading</c> is out of range.</exception>
        public static long AudioReadingSqlFileStreamDataLength(AudioReading reading)
        {
            if (reading == null || reading.AudioReadingID == Guid.Empty)
            {
                throw new ArgumentException("Audio reading id must be set.", "reading");
            }

            using (var db = new QutSensorsDb())
            {
                var items =
                    db.ExecuteQuery(
                        @"
SELECT top 1 ar.[AudioReadingID], datalength(ar.[Data]) as [DataLength] 
FROM AudioReadings ar
WHERE ar.[AudioReadingID] = '" +
                        reading.AudioReadingID + "';");

                if (items.Count() > 0)
                {
                    var check =
                        items.Select(
                            i =>
                            new
                                {
                                    Id = i["AudioReadingID"] == null ? new Guid?() : Guid.Parse(i["AudioReadingID"].ToString()),
                                    DataLength = i["DataLength"] == null ? new long?() : long.Parse(i["DataLength"].ToString())
                                });

                    var item = check.FirstOrDefault();

                    if (item == null)
                    {
                        throw new InvalidOperationException("Audio reading with id " + reading.AudioReadingID + " not found in database.");
                    }

                    if (item.Id.HasValue && item.DataLength.HasValue && item.Id == reading.AudioReadingID)
                    {
                        return item.DataLength.Value;
                    }

                    if (!item.Id.HasValue)
                    {
                        throw new InvalidOperationException("Audio reading with id " + reading.AudioReadingID + " did not return matching id from database.");
                    }

                    if (!item.DataLength.HasValue)
                    {
                        throw new InvalidOperationException("Audio reading with id " + reading.AudioReadingID + " did not return a data length.");
                    }
                }

                throw new ArgumentOutOfRangeException(
                    "reading",
                    reading.AudioReadingID,
                    "Could not get data length of given audio reading id from database.");
            }
        }

        /// <summary>
        /// Check that time spans match or are close enough.
        /// </summary>
        /// <param name="ts1">Time Span 1.</param>
        /// <param name="ts2">Time Span 2.</param>
        /// <returns>True if time spans are within range.</returns>
        public static bool TimeSpansWithinRange(TimeSpan ts1, TimeSpan ts2)
        {
            return TimeSpansWithinRange(ts1, ts2, 200);
        }

        /// <summary>
        /// Check that time spans match or are close enough.
        /// </summary>
        /// <param name="ts1">
        /// Time Span 1.
        /// </param>
        /// <param name="ts2">
        /// Time Span 2.
        /// </param>
        /// <param name="milliseconds">
        /// The milliseconds.
        /// </param>
        /// <returns>
        /// True if time spans are within range.
        /// </returns>
        public static bool TimeSpansWithinRange(TimeSpan ts1, TimeSpan ts2, double milliseconds)
        {
            TimeSpan difference;

            if (ts1 > ts2)
            {
                difference = ts1 - ts2;
            }
            else
            {
                difference = ts2 - ts1;
            }

            return difference <= TimeSpan.FromMilliseconds(milliseconds);
        }

        /// <summary>
        /// Clear sql file stream data.
        /// </summary>
        /// <param name="reading">
        /// The reading.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Audio reading id must be set.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <c>InvalidOperationException</c>.
        /// </exception>
        public static void ClearSqlFileStreamData(AudioReading reading)
        {
            if (reading == null || reading.AudioReadingID == Guid.Empty)
            {
                throw new ArgumentException("Audio reading id must be set.", "reading");
            }

            using (var db = new QutSensorsDb())
            {
                // parameters use braces eg. {0} {1}
                ////const string Sql1 = "Update [AudioReadings] Set [Data] = 0x Where [AudioReadingID] = {0}";
                const string Sql2 = "Update [AudioReadings] Set [Data] = NULL Where [AudioReadingID] = {0}";

                int rowsAffected = db.ExecuteCommand(Sql2, reading.AudioReadingID.ToString());

                if (rowsAffected != 1)
                {
                    throw new InvalidOperationException(
                        "Updated " + rowsAffected + " rows instead of 1. Drop everything, and check this!!!");
                }
            }
        }
    }
}
