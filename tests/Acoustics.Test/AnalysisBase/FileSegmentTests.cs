// <copyright file="FileSegmentTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisBase
{
    using System;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisBase;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FileSegmentTests
    {
        [TestMethod]
        public void CloneCopiesAllAttributes()
        {
            string testFile = "Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3";
            var asset = PathHelper.ResolveAsset(testFile);
            var info = TestHelper.AudioDetails[testFile];

            FileSegment s = new FileSegment(asset, info.SampleRate.Value, info.Duration.Value);

            FileSegment s2 = (FileSegment)s.Clone();

            Assert.AreNotEqual(s, s2);
            Assert.AreEqual(s.Source.FullName, s2.Source.FullName);
            Assert.AreEqual(s.TargetFileSampleRate, s2.TargetFileSampleRate);
            Assert.AreEqual(s.TargetFileDuration, s2.TargetFileDuration);
            Assert.AreEqual(s.TargetFileStartDate, s2.TargetFileStartDate);
            Assert.AreEqual(s.Alignment, s2.Alignment);
            Assert.AreEqual(s.SegmentStartOffset, s2.SegmentStartOffset);
            Assert.AreEqual(s.SegmentEndOffset, s2.SegmentEndOffset);
        }

        [TestMethod]
        public void SplitActsLikeClone()
        {
            string testFile = "Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3";
            var asset = PathHelper.ResolveAsset(testFile);
            var info = TestHelper.AudioDetails[testFile];

            FileSegment s = new FileSegment(asset, info.SampleRate.Value, info.Duration.Value);

            FileSegment s2 = (FileSegment)s.SplitSegment(30, 40);

            Assert.AreNotEqual(s, s2);
            Assert.AreEqual(s.Source.FullName, s2.Source.FullName);
            Assert.AreEqual(s.TargetFileSampleRate, s2.TargetFileSampleRate);
            Assert.AreEqual(s.TargetFileDuration, s2.TargetFileDuration);
            Assert.AreEqual(s.TargetFileStartDate, s2.TargetFileStartDate);
            Assert.AreEqual(s.Alignment, s2.Alignment);

            Assert.AreNotEqual(s.SegmentStartOffset, s2.SegmentStartOffset);
            Assert.AreNotEqual(s.SegmentEndOffset, s2.SegmentEndOffset);
            Assert.AreEqual(30.Seconds(), s2.SegmentStartOffset);
            Assert.AreEqual(40.Seconds(), s2.SegmentEndOffset);
        }

        [TestMethod]
        public void SecondConstructorAutomaticallyExtractsInfo()
        {
            string testFile = "Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3";
            var asset = PathHelper.ResolveAsset(testFile);
            var info = TestHelper.AudioDetails[testFile];

            FileSegment s = new FileSegment(asset, TimeAlignment.None, null, FileSegment.FileDateBehavior.Try);

            Assert.AreEqual(null, s.SegmentStartOffset);
            Assert.AreEqual(null, s.SegmentEndOffset);
            Assert.AreEqual(0, s.StartOffsetSeconds);
            Assert.AreEqual(info.Duration.Value.TotalSeconds, s.EndOffsetSeconds, 1.0);
            Assert.That.AreEqual(info.Duration.Value, s.TargetFileDuration.Value, 0.1.Seconds());
            Assert.AreEqual(info.SampleRate, s.TargetFileSampleRate);
            Assert.AreEqual("Currawongs_curlew_West_Knoll_Bees_20091102-183000", s.SourceMetadata.Identifier);
            Assert.IsNull(s.TargetFileStartDate);
        }
    }
}
