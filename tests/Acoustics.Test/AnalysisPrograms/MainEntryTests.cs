// <copyright file="MainEntryTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisPrograms;
    using global::AnalysisPrograms.Production.Arguments;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static Acoustics.Shared.AppConfigHelper;

    [TestClass]
    [DoNotParallelize]
    public class MainEntryTests
    {
        [DoNotParallelize]
        [RuntimeIdentifierSpecificDataTestMethod(RidType.Compiled)]
        [TestCategory("UnsupportedAzurePipelinesPlatform")]
        [DataRow(WinX64)]
        [DataRow(OsxX64)]
        [DataRow(LinuxX64)]
        [DataRow(LinuxMuslX64)]

        //[DataRow(LinuxArm)]
        //[DataRow(LinuxArm64)]
        //[DataRow(WinArm64)]
        public async Task DefaultCliWorks()
        {
            using var console = new ConsoleRedirector();
            var code = await MainEntry.Main(Array.Empty<string>());

            Assert.AreEqual(2, code);

            this.AssertContainsCopyright(console.Lines);
            this.AssertContainsGitHashAndVersion(console.Lines);
        }

        [DoNotParallelize]
        [RuntimeIdentifierSpecificDataTestMethod(RidType.Compiled)]
        [TestCategory("UnsupportedAzurePipelinesPlatform")]
        [DataRow(WinX64)]
        [DataRow(OsxX64)]
        [DataRow(LinuxX64)]
        [DataRow(LinuxMuslX64)]

        //[DataRow(LinuxArm)]
        //[DataRow(LinuxArm64)]
        //[DataRow(WinArm64)]
        public async Task DefaultHelpWorks()
        {
            using var console = new ConsoleRedirector();
            var code = await MainEntry.Main(new[] { "--help" });

            Assert.AreEqual(0, code);

            this.AssertContainsCopyright(console.Lines);
            this.AssertContainsGitHashAndVersion(console.Lines);
            StringAssert.StartsWith(console.Lines[6], Meta.Description);
        }

        [DoNotParallelize]
        [RuntimeIdentifierSpecificDataTestMethod(RidType.Compiled)]
        [TestCategory("UnsupportedAzurePipelinesPlatform")]
        [DataRow(WinX64)]
        [DataRow(OsxX64)]
        [DataRow(LinuxX64)]
        [DataRow(LinuxMuslX64)]

        //[DataRow(LinuxArm)]
        //[DataRow(LinuxArm64)]
        //[DataRow(WinArm64)]
        public async Task DefaultVersionWorks()
        {
            using var console = new ConsoleRedirector();
            var code = await MainEntry.Main(new[] { "--version" });

            Assert.AreEqual(0, code);

            this.AssertContainsCopyright(console.Lines);
            this.AssertContainsGitHashAndVersion(console.Lines);
            StringAssert.StartsWith(console.Lines[3], BuildMetadata.VersionString);
        }

        [DoNotParallelize]
        [RuntimeIdentifierSpecificDataTestMethod(RidType.Compiled)]
        [TestCategory("UnsupportedAzurePipelinesPlatform")]
        [DataRow(WinX64)]
        [DataRow(OsxX64)]
        [DataRow(LinuxX64)]
        [DataRow(LinuxMuslX64)]

        //[DataRow(LinuxArm)]
        //[DataRow(LinuxArm64)]
        //[DataRow(WinArm64)]
        public async Task CheckEnvironmentWorks()
        {
            using var console = new ConsoleRedirector();
            var code = await MainEntry.Main(new[] { "CheckEnvironment" });

            Trace.WriteLine(console.Lines.Join(Environment.NewLine));

            Assert.AreEqual(0, code);

            this.AssertContainsCopyright(console.Lines);
            this.AssertContainsGitHashAndVersion(console.Lines);
            StringAssert.Contains(console.Lines[4], "SUCCESS - Valid environment");
        }

        [TestMethod]
        public void OptionClusteringIsDisabled()
        {
            var args = new[]
            {
                "ConcatenateIndexFiles",
                PathHelper.ResolveAssetPath("Concatenation"),
                "-o:null",

                // with option clustering enabled the -fcs actually means -f -c -s
                "-fcs",
                "foo.yml",

                // which conflicts with the -f argument here
                "-f",
                "blah",
            };

            var app = MainEntry.CreateCommandLineApplication();

            // before the code for this test was fixed an exception was thrown on the following line
            // `McMaster.Extensions.CommandLineUtils.CommandParsingException: Unexpected value 'blah' for option 'f'`
            var parseResult = app.Parse(args);

            Assert.AreEqual("blah", parseResult.SelectedCommand.Options.Single(x => x.LongName == "file-stem-name").Value());
            Assert.AreEqual("foo.yml", parseResult.SelectedCommand.Options.Single(x => x.LongName == "false-colour-spectrogram-config").Value());
            Assert.IsFalse(parseResult.SelectedCommand.ClusterOptions);
        }

        [TestMethod]
        public void HelpPagingIsDisabled()
        {
            var app = MainEntry.CreateCommandLineApplication();

            Assert.IsFalse(app.UsePagerForHelpText);
        }

        [TestMethod]
        [Ignore("This test is not valid. I can't find a way to replicate the failure on any real-world invocation of AP.exe, even with R and system2, without dotnet in env PATH")]
        public void TestConfigCanBeLoadedWithShortName()
        {
            // TODO CORE: remove when tested
            // https://github.com/QutEcoacoustics/audio-analysis/issues/241

            const string shortName = "ANALYS~1.EXE";

            Trace.WriteLine("Executing in: " + PathHelper.AnalysisProgramsBuild);
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo(shortName, $"{CheckEnvironment.CommandName} -n")
                {
                    EnvironmentVariables =
                    {
                        { MainEntry.ApDefaultLogVerbosityKey, LogVerbosity.All.ToString() },
                    },
                    WorkingDirectory = PathHelper.AnalysisProgramsBuild,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                },
            };
            process.Start();
            process.WaitForExit(milliseconds: 30_000);

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            Trace.WriteLine("Output:\n" + output);
            Trace.WriteLine("Error:\n" + error);

            StringAssert.Contains(
                output,
                "!!!IMPORTANT: Executable name is ANALYS~1.EXE and expected name is AnalysisPrograms.exe");
            Assert.IsFalse(output.Contains("ReflectionTypeLoadException"), $"Output should not contain `ReflectionTypeLoadException`.");
            Assert.IsFalse(error.Contains("ReflectionTypeLoadException"), $"Output should not contain `ReflectionTypeLoadException`.");

            Assert.AreEqual(0, process.ExitCode);
        }

        private void AssertContainsCopyright(ReadOnlyCollection<string> lines)
        {
            // copyright always on third line
            var expected = $"Copyright {DateTime.Now.Year} QUT";
            Assert.That.StringEqualWithDiff(expected, lines[2]);
        }

        private void AssertContainsGitHashAndVersion(ReadOnlyCollection<string> lines)
        {
            StringAssert.StartsWith(lines[0], Meta.Description);
            StringAssert.Contains(lines[0], BuildMetadata.VersionString);
            StringAssert.Contains(lines[0], BuildMetadata.BuildDate);
            StringAssert.Contains(lines[1], BuildMetadata.GitBranch);
            StringAssert.Contains(lines[1], BuildMetadata.GitCommit);
        }
    }
}
