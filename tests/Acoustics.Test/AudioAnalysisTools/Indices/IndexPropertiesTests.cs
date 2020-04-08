// <copyright file="IndexPropertiesTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Indices
{
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisBase;
    using global::AnalysisPrograms;
    using global::AudioAnalysisTools.Indices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class IndexPropertiesTests
    {
        public IndexPropertiesTests()
        {
            // setup logic here
        }

        [TestMethod]
        public void FindReturnsNullOnGivenNull()
        {
            var nullConfig = new IndexCalculateConfig()
            {
                IndexPropertiesConfig = null,
            };

            Assert.IsNull(IndexProperties.Find(nullConfig));

            var genericConfig = new AnalyzerConfig()
            {
                // property does not exist
                //IndexPropertiesConfig = string.Empty,
            };

            Assert.IsNull(IndexProperties.Find(genericConfig as IIndexPropertyReferenceConfiguration));

            // and test direct
            Assert.IsNull(IndexProperties.Find((string)null, null));
        }

        [TestMethod]
        public void FindReturnsNullOnGivenEmpty()
        {
            var emptyConfig = new IndexCalculateConfig()
            {
                IndexPropertiesConfig = string.Empty,
            };

            Assert.IsNull(IndexProperties.Find(emptyConfig));

            AnalyzerConfig genericConfig = new AcousticIndices.AcousticIndicesConfig()
            {
                IndexPropertiesConfig = string.Empty,
            };

            Assert.IsNull(IndexProperties.Find(genericConfig as IIndexPropertyReferenceConfiguration));

            // and test direct
            Assert.IsNull(IndexProperties.Find(string.Empty, null));
        }

        [TestMethod]
        public void FindWorksForAbolsutePath()
        {
            var realConfig = PathHelper.ResolveConfigFile("IndexPropertiesConfig.yml");

            var parentConfig = new IndexCalculateConfig()
            {
                IndexPropertiesConfig = realConfig.FullName,
            };

            Assert.AreEqual(realConfig.FullName, IndexProperties.Find(parentConfig).FullName);

            // and test direct
            Assert.AreEqual(realConfig.FullName, IndexProperties.Find(realConfig.FullName, null).FullName);
        }

        [TestMethod]
        public void FindWorksForRelativePath()
        {
            var parentConfigPath = PathHelper.ResolveConfigFile("Towsey.Acoustic.yml");
            var realConfig = PathHelper.ResolveConfigFile("IndexPropertiesConfig.yml");

            var parentConfig = new IndexCalculateConfig()
            {
                IndexPropertiesConfig = realConfig.Name,
                ConfigPath = parentConfigPath.FullName,
            };

            Assert.AreEqual(realConfig.FullName, IndexProperties.Find(parentConfig).FullName);

            // and test direct
            Assert.AreEqual(realConfig.FullName, IndexProperties.Find(realConfig.Name, parentConfigPath).FullName);
        }

        [TestMethod]
        public void FindFailsWithConfigFileErrorForMissing()
        {
            var parentConfigPath = PathHelper.ResolveConfigFile("Towsey.Acoustic.yml");
            var nonExisting = "blahblahblah.yml";

            var parentConfig = new IndexCalculateConfig()
            {
                IndexPropertiesConfig = nonExisting,
                ConfigPath = parentConfigPath.FullName,
            };

            Assert.ThrowsException<ConfigFileException>(
                () => IndexProperties.Find(parentConfig),
                $"The specified config file ({nonExisting}) could not be found");

            // and test direct
            Assert.ThrowsException<ConfigFileException>(
                () => IndexProperties.Find(nonExisting, parentConfigPath),
                $"The specified config file ({nonExisting}) could not be found");
        }
    }
}