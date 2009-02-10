using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QutSensors.Data.Linq;

namespace QutSensors.Data.Tests
{
	[TestClass]
	public class DeploymentsTests : DatabaseTest
	{
		[TestMethod]
		public void AddDeployment()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);
			CreateDeployment(user, hardware);
		}

		[TestMethod]
		public void AddingDeploymentUpdatesReadingsDeploymentIDs()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);

			var reading1 = AddAudioReading(hardware, DateTime.UtcNow.AddHours(-4));
			var reading2 = AddAudioReading(hardware, DateTime.UtcNow.AddHours(-2));

			Assert.AreEqual(0, db.AudioReadings.Count(r => r.DeploymentID != null));
			Assert.AreEqual(2, db.AudioReadings.Count(r => r.DeploymentID == null));
			
			var deployment1 = hardware.AddDeployment(db, "TEST DEPLOYMENT1", DateTime.UtcNow.AddHours(-5), TestUserName);

			Assert.AreEqual(2, db.AudioReadings.Count(r => r.DeploymentID != null));
			Assert.AreEqual(0, db.AudioReadings.Count(r => r.DeploymentID == null));
			Assert.AreEqual(2, db.AudioReadings.Count(r => r.DeploymentID == deployment1.DeploymentID));

			var deployment2 = hardware.AddDeployment(db, "TEST DEPLOYMENT2", DateTime.UtcNow.AddHours(-3), TestUserName);

			Assert.AreEqual(1, db.AudioReadings.Count(r => r.DeploymentID == deployment1.DeploymentID));
			Assert.AreEqual(1, db.AudioReadings.Count(r => r.DeploymentID == deployment2.DeploymentID));
		}

		[TestMethod]
		public void AddingDeploymentUpdatesReadingsDeploymentIDsForUnfinishedUploads()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);

			var reading1 = AddAudioReading(hardware, DateTime.UtcNow.AddHours(-4));
			var reading2 = AddAudioReading(hardware, DateTime.UtcNow.AddHours(-2));
			reading2.Uploaded = true; // Indicates not finished uploading
			db.SubmitChanges();

			Assert.AreEqual(0, db.AudioReadings.Count(r => r.DeploymentID != null));
			Assert.AreEqual(2, db.AudioReadings.Count(r => r.DeploymentID == null));

			var deployment1 = hardware.AddDeployment(db, "TEST DEPLOYMENT1", DateTime.UtcNow.AddHours(-5), TestUserName);

			Assert.AreEqual(2, db.AudioReadings.Count(r => r.DeploymentID != null));
			Assert.AreEqual(0, db.AudioReadings.Count(r => r.DeploymentID == null));
			Assert.AreEqual(2, db.AudioReadings.Count(r => r.DeploymentID == deployment1.DeploymentID));

			var deployment2 = hardware.AddDeployment(db, "TEST DEPLOYMENT2", DateTime.UtcNow.AddHours(-3), TestUserName);

			Assert.AreEqual(1, db.AudioReadings.Count(r => r.DeploymentID == deployment1.DeploymentID));
			Assert.AreEqual(1, db.AudioReadings.Count(r => r.DeploymentID == deployment2.DeploymentID));
		}

		[TestMethod]
		public void DeleteTestReadings()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);
			var deployment = hardware.AddDeployment(db, "TEST DEPLOYMENT1", DateTime.Now, user.UserName);
			deployment.IsTest = true;
			var deployment2 = hardware.AddDeployment(db, "TEST DEPLOYMENT2", DateTime.Now.AddDays(1), user.UserName);
			deployment2.IsTest = true;
			db.SubmitChanges();

			AddAudioReading(hardware, DateTime.Now);
			AddAudioReading(hardware, DateTime.Now);
			AddAudioReading(hardware, DateTime.Now.AddDays(1));
			AddAudioReading(hardware, DateTime.Now.AddDays(1));

			Assert.AreEqual(4, db.AudioReadings.Count());
			Assert.AreEqual(2, db.AudioReadings.Count(r => r.DeploymentID == deployment.DeploymentID));
			Assert.AreEqual(2, db.AudioReadings.Count(r => r.DeploymentID == deployment2.DeploymentID));

			deployment.DeleteAllReadings(db);
			Assert.AreEqual(2, db.AudioReadings.Count());
			Assert.AreEqual(0, db.AudioReadings.Count(r => r.DeploymentID == deployment.DeploymentID));
			Assert.AreEqual(2, db.AudioReadings.Count(r => r.DeploymentID == deployment2.DeploymentID));
		}

		[TestMethod, ExpectedException(typeof(MultiException))]
		public void DeleteOnlyTestReadings()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);
			var deployment = hardware.AddDeployment(db, "TEST DEPLOYMENT1", DateTime.Now, user.UserName);

			AddAudioReading(hardware, DateTime.Now);
			AddAudioReading(hardware, DateTime.Now);

			Assert.AreEqual(2, db.AudioReadings.Count());
			Assert.AreEqual(2, db.AudioReadings.Count(r => r.DeploymentID == deployment.DeploymentID));

			deployment.DeleteAllReadings(db);
		}

		[TestMethod]
		public void ReadingsPerDay()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);
			var deployment = hardware.AddDeployment(db, "TEST DEPLOYMENT1", DateTime.Now.AddDays(-1), user.UserName);

			AddAudioReading(hardware, DateTime.Today);
			AddAudioReading(hardware, DateTime.Today);
			AddAudioReading(hardware, DateTime.Today);
			AddAudioReading(hardware, DateTime.Today);

			AddAudioReading(hardware, DateTime.Today.AddDays(1));
			AddAudioReading(hardware, DateTime.Today.AddDays(1));
			AddAudioReading(hardware, DateTime.Today.AddDays(1));

			AddAudioReading(hardware, DateTime.Today.AddDays(2));
			AddAudioReading(hardware, DateTime.Today.AddDays(2));

			AddAudioReading(hardware, DateTime.Today.AddDays(3));

			var count = deployment.GetCountOfReadingsByDate(db);
			Assert.AreEqual(4, count.Where(c => c.Date == DateTime.Today).Select(c => c.Total).Single());
			Assert.AreEqual(3, count.Where(c => c.Date == DateTime.Today.AddDays(1)).Select(c => c.Total).Single());
			Assert.AreEqual(2, count.Where(c => c.Date == DateTime.Today.AddDays(2)).Select(c => c.Total).Single());
			Assert.AreEqual(1, count.Where(c => c.Date == DateTime.Today.AddDays(3)).Select(c => c.Total).Single());
		}

		[TestMethod]
		public void DateEnded()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);
			var deployment = hardware.AddDeployment(db, "TEST DEPLOYMENT1", DateTime.Now.AddDays(-1), user.UserName);
			Assert.IsNull(deployment.GetDateEnded(db));
			var d2Date = DateTime.Now.AddDays(1);
			var deployment2 = hardware.AddDeployment(db, "TEST DEPLOYMENT2", d2Date, user.UserName);
			Assert.AreEqual(d2Date.ToString(), deployment.GetDateEnded(db).Value.ToString());
		}

		[TestMethod]
		public void GetStatuses()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);
			var deployment = hardware.AddDeployment(db, "TEST DEPLOYMENT1", DateTime.Now.AddDays(-1), user.UserName);
			var deployment2 = hardware.AddDeployment(db, "TEST DEPLOYMENT2", DateTime.Now.AddDays(1), user.UserName);
			var q = deployment.GetStatuses(db);
			Assert.AreEqual(0, deployment.GetStatuses(db).Count());

			
			db.DeviceStatuses.InsertOnSubmit(new DeviceStatuses() { StatusID = Guid.NewGuid(), HardwareID = hardware.HardwareID, Time = DateTime.Now });
			db.SubmitChanges();
			Assert.AreEqual(1, deployment.GetStatuses(db).Count());

			db.DeviceStatuses.InsertOnSubmit(new DeviceStatuses() { StatusID = Guid.NewGuid(), HardwareID = hardware.HardwareID, Time = DateTime.Now.AddDays(-2) });
			db.SubmitChanges();
			Assert.AreEqual(1, deployment.GetStatuses(db).Count());

			db.DeviceStatuses.InsertOnSubmit(new DeviceStatuses() { StatusID = Guid.NewGuid(), HardwareID = hardware.HardwareID, Time = DateTime.Now.AddDays(2) });
			db.SubmitChanges();
			Assert.AreEqual(1, deployment.GetStatuses(db).Count());
		}
	}
}