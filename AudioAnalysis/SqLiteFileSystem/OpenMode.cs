namespace Zio.FileSystems.Community.SqliteFileSystem
{
    using Microsoft.Data.Sqlite;

    /// <summary>
    /// A loose copy of SqliteOpenMode so that we do not introduce a leaky abstraction to consumers of
    /// SqliteFileSystem
    /// </summary>
    public enum OpenMode
    {
        //
        // Summary:
        //     Opens the database for reading and writing, and creates it if it doesn't exist.
        ReadWriteCreate = SqliteOpenMode.ReadWriteCreate,
        //
        // Summary:
        //     Opens the database for reading and writing.
        ReadWrite = SqliteOpenMode.ReadWrite,
        //
        // Summary:
        //     Opens the database in read-only mode.
        ReadOnly = SqliteOpenMode.ReadOnly,
        //
        // Summary:
        //     Opens an in-memory database.
        Memory = SqliteOpenMode.Memory,
    }
}
