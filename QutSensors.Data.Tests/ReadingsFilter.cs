using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QutSensors.Data.Tests
{
	[TestClass]
	public class ReadingsFilterTest : DatabaseTest
	{
		[TestMethod]
		public void FilterRead()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);
			var deployment = CreateDeployment(user, hardware);
			var reading1 = AddAudioReading(hardware, DateTime.Now);
			var reading2 = AddAudioReading(hardware, DateTime.Now);

			Assert.AreEqual(2, db.AudioReadings.Count());

			var nullFilter = new ReadingsFilter();
			var readFilter = new ReadingsFilter() { IsRead = true };
			var unreadFilter = new ReadingsFilter() { IsRead = false };

			Assert.AreEqual(2, nullFilter.GetAudioReadings(db).Count());
			Assert.AreEqual(2, nullFilter.GetAudioReadingsSearch(db, user.UserName).Count());
			Assert.AreEqual(0, readFilter.GetAudioReadingsSearch(db, user.UserName).Count());
			Assert.AreEqual(2, unreadFilter.GetAudioReadingsSearch(db, user.UserName).Count());

			reading1.MarkAsRead(db, user.UserName);
			Assert.AreEqual(1, db.ReadReadings.Count());
			Assert.AreEqual(2, nullFilter.GetAudioReadingsSearch(db, user.UserName).Count());
			Assert.AreEqual(1, readFilter.GetAudioReadingsSearch(db, user.UserName).Count());
			Assert.AreEqual(1, unreadFilter.GetAudioReadingsSearch(db, user.UserName).Count());

			reading2.MarkAsRead(db, user.UserName);
			Assert.AreEqual(2, db.ReadReadings.Count());
			Assert.AreEqual(2, nullFilter.GetAudioReadingsSearch(db, user.UserName).Count());
			Assert.AreEqual(2, readFilter.GetAudioReadingsSearch(db, user.UserName).Count());
			Assert.AreEqual(0, unreadFilter.GetAudioReadingsSearch(db, user.UserName).Count());
		}

		[TestMethod]
		public void FilterFromDate()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);
			var deployment = CreateDeployment(user, hardware);
			var reading1 = AddAudioReading(hardware, DateTime.Now);
			var reading2 = AddAudioReading(hardware, DateTime.Now.AddDays(2));

			Assert.AreEqual(2, db.AudioReadings.Count());

			var nullFilter = new ReadingsFilter();
			var testFilter = new ReadingsFilter() { FromDate = DateTime.Now.AddDays(1) };

			Assert.AreEqual(2, nullFilter.GetAudioReadings(db).Count());
			Assert.AreEqual(1, testFilter.GetAudioReadings(db).Count());
			Assert.AreEqual(reading2.AudioReadingID, testFilter.GetAudioReadings(db).First().AudioReadingID);
		}

		[TestMethod]
		public void FilterToDate()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);
			var deployment = CreateDeployment(user, hardware);
			var reading1 = AddAudioReading(hardware, DateTime.Now.AddDays(2));
			var reading2 = AddAudioReading(hardware, DateTime.Now);

			Assert.AreEqual(2, db.AudioReadings.Count());

			var nullFilter = new ReadingsFilter();
			var testFilter = new ReadingsFilter() { ToDate = DateTime.Now.AddDays(1) };

			Assert.AreEqual(2, nullFilter.GetAudioReadings(db).Count());
			Assert.AreEqual(1, testFilter.GetAudioReadings(db).Count());
			Assert.AreEqual(reading2.AudioReadingID, testFilter.GetAudioReadings(db).First().AudioReadingID);
		}

		[TestMethod]
		public void FilterAudioTag()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);
			var deployment = CreateDeployment(user, hardware);
			var reading1 = AddAudioReading(hardware, DateTime.Now);
			var reading2 = AddAudioReading(hardware, DateTime.Now);
			reading2.AddTag(db, "test", 0, 10000, 4040, 7070, user.UserName, false);

			Assert.AreEqual(2, db.AudioReadings.Count());

			var nullFilter = new ReadingsFilter();
			var testFilter = new ReadingsFilter();
			testFilter.AudioTags.Add("test");

			Assert.AreEqual(2, nullFilter.GetAudioReadings(db).Count());
			Assert.AreEqual(1, testFilter.GetAudioReadings(db).Count());
			Assert.AreEqual(reading2.AudioReadingID, testFilter.GetAudioReadings(db).First().AudioReadingID);
		}

		[TestMethod]
		public void FilterDeploymentNames()
		{
			var user = CreateUser();
			var hardware = CreateHardware(user);
			var deployment1 = CreateDeployment(user, hardware);
			var deployment2 = hardware.AddDeployment(db, "TEST DEPLOYMENT 2", DateTime.Now.AddDays(5), user.UserName);
			var reading1 = AddAudioReading(hardware, DateTime.Now);
			var reading2 = AddAudioReading(hardware, DateTime.Now.AddDays(10));

			Assert.AreEqual(2, db.AudioReadings.Count());

			var nullFilter = new ReadingsFilter();
			var testFilter = new ReadingsFilter() { CommaSeparatedDeploymentNames = "TEST DEPLOYMENT 2" };

			Assert.AreEqual(2, nullFilter.GetAudioReadings(db).Count());
			Assert.AreEqual(1, testFilter.GetAudioReadings(db).Count());
			Assert.AreEqual(reading2.AudioReadingID, testFilter.GetAudioReadings(db).First().AudioReadingID);

			testFilter.DeploymentNames.Add(TestDeploymentName);
			Assert.AreEqual(2, testFilter.GetAudioReadings(db).Count());
		}

		[TestMethod]
		public void Serialization()
		{
			var filter = new ReadingsFilter();

			Assert.IsTrue(FiltersEqual(filter, ReadingsFilter.Load(filter.SaveAsString(false), false)));

			filter.AudioTags.Add("AT1");
			filter.AudioTags.Add("AT2");
			filter.DeploymentNames.Add("DN1");
			filter.DeploymentNames.Add("DN2");
			filter.DeploymentTags.Add("DT1");
			filter.DeploymentTags.Add("DT2");
			filter.FromDate = DateTime.Now.Date;
			filter.IsRead = false;
			filter.TestDeploymentsFilter = true;
			filter.ToDate = DateTime.Now.Date.AddDays(4);

			var serialisedFilter = filter.SaveAsString(false);
			var deserialisedFilter = ReadingsFilter.Load(serialisedFilter, false);
			Assert.IsTrue(FiltersEqual(filter, filter));
			Assert.IsTrue(FiltersEqual(filter, deserialisedFilter));
		}

		bool FiltersEqual(ReadingsFilter a, ReadingsFilter b)
		{
			return a.AudioTags.Count == b.AudioTags.Count && a.AudioTags.All(t => b.AudioTags.Contains(t))
				&& a.DeploymentNames.Count == b.DeploymentNames.Count && a.DeploymentNames.All(t => b.DeploymentNames.Contains(t))
				&& a.DeploymentTags.Count == b.DeploymentTags.Count && a.DeploymentTags.All(t => b.DeploymentTags.Contains(t))
				&& a.FromDate == b.FromDate
				&& a.IsRead == b.IsRead
				&& a.TestDeploymentsFilter == b.TestDeploymentsFilter
				&& a.ToDate == b.ToDate;
		}
	}
}