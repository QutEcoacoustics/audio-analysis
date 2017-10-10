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
    using SqLiteFileSystem;
    using TestHelpers;
    using Zio;
    using Zio.FileSystems;

    [TestClass]
    public class SqliteFileSystemTests
    {

        protected DirectoryInfo outputDirectory;
        private FileInfo testFile;
        private PhysicalFileSystem localFileSystem;
        private SqliteFileSystem sqliteFileSystem;
        private readonly System.Random random;
        private (string Name, byte[] Data) testData;
        private readonly UPath root = (UPath)"/";

        public SqliteFileSystemTests()
        {
            this.random = Random.GetRandom();

        }

        [TestInitialize]
        public void Setup()
        {
            this.testFile = PathHelper.GetTempFile("sqlite3");
            this.outputDirectory = this.testFile.Directory;
            this.localFileSystem = new PhysicalFileSystem();
            this.sqliteFileSystem = new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadWriteCreate);


            this.testData = this.GenerateTestData();
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.localFileSystem.Dispose();
            this.sqliteFileSystem.Dispose();

            Debug.WriteLine("Deleting output directory:" + this.outputDirectory.FullName);

            PathHelper.DeleteTempDir(this.outputDirectory);
        }

        private static string GetRandomName()
        {
            return Path.GetRandomFileName();
        }

        private (string Name, byte[] Data) GenerateTestData()
        {
            var data = new byte[1024];
            this.random.NextBytes(data);

            return (GetRandomName(), data);
        }

        [TestMethod]
        public void TestGetVersion()
        {
            var version = this.sqliteFileSystem.GetSqliteVersion();

            Debug.WriteLine($"Sqlite version obtained: {version}");

            Assert.IsFalse(string.IsNullOrWhiteSpace(version));
        }


        [TestMethod]
        public void TestFileNotExists()
        {
            var path = UPath.Root / GetRandomName();
            var exists = this.sqliteFileSystem.FileExists(path);

            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void FileCreation()
        {
            var path = UPath.Root / this.testData.Name;

            var before = DateTime.Now;
            this.sqliteFileSystem.WriteAllBytes(path, this.testData.Data);
            var after = DateTime.Now;

            Assert.IsTrue(this.sqliteFileSystem.FileExists(path));
            Assert.AreEqual(1024, this.sqliteFileSystem.GetFileLength(path));

            var attributes = this.sqliteFileSystem.GetAttributes(path);
            Assert.AreEqual(FileAttributes.Normal, attributes);

            var creationTime = this.sqliteFileSystem.GetCreationTime(path);
            var lastAccessTime = this.sqliteFileSystem.GetCreationTime(path);
            var getLastWriteTime = this.sqliteFileSystem.GetCreationTime(path);

            var timeTaken = after - before;
            
            Assert.IsTrue(creationTime - before < timeTaken );
            Assert.IsTrue(lastAccessTime - before < timeTaken );
            Assert.IsTrue(getLastWriteTime - before < timeTaken );
        }

        [DataTestMethod]
        public void TestOpenFileImpl()
        {
            
        }




    }
}
