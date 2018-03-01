// <copyright file="DotNetVersionTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acoustics.Test.TestHelpers
{
    using global::AnalysisPrograms;
    using global::AnalysisPrograms.Production.Arguments;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class Setup
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            MainEntry.SetLogVerbosity(LogVerbosity.Warn, false);
        }
    }
}
