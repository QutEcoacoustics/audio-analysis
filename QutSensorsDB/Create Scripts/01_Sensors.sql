IF EXISTS (SELECT * FROM sysobjects WHERE type = 'U' AND name = 'Sensors')
	BEGIN
		DROP TABLE AudioReadings
		DROP TABLE PhotoReadings
		DROP  Table Sensors
	END
GO

CREATE TABLE Sensors
(
	SensorID	uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
	[Name]	nvarchar(16), /* The ID used by the sensor in its files */
	FriendlyName	nvarchar(64),
	Description	ntext
)
GO

/*
GRANT SELECT ON Table_Name TO PUBLIC

GO
*/
