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


    }
}
