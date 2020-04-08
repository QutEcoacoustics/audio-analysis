// <copyright file="ImageAssert.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Acoustics.Shared.Contracts;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Advanced;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    public static class ImageAssert
    {
        public static void ImageIsSize(this Assert assert, int expectedWidth, int expectedHeight, Image actualImage)
        {
            Assert.AreEqual(expectedWidth, actualImage.Width, "Expected image width did not match actual image width");
            Assert.AreEqual(expectedHeight, actualImage.Height, "Expected image height did not match actual image height");
        }

        public static void PixelIsColor(this Assert assert, Point pixel, Color expectedColor, Image<Rgb24> actualImage)
        {
            var actualColor = actualImage[pixel.X, pixel.Y];
            Assert.AreEqual((Rgb24)expectedColor, actualColor, $"Expected color at pixel {pixel} did not match actual color");
        }

        /// <summary>
        /// Assert a certain ratio of colors are present in a region.
        /// NOT WELL TESTED.
        /// </summary>
        /// <example>
        /// var expectedColors = new Dictionary&lt;Color, double&gt;()
        ///            {
        ///                { Color.FromRgb(0, 0, 0), 0.7 },
        ///                { Color.FromRgb(255, 255, 255), 0.3 },
        ///            };
        /// ImageAssert.ImageRegionHasColors(new Rectangle(0, 24, 210, 3),  expectedColors, actualImage1, 0.07).
        /// </example>
        public static void ImageRegionHasColors(
            this Assert assert,
            Rectangle region,
            Dictionary<Color, double> expectedColors,
            Image<Rgb24> actualImage,
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

        public static void ImageRegionIsRepeatedHorizontally(this Assert assert, Rectangle region, int repeats, int spacing, Image<Rgb24> actualImage, double tolerance = 0.0)
        {
            Contract.Requires(spacing >= 1);
            Contract.Requires(spacing >= 1);

            // extract first region
            var expected = actualImage.Clone(x => x.Crop(region));

            // extract the next regions
            var tiles = Enumerable.Range(1, repeats)
                .Select(
                    i => actualImage.Clone(x => x.Crop(
                        new Rectangle(region.X + (spacing * i), region.Y, region.Width, region.Height))));

            int index = 0;
            foreach (var tile in tiles)
            {
                Assert.That.ImageMatches(expected, tile, tolerance, $"Repeat {index + 1} did not match original region");

                index++;
            }
        }

        public static void ImageColorsWellDistributed(
            this Assert assert,
            Image<Rgb24> actualImage,
            double allowedError = 0.1,
            Dictionary<Color, double> colorHistogram = null,
            string message = "")
        {
            colorHistogram ??= ImageTools.GetColorHistogramNormalized(actualImage);

            var perfectColorAverage = 1.0 / colorHistogram.Count;
            var sumOfDeltas = colorHistogram.Select(x => Math.Abs(perfectColorAverage - x.Value)).Sum();
            var totalError = sumOfDeltas / colorHistogram.Count;
            Assert.IsTrue(
                totalError <= allowedError,
                $"The total error for all colors ({totalError}) is greater than the allowable limit (scaled: {allowedError}).\n" + message);
        }

        public static void ImageIsSize<T>(this Assert assert, int expectedWidth, int expectedHeight, Image<T> actualImage)
            where T : struct, IPixel<T>
        {
            Assert.AreEqual(expectedWidth, actualImage.Width, "Expected image width did not match actual image width");
            Assert.AreEqual(expectedHeight, actualImage.Height, "Expected image height did not match actual image height");
        }

        public static void ImageRegionIsColor(this Assert assert, Rectangle region, Rgb24 expectedColor, Image<Rgb24> actualImage, double tolerance = 0.0)
        {
            //var width = region.Width;
            var area = region.Width * region.Height;

            var red = new int[area];
            var green = new int[area];
            var blue = new int[area];
            var indices = new HashSet<int>();
            for (var x = region.Left; x < region.Right; x++)
            {
                for (var y = region.Top; y < region.Bottom; y++)
                {
                    var color = actualImage[x, y];

                    var i = x - region.Left;
                    var j = y - region.Top;
                    var index0 = (i * region.Height) + j;
                    if (indices.Contains(index0))
                    {
                        Debugger.Break();
                    }

                    indices.Add(index0);
                    red[index0] = color.R;
                    green[index0] = color.G;
                    blue[index0] = color.B;
                }
            }

            var averageRed = red.Average();
            var averageBlue = blue.Average();
            var averageGreen = green.Average();

            Assert.IsTrue(
                Math.Abs(averageRed - expectedColor.R) <= tolerance &&
                Math.Abs(averageGreen - expectedColor.G) <= tolerance &&
                Math.Abs(averageBlue - expectedColor.B) <= tolerance,
                $"Region {region} is not expected color {expectedColor} - actual averages: R={averageRed:F20}, G={averageGreen:F20}, B={averageBlue:F20}");
        }

        public static void ImageMatches<T>(this Assert assert, Image<T> expectedImage, Image<T> actualImage, double tolerance = 0.0, string message = "")
            where T : struct, IPixel<T>
        {
            Assert.AreEqual(expectedImage.Size(), actualImage.Size());

            var (normalizedDifference, differences) = CompareImage(expectedImage, actualImage);

            if (normalizedDifference > tolerance)
            {
                var deltaStrings = differences.Take(10).Select(x => x.ToString()).FormatList();
                var assertReason =
                    $@"Images are not equal - total delta {normalizedDifference} is not less than tolerance {tolerance}.
Difference are:
{deltaStrings}
(and {differences.Count} more..)

{message}";
                Assert.Fail(assertReason);
            }
        }

        public static void ImageContainsExpected<T>(this Assert assert, Image<T> expectedImage, Point expectedLocation, Image<T> actualImage, double tolerance = 0.0, string message = "")
            where T : struct, IPixel<T>
        {
            var regionToCheck = new Rectangle(expectedLocation, expectedImage.Size());
            Assert.IsTrue(actualImage.Bounds().Contains(regionToCheck));

            var (normalizedDifference, differences) = CompareImage(expectedImage, actualImage, regionToCheck);

            if (normalizedDifference > tolerance)
            {
                var deltaStrings = differences.Take(10).Select(x => x.ToString()).FormatList();
                var assertReason =
                    $@"Images are not equal - total delta {normalizedDifference} is not less than tolerance {tolerance}.
Difference are:
{deltaStrings}
(and {differences.Count} more..)

{message}";
                Assert.Fail(assertReason);
            }
        }

        private static (float normalizedDifference, List<PixelDifference> differences) CompareImage<TPixel>(
            Image<TPixel> expected, Image<TPixel> actual, Rectangle? regionToCheck = null)
            where TPixel : struct, IPixel<TPixel>
        {
            // implementation based off of https://github.com/SixLabors/ImageSharp/blob/9ab02b6ee67b25fd3653146c069dab3687fc0ac8/tests/ImageSharp.Tests/TestUtilities/ImageComparison/TolerantImageComparer.cs
            var bounds = regionToCheck ?? actual.Bounds();

            Assert.IsTrue(actual.Bounds().Contains(bounds), "Actual image must fit within expected image");

            var offset = bounds.Location;

            float totalDifference = 0F;

            var differences = new List<PixelDifference>();

            for (int y = bounds.Top; y < bounds.Height; y++)
            {
                var expectedSpan = expected.GetPixelRowSpan(y - offset.Y);
                var actualSpan = actual.GetPixelRowSpan(y);
                Rgba32 a = default, b = default;

                for (int x = bounds.Left; x < bounds.Width; x++)
                {
                    expectedSpan[x - offset.X].ToRgba32(ref a);
                    actualSpan[x].ToRgba32(ref b);
                    int d = GetManhattanDistanceInRgbaSpace(ref a, ref b);

                    if (d > 0)
                    {
                        var diff = new PixelDifference(new Point(x, y), a, b);
                        differences.Add(diff);

                        totalDifference += d;
                    }
                }
            }

            float normalizedDifference = totalDifference / (actual.Width * (float)actual.Height);
            normalizedDifference /= 4F * 65535F;

            return (normalizedDifference, differences);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetManhattanDistanceInRgbaSpace(ref Rgba32 a, ref Rgba32 b)
        {
            return Diff(a.R, b.R) + Diff(a.G, b.G) + Diff(a.B, b.B) + Diff(a.A, b.A);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Diff(ushort a, ushort b) => Math.Abs(a - b);
    }

    internal class PixelDifference
    {
        public Point Point { get; }

        public Rgba32 A { get; }

        public Rgba32 B { get; }

        public PixelDifference(Point point, Rgba32 a, Rgba32 b)
        {
            this.Point = point;
            this.A = a;
            this.B = b;
        }

        public override string ToString() => $"at ({this.Point.X},{this.Point.Y}) in actual the expected color is {this.A} and actual is {this.B}";
    }
}