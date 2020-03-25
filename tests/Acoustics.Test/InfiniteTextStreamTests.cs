// <copyright file="InfiniteTextStreamTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    [TestClass]
    [DoNotParallelize]
    public class InfiniteTextStreamTests
    {
        private readonly TimeSpan timeout = TimeSpan.FromMilliseconds(1000);

        [TestMethod]
        [Timeout(5_000)]

        public void InfiniteStreamIsInfinite()
        {
            Trace.WriteLine("Generating output");

            var builder = new StringBuilder(1_000_000);
            var source = new CancellationTokenSource();
            var token = source.Token;

            void Generate()
            {
                using (var reader = new InfiniteTextStream(random: TestHelpers.Random.GetRandom()))
                {
                    while (true)
                    {
                        char[] chars = new char[1000];
                        for (int i = 0; i < 1000; i++)
                        {
                            chars[i] = (char)reader.Read();
                        }

                        builder.Append(chars);

                        if (token.IsCancellationRequested)
                        {
                            return;
                        }
                    }
                }
            }

            var work = Task.Run((Action)Generate, token);

            Assert.IsFalse(work.IsCompleted);
            source.CancelAfter(this.timeout);
            work.Wait();
            Assert.IsTrue(work.IsCompleted);

            string s = builder.ToString();
            Trace.WriteLine($"Generation complete (length: {s.Length}):");

            Assert.IsTrue(s.Length > 1_000, $"Length {s.Length} was not greater than expected length of 1000");

            //Trace.WriteLine(s);
        }

        [TestMethod]
        [Timeout(5_000)]
        public void InfiniteStreamCanReadLines()
        {
            Trace.WriteLine("Generating output");

            StringBuilder builder = new StringBuilder(1_000_000);
            var source = new CancellationTokenSource();
            var token = source.Token;

            void Generate()
            {
                using (var reader = new InfiniteTextStream(1000, random: TestHelpers.Random.GetRandom()))
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }

                        builder.Append(reader.ReadLine());
                    }
                }
            }

            var work = Task.Run((Action)Generate, token);

            Assert.IsFalse(work.IsCompleted);
            source.CancelAfter(this.timeout);
            work.Wait();
            Assert.IsTrue(work.IsCompleted);

            string s = builder.ToString();
            Trace.WriteLine($"Generation complete (length: {s.Length}:");

            Assert.IsTrue(s.Length > 10_000);

            //Trace.WriteLine(s);
        }
    }
}