namespace Acoustics.Test.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using Acoustics.Shared;
    using Acoustics.Tools.Audio;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    [TestClass]
    public class AudioUtilityInfoTests
    {
        [TestMethod]
        [Timeout(10_000)]
        public void InfoCanTimeout()
        {
            //TestSetup.TestLogging.ModifyVerbosity(Level.All, false);

            // with sox --version v14.4.1 it should take ~2minutes to run sox --info -V on the corrupt file
            var source = PathHelper.GetTestAudioFile("corrupt.wav");
            AbstractAudioUtility util = (AbstractAudioUtility)TestHelper.GetAudioUtilitySox();

            util.ProcessRunnerTimeout = TimeSpan.FromMilliseconds(300);
            util.ProcessRunnerMaxRetries = 0;

            TestHelper.ExceptionMatches<ProcessRunner.ProcessMaximumRetriesException>(
                () =>
                {
                    var info = util.Info(source);
                },
                "Process had not already terminated after timeout.");
        }

        [DataTestMethod]
        [DataRow("06Sibylla.asf")]
        [DataRow("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3")]
        [DataRow("A French Fiddle Speaks.mp3")]
        [DataRow("ocioncosta-lindamenina.ogg")]
        [DataRow("Lewins Rail Kekkek.wav")]
        [DataRow("FemaleKoala MaleKoala.wav")]
        [DataRow("geckos.wav")]
        [DataRow("Lewins Rail Kekkek.webm")]
        [DataRow("06Sibylla.wma")]
        [DataRow("Raw_audio_id_cd6e8ba1-11b4-4724-9562-f6ec893110aa.wv")]
        [DataRow("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv")]
        [DataRow("4channelsPureTones.wav")]
        [DataRow("4channelsPureTones.flac")]
        [DataRow("4channelsPureTones.ogg")]
        [DataRow("4channelsPureTones.wv")]
        [DataRow("different_channels_tone.wav")]
        [DataRow("different_channels_tone.mp3")]
        [DataRow("4min test.mp3")]
        public void InfoWorksForMaster(string file)
        {
            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile(file);
            var info = util.Info(source);

            var expected = TestHelper.AudioDetails[file];

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        public static IEnumerable<object[]> TestFilesAndCultureData()
        {
            var files = new[]
            {
                "06Sibylla.asf",
                "Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3",
                "A French Fiddle Speaks.mp3",
                "ocioncosta-lindamenina.ogg",
                "Lewins Rail Kekkek.wav",
                "FemaleKoala MaleKoala.wav",
                "geckos.wav",
                "Lewins Rail Kekkek.webm",
                "06Sibylla.wma",
                "Raw_audio_id_cd6e8ba1-11b4-4724-9562-f6ec893110aa.wv",
                "f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv",
                "4channelsPureTones.wav",
                "4channelsPureTones.flac",
                "4channelsPureTones.ogg",
                "4channelsPureTones.wv",
                "different_channels_tone.wav",
                "different_channels_tone.mp3",
                "4min test.mp3",
            };

            var cultures = new string[]
            {
                "en-AU",
                "de-DE",
                "it-it",
            };

            return files.SelectMany(f => cultures.Select(c => new object[] {f, c}));
        }

        [DataTestMethod]
        [DynamicData(nameof(TestFilesAndCultureData), DynamicDataSourceType.Method)]
        public void InfoWorksForMasterInDifferentCultures(string file, string culture)
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            try
            {
                var util = TestHelper.GetAudioUtility();

                var source = TestHelper.GetAudioFile(file);
                var info = util.Info(source);

                var expected = TestHelper.AudioDetails[file];

                TestHelper.CheckAudioUtilityInfo(expected, info);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        [DataTestMethod]
        [DataRow("very_very_large_file_24h_ffmpeg.flac", 86400)]
        [DataRow("very_very_large_file_PT26H59M23.456S.flac", 97163.456)]
        public void InfoWorksForVeryLongFiles(string file, double duration)
        {
            var utilities = new[] { TestHelper.GetAudioUtilitySox(), TestHelper.GetAudioUtilityFfmpeg() };

            foreach (var utility in utilities)
            {
                var info = utility.Info(PathHelper.GetTestAudioFile(file));

                Assert.IsNotNull(info.Duration);
                Assert.AreEqual(duration, info.Duration.Value.TotalSeconds, 0.00001);
            }
        }

        [DataTestMethod]
        [DataRow("4channelsPureTones.raw", typeof(NotImplementedException), "Raw formats inherently have no information to gather")]
        public void InfoFailsForMaster(string file, Type exception, string message)
        {
            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile(file);

            TestHelper.ExceptionMatches(
                exception,
                () =>
                {
                    util.Info(source);
                },
                message);
        }

        [DataTestMethod]
        [DataRow("Lewins Rail Kekkek.wav")]
        [DataRow("FemaleKoala MaleKoala.wav")]
        [DataRow("geckos.wav")]
        [DataRow("4channelsPureTones.wav")]
        [DataRow("different_channels_tone.wav")]
        public void InfoWorksShnTool(string file)
        {
            var util = TestHelper.GetAudioUtilityShntool();

            var source = TestHelper.GetAudioFile(file);
            var info = util.Info(source);

            var expected = TestHelper.AudioDetails[file];

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [DataTestMethod]
        [DataRow("06Sibylla.asf")]
        [DataRow("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3")]
        [DataRow("A French Fiddle Speaks.mp3")]
        [DataRow("ocioncosta-lindamenina.ogg")]
        [DataRow("Lewins Rail Kekkek.webm")]
        [DataRow("06Sibylla.wma")]
        [DataRow("Raw_audio_id_cd6e8ba1-11b4-4724-9562-f6ec893110aa.wv")]
        [DataRow("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv")]
        [DataRow("4channelsPureTones.flac")]
        [DataRow("4channelsPureTones.ogg")]
        [DataRow("4channelsPureTones.raw")]
        [DataRow("4channelsPureTones.wv")]
        [DataRow("different_channels_tone.mp3")]
        [DataRow("4min test.mp3")]
        public void InfoFailsForShnTool(string file)
        {
            var util = TestHelper.GetAudioUtilityShntool();

            var source = TestHelper.GetAudioFile(file);
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Valid formats are: wav (audio/wav).");
        }

        [DataTestMethod]
        [DataRow("A French Fiddle Speaks.mp3")]
        [DataRow("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3")]
        [DataRow("Lewins Rail Kekkek.wav")]
        [DataRow("FemaleKoala MaleKoala.wav")]
        [DataRow("geckos.wav")]
        [DataRow("4channelsPureTones.flac")]
        [DataRow("4channelsPureTones.wav")]
        [DataRow("different_channels_tone.wav")]
        [DataRow("different_channels_tone.mp3")]
        [DataRow("4min test.mp3")]
        public void InfoWorksSoxTool(string file)
        {
            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile(file);
            var info = util.Info(source);

            var expected = TestHelper.AudioDetails[file];

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [TestMethod]
        [DataRow("06Sibylla.asf")]
        [DataRow("ocioncosta-lindamenina.ogg")]
        [DataRow("Lewins Rail Kekkek.webm")]
        [DataRow("06Sibylla.wma")]
        [DataRow("Raw_audio_id_cd6e8ba1-11b4-4724-9562-f6ec893110aa.wv")]
        [DataRow("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv")]
        [DataRow("4channelsPureTones.ogg")]
        [DataRow("4channelsPureTones.raw")]
        [DataRow("4channelsPureTones.wv")]
        public void InfoFailsForSoxTool(string file)
        {
            var util = TestHelper.GetAudioUtilitySox();

            var source = TestHelper.GetAudioFile(file);
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Valid formats are: wav (audio/wav), mp3 (audio/mpeg), flac (audio/flac).");
        }

        [DataTestMethod]
        [DataRow("06Sibylla.asf")]
        [DataRow("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3")]
        [DataRow("A French Fiddle Speaks.mp3")]
        [DataRow("ocioncosta-lindamenina.ogg")]
        [DataRow("Lewins Rail Kekkek.wav")]
        [DataRow("FemaleKoala MaleKoala.wav")]
        [DataRow("geckos.wav")]
        [DataRow("Lewins Rail Kekkek.webm")]
        [DataRow("06Sibylla.wma")]
        [DataRow("Raw_audio_id_cd6e8ba1-11b4-4724-9562-f6ec893110aa.wv")]
        [DataRow("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv")]
        [DataRow("4channelsPureTones.wav")]
        [DataRow("4channelsPureTones.flac")]
        [DataRow("4channelsPureTones.ogg")]
        [DataRow("4channelsPureTones.wv")]
        [DataRow("different_channels_tone.wav")]
        [DataRow("different_channels_tone.mp3")]
        [DataRow("4min test.mp3")]
        public void InfoWorksForFfmpeg(string file)
        {
            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile(file);
            var info = util.Info(source);

            var expected = TestHelper.AudioDetails[file];

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [DataTestMethod]
        [DataRow("4channelsPureTones.raw")]
        public void InfoFailsForFfmpeg(string file)
        {
            var util = TestHelper.GetAudioUtilityFfmpeg();

            var source = TestHelper.GetAudioFile(file);
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Invalid formats are: raw (audio/pcm)");
        }

        [DataTestMethod]
        [DataRow("06Sibylla.asf")]
        [DataRow("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3")]
        [DataRow("A French Fiddle Speaks.mp3")]
        [DataRow("ocioncosta-lindamenina.ogg")]
        [DataRow("Lewins Rail Kekkek.wav")]
        [DataRow("FemaleKoala MaleKoala.wav")]
        [DataRow("geckos.wav")]
        [DataRow("Lewins Rail Kekkek.webm")]
        [DataRow("06Sibylla.wma")]
        [DataRow("Raw_audio_id_cd6e8ba1-11b4-4724-9562-f6ec893110aa.wv")]
        [DataRow("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv")]
        [DataRow("4channelsPureTones.wav")]
        [DataRow("4channelsPureTones.flac")]
        [DataRow("4channelsPureTones.ogg")]
        [DataRow("4channelsPureTones.wv")]
        [DataRow("different_channels_tone.wav")]
        [DataRow("different_channels_tone.mp3")]
        [DataRow("4min test.mp3")]
        public void InfoWorksForFfmpegRawPcm(string file)
        {
            var util = TestHelper.GetAudioUtilityFfmpegRawPcm();

            var source = TestHelper.GetAudioFile(file);
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Valid formats are: raw (audio/pcm)");
        }

        [DataTestMethod]
        [DataRow("4channelsPureTones.raw")]
        public void InfoFailsForFfmpegRawPcm(string file)
        {
            var util = TestHelper.GetAudioUtilityFfmpegRawPcm();

            var source = TestHelper.GetAudioFile(file);
            TestHelper.ExceptionMatches<NotImplementedException>(
                () => util.Info(source),
                "Raw formats inherently have no information to gather");
        }

        [DataTestMethod]
        [DataRow("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3")]
        [DataRow("A French Fiddle Speaks.mp3")]
        [DataRow("different_channels_tone.mp3")]
        [DataRow("4min test.mp3")]
        public void InfoWorksForMp3Splt(string file)
        {
            var util = TestHelper.GetAudioUtilityMp3Splt();

            var source = TestHelper.GetAudioFile(file);
            TestHelper.ExceptionMatches<NotImplementedException>(
                () => util.Info(source),
                "The method or operation is not implemented.");
        }

        [DataTestMethod]
        [DataRow("06Sibylla.asf")]
        [DataRow("4channelsPureTones.raw")]
        [DataRow("Lewins Rail Kekkek.wav")]
        [DataRow("FemaleKoala MaleKoala.wav")]
        [DataRow("geckos.wav")]
        [DataRow("Lewins Rail Kekkek.webm")]
        [DataRow("ocioncosta-lindamenina.ogg")]
        [DataRow("06Sibylla.wma")]
        [DataRow("Raw_audio_id_cd6e8ba1-11b4-4724-9562-f6ec893110aa.wv")]
        [DataRow("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv")]
        [DataRow("4channelsPureTones.wav")]
        [DataRow("4channelsPureTones.flac")]
        [DataRow("4channelsPureTones.ogg")]
        [DataRow("4channelsPureTones.wv")]
        [DataRow("different_channels_tone.wav")]
        public void InfoFailsForMp3Splt(string file)
        {
            var util = TestHelper.GetAudioUtilityMp3Splt();

            var source = TestHelper.GetAudioFile(file);
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Valid formats are: mp3 (audio/mpeg).");
        }

        [DataTestMethod]
        [DataRow("Raw_audio_id_cd6e8ba1-11b4-4724-9562-f6ec893110aa.wv")]
        [DataRow("f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv")]
        [DataRow("4channelsPureTones.wv")]
        public void InfoWorksFoWavunpack(string file)
        {
            var util = TestHelper.GetAudioUtilityWavunpack();

            var source = TestHelper.GetAudioFile(file);
            var info = util.Info(source);

            var expected = TestHelper.AudioDetails[file];

            TestHelper.CheckAudioUtilityInfo(expected, info);
        }

        [DataTestMethod]
        [DataRow("06Sibylla.asf")]
        [DataRow("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3")]
        [DataRow("A French Fiddle Speaks.mp3")]
        [DataRow("ocioncosta-lindamenina.ogg")]
        [DataRow("Lewins Rail Kekkek.wav")]
        [DataRow("FemaleKoala MaleKoala.wav")]
        [DataRow("geckos.wav")]
        [DataRow("Lewins Rail Kekkek.webm")]
        [DataRow("06Sibylla.wma")]
        [DataRow("4channelsPureTones.wav")]
        [DataRow("4channelsPureTones.flac")]
        [DataRow("4channelsPureTones.ogg")]
        [DataRow("4channelsPureTones.raw")]
        [DataRow("different_channels_tone.wav")]
        [DataRow("different_channels_tone.mp3")]
        [DataRow("4min test.mp3")]
        public void InfoFailsForWavunpack(string file)
        {
            var util = TestHelper.GetAudioUtilityWavunpack();

            var source = TestHelper.GetAudioFile(file);
            TestHelper.ExceptionMatches<NotSupportedException>(
                () => util.Info(source),
                "cannot be processed.  Valid formats are: wv (audio/x-wv).");
        }
    }
}
