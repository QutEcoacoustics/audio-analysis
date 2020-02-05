// <copyright file="LoggingTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared.LoggingTests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Logging;
    
    using log4net;
    using log4net.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    [TestClass]
    [DoNotParallelize]
    public class LoggingTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
        }

        [DataTestMethod]
        [DataRow(nameof(LogExtensions.PromptLevel), 1)]
        [DataRow(nameof(Level.Fatal), 4)]
        [DataRow(nameof(Level.Error), 7)]
        [DataRow(nameof(Level.Warn), 10)]
        [DataRow(nameof(LogExtensions.SuccessLevel), 13)]
        [DataRow(nameof(Level.Notice), 13)]
        [DataRow(nameof(Level.Info), 16)]
        [DataRow(nameof(Level.Debug), 17)]
        [DataRow(nameof(Level.Trace), 18)]
        [DataRow(nameof(Level.Verbose), 19)]
        [DataRow(nameof(Level.All), 19)]
        public void TestVerbosityModifier(string targetVerbosity, int expectedMessageCount)
        {
            lock (TestSetup.TestLogging.MemoryAppender)
            {
                // clear the log
                TestSetup.TestLogging.MemoryAppender.Clear();

                
                if (!(typeof(Level).GetField(targetVerbosity, BindingFlags.GetField).GetValue(null) is Level level))
                {
                    
                    level = typeof(LogExtensions).GetField(targetVerbosity, BindingFlags.GetField).GetValue(null) as Level;
                }

                Assert.IsNotNull(level);

                TestSetup.TestLogging.ModifyVerbosity(level, true);

                TestSetup.TestLogging.TestLogging();

                Assert.AreEqual(expectedMessageCount, TestSetup.TestLogging.MemoryAppender.GetEvents().Length);
            }
        }

        [TestMethod]
        public void TestLogFileIsCreated()
        {
            var logging = new Logging(false, Level.Info, quietConsole: false);

            var expectedPath = Path.Combine(LoggedConsole.LogFolder, logging.LogFileName);
            Assert.That.PathExists(expectedPath);

            StringAssert.StartsWith(logging.LogFileName, "log_20");
        }

        [TestMethod]
        public async Task TestLogFilesAreCleaned()
        {
            var logDirectory = LoggedConsole.LogFolder;

            // get count
            var files = Directory.GetFiles(logDirectory);
            var oldCount = files.Length;

            // it should allow up to 60 log files, before it cleans 10
            var delta = 60 - oldCount + 1;

            // "do" because we need to create a new log at least once for this test to trigger
            // its cleaning logic
            do
            {
                var log = new Logging(false, Level.Info, quietConsole: false);
                delta--;
            }
            while (delta > 0);

            // wait for delete async task to fire
            await Task.Delay(1.Seconds());

            // get count
            files = Directory.GetFiles(logDirectory);
            Assert.AreEqual(50, files.Length);
        }
    }
}
