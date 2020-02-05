// <copyright file="NonClosingStreamWrapper.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared
{
    using System;
    using System.IO;
    using System.Runtime.Remoting;

    /// <summary>
    /// Wraps a stream for all operations except Close and Dispose, which
    /// merely flush the stream and prevent further operations from being
    /// carried out using this wrapper.
    /// </summary>
    public sealed class NonClosingStreamWrapper : Stream
    {
        /// <summary>
        /// Creates a new instance of the class, wrapping the specified stream.
        /// </summary>
        /// <param name="stream">The stream to wrap. Must not be null.</param>
        /// <exception cref="ArgumentNullException">stream is null</exception>
        public NonClosingStreamWrapper(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            this.BaseStream = stream;
        }

        /// <summary>
        /// Gets stream wrapped by this wrapper
        /// </summary>
        public Stream BaseStream { get; }

        /// <summary>
        /// Whether this stream has been closed or not
        /// </summary>
        private bool closed = false;

        /// <summary>
        /// Throws an InvalidOperationException if the wrapper is closed.
        /// </summary>
        private void CheckClosed()
        {
            if (this.closed)
            {
                throw new InvalidOperationException("Wrapper has been closed or disposed");
            }
        }

        /// <summary>
        /// Begins an asynchronous read operation.
        /// </summary>
        /// <param name="buffer">The buffer to read the data into. </param>
        /// <param name="offset">
        /// The byte offset in buffer at which to begin writing data read from the stream.
        /// </param>
        /// <param name="count">The maximum number of bytes to read. </param>
        /// <param name="callback">
        /// An optional asynchronous callback, to be called when the read is complete.
        /// </param>
        /// <param name="state">
        /// A user-provided object that distinguishes this particular
        /// asynchronous read request from other requests.
        /// </param>
        /// <returns>
        /// An IAsyncResult that represents the asynchronous read,
        /// which could still be pending.
        /// </returns>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count,
                                               AsyncCallback callback, object state)
        {
            this.CheckClosed();
            return this.BaseStream.BeginRead(buffer, offset, count, callback, state);
        }

        /// <summary>
        /// Begins an asynchronous write operation.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The byte offset in buffer from which to begin writing.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="callback">
        /// An optional asynchronous callback, to be called when the write is complete.
        /// </param>
        /// <param name="state">
        /// A user-provided object that distinguishes this particular asynchronous
        /// write request from other requests.
        /// </param>
        /// <returns>
        /// An IAsyncResult that represents the asynchronous write,
        /// which could still be pending.
        /// </returns>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count,
                                                AsyncCallback callback, object state)
        {
            this.CheckClosed();
            return this.BaseStream.BeginWrite(buffer, offset, count, callback, state);
        }

        /// <summary>
        /// Gets a value indicating whether indicates whether or not the underlying stream can be read from.
        /// </summary>
        public override bool CanRead => this.closed ? false : this.BaseStream.CanRead;

        /// <summary>
        /// Gets a value indicating whether indicates whether or not the underlying stream supports seeking.
        /// </summary>
        public override bool CanSeek => this.closed ? false : this.BaseStream.CanSeek;

        /// <summary>
        /// Gets a value indicating whether indicates whether or not the underlying stream can be written to.
        /// </summary>
        public override bool CanWrite => this.closed ? false : this.BaseStream.CanWrite;

        /// <summary>
        /// This method is not proxied to the underlying stream; instead, the wrapper
        /// is marked as unusable for other (non-close/Dispose) operations. The underlying
        /// stream is flushed if the wrapper wasn't closed before this call.
        /// </summary>
        public override void Close()
        {
            if (!this.closed)
            {
                this.BaseStream.Flush();
            }

            this.closed = true;
        }

        /// <summary>
        /// Waits for the pending asynchronous read to complete.
        /// </summary>
        /// <param name="asyncResult">
        /// The reference to the pending asynchronous request to finish.
        /// </param>
        /// <returns>
        /// The number of bytes read from the stream, between zero (0)
        /// and the number of bytes you requested. Streams only return
        /// zero (0) at the end of the stream, otherwise, they should
        /// block until at least one byte is available.
        /// </returns>
        public override int EndRead(IAsyncResult asyncResult)
        {
            this.CheckClosed();
            return this.BaseStream.EndRead(asyncResult);
        }

        /// <summary>
        /// Ends an asynchronous write operation.
        /// </summary>
        /// <param name="asyncResult">A reference to the outstanding asynchronous I/O request.</param>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.CheckClosed();
            this.BaseStream.EndWrite(asyncResult);
        }

        /// <summary>
        /// Flushes the underlying stream.
        /// </summary>
        public override void Flush()
        {
            this.CheckClosed();
            this.BaseStream.Flush();
        }

        /// <summary>
        /// Throws a NotSupportedException.
        /// </summary>
        /// <returns>n/a</returns>
        public override object InitializeLifetimeService()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets returns the length of the underlying stream.
        /// </summary>
        public override long Length
        {
            get
            {
                this.CheckClosed();
                return this.BaseStream.Length;
            }
        }

        /// <summary>
        /// Gets or sets the current position in the underlying stream.
        /// </summary>
        public override long Position
        {
            get
            {
                this.CheckClosed();
                return this.BaseStream.Position;
            }

            set
            {
                this.CheckClosed();
                this.BaseStream.Position = value;
            }
        }

        /// <summary>
        /// Reads a sequence of bytes from the underlying stream and advances the
        /// position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes. When this method returns, the buffer contains
        /// the specified byte array with the values between offset and
        /// (offset + count- 1) replaced by the bytes read from the underlying source.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in buffer at which to begin storing the data
        /// read from the underlying stream.
        /// </param>
        /// <param name="count">
        /// The maximum number of bytes to be read from the
        /// underlying stream.
        /// </param>
        /// <returns>The total number of bytes read into the buffer.
        /// This can be less than the number of bytes requested if that many
        /// bytes are not currently available, or zero (0) if the end of the
        /// stream has been reached.
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            this.CheckClosed();
            return this.BaseStream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Reads a byte from the stream and advances the position within the
        /// stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream.</returns>
        public override int ReadByte()
        {
            this.CheckClosed();
            return this.BaseStream.ReadByte();
        }

        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">
        /// A value of type SeekOrigin indicating the reference
        /// point used to obtain the new position.
        /// </param>
        /// <returns>The new position within the underlying stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            this.CheckClosed();
            return this.BaseStream.Seek(offset, origin);
        }

        /// <summary>
        /// Sets the length of the underlying stream.
        /// </summary>
        /// <param name="value">The desired length of the underlying stream in bytes.</param>
        public override void SetLength(long value)
        {
            this.CheckClosed();
            this.BaseStream.SetLength(value);
        }

        /// <summary>
        /// Writes a sequence of bytes to the underlying stream and advances
        /// the current position within the stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes. This method copies count bytes
        /// from buffer to the underlying stream.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in buffer at
        /// which to begin copying bytes to the underlying stream.
        /// </param>
        /// <param name="count">The number of bytes to be written to the underlying stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            this.CheckClosed();
            this.BaseStream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Writes a byte to the current position in the stream and
        /// advances the position within the stream by one byte.
        /// </summary>
        /// <param name="value">The byte to write to the stream. </param>
        public override void WriteByte(byte value)
        {
            this.CheckClosed();
            this.BaseStream.WriteByte(value);
        }
    }
}
