// <copyright file="RemoteSegmentTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.AcousticWorkbench.Orchestration.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Test.TestHelpers.Factories;
    using global::AcousticWorkbench.Models;

    using global::AnalysisPrograms.AcousticWorkbench.Orchestration;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Orchestration;

    [TestClass]
    public class RemoteSegmentTests
    {
        private RemoteSegment actualD;
        private RemoteSegment actualC;
        private RemoteSegment actualB;
        private RemoteSegment actualA;
        private AudioRecording recordingA;
        private AudioRecording recordingB;
        private AudioRecording recordingC;

        [TestInitialize]
        public void TestInitialize()
        {
            this.recordingA = AudioRecordingFactory.Create(123);
            this.recordingB = AudioRecordingFactory.Create(123);
            this.recordingC = AudioRecordingFactory.Create(321);
            var offsets = 0.0.To(60);

            this.actualA = new RemoteSegment(this.recordingA, offsets);
            this.actualB = new RemoteSegment(this.recordingB, offsets);
            this.actualC = new RemoteSegment(this.recordingC, offsets);
            this.actualD = new RemoteSegment(this.recordingC, 60.0.To(120.0));
        }

        [TestMethod]
        public void RemoteSegmentTest()
        {
            var recording = AudioRecordingFactory.Create();
            var offsets = 0.0.To(60);

            var actual = new RemoteSegment(recording, offsets);

            Assert.AreEqual(recording, actual.Source);
            Assert.AreEqual(offsets, actual.Offsets);
            Assert.AreEqual(0.0, actual.StartOffsetSeconds);
            Assert.AreEqual(60.0, actual.EndOffsetSeconds);
            Assert.AreEqual(recording.DurationSeconds, actual.SourceMetadata.Duration.TotalSeconds);
            Assert.AreEqual(recording.SampleRateHertz, actual.SourceMetadata.SampleRate);
            Assert.AreEqual(recording.Uuid, actual.SourceMetadata.Identifier);
            Assert.AreEqual(recording.RecordedDate, actual.SourceMetadata.RecordedDate);
        }

        [TestMethod]
        public void RemoteSegmentTestNoOffsetsProvided()
        {
            var recording = AudioRecordingFactory.Create();

            var actual = new RemoteSegment(recording);

            Assert.AreEqual(recording, actual.Source);
            Assert.AreEqual(0.0.To(recording.DurationSeconds), actual.Offsets);
            Assert.AreEqual(0.0, actual.StartOffsetSeconds);
            Assert.AreEqual(recording.DurationSeconds, actual.EndOffsetSeconds);
            Assert.AreEqual(recording.DurationSeconds, actual.SourceMetadata.Duration.TotalSeconds);
            Assert.AreEqual(recording.SampleRateHertz, actual.SourceMetadata.SampleRate);
            Assert.AreEqual(recording.Uuid, actual.SourceMetadata.Identifier);
            Assert.AreEqual(recording.RecordedDate, actual.SourceMetadata.RecordedDate);
        }

        [TestMethod]
        public void EqualsTest()
        {
            Assert.AreNotEqual(this.recordingA, this.recordingB);
            Assert.AreEqual(this.recordingA.Uuid, this.recordingB.Uuid);

            Assert.AreEqual(this.actualA, this.actualB);
            Assert.AreNotEqual(this.actualA, this.actualC);
            Assert.AreNotEqual(this.actualC, this.actualD);
        }

        [TestMethod]
        public void GetHashCodeTest()
        {
            Assert.AreNotEqual(this.recordingA, this.recordingB);
            Assert.AreEqual(this.recordingA.Uuid, this.recordingB.Uuid);

            Assert.AreEqual(this.actualA.GetHashCode(), this.actualB.GetHashCode());
            Assert.AreNotEqual(this.actualA.GetHashCode(), this.actualC.GetHashCode());
            Assert.AreNotEqual(this.actualC.GetHashCode(), this.actualD.GetHashCode());
        }

        [TestMethod]
        public void SplitSegmentTest()
        {
            var split = this.actualD.SplitSegment(80, 100);

            Assert.AreEqual(this.actualD.Source, split.Source);
            Assert.AreEqual(80, split.StartOffsetSeconds);
            Assert.AreEqual(100, split.EndOffsetSeconds);
        }
    }
}