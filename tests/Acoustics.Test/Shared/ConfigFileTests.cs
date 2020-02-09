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
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using global::AnalysisPrograms;
    using global::AudioAnalysisTools.Indices;
    using log4net.Appender;
    using log4net.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    [TestClass]
    [DoNotParallelize]
    public class ConfigFileTests : OutputDirectoryTest
    {
        private static FileInfo knownConfigFile;
        private static MemoryAppender memoryAppender;

        private FileInfo currentPath;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // for the config dumping events
            knownConfigFile = PathHelper.ResolveConfigFile("Towsey.Acoustic.yml");
            memoryAppender = TestSetup.TestLogging.MemoryAppender;
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

            // clear the log
            memoryAppender.Clear();

            // flush the cache (important to reset state between tests)
            ConfigFile.FlushCache();
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
                () => ConfigFile.ConfigFolder);

            Assert.ThrowsException<DirectoryNotFoundException>(
                () => ConfigFile.Resolve("whatever.yml"));
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
            var configuration = ConfigFile.Deserialize<AcousticIndices.AcousticIndicesConfig>(file);

            // we don't care so much about the value
            Assert.IsTrue(configuration.IndexCalculationDuration > 0);
            Assert.IsNotNull(configuration.ConfigPath);
            Assert.That.FileExists(configuration.ConfigPath);

            // the type should autoload indexproperties
            Assert.IsNotNull(configuration.IndexProperties);
        }

        [TestMethod]
        public void SupportForUntypedDeserializing()
        {
            var file = ConfigFile.Resolve("Towsey.Acoustic.yml");

            // this mainly tests if the machinery works
            var configuration = ConfigFile.Deserialize(file);

            // we don't care so much about the value
            Assert.IsTrue(configuration.GetDouble("IndexCalculationDuration") > 0);
            Assert.IsNotNull(configuration.ConfigPath);
            Assert.That.FileExists(configuration.ConfigPath);

            // we should not be dealing with any sub-types
            Assert.IsInstanceOfType(configuration, typeof(Config));
        }

        [TestMethod]
        public void TheDeserializeMethodsCachesConfigReads()
        {
            void AssertMessageCount(int typedCount, int untypedCount)
            {
                var messages = memoryAppender.GetEvents();
                Assert.AreEqual(typedCount, messages.Count(x => x.RenderedMessage.Contains(" typed ")));
                Assert.AreEqual(untypedCount, messages.Count(x => x.RenderedMessage.Contains(" untyped ")));
            }

            TestSetup.TestLogging.ModifyVerbosity(Level.All, quietConsole: true);

            // this should be a fresh read
            var configuration1 = ConfigFile.Deserialize<AcousticIndices.AcousticIndicesConfig>(knownConfigFile);

            // index properties should get loaded as well
            AssertMessageCount(2, 0);

            // but not this, it was already read as a "typed" variant
            var configuration2 = ConfigFile.Deserialize(knownConfigFile);

            AssertMessageCount(2, 0);

            // this should be pulled from the cache
            var configuration3 = ConfigFile.Deserialize<AcousticIndices.AcousticIndicesConfig>(knownConfigFile);

            AssertMessageCount(2, 0);

            // so should this
            var configuration4 = ConfigFile.Deserialize(knownConfigFile);

            AssertMessageCount(2, 0);

            // none of them should be the same object
            Assert.AreNotSame(configuration1, configuration2);
            Assert.AreNotSame(configuration1, configuration3);
            Assert.AreNotSame(configuration3, configuration4);
            Assert.AreNotSame(configuration2, configuration4);

            // they all should have values
            Assert.AreEqual(60.0, configuration1.IndexCalculationDuration);
            Assert.AreEqual(60.0, configuration2.GetDouble(nameof(AcousticIndices.AcousticIndicesConfig.IndexCalculationDuration)));
            Assert.AreEqual(60.0, configuration3.IndexCalculationDuration);
            Assert.AreEqual(60.0, configuration4.GetDouble(nameof(AcousticIndices.AcousticIndicesConfig.IndexCalculationDuration)));

            TestSetup.TestLogging.ModifyVerbosity(Level.Info, quietConsole: true);
        }

        [TestMethod]
        public void DumpsTypedConfigUsedIntoLog()
        {
            // the log should be empty
            Assert.AreEqual(0, memoryAppender.GetEvents().Length);

            // read the config file
            ConfigFile.Deserialize<AcousticIndices.AcousticIndicesConfig>(knownConfigFile);

            // the log should contain the serialized config
            var actualEvents = memoryAppender.GetEvents();

            // acoustic indices config loads another config, hence we expect two log messages
            Assert.AreEqual(2, actualEvents.Length);
            var expectedMessage = new Regex(@"Config file `.*Towsey\.Acoustic\.yml` loaded:\r?\n{"".*");
            StringAssert.Matches(actualEvents[0].RenderedMessage, expectedMessage);

            string expectedContent = "\"IndexCalculationDuration\":60.0";
            StringAssert.Contains(actualEvents[0].RenderedMessage, expectedContent);
            Assert.AreEqual(1, Regex.Matches(actualEvents[0].RenderedMessage, expectedContent).Count);
            StringAssert.DoesNotMatch(actualEvents[0].RenderedMessage, new Regex("\"RankOrder\""));

            var expectedMessage2 = new Regex(@"Config file `.*IndexPropertiesConfig\.yml` loaded:\r?\n{""RankOrder"":{.*");
            StringAssert.Matches(actualEvents[1].RenderedMessage, expectedMessage2);
        }

        [TestMethod]
        public void DumpsUntypedConfigUsedIntoLog()
        {
            // the log should be empty
            Assert.AreEqual(0, memoryAppender.GetEvents().Length);

            // read the config file
            ConfigFile.Deserialize(knownConfigFile);

            // the log should contain the serialized config
            var actualEvents = memoryAppender.GetEvents();

            // acoustic indices config normally loads another config, but since we're not using the "static"
            // config, the extra on-loaded behaviour does not happen!
            Assert.AreEqual(1, actualEvents.Length);
            var expectedMessage = new Regex(@"Config file `.*Towsey\.Acoustic\.yml` loaded:\r?\n{"".*");
            StringAssert.Matches(actualEvents[0].RenderedMessage, expectedMessage);

            // all values are strings in generic configs
            string expectedContent = "\"IndexCalculationDuration\":\"60.0\"";
            StringAssert.Contains(actualEvents[0].RenderedMessage, expectedContent);
            Assert.AreEqual(1, Regex.Matches(actualEvents[0].RenderedMessage, expectedContent).Count);
            StringAssert.DoesNotMatch(actualEvents[0].RenderedMessage, new Regex("\"RankOrder\""));

        }

        [TestMethod]
        public void OnlyDumpsEachConfigFileOnce()
        {
            // the log should be empty
            Assert.AreEqual(0, memoryAppender.GetEvents().Length);

            // read the config file
            ConfigFile.Deserialize<AcousticIndices.AcousticIndicesConfig>(knownConfigFile);
            ConfigFile.Deserialize(knownConfigFile);
            ConfigFile.Deserialize<AcousticIndices.AcousticIndicesConfig>(knownConfigFile);

            var actualEvents = memoryAppender.GetEvents();

            // acoustic indices config loads another config, hence we expect two log messages
            // despite how many times this config is "loaded" it's info should have only been dumped once
            Assert.AreEqual(2, actualEvents.Length);
            var expectedMessage = new Regex(@"Config file `.*Towsey\.Acoustic\.yml` loaded:\r?\n{"".*");
            StringAssert.Matches(actualEvents[0].RenderedMessage, expectedMessage);

            string expectedContent = "\"IndexCalculationDuration\":60.0";
            StringAssert.Contains(actualEvents[0].RenderedMessage, expectedContent);
            Assert.AreEqual(1, Regex.Matches(actualEvents[0].RenderedMessage, expectedContent).Count);
            StringAssert.DoesNotMatch(actualEvents[0].RenderedMessage, new Regex("\"RankOrder\""));

            var expectedMessage2 = new Regex(@"Config file `.*IndexPropertiesConfig\.yml` loaded:\r?\n{""RankOrder"":{.*");
            StringAssert.Matches(actualEvents[1].RenderedMessage, expectedMessage2);
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