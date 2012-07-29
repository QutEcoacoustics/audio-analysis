// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MasterAudioUtilityTests.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace EcoSounds.Mvc.Tests.AcousticsTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Acoustics.Shared;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The master audio utility tests.
    /// </summary>
    [TestClass]
    public class MasterAudioUtilityTests
    {
        /*
         * To get exe paths:
         * AppConfigHelper.FfmpegExe
         * AppConfigHelper.FfprobeExe
         * AppConfigHelper.Mp3SpltExe
         * ...etc...
         * 
         * To get test audio path:
         * 
         */
        #region Public Methods and Operators

        /// <summary>
        /// The calculates correct duration asf.
        /// </summary>
        [TestMethod]
        public void CalculatesCorrectDurationAsf()
        {
            CalculatesCorrectDurationTest(
                "06Sibylla.asf", MediaTypes.MediaTypeAsf, TimeSpan.FromSeconds(109.53), TimeSpan.FromMilliseconds(0));
        }

        /// <summary>
        /// The calculates correct duration mp 3.
        /// </summary>
        [TestMethod]
        public void CalculatesCorrectDurationMp3()
        {
            CalculatesCorrectDurationTest(
                "Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3",
                MediaTypes.MediaTypeMp3,
                TimeSpan.FromSeconds(240),
                TimeSpan.FromMilliseconds(30));
        }

        /// <summary>
        /// The calculates correct duration ogg.
        /// </summary>
        [TestMethod]
        public void CalculatesCorrectDurationOgg()
        {
            CalculatesCorrectDurationTest(
                "ocioncosta-lindamenina.ogg",
                MediaTypes.MediaTypeOggAudio,
                TimeSpan.FromSeconds(152.17),
                TimeSpan.FromMilliseconds(0));
        }

        /// <summary>
        /// The calculates correct duration wav.
        /// </summary>
        [TestMethod]
        public void CalculatesCorrectDurationWav()
        {
            CalculatesCorrectDurationTest(
                "Lewins Rail Kekkek.wav",
                MediaTypes.MediaTypeWav,
                TimeSpan.FromSeconds(60.24),
                TimeSpan.FromMilliseconds(0));
        }

        /// <summary>
        /// The calculates correct duration wav.
        /// </summary>
        [TestMethod]
        public void CalculatesCorrectDurationWebm()
        {
            CalculatesCorrectDurationTest(
                "Lewins Rail Kekkek.webm",
                MediaTypes.MediaTypeWebMAudio,
                TimeSpan.FromSeconds(60.71),
                TimeSpan.FromMilliseconds(0));
        }

        /// <summary>
        /// The calculates correct duration wma.
        /// </summary>
        [TestMethod]
        public void CalculatesCorrectDurationWma()
        {
            CalculatesCorrectDurationTest(
                "06Sibylla.wma", MediaTypes.MediaTypeWma, TimeSpan.FromSeconds(109.53), TimeSpan.FromMilliseconds(10));
        }

        /// <summary>
        /// The calculates correct duration wv.
        /// </summary>
        [TestMethod]
        public void CalculatesCorrectDurationWv()
        {
            CalculatesCorrectDurationTest(
                "Raw_audio_id_cd6e8ba1-11b4-4724-9562-f6ec893110aa.wv",
                MediaTypes.MediaTypeWavpack,
                TimeSpan.FromSeconds(120),
                TimeSpan.FromMilliseconds(0));
        }

        /// <summary>
        /// The converts mp 3 to mp 3 corectly.
        /// </summary>
        [TestMethod]
        public void ConvertsMp3ToMp3Corectly()
        {
            ConvertsCorrectly(
                "Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3",
                MediaTypes.MediaTypeMp3,
                MediaTypes.MediaTypeMp3,
                TimeSpan.FromSeconds(240),
                TimeSpan.FromMilliseconds(60));
        }

        /// <summary>
        /// The converts mp 3 to wav correctly.
        /// </summary>
        [TestMethod]
        public void ConvertsMp3ToWavCorrectly()
        {
            ConvertsCorrectly(
                "Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3",
                MediaTypes.MediaTypeMp3,
                MediaTypes.MediaTypeWav,
                TimeSpan.FromSeconds(240),
                TimeSpan.FromMilliseconds(30));
        }

        /// <summary>
        /// The converts ogg to ogg corectly.
        /// </summary>
        [TestMethod]
        public void ConvertsOggToOggCorectly()
        {
            ConvertsCorrectly(
                "ocioncosta-lindamenina.ogg",
                MediaTypes.MediaTypeOggAudio,
                MediaTypes.MediaTypeOggAudio,
                TimeSpan.FromSeconds(152),
                TimeSpan.FromMilliseconds(200));
        }

        /// <summary>
        /// The converts ogg to wav correctly.
        /// </summary>
        [TestMethod]
        public void ConvertsOggToWavCorrectly()
        {
            ConvertsCorrectly(
                "ocioncosta-lindamenina.ogg",
                MediaTypes.MediaTypeOggAudio,
                MediaTypes.MediaTypeWav,
                TimeSpan.FromSeconds(152),
                TimeSpan.FromMilliseconds(200));
        }

        /// <summary>
        /// The converts wv to wav correctly.
        /// </summary>
        [TestMethod]
        public void ConvertsWvToWavCorrectly()
        {
            ConvertsCorrectly(
                "Raw_audio_id_cd6e8ba1-11b4-4724-9562-f6ec893110aa.wv",
                MediaTypes.MediaTypeWavpack,
                MediaTypes.MediaTypeWav,
                TimeSpan.FromSeconds(120),
                TimeSpan.FromMilliseconds(0));
        }

        /// <summary>
        /// The converts wav to wav corectly.
        /// </summary>
        [TestMethod]
        public void ConvertsWavToWavCorectly()
        {
            ConvertsCorrectly(
                "Lewins Rail Kekkek.wav",
                MediaTypes.MediaTypeWav,
                MediaTypes.MediaTypeWav,
                TimeSpan.FromSeconds(60.24),
                TimeSpan.FromMilliseconds(0));
        }

        /// <summary>
        /// The converts wav to wav corectly.
        /// </summary>
        [TestMethod]
        public void ConvertsWebmToWavCorectly()
        {
            ConvertsCorrectly(
                "Lewins Rail Kekkek.webm",
                MediaTypes.MediaTypeWebMAudio,
                MediaTypes.MediaTypeWav,
                TimeSpan.FromSeconds(60.24),
                TimeSpan.FromMilliseconds(0));
        }

        /// <summary>
        /// The converts wav to wav corectly.
        /// </summary>
        [TestMethod]
        public void ConvertsWebmToWebmCorectly()
        {
            ConvertsCorrectly(
                "Lewins Rail Kekkek.webm",
                MediaTypes.MediaTypeWebMAudio,
                MediaTypes.MediaTypeWebMAudio,
                TimeSpan.FromSeconds(60.71),
                TimeSpan.FromMilliseconds(20));
        }

        /// <summary>
        /// The detects media type file ext mismatch.
        /// </summary>
        [TestMethod]
        public void DetectsMediaTypeFileExtMismatch()
        {
            var combined = GetAudioUtility();

            TestHelper.ExceptionMatches<ArgumentException>(
                () =>
                combined.Duration(
                    TestHelper.GetTestAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3"), MediaTypes.MediaTypeWav),
                "does not match File extension");
        }

        /// <summary>
        /// The one is one.
        /// </summary>
        [TestMethod]
        public void OneIsOne()
        {
            using (ConsoleRedirector cr = new ConsoleRedirector())
            {
                Assert.IsFalse(cr.ToString().Contains("New text"));

                /* call some method that writes "New text" to stdout */
                Console.Write("New text");
                Assert.IsTrue(cr.ToString().Contains("New text"));
            }
        }

        /// <summary>
        /// The rejects existing but incorrect exe paths.
        /// </summary>
        [TestMethod]
        public void RejectsExistingButIncorrectExePaths()
        {
            TestHelper.ExceptionMatches<ArgumentException>(
                () => new FfmpegAudioUtility(GetAudioUtilityExe(AppConfigHelper.WvunpackExe)),
                "Expected file name to be");

            TestHelper.ExceptionMatches<ArgumentException>(
                () => new Mp3SpltAudioUtility(GetAudioUtilityExe(AppConfigHelper.FfmpegExe)), "Expected file name to be");

            TestHelper.ExceptionMatches<ArgumentException>(
                () => new WavPackAudioUtility(GetAudioUtilityExe(AppConfigHelper.Mp3SpltExe)),
                "Expected file name to be");

            TestHelper.ExceptionMatches<ArgumentException>(
                () => new SoxAudioUtility(GetAudioUtilityExe(AppConfigHelper.Mp3SpltExe)), "Expected file name to be");
        }

        /// <summary>
        /// The rejects not existing file.
        /// </summary>
        [TestMethod]
        public void RejectsNotExistingFile()
        {
            var combined = GetAudioUtility();

            TestHelper.ExceptionMatches<ArgumentException>(
                () => combined.Duration(TestHelper.GetTestAudioFile("does not exist.wav"), MediaTypes.MediaTypeWav),
                "Could not find source file");
        }

        /// <summary>
        /// The rejects unknown file ext.
        /// </summary>
        [TestMethod]
        public void RejectsUnknownFileExt()
        {
            var combined = GetAudioUtility();

            TestHelper.ExceptionMatches<ArgumentException>(
                () =>
                combined.Duration(
                    TestHelper.GetTestAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.unknown"),
                    MediaTypes.MediaTypeBin),
                "is not recognised");
        }

        /// <summary>
        /// The rejects unknown media type.
        /// </summary>
        [TestMethod]
        public void RejectsUnknownMediaType()
        {
            var combined = GetAudioUtility();

            TestHelper.ExceptionMatches<ArgumentException>(
                () =>
                combined.Duration(
                    TestHelper.GetTestAudioFile("Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3"), "garbage in garbage out"),
                " does not match File extension");
        }

        /// <summary>
        /// The segments mp 3 correctly.
        /// </summary>
        [TestMethod]
        public void SegmentsMp3Correctly()
        {
            SegmentsCorrectly(
                "Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3",
                MediaTypes.MediaTypeMp3,
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(2),
                TimeSpan.FromMilliseconds(50));
        }

        /// <summary>
        /// The segments ogg correctly.
        /// </summary>
        [TestMethod]
        public void SegmentsOggCorrectly()
        {
            SegmentsCorrectly(
                "ocioncosta-lindamenina.ogg",
                MediaTypes.MediaTypeOggAudio,
                TimeSpan.FromSeconds(50),
                TimeSpan.FromSeconds(110),
                TimeSpan.FromMilliseconds(20));
        }

        /// <summary>
        /// The segments wav correctly.
        /// </summary>
        [TestMethod]
        public void SegmentsWavCorrectly()
        {
            SegmentsCorrectly(
                "Lewins Rail Kekkek.wav",
                MediaTypes.MediaTypeWav,
                TimeSpan.FromSeconds(20),
                TimeSpan.FromSeconds(50),
                TimeSpan.FromMilliseconds(0));
        }

        /// <summary>
        /// The segments wwebm correctly.
        /// </summary>
        [TestMethod]
        public void SegmentsWebmCorrectly()
        {
            SegmentsCorrectly(
                "Lewins Rail Kekkek.webm",
                MediaTypes.MediaTypeWebMAudio,
                TimeSpan.FromSeconds(20),
                TimeSpan.FromSeconds(50),
                TimeSpan.FromMilliseconds(500));
        }

        /// <summary>
        /// The segments wma correctly.
        /// </summary>
        [TestMethod]
        public void SegmentsWmaCorrectly()
        {
            SegmentsCorrectly(
                "06Sibylla.wma",
                MediaTypes.MediaTypeWma,
                TimeSpan.FromSeconds(15),
                TimeSpan.FromSeconds(85),
                TimeSpan.FromMilliseconds(330)); //110
        }

        /// <summary>
        /// The segments wv correctly.
        /// </summary>
        [TestMethod]
        public void SegmentsWvCorrectly()
        {
            // not able to segment from .wv to .wv - no way to compress .wav to .wv
            SegmentsCorrectly(
                "Raw_audio_id_cd6e8ba1-11b4-4724-9562-f6ec893110aa.wv",
                MediaTypes.MediaTypeWavpack,
                TimeSpan.FromSeconds(40),
                TimeSpan.FromSeconds(110),
                TimeSpan.FromMilliseconds(0));
        }

        /// <summary>
        /// The test master utility no ff probe.
        /// </summary>
        [TestMethod]
        public void TestMasterUtilityNoFfProbe()
        {
            GetAudioUtilityNoFfprobe();
        }

        /// <summary>
        /// The test master utility no sox.
        /// </summary>
        [TestMethod]
        public void TestMasterUtilityNoSox()
        {
            GetAudioUtilityNoSox();
        }

        /// <summary>
        /// The test sox.
        /// </summary>
        [TestMethod]
        public void TestSox()
        {
            GetAudioUtility().Info(TestHelper.GetTestAudioFile("TorresianCrow.wav"));
        }

        /// <summary>
        /// The validates non existing exe paths.
        /// </summary>
        [TestMethod]
        public void ValidatesNonExistingExePaths()
        {
            var randomFile = new FileInfo(@"X:\hello-my-dear\where-are-you\hey-its-adirectory\blah.exe");

            TestHelper.ExceptionMatches<FileNotFoundException>(
                () => new FfmpegAudioUtility(randomFile), "Could not find exe");

            TestHelper.ExceptionMatches<FileNotFoundException>(
                () => new Mp3SpltAudioUtility(randomFile), "Could not find exe");

            TestHelper.ExceptionMatches<FileNotFoundException>(
                () => new WavPackAudioUtility(randomFile), "Could not find exe");

            TestHelper.ExceptionMatches<FileNotFoundException>(
                () => new SoxAudioUtility(randomFile), "Could not find exe");
        }

        /// <summary>
        /// The validates null exe paths.
        /// </summary>
        [TestMethod]
        public void ValidatesNullExePaths()
        {
            TestHelper.ExceptionMatches<ArgumentNullException>(
                () => new FfmpegAudioUtility(null), "Value cannot be null");

            TestHelper.ExceptionMatches<ArgumentNullException>(
                () => new Mp3SpltAudioUtility(null), "Value cannot be null");

            TestHelper.ExceptionMatches<ArgumentNullException>(
                () => new WavPackAudioUtility(null), "Value cannot be null");

            TestHelper.ExceptionMatches<ArgumentNullException>(() => new SoxAudioUtility(null), "Value cannot be null");
        }

        #endregion

        #region Methods

        private static void CalculatesCorrectDurationTest(
            string filename, string mediatype, TimeSpan expectedDuration, TimeSpan range)
        {
            foreach (var combined in new[] { GetAudioUtility(), GetAudioUtilityNoFfprobe(), GetAudioUtilityNoSox() })
            {
                var duration = combined.Duration(TestHelper.GetTestAudioFile(filename), mediatype);
                var info = GetDurationInfo(combined.Info(TestHelper.GetTestAudioFile(filename)));

                var compareResult = "Expected duration " + expectedDuration + " actual duration " + duration
                                    + " expected max variation " + range + " actual variation "
                                    + expectedDuration.Subtract(duration).Duration();

                using (ConsoleRedirector cr = new ConsoleRedirector())
                {
                    Console.WriteLine(compareResult);
                }

                Assert.IsTrue(
                    TestHelper.CompareTimeSpans(duration, expectedDuration, range), compareResult + ". Info: " + info);
            }
        }

        private static void ConvertsCorrectly(
            string filename, string mimetype, string outputMimeType, TimeSpan expectedDuration, TimeSpan maxVariance)
        {
            foreach (var util in new[] { GetAudioUtility(), GetAudioUtilityNoFfprobe(), GetAudioUtilityNoSox() })
            {
                var dir = TestHelper.GetTempDir();
                var output =
                    new FileInfo(
                        Path.Combine(
                            dir.FullName,
                            Path.GetFileNameWithoutExtension(filename) + "_converted."
                            + MediaTypes.GetExtension(outputMimeType)));

                var audioUtilRequest = new AudioUtilityRequest { };

                var input = TestHelper.GetTestAudioFile(filename);

                util.Segment(input, mimetype, output, outputMimeType, audioUtilRequest);

                var outputduration = util.Duration(output, outputMimeType);
                var infoInput = GetDurationInfo(util.Info(input));
                var infoOutput = GetDurationInfo(util.Info(output));

                var compareResult = "Expected duration " + expectedDuration + " actual duration " + outputduration
                                    + " expected max variation " + maxVariance + " actual variation "
                                    + expectedDuration.Subtract(outputduration).Duration();

                using (ConsoleRedirector cr = new ConsoleRedirector())
                {
                    Console.WriteLine(compareResult);
                }

                Assert.IsTrue(
                    TestHelper.CompareTimeSpans(expectedDuration, outputduration, maxVariance),
                    compareResult + ". Info input: " + infoInput + "." + Environment.NewLine + "Info output: "
                    + infoOutput);

                var info = util.Info(output);
                TestHelper.DeleteTempDir(dir);

                /*
                var sb = new StringBuilder();
                foreach (var item in info)
                {
                    sb.AppendLine(item.Key + ": " + item.Value);
                }
                */
                if (info != null && info.ContainsKey("STREAM codec_long_name"))
                {
                    var codec = info["STREAM codec_long_name"];

                    if (outputMimeType == MediaTypes.MediaTypeWav)
                    {
                        Assert.IsTrue(codec == MediaTypes.CodecWavPcm16BitLe);
                    }
                    else if (outputMimeType == MediaTypes.MediaTypeOggAudio)
                    {
                        Assert.IsTrue(codec == MediaTypes.CodecVorbis);
                    }
                    else if (outputMimeType == MediaTypes.MediaTypeMp3)
                    {
                        Assert.IsTrue(codec == MediaTypes.CodecMp3);
                    }
                    else if (outputMimeType == MediaTypes.MediaTypeWebMAudio)
                    {
                        Assert.IsTrue(codec == MediaTypes.CodecVorbis);
                    }
                    else
                    {
                        Assert.IsTrue(codec == MediaTypes.ExtUnknown);
                    }
                }
            }
        }

        private static IAudioUtility GetAudioUtility()
        {
            var baseresourcesdir = TestHelper.GetResourcesBaseDir().FullName;

            var ffmpegExe = new FileInfo(Path.Combine(baseresourcesdir, AppConfigHelper.FfmpegExe));
            var ffprobeExe = new FileInfo(Path.Combine(baseresourcesdir, AppConfigHelper.FfprobeExe));
            var mp3SpltExe = new FileInfo(Path.Combine(baseresourcesdir, AppConfigHelper.Mp3SpltExe));
            var wvunpackExe = new FileInfo(Path.Combine(baseresourcesdir, AppConfigHelper.WvunpackExe));
            var soxExe = new FileInfo(Path.Combine(baseresourcesdir, AppConfigHelper.SoxExe));

            var ffmpeg = new FfmpegAudioUtility(ffmpegExe, ffprobeExe);
            var mp3Splt = new Mp3SpltAudioUtility(mp3SpltExe);
            var wvunpack = new WavPackAudioUtility(wvunpackExe);
            var sox = new SoxAudioUtility(soxExe);

            return new MasterAudioUtility(ffmpeg, mp3Splt, wvunpack, sox);
        }

        private static FileInfo GetAudioUtilityExe(string name)
        {
            var baseresourcesdir = TestHelper.GetResourcesBaseDir().FullName;
            var exe = new FileInfo(Path.Combine(baseresourcesdir, name));
            return exe;
        }

        private static IAudioUtility GetAudioUtilityNoFfprobe()
        {
            var baseresourcesdir = TestHelper.GetResourcesBaseDir().FullName;

            var ffmpegExe = new FileInfo(Path.Combine(baseresourcesdir, AppConfigHelper.FfmpegExe));
            var mp3SpltExe = new FileInfo(Path.Combine(baseresourcesdir, AppConfigHelper.Mp3SpltExe));
            var wvunpackExe = new FileInfo(Path.Combine(baseresourcesdir, AppConfigHelper.WvunpackExe));
            var soxExe = new FileInfo(Path.Combine(baseresourcesdir, AppConfigHelper.SoxExe));

            var ffmpeg = new FfmpegAudioUtility(ffmpegExe);
            var mp3Splt = new Mp3SpltAudioUtility(mp3SpltExe);
            var wvunpack = new WavPackAudioUtility(wvunpackExe);
            var sox = new SoxAudioUtility(soxExe);

            return new MasterAudioUtility(ffmpeg, mp3Splt, wvunpack, sox);
        }

        private static IAudioUtility GetAudioUtilityNoSox()
        {
            var baseresourcesdir = TestHelper.GetResourcesBaseDir().FullName;

            var ffmpegExe = new FileInfo(Path.Combine(baseresourcesdir, AppConfigHelper.FfmpegExe));
            var ffprobeExe = new FileInfo(Path.Combine(baseresourcesdir, AppConfigHelper.FfprobeExe));
            var mp3SpltExe = new FileInfo(Path.Combine(baseresourcesdir, AppConfigHelper.Mp3SpltExe));
            var wvunpackExe = new FileInfo(Path.Combine(baseresourcesdir, AppConfigHelper.WvunpackExe));

            var ffmpeg = new FfmpegAudioUtility(ffmpegExe, ffprobeExe);
            var mp3Splt = new Mp3SpltAudioUtility(mp3SpltExe);
            var wvunpack = new WavPackAudioUtility(wvunpackExe);

            return new MasterAudioUtility(ffmpeg, mp3Splt, wvunpack, null);
        }

        private static string GetDurationInfo(Dictionary<string, string> info)
        {
            var durationText = string.Join(
                ", ",
                info.Where(
                    l => l.Key.ToLowerInvariant().Contains("duration") || l.Key.ToLowerInvariant().Contains("length")));

            using (ConsoleRedirector cr = new ConsoleRedirector())
            {
                Console.WriteLine(durationText);
            }

            return durationText;
        }

        private static void SegmentsCorrectly(
            string filename, string mimetype, TimeSpan start, TimeSpan end, TimeSpan maxVariance)
        {
            foreach (var util in new[] { GetAudioUtility(), GetAudioUtilityNoFfprobe(), GetAudioUtilityNoSox() })
            {
                var dir = TestHelper.GetTempDir();

                var destMimeType = mimetype;
                if (mimetype == MediaTypes.MediaTypeWavpack)
                {
                    destMimeType = MediaTypes.MediaTypeWav;
                }

                var output =
                    new FileInfo(
                        Path.Combine(
                            dir.FullName,
                            Path.GetFileNameWithoutExtension(filename) + "_segmented."
                            + MediaTypes.GetExtension(destMimeType)));

                var audioUtilRequest = new AudioUtilityRequest { OffsetStart = start, OffsetEnd = end };

                var input = TestHelper.GetTestAudioFile(filename);
                util.Segment(input, mimetype, output, destMimeType, audioUtilRequest);

                var duration = util.Duration(output, destMimeType);
                var infoInput = GetDurationInfo(util.Info(input));
                var infoOutput = GetDurationInfo(util.Info(output));

                var compareResult = "Expected duration " + (end - start) + " actual duration " + duration
                                    + " expected max variation " + maxVariance + " actual variation "
                                    + (end - start).Subtract(duration).Duration();

                using (ConsoleRedirector cr = new ConsoleRedirector())
                {
                    Console.WriteLine(compareResult);
                }

                Assert.IsTrue(
                    TestHelper.CompareTimeSpans(duration, end - start, maxVariance),
                    compareResult + ". Info input: " + infoInput + "." + Environment.NewLine + "Info output: "
                    + infoOutput);

                TestHelper.DeleteTempDir(dir);
            }
        }

        #endregion
    }
}