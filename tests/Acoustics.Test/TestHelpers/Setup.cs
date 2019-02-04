// <copyright file="Setup.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared.Logging;
    using global::AnalysisPrograms;
    using global::AnalysisPrograms.Production.Arguments;
    using log4net.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class Setup
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            Logging.Initialize(
                enableMemoryLogger: true,
                enableFileLogger: false,
                colorConsole: false,
                defaultLevel: Level.Info,
                quietConsole: true);
        }
    }
}
