// <copyright file="LoggedConsoleTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LoggedConsoleTests
    {
        public LoggedConsoleTests()
        {
        }

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
        public void IsNotInteractive()
        {
            Assert.IsTrue(LoggedConsole.IsInteractive);
        }

        [TestMethod]
        public void SuppressInteractive()
        {
            LoggedConsole.SuppressInteractive = true;

            Assert.IsFalse(LoggedConsole.IsInteractive);
        }

        [TestMethod]
        public void PromptInteractive()
        {
            using (var memoryStream = new MemoryStream())
            {
                var inWriter = new StreamWriter(memoryStream);
                using (var streamReader = new StreamReader(memoryStream))
                using (var stringWriter = new StringWriter())
                {
                    inWriter.AutoFlush = true;
                    Console.SetIn(streamReader);

                    Console.SetOut(stringWriter);

                    LoggedConsole.WriteLine("Test");

                    var task = Task.Run(() => LoggedConsole.Prompt("Enter your name:"));

                    inWriter.WriteLine("anthony");
                    streamReader.BaseStream.Position = 0;
                    streamReader.BaseStream.Flush();

                    Assert.IsTrue(task.Wait(TimeSpan.FromSeconds(10)));

                    var actual = stringWriter.ToString();
                    StringAssert.Contains(actual, "Enter your name:");
                    //StringAssert.Contains(actual, "anthony:");

                    Assert.AreEqual("anthony", task.Result);
                }
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public void PromptNonInteractive()
        {
            using (var stringWriter = new StringWriter())
            {
                Console.SetOut(stringWriter);

                LoggedConsole.WriteLine("Test");
                LoggedConsole.SuppressInteractive = true;
                var result = LoggedConsole.Prompt("Enter your name:");

                var actual = stringWriter.ToString();
                StringAssert.Contains(actual, "User prompt \"Enter your name:\" suppressed because session is not interactive");

                Assert.AreEqual(null, result);
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public void PromptTimesout()
        {
            Assert.IsTrue(LoggedConsole.IsInteractive);

            Assert.ThrowsException<TimeoutException>(
                () => LoggedConsole.Prompt("Enter your name:", timeout: TimeSpan.FromSeconds(2.5)),
                "Timed out waiting for user input to prompt:");
        }
    }
}
