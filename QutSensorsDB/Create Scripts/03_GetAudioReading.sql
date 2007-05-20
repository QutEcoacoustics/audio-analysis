IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetAudioReading')
	BEGIN
		DROP  Procedure  GetAudioReading
	END

GO

CREATE Procedure GetAudioReading(@readingID uniqueidentifier) AS

--RETURNS: Tuple[AudioReading](Guid? readingID, Guid sensorID, DateTime time, int spectrumDataLength, int spectrogramDataLength)

SELECT AudioReadingID, SensorID, [Time], ISNULL(DataLength(SpectrumData), 0) AS SpectrumDataLength, ISNULL(DataLength(SpectrogramData), 0) AS SpectrogramDataLength
FROM AudioReadings
WHERE AudioReadingID = @readingID

GO
 