namespace Zio.FileSystems.Community.SqliteFileSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using FileSystems;
    using Microsoft.Data.Sqlite;
    using Zio;
    using static FileSystemExceptionHelper;

    /// <summary>
    /// Implements a virtual file system backed by a SQLite3 file database.
    /// </summary>
    /// <remarks>
    /// Note: the file system stored in the SQLite database is a *flat* file system. Thus there is no such thing as
    /// directories and directory support is emulated by this implementation. Generally, get operations are simulated
    /// and set operations fail silently.
    /// <para>
    /// Thie file system has no notion of locking or security. It may be implemented in the future if it is needed.
    /// </para>
    /// <para>
    /// Note: Microsoft.Data.Sqlite does not implement GetStream or GetBytes so all blobs will be buffered to memory.
    /// This has the potential to change with https://github.com/aspnet/Microsoft.Data.Sqlite/issues/18
    /// </para>
    /// <para>
    /// This file system is designed to store many small files in a similar fashion to a zip file. Try to avoid storing
    /// any blobs that are much bigger than traditional disk block/sector sizes.
    /// </para>
    /// </remarks>
    public class SqliteFileSystem : FileSystem
    {
        internal SqliteConnection Connection { get; }
        private readonly SqliteOpenMode mode;
        private bool throwOnDirectorySet = false;

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
            this.ConnectionString = builder.ToString();
            this.mode = builder.Mode;
            this.Connection = new SqliteConnection(this.ConnectionString);
            this.IsReadOnly = this.mode == SqliteOpenMode.ReadOnly;

            this.Connection.Open();

            // ensure schema exists in this file
            var schemaValid = Adapter.ExecuteScalarBool(this.Connection, Adapter.VerifySchema);

            if (!schemaValid)
            {
               try
                {
                    Adapter.ExecuteNonQuery(this.Connection, Adapter.CreateSchema);
                }
                catch (SqliteException se)
                {
                    throw new SqliteFileSystemException(
                        "Schema not found in provided Sqlite filesystem and could not be created",
                        se);
                }
            }

            // check schema version matches
            this.SchemaVersion = Adapter.ExecuteScalarString(this.Connection, Adapter.GetSchemaVersion);

            if (this.SchemaVersion != Adapter.SchemaVersion)
            {
                throw new SqliteFileSystemException($"Schema version {this.SchemaVersion} does not match library required version {Adapter.SchemaVersion}");
            }
        }

        public string SchemaVersion { get; }


        public bool IsReadOnly { get; }

        public string ConnectionString { get; }

        public new void Dispose()
        {
            this.Connection.Dispose();
            base.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Connection.Dispose();
            }

            base.Dispose(disposing);
        }

        public string GetSqliteVersion()
        {
            return Adapter.ExecuteScalarString(this.Connection, "select sqlite_version();");
        }

        protected override void CreateDirectoryImpl(UPath path)
        {
            this.SetCheck(true);

            var node = this.SafeExists(path);
            switch (node)
            {
                case Adapter.Node.File:
                    throw NewFileExistsException(path, null);
                case Adapter.Node.Directory:
                    return;
                case Adapter.Node.NotFound:
                    // flat file system... creating a directory has no effect
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override bool DirectoryExistsImpl(UPath path)
        {
            if (path == UPath.Root)
            {
                return true;
            }

            // Note: this will only return true IFF there is a file with the parent path of the directory
            // because directories do not actually exist in this file system.
            return Adapter.ExecuteScalarLong(this.Connection, Adapter.DirectoryExists, path.FullName) == 1;
        }

        protected override void MoveDirectoryImpl(UPath srcPath, UPath destPath)
        {
            this.SetCheck(true);
            this.EnsureDirectoryExists(srcPath);
            this.EnsureDestinationNotExists(destPath);

            this.CheckNotSubDirectory(srcPath, destPath);

            Adapter.MoveDirectory(this.Connection, srcPath, destPath);
        }

        protected override void DeleteDirectoryImpl(UPath path, bool isRecursive)
        {
            this.SetCheck(true);

            this.EnsureDirectoryExists(path);

            // non-recursive can only delete empty directories... which do not exist on our file system
            if (isRecursive)
            {
                Adapter.DeleteDirectory(this.Connection, path);
            }
            else
            {
                throw NewDirectoryNotEmptyException(path);
            }
    }

        protected override void CopyFileImpl(UPath srcPath, UPath destPath, bool overwrite)
        {
            this.SetCheck(false);
            this.EnsureFileExists(srcPath);
            if (this.DirectoryExists(destPath))
            {
                throw NewDestinationDirectoryExistException(destPath);
            }

            Adapter.CopyFile(this.Connection, srcPath, destPath, overwrite);
        }

        protected override void ReplaceFileImpl(UPath srcPath, UPath destPath, UPath destBackupPath, bool ignoreMetadataErrors)
        {
            this.SetCheck(false);
            this.EnsureFileExists(srcPath);
            this.EnsureFileExists(destPath);
            if (srcPath == destPath)
            {
                throw new IOException($"Cannot replace a file with itself");
            }

            if (!destBackupPath.IsNull && srcPath == destBackupPath)
            {
                throw new IOException("Source file cannot be the same file as the backup file");
            }
            
            // Note: metadata errors has no effect in this filesystem

            Adapter.ReplaceFile(this.Connection, srcPath, destPath, destBackupPath);
        }

        protected override long GetFileLengthImpl(UPath path)
        {
            this.EnsureFileExists(path, true);

            return Adapter.ExecuteScalarLong(this.Connection, Adapter.FileLength, path.FullName);
        }

        protected override bool FileExistsImpl(UPath path)
        {
            return Adapter.ExecuteScalarLong(this.Connection, Adapter.FileExists, path.FullName) == 1;
        }

        protected override void MoveFileImpl(UPath srcPath, UPath destPath)
        {
            this.SetCheck(false);
            this.EnsureFileExists(srcPath);

            Adapter.MoveFile(this.Connection, srcPath, destPath);
        }

        protected override void DeleteFileImpl(UPath path)
        {
            this.SetCheck(false);

            var node = this.SafeExists(path);
            switch (node)
            {
                    
                case Adapter.Node.File:
                    Adapter.DeleteFile(this.Connection, path);
                    break;
                case Adapter.Node.Directory:
                    throw new UnauthorizedAccessException($"Access to path `{path}` is denied.");
                case Adapter.Node.NotFound:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override Stream OpenFileImpl(UPath path, FileMode mode, FileAccess access, FileShare share)
        {
            // original implmentation lifted from: https://raw.githubusercontent.com/xoofx/zio/06e59868adaacd3fc9d174c992009a6a2520f659/src/Zio/FileSystems/MemoryFileSystem.cs
            // all notion of locking has been stripped but should probably be readded

            if (mode == FileMode.Append && (access & FileAccess.Read) != 0)
            {
                throw new ArgumentException(
                    "Combining FileMode: Append with FileAccess: Read is invalid.",
                    nameof(access));
            }

            if (mode != FileMode.Open)
            {
                this.SetCheck(false);
            }

            // no support for sharing or locking files
            
            var isReading = (access & FileAccess.Read) != 0;
            var isWriting = (access & FileAccess.Write) != 0;
            var isExclusive = share == FileShare.None;


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
                var node = this.SafeExists(path);

                if (node == Adapter.Node.Directory)
                {
                    throw NewDirectoryNotFileException(path);
                }

                bool exists = node == Adapter.Node.File;

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
                        throw NewFileNotFoundException(path);
                    }
                }

                // Here we should only have Open or CreateNew
                Debug.Assert(mode == FileMode.Open || mode == FileMode.CreateNew);

                if (mode == FileMode.CreateNew)
                {
                    // This is not completely accurate to throw an exception (as we have been called with an option to OpenOrCreate)
                    // But we assume that between the beginning of the method and here, the filesystem is not changing, and 
                    // if it is, it is an unfortunate conrurrency
                    if (exists)
                    {
                        throw NewDestinationFileExistException(path);
                    }

                    Adapter.CreateFile(this.Connection, path);
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
                var stream = new DatabaseBackedMemoryStream(this.Connection, path, isReading, isWriting);
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
        }

        protected override FileAttributes GetAttributesImpl(UPath path)
        {
            var fileType = this.EnsureExists(path);

            // this filesystem does not encode any special attributes
            FileAttributes attributes;
            switch (fileType)
            {
                case Adapter.Node.File:
                    attributes = FileAttributes.Normal;
                    break;
                case Adapter.Node.Directory:
                    attributes = FileAttributes.Directory;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (this.IsReadOnly)
            {
                return attributes | FileAttributes.ReadOnly;
            }

            return attributes;
        }

        protected override void SetAttributesImpl(UPath path, FileAttributes attributes)
        {
            var fileType = this.EnsureExists(path);
            this.SetCheck(fileType == Adapter.Node.Directory);

            // this FileSystem does not support FileAttributes
            return;
        }

        protected override DateTime GetCreationTimeImpl(UPath path)
        {
            return this.GetTimeStamps(path).Created.ToLocalTime();
        }
        
        protected override void SetCreationTimeImpl(UPath path, DateTime time)
        {
            this.SetTimeStamps(Adapter.FilesCreated, path, time);
        }

        protected override DateTime GetLastAccessTimeImpl(UPath path)
        {
            return this.GetTimeStamps(path).Accessed.ToLocalTime();
        }

        protected override void SetLastAccessTimeImpl(UPath path, DateTime time)
        {
            this.SetTimeStamps(Adapter.FilesAccessed, path, time);
        }

        protected override DateTime GetLastWriteTimeImpl(UPath path)
        {
            return this.GetTimeStamps(path).Written.ToLocalTime();
        }

        protected override void SetLastWriteTimeImpl(UPath path, DateTime time)
        {
            this.SetTimeStamps(Adapter.FilesWritten, path, time);
        }

        protected override IEnumerable<UPath> EnumeratePathsImpl(UPath path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget)
        {
            var pattern = SearchPattern.Parse(ref path, ref searchPattern);

            var paths = Adapter.ListPaths(this.Connection, path, searchOption);

            // Remember directories do not exist in this file system
            // I can't work out how to easily filter directory components within SQLite's query language, so
            // we'll just have to do it here.
            HashSet<UPath> foundDirectories = new HashSet<UPath>();
            var searchDepth = path.FullName.Count(c => c == UPath.DirectorySeparator);
            foreach (var foundPath in paths)
            {
                if (searchTarget != SearchTarget.File)
                {
                    var parent = foundPath.GetDirectory();

                    // have we seen this directory before?
                    if (!foundDirectories.Contains(parent))
                    {
                        // if not yield it and all it's parent directories
                        var fragments = parent.Split();
                        var directory = path;
                        for (int i = searchDepth; i <= fragments.Count; i++)
                        {
                            // construct a fragment directory (or if at end, it's the whole directory)
                            directory = i == searchDepth ? directory : directory / fragments[i-1];
                            bool added = foundDirectories.Add(directory);
                            if (added)
                            {
                                if (pattern.Match(directory))
                                {
                                    yield return directory;
                                }
                            }
                        }
                    }
                }

                if (searchTarget != SearchTarget.Directory)
                {
                    if (pattern.Match(foundPath))
                    {
                        yield return foundPath;
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override string ConvertPathToInternalImpl(UPath path)
        {
            return path.FullName;
        }

        /// <inheritdoc />
        protected override UPath ConvertPathFromInternalImpl(string innerPath)
        {
            return new UPath(innerPath);
        }

        private Adapter.Node SafeExists(UPath path)
        {
            return (Adapter.Node)Adapter.ExecuteScalarLong(this.Connection, Adapter.FileOrDirectoryExists, path.FullName);
        }

        private Adapter.Node EnsureExists(UPath path)
        {
            var node = this.SafeExists(path);

            if (node == Adapter.Node.NotFound)
            {
                throw NewFileNotFoundException(path);
            }

            return node;
        }

        private void EnsureFileExists(UPath path, bool notFoundForDirectory = false)
        {
            var node = this.SafeExists(path);
            switch (node)
            {
                case Adapter.Node.File:
                    return;
                case Adapter.Node.Directory:
                    if (notFoundForDirectory)
                    {
                        throw NewFileNotFoundException(path);
                    }

                    throw NewDirectoryNotFileException(path);
                case Adapter.Node.NotFound:
                    throw NewFileNotFoundException(path);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void EnsureDirectoryExists(UPath path)
        {                   
            var node = this.SafeExists(path);
            switch (node)
            {
                case Adapter.Node.File:
                    throw NewNotFileException(path);
                case Adapter.Node.Directory:
                    return;
                case Adapter.Node.NotFound:
                    throw NewDirectoryNotFoundException(path);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void EnsureDestinationNotExists(UPath path)
        {
            var node = this.SafeExists(path);
            switch (node)
            {
                case Adapter.Node.File:
                    throw NewDestinationFileExistException(path);
                case Adapter.Node.Directory:
                    throw NewDestinationDirectoryExistException(path);
                case Adapter.Node.NotFound:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }



        private (DateTime Accessed, DateTime Created, DateTime Written) GetTimeStamps(UPath path)
        {
            var fileType = this.SafeExists(path);

            (DateTime Accessed, DateTime Created, DateTime Written) result;

            switch (fileType)
            {
                case Adapter.Node.NotFound:
                    result = (DefaultFileTime, DefaultFileTime, DefaultFileTime);
                    break;
                case Adapter.Node.File:
                    result = Adapter.GetFileTimeStamps(this.Connection, path);
                    break;
                case Adapter.Node.Directory:
                    result = Adapter.GetDirectoryTimeStamps(this.Connection, path);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }
        private void SetTimeStamps(string field, UPath path, DateTime value)
        {
            var fileType = this.EnsureExists(path);

            switch (fileType)
            {
                case Adapter.Node.File:
                    this.SetCheck(false);
                    break;
                case Adapter.Node.Directory:
                    this.SetCheck(true);
                    break;
                case Adapter.Node.NotFound:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Adapter.SetTimeStamp(this.Connection, path, field, value);
        }

        private void SetCheck(bool isDirectory)
        {
            if (isDirectory && this.throwOnDirectorySet)
            {
                throw new SqliteFileSystemException("Modification of directories is not allowed - this is a flat file system");
            }

            if (this.IsReadOnly)
            {
                throw NewReadOnlyException();
            }
        }

        private void CheckNotSubDirectory(UPath srcPath, UPath destPath)
        {
            // Same directory move
            if (srcPath == destPath)
            {
                throw new IOException(
                    $"Cannot move the source directory `{srcPath}` to a a sub-folder of itself `{destPath}`");
            }

            // Check that Destination folder is not a subfolder of source directory

            var checkParentDestDirectory = destPath.GetDirectory();
            while (checkParentDestDirectory != null)
            {
                if (checkParentDestDirectory == srcPath)
                {
                    throw new IOException(
                        $"Cannot move the source directory `{srcPath}` to a a sub-folder of itself `{destPath}`");
                }

                checkParentDestDirectory = checkParentDestDirectory.GetDirectory();
            }

        }

    }
}
