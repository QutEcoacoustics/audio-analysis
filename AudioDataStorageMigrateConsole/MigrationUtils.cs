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

                    if (!item.Id.HasValue)
                    {
                        throw new InvalidOperationException("Audio reading with id " + reading.AudioReadingID + " did not return matching id from database.");
                    }

                    if (!item.DataLength.HasValue)
                    {
                        throw new InvalidOperationException("Audio reading with id " + reading.AudioReadingID + " did not return a data length.");
                    }

                    if (item.Id.HasValue && item.DataLength.HasValue && item.Id == reading.AudioReadingID)
                    {
                        return item.DataLength.Value;
                    }
                }

                throw new ArgumentOutOfRangeException(
                    "reading",
                    reading.AudioReadingID,
                    "Could not get data length of given audio reading id from database.");
            }
        }

        /// <summary>
        /// Get audio reading sql file stream sha1 hash. Will not be the same as simply hashing the data, 
        /// as SQL Server HASHBYTES function is limited to 8000 length. This method returns the result of 
        /// hashing each 8000-segment separately.
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
        /// <remarks>
        /// Requires a more than built-in HASHBYTES function for data longer than 8000.
        /// See: http://www.scribd.com/doc/54158465/99/HashBytes-SHA-1-Limitations
        /// </remarks>
        public static string AudioReadingSqlFileStreamDataSha1Hash(AudioReading reading)
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
DECLARE @hashRegister1 varbinary(20) = NULL,
@hashRegister2 varbinary(40) = NULL,
@i int = 1;

SELECT @hashRegister1 = HashBytes('SHA1', SUBSTRING(@input, @i, 8000));
SET @i = @i + 8000;

WHILE @i < DATALENGTH(@input)
BEGIN
SET @hashRegister2 = @hashRegister1 +
HashBytes ('SHA1', SUBSTRING(@input, @i, 8000));
SET @hashRegister1 = HashBytes('SHA1', @hashRegister2);
SET @i = @i + 8000;
END;
SELECT @hashRegister1 as hashresult;

");
                
                if (items.Count() > 0)
                {
                    var check =
                        items.Select(
                            i =>
                            new
                                {
                                    Id =
                                i["AudioReadingID"] == null ? new Guid?() : Guid.Parse(i["AudioReadingID"].ToString()),
                                    HashResult =
                                i["hashresult"] == null
                                    ? string.Empty
                                    : (string.IsNullOrEmpty(i["hashresult"].ToString())
                                           ? string.Empty
                                           : i["hashresult"].ToString()),
                                });

                    var item = check.FirstOrDefault();

                    if (item == null)
                    {
                        throw new InvalidOperationException(
                            "Audio reading with id " + reading.AudioReadingID + " not found in database.");
                    }

                    if (!item.Id.HasValue)
                    {
                        throw new InvalidOperationException(
                            "Audio reading with id " + reading.AudioReadingID +
                            " did not return matching id from database.");
                    }

                    if (!item.DataLength.HasValue)
                    {
                        throw new InvalidOperationException(
                            "Audio reading with id " + reading.AudioReadingID + " did not return a data length.");
                    }

                    if (item.Id.HasValue && item.DataLength.HasValue && item.Id == reading.AudioReadingID)
                    {
                        return item.DataLength.Value;
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

        public static bool AudioReadingSqlFileStreamDataIsNull(AudioReading reading)
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
SELECT top 1 ar.[AudioReadingID], CASE WHEN ar.[Data] IS NULL THEN 0 ELSE 1 END AS [DataIsNull]
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
                                DataIsNullResult = i["DataIsNull"] == null ? new int?() : int.Parse(i["DataIsNull"].ToString())
                            });

                    var item = check.FirstOrDefault();

                    if (item == null)
                    {
                        throw new InvalidOperationException("Audio reading with id " + reading.AudioReadingID + " not found in database.");
                    }

                    if (!item.Id.HasValue)
                    {
                        throw new InvalidOperationException("Audio reading with id " + reading.AudioReadingID + " did not return matching id from database.");
                    }

                    if (!item.DataIsNullResult.HasValue)
                    {
                        throw new InvalidOperationException("Audio reading with id " + reading.AudioReadingID + " did not return a data length.");
                    }

                    if (item.Id.HasValue && item.DataIsNullResult.HasValue && item.Id == reading.AudioReadingID)
                    {
                        // data is null when query returns 0
                        return item.DataIsNullResult.Value == 0 ? true : false;
                    }
                }

                throw new ArgumentOutOfRangeException(
                    "reading",
                    reading.AudioReadingID,
                    "Could not determine if database data is null for given audio reading id.");
            }
        }
    }
}
