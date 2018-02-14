// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigFileTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the ConfigFileTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Acoustics.Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Test.TestHelpers;

    using global::AnalysisPrograms;

    using global::AudioAnalysisTools.Indices;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using MSTestExtensions;

    using Zio;

    [TestClass]
    public class ConfigFileTests
    {
        private FileInfo currentPath;

        [TestMethod]
        public void ChecksPresentWorkingDirectory()
        {
            // check for file of the same name in (relative to) working directory
            var config = this.WriteConfigFile().Name;

            var actual = ConfigFile.Resolve(config);

            Assert.AreEqual(config, actual.Name);
            Assert.IsTrue(actual.Exists);
        }

        [TestMethod]
        public void ChecksShippedConfigFiles()
        {
            // check for file of the same name in (relative to AnalysisPrograms.exe) ConfigFiles directory
            var config = "Towsey.Acoustic.yml";

            var actual = ConfigFile.Resolve(config);

            Assert.AreEqual(config, actual.Name);
            Assert.IsTrue(actual.Exists);
        }

        [TestMethod]
        public void ChecksShippedConfigFilesNested()
        {
            // check for file of the same name in (relative to AnalysisPrograms.exe) ConfigFiles directory
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nested folder", "nestedAgain!");
            var config = this.WriteConfigFile(fullPath.ToDirectoryInfo());

            var actual = ConfigFile.Resolve(config);

            Assert.AreEqual(config.FullName, actual.FullName);

            Directory.Delete(Path.Combine(fullPath, "../"), true);
        }

        [TestMethod]
        public void ConfigFolderPropertyThrowsIfDoesNotExist()
        {
            // A folder that does not exist
            ConfigFile.ConfigFolder = Path.Combine(Directory.GetCurrentDirectory(), Path.GetRandomFileName().Replace(".", string.Empty));

            Assert.ThrowsException<DirectoryNotFoundException>(
                () =>
                    {
                        var value = ConfigFile.ConfigFolder;
                    });

            Assert.ThrowsException<DirectoryNotFoundException>(
                () =>
                    {
                        var actual = ConfigFile.Resolve("whatever.yml");
                    });
        }

        [TestMethod]
        public void IfAbsolutePathReturnsSameFile()
        {
            // an absolute file path returns whatever config exists
            var config = this.WriteConfigFile(TempFileHelper.TempDir());

            var actual = ConfigFile.Resolve(config.FullName);

            Assert.AreEqual(config.FullName, actual.FullName);
        }

        [TestMethod]
        public void IfAbsolutePathReturnsSameFileEvenFailing()
        {
            // an absolute file path it should not try relative file paths!
            // the below file will not exist
            var config = TempFileHelper.NewTempFile();

            var actual = ConfigFile.TryResolve(config.FullName, null, out _);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void TheResolveMethodThrows()
        {
            string config = "doesNotExist.yml";

            Assert.ThrowsException<ConfigFileException>(() => { ConfigFile.Resolve(config); });
            Assert.ThrowsException<ConfigFileException>(
                () => { ConfigFile.Resolve(config, Environment.CurrentDirectory.ToDirectoryInfo()); });
        }

        [TestMethod]
        public void TheResolveMethodThrowsAbsolute()
        {
            string config = "C:\\doesNotExist.yml";

            Assert.ThrowsException<ConfigFileException>(() => { ConfigFile.Resolve(config); });
            Assert.ThrowsException<ConfigFileException>(
                () => { ConfigFile.Resolve(config, Environment.CurrentDirectory.ToDirectoryInfo()); });
        }

        [TestMethod]
        public void TheResolveMethodThrowsForBadInput()
        {
            Assert.ThrowsException<ConfigFileException>(() => { ConfigFile.Resolve("   "); });

            Assert.ThrowsException<ArgumentException>(() => { ConfigFile.Resolve(string.Empty); });

            Assert.ThrowsException<ArgumentException>(() => { ConfigFile.Resolve((string)null); });

            Assert.ThrowsException<ArgumentNullException>(() => { ConfigFile.Resolve((FileInfo)null); });
        }

        [TestMethod]
        public void TheResolveMethodWorksForFileInfo()
        {
            var config = this.WriteConfigFile();

            var actual = ConfigFile.Resolve(config);

            Assert.AreEqual(config.FullName, actual.FullName);
        }

        [TestMethod]
        public void TheTryMethodDoesNotThrow()
        {
            string config = "doesNotExist.yml";

            Assert.IsFalse(ConfigFile.TryResolve(config, null, out _));
        }

        [TestMethod]
        public void SupportForDefaultConfigs()
        {
            var indexProperties = ConfigFile.Default<Dictionary<string, IndexProperties>>();

            Assert.IsTrue(indexProperties.Length > 10);
        }

        [TestMethod]
        public void SupportForDeserializing()
        {
            var file = ConfigFile.Resolve("Towsey.Acoustic.yml");

            // this mainly tests if the machinery works
            var configuration = ConfigFile.Deserialize<Acoustic.AcousticIndicesConfig>(file);

            // we don't care so much about the value
            Assert.IsTrue(configuration.IndexCalculationDuration > 0);
            Assert.IsNotNull(configuration.ConfigPath);
            Assert.That.FileExists(configuration.ConfigPath);

            // the type should autoload indexproperties
            Assert.IsNotNull(configuration.IndexProperties);
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
                        "../ConfigFiles"));
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