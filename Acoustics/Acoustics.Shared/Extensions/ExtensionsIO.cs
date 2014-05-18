namespace System.IO
{
    using System;

    public static class ExtensionsIO
    {
        /// <summary>
        /// Copy from <paramref name="source"/> Stream to <paramref name="destination"/> Stream.
        /// Only needed before .Net 4.0.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="bufferSize">
        /// The buffer size.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// http://stackoverflow.com/questions/1933742/how-is-the-stream-copytostream-method-implemented-in-net-4
        /// </remarks>
        public static void CopyToStream(this Stream source, Stream destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }

            if (!source.CanRead)
            {
                throw new ArgumentException("Must be able to read from stream.", "source");
            }

            if (!destination.CanWrite)
            {
                throw new ArgumentException("Must be able to write to stream.", "destination");
            }

            int num;
            byte[] buffer = new byte[4096];
            while ((num = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, num);
            }
        }

        /// <summary>
        /// Reads data from a stream until the end is reached. The
        /// data is returned as a byte array. An IOException is
        /// thrown if any of the underlying IO calls fail.
        /// </summary>
        /// <param name="stream">
        /// The stream to read data from.
        /// </param>
        /// <param name="initialLength">
        /// The initial buffer length.
        /// </param>
        /// <remarks>
        /// from: http://www.yoda.arachsys.com/csharp/readbinary.html.
        /// </remarks>
        /// <returns>
        /// The read fully.
        /// </returns>
        public static byte[] ReadFully(this Stream stream, int initialLength)
        {
            // If we've been passed an unhelpful initial length, just
            // use 32K.
            if (initialLength < 1)
            {
                initialLength = 32768;
            }

            byte[] buffer = new byte[initialLength];
            int read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();

                    // End of stream? If so, we're done
                    if (nextByte == -1)
                    {
                        return buffer;
                    }

                    // Nope. Resize the buffer, put in the byte we've just
                    // read, and continue
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            // Buffer is now too big. Shrink it.
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }
    }
}
