namespace SqliteFileSystem.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Helpers;
    using Microsoft.Data.Sqlite;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Zio;
    using Zio.FileSystems.Community.SqliteFileSystem;

    [TestClass]
    public class DatabaseBackedMemoryStreamTests
    {
        private DirectoryInfo outputDirectory;
        private FileInfo testFile;
        private System.Random random;
        private byte[] sampleBlob;

        [TestInitialize]
        public void Setup()
        {
            this.testFile = TestHelper.GetTempFile("sqlite3");
            this.outputDirectory = this.testFile.Directory;
            this.random = TestHelper.GetRandom();
            this.sampleBlob = new byte[1024];

            this.random.NextBytes(this.sampleBlob);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Debug.WriteLine("Deleting output directory:" + this.outputDirectory.FullName);

            TestHelper.DeleteTempDir(this.outputDirectory);
        }

        [TestMethod]
        public void TestDatabaseBackedMemoryStream()
        {
            var prepared = TestHelper.PrepareDatabaseAndBlob(this.testFile.FullName, this.random);
            using (var connection = new SqliteConnection(prepared.ConnectionString))
            {
                connection.Open();
                TestHelper.AssertBlobMetadata(connection, 1024, 0, 0, 0);
                var now = Date.Now;
                using (var stream = new DatabaseBackedMemoryStream(connection, UPath.Root / "test.blob", true, true))
                {
                    TestHelper.AssertBlobMetadata(connection, 1024, now, 0, 0);

                    Assert.IsTrue(stream.CanRead);
                    Assert.IsTrue(stream.CanSeek);
                    Assert.IsTrue(stream.CanWrite);
                    Assert.AreEqual(0, stream.Position);
                    Assert.AreEqual(1024, stream.Length);

                    var actualBlob = new byte[stream.Length];
                    var read = stream.Read(actualBlob, 0, 1024);

                    Assert.AreEqual(1024, read);
                    CollectionAssert.AreEqual(prepared.blobData, actualBlob);
                }

                // the meta data should not have been updated because no changes were made
                TestHelper.AssertBlobMetadata(connection, 1024, now, 0, 0);
            }
        }


        [TestMethod]
        public void TestDatabaseBackedMemoryStreamWriting()
        {
            var prepared = TestHelper.PrepareDatabaseAndBlob(this.testFile.FullName, this.random);

            var additionalBytes = new byte[512];
            this.random.NextBytes(additionalBytes);

            using (var connection = new SqliteConnection(prepared.ConnectionString))
            {
                connection.Open();
                using (var stream = new DatabaseBackedMemoryStream(connection, UPath.Root / "test.blob", true, true))
                {
                    // we're going to append extra data onto the stream
                    stream.Seek(0, SeekOrigin.End);
                    Assert.AreEqual(1024, stream.Position);
                    Assert.AreEqual(1024, stream.Length);

                    stream.Write(additionalBytes, 0, 256);

                    Assert.AreEqual(1024 + 256, stream.Position);
                    Assert.AreEqual(1024 + 256, stream.Length);

                    // no changes should have happened to the database
                    AssertDatabaseBlobLength(connection, 1024);

                    // when changes are flushed we expect an update
                    stream.Flush();

                    Assert.AreEqual(1024 + 256, stream.Position);
                    Assert.AreEqual(1024 + 256, stream.Length);

                    AssertDatabaseBlobLength(connection, 1024 + 256);

                    // write another set of bytes
                    stream.Write(additionalBytes, 256, 256);

                    Assert.AreEqual(1024 + 512, stream.Position);
                    Assert.AreEqual(1024 + 512, stream.Length);

                    // no flush, so no db update
                    AssertDatabaseBlobLength(connection, 1024 + 256);
                }

                // dispose should flush
                AssertDatabaseBlobLength(connection, 1024 + 512);
            }
        }

        private static void AssertDatabaseBlobLength(SqliteConnection connection, int expectedLength)
        {
            using (var command = new SqliteCommand(
                "SELECT length(blob) FROM files WHERE path = '/test.blob'",
                connection))
            {
                long dbBlobLength = (long)command.ExecuteScalar();
                Assert.AreEqual(expectedLength, dbBlobLength);
            }
        }
    }
}

