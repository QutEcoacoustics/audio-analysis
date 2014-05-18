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

    /// <summary>
    /// The audio utility wavpack tests.
    /// Segments wavpack (.wv) to wave (.wav).
    /// </summary>
    [TestClass]
    public class AudioUtilityWavpackTests
    {
        [TestMethod]
        public void SegmentsWavpackCorrectly1Wavunpack()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(30),
                SampleRate = 22050,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 353000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = false,
                OffsetStart = TimeSpan.FromSeconds(20),
                OffsetEnd = TimeSpan.FromSeconds(50),
                //SampleRate = 11025
            };

            var source = TestHelper.GetAudioFile("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            TestHelper.GetAudioUtilityWavunpack().Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = TestHelper.GetAudioUtility().Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavpackCorrectly2Wavunpack()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(20),
                SampleRate = 22050,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 353000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = false,
                OffsetStart = TimeSpan.FromSeconds(15),
                OffsetEnd = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(35)
            };

            var source = TestHelper.GetAudioFile("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            TestHelper.GetAudioUtilityWavunpack().Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = TestHelper.GetAudioUtility().Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavpackCorrectly3Wavunpack()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(55),
                SampleRate = 22050,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 353000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = false,
                OffsetStart = TimeSpan.FromSeconds(0),
                OffsetEnd = TimeSpan.FromSeconds(55),
                //SampleRate = 11025
            };

            var source = TestHelper.GetAudioFile("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            TestHelper.GetAudioUtilityWavunpack().Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = TestHelper.GetAudioUtility().Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavpackCorrectly4Wavunpack()
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
                MixDownToMono = false,
                OffsetStart = TimeSpan.FromSeconds(15),
                OffsetEnd = TimeSpan.FromSeconds(75),
                //SampleRate = 11025
            };

            var source = TestHelper.GetAudioFile("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            TestHelper.GetAudioUtilityWavunpack().Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = TestHelper.GetAudioUtility().Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavpackCorrectly5Wavunpack()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(9) + TimeSpan.FromSeconds(33),
                SampleRate = 22050,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 353000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = false,
                OffsetStart = TimeSpan.FromSeconds(27),
                //SampleRate = 44100
            };


            var source = TestHelper.GetAudioFile("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            TestHelper.GetAudioUtilityWavunpack().Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = TestHelper.GetAudioUtility().Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavpackCorrectly6Wavunpack()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(4) + TimeSpan.FromSeconds(33),
                SampleRate = 22050,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 353000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = false,
                //Channel = 2,
                OffsetStart = TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(27),
                //SampleRate = 11025,
            };

            var source = TestHelper.GetAudioFile("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            TestHelper.GetAudioUtilityWavunpack().Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = TestHelper.GetAudioUtility().Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavpackCorrectly7Wavunpack()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(10) + TimeSpan.FromSeconds(0),
                SampleRate = 22050,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 353000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = false,
            };

            var source = TestHelper.GetAudioFile("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            TestHelper.GetAudioUtilityWavunpack().Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = TestHelper.GetAudioUtility().Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavpackCorrectly1Master()
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

            var source = TestHelper.GetAudioFile("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavpackCorrectly2Master()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(20),
                SampleRate = 22050,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 353000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(15),
                OffsetEnd = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(35)
            };

            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavpackCorrectly3Master()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(52),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 279000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetEnd = TimeSpan.FromSeconds(52),
                TargetSampleRate = 17460
            };

            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavpackCorrectly4Master()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(60),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 279000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(15),
                OffsetEnd = TimeSpan.FromSeconds(75),
                TargetSampleRate = 17460
            };

            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavpackCorrectly5Master()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(9) + TimeSpan.FromSeconds(33),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 279000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(27),
                TargetSampleRate = 17460
            };

            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavpackCorrectly6Master()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(7) + TimeSpan.FromSeconds(33),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 279000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = false,
                Channel = 1,
                OffsetStart = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(27),
                TargetSampleRate = 17460,
            };

            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsWavpackCorrectly7Master()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(10) + TimeSpan.FromSeconds(0),
                SampleRate = 17460,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 279000
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                TargetSampleRate = 17460,
            };

            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv");
            var output = TestHelper.GetTempFile(MediaTypes.ExtWav);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }
    }
}
