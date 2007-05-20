IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateAudioSpectrumData')
	BEGIN
		DROP  Procedure  UpdateAudioSpectrumData
	END

GO

CREATE Procedure UpdateAudioSpectrumData(@readingID uniqueidentifier, @data image) AS

UPDATE AudioReadings
SET SpectrumData = @data
WHERE AudioReadingID = @readingID

GO

