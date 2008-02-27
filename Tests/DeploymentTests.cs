﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QutSensors.Data.ActiveRecords;

namespace WebsiteTests
{
	[TestClass]
	public class DeploymentTests
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
		public void GetAudioReading()
		{
			var r = AudioReading.FindFirst();
			Assert.IsNotNull(r, "This test requires a deployment with readings in it. You must set this up manually.");
			var r2 = r.Deployment.GetAudioReading(r.Time);
			Assert.AreEqual(r.ID, r2.ID);
		}
	}
}
