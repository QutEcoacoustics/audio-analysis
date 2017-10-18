// This class was adapted from https://github.com/xoofx/zio/blob/06e59868adaacd3fc9d174c992009a6a2520f659/src/Zio/FileSystems/MemoryFileSystem.cs
// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

namespace Zio.FileSystems.Community.SqliteFileSystem
{
    using System;
    using System.Data.Common;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using Microsoft.Data.Sqlite;

    /// <inheritdoc />
    /// <summary>
    /// An implementation of a memory stream that flushes it's buffer to a database.
    /// This is a mock stream that makes up for the lack of streaming support in the current sqlite library.
    /// </summary>
    internal class DatabaseBackedMemoryStream : MemoryStream
    {
        /// <summary>
        /// This size is chosen as a rather large default guess at the size of a file.
        /// When using this class we prefer intially large memory outlay with limited buffer growth phases (which 
        /// occur in multiples of 2 from this base)
        /// </summary>
        public const int DefaultCapacity = 25_000;

        private readonly SqliteConnection connection;
        private readonly UPath path;

        private readonly bool canRead;
        private readonly bool canWrite;
        private int isDisposed;
        private bool shouldFlush;

        public DatabaseBackedMemoryStream(SqliteConnection connection, UPath path, bool canRead, bool canWrite)
            : base(DefaultCapacity)
        {
            this.connection = connection;
            this.path = path;

            // temporaily allow writing so we can read the blob into the memory stream
            this.canWrite = true;

            // read in the blob
            var blob = Adapter.GetBlob(connection, path);
            
            // write the blob to our in-memory backing store
            base.Write(blob, 0, blob.Length);
            base.Position = 0;
            this.shouldFlush = false;
            this.canWrite = canWrite;
            this.canRead = canRead;
        }

        public override bool CanRead => this.isDisposed == 0 && this.canRead;

        public override bool CanSeek => this.isDisposed == 0;

        public override bool CanWrite => this.isDisposed == 0 && this.canWrite;

        ~DatabaseBackedMemoryStream()
        {
            this.Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Flush();
            }

            this.isDisposed = 1;

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        /// <summary>
        /// Important: flush will write the full stream to the database each time!
        /// This occurs because we are only mocking a streamable interface!
        /// It also simplifies the semantics around SetLength
        /// </summary>
        public override void Flush()
        {
            this.CheckNotDisposed();

            // don't write an update unless it is needed
            if (this.shouldFlush)
            {
                // inefficient operation ToArray creates a copy of the array
                Adapter.SetBlob(this.connection, this.path, base.ToArray());
                this.shouldFlush = false;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.CheckNotDisposed();
            if (!this.canRead)
            {
                throw new NotSupportedException("Stream does not support reading");
            }

            return base.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            this.CheckNotDisposed();
            return base.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.CheckNotDisposed();
            if (!this.canWrite)
            {
                throw new NotSupportedException("Stream does not support writing");
            }

            base.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.CheckNotDisposed();
            if (!this.canWrite)
            {
                throw new NotSupportedException("Stream does not support writing");
            }

            base.Write(buffer, offset, count);
            this.shouldFlush = true;
        }

        private void CheckNotDisposed()
        {
            if (this.isDisposed > 0)
            {
                throw new ObjectDisposedException("Cannot access a closed file.");
            }
        }
    }


}