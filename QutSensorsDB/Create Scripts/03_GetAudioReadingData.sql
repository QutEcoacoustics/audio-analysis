IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetAudioReadingData')
	BEGIN
		DROP  Procedure  GetAudioReadingData
	END

GO

CREATE Procedure GetAudioReadingData(@readingID uniqueidentifier) AS

--RETURNS: Scalar(byte[])

SELECT Data
FROM AudioReadings
WHERE AudioReadingID = @readingID

GO

/*
GRANT EXEC ON Stored_Procedure_Name TO PUBLIC

*/

