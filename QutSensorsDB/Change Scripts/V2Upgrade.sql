 /*
  * Add Hardware table
  * Add deployments table
  * Copy sensor info to deployments table
  * Add referential integrity to readings tables
  */
 
CREATE TABLE Hardware
(
	HardwareID	int IDENTITY(1,1) PRIMARY KEY,
	UniqueID nvarchar(128) UNIQUE
)
 
GO
 
CREATE TABLE Deployments
(
	DeploymentID uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
	HardwareID int REFERENCES Hardware(HardwareID) NOT NULL,
	[Name] nvarchar(128) UNIQUE,
	DateStarted datetime NOT NULL DEFAULT GetUtcDate(),
	Description ntext,
	Longitude varchar(16),
	Latitude varchar(16)
)
 
GO

INSERT INTO Hardware (UniqueID)
SELECT DISTINCT SensorID FROM Sensors

GO

INSERT INTO Deployments (HardwareID, DeploymentID, [Name], Description, Longitude, Latitude)
SELECT (SELECT HardwareID FROM Hardware WHERE UniqueID = SensorID), SensorID, FriendlyName, Description, Longitude, Latitude FROM Sensors

GO

EXEC sp_rename 
    @objname = 'AudioReadings.SensorID', 
    @newname = 'DeploymentID', 
    @objtype = 'COLUMN'
GO

EXEC sp_rename 
    @objname = 'PhotoReadings.SensorID', 
    @newname = 'DeploymentID', 
    @objtype = 'COLUMN'
GO

ALTER TABLE AudioReadings
ADD FOREIGN KEY (DeploymentID)
REFERENCES Deployments(DeploymentID)

GO

ALTER TABLE PhotoReadings
ADD FOREIGN KEY (DeploymentID)
REFERENCES Deployments(DeploymentID)

GO
-----

ALTER TABLE SensorStatus
DROP CONSTRAINT FK__SensorSta__Senso__6E01572D

GO

EXEC sp_rename 
    @objname = 'SensorStatus.SensorID', 
    @newname = 'DeploymentID',
    @objtype = 'COLUMN'
GO

ALTER TABLE SensorStatus
ADD FOREIGN KEY (DeploymentID)
REFERENCES Deployments(DeploymentID)

GO