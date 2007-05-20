IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetPhotoReadingData')
	BEGIN
		DROP  Procedure  GetPhotoReadingData
	END

GO

CREATE Procedure GetPhotoReadingData(@readingID uniqueidentifier) AS

--RETURNS: Scalar(byte[])

SELECT Data
FROM PhotoReadings
WHERE PhotoReadingID = @readingID

GO

/*
GRANT EXEC ON Stored_Procedure_Name TO PUBLIC

GO
*/

