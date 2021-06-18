// <copyright file="RuntimesTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;
    using Acoustics.Shared.Extensions;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisPrograms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static Acoustics.Shared.AppConfigHelper;

    [TestClass]
    [TestCategory("Runtime")]
    public class RuntimesTests
    {
        [RuntimeIdentifierSpecificDataTestMethod(RidType.CompiledIfSelfContained)]
        [DataRow(WinX64, "win-x64/native/e_sqlite3.dll")]
        [DataRow(WinArm64, "win-arm64/native/e_sqlite3.dll")]
        [DataRow(OsxX64, "osx-x64/native/libe_sqlite3.dylib")]
        [DataRow(LinuxX64, "linux-x64/native/libe_sqlite3.so")]
        [DataRow(LinuxMuslX64, "linux-musl-x64/native/libe_sqlite3.so")]
        [DataRow(LinuxArm, "linux-arm/native/libe_sqlite3.so")]
        [DataRow(LinuxArm64, "linux-arm64/native/libe_sqlite3.so")]
        public void TestRequiredSqliteLibsCopiedToBuildDir(string rid, string expected)
        {
            var buildDir = PathHelper.AnalysisProgramsBuild;

            Assert.That.DirectoryExists(buildDir);

#pragma warning disable IDE0035, CS0162
            if (BuildMetadata.CompiledAsSelfContained)
            {
                Assert.That.FileExists(Path.Combine(buildDir, Path.GetFileName(expected)));
            }
            else
            {
                Assert.That.FileExists(Path.Combine(buildDir, "runtimes", expected));
            }
#pragma warning restore IDE0035, CS0162
        }

        [RuntimeIdentifierSpecificDataTestMethod(RidType.CompiledIfSelfContained)]
        [DataRow(WinX64, "win-x64/lib/netstandard2.0/Mono.Posix.NETStandard.dll")]
        [DataRow(OsxX64, "osx-x64/lib/netstandard2.0/Mono.Posix.NETStandard.dll")]
        [DataRow(LinuxX64, "linux-x64/lib/netstandard2.0/Mono.Posix.NETStandard.dll")]
        [DataRow(LinuxArm, "linux-arm/lib/netstandard2.0/Mono.Posix.NETStandard.dll")]
        [DataRow(LinuxArm64, "linux-arm64/lib/netstandard2.0/Mono.Posix.NETStandard.dll")]
        [DataRow(LinuxMuslX64, null)]
        [DataRow(WinArm64, null)]

        [DataRow(WinX64, "win-x64/native/libMonoPosixHelper.dll")]
        [DataRow(OsxX64, "osx-x64/native/libMonoPosixHelper.dylib")]
        [DataRow(LinuxX64, "linux-x64/native/libMonoPosixHelper.so")]
        /*[DataRow(LinuxMuslX64, "linux-musl-x64/native/libMonoPosixHelper.so")]*/
        [DataRow(LinuxArm, "linux-arm/native/libMonoPosixHelper.so")]
        [DataRow(LinuxArm64, "linux-arm64/native/libMonoPosixHelper.so")]
        public void TestRequiredMonoPosixDllCopiedToBuildDir(string rid, string expected)
        {
            if (expected == null)
            {
                Assert.Inconclusive($"Mono.Posix.NETStandard.dll not expected on {rid}");
            }

            var buildDir = PathHelper.AnalysisProgramsBuild;

            Assert.That.DirectoryExists(buildDir);

#pragma warning disable IDE0035, CS0162
            if (BuildMetadata.CompiledAsSelfContained)
            {
                Assert.That.FileExists(Path.Combine(buildDir, Path.GetFileName(expected)));
            }
            else
            {
                Assert.That.FileExists(Path.Combine(buildDir, "runtimes", expected));
            }
#pragma warning restore IDE0035, CS0162
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
        public void TestRobotoFontCopiedToOutputDirectory()
        {
            var buildDir = PathHelper.CodeBase;

            Assert.That.DirectoryExists(buildDir);

            var runtimeDir = Path.GetFullPath(Path.Combine(buildDir, "fonts", "Roboto"));

            var expected = new[]
            {
                "Roboto-ThinItalic.ttf",
                "LICENSE.txt",
                "Roboto-Black.ttf",
                "Roboto-BlackItalic.ttf",
                "Roboto-Bold.ttf",
                "Roboto-BoldItalic.ttf",
                "Roboto-Italic.ttf",
                "Roboto-Light.ttf",
                "Roboto-LightItalic.ttf",
                "Roboto-Medium.ttf",
                "Roboto-MediumItalic.ttf",
                "Roboto-Regular.ttf",
                "Roboto-Thin.ttf",
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
            Trace.WriteLine(generatedBuildData);
            var actual = File.GetLastWriteTimeUtc(generatedBuildData);
            Assert.That.AreEqual(now, actual, TimeSpan.FromMinutes(5));

            var ciBuild = TestHelper.CIRunNumber;
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
            _ = new FileInfo(longPath);
        }
    }
}