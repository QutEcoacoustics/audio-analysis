IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetSensors')
	BEGIN
		DROP  Procedure  GetSensors
	END

GO

CREATE Procedure GetSensors AS

--RETURNS: TupleList[Sensor](Guid? id, string name, string friendlyName, string description)

SELECT SensorID, [Name], FriendlyName, Description
FROM Sensors

GO

/*
GRANT EXEC ON Stored_Procedure_Name TO PUBLIC

GO
*/