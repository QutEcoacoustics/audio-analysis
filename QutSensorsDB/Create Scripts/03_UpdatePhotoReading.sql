IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdatePhotoReading')
	BEGIN
		DROP  Procedure  UpdatePhotoReading
	END

GO

CREATE Procedure UpdatePhotoReading(@readingID uniqueidentifier, @sensorID uniqueidentifier, @time datetime) AS

--RETURNS: Scalar(Guid)

IF @readingID IS NULL
BEGIN
	SET @readingID = NEWID()
	
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

GO
*/

