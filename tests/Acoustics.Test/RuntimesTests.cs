// <copyright file="RuntimesTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using Acoustics.Shared.Extensions;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisPrograms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static System.Runtime.InteropServices.OSPlatform;

    [TestClass]
    public class RuntimesTests
    {

        [RuntimeIdentifierSpecificDataTestMethod]
        [DataRow("win-x64", "ffmpeg", "audio-utils/win-x64/ffmpeg/ffmpeg.exe", true)]

        [DataRow("win-arm64", "ffmpeg", "audio-utils/win-arm64/ffmpeg/ffmpeg.exe", true)]

        [DataRow("osx-x64", "ffmpeg", "audio-utils/osx-x64/ffmpeg/ffmpeg", true)]

        [DataRow("linux-x64", "ffmpeg", "audio-utils/linux-x64/ffmpeg/ffmpeg", true)]

        [DataRow("linux-musl-x64", "ffmpeg", "audio-utils/linux-musl-x64/ffmpeg/ffmpeg", true)]

        [DataRow("linux-arm", "ffmpeg", "audio-utils/linux-arm/ffmpeg/ffmpeg", true)]

        [DataRow("linux-arm64", "ffmpeg", "audio-utils/linux-arm64/ffmpeg/ffmpeg", true)]
        public void TestRequiredDllsCopiedToBuildDir()
        {
            var buildDir = TestHelpers.PathHelper.AnalysisProgramsBuild;

            Assert.That.DirectoryExists(buildDir);

            var runtimeDir = Path.Combine(buildDir, "runtimes");

            var expected = new string[]
            {
                "linux-arm/native/libe_sqlite3.so",
                "linux-armel/native/libe_sqlite3.so",
                "linux-x64/native/libe_sqlite3.so",
                "linux-x86/native/libe_sqlite3.so",
                "osx-x64/native/libe_sqlite3.dylib",
            };

            foreach (var directory in expected)
            {
                Assert.That.FileExists(Path.Combine(runtimeDir, directory));
            }
        }

        [TestMethod]
        public void TestRequiredAudioFilesCopiedToBuildDir()
        {
            var buildDir = TestHelpers.PathHelper.CodeBase;

            Assert.That.DirectoryExists(buildDir);

            var runtimeDir = Path.GetFullPath(Path.Combine(buildDir, "audio-utils"));

            var expected = new[]
            {
                "osx-x64/ffmpeg",
                "osx-x64/ffmpeg/ffmpeg",
                "osx-x64/ffmpeg/ffprobe",
                "osx-x64/sox",
                "osx-x64/sox/sox",
                "win-x64/ffmpeg/ffmpeg.exe",
                "win-x64/ffmpeg/ffprobe.exe",
                "win-x64/sox/sox.exe",
                "linux-x64/ffmpeg/ffmpeg",
                "linux-x64/ffmpeg/ffprobe",
            };

            foreach (var directory in expected)
            {
                Assert.That.PathExists(Path.Combine(runtimeDir, directory));
            }
        }

        [TestMethod]
        public void TestAssemblyMetadataHasBeenGenerated()
        {
            var now = DateTimeOffset.UtcNow;
            var expectedVersion = new Regex($"{now:yy\\.M}\\.\\d+\\.\\d+");
            StringAssert.Matches(BuildMetadata.VersionString, expectedVersion);

            // We don't know the exact build date... without the mechanism we use to get the build date
            // so we're just going to detect of the modified date is close to the build date.
            // This won't fail if a build stops generating build data immediately, but the time
            // it takes to commit->push->run on ci should definitely trigger the failure.
            var generatedBuildData = Path.GetFullPath(Path.Combine(PathHelper.SolutionRoot, "src", "AssemblyMetadata.Generated.cs"));
            Debug.WriteLine(generatedBuildData);
            var actual = File.GetLastWriteTimeUtc(generatedBuildData);
            Assert.That.AreEqual(now, actual, TimeSpan.FromMinutes(60));

            var ciBuild = Environment.GetEnvironmentVariable("BUILD_BUILDID");
            Assert.AreEqual(string.IsNullOrWhiteSpace(ciBuild) ? "000" : ciBuild, BuildMetadata.CiBuild);
        }

        [PlatformSpecificTestMethod("Windows")]
        public void HasSupportForLongPaths()
        {
            var random = TestHelpers.Random.GetRandom();
            var longPath = @"\\?\C:\";
            while (longPath.Length < 1024)
            {
                longPath += random.NextGuid().ToString();
            }

            // this should fail if not supported
            var info = new FileInfo(longPath);
        }
    }
}
