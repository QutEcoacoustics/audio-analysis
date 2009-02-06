using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Transactions;
using QutSensors.Data.Linq;

namespace QutSensors.Data.Tests
{
	[TestClass]
	public class AudioReadingsTests : DatabaseTest
	{
		[TestMethod]
		public void AddAudioReading()
		{
			Hardware hardware;
			var deployment = CreateTestDeployment(out hardware);
			// Create some audio readings
			hardware.AddAudioReading(db, DateTime.UtcNow, new byte[0], "test", true);
		}

		[TestMethod]
		public void MarkAsRead()
		{
			var notReadFilter = new ReadingsFilter() { IsRead = false };
			var readFilter = new ReadingsFilter() { IsRead = true };
			Assert.AreEqual(0, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, notReadFilter, TestUserName));
			Assert.AreEqual(0, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, readFilter, TestUserName));

			AddAudioReading();
			var reading = db.AudioReadings.First();
			Assert.AreEqual(1, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, notReadFilter, TestUserName));
			Assert.AreEqual(0, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, readFilter, TestUserName));

			reading.MarkAsRead(db, TestUserName);
			Assert.AreEqual(0, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, notReadFilter, TestUserName));
			Assert.AreEqual(1, QutSensors.Data.Linq.AudioReadingsInfo.FindCount(db, readFilter, TestUserName));
		}

		[TestMethod]
		public void AddTag()
		{
			AddAudioReading();
			var reading = db.AudioReadings.First();
			reading.AddTag(db, "TEST TAG", 0, 5000, TestUserName);
		}

		Deployments CreateTestDeployment(out Hardware hardware)
		{
			var user = CreateUser();

			// Create hardware
			hardware = new Hardware() { UniqueID = "TEST_HARDWARE", CreatedTime = DateTime.UtcNow, LastContacted = DateTime.Now, CreatedBy = user.UserName };
			db.Hardware.InsertOnSubmit(hardware);
			db.SubmitChanges();

			// Create deployment
			var deployment = hardware.AddDeployment(db, "TEST DEPLOYMENT", DateTime.UtcNow, user);
			return deployment;
		}
	}
}