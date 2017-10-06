using System;
using System.Collections.Generic;
using System.Text;

namespace SqliteFileSystem
{
    using System.Runtime.CompilerServices;
    using Microsoft.Data.Sqlite;

    public static class Adapter
    {
        public const string SchemaVersion = "1.0.0";
        private const string FilesTable = "files";
        private const string FilesPath = "path";
        private const string FilesCreated = "created";
        private const string FilesAccessed = "accessed";
        private const string FilesModified = "modified";

        internal static readonly string FileExists = $"SELECT EXISTS(SELECT 1 FROM {FilesTable} WHERE {FilesPath} = '?' LIMIT 1)";
        internal static readonly string GetSchemaVersion = $"SELECT {MetaVersion} FROM meta LIMIT 1";
        internal static readonly string CreateSchema = $@"
PRAGMA page_size = {PageSize};
VACUUM;

CREATE TABLE {FilesTable} ({FilesPath} TEXT PRIMARY KEY, {FilesBlob} BLOB NOT NULL, {FilesAccessed} INTEGER NOT NULL, {FilesCreated} INTEGER NOT NULL, {FilesModified} INTEGER NOT NULL);

CREATE TABLE {MetaTable} ({MetaVersion} TEXT);

INSERT INTO {MetaTable} ({MetaVersion}) VALUES ('{SchemaVersion}');
";

        internal static readonly string VerifySchema = $@"SELECT
EXISTS (SELECT name FROM sqlite_master WHERE type = 'table' AND name = '{FilesTable}' LIMIT 1)
AND
EXISTS (SELECT name FROM sqlite_master WHERE type = 'table' AND name = '{MetaTable}' LIMIT 1)
";

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
                command.Parameters.AddWithValue("?", param1);
                return (long)command.ExecuteScalar();
            }
        }
    }
}
