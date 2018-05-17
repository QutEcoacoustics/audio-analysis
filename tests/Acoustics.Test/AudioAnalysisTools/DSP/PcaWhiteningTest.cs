// <copyright file="PcaWhiteningTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using Accord.MachineLearning;
    using Accord.Math;
    using Accord.Statistics.Analysis;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NeuralNets;
    using TestHelpers;

    [TestClass]
    public class PcaWhiteningTest
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
        public void TestPcaWhitening()
        {
            // DO UNIT TESTING ONLY FOR PCAWHITENING
            //var expected = new double[100, 100];
            //var actual = PcaWhitening.Whitening(sonogram.Data);
            //var actual = PcaWhitening.Whitening(sequentialPatchMatrix);
            //var actual = PcaWhitening.Whitening(randomPatchMatrix);
            double[][] data =
            {
                new[] { 2.5,  2.4 },
                new[] { 0.5,  0.7 },
                new[] { 2.2,  2.9 },
                new[] { 1.9,  2.2 },
                new[] { 3.1,  3.0 },
                new[] { 2.3,  2.7 },
                new[] { 2.0,  1.6 },
                new[] { 1.0,  1.1 },
                new[] { 1.5,  1.6 },
                new[] { 1.1,  0.9 }
            };

            var method = PrincipalComponentMethod.Center;


            var pca = new PrincipalComponentAnalysis()
            {
                Method = PrincipalComponentMethod.Center,
                Whiten = true,
                //ExplainedVariance = 0.1,
            };
            pca.Learn(data);
            var i = pca.Eigenvalues;
            var j = pca.NumberOfOutputs;
            double[][] output1 = pca.Transform(data);

            pca.ExplainedVariance = 0.7;
            double[][] output2 = pca.Transform(data);
            var k = pca.NumberOfInputs;

            //Assert.AreEqual(expected, actual);

        }
    }
}
