IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetAudioSpectrumData')
	BEGIN
		DROP  Procedure  GetAudioSpectrumData
	END

GO

CREATE Procedure GetAudioSpectrumData(@readingID uniqueidentifier) AS

--RETURNS: Scalar(byte[])

SELECT SpectrumData
FROM AudioReadings
WHERE AudioReadingID = @readingID

GO