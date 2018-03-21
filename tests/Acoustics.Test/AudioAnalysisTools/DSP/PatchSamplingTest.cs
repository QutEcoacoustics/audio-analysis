using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acoustics.Test.AudioAnalysisTools.DSP
{
    using System.IO;
    using Acoustics.Test.TestHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class PatchSamplingTest
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
        public void TestPatchSampling()
        {
            var outputDir = this.outputDirectory;
            var resultDir = PathHelper.ResolveAssetPath("PatchSampling");

        }
    }
}
