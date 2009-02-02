using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Transactions;
using System.Configuration;
using QutSensors.Data.Linq;
using AudioStuff;
using QutSensors.Data;

namespace QutSensors.Processor.Tests
{
	[TestClass]
	public class Processor_Jobs
	{
		QutSensors.Data.Linq.QutSensors db;
		TransactionScope transaction;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			var connectionString = ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString;
			QutSensors.DB.ConnectionString = connectionString;

			RikMigrations.DbProvider.DefaultConnectionString = connectionString;
			RikMigrations.MigrationManager.UpgradeMax(typeof(QutSensors.DB).Assembly);
		}
		//
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

		[TestMethod]
		public void CreateJobItems()
		{
			var user = CreateAudioReading();

			// Create template
			var template = JobManager.AddTemplate(new DummyTemplateParameters(), "TEST TEMPLATE", "This is a template used for testing.");

			// Create job
			var filter = new ReadingsFilter() { FromDate = DateTime.Today };
			var job = JobManager.Add(db, filter, "TEST JOB", user.UserName, template);

			Assert.AreEqual(job.Filter.GetAudioReadings(db).Count(), db.Processor_JobItems.Where(i => i.JobID == job.JobID).Count());
			var recordsAffected = job.CreateJobItems(db);
			Assert.AreEqual(0, recordsAffected);
			Assert.AreEqual(job.Filter.GetAudioReadings(db).Count(), db.Processor_JobItems.Where(i => i.JobID == job.JobID).Count());
		}

		[TestMethod]
		public void GetJobItem()
		{
			CreateJobItems();
			var item = JobManager.GetJobItem(db, "TEST WORKER", null);
			Assert.IsNotNull(item);
			Assert.AreNotEqual(0, db.Processor_JobItems.Count(i => i.Worker != null));
			Assert.AreEqual(1, db.Processor_JobItems.Count(i => i.Worker == "TEST WORKER"));
		}

		[TestMethod]
		public void ReserveJobItem()
		{
			var user = CreateAudioReading();

			// Create template
			var template = JobManager.AddTemplate(new DummyTemplateParameters(), "TEST TEMPLATE", "This is a template used for testing.");

			// Create job
			var filter = new ReadingsFilter() { FromDate = DateTime.Today };
			var job = JobManager.Add(db, filter, "TEST JOB", user.UserName, template);

			Assert.AreEqual(0, db.Processor_JobItems.Where(i => i.Worker == "TEST WORKER").Count());
			var item = db.Processor_JobItems.Where(i => i.JobID == job.JobID).FirstOrDefault();
			Assert.IsNotNull(item);
			item.Reserve(db, "TEST WORKER");
			Assert.AreEqual(JobStatus.Running, item.Status);
			Assert.AreEqual(1, db.Processor_JobItems.Where(i => i.Worker == "TEST WORKER").Count());
		}

		[TestMethod]
		public void CompleteJobItem()
		{
			ReserveJobItem();
			var item = db.Processor_JobItems.FirstOrDefault();
			JobManager.CompleteJobItem(db, item.JobItemID);
			Assert.AreEqual(JobStatus.Complete, item.Status);
		}

		[TestMethod]
		public void ReturnJobItem()
		{
			ReserveJobItem();
			var item = db.Processor_JobItems.FirstOrDefault();
			JobManager.ReturnJob(db, item.Worker, item.JobItemID);
			Assert.AreEqual(JobStatus.Ready, item.Status);
			Assert.IsNull(item.Worker);
			Assert.IsNull(item.WorkerAcceptedTimeUTC);
		}

		[TestMethod]
		public void FailJobItems()
		{
			ReserveJobItem();
			JobManager.FailAnyIncompleteJobs(db, "TEST WORKER");
			db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, db.Processor_JobItems);
			var c = db.ExecuteQuery<int>("SELECT COUNT(*) FROM Processor_JobItems WHERE Worker = 'TEST WORKER'").First();
			Assert.AreEqual(0, c);
			Assert.AreEqual(0, db.Processor_JobItems.Where(i => i.Worker == "TEST WORKER").Count());
		}

		#region Utilities
		System.Web.Security.MembershipUser CreateAudioReading()
		{
			// Create user
			var user = System.Web.Security.Membership.CreateUser("TEST", "TEST123", "test@test.com");

			// Create hardware
			var hardware = new Hardware() { UniqueID = "TEST_HARDWARE", CreatedTime = DateTime.UtcNow, LastContacted = DateTime.Now/*, CreatedBy = user.UserName*/ };
			db.Hardware.InsertOnSubmit(hardware);
			db.SubmitChanges();

			// Create deployment
			var deployment = hardware.AddDeployment(db, "TEST DEPLOYMENT", DateTime.UtcNow, user);

			// Create some audio readings
			var reading = new QutSensors.Data.Linq.AudioReadings
			{
				AudioReadingID = Guid.NewGuid(),
				Hardware = hardware,
				Deployments = deployment,
				Time = DateTime.Now,
				MimeType = "application\\test",
				Data = new byte[0]
			};
			db.AudioReadings.InsertOnSubmit(reading);
			db.SubmitChanges();
			return user;
		}
		#endregion

		[Serializable]
		class DummyTemplateParameters : BaseClassifierParameters
		{
		}
	}
}
