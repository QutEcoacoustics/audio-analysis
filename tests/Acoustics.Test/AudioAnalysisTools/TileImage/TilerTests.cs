// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TilerTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Acoustics.Test.AudioAnalysisTools.TileImage
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using SixLabors.ImageSharp;
    using System.IO;
    using System.Linq;
    using global::AudioAnalysisTools.LongDurationSpectrograms;
    using global::AudioAnalysisTools.TileImage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using SixLabors.Primitives;
    using TestHelpers;
    using Zio;

    [TestClass]
    public class TilerTests
    {
        private PanoJsTilingProfile tilingProfile;
        private Tiler tiler;
        private DirectoryInfo outputDirectory;

        [TestInitialize]
        public void Setup()
        {
            this.tilingProfile = new PanoJsTilingProfile();

            this.outputDirectory = PathHelper.GetTempDir();

            this.tiler = new Tiler(
                this.outputDirectory.ToDirectoryEntry(),
                this.tilingProfile,
                new SortedSet<double>() { 60.0, 24, 12, 6, 2, 1 },
                60.0,
                1440,
                new SortedSet<double>() { 1, 1, 1, 1, 1, 1 },
                1.0,
                300);
        }

        [TestCleanup]
        public void Cleanup()
        {
            PathHelper.DeleteTempDir(this.outputDirectory);
        }

        [TestMethod]
        public void TestCalculatedLayers()
        {
            var layers = this.tiler.CalculatedLayers;

            Assert.AreEqual(6, layers.Count);

            var xScales = new[] { 60.0, 24, 12, 6, 2, 1 };
            var xNormalScales = new[] { 1.0, 2.5, 5, 10, 30, 60 };
            var numberOfTiles = new[] { 6, 12, 24, 48, 144, 288 };
            var layerWidths = new[] { 1440, 300 * 12, 300 * 24, 300 * 48, 300 * 144, 300 * 288 };
            var i = 0;
            foreach (var layer in layers)
            {
                Assert.AreEqual(numberOfTiles[i], layer.XTiles);
                Assert.AreEqual(1, layer.YTiles);
                Assert.AreEqual(layerWidths[i], layer.Width);
                Assert.AreEqual(300, layer.Height);
                Assert.AreEqual(xScales[i], layer.XScale);
                Assert.AreEqual(1.0, layer.YScale);
                Assert.AreEqual(i, layer.ScaleIndex);
                Assert.AreEqual(xNormalScales[i], layer.XNormalizedScale);
                Assert.AreEqual(1.0, layer.YNormalizedScale);

                i++;
            }
        }

        [TestMethod]
        public void TestTileManyGroupsTilesByScaleAndSortsByOffset()
        {
            

            ISuperTile[] testCases =
                {
                    MakeTile(60, 0),
                    MakeTile(60, 1),
                    MakeTile(30, 16),
                    MakeTile(30, 15.5),
                    MakeTile(30, 15),
                    MakeTile(120, 0),
                    MakeTile(1, 0),
                };

            List<ISuperTile> moqCurrent = new List<ISuperTile>(testCases.Length),
                             moqNext = new List<ISuperTile>(testCases.Length);
            var tilerMock = new Mock<Tiler>(
                this.outputDirectory.ToDirectoryEntry(),
                this.tilingProfile,
                new SortedSet<double>() { 60.0, 24, 12, 6, 2, 1 },
                60.0,
                1440,
                new SortedSet<double>() { 1, 1, 1, 1, 1, 1 },
                1.0,
                300);
            tilerMock.Setup(t => t.Tile(It.IsAny<ISuperTile>(), It.IsAny<ISuperTile>(), It.IsAny<ISuperTile>()))
                .Callback<ISuperTile, ISuperTile, ISuperTile>(
                    (previous, current, next) =>
                        {
                            moqCurrent.Add(current);
                            moqNext.Add(next);
                        });

            tilerMock.Object.TileMany(testCases);

            const ISuperTile empty = null;
            var expected = new[]
                               {
                                   Tuple.Create(empty, testCases[5]), Tuple.Create(testCases[5], empty),
                                   Tuple.Create(empty, empty), Tuple.Create(empty, testCases[0]),
                                   Tuple.Create(testCases[0], testCases[1]), Tuple.Create(testCases[1], empty),
                                   Tuple.Create(empty, empty), Tuple.Create(empty, testCases[4]),
                                   Tuple.Create(testCases[4], testCases[3]), Tuple.Create(testCases[3], testCases[2]),
                                   Tuple.Create(testCases[2], empty), Tuple.Create(empty, empty),
                                   Tuple.Create(empty, testCases[6]), Tuple.Create(testCases[6], empty),
                                   Tuple.Create(empty, empty),
                               };

            Assert.AreEqual(expected.Length, moqCurrent.Count);

            for (var i = 0; i < expected.Length; i++)
            {
                var expectedArgs = expected[i];

                Assert.AreEqual(expectedArgs.Item1, moqCurrent[i]);
                Assert.AreEqual(expectedArgs.Item2, moqNext[i]);
            }
        }

        private readonly ISuperTile[] superTileTestCases =
            {
                MakeTile(24, 0),
                MakeTile(24, 1),
                MakeTile(12, 16),
                MakeTile(12, 15.5),
                MakeTile(12, 15),
                MakeTile(60, 0),
                MakeTile(1.0, 0),
            };

        [TestMethod]
        public void TestTileManyCatchesRepeatedTilesA()
        {
            this.tiler.WriteImages = false;

            this.tiler.TileMany(
                this.superTileTestCases.Concat(
                    new[]
                        {
                            MakeTile(60, 0),
                        }));
        }

        [TestMethod]
        public void TestTileManyCatchesRepeatedTilesKeepsStateAfterMultipleCalls()
        {
            this.tiler.WriteImages = false;

            this.tiler.TileMany(this.superTileTestCases);

            this.tiler.Tile(MakeTile(30, 15.5));
        }

        [TestMethod]
        public void Test60Resolution()
        {
            var testBitmap = Image.Load<Rgba32>(PathHelper.ResolveAssetPath("1440px2.png"));
            var superTile = new TimeOffsetSingleLayerSuperTile(
                TimeSpan.Zero,
                SpectrogramType.Index,
                60.Seconds(),
                testBitmap,
                TimeSpan.Zero);
            this.tiler.Tile(superTile);

            ////Debug.WriteLine(this.outputDirectory.FullName);
            ////Debug.WriteLine(this.outputDirectory.GetFiles().Length);
            var producedFiles = this.outputDirectory.GetFiles();

            Assert.AreEqual(6, producedFiles.Length);

            var expectedImages =
                new[]
                    {
                        "panojstile_005_004_000.png", "panojstile_005_005_000.png", "panojstile_005_000_000.png",
                        "panojstile_005_001_000.png", "panojstile_005_002_000.png", "panojstile_005_003_000.png",
                    }
                .OrderBy(x => x)
                .Select(args => PathHelper.ResolveAssetPath(args))
                .ToArray();
            var loadedImages = expectedImages.Select(Image.Load<Rgba32>).ToArray();

            for (int i = 0; i < loadedImages.Length; i++)
            {
                var producedImage = Image.Load<Rgba32>(producedFiles[i].FullName);
                var areEqual = BitmapEquals(loadedImages[i], producedImage);
                Assert.IsTrue(areEqual, "Bitmaps were not equal {0}, {1}", expectedImages[i], producedFiles[i].Name);
            }
        }

        [TestMethod]
        public void Test1Resolution()
        {
            var testBitmap = Image.Load<Rgba32>(PathHelper.ResolveAssetPath("Farmstay_ECLIPSE3_201_scale-1.0_supertile-1.png"));
            var superTile = new TimeOffsetSingleLayerSuperTile(
                TimeSpan.Zero,
                SpectrogramType.Index,
                1.Seconds(),
                testBitmap,
                TimeSpan.FromHours(1));
            this.tiler.Tile(superTile);

            ////Debug.WriteLine(this.outputDirectory.FullName);
            ////Debug.WriteLine(this.outputDirectory.GetFiles().Length);
            var producedFiles = this.outputDirectory.GetFiles().OrderBy(x => x.Name).ToArray();

            Assert.AreEqual(12, producedFiles.Length);

            var expectedImages =
                new[]
                    {
                        "panojstile_000_012_000.png", "panojstile_000_013_000.png", "panojstile_000_014_000.png",
                        "panojstile_000_015_000.png", "panojstile_000_016_000.png", "panojstile_000_017_000.png",
                        "panojstile_000_018_000.png", "panojstile_000_019_000.png", "panojstile_000_020_000.png",
                        "panojstile_000_021_000.png", "panojstile_000_022_000.png", "panojstile_000_023_000.png",
                    }
                    .OrderBy(x => x)
                    .Select(args => PathHelper.ResolveAssetPath(args))
                    .ToArray();

            var loadedImages = expectedImages.Select(Image.Load<Rgba32>).ToArray();

            for (int i = 0; i < loadedImages.Length; i++)
            {
                var producedImage = Image.Load<Rgba32>(producedFiles[i].FullName);
                var areEqual = BitmapEquals(loadedImages[i], producedImage);
                Assert.IsTrue(areEqual, "Bitmaps were not equal {0}, {1}", expectedImages[i], producedFiles[i].Name);
            }
        }

        public static bool BitmapEquals(Image<Rgba32> bmp1, Image<Rgba32> bmp2)
        {
            if (!bmp1.Size().Equals(bmp2.Size()))
            {
                return false;
            }

            for (int x = 0; x < bmp1.Width; ++x)
            {
                for (int y = 0; y < bmp1.Height; ++y)
                {
                    var pixel1 = bmp1[x, y];
                    var pixel2 = bmp2[x, y];
                    var alphaEqual = pixel1.A == 0 && pixel2.A == 0;
                    if (pixel1 != pixel2 && !alphaEqual)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        [TestMethod]
        public void TimeSpanTest()
        {
            var timeSpan = TimeSpan.FromSeconds(0.2);

            Assert.AreEqual(TimeSpan.FromMilliseconds(200), timeSpan);

            Assert.AreEqual(0.2, timeSpan.TotalSeconds, 1.0 / (2.0 * TimeSpan.TicksPerSecond));
        }

        [TestMethod]
        public void EnsureSameTileNotRenderedTwice()
        {
            var testBitmap = Image.Load<Rgba32>(PathHelper.ResolveAssetPath("Farmstay_ECLIPSE3_201_scale-1.0_supertile-1.png"));
            var superTile = new TimeOffsetSingleLayerSuperTile(
                TimeSpan.Zero,
                SpectrogramType.Index,
                1.Seconds(),
                testBitmap,
                TimeSpan.FromHours(1));

            this.tiler.Tile(superTile);

            TestHelper.ExceptionMatches<DuplicateTileException>(
                () => { this.tiler.Tile(superTile); },
                "Tile 'panojstile_00000_00012_00000' has already been created");
        }

        [TestMethod]
        public void EnsureSameTileNotRenderedTwice_ForWeirdlyOrderedResults()
        {
            ISuperTile[] testCases =
                {
                    new TimeOffsetSingleLayerSuperTile(
                        TimeSpan.Zero,
                        SpectrogramType.Index,
                        0.16.Seconds(),
                        Image.Load<Rgba32>(PathHelper.ResolveAssetPath("60s@0.16pxps_0.png")),
                        TimeSpan.FromMinutes(0)),
                    new TimeOffsetSingleLayerSuperTile(
                        TimeSpan.Zero,
                        SpectrogramType.Index,
                        0.16.Seconds(),
                        Image.Load<Rgba32>(PathHelper.ResolveAssetPath("60s@0.16pxps_1.png")),
                        TimeSpan.FromMinutes(1)),
                };

            // scale size results in two images drawn
            var singleScaleTiler = new Tiler(
                this.outputDirectory.ToDirectoryEntry(),
                this.tilingProfile,
                new SortedSet<double>() { 0.16 },
                60.0,
                1440,
                new SortedSet<double>() { 1 },
                1.0,
                300);

            // test
            // super tile 1 -> image a , b
            // super tile 2 -> image b , c
            // second super tile fails because duplicate partial tile b
            singleScaleTiler.Tile(testCases[0]);

            TestHelper.ExceptionMatches<DuplicateTileException>(
                () => { singleScaleTiler.Tile(testCases[1]); },
                "Tile 'panojstile_00000_00001_00000' has already been created");

        }

        [TestMethod]
        public void EnsureSameTileNotRenderedTwice_ForWeirdlyOrderedResultsReversed()
        {
            ISuperTile[] testCases =
                {
                    new TimeOffsetSingleLayerSuperTile(TimeSpan.Zero, SpectrogramType.Index, 0.16.Seconds(), Image.Load<Rgba32>(PathHelper.ResolveAssetPath("60s@0.16pxps_0.png")), TimeSpan.FromMinutes(0)),
                    new TimeOffsetSingleLayerSuperTile(TimeSpan.Zero, SpectrogramType.Index, 0.16.Seconds(), Image.Load<Rgba32>(PathHelper.ResolveAssetPath("60s@0.16pxps_1.png")), TimeSpan.FromMinutes(1)),
                };

            // scale size results in two images drawn
            var singleScaleTiler = new Tiler(
                this.outputDirectory.ToDirectoryEntry(),
                this.tilingProfile,
                new SortedSet<double>() { 0.16 },
                60.0,
                1440,
                new SortedSet<double>() { 1 },
                1.0,
                300);

            // test
            // super tile 2 -> image b , c
            // super tile 1 -> image a , b
            // second super tile call fails because duplicate b
            singleScaleTiler.Tile(testCases[1]);
            TestHelper.ExceptionMatches<DuplicateTileException>(
                () => { singleScaleTiler.Tile(testCases[0]); },
                "Tile 'panojstile_00000_00001_00000' has already been created.");
        }

        [TestMethod]
        public void EnsureSameThreeTilesWrittenForTwoOddlySizedSuperTiles()
        {
            ISuperTile[] testCases =
                {
                    new TimeOffsetSingleLayerSuperTile(TimeSpan.Zero, SpectrogramType.Index, 0.16.Seconds(), Image.Load<Rgba32>(PathHelper.ResolveAssetPath("60s@0.16pxps_0.png")), TimeSpan.FromMinutes(0)),
                    new TimeOffsetSingleLayerSuperTile(TimeSpan.Zero, SpectrogramType.Index, 0.16.Seconds(), Image.Load<Rgba32>(PathHelper.ResolveAssetPath("60s@0.16pxps_1.png")), TimeSpan.FromMinutes(1)),
                };

            var singleScaleTiler = new Tiler(
                this.outputDirectory.ToDirectoryEntry(),
                this.tilingProfile,
                new SortedSet<double>() { 0.16 },
                60.0,
                1440,
                new SortedSet<double>() { 1 },
                1.0,
                300);

            // test
            // super tile 1 -> image a , b
            // super tile 2 -> image c
            singleScaleTiler.Tile(null, testCases[0], testCases[1]);
            singleScaleTiler.Tile(testCases[0], testCases[1], null);

            var producedFiles = this.outputDirectory.GetFiles().OrderBy(x => x.Name).ToArray();

            Assert.AreEqual(3, producedFiles.Length);

            var expectedImages =
                new[]
                    {
                        "panojstile_00000_00000_00000.png", "panojstile_00000_00001_00000.png", "panojstile_00000_00002_00000.png",
                    }
                .OrderBy(x => x)
                .Select(x => PathHelper.ResolveAssetPath(x))
                .ToArray();

            var loadedImages = expectedImages.Select(Image.Load<Rgba32>).ToArray();

            for (int i = 0; i < loadedImages.Length; i++)
            {
                var producedImage = Image.Load<Rgba32>(producedFiles[i].FullName);
                var areEqual = BitmapEquals(loadedImages[i], (producedImage));
                Assert.IsTrue(areEqual, "Bitmaps were not equal {0}, {1}", expectedImages[i], producedFiles[i].Name);
            }

        }

        [TestMethod]
        public void EnsureReallyShortRecordingsWork()
        {
            var testBitmap = new Image<Rgba32>(2, 300);
            testBitmap.Mutate(graphics =>
            {
                graphics.Fill(Brushes.Solid(Color.Red), testBitmap.Bounds());

            });

                var superTile = new TimeOffsetSingleLayerSuperTile(TimeSpan.Zero, SpectrogramType.Index, 60.Seconds(), testBitmap, TimeSpan.Zero);
            this.tiler.Tile(superTile);

            var producedFiles = this.outputDirectory.GetFiles();

            Assert.AreEqual(1, producedFiles.Length);

            // produced image should have 180px of transparency, 2px of color, and then 118px of transparency
            var expected = new Image<Rgba32>(300, 300);
            expected.Mutate(graphics =>
            {
                graphics.Fill(Brushes.Solid(Color.Red), new Rectangle(180, 0, 2, 300));

            });

            var expectedImages = new[] { expected };

            for (int i = 0; i < expectedImages.Length; i++)
            {
                var producedImage = Image.Load<Rgba32>(producedFiles[i].FullName);
                var areEqual = BitmapEquals(expectedImages[i], (Image<Rgba32>)producedImage);
                Assert.IsTrue(areEqual, "Bitmaps were not equal {0}, {1}", expectedImages[i], producedFiles[i].Name);
            }
        }

        /// <summary>
        /// 2B.abcd = Rect.FromLTRB(100, 100, 200, 200)
        ///
        ///    A A   B B   C C
        /// 1  b a | b a | b a
        /// 1  c d | c d | c d
        ///    ---------------
        /// 2  b a | b a | b a
        /// 2  c d | c d | c d
        ///    ---------------
        /// 3  b a | b a | b a
        /// 3  c d | c d | c d
        ///
        /// </summary>
        [TestMethod]
        public void TestGetImageParts()
        {
            var baseRectangle = new Rectangle(100, 100, 100, 100);
            this.InitializeAnswers();

            for (int i = 0; i < this.testCases.Length; i++)
            {
                var testCase = this.testCases[i];
                var fragments = Tiler.GetImageParts(baseRectangle, testCase);
                Assert.AreEqual(this.answers[i].Length, fragments.Length);

                for (int j = 0; j < fragments.Length; j++)
                {
                    var fragment = fragments[j];
                    var answer = this.answers[i][j];

                    Assert.AreEqual(answer.XBias, fragment.XBias);
                    Assert.AreEqual(answer.YBias, fragment.YBias);
                    Assert.AreEqual(answer.Fragment, fragment.Fragment);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestGetImagePartsNonInstersectingRectangle()
        {
            var baseRectangle = new Rectangle(100, 100, 100, 100);
            var otherRectangle = new Rectangle(0, 0, 50, 50);

            Tiler.GetImageParts(baseRectangle, otherRectangle);
        }

        private Rectangle[] testCases;

        private ImageComponent[][] answers;

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        private void InitializeAnswers()
        {
            // ReSharper disable InconsistentNaming
            this.testCases = new[]
                                 {
                                     Rectangle.FromLTRB(100, 100, 200, 200), // 2B.abcd
                                     Rectangle.FromLTRB(50, 100, 150, 200), // 2A.ad 2B.bc
                                     Rectangle.FromLTRB(150, 100, 250, 200), // 2B.ad 2C.bc
                                     Rectangle.FromLTRB(100, 50, 200, 150), // 1B.cd 2B.ab
                                     Rectangle.FromLTRB(100, 150, 200, 250), // 2B.cd 3B.ab
                                     Rectangle.FromLTRB(50, 50, 150, 150), // 1A.d 1B.c 2A.a 2B.b
                                     Rectangle.FromLTRB(150, 150, 250, 250), // 2B.d 2C.c 3B.a 3C.b
                                     Rectangle.FromLTRB(50, 150, 150, 250), // 2A.d 2B.c 3A.a 3B.b
                                     Rectangle.FromLTRB(150, 50, 250, 150), // 1B.d 1C.c 2B.a 2C.b
                                     Rectangle.FromLTRB(50, 50, 250, 150), // 1A.d 1B.cd 1C.c 2A.a 2B.ab 2C.b
                                     Rectangle.FromLTRB(50, 150, 250, 250), // 2A.d 2B.cd 2C.c 3A.a 3B.ab 3C.b
                                     Rectangle.FromLTRB(50, 50, 150, 250), // 1A.d 1B.c 2A.ad 2B.bc 3A.a 3B.b
                                     Rectangle.FromLTRB(150, 50, 250, 250), // 1B.d 1C.c 2B.ad 2C.bc 3B.a 3C.b
                                     Rectangle.FromLTRB(50, 50, 250, 250),

                                     // 1A.d 1B.cd 1C.c 2A.ad 2B.abcd 2C.bc 3A.a 3B.ab 3C.b
                                 };

            ImageComponent f1A_d = new ImageComponent(Rectangle.FromLTRB(50, 50, 100, 100), -1, -1),
                           f1C_c = new ImageComponent(Rectangle.FromLTRB(200, 50, 250, 100), 1, -1),
                           f1B_cd = new ImageComponent(Rectangle.FromLTRB(100, 50, 200, 100), 0, -1),
                           f2A_a = new ImageComponent(Rectangle.FromLTRB(50, 100, 100, 150), -1, 0),
                           f2B_ab = new ImageComponent(Rectangle.FromLTRB(100, 100, 200, 150), 0, 0),
                           f2C_b = new ImageComponent(Rectangle.FromLTRB(200, 100, 250, 150), 1, 0),
                           f2A_d = new ImageComponent(Rectangle.FromLTRB(50, 150, 100, 200), -1, 0),
                           f2B_cd = new ImageComponent(Rectangle.FromLTRB(100, 150, 200, 200), 0, 0),
                           f3B_ab = new ImageComponent(Rectangle.FromLTRB(100, 200, 200, 250), 0, 1),
                           f2C_c = new ImageComponent(Rectangle.FromLTRB(200, 150, 250, 200), 1, 0),
                           f3A_a = new ImageComponent(Rectangle.FromLTRB(50, 200, 100, 250), -1, 1),
                           f3C_b = new ImageComponent(Rectangle.FromLTRB(200, 200, 250, 250), 1, 1),
                           f1B_c = new ImageComponent(Rectangle.FromLTRB(100, 50, 150, 100), 0, -1),
                           f2A_ad = new ImageComponent(Rectangle.FromLTRB(50, 100, 100, 200), -1, 0),
                           f2B_bc = new ImageComponent(Rectangle.FromLTRB(100, 100, 150, 200), 0, 0),
                           f2B_ad = new ImageComponent(Rectangle.FromLTRB(150, 100, 200, 200), 0, 0),
                           f2C_bc = new ImageComponent(Rectangle.FromLTRB(200, 100, 250, 200), 1, 0),
                           f2B_b = new ImageComponent(Rectangle.FromLTRB(100, 100, 150, 150), 0, 0),
                           f2B_d = new ImageComponent(Rectangle.FromLTRB(150, 150, 200, 200), 0, 0),
                           f3B_a = new ImageComponent(Rectangle.FromLTRB(150, 200, 200, 250), 0, 1),
                           f2B_c = new ImageComponent(Rectangle.FromLTRB(100, 150, 150, 200), 0, 0),
                           f3B_b = new ImageComponent(Rectangle.FromLTRB(100, 200, 150, 250), 0, 1),
                           f1B_d = new ImageComponent(Rectangle.FromLTRB(150, 50, 200, 100), 0, -1),
                           f2B_a = new ImageComponent(Rectangle.FromLTRB(150, 100, 200, 150), 0, 0),
                           f2B_abcd = new ImageComponent(Rectangle.FromLTRB(100, 100, 200, 200), 0, 0);

            this.answers = new[]
                               {
                                   new[]
                                       {
                                           // 2B.abcd
                                           f2B_abcd,
                                       },
                                   new[]
                                       {
                                           // 2A.ad 2B.bc
                                           f2A_ad, f2B_bc,
                                       },
                                   new[]
                                       {
                                           // 2B.ad 2C.bc
                                           f2B_ad, f2C_bc,
                                       },
                                   new[]
                                       {
                                           // 1B.cd 2B.ab
                                           f1B_cd, f2B_ab,
                                       },
                                   new[]
                                       {
                                           // 2B.cd 3B.ab
                                           f2B_cd, f3B_ab,
                                       },
                                   new[]
                                       {
                                           // 1A.d 1B.c 2A.a 2B.b
                                           f1A_d, f1B_c, f2A_a, f2B_b,
                                       },
                                   new[]
                                       {
                                           // 2B.d 2C.c 3B.a 3C.b
                                           f2B_d, f2C_c, f3B_a, f3C_b,
                                       },
                                   new[]
                                       {
                                           // 2A.d 2B.c 3A.a 3B.b
                                           f2A_d, f2B_c, f3A_a, f3B_b,
                                       },
                                   new[]
                                       {
                                           // 1B.d 1C.c 2B.a 2C.b
                                           f1B_d, f1C_c, f2B_a, f2C_b,
                                       },
                                   new[]
                                       {
                                           // 1A.d 1B.cd 1C.c 2A.a 2B.ab 2C.b
                                           f1A_d, f1B_cd, f1C_c, f2A_a, f2B_ab, f2C_b,
                                       },
                                   new[]
                                       {
                                           // 2A.d 2B.cd 2C.c 3A.a 3B.ab 3C.b
                                           f2A_d, f2B_cd, f2C_c, f3A_a, f3B_ab, f3C_b,
                                       },
                                   new[]
                                       {
                                           // 1A.d 1B.c 2A.ad 2B.bc 3A.a 3B.b
                                           f1A_d, f1B_c, f2A_ad, f2B_bc, f3A_a, f3B_b,
                                       },
                                   new[]
                                       {
                                           // 1B.d 1C.c 2B.ad 2C.bc 3B.a 3C.b
                                           f1B_d, f1C_c, f2B_ad, f2C_bc, f3B_a, f3C_b,
                                       },
                                   new[]
                                       {
                                           // 1A.d 1B.cd 1C.c 2A.ad 2B.abcd 2C.bc 3A.a 3B.ab 3C.b
                                           f1A_d, f1B_cd, f1C_c, f2A_ad, f2B_abcd, f2C_bc, f3A_a, f3B_ab, f3C_b,
                                       },
                               };

            // ReSharper restore InconsistentNaming
        }

        private static TimeOffsetSingleLayerSuperTile MakeTile(double scale, double offsetMinutes)
        {
            return new TimeOffsetSingleLayerSuperTile(
                TimeSpan.Zero,
                SpectrogramType.Index,
                scale.Seconds(),
                null,
                TimeSpan.FromMinutes(offsetMinutes));
        }
    }
}