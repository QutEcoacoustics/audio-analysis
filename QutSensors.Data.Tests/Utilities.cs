using System.Configuration;

namespace QutSensors.Data.Tests
{
	static class Utilities
	{
		public static void InitialiseDB()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString;
			QutSensors.Data.Linq.QutSensors.ConnectionString = connectionString;

			RikMigrations.DbProvider.DefaultConnectionString = connectionString;

            RikMigrations.Providers.MssqlProvider provider = new RikMigrations.Providers.MssqlProvider(null);

			RikMigrations.MigrationManager.UpgradeMax(typeof(QutSensors.Data.Linq.QutSensors).Assembly, GetDbProvider(), "DEFAULT", "QutSensors.Processor");
		}


        public static RikMigrations.DbProvider GetDbProvider()
        {
            return new RikMigrations.Providers.MssqlProvider(null);
        }
	}
}