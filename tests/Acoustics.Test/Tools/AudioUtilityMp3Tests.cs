namespace Acoustics.Test.Tools
{
    using System;
    using System.IO;
    using Acoustics.Shared;
    using Acoustics.Tools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    [TestClass]
    public class AudioUtilityMp3Tests
    {
        [TestMethod]
        public void SegmentsMp3Correctly1Sox()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(30),
                SampleRate = 11025,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeMp3,
                BitsPerSecond = 16000,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(20),
                OffsetEnd = TimeSpan.FromSeconds(50),
                TargetSampleRate = 11025,
            };

            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            var output = PathHelper.GetTempFile(MediaTypes.ExtMp3);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsMp3Correctly2Sox()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(20),
                SampleRate = 22050,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeMp3,
                BitsPerSecond = 32000,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(15),
                OffsetEnd = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(35),
            };

            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            var output = PathHelper.GetTempFile(MediaTypes.ExtMp3);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsMp3Correctly3Sox()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(55),
                SampleRate = 11025,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeMp3,
                BitsPerSecond = 16000,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(0),
                OffsetEnd = TimeSpan.FromSeconds(55),
                TargetSampleRate = 11025,
            };

            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            var output = PathHelper.GetTempFile(MediaTypes.ExtMp3);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsMp3Correctly4Sox()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(60),
                SampleRate = 11025,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeMp3,
                BitsPerSecond = 16000,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(15),
                OffsetEnd = TimeSpan.FromSeconds(75),
                TargetSampleRate = 11025,
            };

            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            var output = PathHelper.GetTempFile(MediaTypes.ExtMp3);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsMp3Correctly5Sox()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(213),
                SampleRate = 44100,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeMp3,
                BitsPerSecond = 64000,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(27),
                TargetSampleRate = 44100,
            };

            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            var output = PathHelper.GetTempFile(MediaTypes.ExtMp3);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsMp3Correctly6Sox()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(134.6),
                SampleRate = 11025,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeMp3,
                BitsPerSecond = 16000,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = false,
                Channels = 2.AsArray(),
                OffsetStart = TimeSpan.FromSeconds(27),
                TargetSampleRate = 11025,
            };

            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("A French Fiddle Speaks.mp3");
            var output = PathHelper.GetTempFile(MediaTypes.ExtMp3);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsMp3Correctly1Ffmpeg()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(30),
                SampleRate = 11025,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeMp3,
                BitsPerSecond = 16100,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(20),
                OffsetEnd = TimeSpan.FromSeconds(50),
                TargetSampleRate = 11025,
            };

            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            var output = PathHelper.GetTempFile(MediaTypes.ExtMp3);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsMp3Correctly2Ffmpeg()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(20),
                SampleRate = 22050,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeMp3,
                BitsPerSecond = 32000,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(15),
                OffsetEnd = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(35),
            };

            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            var output = PathHelper.GetTempFile(MediaTypes.ExtMp3);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsMp3Correctly3Ffmpeg()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(52),
                SampleRate = 11025,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeMp3,
                BitsPerSecond = 16000,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetEnd = TimeSpan.FromSeconds(52),
                TargetSampleRate = 11025,
            };

            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            var output = PathHelper.GetTempFile(MediaTypes.ExtMp3);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsMp3Correctly4Ffmpeg()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(60),
                SampleRate = 44100,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeMp3,
                BitsPerSecond = 64000,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(15),
                OffsetEnd = TimeSpan.FromSeconds(75),
                TargetSampleRate = 44100,
            };

            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            var output = PathHelper.GetTempFile(MediaTypes.ExtMp3);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsMp3Correctly5Ffmpeg()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(213),
                SampleRate = 44100,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeMp3,
                BitsPerSecond = 64000,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(27),
                TargetSampleRate = 44100,
            };

            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            var output = PathHelper.GetTempFile(MediaTypes.ExtMp3);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsMp3Correctly6Ffmpeg()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(134.6),
                SampleRate = 11025,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeMp3,
                BitsPerSecond = 16000,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = false,
                Channels = 2.AsArray(),
                OffsetStart = TimeSpan.FromSeconds(27),
                TargetSampleRate = 11025,
            };

            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("A French Fiddle Speaks.mp3");
            var output = PathHelper.GetTempFile(MediaTypes.ExtMp3);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsMp3Correctly1Master()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(30),
                SampleRate = 44100,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeMp3,
                BitsPerSecond = 64100,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(20),
                OffsetEnd = TimeSpan.FromSeconds(50),
                TargetSampleRate = 44100,
            };

            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            var output = PathHelper.GetTempFile(MediaTypes.ExtMp3);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsMp3Correctly2Master()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(20),
                SampleRate = 22050,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeMp3,
                BitsPerSecond = 32000,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(15),
                OffsetEnd = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(35),
            };

            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            var output = PathHelper.GetTempFile(MediaTypes.ExtMp3);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsMp3Correctly3Master()
        {
            /*
             *
             * mp3splt accuracy varies with the quality of the input file!
             *
             */
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(48),
                SampleRate = 11025,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeMp3,
                BitsPerSecond = 16000,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.Zero,
                OffsetEnd = TimeSpan.FromSeconds(48),
                TargetSampleRate = 11025,
            };

            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            var output = PathHelper.GetTempFile(MediaTypes.ExtMp3);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual, 380);
        }

        [TestMethod]
        public void SegmentsMp3Correctly4Master()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(60),
                SampleRate = 44100,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeMp3,
                BitsPerSecond = 64000,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(15),
                OffsetEnd = TimeSpan.FromSeconds(75),
                TargetSampleRate = 44100,
            };

            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            var output = PathHelper.GetTempFile(MediaTypes.ExtMp3);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsMp3Correctly5Master()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(213.031020),
                SampleRate = 11025,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeMp3,
                BitsPerSecond = 16000,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = TimeSpan.FromSeconds(27),
                TargetSampleRate = 11025,
            };

            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            var output = PathHelper.GetTempFile(MediaTypes.ExtMp3);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }

        [TestMethod]
        public void SegmentsMp3Correctly6Master()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(134.6),
                SampleRate = 44100,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeMp3,
                BitsPerSecond = 64000,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = false,
                Channels = 2.AsArray(),
                OffsetStart = TimeSpan.FromSeconds(27),
                TargetSampleRate = 44100,
            };

            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("A French Fiddle Speaks.mp3");
            var output = PathHelper.GetTempFile(MediaTypes.ExtMp3);

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension), request);

            var actual = util.Info(output);

            File.Delete(output.FullName);

            TestHelper.CheckAudioUtilityInfo(expected, actual);
        }
    }
}
