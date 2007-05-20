IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetAudioReadingsWithoutSpectrograms')
	BEGIN
		DROP  Procedure  GetAudioReadingsWithoutSpectrograms
	END

GO

CREATE Procedure GetAudioReadingsWithoutSpectrograms AS

--RETURNS: TupleList[AudioReading](Guid? readingID, Guid sensorID, DateTime time, int spectrumDataLength, int spectrogramDataLength)

SELECT AudioReadingID, SensorID, [Time], ISNULL(DataLength(SpectrumData), 0) AS SpectrumDataLength, ISNULL(DataLength(SpectrogramData), 0) AS SpectrogramDataLength
FROM AudioReadings
WHERE DataLength(SpectrogramData) IS NULL

GO 