IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetSensor')
	BEGIN
		DROP  Procedure  GetSensor
	END

GO

CREATE Procedure GetSensor(@sensorID uniqueidentifier) AS

--RETURNS: Tuple[Sensor](Guid? id, string name, string friendlyName, string description)

SELECT SensorID, [Name], FriendlyName, Description
FROM Sensors
WHERE SensorID = @sensorID

GO

/*
GRANT EXEC ON Stored_Procedure_Name TO PUBLIC

*/

