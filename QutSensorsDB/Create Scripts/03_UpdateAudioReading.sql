IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateAudioReading')
	BEGIN
		DROP  Procedure  UpdateAudioReading
	END

GO

CREATE Procedure UpdateAudioReading(@readingID uniqueidentifier, @sensorID uniqueidentifier, @time datetime) AS

--RETURNS: Scalar(Guid)

DECLARE @idLookup uniqueidentifier
SELECT @idLookup = AudioReadingID FROM AudioReadings WHERE AudioReadingID = @readingID

IF @readingID IS NULL OR @idLookup IS NULL
BEGIN
	IF @readingID IS NULL SET @readingID = NEWID()
	
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

*/

