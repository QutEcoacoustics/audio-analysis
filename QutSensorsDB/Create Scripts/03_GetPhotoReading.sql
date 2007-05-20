IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetPhotoReading')
	BEGIN
		DROP  Procedure  GetPhotoReading
	END

GO

CREATE Procedure GetPhotoReading(@readingID uniqueidentifier) AS

--RETURNS: Tuple[PhotoReading](Guid? readingID, Guid sensorID, DateTime time)

SELECT PhotoReadingID, SensorID, [Time]
FROM PhotoReadings
WHERE PhotoReadingID = @readingID

GO