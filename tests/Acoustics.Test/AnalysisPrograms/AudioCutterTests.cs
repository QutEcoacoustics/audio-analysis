// <copyright file="AudioCutterTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisPrograms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AudioCutterTests : OutputDirectoryTest
    {
        private static readonly Func<FileInfo, string> FileInfoMapper = info => info.Name;
        private readonly string testFile = PathHelper.ResolveAssetPath("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv");

        [TestMethod]
        public void ArgsAllowedValues()
        {
            var args = new[]
            {
                "AudioCutter",
                this.testFile,
                this.outputDirectory.FullName,

                // test we can allow really long segments
                "-d",
                "43200",

                // test we can allow really long overlaps
                "--segment-overlap",
                "43200",
            };

            var app = MainEntry.CreateCommandLineApplication();

            // the following lines should throw a validation exception if any of the test above are failing
            var parseResult = app.Parse(args);
            var result = parseResult.SelectedCommand.GetValidationResult();

            Assert.IsTrue(result == ValidationResult.Success);
        }

        [TestMethod]
        public void ArgsNotAllowedValues()
        {
            var args = new[]
            {
                "AudioCutter",
                this.testFile,
                this.outputDirectory.FullName,

                // must be >= 1
                "--segment-duration-minimum",
                "0",

                // must be >= 1
                "-d",
                "0",
            };

            var app = MainEntry.CreateCommandLineApplication();

            // the following lines should throw a validation exception if any of the test above are failing
            var parseResult = app.Parse(args);
            var result = parseResult.SelectedCommand.GetValidationResult();

            CollectionAssert.AreEquivalent(
                new[]
                {
                    nameof(AudioCutter.Arguments.SegmentDuration),
                    nameof(AudioCutter.Arguments.SegmentDurationMinimum),
                },
                result.MemberNames.ToArray());
        }

        [TestMethod]
        public void ArgsNotAllowedValues2()
        {
            var args = new[]
            {
                "AudioCutter",
                this.testFile,
                this.outputDirectory.FullName,

                // must be >= 0
                "--segment-overlap",
                "-1",
            };

            var app = MainEntry.CreateCommandLineApplication();

            // the following lines should throw a validation exception if any of the test above are failing
            var parseResult = app.Parse(args);
            var result = parseResult.SelectedCommand.GetValidationResult();

            CollectionAssert.AreEquivalent(
                new[]
                {
                    "--segment-overlap",
                },
                result.MemberNames.ToArray());
        }

        [TestMethod]
        public async Task TestAudioCutterSimple()
        {
            await AudioCutter.Execute(new AudioCutter.Arguments
            {
                InputFile = this.testFile,
                OutputDir = this.outputDirectory.FullName,
                Parallel = false,
            });

            this.CommonAssertions(this.outputDirectory, "wav");
        }

        [TestMethod]
        public async Task TestAudioCutterParallel()
        {
            await AudioCutter.Execute(new AudioCutter.Arguments
            {
                InputFile = this.testFile,
                OutputDir = this.outputDirectory.FullName,
                Parallel = true,
            });

            this.CommonAssertions(this.outputDirectory, "wav");
        }

        [TestMethod]
        public async Task TestAudioCutterFormat()
        {
            await AudioCutter.Execute(new AudioCutter.Arguments
            {
                InputFile = this.testFile,
                OutputDir = this.outputDirectory.FullName,
                Parallel = true,
                SegmentFileExtension = "mp3",
            });

            this.CommonAssertions(this.outputDirectory, "mp3");
        }

        [TestMethod]
        public async Task TestAudioCutterReallyLargeSegmentDuration()
        {
            await AudioCutter.Execute(new AudioCutter.Arguments
            {
                InputFile = this.testFile,
                OutputDir = this.outputDirectory.FullName,
                Parallel = true,
                SegmentDuration = 6 * 3600,
            });

            var files = this.outputDirectory.GetFiles().ToArray();
            Assert.AreEqual(1, files.Length);
            CollectionAssert.That.Contains(files, $"f969b39d-2705-42fc-992c-252a776f1af3_090705-0600_0-600.wav", FileInfoMapper);
        }

        [TestMethod]
        public async Task TestAudioCutterOffsetsAndDuration()
        {
            await AudioCutter.Execute(new AudioCutter.Arguments
            {
                InputFile = this.testFile,
                OutputDir = this.outputDirectory.FullName,
                StartOffset = 150,
                EndOffset = 300,
                SegmentDuration = 90.0,
            });

            var files = this.outputDirectory.GetFiles();
            Assert.AreEqual(2, files.Length);
            CollectionAssert.That.Contains(files, $"f969b39d-2705-42fc-992c-252a776f1af3_090705-0600_150-240.wav", FileInfoMapper);
            CollectionAssert.That.Contains(files, $"f969b39d-2705-42fc-992c-252a776f1af3_090705-0600_240-300.wav", FileInfoMapper);
        }

        [TestMethod]
        public async Task TestAudioCutterSampleRate()
        {
            await AudioCutter.Execute(new AudioCutter.Arguments
            {
                InputFile = this.testFile,
                OutputDir = this.outputDirectory.FullName,
                SampleRate = 8000,
            });

            var files = this.CommonAssertions(this.outputDirectory, "wav");
            var info = TestHelper.GetAudioUtility().Info(files[0]);
            Assert.AreEqual(8000, info.SampleRate);
        }

        [TestMethod]
        public async Task TestAudioCutterNoMixDown()
        {
            await AudioCutter.Execute(new AudioCutter.Arguments
            {
                InputFile = PathHelper.ResolveAssetPath("4channelsPureTones.wav"),
                OutputDir = this.outputDirectory.FullName,
                MixDownToMono = false,
                StartOffset = 20,
                EndOffset = 35,
                SegmentDuration = 5,
            });

            var files = this.outputDirectory.GetFiles();
            Assert.AreEqual(3, files.Length);
            CollectionAssert.That.Contains(files, $"4channelsPureTones_25-30.wav", FileInfoMapper);
            var info = TestHelper.GetAudioUtility().Info(files[0]);
            Assert.AreEqual(22050, info.SampleRate);
            Assert.AreEqual(4, info.ChannelCount);
        }

        [TestMethod]
        public async Task TestAudioCutterMixDown()
        {
            await AudioCutter.Execute(new AudioCutter.Arguments
            {
                InputFile = PathHelper.ResolveAssetPath("4channelsPureTones.wav"),
                OutputDir = this.outputDirectory.FullName,
                MixDownToMono = true,
                StartOffset = 20,
                EndOffset = 35,
                SegmentDuration = 5,
            });

            var files = this.outputDirectory.GetFiles();
            Assert.AreEqual(3, files.Length);
            CollectionAssert.That.Contains(files, $"4channelsPureTones_25-30.wav", FileInfoMapper);
            var info = TestHelper.GetAudioUtility().Info(files[0]);
            Assert.AreEqual(22050, info.SampleRate);
            Assert.AreEqual(1, info.ChannelCount);
        }

        [TestMethod]
        public async Task TestAudioCutterOverlap()
        {
            await AudioCutter.Execute(new AudioCutter.Arguments
            {
                InputFile = this.testFile,
                OutputDir = this.outputDirectory.FullName,
                SegmentOverlap = 30,
                SegmentDuration = 60 * 4,
                SegmentDurationMinimum = 130,
            });

            var files = this.outputDirectory.GetFiles();

            // the last segment should be 120 seconds long but we set SegmentDurationMinimum to 130 so it should not  be output
            Assert.AreEqual(2, files.Length);
            CollectionAssert.That.Contains(files, $"f969b39d-2705-42fc-992c-252a776f1af3_090705-0600_0-270.wav", FileInfoMapper);
            CollectionAssert.That.Contains(files, $"f969b39d-2705-42fc-992c-252a776f1af3_090705-0600_240-510.wav", FileInfoMapper);
        }

        private FileInfo[] CommonAssertions(DirectoryInfo targetDirectory, string expectedExtension)
        {
            var files = targetDirectory.GetFiles().ToArray();
            Assert.AreEqual(10, files.Length);
            CollectionAssert.That.Contains(files,  $"f969b39d-2705-42fc-992c-252a776f1af3_090705-0600_0-60.{expectedExtension}", FileInfoMapper);
            CollectionAssert.That.Contains(files,  $"f969b39d-2705-42fc-992c-252a776f1af3_090705-0600_540-600.{expectedExtension}", FileInfoMapper);
            return files;
        }
    }
}
