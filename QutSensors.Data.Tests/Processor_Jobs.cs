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

namespace QutSensors.Data.Tests
{
	[TestClass]
	public class Processor_Jobs : DatabaseTest
	{
		[TestMethod]
		public void CreateJob()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);
			var deployment = CreateDeployment(user, hardware);
			AddAudioReading(hardware, DateTime.Now);

			// Create template
			var template = JobManager.Instance.AddTemplate(new DummyTemplateParameters(), "TEST TEMPLATE", "This is a template used for testing.");

			// Create job
			var filter = new ReadingsFilter() { FromDate = DateTime.UtcNow.AddHours(-1) };
			var job = JobManager.Instance.Add(db, filter, "TEST JOB", user.UserName, template);
		}

		[TestMethod]
		public void CreateJobItems()
		{
			CreateJob();
			var job = db.Processor_Jobs.First();

			Assert.AreEqual(job.Filter.GetAudioReadings(db).Count(), db.Processor_JobItems.Where(i => i.JobID == job.JobID).Count());
			var recordsAffected = job.CreateJobItems(db);
			Assert.AreEqual(0, recordsAffected);
			Assert.AreEqual(job.Filter.GetAudioReadings(db).Count(), db.Processor_JobItems.Where(i => i.JobID == job.JobID).Count());
		}

		[TestMethod]
		public void GetJobItem()
		{
			CreateJob();
			var item = JobManager.Instance.GetJobItem(db, "TEST WORKER");
			Assert.IsNotNull(item);
			Assert.AreNotEqual(0, db.Processor_JobItems.Count(i => i.Worker != null));
			Assert.AreEqual(1, db.Processor_JobItems.Count(i => i.Worker == "TEST WORKER"));
		}

		[TestMethod]
		public void ReserveJobItem()
		{
			CreateJob();
			var job = db.Processor_Jobs.First();

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
			JobManager.Instance.CompleteJobItem(db, item.JobItemID);
			Assert.AreEqual(JobStatus.Complete, item.Status);
		}

		[TestMethod]
		public void ReturnJobItem()
		{
			ReserveJobItem();
			var item = db.Processor_JobItems.FirstOrDefault();
			JobManager.Instance.ReturnJob(db, item.Worker, item.JobItemID);
			Assert.AreEqual(JobStatus.Ready, item.Status);
			Assert.IsNull(item.Worker);
			Assert.IsNull(item.WorkerAcceptedTimeUTC);
		}

		[TestMethod]
		public void FailJobItems()
		{
			ReserveJobItem();
			Assert.AreEqual(1, db.Processor_JobItems.Where(i => i.Worker == "TEST WORKER" && i.Status == JobStatus.Running).Count());
			JobManager.Instance.FailAnyIncompleteJobs(db, "TEST WORKER");
			db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, db.Processor_JobItems);
			Assert.AreEqual(0, db.Processor_JobItems.Where(i => i.Worker == "TEST WORKER" && i.Status == JobStatus.Running).Count());
		}

		[TestMethod]
		public void GetOwnersJobs()
		{
			CreateJob();
			var job = db.Processor_Jobs.First();

			Assert.AreEqual(1, JobManager.Instance.GetOwnersJobs(db, TestUserName).Count());
			var job2 = JobManager.Instance.GetOwnersJobs(db, TestUserName).First();
			Assert.AreEqual(job.Name, job2.Name);
		}

		[TestMethod]
		public void ReturnJobItemWithError()
		{
			ReserveJobItem();
			var item = db.Processor_JobItems.FirstOrDefault();
			JobManager.Instance.ReturnJobWithError(db, item.Worker, item.JobItemID, "ERROR DETAILS");
			Assert.AreEqual(JobStatus.Error, item.Status);
			Assert.AreEqual(0, JobManager.Instance.GetJobItem(db, "TEST WORKER"));
		}

		[Serializable]
		class DummyTemplateParameters : BaseClassifierParameters
		{
		}
	}
}