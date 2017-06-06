namespace Acoustics.Test.AnalysisPrograms.SourcePreparers
{
    using System;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using AnalysisBase;
    using global::AnalysisPrograms.SourcePreparers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MSTestExtensions;
    using TestHelpers;

    [TestClass]
    public class LocalSourcePreparerTests : BaseTest
    {
        private LocalSourcePreparer preparer;
        private readonly FileInfo sourceFile = TestHelper.GetAudioFile("4min test.mp3");
        private AnalysisSettings settings;
        private DirectoryInfo testDirectory;

        [TestInitialize]
        public void Initialize()
        {
            this.testDirectory = PathHelper.GetTempDir();
            this.preparer = new LocalSourcePreparer();
            this.settings = new AnalysisSettings()
            {
                SegmentTargetSampleRate = 22050,
                SegmentDuration = TimeSpan.FromSeconds(60),
            };
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
            var fileSegment = new FileSegment(this.sourceFile, source.SampleRate, source.Duration);

            var analysisSegments = this.preparer.CalculateSegments(new[] {fileSegment}, this.settings).ToArray();

            var expected = new[]
            {
                Tuple.Create(0.0, 60.0),
                Tuple.Create(60.0, 120.0),
                Tuple.Create(120.0, 180.0),
                Tuple.Create(180.0, 240.0),
                Tuple.Create(240.0, 240.113),
            };

            for (int i = 0; i < analysisSegments.Length; i++)
            {
                var expectedStart = expected[i].Item1;
                var expectedEnd = expected[i].Item2;
                var actual = analysisSegments[i];

                Assert.IsTrue(actual.IsSegmentSet);
                Assert.AreEqual(expectedStart, actual.SegmentStartOffset.Value.TotalSeconds);
                Assert.AreEqual(expectedEnd, actual.SegmentEndOffset.Value.TotalSeconds);
            }
        }


        [TestMethod]
        public void ShouldHonorLimits()
        {
            var source = TestHelper.AudioDetails[this.sourceFile.Name];
            var fileSegment = new FileSegment(this.sourceFile, source.SampleRate, source.Duration);
            fileSegment.SegmentStartOffset = TimeSpan.FromMinutes(1);
            fileSegment.SegmentEndOffset = TimeSpan.FromMinutes(3);

            var analysisSegments = this.preparer.CalculateSegments(new[] { fileSegment }, this.settings).ToArray();

            var expected = new[]
            {
                Tuple.Create(60.0, 120.0),
                Tuple.Create(120.0, 180.0),
            };

            AssertSegmentsAreEqual(analysisSegments, expected);
        }


        [TestMethod]
        public void ShouldSupportOverlap()
        {
            var source = TestHelper.AudioDetails[this.sourceFile.Name];
            var fileSegment = new FileSegment(this.sourceFile, source.SampleRate, source.Duration);

            this.settings.SegmentOverlapDuration = TimeSpan.FromSeconds(30);

            var analysisSegments = this.preparer.CalculateSegments(new[] { fileSegment }, this.settings).ToArray();

            var expected = new[]
            {
                Tuple.Create(0.0, 60.0 + 30.0),
                Tuple.Create(60.0, 120.0 + 30.0),
                Tuple.Create(120.0, 180.0 + 30.0),
                Tuple.Create(180.0, 240.0 + 0.113),
                Tuple.Create(240.0, 240.113),
            };

            AssertSegmentsAreEqual(analysisSegments, expected);
        }


        [TestMethod]
        public void AbsoluteTimeAlignmentHasNoEffectWhenOffsetIsZero()
        {
            var source = TestHelper.AudioDetails[this.sourceFile.Name];

            var newFile = this.testDirectory.CombineFile("4minute test_20161006-013000Z.mp3");
            this.sourceFile.CopyTo(newFile.FullName);

            var fileSegment = new FileSegment(newFile, TimeAlignment.TrimBoth);

            var analysisSegments = this.preparer.CalculateSegments(new[] { fileSegment }, this.settings).ToArray();

            var expected = new[]
            {
                Tuple.Create(0.0, 60.0),
                Tuple.Create(60.0, 120.0),
                Tuple.Create(120.0, 180.0),
                Tuple.Create(180.0, 240.0),
                Tuple.Create(240.0, 240.113),
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
                Tuple.Create(60.0 + d, 120.0 + d),
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
                Tuple.Create(0.0 + d, 60.0 + d),
                Tuple.Create(60.0 + d, 120.0 + d),
                Tuple.Create(120.0 + d, 180.0 + d),
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
                Tuple.Create(0.0,  d),
                Tuple.Create(0.0 + d, 60.0 + d),
                Tuple.Create(60.0 + d, 120.0 + d),
                Tuple.Create(120.0 + d, 180.0 + d),
                Tuple.Create(180.0 + d, 240.113),
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
                Tuple.Create(0.0 + d, 60.0 + d),
                Tuple.Create(60.0 + d, 120.0 + d),
                Tuple.Create(120.0 + d, 180.0 + d),
                Tuple.Create(180.0 + d, 240.113),
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
                Tuple.Create(0.0, d),
                Tuple.Create(0.0 + d, 60.0 + d),
                Tuple.Create(60.0 + d, 120.0 + d),
                Tuple.Create(120.0 + d, 180.0 + d),
            };

            AssertSegmentsAreEqual(analysisSegments, expected);
        }


        private static void AssertSegmentsAreEqual(FileSegment[] acutal, Tuple<double, double>[] expected)
        {
            for (int i = 0; i < acutal.Length; i++)
            {
                var expectedStart = expected[i].Item1;
                var expectedEnd = expected[i].Item2;
                var actual = acutal[i];

                Assert.IsTrue(actual.IsSegmentSet);
                Assert.AreEqual(expectedStart, actual.SegmentStartOffset.Value.TotalSeconds);
                Assert.AreEqual(expectedEnd, actual.SegmentEndOffset.Value.TotalSeconds);
            }
        }
    }
}
