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
        /// The converts mp 3 to mp 3 corectly.
        /// </summary>
        [TestMethod]
        public void ConvertsMp3ToMp3Corectly()
        {
            var expected = new AudioUtilityInfo
            {
                ChannelCount = 1,
                SampleRate = 22050,
                Duration = TimeSpan.FromSeconds(240.031),
                BitsPerSecond = 96000,
                MediaType = MediaTypes.MediaTypeMp3
            };

            Modify(
                "Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3",
                expected,
                new AudioUtilityRequest { },
                MediaTypes.MediaTypeMp3,
                expected);
        }

        /// <summary>
        /// The converts mp 3 to wav correctly.
        /// </summary>
        [TestMethod]
        public void ConvertsMp3ToWavCorrectly()
        {
            var expected = new AudioUtilityInfo
            {
                ChannelCount = 1,
                SampleRate = 22050,
                Duration = TimeSpan.FromSeconds(240.031),
                BitsPerSecond = 96000,
                MediaType = MediaTypes.MediaTypeWav
            };

            Modify(
                "Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3",
                expected,
                new AudioUtilityRequest { },
                MediaTypes.MediaTypeWav,
                expected);
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
                TimeSpan.FromMilliseconds(5));
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
                TimeSpan.FromMilliseconds(10));
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
                TimeSpan.FromMilliseconds(30));
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
                LoggedConsole.Write("New text");
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
                () => new FfmpegAudioUtility(GetAudioUtilityExe(AppConfigHelper.WvunpackExe), GetAudioUtilityExe(AppConfigHelper.WvunpackExe)),
                "Expected file name to contain ");

            TestHelper.ExceptionMatches<ArgumentException>(
                () => new Mp3SpltAudioUtility(GetAudioUtilityExe(AppConfigHelper.FfmpegExe)), "Expected file name to contain ");

            TestHelper.ExceptionMatches<ArgumentException>(
                () => new WavPackAudioUtility(GetAudioUtilityExe(AppConfigHelper.Mp3SpltExe)),
                "Expected file name to contain ");

            TestHelper.ExceptionMatches<ArgumentException>(
                () => new SoxAudioUtility(GetAudioUtilityExe(AppConfigHelper.Mp3SpltExe)), "Expected file name to contain ");
        }

        /// <summary>
        /// The rejects not existing file.
        /// </summary>
        [TestMethod]
        public void RejectsNotExistingFile()
        {
            var combined = GetAudioUtility();

            TestHelper.ExceptionMatches<ArgumentException>(
                () => combined.Info(TestHelper.GetTestAudioFile("does not exist.wav")),
                "File does not exist");
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
                () => new FfmpegAudioUtility(randomFile, randomFile), "Could not find exe");

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
                () => new FfmpegAudioUtility(null,null), "Value cannot be null");

            TestHelper.ExceptionMatches<ArgumentNullException>(
                () => new Mp3SpltAudioUtility(null), "Value cannot be null");

            TestHelper.ExceptionMatches<ArgumentNullException>(
                () => new WavPackAudioUtility(null), "Value cannot be null");

            TestHelper.ExceptionMatches<ArgumentNullException>(() => new SoxAudioUtility(null), "Value cannot be null");
        }

        #endregion

        #region Methods

        private static FileInfo GetAudioUtilityExe(string name)
        {
            var baseresourcesdir = TestHelper.GetResourcesBaseDir().FullName;
            var exe = new FileInfo(Path.Combine(baseresourcesdir, name));
            return exe;
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

        private static void CalculatesCorrectDurationTest(
            string filename, string mediatype, TimeSpan expectedDuration, TimeSpan range)
        {
            foreach (var combined in new[] { GetAudioUtility() })
            {
                var utilInfo = combined.Info(TestHelper.GetTestAudioFile(filename));
                var info = GetDurationInfo(utilInfo);

                var compareResult = "Expected duration " + expectedDuration + " actual duration " + utilInfo.Duration
                                    + " expected max variation " + range + " actual variation "
                                    + expectedDuration.Subtract(utilInfo.Duration.HasValue ? utilInfo.Duration.Value : TimeSpan.Zero).Duration();

                using (ConsoleRedirector cr = new ConsoleRedirector())
                {
                    LoggedConsole.WriteLine(compareResult);
                }

                Assert.IsTrue(
                    TestHelper.CompareTimeSpans(
                        utilInfo.Duration.HasValue ? utilInfo.Duration.Value : TimeSpan.Zero, expectedDuration, range),
                    compareResult + ". Info: " + info);
            }
        }

        private static void ConvertsCorrectly(
            string filename, string mimetype, string outputMimeType, TimeSpan expectedDuration, TimeSpan maxVariance)
        {
            foreach (var util in new[] { GetAudioUtility() })
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

                util.Modify(input, mimetype, output, outputMimeType, audioUtilRequest);

                var utilInfoInput = util.Info(input);
                var utilInfoOutput = util.Info(output);
                var infoInput = GetDurationInfo(util.Info(input));
                var infoOutput = GetDurationInfo(util.Info(output));

                var compareResult = "Expected duration " + expectedDuration + " actual duration "
                                    + utilInfoOutput.Duration + " expected max variation " + maxVariance
                                    + " actual variation "
                                    +
                                    expectedDuration.Subtract(
                                        utilInfoOutput.Duration.HasValue ? utilInfoOutput.Duration.Value : TimeSpan.Zero)
                                        .Duration();

                using (ConsoleRedirector cr = new ConsoleRedirector())
                {
                    LoggedConsole.WriteLine(compareResult);
                }

                Assert.IsTrue(
                    TestHelper.CompareTimeSpans(expectedDuration, utilInfoOutput.Duration.Value, maxVariance),
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

                if (info != null && info.RawData != null && info.RawData.ContainsKey("STREAM codec_long_name"))
                {
                    var codec = info.RawData["STREAM codec_long_name"];

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

        private static string GetDurationInfo(AudioUtilityInfo info)
        {
            var durationText = string.Join(
                ", ",
                info.RawData.Where(
                    l => l.Key.ToLowerInvariant().Contains("duration") || l.Key.ToLowerInvariant().Contains("length")));

            if (info.Duration.HasValue)
            {
                durationText += ", Duration: " + info.Duration;
                durationText = durationText.Trim(' ', ',');
            }

            //using (var cr = new ConsoleRedirector())
            //{
            //    LoggedConsole.WriteLine(durationText);
            //}

            return durationText;
        }

        private static void SegmentsCorrectly(
            string filename, string mimetype, TimeSpan start, TimeSpan end, TimeSpan maxVariance)
        {
            foreach (var util in new[] { GetAudioUtility()})
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
                util.Modify(input, mimetype, output, destMimeType, audioUtilRequest);

                var utilInfoInput = util.Info(input);
                var utilInfoOutput = util.Info(output);
                var infoInput = GetDurationInfo(utilInfoInput);
                var infoOutput = GetDurationInfo(utilInfoOutput);

                var compareResult = "Expected duration " + (end - start) + " actual duration " + utilInfoOutput.Duration.Value
                                    + " expected max variation " + maxVariance + " actual variation "
                                    + (end - start).Subtract(utilInfoOutput.Duration.Value).Duration();

                using (var cr = new ConsoleRedirector())
                {
                    LoggedConsole.WriteLine(compareResult);
                }

                Assert.IsTrue(
                    TestHelper.CompareTimeSpans(utilInfoOutput.Duration.Value, end - start, maxVariance),
                    compareResult + ". Info input: " + infoInput + "." + Environment.NewLine + "Info output: "
                    + infoOutput);

                TestHelper.DeleteTempDir(dir);
            }
        }

        private static void Modify(string filename, AudioUtilityInfo sourceExpected, AudioUtilityRequest request, string outputMimeType, AudioUtilityInfo outputExpected)
        {
            var source = TestHelper.GetTestAudioFile(filename);

            var destExtension = MediaTypes.GetExtension(outputMimeType);
            var outputFilename = Path.GetFileNameWithoutExtension(filename) + "_modified." + destExtension;

            foreach (var util in new[] { GetAudioUtility() })
            {
                var dir = TestHelper.GetTempDir();
                var output = new FileInfo(Path.Combine(dir.FullName, outputFilename));

                util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, outputMimeType, request);

                var sourceInfo = util.Info(source);

                CheckSource(source, sourceExpected, sourceInfo);

                var outputInfo = util.Info(output);
                var outputInfoText = GetDurationInfo(outputInfo);


                TestHelper.DeleteTempDir(dir);
            }
        }

        private static void CheckSource(FileInfo audioFile, AudioUtilityInfo expected, AudioUtilityInfo actual)
        {
            if (expected.BitsPerSample.HasValue && actual.BitsPerSample.HasValue)
            {
                Assert.AreEqual(expected.BitsPerSample.Value, actual.BitsPerSample.Value);
            }

            if (expected.BitsPerSample.HasValue && !actual.BitsPerSample.HasValue)
            {
                Assert.Fail("BitsPerSample");
            }

            if (!expected.BitsPerSample.HasValue && actual.BitsPerSample.HasValue)
            {
                Assert.Fail("BitsPerSample");
            }


            if (expected.BitsPerSecond.HasValue && actual.BitsPerSecond.HasValue)
            {
                Assert.AreEqual(expected.BitsPerSecond.Value, actual.BitsPerSecond.Value);
            }

            if (expected.BitsPerSecond.HasValue && !actual.BitsPerSecond.HasValue)
            {
                Assert.Fail("BitsPerSecond");
            }

            if (!expected.BitsPerSecond.HasValue && actual.BitsPerSecond.HasValue)
            {
                Assert.Fail("BitsPerSecond");
            }

            Assert.IsTrue(expected.ChannelCount.HasValue);
            Assert.IsTrue(expected.Duration.HasValue);
            Assert.IsTrue(expected.SampleRate.HasValue);

            Assert.IsTrue(actual.ChannelCount.HasValue);
            Assert.IsTrue(actual.Duration.HasValue);
            Assert.IsTrue(actual.SampleRate.HasValue);

            Assert.AreEqual(expected.ChannelCount.Value, actual.ChannelCount.Value);
            Assert.AreEqual(expected.Duration.Value, actual.Duration.Value);
            Assert.AreEqual(expected.SampleRate.Value, actual.SampleRate.Value);
        }

        

        #endregion
    }
}