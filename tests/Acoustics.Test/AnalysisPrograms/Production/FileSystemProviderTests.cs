// <copyright file="FileSystemProviderTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Production
{
    using System;
    using System.IO;
    using global::AnalysisPrograms.Production;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;
    using Zio;
    using Zio.FileSystems;
    using Zio.FileSystems.Community.SqliteFileSystem;

    [TestClass]
    public class FileSystemProviderTests : OutputDirectoryTest
    {
        [DataTestMethod]
        [DataRow("")]
        [DataRow(null)]
        public void TestFullFileSystem(string path)
        {
            var fs = FileSystemProvider.DetermineFileSystem(path).Item1;

            Assert.IsInstanceOfType(fs, typeof(PhysicalFileSystem));
        }

        [TestMethod]
        public void TestSubFileSystem()
        {
            var fs = FileSystemProvider.DetermineFileSystem(this.outputDirectory.FullName).Item1;

            Assert.IsInstanceOfType(fs, typeof(SubFileSystem));

            Assert.AreEqual(this.outputDirectory.ToUPath(), ((SubFileSystem)fs).SubPath);
        }

        [TestMethod]
        public void TestSqliteFileSystem()
        {
            var path = this.outputDirectory.FullName + "\\test.sqlite3";
            var fs = FileSystemProvider.DetermineFileSystem(path).Item1;

            Assert.IsInstanceOfType(fs, typeof(SqliteFileSystem));

            StringAssert.Contains(((SqliteFileSystem)fs).ConnectionString, path);
            Assert.IsTrue(File.Exists(path));
        }

        [TestMethod]
        public void TestInvalidFileSystem()
        {
            Assert.ThrowsException<NotSupportedException>(
            () => FileSystemProvider.DetermineFileSystem(this.outputDirectory.FullName + "\\test.zip"));
        }
    }
}
