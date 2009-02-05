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
			// Create user
			var user = System.Web.Security.Membership.CreateUser(TestUserName, "TEST123", "test@test.com");

			var hardware = new Hardware() { UniqueID = "TEST_HARDWARE", CreatedTime = DateTime.UtcNow, LastContacted = DateTime.Now, CreatedBy = user.UserName };
			db.Hardware.InsertOnSubmit(hardware);
			db.SubmitChanges();

			hardware.AddDeployment(db, "TEST DEPLOYMENT", DateTime.Now, TestUserName);
		}

		[TestMethod]
		public void AddingDeploymentUpdatesReadingsDeploymentIDs()
		{
			// Create user
			var user = System.Web.Security.Membership.CreateUser(TestUserName, "TEST123", "test@test.com");

			var hardware = new Hardware() { UniqueID = "TEST_HARDWARE", CreatedTime = DateTime.UtcNow, LastContacted = DateTime.Now, CreatedBy = user.UserName };
			db.Hardware.InsertOnSubmit(hardware);
			db.SubmitChanges();

			var reading1 = hardware.AddAudioReading(db, DateTime.UtcNow.AddHours(-4), new byte[0], "test", true);
			var reading2 = hardware.AddAudioReading(db, DateTime.UtcNow.AddHours(-2), new byte[0], "test", true);

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
	}
}