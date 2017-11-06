// <copyright file="OutputDirectoryTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OutputDirectoryTest
    {
        protected static DirectoryInfo SharedDirectory { get; } = PathHelper.GetTempDir();

        protected DirectoryInfo outputDirectory;

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

        [ClassCleanup]
        public static void CleanupStatic()
        {
            PathHelper.DeleteTempDir(SharedDirectory);
        }
    }
}
