using QutSensors.Data.ActiveRecords;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using NHibernate.Expression;

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
			var actual = AudioReading.GetReadingsSql((string[])null, ReadStatus.Both, null, null, null, null, 0, 20);
			Assert.IsNotNull(actual);
		}

		[TestMethod()]
		public void GetReadingsWithSensorTag()
		{
			var actual = AudioReading.GetReadingsSql(new string[] {"Active"}, ReadStatus.Both, null, null, null, null, 0, 20);
			Assert.IsNotNull(actual);
		}

		[TestMethod()]
		public void GetReadingsWithReadRestriction()
		{
			var rr = ReadReading.FindFirst();
			Assert.IsNotNull(rr, "This test requires a reading to have been read on the database. You must set this up manually");

			var actual = AudioReading.GetReadingsSql((string[])null, ReadStatus.Read, rr.UserName, null, null, null, 0, 20);
			Assert.IsNotNull(actual);
			Assert.IsTrue(actual.Count > 0);
		}

		[TestMethod()]
		public void GetReadingsWithUnreadRestriction()
		{
			var rr = ReadReading.FindFirst();
			Assert.IsNotNull(rr, "This test requires a reading to have been read on the database. You must set this up manually");

			var actual = AudioReading.GetReadingsSql((string[])null, ReadStatus.Unread, rr.UserName, null, null, null, 0, 20);
			Assert.IsNotNull(actual);
			//Assert.IsTrue(actual.Count > 0);
			foreach (var reading in actual)
				Assert.AreNotEqual(reading.ID, rr.AudioReading.ID);
		}

		[TestMethod()]
		public void GetReadingsWithBothAnonymous()
		{
			var rr = ReadReading.FindFirst();
			Assert.IsNotNull(rr, "This test requires a reading to have been read on the database. You must set this up manually");

			int anonymousCount = AudioReading.GetReadingsCountSql((string[])null, ReadStatus.Both, null, null, null, null);
			int userCount = AudioReading.GetReadingsCountSql((string[])null, ReadStatus.Both, rr.UserName, null, null, null);
			Assert.AreEqual(anonymousCount, userCount);
		}

		[TestMethod()]
		public void GetReadingsWithReadMultipleUsers()
		{
			// Checks that read readings are filtered per user, not all users have all readings marked as read
			var r1 = ReadReading.FindFirst();
			Assert.IsNotNull(r1, "This test requires a reading to have been read on the database. You must set this up manually");
			var r2 = ReadReading.FindFirst(Expression.Not(Expression.Eq("UserName", r1.UserName)));
			Assert.IsNotNull(r2, "This test requires a different readings to have been read on the database by different users. You must set this up manually");

			var rr1 = AudioReading.GetReadingsSql((string[])null, ReadStatus.Read, r1.UserName, null, null, null, 0, int.MaxValue);
			var rr2 = AudioReading.GetReadingsSql((string[])null, ReadStatus.Read, r2.UserName, null, null, null, 0, int.MaxValue);
			if (rr1.Count == rr2.Count)
			{
				int removedCount = 0;
				foreach (var r in rr1)
					removedCount += rr2.RemoveAll(ar => ar.ID == r.ID);
				Assert.IsTrue(rr2.Count > 0 || removedCount != rr1.Count);
			}
		}

		[TestMethod()]
		public void GetReadingsWithMinDateRestriction()
		{
			var actual = AudioReading.GetReadingsSql((string[])null, ReadStatus.Both, null, System.Data.SqlTypes.SqlDateTime.MinValue.Value, null, null, 0, 20);
			Assert.IsNotNull(actual);
			Assert.IsTrue(actual.Count > 0, "This test requires there to be some readings in the database. You must set this up manually.");
		}

		[TestMethod()]
		public void GetReadingsWithMaxDateRestriction()
		{
			var actual = AudioReading.GetReadingsSql((string[])null, ReadStatus.Both, null, null, System.Data.SqlTypes.SqlDateTime.MaxValue.Value, null, 0, 20);
			Assert.IsNotNull(actual);
			Assert.IsTrue(actual.Count > 0, "This test requires there to be some readings in the database. You must set this up manually.");
		}

		[TestMethod()]
		public void GetReadingsWithAudioTags()
		{
			var actual = AudioReading.GetReadingsSql((string[])null, ReadStatus.Both, null, null, null, new string[] { "Bird" }, 0, 20);
			Assert.IsNotNull(actual);
		}
	}
}
