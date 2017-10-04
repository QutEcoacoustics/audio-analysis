namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Acoustics.Shared;
    using Acoustics.Shared.Contracts;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;
    using Acoustics.Tools.Wav;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MSTestExtensions;
    using static System.Environment;

    /// <summary>
    /// The test helper.
    /// </summary>
    public static class TestHelper
    {
        public static Dictionary<string, AudioUtilityInfo> AudioDetails = new Dictionary<string, AudioUtilityInfo>
        {
                {
                    "06Sibylla.asf",
                    new AudioUtilityInfo
                        {
                            Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(49.6),
                            SampleRate = 44100,
                            ChannelCount = 2,
                            BitsPerSecond = 130000,
                            MediaType = MediaTypes.MediaTypeAsf,
                        }
                },
                {
                    "Currawongs_curlew_West_Knoll_Bees_20091102-183000.mp3",
                    new AudioUtilityInfo
                        {
                            Duration = TimeSpan.FromMinutes(4) + TimeSpan.FromSeconds(0.031),
                            SampleRate = 22050,
                            ChannelCount = 1,
                            BitsPerSecond = 96000,
                            MediaType = MediaTypes.MediaTypeMp3,
                        }
                },
                {
                    "A French Fiddle Speaks.mp3",
                    new AudioUtilityInfo
                        {
                            Duration = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(41.6),
                            SampleRate = 44100,
                            ChannelCount = 2,
                            BitsPerSecond = 160000,
                            MediaType = MediaTypes.MediaTypeMp3,
                        }
                },
                {
                    "ocioncosta-lindamenina.ogg",
                    new AudioUtilityInfo
                        {
                            Duration = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(32.173),
                            SampleRate = 32000,
                            ChannelCount = 2,
                            BitsPerSecond = 84000,
                            MediaType = MediaTypes.MediaTypeOggAudio,
                        }
                },
                {
                    "Lewins Rail Kekkek.wav",
                    new AudioUtilityInfo
                        {
                            Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(0.245),
                            SampleRate = 22050,
                            ChannelCount = 1,
                            BitsPerSecond = 353000,
                            MediaType = MediaTypes.MediaTypeWav,
                            BitsPerSample = 16,
                        }
                },
                {
                    "FemaleKoala MaleKoala.wav",
                    new AudioUtilityInfo
                        {
                            Duration = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(0),
                            SampleRate = 22050,
                            ChannelCount = 1,
                            BitsPerSecond = 353000,
                            MediaType = MediaTypes.MediaTypeWav,
                            BitsPerSample = 16,
                        }
                },
                {
                    "geckos.wav",
                    new AudioUtilityInfo
                        {
                            Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(59.902),
                            SampleRate = 44100,
                            ChannelCount = 2,
                            BitsPerSecond = 1410000,
                            MediaType = MediaTypes.MediaTypeWav,
                            BitsPerSample = 16,
                        }
                },
                {
                    "Lewins Rail Kekkek.webm",
                    new AudioUtilityInfo
                        {
                            Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(0.257),
                            SampleRate = 22050,
                            ChannelCount = 1,
                            BitsPerSecond = 66400,
                            MediaType = MediaTypes.MediaTypeWebMAudio,
                            //BitsPerSample = 32 // only relevant to PCM data
                        }
                },
                {
                    "06Sibylla.wma",
                    new AudioUtilityInfo
                        {
                            Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(49.6),
                            SampleRate = 44100,
                            ChannelCount = 2,
                            BitsPerSecond = 130000,
                            MediaType = MediaTypes.MediaTypeWma,
                        }
                },
                {
                    "Raw_audio_id_cd6e8ba1-11b4-4724-9562-f6ec893110aa.wv",
                    new AudioUtilityInfo
                        {
                            Duration = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(0),
                            SampleRate = 22050,
                            ChannelCount = 1,
                            BitsPerSecond = 171000,
                            MediaType = MediaTypes.MediaTypeWavpack,
                            //BitsPerSample = 16,
                        }
                },
                {
                    "f969b39d-2705-42fc-992c-252a776f1af3_090705-0600.wv",
                    new AudioUtilityInfo
                        {
                            Duration = TimeSpan.FromMinutes(10) + TimeSpan.FromSeconds(0),
                            SampleRate = 22050,
                            ChannelCount = 1,
                            BitsPerSecond = 158000,
                            MediaType = MediaTypes.MediaTypeWavpack,
                            //BitsPerSample = 16,
                        }
                },
                {
                    "4channelsPureTones.wav",
                    new AudioUtilityInfo
                        {
                            Duration = TimeSpan.FromSeconds(60.0),
                            SampleRate = 44100,
                            ChannelCount = 4,
                            BitsPerSecond = 2822400,
                            MediaType = MediaTypes.MediaTypeWav,
                            BitsPerSample = 16,
                        }
                },
                {
                    "4channelsPureTones.flac",
                    new AudioUtilityInfo
                        {
                            Duration = TimeSpan.FromSeconds(60.0),
                            SampleRate = 44100,
                            ChannelCount = 4,
                            BitsPerSecond = 693000,
                            MediaType = MediaTypes.MediaTypeFlacAudio1,
                            BitsPerSample = 16,
                        }
                },
                {
                    "4channelsPureTones.mp3",
                    new AudioUtilityInfo
                        {
                            Duration = TimeSpan.FromSeconds(60.0),
                            SampleRate = 44100,
                            ChannelCount = 4,
                            BitsPerSecond = 125000,
                            MediaType = MediaTypes.MediaTypeMp3,
                            //BitsPerSample = 16,
                        }
                },
                {
                    "4channelsPureTones.ogg",
                    new AudioUtilityInfo
                        {
                            Duration = TimeSpan.FromSeconds(60.0),
                            SampleRate = 44100,
                            ChannelCount = 4,
                            BitsPerSecond = 694989,
                            MediaType = MediaTypes.MediaTypeOggAudio,
                            BitsPerSample = 16,
                        }
                },
                {
                    "4channelsPureTones.raw",
                    new AudioUtilityInfo()
                    {
                        // We can't actually determine this information from the file (since it is headerless)
                        // but this is the 'true' information of the file that generated this raw file.
                        Duration = TimeSpan.FromSeconds(60.0),
                        SampleRate = 44100,
                        ChannelCount = 4,
                        BitsPerSecond = 2822400,
                        MediaType = MediaTypes.MediaTypeWav1,
                        BitsPerSample = 16,
                    }
                },
                {
                    "4channelsPureTones.wv",
                    new AudioUtilityInfo
                        {
                            Duration = TimeSpan.FromSeconds(60.0),
                            SampleRate = 44100,
                            ChannelCount = 4,
                            BitsPerSecond = 1275000,
                            MediaType = MediaTypes.MediaTypeWavpack,
                            //BitsPerSample = 16,
                        }
                },
                {
                    "different_channels_tone.wav",
                    new AudioUtilityInfo
                        {
                            Duration = TimeSpan.FromSeconds(30.0),
                            SampleRate = 22050,
                            ChannelCount = 2,
                            BitsPerSecond = 706000,
                            MediaType = MediaTypes.MediaTypeWav,
                            BitsPerSample = 16,
                        }
                },
                {
                    "different_channels_tone.mp3",
                    new AudioUtilityInfo
                        {
                            Duration = TimeSpan.FromSeconds(30.07),
                            SampleRate = 22050,
                            ChannelCount = 2,
                            BitsPerSecond = 64100,
                            MediaType = MediaTypes.MediaTypeMp3,
                            //BitsPerSample = 16,
                        }
                },
                {
                    "4min test.mp3",
                    new AudioUtilityInfo
                        {
                            Duration = TimeSpan.FromSeconds(240.113),
                            SampleRate = 44100,
                            ChannelCount = 1,
                            BitsPerSecond = 128000,
                            MediaType = MediaTypes.MediaTypeMp3,
                            //BitsPerSample = 16,
                        }
                },
            };

        /// <summary>
        /// Tests that an exception is thrown, and that it is of
        /// the correct type, with the correct error message.
        /// If anything does not match what is supplied, the test fails.
        /// </summary>
        /// <param name="testCode">
        /// The test code.
        /// </param>
        /// <param name="expectedExceptionPartialString">
        /// The expected exception partial string.
        /// </param>
        /// <typeparam name="T">
        /// Expected Exception Type.
        /// </typeparam>
        /// <returns>
        /// Actual Exception, if any.
        /// </returns>
        [DebuggerHidden]
        public static T ExceptionMatches<T>(Action testCode, string expectedExceptionPartialString)
            where T : Exception
        {
            try
            {
                testCode();
            }
            catch (Exception exception)
            {
                Check(typeof(T), expectedExceptionPartialString, exception);

                return (T)exception;
            }

            Assert.Fail("Did not throw " + typeof(T));
            return null;
        }

        /// <summary>
        /// Tests that an exception is thrown, and that it is of
        /// the correct type, with the correct error message.
        /// If anything does not match what is supplied, the test fails.
        /// </summary>
        /// <param name="exceptionType">
        /// Expected Exception Type.
        /// </param>
        /// <param name="testCode">
        /// The test code.
        /// </param>
        /// <param name="expectedExceptionPartialString">
        /// The expected exception partial string.
        /// </param>
        /// <returns>
        /// Actual Exception, if any.
        /// </returns>
        public static object ExceptionMatches(Type exceptionType, Action testCode, string expectedExceptionPartialString)
        {
            Contract.Requires<ArgumentException>(
                exceptionType.IsSubclassOf(typeof(Exception)) || exceptionType == typeof(Exception),
                "must be a TypeInfo for an Exception type",
                nameof(exceptionType));

            try
            {
                testCode();
            }
            catch (Exception exception)
            {
                Check(exceptionType, expectedExceptionPartialString, exception);

                return exception;
            }

            Assert.Fail("Did not throw " + exceptionType);
            return null;
        }

        /// <summary>
        /// Tests that an exception is thrown, and that it is of
        /// the correct type, with the correct error message.
        /// If anything does not match what is supplied, the test fails.
        /// </summary>
        /// <param name="testCode">
        /// The test code.
        /// </param>
        /// <param name="expectedExceptionPartialString">
        /// The expected exception partial string.
        /// </param>
        /// <typeparam name="T">
        /// Expected Exception Type.
        /// </typeparam>
        /// <returns>
        /// Actual Exception, if any.
        /// </returns>
        public static T ExceptionMatches<T>(Func<object> testCode, string expectedExceptionPartialString)
            where T : Exception
        {
            try
            {
                testCode();
            }
            catch (Exception exception)
            {
                Check(typeof(T), expectedExceptionPartialString, exception);

                return (T)exception;
            }

            Assert.Fail("Did not throw " + typeof(T));
            return null;
        }

        /// <summary>
        /// Datetimes may not be exactly equal.
        /// </summary>
        /// <param name="dt1">
        /// First DateTime.
        /// </param>
        /// <param name="dt2">
        /// Second DateTime.
        /// </param>
        /// <returns>
        /// True if datetimes are within +-2 milliseconds of each other.
        /// </returns>
        public static bool CompareDates(DateTime dt1, DateTime dt2)
        {
            // date times loose precision
            return dt1.Year == dt2.Year && dt1.Month == dt2.Month && dt1.Day == dt2.Day && dt1.Hour == dt2.Hour
                   && dt1.Minute == dt2.Minute && dt1.Second == dt2.Second && ( // millisecond may be -+2
                                                                              dt1.Millisecond == dt2.Millisecond
                                                                              || dt1.Millisecond == dt2.Millisecond + 2
                                                                              || dt1.Millisecond == dt2.Millisecond - 2);
        }

        /// <summary>
        /// Datetimes may not be exactly equal.
        /// </summary>
        /// <param name="dt1">
        /// First DateTime.
        /// </param>
        /// <param name="dt2">
        /// Second DateTime.
        /// </param>
        /// <param name="range">
        /// Max variance in milliseconds.
        /// </param>
        /// <returns>
        /// True if datetimes are within +-<paramref name="range"/> milliseconds of each other.
        /// </returns>
        public static bool CompareDates(DateTime dt1, DateTime dt2, int range)
        {
            // date times loose precision
            return dt1.Year == dt2.Year && dt1.Month == dt2.Month && dt1.Day == dt2.Day && dt1.Hour == dt2.Hour
                   && dt1.Minute == dt2.Minute && dt1.Second == dt2.Second && ( // millisecond may be -+ range
                                                                              dt1.Millisecond == dt2.Millisecond
                                                                              ||
                                                                              dt1.Millisecond == dt2.Millisecond + range
                                                                              ||
                                                                              dt1.Millisecond == dt2.Millisecond - range);
        }

        /// <summary>
        /// TimeSpans may not be exactly equal.
        /// </summary>
        /// <param name="ts1">
        /// First TimeSpan.
        /// </param>
        /// <param name="ts2">
        /// Second TimeSpan.
        /// </param>
        /// <param name="range">
        /// Max variance.
        /// </param>
        /// <returns>
        /// True if TimeSpan are within +-<paramref name="range"/> milliseconds of each other.
        /// </returns>
        public static bool CompareTimeSpans(TimeSpan ts1, TimeSpan ts2, TimeSpan range)
        {
            return ts1 == ts2
                   ||
                   ((ts1 <= ts2 + range)
                    && (ts1 >= ts2 - range));
        }

        public static object GetValue<T>(T obj, string name)
        {
            var objType = typeof(T);

            //create reflection bindings - will be used to retrive private fields,methods or properties
            BindingFlags privateBindings = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;

            var field = objType.GetField(name, privateBindings);
            if (field != null)
            {
                var value = field.GetValue(obj);
                return value;
            }

            var property = objType.GetProperty(name, privateBindings);
            if (property != null)
            {
                var value = property.GetValue(obj, null);
                return value;
            }

            return null;
        }

        public static void SetValue<T>(T obj, string name, object newValue)
        {
            var objType = typeof(T);

            //create reflection bindings - will be used to retrive private fields,methods or properties
            BindingFlags privateBindings = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField;

            var field = objType.GetField(name, privateBindings);
            if (field != null)
            {
                field.SetValue(obj, newValue);
                return;
            }

            var property = objType.GetProperty(name, privateBindings);
            if (property != null)
            {
                property.SetValue(obj, newValue, null);
            }
        }

        public static void CheckAudioUtilityInfo(AudioUtilityInfo expected, AudioUtilityInfo actual, int epsilonDurationMilliseconds = 150)
        {
            if (expected.BitsPerSample.HasValue && actual.BitsPerSample.HasValue)
            {
                Assert.AreEqual(expected.BitsPerSample.Value, actual.BitsPerSample.Value);
            }

            if (expected.BitsPerSample.HasValue && !actual.BitsPerSample.HasValue)
            {
                Assert.Fail($"BitsPerSample failed. Expected: {expected.BitsPerSample}, actual: {actual.BitsPerSample}");
            }

            if (expected.BitsPerSecond.HasValue && actual.BitsPerSecond.HasValue)
            {
                // Sox only reports three decimal places and rounds other things
                var actualBps = (int)((double)actual.BitsPerSecond.Value).RoundToSignficantDigits(3);
                var expectedBps = (int)((double)expected.BitsPerSecond.Value).RoundToSignficantDigits(3);
                Assert.AreEqual(expectedBps, actualBps, 0);
            }

            if (expected.BitsPerSecond.HasValue && !actual.BitsPerSecond.HasValue)
            {
                Assert.Fail($"BitsPerSecond failed. Expected: {expected.BitsPerSecond}, actual: {actual.BitsPerSecond}");
            }

            Assert.IsTrue(!string.IsNullOrWhiteSpace(expected.MediaType));
            Assert.IsTrue(expected.ChannelCount.HasValue);
            Assert.IsTrue(expected.Duration.HasValue);
            Assert.IsTrue(expected.SampleRate.HasValue);

            Assert.IsTrue(!string.IsNullOrWhiteSpace(actual.MediaType));
            Assert.IsTrue(actual.ChannelCount.HasValue);
            Assert.IsTrue(actual.Duration.HasValue);
            Assert.IsTrue(actual.SampleRate.HasValue);

            Assert.AreEqual(expected.MediaType, actual.MediaType);
            Assert.AreEqual(expected.ChannelCount.Value, actual.ChannelCount.Value);
            Assert.AreEqual(expected.Duration.Value.TotalMilliseconds, actual.Duration.Value.TotalMilliseconds, TimeSpan.FromMilliseconds(epsilonDurationMilliseconds).TotalMilliseconds);
            Assert.AreEqual(expected.SampleRate.Value, actual.SampleRate.Value);
        }

        public static FileInfo GetAudioFile(string filename)
        {
            var source = PathHelper.GetTestAudioFile(filename);
            return source;
        }

        public static IAudioUtility GetAudioUtility()
        {
            var ffmpegExe = new FileInfo(AppConfigHelper.FfmpegExe);

            var ffmpeg = new FfmpegAudioUtility(ffmpegExe, new FileInfo( AppConfigHelper.FfprobeExe));
            var ffmpegRawPcm = new FfmpegRawPcmAudioUtility(ffmpegExe);
            var mp3Splt = new Mp3SpltAudioUtility(new FileInfo(AppConfigHelper.Mp3SpltExe));
            var wvunpack = new WavPackAudioUtility(new FileInfo(AppConfigHelper.WvunpackExe));
            var sox = new SoxAudioUtility(new FileInfo(AppConfigHelper.SoxExe));

            return new MasterAudioUtility(ffmpeg, mp3Splt, wvunpack, sox, ffmpegRawPcm);
        }

        public static IAudioUtility GetAudioUtilitySox()
        {
            var soxExe = PathHelper.GetExe(AppConfigHelper.SoxExe);

            var sox = new SoxAudioUtility(soxExe);

            return sox;
        }

        public static IAudioUtility GetAudioUtilityFfmpeg()
        {
            var ffmpegExe = PathHelper.GetExe(AppConfigHelper.FfmpegExe);
            var ffprobeExe = PathHelper.GetExe(AppConfigHelper.FfprobeExe);

            var ffmpeg = new FfmpegAudioUtility(ffmpegExe, ffprobeExe);

            return ffmpeg;
        }

        public static IAudioUtility GetAudioUtilityFfmpegRawPcm()
        {
            var ffmpegExe = PathHelper.GetExe(AppConfigHelper.FfmpegExe);

            var ffmpeg = new FfmpegRawPcmAudioUtility(ffmpegExe);

            return ffmpeg;
        }

        public static IAudioUtility GetAudioUtilityWavunpack()
        {
            var wavunpackExe = PathHelper.GetExe(AppConfigHelper.WvunpackExe);

            var util = new WavPackAudioUtility(wavunpackExe);

            return util;
        }

        public static IAudioUtility GetAudioUtilityMp3Splt()
        {
            var mp3SpltExe = PathHelper.GetExe(AppConfigHelper.Mp3SpltExe);

            var mp3Splt = new Mp3SpltAudioUtility(mp3SpltExe);

            return mp3Splt;
        }

        public static IAudioUtility GetAudioUtilityShntool()
        {
            var shntoolExe = PathHelper.GetExe(AppConfigHelper.ShntoolExe);

            var shntool = new ShntoolAudioUtility(shntoolExe);

            return shntool;
        }

        public static void AssertFrequencyInSignal(WavReader wavReader, double[] signal, int[] frequencies, int variance = 1)
        {
            var fft = DSP_Frames.ExtractEnvelopeAndAmplSpectrogram(signal, wavReader.SampleRate, wavReader.Epsilon, 512, 0.0);

            var histogram = SpectrogramTools.CalculateAvgSpectrumFromSpectrogram(fft.AmplitudeSpectrogram);

            var max = histogram.Max();
            double threshold = max * 0.8;
            var highBins = frequencies.Select(f => (int)(f / fft.FreqBinWidth)).ToArray();

            bool isOk = true;
            for (int bin = 0; bin < histogram.Length; bin++)
            {
                var value = histogram[bin];
                if (value > threshold)
                {
                    bool anyMatch = false;
                    foreach (var highBin in highBins)
                    {
                        if ((bin >= highBin - variance) && (bin <= highBin + variance))
                        {
                            anyMatch = true;
                            break;
                        }
                    }

                    isOk = anyMatch;
                }

                if (!isOk)
                {
                    break;
                }
            }

            BaseTest.Assert.IsTrue(isOk);
        }

        public static void WavReaderAssertions(WavReader reader, AudioUtilityInfo info)
        {
            BaseTest.Assert.AreEqual(info.ChannelCount.Value, reader.Channels);
            if (info.BitsPerSample.HasValue)
            {
                BaseTest.Assert.AreEqual(info.BitsPerSample.Value, reader.BitsPerSample);
            }

            BaseTest.Assert.AreEqual(info.SampleRate.Value, reader.SampleRate);
            BaseTest.Assert.AreEqual(info.Duration.Value.TotalMilliseconds, reader.Time.TotalMilliseconds, 150);
            BaseTest.Assert.AreEqual(
                (int)(info.Duration.Value.TotalSeconds * info.SampleRate.Value),
                reader.BlockCount,
                100);
        }

        //[DebuggerHidden]
        private static void Check(Type expectedException, string expectedExceptionPartialString, Exception actualException)
        {
            string FormatException(Exception actual)
            {
                var type = actual.GetType();
                var message = actual.Message;
                var stack = actual.StackTrace;
                return $"{type}: {message}\n{stack}";
            }

            var formattedException = FormatException(actualException);

            var error = $"Exception type did not match expected type. Expected: {expectedException}" +
                    " Actual: " + formattedException;

            Assert.AreEqual(expectedException, actualException.GetType(), error);

            if (string.IsNullOrWhiteSpace(actualException.Message))
            {
                Assert.Fail("Exception was thrown with an empty message - this is not allowed." + formattedException);
            }

            if (string.IsNullOrWhiteSpace(expectedExceptionPartialString))
            {
                var messageEmpty =
                    "Parameter \'expectedExceptionPartialString\' was null, empty or white space." +
                    $"{NewLine}Actual: {formattedException}{NewLine}";

                throw new ArgumentException(messageEmpty, nameof(expectedExceptionPartialString));
            }

            StringAssert.Contains(
                actualException.Message,
                expectedExceptionPartialString,
                $"\nException did not contain expected actual partial string.\nExpected: {expectedExceptionPartialString}");
        }
    }
}
