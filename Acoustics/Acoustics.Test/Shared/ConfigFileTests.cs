// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigFileTests.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the ConfigFileTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Acoustics.Test.Shared
{
    using System;
    using System.IO;
    using System.Reflection;

    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using MSTestExtensions;

    [TestClass]
    public class ConfigFileTests : BaseTest
    {
        private FileInfo currentPath;

        [TestMethod]
        public void ChecksPresentWorkingDirectory()
        {
            // check for file of the same name in (relative to) working directory
            var config = this.WriteConfigFile().Name;

            var actual = ConfigFile.ResolveConfigFile(config);

            Assert.AreEqual(config, actual.Name);
            Assert.IsTrue(actual.Exists);
        }

        [TestMethod]
        public void ChecksShippedConfigFiles()
        {
            // check for file of the same name in (relative to AnalysisPrograms.exe) ConfigFiles directory
            var config = "Towsey.Acoustic.yml";

            var actual = ConfigFile.ResolveConfigFile(config);

            Assert.AreEqual(config, actual.Name);
            Assert.IsTrue(actual.Exists);
        }

        [TestMethod]
        public void ChecksShippedConfigFilesNested()
        {
            // check for file of the same name in (relative to AnalysisPrograms.exe) ConfigFiles directory
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nested folder", "nestedAgain!");
            var config = this.WriteConfigFile(fullPath.ToDirectoryInfo());

            var actual = ConfigFile.ResolveConfigFile(config);

            Assert.AreEqual(config.FullName, actual.FullName);

            Directory.Delete(Path.Combine(fullPath, "../"), true);
        }

        [TestMethod]
        public void ConfigFolderPropertyThrowsIfDoesNotExist()
        {
            // A folder that does not exist
            ConfigFile.ConfigFolder = Path.Combine(Directory.GetCurrentDirectory(), Path.GetRandomFileName().Replace(".", string.Empty));

            Assert.Throws<DirectoryNotFoundException>(
                () =>
                    {
                        var value = ConfigFile.ConfigFolder;
                    });

            Assert.Throws<DirectoryNotFoundException>(
                () =>
                    {
                        var actual = ConfigFile.ResolveConfigFile("whatever.yml");
                    });
        }

        [TestMethod]
        public void IfAbsolutePathReturnsSameFile()
        {
            // an absolute file path returns whatever config exists
            var config = this.WriteConfigFile(TempFileHelper.TempDir());

            var actual = ConfigFile.ResolveConfigFile(config.FullName);

            Assert.AreEqual(config.FullName, actual.FullName);
        }

        [TestMethod]
        public void IfAbsolutePathReturnsSameFileEvenFailing()
        {
            // an absolute file path it should not try relative file paths!
            // the below file will not exist
            var config = TempFileHelper.NewTempFile();

            FileInfo foundConfig;
            var actual = ConfigFile.TryResolveConfigFile(config.FullName, null, out foundConfig);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void TheResolveMethodThrows()
        {
            string config = "doesNotExist.yml";

            Assert.Throws<ConfigFileException>(() => { ConfigFile.ResolveConfigFile(config); });
        }

        [TestMethod]
        public void TheResolveMethodThrowsForBadInput()
        {
            Assert.Throws<ConfigFileException>(() => { ConfigFile.ResolveConfigFile("   "); });

            Assert.Throws<ConfigFileException>(() => { ConfigFile.ResolveConfigFile(string.Empty); });

            Assert.Throws<ConfigFileException>(() => { ConfigFile.ResolveConfigFile((string)null); });

            Assert.Throws<ArgumentNullException>(() => { ConfigFile.ResolveConfigFile((FileInfo)null); });
        }

        [TestMethod]
        public void TheResolveMethodWorksForFileInfo()
        {
            var config = this.WriteConfigFile();

            var actual = ConfigFile.ResolveConfigFile(config);

            Assert.AreEqual(config.FullName, actual.FullName);
        }

        [TestMethod]
        public void TheTryMethodDoesNotThrow()
        {
            string config = "doesNotExist.yml";

            FileInfo foundConfig;
            Assert.IsFalse(ConfigFile.TryResolveConfigFile(config, null, out foundConfig));
        }

        [TestCleanup]
        public void DeleteConfigFile()
        {
            this.currentPath?.Refresh();

            if (this.currentPath?.Exists ?? false)
            {
                this.currentPath.Delete();
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            // The test assembly has no notion of the default AnalysisPrograms.exe binary location.
            // We have to mock it.
            ConfigFile.ConfigFolder =
                Path.GetFullPath(
                    Path.Combine(
                        Assembly.GetExecutingAssembly().Location,
                        "../../../../../AudioAnalysis/AnalysisPrograms/bin/Debug/ConfigFiles"));
        }

        private FileInfo WriteConfigFile(DirectoryInfo directory = null)
        {
            var parent = directory ?? Directory.GetCurrentDirectory().ToDirectoryInfo();

            if (!parent.Exists)
            {
                parent.Create();
            }

            this.currentPath = TempFileHelper.NewTempFile(parent);
            this.currentPath.Create().Dispose();

            return this.currentPath;
        }
    }
}