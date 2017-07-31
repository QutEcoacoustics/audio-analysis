// <copyright file="SegmentSettingsTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using global::AnalysisBase;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SegmentSettingsTests
    {
        private AnalysisSettings original;
        private AnalysisSettings cloned;

        [TestInitialize]
        public void Initialize()
        {
            this.original = new AnalysisSettings();

            this.original.AnalysisOutputDirectory = new DirectoryInfo("/");

            this.cloned = (AnalysisSettings)this.original.Clone();

        }

        [TestMethod]
        public void TestTempDirectoryFieldIsCloned()
        {
            Assert.AreEqual(
                this.original.AnalysisTempDirectoryFallback,
                this.cloned.AnalysisTempDirectoryFallback);
        }

        [TestMethod]
        public void EnsureClonedObjectHasDifferentId()
        {
            Assert.AreNotEqual(this.original.InstanceId, this.cloned.InstanceId);
        }

        [TestMethod]
        public void EnsureClonedObjectIsNotEquatable()
        {
            Assert.AreNotEqual(this.original, this.cloned);
        }

        [TestMethod]
        public void EnsureClonedObjectDoesNotShareReferences()
        {
            Assert.AreNotEqual(this.original.AnalysisOutputDirectory, this.cloned.AnalysisOutputDirectory);
        }

        [TestMethod]
        public void EnsureClonedObjectDoesCopyData()
        {
            Assert.AreEqual(
                this.original.AnalysisOutputDirectory.FullName,
                this.cloned.AnalysisOutputDirectory.FullName);
        }
    }
}
