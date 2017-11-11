namespace Acoustics.Test.AudioAnalysisTools.TileImage
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    using global::AudioAnalysisTools.LongDurationSpectrograms;
    using global::AudioAnalysisTools.LongDurationSpectrograms.Zooming;
    using global::AudioAnalysisTools.TileImage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;
    using Zio;

    [TestClass]
    public class AbsoluteDateTimeTilerTests
    {
        private AbsoluteDateTilingProfile tilingProfile;
        private Tiler tiler;
        private DirectoryInfo outputDirectory;
        private AbsoluteDateTilingProfile tilingProfileNotRoundStart;
        private readonly DateTimeOffset dateTimeOffset = new DateTimeOffset(2015, 04, 10, 3, 30, 15, 123, TimeSpan.FromHours(10));

        [TestInitialize]
        public void Setup()
        {
            this.tilingProfile = new AbsoluteDateTilingProfile(
                "Filename",
                "Tile",
                new DateTimeOffset(2015, 04, 10, 0, 0, 0, TimeSpan.FromHours(10)),
                256,
                60);
            this.tilingProfileNotRoundStart = new AbsoluteDateTilingProfile(
                "Filename",
                "Tile",
                this.dateTimeOffset,
                256,
                60);

            this.outputDirectory = PathHelper.GetTempDir();

            this.tiler = new Tiler(
                this.outputDirectory.ToDirectoryEntry(),
                this.tilingProfile,
                new SortedSet<double>() { 60.0 },
                60.0,
                1440,
                new SortedSet<double>() { 1 },
                1.0,
                256);
        }

        [TestCleanup]
        public void Cleanup()
        {
            PathHelper.DeleteTempDir(this.outputDirectory);
        }

        [TestMethod]
        public void TestNamingPattern()
        {
            var profile = new AbsoluteDateTilingProfile("Basename", "Tag", this.dateTimeOffset, 256, 300);

            Assert.AreEqual("Basename__Tag_20150409T173015.123Z_60", profile.GetFileBaseName(this.tiler.CalculatedLayers, this.tiler.CalculatedLayers.First(), new Point(0, 0)));

            var profile2 = new AbsoluteDateTilingProfile("", "Tag", this.dateTimeOffset, 256, 300);

            Assert.AreEqual("Tag_20150409T173015.123Z_60", profile2.GetFileBaseName(this.tiler.CalculatedLayers, this.tiler.CalculatedLayers.First(), new Point(0, 0)));
        }

        [TestMethod]
        public void TestLeftPaddingInLowerLayers()
        {
            const int TileWidth = 180;
            var startDate = new DateTimeOffset(2014, 05, 29, 08,13, 58, TimeSpan.FromHours(10));
            var boundary = ZoomTiledSpectrograms.GetPreviousTileBoundary(TileWidth, 0.1, startDate);
            var padding = startDate - boundary;

            var profile = new AbsoluteDateTilingProfile(
                "Filename",
                "Tile",
                boundary,
                256,
                TileWidth);

            var tiler = new Tiler(
                            this.outputDirectory.ToDirectoryEntry(),
                            profile,
                            new SortedSet<double>() { 60.0, 0.1 },
                            60.0,
                            1440,
                            new SortedSet<double>() { 1, 1 },
                            1.0,
                            256);

            var testBitmap = new Bitmap(1200, 256);
            using (var graphics = Graphics.FromImage(testBitmap))
            {
                var points = new[]
                    {
                        new Point(0, 0), new Point(180, 256), new Point(360, 0), new Point(540, 256), new Point(720, 0),
                        new Point(900, 256), new Point(1080, 0), new Point(1260, 256),
                    };
                graphics.DrawLines(
                    Pens.Red,
                    points);
            }

            var superTile = new TimeOffsetSingleLayerSuperTile(
                padding,
                SpectrogramType.Index,
                0.1.Seconds(),
                testBitmap,
                0.Seconds());

            tiler.Tile(superTile);

            ////Debug.WriteLine(this.outputDirectory.FullName);
            ////Debug.WriteLine(this.outputDirectory.GetFiles().Length);
            var actualFiles = this.outputDirectory.GetFiles().OrderBy(x => x.Name).ToArray();

            Assert.AreEqual(8, actualFiles.Length);

            var expectedFiles = new[]
                {
                    "Filename__Tile_20140528T221348Z_0.1.png",
                    "Filename__Tile_20140528T221406Z_0.1.png",
                    "Filename__Tile_20140528T221424Z_0.1.png",
                    "Filename__Tile_20140528T221442Z_0.1.png",
                    "Filename__Tile_20140528T221500Z_0.1.png",
                    "Filename__Tile_20140528T221518Z_0.1.png",
                    "Filename__Tile_20140528T221536Z_0.1.png",
                    "Filename__Tile_20140528T221554Z_0.1.png",
                };
            var expectedImages =
                expectedFiles
                    .OrderBy(x => x)
                    .Select((x, i) => testBitmap.Crop(new Rectangle((i * TileWidth) - 100, 0, TileWidth, 256)))
                    .ToArray();

            for (var i = 0; i < expectedImages.Length; i++)
            {
                Assert.AreEqual(expectedFiles[i], actualFiles[i].Name);

                var expectedImage = expectedImages[i];
                var actualImage = Image.FromFile(actualFiles[i].FullName);
                var areEqual = TilerTests.BitmapEquals((Bitmap)expectedImage, (Bitmap)actualImage);
                Assert.IsTrue(areEqual, "Bitmaps were not equal {0}, {1}", expectedFiles[i], actualFiles[i]);
            }
        }

        [TestMethod]
        public void TestItShouldCutAndPadRightWithTransparency()
        {
            var testBitmap = new Bitmap(PathHelper.ResolveAssetPath("4c77b524-1857-4550-afaa-c0ebe5e3960a_20101013_000000+1000.ACI-ENT-EVN.png"));
            var superTile = new DefaultSuperTile() { Image = testBitmap, OffsetX = 0, Scale = 60.0 };
            this.tiler.Tile(superTile);

            ////Debug.WriteLine(this.outputDirectory.FullName);
            ////Debug.WriteLine(this.outputDirectory.GetFiles().Length);
            var producedFiles = this.outputDirectory.GetFiles().OrderBy(x => x.Name).ToArray();

            Assert.AreEqual(24, producedFiles.Length);

            var expectedImages =
                new[] { "4c77b524-1857-4550-afaa-c0ebe5e3960a_20101013_000000+1000.ACI-ENT-EVN-endtile.png" }
                    .OrderBy(x => x)
                    .Select(x => Image.FromFile(PathHelper.ResolveAssetPath(x)))
                    .ToArray();

            var producedImage = Image.FromFile(producedFiles[23].FullName);
            var areEqual = TilerTests.BitmapEquals((Bitmap)expectedImages[0], (Bitmap)producedImage);
            Assert.IsTrue(areEqual, "Bitmaps were not equal {0}, {1}", expectedImages[0], producedFiles[23].Name);
        }

        [TestMethod]
        public void TestPaddingANonBlockTime()
        {
            this.tiler = new Tiler(
                this.outputDirectory.ToDirectoryEntry(),
                this.tilingProfileNotRoundStart,
                new SortedSet<double>() { 60.0 },
                60.0,
                1440,
                new SortedSet<double>() { 1 },
                1.0,
                256);

            var testBitmap = new Bitmap(PathHelper.ResolveAssetPath("4c77b524-1857-4550-afaa-c0ebe5e3960a_20101013_000000+1000.ACI-ENT-EVN.png"));
            var superTile = new DefaultSuperTile()
            {
                Image = testBitmap,
                OffsetX = 30,

                // starts on a half hour, at 60s/px hence half a tile, hence half 60px
                Scale = 60.0,
            };
            this.tiler.Tile(superTile);

            ////Debug.WriteLine(this.outputDirectory.FullName);
            ////Debug.WriteLine(this.outputDirectory.GetFiles().Length);
            var producedFiles = this.outputDirectory.GetFiles().OrderBy(x => x.Name).ToArray();

            Assert.AreEqual(25, producedFiles.Length);

            var expectedImages =
                new[]
                {
                    "TILE_20150410_083000Z_60.00.png", "TILE_20150410_093000Z_60.00.png",
                    "TILE_20150410_103000Z_60.00.png", "TILE_20150410_113000Z_60.00.png",
                    "TILE_20150410_123000Z_60.00.png", "TILE_20150410_133000Z_60.00.png",
                    "TILE_20150410_143000Z_60.00.png", "TILE_20150410_153000Z_60.00.png",
                    "TILE_20150410_163000Z_60.00.png", "TILE_20150410_173000Z_60.00.png",
                    "TILE_20150409_173000Z_60.00.png", "TILE_20150409_183000Z_60.00.png",
                    "TILE_20150409_193000Z_60.00.png", "TILE_20150409_203000Z_60.00.png",
                    "TILE_20150409_213000Z_60.00.png", "TILE_20150409_223000Z_60.00.png",
                    "TILE_20150409_233000Z_60.00.png", "TILE_20150410_003000Z_60.00.png",
                    "TILE_20150410_013000Z_60.00.png", "TILE_20150410_023000Z_60.00.png",
                    "TILE_20150410_033000Z_60.00.png", "TILE_20150410_043000Z_60.00.png",
                    "TILE_20150410_053000Z_60.00.png", "TILE_20150410_063000Z_60.00.png",
                    "TILE_20150410_073000Z_60.00.png",
                }
                .OrderBy(x => x)
                .Select(x => Image.FromFile(PathHelper.ResolveAssetPath(x)))
                .ToArray();

            for (int i = 0; i < expectedImages
                                .ToArray().Length; i++)
            {
                var producedImage = Image.FromFile(producedFiles[i].FullName);
                var areEqual = TilerTests.BitmapEquals((Bitmap)expectedImages[i], (Bitmap)producedImage);
                Assert.IsTrue(areEqual, "Bitmaps were not equal {0}, {1}", expectedImages[i], producedFiles[i].Name);
            }
        }
    }
}