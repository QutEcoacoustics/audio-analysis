// <copyright file="LdSpectrogramStitching.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.IO;
    using System.Linq;
    using global::AudioAnalysisTools.LongDurationSpectrograms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    [TestClass]
    public class LdSpectrogramStitchingTests
    {
        private DirectoryInfo outputDirectory;

        [TestInitialize]
        public void Setup()
        {
            this.outputDirectory = PathHelper.GetTempDir();

        }

        [TestCleanup]
        public void Cleanup()
        {
            PathHelper.DeleteTempDir(this.outputDirectory);
        }

        [TestMethod]
        public void TestDirectoryScannerMethod()
        {
            // setup
            var testDirs = new[] { "a", "a/b", "a/c", "a/c/d", "e", "e/b", "e/g", "e/g/h", "e/i" };
            var test = testDirs.Select(p =>
            {
                var directory = this.outputDirectory.Combine(p);
                directory.Create();
                return directory;
            });

            var expected = test.Where(d => d.FullName.Contains(@"\b\")).ToArray();
            var actual = LdSpectrogramStitching.GetSubDirectoriesForSiteData(expected, "b", SearchOption.AllDirectories);

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
