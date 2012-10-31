using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Acoustics.Test.Tools
{
    using System.IO;

    using Acoustics.Shared;
    using Acoustics.Tools;

    using EcoSounds.Mvc.Tests;

    [TestClass]
    public class AudioUtilityWavTests
    {
        [TestMethod]
        public void SegmentsWavCorrectly1()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(30),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 279000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(20),
                OffsetEnd = TimeSpan.FromSeconds(50),
                TargetSampleRate = 17460
            };

            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("Lewins Rail Kekkek.wav");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavCorrectly2()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(35),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 279000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetEnd = TimeSpan.FromSeconds(35),
                TargetSampleRate = 17460
            };

            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("Lewins Rail Kekkek.wav");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavCorrectly3()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(22.2445),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 279000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(38),
                TargetSampleRate = 17460
            };

            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("Lewins Rail Kekkek.wav");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavCorrectly4()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(60),
                SampleRate = 22050,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 353000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(15),
                OffsetEnd = TimeSpan.FromSeconds(75)
            };

            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("FemaleKoala MaleKoala.wav");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavCorrectly5()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(90),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 279000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(27),
                OffsetEnd = TimeSpan.FromSeconds(117),
                TargetSampleRate = 17460
            };

            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("FemaleKoala MaleKoala.wav");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavCorrectly6()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(93),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 279000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = false,
                Channel = 2,
                OffsetStart = TimeSpan.FromSeconds(27),
                TargetSampleRate = 17460,
            };

            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("geckos.wav");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavCorrectly1Sox()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(30),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 279000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(20),
                OffsetEnd = TimeSpan.FromSeconds(50),
                TargetSampleRate = 17460
            };

            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("Lewins Rail Kekkek.wav");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavCorrectly2Sox()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(35),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 279000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetEnd = TimeSpan.FromSeconds(35),
                TargetSampleRate = 17460
            };

            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("Lewins Rail Kekkek.wav");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavCorrectly3Sox()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(22.2445),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 279000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(38),
                TargetSampleRate = 17460
            };

            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("Lewins Rail Kekkek.wav");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavCorrectly4Sox()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(60),
                SampleRate = 22050,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 353000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(15),
                OffsetEnd = TimeSpan.FromSeconds(75)
            };

            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("FemaleKoala MaleKoala.wav");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavCorrectly5Sox()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(90),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 279000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(27),
                OffsetEnd = TimeSpan.FromSeconds(117),
                TargetSampleRate = 17460
            };

            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("FemaleKoala MaleKoala.wav");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavCorrectly6Sox()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(93),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 279000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = false,
                Channel = 2,
                OffsetStart = TimeSpan.FromSeconds(27),
                TargetSampleRate = 17460,
            };

            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("geckos.wav");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavCorrectly1Ffmpeg()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(30),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 279000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(20),
                OffsetEnd = TimeSpan.FromSeconds(50),
                TargetSampleRate = 17460
            };

            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("Lewins Rail Kekkek.wav");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavCorrectly2Ffmpeg()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(35),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 280000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetEnd = TimeSpan.FromSeconds(35),
                TargetSampleRate = 17460
            };

            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("Lewins Rail Kekkek.wav");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavCorrectly3Ffmpeg()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(22.2445),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 279000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(38),
                TargetSampleRate = 17460
            };

            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("Lewins Rail Kekkek.wav");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavCorrectly4Ffmpeg()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(60),
                SampleRate = 22050,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 353000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(15),
                OffsetEnd = TimeSpan.FromSeconds(75)
            };

            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("FemaleKoala MaleKoala.wav");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavCorrectly5Ffmpeg()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(90),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 279000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(27),
                OffsetEnd = TimeSpan.FromSeconds(117),
                TargetSampleRate = 17460
            };

            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("FemaleKoala MaleKoala.wav");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavCorrectly6Ffmpeg()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(93),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 279000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = false,
                Channel = 2,
                OffsetStart = TimeSpan.FromSeconds(27),
                TargetSampleRate = 17460,
            };

            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("geckos.wav");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }
    }
}
