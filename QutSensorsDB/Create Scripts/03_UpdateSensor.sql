IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateSensor')
	BEGIN
		DROP  Procedure  UpdateSensor
	END

GO

CREATE Procedure UpdateSensor(@sensorID uniqueidentifier, @name nvarchar(16), @friendlyName nvarchar(64), @description ntext) AS

--RETURNS: Scalar(Guid)

IF @sensorID IS NULL
BEGIN
	SET @sensorID = NEWID()
	
	INSERT INTO Sensors(SensorID, [Name], FriendlyName, Description)
	SELECT @sensorID, @name, @friendlyName, @description
END
ELSE
BEGIN
	UPDATE Sensors
	SET [Name] = @name, FriendlyName = @friendlyName, Description = @description
	WHERE SensorID = @sensorID
END

SELECT @sensorID

GO

/*
GRANT EXEC ON Stored_Procedure_Name TO PUBLIC

GO
*/

