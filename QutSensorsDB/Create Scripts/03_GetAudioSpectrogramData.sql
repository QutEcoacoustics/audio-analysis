 IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetAudioSpectrogramData')
	BEGIN
		DROP  Procedure  GetAudioSpectrogramData
	END

GO

CREATE Procedure GetAudioSpectrogramData(@readingID uniqueidentifier) AS

--RETURNS: Scalar(byte[])

SELECT SpectrogramData
FROM AudioReadings
WHERE AudioReadingID = @readingID

GO