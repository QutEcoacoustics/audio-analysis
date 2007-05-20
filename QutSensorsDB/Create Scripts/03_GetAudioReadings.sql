IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetAudioReadings')
	BEGIN
		DROP  Procedure  GetAudioReadings
	END

GO

CREATE Procedure GetAudioReadings(@sensorID uniqueidentifier) AS

--RETURNS: TupleList[AudioReading](Guid? readingID, Guid sensorID, DateTime time, int spectrumDataLength, int spectrogramDataLength)

SELECT AudioReadingID, SensorID, [Time], ISNULL(DataLength(SpectrumData), 0) AS SpectrumDataLength, ISNULL(DataLength(SpectrogramData), 0) AS SpectrogramDataLength
FROM AudioReadings
WHERE SensorID = @sensorID

GO