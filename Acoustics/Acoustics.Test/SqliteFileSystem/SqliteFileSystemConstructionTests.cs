using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acoustics.Test.SqliteFileSystem
{
    using System.Diagnostics;
    using System.IO;
    using global::SqliteFileSystem;
    using Microsoft.Data.Sqlite;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SqLiteFileSystem;
    using TestHelpers;
    using Zio;
    using Zio.FileSystems;

    [TestClass]
    public class SqliteFileSystemConstructionTests
    {

        protected DirectoryInfo outputDirectory;
        private FileInfo testFile;
        private PhysicalFileSystem localFileSystem;

        [TestInitialize]
        public void Setup()
        {
            this.testFile = PathHelper.GetTempFile("sqlite3");
            this.outputDirectory = this.testFile.Directory;
            this.localFileSystem = new PhysicalFileSystem();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Debug.WriteLine("Deleting output directory:" + this.outputDirectory.FullName);

            PathHelper.DeleteTempDir(this.outputDirectory);
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
                Assert.IsFalse(this.testFile.RefreshInfo().Exists);
                Assert.IsFalse(fs.IsReadOnly);
            }
        }

        [TestMethod]
        public void TestConstructorReadOnly()
        {
            Assert.That.ExceptionMatches<SqliteException>(
                () => new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadOnly),
                "unable to open database file");

            Assert.IsFalse(this.testFile.RefreshInfo().Exists);

            using (new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadWriteCreate))
            {
                // touch the db first
            }

            // try again
            using (var fs = new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadOnly))
            {
                Assert.IsTrue(this.testFile.RefreshInfo().Exists);
                Assert.IsTrue(fs.IsReadOnly);
            }
        }

        [TestMethod]
        public void TestConstructorReadWrite()
        {
            Assert.That.ExceptionMatches<SqliteException>(
                () => new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadWrite),
                "unable to open database file");

            Assert.IsFalse(this.testFile.RefreshInfo().Exists);

            using (new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadWriteCreate))
            {
                // touch the db first
            }

            // try again
            using (var fs = new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadWrite))
            {
                Assert.IsTrue(this.testFile.RefreshInfo().Exists);
                Assert.IsFalse(fs.IsReadOnly);
            }
        }

        [TestMethod]
        public void TestConstructorReadWriteCreate()
        {
            using (var fs = new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadWriteCreate))
            {
                Assert.IsTrue(this.testFile.RefreshInfo().Exists);
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
            string hasFileTable = (string)this.DirectQuery("SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'files';");
            string hasMetaTable = (string)this.DirectQuery("SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'meta';");

            Assert.AreEqual("files", hasFileTable);
            Assert.AreEqual("meta", hasMetaTable);

            string versionMatches = (string)this.DirectQuery("SELECT version FROM meta LIMIT 1;");
            Assert.AreEqual(Adapter.SchemaVersion, versionMatches);

            long pageSize = (long)this.DirectQuery("PRAGMA page_size;");
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
            int? affected = (int?)this.DirectQuery(
                "UPDATE meta SET version = '0.5.0';",
                "ReadWrite");


            Assert.AreEqual(affected, null);
            
            Assert.That.ExceptionMatches<SqliteFileSystemException>(
                () => new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadOnly),
                "Schema version 0.5.0 does not match library required version 1.0.0");
        }

        private object DirectQuery(string query, string mode = "ReadOnly")
        {
            using (var connection = new SqliteConnection($"Data source='{this.testFile.FullName}';Mode={mode}"))
            {
                connection.Open();

                using (var command = new SqliteCommand(query, connection))
                {
                    return command.ExecuteScalar();
                }
            }
        }
    }
}
