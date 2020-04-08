// <copyright file="InfiniteTextStream.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.IO;
    using System.Threading;

    public class InfiniteTextStream : TextReader
    {
        private const int Min = ' ';
        private const int Max = '~';
        private const int Difference = Max - Min;
        private readonly ulong lineLength;
        private readonly int delay;
        private readonly char[] newLine;
        private readonly System.Random random;
        private int next;
        private ulong nextPosition;

        public InfiniteTextStream(ulong lineLength = 100_000_000, System.Random random = null, int delay = 1000)
        {
            this.lineLength = lineLength;
            this.delay = delay;
            this.random = random ?? new System.Random();

            this.next = Min + this.random.Next(Difference);
            this.nextPosition = 1;

            this.newLine = Environment.NewLine.ToCharArray();
            if (this.newLine.Length > 2)
            {
                throw new NotSupportedException("This code does not work on platforms that have a line length more than two chars long");
            }
        }

        public override int Peek()
        {
            return this.next;
        }

        /// <summary>
        /// Returns the next character from the "stream".
        /// Never returns -1 (which would denote the end of the stream).
        /// </summary>
        /// <returns>A character as an int.</returns>
        public override int Read()
        {
            var read = this.next;

            // generate the next character in advance so peek has something to peek at
            this.nextPosition++;
            if (this.nextPosition % this.lineLength == 0)
            {
                // if time for a new line, insert CR or LF
                this.next = this.newLine[0];
            }
            else if (this.newLine.Length == 2 && this.nextPosition > 2 && this.nextPosition % this.lineLength == 1)
            {
                // if windows, insert the LF afterwards
                this.next = this.newLine[1];
            }
            else
            {
                this.next = Min + this.random.Next(Difference);
            }

            // This implementation is far too fast (it generates gigabytes of text in 200 milliseconds).
            // So we artificially slow it down.
            if (this.delay > 0)
            {
                Thread.SpinWait(this.delay);
            }

            return read;
        }
    }
}