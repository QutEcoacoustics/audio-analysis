// <copyright file="RuntimesTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    [TestClass]
    public class RuntimesTests
    {
        public RuntimesTests()
        {
        }

        [TestMethod]
        public void TestRequiredDllsCopiedToBuildDir()
        {
            var buildDir = TestHelpers.PathHelper.AnalysisProgramsBuild;

            Assert.That.DirectoryExists(buildDir);

            var runtimeDir = Path.Combine(buildDir, "libruntimes");

            var expected = new string[]
            {
                "linux-arm/native/libe_sqlite3.so",
                "linux-armel/native/libe_sqlite3.so",
                "linux-x64/native/libe_sqlite3.so",
                "linux-x86/native/libe_sqlite3.so",
                "osx-x64/native/libe_sqlite3.dylib",
            };

            foreach (var directory in expected)
            {
                Assert.That.FileExists(Path.Combine(runtimeDir, directory));
            }
        }
    }
}
