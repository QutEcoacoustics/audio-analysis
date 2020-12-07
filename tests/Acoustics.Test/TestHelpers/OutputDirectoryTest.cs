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
        private DirectoryInfo dailyOutputDirectory = null;

        public static DirectoryInfo ResultsDirectory { get; private set; } = PathHelper.ClassOutputDirectory();

        public TestContext TestContext { get; set; }

        /// <summary>
        /// Gets a directory that is shared by all tests in the current class.
        /// e.g. C:\Work\Github\audio-analysis\TestResults\Deploy_Anthony 2020-02-27 16_56_40\In\Acoustics.Shared.ImageTests.
        /// </summary>
        protected DirectoryInfo ClassOutputDirectory =>
            this.classOutputDirectory ??= PathHelper.ClassOutputDirectory(this.TestContext);

        /// <summary>
        /// Gets a directory scoped to only the current test
        /// e.g. C:\Work\Github\audio-analysis\TestResults\Deploy_Anthony 2020-02-27 16_56_40\In\Acoustics.Shared.ImageTests\TestImageTest.
        /// </summary>
        protected DirectoryInfo TestOutputDirectory =>
            this.testOutputDirectory ??= PathHelper.TestOutputDirectory(this.TestContext);

        /// <summary>
        /// Gets a directory that has results grouped by day.
        /// </summary>
        protected DirectoryInfo DailyOutputDirectory =>
            this.dailyOutputDirectory ??= PathHelper.DailyOutputDirectory(this.TestContext);

        /// <summary>
        /// Save a test result.
        /// Also saves copies of test results to daily output directories.
        /// The callback provides the output directory to save your file to.
        /// You need to return the full path of the file saved.
        /// </summary>
        protected FileInfo SaveTestOutput(Func<DirectoryInfo, FileInfo> callback)
        {
            var savedFile = callback.Invoke(this.TestOutputDirectory);

            if (!savedFile.Exists)
            {
                throw new InvalidOperationException("You must return the full path of the file that was saved.");
            }

            this.TestContext.AddResultFile(savedFile.FullName);

            // if we're  on the CI the DO NOT save the file to a daily work folder
            if (!TestHelper.OnContinuousIntegrationServer)
            {
                var newName = PathHelper.DailyOutputFileNamePrefix(this.TestContext) + savedFile.Name;
                var newPath = this
                    .DailyOutputDirectory
                    .CombinePath(newName);
                savedFile.CopyTo(newPath, overwrite: true);
            }

            return savedFile;
        }

        /// <summary>
        /// Save a test result.
        /// Also saves copies of test results to daily output directories.
        /// </summary>
        protected FileInfo SaveTestOutput(Func<DirectoryInfo, string> callback)
        {
            return this.SaveTestOutput(directory => callback.Invoke(directory)?.ToFileInfo());
        }
    }
}