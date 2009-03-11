using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using WatiN.Core;

namespace QutSensors.Data.Tests.WatiN
{
	[TestClass]
	public class WatiNTest
	{
		public const int ServerPort = 9782;
		public const string ServerPath = "http://localhost:9782";

		protected Microsoft.VisualStudio.WebHost.Server server;

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
		[TestInitialize()]
		public void MyTestInitialize()
		{
			if (server == null)
				StartWebServer();
			//IE.Settings.MakeNewIeInstanceVisible = true;
			IE.Settings.AutoMoveMousePointerToTopLeft = false;
		}
		
		// Use TestCleanup to run code after each test has run
		[TestCleanup()]
		public void MyTestCleanup()
		{
		}
		#endregion

		protected void StartWebServer()
		{
			var testPath = @"../../../WebFrontend/";
			Assert.IsTrue(Directory.Exists(Path.Combine(Environment.CurrentDirectory, testPath)), "Web frontend not found- " + Path.Combine(Environment.CurrentDirectory, testPath));
		}

		protected void StartWebServer(string path)
		{
			Environment.SetEnvironmentVariable("ConnString", "asdfasdf");
			server = new Microsoft.VisualStudio.WebHost.Server(ServerPort, "/", path, false, true);
			server.Start();
		}
	}
}