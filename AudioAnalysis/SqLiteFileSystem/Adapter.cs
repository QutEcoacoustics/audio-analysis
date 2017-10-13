namespace Zio.FileSystems.Community.SqliteFileSystem
{
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using Microsoft.Data.Sqlite;
    using Zio;

    public static class Adapter
    {
        internal enum Node
        {
            NotFound = 0,
            File = 1,
            Directory = 2
        }

        public const string SchemaVersion = "1.0.0";
        private const string FilesTable = "files";
        private const string FilesPath = "path";
        private const string FilesCreated = "created";
        private const string FilesAccessed = "accessed";
        private const string FilesWritten = "written";
        private const string UnnamedParameter1 = "unnamed1";

        internal static readonly string FileExists = $"SELECT EXISTS(SELECT 1 FROM {FilesTable} WHERE {FilesPath} = @{UnnamedParameter1} LIMIT 1)";
        /// this is a flat file system... directories only exist if there are files 'in them'. Thus we only search for 
        /// files which have at least one / and then at least one of more file name characters afterwards
        internal static readonly string DirectoryExists = $"SELECT EXISTS(SELECT 1 FROM {FilesTable} WHERE {FilesPath} LIKE @{UnnamedParameter1} + '{UPath.DirectorySeparator}_%' LIMIT 1)";

        internal static readonly string FileOrDirectoryExists = $@"SELECT CASE
WHEN ({FileExists}) = 1 THEN {(int)Node.File}
WHEN ({DirectoryExists}) = 1 THEN {(int)Node.Directory}
ELSE {(int)Node.NotFound}
END;";
        internal static readonly string FileLength = $"SELECT length({FilesBlob}) FROM {FilesTable} WHERE {FilesPath} = @{UnnamedParameter1} LIMIT 1";
        internal static readonly string GetSchemaVersion = $"SELECT {MetaVersion} FROM meta LIMIT 1";
        internal static readonly string CreateSchema = $@"
PRAGMA page_size = {PageSize};
VACUUM;

CREATE TABLE {FilesTable} ({FilesPath} TEXT PRIMARY KEY, {FilesBlob} BLOB NOT NULL, {FilesAccessed} INTEGER NOT NULL, {FilesCreated} INTEGER NOT NULL, {FilesWritten} INTEGER NOT NULL);

CREATE TABLE {MetaTable} ({MetaVersion} TEXT);

INSERT INTO {MetaTable} ({MetaVersion}) VALUES ('{SchemaVersion}');";

        internal static readonly string VerifySchema = $@"SELECT
EXISTS (SELECT name FROM sqlite_master WHERE type = 'table' AND name = '{FilesTable}' LIMIT 1)
AND
EXISTS (SELECT name FROM sqlite_master WHERE type = 'table' AND name = '{MetaTable}' LIMIT 1)
";

        internal static readonly string RetrieveBlob =
            $@"BEGIN;
SELECT {FilesBlob} FROM {FilesTable} WHERE {FilesPath} = @{FilesPath} LIMIT 1;
UPDATE {FilesTable} SET {FilesAccessed} = @{FilesAccessed} WHERE {FilesPath} = @{FilesPath};
COMMIT;";
        internal static readonly string StoreBlob =
            $@"BEGIN;
UPDATE {FilesTable} SET {FilesBlob} = @{FilesBlob}, {FilesAccessed} = @{FilesAccessed} , {FilesWritten} = @{FilesWritten} WHERE {FilesPath} = @{FilesPath};
INSERT INTO {FilesTable} ({FilesPath}, {FilesBlob}, {FilesAccessed}, {FilesCreated}, {FilesWritten}) SELECT @{FilesPath}, @{FilesBlob}, @{FilesAccessed}, @{FilesCreated}, @{FilesWritten} WHERE changes() = 0;
COMMIT;";
        internal static readonly string InsertFile =
            $@"INSERT INTO {FilesTable} ({FilesPath}, {FilesBlob}, {FilesAccessed}, {FilesCreated}, {FilesWritten}) VALUES (@{FilesPath}, x'', @{FilesAccessed}, @{FilesCreated}, @{FilesWritten})";

        internal static readonly string DeleteFile =
            $@"DELETE FROM {FilesTable} WHERE {FilesPath} = @{FilesPath}";

        private const string FilesBlob = "blob";

        private const string MetaTable = "meta";
        private const string MetaVersion = "version";

        public const int PageSize = 8192;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ExecuteNonQuery(SqliteConnection connection, string commandText)
        {
            using (var command = new SqliteCommand(commandText, connection))
            {
                return command.ExecuteNonQuery();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte[] GetBlob(SqliteConnection connection, UPath path)
        {
            var now = Date.Now;
            using (var command = new SqliteCommand(RetrieveBlob, connection))
            {
                command.Parameters.AddWithValue(FilesPath, path.FullName);
                command.Parameters.AddWithValue(FilesAccessed, now);
                using (var reader = command.ExecuteReader())
                {
                    Debug.Assert(reader.NextResult());
                    reader.Read();

                    return reader.GetFieldValue<byte[]>(0);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int SetBlob(SqliteConnection connection, UPath path, byte[] blob)
        {
            Debug.Assert(blob != null);
            var now = Date.Now;

            using (var command = new SqliteCommand(StoreBlob, connection))
            {
                command.Parameters.AddWithValue(FilesPath, path.FullName);
                command.Parameters.Add(new SqliteParameter(FilesBlob, SqliteType.Blob) { Value = blob });
                command.Parameters.AddWithValue(FilesAccessed, now);

                // note: created will be ignored if it is an INSERT instead of an UPDATE
                command.Parameters.AddWithValue(FilesCreated, now);
                command.Parameters.AddWithValue(FilesWritten, now);
                var affected = command.ExecuteNonQuery();

                if (affected != 1)
                {
                    throw new InvalidOperationException($"Storing blob at {path} failed because affected rows did not equal 1.");
                }

                return affected;
            }
        }

        internal static int CreateFile(SqliteConnection connection, UPath path)
        {
            var now = Date.Now;
            using (var command = new SqliteCommand(InsertFile, connection))
            {
                command.Parameters.AddWithValue(FilesPath, path.FullName);
                command.Parameters.AddWithValue(FilesAccessed, now);
                command.Parameters.AddWithValue(FilesCreated, now);
                command.Parameters.AddWithValue(FilesWritten, now);
                
                return command.ExecuteNonQuery();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ExecuteScalarString(SqliteConnection connection, string commandText)
        {
            using (var command = new SqliteCommand(commandText, connection))
            {
                return (string)command.ExecuteScalar();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool ExecuteScalarBool(SqliteConnection connection, string commandText)
        {
            
            using (var command = new SqliteCommand(commandText, connection))
            {
                return (long)command.ExecuteScalar() == 1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static long ExecuteScalarLong(SqliteConnection connection, string commandText)
        {
            
            using (var command = new SqliteCommand(commandText, connection))
            {
                return (long)command.ExecuteScalar();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static long ExecuteScalarLong(SqliteConnection connection, string commandText, string param1)
        {
            using (var command = new SqliteCommand(commandText, connection))
            {
                command.Parameters.AddWithValue(UnnamedParameter1, param1);
                return (long)command.ExecuteScalar();
            }
        }
    }
}
