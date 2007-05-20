IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetSensorByName')
	BEGIN
		DROP  Procedure  GetSensorByName
	END

GO

CREATE Procedure GetSensorByName(@name nvarchar(16)) AS

--RETURNS: Tuple[Sensor](Guid? id, string name, string friendlyName, string description)

SELECT SensorID, [Name], FriendlyName, Description
FROM Sensors
WHERE [Name] = @name

GO

/*
GRANT EXEC ON Stored_Procedure_Name TO PUBLIC

GO
*/

