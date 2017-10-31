using System;
using System.Collections.Generic;
using System.Text;

namespace SqliteFileSystem.Tests.Helpers
{
    using System.Diagnostics;
    using System.IO;
    using Microsoft.Data.Sqlite;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Zio;
    using Zio.FileSystems.Community.SqliteFileSystem;

    public static class TestHelper
    {
        public static FileInfo GetTempFile(string ext)
        {
            return new FileInfo(Path.Combine(GetTempDir().FullName, Path.GetRandomFileName().Substring(0, 9) + ext));
        }

        public static DirectoryInfo GetTempDir()
        {
            var dir = "." + Path.DirectorySeparatorChar + Path.GetRandomFileName();

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return new DirectoryInfo(dir);
        }

        public static Random GetRandom(int? seed = null)
        {
            seed = seed ?? Environment.TickCount;

            Debug.WriteLine("\n\nRandom seed used: " + seed.Value);

            return new Random(seed.Value);
        }


        public static void DeleteTempDir(DirectoryInfo directory)
        {
            try
            {
                Directory.Delete(directory.FullName, true);
            }
            catch
            {
                Debug.WriteLine($"Deleting directory {directory} failed");
            }
        }

        public static string GetRandomName()
        {
            return Path.GetRandomFileName();
        }

        public static (UPath Path, byte[] Data) GenerateTestData(Random random, string path = null)
        {
            var data = new byte[1024];
            random.NextBytes(data);

            return (path ?? UPath.Root / GetRandomName(), data);
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
            var testData = TestHelper.GenerateTestData(random, "/test.blob");

            // create a new empty database - mainly doing this to get a schema
            using (var fs = new SqliteFileSystem(testFile, OpenMode.ReadWriteCreate))
            {
            }

            var connectionString = $"Data source='{testFile}';Mode={OpenMode.ReadWrite}";
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
            AssertBlobMetadata(
                connection,
                expectedLength,
                Date.FromTicks(expectedAccessed),
                Date.FromTicks(expectedCreated),
                Date.FromTicks(expectedModified),
                path);
        }

        public static void AssertBlobMetadata(
            SqliteConnection connection,
            int expectedLength,
            DateTime expectedAccessed,
            DateTime expectedCreated,
            DateTime expectedModified,
            string path = "/test.blob",
            TimeSpan? delta = null)
        {
            using (var command = new SqliteCommand(
                $"SELECT length(blob), accessed, created, written FROM files WHERE path = '{path}'",
                connection))
            {
                var reader = command.ExecuteReader();

                Assert.IsTrue(reader.Read());
                Assert.AreEqual(expectedLength, reader.GetInt32(0));
                Assert.That.AreClose(expectedAccessed, Date.FromTicks(reader.GetInt64(1)), delta ?? TimeSpan.FromMilliseconds(15));
                Assert.That.AreClose(expectedCreated, Date.FromTicks(reader.GetInt64(2)), delta ?? TimeSpan.FromMilliseconds(15));
                Assert.That.AreClose(expectedModified, Date.FromTicks(reader.GetInt64(3)), delta ?? TimeSpan.FromMilliseconds(15));
            }
        }
        public static object DirectQuery(string query, string path, string mode = "ReadOnly")
        {
            using (var connection = new SqliteConnection($"Data source='{path}';Mode={mode}"))
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
