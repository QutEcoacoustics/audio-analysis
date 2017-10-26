namespace SqliteFileSystem.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Helpers;
    using Microsoft.Data.Sqlite;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Zio;
    using Zio.FileSystems;
    using Zio.FileSystems.Community;
    using Zio.FileSystems.Community.SqliteFileSystem;

    [TestClass]
    public class SqliteFileSystemConstructionTests
    {

        protected DirectoryInfo outputDirectory;
        private FileInfo testFile;
        private PhysicalFileSystem localFileSystem;

        [TestInitialize]
        public void Setup()
        {
            this.testFile = TestHelper.GetTempFile("sqlite3");
            this.outputDirectory = this.testFile.Directory;
            this.localFileSystem = new PhysicalFileSystem();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Debug.WriteLine("Deleting output directory:" + this.outputDirectory.FullName);

            TestHelper.DeleteTempDir(this.outputDirectory);
        }

        [TestMethod]
        public void TestUPathBehaviour()
        {
            var testPath = (UPath)"/mnt/c/Users/Anthony";

            var actual = this.localFileSystem.ConvertPathToInternal(testPath.ToAbsolute());

            Assert.AreEqual(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), actual);
        }

        [TestMethod]
        public void TestConstructorInMemory()
        {
            using (var fs = new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.Memory))
            {
                this.testFile.Refresh();
                Assert.IsFalse(this.testFile.Exists);
                Assert.IsFalse(fs.IsReadOnly);
            }
        }

        [TestMethod]
        public void TestConstructorReadOnly()
        {
            Assert.ThrowsException<SqliteException>(
                () => new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadOnly),
                "unable to open database file");

            this.testFile.Refresh();
            Assert.IsFalse(this.testFile.Exists);

            using (new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadWriteCreate))
            {
                // touch the db first
            }

            // try again
            using (var fs = new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadOnly))
            {
                this.testFile.Refresh();
                Assert.IsTrue(this.testFile.Exists);
                Assert.IsTrue(fs.IsReadOnly);
            }
        }

        [TestMethod]
        public void TestConstructorReadWrite()
        {
            Assert.ThrowsException<SqliteException>(
                () => new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadWrite),
                "unable to open database file");

            this.testFile.Refresh();
            Assert.IsFalse(this.testFile.Exists);

            using (new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadWriteCreate))
            {
                // touch the db first
            }

            // try again
            using (var fs = new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadWrite))
            {
                this.testFile.Refresh();
                Assert.IsTrue(this.testFile.Exists);
                Assert.IsFalse(fs.IsReadOnly);
            }
        }

        [TestMethod]
        public void TestConstructorReadWriteCreate()
        {
            using (var fs = new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadWriteCreate))
            {
                this.testFile.Refresh();
                Assert.IsTrue(this.testFile.Exists);
                Assert.IsFalse(fs.IsReadOnly);
            }
        }

        [TestMethod]
        public void TestAutomaticSchemaCreation()
        {
            // create a new empty database
            using (var fs = new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadWriteCreate))
            {
                
            }

            // ensure a schema has been created
            string hasFileTable = (string)TestHelper.DirectQuery("SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'files';", this.testFile.FullName);
            string hasMetaTable = (string)TestHelper.DirectQuery("SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'meta';", this.testFile.FullName);

            Assert.AreEqual("files", hasFileTable);
            Assert.AreEqual("meta", hasMetaTable);

            string versionMatches = (string)TestHelper.DirectQuery("SELECT version FROM meta LIMIT 1;", this.testFile.FullName);
            Assert.AreEqual(Adapter.SchemaVersion, versionMatches);

            long pageSize = (long)TestHelper.DirectQuery("PRAGMA page_size;", this.testFile.FullName);
            Assert.AreEqual(Adapter.PageSize, pageSize);

            Debug.WriteLine($"Version: {versionMatches}, Page Size: {pageSize}");
        }


        [TestMethod]
        public void EnsureSchemaMismatchesThrow()
        {
            // create a new empty database
            using (var fs = new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadWriteCreate))
            {

            }

            // ensure a schema has been created
            int? affected = (int?)TestHelper.DirectQuery(
                "UPDATE meta SET version = '0.5.0';",
                this.testFile.FullName,
                "ReadWrite");


            Assert.AreEqual(affected, null);
            
            Assert.ThrowsException<SqliteFileSystemException>(
                () => new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadOnly),
                "Schema version 0.5.0 does not match library required version 1.0.0");
        }

    }
}
