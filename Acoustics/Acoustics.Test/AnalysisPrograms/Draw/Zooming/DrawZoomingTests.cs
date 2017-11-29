﻿// <copyright file="TestAnalyzeLongRecording.cs" company="QutEcoacoustics">
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
    using global::AudioAnalysisTools.LongDurationSpectrograms.Zooming;

    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    using Zio;
    using Zio.FileSystems.Community.SqliteFileSystem;

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
            MainEntry.SetLogVerbosity(LogVerbosity.Warn, false);
            AnalyseLongRecording.Execute(arguments);
            MainEntry.SetLogVerbosity(LogVerbosity.Debug, false);
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
        public void TestGenerateTilesFailsWithInvalidScales()
        {
            PathHelper.ResolveConfigFile("IndexPropertiesConfig.HiRes.yml").CopyTo(this.outputDirectory.CombineFile("IndexPropertiesConfig.HiRes.yml").FullName);

            void SetupAndRun(params double[] scales)
            {
                SpectrogramZoomingConfig config = new SpectrogramZoomingConfig();
                config.SpectralIndexScale = scales;
                config.IndexPropertiesConfig = ".\\IndexPropertiesConfig.HiRes.yml";

                var newConfigFile = this.outputDirectory.CombineFile("SpectrogramZoomingConfig.yml");
                Yaml.Serialise(newConfigFile, config);

                // generate the zooming spectrograms
                DrawZoomingSpectrograms.Execute(
                    new DrawZoomingSpectrograms.Arguments()
                        {
                            Output = this.outputDirectory.FullName,
                            SourceDirectory = ResultsDirectory.FullName,
                            SpectrogramZoomingConfig = newConfigFile,
                            ZoomAction = DrawZoomingSpectrograms.Arguments.ZoomActionType.Tile,
                        });
            }

            Assert.ThrowsException<InvalidScaleException>(() => SetupAndRun(0.125));
            Assert.ThrowsException<InvalidScaleException>(() => SetupAndRun(-3));
            Assert.ThrowsException<InvalidScaleException>(() => SetupAndRun(0));
            Assert.ThrowsException<InvalidScaleException>(() => SetupAndRun(0.01));
            Assert.ThrowsException<InvalidScaleException>(() => SetupAndRun(13.33));
        }

        /// <summary>
        /// Tests the rendering of zooming spectrograms for a minute of audio indices
        /// </summary>
        [TestMethod]
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

            var filesProduced = zoomOutput.EnumerateFiles().ToArray();

            // there are 11 zoom levels in the default config, but the test recording is only 2min long
            // at scale 240, we're rendering <1px of content and that tile is not generated
            // for scales 120-1.0 only one tile is produced (subtotal: 10)
            // for 0.5: 2 tiles (subtotal: 12)
            // for 0.25: 3 tiles (subtotal: 15)
            // for 0.125: 6 tiles (subtotal: 21)
            Assert.AreEqual(21, filesProduced.Length);
            Assert.AreEqual(6, filesProduced.Count(x => x.Name.Contains("0.125")));

            // not sure what else to test - generally exceptions should be thrown if anything goes wrong
        }

        /// <summary>
        /// Tests the rendering of zooming spectrograms for a minute of audio indices
        /// </summary>
        [TestMethod]
        //[Timeout(45_000)]
        public void TestGenerateTilesSqlite()
        {
            // generate the zooming spectrograms
            var zoomOutput = this.outputDirectory.Combine("Zooming");
            DrawZoomingSpectrograms.Execute(
                new DrawZoomingSpectrograms.Arguments()
                {
                    Output = zoomOutput.FullName,
                    OutputFormat = "sqlite3",
                    SourceDirectory = ResultsDirectory.FullName,
                    SpectrogramZoomingConfig = PathHelper.ResolveConfigFile("SpectrogramZoomingConfig.yml"),
                    ZoomAction = DrawZoomingSpectrograms.Arguments.ZoomActionType.Tile,
                });

            var tiles = zoomOutput.CombineFile("OxleyCreek_site_1_1060_244333_20140529T081358+1000_120_0__Tiles.sqlite3");
            Assert.IsTrue(tiles.Exists);

            using (var fs = new SqliteFileSystem(tiles.FullName, OpenMode.ReadOnly))
            {
                var filesProduced = fs.EnumerateFiles(UPath.Root).ToArray();

                // there are 11 zoom levels in the default config, but the test recording is only 2min long
                // at scale 240, we're rendering <1px of content and that tile is not generated
                // for scales 120-1.0 only one tile is produced (subtotal: 10)
                // for 0.5: 2 tiles (subtotal: 12)
                // for 0.25: 3 tiles (subtotal: 15)
                // for 0.125: 6 tiles (subtotal: 21)
                Assert.AreEqual(21, filesProduced.Length);
                Assert.AreEqual(6, filesProduced.Count(x => x.GetName().Contains("0.125")));
            }

            // not sure what else to test - generally exceptions should be thrown if anything goes wrong
        }
    }
}