// <copyright file="AudioUtilityFfmpegPcmRawTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Tools
{
    using System;
    using System.IO;
    using Acoustics.Shared;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    /// <summary>
    /// The audio utility wavpack tests.
    /// Segments wavpack (.wv) to wave (.wav).
    /// </summary>
    [TestClass]
    public class AudioUtilityFfmpegPcmRawTests
    {
        private readonly FileInfo source = TestHelper.GetAudioFile("4channelsPureTones.raw");
        private FileInfo output;

        [TestInitialize]
        public void BeforeTest()
        {
            this.output = PathHelper.GetTempFile(MediaTypes.ExtWav);
        }

        [TestCleanup]
        public void AfterTest()
        {
            File.Delete(this.output.FullName);
        }

        [DataTestMethod]
        [DataRow(20.0, 50.0, false, 4, 2_822_000)]
        [DataRow(20.0, 50.0, true, 1, 2_822_000 / 4)]
        [DataRow(00.0, 60.0, true, 1, 2_822_000 / 4)]
        [DataRow(27.0, 30.5, false, 4, 2_822_000)]
        [DataRow(null, null, false, 4, 2_822_000)]
        public void SegmentsRawPcmCorrectly(object startWrapped, object endWrapped, bool mixDown, int expectedChannels, int expectedBitRate)
        {
            double? start = (double?)startWrapped;
            double? end = (double?)endWrapped;

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds((end ?? 60.0) - (start ?? 0.0)),
                SampleRate = 44100,
                ChannelCount = expectedChannels,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = expectedBitRate,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = mixDown,
                OffsetStart = start?.Seconds(),
                OffsetEnd = end?.Seconds(),
                BitDepth = 16,
                TargetSampleRate = 44100,
                Channels = new[] { 1, 2, 3, 4 },
            };

            TestHelper
                .GetAudioUtilityFfmpegRawPcm()
                .Modify(
                    this.source,
                    MediaTypes.GetMediaType(this.source.Extension),
                    this.output,
                    MediaTypes.GetMediaType(this.output.Extension),
                    request);

            var actual = TestHelper.GetAudioUtility().Info(this.output);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [DataTestMethod]
        [DataRow(20.0, 50.0, false, 4, 2_822_000)]
        [DataRow(20.0, 50.0, true, 1, 2_822_000 / 4)]
        [DataRow(00.0, 60.0, true, 1, 2_822_000 / 4)]
        [DataRow(27.0, 30.5, false, 4, 2_822_000)]
        [DataRow(null, null, false, 4, 2_822_000)]
        public void SegmentsRawPcmCorrectlyMaster(object startWrapped, object endWrapped, bool mixDown, int expectedChannels, int expectedBitRate)
        {
            double? start = (double?)startWrapped;
            double? end = (double?)endWrapped;

            var duration = TimeSpan.FromSeconds((end ?? 60.0) - (start ?? 0.0));

            var expected = new AudioUtilityInfo
            {
                Duration = duration,
                SampleRate = 44100,
                ChannelCount = expectedChannels,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = expectedBitRate,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = mixDown,
                OffsetStart = start?.Seconds(),
                OffsetEnd = end?.Seconds(),
                BitDepth = 16,
                TargetSampleRate = 44100,
                Channels = new[] { 1, 2, 3, 4 },
            };

            TestHelper
                .GetAudioUtility()
                .Modify(
                    this.source,
                    MediaTypes.GetMediaType(this.source.Extension),
                    this.output,
                    MediaTypes.GetMediaType(this.output.Extension),
                    request);

            var actual = TestHelper.GetAudioUtility().Info(this.output);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void FailsWithNullRequest()
        {
            var tester = new FfmpegRawPcmAudioUtilityTester();

            TestHelper.ExceptionMatches<ArgumentNullException>(
                () =>
                {
                    tester.InvokeCheckRequestValid(
                        this.source,
                        MediaTypes.GetMediaType(this.source.Extension),
                        this.output,
                        MediaTypes.GetMediaType(this.output.Extension),
                        null);
                },
                "raw PCM data requires prior knowledge");
        }

        [TestMethod]
        public void FailsWithMissingBitDepth()
        {
            var request = new AudioUtilityRequest
            {
                //BitDepth = 16,
                TargetSampleRate = 22050,
                Channels = new[] { 1, 2, 3, 4 },
                //BandPass
            };

            var tester = new FfmpegRawPcmAudioUtilityTester();

            TestHelper.ExceptionMatches<InvalidOperationException>(
                () =>
                {
                    tester.InvokeCheckRequestValid(
                        this.source,
                        MediaTypes.GetMediaType(this.source.Extension),
                        this.output,
                        MediaTypes.GetMediaType(this.output.Extension),
                        request);
                },
                "A BitDepth must be supplied");
        }

        [TestMethod]
        public void FailsWithInvalidBitDepth()
        {
            var request = new AudioUtilityRequest
            {
                BitDepth = 64,
                TargetSampleRate = 22050,
                Channels = new[] { 1, 2, 3, 4 },
                //BandPass
            };

            var tester = new FfmpegRawPcmAudioUtilityTester();

            TestHelper.ExceptionMatches<BitDepthOperationNotImplemented>(
                () =>
                {
                  tester.InvokeCheckRequestValid(
                        this.source,
                        MediaTypes.GetMediaType(this.source.Extension),
                        this.output,
                        MediaTypes.GetMediaType(this.output.Extension),
                        request);
                },
                "Supplied bit depth of 64");
        }

        [TestMethod]
        public void FailsWithMissingSampleRate()
        {
            var request = new AudioUtilityRequest
            {
                BitDepth = 16,
                //TargetSampleRate = 22050,
                Channels = new[] { 1, 2, 3, 4 },
                //BandPass
            };

            TestHelper.ExceptionMatches<InvalidOperationException>(
                () => { this.RunUtility(request); },
                "A TargetSampleRate must be supplied");
        }

        [TestMethod]
        public void FailsWithMissingChannels()
        {
            var request = new AudioUtilityRequest
            {
                BitDepth = 16,
                TargetSampleRate = 22050,
                //Channels = new[] { 1, 2, 3, 4 },
                //BandPass
            };

            TestHelper.ExceptionMatches<InvalidOperationException>(
                () => { this.RunUtility(request); },
                "The Channels must be set");
        }

        [TestMethod]
        public void FailsChannelSelection()
        {
            var request = new AudioUtilityRequest
            {
                BitDepth = 16,
                TargetSampleRate = 22050,
                Channels = new[] { 5, 2, 3, 4 },
                //BandPass
            };

            TestHelper.ExceptionMatches<ChannelSelectionOperationNotImplemented>(
                () => { this.RunUtility(request); },
                "The Channels specifier must contain channel numbers");
        }

        [TestMethod]
        public void FailsWithBandpassSet()
        {
            var request = new AudioUtilityRequest
            {
                BitDepth = 16,
                TargetSampleRate = 22050,
                Channels = new[] { 1, 2, 3, 4 },
                BandPassType = BandPassType.Bandpass,
            };

            TestHelper.ExceptionMatches<NotSupportedException>(
                () => { this.RunUtility(request); },
                "Bandpass operations are not supported");
        }

        private void RunUtility(AudioUtilityRequest request)
        {
            TestHelper
                .GetAudioUtilityFfmpegRawPcm()
                .Modify(
                    this.source,
                    MediaTypes.GetMediaType(this.source.Extension),
                    this.output,
                    MediaTypes.GetMediaType(this.output.Extension),
                    request);
        }

        internal class FfmpegRawPcmAudioUtilityTester : FfmpegRawPcmAudioUtility
        {
            public FfmpegRawPcmAudioUtilityTester()
                : base(PathHelper.GetExe(AppConfigHelper.FfmpegExe), null)
            {
            }

            public void InvokeCheckRequestValid
            (
                FileInfo source,
                string sourceMediaType,
                FileInfo output,
                string outputMediaType,
                AudioUtilityRequest request)
            {
                this.CheckRequestValid(source, sourceMediaType, output, outputMediaType, request);
            }
        }
    }
}
