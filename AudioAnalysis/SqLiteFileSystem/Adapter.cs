namespace Zio.FileSystems.Community.SqliteFileSystem
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
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
        internal const string FilesTable = "files";
        internal const string FilesPath = "path";
        internal const string FilesCreated = "created";
        internal const string FilesAccessed = "accessed";
        internal const string FilesWritten = "written";
        internal const string UnnamedParameter1 = "unnamed1";
        internal const string FilesDestPath = "dest_path";
        internal const string FilesBackupPath = "backup_path";
        internal const string NowParam = "date_now";

        private static string MakeFileFilter(string paramName, bool includeSubDirectories)
        {
            var baseFilter = $"{FilesPath} LIKE @{paramName} || '_%'";
            var thisDirFilter = $"AND {FilesPath} NOT LIKE @{paramName} || '_%{UPath.DirectorySeparator}%'";
            return baseFilter + (includeSubDirectories ? string.Empty : thisDirFilter);
        }

        internal static readonly string FileTimeStamps = $"SELECT {FilesAccessed},{FilesCreated},{FilesWritten} FROM {FilesTable} WHERE {FilesPath} = @{FilesPath} LIMIT 1";
        internal static readonly string DirectoryTimeStamps = $@"
SELECT MAX({FilesAccessed}),MIN({FilesCreated}),MAX({FilesWritten})
FROM {FilesTable}
WHERE {MakeFileFilter(FilesPath, true)}
LIMIT 1";

        internal static readonly Func<string, string> FileTimeStampUpdate = (field) =>
            $"UPDATE {FilesTable} SET {field} = @{field} WHERE {FilesPath} = @{FilesPath};";

        internal static readonly string FileExists = $"SELECT EXISTS(SELECT 1 FROM {FilesTable} WHERE {FilesPath} = @{UnnamedParameter1} LIMIT 1)";

        /// this is a flat file system... directories only exist if there are files 'in them'. Thus we only search for 
        /// files which have at least one / and then at least one of more file name characters afterwards
        internal static readonly string DirectoryExists = $"SELECT EXISTS(SELECT 1 FROM {FilesTable} WHERE {MakeFileFilter(UnnamedParameter1, true)} LIMIT 1)";

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
UPDATE {FilesTable} SET {FilesBlob} = @{FilesBlob}, {FilesAccessed} = @{FilesAccessed}, {FilesWritten} = @{FilesWritten} WHERE {FilesPath} = @{FilesPath};
INSERT INTO {FilesTable} ({FilesPath}, {FilesBlob}, {FilesAccessed}, {FilesCreated}, {FilesWritten}) SELECT @{FilesPath}, @{FilesBlob}, @{FilesAccessed}, @{FilesCreated}, @{FilesWritten} WHERE changes() = 0;
COMMIT;";
        internal static readonly string InsertFile =
            $@"INSERT INTO {FilesTable} ({FilesPath}, {FilesBlob}, {FilesAccessed}, {FilesCreated}, {FilesWritten}) VALUES (@{FilesPath}, x'', @{FilesAccessed}, @{FilesCreated}, @{FilesWritten})";

        internal static readonly string DeleteFileQuery =
            $@"DELETE FROM {FilesTable} WHERE {FilesPath} = @{FilesPath}";

        internal static readonly Func<bool, string> CopyFileQuery = (overwrite) => $@"
