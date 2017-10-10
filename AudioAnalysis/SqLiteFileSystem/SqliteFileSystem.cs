using System;

// ReSharper disable once CheckNamespace
namespace Zio.FileSystems.Additional
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using global::SqliteFileSystem;
    using Microsoft.Data.Sqlite;
    using Zio;
    using Zio.FileSystems;

    /// <summary>
    /// Implements a virtual file system backed by a SQLite3 file database.
    /// </summary>
    /// <remarks>
    /// Note: the file system stored in the SQLite database is a *flat* file system. Thus there is no such thing as
    /// directories and directory support is emulated by this implementation.
    /// <para>
    /// Thie file system has no notion of locking or security. It may be implemented in the future if it is needed.
    /// </para>
    /// </remarks>
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
            // original implmentation lifted from: https://raw.githubusercontent.com/xoofx/zio/06e59868adaacd3fc9d174c992009a6a2520f659/src/Zio/FileSystems/MemoryFileSystem.cs

            if (mode == FileMode.Append && (access & FileAccess.Read) != 0)
            {
                throw new ArgumentException(
                    "Combining FileMode: Append with FileAccess: Read is invalid.",
                    nameof(access));
            }

            // no support for sharing or locking files



            var isReading = (access & FileAccess.Read) != 0;
            var isWriting = (access & FileAccess.Write) != 0;
            var isExclusive = share == FileShare.None;

            /*
            //EnterFileSystemShared();
            //DirectoryNode parentDirectory = null;
            //FileNode fileNodeToRelease = null;
            try
            {
                // Append: Opens the file if it exists and seeks to the end of the file, or creates a new file. 
                //         This requires FileIOPermissionAccess.Append permission. FileMode.Append can be used only in 
                //         conjunction with FileAccess.Write. Trying to seek to a position before the end of the file 
                //         throws an IOException exception, and any attempt to read fails and throws a 
                //         NotSupportedException exception.
                //
                //
                // CreateNew: Specifies that the operating system should create a new file.This requires 
                //            FileIOPermissionAccess.Write permission. If the file already exists, an IOException 
                //            exception is thrown.
                //
                // Open: Specifies that the operating system should open an existing file. The ability to open 
                //       the file is dependent on the value specified by the FileAccess enumeration. 
                //       A System.IO.FileNotFoundException exception is thrown if the file does not exist.
                //
                // OpenOrCreate: Specifies that the operating system should open a file if it exists; 
                //               otherwise, a new file should be created. If the file is opened with 
                //               FileAccess.Read, FileIOPermissionAccess.Read permission is required. 
                //               If the file access is FileAccess.Write, FileIOPermissionAccess.Write permission 
                //               is required. If the file is opened with FileAccess.ReadWrite, both 
                //               FileIOPermissionAccess.Read and FileIOPermissionAccess.Write permissions 
                //               are required. 
                //
                // Truncate: Specifies that the operating system should open an existing file. 
                //           When the file is opened, it should be truncated so that its size is zero bytes. 
                //           This requires FileIOPermissionAccess.Write permission. Attempts to read from a file 
                //           opened with FileMode.Truncate cause an ArgumentException exception.

                // Create: Specifies that the operating system should create a new file.If the file already exists, 
                //         it will be overwritten.This requires FileIOPermissionAccess.Write permission. 
                //         FileMode.Create is equivalent to requesting that if the file does not exist, use CreateNew; 
                //         otherwise, use Truncate. If the file already exists but is a hidden file, 
                //         an UnauthorizedAccessException exception is thrown.

                // does the file exist?
                bool exists = this.FileExistsImpl(path);

                bool shouldTruncate = false;
                bool shouldAppend = false;

                if (mode == FileMode.Create)
                {
                    if (exists)
                    {
                        mode = FileMode.Open;
                        shouldTruncate = true;
                    }
                    else
                    {
                        mode = FileMode.CreateNew;
                    }
                }

                if (mode == FileMode.OpenOrCreate)
                {
                    mode = exists ? FileMode.Open : FileMode.CreateNew;
                }

                if (mode == FileMode.Append)
                {
                    if (exists)
                    {
                        mode = FileMode.Open;
                        shouldAppend = true;
                    }
                    else
                    {
                        mode = FileMode.CreateNew;
                    }
                }
                
                if (mode == FileMode.Truncate)
                {
                    if (exists)
                    {
                        mode = FileMode.Open;
                        shouldTruncate = true;
                    }
                    else
                    {
                        throw SqliteFileSystem.FileSystemExceptionHelper.NewFileNotFoundException(path);
                    }
                }

                // Here we should only have Open or CreateNew
                Debug.Assert(mode == FileMode.Open || mode == FileMode.CreateNew);

                if (mode == FileMode.CreateNew)
                {
                    // This is not completely accurate to throw an exception (as we have been called with an option to OpenOrCreate)
                    // But we assume that between the beginning of the method and here, the filesystem is not changing, and 
                    // if it is, it is an unfortunate conrurrency
                    if (!exists)
                    {
                        throw NewDestinationFileExistException(path);
                    }
                }
                else
                {
                    if (!exists)
                    {
                        throw NewFileNotFoundException(path);
                    }
                }

                // TODO: Add checks between mode and access

                // todo: optimize for sending streams

                // Create a memory file stream
                var stream = new MemoryStream();
                var stream = new MemoryFileStream(this, fileNode, isReading, isWriting, isExclusive);
                if (shouldAppend)
                {
                    stream.Position = stream.Length;
                }
                else if (shouldTruncate)
                {
                    stream.SetLength(0);
                }
                return stream;
            }
            finally
            {

            }
            */
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
