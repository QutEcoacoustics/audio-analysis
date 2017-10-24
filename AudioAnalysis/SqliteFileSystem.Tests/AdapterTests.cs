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
    using Zio.FileSystems.Community.SqliteFileSystem;
    using static Zio.FileSystems.Community.SqliteFileSystem.Date;

    [TestClass]
    public class AdapterTests
    {

        protected DirectoryInfo outputDirectory;
        private FileInfo testFile;
        private System.Random random;


        [TestInitialize]
        public void Setup()
        {
            this.testFile = PathHelper.GetTempFile("sqlite3");
            this.outputDirectory = this.testFile.Directory;
            this.random = Random.GetRandom();
            
        }

        [TestCleanup]
        public void Cleanup()
        {
            Debug.WriteLine("Deleting output directory:" + this.outputDirectory.FullName);

            PathHelper.DeleteTempDir(this.outputDirectory);
        }

        /// <summary>
        ///  manually add a blob so we can test other parts of the code
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static (string ConnectionString, UPath blobPath, byte[] blobData) PrepareDatabaseAndBlob(
            string testFile,
            System.Random random)
        {
            var testData = SqliteFileSystemTests.GenerateTestData(random, "/test.blob");

            // create a new empty database - mainly doing this to get a schema
            using (var fs = new SqliteFileSystem(testFile, SqliteOpenMode.ReadWriteCreate))
            {
            }

            var connectionString = $"Data source='{testFile}';Mode={SqliteOpenMode.ReadWrite}";
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                InsertBlobManually(connection, testData);
            }

            return (connectionString, testData.Path, testData.Data);
        }

        public static void InsertBlobManually(SqliteConnection connection, (UPath Path, byte[] Data) testData)
        {
// add a blob we can read
            using (var command = new SqliteCommand(
                "INSERT INTO files VALUES ('" + testData.Path.FullName + "', @blob, 0, 0, 0)",
                connection))
            {
                var parameter = new SqliteParameter("blob", SqliteType.Blob) { Value = testData.Data };
                command.Parameters.Add(parameter);

                command.ExecuteNonQuery();
            }
        }

        public static void AssertBlobMetadata(
            SqliteConnection connection,
            int expectedLength,
            long expectedAccessed,
            long expectedCreated,
            long expectedModified,
            string path = "/test.blob")
        {
            using (var command = new SqliteCommand(
                $"SELECT length(blob), accessed, created, written FROM files WHERE path = '{path}'",
                connection))
            {
                var reader = command.ExecuteReader();

                Assert.IsTrue(reader.Read());
                Assert.AreEqual(expectedLength, reader.GetInt32(0));
                Assert.That.AreClose(expectedAccessed, reader.GetInt64(1), 15_000_0);
                Assert.That.AreClose(expectedCreated, reader.GetInt64(2),  15_000_0);
                Assert.That.AreClose(expectedModified, reader.GetInt64(3), 15_000_0);
            }
        }

        [TestMethod]
        public void GetBlobTest()
        {
            var prepared = PrepareDatabaseAndBlob(this.testFile.FullName, this.random);

            using (var connection = new SqliteConnection(prepared.ConnectionString))
            {
                connection.Open();

                var results = new List<TimeSpan>(10);
                var lastNow = 0L;
                for (var i = 0; i < 10; i++)
                {

                    // before test, everything should be set up according to mock data
                    AssertBlobMetadata(connection, 1024, lastNow, 0, 0);

                    var now = Now;
                    var timer = Stopwatch.StartNew();
                    var blob = Adapter.GetBlob(connection, UPath.Root / "test.blob");

                    timer.Stop();
                    results.Add(timer.Elapsed);

                    CollectionAssert.AreEqual(prepared.blobData, blob);

                    // now the accessed date should have been updated
                    AssertBlobMetadata(connection, 1024, now, 0, 0);
                    lastNow = now;
                }

                var average = results.Average(x => x.TotalSeconds);
                Debug.WriteLine($"10 getblobs took an average of {average} seconds.\nRaw: { string.Join(", ", results) }");
            }
        }

        [TestMethod]
        public void SetBlobTestInsert()
        {
            var prepared = PrepareDatabaseAndBlob(this.testFile.FullName, this.random);

            using (var connection = new SqliteConnection(prepared.ConnectionString))
            {
                connection.Open();

                // before test, everything should be set up according to mock data
                AssertBlobMetadata(connection, 1024, 0, 0, 0);

                // insert an entirely new blob
                var newBlob = SqliteFileSystemTests.GenerateTestData(this.random);

                var now = Now;
                Adapter.SetBlob(connection, UPath.Root / newBlob.Path, newBlob.Data);

                // now all dates should have been updated on the new blob
                AssertBlobMetadata(connection, 1024, 0, 0, 0);
                AssertBlobMetadata(connection, 1024, now, now, now, newBlob.Path.FullName);

                var now2 = Now;
                var blob = Adapter.GetBlob(connection, newBlob.Path.FullName);

                CollectionAssert.AreEqual(newBlob.Data, blob);

                // now the accessed date should have been updated
                AssertBlobMetadata(connection, 1024, 0, 0, 0);
                AssertBlobMetadata(connection, 1024, now2, now, now, newBlob.Path.FullName);
            }
        }

        [TestMethod]
        public void SetBlobTestUpdate()
        {
            var prepared = PrepareDatabaseAndBlob(this.testFile.FullName, this.random);

            using (var connection = new SqliteConnection(prepared.ConnectionString))
            {
                connection.Open();

                // before test, everything should be set up according to mock data
                AssertBlobMetadata(connection, 1024, 0, 0, 0);

                // overwrite the blob
                var newBlob = SqliteFileSystemTests.GenerateTestData(this.random, "/test.blob");

                var now = Now;
                Adapter.SetBlob(connection, UPath.Root / "test.blob", newBlob.Data);

                // now the accessed date and modified date should have been updated
                AssertBlobMetadata(connection, 1024, now, 0, now);

                var now2 = Now;
                var blob = Adapter.GetBlob(connection, UPath.Root / "test.blob");

                CollectionAssert.AreEqual(newBlob.Data, blob);

                // now the accessed date should have been updated
                AssertBlobMetadata(connection, 1024, now2, 0, now);
            }
        }

        [TestMethod]
        [Ignore]
        public void AdapterCanStreamReadBlobs()
        {
            // https://github.com/aspnet/Microsoft.Data.Sqlite/issues/18
            throw new NotImplementedException("Sqlite adapter does not yet support blob streaming");
            /*
            // create a new empty database - mainly doing this to get a schema
            using (var fs = new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadWriteCreate))
            {
            }

            var connectionString = $"Data source='{this.testFile.FullName}';Mode={SqliteOpenMode.ReadWrite}";
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // add a blob we can read
                using (var command = new SqliteCommand("INSERT INTO files VALUES ('/test.blob', @blob, 0, 0, 0)", connection))
                {
                    var parameter = new SqliteParameter("blob", SqliteType.Blob) { Value = this.sampleBlob };
                    command.Parameters.Add(parameter);

                    command.ExecuteNonQuery();
                }


                // verify we can stream the response back
                byte[] result;
                using (var readStream = Adapter.GetBlob(connection, "SELECT blob FROM files LIMIT 1"))
                {
                    // make sure we can read, get length, can't write
                    Assert.AreEqual(1024, readStream.Length);
                    Assert.IsTrue(readStream.CanRead);
                    Assert.IsFalse(readStream.CanWrite);
                    Assert.IsTrue(readStream.CanSeek);

                    // read the result in tiny chunks
                    result = new byte[readStream.Length];
                    var buffer = new byte[128];
                    int offset = 0;
                    while (readStream.Read(buffer, offset, 128) > 0)
                    {
                        buffer.CopyTo(result, offset);
                        offset += 128;
                    }
                }

               // finally make sure the blob was read back accurately
               CollectionAssert.AreEqual(this.sampleBlob, result);
            }
            */
        }

        [TestMethod]
        [Ignore]
        public void AdapterCanStreamWriteBlobs()
        {
            // https://github.com/aspnet/Microsoft.Data.Sqlite/issues/18
            throw new NotImplementedException("Sqlite adapter does not yet support blob streaming");
        }
    }
}