INSERT { (overwrite ? "OR REPLACE" : "OR ABORT") } INTO {FilesTable}
SELECT @{FilesDestPath}, {FilesBlob}, @{FilesAccessed}, @{FilesCreated}, {FilesWritten}
FROM {FilesTable}
WHERE {FilesPath} = @{FilesPath}";

        internal static readonly string MoveFileQuery = $@"UPDATE {FilesTable} SET {FilesPath} = @{FilesDestPath} WHERE {FilesPath} = @{FilesPath}";
        internal static readonly Func<bool, string> ReplaceFileQuery = (keepBackup) =>
        {
            var backup = keepBackup
                ? $"UPDATE OR REPLACE {FilesTable} SET {FilesPath} = @{FilesBackupPath} WHERE {FilesPath} = @{FilesDestPath};"
                : $"DELETE FROM {FilesTable} WHERE {FilesPath} = @{FilesDestPath};";
             return $@"BEGIN;
{backup}
UPDATE OR ABORT {FilesTable} SET {FilesPath} = @{FilesDestPath}, {FilesCreated} = @{FilesCreated} WHERE {FilesPath} = @{FilesPath};
END;";
        };

        internal static readonly string ListFilesQuery = $@"SELECT {FilesPath} FROM {FilesTable} WHERE {MakeFileFilter(FilesPath, false)}";
        internal static readonly string ListFilesRecursiveQuery = $@"SELECT {FilesPath} FROM {FilesTable} WHERE {MakeFileFilter(FilesPath, true)}";

        internal static readonly string DeleteDirectoryQuery =
            $@"DELETE FROM {FilesTable} WHERE {MakeFileFilter(FilesPath, true)}";

        internal static readonly string MoveDirectoryQuery =
            $@"UPDATE OR ABORT {FilesTable} SET {FilesPath} = (@{FilesDestPath} || substr({FilesPath}, length(@{FilesPath}) + 1)) WHERE {MakeFileFilter(FilesPath, true)}";

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

                    return reader.IsDBNull(0) ? null : reader.GetFieldValue<byte[]>(0);
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
                    throw new InvalidOperationException(
                        $"Storing blob at {path} failed because affected rows did not equal 1.");
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

        internal static void CopyFile(SqliteConnection connection, UPath source, UPath destination, bool overwrite)
        {
            var now = Date.Now;
            string query = CopyFileQuery(overwrite);
            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue(FilesPath, source.FullName);
                command.Parameters.AddWithValue(FilesDestPath, destination.FullName);

                command.Parameters.AddWithValue(FilesAccessed, now);
                command.Parameters.AddWithValue(FilesCreated, now);
                int affected;
                try
                {
                    affected = command.ExecuteNonQuery();
                }
                catch (SqliteException sex) when (sex.SqliteErrorCode == SQLitePCL.raw.SQLITE_CONSTRAINT)
                {
                    throw FileSystemExceptionHelper.NewFileExistsException(destination, sex);
                }

                if (affected != 1)
                {
                    throw new InvalidOperationException(
                        $"Copying file form {source} to {destination} failed because affected rows did not equal 1.");
                }
            }
        }

        internal static void DeleteFile(SqliteConnection connection, UPath source)
        {
            using (var command = new SqliteCommand(DeleteFileQuery, connection))
            {
                command.Parameters.AddWithValue(FilesPath, source.FullName);

                int affected = command.ExecuteNonQuery();

                if (affected == 0)
                {
                    FileSystemExceptionHelper.NewFileNotFoundException(source);
                }

                if (affected > 1)
                {
                    throw new InvalidOperationException(
                        $"Deleting file form {source} failed because affected rows did not equal 1.");
                }
            }
        }

        internal static void MoveFile(SqliteConnection connection, UPath source, UPath destination)
        {
            var now = Date.Now;
            using (var command = new SqliteCommand(MoveFileQuery, connection))
            {
                command.Parameters.AddWithValue(FilesPath, source.FullName);
                command.Parameters.AddWithValue(FilesDestPath, destination.FullName);
                
                int affected;
                try
                {
                    affected = command.ExecuteNonQuery();
                }
                catch (SqliteException sex) when (sex.SqliteErrorCode == SQLitePCL.raw.SQLITE_CONSTRAINT)
                {
                    throw FileSystemExceptionHelper.NewFileExistsException(destination, sex);
                }

                if (affected != 1)
                {
                    throw new InvalidOperationException(
                        $"Copying file form {source} to {destination} failed because affected rows did not equal 1.");
                }
            }
        }

        internal static void ReplaceFile(SqliteConnection connection, UPath source, UPath destination, UPath destBackupPath)
        {
            var makeBackup = !destBackupPath.IsNull;
            var destinationCreated = GetFileTimeStamps(connection, destination).Created;
            using (var command = new SqliteCommand(ReplaceFileQuery(makeBackup), connection))
            {
                command.Parameters.AddWithValue(FilesPath, source.FullName);
                command.Parameters.AddWithValue(FilesDestPath, destination.FullName);
                command.Parameters.AddWithValue(FilesBackupPath, destBackupPath.FullName);

                command.Parameters.AddWithValue(FilesCreated, Date.ToTicks(destinationCreated));

                int affected = command.ExecuteNonQuery();

                if (affected != 2)
                {
                    throw new InvalidOperationException(
                        $"Replacing file {destination} with {source }failed because affected rows did not equal 2.");
                }
            }
        }

        internal static IEnumerable<UPath> ListPaths(SqliteConnection connection, UPath searchPath, SearchOption searchOption)
        {
            var query = searchOption == SearchOption.AllDirectories ? ListFilesRecursiveQuery : ListFilesQuery;
            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue(FilesPath, searchPath.FullName);
                using (var reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        yield return (UPath)reader.GetString(0);
                    }
                }
            }
        }

        internal static (DateTime Accessed, DateTime Created, DateTime Written) GetFileTimeStamps(SqliteConnection connection, UPath path)
        {
            using (var command = new SqliteCommand(FileTimeStamps, connection))
            {
                command.Parameters.AddWithValue(FilesPath, path.FullName);

                using (var reader = command.ExecuteReader())
                {
                    Debug.Assert(reader.Read());


                    var accessed = Date.FromTicks(reader.GetFieldValue<long>(0));
                    var created = Date.FromTicks(reader.GetFieldValue<long>(1));
                    var written = Date.FromTicks(reader.GetFieldValue<long>(2));

                    Debug.Assert(!reader.Read());

                    return (accessed, created, written);
                }
            }
        }

        internal static void SetTimeStamp(SqliteConnection connection, UPath path, string field, DateTime value)
        {
            var validFields = new[] { FilesAccessed, FilesCreated, FilesWritten };

            if (!validFields.Contains(field))
            {
                throw new ArgumentException($"Supplied field `{field}` is not valid");
            }

            using (var command = new SqliteCommand(FileTimeStampUpdate(field), connection))
            {
                command.Parameters.AddWithValue(field, Date.ToTicks(value));
                command.Parameters.AddWithValue(FilesPath, path.FullName);

                var result = command.ExecuteNonQuery();
                if (result != 1)
                {
                    throw new InvalidOperationException($"Updating timestamp at {path} failed because affected rows did not equal 1.");
                }
            }
        }

        internal static (DateTime Accessed, DateTime Created, DateTime Written) GetDirectoryTimeStamps(SqliteConnection connection, UPath path)
        {
            using (var command = new SqliteCommand(DirectoryTimeStamps, connection))
            {
                command.Parameters.AddWithValue(FilesPath, path.FullName);

                using (var reader = command.ExecuteReader())
                {
                    Debug.Assert(reader.Read());

                    return (
                        Date.FromTicks(reader.GetFieldValue<long>(0)), 
                        Date.FromTicks(reader.GetFieldValue<long>(0)),
                        Date.FromTicks(reader.GetFieldValue<long>(0)));
                }
            }
        }

        internal static void DeleteDirectory(SqliteConnection connection, UPath source)
        {
            using (var command = new SqliteCommand(DeleteDirectoryQuery, connection))
            {
                command.Parameters.AddWithValue(FilesPath, source.FullName);

                int affected = command.ExecuteNonQuery();

                if (affected == 0)
                {
                    throw FileSystemExceptionHelper.NewDirectoryNotFoundException(source);
                }

                if (affected < 1)
                {
                    throw new InvalidOperationException(
                        $"Deleting file form {source} failed because affected rows did not equal 1.");
                }
            }
        }

        internal static void MoveDirectory(SqliteConnection connection, UPath source, UPath destination)
        {
            using (var command = new SqliteCommand(MoveDirectoryQuery, connection))
            {
                command.Parameters.AddWithValue(FilesPath, source.FullName);
                command.Parameters.AddWithValue(FilesDestPath, destination.FullName);

                int affected;
                try
                {
                    affected = command.ExecuteNonQuery();
                }
                catch (SqliteException sex) when (sex.SqliteErrorCode == SQLitePCL.raw.SQLITE_CONSTRAINT)
                {
                    throw FileSystemExceptionHelper.NewDestinationDuplicateException(destination, sex);
                }

                if (affected < 2)
                {
                    throw new InvalidOperationException(
                        $"Moving directory file form {source} to {destination} failed because affected rows were not greater or equal than 2.");
                }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static long? ExecuteScalarLongNullable(SqliteConnection connection, string commandText, string param1)
        {
            using (var command = new SqliteCommand(commandText, connection))
            {
                command.Parameters.AddWithValue(UnnamedParameter1, param1);
                return (long?)command.ExecuteScalar();
            }
        }



    }
}
