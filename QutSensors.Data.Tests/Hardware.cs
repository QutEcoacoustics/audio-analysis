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
			var user = CreateUser();

			var hardware = new Hardware() { UniqueID = "TEST_HARDWARE", CreatedTime = DateTime.UtcNow, LastContacted = DateTime.Now, CreatedBy = user.UserName };
			db.Hardware.InsertOnSubmit(hardware);
			db.SubmitChanges();
		}
	}
}