using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AudioAnalysis;

namespace QutSensors.Data.Tests
{
	[TestClass]
	public class JobManagerTests : DatabaseTest
	{
		[TestMethod]
		public void ProcessReading()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);
			var deployment = CreateDeployment(user, hardware);

			var template = JobManager.Instance.AddTemplate(new DummyTemplateParameters(), "TEST TEMPLATE", "This is a template used for testing.");

			var filter = new ReadingsFilter() {FromDate = DateTime.Today};
			var job = JobManager.Instance.Add(db, filter, "TEST JOB", user.UserName, template);

			Assert.AreEqual(0, job.JobItems.Count());

			var reading1 = AddAudioReading(hardware, DateTime.Now);
			Assert.AreEqual(1, job.JobItems.Count());
			Assert.AreEqual(reading1.AudioReadingID, job.JobItems.First().AudioReadingID);

			var reading2 = AddAudioReading(hardware, DateTime.Now.AddDays(-2));
			Assert.AreEqual(1, job.JobItems.Count());
		}

		[TestMethod]
		public void ProcessReadingMultipleJobs()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);
			var deployment = CreateDeployment(user, hardware);

			var template = JobManager.Instance.AddTemplate(new DummyTemplateParameters(), "TEST TEMPLATE", "This is a template used for testing.");

			var filter1 = new ReadingsFilter() { FromDate = DateTime.Today };
			var job1 = JobManager.Instance.Add(db, filter1, "TEST JOB 1", user.UserName, template);
			var filter2 = new ReadingsFilter() { ToDate = DateTime.Today };
			var job2 = JobManager.Instance.Add(db, filter2, "TEST JOB 2", user.UserName, template);

			Assert.AreEqual(0, job1.JobItems.Count());
			Assert.AreEqual(0, job2.JobItems.Count());

			var reading1 = AddAudioReading(hardware, DateTime.Now.AddDays(1));
			Assert.AreEqual(1, filter1.GetAudioReadings(db).Count());
			Assert.AreEqual(0, filter2.GetAudioReadings(db).Count());
			Assert.AreEqual(1, job1.JobItems.Count());
			Assert.AreEqual(0, job2.JobItems.Count());
			Assert.AreEqual(reading1.AudioReadingID, job1.JobItems.First().AudioReadingID);

			var reading2 = AddAudioReading(hardware, DateTime.Now.AddDays(-2));
			Assert.AreEqual(2, db.AudioReadings.Count());
			Assert.AreEqual(1, filter1.GetAudioReadings(db).Count());
			Assert.AreEqual(1, filter2.GetAudioReadings(db).Count());
			Assert.AreEqual(1, job1.JobItems.Count());
			Assert.AreEqual(1, job2.JobItems.Count());
			Assert.AreEqual(reading2.AudioReadingID, job2.JobItems.First().AudioReadingID);
		}

		[Serializable]
		class DummyTemplateParameters : BaseTemplate
		{
		}
	}
}