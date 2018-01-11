﻿// <copyright file="ImageAssert.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;

    using Acoustics.Shared.Contracts;
    using Acoustics.Shared.Extensions;

    using global::TowseyLibrary;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class ImageAssert
    {
        public static void ImageIsSize(this Assert assert, int expectedWidth, int expectedHeight, Image actualImage)
        {
            Assert.AreEqual(expectedWidth, actualImage.Width, "Expected image width did not match actual image width");
            Assert.AreEqual(expectedHeight, actualImage.Height, "Expected image height did not match actual image height");
        }

        public static void PixelIsColor(this Assert assert, Point pixel, Color expectedColor, Bitmap actualImage)
        {
            var actualColor = actualImage.GetPixel(pixel.X, pixel.Y);
            Assert.AreEqual(expectedColor, actualColor, $"Expected color at pixel {pixel} did not match actual color");
        }

        /// <summary>
        /// Check whether a region in an image matches an expected color.
        /// Does a simplisitic average on each color channel and includes and inbuilt tolerance parameter.
        /// A repeating pattern can fool this test.
        /// </summary>
        /// <example>
        /// ImageAssert.ImageRegionIsColor(new Rectangle(10, 20, 100, 200), Color.FromArgb(41, 42, 31), actualImage, 1.0);
        /// </example>
        /// <param name="region">The rectangle within the image to test.</param>
        /// <param name="expectedColor">The single color you expect the region to have</param>
        /// <param name="actualImage">The image to test</param>
        /// <param name="tolerance">The tolerance allowed for each color channel</param>
        public static void ImageRegionIsColor(this Assert assert, Rectangle region, Color expectedColor, Bitmap actualImage, double tolerance = 0.0)
        {
            var width = region.Width;
            var area = region.Area();

            var red = new int[area];
            var green = new int[area];
            var blue = new int[area];
            var indices = new HashSet<int>();
            for (var x = region.Left; x < region.Right; x++)
            {
                for (var y = region.Top; y < region.Bottom; y++)
                {
                    var color = actualImage.GetPixel(x, y);

                    var i = x - region.Left;
                    var j = y - region.Top;
                    var index0 = (i * region.Height) + j;
                    if (indices.Contains(index0))
                    {
                        Debugger.Break();
                    }
                    indices.Add(index0);
                    red  [index0] = color.R;
                    green[index0] = color.G;
                    blue [index0] = color.B;
                }
            }

            var averageRed = red.Average();
            var averageBlue = blue.Average();
            var averageGreen = green.Average();

            Assert.IsTrue(
                Math.Abs(averageRed - expectedColor.R) < tolerance &&
                Math.Abs(averageGreen - expectedColor.G) < tolerance &&
                Math.Abs(averageBlue - expectedColor.B) < tolerance,
                $"Region {region} is not expected color {expectedColor} - actual averages: R={averageRed}, G={averageGreen}, B={averageBlue}");
        }

        /// <summary>
        /// Assert a certain ratio of colors are present in a region.
        /// NOT WELL TESTED.
        /// </summary>
        /// <example>
        /// var expectedColors = new Dictionary&lt;Color, double&gt;()
        ///            {
        ///                { Color.FromArgb(0, 0, 0), 0.7 },
        ///                { Color.FromArgb(255, 255, 255), 0.3 },
        ///            };
        /// ImageAssert.ImageRegionHasColors(new Rectangle(0, 24, 210, 3),  expectedColors, actualImage1, 0.07);
        /// </example>
        /// <param name="region"></param>
        /// <param name="expectedColors"></param>
        /// <param name="actualImage"></param>
        /// <param name="tolerance"></param>
        public static void ImageRegionHasColors(
            this Assert assert,
            Rectangle region,
            Dictionary<Color, double> expectedColors,
            Bitmap actualImage,
            double tolerance = 0.0)
        {
            var actualColors = ImageTools.GetColorHistogramNormalized(actualImage);

            var allColors = expectedColors.Keys.Union(actualColors.Keys);
            var genericError = $"\nExpected: {expectedColors.ToDebugString()}\nActual: {actualColors.ToDebugString()}";
            foreach (var color in allColors)
            {
                if (!expectedColors.ContainsKey(color) && actualColors[color] > tolerance)
                {
                    Assert.Fail($"Unexpected extra color {color} found in actual" + genericError);
                }

                if (!actualColors.ContainsKey(color))
                {
                    Assert.Fail($"Expected color {color} not found in actual" + genericError);
                }

                Assert.AreEqual(expectedColors[color], actualColors[color], tolerance, genericError);
            }
        }

        public static void ImageMatches(this Assert assert, Bitmap expectedImage, Bitmap actualImage, double tolerance = 0.0, string message = "")
        {
            Assert.AreEqual(expectedImage.Size, actualImage.Size);

            var expectedHistogram = ImageTools.GetColorHistogramNormalized(expectedImage);
            var actualHistogram = ImageTools.GetColorHistogramNormalized(actualImage);

            // sum the deltas
            var delta = expectedHistogram
                .Zip(actualHistogram, ValueTuple.Create)
                .Select(pair => Math.Abs(pair.Item1.Value - pair.Item2.Value))
                .Sum();

            Assert.IsTrue(delta <= tolerance, $"Images are not equal - total delta {delta} is not less than tolerance {tolerance}.\n" + message);
        }

        public static void ImageRegionIsRepeatedHorizontally(this Assert assert, Rectangle region, int repeats, int spacing, Bitmap actualImage, double tolerance = 0.0)
        {
            Contract.Requires(spacing >= 1);
            Contract.Requires(spacing >= 1);

            // extract first region
            var expected = actualImage.Clone(region, PixelFormat.DontCare);

            // extract the nect regions
            var tiles = Enumerable.Range(1, repeats)
                .Select(
                    i => actualImage.Clone(
                        new Rectangle(region.X + (spacing * i), region.Y, region.Width, region.Height),
                        PixelFormat.DontCare));

            int index = 0;
            foreach (var tile in tiles)
            {
                Assert.That.ImageMatches(expected, tile, tolerance, $"Repeat {index + 1} did not match original region");

                index++;
            }
        }

        public static void ImageColorsWellDistributed(
            this Assert assert,
            Bitmap actualImage,
            double allowedError = 0.1,
            Dictionary<Color, double> colorHistogram = null,
            string message = "")
        {
            colorHistogram = colorHistogram ?? ImageTools.GetColorHistogramNormalized(actualImage);

            var perfectColorAverage = 1.0 / colorHistogram.Count;
            var sumOfDeltas = colorHistogram.Select(x => Math.Abs(perfectColorAverage - x.Value)).Sum();
            var totalError = sumOfDeltas / colorHistogram.Count;
            Assert.IsTrue(
                totalError <= allowedError,
                $"The total error for all colors ({totalError}) is greater than the allowable limit (scaled: {allowedError}).\n" + message);
        }

    }

    [TestClass]
    public class ImageAssertTests
    {
        [TestMethod]
        public void TestColorAssertEmpty()
        {
            var bitmap = new Bitmap(200, 200);
            Assert.That.ImageColorsWellDistributed(bitmap);
        }

        [TestMethod]
        public void TestColorAssertFails()
        {
            var bitmap = new Bitmap(200, 200);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawRectangle(Pens.Aqua, 50, 50, 50, 50);
                g.DrawRectangle(Pens.Red, 60, 60, 140, 140);
            }

            Assert.ThrowsException<AssertFailedException>(() => Assert.That.ImageColorsWellDistributed(bitmap));
        }

        [TestMethod]
        public void TestColorAssertFails2()
        {
            var bitmap = new Bitmap(200, 200);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawRectangle(Pens.Red, 20, 0, 180, 200);
            }

            Assert.ThrowsException<AssertFailedException>(() => Assert.That.ImageColorsWellDistributed(bitmap));
        }

        [TestMethod]
        public void TestColorAssertRandomImage()
        {
            var random = Random.GetRandom();
            var bitmap = new Bitmap(200, 200);

            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    bitmap.SetPixel(i, j, random.NextColor());
                }
            }

            Assert.That.ImageColorsWellDistributed(bitmap);
        }
    }
}
