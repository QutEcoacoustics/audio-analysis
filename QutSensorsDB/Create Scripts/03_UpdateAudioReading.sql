IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateAudioReading')
	BEGIN
		DROP  Procedure  UpdateAudioReading
	END

GO

CREATE Procedure UpdateAudioReading(@readingID uniqueidentifier, @sensorID uniqueidentifier, @time datetime) AS

--RETURNS: Scalar(Guid)

IF @readingID IS NULL
BEGIN
	SET @readingID = NEWID()
	
	INSERT INTO AudioReadings(AudioReadingID, SensorID, [Time])
	SELECT @readingID, @sensorID, @time
END
ELSE
BEGIN
	UPDATE AudioReadings
	SET SensorID = @sensorID, [Time] = @time
	WHERE AudioReadingID = @readingID
END

SELECT @readingID

GO

/*
GRANT EXEC ON Stored_Procedure_Name TO PUBLIC

GO
*/

