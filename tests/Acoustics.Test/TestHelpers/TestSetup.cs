// <copyright file="TestSetup.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using Acoustics.Shared.Logging;
    using log4net.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestSetup
    {
        public static Logging TestLogging { get; set; }

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            PathHelper.Initialize(context);
            TestLogging = new Logging(
                enableMemoryLogger: true,
                enableFileLogger: false,
                colorConsole: false,
                defaultLevel: Level.Verbose,
                quietConsole: true);
        }
    }
}
