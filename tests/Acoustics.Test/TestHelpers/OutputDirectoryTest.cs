// <copyright file="OutputDirectoryTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OutputDirectoryTest
    {
        private DirectoryInfo classOutputDirectory = null;
        private DirectoryInfo testOutputDirectory = null;

        public static DirectoryInfo ResultsDirectory { get; private set; }

        public TestContext TestContext { get; set; }

        /// <summary>
        /// Gets a directory that is shared by all tests in the current class.
        /// e.g. C:\Work\Github\audio-analysis\TestResults\Deploy_Anthony 2020-02-27 16_56_40\In\Acoustics.Shared.ImageTests
        /// </summary>
        protected DirectoryInfo ClassOutputDirectory =>
            this.classOutputDirectory ??= this.TestContext.TestResultsDirectory
                .ToDirectoryInfo()
                .CreateSubdirectory(this.TestContext.FullyQualifiedTestClassName);

        /// <summary>
        /// Gets a directory scoped to only the current test
        /// e.g. C:\Work\Github\audio-analysis\TestResults\Deploy_Anthony 2020-02-27 16_56_40\In\Acoustics.Shared.ImageTests\TestImageTest
        /// </summary>
        protected DirectoryInfo TestOutputDirectory =>
            this.testOutputDirectory ??= this.ClassOutputDirectory.CreateSubdirectory(this.TestContext.TestName);

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            ResultsDirectory = testContext.ResultsDirectory.ToDirectoryInfo();
        }
    }
}