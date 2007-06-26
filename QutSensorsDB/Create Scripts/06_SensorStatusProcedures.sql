IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'AddSensorStatus')
	BEGIN
		DROP PROCEDURE AddSensorStatus
	END
GO

CREATE PROCEDURE AddSensorStatus(@sensorID uniqueidentifier, @time datetime, @batteryLevel tinyint) AS

INSERT INTO SensorStatus (SensorID, [Time], BatteryLevel)
SELECT @sensorID, @time, @batteryLevel

GO