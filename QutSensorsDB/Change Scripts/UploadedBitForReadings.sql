ALTER TABLE AudioReadings ADD Uploaded bit NOT NULL DEFAULT 0
GO
ALTER TABLE PhotoReadings ADD Uploaded bit NOT NULL DEFAULT 0
GO

CREATE PROCEDURE MarkAudioReadingAsUploaded(@readingID uniqueidentifier) AS

UPDATE AudioReadings
SET Uploaded = 1
WHERE AudioReadingID = @readingID

GO

CREATE PROCEDURE MarkPhotoReadingAsUploaded(@readingID uniqueidentifier) AS

UPDATE PhotoReadings
SET Uploaded = 1
WHERE PhotoReadingID = @readingID

GO

CREATE PROCEDURE GetAudioReadingsNotUploaded(@sensorID uniqueidentifier) AS

--RETURNS: TupleList[AudioReading](Guid? readingID, Guid sensorID, DateTime time, int spectrumDataLength, int spectrogramDataLength)

SELECT AudioReadingID, SensorID, [Time], ISNULL(DataLength(SpectrumData), 0) AS SpectrumDataLength, ISNULL(DataLength(SpectrogramData), 0) AS SpectrogramDataLength
FROM AudioReadings
WHERE SensorID = @sensorID AND Uploaded = 0

GO

CREATE Procedure GetPhotoReadingsNotUploaded(@sensorID uniqueidentifier) AS

--RETURNS: TupleList[PhotoReading](Guid? readingID, Guid sensorID, DateTime time)

SELECT PhotoReadingID, SensorID, [Time]
FROM PhotoReadings
WHERE SensorID = @sensorID AND Uploaded = 0

GO