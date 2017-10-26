namespace SqliteFileSystem.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Helpers;
    using Microsoft.Data.Sqlite;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Zio;
    using Zio.FileSystems.Community.SqliteFileSystem;

    [TestClass]
    public class AdapterTests
    {

        protected DirectoryInfo outputDirectory;
        private FileInfo testFile;
        private System.Random random;


        [TestInitialize]
        public void Setup()
        {
            this.testFile = TestHelper.GetTempFile("sqlite3");
            this.outputDirectory = this.testFile.Directory;
            this.random = TestHelper.GetRandom();
            
        }

        [TestCleanup]
        public void Cleanup()
        {
            Debug.WriteLine("Deleting output directory:" + this.outputDirectory.FullName);

            TestHelper.DeleteTempDir(this.outputDirectory);
        }

        [TestMethod]
        public void GetBlobTest()
        {
            var prepared = TestHelper.PrepareDatabaseAndBlob(this.testFile.FullName, this.random);

            using (var connection = new SqliteConnection(prepared.ConnectionString))
            {
                connection.Open();

                var results = new List<TimeSpan>(10);
                var lastNow = 0L;
                for (var i = 0; i < 10; i++)
                {

                    // before test, everything should be set up according to mock data
                    TestHelper.AssertBlobMetadata(connection, 1024, lastNow, 0, 0);

                    var now = Date.Now;
                    var timer = Stopwatch.StartNew();
                    var blob = Adapter.GetBlob(connection, UPath.Root / "test.blob");

                    timer.Stop();
                    results.Add(timer.Elapsed);

                    CollectionAssert.AreEqual(prepared.blobData, blob);

                    // now the accessed date should have been updated
                    TestHelper.AssertBlobMetadata(connection, 1024, now, 0, 0);
                    lastNow = now;
                }

                var average = results.Average(x => x.TotalSeconds);
                Debug.WriteLine($"10 getblobs took an average of {average} seconds.\nRaw: { string.Join(", ", results) }");
            }
        }

        [TestMethod]
        public void SetBlobTestInsert()
        {
            var prepared = TestHelper.PrepareDatabaseAndBlob(this.testFile.FullName, this.random);

            using (var connection = new SqliteConnection(prepared.ConnectionString))
            {
                connection.Open();

                // before test, everything should be set up according to mock data
                TestHelper.AssertBlobMetadata(connection, 1024, 0, 0, 0);

                // insert an entirely new blob
                var newBlob = TestHelper.GenerateTestData(this.random);

                var now = Date.Now;
                Adapter.SetBlob(connection, UPath.Root / newBlob.Path, newBlob.Data);

                // now all dates should have been updated on the new blob
                TestHelper.AssertBlobMetadata(connection, 1024, 0, 0, 0);
                TestHelper.AssertBlobMetadata(connection, 1024, now, now, now, newBlob.Path.FullName);

                var now2 = Date.Now;
                var blob = Adapter.GetBlob(connection, newBlob.Path.FullName);

                CollectionAssert.AreEqual(newBlob.Data, blob);

                // now the accessed date should have been updated
                TestHelper.AssertBlobMetadata(connection, 1024, 0, 0, 0);
                TestHelper.AssertBlobMetadata(connection, 1024, now2, now, now, newBlob.Path.FullName);
            }
        }

        [TestMethod]
        public void SetBlobTestUpdate()
        {
            var prepared = TestHelper.PrepareDatabaseAndBlob(this.testFile.FullName, this.random);

            using (var connection = new SqliteConnection(prepared.ConnectionString))
            {
                connection.Open();

                // before test, everything should be set up according to mock data
                TestHelper.AssertBlobMetadata(connection, 1024, 0, 0, 0);

                // overwrite the blob
                var newBlob = TestHelper.GenerateTestData(this.random, "/test.blob");

                var now = Date.Now;
                Adapter.SetBlob(connection, UPath.Root / "test.blob", newBlob.Data);

                // now the accessed date and modified date should have been updated
                TestHelper.AssertBlobMetadata(connection, 1024, now, 0, now);

                var now2 = Date.Now;
                var blob = Adapter.GetBlob(connection, UPath.Root / "test.blob");

                CollectionAssert.AreEqual(newBlob.Data, blob);

                // now the accessed date should have been updated
                TestHelper.AssertBlobMetadata(connection, 1024, now2, 0, now);
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
