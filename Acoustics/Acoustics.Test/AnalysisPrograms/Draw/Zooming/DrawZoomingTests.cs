// <copyright file="TestAnalyzeLongRecording.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Draw.Zooming
{
    using System;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;

    using global::AnalysisPrograms;
    using global::AnalysisPrograms.AnalyseLongRecordings;
    using global::AnalysisPrograms.Draw.Zooming;
    using global::AnalysisPrograms.Production;

    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.Indices;
    using global::AudioAnalysisTools.LongDurationSpectrograms;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    [TestClass]
    public class DrawZoomingTests : OutputDirectoryTest
    {
        public static DirectoryInfo ResultsDirectory { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // calculate indices
            var recordingPath = PathHelper.ResolveAsset("Recordings", "OxleyCreek_site_1_1060_244333_20140529T081358+1000_120_0.wav");
            var configPath = PathHelper.ResolveConfigFile("Towsey.Acoustic.HiRes.yml");
            var arguments = new AnalyseLongRecording.Arguments
            {
                Source = recordingPath,
                Config = configPath,
                Output = SharedDirectory,
                TempDir = SharedDirectory.Combine("Temp"),
            };

            context.WriteLine($"{DateTime.Now} generating indices fixture data");
            MainEntry.SetLogVerbosity(LogVerbosity.Warn, true);
            AnalyseLongRecording.Execute(arguments);
            MainEntry.SetLogVerbosity(LogVerbosity.Debug, true);
            context.WriteLine($"{DateTime.Now} finished generting fixture");

            ResultsDirectory = SharedDirectory.Combine("Towsey.Acoustic");

            // do some basic checks that the indices were generated
            var listOfFiles = ResultsDirectory.EnumerateFiles().ToArray();
            Assert.AreEqual(20, listOfFiles.Length);
            var csvCount = listOfFiles.Count(f => f.Name.EndsWith(".csv"));
            Assert.AreEqual(16, csvCount);
            var jsonCount = listOfFiles.Count(f => f.Name.EndsWith(".json"));
            Assert.AreEqual(2, jsonCount);
            var pngCount = listOfFiles.Count(f => f.Name.EndsWith(".png"));
            Assert.AreEqual(2, pngCount);
        }

        /// <summary>
        /// Tests the rendering of zooming spectrograms for a minute of audio indices
        /// </summary>
        [TestMethod]
        [Timeout(45_000)]
        public void TestGenerateTiles()
        {
            // generate the zooming spectrograms
            var zoomOutput = this.outputDirectory.Combine("Zooming");
            DrawZoomingSpectrograms.Execute(
                new DrawZoomingSpectrograms.Arguments()
                {
                    Output = zoomOutput.FullName,
                    SourceDirectory = ResultsDirectory.FullName,
                    SpectrogramZoomingConfig = PathHelper.ResolveConfigFile("SpectrogramZoomingConfig.yml"),
                    ZoomAction = DrawZoomingSpectrograms.Arguments.ZoomActionType.Tile,
                });

            Assert.Fail();
        }

        /// <summary>
        /// Tests the rendering of zooming spectrograms for a minute of audio indices
        /// </summary>
        [TestMethod]
        [Timeout(45_000)]
        public void TestGenerateTilesSqlite()
        {
            // generate the zooming spectrograms
            var zoomOutput = this.outputDirectory.CombineFile("Zooming/tiles.sqlite3");
            DrawZoomingSpectrograms.Execute(
                new DrawZoomingSpectrograms.Arguments()
                {
                    Output = zoomOutput.FullName,
                    SourceDirectory = ResultsDirectory.FullName,
                    SpectrogramZoomingConfig = PathHelper.ResolveConfigFile("SpectrogramZoomingConfig.yml"),
                    ZoomAction = DrawZoomingSpectrograms.Arguments.ZoomActionType.Tile,
                });

            Assert.IsTrue(File.Exists(zoomOutput.FullName));
            Assert.Fail();
        }

       
    }
}