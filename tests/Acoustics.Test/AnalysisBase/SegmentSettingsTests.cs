// <copyright file="SegmentSettingsTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisBase
{
    using System;
    using System.IO;
    using global::AnalysisBase;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    [TestClass]
    public class SegmentSettingsTests : OutputDirectoryTest
    {
        private AnalysisSettings analysisSettings;
        private SegmentSettings<FileInfo> segmentSettings;
        private FileSegment segment;
        private FileSegment preparedSegment;

        [TestInitialize]
        public void Initialize()
        {
            this.analysisSettings = new AnalysisSettings();

            this.analysisSettings.AnalysisOutputDirectory = this.outputDirectory.Combine("output");
            this.analysisSettings.AnalysisTempDirectory = this.outputDirectory.Combine("tmp");

            var fakeSource = this.outputDirectory.CombineFile("abc_min1.wav");
            fakeSource.Touch();
            this.segment = new FileSegment(fakeSource, 123456, 60.0.Seconds())
            {
                SegmentStartOffset = TimeSpan.Zero,
                SegmentEndOffset = 60.0.Seconds(),
            };

            var fakePrepared = this.outputDirectory.CombineFile("abc_min1.wav");
            fakePrepared.Touch();
            this.preparedSegment = new FileSegment(fakePrepared, 123456, 30.Seconds())
            {
                SegmentStartOffset = 0.Seconds(),
                SegmentEndOffset = 59.999.Seconds(),
            };

            this.segmentSettings = new SegmentSettings<FileInfo>(
                this.analysisSettings,
                this.segment,
                (this.analysisSettings.AnalysisOutputDirectory, this.analysisSettings.AnalysisTempDirectory),
                this.preparedSegment);
        }

        [TestMethod]
        public void ThrowsIfArgumentNull()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () =>
                {
                   this.segmentSettings = new SegmentSettings<FileInfo>(
                        null,
                        this.segment,
                        (this.analysisSettings.AnalysisOutputDirectory, this.analysisSettings.AnalysisTempDirectory),
                        this.preparedSegment);
                });
        }

        [TestMethod]
        public void ThrowsIfArgumentNull1()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () =>
                {
                   this.segmentSettings = new SegmentSettings<FileInfo>(
                        this.analysisSettings,
                        null,
                        (this.analysisSettings.AnalysisOutputDirectory, this.analysisSettings.AnalysisTempDirectory),
                        this.preparedSegment);
                });
        }

        [TestMethod]
        public void ThrowsIfArgumentNull2()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () =>
                {
                   this.segmentSettings = new SegmentSettings<FileInfo>(
                        this.analysisSettings,
                        this.segment,
                        (null, null),
                        this.preparedSegment);
                });
        }

        [TestMethod]
        public void ThrowsIfArgumentNull3()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () =>
                {
                   this.segmentSettings = new SegmentSettings<FileInfo>(
                        this.analysisSettings,
                        this.segment,
                        (this.analysisSettings.AnalysisOutputDirectory, this.analysisSettings.AnalysisTempDirectory),
                        null);
                });
        }

        [TestMethod]
        public void IdealSegmentDurationIsAutomaticallyCalculated()
        {
            Assert.AreEqual(60.0.Seconds(), this.segmentSettings.AnalysisIdealSegmentDuration);
        }

        [TestMethod]
        public void SegmentAudioFileIsProvidedForBackwardsCompatibility()
        {
            Assert.AreEqual("abc_min1.wav", this.segmentSettings.Segment.Source.Name);
            Assert.AreEqual("abc_min1.wav", this.segmentSettings.SegmentAudioFile.Name);
        }

        [TestMethod]
        public void InstanceIdProxiesAnalysisSettings()
        {
            Assert.AreEqual(this.analysisSettings.InstanceId, this.segmentSettings.InstanceId);
        }

        [TestMethod]
        public void PathPropertiesAreAlwaysDefined()
        {
            Assert.AreEqual("abc_min1__Events.csv", this.segmentSettings.SegmentEventsFile.Name);
            Assert.AreEqual("abc_min1__Indices.csv", this.segmentSettings.SegmentSummaryIndicesFile.Name);
            Assert.AreEqual("abc_min1__Image.png", this.segmentSettings.SegmentImageFile.Name);
            Assert.AreEqual(this.segmentSettings.SegmentOutputDirectory, this.segmentSettings.SegmentSpectrumIndicesDirectory);
        }
    }
}
