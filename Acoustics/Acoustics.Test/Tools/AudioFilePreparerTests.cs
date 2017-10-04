// <copyright file="AudioFilePreparerTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Tools
{
    using System;
    using Acoustics.Shared;
    using Acoustics.Tools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AudioFilePreparerTests
    {
        [TestMethod]
        public void GetFileNameTestNullOffsets()
        {
            var actual = AudioFilePreparer.GetFileName("original.mp3", MediaTypes.MediaTypeWav, null, null, true);

            Assert.AreEqual("original_0min.wav", actual);
        }

        [TestMethod]
        public void GetFileNameTestStartOffset()
        {
            var actual = AudioFilePreparer.GetFileName("original.mp3", MediaTypes.MediaTypeWav, 3660.Seconds(), null, true);

            Assert.AreEqual("original_61min.wav", actual);
        }

        [TestMethod]
        public void GetFileNameTestNonRoundedOffset()
        {
            var actual = AudioFilePreparer.GetFileName("original.mp3", MediaTypes.MediaTypeWav, 90.Seconds(), null, true);

            Assert.AreEqual("original_1.5min.wav", actual);
        }

        [TestMethod]
        public void GetFileNameTestRealFractionRoundedOffsetCappedAtSixPlaces()
        {
            var actual = AudioFilePreparer.GetFileName("original.mp3", MediaTypes.MediaTypeWav, 67.Seconds(), null, true);

            Assert.AreEqual("original_1.116667min.wav", actual);
        }

        [TestMethod]
        public void GetFileNameTestStartAndEndOffsets()
        {
            var actual = AudioFilePreparer.GetFileName("original.mp3", MediaTypes.MediaTypeWav, 72.Seconds(), 600.Seconds(), true);

            Assert.AreEqual("original_1.2-10min.wav", actual);
        }

        [TestMethod]
        public void GetFileNameTestStartAndEndOffsetsRealFractionCappedAtSixPlaces()
        {
            var actual = AudioFilePreparer.GetFileName("original.mp3", MediaTypes.MediaTypeWav, 73.123.Seconds(), 127.Seconds(), true);

            Assert.AreEqual("original_1.218717-2.116667min.wav", actual);
        }

        [TestMethod]
        public void GetFileNameTestNullOffsetsNew()
        {
            var actual = AudioFilePreparer.GetFileName("original.mp3", MediaTypes.MediaTypeWav, null, null);

            Assert.AreEqual("original_0.wav", actual);
        }

        [TestMethod]
        public void GetFileNameTestStartOffsetNew()
        {
            var actual = AudioFilePreparer.GetFileName("original.mp3", MediaTypes.MediaTypeWav, 3660.Seconds(), null);

            Assert.AreEqual("original_3660.wav", actual);
        }

        [TestMethod]
        public void GetFileNameTestNonRoundedOffsetNew()
        {
            var actual = AudioFilePreparer.GetFileName("original.mp3", MediaTypes.MediaTypeWav, 90.Seconds(), null);

            Assert.AreEqual("original_90.wav", actual);
        }

        [TestMethod]
        public void GetFileNameTestRealFractionRoundedOffsetCappedAtThreePlacesNew()
        {
            var actual = AudioFilePreparer.GetFileName("original.mp3", MediaTypes.MediaTypeWav, 67.6666.Seconds(), null);

            Assert.AreEqual("original_67.667.wav", actual);
        }

        [TestMethod]
        public void GetFileNameTestStartAndEndOffsetsNew()
        {
            var actual = AudioFilePreparer.GetFileName("original.mp3", MediaTypes.MediaTypeWav, 72.Seconds(), 600.Seconds());

            Assert.AreEqual("original_72-600.wav", actual);
        }

        [TestMethod]
        public void GetFileNameTestStartAndEndOffsetsRealFractionCappedAtThreePlacesNew()
        {
            var actual = AudioFilePreparer.GetFileName("original.mp3", MediaTypes.MediaTypeWav, 73.12399.Seconds(), 127.33333333.Seconds());

            Assert.AreEqual("original_73.124-127.333.wav", actual);
        }
    }
}