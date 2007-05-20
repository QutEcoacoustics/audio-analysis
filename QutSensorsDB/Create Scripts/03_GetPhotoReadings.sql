IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetPhotoReadings')
	BEGIN
		DROP  Procedure  GetPhotoReadings
	END

GO

CREATE Procedure GetPhotoReadings(@sensorID uniqueidentifier) AS

--RETURNS: TupleList[PhotoReading](Guid? readingID, Guid sensorID, DateTime time)

SELECT PhotoReadingID, SensorID, [Time]
FROM PhotoReadings
WHERE SensorID = @sensorID

GO