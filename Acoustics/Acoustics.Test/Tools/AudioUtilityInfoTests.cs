namespace Acoustics.Test.Tools
{
    using System;

    using Acoustics.Shared;
    using Acoustics.Tools;

    using EcoSounds.Mvc.Tests;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AudioUtilityInfoTests
    {
        [TestMethod]
        public void InfoAsfMaster()
        {
            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("06Sibylla.asf");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
                {
                    Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(49.6),
                    SampleRate = 44100,
                    ChannelCount = 2,
                    BitsPerSecond = 128000,
                    MediaType = MediaTypes.MediaTypeAsf,
                };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoMp3Master()
        {
            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(59.987),
                SampleRate = 22050,
                ChannelCount = 1,
                BitsPerSecond = 96000,
                MediaType = MediaTypes.MediaTypeMp3,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoMp32Master()
        {
            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("A French Fiddle Speaks.mp3");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(41.6),
                SampleRate = 44100,
                ChannelCount = 2,
                BitsPerSecond = 160000,
                MediaType = MediaTypes.MediaTypeMp3,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoOggMaster()
        {
            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("ocioncosta-lindamenina.ogg");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(32.173),
                SampleRate = 32000,
                ChannelCount = 2,
                BitsPerSecond = 84000,
                MediaType = MediaTypes.MediaTypeOggAudio,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoWavMaster()
        {
            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("Lewins Rail Kekkek.wav");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(0.245),
                SampleRate = 22050,
                ChannelCount = 1,
                BitsPerSecond = 352000,
                MediaType = MediaTypes.MediaTypeWav,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoWav2Master()
        {
            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("FemaleKoala MaleKoala.wav");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(0),
                SampleRate = 22050,
                ChannelCount = 1,
                BitsPerSecond = 352000,
                MediaType = MediaTypes.MediaTypeWav,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoWav3Master()
        {
            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("geckos.wav");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(59.902),
                SampleRate = 44100,
                ChannelCount = 2,
                BitsPerSecond = 1410000,
                MediaType = MediaTypes.MediaTypeWav,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoWebmMaster()
        {
            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("Lewins Rail Kekkek.webm");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(0.7),
                SampleRate = 22050,
                ChannelCount = 1,
                BitsPerSecond = 43032,
                MediaType = MediaTypes.MediaTypeWebMAudio,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoWmaMaster()
        {
            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("06Sibylla.wma");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(49.6),
                SampleRate = 44100,
                ChannelCount = 2,
                BitsPerSecond = 128000,
                MediaType = MediaTypes.MediaTypeWma,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoWvMaster()
        {
            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("Raw_audio_id_cd6e8ba1-11b4-4724-9562-f6ec893110aa.wv");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(0),
                SampleRate = 22050,
                ChannelCount = 1,
                BitsPerSecond = 171000,
                MediaType = MediaTypes.MediaTypeWavpack,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoWv2Master()
        {
            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(10) + TimeSpan.FromSeconds(0),
                SampleRate = 22050,
                ChannelCount = 1,
                BitsPerSecond = 158000,
                MediaType = MediaTypes.MediaTypeWavpack,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoAsfSox()
        {
            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("06Sibylla.asf");
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Valid formats are: wav (audio/x-wav), mp3 (audio/mpeg).");
        }

        [TestMethod]
        public void InfoMp3Sox()
        {
            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(59.987),
                SampleRate = 22050,
                ChannelCount = 1,
                BitsPerSecond = 96000,
                MediaType = MediaTypes.MediaTypeMp3,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoMp32Sox()
        {
            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("A French Fiddle Speaks.mp3");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(41.6),
                SampleRate = 44100,
                ChannelCount = 2,
                BitsPerSecond = 160000,
                MediaType = MediaTypes.MediaTypeMp3,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoOggSox()
        {
            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("ocioncosta-lindamenina.ogg");
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Valid formats are: wav (audio/x-wav), mp3 (audio/mpeg).");
        }

        [TestMethod]
        public void InfoWavSox()
        {
            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("Lewins Rail Kekkek.wav");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(0.245),
                SampleRate = 22050,
                ChannelCount = 1,
                BitsPerSecond = 352000,
                MediaType = MediaTypes.MediaTypeWav,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoWav2Sox()
        {
            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("FemaleKoala MaleKoala.wav");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(0),
                SampleRate = 22050,
                ChannelCount = 1,
                BitsPerSecond = 352000,
                MediaType = MediaTypes.MediaTypeWav,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoWav3Sox()
        {
            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("geckos.wav");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(59.902),
                SampleRate = 44100,
                ChannelCount = 2,
                BitsPerSecond = 1410000,
                MediaType = MediaTypes.MediaTypeWav,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoWebmSox()
        {
            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("Lewins Rail Kekkek.webm");
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Valid formats are: wav (audio/x-wav), mp3 (audio/mpeg).");
        }

        [TestMethod]
        public void InfoWmaSox()
        {
            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("06Sibylla.wma");
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Valid formats are: wav (audio/x-wav), mp3 (audio/mpeg).");
        }

        [TestMethod]
        public void InfoWvSox()
        {
            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("Raw_audio_id_cd6e8ba1-11b4-4724-9562-f6ec893110aa.wv");
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Valid formats are: wav (audio/x-wav), mp3 (audio/mpeg).");
        }

        [TestMethod]
        public void InfoWv2Sox()
        {
            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv");
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Valid formats are: wav (audio/x-wav), mp3 (audio/mpeg).");
        }

        [TestMethod]
        public void InfoAsfFfmpeg()
        {
            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("06Sibylla.asf");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(49.6),
                SampleRate = 44100,
                ChannelCount = 2,
                BitsPerSecond = 128000,
                MediaType = MediaTypes.MediaTypeAsf,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoMp3Ffmpeg()
        {
            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(59.987),
                SampleRate = 22050,
                ChannelCount = 1,
                BitsPerSecond = 96000,
                MediaType = MediaTypes.MediaTypeMp3,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoMp32Ffmpeg()
        {
            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("A French Fiddle Speaks.mp3");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(41.6),
                SampleRate = 44100,
                ChannelCount = 2,
                BitsPerSecond = 160000,
                MediaType = MediaTypes.MediaTypeMp3,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoOggFfmpeg()
        {
            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("ocioncosta-lindamenina.ogg");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(32.173),
                SampleRate = 32000,
                ChannelCount = 2,
                BitsPerSecond = 84000,
                MediaType = MediaTypes.MediaTypeOggAudio,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoWavFfmpeg()
        {
            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("Lewins Rail Kekkek.wav");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(0.245),
                SampleRate = 22050,
                ChannelCount = 1,
                BitsPerSecond = 352000,
                MediaType = MediaTypes.MediaTypeWav,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoWav2Ffmpeg()
        {
            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("FemaleKoala MaleKoala.wav");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(0),
                SampleRate = 22050,
                ChannelCount = 1,
                BitsPerSecond = 352000,
                MediaType = MediaTypes.MediaTypeWav,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoWav3Ffmpeg()
        {
            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("geckos.wav");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(59.902),
                SampleRate = 44100,
                ChannelCount = 2,
                BitsPerSecond = 1410000,
                MediaType = MediaTypes.MediaTypeWav
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoWebmFfmpeg()
        {
            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("Lewins Rail Kekkek.webm");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(0.7),
                SampleRate = 22050,
                ChannelCount = 1,
                BitsPerSecond = 43032,
                MediaType = MediaTypes.MediaTypeWebMAudio,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoWmaFfmpeg()
        {
            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("06Sibylla.wma");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(49.6),
                SampleRate = 44100,
                ChannelCount = 2,
                BitsPerSecond = 128000,
                MediaType = MediaTypes.MediaTypeWma,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoWvFfmpeg()
        {
            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile("Raw_audio_id_cd6e8ba1-11b4-4724-9562-f6ec893110aa.wv");
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Invalid formats are: wv (audio/x-wv).");
        }

        [TestMethod]
        public void InfoAsfMp3Splt()
        {
            var util = TestHelper.GetAudioUtilityMp3Splt();

            var source = TestHelper.GetAudioFile("06Sibylla.asf");
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Valid formats are: mp3 (audio/mpeg).");
        }

        [TestMethod]
        public void InfoMp3Mp3Splt()
        {
            var util = TestHelper.GetAudioUtilityMp3Splt();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(59.987),
                SampleRate = 22050,
                ChannelCount = 1,
                BitsPerSecond = 96000,
                MediaType = MediaTypes.MediaTypeMp3,
            };

            TestHelper.ExceptionMatches<NullReferenceException>(
                () => TestHelper.CheckAudioUtilityInfo(expected, info),
                "Object reference not set to an instance of an object.");
        }

        [TestMethod]
        public void InfoMp32Mp3Splt()
        {
            var util = TestHelper.GetAudioUtilityMp3Splt();

            var source = TestHelper.GetAudioFile("A French Fiddle Speaks.mp3");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(41.6),
                SampleRate = 44100,
                ChannelCount = 2,
                BitsPerSecond = 160000,
                MediaType = MediaTypes.MediaTypeMp3,
            };

            TestHelper.ExceptionMatches<NullReferenceException>(
                () => TestHelper.CheckAudioUtilityInfo(expected, info),
                "Object reference not set to an instance of an object.");
        }

        [TestMethod]
        public void InfoOggMp3Splt()
        {
            var util = TestHelper.GetAudioUtilityMp3Splt();

            var source = TestHelper.GetAudioFile("ocioncosta-lindamenina.ogg");
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Valid formats are: mp3 (audio/mpeg).");
        }

        [TestMethod]
        public void InfoWavMp3Splt()
        {
            var util = TestHelper.GetAudioUtilityMp3Splt();

            var source = TestHelper.GetAudioFile("Lewins Rail Kekkek.wav");
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Valid formats are: mp3 (audio/mpeg).");
        }

        [TestMethod]
        public void InfoWav2Mp3Splt()
        {
            var util = TestHelper.GetAudioUtilityMp3Splt();

            var source = TestHelper.GetAudioFile("FemaleKoala MaleKoala.wav");
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Valid formats are: mp3 (audio/mpeg).");
        }

        [TestMethod]
        public void InfoWav3Mp3Splt()
        {
            var util = TestHelper.GetAudioUtilityMp3Splt();

            var source = TestHelper.GetAudioFile("geckos.wav");
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Valid formats are: mp3 (audio/mpeg).");
        }

        [TestMethod]
        public void InfoWebmMp3Splt()
        {
            var util = TestHelper.GetAudioUtilityMp3Splt();

            var source = TestHelper.GetAudioFile("Lewins Rail Kekkek.webm");
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Valid formats are: mp3 (audio/mpeg).");
        }

        [TestMethod]
        public void InfoWmaMp3Splt()
        {
            var util = TestHelper.GetAudioUtilityMp3Splt();

            var source = TestHelper.GetAudioFile("06Sibylla.wma");
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Valid formats are: mp3 (audio/mpeg).");
        }

        [TestMethod]
        public void InfoWvMp3Splt()
        {
            var util = TestHelper.GetAudioUtilityMp3Splt();

            var source = TestHelper.GetAudioFile("Raw_audio_id_cd6e8ba1-11b4-4724-9562-f6ec893110aa.wv");
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Valid formats are: mp3 (audio/mpeg).");
        }

        [TestMethod]
        public void InfoAsfWavunpack()
        {
            var util = TestHelper.GetAudioUtilityWavunpack();

            var source = TestHelper.GetAudioFile("06Sibylla.asf");
            AudioUtilityInfo info;
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => info = util.Info(source),
                "cannot be processed.  Valid formats are: wv (audio/x-wv).");

        }

        [TestMethod]
        public void InfoMp3Wavunpack()
        {
            var util = TestHelper.GetAudioUtilityWavunpack();

            var source = TestHelper.GetAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3");
            AudioUtilityInfo info;
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => info = util.Info(source),
                "cannot be processed.  Valid formats are: wv (audio/x-wv).");
        }

        [TestMethod]
        public void InfoMp32Wavunpack()
        {
            var util = TestHelper.GetAudioUtilityWavunpack();

            var source = TestHelper.GetAudioFile("A French Fiddle Speaks.mp3");
            AudioUtilityInfo info;
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => info = util.Info(source),
                "cannot be processed.  Valid formats are: wv (audio/x-wv).");
        }

        [TestMethod]
        public void InfoOggWavunpack()
        {
            var util = TestHelper.GetAudioUtilityWavunpack();

            var source = TestHelper.GetAudioFile("ocioncosta-lindamenina.ogg");
            AudioUtilityInfo info;
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => info = util.Info(source),
                "cannot be processed.  Valid formats are: wv (audio/x-wv).");
        }

        [TestMethod]
        public void InfoWavWavunpack()
        {
            var util = TestHelper.GetAudioUtilityWavunpack();

            var source = TestHelper.GetAudioFile("Lewins Rail Kekkek.wav");
            AudioUtilityInfo info;
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => info = util.Info(source),
                "cannot be processed.  Valid formats are: wv (audio/x-wv).");
        }

        [TestMethod]
        public void InfoWav2Wavunpack()
        {
            var util = TestHelper.GetAudioUtilityWavunpack();

            var source = TestHelper.GetAudioFile("FemaleKoala MaleKoala.wav");
            AudioUtilityInfo info;
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => info = util.Info(source),
                "cannot be processed.  Valid formats are: wv (audio/x-wv).");
        }

        [TestMethod]
        public void InfoWav3Wavunpack()
        {
            var util = TestHelper.GetAudioUtilityWavunpack();

            var source = TestHelper.GetAudioFile("geckos.wav");
            AudioUtilityInfo info;
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => info = util.Info(source),
                "cannot be processed.  Valid formats are: wv (audio/x-wv).");
        }

        [TestMethod]
        public void InfoWebmWavunpack()
        {
            var util = TestHelper.GetAudioUtilityWavunpack();

            var source = TestHelper.GetAudioFile("Lewins Rail Kekkek.webm");
            AudioUtilityInfo info;
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => info = util.Info(source),
                "cannot be processed.  Valid formats are: wv (audio/x-wv).");
        }

        [TestMethod]
        public void InfoWmaWavunpack()
        {
            var util = TestHelper.GetAudioUtilityWavunpack();

            var source = TestHelper.GetAudioFile("06Sibylla.wma");
            AudioUtilityInfo info;
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => info = util.Info(source),
                "cannot be processed.  Valid formats are: wv (audio/x-wv).");
        }

        [TestMethod]
        public void InfoWvWavunpack()
        {
            var util = TestHelper.GetAudioUtilityWavunpack();

            var source = TestHelper.GetAudioFile("Raw_audio_id_cd6e8ba1-11b4-4724-9562-f6ec893110aa.wv");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(0),
                SampleRate = 22050,
                ChannelCount = 1,
                BitsPerSecond = 171000,
                MediaType = MediaTypes.MediaTypeWavpack,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        public void InfoWv2Wavunpack()
        {
            var util = TestHelper.GetAudioUtilityWavunpack();

            var source = TestHelper.GetAudioFile("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv");
            var info = util.Info(source);

            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromMinutes(10) + TimeSpan.FromSeconds(0),
                SampleRate = 22050,
                ChannelCount = 1,
                BitsPerSecond = 158000,
                MediaType = MediaTypes.MediaTypeWavpack,
            };

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }
    }
}
