namespace Acoustics.Test.AnalysisPrograms.SourcePreparers
{
    using System;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using AnalysisBase;
    using global::AnalysisBase;
    using global::AnalysisBase.Segment;
    using global::AnalysisPrograms.SourcePreparers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MSTestExtensions;
    using TestHelpers;

    [TestClass]
    public class LocalSourcePreparerTests : BaseTest
    {
        private readonly FileInfo sourceFile = TestHelper.GetAudioFile("4min test.mp3");
        private LocalSourcePreparer preparer;
        private AnalysisSettings settings;
        private DirectoryInfo testDirectory;

        [TestInitialize]
        public void Initialize()
        {
            this.testDirectory = PathHelper.GetTempDir();
            this.preparer = new LocalSourcePreparer();
            this.settings = new AnalysisSettings();
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.testDirectory.Delete(true);
        }

        [TestMethod]
        public void ShouldDoBasicSplits()
        {
            var source = TestHelper.AudioDetails[this.sourceFile.Name];
            var fileSegment = new FileSegment(this.sourceFile, source.SampleRate.Value, source.Duration.Value);

            var analysisSegments = this.preparer.CalculateSegments(new[] { fileSegment }, this.settings).ToArray();

            var expected = new[]
            {
                (0.0, 60.0).AsRange(),
                (60.0, 120.0).AsRange(),
                (120.0, 180.0).AsRange(),
                (180.0, 240.0).AsRange(),
                (240.0, 240.113).AsRange(),
            };

            for (int i = 0; i < analysisSegments.Length; i++)
            {
                var expectedStart = expected[i].Minimum;
                var expectedEnd = expected[i].Maximum;
                var actual = (FileSegment)analysisSegments[i];

                Assert.IsTrue(actual.IsSegmentSet);
                Assert.AreEqual(expectedStart, actual.SegmentStartOffset.Value.TotalSeconds);
                Assert.AreEqual(expectedEnd, actual.SegmentEndOffset.Value.TotalSeconds);
            }
        }

        [TestMethod]
        public void ShouldHonorLimits()
        {
            var source = TestHelper.AudioDetails[this.sourceFile.Name];
            var fileSegment = new FileSegment(this.sourceFile, source.SampleRate.Value, source.Duration.Value);
            fileSegment.SegmentStartOffset = TimeSpan.FromMinutes(1);
            fileSegment.SegmentEndOffset = TimeSpan.FromMinutes(3);

            var analysisSegments = this.preparer.CalculateSegments(new[] { fileSegment }, this.settings).ToArray();

            var expected = new[]
            {
                (60.0, 120.0).AsRange(),
                (120.0, 180.0).AsRange(),
            };

            AssertSegmentsAreEqual(analysisSegments, expected);
        }

        [TestMethod]
        public void ShouldSupportOverlap()
        {
            var source = TestHelper.AudioDetails[this.sourceFile.Name];
            var fileSegment = new FileSegment(this.sourceFile, source.SampleRate.Value, source.Duration.Value);

            this.settings.SegmentOverlapDuration = TimeSpan.FromSeconds(30);

            var analysisSegments = this.preparer.CalculateSegments(new[] { fileSegment }, this.settings).ToArray();

            var expected = new[]
            {
                (0.0, 60.0 + 30.0).AsRange(),
                (60.0, 120.0 + 30.0).AsRange(),
                (120.0, 180.0 + 30.0).AsRange(),
                (180.0, 240.0 + 0.113).AsRange(),
                (240.0, 240.113).AsRange(),
            };

            AssertSegmentsAreEqual(analysisSegments, expected);
        }

        [TestMethod]
        public void ShouldSupportMinimumSegmentFilter()
        {
            var source = TestHelper.AudioDetails[this.sourceFile.Name];
            var fileSegment = new FileSegment(this.sourceFile, source.SampleRate.Value, source.Duration.Value);

            this.preparer = new LocalSourcePreparer(filterShortSegments: true);

            var analysisSegments = this.preparer.CalculateSegments(new[] { fileSegment }, this.settings).ToArray();

            var expected = new[]
            {
                (0.0, 60.0).AsRange(),
                (60.0, 120.0).AsRange(),
                (120.0, 180.0).AsRange(),
                (180.0, 240.0).AsRange(),
            };
            AssertSegmentsAreEqual(analysisSegments, expected);
        }

        [TestMethod]
        public void AbsoluteTimeAlignmentHasNoEffectWhenOffsetIsZero()
        {
            var newFile = this.testDirectory.CombineFile("4minute test_20161006-013000Z.mp3");
            this.sourceFile.CopyTo(newFile.FullName);

            var fileSegment = new FileSegment(newFile, TimeAlignment.TrimBoth);

            var analysisSegments = this.preparer.CalculateSegments(new[] { fileSegment }, this.settings).ToArray();

            var expected = new[]
            {
                (0.0, 60.0).AsRange(),
                (60.0, 120.0).AsRange(),
                (120.0, 180.0).AsRange(),
                (180.0, 240.0).AsRange(),
                (240.0, 240.113).AsRange(),
            };

            AssertSegmentsAreEqual(analysisSegments, expected);
        }

        [TestMethod]
        public void AbsoluteTimeAlignmentFailsWithoutDate()
        {
            var source = TestHelper.AudioDetails[this.sourceFile.Name];
            Assert.Throws<InvalidFileDateException>(
                () =>
                    {
                        var fileSegment = new FileSegment(this.sourceFile, TimeAlignment.TrimBoth);
                    });

        }

