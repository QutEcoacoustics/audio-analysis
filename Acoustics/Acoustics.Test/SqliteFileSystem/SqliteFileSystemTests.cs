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
        public void GetTimesReturnsDefaultsWhenFilesDoNotExist()
        {
            var path = UPath.Root / GetRandomName();

            Assert.AreEqual(FileSystem.DefaultFileTime, this.fs.GetCreationTime(path));
            Assert.AreEqual(FileSystem.DefaultFileTime, this.fs.GetLastWriteTime(path));
            Assert.AreEqual(FileSystem.DefaultFileTime, this.fs.GetLastAccessTime(path));
        }

        [DataTestMethod]
        [DataRow(FileMode.Open, true)]
        [DataRow(FileMode.Append, false)]
        [DataRow(FileMode.Create, false)]
        [DataRow(FileMode.CreateNew, false)]
        [DataRow(FileMode.OpenOrCreate, false)]
        [DataRow(FileMode.Truncate, false)]
        public void TestFileOpenReadOnly(FileMode openMode, bool shouldSucceed)
        {
            this.fs.WriteAllBytes(this.testData.Path, this.testData.Data);
            var readonlyFs = new SqliteFileSystem(this.testDatabase.FullName, SqliteOpenMode.ReadOnly);

            if (shouldSucceed)
            {
                using (readonlyFs.OpenFile(this.testData.Path, openMode, FileAccess.Read))
                {
                    Assert.IsTrue(true);
                }
            }
            else
            {
                Assert.ThrowsException<IOException>(
                    () => readonlyFs.OpenFile(this.testData.Path, openMode, FileAccess.ReadWrite));
            }
        }

        [TestMethod]
        public void TestFileDeleteCopyMoveReadOnly()
        {
            this.fs.WriteAllBytes(this.testData.Path, this.testData.Data);
            var readonlyFs = new SqliteFileSystem(this.testDatabase.FullName, SqliteOpenMode.ReadOnly);

          Assert.ThrowsException<IOException>(
                    () => readonlyFs.DeleteFile(this.testData.Path));
          Assert.ThrowsException<IOException>(
                    () => readonlyFs.CopyFile(this.testData.Path, "/elsewhere.blob", true));
          Assert.ThrowsException<IOException>(
                    () => readonlyFs.MoveFile(this.testData.Path, "/elsewhere.blob"));
          
        }

        [TestMethod]
        public void TestFile()
        {
            var path = this.testData.Path;

            Assert.IsFalse(this.fs.FileExists(path));

            var before = DateTime.Now;
            this.fs.WriteAllBytes(path, this.testData.Data);
            var after = DateTime.Now;

            Assert.IsTrue(this.fs.FileExists(path));
            Assert.AreEqual(1024, this.fs.GetFileLength(path));

            var attributes = this.fs.GetAttributes(path);
            Assert.AreEqual(FileAttributes.Normal, attributes);

            var lastAccessTime = this.fs.GetLastAccessTime(path);
            var creationTime = this.fs.GetCreationTime(path);
            var lastWriteTime = this.fs.GetLastWriteTime(path);

            Assert.IsTrue(
                lastAccessTime.Kind == DateTimeKind.Local &&
                creationTime.Kind == DateTimeKind.Local &&
                lastWriteTime.Kind == DateTimeKind.Local);

            // when file is opened it is created first, and thus created stamp is closer to our makrer
            Assert.That.AreClose(creationTime, before, after - before);
            Assert.IsTrue(creationTime < lastAccessTime);

            // other two timestamps are set after writing has finished
            Assert.That.AreClose(lastAccessTime, before, after - before);
            Assert.That.AreClose(lastWriteTime, before, after - before);
            Assert.AreEqual(lastWriteTime, lastAccessTime);

            // read the data
            var actualBytes = this.fs.ReadAllBytes(path);
            CollectionAssert.AreEqual(this.testData.Data, actualBytes);

            // copy the file
            var copyPath = UPath.Root / GetRandomName();
            this.fs.CopyFile(path, copyPath, true);

            Assert.IsTrue(this.fs.FileExists(path));
            Assert.IsTrue(this.fs.FileExists(copyPath));
            CollectionAssert.AreEqual(actualBytes, this.fs.ReadAllBytes(copyPath));

            // copy throws when overwrite is false
            Assert.ThrowsException<IOException>(() => this.fs.CopyFile(path, copyPath, false));

            // copied files attributes test
            Assert.AreEqual(1024, this.fs.GetFileLength(copyPath));
            Assert.AreEqual(attributes, this.fs.GetAttributes(copyPath));
            var copyAccessTime = this.fs.GetLastAccessTime(copyPath);
            var copyCreationTime = this.fs.GetCreationTime(copyPath);
            var copyWriteTime = this.fs.GetLastWriteTime(copyPath);
            Assert.IsTrue(lastAccessTime < copyAccessTime);
            Assert.IsTrue(creationTime < copyCreationTime);
            Assert.AreEqual(lastWriteTime, copyWriteTime);

            // Test move file
            var movePath = UPath.Root / GetRandomName();
            this.fs.MoveFile(copyPath, movePath);
            Assert.IsTrue(this.fs.FileExists(movePath));
            Assert.IsFalse(this.fs.FileExists(copyPath));
            Assert.IsTrue(this.fs.FileExists(path));
            CollectionAssert.AreEqual(actualBytes, this.fs.ReadAllBytes(movePath));

            // modify timestamps
            var now = DateTime.Now;
            this.fs.SetLastAccessTime(path, now);
            this.fs.SetCreationTime(path, now + 1.0.Seconds());
            this.fs.SetLastWriteTime(path, now + 2.0.Seconds());

            lastAccessTime = this.fs.GetLastAccessTime(path);
            creationTime = this.fs.GetCreationTime(path);
            lastWriteTime = this.fs.GetLastWriteTime(path);

            Assert.That.AreClose(lastAccessTime, now, 0.Seconds());
            Assert.That.AreClose(creationTime, now + 1.0.Seconds(), 0.Seconds());
            Assert.That.AreClose(lastWriteTime, now + 2.0.Seconds(), 0.Seconds());

            // test enumerate files
            var allFiles = this.fs.EnumeratePaths(UPath.Root);
            CollectionAssert.AreEquivalent(new [] {path, movePath}, allFiles.ToList());

            var allDirs = this.fs.EnumerateDirectories(UPath.Root).ToList();
            Assert.AreEqual(0, allDirs.Count);

            // check replace file
            var replaceFile = GenerateTestData(this.random);
            this.fs.WriteAllBytes(replaceFile.Path, replaceFile.Data);
            this.fs.ReplaceFile(replaceFile.Path, movePath, movePath + ".bak", true);
            Assert.IsTrue(this.fs.FileExists(movePath));
            Assert.IsTrue(this.fs.FileExists(movePath + ".bak"));
            Assert.IsFalse(this.fs.FileExists(replaceFile.Path));
            CollectionAssert.AreEqual(replaceFile.Data, this.fs.ReadAllBytes(movePath));
            CollectionAssert.AreEqual(actualBytes, this.fs.ReadAllBytes(movePath + ".bak"));

            // check set attributes has no effect (this file system does not encode attributes)
            this.fs.SetAttributes(path, FileAttributes.ReadOnly);
            Assert.AreEqual(FileAttributes.Normal, this.fs.GetAttributes(path));

            // check deleting a file
            Assert.IsTrue(this.fs.FileExists(path));
            this.fs.DeleteFile(path);
            Assert.IsFalse(this.fs.FileExists(path));
            Assert.IsTrue(this.fs.FileExists(movePath));
        }

        [DataTestMethod]
        public void TestDirectory()
        {
            //var attributes = this.fs.GetAttributes(path);
            //Assert.AreEqual(FileAttributes.Directory, attributes);
        }




    }
}
