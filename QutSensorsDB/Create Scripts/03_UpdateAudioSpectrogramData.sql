IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateAudioSpectrogramData')
	BEGIN
		DROP  Procedure  UpdateAudioSpectrogramData
	END

GO

CREATE Procedure UpdateAudioSpectrogramData(@readingID uniqueidentifier, @data image) AS

UPDATE AudioReadings
SET SpectrogramData = @data
WHERE AudioReadingID = @readingID

GO