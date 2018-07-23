// <copyright file="LoggingTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared.LoggingTests
{
    using System;
    using System.IO;
    using System.Reflection;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Logging;
    using Fasterflect;
    using log4net;
    using log4net.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            lock (Logging.MemoryAppender)
            {
                // clear the log
                Logging.MemoryAppender.Clear();

                Level level = typeof(Level).TryGetFieldValue(targetVerbosity, Flags.AllMembers) as Level;
                if (level == null)
                {
                    level = typeof(LogExtensions).TryGetFieldValue(targetVerbosity, Flags.AllMembers) as Level;
                }

                Assert.IsNotNull(level);

                Logging.ModifyVerbosity(level, true);

                Logging.TestLogging();

                Assert.AreEqual(expectedMessageCount, Logging.MemoryAppender.GetEvents().Length);
            }
        }

        [TestMethod]
        public void InitializeCanOnlyBeCalledOnce()
        {
            Assert.ThrowsException<InvalidOperationException>(
                () => Logging.Initialize(false, Level.Info, true));
        }

    }
}
