using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using System.Transactions;
using QutSensors.Data.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QutSensors.Data.Tests
{
	public class DatabaseTest
	{
		public const string TestUserName = "TEST USER";
		public const string TestHardwareID = "TEST HARDWARE";
		public const string TestDeploymentName = "TEST DEPLOYMENT";

		protected TransactionScope transaction;
		protected QutSensors.Data.Linq.QutSensors db;

		public TestContext TestContext { get; set; }

		#region Additional test attributes
		// Use TestInitialize to run code before running each test 
		[TestInitialize()]
		public void MyTestInitialize()
		{
			// Ideally this would be in a class initialise... but it's a static method so we can't
			// do it via this cool DatabaseTest base class. This works, but slows things down a bit.
			Utilities.InitialiseDB();

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

		protected static MembershipUser CreateUser()
		{
			return Membership.CreateUser(TestUserName, "TEST123", "test@test.com");
		}

		protected Hardware CreateHardware(MembershipUser user)
		{
			var hardware = new Hardware() { UniqueID = TestHardwareID, CreatedTime = DateTime.UtcNow, LastContacted = DateTime.Now, CreatedBy = user.UserName };
			db.Hardware.InsertOnSubmit(hardware);
			db.SubmitChanges();
			return hardware;
		}

		protected Deployments CreateDeployment(MembershipUser user, Hardware hardware)
		{
			return hardware.AddDeployment(db, TestDeploymentName, DateTime.UtcNow, user);
		}

		protected AudioReadings AddAudioReading(Hardware hardware, DateTime time)
		{
			return hardware.AddAudioReading(db, time, new byte[0], "TEST MIMETYPE", true);
		}
	}
}