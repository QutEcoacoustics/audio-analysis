using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Acoustics.Test.SqliteFileSystem
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Acoustics.Tools.Wav;
    using Microsoft.Data.Sqlite;
    using TestHelpers;
    using Zio;
    using Zio.FileSystems.Community.SqliteFileSystem;
    using Random = TestHelpers.Random;

    [TestClass]
    public class DatabaseBackedMemoryStreamTests
    {
        protected DirectoryInfo outputDirectory;
        private FileInfo testFile;
        private System.Random random;
        private byte[] sampleBlob;

        [TestInitialize]
        public void Setup()
        {
            this.testFile = PathHelper.GetTempFile("sqlite3");
            this.outputDirectory = this.testFile.Directory;
            this.random = Random.GetRandom();
            this.sampleBlob = new byte[1024];

            this.random.NextBytes(this.sampleBlob);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Debug.WriteLine("Deleting output directory:" + this.outputDirectory.FullName);

            PathHelper.DeleteTempDir(this.outputDirectory);
        }

        [TestMethod]
        public void TestDatabaseBackedMemoryStream()
        {
            var prepared = AdapterTests.PrepareDatabaseAndBlob(this.testFile.FullName, this.random);
            using (var connection = new SqliteConnection(prepared.ConnectionString))
            {
                connection.Open();
                using (var stream = new DatabaseBackedMemoryStream(connection, UPath.Root / "test.blob", true, true))
                {

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
            }
        }


        [TestMethod]
        public void TestDatabaseBackedMemoryStreamWriting()
        {
            var prepared = AdapterTests.PrepareDatabaseAndBlob(this.testFile.FullName, this.random);

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

