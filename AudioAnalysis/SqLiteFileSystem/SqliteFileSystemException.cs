// ReSharper disable once CheckNamespace
namespace Zio.FileSystems.Additional
{
    using System;

    public class SqliteFileSystemException : Exception
    {
        public SqliteFileSystemException(string message) : base(message)
        {
        }

        public SqliteFileSystemException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}