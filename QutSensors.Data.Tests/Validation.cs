using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QutSensors.Data.Tests
{
	/// <summary>
	/// Summary description for Validation
	/// </summary>
	[TestClass]
	public class ValidationTests
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

		[TestMethod, ExpectedException(typeof(MultiException))]
		public void DoesntContainFail()
		{
			Validation.Begin()
						.DoesntContain("Joe", 'e', "TEST")
						.Check();
		}

		[TestMethod]
		public void DoesntContainPass()
		{
			Validation.Begin()
						.DoesntContain("Joe", 'c', "TEST")
						.Check();
		}
	}
}
