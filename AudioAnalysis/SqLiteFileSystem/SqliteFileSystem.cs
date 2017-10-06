using System;

namespace SqLiteFileSystem
{
    using System.Collections.Generic;
    using System.IO;
    using global::SqliteFileSystem;
    using Microsoft.Data.Sqlite;
    using Zio;
    using Zio.FileSystems;

    public class SqliteFileSystem : FileSystem
    {
        

        private readonly SqliteConnection connection;
        private readonly string connectionString;
        private readonly SqliteOpenMode mode;

        static SqliteFileSystem()
        {
            SQLitePCL.Batteries_V2.Init();
        }

        public SqliteFileSystem(string connectionString)
            : this(new SqliteConnectionStringBuilder(connectionString))
        {
        }

        public SqliteFileSystem(string databasePath, SqliteOpenMode mode)
            : this(BuildConnection(databasePath, mode))
        {
        }

        private static SqliteConnectionStringBuilder BuildConnection(string databasePath, SqliteOpenMode mode)
        {
            if (!Path.IsPathRooted(databasePath))
            {
                throw new ArgumentException("Path for file must be rooted", nameof(databasePath));
            }

            var builder = new SqliteConnectionStringBuilder
            {
                Cache = SqliteCacheMode.Shared,
                DataSource = databasePath,
                Mode = mode,
            };
            return builder;
        }

        private SqliteFileSystem(SqliteConnectionStringBuilder builder)
        {
            this.connectionString = builder.ToString();
            this.mode = builder.Mode;
            this.connection = new SqliteConnection(this.connectionString);

            this.connection.Open();

            // ensure schema exists in this file
            var schemaValid = Adapter.ExecuteScalarBool(this.connection, Adapter.VerifySchema);

            if (!schemaValid)
            {
               try
                {
                    Adapter.ExecuteNonQuery(this.connection, Adapter.CreateSchema);
                }
                catch (SqliteException se)
                {
                    throw new SqliteFileSystemException(
                        "Schema not found in provided Sqlite filesystem and could not be created",
                        se);
                }
            }

            // check schema version matches
            this.SchemaVersion = Adapter.ExecuteScalarString(this.connection, Adapter.GetSchemaVersion);

            if (this.SchemaVersion != Adapter.SchemaVersion)
            {
                throw new SqliteFileSystemException($"Schema version {this.SchemaVersion} does not match library required version {Adapter.SchemaVersion}");
            }
        }

        public string SchemaVersion { get; }


        public bool IsReadOnly => this.mode == SqliteOpenMode.ReadOnly;

        public new void Dispose()
        {
            this.connection.Dispose();
            base.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.connection.Dispose();
            }

            base.Dispose(disposing);
        }

        public string GetSqliteVersion()
        {
            return Adapter.ExecuteScalarString(this.connection, "select sqlite_version();");
        }

        protected override void CreateDirectoryImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override bool DirectoryExistsImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void MoveDirectoryImpl(UPath srcPath, UPath destPath)
        {
            throw new NotImplementedException();
        }

        protected override void DeleteDirectoryImpl(UPath path, bool isRecursive)
        {
            throw new NotImplementedException();
        }

        protected override void CopyFileImpl(UPath srcPath, UPath destPath, bool overwrite)
        {
            throw new NotImplementedException();
        }

        protected override void ReplaceFileImpl(UPath srcPath, UPath destPath, UPath destBackupPath, bool ignoreMetadataErrors)
        {
            throw new NotImplementedException();
        }

        protected override long GetFileLengthImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override bool FileExistsImpl(UPath path)
        {
            return Adapter.ExecuteScalarLong(this.connection, Adapter.FileExists, path.FullName) == 1;
        }

        protected override void MoveFileImpl(UPath srcPath, UPath destPath)
        {
            throw new NotImplementedException();
        }

        protected override void DeleteFileImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override Stream OpenFileImpl(UPath path, FileMode mode, FileAccess access, FileShare share)
        {
            throw new NotImplementedException();
        }

        protected override FileAttributes GetAttributesImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void SetAttributesImpl(UPath path, FileAttributes attributes)
        {
            throw new NotImplementedException();
        }

        protected override DateTime GetCreationTimeImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void SetCreationTimeImpl(UPath path, DateTime time)
        {
            throw new NotImplementedException();
        }

        protected override DateTime GetLastAccessTimeImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void SetLastAccessTimeImpl(UPath path, DateTime time)
        {
            throw new NotImplementedException();
        }

        protected override DateTime GetLastWriteTimeImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void SetLastWriteTimeImpl(UPath path, DateTime time)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<UPath> EnumeratePathsImpl(UPath path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget)
        {
            throw new NotImplementedException();
        }

        protected override string ConvertPathToInternalImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override UPath ConvertPathFromInternalImpl(string innerPath)
        {
            throw new NotImplementedException();
        }
    }
}
