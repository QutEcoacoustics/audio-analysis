IF EXISTS (SELECT * FROM sysobjects WHERE type = 'U' AND name = 'AudioReadings')
	BEGIN
		DROP  Table AudioReadings
	END
GO

CREATE TABLE AudioReadings
(
	AudioReadingID	uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
	SensorID	uniqueidentifier REFERENCES Sensors(SensorID),
	[Time] datetime,
	Data	image,
	SpectrumData image,
	SpectrogramData image
)
GO