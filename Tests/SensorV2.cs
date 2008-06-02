using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebsiteTests.NewService;

namespace WebsiteTests
{
	[TestClass]
	public class SensorV2
	{
		public TestContext TestContext { get; set; }

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void CheckUpdatingLastUpdateTimeWorks()
		{
			// NOTE: This isn't really a test. just a quick check that it doesn't throw an exception
			var client = new SensorV2SoapClient();
			client.GetTaskSchedule("testDeviceId");
			client.GetTaskSchedule("testDeviceId");
		}
	}
}
