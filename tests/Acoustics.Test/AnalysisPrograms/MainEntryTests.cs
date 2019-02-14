// <copyright file="MainEntryTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Accord.Math.Optimization;
    using Acoustics.Shared;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisPrograms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    [DoNotParallelize]
    public class MainEntryTests
    {
        [DoNotParallelize]
        [TestMethod]
        public async Task DefaultCliWorks()
        {
            using (var console = new ConsoleRedirector())
            {
                var code = await MainEntry.Main(Array.Empty<string>());

                Assert.AreEqual(2, code);

                this.AssertContainsCopyright(console.Lines);
                this.AssertContainsGitHashAndVersion(console.Lines);
            }
        }

        [DoNotParallelize]
        [TestMethod]
        public async Task DefaultHelpWorks()
        {
            using (var console = new ConsoleRedirector())
            {
                var code = await MainEntry.Main(new[] { "--help" });

                Assert.AreEqual(0, code);

                this.AssertContainsCopyright(console.Lines);
                this.AssertContainsGitHashAndVersion(console.Lines);
                StringAssert.StartsWith(console.Lines[6], Meta.Description);
            }
        }

        [DoNotParallelize]
        [TestMethod]
        public async Task DefaultVersionWorks()
        {
            using (var console = new ConsoleRedirector())
            {
                var code = await MainEntry.Main(new[] { "--version" });

                Assert.AreEqual(0, code);

                this.AssertContainsCopyright(console.Lines);
                this.AssertContainsGitHashAndVersion(console.Lines);
                StringAssert.StartsWith(console.Lines[3], BuildMetadata.VersionString);
            }
        }

        [DoNotParallelize]
        [TestMethod]
        public async Task CheckEnvironmentWorks()
        {
            using (var console = new ConsoleRedirector())
            {
                var code = await MainEntry.Main(new[] { "CheckEnvironment" });

                Assert.AreEqual(0, code);

                this.AssertContainsCopyright(console.Lines);
                this.AssertContainsGitHashAndVersion(console.Lines);
                StringAssert.Contains(console.Lines[4], "SUCCESS - Valid environment");
            }
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
