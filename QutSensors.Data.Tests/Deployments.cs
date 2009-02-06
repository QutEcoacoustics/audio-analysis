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
	}
}