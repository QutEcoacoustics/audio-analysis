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
    using System.Threading;
    using System.Threading.Tasks;
    using Acoustics.Shared.Logging;
    using global::AnalysisPrograms;
    using global::AnalysisPrograms.Production.Arguments;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    [TestClass]
    public class LoggedConsoleTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void UsesCleanlayout()
        {
            Logging.MemoryAppender.Clear();
            LoggedConsole.WriteLine("test message");
            Assert.AreEqual("test message", Logging.MemoryAppender.GetEvents()[0].RenderedMessage);
        }

        [TestMethod]
        public void IsInteractive()
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
            LoggedConsole.SuppressInteractive = false;
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

                    inWriter.WriteLine("anthony");
                    streamReader.BaseStream.Position = 0;
                    streamReader.BaseStream.Flush();

                    var task = Task.Run(() => LoggedConsole.Prompt("Enter your name:"));

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
            MainEntry.SetLogVerbosity(LogVerbosity.Info, false);
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

            MainEntry.SetLogVerbosity(LogVerbosity.Warn, true);
        }

        [TestMethod]
        [Timeout(30_000)]
        public void PromptTimesOut()
        {
            LoggedConsole.SuppressInteractive = false;
            Assert.IsTrue(LoggedConsole.IsInteractive);

            using (var reader = new InfiniteTextStream(random: TestHelpers.Random.GetRandom()))
            {
                Console.SetIn(reader);

                Assert.ThrowsException<TimeoutException>(
                    () => LoggedConsole.Prompt("Enter your name:", timeout: TimeSpan.FromMilliseconds(500)),
                    "Timed out waiting for user input to prompt:");
            }
        }
    }
}
