// <copyright file="EventMetadataResolverTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.AcousticWorkbench.Orchestration
{
    using System;
    using System.Linq;
    using Acoustics.Shared;
    using global::AnalysisPrograms.AcousticWorkbench.Orchestration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers.Factories;

    [TestClass]
    public class EventMetadataResolverTests
    {
        [TestMethod]
        public void TestDedupeSegments()
        {
            var audioRecording = AudioRecordingFactory.Create();

            var segments = new RemoteSegmentWithData[]
            {
                new RemoteSegmentWithData(audioRecording, new Range<double>(0, 60), 1.AsArray<object>()),
                new RemoteSegmentWithData(audioRecording, new Range<double>(0, 60), 2.AsArray<object>()),
                new RemoteSegmentWithData(audioRecording, new Range<double>(33, 93), 3.AsArray<object>()),
                new RemoteSegmentWithData(audioRecording, new Range<double>(100.375, 160.375), 4.AsArray<object>()),
                new RemoteSegmentWithData(audioRecording, new Range<double>(100.375, 160.375), 5.AsArray<object>()),
                new RemoteSegmentWithData(audioRecording, new Range<double>(900, 960), 6.AsArray<object>()),
                new RemoteSegmentWithData(audioRecording, new Range<double>(0, 60), 7.AsArray<object>()),
            };

            var actual = EventMetadataResolver.DedupeSegments(segments);

            var expected = new RemoteSegmentWithData[]
            {
                new RemoteSegmentWithData(audioRecording, new Range<double>(0, 60), new object[] { 1, 2, 7 }),
                new RemoteSegmentWithData(audioRecording, new Range<double>(33, 93), 3.AsArray<object>()),
                new RemoteSegmentWithData(audioRecording, new Range<double>(100.375, 160.375), new object[] { 4, 5 }),
                new RemoteSegmentWithData(audioRecording, new Range<double>(900, 960), 6.AsArray<object>()),
            };

            Assert.AreEqual(expected.Length, actual.Length);
            foreach (var (expectedItem, actualItem) in expected.Zip(actual, ValueTuple.Create))
            {
                Assert.AreEqual(expectedItem.Source, actualItem.Source);
                Assert.AreEqual(expectedItem.Offsets, actualItem.Offsets);
                CollectionAssert.AreEqual(expectedItem.Data.ToArray(), actualItem.Data.ToArray());
            }
        }
    }
}
