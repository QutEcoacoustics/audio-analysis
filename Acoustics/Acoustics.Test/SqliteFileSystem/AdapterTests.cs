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
    using TestHelpers;
    using Zio.FileSystems;
    using Zio.FileSystems.Additional;

    [TestClass]
    public class AdapterTests
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
        public void AdapterCanStreamBlobs()
        {
            // create a new empty database - mainly doing this to get a schema
            using (var fs = new SqliteFileSystem(this.testFile.FullName, SqliteOpenMode.ReadWriteCreate))
            {
            }

            var connectionString = $"Data source='{this.testFile.FullName}';Mode={SqliteOpenMode.ReadWrite}";
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // add a blob we can read

                using (var command = new SqliteCommand("INSERT INTO files VALUES ('/test.blob', @blob)"))
                {
                    var parameter = new SqliteParameter("blob", SqliteType.Blob) { Value = this.sampleBlob };
                    command.Parameters.Add(parameter);

                    command.ExecuteNonQuery();
                }


                // verify we can stream the response back
                byte[] result;
                using (var readStream = Adapter.ExecuteNonQueryStream(connection, "SELECT blob FROM files LIMIT 1"))
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
        }

    }
}
