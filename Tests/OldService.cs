using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QutSensors.Data.ActiveRecords;

namespace WebsiteTests
{
	[TestClass]
	public class OldServiceTests
	{
		const string TestSensorID = "TestSensor";
		const string TestDeploymentName = "TestDeploymentName";
		OldService.ServiceSoapClient service = new WebsiteTests.OldService.ServiceSoapClient();

		public TestContext TestContext
		{
			get; set;
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
			
		//}

		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }

		[TestInitialize()]
		public void MyTestInitialize()
		{
			Utilities.InitialiseActiveRecord();

			Hardware hardware = EnsureHardwareExists(TestSensorID);
			hardware.AddDeployment(TestDeploymentName);
		}

		[TestCleanup()]
		public void MyTestCleanup()
		{
			Hardware hardware = EnsureHardwareExists(TestSensorID);
			foreach (Deployment d in hardware.Deployments)
				d.Delete();
			foreach (var r in AudioReading.FindAllByProperty("Hardware", hardware))
				r.Delete();
			hardware.Delete();
		}
		#endregion

		[TestMethod]
		public void TestConnection()
		{
			Assert.IsTrue(service.TestConnection());
		}

		[TestMethod]
		public void GetInvalidDeployment()
		{
			Assert.IsNull(service.GetLatestDeployment("InvalidSensorID"));
		}

		[TestMethod]
		public void GetTestDeployment()
		{
			var d = service.GetLatestDeployment(TestSensorID);
			Assert.IsNotNull(d);
			Assert.AreEqual(TestDeploymentName, d.Name);
		}

		[TestMethod]
		public void AddAudioReading()
		{
			var d = service.GetLatestDeployment(TestSensorID);
			Assert.IsNotNull(d);
			Assert.IsTrue(service.AddAudioReading(d.DeploymentID, null, DateTime.Now, new byte[0]));
			Hardware hardware = EnsureHardwareExists(TestSensorID);
			var readings = AudioReading.FindAllByProperty("Hardware", hardware);
			Assert.IsNotNull(readings);
			Assert.AreEqual(1, readings.Length);
		}

		#region Utilities
		static Hardware EnsureHardwareExists(string sensorID)
		{
			Hardware hardware = Hardware.GetByID(sensorID);
			if (hardware == null)
			{
				hardware = new Hardware();
				hardware.UniqueID = sensorID;
				hardware.Create();
			}
			return hardware;
		}
		#endregion
	}
}