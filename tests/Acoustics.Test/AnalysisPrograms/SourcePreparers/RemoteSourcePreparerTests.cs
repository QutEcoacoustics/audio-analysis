namespace Acoustics.Test.AnalysisPrograms.SourcePreparers
{
    using System;
    using System.Linq;
    using Acoustics.Shared;
    using global::AcousticWorkbench;
    using global::AcousticWorkbench.Models;
    using global::AnalysisBase;
    using global::AnalysisBase.Segment;
    using global::AnalysisPrograms.AcousticWorkbench.Orchestration;
    using global::AnalysisPrograms.SourcePreparers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers.Factories;

    [TestClass]
    public class RemoteSourcePreparerTests
    {
        private RemoteSourcePreparer preparer;
        private AnalysisSettings settings;
        private AudioRecording audioRecording;
        private IAuthenticatedApi authenticatedApi;

        [TestInitialize]
        public void Initialize()
        {
            this.authenticatedApi = AuthenticatedApi.Merge(Api.Default, "dummy", "dummyToken");
            this.preparer = new RemoteSourcePreparer(this.authenticatedApi);
            this.settings = new AnalysisSettings();

            this.audioRecording = AudioRecordingFactory.Create();
            this.audioRecording.DurationSeconds = 240.113;
        }

        [TestMethod]
        public void SupportsNonSplittingMode()
        {
            this.preparer = new RemoteSourcePreparer(this.authenticatedApi, false);
            var segment = new RemoteSegment(this.audioRecording);

            Assert.ThrowsException<SegmentSplitException>(
                () =>
                {
                    this.preparer.CalculateSegments(new[] { segment }, this.settings).ToArray();
                });

            // try again, but with a segment that is small enough
            segment = new RemoteSegment(
                this.audioRecording,
                0.0.To(this.settings.AnalysisMaxSegmentDuration.Value.TotalSeconds));

            var analysisSegments = this.preparer.CalculateSegments(new[] { segment }, this.settings).ToArray();

            var expected = new[]
            {
                (0.0, 60.0).AsRange(),
            };

            AssertSegmentsAreEqual(analysisSegments, expected, this.audioRecording);
        }

        [TestMethod]
        public void ShouldDoBasicSplits()
        {
            var segment = new RemoteSegment(this.audioRecording);

            var analysisSegments = this.preparer.CalculateSegments(new[] { segment }, this.settings).ToArray();

            var expected = new[]
            {
                (0.0, 60.0).AsRange(),
                (60.0, 120.0).AsRange(),
                (120.0, 180.0).AsRange(),
                (180.0, 240.0).AsRange(),
                (240.0, 240.113).AsRange(),
            };

            AssertSegmentsAreEqual(analysisSegments, expected, this.audioRecording);
        }

        [TestMethod]
        public void ShouldHonorLimits()
        {
            var segment = new RemoteSegment(this.audioRecording, 60.0.To(180.0));

            var analysisSegments = this.preparer.CalculateSegments(new[] { segment }, this.settings).ToArray();

            var expected = new[]
            {
                (60.0, 120.0).AsRange(),
                (120.0, 180.0).AsRange(),
            };

            AssertSegmentsAreEqual(analysisSegments, expected, this.audioRecording);
        }

        [TestMethod]
        public void ShouldSupportOverlap()
        {
            var segment = new RemoteSegment(this.audioRecording);

            this.settings.SegmentOverlapDuration = TimeSpan.FromSeconds(30);

            var analysisSegments = this.preparer.CalculateSegments(new[] { segment }, this.settings).ToArray();

            var expected = new[]
            {
                (0.0, 60.0 + 30.0).AsRange(),
                (60.0, 120.0 + 30.0).AsRange(),
                (120.0, 180.0 + 30.0).AsRange(),
                (180.0, 240.0 + 0.113).AsRange(),
                (240.0, 240.113).AsRange(),
            };

            AssertSegmentsAreEqual(analysisSegments, expected, this.audioRecording);
        }

        private static void AssertSegmentsAreEqual(ISegment<AudioRecording>[] acutals, Range<double>[] expected, AudioRecording source)
        {
            for (int i = 0; i < acutals.Length; i++)
            {
                var expectedStart = expected[i].Minimum;
                var expectedEnd = expected[i].Maximum;
                var actual = acutals[i];

                Assert.AreEqual(source, actual.Source);
                Assert.AreEqual(expectedStart, actual.StartOffsetSeconds);
                Assert.AreEqual(expectedEnd, actual.EndOffsetSeconds);
            }
        }
    }
}
