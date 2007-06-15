IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdatePhotoReading')
	BEGIN
		DROP  Procedure  UpdatePhotoReading
	END

GO

CREATE Procedure UpdatePhotoReading(@readingID uniqueidentifier, @sensorID uniqueidentifier, @time datetime) AS

--RETURNS: Scalar(Guid)

DECLARE @idLookup uniqueidentifier
SELECT @idLookup = PhotoReadingID FROM PhotoReadings WHERE PhotoReadingID = @readingID

IF @readingID IS NULL OR @idLookup IS NULL
BEGIN
	IF @readingID IS NULL SET @readingID = NEWID()
	
	INSERT INTO PhotoReadings(PhotoReadingID, SensorID, [Time])
	SELECT @readingID, @sensorID, @time
END
ELSE
BEGIN
	UPDATE VideoReadings
	SET SensorID = @sensorID, [Time] = @time
	WHERE PhotoReadingID = @readingID
END

SELECT @readingID
GO

/*
GRANT EXEC ON Stored_Procedure_Name TO PUBLIC

*/