        [TestMethod]
        public void ShouldSupportOffsetsAndAbsoluteTimeAlignment()
        {
            var source = TestHelper.AudioDetails[this.sourceFile.Name];

            var newFile = this.testDirectory.CombineFile("4minute test_20161006-013012Z.mp3");
            this.sourceFile.CopyTo(newFile.FullName);

            var fileSegment = new FileSegment(newFile, TimeAlignment.TrimBoth);

            fileSegment.SegmentStartOffset = TimeSpan.FromMinutes(1);
            fileSegment.SegmentEndOffset = TimeSpan.FromMinutes(3);

            var analysisSegments = this.preparer.CalculateSegments(new[] { fileSegment }, this.settings).ToArray();

            var d = 48.0;
            var expected = new[]
            {
                (60.0 + d, 120.0 + d).AsRange(),
            };

            AssertSegmentsAreEqual(analysisSegments, expected);
        }

        [TestMethod]
        public void ShouldSupportAbsoluteTimeAlignmentTrimBoth()
        {
            var source = TestHelper.AudioDetails[this.sourceFile.Name];

            var newFile = this.testDirectory.CombineFile("4minute test_20161006-013012Z.mp3");
            this.sourceFile.CopyTo(newFile.FullName);

            var fileSegment = new FileSegment(newFile, TimeAlignment.TrimBoth);

            var analysisSegments = this.preparer.CalculateSegments(new[] { fileSegment }, this.settings).ToArray();

            var d = 48.0;
            var expected = new[]
            {
                (0.0 + d, 60.0 + d).AsRange(),
                (60.0 + d, 120.0 + d).AsRange(),
                (120.0 + d, 180.0 + d).AsRange(),
            };

            AssertSegmentsAreEqual(analysisSegments, expected);
        }

        [TestMethod]
        public void ShouldSupportAbsoluteTimeAlignmentTrimNeither()
        {
            var source = TestHelper.AudioDetails[this.sourceFile.Name];

            var newFile = this.testDirectory.CombineFile("4minute test_20161006-013012Z.mp3");
            this.sourceFile.CopyTo(newFile.FullName);

            var fileSegment = new FileSegment(newFile, TimeAlignment.TrimNeither);

            var analysisSegments = this.preparer.CalculateSegments(new[] { fileSegment }, this.settings).ToArray();

            var d = 48.0;
            var expected = new[]
            {
                (0.0,  d).AsRange(),
                (0.0 + d, 60.0 + d).AsRange(),
                (60.0 + d, 120.0 + d).AsRange(),
                (120.0 + d, 180.0 + d).AsRange(),
                (180.0 + d, 240.113).AsRange(),
            };

            AssertSegmentsAreEqual(analysisSegments, expected);
        }

        [TestMethod]
        public void ShouldSupportAbsoluteTimeAlignmentTrimStart()
        {
            var source = TestHelper.AudioDetails[this.sourceFile.Name];

            var newFile = this.testDirectory.CombineFile("4minute test_20161006-013012Z.mp3");
            this.sourceFile.CopyTo(newFile.FullName);

            var fileSegment = new FileSegment(newFile, TimeAlignment.TrimStart);

            var analysisSegments = this.preparer.CalculateSegments(new[] { fileSegment }, this.settings).ToArray();

            var d = 48.0;
            var expected = new[]
            {
                (0.0 + d, 60.0 + d).AsRange(),
                (60.0 + d, 120.0 + d).AsRange(),
                (120.0 + d, 180.0 + d).AsRange(),
                (180.0 + d, 240.113).AsRange(),
            };

            AssertSegmentsAreEqual(analysisSegments, expected);
        }

        [TestMethod]
        public void ShouldSupportAbsoluteTimeAlignmentTrimEnd()
        {
            var source = TestHelper.AudioDetails[this.sourceFile.Name];

            var newFile = this.testDirectory.CombineFile("4minute test_20161006-013012Z.mp3");
            this.sourceFile.CopyTo(newFile.FullName);

            var fileSegment = new FileSegment(newFile, TimeAlignment.TrimEnd);

            var analysisSegments = this.preparer.CalculateSegments(new[] { fileSegment }, this.settings).ToArray();

            var d = 48.0;
            var expected = new[]
            {
                (0.0, d).AsRange(),
                (0.0 + d, 60.0 + d).AsRange(),
                (60.0 + d, 120.0 + d).AsRange(),
                (120.0 + d, 180.0 + d).AsRange(),
            };

            AssertSegmentsAreEqual(analysisSegments, expected);
        }

        private static void AssertSegmentsAreEqual(ISegment<FileInfo>[] acutal, Range<double>[] expected)
        {
            Assert.AreEqual(acutal.Length, expected.Length, "The number of segments in actual and expected do not match");

            for (int i = 0; i < acutal.Length; i++)
            {
                var expectedStart = expected[i].Minimum;
                var expectedEnd = expected[i].Maximum;
                var actual = (FileSegment)acutal[i];

                Assert.IsTrue(actual.IsSegmentSet);
                Assert.AreEqual(expectedStart, actual.SegmentStartOffset.Value.TotalSeconds);
                Assert.AreEqual(expectedEnd, actual.SegmentEndOffset.Value.TotalSeconds);
            }
        }
    }
}
