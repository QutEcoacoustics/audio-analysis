using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QutSensors.Data.Linq;

namespace QutSensors.Data.Tests
{
	[TestClass]
	public class HardwareTests : DatabaseTest
	{
		[TestMethod]
		public void AddHardware()
		{
			// Create user
			var user = System.Web.Security.Membership.CreateUser(TestUserName, "TEST123", "test@test.com");

			var hardware = new Hardware() { UniqueID = "TEST_HARDWARE", CreatedTime = DateTime.UtcNow, LastContacted = DateTime.Now, CreatedBy = user.UserName };
			db.Hardware.InsertOnSubmit(hardware);
			db.SubmitChanges();
		}
	}
}