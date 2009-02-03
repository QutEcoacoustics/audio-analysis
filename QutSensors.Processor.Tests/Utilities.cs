using System.Configuration;

namespace QutSensors.Processor.Tests
{
	static class Utilities
	{
		public static void InitialiseDB()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString;
			QutSensors.DB.ConnectionString = connectionString;

			RikMigrations.DbProvider.DefaultConnectionString = connectionString;
			RikMigrations.MigrationManager.UpgradeMax(typeof(QutSensors.DB).Assembly);
		}
	}
}