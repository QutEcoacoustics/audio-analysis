CREATE TABLE AudioTags
(
	AudioTagID	int IDENTITY(1,1) PRIMARY KEY,
	AudioReadingID	uniqueidentifier NOT NULL REFERENCES AudioReadings(AudioReadingID),
	Tag	nvarchar(64) NOT NULL,
	StartTime	int,
	EndTime	int
)