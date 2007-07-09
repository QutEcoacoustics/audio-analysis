ALTER TABLE Sensors ADD Longitude varchar(16)
GO
ALTER TABLE Sensors ADD Latitude varchar(16)
GO
ALTER TABLE Sensors ADD MacAddress varchar(17)
GO

ALTER PROCEDURE GetSensor(@sensorID uniqueidentifier) AS

--RETURNS: Tuple[Sensor](Guid? id, string name, string friendlyName, string description, string longitude, string latitude)

SELECT SensorID, [Name], FriendlyName, Description, Longitude, Latitude
FROM Sensors
WHERE SensorID = @sensorID

GO

ALTER PROCEDURE GetSensorByName(@name nvarchar(16)) AS

--RETURNS: Tuple[Sensor](Guid? id, string name, string friendlyName, string description, string longitude, string latitude)

SELECT SensorID, [Name], FriendlyName, Description, Longitude, Latitude
FROM Sensors
WHERE [Name] = @name

GO

ALTER PROCEDURE GetSensors AS

--RETURNS: TupleList[Sensor](Guid? id, string name, string friendlyName, string description, string longitude, string latitude)

SELECT SensorID, [Name], FriendlyName, Description, Longitude, Latitude
FROM Sensors

GO