namespace EcoSounds.Mvc.Tests
{
    using System;
    using System.IO;
    using System.Reflection;

    using Acoustics.Shared;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The test helper.
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Used by the Throws and DoesNotThrow methods.
        /// </summary>
        public delegate void ThrowsDelegate();

        /// <summary>
        /// Used by the Throws and DoesNotThrow methods.
        /// </summary>
        public delegate object ThrowsDelegateWithReturn();

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
        public static T ExceptionMatches<T>(ThrowsDelegate testCode, string expectedExceptionPartialString) where T : Exception
        {
            try
            {
                testCode();
            }
            catch (Exception exception)
            {
                Check(exception, typeof(T), expectedExceptionPartialString);

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
        public static T ExceptionMatches<T>(ThrowsDelegateWithReturn testCode, string expectedExceptionPartialString) where T : Exception
        {
            try
            {
                testCode();
            }
            catch (Exception exception)
            {
                Check(exception, typeof(T), expectedExceptionPartialString);

                return (T)exception;
            }

            Assert.Fail("Did not throw " + typeof(T));
            return null;
        }

        private static void Check(Exception thrownException, Type expectedException, string expectedExceptionPartialString)
        {
            Assert.AreEqual(expectedException, thrownException.GetType(),
                    "Exception type did not match expected type. " +
                    " Expected: " + expectedException,
                    " Actual: " + thrownException.GetType());

            Assert.IsFalse(String.IsNullOrWhiteSpace(expectedExceptionPartialString), "Parameter 'expectedExceptionPartialString' was null, empty or white space.");

            Assert.IsTrue(
                thrownException.ToString().Contains(expectedExceptionPartialString),
                "Exception did not contain expected exception partial string." +
                  " Expected: " + expectedExceptionPartialString +
                  " Actual: " + thrownException);
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

        public static FileInfo GetTempFile(string ext)
        {
            return new FileInfo(Path.Combine(GetTempDir().FullName, Path.GetRandomFileName().Substring(0, 9) + ext));
        }

        public static DirectoryInfo GetTempDir()
        {
            var dir = "." + Path.DirectorySeparatorChar + Path.GetRandomFileName();

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return new DirectoryInfo(dir);
        }

        public static void DeleteTempDir(DirectoryInfo dir)
        {
            var baseDir = "." + Path.DirectorySeparatorChar;

            try
            {
                Directory.Delete(dir.FullName, true);
            }
            catch
            {

            }
        }

        public static DirectoryInfo GetResourcesBaseDir()
        {
            return AppConfigHelper.GetDir("BaseTestResourcesDir", true);
        }

        public static FileInfo GetTestAudioFile(string filename)
        {
            return
                new FileInfo(
                    Path.Combine(
                        GetResourcesBaseDir().FullName, AppConfigHelper.GetString("TestAudioDir"), filename));
        }

        public static FileInfo GetAnalysisConfigFile(string identifier)
        {
            return
               new FileInfo(
                   Path.Combine(
                       GetResourcesBaseDir().FullName, AppConfigHelper.GetString("AnalysisConfigDir"), identifier + ".cfg"));

        }

        public static void CheckAudioUtilityInfo(AudioUtilityInfo expected, AudioUtilityInfo actual)
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
                Assert.AreEqual(expected.BitsPerSecond.Value, actual.BitsPerSecond.Value, 1700);
            }

            if (expected.BitsPerSecond.HasValue && !actual.BitsPerSecond.HasValue)
            {
                Assert.Fail("BitsPerSecond");
            }

            if (!expected.BitsPerSecond.HasValue && actual.BitsPerSecond.HasValue)
            {
                Assert.Fail("BitsPerSecond");
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
            Assert.AreEqual(expected.Duration.Value.TotalMilliseconds, actual.Duration.Value.TotalMilliseconds, TimeSpan.FromMilliseconds(150).TotalMilliseconds);
            Assert.AreEqual(expected.SampleRate.Value, actual.SampleRate.Value);
        }

        public static FileInfo GetAudioFile(string filename)
        {
            var source = TestHelper.GetTestAudioFile(filename);
            return source;
        }

        public static IAudioUtility GetAudioUtility()
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

        public static IAudioUtility GetAudioUtilitySox()
        {
            var baseresourcesdir = TestHelper.GetResourcesBaseDir().FullName;

            var soxExe = new FileInfo(Path.Combine(baseresourcesdir, AppConfigHelper.SoxExe));

            var sox = new SoxAudioUtility(soxExe);

            return sox;
        }

        public static IAudioUtility GetAudioUtilityFfmpeg()
        {
            var baseresourcesdir = TestHelper.GetResourcesBaseDir().FullName;

            var ffmpegExe = new FileInfo(Path.Combine(baseresourcesdir, AppConfigHelper.FfmpegExe));
            var ffprobeExe = new FileInfo(Path.Combine(baseresourcesdir, AppConfigHelper.FfprobeExe));
           
            var ffmpeg = new FfmpegAudioUtility(ffmpegExe, ffprobeExe);

            return ffmpeg;
        }

        public static IAudioUtility GetAudioUtilityWavunpack()
        {
            var baseresourcesdir = TestHelper.GetResourcesBaseDir().FullName;

            var wavunpackExe = new FileInfo(Path.Combine(baseresourcesdir, AppConfigHelper.WvunpackExe));

            var util = new WavPackAudioUtility(wavunpackExe);

            return util;
        }

        public static IAudioUtility GetAudioUtilityMp3Splt()
        {
            var baseresourcesdir = TestHelper.GetResourcesBaseDir().FullName;

            var mp3SpltExe = new FileInfo(Path.Combine(baseresourcesdir, AppConfigHelper.Mp3SpltExe));
            
            var mp3Splt = new Mp3SpltAudioUtility(mp3SpltExe);

            return mp3Splt;
        }
    }

    internal class ConsoleRedirector : IDisposable
    {
        private StringWriter _consoleOutput = new StringWriter();
        private TextWriter _originalConsoleOutput;
        public ConsoleRedirector()
        {
            this._originalConsoleOutput = Console.Out;
            Console.SetOut(_consoleOutput);
        }
        public void Dispose()
        {
            Console.SetOut(_originalConsoleOutput);
            LoggedConsole.Write(this.ToString());
            this._consoleOutput.Dispose();
        }
        public override string ToString()
        {
            return this._consoleOutput.ToString();
        }
    }
}
