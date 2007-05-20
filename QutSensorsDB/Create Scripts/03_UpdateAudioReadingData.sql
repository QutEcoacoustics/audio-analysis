IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateAudioReadingData')
	BEGIN
		DROP  Procedure  UpdateAudioReadingData
	END

GO

CREATE Procedure UpdateAudioReadingData(@readingID uniqueidentifier, @data image) AS

UPDATE AudioReadings
SET Data = @data
WHERE AudioReadingID = @readingID

GO

/*
GRANT EXEC ON Stored_Procedure_Name TO PUBLIC

GO
*/

