using RikMigrations;
using QutSensors.Processor.Migrations;

[assembly: Migration(typeof(JobsInitialiser), 1, "QutSensors.Processor")]
[assembly: Migration(typeof(JobItemsInitialiser), 2, "QutSensors.Processor")]
[assembly: Migration(typeof(ResultsInitialiser), 3, "QutSensors.Processor")]