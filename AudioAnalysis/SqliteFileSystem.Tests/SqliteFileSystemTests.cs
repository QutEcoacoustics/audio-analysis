namespace SqliteFileSystem.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Helpers;
    using Microsoft.Data.Sqlite;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Zio;
    using Zio.FileSystems;
    using Zio.FileSystems.Community.SqliteFileSystem;

    [TestClass]
    public class SqliteFileSystemTests
    {

        protected DirectoryInfo outputDirectory;
        private FileInfo testDatabase;
        private PhysicalFileSystem localFileSystem;
        private SqliteFileSystem fs;
        private readonly Random random;
        private (UPath Path, byte[] Data) testData;

        public SqliteFileSystemTests()
        {
            this.random = TestHelper.GetRandom();

        }

        [TestInitialize]
        public void Setup()
        {
            this.testDatabase = TestHelper.GetTempFile("sqlite3");
            this.outputDirectory = this.testDatabase.Directory;
            this.localFileSystem = new PhysicalFileSystem();
            this.fs = new SqliteFileSystem(this.testDatabase.FullName, SqliteOpenMode.ReadWriteCreate);


            this.testData = TestHelper.GenerateTestData(this.random);
        }

        [TestCleanup]
        public void Cleanup()
         {
            this.localFileSystem.Dispose();
            this.fs.Dispose();

            Debug.WriteLine("Deleting output directory:" + this.outputDirectory.FullName);

             TestHelper.DeleteTempDir(this.outputDirectory);
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
            var path = UPath.Root / TestHelper.GetRandomName();
            var exists = this.fs.FileExists(path);

            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void TestFileExists()
        {
            TestHelper.InsertBlobManually(this.fs.Connection, this.testData);
            
            var path = this.testData.Path;
            var exists = this.fs.FileExists(path);

            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void GetAttributesThrowsForNotExists()
        {
            var path = UPath.Root / TestHelper.GetRandomName();

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
            var path = UPath.Root / TestHelper.GetRandomName();

            Assert.AreEqual(FileSystem.DefaultFileTime, this.fs.GetCreationTime(path));
            Assert.AreEqual(FileSystem.DefaultFileTime, this.fs.GetLastWriteTime(path));
            Assert.AreEqual(FileSystem.DefaultFileTime, this.fs.GetLastAccessTime(path));
        }

        [DataTestMethod]
        [DataRow(FileMode.Open, true, typeof(IOException))]
        [DataRow(FileMode.Append, false, typeof(ArgumentException))]
        [DataRow(FileMode.Create, false, typeof(IOException))]
        [DataRow(FileMode.CreateNew, false, typeof(IOException))]
        [DataRow(FileMode.OpenOrCreate, false, typeof(IOException))]
        [DataRow(FileMode.Truncate, false, typeof(IOException))]
        public void TestFileOpenReadOnly(FileMode openMode, bool shouldSucceed, Type excpectedExceptionType)
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
                var assertionMethod = typeof(Assert).GetMethod(nameof(Assert.ThrowsException), new []{typeof(Action)});

                var genericAssert = assertionMethod.MakeGenericMethod(excpectedExceptionType);

                void Assertion() => readonlyFs.OpenFile(this.testData.Path, openMode, FileAccess.ReadWrite);

                genericAssert.Invoke(Assert.That, new object[] { (Action)Assertion });
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
            var copyPath = UPath.Root / TestHelper.GetRandomName();
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
            var movePath = UPath.Root / TestHelper.GetRandomName();
            this.fs.MoveFile(copyPath, movePath);
            Assert.IsTrue(this.fs.FileExists(movePath));
            Assert.IsFalse(this.fs.FileExists(copyPath));
            Assert.IsTrue(this.fs.FileExists(path));
            CollectionAssert.AreEqual(actualBytes, this.fs.ReadAllBytes(movePath));

            // move throws when destination exists
            Assert.ThrowsException<IOException>(() => this.fs.MoveFile(path, movePath));

            // test enumerate files
            var allFiles = this.fs.EnumeratePaths(UPath.Root);
            CollectionAssert.AreEquivalent(new[] { path, movePath}, allFiles.ToList());

            var allDirs = this.fs.EnumerateDirectories(UPath.Root).ToList();
            Assert.AreEqual(0, allDirs.Count);

            // check replace file
            var replacedFile = TestHelper.GenerateTestData(this.random);
            this.fs.WriteAllBytes(replacedFile.Path, replacedFile.Data);
            var replaceLastAccessTime = this.fs.GetLastAccessTime(replacedFile.Path);
            var replaceCreationTime = this.fs.GetCreationTime(replacedFile.Path);
            var replaceLastWriteTime = this.fs.GetLastWriteTime(replacedFile.Path);
            var moveLastAccessTime = this.fs.GetLastAccessTime(movePath);
            var moveCreationTime = this.fs.GetCreationTime(movePath);
            var moveLastWriteTime = this.fs.GetLastWriteTime(movePath);

            this.fs.ReplaceFile(replacedFile.Path, movePath, movePath + ".bak", true);

            Assert.IsTrue(this.fs.FileExists(movePath));
            Assert.IsTrue(this.fs.FileExists(movePath + ".bak"));
            Assert.IsFalse(this.fs.FileExists(replacedFile.Path));

            TestHelper.AssertBlobMetadata(
                this.fs.Connection,
                1024,
                replaceLastAccessTime.ToUniversalTime(),
                moveCreationTime.ToUniversalTime(),
                replaceLastWriteTime.ToUniversalTime(),
                movePath.FullName,
                TimeSpan.Zero);

            TestHelper.AssertBlobMetadata(
                this.fs.Connection,
                1024,
                moveLastAccessTime.ToUniversalTime(),
                moveCreationTime.ToUniversalTime(),
                moveLastWriteTime.ToUniversalTime(),
                (movePath + ".bak"),
                TimeSpan.Zero);

            CollectionAssert.AreEqual(replacedFile.Data, this.fs.ReadAllBytes(movePath));
            CollectionAssert.AreEqual(actualBytes, this.fs.ReadAllBytes(movePath + ".bak"));

            // modify timestamps
            var now = DateTime.Now;
            this.fs.SetLastAccessTime(path, now);
            this.fs.SetCreationTime(path, now.AddSeconds(1.0));
            this.fs.SetLastWriteTime(path, now.AddSeconds(2.0));

            lastAccessTime = this.fs.GetLastAccessTime(path);
            creationTime = this.fs.GetCreationTime(path);
            lastWriteTime = this.fs.GetLastWriteTime(path);

            Assert.That.AreClose(lastAccessTime, now, TimeSpan.Zero);
            Assert.That.AreClose(creationTime, now.AddSeconds(1.0), TimeSpan.Zero);
            Assert.That.AreClose(lastWriteTime, now.AddSeconds(2.0), TimeSpan.Zero);

            // check set attributes has no effect (this file system does not encode attributes)
            this.fs.SetAttributes(path, FileAttributes.ReadOnly);
            Assert.AreEqual(FileAttributes.Normal, this.fs.GetAttributes(path));

            // check deleting a file
            Assert.IsTrue(this.fs.FileExists(path));
            this.fs.DeleteFile(path);
            Assert.IsFalse(this.fs.FileExists(path));
            Assert.IsTrue(this.fs.FileExists(movePath));
        }

        [TestMethod]
        public void TestEnumeratePaths()
        {
            this.fs.WriteAllText("/a/b/c/d/e/test.txt", "Hello");
            this.fs.WriteAllText("/a/b/c/d/e/hello.txt", "World");
            this.fs.WriteAllText("/a/b/c/box.txt", "I'm Mr. Meeseeks");
            this.fs.WriteAllText("/a/b/c/box2.txt", "Look at me");
            this.fs.WriteAllText("/a/b/box3.txt", "Existence is pain");
            this.fs.WriteAllText("/nothing.txt", "When you know nothing matters, the universe is yours");

            var paths = this.fs.EnumeratePaths("/a/b", "*", SearchOption.AllDirectories, SearchTarget.Both).ToList();
            CollectionAssert.AreEquivalent(new UPath[]
            {
                "/a/b",
                "/a/b/c",
                "/a/b/c/d",
                "/a/b/c/d/e",
                "/a/b/c/d/e/test.txt",
                "/a/b/c/d/e/hello.txt",
                "/a/b/c/box.txt",
                "/a/b/c/box2.txt",
                "/a/b/box3.txt",
            }, paths);
        }

        [TestMethod]
        public void TestDirectory()
        {
            // Note: some of the following tests may seem non-sensical because directories do not "exist"
            // in this file system. As such mehthods like `CreateDirectory` are usually no-ops.

            Assert.IsTrue(this.fs.DirectoryExists("/"));

            // Test CreateDirectory
            this.fs.CreateDirectory("/test");
            Assert.IsFalse(this.fs.DirectoryExists("/test"));
            this.fs.WriteAllText("/test/text.txt", "Hello");
            Assert.IsTrue(this.fs.DirectoryExists("/test"));
            Assert.IsFalse(this.fs.DirectoryExists("/test2"));

            // Test CreateDirectory (sub folders)
            this.fs.WriteAllText("/test/test1/test2/test3/test.txt", "hello world");
            Assert.IsTrue(this.fs.DirectoryExists("/test/test1/test2/test3"));
            Assert.IsTrue(this.fs.DirectoryExists("/test/test1/test2"));
            Assert.IsTrue(this.fs.DirectoryExists("/test/test1"));
            Assert.IsTrue(this.fs.DirectoryExists("/test"));

            // Test DeleteDirectory
            this.fs.DeleteDirectory("/test/test1/test2/test3", true);
            Assert.IsFalse(this.fs.DirectoryExists("/test/test1/test2/test3"));
            // parent nodes deleted because there are no files in them
            Assert.IsFalse(this.fs.DirectoryExists("/test/test1/test2"));
            Assert.IsFalse(this.fs.DirectoryExists("/test/test1"));
            Assert.IsTrue(this.fs.DirectoryExists("/test"));

            // Test MoveDirectory
            this.fs.WriteAllText("/test/test1/test2/world.txt", "world hello");
            this.fs.MoveDirectory("/test", "/test2");
            Assert.IsTrue(this.fs.DirectoryExists("/test2/test1/test2"));
            Assert.IsTrue(this.fs.DirectoryExists("/test2/test1"));
            Assert.IsTrue(this.fs.DirectoryExists("/test2"));
            Assert.IsTrue(this.fs.FileExists("/test2/text.txt"));
            Assert.IsTrue(this.fs.FileExists("/test2/test1/test2/world.txt"));

            // Test MoveDirectory to sub directory
            this.fs.MoveDirectory("/test2", "/testsub/testx");
            Assert.IsFalse(this.fs.DirectoryExists("/test2"));
            Assert.IsTrue(this.fs.DirectoryExists("/testsub/testx/test1/test2"));
            Assert.IsTrue(this.fs.DirectoryExists("/testsub/testx/test1"));
            Assert.IsTrue(this.fs.DirectoryExists("/testsub/testx"));

            // Test DeleteDirectory - recursive
            this.fs.DeleteDirectory("/testsub", true);
            Assert.IsFalse(this.fs.DirectoryExists("/testsub/testx/test1/test2"));
            Assert.IsFalse(this.fs.DirectoryExists("/testsub/testx/test1"));
            Assert.IsFalse(this.fs.DirectoryExists("/testsub/testx"));
            Assert.IsFalse(this.fs.DirectoryExists("/testsub"));
        }

        [TestMethod]
        public void TestDirectoryExceptions()
        {
            Assert.ThrowsException<DirectoryNotFoundException>(() => this.fs.DeleteDirectory("/dir", true));

            Assert.ThrowsException<DirectoryNotFoundException>(() => this.fs.MoveDirectory("/dir1", "/dir2"));

            Assert.ThrowsException<UnauthorizedAccessException>(() => this.fs.CreateDirectory("/"));

            this.fs.WriteAllText("/dir1/txt.txt", "hello");
            Assert.ThrowsException<UnauthorizedAccessException>(() => this.fs.DeleteFile("/dir1"));
            Assert.ThrowsException<IOException>(() => this.fs.MoveDirectory("/dir1", "/dir1"));

            this.fs.WriteAllText("/toto.txt", "test");
            Assert.ThrowsException<IOException>(() => this.fs.CreateDirectory("/toto.txt"));
            Assert.ThrowsException<IOException>(() => this.fs.DeleteDirectory("/toto.txt", true));
            Assert.ThrowsException<IOException>(() => this.fs.MoveDirectory("/toto.txt", "/test"));

            this.fs.WriteAllText("/dir2/txtr.txt", "World");
            Assert.ThrowsException<IOException>(() => this.fs.MoveDirectory("/dir1", "/dir2"));
        }

        [TestMethod]
        public void TestMoveFileDifferentDirectory()
        {
            this.fs.WriteAllText("/toto.txt", "content");

            this.fs.CreateDirectory("/dir");

            this.fs.MoveFile("/toto.txt", "/dir/titi.txt");

            Assert.IsFalse(this.fs.FileExists("/toto.txt"));
            Assert.IsTrue(this.fs.FileExists("/dir/titi.txt"));

            Assert.AreEqual("content", this.fs.ReadAllText("/dir/titi.txt"));
        }

        [TestMethod]
        [Ignore("Don't support locking yet")]
        public void TestDirectoryDeleteAndOpenFile()
        {
            this.fs.CreateDirectory("/dir");
            this.fs.WriteAllText("/dir/toto.txt", "content");
            var stream = this.fs.OpenFile("/dir/toto.txt", FileMode.Open, FileAccess.Read);

            Assert.ThrowsException<IOException>(() => this.fs.DeleteFile("/dir/toto.txt"));
            Assert.ThrowsException<IOException>(() => this.fs.DeleteDirectory("/dir", true));

            stream.Dispose();
            this.fs.SetAttributes("/dir/toto.txt", FileAttributes.ReadOnly);
            Assert.ThrowsException<UnauthorizedAccessException>(() => this.fs.DeleteDirectory("/dir", true));
            this.fs.SetAttributes("/dir/toto.txt", FileAttributes.Normal);
            this.fs.DeleteDirectory("/dir", true);

            var entries = this.fs.EnumeratePaths("/").ToList();
            Assert.AreEqual(0, entries.Count);
        }

        [TestMethod]
        [Ignore("Don't support locking yet")]
        public void TestOpenFileMultipleRead()
        {
            this.fs.WriteAllText("/toto.txt", "content");

            Assert.IsTrue(this.fs.FileExists("/toto.txt"));

            using (var tmp = this.fs.OpenFile("/toto.txt", FileMode.Open, FileAccess.Read))
            {
                Assert.ThrowsException<IOException>(() => this.fs.OpenFile("/toto.txt", FileMode.Open, FileAccess.Read, FileShare.Read));
            }

            var stream1 = this.fs.OpenFile("/toto.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
            var stream2 = this.fs.OpenFile("/toto.txt", FileMode.Open, FileAccess.Read, FileShare.Read);

            stream1.ReadByte();
            Assert.AreEqual<long>(1, stream1.Position);

            stream2.ReadByte();
            stream2.ReadByte();
            Assert.AreEqual<long>(2, stream2.Position);

            stream1.Dispose();
            stream2.Dispose();

            // We try to write back on the same file after closing
            this.fs.WriteAllText("/toto.txt", "content2");
        }

        [TestMethod]
        public void TestOpenFileCreateNewAlreadyExist()
        {
            this.fs.WriteAllText("/toto.txt", "content");

            Assert.ThrowsException<IOException>(() =>
            {
                using (var stream = this.fs.OpenFile("/toto.txt", FileMode.CreateNew, FileAccess.Write))
                {
                }
            });

            Assert.ThrowsException<IOException>(() =>
            {
                using (var stream = this.fs.OpenFile("/toto.txt", FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite))
                {
                }
            });
        }

        [TestMethod]
        public void TestMoveDirectorySubFolderFail()
        {
            this.fs.WriteAllText("/dir/dir1/txt.txt", "Scary Terry");
            
            Assert.ThrowsException<IOException>(() => this.fs.MoveDirectory("/dir", "/dir/dir1/dir2"));
        }

        [TestMethod]
        public void TestReplaceFileSameFileFail()
        {
            this.fs.WriteAllText("/toto.txt", "content");
            Assert.ThrowsException<IOException>(() => this.fs.ReplaceFile("/toto.txt", "/toto.txt", null, true));

            this.fs.WriteAllText("/tata.txt", "content2");

            Assert.ThrowsException<IOException>(() => this.fs.ReplaceFile("/toto.txt", "/tata.txt", "/toto.txt", true));
        }


        [TestMethod]
        public void TestDeleteDirectoryNonEmpty()
        {
            this.fs.WriteAllText("/dir/dir1/txt.txt", "Oh geez Rick");
            Assert.ThrowsException<IOException>(() => this.fs.DeleteDirectory("/dir", false));
        }

        [TestMethod]
        public void TestInvalidCharacter()
        {
            Assert.ThrowsException<NotSupportedException>(() => this.fs.CreateDirectory("/toto/ta:ta"));
        }

        [TestMethod]
        public void TestFileExceptions()
        {
            this.fs.WriteAllText("/dir1/file.txt", "Wubbadubbadubdub");

            Assert.ThrowsException<FileNotFoundException>(() => this.fs.GetFileLength("/toto.txt"));
            Assert.ThrowsException<FileNotFoundException>(() => this.fs.CopyFile("/toto.txt", "/toto.bak.txt", true));
            Assert.ThrowsException<UnauthorizedAccessException>(() => this.fs.CopyFile("/dir1", "/toto.bak.txt", true));
            Assert.ThrowsException<FileNotFoundException>(() => this.fs.MoveFile("/toto.txt", "/titi.txt"));

            // If the file to be deleted does not exist, no exception is thrown.
            this.fs.DeleteFile("/toto.txt");
            Assert.ThrowsException<FileNotFoundException>(() => this.fs.OpenFile("/toto.txt", FileMode.Open, FileAccess.Read));
            Assert.ThrowsException<FileNotFoundException>(() => this.fs.OpenFile("/toto.txt", FileMode.Truncate, FileAccess.Write));

            Assert.ThrowsException<FileNotFoundException>(() => this.fs.GetFileLength("/dir1/toto.txt"));
            Assert.ThrowsException<FileNotFoundException>(() => this.fs.CopyFile("/dir1/toto.txt", "/toto.bak.txt", true));
            Assert.ThrowsException<FileNotFoundException>(() => this.fs.MoveFile("/dir1/toto.txt", "/titi.txt"));

            // If the file to be deleted does not exist, no exception is thrown.
            this.fs.DeleteFile("/dir1/toto.txt");
            Assert.ThrowsException<FileNotFoundException>(() => this.fs.OpenFile("/dir1/toto.txt", FileMode.Open, FileAccess.Read));

            this.fs.WriteAllText("/toto.txt", "yo");
            this.fs.CopyFile("/toto.txt", "/titi.txt", false);
            this.fs.CopyFile("/toto.txt", "/titi.txt", true);

            Assert.ThrowsException<FileNotFoundException>(() => this.fs.GetFileLength("/dir1"));

            var defaultTime = new DateTime(1601, 01, 01, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
            Assert.AreEqual(defaultTime, this.fs.GetCreationTime("/dest"));
            Assert.AreEqual(defaultTime, this.fs.GetLastWriteTime("/dest"));
            Assert.AreEqual(defaultTime, this.fs.GetLastAccessTime("/dest"));

            // We don't support locking
            /*
            using (var stream1 = fs.OpenFile("/toto.txt", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Assert.ThrowsException<IOException>(() =>
                {
                    using (var stream2 = fs.OpenFile("/toto.txt", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                    }
                });
            }
            */

            Assert.ThrowsException<UnauthorizedAccessException>(() => this.fs.OpenFile("/dir1", FileMode.Open, FileAccess.Read));

            // directories are made automatically, these examples should not fail (does fail because file does not exist)
            Assert.ThrowsException<FileNotFoundException>(() => this.fs.OpenFile("/dir/toto.txt", FileMode.Open, FileAccess.Read));

            // directories are made automatically, these examples should not fail
            this.fs.CopyFile("/toto.txt", "/dest/toto.txt", true);

            Assert.ThrowsException<IOException>(() => this.fs.CopyFile("/toto.txt", "/titi.txt", false));

            Assert.ThrowsException<IOException>(() => this.fs.CopyFile("/toto.txt", "/dir1", true));

            // directories are made automatically, these examples should not fail
            this.fs.MoveFile("/toto.txt", "/dest/toto2.txt");
            this.fs.MoveFile("/dest/toto2.txt", "/toto.txt");


            this.fs.WriteAllText("/titi.txt", "yo2");
            Assert.ThrowsException<IOException>(() => this.fs.MoveFile("/toto.txt", "/titi.txt"));

            Assert.ThrowsException<FileNotFoundException>(() => this.fs.ReplaceFile("/1.txt", "/1.txt", default(UPath), true));
            Assert.ThrowsException<FileNotFoundException>(() => this.fs.ReplaceFile("/1.txt", "/2.txt", "/1.txt", true));
            Assert.ThrowsException<FileNotFoundException>(() => this.fs.ReplaceFile("/1.txt", "/2.txt", "/2.txt", true));
            Assert.ThrowsException<FileNotFoundException>(() => this.fs.ReplaceFile("/1.txt", "/2.txt", "/3.txt", true));
            Assert.ThrowsException<FileNotFoundException>(() => this.fs.ReplaceFile("/toto.txt", "/dir/2.txt", "/3.txt", true));
            Assert.ThrowsException<FileNotFoundException>(() => this.fs.ReplaceFile("/toto.txt", "/2.txt", "/3.txt", true));
            Assert.ThrowsException<FileNotFoundException>(() => this.fs.ReplaceFile("/toto.txt", "/2.txt", "/toto.txt", true));
        }
    }
}
