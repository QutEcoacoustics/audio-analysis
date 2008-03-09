using QutSensors.Data.ActiveRecords;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace WebsiteTests
{
	[TestClass()]
	public class AudioReadingTest
	{
		public TestContext TestContext { get; set; }

		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		[TestInitialize()]
		public void MyTestInitialize()
		{
			Utilities.InitialiseActiveRecord();
		}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion

		[TestMethod()]
		public void GetReadingsSqlTest()
		{
			var actual = AudioReading.GetReadingsSql((string[])null, ReadStatus.Both, Guid.Empty, null, null, null, 0, 20);
			Assert.IsNotNull(actual);
		}

		[TestMethod()]
		public void GetReadingsWithSensorTag()
		{
			var actual = AudioReading.GetReadingsSql(new string[] {"Active"}, ReadStatus.Both, Guid.Empty, null, null, null, 0, 20);
			Assert.IsNotNull(actual);
		}

		[TestMethod()]
		public void GetReadingsWithReadRestriction()
		{
			var rr = ReadReading.FindFirst();
			Assert.IsNotNull(rr, "This test requires a reading to have been read on the database. You must set this up manually");

			var actual = AudioReading.GetReadingsSql((string[])null, ReadStatus.Read, rr.AspNetProfilesUserId, null, null, null, 0, 20);
			Assert.IsNotNull(actual);
			Assert.IsTrue(actual.Count > 0);
		}

		[TestMethod()]
		public void GetReadingsWithMinDateRestriction()
		{
			var actual = AudioReading.GetReadingsSql((string[])null, ReadStatus.Both, Guid.Empty, System.Data.SqlTypes.SqlDateTime.MinValue.Value, null, null, 0, 20);
			Assert.IsNotNull(actual);
			Assert.IsTrue(actual.Count > 0, "This test requires there to be some readings in the database. You must set this up manually.");
		}

		[TestMethod()]
		public void GetReadingsWithMaxDateRestriction()
		{
			var actual = AudioReading.GetReadingsSql((string[])null, ReadStatus.Both, Guid.Empty, null, System.Data.SqlTypes.SqlDateTime.MaxValue.Value, null, 0, 20);
			Assert.IsNotNull(actual);
			Assert.IsTrue(actual.Count > 0, "This test requires there to be some readings in the database. You must set this up manually.");
		}

		[TestMethod()]
		public void GetReadingsWithAudioTags()
		{
			var actual = AudioReading.GetReadingsSql((string[])null, ReadStatus.Both, Guid.Empty, null, null, new string[] { "Bird" }, 0, 20);
			Assert.IsNotNull(actual);
		}
	}
}
