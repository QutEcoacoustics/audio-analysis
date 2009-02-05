using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Transactions;

namespace QutSensors.Data.Tests
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

	public class DatabaseTest
	{
		public const string TestUserName = "TEST USER";

		protected TransactionScope transaction;
		protected QutSensors.Data.Linq.QutSensors db;

		public TestContext TestContext { get; set; }

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			Utilities.InitialiseDB();
		}
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		[TestInitialize()]
		public void MyTestInitialize()
		{
			transaction = new TransactionScope();
			db = new QutSensors.Data.Linq.QutSensors();
		}
		//
		// Use TestCleanup to run code after each test has run
		[TestCleanup()]
		public void MyTestCleanup()
		{
			db.Dispose();
			transaction.Dispose();
		}
		#endregion
	}
}