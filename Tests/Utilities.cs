using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace WebsiteTests
{
	static class Utilities
	{
		static bool activeRecordInitialised = false;
		public static void InitialiseActiveRecord()
		{
			if (!activeRecordInitialised)
			{
				string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString;
				QutSensors.DB.ConnectionString = connectionString;

				RikMigrations.DbProvider.DefaultConnectionString = connectionString;
				RikMigrations.MigrationManager.UpgradeMax(typeof(QutSensors.DB).Assembly);

				Castle.ActiveRecord.Framework.Config.InPlaceConfigurationSource source = new Castle.ActiveRecord.Framework.Config.InPlaceConfigurationSource();

				Hashtable properties = new Hashtable();

				properties.Add("hibernate.connection.driver_class", "NHibernate.Driver.SqlClientDriver");
				properties.Add("hibernate.dialect", "NHibernate.Dialect.MsSql2000Dialect");
				properties.Add("hibernate.connection.provider", "NHibernate.Connection.DriverConnectionProvider");
				properties.Add("hibernate.connection.connection_string", connectionString);

				source.Add(typeof(Castle.ActiveRecord.ActiveRecordBase), properties);

				Castle.ActiveRecord.ActiveRecordStarter.Initialize(typeof(QutSensors.DB).Assembly, source);

				activeRecordInitialised = true;
			}
		}
	}
}
