IF EXISTS (SELECT * FROM sysobjects WHERE type = 'U' AND name = 'PhotoReadings')
	BEGIN
		DROP  Table PhotoReadings
	END
GO

CREATE TABLE PhotoReadings
(
	PhotoReadingID	uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
	SensorID	uniqueidentifier REFERENCES Sensors(SensorID),
	[Time] datetime,
	Data	image
)
GO