IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdatePhotoReadingData')
	BEGIN
		DROP  Procedure  UpdatePhotoReadingData
	END

GO

CREATE Procedure UpdatePhotoReadingData(@readingID uniqueidentifier, @data image) AS

UPDATE PhotoReadings
SET Data = @data
WHERE PhotoReadingID = @readingID

GO

/*
GRANT EXEC ON Stored_Procedure_Name TO PUBLIC

*/

