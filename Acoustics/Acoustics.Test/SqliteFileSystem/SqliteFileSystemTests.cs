using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acoustics.Test.SqliteFileSystem
{
    using System.Diagnostics;
    using System.IO;
    using Microsoft.Data.Sqlite;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;
    using Zio;
    using Zio.FileSystems;
    using Zio.FileSystems.Community;
    using Zio.FileSystems.Community.SqliteFileSystem;

    [TestClass]
    public class SqliteFileSystemTests
    {

        protected DirectoryInfo outputDirectory;
        private FileInfo testDatabase;
        private PhysicalFileSystem localFileSystem;
        private SqliteFileSystem fs;
        private readonly System.Random random;
        private (UPath Path, byte[] Data) testData;

        public SqliteFileSystemTests()
        {
            this.random = Random.GetRandom();

        }

        [TestInitialize]
        public void Setup()
        {
            this.testDatabase = PathHelper.GetTempFile("sqlite3");
            this.outputDirectory = this.testDatabase.Directory;
            this.localFileSystem = new PhysicalFileSystem();
            this.fs = new SqliteFileSystem(this.testDatabase.FullName, SqliteOpenMode.ReadWriteCreate);


            this.testData = GenerateTestData(this.random);
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.localFileSystem.Dispose();
            this.fs.Dispose();

            Debug.WriteLine("Deleting output directory:" + this.outputDirectory.FullName);

            PathHelper.DeleteTempDir(this.outputDirectory);
        }

        private static string GetRandomName()
        {
            return Path.GetRandomFileName();
        }

        public static (UPath Path, byte[] Data) GenerateTestData(System.Random random, string path = null)
        {
            var data = new byte[1024];
            random.NextBytes(data);

            return (path ?? UPath.Root / GetRandomName(), data);
        }

        [TestMethod]
        public void TestGetVersion()
        {
            var version = this.fs.GetSqliteVersion();

            Debug.WriteLine($"Sqlite version obtained: {version}");

            Assert.IsFalse(string.IsNullOrWhiteSpace(version));
        }


        [TestMethod]
        public void TestFileNotExists()
        {
            var path = UPath.Root / GetRandomName();
            var exists = this.fs.FileExists(path);

            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void TestFileExists()
        {
            AdapterTests.InsertBlobManually(this.fs.Connection, this.testData);
            
            var path = this.testData.Path;
            var exists = this.fs.FileExists(path);

            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void GetAttributesThrowsForNotExists()
        {
            var path = UPath.Root / GetRandomName();

            Assert.ThrowsException<FileNotFoundException>(() => this.fs.GetAttributes(path));
        }

        [TestMethod]
        public void GetAttributesReturnsReadOnlyWhenConnectionReadOnly()
        {
            this.fs.WriteAllBytes(this.testData.Path, this.testData.Data);
            

            var readonlyFs = new SqliteFileSystem(this.testDatabase.FullName, SqliteOpenMode.ReadOnly);

            Assert.IsTrue(readonlyFs.FileExists(this.testData.Path));
            Assert.AreEqual(
                FileAttributes.Normal | FileAttributes.ReadOnly,
                readonlyFs.GetAttributes(this.testData.Path));
        }

        [TestMethod]
        public void TestFile()
        {
            var path = this.testData.Path;

            Assert.IsFalse(this.fs.FileExists(path));

            var before = DateTime.Now;
            this.fs.WriteAllBytes(path, this.testData.Data);
            

            Assert.IsTrue(this.fs.FileExists(path));
            Assert.AreEqual(1024, this.fs.GetFileLength(path));

            var attributes = this.fs.GetAttributes(path);
            Assert.AreEqual(FileAttributes.Normal, attributes);

            var creationTime = this.fs.GetCreationTime(path);
            var lastAccessTime = this.fs.GetLastAccessTime(path);
            var getLastWriteTime = this.fs.GetLastWriteTime(path);

            Assert.That.AreClose(creationTime.Ticks     , before.Ticks, 15_000_0);
            Assert.That.AreClose(lastAccessTime.Ticks,    before.Ticks, 15_000_0);
            Assert.That.AreClose(getLastWriteTime.Ticks,  before.Ticks, 15_000_0);

            var actualBytes = this.fs.ReadAllBytes(path);
            CollectionAssert.AreEqual(this.testData.Data, actualBytes);


        }

        [DataTestMethod]
        public void TestDirectory()
        {
            //var attributes = this.fs.GetAttributes(path);
            //Assert.AreEqual(FileAttributes.Directory, attributes);
        }




    }
}
