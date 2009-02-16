using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Transactions;
using QutSensors.Data.Linq;
using Moq;

namespace QutSensors.Data.Tests
{
	[TestClass]
	public class AudioReadingsTests : DatabaseTest
	{
		[TestMethod]
		public void AddAudioReading()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);
			var deployment = CreateDeployment(user, hardware);

			// Create some audio readings
			hardware.AddAudioReading(db, DateTime.UtcNow, new byte[0], "test", true);
		}

		[TestMethod]
		public void MarkAsRead()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);
			var deployment = CreateDeployment(user, hardware);

			var notReadFilter = new ReadingsFilter() { IsRead = false };
			var readFilter = new ReadingsFilter() { IsRead = true };
			Assert.AreEqual(0, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, notReadFilter, TestUserName));
			Assert.AreEqual(0, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, readFilter, TestUserName));

			var reading = AddAudioReading(hardware, DateTime.Now);
			Assert.AreEqual(1, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, notReadFilter, TestUserName));
			Assert.AreEqual(0, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, readFilter, TestUserName));

			reading.MarkAsRead(db, TestUserName);
			Assert.AreEqual(0, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, notReadFilter, TestUserName));
			Assert.AreEqual(1, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, readFilter, TestUserName));
		}

		[TestMethod]
		public void MarkAsRead2()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);
			var deployment = CreateDeployment(user, hardware);

			var notReadFilter = new ReadingsFilter() { IsRead = false };
			var readFilter = new ReadingsFilter() { IsRead = true };
			Assert.AreEqual(0, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, notReadFilter, TestUserName));
			Assert.AreEqual(0, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, readFilter, TestUserName));

			var reading1 = AddAudioReading(hardware, DateTime.Now);
			var reading2 = AddAudioReading(hardware, DateTime.Now);
			Assert.AreEqual(2, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, notReadFilter, TestUserName));
			Assert.AreEqual(0, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, readFilter, TestUserName));

			reading1.MarkAsRead(db, TestUserName);
			Assert.AreEqual(1, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, notReadFilter, TestUserName));
			Assert.AreEqual(1, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, readFilter, TestUserName));

			reading2.MarkAsRead(db, TestUserName);
			Assert.AreEqual(0, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, notReadFilter, TestUserName));
			Assert.AreEqual(2, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, readFilter, TestUserName));
		}

		[TestMethod]
		public void MarkAsReadRepeated()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);
			var deployment = CreateDeployment(user, hardware);

			var notReadFilter = new ReadingsFilter() { IsRead = false };
			var readFilter = new ReadingsFilter() { IsRead = true };
			Assert.AreEqual(0, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, notReadFilter, TestUserName));
			Assert.AreEqual(0, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, readFilter, TestUserName));

			var reading1 = AddAudioReading(hardware, DateTime.Now);
			var reading2 = AddAudioReading(hardware, DateTime.Now);
			Assert.AreEqual(2, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, notReadFilter, TestUserName));
			Assert.AreEqual(0, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, readFilter, TestUserName));

			reading1.MarkAsRead(db, TestUserName);
			Assert.AreEqual(1, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, notReadFilter, TestUserName));
			Assert.AreEqual(1, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, readFilter, TestUserName));

			reading1.MarkAsRead(db, TestUserName);
			Assert.AreEqual(1, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, notReadFilter, TestUserName));
			Assert.AreEqual(1, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, readFilter, TestUserName));
		}

		[TestMethod]
		public void AddTag()
		{
			AddAudioReading();
			var reading = db.AudioReadings.First();
			reading.AddTag(db, "TEST TAG", 0, 5000, TestUserName);
		}

		[TestMethod]
		public void AddData()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);

			var reading = hardware.AddAudioReading(db, DateTime.Now, new byte[8], "audio/x-wav", false);
			Assert.AreEqual(1, db.AudioReadings.Count());
			Assert.AreEqual(0, db.AudioReadings.Count(r => !r.Uploaded)); // Uploaded is mis-named in DB
			Assert.AreEqual(8, db.AudioReadings.Single().Data.Length);

			reading.AddData(db, new byte[16], 8, false);
			Assert.AreEqual(1, db.AudioReadings.Count());
			Assert.AreEqual(0, db.AudioReadings.Count(r => !r.Uploaded)); // Uploaded is mis-named in DB
			db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, reading);
			Assert.AreEqual(24, db.AudioReadings.Single().Data.Length);

			reading.AddData(db, new byte[32], 24, true);
			Assert.AreEqual(1, db.AudioReadings.Count());
			Assert.AreEqual(1, db.AudioReadings.Count(r => !r.Uploaded)); // Uploaded is mis-named in DB
			db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, reading);
			Assert.AreEqual(56, db.AudioReadings.Single().Data.Length);
		}

		[TestMethod]
		public void AddDataProcessesTemplates()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);

			var jobManager = new Mock<IJobManager>(MockBehavior.Strict);
			JobManager.Instance = jobManager.Object;

			var reading = hardware.AddAudioReading(db, DateTime.Now, new byte[8], "audio/x-wav", false);

			jobManager.Setup(m => m.ProcessReading(It.IsAny<QutSensors.Data.Linq.QutSensors>(), It.IsAny<AudioReadings>()));
			reading.AddData(db, new byte[8], 8, true);

			jobManager.VerifyAll();
		}

		[TestMethod]
		public void AddAudioReadingProcessesTemplates()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);

			var jobManager = new Mock<IJobManager>(MockBehavior.Strict);
			JobManager.Instance = jobManager.Object;

			jobManager.Setup(m => m.ProcessReading(It.IsAny<QutSensors.Data.Linq.QutSensors>(), It.IsAny<AudioReadings>()));
			var reading = hardware.AddAudioReading(db, DateTime.Now, new byte[8], "audio/x-wav", true);

			jobManager.VerifyAll();
		}
	}
}