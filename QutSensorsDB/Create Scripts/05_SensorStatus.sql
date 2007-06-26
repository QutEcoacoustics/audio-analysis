IF EXISTS (SELECT * FROM sysobjects WHERE type = 'U' AND name = 'SensorStatus')
	BEGIN
		DROP TABLE SensorStatus
	END
GO

CREATE TABLE SensorStatus
(
	SensorID	uniqueidentifier REFERENCES Sensors(SensorID),
	[Time] DateTime,
	BatteryLevel	tinyint,
	
	CONSTRAINT PK_SensorStatus PRIMARY KEY
	(
		SensorID,
		[Time]
	)
)
GO