ALTER TABLE Sensors ADD MACAddress varchar(17)
GO
ALTER TABLE SensorStatus ADD FreeMemory numeric
GO
ALTER TABLE SensorStatus ADD MemoryUsage numeric
GO



ALTER PROCEDURE GetSensor(@sensorID uniqueidentifier) AS

--RETURNS: Tuple[Sensor](Guid? id, string name, string friendlyName, string description, string longitude, string latitude, string MACAddress)

SELECT SensorID, [Name], FriendlyName, Description, Longitude, Latitude, MACAddress
FROM Sensors
WHERE SensorID = @sensorID

GO

ALTER PROCEDURE GetSensorByName(@name nvarchar(16)) AS

--RETURNS: Tuple[Sensor](Guid? id, string name, string friendlyName, string description, string longitude, string latitude)

SELECT SensorID, [Name], FriendlyName, Description, Longitude, Latitude, MACAddress
FROM Sensors
WHERE [Name] = @name

GO

ALTER PROCEDURE GetSensors AS

--RETURNS: TupleList[Sensor](Guid? id, string name, string friendlyName, string description, string longitude, string latitude)

SELECT SensorID, [Name], FriendlyName, Description, Longitude, Latitude, MACAddress
FROM Sensors

GO 

ALTER PROCEDURE AddSensorStatus(@sensorID uniqueidentifier, @time datetime, @batteryLevel tinyint, @freememory numeric, @memoryusage numeric) AS

INSERT INTO SensorStatus (SensorID, [Time], BatteryLevel, FreeMemory, MemoryUsage)
SELECT @sensorID, @time, @batteryLevel, @freememory, @memoryusage

GO