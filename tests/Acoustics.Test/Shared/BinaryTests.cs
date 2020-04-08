// <copyright file="BinaryTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared
{
    using System;
    using System.IO;
    using Acoustics.Shared;
    using Acoustics.Test.TestHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BinaryTests
    {
        private DirectoryInfo outputDirectory;

        [TestInitialize]
        public void Setup()
        {
            this.outputDirectory = PathHelper.GetTempDir();
        }

        [TestCleanup]
        public void Cleanup()
        {
            PathHelper.DeleteTempDir(this.outputDirectory);
        }

        [TestMethod]
        public void TestBinarySerializationRoundTrip()
        {
            var random = TestHelpers.Random.GetRandom();

            var numbers = new double[100];
            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = random.NextDouble();
            }

            // serialize to temporay file
            var tempFile = this.outputDirectory.CombineFile("dump.bin");
            Binary.Serialize(tempFile, numbers);

            // deserialize
            var actual = Binary.Deserialize<double[]>(tempFile);

            // test
            CollectionAssert.AreEqual(numbers, actual);
        }
    }
}