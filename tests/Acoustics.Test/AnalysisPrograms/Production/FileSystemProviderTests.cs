// <copyright file="FileSystemProviderTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Production
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using Acoustics.Shared.Contracts;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisPrograms.Production;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    //using Zio.FileSystems.Community.SqliteFileSystem;

    [TestClass]
    public class FileSystemProviderTests : OutputDirectoryTest
    {
        [DataTestMethod]
        [DataRow("")]
        [DataRow(null)]
        public void TestFullFileSystem(string path)
        {
            var fs = FileSystemProvider.DetermineFileSystem(path).Item1;

            Assert.IsInstanceOfType(fs, typeof(FileSystem));
        }

        [TestMethod]
        [Ignore("Broken to resolve .NET Core dependencies. https://github.com/QutEcoacoustics/audio-analysis/issues/289")]
        public void TestSqliteFileSystem()
        {
            var path = this.TestOutputDirectory.FullName + "\\test.sqlite3";
            var fs = FileSystemProvider.DetermineFileSystem(path).Item1;

            throw new PlatformNotSupportedException("See https://github.com/QutEcoacoustics/audio-analysis/issues/289");
            /*Assert.IsInstanceOfType(fs, typeof(SqliteFileSystem));

            StringAssert.Contains(((SqliteFileSystem)fs).ConnectionString, path);
            Assert.IsTrue(File.Exists(path));
            */
        }

        [TestMethod]
        public void TestInvalidFileSystem()
        {
            Assert.ThrowsException<NotSupportedException>(
            () => FileSystemProvider.DetermineFileSystem(Path.Join(this.TestOutputDirectory.FullName, "test.zip")));
        }
    }
}
